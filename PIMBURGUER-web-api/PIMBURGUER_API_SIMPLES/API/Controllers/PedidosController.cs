using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PimBurguer.Api.Dtos;
using PimBurguer.Api.Services;

namespace PimBurguer.Api.Controllers
{
    [ApiController]
    [Route("api/pedidos")]
    [Route("mobile/api/pedidos")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class PedidosController : ControllerBase
    {
        private readonly PedidoService _pedidos;

        public PedidosController(PedidoService pedidos)
        {
            _pedidos = pedidos;
        }

        [HttpPost("orcamento")]
        public async Task<IActionResult> Orcar(OrcamentoRequisicao entrada, CancellationToken ct)
        {
            try
            {
                return Ok(await _pedidos.OrcarAsync(entrada, ct, Identidade.Id(User)));
            }
            catch (PedidoInvalidoException erro)
            {
                return Problem(erro.Message, statusCode: erro.Status, title: "Revise seu pedido");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Criar(PedidoRequisicao entrada, CancellationToken ct)
        {
            try
            {
                var pedido = await _pedidos.CriarAsync(entrada, ct, Identidade.Id(User));
                if (Request.Path.StartsWithSegments("/mobile"))
                {
                    return Created($"/mobile/api/pedidos/{pedido.Id}", pedido);
                }
                return CreatedAtAction(nameof(Consultar), new
                {
                    id = pedido.Id
                }, pedido);
            }
            catch (PedidoInvalidoException erro)
            {
                return Problem(erro.Message, statusCode: erro.Status, title: "Revise seu pedido");
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Consultar(Guid id, [FromHeader(Name = "X-Pedido-Token")] string? token, CancellationToken ct)
        {
            var pedido = await _pedidos.ConsultarAsync(id, token, ct);
            if (pedido is null)
            {
                return NotFound();
            }
            else
            {
                return Ok(pedido);
            }
        }
    }
}
