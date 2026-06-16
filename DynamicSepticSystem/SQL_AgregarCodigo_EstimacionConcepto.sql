-- ============================================================================
-- Script: Agregar y Poblar Columna Codigo en Estimacion(Concepto)
-- Propósito: Ordenar conceptos numéricamente (1-12) en lugar de alfabéticamente
-- Fecha: 2025-01-XX
-- ============================================================================

USE CalandriaResidencial;  -- ?? CAMBIAR por el nombre de tu base de datos
GO

-- Paso 1: Verificar si existe la columna Codigo
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Estimacion(Concepto)' AND COLUMN_NAME = 'Codigo')
BEGIN
    PRINT '? Agregando columna Codigo a Estimacion(Concepto)...'
    ALTER TABLE [Estimacion(Concepto)] ADD Codigo NVARCHAR(10) NULL;
END
ELSE
BEGIN
    PRINT '?? La columna Codigo ya existe'
END
GO

-- Paso 2: Poblar la columna Codigo según el concepto
PRINT '?? Actualizando códigos según el concepto...'

-- Código 1: Preliminares
UPDATE [Estimacion(Concepto)]
SET Codigo = '1' 
WHERE Concepto LIKE '%Preliminares%';

-- Código 2: Cimentación
UPDATE [Estimacion(Concepto)]
SET Codigo = '2' 
WHERE Concepto LIKE '%Cimentaci%n%' OR Concepto LIKE '%Cimentacion%';

-- Código 3: Estructura
UPDATE [Estimacion(Concepto)]
SET Codigo = '3' 
WHERE Concepto LIKE '%Estructura%';

-- Código 4: Instalaciones Hidráulica, Sanitaria y Gas LP
UPDATE [Estimacion(Concepto)]
SET Codigo = '4' 
WHERE Concepto LIKE '%Ins. Hidraulica%Sanitaria%Gas LP%'
   OR Concepto LIKE '%Inst. Hidraulica%Sanitaria%Gas LP%'
   OR Concepto LIKE '%Hidraulica%Sanitaria%Gas%';

-- Código 5: Instalación Eléctrica
UPDATE [Estimacion(Concepto)]
SET Codigo = '5' 
WHERE Concepto LIKE '%Inst. El_ctrica%' 
   OR Concepto LIKE '%Inst. Electrica%';

-- Código 6: Albañilería
UPDATE [Estimacion(Concepto)]
SET Codigo = '6' 
WHERE Concepto LIKE '%Alba_ileria%' OR Concepto LIKE '%Alba_iler_a%';

-- Código 7: Acabados
UPDATE [Estimacion(Concepto)]
SET Codigo = '7' 
WHERE Concepto LIKE '%Acabados%' AND Concepto NOT LIKE '%Preliminares%';

-- Código 8: Herrería, Aluminio y Vidrio
UPDATE [Estimacion(Concepto)]
SET Codigo = '8' 
WHERE Concepto LIKE '%Herrer_a%Aluminio%Vidrio%';

-- Código 9: Carpintería y Cerrajería
UPDATE [Estimacion(Concepto)]
SET Codigo = '9' 
WHERE Concepto LIKE '%Carpinter_a%Cerrajer_a%';

-- Código 10: Muebles y Accesorios
UPDATE [Estimacion(Concepto)]
SET Codigo = '10' 
WHERE Concepto LIKE '%Muebles%Accesorios%';

-- Código 11: Instalaciones Especiales y Obra Exterior
UPDATE [Estimacion(Concepto)]
SET Codigo = '11' 
WHERE Concepto LIKE '%Inst especiales%Obra Exterior%'
   OR Concepto LIKE '%Limpieza%Instalaciones especiales%';

-- Código 12: Urbanización
UPDATE [Estimacion(Concepto)]
SET Codigo = '12' 
WHERE Concepto LIKE '%Urbanizaci_n%';

GO

-- Paso 3: Verificar resultados
PRINT ''
PRINT '?? VERIFICACIÓN DE RESULTADOS:'
PRINT '=============================='

SELECT 
    Codigo,
    Concepto,
    COUNT(*) as CantidadRegistros
FROM [Estimacion(Concepto)]
GROUP BY Codigo, Concepto
ORDER BY 
    CASE 
        WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
        THEN TRY_CAST(Codigo AS INT)
        ELSE 999999 
    END,
    Codigo;

-- Paso 4: Detectar registros sin código asignado
PRINT ''
PRINT '?? REGISTROS SIN CÓDIGO ASIGNADO:'
PRINT '=================================='

SELECT 
    Concepto,
    COUNT(*) as Cantidad
FROM [Estimacion(Concepto)]
WHERE Codigo IS NULL OR Codigo = ''
GROUP BY Concepto
ORDER BY Concepto;

-- Paso 5: Resumen
PRINT ''
PRINT '?? RESUMEN:'
PRINT '==========='
SELECT 
    COUNT(*) as TotalRegistros,
    SUM(CASE WHEN Codigo IS NOT NULL AND Codigo != '' THEN 1 ELSE 0 END) as ConCodigo,
    SUM(CASE WHEN Codigo IS NULL OR Codigo = '' THEN 1 ELSE 0 END) as SinCodigo
FROM [Estimacion(Concepto)];

GO

PRINT ''
PRINT '? Script completado exitosamente!'
PRINT 'Ahora cierra y vuelve a abrir la aplicación para ver los cambios.'
