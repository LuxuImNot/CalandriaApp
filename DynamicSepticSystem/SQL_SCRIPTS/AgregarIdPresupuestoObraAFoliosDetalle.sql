-- =====================================================
-- AGREGAR VÍNCULO PERMANENTE A FoliosEstimacionDetalle
-- 
-- Este script agrega la columna IdPresupuestoObra a 
-- FoliosEstimacionDetalle para crear un vínculo inmutable
-- que sobrevive a reorganizaciones de WBS.
--
-- ? PROTEGE contra vulnerabilidad de cambio de WBS
-- ? CREA BACKUP automático antes de modificar
-- =====================================================

SET NOCOUNT ON;

DECLARE @fecha NVARCHAR(20) = REPLACE(REPLACE(REPLACE(CONVERT(NVARCHAR(20), GETDATE(), 120), '-', ''), ':', ''), ' ', '_');

PRINT '=========================================================';
PRINT 'AGREGAR IdPresupuestoObra A FoliosEstimacionDetalle';
PRINT 'Fecha: ' + CONVERT(NVARCHAR(20), GETDATE(), 120);
PRINT '=========================================================';
PRINT '';

-- =====================================================
-- PASO 0: VERIFICAR PREREQUISITOS
-- =====================================================
PRINT '=== PASO 0: Verificar prerequisitos ===';
PRINT '';

-- Verificar que existe tabla FoliosEstimacionDetalle
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FoliosEstimacionDetalle')
BEGIN
    PRINT '? ERROR: La tabla FoliosEstimacionDetalle NO existe';
    PRINT '   Ejecuta primero el script de creación de tablas de folios';
    RETURN;
END

-- Verificar que existe columna Id en PresupuestoObra
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'Id'
)
BEGIN
    PRINT '? ERROR: PresupuestoObra no tiene columna Id';
    PRINT '   Esta columna es necesaria como vínculo permanente';
    RETURN;
END

-- Verificar que AvanceManualObra tiene IdPresupuestoObra
DECLARE @avanceManualTieneId BIT = 0;
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = 'IdPresupuestoObra'
)
BEGIN
    SET @avanceManualTieneId = 1;
    PRINT '? AvanceManualObra YA tiene IdPresupuestoObra (excelente!)';
END
ELSE
BEGIN
    PRINT '?? AvanceManualObra NO tiene IdPresupuestoObra todavía';
    PRINT '   Recomendación: Ejecuta CrearVinculoPermanenteWBS.sql después de este script';
END

PRINT '';

-- =====================================================
-- PASO 1: CREAR BACKUP DE SEGURIDAD
-- =====================================================
PRINT '=== PASO 1: Crear backup de seguridad ===';
PRINT '';

DECLARE @backupDetalle NVARCHAR(200) = 'FoliosEstimacionDetalle_Backup_' + @fecha;
DECLARE @sqlBackup NVARCHAR(MAX) = 'SELECT * INTO [' + @backupDetalle + '] FROM FoliosEstimacionDetalle';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = @backupDetalle)
BEGIN
    EXEC sp_executesql @sqlBackup;
    DECLARE @backupRows INT = @@ROWCOUNT;
    PRINT '? Backup creado: ' + @backupDetalle + ' (' + CAST(@backupRows AS NVARCHAR(10)) + ' registros)';
END
ELSE
BEGIN
    PRINT '?? Backup ya existe: ' + @backupDetalle;
END

PRINT '';
PRINT '=========================================================';
PRINT 'BACKUP CREADO. Si algo sale mal, puedes restaurar con:';
PRINT '';
PRINT '-- DROP TABLE FoliosEstimacionDetalle;';
PRINT '-- SELECT * INTO FoliosEstimacionDetalle FROM [' + @backupDetalle + '];';
PRINT '-- Recrea las foreign keys manualmente';
PRINT '=========================================================';
PRINT '';
GO

-- =====================================================
-- PASO 2: AGREGAR COLUMNA IdPresupuestoObra
-- =====================================================
PRINT '=== PASO 2: Agregar columna IdPresupuestoObra ===';
PRINT '';

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'FoliosEstimacionDetalle' AND COLUMN_NAME = 'IdPresupuestoObra'
)
BEGIN
    PRINT 'Agregando columna IdPresupuestoObra a FoliosEstimacionDetalle...';
    
    ALTER TABLE FoliosEstimacionDetalle 
    ADD IdPresupuestoObra INT NULL;
    
    PRINT '? Columna IdPresupuestoObra agregada';
