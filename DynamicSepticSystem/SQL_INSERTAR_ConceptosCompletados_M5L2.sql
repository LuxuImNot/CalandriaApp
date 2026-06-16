-- ?? SOLUCIÓN: Insertar Conceptos Completados con WBS Negativos
-- Base de datos: CALANDRIA
-- Casa: M5-L2

USE CALANDRIA;
GO

-- ============================================
-- PASO 1: Calcular montos de conceptos completados
-- ============================================

PRINT '?? Calculando montos de conceptos completados...';

-- Preliminares: WBS 1, 2, 3 (todas al 100%)
DECLARE @MontoPreliminares FLOAT;
SELECT @MontoPreliminares = SUM(MontoEjecutado)
FROM dbo.AvanceManualObra
WHERE Manzana = '5' AND Lote = '2'
  AND WBS IN ('1', '2', '3')
  AND AvancePorcentaje = 100;

PRINT '   Preliminares (WBS 1-3): $' + CAST(@MontoPreliminares AS VARCHAR(20));

-- Cimentación: WBS 4-11 (todas al 100%)
DECLARE @MontoCimentacion FLOAT;
SELECT @MontoCimentacion = SUM(MontoEjecutado)
FROM dbo.AvanceManualObra
WHERE Manzana = '5' AND Lote = '2'
  AND WBS IN ('4', '5', '6', '7', '8', '9', '10', '11')
  AND AvancePorcentaje = 100;

PRINT '   Cimentación (WBS 4-11): $' + CAST(@MontoCimentacion AS VARCHAR(20));

-- ============================================
-- PASO 2: Insertar conceptos con WBS negativos
-- ============================================

PRINT '';
PRINT '?? Insertando conceptos completados...';

-- Insertar Preliminares (WBS = -1)
IF NOT EXISTS (SELECT 1 FROM dbo.AvanceManualObra WHERE Manzana = '5' AND Lote = '2' AND WBS = '-1')
BEGIN
    INSERT INTO dbo.AvanceManualObra (
        Manzana, 
        Lote, 
        Prototipo, 
        WBS, 
        Concepto, 
        AvancePorcentaje, 
        MontoEjecutado, 
        FechaFinalizacion,
        FechaActualizacion
    )
    VALUES (
        '5',                    -- Manzana
        '2',                    -- Lote
        'TUNERA',              -- Prototipo
        '-1',                  -- WBS negativo para concepto
        'Preliminares',        -- Nombre del concepto
        100.0,                 -- 100% completado
        @MontoPreliminares,    -- Monto calculado
        GETDATE(),             -- Fecha de finalización
        GETDATE()              -- Fecha de actualización
    );
    PRINT '   ? Concepto "Preliminares" insertado (WBS = -1, Monto = $' + CAST(@MontoPreliminares AS VARCHAR(20)) + ')';
END
ELSE
BEGIN
    PRINT '   ?? Concepto "Preliminares" ya existe (WBS = -1)';
END

-- Insertar Cimentación (WBS = -2)
IF NOT EXISTS (SELECT 1 FROM dbo.AvanceManualObra WHERE Manzana = '5' AND Lote = '2' AND WBS = '-2')
BEGIN
    INSERT INTO dbo.AvanceManualObra (
        Manzana, 
        Lote, 
        Prototipo, 
        WBS, 
        Concepto, 
        AvancePorcentaje, 
        MontoEjecutado, 
        FechaFinalizacion,
        FechaActualizacion
    )
    VALUES (
        '5',                    -- Manzana
        '2',                    -- Lote
        'TUNERA',              -- Prototipo
        '-2',                  -- WBS negativo para concepto
        'Cimentación',         -- Nombre del concepto
        100.0,                 -- 100% completado
        @MontoCimentacion,     -- Monto calculado
        GETDATE(),             -- Fecha de finalización
        GETDATE()              -- Fecha de actualización
    );
    PRINT '   ? Concepto "Cimentación" insertado (WBS = -2, Monto = $' + CAST(@MontoCimentacion AS VARCHAR(20)) + ')';
END
ELSE
BEGIN
    PRINT '   ?? Concepto "Cimentación" ya existe (WBS = -2)';
END

-- ============================================
-- PASO 3: Verificar resultados
-- ============================================

PRINT '';
PRINT '?? Verificando conceptos insertados...';
PRINT '';

SELECT 
    WBS AS 'WBS Negativo',
    Concepto AS 'Nombre Concepto',
    AvancePorcentaje AS 'Avance %',
    MontoEjecutado AS 'Monto Ejecutado',
    FechaFinalizacion AS 'Fecha Finalización',
    FechaActualizacion AS 'Fecha Actualización'
FROM dbo.AvanceManualObra
WHERE Manzana = '5' 
  AND Lote = '2'
  AND TRY_CAST(WBS AS INT) < 0
ORDER BY CAST(WBS AS INT);

-- ============================================
-- PASO 4: Calcular progreso para PanelPrincipal
-- ============================================

PRINT '';
PRINT '?? Cálculo de progreso para PanelPrincipal:';
PRINT '';

SELECT 
    COUNT(CASE WHEN AvancePorcentaje = 100 THEN 1 END) AS 'Conceptos Completados',
    12 AS 'Total Conceptos',
    CAST(COUNT(CASE WHEN AvancePorcentaje = 100 THEN 1 END) AS FLOAT) / 12 * 100 AS 'Porcentaje Progreso'
FROM dbo.AvanceManualObra
WHERE Manzana = '5' 
  AND Lote = '2'
  AND TRY_CAST(WBS AS INT) < 0;

PRINT '';
PRINT '? Proceso completado. Ahora recarga la casa M5-L2 en PanelPrincipal.';
PRINT '';
