-- =====================================================
-- SCRIPT: ELIMINAR ÓRDENES DE COMPRA
-- Descripción: Elimina todas las órdenes de compra y sus datos relacionados
-- Versión: 1.0
-- Fecha: 2024
-- =====================================================

USE CalandriaDB;
GO

-- Verificar que estamos en la base de datos correcta
PRINT '========================================';
PRINT 'INICIANDO ELIMINACIÓN DE ÓRDENES DE COMPRA';
PRINT 'Base de datos: ' + DB_NAME();
PRINT 'Fecha: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- =====================================================
-- OPCIÓN 1: ELIMINAR TODAS LAS ÓRDENES DE COMPRA
-- =====================================================
-- Descomenta esta sección para eliminar TODAS las órdenes

/*
BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'Paso 1: Eliminando entradas de almacén...';
    DELETE FROM EntradasAlmacen;
    PRINT '  ? Entradas de almacén eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 2: Eliminando detalles de órdenes de compra...';
    DELETE FROM OrdenesCompraDetalle;
    PRINT '  ? Detalles eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 3: Eliminando relación órdenes-casas...';
    DELETE FROM OrdenesCompra_Casas;
    PRINT '  ? Relaciones órdenes-casas eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 4: Eliminando PDFs del repositorio...';
    DELETE FROM RepositorioPDFsOrdenesCompra;
    PRINT '  ? PDFs del repositorio eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 5: Eliminando órdenes de compra principales...';
    DELETE FROM OrdenesCompra;
    PRINT '  ? Órdenes de compra eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT '';
    PRINT '========================================';
    PRINT '? TODAS LAS ÓRDENES ELIMINADAS CORRECTAMENTE';
    PRINT '========================================';
    
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT '';
    PRINT '========================================';
    PRINT '? ERROR AL ELIMINAR ÓRDENES DE COMPRA';
    PRINT '========================================';
    PRINT 'Error: ' + ERROR_MESSAGE();
    PRINT 'Línea: ' + CAST(ERROR_LINE() AS VARCHAR);
    PRINT 'Procedimiento: ' + ISNULL(ERROR_PROCEDURE(), 'N/A');
END CATCH;
*/

-- =====================================================
-- OPCIÓN 2: ELIMINAR ÓRDENES POR TIPO
-- =====================================================
-- Descomenta UNA de estas secciones según el tipo que quieras eliminar

-- ELIMINAR SOLO ÓRDENES INDIRECTAS
/*
DECLARE @TipoOrden VARCHAR(50) = 'INDIRECTA';

BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'Eliminando órdenes de tipo: ' + @TipoOrden;
    PRINT '';

    -- Obtener folios a eliminar
    DECLARE @Folios TABLE (FolioOC VARCHAR(50));
    INSERT INTO @Folios (FolioOC)
    SELECT FolioOC FROM OrdenesCompra WHERE TipoOrden = @TipoOrden;

    DECLARE @Count INT = (SELECT COUNT(*) FROM @Folios);
    PRINT 'Órdenes a eliminar: ' + CAST(@Count AS VARCHAR);
    PRINT '';

    PRINT 'Paso 1: Eliminando entradas de almacén...';
    DELETE e FROM EntradasAlmacen e
    INNER JOIN @Folios f ON e.FolioOC = f.FolioOC;
    PRINT '  ? Entradas eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 2: Eliminando detalles...';
    DELETE d FROM OrdenesCompraDetalle d
    INNER JOIN @Folios f ON d.FolioOC = f.FolioOC;
    PRINT '  ? Detalles eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 3: Eliminando relaciones órdenes-casas...';
    DELETE c FROM OrdenesCompra_Casas c
    INNER JOIN @Folios f ON c.FolioOC = f.FolioOC;
    PRINT '  ? Relaciones eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 4: Eliminando PDFs del repositorio...';
    DELETE r FROM RepositorioPDFsOrdenesCompra r
    INNER JOIN @Folios f ON r.FolioOC = f.FolioOC;
    PRINT '  ? PDFs eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 5: Eliminando órdenes principales...';
    DELETE o FROM OrdenesCompra o
    INNER JOIN @Folios f ON o.FolioOC = f.FolioOC;
    PRINT '  ? Órdenes eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT '';
    PRINT '========================================';
    PRINT '? ÓRDENES ' + @TipoOrden + ' ELIMINADAS CORRECTAMENTE';
    PRINT '========================================';
    
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '? ERROR: ' + ERROR_MESSAGE();
END CATCH;
*/

