using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using PimBurguer.Api.Dtos;
using PimBurguer.Api.Models;

namespace PimBurguer.Api.Services
{
    public static class Identidade
    {
        public static Guid? Id(ClaimsPrincipal pessoa)
        {
            if (Guid.TryParse(pessoa.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            {
                return id;
            }
            else
            {
                return null;
            }
        }

        public static string Hash(string texto)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto)));
        }

        public static DateOnly Hoje
        {
            get
            {
                return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Fuso).DateTime);
            }
        }

        public static TimeZoneInfo Fuso
        {
            get
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
        }

        public static UsuarioResposta Resposta(Usuario u)
        {
            return new UsuarioResposta(
                u.Id,
                u.Nome,
                u.Email,
                u.Telefone,
                u.Perfil,
                u.NomeUsuario,
                u.Cpf);
        }
    }
}
