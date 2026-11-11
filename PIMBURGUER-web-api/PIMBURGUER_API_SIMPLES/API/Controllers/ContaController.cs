using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PimBurguer.Api.Dtos;
using PimBurguer.Api.Services;

namespace PimBurguer.Api.Controllers
{
    [ApiController]
    [Route("api/conta")]
    [Route("mobile/api/conta")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ContaController : ControllerBase
    {
        private readonly AcessoService _acesso;

        public ContaController(AcessoService acesso)
        {
            _acesso = acesso;
        }

        [AllowAnonymous]
        [HttpPost("cadastro")]
        [EnableRateLimiting("acesso")]
        public async Task<IActionResult> Cadastro(CadastroEntrada entrada, CancellationToken ct)
        {
            try
            {
                var resultado = await _acesso.Cadastrar(entrada, ct);
                return Ok(resultado);
            }
            catch (RegraException erro)
            {
                return Problem(erro.Message, statusCode: erro.Status);
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [EnableRateLimiting("acesso")]
        public async Task<IActionResult> Login(LoginEntrada entrada, CancellationToken ct)
        {
            try
            {
                var resultado = await _acesso.Entrar(entrada, ct);
                return Ok(resultado);
            }
            catch (RegraException erro)
            {
                return Problem(erro.Message, statusCode: erro.Status);
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var usuarioId = Identidade.Id(User)!.Value;
            var usuario = await _acesso.BuscarPerfilAsync(usuarioId, ct);
            return Ok(usuario);
        }

        [Authorize]
        [HttpPost("sair")]
        public async Task<IActionResult> Sair(CancellationToken ct)
        {
            var sessaoId = Guid.Parse(User.FindFirst("sessao")!.Value);
            await _acesso.SairAsync(sessaoId, ct);
            return NoContent();
        }

        [Authorize]
        [HttpPut("perfil")]
        public async Task<IActionResult> Perfil(PerfilEntrada entrada, CancellationToken ct)
        {
            try
            {
                var usuarioId = Identidade.Id(User)!.Value;
                var resultado = await _acesso.Perfil(usuarioId, entrada, ct);
                return Ok(resultado);
            }
            catch (RegraException erro)
            {
                return Problem(erro.Message, statusCode: erro.Status);
            }
        }

        [Authorize]
        [HttpPut("senha")]
        [EnableRateLimiting("acesso")]
        public async Task<IActionResult> Senha(SenhaEntrada entrada, CancellationToken ct)
        {
            try
            {
                var usuarioId = Identidade.Id(User)!.Value;
                await _acesso.TrocarSenha(usuarioId, entrada, ct);
                return NoContent();
            }
            catch (RegraException erro)
            {
                return Problem(erro.Message, statusCode: erro.Status);
            }
        }
    }
}
