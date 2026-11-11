using Microsoft.AspNetCore.Mvc;
using PimBurguer.Api.Dtos;
using PimBurguer.Api.Services;

namespace PimBurguer.Api.Controllers
{
    [ApiController]
    [Route("api/produtos")]
    [Route("mobile/api/produtos")]
    public class ProdutosController : ControllerBase
    {
        private readonly ProdutoService _produtos;

        public ProdutosController(ProdutoService produtos)
        {
            _produtos = produtos;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProdutoResposta>>> Listar([FromQuery] string? busca, CancellationToken cancellationToken)
        {
            return Ok(await _produtos.ListarAsync(busca, cancellationToken));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType<ProdutoResposta>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProdutoResposta>> Buscar(int id, CancellationToken cancellationToken)
        {
            var produto = await _produtos.BuscarAsync(id, cancellationToken);
            if (produto is null)
            {
                return NotFound();
            }
            else
            {
                return Ok(produto);
            }
        }
    }
}
