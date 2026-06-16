-- ================================================================
-- VERIFICAR BASE DE DATOS Y TABLAS
-- Compatible con Azure SQL Database (sin USE)
-- ================================================================
-- Este script te ayuda a confirmar que estás en la BD correcta
-- ================================================================

PRINT '=== INFORMACIÓN DE LA BASE DE DATOS ===';
PRINT '';

-- Ver la base de datos actual conectada
PRINT 'Base de datos actual: ' + DB_NAME();
PRINT '';

-- Ver el servidor
PRINT 'Servidor: ' + @@SERVERNAME;
PRINT '';

-- Ver la versión de SQL Server
PRINT 'Versión: ' + @@VERSION;
PRINT '';
PRINT '========================================';
PRINT '';

-- Ver todas las tablas en la base de datos actual
PRINT '=== TABLAS EN ' + DB_NAME() + ' ===';
PRINT '';

SELECT 
    TABLE_SCHEMA AS [Esquema],
    TABLE_NAME AS [Nombre Tabla],
    TABLE_TYPE AS [Tipo]
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

PRINT '';
PRINT '========================================';
PRINT '';

-- Verificar específicamente las tablas del sistema de órdenes de compra
PRINT '=== VERIFICACIÓN DE TABLAS DEL SISTEMA ===';
PRINT '';

DECLARE @ResultadoEntradasAlmacen NVARCHAR(20);
DECLARE @ResultadoOrdenesCompra NVARCHAR(20);
DECLARE @ResultadoOrdenesCompraDetalle NVARCHAR(20);
DECLARE @ResultadoOrdenesCompra_Casas NVARCHAR(20);

SET @ResultadoEntradasAlmacen = CASE 
    WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntradasAlmacen')
    THEN '? EXISTE'
    ELSE '? NO EXISTE'
END;

SET @ResultadoOrdenesCompra = CASE 
    WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompra')
    THEN '? EXISTE'
    ELSE '? NO EXISTE'
END;

SET @ResultadoOrdenesCompraDetalle = CASE 
    WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompraDetalle')
    THEN '? EXISTE'
    ELSE '? NO EXISTE'
END;

SET @ResultadoOrdenesCompra_Casas = CASE 
    WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompra_Casas')
    THEN '? EXISTE'
    ELSE '? NO EXISTE'
END;

PRINT 'EntradasAlmacen: ' + @ResultadoEntradasAlmacen;
PRINT 'OrdenesCompra: ' + @ResultadoOrdenesCompra;
PRINT 'OrdenesCompraDetalle: ' + @ResultadoOrdenesCompraDetalle;
PRINT 'OrdenesCompra_Casas: ' + @ResultadoOrdenesCompra_Casas;

-- Mostrar en tabla
SELECT 
    @ResultadoEntradasAlmacen AS EntradasAlmacen,
    @ResultadoOrdenesCompra AS OrdenesCompra,
    @ResultadoOrdenesCompraDetalle AS OrdenesCompraDetalle,
    @ResultadoOrdenesCompra_Casas AS OrdenesCompra_Casas;

PRINT '';
PRINT '========================================';
PRINT '';

-- Si las tablas existen, mostrar conteo de registros
IF @ResultadoEntradasAlmacen = '? EXISTE'
BEGIN
    PRINT '=== CONTEO DE REGISTROS ===';
    PRINT '';
    
    DECLARE @CountEntradasAlmacen INT = 0;
    DECLARE @CountOrdenesCompra INT = 0;
    DECLARE @CountOrdenesCompraDetalle INT = 0;
    DECLARE @CountOrdenesCompra_Casas INT = 0;
    
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntradasAlmacen')
        SELECT @CountEntradasAlmacen = COUNT(*) FROM EntradasAlmacen;
    
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompra')
        SELECT @CountOrdenesCompra = COUNT(*) FROM OrdenesCompra;
    
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompraDetalle')
        SELECT @CountOrdenesCompraDetalle = COUNT(*) FROM OrdenesCompraDetalle;
    
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompra_Casas')
        SELECT @CountOrdenesCompra_Casas = COUNT(*) FROM OrdenesCompra_Casas;
    
    PRINT 'EntradasAlmacen: ' + CAST(@CountEntradasAlmacen AS NVARCHAR(10)) + ' registros';
    PRINT 'OrdenesCompra: ' + CAST(@CountOrdenesCompra AS NVARCHAR(10)) + ' registros';
    PRINT 'OrdenesCompraDetalle: ' + CAST(@CountOrdenesCompraDetalle AS NVARCHAR(10)) + ' registros';
    PRINT 'OrdenesCompra_Casas: ' + CAST(@CountOrdenesCompra_Casas AS NVARCHAR(10)) + ' registros';
    
    -- Mostrar en tabla
    SELECT 
        'EntradasAlmacen' AS Tabla,
        @CountEntradasAlmacen AS [Total Registros]
    UNION ALL
    SELECT 'OrdenesCompra', @CountOrdenesCompra
    UNION ALL
    SELECT 'OrdenesCompraDetalle', @CountOrdenesCompraDetalle
    UNION ALL
    SELECT 'OrdenesCompra_Casas', @CountOrdenesCompra_Casas;
    
    PRINT '';
    PRINT '========================================';
    PRINT '';
END
ELSE
BEGIN
    PRINT '? Las tablas del sistema NO EXISTEN en esta base de datos';
    PRINT 'Estás conectado a: ' + DB_NAME();
    PRINT '';
    PRINT 'SOLUCIÓN:';
    PRINT '1. Verifica el nombre de tu base de datos en App.config';
    PRINT '2. Conéctate a la base de datos correcta';
    PRINT '3. Ejecuta nuevamente este script';
    PRINT '';
END

-- Mostrar connection string sugerido
PRINT '=== INFORMACIÓN PARA APP.CONFIG ===';
PRINT '';
PRINT 'Nombre de la base de datos actual: ' + DB_NAME();
PRINT '';
PRINT 'Connection String sugerido:';
PRINT '';

-- Detectar si es Azure SQL
IF CHARINDEX('.database.windows.net', @@SERVERNAME) > 0
BEGIN
    PRINT '-- Para Azure SQL Database:';
    PRINT 'Server=tcp:' + @@SERVERNAME + ',1433;';
    PRINT 'Database=' + DB_NAME() + ';';
    PRINT 'User Id=tu_usuario;';
    PRINT 'Password=tu_contraseña;';
    PRINT 'Encrypt=True;';
    PRINT 'TrustServerCertificate=False;';
    PRINT 'Connection Timeout=30;';
END
ELSE
BEGIN
    PRINT '-- Para SQL Server local:';
    PRINT 'Server=' + @@SERVERNAME + ';';
    PRINT 'Database=' + DB_NAME() + ';';
    PRINT 'Integrated Security=True;';
    PRINT '';
    PRINT '-- o con autenticación SQL Server:';
    PRINT 'Server=' + @@SERVERNAME + ';';
    PRINT 'Database=' + DB_NAME() + ';';
    PRINT 'User Id=tu_usuario;';
    PRINT 'Password=tu_contraseña;';
END

PRINT '';
PRINT '========================================';
PRINT '';
PRINT '=== VERIFICACIÓN COMPLETADA ===';
PRINT 'Base de datos verificada: ' + DB_NAME();
