using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PimBurguer.Api.Data;
using PimBurguer.Api.Dtos;
using PimBurguer.Api.Models;
using PimBurguer.Validacao;

namespace PimBurguer.Api.Services
{
    public class AcessoService
    {
        private readonly ContasDbContext _banco;
        private readonly PimBurguerDbContext _local;
        private readonly ContasCompartilhadas _contas;
        private readonly IPasswordHasher<Usuario> _hasher;
        private readonly TokenService _tokens;

        public AcessoService(PimBurguerDbContext db, ContasCompartilhadas contas,
            IPasswordHasher<Usuario> hasher, TokenService tokens)
        {
            _local = db;
            _contas = contas;
            _banco = contas.Banco;
            _hasher = hasher;
            _tokens = tokens;
        }

        private static string Telefone(string texto)
        {
            var valor = Regex.Replace(texto, "[^0-9]", "");
            if (!Regex.IsMatch(valor, @"^\d{10,11}$"))
            {
                throw new RegraException("Informe um telefone brasileiro com DDD.");
            }

            return valor;
        }

        public async Task<AcessoResposta> Cadastrar(CadastroEntrada entrada, CancellationToken ct)
        {
            if (!Cpf.Valido(entrada.Cpf))
            {
                throw new RegraException("Informe um CPF válido.");
            }

            var nome = entrada.Nome.Trim();
            if (nome.Length < 2)
            {
                throw new RegraException("Informe seu nome completo.");
            }

            var email = entrada.Email.Trim().ToLowerInvariant();
            var telefone = Telefone(entrada.Telefone);
            await using var transacao = await _banco.Database.BeginTransactionAsync(ct);
            if (await _banco.Usuarios.AnyAsync(u => u.EmailNormalizado == email, ct)
                || await _contas.EmailLegadoExisteAsync(email, ct))
            {
                throw new RegraException("Este e-mail já está cadastrado. Entre na sua conta.", 409);
            }

            var usuario = new Usuario
            {
                Nome = nome,
                Email = email,
                EmailNormalizado = email,
                Telefone = telefone,
                Perfil = "Cliente",
                Cpf = Cpf.Normalizar(entrada.Cpf)

            };
            usuario.SenhaHash = _hasher.HashPassword(usuario, entrada.Senha);
            _banco.Usuarios.Add(usuario);
            var acesso = NovaSessao(usuario);
            try
            {
                await _banco.SaveChangesAsync(ct);
                await transacao.CommitAsync(ct);
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: "23505" })
            {
                throw new RegraException("Este e-mail já está cadastrado. Entre na sua conta.", 409);
            }

            var local = await _contas.UsuarioLocalAsync(usuario, ct);
            return acesso with { Usuario = Identidade.Resposta(local) };
        }

