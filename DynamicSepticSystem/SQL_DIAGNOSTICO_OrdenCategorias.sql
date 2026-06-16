-- ============================================================================
-- DIAGNÓSTICO: Verificar Orden de Categorías en FormEstimacionConceptoMigrado
-- ============================================================================
-- Ejecuta estos queries en SQL Server Management Studio y pega los resultados
-- ============================================================================

PRINT '================================================'
PRINT '1. VERIFICAR ESTRUCTURA DE Estimacion(Concepto)'
PRINT '================================================'

-- Ver columnas disponibles
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Estimacion(Concepto)'
ORDER BY ORDINAL_POSITION;

PRINT ''
PRINT '================================================'
PRINT '2. VER DATOS ACTUALES (Primeros 20 registros)'
PRINT '================================================'

-- Ver datos actuales y su orden
SELECT TOP 20 *
FROM [Estimacion(Concepto)]
ORDER BY 
    CASE 
        WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
        THEN TRY_CAST(Codigo AS INT)
        ELSE 999999 
    END,
    Codigo;

PRINT ''
PRINT '================================================'
PRINT '3. CONCEPTOS AGRUPADOS (Como aparecen en el form)'
PRINT '================================================'

-- Ver cómo se agrupan los conceptos actualmente
SELECT 
    Codigo,
    Concepto,
    COUNT(*) AS CantidadPartidas,
    SUM(CAST(ISNULL(TOTAL, 0) AS FLOAT)) AS TotalConcepto
FROM [Estimacion(Concepto)]
GROUP BY Codigo, Concepto
ORDER BY 
    CASE 
        WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
        THEN TRY_CAST(Codigo AS INT)
        ELSE 999999 
    END,
    Codigo;

PRINT ''
PRINT '================================================'
PRINT '4. VERIFICAR SI EXISTE COLUMNA Codigo'
PRINT '================================================'

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Estimacion(Concepto)' AND COLUMN_NAME = 'Codigo')
BEGIN
    PRINT '? Columna Codigo EXISTE'
    
    -- Ver valores únicos de Codigo
    SELECT DISTINCT Codigo
    FROM [Estimacion(Concepto)]
    ORDER BY 
        CASE 
            WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
            THEN TRY_CAST(Codigo AS INT)
            ELSE 999999 
        END,
        Codigo;
END
ELSE
BEGIN
    PRINT '? Columna Codigo NO EXISTE - Se necesita agregar'
    PRINT 'Ejecuta el script: SQL_AgregarCodigo_EstimacionConcepto.sql'
END

PRINT ''
PRINT '================================================'
PRINT '5. VERIFICAR PresupuestoObra'
PRINT '================================================'

-- Ver columnas de PresupuestoObra
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PresupuestoObra'
ORDER BY ORDINAL_POSITION;

PRINT ''

-- Ver datos de PresupuestoObra
SELECT TOP 20 *
FROM PresupuestoObra
ORDER BY 
    CASE 
        WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
        THEN TRY_CAST(Codigo AS INT)
        ELSE 999999 
    END,
    Codigo,
    Padre,
    Etapa;

PRINT ''
PRINT '================================================'
PRINT '6. VERIFICAR PADRE en PresupuestoObra'
PRINT '================================================'

-- Ver valores únicos de Padre (usado para mapear a conceptos)
SELECT DISTINCT Padre, COUNT(*) AS CantidadPartidas
FROM PresupuestoObra
GROUP BY Padre
ORDER BY Padre;

PRINT ''
PRINT '================================================'
PRINT '7. ORDEN ESPERADO (1-12)'
PRINT '================================================'

-- Mostrar el orden que DEBERÍA aparecer
WITH OrdenEsperado AS (
    SELECT 1 AS Orden, 'Preliminares' AS NombreConcepto UNION ALL
    SELECT 2, 'Cimentación' UNION ALL
    SELECT 3, 'Estructura' UNION ALL
    SELECT 4, 'Ins. Hidraulica, Sanitaria y Gas LP' UNION ALL
    SELECT 5, 'Inst. Eléctrica' UNION ALL
    SELECT 6, 'Albañilería' UNION ALL
    SELECT 7, 'Acabados' UNION ALL
    SELECT 8, 'Herrería, Aluminio y Vidrio' UNION ALL
    SELECT 9, 'Carpintería y Cerrajería' UNION ALL
    SELECT 10, 'Muebles y Accesorios' UNION ALL
    SELECT 11, 'Inst especiales y Obra Exterior' UNION ALL
    SELECT 12, 'Urbanización'
)
SELECT 
    oe.Orden,
    oe.NombreConcepto AS ConceptoEsperado,
    ec.Codigo AS CodigoActual,
    ec.Concepto AS ConceptoActual,
    CASE 
        WHEN ec.Concepto IS NULL THEN '? NO EXISTE en tabla'
        WHEN ec.Codigo IS NULL THEN '? Sin código asignado'
        WHEN CAST(ec.Codigo AS INT) = oe.Orden THEN '? Código correcto'
        ELSE '? Código incorrecto: ' + ec.Codigo
    END AS Estado
FROM OrdenEsperado oe
LEFT JOIN (
    SELECT DISTINCT Codigo, Concepto
    FROM [Estimacion(Concepto)]
) ec ON REPLACE(REPLACE(REPLACE(LOWER(ec.Concepto), 'á', 'a'), 'é', 'e'), 'í', 'i') 
        LIKE '%' + REPLACE(REPLACE(REPLACE(LOWER(oe.NombreConcepto), 'á', 'a'), 'é', 'e'), 'í', 'i') + '%'
     OR REPLACE(REPLACE(REPLACE(LOWER(oe.NombreConcepto), 'á', 'a'), 'é', 'e'), 'í', 'i') 
        LIKE '%' + REPLACE(REPLACE(REPLACE(LOWER(ec.Concepto), 'á', 'a'), 'é', 'e'), 'í', 'i') + '%'
ORDER BY oe.Orden;

PRINT ''
PRINT '================================================'
PRINT 'DIAGNÓSTICO COMPLETADO'
PRINT '================================================'
PRINT 'Revisa los resultados y comparte:'
PRINT '- Sección 3: Conceptos agrupados'
PRINT '- Sección 4: Estado de columna Codigo'
PRINT '- Sección 7: Orden esperado vs actual'
PRINT '================================================'
