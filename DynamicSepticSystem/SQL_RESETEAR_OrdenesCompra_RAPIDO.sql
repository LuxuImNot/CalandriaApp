-- ================================================================
-- SCRIPT RÁPIDO: RESETEAR SISTEMA DE ÓRDENES DE COMPRA
-- Sistema: Calandria Residencial
-- ================================================================
-- Este script limpia todas las entradas y resetea las órdenes a PENDIENTE
-- ================================================================

-- ?? PASO 1: Seleccionar la base de datos correcta
-- IMPORTANTE: Verifica que este sea el nombre correcto de tu base de datos
-- Mira en tu connection string de App.config para confirmar el nombre

USE [CalandriaDB];  -- ?? CAMBIAR si tu base de datos tiene otro nombre
GO

-- ================================================================
-- ?? DIAGNÓSTICO PREVIO - Ver estado actual
-- ================================================================
PRINT '=== ESTADO ACTUAL DEL SISTEMA ===';

SELECT 'EntradasAlmacen' AS Tabla, COUNT(*) AS Total 
FROM EntradasAlmacen
UNION ALL
SELECT 'Órdenes PENDIENTE', COUNT(*) 
FROM OrdenesCompraDetalle 
WHERE Estado = 'PENDIENTE'
UNION ALL
SELECT 'Órdenes COMPLETADA', COUNT(*) 
FROM OrdenesCompraDetalle 
WHERE Estado = 'COMPLETADA';

PRINT '';
PRINT '=== LISTA DE ENTRADAS DE ALMACÉN ===';
SELECT TOP 10 
    FolioOC,
    Manzana,
    Lote,
    Clave,
    Cantidad,
    FechaEntrada
FROM EntradasAlmacen
ORDER BY FechaEntrada DESC;

PRINT '';
PRINT '=== ÓRDENES CON PROGRESO ===';
SELECT 
    o.FolioOC,
    o.TipoOrden,
    o.Fecha,
    SUM(CASE WHEN d.Estado = 'PENDIENTE' THEN 1 ELSE 0 END) AS Pendientes,
    SUM(CASE WHEN d.Estado = 'COMPLETADA' THEN 1 ELSE 0 END) AS Completadas
FROM OrdenesCompra o
LEFT JOIN OrdenesCompraDetalle d ON o.FolioOC = d.FolioOC
GROUP BY o.FolioOC, o.TipoOrden, o.Fecha
ORDER BY o.Fecha DESC;

-- ================================================================
-- ?? RESETEO COMPLETO DEL SISTEMA
-- ================================================================
-- ?? DESCOMENTA LA SIGUIENTE SECCIÓN SOLO SI QUIERES RESETEAR TODO

/*
PRINT '';
PRINT '=== INICIANDO RESETEO DEL SISTEMA ===';

BEGIN TRANSACTION;

    -- Eliminar todas las entradas de almacén
    DELETE FROM EntradasAlmacen;
    PRINT '? Entradas de almacén eliminadas';
    
    -- Resetear todas las órdenes a PENDIENTE
    UPDATE OrdenesCompraDetalle SET Estado = 'PENDIENTE';
    PRINT '? Órdenes reseteadas a PENDIENTE';
    
    -- Mostrar resumen final
    PRINT '';
    PRINT '=== ESTADO DESPUÉS DEL RESETEO ===';
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

-- ?? IMPORTANTE: Revisa los resultados arriba
-- Si todo está correcto, ejecuta: COMMIT;
-- Si algo salió mal, ejecuta: ROLLBACK;

COMMIT;
PRINT '';
PRINT '??? RESETEO COMPLETADO EXITOSAMENTE ???';

*/

-- ================================================================
-- ?? INSTRUCCIONES DE USO:
-- ================================================================
-- 1. Ejecuta el script COMPLETO para ver el diagnóstico
-- 2. Revisa los resultados y confirma que es la base de datos correcta
-- 3. Si quieres resetear, descomenta la sección "RESETEO COMPLETO"
--    (elimina /* al inicio y */ al final)
-- 4. Ejecuta nuevamente
-- 5. Revisa los resultados antes de hacer COMMIT
-- ================================================================

PRINT '';
PRINT '=== SCRIPT COMPLETADO ===';
PRINT 'Si necesitas resetear el sistema, descomenta la sección RESETEO COMPLETO';
