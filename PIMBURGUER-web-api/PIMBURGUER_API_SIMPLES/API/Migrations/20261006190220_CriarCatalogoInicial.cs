using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PimBurguer.Api.Migrations
{
    /// <inheritdoc />
    public partial class CriarCatalogoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "produtos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    preco = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    categoria = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    imagem = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produtos", x => x.id);
                    table.CheckConstraint("ck_produtos_preco_positivo", "preco > 0");
                });

            migrationBuilder.InsertData(
                table: "produtos",
                columns: new[] { "id", "ativo", "categoria", "descricao", "imagem", "nome", "preco" },
                values: new object[,]
                {
                    { 1, true, "Hambúrgueres", "Pão, hambúrguer bovino, queijo e molho da casa.", "/imagens/produto-classico.png", "Pim Clássico", 19.90m },
                    { 2, true, "Hambúrgueres", "Hambúrguer bovino, queijo, bacon crocante e molho da casa.", "/imagens/produto-bacon.png", "Pim Bacon", 24.90m },
                    { 3, true, "Hambúrgueres", "Dois hambúrgueres bovinos, queijo e molho da casa.", "/imagens/produto-duplo.png", "Pim Duplo", 29.90m },
                    { 4, true, "Acompanhamentos", "Porção de batatas douradas e crocantes.", "/imagens/produto-batata.png", "Batata frita", 12.00m },
                    { 5, true, "Bebidas", "Uma bebida gelada para acompanhar seu lanche.", "/imagens/produto-refrigerante.png", "Refrigerante", 6.00m },
                    { 6, true, "Bebidas", "Suco de laranja para acompanhar seu pedido.", "/imagens/produto-suco.png", "Suco de laranja", 8.00m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "produtos");
        }
    }
}
