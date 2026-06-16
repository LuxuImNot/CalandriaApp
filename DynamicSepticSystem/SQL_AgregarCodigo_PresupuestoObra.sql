-- ============================================================================
-- Script: Agregar y Poblar Columna Codigo en PresupuestoObra
-- Propósito: Ordenar conceptos numéricamente (1-12) en lugar de alfabéticamente
-- Fecha: 2025-01-XX
-- ============================================================================

USE CalandriaResidencial;  -- CAMBIAR por el nombre de tu base de datos
GO

-- Paso 1: Verificar si existe la columna Codigo
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'PresupuestoObra' AND COLUMN_NAME = 'Codigo')
BEGIN
    PRINT '? Agregando columna Codigo a PresupuestoObra...'
    ALTER TABLE PresupuestoObra ADD Codigo NVARCHAR(10) NULL;
END
ELSE
BEGIN
    PRINT '?? La columna Codigo ya existe'
END
GO

-- Paso 2: Poblar la columna Codigo según el campo Padre
PRINT '?? Actualizando códigos según el concepto...'

-- Código 1: Preliminares
UPDATE PresupuestoObra 
SET Codigo = '1' 
WHERE Padre LIKE '%Preliminares%';

-- Código 2: Cimentación
UPDATE PresupuestoObra 
SET Codigo = '2' 
WHERE Padre LIKE '%Cimentaci%n%' OR Padre LIKE '%Cimentacion%';

-- Código 3: Estructura
UPDATE PresupuestoObra 
SET Codigo = '3' 
WHERE Padre LIKE '%Estructura%' OR Padre LIKE '%Losas%' OR Padre LIKE '%Muros%';

-- Código 4: Instalaciones Hidráulica, Sanitaria y Gas LP
UPDATE PresupuestoObra 
SET Codigo = '4' 
WHERE Padre LIKE '%Inst. Hidraulica%' 
   OR Padre LIKE '%Instalacion Hidraulica%'
   OR Padre LIKE '%Inst. Sanitaria%' 
   OR Padre LIKE '%Instalacion Sanitaria%'
   OR Padre LIKE '%Gas LP%';

-- Código 5: Instalación Eléctrica
UPDATE PresupuestoObra 
SET Codigo = '5' 
WHERE Padre LIKE '%Inst. Electrica%' OR Padre LIKE '%Instalacion Electrica%';

-- Código 6: Albañilería
UPDATE PresupuestoObra 
SET Codigo = '6' 
WHERE Padre LIKE '%Alba%ileria%' OR Padre LIKE '%Alba%iler%a%';

-- Código 7: Acabados
UPDATE PresupuestoObra 
SET Codigo = '7' 
WHERE Padre LIKE '%Acabados%';

-- Código 8: Herrería, Aluminio y Vidrio
UPDATE PresupuestoObra 
SET Codigo = '8' 
WHERE Padre LIKE '%Herrer%a%' OR Padre LIKE '%Aluminio%' OR Padre LIKE '%Vidrio%';

-- Código 9: Carpintería y Cerrajería
UPDATE PresupuestoObra 
SET Codigo = '9' 
WHERE Padre LIKE '%Carpinter%a%' OR Padre LIKE '%Cerrajer%a%';

-- Código 10: Muebles y Accesorios
UPDATE PresupuestoObra 
SET Codigo = '10' 
WHERE Padre LIKE '%Muebles%' OR Padre LIKE '%Accesorios%';

-- Código 11: Instalaciones Especiales y Obra Exterior
UPDATE PresupuestoObra 
SET Codigo = '11' 
WHERE Padre LIKE '%Inst especiales%' 
   OR Padre LIKE '%Obra Exterior%'
   OR Padre LIKE '%Limpieza%Instalaciones%';

-- Código 12: Urbanización
UPDATE PresupuestoObra 
SET Codigo = '12' 
WHERE Padre LIKE '%Urbanizaci%n%';

GO

-- Paso 3: Verificar resultados
PRINT ''
PRINT '?? VERIFICACIÓN DE RESULTADOS:'
PRINT '=============================='

SELECT 
    Codigo,
    Padre,
    COUNT(*) as CantidadPartidas
FROM PresupuestoObra
GROUP BY Codigo, Padre
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
    Padre,
    Etapa,
    Partida,
    COUNT(*) as Cantidad
FROM PresupuestoObra
WHERE Codigo IS NULL OR Codigo = ''
GROUP BY Padre, Etapa, Partida
ORDER BY Padre, Etapa, Partida;

-- Paso 5: Resumen
PRINT ''
PRINT '?? RESUMEN:'
PRINT '==========='
SELECT 
    COUNT(*) as TotalRegistros,
    SUM(CASE WHEN Codigo IS NOT NULL AND Codigo != '' THEN 1 ELSE 0 END) as ConCodigo,
    SUM(CASE WHEN Codigo IS NULL OR Codigo = '' THEN 1 ELSE 0 END) as SinCodigo
FROM PresupuestoObra;

GO

PRINT ''
PRINT '? Script completado exitosamente!'
PRINT 'Ahora cierra y vuelve a abrir la aplicación para ver los cambios.'
