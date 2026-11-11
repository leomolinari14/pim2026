using Microsoft.EntityFrameworkCore;
using PimBurguer.Api.Models;

namespace PimBurguer.Api.Data
{
    public class PimBurguerDbContext : DbContext
    {
        public PimBurguerDbContext(DbContextOptions<PimBurguerDbContext> options) : base(options)
        {
        }

        protected PimBurguerDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Produto> Produtos
        {
            get
            {
                return Set<Produto>();
            }
        }

        public DbSet<Pedido> Pedidos
        {
            get
            {
                return Set<Pedido>();
            }
        }

        public DbSet<Usuario> Usuarios
        {
            get
            {
                return Set<Usuario>();
            }
        }

        public DbSet<SessaoAcesso> SessoesAcesso
        {
            get
            {
                return Set<SessaoAcesso>();
            }
        }

        public DbSet<Cupom> Cupons
        {
            get
            {
                return Set<Cupom>();
            }
        }

        public DbSet<CupomCliente> CuponsClientes => Set<CupomCliente>();

        public DbSet<UsoCupom> UsosCupom
        {
            get
            {
                return Set<UsoCupom>();
            }
        }

        public DbSet<MovimentacaoPedido> MovimentacoesPedido
        {
            get
            {
                return Set<MovimentacaoPedido>();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            MapeamentoPedido.Configurar(modelBuilder);
            MapeamentoConta.Configurar(modelBuilder);
            var produto = modelBuilder.Entity<Produto>();
            produto.ToTable("produtos", tabela => tabela.HasCheckConstraint("ck_produtos_preco_positivo", "preco > 0"));
            produto.HasKey(p => p.Id);
            // Começar novos IDs em 100 evita colisão com os exemplos de IDs 1 a 6.
            produto.Property(p => p.Id).HasColumnName("id").HasIdentityOptions(startValue: 100);
            produto.Property(p => p.Nome).HasColumnName("nome");
            produto.Property(p => p.Descricao).HasColumnName("descricao");
            produto.Property(p => p.Preco).HasColumnName("preco").HasPrecision(10, 2);
            produto.Property(p => p.Categoria).HasColumnName("categoria");
            produto.Property(p => p.Imagem).HasColumnName("imagem");
            produto.Property(p => p.Ativo).HasColumnName("ativo");
            // HasData vira INSERT na migration. Os registros ficam salvos no banco.
            produto.HasData(new Produto
            {
                Id = 1,
                Nome = "Pim Clássico",
                Descricao = "Pão, hambúrguer bovino, queijo e molho da casa.",
                Preco = 19.90m,
                Categoria = "Hambúrgueres",
                Imagem = "/imagens/produto-classico.png"
            }, new Produto
            {
                Id = 2,
                Nome = "Pim Bacon",
                Descricao = "Hambúrguer bovino, queijo, bacon crocante e molho da casa.",
                Preco = 24.90m,
                Categoria = "Hambúrgueres",
                Imagem = "/imagens/produto-bacon.png"
            }, new Produto
            {
                Id = 3,
                Nome = "Pim Duplo",
                Descricao = "Dois hambúrgueres bovinos, queijo e molho da casa.",
                Preco = 29.90m,
                Categoria = "Hambúrgueres",
                Imagem = "/imagens/produto-duplo.png"
            }, new Produto
            {
                Id = 4,
                Nome = "Batata frita",
                Descricao = "Porção de batatas douradas e crocantes.",
                Preco = 12.00m,
                Categoria = "Acompanhamentos",
                Imagem = "/imagens/produto-batata.png"
            }, new Produto
            {
                Id = 5,
                Nome = "Refrigerante",
                Descricao = "Uma bebida gelada para acompanhar seu lanche.",
                Preco = 6.00m,
                Categoria = "Bebidas",
                Imagem = "/imagens/produto-refrigerante.png"
            }, new Produto
            {
                Id = 6,
                Nome = "Suco de laranja",
                Descricao = "Suco de laranja para acompanhar seu pedido.",
                Preco = 8.00m,
                Categoria = "Bebidas",
                Imagem = "/imagens/produto-suco.png"
            });
        }
    }
}
