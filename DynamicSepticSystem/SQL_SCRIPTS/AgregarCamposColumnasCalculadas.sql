/*
==============================================================================
SCRIPT: Agregar Campos para Columnas Calculadas
==============================================================================
Descripción: Agrega los campos necesarios para soportar columnas calculadas
            en las tablas de definición de columnas
Autor: Sistema Calandria Residencial
Fecha: Enero 2025
Versión: 1.0
==============================================================================
*/

USE [BaseDatosCalandria]
GO

PRINT ''
PRINT '============================================================================'
PRINT '         AGREGANDO CAMPOS PARA COLUMNAS CALCULADAS'
PRINT '============================================================================'
PRINT ''

-- ============================================================================
-- 1. ACTUALIZAR RutaTuneraDestajo_ColumnasDefinicion
-- ============================================================================

PRINT 'Actualizando tabla RutaTuneraDestajo_ColumnasDefinicion...'

-- Agregar campo EsCalculada
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RutaTuneraDestajo_ColumnasDefinicion') AND name = 'EsCalculada')
BEGIN
    ALTER TABLE RutaTuneraDestajo_ColumnasDefinicion
    ADD EsCalculada BIT NOT NULL DEFAULT 0
    PRINT '? Campo EsCalculada agregado'
END
ELSE
BEGIN
    PRINT '? Campo EsCalculada ya existe'
END
GO

-- Agregar campo TipoOperacion
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RutaTuneraDestajo_ColumnasDefinicion') AND name = 'TipoOperacion')
BEGIN
    ALTER TABLE RutaTuneraDestajo_ColumnasDefinicion
    ADD TipoOperacion INT NOT NULL DEFAULT 0
    PRINT '? Campo TipoOperacion agregado'
END
ELSE
BEGIN
    PRINT '? Campo TipoOperacion ya existe'
END
GO

-- Agregar campo ColumnaOrigen1
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RutaTuneraDestajo_ColumnasDefinicion') AND name = 'ColumnaOrigen1')
BEGIN
    ALTER TABLE RutaTuneraDestajo_ColumnasDefinicion
    ADD ColumnaOrigen1 NVARCHAR(100) NULL
    PRINT '? Campo ColumnaOrigen1 agregado'
END
ELSE
BEGIN
    PRINT '? Campo ColumnaOrigen1 ya existe'
END
GO

-- Agregar campo ColumnaOrigen2
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RutaTuneraDestajo_ColumnasDefinicion') AND name = 'ColumnaOrigen2')
BEGIN
    ALTER TABLE RutaTuneraDestajo_ColumnasDefinicion
    ADD ColumnaOrigen2 NVARCHAR(100) NULL
    PRINT '? Campo ColumnaOrigen2 agregado'
END
ELSE
BEGIN
    PRINT '? Campo ColumnaOrigen2 ya existe'
END
GO

-- ============================================================================
-- 2. ACTUALIZAR RutaCalandraDestajo_ColumnasDefinicion
-- ============================================================================

PRINT ''
PRINT 'Actualizando tabla RutaCalandraDestajo_ColumnasDefinicion...'

-- Agregar campo EsCalculada
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RutaCalandraDestajo_ColumnasDefinicion') AND name = 'EsCalculada')
BEGIN
    ALTER TABLE RutaCalandraDestajo_ColumnasDefinicion
    ADD EsCalculada BIT NOT NULL DEFAULT 0
    PRINT '? Campo EsCalculada agregado'
END
ELSE
BEGIN
    PRINT '? Campo EsCalculada ya existe'
END
GO

-- Agregar campo TipoOperacion
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RutaCalandraDestajo_ColumnasDefinicion') AND name = 'TipoOperacion')
BEGIN
    ALTER TABLE RutaCalandraDestajo_ColumnasDefinicion
    ADD TipoOperacion INT NOT NULL DEFAULT 0
    PRINT '? Campo TipoOperacion agregado'
