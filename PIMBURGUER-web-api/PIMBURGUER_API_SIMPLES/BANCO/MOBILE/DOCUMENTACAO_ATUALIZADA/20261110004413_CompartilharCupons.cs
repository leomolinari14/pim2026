using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PimBurguer.Api.Migrations
{
    /// <inheritdoc />
    public partial class CompartilharCupons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cupom_compartilhado_id",
                table: "cupons",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cupons_cliente",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cupom_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cupons_cliente", x => new { x.usuario_id, x.cupom_id });
                    table.ForeignKey(
                        name: "FK_cupons_cliente_cupons_cupom_id",
                        column: x => x.cupom_id,
                        principalTable: "cupons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cupons_cliente_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cupons_cupom_compartilhado_id",
                table: "cupons",
                column: "cupom_compartilhado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cupons_cliente_cupom_id",
                table: "cupons_cliente",
                column: "cupom_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cupons_cliente");

            migrationBuilder.DropIndex(
                name: "IX_cupons_cupom_compartilhado_id",
                table: "cupons");

            migrationBuilder.DropColumn(
                name: "cupom_compartilhado_id",
                table: "cupons");
        }
    }
}
