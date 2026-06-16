-- ================================================================
-- VERIFICAR NOMBRE DE BASE DE DATOS Y TABLAS
-- ================================================================
-- Ejecuta este script para confirmar el nombre de tu base de datos
-- y verificar que las tablas existan
-- ================================================================

-- Ver la base de datos actual conectada
SELECT DB_NAME() AS [Base de Datos Actual];

-- Ver todas las bases de datos disponibles en el servidor
SELECT 
    name AS [Nombre Base de Datos],
    database_id AS [ID],
    create_date AS [Fecha Creación],
    state_desc AS [Estado]
FROM sys.databases
ORDER BY name;

-- Ver todas las tablas en la base de datos actual
SELECT 
    TABLE_SCHEMA AS [Esquema],
    TABLE_NAME AS [Nombre Tabla],
    TABLE_TYPE AS [Tipo]
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Verificar específicamente las tablas del sistema de órdenes de compra
PRINT '';
PRINT '=== VERIFICACIÓN DE TABLAS DEL SISTEMA ===';

SELECT 
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntradasAlmacen')
        THEN '? EXISTE'
        ELSE '? NO EXISTE'
    END AS EntradasAlmacen,
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompra')
        THEN '? EXISTE'
        ELSE '? NO EXISTE'
    END AS OrdenesCompra,
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompraDetalle')
        THEN '? EXISTE'
        ELSE '? NO EXISTE'
    END AS OrdenesCompraDetalle,
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrdenesCompra_Casas')
        THEN '? EXISTE'
        ELSE '? NO EXISTE'
    END AS OrdenesCompra_Casas;

-- Ver el connection string que probablemente usas
PRINT '';
PRINT '=== INFORMACIÓN PARA APP.CONFIG ===';
PRINT 'Connection String sugerido:';
PRINT 'Server=localhost;Database=' + DB_NAME() + ';Integrated Security=True;';
PRINT '';
PRINT 'o con autenticación SQL Server:';
PRINT 'Server=localhost;Database=' + DB_NAME() + ';User Id=tu_usuario;Password=tu_contraseña;';

-- Contar registros en cada tabla
PRINT '';
PRINT '=== CONTEO DE REGISTROS ===';

DECLARE @sql NVARCHAR(MAX);
DECLARE @tableName NVARCHAR(255);
DECLARE @count INT;

DECLARE table_cursor CURSOR FOR
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_NAME IN ('EntradasAlmacen', 'OrdenesCompra', 'OrdenesCompraDetalle', 'OrdenesCompra_Casas')
ORDER BY TABLE_NAME;

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @tableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'SELECT @count = COUNT(*) FROM [' + @tableName + ']';
    EXEC sp_executesql @sql, N'@count INT OUTPUT', @count OUTPUT;
    PRINT @tableName + ': ' + CAST(@count AS NVARCHAR(10)) + ' registros';
    
    FETCH NEXT FROM table_cursor INTO @tableName;
END;

CLOSE table_cursor;
DEALLOCATE table_cursor;

PRINT '';
PRINT '=== VERIFICACIÓN COMPLETADA ===';
