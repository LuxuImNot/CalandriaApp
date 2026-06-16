-- Script para verificar e insertar datos de prueba en COMPRASINDIRECTAS
-- Ejecutar este script en SQL Server Management Studio

-- 1. Verificar si la tabla existe
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.COMPRASINDIRECTAS') AND type in (N'U'))
BEGIN
    PRINT 'La tabla COMPRASINDIRECTAS ya existe'
    
    -- Mostrar los registros actuales
    SELECT COUNT(*) AS TotalInsumos FROM dbo.COMPRASINDIRECTAS
    SELECT TOP 10 * FROM dbo.COMPRASINDIRECTAS ORDER BY Clave
END
ELSE
BEGIN
    PRINT 'La tabla COMPRASINDIRECTAS NO existe. Se creará automáticamente al abrir FormCompraIndirecta.'
END

-- 2. (OPCIONAL) Insertar algunos insumos de prueba
-- Descomentar estas líneas para insertar datos de ejemplo

/*
-- Verificar que la tabla existe antes de insertar
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.COMPRASINDIRECTAS') AND type in (N'U'))
BEGIN
    -- Insertar insumos de prueba (solo si no existen)
    IF NOT EXISTS (SELECT 1 FROM COMPRASINDIRECTAS WHERE Clave = 'ADMIN-001-A')
    BEGIN
        INSERT INTO COMPRASINDIRECTAS (Clave, Descripcion, Unidad)
        VALUES 
            ('ADMIN-001-A', 'Papelería oficina general', 'PAQUETE'),
            ('ADMIN-002-A', 'Toner para impresora HP LaserJet', 'PIEZA'),
            ('ADMIN-003-A', 'Café para oficina', 'KILO'),
            ('ADMIN-004-A', 'Agua purificada garrafón', 'PIEZA'),
            ('MANT-001-A', 'Herramienta eléctrica diversos', 'LOTE'),
            ('LIMP-001-A', 'Material de limpieza general', 'PAQUETE'),
            ('SEG-001-A', 'Equipo de protección personal', 'LOTE')
        
        PRINT 'Se insertaron 7 insumos de prueba'
    END
    ELSE
    BEGIN
        PRINT 'Ya existen insumos en la tabla'
    END
END
*/

-- 3. Consultar estructura de la tabla
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'COMPRASINDIRECTAS'
ORDER BY ORDINAL_POSITION
