using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PimBurguer.Api.Data;
using PimBurguer.Api.Services;

namespace PimBurguer.Api.Controllers
{
    [ApiController]
    [Route("api/saude")]
    [Route("mobile/api/saude")]
    public class SaudeController : ControllerBase
    {
        private readonly PimBurguerDbContext _banco;
        private readonly ILogger<SaudeController> _logger;
        private readonly CanalAcesso _canal;

        public SaudeController(PimBurguerDbContext banco, ILogger<SaudeController> logger, CanalAcesso canal)
        {
            _banco = banco;
            _logger = logger;
            _canal = canal;
        }

        [HttpGet]
        public async Task<IActionResult> Verificar(CancellationToken cancellationToken)
        {
            try
            {
                // Consultar a tabela também verifica se a migration já foi aplicada.
                var quantidade = await _banco.Produtos.CountAsync(cancellationToken);
                return Ok(new
                {
                    status = "ok",
                    banco = "conectado",
                    nomeBanco = _banco.Database.GetDbConnection().Database,
                    canal = _canal.Nome,
                    frontend = _canal.Nome == "mobile" ? "flutter" : "html",
                    produtos = quantidade
                });
            }
            catch (Exception erro) when (erro is not OperationCanceledException)
            {
                _logger.LogWarning(erro, "Falha ao consultar o banco na verificação de saúde.");
                return Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Banco indisponível", detail: "Verifique se o PostgreSQL está iniciado e se a migration foi aplicada.");
            }
        }
    }
}
