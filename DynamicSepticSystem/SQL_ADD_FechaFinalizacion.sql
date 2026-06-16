-- Script para agregar columna FechaFinalizacion a la tabla AvanceManualObra
-- Esta columna capturará la fecha cuando una partida se marca como completada (100%)

USE [CalandriaDB]
GO

-- Verificar si la columna ya existe antes de agregarla
IF NOT EXISTS (
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AvanceManualObra' 
    AND COLUMN_NAME = 'FechaFinalizacion'
)
BEGIN
    ALTER TABLE AvanceManualObra
    ADD FechaFinalizacion DATETIME NULL;
    
    PRINT 'Columna FechaFinalizacion agregada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La columna FechaFinalizacion ya existe.';
END
GO

-- Actualizar las partidas que ya están al 100% con la fecha actual
-- (solo si no tienen fecha de finalización)
UPDATE AvanceManualObra
SET FechaFinalizacion = FechaActualizacion
WHERE AvancePorcentaje >= 100 
  AND FechaFinalizacion IS NULL
  AND FechaActualizacion IS NOT NULL;

PRINT 'Se actualizaron las partidas completadas con su fecha de actualización.';
GO
