-- =====================================================
-- DIAGNÓSTICO Y CORRECCIÓN DE WBS DESINCRONIZADOS
-- Ejecutar este script cuando el progreso se haya "roto"
-- después de agregar partidas o reorganizar WBS
-- =====================================================

-- 1. VER TIPOS DE DATOS DE LAS COLUMNAS WBS Y CODIGO
PRINT '=== TIPOS DE DATOS DE COLUMNAS CLAVE ===';
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME IN ('WBS', 'WBS_Correcto', 'Codigo', 'Padre', 'Etapa', 'Partida')
AND TABLE_NAME IN ('PresupuestoObra', 'AvanceManualObra')
ORDER BY TABLE_NAME, COLUMN_NAME;

-- 2. VER ESTRUCTURA COMPLETA DE PresupuestoObra
PRINT '';
PRINT '=== ESTRUCTURA DE PRESUPUESTOOBRA ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PresupuestoObra'
ORDER BY ORDINAL_POSITION;

-- 3. VER CONCEPTOS ÚNICOS (CÓDIGOS) EN PRESUPUESTOOBRA
PRINT '';
PRINT '=== CONCEPTOS (CÓDIGOS) EN PRESUPUESTOOBRA ===';
SELECT 
    Codigo,
    COUNT(*) AS CantidadPartidas,
    MIN(WBS_Correcto) AS WBS_Min,
    MAX(WBS_Correcto) AS WBS_Max,
    STRING_AGG(CAST(Padre AS NVARCHAR(100)), ', ') AS Padres
FROM PresupuestoObra
WHERE WBS_Correcto IS NOT NULL AND WBS_Correcto > 0
GROUP BY Codigo
ORDER BY 
    CASE WHEN ISNUMERIC(Codigo) = 1 THEN CAST(Codigo AS INT) ELSE 999 END,
    Codigo;

-- 4. VER PARTIDAS DE CADA CONCEPTO
PRINT '';
PRINT '=== DETALLE DE PARTIDAS POR CONCEPTO ===';
SELECT 
    Codigo,
    WBS_Correcto,
    Padre,
    Etapa,
    Partida,
    ISNULL(CostoTunera, 0) AS CostoTunera,
    ISNULL(CostoCalandra, 0) AS CostoCalandra,
    CASE WHEN ISNULL(EsDinamica, 0) = 1 THEN 'Dinámica' ELSE 'Fija' END AS TipoPartida
FROM PresupuestoObra
WHERE WBS_Correcto IS NOT NULL AND WBS_Correcto > 0
ORDER BY 
    CASE WHEN ISNUMERIC(Codigo) = 1 THEN CAST(Codigo AS INT) ELSE 999 END,
    WBS_Correcto;

-- 5. VER AVANCES REGISTRADOS QUE NO COINCIDEN CON PARTIDAS
PRINT '';
PRINT '=== AVANCES SIN PARTIDA CORRESPONDIENTE ===';
SELECT 
    amo.Manzana,
    amo.Lote,
    amo.WBS AS WBS_Avance,
    amo.AvancePorcentaje,
    amo.MontoEjecutado,
    amo.FechaFinalizacion,
    'SIN PARTIDA EN PRESUPUESTO' AS Estado
FROM AvanceManualObra amo
WHERE NOT EXISTS (
    SELECT 1 FROM PresupuestoObra p 
    WHERE CAST(p.WBS_Correcto AS NVARCHAR(50)) = CAST(amo.WBS AS NVARCHAR(50))
)
AND amo.AvancePorcentaje > 0
ORDER BY amo.Manzana, amo.Lote, amo.WBS;

-- 6. VER PARTIDAS CON AVANCE REGISTRADO
PRINT '';
PRINT '=== PARTIDAS CON AVANCE ===';
SELECT 
    p.Codigo,
    p.Padre,
    p.Etapa,
    p.Partida,
    p.WBS_Correcto,
    amo.Manzana,
    amo.Lote,
    amo.AvancePorcentaje,
    amo.MontoEjecutado,
    amo.MetrosCuadrados,
    amo.FechaFinalizacion
FROM PresupuestoObra p
LEFT JOIN AvanceManualObra amo ON CAST(p.WBS_Correcto AS NVARCHAR(50)) = CAST(amo.WBS AS NVARCHAR(50))
WHERE amo.AvancePorcentaje > 0 OR amo.MontoEjecutado > 0
ORDER BY p.Codigo, p.WBS_Correcto, amo.Manzana, amo.Lote;

