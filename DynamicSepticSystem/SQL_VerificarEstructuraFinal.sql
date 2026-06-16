-- =====================================================
-- VERIFICACIÓN FINAL - Estructura PresupuestoObra
-- Sistema Calandria Residencial
-- =====================================================

PRINT '======================================================'
PRINT 'VERIFICACIÓN ESTRUCTURA CORRECTA'
PRINT '======================================================'
PRINT ''

-- 1. Verificar que la tabla existe
IF OBJECT_ID('PresupuestoObra', 'U') IS NULL
BEGIN
    PRINT '? ERROR: La tabla PresupuestoObra NO EXISTE'
    RETURN
END

PRINT '? La tabla PresupuestoObra existe'
PRINT ''

-- 2. Mostrar estructura real de la tabla
PRINT '------------------------------------------------------'
PRINT 'COLUMNAS ACTUALES EN LA TABLA:'
PRINT '------------------------------------------------------'

SELECT 
    COLUMN_NAME as Columna,
    DATA_TYPE as Tipo,
    CHARACTER_MAXIMUM_LENGTH as Longitud,
    IS_NULLABLE as Nulable
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PresupuestoObra'
ORDER BY ORDINAL_POSITION

PRINT ''
PRINT '------------------------------------------------------'
PRINT 'VERIFICACIÓN DE COLUMNAS REQUERIDAS:'
PRINT '------------------------------------------------------'

DECLARE @tienePadre BIT = 0
DECLARE @tieneEtapa BIT = 0
DECLARE @tienePartida BIT = 0
DECLARE @tieneCostoCalandra BIT = 0
DECLARE @tieneCostoTunera BIT = 0
DECLARE @tienePrototipo BIT = 0

SELECT @tienePadre = 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'Padre'
SELECT @tieneEtapa = 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'Etapa'
SELECT @tienePartida = 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'Partida'
SELECT @tieneCostoCalandra = 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'CostoCalandra'
SELECT @tieneCostoTunera = 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'CostoTunera'
SELECT @tienePrototipo = 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'Prototipo'

PRINT 'Padre: ' + CASE WHEN @tienePadre = 1 THEN '? Existe' ELSE '? Falta' END
PRINT 'Etapa: ' + CASE WHEN @tieneEtapa = 1 THEN '? Existe' ELSE '? Falta' END
PRINT 'Partida: ' + CASE WHEN @tienePartida = 1 THEN '? Existe' ELSE '? Falta' END
PRINT 'CostoCalandra: ' + CASE WHEN @tieneCostoCalandra = 1 THEN '? Existe' ELSE '? Falta' END
PRINT 'CostoTunera: ' + CASE WHEN @tieneCostoTunera = 1 THEN '? Existe' ELSE '? Falta' END
PRINT 'Prototipo: ' + CASE WHEN @tienePrototipo = 1 THEN '?? Existe (NO requerida)' ELSE '? NO existe (correcto)' END

PRINT ''
PRINT '------------------------------------------------------'
PRINT 'DATOS EN LA TABLA:'
PRINT '------------------------------------------------------'

DECLARE @totalRegistros INT
SELECT @totalRegistros = COUNT(*) FROM PresupuestoObra

PRINT 'Total de registros: ' + CAST(@totalRegistros AS NVARCHAR(10))

PRINT ''
PRINT '------------------------------------------------------'
PRINT 'PRIMEROS 5 REGISTROS:'
PRINT '------------------------------------------------------'

SELECT TOP 5
    Padre,
    Etapa,
    Partida,
    CostoCalandra,
    CostoTunera
FROM PresupuestoObra
ORDER BY Padre, Etapa, Partida

PRINT ''
PRINT '------------------------------------------------------'
PRINT 'RESUMEN POR PADRE:'
PRINT '------------------------------------------------------'

SELECT 
    Padre,
    COUNT(*) as Cantidad,
    SUM(CostoCalandra) as TotalCalandra,
    SUM(CostoTunera) as TotalTunera
FROM PresupuestoObra
GROUP BY Padre
ORDER BY Padre

PRINT ''
PRINT '------------------------------------------------------'
PRINT 'VALIDACIÓN FINAL:'
PRINT '------------------------------------------------------'

IF @tienePadre = 1 AND @tieneEtapa = 1 AND @tienePartida = 1 
   AND @tieneCostoCalandra = 1 AND @tieneCostoTunera = 1
BEGIN
    IF @totalRegistros > 0
    BEGIN
        PRINT '? ESTRUCTURA CORRECTA Y DATOS CARGADOS'
        PRINT ''
        PRINT '?? El FormAvanceObra está listo para usarse'
        PRINT ''
        PRINT '?? IMPORTANTE:'
        PRINT '   • La tabla NO tiene columna Prototipo (correcto)'
        PRINT '   • Todos los registros están en una sola tabla'
        PRINT '   • Se usa CostoCalandra para casas CALANDRIA'
        PRINT '   • Se usa CostoTunera para casas TUNERA'
    END
    ELSE
    BEGIN
        PRINT '?? ESTRUCTURA CORRECTA PERO SIN DATOS'
        PRINT ''
        PRINT 'Ejecuta el script de importación para cargar datos'
    END
END
ELSE
BEGIN
    PRINT '? ERROR: Faltan columnas requeridas'
    PRINT ''
    PRINT 'Verifica la estructura de la tabla'
END

PRINT ''
PRINT '======================================================'
PRINT 'VERIFICACIÓN COMPLETADA'
PRINT '======================================================'
