-- ========================================
-- Script: Crear tablas para Repositorio de Vales de Salida de Almacén
-- Fecha: 2025-01-XX
-- Descripción: Sistema de folios únicos y almacenamiento de PDFs para vales de salida
-- ========================================

USE CALANDRIA;
GO

PRINT '=== CREANDO TABLAS PARA REPOSITORIO DE VALES DE SALIDA ===';
PRINT '';

-- ================================================================
-- 1. Tabla: FoliosSalidaAlmacen (Encabezado de vales)
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FoliosSalidaAlmacen]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FoliosSalidaAlmacen] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Folio] NVARCHAR(50) NOT NULL UNIQUE,
        [Manzana] NVARCHAR(10) NOT NULL,
        [Lote] NVARCHAR(10) NOT NULL,
        [Prototipo] NVARCHAR(50) NULL,
        [Obra] NVARCHAR(200) NULL,
        [Edificacion] NVARCHAR(200) NULL,
        [Urbanizacion] NVARCHAR(200) NULL,
        [FechaSolicitud] DATETIME NOT NULL DEFAULT(GETDATE()),
        [DiaSolicitud] INT NULL,
        [MesSolicitud] INT NULL,
        [AnioSolicitud] INT NULL,
        [TotalImporte] DECIMAL(18,2) NULL DEFAULT(0),
        [NumeroSalida] INT NULL, -- Número correlativo por casa
        [Usuario] NVARCHAR(100) NULL,
        [Solicitante] NVARCHAR(200) NULL,
        [ResidenteObra] NVARCHAR(200) NULL,
        [EncargadoAlmacen] NVARCHAR(200) NULL,
        [Observaciones] NVARCHAR(MAX) NULL,
        [Estado] NVARCHAR(20) NULL DEFAULT('PENDIENTE'), -- PENDIENTE, ENTREGADO, CANCELADO
        CONSTRAINT [UQ_FolioSalida] UNIQUE ([Folio])
    );
    
    PRINT '? Tabla FoliosSalidaAlmacen creada correctamente';
END
ELSE
BEGIN
    PRINT '?? Tabla FoliosSalidaAlmacen ya existe';
END
GO

-- ================================================================
-- 2. Tabla: FoliosSalidaAlmacenDetalle (Detalle de insumos)
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FoliosSalidaAlmacenDetalle]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FoliosSalidaAlmacenDetalle] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [FolioId] INT NOT NULL,
        [NumeroFila] INT NULL, -- Número de fila en el PDF (1-10)
        [Codigo] NVARCHAR(100) NULL,
        [Descripcion] NVARCHAR(500) NULL,
        [Unidad] NVARCHAR(50) NULL,
        [Cantidad] DECIMAL(18,3) NULL,
        [PrecioUnitario] DECIMAL(18,2) NULL,
        [Importe] DECIMAL(18,2) NULL,
        [Observaciones] NVARCHAR(500) NULL,
        CONSTRAINT [FK_FoliosSalidaDetalle_Folio] 
            FOREIGN KEY ([FolioId]) REFERENCES [dbo].[FoliosSalidaAlmacen]([Id]) 
            ON DELETE CASCADE
    );
    
    PRINT '? Tabla FoliosSalidaAlmacenDetalle creada correctamente';
END
ELSE
BEGIN
    PRINT '?? Tabla FoliosSalidaAlmacenDetalle ya existe';
END
GO

