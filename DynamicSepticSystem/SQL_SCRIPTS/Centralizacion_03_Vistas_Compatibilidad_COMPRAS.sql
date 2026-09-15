/*
  Centralizacion de insumos · FASE 2 — Reemplazar COMPRAS* por VISTAS sobre Insumos*EXP.

  Tras el backfill (FASE 1), todas las claves de COMPRAS* ya viven en Insumos*EXP.
  Aqui se ELIMINAN las tablas COMPRASTUNERA/COMPRASCALANDRA/COMPRASINDIRECTAS y se
  recrean como VISTAS sobre el catalogo canonico, exponiendo las mismas columnas
  (incluido [Descripción] AS Descripcion) para que el codigo existente que aun lee
  esas tablas (FormEditarExplosiones, maximos del almacen, ComprasController.Catalogo/
  Upsert/Eliminar) siga funcionando sin cambios contra un unico catalogo fisico.

  Las vistas COMPRASTUNERA/COMPRASCALANDRA son ACTUALIZABLES (una sola tabla base,
  columnas simples): INSERT/UPDATE/DELETE escriben directo en Insumos*EXP.
  COMPRASINDIRECTAS se recrea como vista de SOLO LECTURA (sus altas/consultas ya
  van por /api/compras/indirectas, repuntado en codigo).

  SEGURIDAD: aborta si alguna clave de COMPRAS* aun no esta en Insumos*EXP
  (señal de que el backfill no se corrio). Correr DESPUES de FASE 1.

  Archivo UTF-8 con BOM (contiene la columna acentuada [Descripción]).
*/
-- Ejecutar conectado a la BD CALANDRIA (el .bat lo hace con: sqlcmd -d CALANDRIA).
SET NOCOUNT ON;
SET XACT_ABORT ON;

-- ---- Guarda de seguridad: no perder claves ----
-- Nota: se ignoran las claves NULL/vacias (filas basura que el backfill descarta a proposito).
IF OBJECT_ID(N'dbo.COMPRASTUNERA', N'U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM dbo.COMPRASTUNERA c
               WHERE c.Clave IS NOT NULL AND LTRIM(RTRIM(c.Clave)) <> ''
                 AND NOT EXISTS (SELECT 1 FROM dbo.InsumosTuneraEXP i WHERE i.Clave = c.Clave))
BEGIN
    RAISERROR('ABORTADO: COMPRASTUNERA tiene claves que faltan en InsumosTuneraEXP. Corre primero FASE 1 (backfill).', 16, 1);
    RETURN;
END
IF OBJECT_ID(N'dbo.COMPRASCALANDRA', N'U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM dbo.COMPRASCALANDRA c
               WHERE c.Clave IS NOT NULL AND LTRIM(RTRIM(c.Clave)) <> ''
                 AND NOT EXISTS (SELECT 1 FROM dbo.InsumosCalandraEXP i WHERE i.Clave = c.Clave))
BEGIN
    RAISERROR('ABORTADO: COMPRASCALANDRA tiene claves que faltan en InsumosCalandraEXP. Corre primero FASE 1 (backfill).', 16, 1);
    RETURN;
END
IF OBJECT_ID(N'dbo.COMPRASINDIRECTAS', N'U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM dbo.COMPRASINDIRECTAS ind
               WHERE ind.Clave IS NOT NULL AND LTRIM(RTRIM(ind.Clave)) <> ''
                 AND NOT EXISTS (SELECT 1 FROM dbo.InsumosTuneraEXP   i WHERE i.Clave = ind.Clave)
                 AND NOT EXISTS (SELECT 1 FROM dbo.InsumosCalandraEXP i WHERE i.Clave = ind.Clave))
BEGIN
    RAISERROR('ABORTADO: COMPRASINDIRECTAS tiene claves sin absorber en Insumos*EXP. Corre primero FASE 1 (backfill).', 16, 1);
    RETURN;
END
GO

-- ---- COMPRASTUNERA: tabla -> vista actualizable ----
IF OBJECT_ID(N'dbo.COMPRASTUNERA', N'U') IS NOT NULL DROP TABLE dbo.COMPRASTUNERA;
IF OBJECT_ID(N'dbo.COMPRASTUNERA', N'V') IS NOT NULL DROP VIEW dbo.COMPRASTUNERA;
GO
CREATE VIEW dbo.COMPRASTUNERA AS
    SELECT [Clave], [Descripción] AS [Descripcion], [Unidad], [Cantidad], [Costo], [Familia]
    FROM dbo.InsumosTuneraEXP;
GO

-- ---- COMPRASCALANDRA: tabla -> vista actualizable ----
IF OBJECT_ID(N'dbo.COMPRASCALANDRA', N'U') IS NOT NULL DROP TABLE dbo.COMPRASCALANDRA;
IF OBJECT_ID(N'dbo.COMPRASCALANDRA', N'V') IS NOT NULL DROP VIEW dbo.COMPRASCALANDRA;
GO
CREATE VIEW dbo.COMPRASCALANDRA AS
    SELECT [Clave], [Descripción] AS [Descripcion], [Unidad], [Cantidad], [Costo], [Familia]
    FROM dbo.InsumosCalandraEXP;
GO

-- ---- COMPRASINDIRECTAS: tabla -> vista de SOLO LECTURA (altas/consultas via API) ----
IF OBJECT_ID(N'dbo.COMPRASINDIRECTAS', N'U') IS NOT NULL DROP TABLE dbo.COMPRASINDIRECTAS;
IF OBJECT_ID(N'dbo.COMPRASINDIRECTAS', N'V') IS NOT NULL DROP VIEW dbo.COMPRASINDIRECTAS;
GO
CREATE VIEW dbo.COMPRASINDIRECTAS AS
    SELECT Clave, MAX(Descripcion) AS Descripcion, MAX(Unidad) AS Unidad
    FROM (
        SELECT [Clave], [Descripción] AS Descripcion, [Unidad] FROM dbo.InsumosTuneraEXP   WHERE [Tipo] = N'Indirecto'
        UNION ALL
        SELECT [Clave], [Descripción] AS Descripcion, [Unidad] FROM dbo.InsumosCalandraEXP WHERE [Tipo] = N'Indirecto'
    ) x
    GROUP BY Clave;
GO

PRINT '=== Vistas creadas. COMPRAS* ahora apuntan a Insumos*EXP (unico catalogo). ===';
GO