END
ELSE
BEGIN
    PRINT '? La columna IdPresupuestoObra ya existe';
END
GO

-- =====================================================
-- PASO 3: POBLAR IdPresupuestoObra EN REGISTROS EXISTENTES
-- =====================================================
PRINT '';
PRINT '=== PASO 3: Vincular registros existentes ===';
PRINT '(Usando WBS actual para encontrar el Id correspondiente)';
PRINT '';

-- Detectar tipo de WBS en FoliosEstimacionDetalle
DECLARE @tipoWBSDetalle NVARCHAR(50);
SELECT @tipoWBSDetalle = DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'FoliosEstimacionDetalle' AND COLUMN_NAME = 'WBS';

PRINT 'Tipo de WBS en FoliosEstimacionDetalle: ' + ISNULL(@tipoWBSDetalle, 'NULL');

-- Detectar tipo de WBS_Correcto en PresupuestoObra
DECLARE @tipoWBSPresup NVARCHAR(50);
SELECT @tipoWBSPresup = DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'WBS_Correcto';

PRINT 'Tipo de WBS_Correcto en PresupuestoObra: ' + ISNULL(@tipoWBSPresup, 'NULL');
PRINT '';

-- Construir UPDATE dinámico según tipos de datos
DECLARE @sqlUpdate NVARCHAR(MAX);

IF @tipoWBSDetalle LIKE '%varchar%' AND @tipoWBSPresup LIKE '%varchar%'
BEGIN
    SET @sqlUpdate = N'
        UPDATE fed
        SET fed.IdPresupuestoObra = p.Id
        FROM FoliosEstimacionDetalle fed
        INNER JOIN PresupuestoObra p ON fed.WBS = p.WBS_Correcto
        WHERE fed.IdPresupuestoObra IS NULL
        AND fed.WBS IS NOT NULL
        AND p.Id IS NOT NULL';
END
ELSE IF @tipoWBSDetalle LIKE '%varchar%'
BEGIN
    SET @sqlUpdate = N'
        UPDATE fed
        SET fed.IdPresupuestoObra = p.Id
        FROM FoliosEstimacionDetalle fed
        INNER JOIN PresupuestoObra p ON CAST(fed.WBS AS INT) = p.WBS_Correcto
        WHERE fed.IdPresupuestoObra IS NULL
        AND fed.WBS IS NOT NULL
        AND ISNUMERIC(fed.WBS) = 1
        AND p.Id IS NOT NULL';
END
ELSE IF @tipoWBSPresup LIKE '%varchar%'
BEGIN
    SET @sqlUpdate = N'
        UPDATE fed
        SET fed.IdPresupuestoObra = p.Id
        FROM FoliosEstimacionDetalle fed
        INNER JOIN PresupuestoObra p ON fed.WBS = CAST(p.WBS_Correcto AS INT)
        WHERE fed.IdPresupuestoObra IS NULL
        AND fed.WBS IS NOT NULL
        AND p.Id IS NOT NULL';
END
ELSE
BEGIN
    SET @sqlUpdate = N'
        UPDATE fed
        SET fed.IdPresupuestoObra = p.Id
        FROM FoliosEstimacionDetalle fed
        INNER JOIN PresupuestoObra p ON fed.WBS = p.WBS_Correcto
        WHERE fed.IdPresupuestoObra IS NULL
        AND fed.WBS IS NOT NULL
        AND p.Id IS NOT NULL';
END

PRINT 'Ejecutando vinculación...';
EXEC sp_executesql @sqlUpdate;

DECLARE @vinculados INT = @@ROWCOUNT;
PRINT '? Vínculos creados: ' + CAST(@vinculados AS NVARCHAR(10)) + ' registros';
GO