-- 7. DETECTAR WBS DUPLICADOS EN PRESUPUESTOOBRA
PRINT '';
PRINT '=== WBS DUPLICADOS EN PRESUPUESTOOBRA ===';
SELECT 
    WBS_Correcto,
    COUNT(*) AS Cantidad,
    STRING_AGG(CAST(Codigo AS NVARCHAR(10)) + ': ' + ISNULL(Partida, 'Sin nombre'), ', ') AS Partidas
FROM PresupuestoObra
WHERE WBS_Correcto IS NOT NULL AND WBS_Correcto > 0
GROUP BY WBS_Correcto
HAVING COUNT(*) > 1
ORDER BY WBS_Correcto;

-- 8. VER PARTIDAS DINÁMICAS
PRINT '';
PRINT '=== PARTIDAS DINÁMICAS ===';
SELECT 
    Codigo,
    WBS_Correcto,
    Padre,
    Etapa,
    Partida,
    ValorM2Tunera,
    ValorM2Calandra,
    LimiteM2,
    ISNULL(LimiteM2Tunera, 0) AS LimiteM2Tunera,
    ISNULL(LimiteM2Calandra, 0) AS LimiteM2Calandra,
    PrototiposAplicables
FROM PresupuestoObra
WHERE ISNULL(EsDinamica, 0) = 1
AND WBS_Correcto IS NOT NULL AND WBS_Correcto > 0
ORDER BY Codigo, WBS_Correcto;

-- 9. RESUMEN POR PROTOTIPO
PRINT '';
PRINT '=== RESUMEN DE COSTOS POR CONCEPTO ===';
SELECT 
    Codigo,
    COUNT(*) AS CantidadPartidas,
    SUM(ISNULL(CostoTunera, 0)) AS TotalCostoTunera,
    SUM(ISNULL(CostoCalandra, 0)) AS TotalCostoCalandra,
    SUM(CASE WHEN ISNULL(EsDinamica, 0) = 1 THEN 1 ELSE 0 END) AS PartidasDinamicas
FROM PresupuestoObra
WHERE WBS_Correcto IS NOT NULL AND WBS_Correcto > 0
GROUP BY Codigo
ORDER BY 
    CASE WHEN ISNUMERIC(Codigo) = 1 THEN CAST(Codigo AS INT) ELSE 999 END;

-- =====================================================
-- CORRECCIÓN: Reagrupar WBS manualmente si es necesario
-- =====================================================

-- Para corregir WBS huérfanos en AvanceManualObra, 
-- primero necesitas identificar qué partida corresponde a cada avance.
-- Esto requiere intervención manual basada en Etapa + Partida

-- EJEMPLO: Si tienes un avance con WBS=999001 que debería ser WBS=45
-- UPDATE AvanceManualObra SET WBS = '45' WHERE WBS = '999001';

-- =====================================================
-- VERIFICAR PROTOTIPOS APLICABLES
-- =====================================================
PRINT '';
PRINT '=== VERIFICAR CONFIGURACIÓN DE PROTOTIPOS ===';
SELECT 
    Codigo,
    Partida,
    PrototiposAplicables,
    CASE 
        WHEN PrototiposAplicables IS NULL OR PrototiposAplicables = '' THEN 'Todos'
        WHEN PrototiposAplicables LIKE '%TUNERA%' AND PrototiposAplicables LIKE '%CALANDRA%' THEN 'Ambos'
        WHEN PrototiposAplicables LIKE '%TUNERA%' THEN 'Solo Tunera'
        WHEN PrototiposAplicables LIKE '%CALANDRA%' THEN 'Solo Calandra'
        ELSE 'Otros'
    END AS Aplica
FROM PresupuestoObra
WHERE WBS_Correcto IS NOT NULL AND WBS_Correcto > 0
AND PrototiposAplicables IS NOT NULL AND PrototiposAplicables != ''
ORDER BY Codigo, WBS_Correcto;

-- =====================================================
-- BACKUP ANTES DE CUALQUIER CORRECCIÓN
-- =====================================================
/*
-- Crear backup de AvanceManualObra
SELECT * INTO AvanceManualObra_Backup_YYYYMMDD
FROM AvanceManualObra;

-- Crear backup de PresupuestoObra
SELECT * INTO PresupuestoObra_Backup_YYYYMMDD
FROM PresupuestoObra;
*/
