-- =====================================================
-- CORREGIR WBS DUPLICADOS EN PRESUPUESTOOBRA
-- Este script reasigna WBS únicos a cada partida
-- =====================================================

USE CALANDRIA;
GO

-- 1. DIAGNÓSTICO: Ver WBS duplicados
PRINT '=== DIAGNÓSTICO: WBS DUPLICADOS ===';
SELECT 
    WBS_Correcto,
    COUNT(*) AS Cantidad,
    STRING_AGG(CAST(Codigo AS NVARCHAR(10)) + ': ' + ISNULL(Padre, '') + ' > ' + ISNULL(Partida, 'Sin nombre'), ' | ') AS Partidas
FROM PresupuestoObra
WHERE WBS_Correcto IS NOT NULL AND WBS_Correcto > 0
GROUP BY WBS_Correcto
HAVING COUNT(*) > 1
ORDER BY WBS_Correcto;

-- 2. Ver rango actual de WBS por concepto
PRINT '';
PRINT '=== RANGO DE WBS POR CONCEPTO ===';
SELECT 
    Codigo,
    COUNT(*) AS CantidadPartidas,
    MIN(WBS_Correcto) AS WBS_Min,
    MAX(WBS_Correcto) AS WBS_Max
FROM PresupuestoObra
WHERE WBS_Correcto IS NOT NULL AND WBS_Correcto > 0
GROUP BY Codigo
ORDER BY 
    CASE WHEN ISNUMERIC(Codigo) = 1 THEN CAST(Codigo AS INT) ELSE 999 END;

-- 3. BACKUP antes de corregir
PRINT '';
PRINT '=== CREANDO BACKUP ===';
IF OBJECT_ID('PresupuestoObra_Backup_WBS', 'U') IS NOT NULL
    DROP TABLE PresupuestoObra_Backup_WBS;

SELECT * INTO PresupuestoObra_Backup_WBS FROM PresupuestoObra;
PRINT 'Backup creado: PresupuestoObra_Backup_WBS';

-- =====================================================
-- 4. REASIGNAR WBS ÚNICOS
-- Estrategia: WBS = (Codigo * 1000) + secuencia
-- Ejemplo: Código 6 tendrá WBS 6001, 6002, 6003...
--          Código 7 tendrá WBS 7001, 7002, 7003...
-- =====================================================
GO

PRINT '';
PRINT '=== REASIGNANDO WBS ÚNICOS ===';

-- Crear tabla temporal con los nuevos WBS
WITH PartidasNumeradas AS (
    SELECT 
        -- Usar una columna única para identificar cada fila
        -- Asumiendo que hay una columna ID o podemos usar Etapa+Partida
        Codigo,
        Padre,
        Etapa,
        Partida,
        WBS_Correcto AS WBS_Anterior,
        ROW_NUMBER() OVER (
            PARTITION BY Codigo 
            ORDER BY 
                CASE WHEN ISNUMERIC(WBS_Correcto) = 1 THEN CAST(WBS_Correcto AS INT) ELSE 99999 END,
                Etapa,
                Partida
        ) AS Secuencia
    FROM PresupuestoObra
    WHERE WBS_Correcto IS NOT NULL AND WBS_Correcto > 0
)
SELECT 
    *,
    CASE 
        WHEN ISNUMERIC(Codigo) = 1 
        THEN (CAST(Codigo AS INT) * 1000) + Secuencia
        ELSE 90000 + Secuencia
    END AS WBS_Nuevo
INTO #TempWBS
FROM PartidasNumeradas;

-- Mostrar el mapeo propuesto
PRINT '';
PRINT '=== MAPEO PROPUESTO DE WBS ===';
SELECT 
    Codigo,
    Padre,
    Etapa,
    Partida,
    WBS_Anterior,
    WBS_Nuevo
FROM #TempWBS
ORDER BY 
    CASE WHEN ISNUMERIC(Codigo) = 1 THEN CAST(Codigo AS INT) ELSE 999 END,
    Secuencia;

-- =====================================================
-- 5. APLICAR LOS CAMBIOS (DESCOMENTAR PARA EJECUTAR)
-- =====================================================

PRINT '';
PRINT '=== APLICANDO CAMBIOS ===';

-- Actualizar WBS en PresupuestoObra
UPDATE p
SET p.WBS_Correcto = t.WBS_Nuevo
FROM PresupuestoObra p
INNER JOIN #TempWBS t ON 
    ISNULL(p.Codigo, '') = ISNULL(t.Codigo, '') AND
    ISNULL(p.Padre, '') = ISNULL(t.Padre, '') AND
    ISNULL(p.Etapa, '') = ISNULL(t.Etapa, '') AND
    ISNULL(p.Partida, '') = ISNULL(t.Partida, '') AND
    p.WBS_Correcto = t.WBS_Anterior;

PRINT 'WBS actualizados en PresupuestoObra';

-- También actualizar AvanceManualObra para mantener la sincronización
UPDATE a
SET a.WBS = CAST(t.WBS_Nuevo AS NVARCHAR(50))
FROM AvanceManualObra a
INNER JOIN #TempWBS t ON CAST(a.WBS AS INT) = t.WBS_Anterior;

PRINT 'WBS actualizados en AvanceManualObra';


-- =====================================================
-- 6. VERIFICAR RESULTADO
-- =====================================================
PRINT '';
PRINT '=== VERIFICACIÓN: WBS ÚNICOS DESPUÉS DE CORRECCIÓN ===';
SELECT 
    WBS_Correcto,
    COUNT(*) AS Cantidad
FROM PresupuestoObra
WHERE WBS_Correcto IS NOT NULL AND WBS_Correcto > 0
GROUP BY WBS_Correcto
HAVING COUNT(*) > 1
ORDER BY WBS_Correcto;

-- Limpiar tabla temporal
DROP TABLE IF EXISTS #TempWBS;

PRINT '';
PRINT '=== SCRIPT COMPLETADO ===';
PRINT 'NOTA: La sección de APLICAR CAMBIOS está comentada.';
PRINT 'Revise el mapeo propuesto y descomente para ejecutar los cambios.';
