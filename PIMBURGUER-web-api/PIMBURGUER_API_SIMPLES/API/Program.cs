using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using PimBurguer.Api.Data;
using PimBurguer.Api.Models;
using PimBurguer.Api.Services;

namespace PimBurguer.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // O site fica na pasta WEB, ao lado da pasta API.
            var pastaApi = Directory.GetCurrentDirectory();
            var pastaSite = Path.GetFullPath(Path.Combine(pastaApi, "..", "WEB"));
            var opcoes = new WebApplicationOptions
            {
                Args = args,
                ContentRootPath = pastaApi,
                WebRootPath = pastaSite
            };
            var builder = WebApplication.CreateBuilder(opcoes);

            builder.Configuration.AddJsonFile("CONFIGURACAO_LOJA.json", optional: false, reloadOnChange: true);
            builder.Configuration.AddEnvironmentVariables();
            builder.Services.Configure<LojaOptions>(builder.Configuration.GetSection("Loja"));

            // Entity Framework Core usa esta conexão para acessar o PostgreSQL.
            var conexao = builder.Configuration.GetConnectionString("PimBurguer");
            if (string.IsNullOrWhiteSpace(conexao))
            {
                throw new InvalidOperationException("Configure ConnectionStrings:PimBurguer.");
            }
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<CanalAcesso>();
            builder.Services.AddDbContext<ContasDbContext>(options => options.UseNpgsql(conexao));
            builder.Services.AddScoped<ContasCompartilhadas>();
            builder.Services.AddDbContext<PimBurguerDbContext>((servicos, options) =>
            {
                var canal = servicos.GetRequiredService<CanalAcesso>();
                var conexaoDoCanal = builder.Configuration.GetConnectionString(canal.ChaveConexao);
                if (string.IsNullOrWhiteSpace(conexaoDoCanal))
                {
                    throw new RegraException("Configure a conexão do banco deste canal.", 503);
                }
                options.UseNpgsql(conexaoDoCanal);
            });

            // A injeção de dependência fornece os serviços aos controllers.
            builder.Services.AddScoped<ProdutoService>();
            builder.Services.AddScoped<PedidoService>();
            builder.Services.AddScoped<AcessoService>();
            builder.Services.AddScoped<CupomService>();
            builder.Services.AddScoped<PainelService>();
            builder.Services.AddScoped<AdministradorLocalService>();
            builder.Services.AddSingleton<ImagensProdutoService>();
            builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

            // O JWT identifica o usuário. A sessão no banco permite revogar o acesso.
            var jwt = JwtConfiguracao.Carregar(builder.Configuration, builder.Environment);
            builder.Services.AddSingleton(jwt);
            builder.Services.AddScoped<TokenService>();
            builder.Services.AddScoped<AutenticacaoToken>();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = jwt.Validacao();
                    options.EventsType = typeof(AutenticacaoToken);
                    options.IncludeErrorDetails = false;
                    options.SaveToken = false;
                });
            builder.Services.AddAuthorization();

            // Limita as tentativas de cadastro, login e troca de senha por IP.
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = 429;
                options.AddPolicy("acesso", context => RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "local",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 40,
                        Window = TimeSpan.FromMinutes(5),
                        QueueLimit = 0
                    }));
            });

            builder.Services.AddControllers();
            builder.Services.AddProblemDetails();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("HtmlLocal", policy => policy
                    .SetIsOriginAllowed(PermitirOrigemLocal)
                    .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                    .WithHeaders("Content-Type", "Authorization", "X-Pedido-Token"));
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT HS256 recebido no login, válido por 30 minutos. Informe o access_token retornado pelo login."
                });
                options.OperationFilter<SwaggerAcessoFilter>();
                options.AddServer(new OpenApiServer { Url = "/", Description = "Web" });
            });

            var app = builder.Build();
            using (var escopoCupons = app.Services.CreateScope())
            {
                await escopoCupons.ServiceProvider.GetRequiredService<CupomService>().PrepararCompartilhamentoAsync();
            }
            if (app.Environment.IsDevelopment())
            {
                using var escopo = app.Services.CreateScope();
                var administrador = escopo.ServiceProvider.GetRequiredService<AdministradorLocalService>();
                try
                {
                    await administrador.CriarSeNecessario();
                }
                catch (Npgsql.NpgsqlException)
                {
                    app.Logger.LogWarning("Banco web indisponível no início; verifique o PostgreSQL.");
                }
            }

            app.UseExceptionHandler();
            app.Use(async (contexto, proximo) =>
            {
                try
                {
                    await proximo();
                }
                catch (RegraException erro)
                {
                    contexto.Response.StatusCode = erro.Status;
                    await contexto.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
                    {
                        Status = erro.Status,
                        Title = "Confira os dados",
                        Detail = erro.Message
                    });
                }
            });
            app.Use(async (contexto, proximo) =>
            {
                contexto.Response.Headers["X-Content-Type-Options"] = "nosniff";
                await proximo();
            });

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture),
                SupportedCultures = new List<CultureInfo> { CultureInfo.InvariantCulture },
                SupportedUICultures = new List<CultureInfo> { CultureInfo.InvariantCulture },
                RequestCultureProviders = new List<IRequestCultureProvider>()
            });

            // Mantém a ordem das gravações locais, além das transações do banco.
            var gravacoes = new SemaphoreSlim(1, 1);
            app.Use(async (contexto, proximo) =>
            {
                var escrita = (contexto.Request.Path.StartsWithSegments("/api")
                    || contexto.Request.Path.StartsWithSegments("/mobile/api"))
                    && !HttpMethods.IsGet(contexto.Request.Method)
                    && !HttpMethods.IsHead(contexto.Request.Method);
                if (!escrita)
                {
                    await proximo();
                    return;
                }
                await gravacoes.WaitAsync(contexto.RequestAborted);
                try
                {
                    var canal = contexto.RequestServices.GetRequiredService<CanalAcesso>();
                    contexto.Response.Headers["X-Pim-Canal"] = canal.Nome;
                    await proximo();
                }
                finally
                {
                    gravacoes.Release();
                }
            });

            app.UseRouting();
            if (app.Environment.IsDevelopment())
            {
                app.UseCors("HtmlLocal");
            }
            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    var nome = context.File.Name;
                    if (nome.EndsWith(".html") || nome.EndsWith(".js") || nome.EndsWith(".css"))
                    {
                        context.Context.Response.Headers.CacheControl = "no-cache";
                    }
                }
            });
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.MapControllers();
            app.MapGet("/api/loja", (IOptionsMonitor<LojaOptions> loja) => Results.Ok(loja.CurrentValue));
            app.MapGet("/mobile/api/loja", (IOptionsMonitor<LojaOptions> loja) => Results.Ok(loja.CurrentValue));
            app.Run();
        }

        private static bool PermitirOrigemLocal(string origem)
        {
            if (origem == "null")
            {
                return true;
            }
            if (!Uri.TryCreate(origem, UriKind.Absolute, out var endereco))
            {
                return false;
            }
            var hostLocal = endereco.Host == "localhost" || endereco.Host == "127.0.0.1" || endereco.Host == "[::1]";
            return endereco.Scheme == "http" && hostLocal;
        }
    }
}