END
ELSE
BEGIN
    PRINT '? Campo TipoOperacion ya existe'
END
GO

-- Agregar campo ColumnaOrigen1
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RutaCalandraDestajo_ColumnasDefinicion') AND name = 'ColumnaOrigen1')
BEGIN
    ALTER TABLE RutaCalandraDestajo_ColumnasDefinicion
    ADD ColumnaOrigen1 NVARCHAR(100) NULL
    PRINT '? Campo ColumnaOrigen1 agregado'
END
ELSE
BEGIN
    PRINT '? Campo ColumnaOrigen1 ya existe'
END
GO

-- Agregar campo ColumnaOrigen2
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RutaCalandraDestajo_ColumnasDefinicion') AND name = 'ColumnaOrigen2')
BEGIN
    ALTER TABLE RutaCalandraDestajo_ColumnasDefinicion
    ADD ColumnaOrigen2 NVARCHAR(100) NULL
    PRINT '? Campo ColumnaOrigen2 agregado'
END
ELSE
BEGIN
    PRINT '? Campo ColumnaOrigen2 ya existe'
END
GO

-- ============================================================================
-- 3. INSERTAR COLUMNAS DE EJEMPLO
-- ============================================================================

PRINT ''
PRINT '============================================================================'
PRINT '         INSERTANDO COLUMNAS DE EJEMPLO'
PRINT '============================================================================'
PRINT ''

-- Ejemplo para RutaTuneraDestajo
IF NOT EXISTS (SELECT * FROM RutaTuneraDestajo_ColumnasDefinicion WHERE Nombre = 'Precio')
BEGIN
    INSERT INTO RutaTuneraDestajo_ColumnasDefinicion (Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato, EsCalculada)
    VALUES ('Precio', 'Precio Unitario', 120, 'System.Decimal', 1, '{0:C2}', 0)
    PRINT '? Columna Precio agregada'
END

IF NOT EXISTS (SELECT * FROM RutaTuneraDestajo_ColumnasDefinicion WHERE Nombre = 'Cantidad')
BEGIN
    INSERT INTO RutaTuneraDestajo_ColumnasDefinicion (Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato, EsCalculada)
    VALUES ('Cantidad', 'Cantidad', 100, 'System.Decimal', 1, '{0:N2}', 0)
    PRINT '? Columna Cantidad agregada'
END

IF NOT EXISTS (SELECT * FROM RutaTuneraDestajo_ColumnasDefinicion WHERE Nombre = 'Total')
BEGIN
    INSERT INTO RutaTuneraDestajo_ColumnasDefinicion 
        (Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato, EsCalculada, TipoOperacion, ColumnaOrigen1, ColumnaOrigen2)
    VALUES 
        ('Total', 'Total', 130, 'System.Decimal', 0, '{0:C2}', 1, 1, 'Precio', 'Cantidad')
    PRINT '? Columna calculada Total (Precio × Cantidad) agregada'
END

-- Ejemplo para RutaCalandraDestajo
IF NOT EXISTS (SELECT * FROM RutaCalandraDestajo_ColumnasDefinicion WHERE Nombre = 'Precio')
BEGIN
    INSERT INTO RutaCalandraDestajo_ColumnasDefinicion (Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato, EsCalculada)
    VALUES ('Precio', 'Precio Unitario', 120, 'System.Decimal', 1, '{0:C2}', 0)
    PRINT '? Columna Precio agregada'
END

IF NOT EXISTS (SELECT * FROM RutaCalandraDestajo_ColumnasDefinicion WHERE Nombre = 'Cantidad')
BEGIN
    INSERT INTO RutaCalandraDestajo_ColumnasDefinicion (Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato, EsCalculada)
    VALUES ('Cantidad', 'Cantidad', 100, 'System.Decimal', 1, '{0:N2}', 0)
    PRINT '? Columna Cantidad agregada'
END

