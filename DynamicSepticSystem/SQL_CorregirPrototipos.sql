-- =============================================
-- Script para corregir prototipos en InventarioCasas
-- ACTUALIZADO: Prototipos válidos son CALANDRA y TUNERA
-- Fecha: 2025-01-11
-- =============================================

-- 1. Mostrar prototipos actuales en InventarioCasas
PRINT '?? Prototipos actuales en InventarioCasas:'
SELECT 
    Manzana, 
    Lote, 
    ISNULL(Prototipo, '(NULL)') AS Prototipo,
    CASE 
        WHEN Prototipo = 'CALANDRA' THEN '? Correcto'
        WHEN Prototipo = 'TUNERA' THEN '? Correcto'
        ELSE '? Incorrecto o NULL'
    END AS Estado
FROM InventarioCasas 
ORDER BY Manzana, Lote

-- 2. Mostrar totales por prototipo en PresupuestoObra
PRINT ''
PRINT '?? Presupuestos totales disponibles:'
SELECT 
    Prototipo,
    COUNT(*) AS TotalPartidas,
    SUM(CostoTotal) AS PresupuestoTotal
FROM PresupuestoObra
GROUP BY Prototipo
ORDER BY Prototipo

-- 3. INSTRUCCIONES
PRINT ''
PRINT '==========================================='
PRINT '?? INSTRUCCIONES:'
PRINT 'Los prototipos válidos son:'
PRINT '  • CALANDRA  (~$2,662,190)'
PRINT '  • TUNERA    (~$2,723,465)'
PRINT ''
PRINT 'Descomenta y ejecuta UNA de las opciones siguientes:'
PRINT '==========================================='
PRINT ''

-- ============================================
-- OPCIÓN 1: Actualizar TODAS las casas a CALANDRA
-- ============================================
-- UPDATE InventarioCasas SET Prototipo = 'CALANDRA'

-- ============================================
-- OPCIÓN 2: Actualizar TODAS las casas a TUNERA
-- ============================================
-- UPDATE InventarioCasas SET Prototipo = 'TUNERA'

-- ============================================
-- OPCIÓN 3: Actualizar casas específicas por manzana y lote
-- ============================================
-- Ejemplos:
-- UPDATE InventarioCasas SET Prototipo = 'CALANDRA' WHERE Manzana = 'M1' AND Lote = 'L1'
-- UPDATE InventarioCasas SET Prototipo = 'TUNERA' WHERE Manzana = 'M1' AND Lote = 'L2'
-- UPDATE InventarioCasas SET Prototipo = 'CALANDRA' WHERE Manzana = 'M2' AND Lote = 'L1'

-- ============================================
-- OPCIÓN 4: Actualizar por rango de manzanas
-- ============================================
-- Ejemplo: Manzanas 1-5 son CALANDRA, 6-10 son TUNERA
-- UPDATE InventarioCasas 
-- SET Prototipo = 'CALANDRA' 
-- WHERE CAST(REPLACE(Manzana, 'M', '') AS INT) BETWEEN 1 AND 5

-- UPDATE InventarioCasas 
-- SET Prototipo = 'TUNERA' 
-- WHERE CAST(REPLACE(Manzana, 'M', '') AS INT) BETWEEN 6 AND 10

-- ============================================
-- OPCIÓN 5: Actualizar por rango de lotes
-- ============================================
-- Ejemplo: Lotes 1-10 son CALANDRA, 11-20 son TUNERA
-- UPDATE InventarioCasas 
-- SET Prototipo = 'CALANDRA' 
-- WHERE CAST(REPLACE(Lote, 'L', '') AS INT) BETWEEN 1 AND 10

-- UPDATE InventarioCasas 
-- SET Prototipo = 'TUNERA' 
-- WHERE CAST(REPLACE(Lote, 'L', '') AS INT) BETWEEN 11 AND 20

-- ============================================
-- OPCIÓN 6: Actualizar por patrón específico
-- ============================================
-- Ejemplo: Manzanas impares CALANDRA, pares TUNERA
-- UPDATE InventarioCasas 
-- SET Prototipo = 'CALANDRA' 
-- WHERE CAST(REPLACE(Manzana, 'M', '') AS INT) % 2 = 1

-- UPDATE InventarioCasas 
-- SET Prototipo = 'TUNERA' 
-- WHERE CAST(REPLACE(Manzana, 'M', '') AS INT) % 2 = 0

-- ============================================
-- 6. VERIFICACIÓN (EJECUTAR DESPUÉS DE ACTUALIZAR)
-- ============================================
PRINT ''
PRINT '? Verificar cambios aplicados:'
SELECT 
    ic.Manzana,
    ic.Lote,
    ic.Prototipo,
    CASE 
        WHEN ic.Prototipo = 'CALANDRA' THEN '? CALANDRA'
        WHEN ic.Prototipo = 'TUNERA' THEN '? TUNERA'
        ELSE '? INCORRECTO'
    END AS Estado,
    (SELECT SUM(CostoTotal) 
     FROM PresupuestoObra 
     WHERE Prototipo = ic.Prototipo) AS PresupuestoTotal,
    (SELECT COUNT(*) 
     FROM PresupuestoObra 
     WHERE Prototipo = ic.Prototipo) AS TotalPartidas
FROM InventarioCasas ic
ORDER BY ic.Manzana, ic.Lote

-- Resumen de distribución
PRINT ''
PRINT '?? Resumen de distribución:'
SELECT 
    Prototipo,
    COUNT(*) AS TotalCasas
FROM InventarioCasas
GROUP BY Prototipo
ORDER BY Prototipo

PRINT ''
PRINT '==========================================='
PRINT '? Script completado'
PRINT 'Si todos los estados muestran ?, puedes volver a la aplicación'
PRINT 'y cargar el avance sin problemas'
