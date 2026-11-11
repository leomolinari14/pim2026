using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PimBurguer.Api.Data;
using PimBurguer.Api.Models;

namespace PimBurguer.Api.Services
{
    // A conta oficial fica no web. O mobile conserva um vínculo local para os pedidos.
    public class ContasCompartilhadas
    {
        private readonly PimBurguerDbContext _local;
        private readonly CanalAcesso _canal;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher<Usuario> _hasher;
        public ContasDbContext Banco { get; }

        public ContasCompartilhadas(ContasDbContext contas, PimBurguerDbContext local,
            CanalAcesso canal, IConfiguration config, IPasswordHasher<Usuario> hasher)
        {
            Banco = contas;
            _local = local;
            _canal = canal;
            _config = config;
            _hasher = hasher;
        }

        private PimBurguerDbContext? AbrirBancoMobile()
        {
            var conexao = _config.GetConnectionString("PimBurguerApp");
            return string.IsNullOrWhiteSpace(conexao) ? null : new PimBurguerDbContext(
                new DbContextOptionsBuilder<PimBurguerDbContext>().UseNpgsql(conexao).Options);
        }

        public async Task<bool> EmailLegadoExisteAsync(string email, CancellationToken ct)
        {
            await using var mobile = AbrirBancoMobile();
            return mobile != null && await mobile.Usuarios.AnyAsync(u => u.EmailNormalizado == email, ct);
        }

        public async Task<Usuario?> BuscarLoginAsync(string identificacao, string senha, CancellationToken ct)
        {
            var conta = await Banco.Usuarios.SingleOrDefaultAsync(
                u => u.EmailNormalizado == identificacao || u.NomeUsuario == identificacao, ct);
            if (conta != null) return conta;

            // Recupera contas antigas que existiam só no app após conferir sua senha.
            // Uma conta web já existente sempre tem prioridade para o mesmo e-mail.
            await using var mobile = AbrirBancoMobile();
            if (mobile == null) return null;
            var legado = await mobile.Usuarios.SingleOrDefaultAsync(
                u => u.EmailNormalizado == identificacao && u.Perfil == "Cliente", ct);
            if (legado == null || !legado.Ativo || legado.BloqueadoAte > DateTimeOffset.UtcNow
                || string.IsNullOrEmpty(legado.SenhaHash)) return null;
            if (_hasher.VerifyHashedPassword(legado, legado.SenhaHash, senha) == PasswordVerificationResult.Failed)
            {
                legado.FalhasLogin++;
                if (legado.FalhasLogin >= 5)
                {
                    legado.BloqueadoAte = DateTimeOffset.UtcNow.AddMinutes(15);
                    legado.FalhasLogin = 0;
                }
                await mobile.SaveChangesAsync(ct);
                return null;
            }

            conta = new Usuario
            {
                Id = await Banco.Usuarios.AnyAsync(u => u.Id == legado.Id, ct) ? Guid.NewGuid() : legado.Id,
                Nome = legado.Nome, Email = legado.Email, EmailNormalizado = legado.EmailNormalizado,
                Telefone = legado.Telefone, Cpf = legado.Cpf, SenhaHash = legado.SenhaHash,
                Perfil = "Cliente", Ativo = legado.Ativo, CriadoEm = legado.CriadoEm
            };
            Banco.Usuarios.Add(conta);
            try
            {
                await Banco.SaveChangesAsync(ct);
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: "23505" })
            {
                Banco.Entry(conta).State = EntityState.Detached;
                // Outra instância pode ter criado a conta; o login ainda verificará sua senha oficial.
                conta = await Banco.Usuarios.SingleOrDefaultAsync(u => u.EmailNormalizado == identificacao, ct);
            }
            return conta;
        }

        public async Task<Usuario> UsuarioLocalAsync(Usuario conta, CancellationToken ct)
        {
            if (_canal.Nome == "web") return conta;
            var local = await _local.Usuarios.SingleOrDefaultAsync(u =>
                conta.EmailNormalizado != null ? u.EmailNormalizado == conta.EmailNormalizado
                    : u.NomeUsuario == conta.NomeUsuario, ct);
            if (local == null)
            {
                local = new Usuario
                {
                    Id = await _local.Usuarios.AnyAsync(u => u.Id == conta.Id, ct) ? Guid.NewGuid() : conta.Id,
                    Perfil = conta.Perfil, CriadoEm = conta.CriadoEm
                };
                _local.Usuarios.Add(local);
            }
            else if (local.Perfil != conta.Perfil)
            {
                throw new RegraException("Há um conflito no perfil desta conta. Confira o cadastro com o administrador.", 409);
            }

            // Preserva o UUID local antigo e, portanto, as referências dos pedidos já feitos.
            local.Nome = conta.Nome;
            local.Email = conta.Email;
            local.EmailNormalizado = conta.EmailNormalizado;
            local.NomeUsuario = conta.NomeUsuario;
            local.Telefone = conta.Telefone;
            local.Cpf = conta.Cpf;
            local.Ativo = conta.Ativo;
            // O vínculo local não autentica: senha e sessões são consultadas no banco de contas.
            local.SenhaHash = "";
            local.FalhasLogin = 0;
            local.BloqueadoAte = null;
            await _local.SaveChangesAsync(ct);
            return local;
        }

        public async Task<Usuario?> ContaPorUsuarioLocalAsync(Guid id, CancellationToken ct)
        {
            if (_canal.Nome == "web") return await Banco.Usuarios.SingleOrDefaultAsync(u => u.Id == id, ct);
            var local = await _local.Usuarios.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id, ct);
            if (local == null) return null;
            return await Banco.Usuarios.SingleOrDefaultAsync(u =>
                local.EmailNormalizado != null ? u.EmailNormalizado == local.EmailNormalizado
                    : u.NomeUsuario == local.NomeUsuario, ct);
        }
    }
}
