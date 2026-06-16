-- =============================================
-- Script: Creación de Tabla de Evidencias Fotográficas
-- Descripción: Tabla para almacenar fotos de avance de obra
-- Fecha: 2024
-- =============================================

-- ?? IMPORTANTE: Reemplaza [NombreDeTuBaseDeDatos] con el nombre real de tu BD
USE [master]
GO

-- Verificar nombre de la base de datos
DECLARE @DBName NVARCHAR(128) = 'CalandriaDB' -- ?? CAMBIA ESTE NOMBRE SI ES DIFERENTE

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = @DBName)
BEGIN
    PRINT '? ERROR: La base de datos [' + @DBName + '] no existe'
    PRINT 'Bases de datos disponibles:'
    SELECT name FROM sys.databases WHERE database_id > 4 -- Excluir bases del sistema
    RAISERROR('Base de datos no encontrada', 16, 1)
    RETURN
END

-- Cambiar al contexto de la base de datos
EXEC('USE [' + @DBName + ']')
GO

PRINT '? Conectado a la base de datos correcta'
GO

-- =============================================
-- PASO 1: Verificar y crear clave primaria en InventarioCasas
-- =============================================
PRINT 'Verificando estructura de InventarioCasas...'
GO

-- Verificar si existe clave primaria compuesta
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE TABLE_NAME = 'InventarioCasas' 
    AND CONSTRAINT_TYPE = 'PRIMARY KEY'
)
BEGIN
    PRINT 'Agregando clave primaria a InventarioCasas...'
    
    -- Eliminar duplicados si existen
    ;WITH CTE AS (
        SELECT *,
            ROW_NUMBER() OVER (PARTITION BY Manzana, Lote ORDER BY (SELECT NULL)) AS rn
        FROM InventarioCasas
    )
    DELETE FROM CTE WHERE rn > 1;
    
    -- Agregar clave primaria
    ALTER TABLE InventarioCasas
    ADD CONSTRAINT PK_InventarioCasas PRIMARY KEY (Manzana, Lote);
    
    PRINT '? Clave primaria agregada correctamente'
END
ELSE
BEGIN
    PRINT '? InventarioCasas ya tiene clave primaria'
END
GO

-- =============================================
-- PASO 2: Crear tabla EvidenciasFotograficas
-- =============================================

-- Eliminar tabla si existe (para desarrollo)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'EvidenciasFotograficas')
BEGIN
    DROP TABLE EvidenciasFotograficas
    PRINT 'Tabla EvidenciasFotograficas eliminada para recreación'
END
GO

-- Crear tabla para almacenar evidencias fotográficas
CREATE TABLE EvidenciasFotograficas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Manzana NVARCHAR(10) NOT NULL,
    Lote NVARCHAR(10) NOT NULL,
    Prototipo NVARCHAR(50),
    TituloFoto NVARCHAR(200) NOT NULL,
    Foto VARBINARY(MAX) NOT NULL,
    Extension NVARCHAR(10),
    TamañoKB FLOAT,
    FechaCaptura DATETIME DEFAULT GETDATE(),
    UsuarioCaptura NVARCHAR(100),
    
    -- Restricción de integridad referencial
    CONSTRAINT FK_Evidencias_Inventario 
        FOREIGN KEY (Manzana, Lote) 
        REFERENCES InventarioCasas(Manzana, Lote)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);
GO

-- Índices para mejorar rendimiento de consultas
CREATE INDEX IX_Evidencias_ManzanaLote 
    ON EvidenciasFotograficas(Manzana, Lote);
GO

CREATE INDEX IX_Evidencias_Fecha 
    ON EvidenciasFotograficas(FechaCaptura DESC);
GO

CREATE INDEX IX_Evidencias_Prototipo 
    ON EvidenciasFotograficas(Prototipo);
GO

-- Verificar creación exitosa
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'EvidenciasFotograficas')
BEGIN
    PRINT '? Tabla EvidenciasFotograficas creada exitosamente'
    
    -- Mostrar estructura de la tabla
    SELECT 
        COLUMN_NAME AS Columna,
        DATA_TYPE AS Tipo,
        CHARACTER_MAXIMUM_LENGTH AS Longitud,
        IS_NULLABLE AS Nulable
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'EvidenciasFotograficas'
    ORDER BY ORDINAL_POSITION
END
ELSE
BEGIN
    PRINT '? Error al crear tabla EvidenciasFotograficas'
END
GO

-- =============================================
-- PASO 3: Crear/Actualizar procedimientos almacenados
-- =============================================

-- Eliminar procedimientos existentes
IF OBJECT_ID('sp_ObtenerUltimaEvidencia', 'P') IS NOT NULL
    DROP PROCEDURE sp_ObtenerUltimaEvidencia;
GO

IF OBJECT_ID('sp_ListarEvidencias', 'P') IS NOT NULL
    DROP PROCEDURE sp_ListarEvidencias;
GO

IF OBJECT_ID('sp_EstadisticasEvidencias', 'P') IS NOT NULL
    DROP PROCEDURE sp_EstadisticasEvidencias;
GO

-- Procedimiento almacenado para obtener última evidencia
CREATE PROCEDURE sp_ObtenerUltimaEvidencia
    @Manzana NVARCHAR(10),
    @Lote NVARCHAR(10)
AS
BEGIN
    SELECT TOP 1
        Id,
        Manzana,
        Lote,
        Prototipo,
        TituloFoto,
        Foto,
        Extension,
        TamañoKB,
        FechaCaptura,
        UsuarioCaptura
    FROM EvidenciasFotograficas
    WHERE Manzana = @Manzana AND Lote = @Lote
    ORDER BY FechaCaptura DESC
END
GO

-- Procedimiento almacenado para listar evidencias
CREATE PROCEDURE sp_ListarEvidencias
    @Manzana NVARCHAR(10),
    @Lote NVARCHAR(10)
AS
BEGIN
    SELECT 
        Id,
        Manzana,
        Lote,
        Prototipo,
        TituloFoto,
        Extension,
        TamañoKB,
        FechaCaptura,
        UsuarioCaptura,
        DATALENGTH(Foto) AS TamañoBytes
    FROM EvidenciasFotograficas
    WHERE Manzana = @Manzana AND Lote = @Lote
    ORDER BY FechaCaptura DESC
END
GO

-- Procedimiento almacenado para obtener estadísticas
CREATE PROCEDURE sp_EstadisticasEvidencias
AS
BEGIN
    SELECT 
        COUNT(*) AS TotalEvidencias,
        COUNT(DISTINCT Manzana) AS ManzanasConEvidencias,
        COUNT(DISTINCT CONCAT(Manzana, '-', Lote)) AS LotesConEvidencias,
        SUM(TamañoKB) / 1024.0 AS TotalMB,
        AVG(TamañoKB) AS PromedioKB,
        MIN(FechaCaptura) AS PrimeraEvidencia,
        MAX(FechaCaptura) AS UltimaEvidencia
    FROM EvidenciasFotograficas
END
GO

PRINT '? Sistema de Evidencias Fotográficas instalado correctamente'
PRINT 'Ejecute: EXEC sp_EstadisticasEvidencias para ver estadísticas'
GO
