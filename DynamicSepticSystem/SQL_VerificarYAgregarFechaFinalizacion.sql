-- SCRIPT: Verificar y Agregar columna FechaFinalizacion a AvanceManualObra

-- 1. Verificar si existe la tabla
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AvanceManualObra')
BEGIN
    PRINT 'Tabla AvanceManualObra existe'
    
    -- 2. Verificar si existe la columna FechaFinalizacion
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'AvanceManualObra' 
                   AND COLUMN_NAME = 'FechaFinalizacion')
    BEGIN
        PRINT 'Columna FechaFinalizacion NO existe - Agregándola...'
        
        ALTER TABLE AvanceManualObra
        ADD FechaFinalizacion DATETIME NULL;
        
        PRINT 'Columna FechaFinalizacion agregada exitosamente'
    END
    ELSE
    BEGIN
        PRINT 'Columna FechaFinalizacion YA EXISTE'
    END
    
    -- 3. Mostrar estructura de la tabla
    SELECT 
        COLUMN_NAME AS Columna,
        DATA_TYPE AS Tipo,
        IS_NULLABLE AS Permite_NULL,
        COLUMN_DEFAULT AS Valor_Defecto
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AvanceManualObra'
    ORDER BY ORDINAL_POSITION;
    
    -- 4. Mostrar registros con fecha de finalización
    SELECT 
        COUNT(*) AS TotalRegistros,
        COUNT(FechaFinalizacion) AS RegistrosConFecha,
        COUNT(*) - COUNT(FechaFinalizacion) AS RegistrosSinFecha
    FROM AvanceManualObra;
    
    -- 5. Mostrar algunos registros para verificar
    SELECT TOP 10
        Manzana,
        Lote,
        WBS,
        AvancePorcentaje,
        MontoEjecutado,
        FechaActualizacion,
        FechaFinalizacion
    FROM AvanceManualObra
    WHERE AvancePorcentaje = 100
    ORDER BY FechaActualizacion DESC;
    
END
ELSE
BEGIN
    PRINT 'ERROR: La tabla AvanceManualObra NO EXISTE'
    PRINT 'Ejecuta primero el script de creación de la tabla'
END

GO
