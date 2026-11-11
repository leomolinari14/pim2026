START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261110004413_CompartilharCupons') THEN
    ALTER TABLE cupons ADD cupom_compartilhado_id integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261110004413_CompartilharCupons') THEN
    CREATE TABLE cupons_cliente (
        usuario_id uuid NOT NULL,
        cupom_id integer NOT NULL,
        CONSTRAINT "PK_cupons_cliente" PRIMARY KEY (usuario_id, cupom_id),
        CONSTRAINT "FK_cupons_cliente_cupons_cupom_id" FOREIGN KEY (cupom_id) REFERENCES cupons (id) ON DELETE CASCADE,
        CONSTRAINT "FK_cupons_cliente_usuarios_usuario_id" FOREIGN KEY (usuario_id) REFERENCES usuarios (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261110004413_CompartilharCupons') THEN
    CREATE UNIQUE INDEX "IX_cupons_cupom_compartilhado_id" ON cupons (cupom_compartilhado_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261110004413_CompartilharCupons') THEN
    CREATE INDEX "IX_cupons_cliente_cupom_id" ON cupons_cliente (cupom_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261110004413_CompartilharCupons') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261110004413_CompartilharCupons', '9.0.20');
    END IF;
END $EF$;
COMMIT;