-- ELIMINAR SOLO ÓRDENES MÚLTIPLES
/*
DECLARE @TipoOrden VARCHAR(50) = 'MULTIPLE';

BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'Eliminando órdenes de tipo: ' + @TipoOrden;
    PRINT '';

    DECLARE @Folios TABLE (FolioOC VARCHAR(50));
    INSERT INTO @Folios (FolioOC)
    SELECT FolioOC FROM OrdenesCompra WHERE TipoOrden = @TipoOrden;

    DECLARE @Count INT = (SELECT COUNT(*) FROM @Folios);
    PRINT 'Órdenes a eliminar: ' + CAST(@Count AS VARCHAR);
    PRINT '';

    PRINT 'Paso 1: Eliminando entradas de almacén...';
    DELETE e FROM EntradasAlmacen e
    INNER JOIN @Folios f ON e.FolioOC = f.FolioOC;
    PRINT '  ? Entradas eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 2: Eliminando detalles...';
    DELETE d FROM OrdenesCompraDetalle d
    INNER JOIN @Folios f ON d.FolioOC = f.FolioOC;
    PRINT '  ? Detalles eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 3: Eliminando relaciones órdenes-casas...';
    DELETE c FROM OrdenesCompra_Casas c
    INNER JOIN @Folios f ON c.FolioOC = f.FolioOC;
    PRINT '  ? Relaciones eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 4: Eliminando PDFs del repositorio...';
    DELETE r FROM RepositorioPDFsOrdenesCompra r
    INNER JOIN @Folios f ON r.FolioOC = f.FolioOC;
    PRINT '  ? PDFs eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 5: Eliminando órdenes principales...';
    DELETE o FROM OrdenesCompra o
    INNER JOIN @Folios f ON o.FolioOC = f.FolioOC;
    PRINT '  ? Órdenes eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT '';
    PRINT '========================================';
    PRINT '? ÓRDENES ' + @TipoOrden + ' ELIMINADAS CORRECTAMENTE';
    PRINT '========================================';
    
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '? ERROR: ' + ERROR_MESSAGE();
END CATCH;
*/

-- =====================================================
-- OPCIÓN 3: ELIMINAR ÓRDENES POR RANGO DE FECHAS
-- =====================================================
/*
DECLARE @FechaInicio DATE = '2024-01-01';
DECLARE @FechaFin DATE = '2024-12-31';

BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'Eliminando órdenes entre ' + CONVERT(VARCHAR, @FechaInicio, 103) + ' y ' + CONVERT(VARCHAR, @FechaFin, 103);
    PRINT '';

    DECLARE @Folios TABLE (FolioOC VARCHAR(50));
    INSERT INTO @Folios (FolioOC)
    SELECT FolioOC FROM OrdenesCompra 
    WHERE CAST(Fecha AS DATE) BETWEEN @FechaInicio AND @FechaFin;

    DECLARE @Count INT = (SELECT COUNT(*) FROM @Folios);
    PRINT 'Órdenes a eliminar: ' + CAST(@Count AS VARCHAR);
    PRINT '';

    PRINT 'Paso 1: Eliminando entradas de almacén...';
    DELETE e FROM EntradasAlmacen e
    INNER JOIN @Folios f ON e.FolioOC = f.FolioOC;
    PRINT '  ? Entradas eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 2: Eliminando detalles...';
    DELETE d FROM OrdenesCompraDetalle d
    INNER JOIN @Folios f ON d.FolioOC = f.FolioOC;
    PRINT '  ? Detalles eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 3: Eliminando relaciones órdenes-casas...';
    DELETE c FROM OrdenesCompra_Casas c
    INNER JOIN @Folios f ON c.FolioOC = f.FolioOC;
    PRINT '  ? Relaciones eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 4: Eliminando PDFs del repositorio...';
    DELETE r FROM RepositorioPDFsOrdenesCompra r
    INNER JOIN @Folios f ON r.FolioOC = f.FolioOC;
    PRINT '  ? PDFs eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 5: Eliminando órdenes principales...';
    DELETE o FROM OrdenesCompra o
    INNER JOIN @Folios f ON o.FolioOC = f.FolioOC;
    PRINT '  ? Órdenes eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT '';
    PRINT '========================================';
    PRINT '? ÓRDENES ELIMINADAS CORRECTAMENTE';
    PRINT '========================================';
    
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '? ERROR: ' + ERROR_MESSAGE();
END CATCH;
*/

