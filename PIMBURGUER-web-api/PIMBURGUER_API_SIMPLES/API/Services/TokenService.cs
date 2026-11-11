using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using PimBurguer.Api.Data;
using PimBurguer.Api.Models;

namespace PimBurguer.Api.Services
{
    // Emite um JWT HS256 com a identificação do usuário e da sessão.
    public sealed class TokenService
    {
        private readonly JwtConfiguracao _config;
        private readonly CanalAcesso _canal;

        public TokenService(JwtConfiguracao config, CanalAcesso canal)
        {
            _config = config;
            _canal = canal;
        }

        public string Gerar(Usuario usuario, SessaoAcesso sessao, DateTimeOffset emitidoEm)
        {
            var claims = new[]
            {
                // O UUID identifica também administradores que não possuem e-mail.
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim("role", usuario.Perfil),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("sid", sessao.Id.ToString()),
                new Claim("canal", _canal.Nome),
                new Claim(JwtRegisteredClaimNames.Iat, emitidoEm.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture), ClaimValueTypes.Integer64)
            };
            var token = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                claims: claims,
                notBefore: emitidoEm.UtcDateTime,
                expires: sessao.ExpiraEm.UtcDateTime,
                signingCredentials: new SigningCredentials(_config.Key, SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
