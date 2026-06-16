-- =====================================================
-- Script: Agregar Columnas para Partidas Dinámicas
-- Tabla: PresupuestoObra
-- Propósito: Permitir partidas con costo dinámico por m²
-- =====================================================

USE [NombreBaseDatos] -- Cambiar por el nombre real de tu base de datos
GO

-- Verificar si las columnas ya existen antes de agregarlas
PRINT '?? Verificando existencia de columnas dinámicas...'
GO

-- ============================================
-- 1. Agregar columna EsDinamica
-- ============================================
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'EsDinamica'
)
BEGIN
    PRINT '? Agregando columna EsDinamica...'
    
    ALTER TABLE PresupuestoObra
    ADD EsDinamica BIT NOT NULL DEFAULT 0
    
    PRINT '   ? Columna EsDinamica agregada exitosamente'
END
ELSE
BEGIN
    PRINT '?? Columna EsDinamica ya existe, omitiendo...'
END
GO

-- ============================================
-- 2. Agregar columna ValorM2Tunera
-- ============================================
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'ValorM2Tunera'
)
BEGIN
    PRINT '? Agregando columna ValorM2Tunera...'
    
    ALTER TABLE PresupuestoObra
    ADD ValorM2Tunera FLOAT NOT NULL DEFAULT 0
    
    PRINT '   ? Columna ValorM2Tunera agregada exitosamente'
END
ELSE
BEGIN
    PRINT '?? Columna ValorM2Tunera ya existe, omitiendo...'
END
GO

-- ============================================
-- 3. Agregar columna ValorM2Calandra
-- ============================================
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'ValorM2Calandra'
)
BEGIN
    PRINT '? Agregando columna ValorM2Calandra...'
    
    ALTER TABLE PresupuestoObra
    ADD ValorM2Calandra FLOAT NOT NULL DEFAULT 0
    
    PRINT '   ? Columna ValorM2Calandra agregada exitosamente'
END
ELSE
BEGIN
    PRINT '?? Columna ValorM2Calandra ya existe, omitiendo...'
END
GO

-- ============================================
-- 4. NUEVO: Agregar columna MetrosCuadrados
-- ============================================
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'MetrosCuadrados'
)
BEGIN
    PRINT '? Agregando columna MetrosCuadrados...'
    
    ALTER TABLE PresupuestoObra
    ADD MetrosCuadrados FLOAT NOT NULL DEFAULT 0
    
    PRINT '   ? Columna MetrosCuadrados agregada exitosamente'
END
ELSE
BEGIN
    PRINT '?? Columna MetrosCuadrados ya existe, omitiendo...'
END
GO

-- ============================================
-- 5. Verificación Final
-- ============================================
PRINT ''
PRINT '?? VERIFICACIÓN FINAL:'
PRINT '===================='

IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME IN ('EsDinamica', 'ValorM2Tunera', 'ValorM2Calandra', 'MetrosCuadrados')
    GROUP BY TABLE_NAME
    HAVING COUNT(*) = 4
)
BEGIN
    PRINT '? Todas las columnas dinámicas están presentes'
    PRINT ''
    PRINT '?? Columnas agregadas:'
    PRINT '   • EsDinamica (BIT) - Indica si la partida usa cálculo dinámico'
    PRINT '   • ValorM2Tunera (FLOAT) - Valor por m² para prototipo Tunera'
    PRINT '   • ValorM2Calandra (FLOAT) - Valor por m² para prototipo Calandra'
    PRINT '   • MetrosCuadrados (FLOAT) - Metros cuadrados específicos de la partida'
END
ELSE
BEGIN
    PRINT '? ERROR: No se pudieron agregar todas las columnas'
    PRINT '   Verifica los mensajes anteriores para más detalles'
END
GO

-- ============================================
-- 6. Mostrar estructura final
-- ============================================
PRINT ''
PRINT '?? ESTRUCTURA ACTUAL DE PRESUPUESTOOBRA:'
PRINT '========================================'

SELECT 
    COLUMN_NAME AS [Columna],
    DATA_TYPE AS [Tipo],
    IS_NULLABLE AS [Acepta NULL],
    COLUMN_DEFAULT AS [Valor Por Defecto]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PresupuestoObra'
AND COLUMN_NAME IN ('EsDinamica', 'ValorM2Tunera', 'ValorM2Calandra', 'MetrosCuadrados')
ORDER BY ORDINAL_POSITION
GO

PRINT ''
PRINT '? Script completado exitosamente'
PRINT ''
PRINT '?? NOTAS IMPORTANTES:'
PRINT '   1. Las partidas existentes tienen EsDinamica = 0 (costo fijo)'
PRINT '   2. Para hacer una partida dinámica, edítala desde FormGestionarPartidas'
PRINT '   3. El cálculo dinámico usa MetrosCuadrados ingresados por el usuario'
PRINT '   4. Cada partida dinámica puede tener sus propios m² personalizados'
PRINT ''
GO