IF NOT EXISTS (SELECT * FROM RutaCalandraDestajo_ColumnasDefinicion WHERE Nombre = 'Total')
BEGIN
    INSERT INTO RutaCalandraDestajo_ColumnasDefinicion 
        (Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato, EsCalculada, TipoOperacion, ColumnaOrigen1, ColumnaOrigen2)
    VALUES 
        ('Total', 'Total', 130, 'System.Decimal', 0, '{0:C2}', 1, 1, 'Precio', 'Cantidad')
    PRINT '? Columna calculada Total (Precio × Cantidad) agregada'
END

-- ============================================================================
-- 4. VERIFICACIÓN FINAL
-- ============================================================================

PRINT ''
PRINT '============================================================================'
PRINT '                    VERIFICACIÓN DE ACTUALIZACIÓN'
PRINT '============================================================================'
PRINT ''

DECLARE @CountTunera int
DECLARE @CountCalandra int

SELECT @CountTunera = COUNT(*) FROM RutaTuneraDestajo_ColumnasDefinicion
SELECT @CountCalandra = COUNT(*) FROM RutaCalandraDestajo_ColumnasDefinicion

PRINT '?? Columnas definidas actuales:'
PRINT '   - RutaTuneraDestajo: ' + CAST(@CountTunera AS nvarchar(10))
PRINT '   - RutaCalandraDestajo: ' + CAST(@CountCalandra AS nvarchar(10))
PRINT ''

-- Verificar campos agregados
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RutaTuneraDestajo_ColumnasDefinicion') AND name = 'EsCalculada')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RutaCalandraDestajo_ColumnasDefinicion') AND name = 'EsCalculada')
BEGIN
    PRINT '? ACTUALIZACIÓN COMPLETADA EXITOSAMENTE'
    PRINT ''
    PRINT '?? Nuevos campos agregados:'
    PRINT '  - EsCalculada (BIT): Indica si la columna es calculada'
    PRINT '  - TipoOperacion (INT): Tipo de operación (0=Ninguna, 1=Multiplicación, 2=Suma, 3=Resta, 4=División)'
    PRINT '  - ColumnaOrigen1 (NVARCHAR): Primera columna para el cálculo'
    PRINT '  - ColumnaOrigen2 (NVARCHAR): Segunda columna para el cálculo'
    PRINT ''
    PRINT '?? Ejemplo de uso:'
    PRINT '   Crear una columna "Total" que multiplique "Precio" × "Cantidad"'
    PRINT '   1. Crear columna "Precio" (tipo Decimal, editable)'
    PRINT '   2. Crear columna "Cantidad" (tipo Decimal, editable)'
    PRINT '   3. Crear columna "Total" marcada como calculada'
    PRINT '      - Tipo operación: Multiplicación'
    PRINT '      - Columna 1: Precio'
    PRINT '      - Columna 2: Cantidad'
END
ELSE
BEGIN
    PRINT '? ERROR: No todos los campos fueron agregados correctamente'
END

PRINT '============================================================================'
GO

/*
==============================================================================
NOTAS DE USO:
==============================================================================

1. Tipos de Operación disponibles:
   - 0: Ninguna (columna normal)
   - 1: Multiplicación (×)
   - 2: Suma (+)
   - 3: Resta (-)
   - 4: División (÷)

2. Ejemplo de INSERT manual:

   -- Columna calculada que multiplica Precio × Cantidad
   INSERT INTO RutaTuneraDestajo_ColumnasDefinicion 
   (Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato, 
    EsCalculada, TipoOperacion, ColumnaOrigen1, ColumnaOrigen2)
   VALUES 
   ('Total', 'Total', 130, 'System.Decimal', 0, '{0:C2}', 
    1, 1, 'Precio', 'Cantidad')

3. Las columnas calculadas:
   - NO son editables (se actualizan automáticamente)
   - Solo soportan operaciones entre columnas numéricas
   - Se recalculan cuando cambian las columnas de origen
   - Siempre son de tipo Decimal

==============================================================================
*/
