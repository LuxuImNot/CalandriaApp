-- =============================================
-- Script: Creación de Estructura Completa para Evidencias
-- Descripción: Crea todas las tablas necesarias desde cero
-- =============================================

-- Paso 1: Verificar base de datos activa
PRINT '?? Base de datos activa: ' + DB_NAME()
GO

-- =============================================
-- PASO 1: CREAR TABLA InventarioCasas SI NO EXISTE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InventarioCasas')
BEGIN
    PRINT 'Creando tabla InventarioCasas...'
    
    CREATE TABLE InventarioCasas (
        Manzana NVARCHAR(10) NOT NULL,
        Lote NVARCHAR(10) NOT NULL,
        Prototipo NVARCHAR(50),
        FotoPath NVARCHAR(500),
        DestajosTerminadosWBS NVARCHAR(MAX), -- JSON array
        FechaCreacion DATETIME DEFAULT GETDATE(),
        FechaActualizacion DATETIME DEFAULT GETDATE(),
        
        -- Clave primaria compuesta
        CONSTRAINT PK_InventarioCasas PRIMARY KEY (Manzana, Lote)
    );
    
    PRINT '? Tabla InventarioCasas creada'
    
    -- Insertar datos de ejemplo
    INSERT INTO InventarioCasas (Manzana, Lote, Prototipo)
    VALUES 
        ('1', '1', 'CALANDRIA'),
        ('1', '2', 'CALANDRIA'),
        ('2', '1', 'TUNERA'),
        ('2', '2', 'TUNERA');
    
    PRINT '? Datos de ejemplo insertados'
END
ELSE
BEGIN
    PRINT '? Tabla InventarioCasas ya existe'
    
    -- Verificar si tiene clave primaria
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
        
        PRINT '? Clave primaria agregada'
    END
    ELSE
    BEGIN
        PRINT '? InventarioCasas ya tiene clave primaria'
    END
END
GO

-- =============================================
-- PASO 2: CREAR TABLA EvidenciasFotograficas
-- =============================================

-- Eliminar tabla si existe (para desarrollo/testing)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'EvidenciasFotograficas')
BEGIN
    DROP TABLE EvidenciasFotograficas
    PRINT 'Tabla EvidenciasFotograficas eliminada para recreación'
END
GO

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

PRINT '? Tabla EvidenciasFotograficas creada exitosamente'
GO

-- Índices para mejorar rendimiento
CREATE INDEX IX_Evidencias_ManzanaLote ON EvidenciasFotograficas(Manzana, Lote);
CREATE INDEX IX_Evidencias_Fecha ON EvidenciasFotograficas(FechaCaptura DESC);
CREATE INDEX IX_Evidencias_Prototipo ON EvidenciasFotograficas(Prototipo);
GO

PRINT '? Índices creados'
GO

-- =============================================
-- PASO 3: PROCEDIMIENTOS ALMACENADOS
-- =============================================

-- Eliminar procedimientos existentes si existen
IF OBJECT_ID('sp_ObtenerUltimaEvidencia', 'P') IS NOT NULL
    DROP PROCEDURE sp_ObtenerUltimaEvidencia;
IF OBJECT_ID('sp_ListarEvidencias', 'P') IS NOT NULL
    DROP PROCEDURE sp_ListarEvidencias;
IF OBJECT_ID('sp_EstadisticasEvidencias', 'P') IS NOT NULL
    DROP PROCEDURE sp_EstadisticasEvidencias;
GO

-- Procedimiento: Obtener última evidencia
CREATE PROCEDURE sp_ObtenerUltimaEvidencia
    @Manzana NVARCHAR(10),
    @Lote NVARCHAR(10)
AS
BEGIN
    SELECT TOP 1
        Id, Manzana, Lote, Prototipo, TituloFoto,
        Foto, Extension, TamañoKB, FechaCaptura, UsuarioCaptura
    FROM EvidenciasFotograficas
    WHERE Manzana = @Manzana AND Lote = @Lote
    ORDER BY FechaCaptura DESC
END
GO

-- Procedimiento: Listar evidencias
CREATE PROCEDURE sp_ListarEvidencias
    @Manzana NVARCHAR(10),
    @Lote NVARCHAR(10)
AS
BEGIN
    SELECT 
        Id, Manzana, Lote, Prototipo, TituloFoto,
        Extension, TamañoKB, FechaCaptura, UsuarioCaptura,
        DATALENGTH(Foto) AS TamañoBytes
    FROM EvidenciasFotograficas
    WHERE Manzana = @Manzana AND Lote = @Lote
    ORDER BY FechaCaptura DESC
END
GO

-- Procedimiento: Estadísticas generales
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

PRINT '? Procedimientos almacenados creados'
GO

-- =============================================
-- PASO 4: VERIFICACIÓN FINAL
-- =============================================
PRINT ''
PRINT '========================================='
PRINT '? INSTALACIÓN COMPLETADA EXITOSAMENTE'
PRINT '========================================='
PRINT ''

-- Mostrar estructura
PRINT '?? Estructura de InventarioCasas:'
SELECT 
    COLUMN_NAME AS Columna,
    DATA_TYPE AS Tipo,
    IS_NULLABLE AS Nulable
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'InventarioCasas'
ORDER BY ORDINAL_POSITION

PRINT ''
PRINT '?? Estructura de EvidenciasFotograficas:'
SELECT 
    COLUMN_NAME AS Columna,
    DATA_TYPE AS Tipo,
    IS_NULLABLE AS Nulable
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'EvidenciasFotograficas'
ORDER BY ORDINAL_POSITION

PRINT ''
PRINT '?? Datos de ejemplo en InventarioCasas:'
SELECT * FROM InventarioCasas

PRINT ''
PRINT '========================================='
PRINT 'Comandos útiles:'
PRINT '  EXEC sp_ObtenerUltimaEvidencia ''1'', ''1'''
PRINT '  EXEC sp_ListarEvidencias ''1'', ''1'''
PRINT '  EXEC sp_EstadisticasEvidencias'
PRINT '========================================='
GO
