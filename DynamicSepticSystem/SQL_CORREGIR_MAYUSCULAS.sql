-- =============================================
-- Script para CORREGIR todos los prototipos a MAYÚSCULAS
-- EJECUTA ESTO AHORA para solucionar el problema
-- =============================================

PRINT '?? Corrigiendo prototipos en InventarioCasas...'
PRINT ''

-- Antes de corregir, mostrar el estado actual
PRINT '?? Estado ANTES de la corrección:'
SELECT 
    Prototipo,
    COUNT(*) AS TotalCasas
FROM InventarioCasas
GROUP BY Prototipo
ORDER BY Prototipo

PRINT ''
PRINT '?? Aplicando correcciones...'

-- Corregir todos los prototipos a MAYÚSCULAS
UPDATE InventarioCasas
SET Prototipo = UPPER(Prototipo)
WHERE Prototipo IS NOT NULL

-- Verificar cuántos registros se actualizaron
DECLARE @TotalActualizados INT
SELECT @TotalActualizados = @@ROWCOUNT
PRINT '? Total de registros actualizados: ' + CAST(@TotalActualizados AS VARCHAR(10))

PRINT ''
PRINT '?? Estado DESPUÉS de la corrección:'
SELECT 
    Prototipo,
    COUNT(*) AS TotalCasas
FROM InventarioCasas
GROUP BY Prototipo
ORDER BY Prototipo

PRINT ''
PRINT '?? Verificando coincidencias con PresupuestoObra:'
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

PRINT ''
PRINT '? Corrección completada'
PRINT ''
PRINT '?? Resumen final:'
SELECT 
    'Total casas con CALANDRA' AS Detalle,
    COUNT(*) AS Cantidad
FROM InventarioCasas
WHERE Prototipo = 'CALANDRA'
UNION ALL
SELECT 
    'Total casas con TUNERA',
    COUNT(*)
FROM InventarioCasas
WHERE Prototipo = 'TUNERA'
UNION ALL
SELECT 
    'Total casas con otros valores',
    COUNT(*)
FROM InventarioCasas
WHERE Prototipo NOT IN ('CALANDRA', 'TUNERA')

PRINT ''
PRINT '==========================================='
PRINT '? TODOS LOS PROTOTIPOS AHORA ESTÁN EN MAYÚSCULAS'
PRINT 'Ahora puedes usar la aplicación sin errores'
PRINT '==========================================='
