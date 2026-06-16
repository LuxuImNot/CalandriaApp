-- ========================================
-- Script: Agregar columnas PrecioUnitario e Importe a SalidasAlmacen
-- Fecha: 2025-01-XX
-- Descripción: Añade las columnas de precio e importe a la tabla de salidas
--              para mantener consistencia con las entradas
-- ========================================

USE CALANDRIA;
GO

-- Verificar si las columnas ya existen antes de agregarlas
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SalidasAlmacen]') AND name = 'PrecioUnitario')
BEGIN
    ALTER TABLE SalidasAlmacen
    ADD PrecioUnitario DECIMAL(18, 2) NULL;
    
    PRINT '? Columna PrecioUnitario agregada a SalidasAlmacen';
END
ELSE
BEGIN
    PRINT '?? Columna PrecioUnitario ya existe en SalidasAlmacen';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SalidasAlmacen]') AND name = 'Importe')
BEGIN
    ALTER TABLE SalidasAlmacen
    ADD Importe DECIMAL(18, 2) NULL;
    
    PRINT '? Columna Importe agregada a SalidasAlmacen';
END
ELSE
BEGIN
    PRINT '?? Columna Importe ya existe en SalidasAlmacen';
END
GO

-- Actualizar registros existentes con precios desde EntradasAlmacen
-- Esto establecerá el precio promedio de las entradas para cada clave
UPDATE sa
SET 
    sa.PrecioUnitario = ISNULL((
        SELECT AVG(ea.PrecioUnitario)
        FROM EntradasAlmacen ea
        WHERE ea.Clave = sa.Clave
    ), 0),
    sa.Importe = sa.Cantidad * ISNULL((
        SELECT AVG(ea.PrecioUnitario)
        FROM EntradasAlmacen ea
        WHERE ea.Clave = sa.Clave
    ), 0)
FROM SalidasAlmacen sa
WHERE sa.PrecioUnitario IS NULL OR sa.Importe IS NULL;

PRINT '? Registros existentes actualizados con precios promedio';
GO

-- Verificar columnas en HistorialMovimientos
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HistorialMovimientos]') AND name = 'PrecioUnitario')
BEGIN
    ALTER TABLE HistorialMovimientos
    ADD PrecioUnitario DECIMAL(18, 2) NULL;
    
    PRINT '? Columna PrecioUnitario agregada a HistorialMovimientos';
END
ELSE
BEGIN
    PRINT '?? Columna PrecioUnitario ya existe en HistorialMovimientos';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HistorialMovimientos]') AND name = 'Importe')
BEGIN
    ALTER TABLE HistorialMovimientos
    ADD Importe DECIMAL(18, 2) NULL;
    
    PRINT '? Columna Importe agregada a HistorialMovimientos';
END
ELSE
BEGIN
    PRINT '?? Columna Importe ya existe en HistorialMovimientos';
END
GO

-- Actualizar historial de salidas existente
UPDATE hm
SET 
    hm.PrecioUnitario = ISNULL((
        SELECT AVG(ea.PrecioUnitario)
        FROM EntradasAlmacen ea
        WHERE ea.Clave = hm.Clave
    ), 0),
    hm.Importe = hm.Cantidad * ISNULL((
        SELECT AVG(ea.PrecioUnitario)
        FROM EntradasAlmacen ea
        WHERE ea.Clave = hm.Clave
    ), 0)
FROM HistorialMovimientos hm
WHERE hm.TipoMovimiento = 'Salida' 
  AND (hm.PrecioUnitario IS NULL OR hm.Importe IS NULL);

PRINT '? Historial de salidas actualizado con precios promedio';
GO

-- Mostrar resumen
SELECT 
    'SalidasAlmacen' AS Tabla,
    COUNT(*) AS TotalRegistros,
    SUM(CASE WHEN PrecioUnitario IS NOT NULL THEN 1 ELSE 0 END) AS ConPrecio,
    SUM(CASE WHEN Importe IS NOT NULL THEN 1 ELSE 0 END) AS ConImporte
FROM SalidasAlmacen

UNION ALL

SELECT 
    'HistorialMovimientos (Salidas)' AS Tabla,
    COUNT(*) AS TotalRegistros,
    SUM(CASE WHEN PrecioUnitario IS NOT NULL THEN 1 ELSE 0 END) AS ConPrecio,
    SUM(CASE WHEN Importe IS NOT NULL THEN 1 ELSE 0 END) AS ConImporte
FROM HistorialMovimientos
WHERE TipoMovimiento = 'Salida';

PRINT '';
PRINT '========================================';
PRINT '? SCRIPT COMPLETADO EXITOSAMENTE';
PRINT '========================================';
GO
