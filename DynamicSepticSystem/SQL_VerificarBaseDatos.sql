-- =============================================
-- Script de Verificación: Nombres de Base de Datos
-- Usar PRIMERO antes de ejecutar SQL_CrearTablaEvidencias.sql
-- =============================================

-- Mostrar todas las bases de datos disponibles
PRINT '?? Bases de datos disponibles en el servidor:'
SELECT 
    name AS NombreBaseDatos,
    database_id AS ID,
    create_date AS FechaCreacion,
    state_desc AS Estado
FROM sys.databases 
WHERE database_id > 4 -- Excluir bases del sistema
ORDER BY name
GO

-- Verificar estructura de InventarioCasas
PRINT ''
PRINT '?? Verificando tabla InventarioCasas...'
GO

-- REEMPLAZA 'TuBaseDeDatos' con el nombre real que aparece arriba
USE [master] -- Cambia 'master' por tu base de datos real
GO

-- Verificar si existe la tabla
IF OBJECT_ID('InventarioCasas', 'U') IS NOT NULL
BEGIN
    PRINT '? Tabla InventarioCasas existe'
    
    -- Mostrar columnas
    SELECT 
        COLUMN_NAME AS Columna,
        DATA_TYPE AS Tipo,
        IS_NULLABLE AS Nulable
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'InventarioCasas'
    ORDER BY ORDINAL_POSITION
    
    -- Verificar clave primaria
    IF EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
        WHERE TABLE_NAME = 'InventarioCasas' 
        AND CONSTRAINT_TYPE = 'PRIMARY KEY'
    )
    BEGIN
        PRINT '? InventarioCasas tiene clave primaria'
        
        SELECT 
            COLUMN_NAME AS ColumnaPK
        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
        WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1
        AND TABLE_NAME = 'InventarioCasas'
    END
    ELSE
    BEGIN
        PRINT '?? InventarioCasas NO tiene clave primaria (se creará automáticamente)'
    END
    
    -- Verificar duplicados
    SELECT 
        Manzana, 
        Lote, 
        COUNT(*) AS Total
    FROM InventarioCasas
    GROUP BY Manzana, Lote
    HAVING COUNT(*) > 1
    
    IF @@ROWCOUNT > 0
        PRINT '?? Hay registros duplicados que serán eliminados'
    ELSE
        PRINT '? No hay duplicados'
END
ELSE
BEGIN
    PRINT '? Tabla InventarioCasas NO existe'
END
GO
