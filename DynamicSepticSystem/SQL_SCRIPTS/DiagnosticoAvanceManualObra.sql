-- ============================================================================
-- SCRIPT DE DIAGNÓSTICO: AvanceManualObra
-- ============================================================================
-- Propósito: Diagnosticar problemas con la carga de avances de partidas
-- Fecha: 2025-01-28
-- Autor: Sistema Calandria
-- ============================================================================

PRINT '========================================='
PRINT 'DIAGNÓSTICO: Tabla AvanceManualObra'
PRINT '========================================='
PRINT ''

-- ============================================================================
-- 1. Verificar existencia de la tabla
-- ============================================================================
PRINT '1. Verificando existencia de la tabla...'
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AvanceManualObra')
BEGIN
    PRINT '   ? La tabla AvanceManualObra EXISTE'
    PRINT ''
END
ELSE
BEGIN
    PRINT '   ? ERROR: La tabla AvanceManualObra NO EXISTE'
    PRINT '   Acción: Crear la tabla primero'
    PRINT ''
    -- No continuar si la tabla no existe
    RETURN
END

-- ============================================================================
-- 2. Mostrar estructura de la tabla
-- ============================================================================
PRINT '2. Estructura de la tabla:'
PRINT '   ---------------------------------------------------------'
SELECT 
    COLUMN_NAME as 'Columna',
    DATA_TYPE as 'Tipo',
    CHARACTER_MAXIMUM_LENGTH as 'Longitud',
    IS_NULLABLE as 'Permite_NULL',
    COLUMN_DEFAULT as 'Valor_Defecto'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AvanceManualObra'
ORDER BY ORDINAL_POSITION
PRINT ''

-- ============================================================================
-- 3. Verificar tipo de dato de columna WBS
-- ============================================================================
PRINT '3. Tipo de dato de columna WBS:'
DECLARE @TipoWBS NVARCHAR(50)
SELECT @TipoWBS = DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AvanceManualObra' 
AND COLUMN_NAME = 'WBS'

PRINT '   Tipo actual: ' + ISNULL(@TipoWBS, 'NO EXISTE')

IF @TipoWBS = 'nvarchar' OR @TipoWBS = 'varchar'
BEGIN
    PRINT '   ??  WBS es de tipo texto - Se convertirá a INT en código C#'
END
ELSE IF @TipoWBS = 'int'
BEGIN
    PRINT '   ??  WBS es de tipo entero - Conversión directa'
END
ELSE
BEGIN
    PRINT '   ??  WBS tiene un tipo inesperado: ' + ISNULL(@TipoWBS, 'NULL')
END
PRINT ''

-- ============================================================================
-- 4. Verificar si existe columna MetrosCuadrados
-- ============================================================================
PRINT '4. Verificando columna MetrosCuadrados:'
IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AvanceManualObra' 
    AND COLUMN_NAME = 'MetrosCuadrados'
)
BEGIN
    PRINT '   ? Columna MetrosCuadrados EXISTE'
END
ELSE
BEGIN
    PRINT '   ??  Columna MetrosCuadrados NO EXISTE'
    PRINT '   Recomendación: Ejecutar AgregarColumnaMetrosCuadradosAvance.sql'
END
PRINT ''

-- ============================================================================
-- 5. Verificar si existe columna FechaFinalizacion
-- ============================================================================
PRINT '5. Verificando columna FechaFinalizacion:'
IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AvanceManualObra' 
    AND COLUMN_NAME = 'FechaFinalizacion'
)
BEGIN
    PRINT '   ? Columna FechaFinalizacion EXISTE'
END
ELSE
BEGIN
    PRINT '   ??  Columna FechaFinalizacion NO EXISTE'
    PRINT '   Recomendación: Ejecutar script para agregar la columna'
END
PRINT ''

-- ============================================================================
-- 6. Analizar contenido de la tabla
-- ============================================================================
PRINT '6. Análisis de contenido:'
PRINT '   ---------------------------------------------------------'

-- Contar registros totales
DECLARE @TotalRegistros INT
SELECT @TotalRegistros = COUNT(*) FROM AvanceManualObra
PRINT '   Total de registros: ' + CAST(@TotalRegistros AS NVARCHAR)

-- Contar registros con WBS NULL o vacío
DECLARE @WBSNulos INT
SELECT @WBSNulos = COUNT(*) FROM AvanceManualObra WHERE WBS IS NULL OR WBS = ''
PRINT '   Registros con WBS NULL o vacío: ' + CAST(@WBSNulos AS NVARCHAR)

-- Contar registros con WBS no numérico
DECLARE @WBSNoNumerico INT
SELECT @WBSNoNumerico = COUNT(*) 
FROM AvanceManualObra 
WHERE WBS IS NOT NULL 
AND WBS <> ''
AND TRY_CAST(WBS AS INT) IS NULL
PRINT '   Registros con WBS no numérico: ' + CAST(@WBSNoNumerico AS NVARCHAR)

-- Contar registros con WBS numérico válido
DECLARE @WBSValidos INT
SELECT @WBSValidos = COUNT(*) 
FROM AvanceManualObra 
WHERE TRY_CAST(WBS AS INT) IS NOT NULL
PRINT '   Registros con WBS numérico válido: ' + CAST(@WBSValidos AS NVARCHAR)
PRINT ''

