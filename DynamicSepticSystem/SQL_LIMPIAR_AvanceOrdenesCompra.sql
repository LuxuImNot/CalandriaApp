-- ================================================================
-- QUERIES PARA LIMPIAR AVANCE DE ÓRDENES DE COMPRA
-- Sistema: Calandria Residencial
-- Fecha: 2025-01-14
-- ================================================================
-- IMPORTANTE: Ejecuta cada sección POR SEPARADO, no todo el archivo junto
-- ================================================================

-- ================================================================
-- 1. LIMPIAR TODAS LAS ENTRADAS DE ALMACÉN (REINICIAR COMPLETAMENTE)
-- ================================================================
-- ?? CUIDADO: Esto eliminará TODO el registro de entradas al almacén
-- Use solo si necesita reiniciar completamente el sistema

/*
-- Eliminar todas las entradas de almacén
DELETE FROM EntradasAlmacen;

-- Reiniciar el contador de identidad (si existe)
DBCC CHECKIDENT ('EntradasAlmacen', RESEED, 0);

-- Verificar que se eliminaron
SELECT COUNT(*) AS [Total Entradas Restantes] FROM EntradasAlmacen;
*/


-- ================================================================
-- 2. LIMPIAR ENTRADAS DE UNA ORDEN DE COMPRA ESPECÍFICA
-- ================================================================
-- Use esto para eliminar las entradas de una orden específica

/*
DECLARE @FolioOC NVARCHAR(50) = 'OC-MULTI-20250114-001'; -- ?? CAMBIAR POR EL FOLIO DESEADO

DELETE FROM EntradasAlmacen
WHERE FolioOC = @FolioOC;

-- Verificar
SELECT * FROM EntradasAlmacen WHERE FolioOC = @FolioOC;
*/


-- ================================================================
-- 3. LIMPIAR ENTRADAS DE UNA CASA ESPECÍFICA
-- ================================================================
-- Use esto para eliminar las entradas relacionadas a una casa

/*
DECLARE @Manzana NVARCHAR(10) = '1';  -- ?? CAMBIAR
DECLARE @Lote NVARCHAR(10) = '2';     -- ?? CAMBIAR

DELETE FROM EntradasAlmacen
WHERE Manzana = @Manzana AND Lote = @Lote;

-- Verificar
SELECT * FROM EntradasAlmacen WHERE Manzana = @Manzana AND Lote = @Lote;
*/


-- ================================================================
-- 4. LIMPIAR ENTRADAS POR RANGO DE FECHAS
-- ================================================================
-- Use esto para eliminar entradas de un período específico

/*
DELETE FROM EntradasAlmacen
WHERE FechaEntrada BETWEEN '2025-01-01' AND '2025-01-14';

-- Verificar
SELECT * FROM EntradasAlmacen 
WHERE FechaEntrada BETWEEN '2025-01-01' AND '2025-01-14';
*/


-- ================================================================
-- 5. RESETEAR ESTADO DE ÓRDENES A 'PENDIENTE'
-- ================================================================
-- Use esto para marcar todas las órdenes como pendientes nuevamente

/*
UPDATE OrdenesCompraDetalle
SET Estado = 'PENDIENTE';

-- Verificar
SELECT Estado, COUNT(*) AS Total
FROM OrdenesCompraDetalle
GROUP BY Estado;
*/


-- ================================================================
-- 6. RESETEAR ESTADO DE ÓRDENES ESPECÍFICAS
-- ================================================================
-- Use esto para marcar órdenes específicas como pendientes

/*
DECLARE @FolioOC NVARCHAR(50) = 'OC-MULTI-20250114-001'; -- ?? CAMBIAR

UPDATE OrdenesCompraDetalle
SET Estado = 'PENDIENTE'
WHERE FolioOC = @FolioOC;

-- Verificar
SELECT Clave, Estado, Cantidad
FROM OrdenesCompraDetalle
WHERE FolioOC = @FolioOC;
*/


-- ================================================================
-- 7. LIMPIAR ENTRADAS Y RESETEAR ESTADO (COMPLETO)
-- ================================================================
-- Use esto para hacer una limpieza completa y consistente

/*
BEGIN TRANSACTION;

    -- Eliminar todas las entradas
    DELETE FROM EntradasAlmacen;
    
    -- Marcar todas las órdenes como pendientes
    UPDATE OrdenesCompraDetalle
    SET Estado = 'PENDIENTE';
    
    -- Verificar resultados
    SELECT 'EntradasAlmacen' AS Tabla, COUNT(*) AS Total FROM EntradasAlmacen
    UNION ALL
    SELECT 'Órdenes Pendientes' AS Tabla, COUNT(*) AS Total FROM OrdenesCompraDetalle WHERE Estado = 'PENDIENTE'
    UNION ALL
    SELECT 'Órdenes Completadas' AS Tabla, COUNT(*) AS Total FROM OrdenesCompraDetalle WHERE Estado = 'COMPLETADA';

-- Si todo está bien, ejecutar:
COMMIT;

-- Si hay algún problema, ejecutar:
-- ROLLBACK;
*/


