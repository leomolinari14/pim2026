using Microsoft.EntityFrameworkCore.Migrations;
using PimBurguer.Api.Data;

namespace PimBurguer.Api.Migrations;

public sealed partial class RemoverIntegracaoMobile : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DO $triggers$
        DECLARE t text;
        BEGIN
          FOREACH t IN ARRAY ARRAY['produtos','usuarios','cupons','pedidos','itens_pedido','usos_cupom','movimentacoes_pedido']
          LOOP EXECUTE format('DROP TRIGGER IF EXISTS integracao_alteracao ON %I', t); END LOOP;
        END $triggers$;
        DROP FUNCTION IF EXISTS registrar_integracao();
        DROP TABLE IF EXISTS integracao_pendentes, integracao_versoes, integracao_estado;
        DO $sequencias$
        DECLARE t text; s text;
        BEGIN
          FOREACH t IN ARRAY ARRAY['produtos','cupons','itens_pedido','usos_cupom','movimentacoes_pedido']
          LOOP
            s := pg_get_serial_sequence(t, 'id');
            IF s IS NOT NULL THEN EXECUTE format('ALTER SEQUENCE %s INCREMENT BY 1', s); END IF;
          END LOOP;
        END $sequencias$;
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(IntegracaoSql.Criar);
}
