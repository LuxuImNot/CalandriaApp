-- ============================================
-- Script para verificar el esquema de la tabla PresupuestoObra
-- ============================================

USE CALANDRIA;
GO

-- 1. Verificar si existe la tabla
IF OBJECT_ID('dbo.PresupuestoObra', 'U') IS NOT NULL
BEGIN
    PRINT '? Tabla PresupuestoObra existe'
    
    -- 2. Mostrar todas las columnas de la tabla
    SELECT 
        COLUMN_NAME AS 'Columna',
        DATA_TYPE AS 'Tipo',
        CHARACTER_MAXIMUM_LENGTH AS 'Longitud',
        IS_NULLABLE AS 'Permite NULL'
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'PresupuestoObra'
    ORDER BY ORDINAL_POSITION;
    
    -- 3. Mostrar las primeras 5 filas para entender la estructura
    SELECT TOP 5 *
    FROM PresupuestoObra;
    
    -- 4. Verificar si existe alguna columna que pueda servir como identificador
    PRINT '-------------------------------------------'
    PRINT 'Buscando columnas que puedan servir como WBS:'
    PRINT '-------------------------------------------'
    
    SELECT COLUMN_NAME
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'PresupuestoObra'
    AND (
        COLUMN_NAME LIKE '%WBS%' OR
        COLUMN_NAME LIKE '%ID%' OR
        COLUMN_NAME LIKE '%Codigo%' OR
        COLUMN_NAME LIKE '%Clave%' OR
        COLUMN_NAME LIKE '%Numero%'
    );
END
ELSE
BEGIN
    PRINT '? La tabla PresupuestoObra NO existe'
    
    -- Buscar tablas similares
    PRINT 'Buscando tablas similares...'
    SELECT TABLE_NAME
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME LIKE '%Presupuesto%' OR TABLE_NAME LIKE '%Obra%'
    ORDER BY TABLE_NAME;
END
GO
