-- ================================================================
-- CREAR TABLAS DEL SISTEMA DE ÓRDENES DE COMPRA
-- Base de datos: CALANDRIA
-- Compatible con Azure SQL Database
-- ================================================================
-- Este script crea todas las tablas necesarias para el sistema
-- ================================================================

PRINT '=== CREANDO TABLAS DEL SISTEMA DE ÓRDENES DE COMPRA ===';
PRINT 'Base de datos: ' + DB_NAME();
PRINT '';

-- ================================================================
-- 1. TABLA: OrdenesCompra (Tabla principal)
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompra')
BEGIN
    CREATE TABLE OrdenesCompra (
        FolioOC NVARCHAR(50) PRIMARY KEY,
        Fecha DATETIME NOT NULL DEFAULT GETDATE(),
        Usuario NVARCHAR(100),
        TipoOrden NVARCHAR(20) -- 'MULTIPLE', 'INDIRECTA', 'INDIVIDUAL'
    );
    PRINT '? Tabla OrdenesCompra creada';
END
ELSE
    PRINT '- Tabla OrdenesCompra ya existe';

-- ================================================================
-- 2. TABLA: OrdenesCompraDetalle (Detalles/Insumos de la orden)
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompraDetalle')
BEGIN
    CREATE TABLE OrdenesCompraDetalle (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        FolioOC NVARCHAR(50) NOT NULL,
        Clave NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(500),
        Unidad NVARCHAR(50),
        Cantidad DECIMAL(18, 3) NOT NULL,
        PrecioUnitario DECIMAL(18, 2) DEFAULT 0,
        ImporteTotal DECIMAL(18, 2) DEFAULT 0,
        Familia NVARCHAR(100),
        Estado NVARCHAR(20) DEFAULT 'PENDIENTE', -- 'PENDIENTE', 'COMPLETADA', 'PARCIAL'
        FOREIGN KEY (FolioOC) REFERENCES OrdenesCompra(FolioOC) ON DELETE CASCADE
    );
    
    -- Índices para mejorar rendimiento
    CREATE INDEX IX_OrdenesCompraDetalle_FolioOC ON OrdenesCompraDetalle(FolioOC);
    CREATE INDEX IX_OrdenesCompraDetalle_Clave ON OrdenesCompraDetalle(Clave);
    CREATE INDEX IX_OrdenesCompraDetalle_Estado ON OrdenesCompraDetalle(Estado);
    
    PRINT '? Tabla OrdenesCompraDetalle creada';
END
ELSE
    PRINT '- Tabla OrdenesCompraDetalle ya existe';

-- ================================================================
-- 3. TABLA: OrdenesCompra_Casas (Relación orden-casas)
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompra_Casas')
BEGIN
    CREATE TABLE OrdenesCompra_Casas (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        FolioOC NVARCHAR(50) NOT NULL,
        Manzana NVARCHAR(10) NOT NULL,
        Lote NVARCHAR(10) NOT NULL,
        FOREIGN KEY (FolioOC) REFERENCES OrdenesCompra(FolioOC) ON DELETE CASCADE
    );
    
    -- Índices
    CREATE INDEX IX_OrdenesCompra_Casas_FolioOC ON OrdenesCompra_Casas(FolioOC);
    CREATE INDEX IX_OrdenesCompra_Casas_Casa ON OrdenesCompra_Casas(Manzana, Lote);
    
    PRINT '? Tabla OrdenesCompra_Casas creada';
END
ELSE
    PRINT '- Tabla OrdenesCompra_Casas ya existe';

-- ================================================================
-- 4. TABLA: EntradasAlmacen (Registro de entradas)
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntradasAlmacen')
BEGIN
    CREATE TABLE EntradasAlmacen (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        FolioOC NVARCHAR(50) NOT NULL,
        Manzana NVARCHAR(10),
        Lote NVARCHAR(10),
        Clave NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(500),
        Unidad NVARCHAR(50),
        Cantidad DECIMAL(18, 3) NOT NULL,
        FechaEntrada DATETIME NOT NULL DEFAULT GETDATE(),
        Usuario NVARCHAR(100),
        Observaciones NVARCHAR(MAX),
        FOREIGN KEY (FolioOC) REFERENCES OrdenesCompra(FolioOC) ON DELETE CASCADE
    );
    
    -- Índices
    CREATE INDEX IX_EntradasAlmacen_FolioOC ON EntradasAlmacen(FolioOC);
    CREATE INDEX IX_EntradasAlmacen_Clave ON EntradasAlmacen(Clave);
    CREATE INDEX IX_EntradasAlmacen_Casa ON EntradasAlmacen(Manzana, Lote);
    CREATE INDEX IX_EntradasAlmacen_Fecha ON EntradasAlmacen(FechaEntrada);
    
    PRINT '? Tabla EntradasAlmacen creada';
END
ELSE
    PRINT '- Tabla EntradasAlmacen ya existe';

PRINT '';
PRINT '=== VERIFICACIÓN DE TABLAS ===';

-- Verificar que todas las tablas se crearon correctamente
SELECT 
    TABLE_NAME AS [Tabla],
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompra')
        THEN '? EXISTE'
        ELSE '? NO EXISTE'
    END AS [Estado]
FROM (VALUES ('OrdenesCompra')) AS T(TABLE_NAME)
UNION ALL
SELECT 'OrdenesCompraDetalle',
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompraDetalle')
        THEN '? EXISTE'
        ELSE '? NO EXISTE'
    END
UNION ALL
SELECT 'OrdenesCompra_Casas',
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompra_Casas')
        THEN '? EXISTE'
        ELSE '? NO EXISTE'
    END
UNION ALL
SELECT 'EntradasAlmacen',
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntradasAlmacen')
        THEN '? EXISTE'
        ELSE '? NO EXISTE'
    END;

PRINT '';
PRINT '=== CREACIÓN DE TABLAS COMPLETADA ===';
PRINT 'Todas las tablas del sistema de órdenes de compra están listas';
PRINT '';
PRINT 'Ahora puedes ejecutar el script de reseteo si lo necesitas:';
PRINT '  - SQL_RESETEAR_AzureSQL.sql';
