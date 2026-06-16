-- ============================================================================
-- SCRIPT DE RECUPERACIÓN: Avances con WBS desincronizados
-- ============================================================================
-- EJECUTAR PASO A PASO - NO TODO DE UNA VEZ
-- ============================================================================

-- ============================================================================
-- PASO 1: DIAGNÓSTICO - VER EL ESTADO ACTUAL
-- ============================================================================

PRINT '=== PASO 1: DIAGNÓSTICO ==='
PRINT ''

-- 1.1 Ver tipo de dato de WBS en cada tabla
SELECT 
    'PresupuestoObra' AS Tabla, 
    COLUMN_NAME, 
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'WBS_Correcto'
UNION ALL
SELECT 
    'AvanceManualObra' AS Tabla, 
    COLUMN_NAME, 
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = 'WBS';

-- 1.2 Ver avances que NO tienen partida correspondiente (HUÉRFANOS)
PRINT ''
PRINT '=== AVANCES HUÉRFANOS (sin partida en PresupuestoObra) ==='
SELECT 
    a.Manzana,
    a.Lote,
    a.WBS AS WBS_Avance,
    a.AvancePorcentaje,
    a.MontoEjecutado,
    a.FechaFinalizacion,
    CASE 
        WHEN CAST(a.WBS AS INT) >= 800000 THEN 'WBS TEMPORAL (800000+)'
        WHEN CAST(a.WBS AS INT) >= 999000 THEN 'WBS NUEVO (999000+)'
        ELSE 'WBS NORMAL'
    END AS TipoWBS
FROM AvanceManualObra a
WHERE NOT EXISTS (
    SELECT 1 FROM PresupuestoObra p 
    WHERE CAST(p.WBS_Correcto AS NVARCHAR(50)) = CAST(a.WBS AS NVARCHAR(50))
)
AND (a.AvancePorcentaje > 0 OR a.MontoEjecutado > 0)
ORDER BY a.Manzana, a.Lote, a.WBS;

-- 1.3 Ver WBS duplicados en PresupuestoObra
PRINT ''
PRINT '=== WBS DUPLICADOS EN PRESUPUESTOOBRA ==='
SELECT 
    WBS_Correcto,
    COUNT(*) AS Cantidad,
    STRING_AGG(Codigo + ': ' + Partida, ' | ') AS Partidas
FROM PresupuestoObra
WHERE WBS_Correcto IS NOT NULL
GROUP BY WBS_Correcto
HAVING COUNT(*) > 1;

-- 1.4 Ver rango de WBS por concepto
PRINT ''
PRINT '=== RANGO DE WBS POR CONCEPTO ==='
SELECT 
    Codigo,
    Padre AS Concepto,
    MIN(CAST(WBS_Correcto AS INT)) AS WBS_Min,
    MAX(CAST(WBS_Correcto AS INT)) AS WBS_Max,
    COUNT(*) AS TotalPartidas
FROM PresupuestoObra
WHERE WBS_Correcto IS NOT NULL 
AND CAST(WBS_Correcto AS INT) < 800000
GROUP BY Codigo, Padre
ORDER BY Codigo;

-- ============================================================================
-- PASO 2: CREAR BACKUP ANTES DE CUALQUIER CAMBIO
-- ============================================================================
/*
-- DESCOMENTA Y EJECUTA ESTO PRIMERO:

-- Backup de AvanceManualObra
IF OBJECT_ID('AvanceManualObra_Backup_Recuperacion', 'U') IS NOT NULL
    DROP TABLE AvanceManualObra_Backup_Recuperacion;

SELECT * INTO AvanceManualObra_Backup_Recuperacion
FROM AvanceManualObra;

PRINT 'Backup creado: AvanceManualObra_Backup_Recuperacion'
PRINT 'Registros: ' + CAST(@@ROWCOUNT AS VARCHAR)

-- Backup de PresupuestoObra
IF OBJECT_ID('PresupuestoObra_Backup_Recuperacion', 'U') IS NOT NULL
    DROP TABLE PresupuestoObra_Backup_Recuperacion;

SELECT * INTO PresupuestoObra_Backup_Recuperacion
FROM PresupuestoObra;

PRINT 'Backup creado: PresupuestoObra_Backup_Recuperacion'
PRINT 'Registros: ' + CAST(@@ROWCOUNT AS VARCHAR)
*/

-- ============================================================================
-- PASO 3: IDENTIFICAR LA CORRESPONDENCIA
-- ============================================================================

-- 3.1 Ver la correspondencia entre avances huérfanos y partidas actuales
-- Esto intenta vincular por Manzana/Lote/Prototipo y orden de partida
PRINT ''
PRINT '=== POSIBLE CORRESPONDENCIA (por posición en lista) ==='

