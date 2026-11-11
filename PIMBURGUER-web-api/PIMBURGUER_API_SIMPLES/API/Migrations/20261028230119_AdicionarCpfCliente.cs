using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PimBurguer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCpfCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cpf",
                table: "usuarios",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cpf_cliente",
                table: "pedidos",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cpf",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "cpf_cliente",
                table: "pedidos");
        }
    }
}
