-- =====================================================
-- VERIFICACIÓN RÁPIDA - PresupuestoObra
-- =====================================================

PRINT '?? VERIFICACIÓN RÁPIDA DEL SISTEMA'
PRINT '======================================'
PRINT ''

-- 1. ¿Existe la tabla?
IF OBJECT_ID('PresupuestoObra', 'U') IS NOT NULL
    PRINT '? Tabla PresupuestoObra existe'
ELSE
BEGIN
    PRINT '? ERROR: Tabla PresupuestoObra NO existe'
    RETURN
END

-- 2. ¿Tiene las columnas correctas?
DECLARE @tienePadre BIT = 0
DECLARE @tieneCostoCalandra BIT = 0
DECLARE @tieneCostoTunera BIT = 0
DECLARE @tienePrototipo BIT = 0

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'Padre')
    SET @tienePadre = 1
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'CostoCalandra')
    SET @tieneCostoCalandra = 1
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'CostoTunera')
    SET @tieneCostoTunera = 1
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'Prototipo')
    SET @tienePrototipo = 1

IF @tienePadre = 1
    PRINT '? Columna Padre existe'
ELSE
    PRINT '? Falta columna Padre'

IF @tieneCostoCalandra = 1
    PRINT '? Columna CostoCalandra existe'
ELSE
    PRINT '? Falta columna CostoCalandra'

IF @tieneCostoTunera = 1
    PRINT '? Columna CostoTunera existe'
ELSE
    PRINT '? Falta columna CostoTunera'

IF @tienePrototipo = 0
    PRINT '? NO tiene columna Prototipo (correcto)'
ELSE
    PRINT '?? Tiene columna Prototipo (no necesaria pero no afecta)'

-- 3. ¿Tiene datos?
DECLARE @totalRegistros INT
SELECT @totalRegistros = COUNT(*) FROM PresupuestoObra

PRINT ''
PRINT 'Total de registros: ' + CAST(@totalRegistros AS VARCHAR(10))

IF @totalRegistros > 0
    PRINT '? Tiene datos cargados'
ELSE
    PRINT '?? Tabla vacía - necesitas cargar datos'

-- 4. Primeros registros
PRINT ''
PRINT '?? PRIMEROS 5 REGISTROS:'
PRINT '======================================'

SELECT TOP 5
    Padre,
    Etapa,
    LEFT(Partida, 30) as Partida,
    CostoCalandra,
    CostoTunera
FROM PresupuestoObra
ORDER BY Padre, Etapa

-- 5. Verificar prototipos en InventarioCasas
PRINT ''
PRINT '?? PROTOTIPOS EN INVENTARIO:'
PRINT '======================================'

IF OBJECT_ID('InventarioCasas', 'U') IS NOT NULL
BEGIN
    SELECT DISTINCT Prototipo, COUNT(*) as Casas
    FROM InventarioCasas
    GROUP BY Prototipo
    
    PRINT ''
    PRINT '? Tabla InventarioCasas existe'
END
ELSE
BEGIN
    PRINT '?? Tabla InventarioCasas no encontrada'
END

-- 6. Resumen final
PRINT ''
PRINT '======================================'
PRINT '?? RESUMEN FINAL:'
PRINT '======================================'

IF @tienePadre = 1 AND @tieneCostoCalandra = 1 AND @tieneCostoTunera = 1 AND @totalRegistros > 0
BEGIN
    PRINT '? SISTEMA LISTO PARA USAR'
    PRINT ''
    PRINT 'El FormAvanceObra funcionará correctamente:'
    PRINT '  • Tiene estructura correcta'
    PRINT '  • Tiene datos cargados'
    PRINT '  • NO necesita columna Prototipo'
    PRINT '  • Usa CostoCalandra para CALANDRIA'
    PRINT '  • Usa CostoTunera para TUNERA'
END
ELSE
BEGIN
    PRINT '?? REVISAR SISTEMA'
    PRINT ''
    IF @tienePadre = 0 OR @tieneCostoCalandra = 0 OR @tieneCostoTunera = 0
        PRINT '  • Faltan columnas requeridas'
    IF @totalRegistros = 0
        PRINT '  • Necesitas cargar datos en la tabla'
END

PRINT ''
PRINT '======================================'
