-- =====================================================
-- VERIFICACIÓN DE ESTRUCTURA - PresupuestoObra
-- Sistema Calandria Residencial
-- =====================================================

PRINT '======================================================'
PRINT 'VERIFICACIÓN DE TABLA: PresupuestoObra'
PRINT '======================================================'
PRINT ''

-- 1. Verificar que la tabla existe
IF OBJECT_ID('PresupuestoObra', 'U') IS NULL
BEGIN
    PRINT '? ERROR: La tabla PresupuestoObra NO EXISTE'
    PRINT ''
    PRINT 'Ejecuta primero el script SQL_CrearEImportarPresupuesto.sql'
    RETURN
END
ELSE
BEGIN
    PRINT '? La tabla PresupuestoObra existe'
END

PRINT ''
PRINT '------------------------------------------------------'
PRINT 'COLUMNAS REQUERIDAS:'
PRINT '------------------------------------------------------'

-- 2. Verificar columnas
DECLARE @columnasRequeridas TABLE (Columna NVARCHAR(50), Existe BIT)

INSERT INTO @columnasRequeridas VALUES ('Padre', 0)
INSERT INTO @columnasRequeridas VALUES ('Etapa', 0)
INSERT INTO @columnasRequeridas VALUES ('Partida', 0)
INSERT INTO @columnasRequeridas VALUES ('CostoCalandra', 0)
INSERT INTO @columnasRequeridas VALUES ('CostoTunera', 0)
INSERT INTO @columnasRequeridas VALUES ('Prototipo', 0)

-- Actualizar columnas que existen
UPDATE @columnasRequeridas
SET Existe = 1
WHERE Columna IN (
    SELECT COLUMN_NAME 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra'
)

-- Mostrar resultado
SELECT 
    Columna,
    CASE WHEN Existe = 1 THEN '? Existe' ELSE '? Falta' END as Estado
FROM @columnasRequeridas
ORDER BY 
    CASE Columna 
        WHEN 'Padre' THEN 1
        WHEN 'Etapa' THEN 2
        WHEN 'Partida' THEN 3
        WHEN 'CostoCalandra' THEN 4
        WHEN 'CostoTunera' THEN 5
        WHEN 'Prototipo' THEN 6
        ELSE 99
    END

PRINT ''
PRINT '------------------------------------------------------'
PRINT 'REGISTROS EN LA TABLA:'
PRINT '------------------------------------------------------'

DECLARE @totalRegistros INT
DECLARE @registrosCalandra INT
DECLARE @registrosTunera INT

SELECT @totalRegistros = COUNT(*) FROM PresupuestoObra

SELECT @registrosCalandra = COUNT(*) 
FROM PresupuestoObra 
WHERE Prototipo = 'CALANDRIA'

SELECT @registrosTunera = COUNT(*) 
FROM PresupuestoObra 
WHERE Prototipo = 'TUNERA'

PRINT 'Total de registros: ' + CAST(@totalRegistros AS NVARCHAR(10))
PRINT 'Prototipo CALANDRIA: ' + CAST(@registrosCalandra AS NVARCHAR(10))
PRINT 'Prototipo TUNERA: ' + CAST(@registrosTunera AS NVARCHAR(10))

PRINT ''
PRINT '------------------------------------------------------'
PRINT 'VALORES DISTINTOS EN COLUMNAS:'
PRINT '------------------------------------------------------'

SELECT 
    'Padre' as Columna,
    COUNT(DISTINCT Padre) as ValoresDistintos
FROM PresupuestoObra
UNION ALL
SELECT 
    'Etapa' as Columna,
    COUNT(DISTINCT Etapa) as ValoresDistintos
FROM PresupuestoObra
UNION ALL
SELECT 
    'Prototipo' as Columna,
    COUNT(DISTINCT Prototipo) as ValoresDistintos
FROM PresupuestoObra

PRINT ''
PRINT '------------------------------------------------------'
PRINT 'PRIMEROS 5 REGISTROS DE EJEMPLO:'
PRINT '------------------------------------------------------'

SELECT TOP 5
    Padre,
    Etapa,
    Partida,
    CostoCalandra,
    CostoTunera,
    Prototipo
FROM PresupuestoObra
ORDER BY Padre, Etapa, Partida

PRINT ''
PRINT '------------------------------------------------------'
PRINT 'VERIFICACIÓN DE VALORES NULOS O CEROS:'
PRINT '------------------------------------------------------'

DECLARE @nullPadre INT, @nullEtapa INT, @nullPartida INT
DECLARE @zeroCal INT, @zeroTun INT

SELECT @nullPadre = COUNT(*) FROM PresupuestoObra WHERE Padre IS NULL OR Padre = ''
SELECT @nullEtapa = COUNT(*) FROM PresupuestoObra WHERE Etapa IS NULL OR Etapa = ''
SELECT @nullPartida = COUNT(*) FROM PresupuestoObra WHERE Partida IS NULL OR Partida = ''
SELECT @zeroCal = COUNT(*) FROM PresupuestoObra WHERE CostoCalandra = 0
SELECT @zeroTun = COUNT(*) FROM PresupuestoObra WHERE CostoTunera = 0

PRINT 'Registros con Padre vacío: ' + CAST(@nullPadre AS NVARCHAR(10))
PRINT 'Registros con Etapa vacía: ' + CAST(@nullEtapa AS NVARCHAR(10))
PRINT 'Registros con Partida vacía: ' + CAST(@nullPartida AS NVARCHAR(10))
PRINT 'Registros con CostoCalandra = 0: ' + CAST(@zeroCal AS NVARCHAR(10))
PRINT 'Registros con CostoTunera = 0: ' + CAST(@zeroTun AS NVARCHAR(10))

PRINT ''
PRINT '------------------------------------------------------'
PRINT 'VALIDACIÓN FINAL:'
PRINT '------------------------------------------------------'

-- Verificar si todas las columnas existen
IF NOT EXISTS (SELECT 1 FROM @columnasRequeridas WHERE Existe = 0)
BEGIN
    PRINT '? ESTRUCTURA CORRECTA: Todas las columnas requeridas existen'
    PRINT ''
    
    IF @totalRegistros > 0
    BEGIN
        PRINT '? DATOS CARGADOS: La tabla tiene ' + CAST(@totalRegistros AS NVARCHAR(10)) + ' registros'
        PRINT ''
        PRINT '?? El FormAvanceObra está listo para usarse'
    END
    ELSE
    BEGIN
        PRINT '?? ADVERTENCIA: La tabla está vacía'
        PRINT ''
        PRINT 'Ejecuta el script de importación para cargar datos'
    END
END
ELSE
BEGIN
    PRINT '? ERROR: Faltan columnas requeridas'
    PRINT ''
    PRINT 'Debes corregir la estructura de la tabla antes de continuar'
END

PRINT ''
PRINT '======================================================'
PRINT 'VERIFICACIÓN COMPLETADA'
PRINT '======================================================'
