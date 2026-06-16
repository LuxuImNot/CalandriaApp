-- =====================================================
-- DIAGNÓSTICO COMPLETO DE VÍNCULOS WBS
-- 
-- Ejecuta este script ANTES de hacer cualquier cambio
-- para entender el estado actual de tus datos.
-- =====================================================

SET NOCOUNT ON;

PRINT '=========================================================';
PRINT 'DIAGNÓSTICO DE VÍNCULOS WBS - PresupuestoObra ? AvanceManualObra';
PRINT 'Fecha: ' + CONVERT(NVARCHAR(20), GETDATE(), 120);
PRINT '=========================================================';
PRINT '';

-- =====================================================
-- 1. ESTRUCTURA DE TABLAS
-- =====================================================
PRINT '=== 1. ESTRUCTURA DE TABLAS ===';
PRINT '';

PRINT 'Columnas en PresupuestoObra:';
SELECT 
    COLUMN_NAME AS Columna,
    DATA_TYPE AS Tipo,
    CASE WHEN CHARACTER_MAXIMUM_LENGTH IS NOT NULL 
         THEN CAST(CHARACTER_MAXIMUM_LENGTH AS NVARCHAR(10)) 
         ELSE '-' END AS Longitud,
    IS_NULLABLE AS Nullable
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PresupuestoObra'
AND COLUMN_NAME IN ('IdPresupuestoObra', 'WBS_Correcto', 'WBS', 'Codigo', 'Etapa', 'Partida')
ORDER BY 
    CASE COLUMN_NAME 
        WHEN 'IdPresupuestoObra' THEN 1
        WHEN 'WBS_Correcto' THEN 2
        WHEN 'WBS' THEN 3
        ELSE 4
    END;

PRINT '';
PRINT 'Columnas en AvanceManualObra:';
SELECT 
    COLUMN_NAME AS Columna,
    DATA_TYPE AS Tipo,
    CASE WHEN CHARACTER_MAXIMUM_LENGTH IS NOT NULL 
         THEN CAST(CHARACTER_MAXIMUM_LENGTH AS NVARCHAR(10)) 
         ELSE '-' END AS Longitud,
    IS_NULLABLE AS Nullable
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AvanceManualObra'
AND COLUMN_NAME IN ('IdPresupuestoObra', 'WBS_Correcto', 'WBS', 'Manzana', 'Lote', 'AvancePorcentaje', 'MontoEjecutado')
ORDER BY 
    CASE COLUMN_NAME 
        WHEN 'IdPresupuestoObra' THEN 1
        WHEN 'WBS_Correcto' THEN 2
        WHEN 'WBS' THEN 3
        ELSE 4
    END;

-- =====================================================
-- 2. VERIFICAR SI EXISTE IdPresupuestoObra
-- =====================================================
PRINT '';
PRINT '=== 2. ESTADO DE IdPresupuestoObra ===';

DECLARE @tieneIdEnPresup BIT = 0, @tieneIdEnAvance BIT = 0;

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'IdPresupuestoObra')
BEGIN
    SET @tieneIdEnPresup = 1;
    PRINT '? IdPresupuestoObra EXISTE en PresupuestoObra';
END
ELSE
BEGIN
    PRINT '? IdPresupuestoObra NO EXISTE en PresupuestoObra';
    PRINT '   ? Ejecuta: CrearVinculoPermanenteWBS.sql';
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = 'IdPresupuestoObra')
BEGIN
    SET @tieneIdEnAvance = 1;
    PRINT '? IdPresupuestoObra EXISTE en AvanceManualObra';
END
ELSE
BEGIN
    PRINT '? IdPresupuestoObra NO EXISTE en AvanceManualObra';
    PRINT '   ? Ejecuta: CrearVinculoPermanenteWBS.sql';
END

-- =====================================================
-- 3. CONTEO DE REGISTROS
-- =====================================================
PRINT '';
PRINT '=== 3. CONTEO DE REGISTROS ===';

