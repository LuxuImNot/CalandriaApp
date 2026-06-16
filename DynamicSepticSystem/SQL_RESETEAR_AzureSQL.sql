-- ================================================================
-- RESETEAR SISTEMA DE ÓRDENES DE COMPRA
-- Compatible con Azure SQL Database (sin USE)
-- ================================================================
-- IMPORTANTE: Conecta directamente a tu base de datos antes de ejecutar
-- ================================================================

-- ================================================================
-- ?? DIAGNÓSTICO PREVIO - Ver estado actual
-- ================================================================
PRINT '=== ESTADO ACTUAL DEL SISTEMA ===';
PRINT 'Base de datos: ' + DB_NAME();
PRINT '';

-- Verificar que las tablas existen
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntradasAlmacen')
BEGIN
    PRINT '? ERROR: La tabla EntradasAlmacen NO EXISTE en esta base de datos';
    PRINT 'Verifica que estás conectado a la base de datos correcta';
    PRINT 'Base de datos actual: ' + DB_NAME();
    RAISERROR('Tabla EntradasAlmacen no encontrada. Conecta a la base de datos correcta.', 16, 1);
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompraDetalle')
BEGIN
    PRINT '? ERROR: La tabla OrdenesCompraDetalle NO EXISTE en esta base de datos';
    PRINT 'Base de datos actual: ' + DB_NAME();
    RAISERROR('Tabla OrdenesCompraDetalle no encontrada. Conecta a la base de datos correcta.', 16, 1);
    RETURN;
END

PRINT '? Tablas verificadas - Base de datos correcta';
PRINT '';

-- Mostrar conteo actual
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
PRINT '=== ÚLTIMAS ENTRADAS DE ALMACÉN ===';
SELECT TOP 10 
    FolioOC,
    Manzana,
    Lote,
    Clave,
    Cantidad,
    FechaEntrada,
    Usuario
FROM EntradasAlmacen
ORDER BY FechaEntrada DESC;

PRINT '';
PRINT '=== ÓRDENES CON PROGRESO ===';
SELECT 
    o.FolioOC,
    o.TipoOrden,
    o.Fecha,
    COUNT(d.Clave) AS [Total Insumos],
    SUM(CASE WHEN d.Estado = 'PENDIENTE' THEN 1 ELSE 0 END) AS Pendientes,
    SUM(CASE WHEN d.Estado = 'COMPLETADA' THEN 1 ELSE 0 END) AS Completadas
FROM OrdenesCompra o
LEFT JOIN OrdenesCompraDetalle d ON o.FolioOC = d.FolioOC
GROUP BY o.FolioOC, o.TipoOrden, o.Fecha
HAVING SUM(CASE WHEN d.Estado = 'COMPLETADA' THEN 1 ELSE 0 END) > 0
ORDER BY o.Fecha DESC;

PRINT '';
PRINT '=== FIN DEL DIAGNÓSTICO ===';
PRINT 'Si quieres resetear el sistema, ejecuta el siguiente bloque:';
PRINT '';

-- ================================================================
-- ?? RESETEO COMPLETO DEL SISTEMA
-- ================================================================
-- ?? DESCOMENTA LA SIGUIENTE SECCIÓN SOLO SI QUIERES RESETEAR TODO

/*
PRINT '';
PRINT '=== INICIANDO RESETEO DEL SISTEMA ===';
PRINT 'Base de datos: ' + DB_NAME();
PRINT '';

BEGIN TRANSACTION ReseteoOrdenesCompra;

BEGIN TRY
    DECLARE @CountEntradas INT;
    DECLARE @CountPendientes INT;
    DECLARE @CountCompletadas INT;
    
    -- Contar antes del reseteo
    SELECT @CountEntradas = COUNT(*) FROM EntradasAlmacen;
    SELECT @CountCompletadas = COUNT(*) FROM OrdenesCompraDetalle WHERE Estado = 'COMPLETADA';
    
    PRINT 'Registros a eliminar:';
    PRINT '  - Entradas de almacén: ' + CAST(@CountEntradas AS NVARCHAR(10));
    PRINT '  - Órdenes completadas a resetear: ' + CAST(@CountCompletadas AS NVARCHAR(10));
    PRINT '';
    
    -- Eliminar todas las entradas de almacén
    DELETE FROM EntradasAlmacen;
    PRINT '? Entradas de almacén eliminadas: ' + CAST(@CountEntradas AS NVARCHAR(10));
    
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
    
    PRINT '';
    PRINT '?? IMPORTANTE: Revisa los resultados arriba';
    PRINT 'Si todo está correcto, el COMMIT se ejecutará automáticamente';
    PRINT 'Si algo salió mal, puedes hacer ROLLBACK manualmente';
    PRINT '';
    
    -- Si llegamos aquí, todo fue exitoso
    COMMIT TRANSACTION ReseteoOrdenesCompra;
    PRINT '';
    PRINT '??? RESETEO COMPLETADO EXITOSAMENTE ???';
    PRINT 'Transacción confirmada (COMMIT)';
    
END TRY
BEGIN CATCH
    -- Si hay error, revertir cambios
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION ReseteoOrdenesCompra;
        PRINT '';
        PRINT '??? ERROR DURANTE EL RESETEO ???';
        PRINT 'Se hizo ROLLBACK - No se aplicaron cambios';
    END
    
    PRINT '';
    PRINT 'Detalles del error:';
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS NVARCHAR(10));
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10));
    
    -- Re-lanzar el error
    THROW;
END CATCH
*/

-- ================================================================
-- ?? INSTRUCCIONES DE USO:
-- ================================================================
-- 1. Conéctate a tu base de datos en SQL Server Management Studio:
--    - Haz clic derecho en tu base de datos
--    - Selecciona "New Query"
--    - La ventana de query ya estará conectada a esa BD
--
-- 2. Ejecuta el script COMPLETO para ver el diagnóstico
--    (Presiona F5 o clic en "Execute")
--
-- 3. Revisa los resultados y confirma que es la base de datos correcta
--
-- 4. Si quieres resetear, descomenta la sección "RESETEO COMPLETO"
--    (elimina /* al inicio y */ al final)
--
-- 5. Ejecuta nuevamente
--
-- 6. El script hace COMMIT automáticamente si todo sale bien
--    o ROLLBACK si hay algún error
-- ================================================================

PRINT '';
PRINT '=== SCRIPT DE DIAGNÓSTICO COMPLETADO ===';
PRINT 'Base de datos: ' + DB_NAME();
PRINT '';
PRINT 'Para resetear el sistema:';
PRINT '1. Descomenta la sección RESETEO COMPLETO';
PRINT '2. Ejecuta nuevamente este script';