-- =====================================================
-- OPCIÓN 4: ELIMINAR UNA ORDEN ESPECÍFICA POR FOLIO
-- =====================================================
/*
DECLARE @FolioOC VARCHAR(50) = 'OC-MULTI-20241201-001'; -- CAMBIA ESTE FOLIO

BEGIN TRANSACTION;

BEGIN TRY
    -- Verificar que existe
    IF NOT EXISTS (SELECT 1 FROM OrdenesCompra WHERE FolioOC = @FolioOC)
    BEGIN
        PRINT '? ERROR: La orden ' + @FolioOC + ' no existe';
        ROLLBACK TRANSACTION;
        RETURN;
    END

    PRINT 'Eliminando orden: ' + @FolioOC;
    PRINT '';

    PRINT 'Paso 1: Eliminando entradas de almacén...';
    DELETE FROM EntradasAlmacen WHERE FolioOC = @FolioOC;
    PRINT '  ? Entradas eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 2: Eliminando detalles...';
    DELETE FROM OrdenesCompraDetalle WHERE FolioOC = @FolioOC;
    PRINT '  ? Detalles eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 3: Eliminando relaciones órdenes-casas...';
    DELETE FROM OrdenesCompra_Casas WHERE FolioOC = @FolioOC;
    PRINT '  ? Relaciones eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 4: Eliminando PDF del repositorio...';
    DELETE FROM RepositorioPDFsOrdenesCompra WHERE FolioOC = @FolioOC;
    PRINT '  ? PDF eliminado: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT 'Paso 5: Eliminando orden principal...';
    DELETE FROM OrdenesCompra WHERE FolioOC = @FolioOC;
    PRINT '  ? Orden eliminada: ' + CAST(@@ROWCOUNT AS VARCHAR);

    PRINT '';
    PRINT '========================================';
    PRINT '? ORDEN ' + @FolioOC + ' ELIMINADA CORRECTAMENTE';
    PRINT '========================================';
    
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '? ERROR: ' + ERROR_MESSAGE();
END CATCH;
*/

-- =====================================================
-- VERIFICACIÓN FINAL
-- =====================================================
PRINT '';
PRINT '========================================';
PRINT 'CONTEO ACTUAL DE REGISTROS';
PRINT '========================================';
PRINT 'Órdenes de Compra: ' + CAST((SELECT COUNT(*) FROM OrdenesCompra) AS VARCHAR);
PRINT 'Detalles de Órdenes: ' + CAST((SELECT COUNT(*) FROM OrdenesCompraDetalle) AS VARCHAR);
PRINT 'Relaciones Órdenes-Casas: ' + CAST((SELECT COUNT(*) FROM OrdenesCompra_Casas) AS VARCHAR);
PRINT 'Entradas de Almacén: ' + CAST((SELECT COUNT(*) FROM EntradasAlmacen) AS VARCHAR);
PRINT 'PDFs en Repositorio: ' + CAST((SELECT COUNT(*) FROM RepositorioPDFsOrdenesCompra) AS VARCHAR);
PRINT '========================================';

-- =====================================================
-- RESUMEN POR TIPO DE ORDEN
-- =====================================================
PRINT '';
PRINT 'RESUMEN POR TIPO DE ORDEN:';
PRINT '========================================';

SELECT 
    TipoOrden,
    COUNT(*) AS TotalOrdenes,
    MIN(Fecha) AS PrimeraOrden,
    MAX(Fecha) AS UltimaOrden
FROM OrdenesCompra
GROUP BY TipoOrden
ORDER BY TipoOrden;

PRINT '';
PRINT '========================================';
PRINT 'SCRIPT COMPLETADO';
PRINT '========================================';

GO

/*
============================================================
INSTRUCCIONES DE USO:
============================================================

Este script proporciona 4 opciones para eliminar órdenes de compra:

1. ELIMINAR TODAS LAS ÓRDENES
   - Descomenta la sección "OPCIÓN 1"
   - Elimina TODAS las órdenes del sistema
   - ?? USAR CON PRECAUCIÓN

2. ELIMINAR POR TIPO
   - Descomenta "ELIMINAR SOLO ÓRDENES INDIRECTAS" o "ELIMINAR SOLO ÓRDENES MÚLTIPLES"
   - Elimina solo las órdenes del tipo especificado

3. ELIMINAR POR RANGO DE FECHAS
   - Descomenta la sección "OPCIÓN 3"
   - Configura @FechaInicio y @FechaFin
   - Elimina órdenes dentro del rango de fechas

4. ELIMINAR ORDEN ESPECÍFICA
   - Descomenta la sección "OPCIÓN 4"
   - Configura @FolioOC con el folio exacto
   - Elimina una sola orden

IMPORTANTE:
- Cada opción usa TRANSACCIONES para garantizar consistencia
- Si hay un error, se hace ROLLBACK automático
- Las eliminaciones respetan el orden de dependencias
- Se eliminan en este orden:
  1. EntradasAlmacen
  2. OrdenesCompraDetalle
  3. OrdenesCompra_Casas
  4. RepositorioPDFsOrdenesCompra
  5. OrdenesCompra

RECOMENDACIONES:
- Hacer un BACKUP antes de ejecutar
- Verificar el conteo de registros después
- Revisar que los archivos PDF físicos también se eliminen si es necesario

============================================================
*/
