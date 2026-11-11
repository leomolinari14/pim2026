using Microsoft.AspNetCore.Mvc;
using PimBurguer.Api.Services;

namespace PimBurguer.Api.Controllers
{
    [ApiController]
    [Route("api/imagens")]
    [Route("mobile/api/imagens")]
    [Route("midia/produtos")]
    [Route("mobile/midia/produtos")]
    public class ImagensController : ControllerBase
    {
        private readonly ImagensProdutoService _imagens;

        public ImagensController(ImagensProdutoService imagens)
        {
            _imagens = imagens;
        }

        [HttpGet("{nome}")]
        public IActionResult Buscar(string nome)
        {
            var imagem = _imagens.Buscar(nome);
            if (imagem is null)
            {
                return NotFound();
            }

            Response.Headers.CacheControl = "public,max-age=604800,immutable";
            return PhysicalFile(imagem.Value.Caminho, imagem.Value.Tipo);
        }
    }
}
