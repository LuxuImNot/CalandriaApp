-- =====================================================
-- Script: Agregar Columna para Partidas por Prototipo
-- Tabla: PresupuestoObra
-- Proposito: Permitir partidas unicas para cada prototipo
-- =====================================================

USE [NombreBaseDatos] -- Cambiar por el nombre real de tu base de datos
GO

-- Verificar si la columna ya existe antes de agregarla
PRINT NCHAR(0x1F50D) + ' Verificando existencia de columna PrototiposAplicables...'
GO

-- ============================================
-- 1. Agregar columna PrototiposAplicables
-- ============================================
-- Esta columna almacena una lista separada por comas de los prototipos
-- a los que aplica la partida. Ejemplos:
-- 'TUNERA,CALANDRA' = Aplica a ambos (comportamiento por defecto)
-- 'TUNERA' = Solo aplica a Tunera
-- 'CALANDRA' = Solo aplica a Calandra
-- NULL o vacio = Aplica a todos (comportamiento legacy)

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'PrototiposAplicables'
)
BEGIN
    PRINT NCHAR(0x2795) + ' Agregando columna PrototiposAplicables...'
    
    ALTER TABLE PresupuestoObra
    ADD PrototiposAplicables NVARCHAR(255) NULL
    
    PRINT '   ' + NCHAR(0x2705) + ' Columna PrototiposAplicables agregada exitosamente'
END
ELSE
BEGIN
    PRINT NCHAR(0x26A0) + NCHAR(0xFE0F) + ' Columna PrototiposAplicables ya existe, omitiendo...'
END
GO

-- ============================================
-- 2. Actualizar partidas existentes
-- ============================================
PRINT NCHAR(0x1F504) + ' Actualizando partidas existentes con valor por defecto...'


IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'PrototiposAplicables'
)
BEGIN
    -- Usar SQL dinamico para evitar error de validacion
    EXEC sp_executesql N'
        UPDATE PresupuestoObra
        SET PrototiposAplicables = ''TUNERA,CALANDRA''
        WHERE PrototiposAplicables IS NULL'
    
    PRINT '   ' + NCHAR(0x2705) + ' Partidas existentes actualizadas (aplican a todos los prototipos)'
END
GO

-- ============================================
-- 3. Verificacion Final
-- ============================================
PRINT ''
PRINT NCHAR(0x1F4CA) + ' VERIFICACION FINAL:'
PRINT '===================='

IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'PrototiposAplicables'
)
BEGIN
    PRINT NCHAR(0x2705) + ' Columna PrototiposAplicables esta presente'
    PRINT ''
    PRINT NCHAR(0x1F4DD) + ' Formato de la columna:'
    PRINT '   ' + NCHAR(0x2022) + ' NULL o "TUNERA,CALANDRA" = Aplica a todos los prototipos'
    PRINT '   ' + NCHAR(0x2022) + ' "TUNERA" = Solo aplica al prototipo Tunera'
    PRINT '   ' + NCHAR(0x2022) + ' "CALANDRA" = Solo aplica al prototipo Calandra'
    PRINT '   ' + NCHAR(0x2022) + ' Puedes agregar mas prototipos separados por coma'
END
ELSE
BEGIN
    PRINT NCHAR(0x274C) + ' ERROR: No se pudo agregar la columna'
    PRINT '   Verifica los mensajes anteriores para mas detalles'
END
GO

-- ============================================
-- 4. Mostrar estructura actualizada
-- ============================================
PRINT ''
PRINT NCHAR(0x1F4CB) + ' ESTRUCTURA ACTUALIZADA DE PRESUPUESTOOBRA:'
PRINT '============================================='

SELECT 
    COLUMN_NAME AS [Columna],
    DATA_TYPE AS [Tipo],
    CHARACTER_MAXIMUM_LENGTH AS [Longitud],
    IS_NULLABLE AS [Acepta NULL],
    COLUMN_DEFAULT AS [Valor Por Defecto]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PresupuestoObra'
AND COLUMN_NAME IN ('PrototiposAplicables', 'EsDinamica', 'ValorM2Tunera', 'ValorM2Calandra', 'CostoTunera', 'CostoCalandra')
ORDER BY ORDINAL_POSITION
GO

-- ============================================
-- 5. Estadisticas de partidas por prototipo
-- ============================================
PRINT ''
PRINT NCHAR(0x1F4CA) + ' ESTADISTICAS DE PARTIDAS POR PROTOTIPO:'
PRINT '==========================================='

-- Usar SQL dinamico para la consulta de estadisticas
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'PrototiposAplicables'
)
BEGIN
    EXEC sp_executesql N'
        SELECT 
            CASE 
                WHEN PrototiposAplicables IS NULL OR PrototiposAplicables = '''' THEN ''Sin especificar (todos)''
                ELSE PrototiposAplicables
            END AS [Prototipos],
            COUNT(*) AS [Cantidad de Partidas]
        FROM PresupuestoObra
        GROUP BY PrototiposAplicables
        ORDER BY COUNT(*) DESC'
END
ELSE
BEGIN
    PRINT NCHAR(0x26A0) + NCHAR(0xFE0F) + ' La columna PrototiposAplicables no existe, no se pueden mostrar estadisticas'
END
GO

PRINT ''
PRINT NCHAR(0x2705) + ' Script completado exitosamente'
PRINT ''
PRINT NCHAR(0x1F4CB) + ' NOTAS IMPORTANTES:'
PRINT '   1. Las partidas existentes ahora aplican a todos los prototipos por defecto'
PRINT '   2. Para hacer una partida exclusiva de un prototipo:'
PRINT '      - Editala desde FormGestionarPartidas'
PRINT '      - O ejecuta: UPDATE PresupuestoObra SET PrototiposAplicables = ''TUNERA'' WHERE WBS_Correcto = X'
PRINT '   3. Los prototipos se especifican en mayusculas separados por coma'
PRINT ''
GO
