-- =====================================================
-- VINCULAR PROGRESO EXISTENTE CON IdPresupuestoObra
-- 
-- Este script vincula todos los registros de AvanceManualObra
-- existentes con su correspondiente partida en PresupuestoObra
-- usando el WBS actual como referencia inicial.
--
-- EJECUTAR DESPUÉS DE: CrearVinculoPermanenteWBS.sql
-- =====================================================

SET NOCOUNT ON;

PRINT '=========================================================';
PRINT 'VINCULACIÓN DE PROGRESO EXISTENTE';
PRINT 'Fecha: ' + CONVERT(NVARCHAR(20), GETDATE(), 120);
PRINT '=========================================================';
PRINT '';

-- =====================================================
-- PASO 0: Verificar que existan las columnas necesarias
-- =====================================================
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'IdPresupuestoObra'
)
BEGIN
    PRINT '? ERROR: La columna IdPresupuestoObra no existe en PresupuestoObra';
    PRINT '   Ejecuta primero el script: CrearVinculoPermanenteWBS.sql';
    RETURN;
END

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = 'IdPresupuestoObra'
)
BEGIN
    PRINT '? ERROR: La columna IdPresupuestoObra no existe en AvanceManualObra';
    PRINT '   Ejecuta primero el script: CrearVinculoPermanenteWBS.sql';
    RETURN;
END

PRINT '? Columnas IdPresupuestoObra verificadas en ambas tablas';
PRINT '';

-- =====================================================
-- PASO 1: Diagnóstico inicial
-- =====================================================
PRINT '=== DIAGNÓSTICO INICIAL ===';

-- Detectar nombre de columna WBS en AvanceManualObra
DECLARE @colWBSAvance NVARCHAR(50);
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = 'WBS_Correcto')
    SET @colWBSAvance = 'WBS_Correcto';
ELSE
    SET @colWBSAvance = 'WBS';

PRINT 'Columna WBS en AvanceManualObra: ' + @colWBSAvance;

-- Contar registros
DECLARE @totalAvances INT, @avancesConVinculo INT, @avancesSinVinculo INT;
DECLARE @avancesConProgreso INT;

SELECT @totalAvances = COUNT(*) FROM AvanceManualObra;
SELECT @avancesConVinculo = COUNT(*) FROM AvanceManualObra WHERE IdPresupuestoObra IS NOT NULL;
SELECT @avancesSinVinculo = COUNT(*) FROM AvanceManualObra WHERE IdPresupuestoObra IS NULL;

DECLARE @sqlContarProgreso NVARCHAR(MAX) = N'
    SELECT @cnt = COUNT(*) 
    FROM AvanceManualObra 
    WHERE IdPresupuestoObra IS NULL 
    AND ' + @colWBSAvance + N' IS NOT NULL
    AND (ISNULL(AvancePorcentaje, 0) > 0 OR ISNULL(MontoEjecutado, 0) > 0)';

EXEC sp_executesql @sqlContarProgreso, N'@cnt INT OUTPUT', @cnt = @avancesConProgreso OUTPUT;

PRINT '';
PRINT 'Total registros en AvanceManualObra: ' + CAST(@totalAvances AS NVARCHAR(10));
PRINT 'Ya vinculados (con IdPresupuestoObra): ' + CAST(@avancesConVinculo AS NVARCHAR(10));
PRINT 'Sin vincular: ' + CAST(@avancesSinVinculo AS NVARCHAR(10));
PRINT 'Sin vincular CON PROGRESO (críticos): ' + CAST(@avancesConProgreso AS NVARCHAR(10));
PRINT '';

IF @avancesSinVinculo = 0
BEGIN
    PRINT '? Todos los registros ya están vinculados. No hay nada que hacer.';
    RETURN;
END

-- =====================================================
-- PASO 2: Mostrar avances sin vincular (para revisión)
-- =====================================================
PRINT '=== AVANCES SIN VINCULAR (primeros 20) ===';