DECLARE @totalPresup INT, @totalAvance INT;
SELECT @totalPresup = COUNT(*) FROM PresupuestoObra;
SELECT @totalAvance = COUNT(*) FROM AvanceManualObra;

PRINT 'Total partidas en PresupuestoObra: ' + CAST(@totalPresup AS NVARCHAR(10));
PRINT 'Total registros en AvanceManualObra: ' + CAST(@totalAvance AS NVARCHAR(10));

-- =====================================================
-- 4. ESTADO DE VINCULACIÓN (si existe IdPresupuestoObra)
-- =====================================================
IF @tieneIdEnPresup = 1 AND @tieneIdEnAvance = 1
BEGIN
    PRINT '';
    PRINT '=== 4. ESTADO DE VINCULACIÓN ===';
    
    DECLARE @vinculados INT, @sinVincular INT, @sinVincularConProgreso INT;
    
    SELECT @vinculados = COUNT(*) FROM AvanceManualObra WHERE IdPresupuestoObra IS NOT NULL;
    SELECT @sinVincular = COUNT(*) FROM AvanceManualObra WHERE IdPresupuestoObra IS NULL;
    
    -- Detectar columna WBS
    DECLARE @colWBS NVARCHAR(50) = 'WBS';
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = 'WBS_Correcto')
        SET @colWBS = 'WBS_Correcto';
    
    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT @cnt = COUNT(*) FROM AvanceManualObra 
        WHERE IdPresupuestoObra IS NULL 
        AND (ISNULL(AvancePorcentaje, 0) > 0 OR ISNULL(MontoEjecutado, 0) > 0)';
    EXEC sp_executesql @sql, N'@cnt INT OUTPUT', @cnt = @sinVincularConProgreso OUTPUT;
    
    PRINT 'Registros vinculados (con IdPresupuestoObra): ' + CAST(@vinculados AS NVARCHAR(10));
    PRINT 'Registros sin vincular: ' + CAST(@sinVincular AS NVARCHAR(10));
    PRINT '?? Sin vincular CON PROGRESO (críticos): ' + CAST(@sinVincularConProgreso AS NVARCHAR(10));
    
    IF @sinVincularConProgreso > 0
    BEGIN
        PRINT '';
        PRINT '=== REGISTROS CRÍTICOS SIN VINCULAR ===';
        
        DECLARE @sqlCriticos NVARCHAR(MAX) = N'
            SELECT TOP 30
                Manzana,
                Lote,
                ' + @colWBS + N' AS WBS,
                AvancePorcentaje,
                ISNULL(MontoEjecutado, 0) AS MontoEjecutado,
                ''NECESITA VINCULACIÓN'' AS Estado
            FROM AvanceManualObra 
            WHERE IdPresupuestoObra IS NULL 
            AND (ISNULL(AvancePorcentaje, 0) > 0 OR ISNULL(MontoEjecutado, 0) > 0)
            ORDER BY Manzana, Lote, ' + @colWBS;
        
        EXEC sp_executesql @sqlCriticos;
    END
END

-- =====================================================
-- 5. ANÁLISIS DE WBS
-- =====================================================
PRINT '';
PRINT '=== 5. ANÁLISIS DE WBS ===';

-- WBS en PresupuestoObra
PRINT '';
PRINT 'Rango de WBS en PresupuestoObra:';
SELECT 
    MIN(CAST(WBS_Correcto AS INT)) AS WBS_Minimo,
    MAX(CAST(WBS_Correcto AS INT)) AS WBS_Maximo,
    COUNT(DISTINCT WBS_Correcto) AS WBS_Unicos,
    COUNT(*) AS TotalRegistros
FROM PresupuestoObra
WHERE WBS_Correcto IS NOT NULL 
AND ISNUMERIC(WBS_Correcto) = 1;

