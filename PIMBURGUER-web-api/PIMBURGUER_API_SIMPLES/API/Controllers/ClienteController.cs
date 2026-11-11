using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PimBurguer.Api.Services;
using PimBurguer.Api.Dtos;

namespace PimBurguer.Api.Controllers
{
    [ApiController]
    [Route("api/cliente")]
    [Route("mobile/api/cliente")]
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ClienteController : ControllerBase
    {
        private readonly PainelService _painel;
        private readonly CupomService _cupons;

        public ClienteController(PainelService painel, CupomService cupons)
        {
            _painel = painel;
            _cupons = cupons;
        }

        [HttpGet("resumo")]
        public async Task<IActionResult> Resumo(CancellationToken ct)
        {
            var resumo = await _painel.ResumoClienteAsync(Identidade.Id(User), ct);
            return Ok(resumo);
        }

        [HttpGet("pedidos")]
        public async Task<IActionResult> Pedidos(int pagina = 1, CancellationToken ct = default)
        {
            var pedidos = await _painel.Pedidos(Identidade.Id(User), pagina, null, ct);
            return Ok(pedidos);
        }

        [HttpGet("pedidos/{id:guid}")]
        public async Task<IActionResult> Pedido(Guid id, CancellationToken ct)
        {
            var pedido = await _painel.BuscarPedidoClienteAsync(id, Identidade.Id(User), ct);
            if (pedido == null)
            {
                return NotFound();
            }

            return Ok(pedido);
        }

        [HttpPost("pedidos/{id:guid}/repetir")]
        public async Task<IActionResult> Repetir(Guid id, CancellationToken ct)
        {
            var repeticao = await _painel.RepetirPedidoAsync(id, Identidade.Id(User), ct);
            if (repeticao == null)
            {
                return NotFound();
            }

            return Ok(repeticao);
        }

        [HttpGet("cupons")]
        public async Task<IActionResult> Cupons(CancellationToken ct)
        {
            var cupons = await _cupons.Listar(false, Identidade.Id(User), ct);
            return Ok(cupons);
        }

        [HttpPost("cupons")]
        public async Task<IActionResult> AdicionarCupom(AdicionarCupomEntrada entrada, CancellationToken ct)
        {
            return Ok(await _cupons.AdicionarAsync(entrada.Codigo, Identidade.Id(User)!.Value, ct));
        }
    }
}