;WITH AvancesHuerfanos AS (
    SELECT 
        a.Manzana,
        a.Lote,
        a.WBS AS WBS_Viejo,
        a.AvancePorcentaje,
        a.MontoEjecutado,
        a.FechaFinalizacion,
        a.Prototipo,
        ROW_NUMBER() OVER (PARTITION BY a.Manzana, a.Lote ORDER BY CAST(a.WBS AS INT)) AS OrdenAvance
    FROM AvanceManualObra a
    WHERE NOT EXISTS (
        SELECT 1 FROM PresupuestoObra p 
        WHERE CAST(p.WBS_Correcto AS NVARCHAR(50)) = CAST(a.WBS AS NVARCHAR(50))
    )
    AND (a.AvancePorcentaje > 0 OR a.MontoEjecutado > 0)
),
PartidasActuales AS (
    SELECT 
        WBS_Correcto,
        Codigo,
        Padre,
        Etapa,
        Partida,
        ROW_NUMBER() OVER (ORDER BY CAST(WBS_Correcto AS INT)) AS OrdenPartida
    FROM PresupuestoObra
    WHERE WBS_Correcto IS NOT NULL
    AND CAST(WBS_Correcto AS INT) < 800000
)
SELECT TOP 50
    ah.Manzana,
    ah.Lote,
    ah.WBS_Viejo,
    ah.AvancePorcentaje,
    ah.MontoEjecutado,
    '--->' AS Mapeo,
    pa.WBS_Correcto AS WBS_Nuevo_Posible,
    pa.Etapa + ' > ' + pa.Partida AS PartidaActual
FROM AvancesHuerfanos ah
LEFT JOIN PartidasActuales pa ON ah.OrdenAvance = pa.OrdenPartida
ORDER BY ah.Manzana, ah.Lote, ah.WBS_Viejo;

-- ============================================================================
-- PASO 4: CORRECCIÓN MANUAL (EJEMPLO)
-- ============================================================================
/*
-- Si identificaste que WBS 800001 debería ser WBS 46, ejecuta:

UPDATE AvanceManualObra 
SET WBS = '46' 
WHERE WBS = '800001';

-- O si tienes varios mapeos:
UPDATE AvanceManualObra SET WBS = '45' WHERE WBS = '800000';
UPDATE AvanceManualObra SET WBS = '46' WHERE WBS = '800001';
UPDATE AvanceManualObra SET WBS = '47' WHERE WBS = '800002';
-- etc...
*/

-- ============================================================================
-- PASO 5: VERIFICACIÓN POST-CORRECCIÓN
-- ============================================================================

-- Después de corregir, ejecuta esto para verificar:
PRINT ''
PRINT '=== VERIFICACIÓN: Avances aún huérfanos ==='
SELECT COUNT(*) AS AvancesHuerfanosRestantes
FROM AvanceManualObra a
WHERE NOT EXISTS (
    SELECT 1 FROM PresupuestoObra p 
    WHERE CAST(p.WBS_Correcto AS NVARCHAR(50)) = CAST(a.WBS AS NVARCHAR(50))
)
AND (a.AvancePorcentaje > 0 OR a.MontoEjecutado > 0);

-- ============================================================================
-- PASO 6: SI TODO FALLA - RESTAURAR BACKUP
-- ============================================================================
/*
-- Si algo salió mal, restaura desde el backup:

TRUNCATE TABLE AvanceManualObra;
INSERT INTO AvanceManualObra SELECT * FROM AvanceManualObra_Backup_Recuperacion;

PRINT 'Datos restaurados desde backup'
*/

-- ============================================================================
-- SCRIPT DE RECUPERACIÓN: Avances con WBS negativos
-- ============================================================================
-- PROBLEMA DETECTADO: Los WBS en AvanceManualObra están guardados como 
-- negativos (-1, -2, -4, etc.) pero en PresupuestoObra son positivos (1, 2, 4)
-- ============================================================================

-- ============================================================================
-- PASO 1: DIAGNÓSTICO - VER EL PROBLEMA
-- ============================================================================

PRINT '=== PASO 1: DIAGNÓSTICO ==='

-- 1.1 Ver avances con WBS negativos
PRINT ''
PRINT '=== AVANCES CON WBS NEGATIVOS ==='
SELECT 
    Manzana,
    Lote,
    WBS AS WBS_Actual,
    ABS(CAST(WBS AS INT)) AS WBS_Correcto,
    AvancePorcentaje,
    MontoEjecutado,
    FechaFinalizacion
FROM AvanceManualObra
WHERE CAST(WBS AS INT) < 0
ORDER BY Manzana, Lote, WBS;

-- 1.2 Contar cuántos hay
PRINT ''
PRINT '=== CONTEO ==='
SELECT 
    COUNT(*) AS TotalConWBSNegativo,
    COUNT(CASE WHEN AvancePorcentaje > 0 OR MontoEjecutado > 0 THEN 1 END) AS ConProgreso
