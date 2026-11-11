using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PimBurguer.Api.Data;

namespace PimBurguer.Api.Migrations;

public sealed partial class SepararBancosIntegracao : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(IntegracaoSql.Criar);
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(IntegracaoSql.Remover);
}
