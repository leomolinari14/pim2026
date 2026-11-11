using System.Text.Json.Serialization;

namespace PimBurguer.Api.Dtos
{
    public record AcessoResposta(string Token, DateTimeOffset ExpiraEm, UsuarioResposta Usuario)
    {
        [JsonPropertyName("access_token")]
        public string AccessToken
        {
            get
            {
                return Token;
            }
        }

        [JsonPropertyName("token_type")]
        public string TokenType
        {
            get
            {
                return "Bearer";
            }
        }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn
        {
            get
            {
                return Services.JwtConfiguracao.ValidadeSegundos;
            }
        }
    }
}