-- ================================================================
-- 8. LIMPIAR ENTRADAS DE UN INSUMO ESPECÍFICO
-- ================================================================
-- Use esto para eliminar entradas de un insumo en particular

/*
DECLARE @ClaveInsumo NVARCHAR(50) = 'EJEMPLO-001'; -- ?? CAMBIAR

DELETE FROM EntradasAlmacen
WHERE Clave = @ClaveInsumo;

-- Verificar
SELECT * FROM EntradasAlmacen WHERE Clave = @ClaveInsumo;
*/


-- ================================================================
-- 9. CONSULTAS DE DIAGNÓSTICO (NO ELIMINAN DATOS)
-- ================================================================
-- Use estas consultas para ver el estado actual antes de limpiar
-- ? ESTA SECCIÓN SÍ SE PUEDE EJECUTAR COMPLETA

-- Ver todas las entradas registradas
SELECT 
    FolioOC,
    Manzana,
    Lote,
    Clave,
    Descripcion,
    Cantidad,
    FechaEntrada,
    Usuario
FROM EntradasAlmacen
ORDER BY FechaEntrada DESC;

-- Ver resumen por orden de compra
SELECT 
    FolioOC,
    COUNT(*) AS [Total Insumos Recibidos],
    SUM(Cantidad) AS [Cantidad Total Recibida],
    MIN(FechaEntrada) AS [Primera Entrada],
    MAX(FechaEntrada) AS [Última Entrada]
FROM EntradasAlmacen
GROUP BY FolioOC
ORDER BY FolioOC;

-- Ver estado de las órdenes
SELECT 
    o.FolioOC,
    o.TipoOrden,
    o.Fecha AS [Fecha Orden],
    COUNT(d.Clave) AS [Total Insumos],
    SUM(CASE WHEN d.Estado = 'PENDIENTE' THEN 1 ELSE 0 END) AS [Pendientes],
    SUM(CASE WHEN d.Estado = 'COMPLETADA' THEN 1 ELSE 0 END) AS [Completadas]
FROM OrdenesCompra o
LEFT JOIN OrdenesCompraDetalle d ON o.FolioOC = d.FolioOC
GROUP BY o.FolioOC, o.TipoOrden, o.Fecha
ORDER BY o.Fecha DESC;

-- Ver insumos con entradas vs cantidades ordenadas
SELECT 
    d.FolioOC,
    d.Clave,
    d.Descripcion,
    d.Cantidad AS [Cantidad Ordenada],
    ISNULL(SUM(e.Cantidad), 0) AS [Cantidad Recibida],
    d.Cantidad - ISNULL(SUM(e.Cantidad), 0) AS [Cantidad Pendiente],
    d.Estado
FROM OrdenesCompraDetalle d
LEFT JOIN EntradasAlmacen e ON d.FolioOC = e.FolioOC AND d.Clave = e.Clave
GROUP BY d.FolioOC, d.Clave, d.Descripcion, d.Cantidad, d.Estado
HAVING d.Cantidad - ISNULL(SUM(e.Cantidad), 0) != 0
ORDER BY d.FolioOC, d.Clave;


-- ================================================================
-- 10. BACKUP ANTES DE LIMPIAR (RECOMENDADO)
-- ================================================================
-- Crear respaldo de las entradas antes de eliminar
-- ? ESTA SECCIÓN SÍ SE PUEDE EJECUTAR COMPLETA

/*
-- Crear tabla de respaldo
SELECT *
INTO EntradasAlmacen_Backup_20250114
FROM EntradasAlmacen;

-- Verificar que se creó el backup
SELECT COUNT(*) AS [Total en Backup] FROM EntradasAlmacen_Backup_20250114;

-- Para restaurar desde el backup (si es necesario):
-- INSERT INTO EntradasAlmacen
-- SELECT * FROM EntradasAlmacen_Backup_20250114;
*/


-- ================================================================
-- 11. LIMPIAR ÓRDENES DE COMPRA ESPECÍFICAS (COMPLETAS)
-- ================================================================
-- Use esto para eliminar completamente una orden de compra

