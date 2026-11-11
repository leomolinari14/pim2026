using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PimBurguer.Api.Data;
using PimBurguer.Api.Models;

namespace PimBurguer.Api.Services
{
    // Executado somente em Development. Cria a conta uma vez, sem redefinir senhas.
    public class AdministradorLocalService
    {
        private readonly PimBurguerDbContext _banco;
        private readonly IPasswordHasher<Usuario> _hasher;
        private readonly IConfiguration _configuracao;

        public AdministradorLocalService(PimBurguerDbContext db, IPasswordHasher<Usuario> hasher, IConfiguration configuracao)
        {
            _banco = db;
            _hasher = hasher;
            _configuracao = configuracao;
        }

        public async Task CriarSeNecessario(CancellationToken ct = default)
        {
            var usuario = _configuracao["AdministradorLocal:Usuario"]?.Trim().ToLowerInvariant();
            var senha = _configuracao["AdministradorLocal:Senha"];
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senha))
            {
                return;
            }

            await using var transacao = await _banco.Database.BeginTransactionAsync(ct);
            // Protege a criação inicial se duas instâncias da API iniciarem juntas.
            await _banco.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(7842051)", ct);
            var existente = await _banco.Usuarios
                .SingleOrDefaultAsync(u => u.NomeUsuario == usuario, ct);
            if (existente is not null)
            {
                if (existente.Perfil != "Administrador")
                {
                    throw new InvalidOperationException("O usuário configurado para o administrador local já pertence a outra conta.");
                }

                return;
            }

            var administrador = new Usuario
            {
                Nome = "Administrador PimBurguer",
                NomeUsuario = usuario,
                Perfil = "Administrador"

            };
            administrador.SenhaHash = _hasher.HashPassword(administrador, senha);
            _banco.Usuarios.Add(administrador);
            await _banco.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);
        }
    }
}
