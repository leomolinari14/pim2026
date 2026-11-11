using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PimBurguer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarUsuarioAdministrador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "email_normalizado",
                table: "usuarios",
                type: "character varying(180)",
                maxLength: 180,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(180)",
                oldMaxLength: 180);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "usuarios",
                type: "character varying(180)",
                maxLength: 180,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(180)",
                oldMaxLength: 180);

            migrationBuilder.AddColumn<string>(
                name: "nome_usuario",
                table: "usuarios",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_nome_usuario",
                table: "usuarios",
                column: "nome_usuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_usuarios_nome_usuario",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "nome_usuario",
                table: "usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "email_normalizado",
                table: "usuarios",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(180)",
                oldMaxLength: 180,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "usuarios",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(180)",
                oldMaxLength: 180,
                oldNullable: true);
        }
    }
}