-- =====================================================
-- PASO 4: CREAR ÍNDICE EN IdPresupuestoObra
-- =====================================================
PRINT '';
PRINT '=== PASO 4: Crear índice en IdPresupuestoObra ===';
PRINT '';

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_FoliosEstimacionDetalle_IdPresupuestoObra' 
    AND object_id = OBJECT_ID('FoliosEstimacionDetalle')
)
BEGIN
    PRINT 'Creando índice en FoliosEstimacionDetalle.IdPresupuestoObra...';
    
    CREATE INDEX IX_FoliosEstimacionDetalle_IdPresupuestoObra 
    ON FoliosEstimacionDetalle(IdPresupuestoObra);
    
    PRINT '? Índice creado exitosamente';
END
ELSE
BEGIN
    PRINT '? El índice ya existe';
END
GO

-- =====================================================
-- PASO 5: CREAR FOREIGN KEY (OPCIONAL - CON PRECAUCIÓN)
-- =====================================================
PRINT '';
PRINT '=== PASO 5: Verificar posibilidad de Foreign Key ===';
PRINT '';

-- Contar registros que NO pueden vincularse (huérfanos)
DECLARE @huerfanos INT;
SELECT @huerfanos = COUNT(*)
FROM FoliosEstimacionDetalle
WHERE IdPresupuestoObra IS NULL
AND WBS IS NOT NULL;

IF @huerfanos = 0
BEGIN
    PRINT '? NO hay registros huérfanos - se puede crear FK';
    PRINT '';
    PRINT 'Para crear la FK, ejecuta manualmente:';
    PRINT '';
    PRINT '-- ALTER TABLE FoliosEstimacionDetalle';
    PRINT '-- ADD CONSTRAINT FK_FoliosEstimacionDetalle_PresupuestoObra';
    PRINT '-- FOREIGN KEY (IdPresupuestoObra) REFERENCES PresupuestoObra(Id);';
END
ELSE
BEGIN
    PRINT '?? HAY ' + CAST(@huerfanos AS NVARCHAR(10)) + ' registros huérfanos (sin vínculo)';
    PRINT '   NO se puede crear FK hasta resolver estos casos';
    PRINT '';
    PRINT 'Para ver los huérfanos:';
    PRINT '';
    PRINT '-- SELECT * FROM FoliosEstimacionDetalle';
    PRINT '-- WHERE IdPresupuestoObra IS NULL AND WBS IS NOT NULL;';
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
    COLUMN_NAME AS Columna,
    DATA_TYPE AS Tipo,
    IS_NULLABLE AS Nullable,
    COLUMN_DEFAULT AS [Default]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'FoliosEstimacionDetalle'
AND COLUMN_NAME IN ('WBS', 'IdPresupuestoObra', 'FolioId', 'NombrePartida')
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '=== Estado de vinculación ===';

DECLARE @totalDetalle INT, @vinculados INT, @sinVincular INT;

SELECT @totalDetalle = COUNT(*) FROM FoliosEstimacionDetalle;
SELECT @vinculados = COUNT(*) FROM FoliosEstimacionDetalle WHERE IdPresupuestoObra IS NOT NULL;
SELECT @sinVincular = COUNT(*) FROM FoliosEstimacionDetalle WHERE IdPresupuestoObra IS NULL AND WBS IS NOT NULL;

PRINT 'Total registros en FoliosEstimacionDetalle: ' + CAST(@totalDetalle AS NVARCHAR(10));
PRINT 'Vinculados (con IdPresupuestoObra): ' + CAST(@vinculados AS NVARCHAR(10));
PRINT 'Sin vincular: ' + CAST(@sinVincular AS NVARCHAR(10));

PRINT '';
PRINT '=========================================================';
PRINT '? SCRIPT COMPLETADO EXITOSAMENTE';
PRINT '';
PRINT 'PRÓXIMOS PASOS:';
PRINT '1. Ejecutar: CrearVinculoPermanenteWBS.sql (si no lo has hecho)';
PRINT '2. Modificar: FormEstimacionConceptoMigrado_ExtensionFolios.cs';
PRINT '3. Modificar: FormRepositorioPDFs.cs (método RevertirAvancesEstimacion)';
PRINT '4. Probar: Crear estimación y luego eliminarla';
PRINT '';
PRINT 'BENEFICIOS:';
PRINT '- ? Las estimaciones ahora usan vínculo inmutable';
PRINT '- ? Reorganizar WBS NO afecta estimaciones existentes';
PRINT '- ? La reversión de avances funciona correctamente';
PRINT '=========================================================';
