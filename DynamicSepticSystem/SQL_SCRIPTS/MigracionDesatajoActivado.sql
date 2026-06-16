-- Script de Migración: Agregar estado DesatajoActivado a ActivacionTareasRuta
-- Fecha: 2024
-- Descripción: Agrega columna DesatajoActivado para registrar si un destajo está 
--              completamente activado (con cuadrilla asignada y listo para PDF)

-- Verificar si la tabla existe y si la columna no existe, crearla
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ActivacionTareasRuta')
BEGIN
    -- Crear tabla si no existe (incluye DesatajoActivado)
    CREATE TABLE ActivacionTareasRuta (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Manzana NVARCHAR(10),
        Lote NVARCHAR(10),
        Prototipo NVARCHAR(50),
        Ruta NVARCHAR(50),
        NodoID INT,
        NombreTarea NVARCHAR(200),
        Activa BIT,
        CuadrillaAsignada NVARCHAR(20) NULL,
        DesatajoActivado BIT DEFAULT 0,
        FechaActualizacion DATETIME DEFAULT GETDATE()
    );
    PRINT 'Tabla ActivacionTareasRuta creada con éxito.';
END
ELSE
BEGIN
    -- Tabla existe, verificar si CuadrillaAsignada existe
    IF NOT EXISTS (
        SELECT * FROM sys.columns
        WHERE Name = N'CuadrillaAsignada'
          AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
    BEGIN
        ALTER TABLE ActivacionTareasRuta 
        ADD CuadrillaAsignada NVARCHAR(20) NULL;
        PRINT 'Columna CuadrillaAsignada agregada.';
    END
    
    -- Verificar si DesatajoActivado existe, si no, crearla
    IF NOT EXISTS (
        SELECT * FROM sys.columns
        WHERE Name = N'DesatajoActivado'
          AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
    BEGIN
        ALTER TABLE ActivacionTareasRuta 
        ADD DesatajoActivado BIT DEFAULT 0;
        PRINT 'Columna DesatajoActivado agregada con éxito.';
    END
    ELSE
    BEGIN
        PRINT 'Columna DesatajoActivado ya existe.';
    END
END

-- Verificar estructura final
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ActivacionTareasRuta'
ORDER BY ORDINAL_POSITION;
