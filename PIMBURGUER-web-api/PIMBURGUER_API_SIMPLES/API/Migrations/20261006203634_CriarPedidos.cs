using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PimBurguer.Api.Migrations
{
    /// <inheritdoc />
    public partial class CriarPedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pedidos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    nome_cliente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    recebimento = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    logradouro = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    numero = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    complemento = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    bairro = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    cidade = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    pagamento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    troco_para = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    subtotal = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    taxa_entrega = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    whatsapp_loja = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    requisicao_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedidos", x => x.id);
                    table.CheckConstraint("ck_pedidos_recebimento", "recebimento IN ('Retirada', 'Entrega')");
                    table.CheckConstraint("ck_pedidos_valores", "subtotal > 0 AND taxa_entrega >= 0 AND total = subtotal + taxa_entrega");
                });

            migrationBuilder.CreateTable(
                name: "itens_pedido",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<int>(type: "integer", nullable: false),
                    nome_produto = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    preco_unitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_pedido", x => x.id);
                    table.CheckConstraint("ck_itens_pedido_quantidade", "quantidade BETWEEN 1 AND 20");
                    table.CheckConstraint("ck_itens_pedido_valores", "preco_unitario > 0 AND subtotal = quantidade * preco_unitario");
                    table.ForeignKey(
                        name: "FK_itens_pedido_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_itens_pedido_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_itens_pedido_pedido_id_produto_id",
                table: "itens_pedido",
                columns: new[] { "pedido_id", "produto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_itens_pedido_produto_id",
                table: "itens_pedido",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_criado_em",
                table: "pedidos",
                column: "criado_em");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "itens_pedido");

            migrationBuilder.DropTable(
                name: "pedidos");
        }
    }
}
