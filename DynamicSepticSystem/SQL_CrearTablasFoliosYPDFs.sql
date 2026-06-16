-- Script para crear las tablas de Folios de Estimación y PDFs

-- Tabla para registrar folios de estimaciones
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FoliosEstimacion')
BEGIN
    CREATE TABLE FoliosEstimacion (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Folio NVARCHAR(50) NOT NULL UNIQUE,
        Manzana NVARCHAR(10) NOT NULL,
        Lote NVARCHAR(10) NOT NULL,
        Prototipo NVARCHAR(50),
        FechaGeneracion DATETIME DEFAULT GETDATE(),
        Proveedor NVARCHAR(200),
        Descripcion NVARCHAR(500),
        ImporteContrato FLOAT,
        TotalRequisicion FLOAT,
        Amortizacion FLOAT,
        PorcentajeAmortizacion FLOAT,
        TotalEstimacion FLOAT,
        NumeroEstimacion INT,
        Usuario NVARCHAR(100),
        Observaciones NVARCHAR(MAX)
    );
    
    PRINT 'Tabla FoliosEstimacion creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla FoliosEstimacion ya existe';
END
GO

-- Tabla para detalles de conceptos incluidos en cada folio
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FoliosEstimacionDetalle')
BEGIN
    CREATE TABLE FoliosEstimacionDetalle (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FolioId INT NOT NULL,
        CodigoConcepto NVARCHAR(10),
        NombreConcepto NVARCHAR(200),
        WBS INT,
        NombrePartida NVARCHAR(500),
        MontoPresupuestado FLOAT,
        MontoEjecutado FLOAT,
        AvancePorcentaje FLOAT,
        FOREIGN KEY (FolioId) REFERENCES FoliosEstimacion(Id) ON DELETE CASCADE
    );
    
    PRINT 'Tabla FoliosEstimacionDetalle creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla FoliosEstimacionDetalle ya existe';
END
GO

-- Tabla para almacenar PDFs de estimaciones
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PDFsEstimacion')
BEGIN
    CREATE TABLE PDFsEstimacion (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FolioId INT NOT NULL,
        Folio NVARCHAR(50) NOT NULL,
        Manzana NVARCHAR(10) NOT NULL,
        Lote NVARCHAR(10) NOT NULL,
        NombreArchivo NVARCHAR(255),
        ContenidoPDF VARBINARY(MAX) NOT NULL,
        TamanioBytes BIGINT,
        FechaAlmacenamiento DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (FolioId) REFERENCES FoliosEstimacion(Id) ON DELETE CASCADE
    );
    
    PRINT 'Tabla PDFsEstimacion creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla PDFsEstimacion ya existe';
END
GO

-- Índices para optimizar búsquedas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoliosEstimacion_ManzanaLote')
BEGIN
    CREATE INDEX IX_FoliosEstimacion_ManzanaLote ON FoliosEstimacion(Manzana, Lote);
    PRINT 'Índice IX_FoliosEstimacion_ManzanaLote creado';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PDFsEstimacion_ManzanaLote')
BEGIN
    CREATE INDEX IX_PDFsEstimacion_ManzanaLote ON PDFsEstimacion(Manzana, Lote);
    PRINT 'Índice IX_PDFsEstimacion_ManzanaLote creado';
END
GO

-- Vista para consultar folios con totales
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VW_FoliosEstimacionResumen')
BEGIN
    DROP VIEW VW_FoliosEstimacionResumen;
END
GO

CREATE VIEW VW_FoliosEstimacionResumen AS
SELECT 
    f.Id,
    f.Folio,
    f.Manzana,
    f.Lote,
    f.Prototipo,
    f.FechaGeneracion,
    f.Proveedor,
    f.TotalEstimacion,
    f.NumeroEstimacion,
    COUNT(d.Id) AS TotalPartidas,
    CASE WHEN p.Id IS NOT NULL THEN 1 ELSE 0 END AS TienePDF
FROM FoliosEstimacion f
LEFT JOIN FoliosEstimacionDetalle d ON f.Id = d.FolioId
LEFT JOIN PDFsEstimacion p ON f.Id = p.FolioId
GROUP BY f.Id, f.Folio, f.Manzana, f.Lote, f.Prototipo, f.FechaGeneracion, 
         f.Proveedor, f.TotalEstimacion, f.NumeroEstimacion, p.Id;
GO

PRINT 'Vista VW_FoliosEstimacionResumen creada exitosamente';
GO
