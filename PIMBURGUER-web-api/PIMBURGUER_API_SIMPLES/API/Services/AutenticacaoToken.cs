using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PimBurguer.Api.Data;

namespace PimBurguer.Api.Services
{
    // Complementa o JWT já validado com a revogação e o perfil atual da conta.
    public sealed class AutenticacaoToken : JwtBearerEvents
    {
        private readonly ContasDbContext _banco;
        private readonly ContasCompartilhadas _contas;
        private readonly CanalAcesso _canal;

        public AutenticacaoToken(ContasCompartilhadas contas, CanalAcesso canal)
        {
            _contas = contas;
            _banco = contas.Banco;
            _canal = canal;
        }

        public override async Task TokenValidated(TokenValidatedContext context)
        {
            var principal = context.Principal;
            if (!Guid.TryParse(principal?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var usuarioId) || !Guid.TryParse(principal?.FindFirstValue("sid"), out var sessaoId) || !Guid.TryParse(principal?.FindFirstValue(JwtRegisteredClaimNames.Jti), out _) || principal?.FindFirstValue("canal") != _canal.Nome)
            {
                context.Fail("JWT inválido para este canal.");
                return;
            }

            var cabecalho = context.Request.Headers.Authorization.ToString();
            var token = cabecalho.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? cabecalho[7..].Trim() : "";
            var hash = Identidade.Hash(token);
            var sessao = await _banco.SessoesAcesso
                .AsNoTracking()
                .Include(s => s.Usuario)
                .SingleOrDefaultAsync(s => s.Id == sessaoId && s.UsuarioId == usuarioId && s.TokenHash == hash && s.ExpiraEm > DateTimeOffset.UtcNow && s.Usuario.Ativo, context.HttpContext.RequestAborted);
            if (sessao is null)
            {
                context.Fail("Sessão encerrada.");
                return;
            }

            var u = sessao.Usuario;
            var local = await _contas.UsuarioLocalAsync(u, context.HttpContext.RequestAborted);
            var identidade = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, u.Id.ToString()), new Claim("name", u.Nome), new Claim("role", u.Perfil), new Claim("sid", sessao.Id.ToString()), new Claim(ClaimTypes.NameIdentifier, local.Id.ToString()), new Claim(ClaimTypes.Email, u.Email ?? ""), new Claim("sessao", sessao.Id.ToString()), new Claim("canal", _canal.Nome)], JwtBearerDefaults.AuthenticationScheme, "name", "role");
            context.Principal = new ClaimsPrincipal(identidade);
        }
    }
}
