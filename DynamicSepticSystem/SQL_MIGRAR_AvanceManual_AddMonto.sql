-- SQL_MIGRAR_AvanceManual_AddMonto.sql
-- Script de migración: añade columna para almacenar montos ejecutados y constraints únicas
-- Ejecutar en la base de datos CALANDRIA (haga backup antes)
SET NOCOUNT ON;

-- BATCH 1: Añadir columna MontoEjecutado (si no existe)
IF COL_LENGTH('dbo.AvanceManualObra','MontoEjecutado') IS NULL
BEGIN
    ALTER TABLE dbo.AvanceManualObra
    ADD MontoEjecutado DECIMAL(18,2) NULL;
END
GO

-- BATCH 2: Asegurar ImporteTotal existe
IF COL_LENGTH('dbo.AvanceManualObra','ImporteTotal') IS NULL
BEGIN
    ALTER TABLE dbo.AvanceManualObra
    ADD ImporteTotal FLOAT NULL;
END
GO

-- BATCH 3: Crear constraint único (Manzana, Lote, WBS)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes ix
    JOIN sys.objects o ON ix.object_id = o.object_id
    WHERE o.name = 'AvanceManualObra' AND ix.is_unique = 1 AND ix.name = 'UQ_AvanceManualObra_Manzana_Lote_WBS'
)
BEGIN
    BEGIN TRY
        ALTER TABLE dbo.AvanceManualObra
        ADD CONSTRAINT UQ_AvanceManualObra_Manzana_Lote_WBS UNIQUE (Manzana, Lote, WBS);
    END TRY
    BEGIN CATCH
        PRINT 'No se pudo crear UQ_AvanceManualObra_Manzana_Lote_WBS. Verifique duplicados en la tabla.';
    END CATCH
END
GO

-- BATCH 4: AvanceManualConcepto: añadir MontoEjecutado
IF COL_LENGTH('dbo.AvanceManualConcepto','MontoEjecutado') IS NULL
BEGIN
    ALTER TABLE dbo.AvanceManualConcepto
    ADD MontoEjecutado DECIMAL(18,2) NULL;
END
GO

-- BATCH 5: Crear constraint único para AvanceManualConcepto (Manzana, Lote, Codigo)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes ix
    JOIN sys.objects o ON ix.object_id = o.object_id
    WHERE o.name = 'AvanceManualConcepto' AND ix.is_unique = 1 AND ix.name = 'UQ_AvanceManualConcepto_Manzana_Lote_Codigo'
)
BEGIN
    BEGIN TRY
        ALTER TABLE dbo.AvanceManualConcepto
        ADD CONSTRAINT UQ_AvanceManualConcepto_Manzana_Lote_Codigo UNIQUE (Manzana, Lote, Codigo);
    END TRY
    BEGIN CATCH
        PRINT 'No se pudo crear UQ_AvanceManualConcepto_Manzana_Lote_Codigo. Verifique duplicados en la tabla.';
    END CATCH
END
GO

-- BATCH 6: Inicializar MontoEjecutado a 0 donde sea NULL (opcional)
UPDATE dbo.AvanceManualObra
SET MontoEjecutado = 0
WHERE MontoEjecutado IS NULL;

UPDATE dbo.AvanceManualConcepto
SET MontoEjecutado = 0
WHERE MontoEjecutado IS NULL;
GO

PRINT 'Migración completada. Revise advertencias en caso de duplicados antes de crear constraints.';
