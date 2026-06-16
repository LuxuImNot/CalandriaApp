-- ============================================================================
-- Script para crear tabla de Avance de Obra Manual
-- ============================================================================

USE CALANDRIA;
GO

-- ============================================================================
-- VERIFICAR SI LA TABLA EXISTE Y ACTUALIZARLA
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.AvanceObraManual') AND type in (N'U'))
BEGIN
    PRINT '?? Tabla AvanceObraManual existe. Verificando columnas...';
    
    -- Verificar si existe columna MontoEjecutado (antigua)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AvanceObraManual') AND name = 'MontoEjecutado')
    BEGIN
        -- Renombrar MontoEjecutado a Ejecutado
        EXEC sp_rename 'dbo.AvanceObraManual.MontoEjecutado', 'Ejecutado', 'COLUMN';
        PRINT '? Columna MontoEjecutado renombrada a Ejecutado';
    END
    
    -- Agregar columna Ejecutado si no existe
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AvanceObraManual') AND name = 'Ejecutado')
    BEGIN
        ALTER TABLE dbo.AvanceObraManual ADD Ejecutado DECIMAL(18,2) DEFAULT 0;
        PRINT '? Columna Ejecutado agregada';
    END
    
    -- Agregar columna Presupuestado si no existe
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AvanceObraManual') AND name = 'Presupuestado')
    BEGIN
        ALTER TABLE dbo.AvanceObraManual ADD Presupuestado DECIMAL(18,2) DEFAULT 0;
        PRINT '? Columna Presupuestado agregada';
    END
END
ELSE
BEGIN
    -- Crear tabla desde cero con las columnas correctas
    CREATE TABLE dbo.AvanceObraManual (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Manzana NVARCHAR(50) NOT NULL,
        Lote NVARCHAR(50) NOT NULL,
        Categoria NVARCHAR(100) NOT NULL,
        Presupuestado DECIMAL(18,2) DEFAULT 0,
        Ejecutado DECIMAL(18,2) DEFAULT 0,
        PorcentajeAvance DECIMAL(5,2) DEFAULT 0,
        Observaciones NVARCHAR(500),
        UsuarioCaptura NVARCHAR(100),
        FechaCaptura DATETIME DEFAULT GETDATE(),
        FechaModificacion DATETIME DEFAULT GETDATE(),
        CONSTRAINT UK_AvanceObra UNIQUE (Manzana, Lote, Categoria)
    );
    
    PRINT '? Tabla AvanceObraManual creada exitosamente';
END
GO

-- ============================================================================
-- CREAR ÍNDICES
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AvanceObra_ManzanaLote' AND object_id = OBJECT_ID('dbo.AvanceObraManual'))
BEGIN
    CREATE INDEX IX_AvanceObra_ManzanaLote ON dbo.AvanceObraManual(Manzana, Lote);
    PRINT '? Índice IX_AvanceObra_ManzanaLote creado';
END
ELSE
    PRINT '?? Índice IX_AvanceObra_ManzanaLote ya existe';
GO

-- ============================================================================
-- VERIFICAR ESTRUCTURA FINAL
-- ============================================================================
PRINT '';
PRINT '========================================';
PRINT '  ?? ESTRUCTURA DE LA TABLA';
PRINT '========================================';

SELECT 
    COLUMN_NAME AS Columna,
    DATA_TYPE AS Tipo,
    CHARACTER_MAXIMUM_LENGTH AS Longitud,
    IS_NULLABLE AS Permite_Null
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AvanceObraManual'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '? Tabla lista para usar en FormAvanceObra';
PRINT '   Columnas necesarias:';
PRINT '   ? Presupuestado (DECIMAL)';
PRINT '   ? Ejecutado (DECIMAL)';
PRINT '   ? PorcentajeAvance (DECIMAL)';
PRINT '   ? UsuarioCaptura (NVARCHAR)';
PRINT '   ? FechaCaptura (DATETIME)';
GO
