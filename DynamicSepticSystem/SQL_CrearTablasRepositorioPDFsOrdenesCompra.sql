-- ================================================================
-- CREAR TABLAS DEL REPOSITORIO DE PDFs PARA ÓRDENES DE COMPRA
-- Base de datos: CALANDRIA
-- Compatible con Azure SQL Database
-- ================================================================
-- Este script crea las tablas necesarias para almacenar PDFs de 
-- órdenes de compra (directas e indirectas) con consulta por 
-- Manzana/Lote
-- ================================================================

PRINT '=== CREANDO TABLAS DEL REPOSITORIO DE PDFs ===';
PRINT 'Base de datos: ' + DB_NAME();
PRINT '';

-- ================================================================
-- 1. TABLA: FoliosOrdenCompra (Registro de órdenes con folio único)
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FoliosOrdenCompra')
BEGIN
    CREATE TABLE FoliosOrdenCompra (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Folio NVARCHAR(50) NOT NULL UNIQUE,
        Manzana NVARCHAR(10) NULL,  -- NULL para órdenes indirectas
        Lote NVARCHAR(10) NULL,     -- NULL para órdenes indirectas
        FechaGeneracion DATETIME DEFAULT GETDATE(),
        TipoOrden NVARCHAR(20) NOT NULL, -- 'MULTIPLE', 'INDIVIDUAL', 'INDIRECTA', 'ADMINISTRATIVA'
        NombreProveedor NVARCHAR(200),
        CodigoProveedor NVARCHAR(50),
        TotalSinIVA DECIMAL(18, 2) DEFAULT 0,
        IVA DECIMAL(18, 2) DEFAULT 0,
        TotalConIVA DECIMAL(18, 2) DEFAULT 0,
        NumeroOrden INT, -- Número correlativo por casa (si aplica)
        Usuario NVARCHAR(100),
        Observaciones NVARCHAR(MAX),
        Estado NVARCHAR(20) DEFAULT 'PENDIENTE' -- 'PENDIENTE', 'COMPLETADA', 'CANCELADA'
    );
    
    CREATE INDEX IX_FoliosOrdenCompra_ManzanaLote ON FoliosOrdenCompra(Manzana, Lote);
    CREATE INDEX IX_FoliosOrdenCompra_TipoOrden ON FoliosOrdenCompra(TipoOrden);
    CREATE INDEX IX_FoliosOrdenCompra_Fecha ON FoliosOrdenCompra(FechaGeneracion);
    
    PRINT '? Tabla FoliosOrdenCompra creada';
END
ELSE
    PRINT '- Tabla FoliosOrdenCompra ya existe';