/*
DECLARE @FolioOC NVARCHAR(50) = 'OC-MULTI-20250114-001'; -- ?? CAMBIAR

BEGIN TRANSACTION;

    -- Eliminar entradas de almacén
    DELETE FROM EntradasAlmacen WHERE FolioOC = @FolioOC;
    
    -- Eliminar relaciones con casas
    DELETE FROM OrdenesCompra_Casas WHERE FolioOC = @FolioOC;
    
    -- Eliminar detalles de la orden
    DELETE FROM OrdenesCompraDetalle WHERE FolioOC = @FolioOC;
    
    -- Eliminar la orden principal
    DELETE FROM OrdenesCompra WHERE FolioOC = @FolioOC;
    
    -- Verificar que se eliminó
    SELECT 'Orden Principal' AS Tabla, COUNT(*) AS Restantes FROM OrdenesCompra WHERE FolioOC = @FolioOC
    UNION ALL
    SELECT 'Detalles', COUNT(*) FROM OrdenesCompraDetalle WHERE FolioOC = @FolioOC
    UNION ALL
    SELECT 'Casas', COUNT(*) FROM OrdenesCompra_Casas WHERE FolioOC = @FolioOC
    UNION ALL
    SELECT 'Entradas', COUNT(*) FROM EntradasAlmacen WHERE FolioOC = @FolioOC;

-- Si todo está bien:
COMMIT;

-- Si hay problemas:
-- ROLLBACK;
*/


-- ================================================================
-- 12. LIMPIAR TODAS LAS ÓRDENES DE UN TIPO ESPECÍFICO
-- ================================================================
-- Use esto para eliminar todas las órdenes de un tipo (MULTIPLE, INDIRECTA, etc.)

/*
DECLARE @TipoOrden NVARCHAR(20) = 'MULTIPLE'; -- ?? CAMBIAR (MULTIPLE, INDIRECTA, etc.)

BEGIN TRANSACTION;

    -- Obtener folios a eliminar
    SELECT FolioOC INTO #FoliosAEliminar
    FROM OrdenesCompra
    WHERE TipoOrden = @TipoOrden;
    
    -- Eliminar entradas
    DELETE FROM EntradasAlmacen
    WHERE FolioOC IN (SELECT FolioOC FROM #FoliosAEliminar);
    
    -- Eliminar casas
    DELETE FROM OrdenesCompra_Casas
    WHERE FolioOC IN (SELECT FolioOC FROM #FoliosAEliminar);
    
    -- Eliminar detalles
    DELETE FROM OrdenesCompraDetalle
    WHERE FolioOC IN (SELECT FolioOC FROM #FoliosAEliminar);
    
    -- Eliminar órdenes principales
    DELETE FROM OrdenesCompra
    WHERE FolioOC IN (SELECT FolioOC FROM #FoliosAEliminar);
    
    -- Limpiar temporal
    DROP TABLE #FoliosAEliminar;

COMMIT; 
-- o ROLLBACK;
*/


-- ================================================================
-- ?? SCRIPT RÁPIDO: RESETEAR TODO EL SISTEMA
-- ================================================================
-- Este script limpia TODAS las entradas y resetea TODAS las órdenes
-- ?? ÚSALO SOLO SI QUIERES EMPEZAR DE CERO

/*
BEGIN TRANSACTION;

    -- 1. Eliminar todas las entradas de almacén
    DELETE FROM EntradasAlmacen;
    PRINT '? Entradas de almacén eliminadas';
    
    -- 2. Resetear todas las órdenes a PENDIENTE
    UPDATE OrdenesCompraDetalle SET Estado = 'PENDIENTE';
    PRINT '? Órdenes reseteadas a PENDIENTE';
    
    -- 3. Mostrar resumen
    SELECT 
        'EntradasAlmacen' AS Tabla, 
        COUNT(*) AS Total 
    FROM EntradasAlmacen
    UNION ALL
    SELECT 
        'Órdenes PENDIENTE', 
        COUNT(*) 
    FROM OrdenesCompraDetalle 
    WHERE Estado = 'PENDIENTE'
    UNION ALL
    SELECT 
        'Órdenes COMPLETADA', 
        COUNT(*) 
    FROM OrdenesCompraDetalle 
    WHERE Estado = 'COMPLETADA';
    
    PRINT '? Sistema reseteado exitosamente';

-- Revisar los resultados arriba y luego ejecutar UNA de estas líneas:
COMMIT;    -- Para confirmar los cambios
-- ROLLBACK;  -- Para cancelar los cambios
*/


-- ================================================================
-- NOTAS IMPORTANTES:
-- ================================================================
-- 1. Cada sección está comentada con /* */ para evitar conflictos
-- 2. Descomenta SOLO la sección que necesites ejecutar
-- 3. Las secciones #9 (Diagnóstico) SÍ se pueden ejecutar completas
-- 4. SIEMPRE haz backup (#10) antes de eliminar datos
-- 5. Usa BEGIN TRANSACTION / COMMIT / ROLLBACK para seguridad
-- 6. Verifica los resultados antes de hacer COMMIT
-- ================================================================

-- ================================================================
-- EJEMPLO DE USO:
-- ================================================================
-- Para resetear una orden específica:
--   1. Ejecuta la sección #9 para ver las órdenes
--   2. Descomenta la sección #6, cambia el folio
--   3. Ejecuta la sección #6
--   4. Verifica con la sección #9
-- ================================================================
