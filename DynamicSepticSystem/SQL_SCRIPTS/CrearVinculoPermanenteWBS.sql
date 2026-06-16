-- =====================================================
-- CREAR VÍNCULO PERMANENTE ENTRE PresupuestoObra Y AvanceManualObra
-- 
-- Este script usa la columna Id existente en PresupuestoObra como
-- identificador único e inmutable, independiente del WBS.
-- Así, aunque el WBS cambie, la relación nunca se rompe.
--
-- ?? ESTE SCRIPT CREA BACKUPS AUTOMÁTICOS ANTES DE CUALQUIER CAMBIO
-- ?? NO MODIFICA LA COLUMNA WBS - Solo agrega IdPresupuestoObra a AvanceManualObra
-- =====================================================

SET NOCOUNT ON;

DECLARE @fecha NVARCHAR(20) = REPLACE(REPLACE(REPLACE(CONVERT(NVARCHAR(20), GETDATE(), 120), '-', ''), ':', ''), ' ', '_');

PRINT '=========================================================';
PRINT 'CREAR VÍNCULO PERMANENTE - CON BACKUP AUTOMÁTICO';
PRINT 'Fecha: ' + CONVERT(NVARCHAR(20), GETDATE(), 120);
PRINT '=========================================================';
PRINT '';

-- =====================================================
-- PASO 0: CREAR BACKUPS DE SEGURIDAD
-- =====================================================
PRINT '=== PASO 0: CREANDO BACKUPS DE SEGURIDAD ===';
PRINT '';

-- Backup de AvanceManualObra (solo esta tabla será modificada)
DECLARE @backupAvance NVARCHAR(200) = 'AvanceManualObra_Backup_' + @fecha;
DECLARE @sqlBackupAvance NVARCHAR(MAX) = 'SELECT * INTO [' + @backupAvance + '] FROM AvanceManualObra';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = @backupAvance)
BEGIN
    EXEC sp_executesql @sqlBackupAvance;
    PRINT '? Backup creado: ' + @backupAvance + ' (' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' registros)';
END
ELSE
BEGIN
    PRINT '? Backup ya existe: ' + @backupAvance;
END

PRINT '';
PRINT '=========================================================';
PRINT 'BACKUP CREADO. Si algo sale mal, puedes restaurar con:';
PRINT '';
PRINT '-- Para restaurar AvanceManualObra:';
PRINT '-- DROP TABLE AvanceManualObra;';
PRINT '-- SELECT * INTO AvanceManualObra FROM [' + @backupAvance + '];';
PRINT '=========================================================';
PRINT '';
GO

-- =====================================================
-- PASO 1: Verificar que existe columna Id en PresupuestoObra
-- =====================================================
PRINT '=== PASO 1: Verificar columna Id en PresupuestoObra ===';

IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'Id'
)
BEGIN
    PRINT '? La columna Id existe en PresupuestoObra - se usará como vínculo permanente';
END
ELSE
BEGIN
    PRINT '? ERROR: La columna Id NO existe en PresupuestoObra';
    PRINT '   Este script requiere que PresupuestoObra tenga una columna Id';
    RETURN;
END
GO

-- =====================================================
-- PASO 2: Agregar columna IdPresupuestoObra a AvanceManualObra
-- =====================================================
PRINT '';
PRINT '=== PASO 2: Agregar columna IdPresupuestoObra a AvanceManualObra ===';

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = 'IdPresupuestoObra'
)
BEGIN
    PRINT 'Agregando columna IdPresupuestoObra a AvanceManualObra...';
    
    ALTER TABLE AvanceManualObra 
    ADD IdPresupuestoObra INT NULL;
    
    PRINT '? Columna IdPresupuestoObra agregada a AvanceManualObra';
END
ELSE
BEGIN
    PRINT '? La columna IdPresupuestoObra ya existe en AvanceManualObra';
END
GO

-- =====================================================
-- PASO 3: Vincular registros existentes por WBS actual
-- (Solo escribe en IdPresupuestoObra, NUNCA toca WBS)
-- =====================================================
PRINT '';
PRINT '=== PASO 3: Vincular registros existentes ===';
PRINT '(Solo escribe en IdPresupuestoObra, NO modifica WBS)';

-- Detectar nombre de columna WBS en AvanceManualObra
DECLARE @colWBSAvance NVARCHAR(50);
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = 'WBS_Correcto')
    SET @colWBSAvance = 'WBS_Correcto';
ELSE
    SET @colWBSAvance = 'WBS';

PRINT 'Columna WBS detectada en AvanceManualObra: ' + @colWBSAvance;

DECLARE @sql NVARCHAR(MAX);

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

-- Construir UPDATE dinámico según tipos de datos
-- IMPORTANTE: Solo actualiza IdPresupuestoObra, NUNCA toca WBS
-- Usa p.Id (la columna IDENTITY existente en PresupuestoObra)
IF @tipoWBSPresup LIKE '%varchar%' AND @tipoWBSAvance LIKE '%varchar%'
BEGIN
    SET @sql = N'
        UPDATE amo
        SET amo.IdPresupuestoObra = p.Id
        FROM AvanceManualObra amo
        INNER JOIN PresupuestoObra p ON amo.' + @colWBSAvance + N' = p.WBS_Correcto
        WHERE amo.IdPresupuestoObra IS NULL
        AND p.Id IS NOT NULL';