-- ================================================================
-- 2. TABLA: FoliosOrdenCompraDetalle (Detalle de insumos por folio)
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FoliosOrdenCompraDetalle')
BEGIN
    CREATE TABLE FoliosOrdenCompraDetalle (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FolioId INT NOT NULL,
        Clave NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(500),
        Unidad NVARCHAR(50),
        Cantidad DECIMAL(18, 3) NOT NULL,
        PrecioUnitario DECIMAL(18, 2) DEFAULT 0,
        ImporteTotal DECIMAL(18, 2) DEFAULT 0,
        Familia NVARCHAR(100),
        FOREIGN KEY (FolioId) REFERENCES FoliosOrdenCompra(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_FoliosOrdenCompraDetalle_FolioId ON FoliosOrdenCompraDetalle(FolioId);
    CREATE INDEX IX_FoliosOrdenCompraDetalle_Clave ON FoliosOrdenCompraDetalle(Clave);
    
    PRINT '? Tabla FoliosOrdenCompraDetalle creada';
END
ELSE
    PRINT '- Tabla FoliosOrdenCompraDetalle ya existe';

-- ================================================================
-- 3. TABLA: FoliosOrdenCompra_Casas (Casas incluidas en orden múltiple)
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FoliosOrdenCompra_Casas')
BEGIN
    CREATE TABLE FoliosOrdenCompra_Casas (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FolioId INT NOT NULL,
        Manzana NVARCHAR(10) NOT NULL,
        Lote NVARCHAR(10) NOT NULL,
        Prototipo NVARCHAR(50),
        FOREIGN KEY (FolioId) REFERENCES FoliosOrdenCompra(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_FoliosOrdenCompra_Casas_FolioId ON FoliosOrdenCompra_Casas(FolioId);
    CREATE INDEX IX_FoliosOrdenCompra_Casas_Casa ON FoliosOrdenCompra_Casas(Manzana, Lote);
    
    PRINT '? Tabla FoliosOrdenCompra_Casas creada';
END
ELSE
    PRINT '- Tabla FoliosOrdenCompra_Casas ya existe';

-- ================================================================
-- 4. TABLA: PDFsOrdenCompra (Almacenamiento de PDFs)
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PDFsOrdenCompra')
BEGIN
    CREATE TABLE PDFsOrdenCompra (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FolioId INT NOT NULL,
        Folio NVARCHAR(50) NOT NULL,
        Manzana NVARCHAR(10) NULL,  -- NULL para indirectas
        Lote NVARCHAR(10) NULL,     -- NULL para indirectas
        TipoOrden NVARCHAR(20) NOT NULL,
        NombreArchivo NVARCHAR(255),
        ContenidoPDF VARBINARY(MAX) NOT NULL,
        TamanioBytes BIGINT,
        FechaAlmacenamiento DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (FolioId) REFERENCES FoliosOrdenCompra(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_PDFsOrdenCompra_FolioId ON PDFsOrdenCompra(FolioId);
    CREATE INDEX IX_PDFsOrdenCompra_ManzanaLote ON PDFsOrdenCompra(Manzana, Lote);
    CREATE INDEX IX_PDFsOrdenCompra_TipoOrden ON PDFsOrdenCompra(TipoOrden);
    CREATE INDEX IX_PDFsOrdenCompra_Folio ON PDFsOrdenCompra(Folio);
    
    PRINT '? Tabla PDFsOrdenCompra creada';
END
ELSE
    PRINT '- Tabla PDFsOrdenCompra ya existe';

PRINT '';
PRINT '=== CREANDO VISTAS DEL REPOSITORIO ===';

-- ================================================================
-- VISTA: Resumen de órdenes de compra con totales
-- ================================================================
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VW_FoliosOrdenCompraResumen')
BEGIN
    DROP VIEW VW_FoliosOrdenCompraResumen;
END
GO

CREATE VIEW VW_FoliosOrdenCompraResumen AS
SELECT 
    f.Id,
    f.Folio,
    f.Manzana,
    f.Lote,
    f.FechaGeneracion,
    f.TipoOrden,
    f.NombreProveedor,
    f.CodigoProveedor,
    f.TotalConIVA,
    f.NumeroOrden,
    f.Estado,
    COUNT(DISTINCT d.Id) AS TotalInsumos,
    CASE WHEN p.Id IS NOT NULL THEN 1 ELSE 0 END AS TienePDF,
    STUFF((
        SELECT ', M' + c.Manzana + '-L' + c.Lote
        FROM FoliosOrdenCompra_Casas c
        WHERE c.FolioId = f.Id
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS CasasIncluidas
FROM FoliosOrdenCompra f
LEFT JOIN FoliosOrdenCompraDetalle d ON f.Id = d.FolioId
LEFT JOIN PDFsOrdenCompra p ON f.Id = p.FolioId
GROUP BY f.Id, f.Folio, f.Manzana, f.Lote, f.FechaGeneracion, f.TipoOrden, 
         f.NombreProveedor, f.CodigoProveedor, f.TotalConIVA, f.NumeroOrden, f.Estado, p.Id;
GO

PRINT '? Vista VW_FoliosOrdenCompraResumen creada';
PRINT '';

-- ================================================================
-- VISTA: Órdenes de compra por casa
-- ================================================================
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VW_OrdenesCompraPorCasa')
BEGIN
    DROP VIEW VW_OrdenesCompraPorCasa;
END
GO

CREATE VIEW VW_OrdenesCompraPorCasa AS
SELECT DISTINCT
    fc.Manzana,
    fc.Lote,
    f.Folio,
    f.TipoOrden,
    f.FechaGeneracion,
    f.NombreProveedor,
    f.TotalConIVA,
    f.Estado,
    (SELECT COUNT(*) FROM FoliosOrdenCompraDetalle WHERE FolioId = f.Id) AS TotalInsumos
FROM FoliosOrdenCompra_Casas fc
INNER JOIN FoliosOrdenCompra f ON fc.FolioId = f.Id

UNION

SELECT 
    f.Manzana,
    f.Lote,
    f.Folio,
    f.TipoOrden,
    f.FechaGeneracion,
    f.NombreProveedor,
    f.TotalConIVA,
    f.Estado,
    (SELECT COUNT(*) FROM FoliosOrdenCompraDetalle WHERE FolioId = f.Id) AS TotalInsumos
FROM FoliosOrdenCompra f
WHERE f.Manzana IS NOT NULL AND f.Lote IS NOT NULL;
GO

PRINT '? Vista VW_OrdenesCompraPorCasa creada';
PRINT '';

PRINT '=== VERIFICACIÓN DE TABLAS ===';

-- Verificar que todas las tablas se crearon correctamente
SELECT 
    TABLE_NAME AS [Tabla],
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = T.TABLE_NAME)
        THEN '? EXISTE'
        ELSE '? NO EXISTE'
    END AS [Estado]
FROM (VALUES 
    ('FoliosOrdenCompra'),
    ('FoliosOrdenCompraDetalle'),
    ('FoliosOrdenCompra_Casas'),
    ('PDFsOrdenCompra')
) AS T(TABLE_NAME);

PRINT '';
PRINT '=== CONSULTAS ÚTILES ===';
PRINT '';
PRINT '-- Ver todas las órdenes generadas:';
PRINT 'SELECT * FROM VW_FoliosOrdenCompraResumen ORDER BY FechaGeneracion DESC';
PRINT '';
PRINT '-- Ver órdenes de una casa específica:';
PRINT 'SELECT * FROM VW_OrdenesCompraPorCasa WHERE Manzana = ''1'' AND Lote = ''1'' ORDER BY FechaGeneracion DESC';
PRINT '';
PRINT '-- Ver tamaño de PDFs almacenados:';
PRINT 'SELECT Folio, TipoOrden, TamanioBytes / 1024.0 / 1024.0 AS TamanioMB FROM PDFsOrdenCompra ORDER BY TamanioBytes DESC';
PRINT '';
PRINT '-- Ver órdenes indirectas (sin casa):';
PRINT 'SELECT * FROM FoliosOrdenCompra WHERE TipoOrden IN (''INDIRECTA'', ''ADMINISTRATIVA'') ORDER BY FechaGeneracion DESC';
PRINT '';

PRINT '=== CREACIÓN COMPLETADA ===';
PRINT 'Sistema de repositorio de PDFs para órdenes de compra listo.';
PRINT '';