-- ================================================================
-- 3. Tabla: PDFsSalidaAlmacen (Almacenamiento de PDFs)
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PDFsSalidaAlmacen]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PDFsSalidaAlmacen] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [FolioId] INT NOT NULL,
        [Folio] NVARCHAR(50) NOT NULL, -- Copia para búsqueda rápida
        [Manzana] NVARCHAR(10) NOT NULL, -- Para filtros
        [Lote] NVARCHAR(10) NOT NULL, -- Para filtros
        [NombreArchivo] NVARCHAR(255) NOT NULL,
        [ContenidoPDF] VARBINARY(MAX) NOT NULL, -- PDF almacenado como bytes
        [TamanioBytes] BIGINT NOT NULL,
        [FechaAlmacenamiento] DATETIME NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT [FK_PDFsSalida_Folio] 
            FOREIGN KEY ([FolioId]) REFERENCES [dbo].[FoliosSalidaAlmacen]([Id]) 
            ON DELETE CASCADE
    );
    
    -- Índice para búsquedas por Manzana/Lote
    CREATE NONCLUSTERED INDEX [IX_PDFsSalida_ManzanaLote] 
        ON [dbo].[PDFsSalidaAlmacen] ([Manzana], [Lote]);
    
    -- Índice para búsquedas por Folio
    CREATE NONCLUSTERED INDEX [IX_PDFsSalida_Folio] 
        ON [dbo].[PDFsSalidaAlmacen] ([Folio]);
    
    PRINT '? Tabla PDFsSalidaAlmacen creada correctamente';
END
ELSE
BEGIN
    PRINT '?? Tabla PDFsSalidaAlmacen ya existe';
END
GO

-- ================================================================
-- 4. Vista: Resumen de Vales de Salida
-- ================================================================
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VW_FoliosSalidaResumen')
    DROP VIEW [dbo].[VW_FoliosSalidaResumen];
GO

CREATE VIEW [dbo].[VW_FoliosSalidaResumen]
AS
SELECT 
    f.Id,
    f.Folio,
    f.Manzana,
    f.Lote,
    f.Prototipo,
    f.Obra,
    f.FechaSolicitud,
    f.TotalImporte,
    f.NumeroSalida,
    f.Usuario,
    f.Solicitante,
    f.Estado,
    (SELECT COUNT(*) FROM FoliosSalidaAlmacenDetalle WHERE FolioId = f.Id) AS TotalInsumos,
    (SELECT CASE WHEN EXISTS(SELECT 1 FROM PDFsSalidaAlmacen WHERE FolioId = f.Id) 
        THEN 'SÍ' ELSE 'NO' END) AS TienePDF
FROM FoliosSalidaAlmacen f;
GO

PRINT '? Vista VW_FoliosSalidaResumen creada correctamente';
PRINT '';

-- ================================================================
-- 5. Procedimiento: Obtener siguiente número de salida
-- ================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_ObtenerSiguienteNumeroSalida]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_ObtenerSiguienteNumeroSalida];
GO

CREATE PROCEDURE [dbo].[SP_ObtenerSiguienteNumeroSalida]
    @Manzana NVARCHAR(10),
    @Lote NVARCHAR(10),
    @SiguienteNumero INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT @SiguienteNumero = ISNULL(MAX(NumeroSalida), 0) + 1
    FROM FoliosSalidaAlmacen
    WHERE Manzana = @Manzana AND Lote = @Lote;
END
GO

PRINT '? Procedimiento SP_ObtenerSiguienteNumeroSalida creado correctamente';
PRINT '';

-- ================================================================
-- 6. Resumen Final
-- ================================================================
PRINT '========================================';
PRINT '? SISTEMA DE REPOSITORIO DE VALES DE SALIDA CREADO';
PRINT '========================================';
PRINT '';
PRINT 'Tablas creadas:';
PRINT '  1. FoliosSalidaAlmacen (Encabezados)';
PRINT '  2. FoliosSalidaAlmacenDetalle (Insumos)';
PRINT '  3. PDFsSalidaAlmacen (PDFs almacenados)';
PRINT '';
PRINT 'Vistas creadas:';
PRINT '  1. VW_FoliosSalidaResumen';
PRINT '';
PRINT 'Procedimientos creados:';
PRINT '  1. SP_ObtenerSiguienteNumeroSalida';
PRINT '';
PRINT '========================================';
GO