        public async Task<AcessoResposta> Entrar(LoginEntrada entrada, CancellationToken ct)
        {
            var identificacao = entrada.Email.Trim().ToLowerInvariant();
            // Clientes continuam entrando pelo e-mail; o administrador usa seu usuário.
            var usuario = await _contas.BuscarLoginAsync(identificacao, entrada.Senha, ct);
            if (usuario is null || !usuario.Ativo || usuario.BloqueadoAte > DateTimeOffset.UtcNow)
            {
                throw new RegraException("Usuário/e-mail ou senha inválidos, ou acesso temporariamente bloqueado.", 401);
            }

            var resultado = _hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, entrada.Senha);
            if (resultado == PasswordVerificationResult.Failed)
            {
                usuario.FalhasLogin++;
                if (usuario.FalhasLogin >= 5)
                {
                    usuario.BloqueadoAte = DateTimeOffset.UtcNow.AddMinutes(15);
                    usuario.FalhasLogin = 0;
                }

                await _banco.SaveChangesAsync(ct);
                throw new RegraException("Usuário/e-mail ou senha inválidos, ou acesso temporariamente bloqueado.", 401);
            }

            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.SenhaHash = _hasher.HashPassword(usuario, entrada.Senha);
            }

            usuario.FalhasLogin = 0;
            usuario.BloqueadoAte = null;
            var acesso = NovaSessao(usuario);
            await _banco.SaveChangesAsync(ct);
            var local = await _contas.UsuarioLocalAsync(usuario, ct);
            return acesso with { Usuario = Identidade.Resposta(local) };
        }

        private AcessoResposta NovaSessao(Usuario usuario)
        {
            var emitidoEm = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            var sessao = new SessaoAcesso
            {
                UsuarioId = usuario.Id,
                ExpiraEm = emitidoEm.AddSeconds(JwtConfiguracao.ValidadeSegundos)

            };
            var token = _tokens.Gerar(usuario, sessao, emitidoEm);
            sessao.TokenHash = Identidade.Hash(token);
            _banco.SessoesAcesso.Add(sessao);
            return new AcessoResposta(token, sessao.ExpiraEm, Identidade.Resposta(usuario));
        }

        public async Task<UsuarioResposta> Perfil(Guid id, PerfilEntrada entrada, CancellationToken ct)
        {
            var u = await _contas.ContaPorUsuarioLocalAsync(id, ct)
                ?? throw new RegraException("Conta não encontrada.", 404);
            u.Nome = entrada.Nome.Trim();
            if (u.Nome.Length < 2)
            {
                throw new RegraException("Informe seu nome completo.");
            }

            u.Telefone = Telefone(entrada.Telefone);
            await _banco.SaveChangesAsync(ct);
            return Identidade.Resposta(await _contas.UsuarioLocalAsync(u, ct));
        }

        public async Task TrocarSenha(Guid id, SenhaEntrada entrada, CancellationToken ct)
        {
            var u = await _contas.ContaPorUsuarioLocalAsync(id, ct)
                ?? throw new RegraException("Conta não encontrada.", 404);
            if (_hasher.VerifyHashedPassword(u, u.SenhaHash, entrada.SenhaAtual) == PasswordVerificationResult.Failed)
            {
                throw new RegraException("A senha atual não confere.");
            }

            await using var transacao = await _banco.Database.BeginTransactionAsync(ct);
            u.SenhaHash = _hasher.HashPassword(u, entrada.NovaSenha);
            await _banco.SaveChangesAsync(ct);
            await _banco.SessoesAcesso
                .Where(s => s.UsuarioId == u.Id)
                .ExecuteDeleteAsync(ct);
            await transacao.CommitAsync(ct);
        }

        public async Task<UsuarioResposta> BuscarPerfilAsync(Guid id, CancellationToken ct)
        {
            var usuario = await _contas.ContaPorUsuarioLocalAsync(id, ct)
                ?? throw new RegraException("Conta não encontrada.", 404);
            return Identidade.Resposta(await _contas.UsuarioLocalAsync(usuario, ct));
        }

        public async Task SairAsync(Guid sessaoId, CancellationToken ct)
        {
            await _banco.SessoesAcesso
                .Where(sessao => sessao.Id == sessaoId)
                .ExecuteDeleteAsync(ct);
        }

        public async Task<List<ClienteAdminResposta>> ListarClientesAsync(CancellationToken ct)
        {
            var clientes = await _local.Usuarios
                .AsNoTracking()
                .Where(usuario => usuario.Perfil == "Cliente")
                .OrderBy(usuario => usuario.Nome)
                .Select(usuario => new ClienteAdminResposta(
                usuario.Id,
                usuario.Nome,
                usuario.Email ?? "",
                usuario.Telefone,
                usuario.Ativo,
                usuario.CriadoEm,
                _local.Pedidos.Count(pedido => pedido.UsuarioId == usuario.Id)))
                .ToListAsync(ct);
            var emails = clientes.Select(c => c.Email.ToLowerInvariant()).ToArray();
            var contas = await _banco.Usuarios.AsNoTracking()
                .Where(u => u.EmailNormalizado != null && emails.Contains(u.EmailNormalizado))
                .ToDictionaryAsync(u => u.EmailNormalizado!, ct);
            return clientes.Select(c => contas.TryGetValue(c.Email.ToLowerInvariant(), out var conta)
                ? c with { Nome = conta.Nome, Telefone = conta.Telefone, Ativo = conta.Ativo }
                : c).ToList();
        }

        public async Task<bool> AlterarAcessoClienteAsync(Guid id, bool ativo, CancellationToken ct)
        {
            var local = await _local.Usuarios
                .SingleOrDefaultAsync(usuario => usuario.Id == id && usuario.Perfil == "Cliente", ct);
            if (local == null)
            {
                return false;
            }

            var usuario = await _contas.ContaPorUsuarioLocalAsync(id, ct);
            if (usuario == null)
            {
                // Uma conta antiga do app ainda não importada continua administrável.
                local.Ativo = ativo;
                await _local.SaveChangesAsync(ct);
                return true;
            }
            if (usuario.Perfil != "Cliente") return false;
            await using var transacao = await _banco.Database.BeginTransactionAsync(ct);
            usuario.Ativo = ativo;
            await _banco.SaveChangesAsync(ct);
            if (!ativo)
            {
                await _banco.SessoesAcesso
                    .Where(sessao => sessao.UsuarioId == usuario.Id)
                    .ExecuteDeleteAsync(ct);
            }
            await transacao.CommitAsync(ct);
            await _contas.UsuarioLocalAsync(usuario, ct);
            return true;
        }
    }
}
