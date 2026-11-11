using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PimBurguer.Api.Migrations
{
    /// <inheritdoc />
    public partial class CriarContasPaineisECupons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_pedidos_valores",
                table: "pedidos");

            migrationBuilder.AddColumn<bool>(
                name: "removido",
                table: "produtos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "codigo_cupom",
                table: "pedidos",
                type: "character varying(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cupom_id",
                table: "pedidos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "desconto",
                table: "pedidos",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "pago",
                table: "pedidos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "pago_em",
                table: "pedidos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "usuario_id",
                table: "pedidos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cupons",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    tipo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    compra_minima = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    valido_de = table.Column<DateOnly>(type: "date", nullable: false),
                    valido_ate = table.Column<DateOnly>(type: "date", nullable: false),
                    limite_usos = table.Column<int>(type: "integer", nullable: true),
                    uma_vez_por_cliente = table.Column<bool>(type: "boolean", nullable: false),
                    divulgar = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cupons", x => x.id);
                    table.CheckConstraint("ck_cupons_validade", "valido_ate >= valido_de AND (limite_usos IS NULL OR limite_usos > 0)");
                    table.CheckConstraint("ck_cupons_valor", "valor > 0 AND compra_minima >= 0 AND tipo IN ('Percentual','ValorFixo') AND (tipo <> 'Percentual' OR valor <= 100)");
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    email_normalizado = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    senha_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    perfil = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    falhas_login = table.Column<int>(type: "integer", nullable: false),
                    bloqueado_ate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                    table.CheckConstraint("ck_usuarios_perfil", "perfil IN ('Cliente', 'Administrador')");
                });

            migrationBuilder.CreateTable(
                name: "movimentacoes_pedido",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
                    administrador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status_anterior = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    status_novo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    pago_anterior = table.Column<bool>(type: "boolean", nullable: false),
                    pago_novo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimentacoes_pedido", x => x.id);
                    table.ForeignKey(
                        name: "FK_movimentacoes_pedido_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movimentacoes_pedido_usuarios_administrador_id",
                        column: x => x.administrador_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sessoes_acesso",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    expira_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessoes_acesso", x => x.id);
                    table.ForeignKey(
                        name: "FK_sessoes_acesso_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usos_cupom",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cupom_id = table.Column<int>(type: "integer", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usos_cupom", x => x.id);
                    table.ForeignKey(
                        name: "FK_usos_cupom_cupons_cupom_id",
                        column: x => x.cupom_id,
                        principalTable: "cupons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usos_cupom_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usos_cupom_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "produtos",
                keyColumn: "id",
                keyValue: 1,
                column: "removido",
                value: false);

            migrationBuilder.UpdateData(
                table: "produtos",
                keyColumn: "id",
                keyValue: 2,
                column: "removido",
                value: false);

            migrationBuilder.UpdateData(
                table: "produtos",
                keyColumn: "id",
                keyValue: 3,
                column: "removido",
                value: false);

            migrationBuilder.UpdateData(
                table: "produtos",
                keyColumn: "id",
                keyValue: 4,
                column: "removido",
                value: false);

            migrationBuilder.UpdateData(
                table: "produtos",
                keyColumn: "id",
                keyValue: 5,
                column: "removido",
                value: false);

            migrationBuilder.UpdateData(
                table: "produtos",
                keyColumn: "id",
                keyValue: 6,
                column: "removido",
                value: false);

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_cupom_id",
                table: "pedidos",
                column: "cupom_id");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_usuario_id",
                table: "pedidos",
                column: "usuario_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_pedidos_valores",
                table: "pedidos",
                sql: "subtotal > 0 AND taxa_entrega >= 0 AND desconto >= 0 AND desconto <= subtotal AND total = subtotal + taxa_entrega - desconto");

            migrationBuilder.CreateIndex(
                name: "IX_cupons_codigo",
                table: "cupons",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_movimentacoes_pedido_administrador_id",
                table: "movimentacoes_pedido",
                column: "administrador_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimentacoes_pedido_pedido_id",
                table: "movimentacoes_pedido",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "IX_sessoes_acesso_token_hash",
                table: "sessoes_acesso",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sessoes_acesso_usuario_id",
                table: "sessoes_acesso",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_usos_cupom_cupom_id_usuario_id",
                table: "usos_cupom",
                columns: new[] { "cupom_id", "usuario_id" });

            migrationBuilder.CreateIndex(
                name: "IX_usos_cupom_pedido_id",
                table: "usos_cupom",
                column: "pedido_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usos_cupom_usuario_id",
                table: "usos_cupom",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email_normalizado",
                table: "usuarios",
                column: "email_normalizado",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_pedidos_cupons_cupom_id",
                table: "pedidos",
                column: "cupom_id",
                principalTable: "cupons",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_pedidos_usuarios_usuario_id",
                table: "pedidos",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pedidos_cupons_cupom_id",
                table: "pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_pedidos_usuarios_usuario_id",
                table: "pedidos");

            migrationBuilder.DropTable(
                name: "movimentacoes_pedido");

            migrationBuilder.DropTable(
                name: "sessoes_acesso");

            migrationBuilder.DropTable(
                name: "usos_cupom");

            migrationBuilder.DropTable(
                name: "cupons");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropIndex(
                name: "IX_pedidos_cupom_id",
                table: "pedidos");

            migrationBuilder.DropIndex(
                name: "IX_pedidos_usuario_id",
                table: "pedidos");

            migrationBuilder.DropCheckConstraint(
                name: "ck_pedidos_valores",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "removido",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "codigo_cupom",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "cupom_id",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "desconto",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "pago",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "pago_em",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "usuario_id",
                table: "pedidos");

            migrationBuilder.AddCheckConstraint(
                name: "ck_pedidos_valores",
                table: "pedidos",
                sql: "subtotal > 0 AND taxa_entrega >= 0 AND total = subtotal + taxa_entrega");
        }
    }
}
