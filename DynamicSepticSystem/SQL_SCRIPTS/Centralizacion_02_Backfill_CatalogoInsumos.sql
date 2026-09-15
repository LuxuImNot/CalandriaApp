/*
  Centralizacion de insumos · FASE 1 — BACKFILL hacia el catalogo canonico.

  Catalogo canonico: InsumosTuneraEXP / InsumosCalandraEXP (el de DESTAJOS).
  Regla: Insumos*EXP DOMINA -> este script SOLO inserta claves que FALTEN; nunca
  sobrescribe descripcion/costo/unidad de una clave ya existente en Insumos*EXP.

  Pasos:
    1) Asegura que ambas tablas EXP existan y tengan Tipo/Familia (+ DEFAULT 'Materiales' en Tipo).
    2) Copia las claves de COMPRASTUNERA/COMPRASCALANDRA que falten en su EXP.
    3) Absorbe COMPRASINDIRECTAS en AMBAS EXP con Tipo='Indirecto' (solo las que falten).

  Idempotente: se puede correr varias veces sin duplicar.
  Correr DESPUES del diagnostico (FASE 0) y ANTES de crear las vistas (FASE 2).

  Archivo UTF-8 con BOM (contiene la columna acentuada [Descripción]).
*/
-- Ejecutar conectado a la BD CALANDRIA (el .bat lo hace con: sqlcmd -d CALANDRIA).
SET NOCOUNT ON;
SET XACT_ABORT ON;

-- ----------------------------------------------------------------------------
-- 1) Asegurar esquema canonico de ambas tablas EXP.
-- ----------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.InsumosTuneraEXP', N'U') IS NULL
    CREATE TABLE dbo.InsumosTuneraEXP(
        [Clave] NVARCHAR(50) NOT NULL,
        [Descripción] NVARCHAR(1000) NULL, [Unidad] NVARCHAR(20) NULL,
        [Cantidad] DECIMAL(18,6) NULL, [Costo] DECIMAL(18,4) NULL,
        [Importe] DECIMAL(18,4) NULL, [Porcentaje] DECIMAL(18,12) NULL,
        [Tipo] NVARCHAR(50) NULL, [Familia] NVARCHAR(80) NULL,
        CONSTRAINT [PK_InsumosTuneraEXP] PRIMARY KEY CLUSTERED ([Clave] ASC));
GO
IF OBJECT_ID(N'dbo.InsumosCalandraEXP', N'U') IS NULL
    CREATE TABLE dbo.InsumosCalandraEXP(
        [Clave] NVARCHAR(50) NOT NULL,
        [Descripción] NVARCHAR(1000) NULL, [Unidad] NVARCHAR(20) NULL,
        [Cantidad] DECIMAL(18,6) NULL, [Costo] DECIMAL(18,4) NULL,
        [Importe] DECIMAL(18,4) NULL, [Porcentaje] DECIMAL(18,12) NULL,
        [Tipo] NVARCHAR(50) NULL, [Familia] NVARCHAR(80) NULL,
        CONSTRAINT [PK_InsumosCalandraEXP] PRIMARY KEY CLUSTERED ([Clave] ASC));
GO

-- Columnas Tipo/Familia por compatibilidad con tablas antiguas.
IF COL_LENGTH('dbo.InsumosTuneraEXP',N'Tipo')      IS NULL ALTER TABLE dbo.InsumosTuneraEXP      ADD [Tipo] NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.InsumosTuneraEXP',N'Familia')   IS NULL ALTER TABLE dbo.InsumosTuneraEXP      ADD [Familia] NVARCHAR(80) NULL;
IF COL_LENGTH('dbo.InsumosCalandraEXP',N'Tipo')    IS NULL ALTER TABLE dbo.InsumosCalandraEXP    ADD [Tipo] NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.InsumosCalandraEXP',N'Familia') IS NULL ALTER TABLE dbo.InsumosCalandraEXP    ADD [Familia] NVARCHAR(80) NULL;
GO

-- DEFAULT 'Materiales' en Tipo: las altas hechas desde Compras (via las vistas de
-- compatibilidad, que no incluyen Tipo) quedaran clasificadas como Materiales.
IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_InsumosTuneraEXP_Tipo')
    ALTER TABLE dbo.InsumosTuneraEXP   ADD CONSTRAINT DF_InsumosTuneraEXP_Tipo   DEFAULT N'Materiales' FOR [Tipo];
IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_InsumosCalandraEXP_Tipo')
    ALTER TABLE dbo.InsumosCalandraEXP ADD CONSTRAINT DF_InsumosCalandraEXP_Tipo DEFAULT N'Materiales' FOR [Tipo];
GO

-- ----------------------------------------------------------------------------
-- 2) Copiar claves faltantes de COMPRAS* hacia su EXP (sin sobrescribir).
-- ----------------------------------------------------------------------------
DECLARE @t INT = 0, @c INT = 0;

