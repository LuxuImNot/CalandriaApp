-- ========================================
-- Script: Agregar columna NombreOrden a tablas de órdenes de compra
-- Fecha: 2025-01-XX
-- Descripción: Añade la columna NombreOrden para facilitar la identificación
--              de las órdenes de compra sin depender solo del folio
-- ========================================

USE CALANDRIA;
GO

PRINT '=== AGREGANDO COLUMNA NombreOrden A TABLAS DE ÓRDENES ===';
PRINT '';

-- ================================================================
-- 1. Agregar NombreOrden a OrdenesCompra
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[OrdenesCompra]') 
               AND name = 'NombreOrden')
BEGIN
    ALTER TABLE OrdenesCompra
    ADD NombreOrden NVARCHAR(200) NULL;
    
    PRINT '? Columna NombreOrden agregada a OrdenesCompra';
END
ELSE
BEGIN
    PRINT '?? Columna NombreOrden ya existe en OrdenesCompra';
END
GO

-- ================================================================
-- 2. Agregar NombreOrden a FoliosOrdenCompra (si existe)
-- ================================================================
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FoliosOrdenCompra')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns 
                   WHERE object_id = OBJECT_ID(N'[dbo].[FoliosOrdenCompra]') 
                   AND name = 'NombreOrden')
    BEGIN
        ALTER TABLE FoliosOrdenCompra
        ADD NombreOrden NVARCHAR(200) NULL;
        
        PRINT '? Columna NombreOrden agregada a FoliosOrdenCompra';
    END
    ELSE
    BEGIN
        PRINT '?? Columna NombreOrden ya existe en FoliosOrdenCompra';
    END
END
ELSE
BEGIN
    PRINT '?? Tabla FoliosOrdenCompra no existe (puede ser normal si no usas repositorio)';
END
GO

-- ================================================================
-- 3. Actualizar órdenes existentes con nombres por defecto
-- ================================================================
-- Actualizar OrdenesCompra con nombres descriptivos por defecto
UPDATE OrdenesCompra
SET NombreOrden = CASE 
    WHEN TipoOrden = 'MULTIPLE' THEN 'Orden Múltiple - ' + CONVERT(VARCHAR(10), Fecha, 103)
    WHEN TipoOrden = 'INDIRECTA' THEN 'Compra Indirecta - ' + CONVERT(VARCHAR(10), Fecha, 103)
    WHEN TipoOrden = 'ADMINISTRATIVA' THEN 'Compra Administrativa - ' + CONVERT(VARCHAR(10), Fecha, 103)
    ELSE 'Orden de Compra - ' + CONVERT(VARCHAR(10), Fecha, 103)
END
WHERE NombreOrden IS NULL;

PRINT '? Órdenes existentes actualizadas con nombres por defecto';
GO

-- Actualizar FoliosOrdenCompra si existe
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FoliosOrdenCompra')
BEGIN
    UPDATE FoliosOrdenCompra
    SET NombreOrden = CASE 
        WHEN TipoOrden = 'MULTIPLE' THEN 'Orden Múltiple - ' + CONVERT(VARCHAR(10), FechaGeneracion, 103)
        WHEN TipoOrden = 'INDIRECTA' THEN 'Compra Indirecta - ' + CONVERT(VARCHAR(10), FechaGeneracion, 103)
        WHEN TipoOrden = 'ADMINISTRATIVA' THEN 'Compra Administrativa - ' + CONVERT(VARCHAR(10), FechaGeneracion, 103)
        ELSE 'Orden de Compra - ' + CONVERT(VARCHAR(10), FechaGeneracion, 103)
    END
    WHERE NombreOrden IS NULL;
    
    PRINT '? FoliosOrdenCompra actualizados con nombres por defecto';
END
GO

-- ================================================================
-- 4. Mostrar resumen de órdenes con nombres
-- ================================================================
PRINT '';
PRINT '=== RESUMEN DE ÓRDENES CON NOMBRES ===';
PRINT '';

SELECT 
    FolioOC AS [Folio],
    NombreOrden AS [Nombre],
    TipoOrden AS [Tipo],
    Fecha AS [Fecha],
    Usuario AS [Usuario]
FROM OrdenesCompra
ORDER BY Fecha DESC;

PRINT '';
PRINT '========================================';
PRINT '? SCRIPT COMPLETADO EXITOSAMENTE';
PRINT '========================================';
PRINT '';
PRINT 'Ahora puedes:';
PRINT '1. Ver las órdenes con sus nombres en FormAlmacen';
PRINT '2. Editar los nombres para personalizarlos';
PRINT '3. El folio sigue siendo el identificador único';
GO
