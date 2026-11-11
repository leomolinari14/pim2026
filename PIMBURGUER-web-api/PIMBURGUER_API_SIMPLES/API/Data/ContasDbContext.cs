using Microsoft.EntityFrameworkCore;

namespace PimBurguer.Api.Data
{
    // Contas e sessões usam sempre a conexão web, inclusive nas rotas mobile.
    public class ContasDbContext : PimBurguerDbContext
    {
        public ContasDbContext(DbContextOptions<ContasDbContext> options) : base(options)
        {
        }
    }
}