DECLARE @sqlMostrar NVARCHAR(MAX) = N'
    SELECT TOP 20
        amo.Manzana,
        amo.Lote,
        amo.' + @colWBSAvance + N' AS WBS_Avance,
        amo.AvancePorcentaje,
        ISNULL(amo.MontoEjecutado, 0) AS MontoEjecutado,
        CASE 
            WHEN p.IdPresupuestoObra IS NOT NULL THEN ''? Encontrado: '' + p.Partida
            ELSE ''? NO ENCONTRADO''
        END AS EstadoVinculo,
        p.Codigo,
        p.Etapa,
        p.Partida
    FROM AvanceManualObra amo
    LEFT JOIN PresupuestoObra p ON amo.' + @colWBSAvance + N' = p.WBS_Correcto
    WHERE amo.IdPresupuestoObra IS NULL
    AND amo.' + @colWBSAvance + N' IS NOT NULL
    ORDER BY 
        CASE WHEN ISNULL(amo.AvancePorcentaje, 0) > 0 OR ISNULL(amo.MontoEjecutado, 0) > 0 THEN 0 ELSE 1 END,
        amo.Manzana, 
        amo.Lote';

EXEC sp_executesql @sqlMostrar;

-- =====================================================
-- PASO 3: Vincular por WBS exacto
-- =====================================================
PRINT '';
PRINT '=== PASO 3: VINCULANDO POR WBS EXACTO ===';

-- Detectar tipos de datos
DECLARE @tipoWBSPresup NVARCHAR(50), @tipoWBSAvance NVARCHAR(50);

SELECT @tipoWBSPresup = DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'WBS_Correcto';

SELECT @tipoWBSAvance = DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = @colWBSAvance;

PRINT 'Tipo WBS en PresupuestoObra: ' + ISNULL(@tipoWBSPresup, 'NULL');
PRINT 'Tipo WBS en AvanceManualObra: ' + ISNULL(@tipoWBSAvance, 'NULL');

DECLARE @sqlVincular NVARCHAR(MAX);

-- Construir UPDATE según tipos de datos
IF @tipoWBSPresup LIKE '%varchar%' AND @tipoWBSAvance LIKE '%varchar%'
BEGIN
    SET @sqlVincular = N'
        UPDATE amo
        SET amo.IdPresupuestoObra = p.IdPresupuestoObra
        FROM AvanceManualObra amo
        INNER JOIN PresupuestoObra p ON amo.' + @colWBSAvance + N' = p.WBS_Correcto
        WHERE amo.IdPresupuestoObra IS NULL
        AND p.IdPresupuestoObra IS NOT NULL
        AND amo.' + @colWBSAvance + N' IS NOT NULL';
END
ELSE IF @tipoWBSPresup LIKE '%varchar%' AND @tipoWBSAvance LIKE '%int%'
BEGIN
    SET @sqlVincular = N'
        UPDATE amo
        SET amo.IdPresupuestoObra = p.IdPresupuestoObra
        FROM AvanceManualObra amo
        INNER JOIN PresupuestoObra p ON CAST(amo.' + @colWBSAvance + N' AS NVARCHAR(50)) = p.WBS_Correcto
        WHERE amo.IdPresupuestoObra IS NULL
        AND p.IdPresupuestoObra IS NOT NULL
        AND amo.' + @colWBSAvance + N' IS NOT NULL';
END
ELSE IF @tipoWBSPresup LIKE '%int%' AND @tipoWBSAvance LIKE '%varchar%'
BEGIN
    SET @sqlVincular = N'
        UPDATE amo
        SET amo.IdPresupuestoObra = p.IdPresupuestoObra
        FROM AvanceManualObra amo
        INNER JOIN PresupuestoObra p ON TRY_CAST(amo.' + @colWBSAvance + N' AS INT) = p.WBS_Correcto
        WHERE amo.IdPresupuestoObra IS NULL
        AND p.IdPresupuestoObra IS NOT NULL
        AND amo.' + @colWBSAvance + N' IS NOT NULL
        AND ISNUMERIC(amo.' + @colWBSAvance + N') = 1';
END
ELSE
BEGIN
    -- Ambos INT
    SET @sqlVincular = N'
        UPDATE amo
        SET amo.IdPresupuestoObra = p.IdPresupuestoObra
        FROM AvanceManualObra amo
        INNER JOIN PresupuestoObra p ON amo.' + @colWBSAvance + N' = p.WBS_Correcto
        WHERE amo.IdPresupuestoObra IS NULL
        AND p.IdPresupuestoObra IS NOT NULL
        AND amo.' + @colWBSAvance + N' IS NOT NULL';
END

PRINT '';
PRINT 'Ejecutando vinculación...';

BEGIN TRY
    EXEC sp_executesql @sqlVincular;
    DECLARE @vinculados INT = @@ROWCOUNT;
    PRINT '? Registros vinculados por WBS exacto: ' + CAST(@vinculados AS NVARCHAR(10));