-- Nota: COMPRAS* puede traer filas basura (Clave NULL/vacia, p.ej. subtotales) o
-- claves repetidas. Se filtran las NULL/vacias y se deduplica por Clave (rn = 1)
-- para no violar la PK de Insumos*EXP.
IF OBJECT_ID(N'dbo.COMPRASTUNERA', N'U') IS NOT NULL
BEGIN
    ;WITH src AS (
        SELECT c.Clave, c.Descripcion, c.Unidad, c.Cantidad, c.Costo, c.Familia,
               rn = ROW_NUMBER() OVER (PARTITION BY c.Clave ORDER BY (SELECT NULL))
        FROM dbo.COMPRASTUNERA c
        WHERE c.Clave IS NOT NULL AND LTRIM(RTRIM(c.Clave)) <> ''
    )
    INSERT INTO dbo.InsumosTuneraEXP ([Clave],[Descripción],[Unidad],[Cantidad],[Costo],[Familia],[Tipo])
    SELECT src.Clave, src.Descripcion, src.Unidad, src.Cantidad, src.Costo, src.Familia, N'Materiales'
    FROM src
    WHERE src.rn = 1
      AND NOT EXISTS (SELECT 1 FROM dbo.InsumosTuneraEXP i WHERE i.Clave = src.Clave);
    SET @t = @@ROWCOUNT;
END

IF OBJECT_ID(N'dbo.COMPRASCALANDRA', N'U') IS NOT NULL
BEGIN
    ;WITH src AS (
        SELECT c.Clave, c.Descripcion, c.Unidad, c.Cantidad, c.Costo, c.Familia,
               rn = ROW_NUMBER() OVER (PARTITION BY c.Clave ORDER BY (SELECT NULL))
        FROM dbo.COMPRASCALANDRA c
        WHERE c.Clave IS NOT NULL AND LTRIM(RTRIM(c.Clave)) <> ''
    )
    INSERT INTO dbo.InsumosCalandraEXP ([Clave],[Descripción],[Unidad],[Cantidad],[Costo],[Familia],[Tipo])
    SELECT src.Clave, src.Descripcion, src.Unidad, src.Cantidad, src.Costo, src.Familia, N'Materiales'
    FROM src
    WHERE src.rn = 1
      AND NOT EXISTS (SELECT 1 FROM dbo.InsumosCalandraEXP i WHERE i.Clave = src.Clave);
    SET @c = @@ROWCOUNT;
END
PRINT 'Claves copiadas de COMPRAS* -> EXP. Tunera: ' + CAST(@t AS NVARCHAR(10)) + ', Calandra: ' + CAST(@c AS NVARCHAR(10)) + '.';

-- ----------------------------------------------------------------------------
-- 3) Absorber COMPRASINDIRECTAS en AMBAS EXP con Tipo='Indirecto'.
--    (Las indirectas son globales, por eso se reflejan en los dos catalogos.
--     Solo se insertan las claves que no existan ya en cada tabla.)
-- ----------------------------------------------------------------------------
DECLARE @it INT = 0, @ic INT = 0;
IF OBJECT_ID(N'dbo.COMPRASINDIRECTAS', N'U') IS NOT NULL
BEGIN
    INSERT INTO dbo.InsumosTuneraEXP ([Clave],[Descripción],[Unidad],[Tipo])
    SELECT ind.Clave, ind.Descripcion, ind.Unidad, N'Indirecto'
    FROM dbo.COMPRASINDIRECTAS ind
    WHERE ind.Clave IS NOT NULL AND LTRIM(RTRIM(ind.Clave)) <> ''
      AND NOT EXISTS (SELECT 1 FROM dbo.InsumosTuneraEXP i WHERE i.Clave = ind.Clave);
    SET @it = @@ROWCOUNT;

    INSERT INTO dbo.InsumosCalandraEXP ([Clave],[Descripción],[Unidad],[Tipo])
    SELECT ind.Clave, ind.Descripcion, ind.Unidad, N'Indirecto'
    FROM dbo.COMPRASINDIRECTAS ind
    WHERE ind.Clave IS NOT NULL AND LTRIM(RTRIM(ind.Clave)) <> ''
      AND NOT EXISTS (SELECT 1 FROM dbo.InsumosCalandraEXP i WHERE i.Clave = ind.Clave);
    SET @ic = @@ROWCOUNT;
END
PRINT 'Indirectos absorbidos (Tipo=Indirecto). Tunera: ' + CAST(@it AS NVARCHAR(10)) + ', Calandra: ' + CAST(@ic AS NVARCHAR(10)) + '.';

PRINT '=== Backfill completado. Verifica y luego corre FASE 2 (vistas de compatibilidad). ===';
GO