FROM AvanceManualObra
WHERE CAST(WBS AS INT) < 0;

-- 1.3 Verificar que los WBS positivos correspondientes existen en PresupuestoObra
PRINT ''
PRINT '=== VERIFICAR CORRESPONDENCIA ==='
SELECT 
    a.Manzana,
    a.Lote,
    a.WBS AS WBS_Negativo,
    ABS(CAST(a.WBS AS INT)) AS WBS_Positivo,
    a.MontoEjecutado,
    p.Etapa + ' > ' + p.Partida AS Partida,
    p.CostoTunera AS CostoPresupuestado,
    CASE 
        WHEN p.WBS_Correcto IS NOT NULL THEN '? ENCONTRADA'
        ELSE '? NO ENCONTRADA'
    END AS Estado
FROM AvanceManualObra a
LEFT JOIN PresupuestoObra p ON ABS(CAST(a.WBS AS INT)) = CAST(p.WBS_Correcto AS INT)
WHERE CAST(a.WBS AS INT) < 0
AND (a.AvancePorcentaje > 0 OR a.MontoEjecutado > 0)
ORDER BY a.Manzana, a.Lote, a.WBS;

-- ============================================================================
-- PASO 2: CREAR BACKUP
-- ============================================================================
/*
-- DESCOMENTA Y EJECUTA PRIMERO:

IF OBJECT_ID('AvanceManualObra_Backup_WBSNegativos', 'U') IS NOT NULL
    DROP TABLE AvanceManualObra_Backup_WBSNegativos;

SELECT * INTO AvanceManualObra_Backup_WBSNegativos
FROM AvanceManualObra;

PRINT 'Backup creado: AvanceManualObra_Backup_WBSNegativos'
PRINT 'Registros: ' + CAST(@@ROWCOUNT AS VARCHAR)
*/

-- ============================================================================
-- PASO 3: CORREGIR - CONVERTIR WBS NEGATIVOS A POSITIVOS
-- ============================================================================
/*
-- DESCOMENTA Y EJECUTA DESPUÉS DEL BACKUP:

PRINT '=== CORRIGIENDO WBS NEGATIVOS ==='

UPDATE AvanceManualObra
SET WBS = CAST(ABS(CAST(WBS AS INT)) AS NVARCHAR(50))
WHERE CAST(WBS AS INT) < 0;

PRINT 'Registros actualizados: ' + CAST(@@ROWCOUNT AS VARCHAR)
*/

-- ============================================================================
-- PASO 4: VERIFICACIÓN POST-CORRECCIÓN
-- ============================================================================

-- Ejecutar después de la corrección:
PRINT ''
PRINT '=== VERIFICACIÓN POST-CORRECCIÓN ==='

-- 4.1 Ya no debe haber WBS negativos
SELECT 
    COUNT(*) AS WBSNegativosRestantes
FROM AvanceManualObra
WHERE CAST(WBS AS INT) < 0;

-- 4.2 Ver avances huérfanos (si aún quedan)
SELECT 
    COUNT(*) AS AvancesHuerfanos
FROM AvanceManualObra a
WHERE NOT EXISTS (
    SELECT 1 FROM PresupuestoObra p 
    WHERE CAST(p.WBS_Correcto AS INT) = CAST(a.WBS AS INT)
)
AND (a.AvancePorcentaje > 0 OR a.MontoEjecutado > 0);

-- 4.3 Ver muestra de avances ahora vinculados correctamente
SELECT TOP 20
    a.Manzana,
    a.Lote,
    a.WBS,
    a.AvancePorcentaje,
    a.MontoEjecutado,
    p.Etapa + ' > ' + p.Partida AS Partida,
    '? OK' AS Estado
FROM AvanceManualObra a
INNER JOIN PresupuestoObra p ON CAST(p.WBS_Correcto AS INT) = CAST(a.WBS AS INT)
WHERE a.AvancePorcentaje > 0 OR a.MontoEjecutado > 0
ORDER BY a.Manzana, a.Lote, CAST(a.WBS AS INT);

-- ============================================================================
-- PASO 5: SI ALGO SALIÓ MAL - RESTAURAR
-- ============================================================================
/*
-- Si necesitas restaurar:

DELETE FROM AvanceManualObra;
INSERT INTO AvanceManualObra SELECT * FROM AvanceManualObra_Backup_WBSNegativos;

PRINT 'Datos restaurados desde backup'
*/


-- ============================================================================
-- NOTAS IMPORTANTES:
-- ============================================================================
-- 1. Los WBS 800000+ son temporales usados durante reorganización
-- 2. Los WBS 999000+ son para partidas nuevas pendientes de asignar
-- 3. Si ves WBS 800000+ en AvanceManualObra, la FASE 2 de reorganización falló
-- 4. La solución es mapear manualmente o encontrar el patrón de correspondencia
-- ============================================================================