END
ELSE IF @tipoWBSPresup LIKE '%varchar%'
BEGIN
    SET @sql = N'
        UPDATE amo
        SET amo.IdPresupuestoObra = p.Id
        FROM AvanceManualObra amo
        INNER JOIN PresupuestoObra p ON CAST(amo.' + @colWBSAvance + N' AS NVARCHAR(50)) = p.WBS_Correcto
        WHERE amo.IdPresupuestoObra IS NULL
        AND p.Id IS NOT NULL';
END
ELSE IF @tipoWBSAvance LIKE '%varchar%'
BEGIN
    SET @sql = N'
        UPDATE amo
        SET amo.IdPresupuestoObra = p.Id
        FROM AvanceManualObra amo
        INNER JOIN PresupuestoObra p ON CAST(amo.' + @colWBSAvance + N' AS INT) = p.WBS_Correcto
        WHERE amo.IdPresupuestoObra IS NULL
        AND p.Id IS NOT NULL';
END
ELSE
BEGIN
    SET @sql = N'
        UPDATE amo
        SET amo.IdPresupuestoObra = p.Id
        FROM AvanceManualObra amo
        INNER JOIN PresupuestoObra p ON amo.' + @colWBSAvance + N' = p.WBS_Correcto
        WHERE amo.IdPresupuestoObra IS NULL
        AND p.Id IS NOT NULL';
END

PRINT '';
PRINT 'Ejecutando vinculación...';
EXEC sp_executesql @sql;

DECLARE @migratedRows INT = @@ROWCOUNT;
PRINT '? Vínculos creados: ' + CAST(@migratedRows AS NVARCHAR(10)) + ' registros';
GO

-- =====================================================
-- PASO 4: Crear índice en AvanceManualObra.IdPresupuestoObra
-- =====================================================
PRINT '';
PRINT '=== PASO 4: Crear índice en AvanceManualObra ===';

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_AvanceManualObra_IdPresupuestoObra' 
    AND object_id = OBJECT_ID('AvanceManualObra')
)
BEGIN
    PRINT 'Creando índice en AvanceManualObra.IdPresupuestoObra...';
    
    CREATE INDEX IX_AvanceManualObra_IdPresupuestoObra 
    ON AvanceManualObra(IdPresupuestoObra);
    
    PRINT '? Índice creado exitosamente';
END
ELSE
BEGIN
    PRINT '? El índice ya existe';
END
GO

-- =====================================================
-- VERIFICACIÓN FINAL
-- =====================================================
PRINT '';
PRINT '=========================================================';
PRINT 'VERIFICACIÓN FINAL';
PRINT '=========================================================';
PRINT '';

PRINT '=== Estructura de columnas ===';
SELECT 
    TABLE_NAME AS Tabla,
    COLUMN_NAME AS Columna,
    DATA_TYPE AS Tipo,
    IS_NULLABLE AS Nullable
FROM INFORMATION_SCHEMA.COLUMNS
WHERE (TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME IN ('Id', 'WBS_Correcto'))
   OR (TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME IN ('IdPresupuestoObra', 'WBS_Correcto', 'WBS'))
ORDER BY TABLE_NAME, COLUMN_NAME;

-- Verificar avances sin vincular
PRINT '';
PRINT '=== Estado de vinculación ===';

DECLARE @colWBS NVARCHAR(50);
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = 'WBS_Correcto')
    SET @colWBS = 'WBS_Correcto';
ELSE
    SET @colWBS = 'WBS';

DECLARE @totalAvances INT, @vinculados INT, @sinVincular INT, @sinVincularConProgreso INT;

SELECT @totalAvances = COUNT(*) FROM AvanceManualObra;
SELECT @vinculados = COUNT(*) FROM AvanceManualObra WHERE IdPresupuestoObra IS NOT NULL;
SELECT @sinVincular = COUNT(*) FROM AvanceManualObra WHERE IdPresupuestoObra IS NULL;

DECLARE @sqlContar NVARCHAR(MAX) = N'
    SELECT @cnt = COUNT(*) FROM AvanceManualObra 
    WHERE IdPresupuestoObra IS NULL 
    AND ' + @colWBS + N' IS NOT NULL
    AND (ISNULL(AvancePorcentaje, 0) > 0 OR ISNULL(MontoEjecutado, 0) > 0)';
EXEC sp_executesql @sqlContar, N'@cnt INT OUTPUT', @cnt = @sinVincularConProgreso OUTPUT;

PRINT 'Total registros en AvanceManualObra: ' + CAST(@totalAvances AS NVARCHAR(10));
PRINT 'Vinculados (con IdPresupuestoObra): ' + CAST(@vinculados AS NVARCHAR(10));
PRINT 'Sin vincular: ' + CAST(@sinVincular AS NVARCHAR(10));
PRINT 'Sin vincular CON PROGRESO: ' + CAST(@sinVincularConProgreso AS NVARCHAR(10));

PRINT '';
PRINT '=========================================================';
PRINT '? SCRIPT COMPLETADO EXITOSAMENTE';
PRINT '';
PRINT 'IMPORTANTE:';
PRINT '- Se creó backup automático de AvanceManualObra';
PRINT '- La columna WBS NO fue modificada';
PRINT '- Se usa PresupuestoObra.Id como vínculo permanente';
PRINT '- IdPresupuestoObra en AvanceManualObra apunta a Id en PresupuestoObra';
PRINT '- Ahora puedes agregar partidas sin perder el progreso';
PRINT '=========================================================';
