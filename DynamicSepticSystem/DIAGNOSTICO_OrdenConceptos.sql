-- ? SCRIPT DE DIAGNÓSTICO: Verificar Orden de Conceptos
-- Ejecuta esto en SQL Server para ver el orden actual

USE CALANDRIA;
GO

-- 1. Ver si existe la columna Codigo en Estimacion(Concepto)
SELECT 
    TABLE_NAME, 
    COLUMN_NAME, 
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Estimacion(Concepto)' 
  AND COLUMN_NAME = 'Codigo';

-- 2. Ver los conceptos actuales y su orden
SELECT 
    Codigo,
    Concepto,
    COUNT(*) AS NumPartidas
FROM [Estimacion(Concepto)]
GROUP BY Codigo, Concepto
ORDER BY 
    CASE 
        WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
        THEN TRY_CAST(Codigo AS INT)
        ELSE 999999 
    END,
    Codigo;

-- 3. Si NO existe la columna Codigo, ver con orden forzado
SELECT 
    CASE Concepto
        WHEN 'Preliminares' THEN 1
        WHEN 'Cimentación' THEN 2
        WHEN 'Cimentacion' THEN 2
        WHEN 'Estructura' THEN 3
        WHEN 'Ins. Hidraulica, Sanitaria y Gas LP' THEN 4
        WHEN 'Inst. Hidraulica, Sanitaria y Gas LP' THEN 4
        WHEN 'Inst. Eléctrica' THEN 5
        WHEN 'Inst. Electrica' THEN 5
        WHEN 'Albañilería' THEN 6
        WHEN 'Albanileria' THEN 6
        WHEN 'Albañileria' THEN 6
        WHEN 'Acabados' THEN 7
        WHEN 'Herrería, Aluminio y Vidrio' THEN 8
        WHEN 'Herreria, Aluminio y Vidrio' THEN 8
        WHEN 'Carpintería y Cerrajería' THEN 9
        WHEN 'Carpinteria y Cerrajeria' THEN 9
        WHEN 'Muebles y Accesorios' THEN 10
        WHEN 'Inst especiales y Obra Exterior' THEN 11
        WHEN 'Urbanización' THEN 12
        WHEN 'Urbanizacion' THEN 12
        ELSE 999
    END AS OrdenNumerico,
    Concepto,
    COUNT(*) AS NumPartidas
FROM [Estimacion(Concepto)]
GROUP BY Concepto
ORDER BY OrdenNumerico;

-- 4. Ver nombres exactos (con espacios/acentos) para detectar variaciones
SELECT DISTINCT
    CONCAT('[', Concepto, ']') AS ConceptoConCorchetes,
    LEN(Concepto) AS Longitud,
    UNICODE(LEFT(Concepto, 1)) AS PrimerCaracter
FROM [Estimacion(Concepto)]
ORDER BY Concepto;