-- ============================================================================
-- 7. Mostrar ejemplos de registros problemáticos
-- ============================================================================
IF @WBSNulos > 0 OR @WBSNoNumerico > 0
BEGIN
    PRINT '7. Registros problemáticos (primeros 10):'
    PRINT '   ---------------------------------------------------------'
    
    IF @WBSNulos > 0
    BEGIN
        PRINT '   a) WBS NULL o vacío:'
        SELECT TOP 10 
            Id,
            Manzana,
            Lote,
            WBS,
            Prototipo
        FROM AvanceManualObra
        WHERE WBS IS NULL OR WBS = ''
        ORDER BY Id
        PRINT ''
    END
    
    IF @WBSNoNumerico > 0
    BEGIN
        PRINT '   b) WBS no numérico:'
        SELECT TOP 10 
            Id,
            Manzana,
            Lote,
            WBS,
            Prototipo
        FROM AvanceManualObra
        WHERE WBS IS NOT NULL 
        AND WBS <> ''
        AND TRY_CAST(WBS AS INT) IS NULL
        ORDER BY Id
        PRINT ''
    END
END
ELSE
BEGIN
    PRINT '7. No se encontraron registros problemáticos ?'
    PRINT ''
END

-- ============================================================================
-- 8. Mostrar muestra de registros válidos
-- ============================================================================
PRINT '8. Muestra de registros válidos (primeros 5):'
PRINT '   ---------------------------------------------------------'
SELECT TOP 5 
    Manzana,
    Lote,
    WBS,
    AvancePorcentaje,
    MontoEjecutado,
    CASE 
        WHEN TRY_CAST(WBS AS INT) IS NOT NULL THEN 'OK ?'
        ELSE 'ERROR ?'
    END AS 'Estado_WBS'
FROM AvanceManualObra
WHERE Manzana IS NOT NULL AND Lote IS NOT NULL
ORDER BY Manzana, Lote, 
    CASE 
        WHEN TRY_CAST(WBS AS INT) IS NOT NULL THEN TRY_CAST(WBS AS INT)
        ELSE 999999 
    END
PRINT ''

-- ============================================================================
-- 9. Recomendaciones
-- ============================================================================
PRINT '9. Recomendaciones:'
PRINT '   ---------------------------------------------------------'

IF @WBSNulos > 0
BEGIN
    PRINT '   ??  Hay registros con WBS NULL o vacío'
    PRINT '      Acción: Eliminar o corregir estos registros'
    PRINT '      Query sugerida:'
    PRINT '      DELETE FROM AvanceManualObra WHERE WBS IS NULL OR WBS = '''''
    PRINT ''
END

IF @WBSNoNumerico > 0
BEGIN
    PRINT '   ??  Hay registros con WBS no numérico'
    PRINT '      Acción: Corregir los valores de WBS'
    PRINT '      Query sugerida:'
    PRINT '      UPDATE AvanceManualObra SET WBS = CAST(CAST(WBS AS FLOAT) AS INT) WHERE TRY_CAST(WBS AS INT) IS NULL'
    PRINT ''
END

IF @TipoWBS NOT IN ('int', 'nvarchar', 'varchar')
BEGIN
    PRINT '   ??  Tipo de dato de WBS no es estándar'
    PRINT '      Tipo actual: ' + @TipoWBS
    PRINT '      Acción: Considerar cambiar a INT o NVARCHAR'
    PRINT ''
END

IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AvanceManualObra' 
    AND COLUMN_NAME = 'MetrosCuadrados'
)
BEGIN
    PRINT '   ??  Para habilitar partidas dinámicas:'
    PRINT '      Ejecutar: AgregarColumnaMetrosCuadradosAvance.sql'
    PRINT ''
END

IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AvanceManualObra' 
    AND COLUMN_NAME = 'FechaFinalizacion'
)
BEGIN
    PRINT '   ??  Para registrar fechas de finalización:'
    PRINT '      Agregar columna: FechaFinalizacion DATETIME NULL'
    PRINT ''
END

IF @WBSNulos = 0 AND @WBSNoNumerico = 0
BEGIN
    PRINT '   ? La tabla está en buen estado'
    PRINT '   ? Todos los WBS son numéricos válidos'
    PRINT ''
END

-- ============================================================================
-- 10. Query de prueba para C#
-- ============================================================================
PRINT '10. Query de prueba (equivalente al código C#):'
PRINT '    ---------------------------------------------------------'
PRINT '    Ejecutar con una Manzana y Lote específicos:'
PRINT ''
PRINT '    DECLARE @manzana NVARCHAR(10) = ''1'''
PRINT '    DECLARE @lote NVARCHAR(10) = ''1'''
PRINT ''
PRINT '    SELECT '
PRINT '        WBS,'
PRINT '        ISNULL(AvancePorcentaje, 0) AS AvancePorcentaje,'
PRINT '        ISNULL(MontoEjecutado, 0) AS MontoEjecutado,'
PRINT '        FechaFinalizacion,'
PRINT '        ISNULL(MetrosCuadrados, 0) AS MetrosCuadrados,'
PRINT '        CASE '
PRINT '            WHEN TRY_CAST(WBS AS INT) IS NOT NULL THEN ''OK ?'''
PRINT '            ELSE ''ERROR: WBS no numérico ?'''
PRINT '        END AS Estado'
PRINT '    FROM AvanceManualObra'
PRINT '    WHERE Manzana = @manzana AND Lote = @lote'
PRINT ''

-- ============================================================================
PRINT '========================================='
PRINT 'FIN DEL DIAGNÓSTICO'
PRINT '========================================='