END TRY
BEGIN CATCH
    PRINT '? Error durante vinculación: ' + ERROR_MESSAGE();
END CATCH

-- =====================================================
-- PASO 4: Verificar avances huérfanos restantes
-- =====================================================
PRINT '';
PRINT '=== VERIFICACIÓN POST-VINCULACIÓN ===';

DECLARE @restantes INT, @restantesConProgreso INT;

SELECT @restantes = COUNT(*) FROM AvanceManualObra WHERE IdPresupuestoObra IS NULL;

DECLARE @sqlRestantes NVARCHAR(MAX) = N'
    SELECT @cnt = COUNT(*) 
    FROM AvanceManualObra 
    WHERE IdPresupuestoObra IS NULL 
    AND ' + @colWBSAvance + N' IS NOT NULL
    AND (ISNULL(AvancePorcentaje, 0) > 0 OR ISNULL(MontoEjecutado, 0) > 0)';

EXEC sp_executesql @sqlRestantes, N'@cnt INT OUTPUT', @cnt = @restantesConProgreso OUTPUT;

PRINT 'Registros sin vincular restantes: ' + CAST(@restantes AS NVARCHAR(10));
PRINT 'Sin vincular CON PROGRESO (críticos): ' + CAST(@restantesConProgreso AS NVARCHAR(10));

IF @restantesConProgreso > 0
BEGIN
    PRINT '';
    PRINT '?? ADVERTENCIA: Hay avances con progreso que no se pudieron vincular.';
    PRINT '   Esto puede ocurrir si el WBS en AvanceManualObra no coincide';
    PRINT '   con ningún WBS_Correcto en PresupuestoObra.';
    PRINT '';
    PRINT '=== AVANCES HUÉRFANOS CON PROGRESO ===';
    
    DECLARE @sqlHuerfanos NVARCHAR(MAX) = N'
        SELECT 
            amo.Manzana,
            amo.Lote,
            amo.' + @colWBSAvance + N' AS WBS_Huerfano,
            amo.AvancePorcentaje,
            ISNULL(amo.MontoEjecutado, 0) AS MontoEjecutado,
            ''Buscar partida manualmente'' AS Accion
        FROM AvanceManualObra amo
        WHERE amo.IdPresupuestoObra IS NULL 
        AND amo.' + @colWBSAvance + N' IS NOT NULL
        AND (ISNULL(amo.AvancePorcentaje, 0) > 0 OR ISNULL(amo.MontoEjecutado, 0) > 0)
        ORDER BY amo.Manzana, amo.Lote, amo.' + @colWBSAvance;
    
    EXEC sp_executesql @sqlHuerfanos;
    
    PRINT '';
    PRINT 'Para vincular manualmente, usa:';
    PRINT 'UPDATE AvanceManualObra SET IdPresupuestoObra = (SELECT IdPresupuestoObra FROM PresupuestoObra WHERE WBS_Correcto = X)';
    PRINT 'WHERE ' + @colWBSAvance + ' = Y AND Manzana = ''MZ'' AND Lote = ''LT'';';
END
ELSE
BEGIN
    PRINT '';
    PRINT '? ¡Excelente! Todos los avances con progreso están vinculados.';
END

-- =====================================================
-- PASO 5: Resumen final
-- =====================================================
PRINT '';
PRINT '=========================================================';
PRINT 'RESUMEN FINAL';
PRINT '=========================================================';

SELECT 
    COUNT(*) AS TotalAvances,
    SUM(CASE WHEN IdPresupuestoObra IS NOT NULL THEN 1 ELSE 0 END) AS Vinculados,
    SUM(CASE WHEN IdPresupuestoObra IS NULL THEN 1 ELSE 0 END) AS SinVincular,
    CAST(
        CAST(SUM(CASE WHEN IdPresupuestoObra IS NOT NULL THEN 1 ELSE 0 END) AS FLOAT) / 
        NULLIF(COUNT(*), 0) * 100 
    AS DECIMAL(5,2)) AS PorcentajeVinculado
FROM AvanceManualObra;

PRINT '';
PRINT '=========================================================';
PRINT 'PROCESO COMPLETADO';
PRINT '';
PRINT 'IMPORTANTE:';
PRINT '- El IdPresupuestoObra es PERMANENTE y no cambia';
PRINT '- El WBS_Correcto puede cambiar al agregar partidas';
PRINT '- La relación se mantiene siempre por IdPresupuestoObra';
PRINT '=========================================================';
