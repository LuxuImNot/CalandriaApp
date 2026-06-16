-- =====================================================
-- Script: Agregar Columnas de Limite m2 por Prototipo
-- Tabla: PresupuestoObra
-- Proposito: Permitir limites de m2 diferentes para cada prototipo
-- =====================================================

USE [NombreBaseDatos] -- Cambiar por el nombre real de tu base de datos
GO

PRINT '=============================================='
PRINT '  AGREGAR COLUMNAS LIMITEM2 POR PROTOTIPO'
PRINT '=============================================='
PRINT ''
GO

-- ============================================
-- 1. Agregar columna LimiteM2Tunera
-- ============================================
PRINT '1. Verificando columna LimiteM2Tunera...'

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'LimiteM2Tunera'
)
BEGIN
    PRINT '   -> Agregando columna LimiteM2Tunera...'
    
    ALTER TABLE PresupuestoObra
    ADD LimiteM2Tunera FLOAT NULL DEFAULT 0
    
    PRINT '   ' + NCHAR(0x2705) + ' Columna LimiteM2Tunera agregada exitosamente'
END
ELSE
BEGIN
    PRINT '   -> Columna LimiteM2Tunera ya existe, omitiendo...'
END
GO

-- ============================================
-- 2. Agregar columna LimiteM2Calandra
-- ============================================
PRINT ''
PRINT '2. Verificando columna LimiteM2Calandra...'

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'LimiteM2Calandra'
)
BEGIN
    PRINT '   -> Agregando columna LimiteM2Calandra...'
    
    ALTER TABLE PresupuestoObra
    ADD LimiteM2Calandra FLOAT NULL DEFAULT 0
    
    PRINT '   ' + NCHAR(0x2705) + ' Columna LimiteM2Calandra agregada exitosamente'
END
ELSE
BEGIN
    PRINT '   -> Columna LimiteM2Calandra ya existe, omitiendo...'
END
GO

-- ============================================
-- 3. Migrar datos de LimiteM2 (legacy) a las nuevas columnas
-- ============================================
PRINT ''
PRINT '3. Migrando datos de LimiteM2 (legacy) a nuevas columnas...'

IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'LimiteM2'
)
AND EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'LimiteM2Tunera'
)
AND EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'LimiteM2Calandra'
)
BEGIN
    -- Usar SQL dinamico para la migracion
    EXEC sp_executesql N'
        UPDATE PresupuestoObra
        SET LimiteM2Tunera = ISNULL(LimiteM2, 0),
            LimiteM2Calandra = ISNULL(LimiteM2, 0)
        WHERE (LimiteM2Tunera = 0 OR LimiteM2Tunera IS NULL)
          AND (LimiteM2Calandra = 0 OR LimiteM2Calandra IS NULL)
          AND ISNULL(LimiteM2, 0) > 0'
    
    PRINT '   ' + NCHAR(0x2705) + ' Datos migrados desde LimiteM2 a las nuevas columnas'
END
ELSE
BEGIN
    PRINT '   -> No se requiere migracion (columna LimiteM2 no existe o columnas nuevas no disponibles)'
END
GO

-- ============================================
-- 4. Verificacion Final
-- ============================================
PRINT ''
PRINT '=============================================='
PRINT '  VERIFICACION FINAL'
PRINT '=============================================='

DECLARE @tunera BIT = 0
DECLARE @calandra BIT = 0

IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'LimiteM2Tunera'
)
SET @tunera = 1

IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'LimiteM2Calandra'
)
SET @calandra = 1

IF @tunera = 1 AND @calandra = 1
BEGIN
    PRINT ''
    PRINT NCHAR(0x2705) + ' EXITO: Ambas columnas estan presentes'
    PRINT ''
    PRINT 'Nuevas columnas disponibles:'
    PRINT '   * LimiteM2Tunera   - Limite de m2 para prototipo TUNERA'
    PRINT '   * LimiteM2Calandra - Limite de m2 para prototipo CALANDRA'
    PRINT ''
    PRINT 'Uso:'
    PRINT '   * Valor 0 = Sin limite (usa todos los m2 del proyecto)'
    PRINT '   * Valor > 0 = Limite maximo de m2 para calcular costo'
END
ELSE
BEGIN
    PRINT ''
    PRINT NCHAR(0x274C) + ' ERROR: No se pudieron agregar todas las columnas'
    IF @tunera = 0 PRINT '   X Falta: LimiteM2Tunera'
    IF @calandra = 0 PRINT '   X Falta: LimiteM2Calandra'
END
GO

-- ============================================
-- 5. Mostrar estructura actualizada
-- ============================================
PRINT ''
PRINT '=============================================='
PRINT '  ESTRUCTURA DE COLUMNAS LIMITEM2'
PRINT '=============================================='

SELECT 
    COLUMN_NAME AS [Columna],
    DATA_TYPE AS [Tipo],
    IS_NULLABLE AS [Acepta NULL],
    COLUMN_DEFAULT AS [Valor Por Defecto]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PresupuestoObra'
AND COLUMN_NAME LIKE 'LimiteM2%'
ORDER BY COLUMN_NAME
GO

-- ============================================
-- 6. Estadisticas de partidas con limites
-- ============================================
PRINT ''
PRINT '=============================================='
PRINT '  ESTADISTICAS DE PARTIDAS CON LIMITES'
PRINT '=============================================='

IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'LimiteM2Tunera'
)
BEGIN
    EXEC sp_executesql N'
        SELECT 
            ''Partidas con limite Tunera'' AS [Tipo],
            COUNT(*) AS [Cantidad]
        FROM PresupuestoObra
        WHERE ISNULL(LimiteM2Tunera, 0) > 0
        
        UNION ALL
        
        SELECT 
            ''Partidas con limite Calandra'' AS [Tipo],
            COUNT(*) AS [Cantidad]
        FROM PresupuestoObra
        WHERE ISNULL(LimiteM2Calandra, 0) > 0
        
        UNION ALL
        
        SELECT 
            ''Partidas con limites diferentes'' AS [Tipo],
            COUNT(*) AS [Cantidad]
        FROM PresupuestoObra
        WHERE ISNULL(LimiteM2Tunera, 0) > 0 
          AND ISNULL(LimiteM2Calandra, 0) > 0
          AND LimiteM2Tunera <> LimiteM2Calandra'
END
GO

PRINT ''
PRINT '=============================================='
PRINT '  SCRIPT COMPLETADO'
PRINT '=============================================='
PRINT ''
PRINT 'NOTAS IMPORTANTES:'
PRINT '   1. La columna LimiteM2 (legacy) se mantiene por compatibilidad'
PRINT '   2. Las nuevas partidas usaran LimiteM2Tunera y LimiteM2Calandra'
PRINT '   3. Si solo un limite esta definido, ese se usara para ambos prototipos'
PRINT '   4. Si ambos son 0, no hay limite (comportamiento original)'
PRINT ''
GO
