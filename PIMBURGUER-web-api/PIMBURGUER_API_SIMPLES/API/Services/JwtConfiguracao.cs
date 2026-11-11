using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PimBurguer.Api.Services
{
    public sealed class JwtConfiguracao
    {
        public JwtConfiguracao(string issuer, string audience, byte[] key)
        {
            Issuer = issuer;
            Audience = audience;
            Key = new SymmetricSecurityKey(key);
        }

        public const int ValidadeSegundos = 1800;
        public string Issuer
        {
            get;
        }
        public string Audience
        {
            get;
        }
        public SymmetricSecurityKey Key
        {
            get;
        }

        public TokenValidationParameters Validacao()
        {
            return new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = Key,
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ClockSkew = TimeSpan.Zero,
                NameClaimType = "name",
                RoleClaimType = "role",
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256]

            };
        }

        public static JwtConfiguracao Carregar(IConfiguration config, IHostEnvironment ambiente)
        {
            var issuer = config["Jwt:Issuer"];
            var audience = config["Jwt:Audience"];
            if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
            {
                throw new InvalidOperationException("Configure Jwt:Issuer e Jwt:Audience.");
            }

            var key = config["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(key))
            {
                if (!ambiente.IsDevelopment())
                {
                    throw new InvalidOperationException("Configure Jwt:Key por variável de ambiente ou serviço de segredos. Use pelo menos 32 bytes aleatórios em UTF-8.");
                }

                // Somente Development: chave aleatória por instalação, fora dos fontes.
                var path = config["Jwt:DevelopmentKeyFile"];
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PimBurguer", "Segredos", "jwt-dev.key");
                }

                path = Path.GetFullPath(path);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                using var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                if (file.Length == 0)
                {
                    key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                    file.Write(Encoding.UTF8.GetBytes(key));
                    file.Flush(true);
                }
                else
                {
                    using var reader = new StreamReader(file, Encoding.UTF8, leaveOpen: true);
                    key = reader.ReadToEnd().Trim();
                }
            }

            var bytes = Encoding.UTF8.GetBytes(key);
            if (bytes.Length < 32)
            {
                throw new InvalidOperationException("Jwt:Key deve conter pelo menos 32 bytes em UTF-8.");
            }

            return new JwtConfiguracao(issuer, audience, bytes);
        }
    }
}
