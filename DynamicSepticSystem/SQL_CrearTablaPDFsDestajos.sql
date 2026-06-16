-- ================================================================
-- TABLA: PDFsDestajos
-- Repositorio de PDFs de destajos generados desde
-- FormActivarTareasTreeList, almacenados como VARBINARY(MAX) y
-- consultables por Manzana / Lote.
-- Compatible con Azure SQL Database.
-- ================================================================

PRINT '=== CREANDO TABLA PDFsDestajos ===';
PRINT 'Base de datos: ' + DB_NAME();
PRINT '';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PDFsDestajos')
BEGIN
    CREATE TABLE PDFsDestajos (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        Manzana             NVARCHAR(10)  NOT NULL,
        Lote                NVARCHAR(10)  NOT NULL,
        Prototipo           NVARCHAR(50)  NULL,
        Ruta                NVARCHAR(50)  NULL,   -- RutaCalandraDestajo / RutaTuneraDestajo
        NodoID              INT           NULL,   -- ID del destajo dentro de la ruta
        NombreDestajo       NVARCHAR(200) NULL,
        CuadrillaAsignada   NVARCHAR(20)  NULL,
        NombreArchivo       NVARCHAR(255) NOT NULL,
        ContenidoPDF        VARBINARY(MAX) NOT NULL,
        TamanioBytes        BIGINT        NOT NULL,
        Usuario             NVARCHAR(100) NULL,
        FechaGeneracion     DATETIME      NOT NULL DEFAULT GETDATE()
    );

    CREATE INDEX IX_PDFsDestajos_ManzanaLote ON PDFsDestajos(Manzana, Lote);
    CREATE INDEX IX_PDFsDestajos_Fecha       ON PDFsDestajos(FechaGeneracion DESC);
    CREATE INDEX IX_PDFsDestajos_Ruta        ON PDFsDestajos(Ruta);

    PRINT 'Tabla PDFsDestajos creada correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla PDFsDestajos ya existe.';
END
GO

PRINT '';
PRINT '=== CONSULTAS UTILES ===';
PRINT '-- PDFs por casa:';
PRINT 'SELECT Id, NombreArchivo, FechaGeneracion, TamanioBytes / 1024 AS KB';
PRINT '  FROM PDFsDestajos WHERE Manzana = ''1'' AND Lote = ''1''';
PRINT '  ORDER BY FechaGeneracion DESC';
PRINT '';
PRINT '-- Tamano total almacenado (MB):';
PRINT 'SELECT SUM(TamanioBytes) / 1024.0 / 1024.0 AS MB FROM PDFsDestajos';
