using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PimBurguer.Api.Dtos;
using PimBurguer.Api.Services;

namespace PimBurguer.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Route("mobile/api/admin")]
    [Authorize(Roles = "Administrador")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AdminController : ControllerBase
    {
        private readonly ProdutoService _produtos;
        private readonly CupomService _cupons;
        private readonly PainelService _painel;
        private readonly AcessoService _acesso;

        public AdminController(ProdutoService produtos, CupomService cupons, PainelService painel, AcessoService acesso)
        {
            _produtos = produtos;
            _cupons = cupons;
            _painel = painel;
            _acesso = acesso;
        }

        [HttpGet("produtos")]
        public async Task<IActionResult> Produtos(bool removidos = false, CancellationToken ct = default)
        {
            var produtos = await _produtos.ListarAdminAsync(removidos, ct);
            return Ok(produtos);
        }

        [HttpGet("produtos/{id:int}")]
        public async Task<IActionResult> Produto(int id, CancellationToken ct)
        {
            var produto = await _produtos.BuscarAdminAsync(id, ct);
            if (produto == null)
            {
                return NotFound();
            }

            return Ok(produto);
        }

        [HttpPost("produtos")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> CriarProduto([FromForm] ProdutoEntrada entrada, CancellationToken ct)
        {
            var produto = await _produtos.SalvarAsync(null, entrada, ct);
            return Ok(produto);
        }

        [HttpPut("produtos/{id:int}")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> EditarProduto(int id, [FromForm] ProdutoEntrada entrada, CancellationToken ct)
        {
            var produto = await _produtos.SalvarAsync(id, entrada, ct);
            return Ok(produto);
        }

        [HttpDelete("produtos/{id:int}")]
        public async Task<IActionResult> RemoverProduto(int id, CancellationToken ct)
        {
            var encontrado = await _produtos.RemoverAsync(id, ct);
            if (!encontrado)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("produtos/{id:int}/restaurar")]
        public async Task<IActionResult> RestaurarProduto(int id, CancellationToken ct)
        {
            var encontrado = await _produtos.RestaurarAsync(id, ct);
            if (!encontrado)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("cupons")]
        public async Task<IActionResult> Cupons(CancellationToken ct)
        {
            var cupons = await _cupons.Listar(true, null, ct);
            return Ok(cupons);
        }

        [HttpPost("cupons")]
        public async Task<IActionResult> CriarCupom(CupomEntrada e, CancellationToken ct)
        {
            await _cupons.Salvar(null, e, ct);
            return NoContent();
        }

        [HttpPut("cupons/{id:int}")]
        public async Task<IActionResult> EditarCupom(int id, CupomEntrada e, CancellationToken ct)
        {
            await _cupons.Salvar(id, e, ct);
            return NoContent();
        }

        [HttpPost("cupons/{id:int}/desativar")]
        public async Task<IActionResult> DesativarCupom(int id, CancellationToken ct)
        {
            var encontrado = await _cupons.DesativarAsync(id, ct);
            if (!encontrado)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("pedidos")]
        public async Task<IActionResult> Pedidos(int pagina = 1, string? status = null, CancellationToken ct = default)
        {
            var pedidos = await _painel.Pedidos(null, pagina, status, ct);
            return Ok(pedidos);
        }

        [HttpGet("pedidos/{id:guid}")]
        public async Task<IActionResult> Detalhe(Guid id, CancellationToken ct)
        {
            var detalhe = await _painel.DetalharPedidoAsync(id, ct);
            if (detalhe == null)
            {
                return NotFound();
            }

            return Ok(detalhe);
        }

        [HttpPut("pedidos/{id:guid}")]
        public async Task<IActionResult> AtualizarPedido(Guid id, AtualizarPedidoEntrada e, CancellationToken ct)
        {
            var administradorId = Identidade.Id(User)!.Value;
            await _painel.AtualizarPedido(id, administradorId, e, ct);
            return NoContent();
        }

        [HttpGet("relatorios")]
        public async Task<IActionResult> Relatorios(DateOnly? inicio, DateOnly? fim, CancellationToken ct)
        {
            var relatorio = await _painel.Relatorio(inicio, fim, ct);
            return Ok(relatorio);
        }

        [HttpGet("clientes")]
        public async Task<IActionResult> Clientes(CancellationToken ct)
        {
            var clientes = await _acesso.ListarClientesAsync(ct);
            return Ok(clientes);
        }

        [HttpPut("clientes/{id:guid}/acesso")]
        public async Task<IActionResult> AcessoCliente(Guid id, [FromBody] bool ativo, CancellationToken ct)
        {
            var encontrado = await _acesso.AlterarAcessoClienteAsync(id, ativo, ct);
            if (!encontrado)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
