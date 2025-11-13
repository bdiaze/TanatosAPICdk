CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251113165331_InitialCreation') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'tanatos') THEN
            CREATE SCHEMA tanatos;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251113165331_InitialCreation') THEN
    CREATE TABLE tanatos.tipo_receptor_notificacion (
        id bigint NOT NULL,
        nombre text NOT NULL,
        vigente boolean NOT NULL,
        CONSTRAINT "PK_tipo_receptor_notificacion" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251113165331_InitialCreation') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251113165331_InitialCreation', '9.0.11');
    END IF;
END $EF$;
COMMIT;

