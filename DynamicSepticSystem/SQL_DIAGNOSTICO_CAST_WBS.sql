-- ?? DIAGNÓSTICO ESPECÍFICO: Problema con CAST(WBS AS INT)
-- Base de datos: CALANDRIA

USE CALANDRIA;
GO

PRINT '==================================================';
PRINT '?? DIAGNÓSTICO: ¿Por qué no se calcula el progreso?';
PRINT '==================================================';
PRINT '';

-- 1?? Ver el tipo de datos de la columna WBS
PRINT '1?? Tipo de datos de la columna WBS:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AvanceManualObra'
  AND COLUMN_NAME = 'WBS';
PRINT '';

-- 2?? Ver los valores RAW de WBS para M5-L2
PRINT '2?? Valores RAW de WBS para M5-L2:';
SELECT 
    WBS,
    Concepto,
    AvancePorcentaje,
    LEN(WBS) AS 'Longitud WBS',
    ASCII(LEFT(WBS, 1)) AS 'Primer caracter ASCII'
FROM dbo.AvanceManualObra
WHERE Manzana = '5' AND Lote = '2'
ORDER BY WBS;
PRINT '';

-- 3?? Probar CAST(WBS AS INT) directamente
PRINT '3?? Probar conversión CAST(WBS AS INT):';
SELECT 
    WBS,
    Concepto,
    TRY_CAST(WBS AS INT) AS 'WBS_Como_Int',
    CASE 
        WHEN TRY_CAST(WBS AS INT) IS NULL THEN '? Fallo conversión'
        WHEN TRY_CAST(WBS AS INT) < 0 THEN '? Negativo (CONCEPTO)'
        WHEN TRY_CAST(WBS AS INT) > 0 THEN '? Positivo (PARTIDA)'
        ELSE '?? Cero o nulo'
    END AS 'Estado Conversión'
FROM dbo.AvanceManualObra
WHERE Manzana = '5' AND Lote = '2'
ORDER BY TRY_CAST(WBS AS INT);
PRINT '';

-- 4?? Ejecutar la MISMA consulta que usa PanelPrincipal
PRINT '4?? CONSULTA EXACTA que usa PanelPrincipal.CargarResumenProgreso():';
PRINT '';

-- Consulta EXACTA copiada del código de PanelPrincipal
SELECT 
    COUNT(CASE WHEN AvancePorcentaje = 100 THEN 1 END) AS ConceptosCompletados,
    COUNT(*) AS TotalRegistrados
FROM dbo.AvanceManualObra
WHERE Manzana = '5' 
  AND Lote = '2'
  AND CAST(WBS AS INT) < 0;

PRINT '';
PRINT '?? Interpretación de resultados:';
PRINT '   - Si ConceptosCompletados = 0 ? El problema está en la consulta SQL';
PRINT '   - Si ConceptosCompletados = 2 ? El problema está en el código C# que actualiza la UI';
PRINT '';

-- 5?? Consulta alternativa SIN CAST (más segura)
PRINT '5?? Consulta ALTERNATIVA (sin CAST, comparando string):';
SELECT 
    COUNT(CASE WHEN AvancePorcentaje = 100 THEN 1 END) AS ConceptosCompletados,
    COUNT(*) AS TotalRegistrados
FROM dbo.AvanceManualObra
WHERE Manzana = '5' 
  AND Lote = '2'
  AND WBS LIKE '-%';  -- Busca strings que empiezan con '-'

PRINT '';

-- 6?? Listar TODOS los conceptos (WBS negativos) independientemente del método
PRINT '6?? Todos los conceptos (varias técnicas de filtrado):';
PRINT '';
PRINT 'Método 1: CAST(WBS AS INT) < 0';
SELECT WBS, Concepto, AvancePorcentaje
FROM dbo.AvanceManualObra
WHERE Manzana = '5' AND Lote = '2'
  AND CAST(WBS AS INT) < 0;

PRINT '';
PRINT 'Método 2: TRY_CAST(WBS AS INT) < 0';
SELECT WBS, Concepto, AvancePorcentaje
FROM dbo.AvanceManualObra
WHERE Manzana = '5' AND Lote = '2'
  AND TRY_CAST(WBS AS INT) < 0;

PRINT '';
PRINT 'Método 3: WBS LIKE ''-%''';
SELECT WBS, Concepto, AvancePorcentaje
FROM dbo.AvanceManualObra
WHERE Manzana = '5' AND Lote = '2'
  AND WBS LIKE '-%';

PRINT '';
PRINT '==================================================';
PRINT '? Diagnóstico completado';
PRINT '==================================================';
