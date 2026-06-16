-- =============================================
-- Script para verificar y corregir datos de PresupuestoObra
-- ACTUALIZADO: Considera prototipos CALANDRA y TUNERA
-- Fecha: 2025-01-11
-- =============================================

-- 1. Verificar que exista la tabla PresupuestoObra
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PresupuestoObra')
BEGIN
    PRINT '? ERROR: La tabla PresupuestoObra no existe'
    PRINT 'Ejecuta primero el script de creación de la tabla'
    RETURN
END
ELSE
BEGIN
    PRINT '? La tabla PresupuestoObra existe'
END

-- 2. Verificar que tenga datos
DECLARE @TotalRegistros INT
SELECT @TotalRegistros = COUNT(*) FROM PresupuestoObra
PRINT ''
PRINT '?? Total de registros en PresupuestoObra: ' + CAST(@TotalRegistros AS VARCHAR(10))

IF @TotalRegistros = 0
BEGIN
    PRINT '?? ADVERTENCIA: La tabla PresupuestoObra está vacía'
    PRINT 'Necesitas importar los datos desde tu Excel'
    RETURN
END

-- 3. Verificar prototipos disponibles
PRINT ''
PRINT '?? Prototipos disponibles en PresupuestoObra:'
SELECT 
    Prototipo, 
    COUNT(*) AS TotalPartidas,
    SUM(CostoTotal) AS PresupuestoTotal
FROM PresupuestoObra
GROUP BY Prototipo
ORDER BY Prototipo

-- 4. Verificar categorías por prototipo
PRINT ''
PRINT '?? Resumen por Categoría y Prototipo:'
SELECT 
    Prototipo,
    Categoria, 
    COUNT(*) AS NumPartidas,
    SUM(CostoTotal) AS CostoCategoria
FROM PresupuestoObra
GROUP BY Prototipo, Categoria
ORDER BY Prototipo, Categoria

-- 5. Verificar que las casas tengan prototipos válidos
PRINT ''
PRINT '?? Verificando casas en InventarioCasas:'
SELECT 
    ic.Manzana,
    ic.Lote,
    ic.Prototipo AS PrototipoCasa,
    CASE 
        WHEN ic.Prototipo IS NULL THEN '? Prototipo NULL'
        WHEN ic.Prototipo = '' THEN '? Prototipo vacío'
        WHEN NOT EXISTS (SELECT 1 FROM PresupuestoObra po WHERE po.Prototipo = ic.Prototipo) 
        THEN '? Prototipo NO EXISTE en presupuesto'
        ELSE '? OK'
    END AS Estado,
    (SELECT SUM(CostoTotal) 
     FROM PresupuestoObra 
     WHERE Prototipo = ic.Prototipo) AS PresupuestoTotal
FROM InventarioCasas ic
ORDER BY ic.Manzana, ic.Lote

-- 6. Comparar presupuestos entre CALANDRA y TUNERA
PRINT ''
PRINT '?? Comparación de presupuestos por categoría:'
SELECT 
    COALESCE(c.Categoria, t.Categoria) AS Categoria,
    ISNULL(c.CostoCALANDRA, 0) AS CALANDRA,
    ISNULL(t.CostoTUNERA, 0) AS TUNERA,
    ISNULL(t.CostoTUNERA, 0) - ISNULL(c.CostoCALANDRA, 0) AS Diferencia,
    CASE 
        WHEN ISNULL(c.CostoCALANDRA, 0) = 0 THEN 0
        ELSE ((ISNULL(t.CostoTUNERA, 0) - ISNULL(c.CostoCALANDRA, 0)) / c.CostoCALANDRA * 100)
    END AS PorcentajeDiferencia
FROM 
    (SELECT Categoria, SUM(CostoTotal) AS CostoCALANDRA 
     FROM PresupuestoObra 
     WHERE Prototipo = 'CALANDRA' 
     GROUP BY Categoria) c
FULL OUTER JOIN
    (SELECT Categoria, SUM(CostoTotal) AS CostoTUNERA 
     FROM PresupuestoObra 
     WHERE Prototipo = 'TUNERA' 
     GROUP BY Categoria) t
ON c.Categoria = t.Categoria
ORDER BY Categoria

-- 7. Verificar valores NULL
PRINT ''
PRINT '?? Verificando valores NULL:'
SELECT 
    'Registros con Categoria NULL' AS Problema,
    COUNT(*) AS Cantidad
FROM PresupuestoObra
WHERE Categoria IS NULL
UNION ALL
SELECT 
    'Registros con CostoTotal NULL',
    COUNT(*)
FROM PresupuestoObra
WHERE CostoTotal IS NULL
UNION ALL
SELECT 
    'Registros con Prototipo NULL',
    COUNT(*)
FROM PresupuestoObra
WHERE Prototipo IS NULL

-- 8. Ejemplo de datos
PRINT ''
PRINT '?? Ejemplo de datos (primeros 10 de cada prototipo):'

-- Primero CALANDRA
SELECT TOP 10
    'CALANDRA' AS Tipo,
    Categoria,
    Descripcion,
    Cantidad,
    PrecioUnitario,
    CostoTotal
FROM PresupuestoObra
WHERE Prototipo = 'CALANDRA'
ORDER BY Categoria, Id

-- Luego TUNERA
SELECT TOP 10
    'TUNERA' AS Tipo,
    Categoria,
    Descripcion,
    Cantidad,
    PrecioUnitario,
    CostoTotal
FROM PresupuestoObra
WHERE Prototipo = 'TUNERA'
ORDER BY Categoria, Id

PRINT ''
PRINT '? Verificación completada'
PRINT '==========================================='
PRINT 'SIGUIENTE PASO:'
PRINT '1. Verifica que cada casa tenga asignado CALANDRA o TUNERA'
PRINT '2. Si hay casas con prototipo NULL o incorrecto, ejecuta SQL_CorregirPrototipos.sql'
PRINT '3. Los prototipos válidos son: CALANDRA y TUNERA (exactamente así, respetando mayúsculas)'
