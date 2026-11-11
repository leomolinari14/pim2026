using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PimBurguer.Api.Data
{
    // As ferramentas de migrations precisam do contexto, sem iniciar a API
    // nem tentar criar o administrador antes de as tabelas estarem prontas.
    public class PimBurguerDbContextFactory : IDesignTimeDbContextFactory<PimBurguerDbContext>
    {
        public PimBurguerDbContext CreateDbContext(string[] args)
        {
            var indice = Array.IndexOf(args, "--environment");
            var ambiente = indice >= 0 && indice + 1 < args.Length ? args[indice + 1] : Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var configuracao = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json").AddJsonFile($"appsettings.{ambiente}.json", optional: true).AddEnvironmentVariables().Build();
            var conexao = configuracao.GetConnectionString("PimBurguer") ?? throw new InvalidOperationException("Configure ConnectionStrings:PimBurguer.");
            return new PimBurguerDbContext(new DbContextOptionsBuilder<PimBurguerDbContext>().UseNpgsql(conexao).Options);
        }
    }
}
