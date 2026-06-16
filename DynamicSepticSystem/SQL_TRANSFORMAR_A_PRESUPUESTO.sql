-- =============================================
-- Script para TRANSFORMAR datos de Excel a PresupuestoObra
-- Agrupa por Padre (Categoría) y suma los costos
-- =============================================

PRINT '?? Transformando datos de TempExcel a PresupuestoObra...'
PRINT ''

-- 1. Verificar que TempExcel tenga datos
DECLARE @CountTemp INT
SELECT @CountTemp = COUNT(*) FROM TempExcel

IF @CountTemp = 0
BEGIN
    PRINT '? ERROR: La tabla TempExcel está vacía'
    PRINT 'Asegúrate de haber importado el Excel correctamente'
    RETURN
END

PRINT '? TempExcel contiene ' + CAST(@CountTemp AS VARCHAR(10)) + ' registros'
PRINT ''

-- 2. Insertar datos de CALANDRA (agrupados por Padre)
INSERT INTO PresupuestoObra (Categoria, CostoTotal, Prototipo)
SELECT 
    Padre AS Categoria,
    SUM(CostoCalandra) AS CostoTotal,
    'CALANDRA' AS Prototipo
FROM TempExcel
WHERE CostoCalandra IS NOT NULL AND CostoCalandra > 0
GROUP BY Padre

DECLARE @TotalCalandra INT = @@ROWCOUNT
PRINT '? CALANDRA: ' + CAST(@TotalCalandra AS VARCHAR(10)) + ' categorías insertadas'

-- 3. Insertar datos de TUNERA (agrupados por Padre)
INSERT INTO PresupuestoObra (Categoria, CostoTotal, Prototipo)
SELECT 
    Padre AS Categoria,
    SUM(CostoTunera) AS CostoTotal,
    'TUNERA' AS Prototipo
FROM TempExcel
WHERE CostoTunera IS NOT NULL AND CostoTunera > 0
GROUP BY Padre

DECLARE @TotalTunera INT = @@ROWCOUNT
PRINT '? TUNERA: ' + CAST(@TotalTunera AS VARCHAR(10)) + ' categorías insertadas'

-- 4. Verificar totales
PRINT ''
PRINT '?? Verificación de datos:'
SELECT 
    Prototipo,
    COUNT(*) AS TotalCategorias,
    SUM(CostoTotal) AS PresupuestoTotal
FROM PresupuestoObra
GROUP BY Prototipo
ORDER BY Prototipo

-- 5. Mostrar detalle por categoría
PRINT ''
PRINT '?? Detalle por categoría:'
SELECT 
    Categoria,
    MAX(CASE WHEN Prototipo = 'CALANDRA' THEN CostoTotal ELSE 0 END) AS CALANDRA,
    MAX(CASE WHEN Prototipo = 'TUNERA' THEN CostoTotal ELSE 0 END) AS TUNERA,
    MAX(CASE WHEN Prototipo = 'TUNERA' THEN CostoTotal ELSE 0 END) - 
    MAX(CASE WHEN Prototipo = 'CALANDRA' THEN CostoTotal ELSE 0 END) AS Diferencia
FROM PresupuestoObra
GROUP BY Categoria
ORDER BY Categoria

-- 6. Limpiar tabla temporal
DROP TABLE TempExcel
PRINT ''
PRINT '? Tabla temporal TempExcel eliminada'

-- 7. Resultado final
PRINT ''
PRINT '==========================================='
PRINT '? IMPORTACIÓN COMPLETADA'
PRINT '==========================================='
PRINT ''
PRINT 'Resultado esperado:'
PRINT '- CALANDRA: 17 categorías (~$2,662,190)'
PRINT '- TUNERA: 17 categorías (~$2,723,465)'
PRINT ''
PRINT 'Ahora puedes usar la aplicación FormAvanceObra'
PRINT '==========================================='