-- WBS duplicados
PRINT '';
PRINT 'WBS duplicados en PresupuestoObra:';
SELECT 
    WBS_Correcto,
    COUNT(*) AS Cantidad,
    STRING_AGG(CAST(Codigo AS NVARCHAR(10)), ', ') AS Codigos
FROM PresupuestoObra
WHERE WBS_Correcto IS NOT NULL
GROUP BY WBS_Correcto
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;

-- =====================================================
-- 6. PARTIDAS POR CONCEPTO
-- =====================================================
PRINT '';
PRINT '=== 6. PARTIDAS POR CONCEPTO ===';

SELECT 
    Codigo,
    COUNT(*) AS CantidadPartidas,
    MIN(CAST(WBS_Correcto AS INT)) AS WBS_Inicio,
    MAX(CAST(WBS_Correcto AS INT)) AS WBS_Fin,
    MAX(CAST(WBS_Correcto AS INT)) - MIN(CAST(WBS_Correcto AS INT)) + 1 AS RangoEsperado,
    CASE 
        WHEN COUNT(*) = MAX(CAST(WBS_Correcto AS INT)) - MIN(CAST(WBS_Correcto AS INT)) + 1 
        THEN '? Secuencial'
        ELSE '?? Huecos'
    END AS Estado
FROM PresupuestoObra
WHERE WBS_Correcto IS NOT NULL 
AND ISNUMERIC(WBS_Correcto) = 1
GROUP BY Codigo
ORDER BY 
    CASE WHEN ISNUMERIC(Codigo) = 1 THEN CAST(Codigo AS INT) ELSE 999 END;

-- =====================================================
-- 7. AVANCES POR CONCEPTO
-- =====================================================
PRINT '';
PRINT '=== 7. AVANCES CON PROGRESO POR CONCEPTO ===';

DECLARE @colWBSFinal NVARCHAR(50) = 'WBS';
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = 'WBS_Correcto')
    SET @colWBSFinal = 'WBS_Correcto';

DECLARE @sqlAvances NVARCHAR(MAX) = N'
    SELECT 
        p.Codigo,
        p.Padre AS Concepto,
        COUNT(DISTINCT CONCAT(amo.Manzana, ''-'', amo.Lote)) AS ObrasConAvance,
        COUNT(*) AS TotalRegistrosAvance,
        AVG(amo.AvancePorcentaje) AS PromedioAvance
    FROM AvanceManualObra amo
    INNER JOIN PresupuestoObra p ON amo.' + @colWBSFinal + N' = p.WBS_Correcto
    WHERE amo.AvancePorcentaje > 0 OR ISNULL(amo.MontoEjecutado, 0) > 0
    GROUP BY p.Codigo, p.Padre
    ORDER BY p.Codigo';

EXEC sp_executesql @sqlAvances;

-- =====================================================
-- 8. RECOMENDACIONES
-- =====================================================
PRINT '';
PRINT '=========================================================';
PRINT 'RECOMENDACIONES';
PRINT '=========================================================';

IF @tieneIdEnPresup = 0 OR @tieneIdEnAvance = 0
BEGIN
    PRINT '1. ? CRÍTICO: Falta IdPresupuestoObra';
    PRINT '   Ejecuta: CrearVinculoPermanenteWBS.sql';
    PRINT '';
END

IF @tieneIdEnPresup = 1 AND @tieneIdEnAvance = 1
BEGIN
    IF @sinVincularConProgreso > 0
    BEGIN
        PRINT '1. ?? Hay ' + CAST(@sinVincularConProgreso AS NVARCHAR(10)) + ' avances con progreso sin vincular';
        PRINT '   Ejecuta: VincularProgresoExistente.sql';
        PRINT '';
    END
    ELSE
    BEGIN
        PRINT '1. ? Todos los avances con progreso están vinculados';
        PRINT '';
    END
END

PRINT '2. Después de vincular, puedes agregar partidas sin perder el progreso.';
PRINT '   El IdPresupuestoObra es permanente y no cambia al reorganizar WBS.';
PRINT '';
PRINT '=========================================================';
