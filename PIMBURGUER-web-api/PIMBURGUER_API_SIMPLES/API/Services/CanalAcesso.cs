using Microsoft.AspNetCore.Http;

namespace PimBurguer.Api.Services
{
    // A rota escolhe o banco no servidor. O cliente não informa uma conexão.
    public class CanalAcesso
    {
        public string Nome { get; }
        public string ChaveConexao { get; }

        public CanalAcesso(IHttpContextAccessor contexto)
        {
            var caminho = contexto.HttpContext?.Request.Path ?? PathString.Empty;
            if (caminho.StartsWithSegments("/mobile"))
            {
                Nome = "mobile";
                ChaveConexao = "PimBurguerApp";
            }
            else
            {
                Nome = "web";
                ChaveConexao = "PimBurguer";
            }
        }
    }
}
