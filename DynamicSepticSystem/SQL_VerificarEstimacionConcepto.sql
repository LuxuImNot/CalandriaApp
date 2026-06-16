-- =====================================================
-- SQL_VerificarEstimacionConcepto.sql
-- Verificación de la tabla dbo.Estimacion(Concepto)
-- =====================================================

PRINT '===== VERIFICACIÓN DE TABLA dbo.Estimacion(Concepto) ====='
PRINT ''

-- =====================================================
-- 1. VERIFICAR EXISTENCIA DE LA TABLA
-- =====================================================
PRINT '1. Verificando existencia de la tabla...'

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Estimacion(Concepto)' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT '   ? Tabla [dbo].[Estimacion(Concepto)] existe'
    PRINT ''
    
    -- Mostrar estructura de columnas
    PRINT '   Columnas de la tabla:'
    SELECT 
        COLUMN_NAME as [Columna],
        DATA_TYPE as [Tipo de Dato],
        CHARACTER_MAXIMUM_LENGTH as [Longitud],
        IS_NULLABLE as [Permite NULL]
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo' 
      AND TABLE_NAME = 'Estimacion(Concepto)'
    ORDER BY ORDINAL_POSITION
    
    PRINT ''
    
    -- Contar registros
    DECLARE @TotalRegistros INT
    SELECT @TotalRegistros = COUNT(*) FROM [dbo].[Estimacion(Concepto)]
    PRINT '   Total de registros: ' + CAST(@TotalRegistros AS NVARCHAR(10))
    PRINT ''
    
    -- Mostrar conceptos únicos
    PRINT '   Conceptos únicos encontrados:'
    SELECT 
        ROW_NUMBER() OVER (ORDER BY Concepto) as [Código],
        Concepto,
        COUNT(*) as [Registros],
        SUM(CAST(TOTAL as FLOAT)) as [Total]
    FROM [dbo].[Estimacion(Concepto)]
    GROUP BY Concepto
    ORDER BY Concepto
    
    PRINT ''
    PRINT '   Primeros 10 registros de la tabla:'
    SELECT TOP 10 
        Concepto,
        TOTAL
    FROM [dbo].[Estimacion(Concepto)]
    ORDER BY Concepto
    
END
ELSE
BEGIN
    PRINT '   ? ERROR: Tabla [dbo].[Estimacion(Concepto)] NO EXISTE'
    PRINT ''
    PRINT '   Verificando si existe con otro nombre...'
    
    -- Buscar tablas similares
    SELECT 
        TABLE_SCHEMA as [Esquema],
        TABLE_NAME as [Tabla]
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME LIKE '%Estimacion%'
       OR TABLE_NAME LIKE '%Concepto%'
    ORDER BY TABLE_SCHEMA, TABLE_NAME
END

PRINT ''
PRINT '-- ------------------------------------------------'
PRINT ''

-- =====================================================
-- 2. VERIFICAR COLUMNA CONCEPTO
-- =====================================================
PRINT '2. Verificando columna Concepto...'

IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
      AND TABLE_NAME = 'Estimacion(Concepto)' 
      AND COLUMN_NAME = 'Concepto'
)
BEGIN
    PRINT '   ? Columna Concepto existe'
    
    -- Verificar tipo de dato
    DECLARE @TipoDato NVARCHAR(50)
    SELECT @TipoDato = DATA_TYPE 
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo' 
      AND TABLE_NAME = 'Estimacion(Concepto)' 
      AND COLUMN_NAME = 'Concepto'
    
    PRINT '   Tipo de dato: ' + @TipoDato
    
    -- Verificar valores únicos
    DECLARE @ConceptosUnicos INT
    SELECT @ConceptosUnicos = COUNT(DISTINCT Concepto) FROM [dbo].[Estimacion(Concepto)]
    PRINT '   Conceptos únicos: ' + CAST(@ConceptosUnicos AS NVARCHAR(10))
    
    -- Verificar si hay NULLs
    DECLARE @ConceptosNull INT
    SELECT @ConceptosNull = COUNT(*) FROM [dbo].[Estimacion(Concepto)] WHERE Concepto IS NULL
    IF @ConceptosNull > 0
        PRINT '   ? ADVERTENCIA: Hay ' + CAST(@ConceptosNull AS NVARCHAR(10)) + ' registros con Concepto NULL'
    ELSE
        PRINT '   ? No hay valores NULL en Concepto'
END
ELSE
BEGIN
    PRINT '   ? ERROR: Columna Concepto NO existe'
    PRINT ''
    PRINT '   Columnas disponibles en la tabla:'
    SELECT COLUMN_NAME as [Columna]
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo' 
      AND TABLE_NAME = 'Estimacion(Concepto)'
    ORDER BY ORDINAL_POSITION
END

PRINT ''
PRINT '-- ------------------------------------------------'
PRINT ''

-- =====================================================
-- 3. VERIFICAR COLUMNA TOTAL
-- =====================================================
PRINT '3. Verificando columna TOTAL...'

IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo' 
      AND TABLE_NAME = 'Estimacion(Concepto)' 
      AND COLUMN_NAME = 'TOTAL'
)
BEGIN
    PRINT '   ? Columna TOTAL existe'
    
    -- Verificar tipo de dato
    DECLARE @TipoDatoTotal NVARCHAR(50)
    SELECT @TipoDatoTotal = DATA_TYPE 
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo' 
      AND TABLE_NAME = 'Estimacion(Concepto)' 
      AND COLUMN_NAME = 'TOTAL'
    
    PRINT '   Tipo de dato: ' + @TipoDatoTotal
    
    -- Estadísticas de valores
    DECLARE @TotalMin FLOAT, @TotalMax FLOAT, @TotalAvg FLOAT, @TotalSum FLOAT
    SELECT 
        @TotalMin = MIN(CAST(TOTAL as FLOAT)),
        @TotalMax = MAX(CAST(TOTAL as FLOAT)),
        @TotalAvg = AVG(CAST(TOTAL as FLOAT)),
        @TotalSum = SUM(CAST(TOTAL as FLOAT))
    FROM [dbo].[Estimacion(Concepto)]
    
    PRINT '   Valor mínimo: $' + CAST(@TotalMin AS NVARCHAR(20))
    PRINT '   Valor máximo: $' + CAST(@TotalMax AS NVARCHAR(20))
    PRINT '   Promedio: $' + CAST(@TotalAvg AS NVARCHAR(20))
    PRINT '   Suma total: $' + CAST(@TotalSum AS NVARCHAR(20))
    
    -- Verificar si hay NULLs o ceros
    DECLARE @TotalesNull INT, @TotalesCero INT
    SELECT @TotalesNull = COUNT(*) FROM [dbo].[Estimacion(Concepto)] WHERE TOTAL IS NULL
    SELECT @TotalesCero = COUNT(*) FROM [dbo].[Estimacion(Concepto)] WHERE CAST(TOTAL as FLOAT) = 0
    
    IF @TotalesNull > 0
        PRINT '   ? ADVERTENCIA: Hay ' + CAST(@TotalesNull AS NVARCHAR(10)) + ' registros con TOTAL NULL'
    ELSE
        PRINT '   ? No hay valores NULL en TOTAL'
    
    IF @TotalesCero > 0
        PRINT '   ? Hay ' + CAST(@TotalesCero AS NVARCHAR(10)) + ' registros con TOTAL = 0'
END
ELSE
BEGIN
    PRINT '   ? ERROR: Columna TOTAL NO existe'
    PRINT ''
    PRINT '   Buscando columnas numéricas alternativas...'
    SELECT 
        COLUMN_NAME as [Columna],
        DATA_TYPE as [Tipo]
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo' 
      AND TABLE_NAME = 'Estimacion(Concepto)'
      AND DATA_TYPE IN ('decimal', 'numeric', 'float', 'money', 'int', 'bigint')
    ORDER BY ORDINAL_POSITION
END

PRINT ''
PRINT '-- ------------------------------------------------'
PRINT ''

-- =====================================================
-- 4. DATOS DE EJEMPLO
-- =====================================================
PRINT '4. Mostrando datos de ejemplo agrupados...'
PRINT ''

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Estimacion(Concepto)' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT '   Conceptos con sus totales (agrupados):'
    PRINT ''
    
    SELECT 
        ROW_NUMBER() OVER (ORDER BY Concepto) as [Código],
        Concepto,
        COUNT(*) as [Partidas],
        SUM(CAST(TOTAL as FLOAT)) as [Total Acumulado],
        MIN(CAST(TOTAL as FLOAT)) as [Mínimo],
        MAX(CAST(TOTAL as FLOAT)) as [Máximo]
    FROM [dbo].[Estimacion(Concepto)]
    GROUP BY Concepto
    ORDER BY Concepto
END

PRINT ''
PRINT '-- ------------------------------------------------'
PRINT ''

-- =====================================================
-- 5. COMPATIBILIDAD CON FormAvanceConcepto
-- =====================================================
PRINT '5. Verificando compatibilidad con FormAvanceConcepto...'
PRINT ''

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Estimacion(Concepto)' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- Simular consulta del FormAvanceConcepto
    PRINT '   Simulando consulta del formulario:'
    PRINT ''
    
    SELECT 
        ROW_NUMBER() OVER (ORDER BY Concepto) as Codigo,
        Concepto,
        SUM(CAST(TOTAL as FLOAT)) as Total
    FROM [dbo].[Estimacion(Concepto)]
    GROUP BY Concepto
    ORDER BY Concepto
    
    PRINT ''
    DECLARE @TotalConceptos INT
    SELECT @TotalConceptos = COUNT(DISTINCT Concepto) FROM [dbo].[Estimacion(Concepto)]
    
    IF @TotalConceptos > 0
    BEGIN
        PRINT '   ? La consulta funciona correctamente'
        PRINT '   Total de conceptos para el formulario: ' + CAST(@TotalConceptos AS NVARCHAR(10))
    END
    ELSE
    BEGIN
        PRINT '   ? ADVERTENCIA: No hay conceptos únicos en la tabla'
    END
END

PRINT ''
PRINT '-- ------------------------------------------------'
PRINT ''

-- =====================================================
-- 6. RECOMENDACIONES
-- =====================================================
PRINT '6. Recomendaciones:'
PRINT ''

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Estimacion(Concepto)' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- Verificar índices
    IF NOT EXISTS (
        SELECT * FROM sys.indexes 
        WHERE object_id = OBJECT_ID('[dbo].[Estimacion(Concepto)]') 
        AND name IS NOT NULL
        AND is_primary_key = 0
    )
    BEGIN
        PRINT '   ? Crear índice para mejorar rendimiento:'
        PRINT '   CREATE INDEX IX_EstimacionConcepto_Concepto'
        PRINT '   ON [dbo].[Estimacion(Concepto)](Concepto);'
        PRINT ''
    END
    ELSE
    BEGIN
        PRINT '   ? Ya existen índices en la tabla'
    END
    
    -- Verificar si hay duplicados exactos
    DECLARE @Duplicados INT
    SELECT @Duplicados = COUNT(*)
    FROM (
        SELECT Concepto, TOTAL, COUNT(*) as Cuenta
        FROM [dbo].[Estimacion(Concepto)]
        GROUP BY Concepto, TOTAL
        HAVING COUNT(*) > 1
    ) as Dups
    
    IF @Duplicados > 0
    BEGIN
        PRINT '   ? Se encontraron ' + CAST(@Duplicados AS NVARCHAR(10)) + ' grupos de registros duplicados'
        PRINT '   Considera revisar estos duplicados:'
        PRINT ''
        SELECT TOP 5
            Concepto,
            TOTAL,
            COUNT(*) as [Veces Repetido]
        FROM [dbo].[Estimacion(Concepto)]
        GROUP BY Concepto, TOTAL
        HAVING COUNT(*) > 1
        ORDER BY COUNT(*) DESC
    END
END

PRINT ''
PRINT '===== FIN DE VERIFICACIÓN ====='
PRINT ''

-- =====================================================
-- RESUMEN FINAL
-- =====================================================
PRINT 'RESUMEN:'
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Estimacion(Concepto)' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    DECLARE @ConceptoCount INT, @RegistroCount INT
    SELECT @ConceptoCount = COUNT(DISTINCT Concepto), @RegistroCount = COUNT(*) 
    FROM [dbo].[Estimacion(Concepto)]
    
    PRINT '? Tabla existe con ' + CAST(@RegistroCount AS NVARCHAR(10)) + ' registros'
    PRINT '? ' + CAST(@ConceptoCount AS NVARCHAR(10)) + ' conceptos únicos encontrados'
    PRINT '? Compatible con FormAvanceConcepto'
END
ELSE
BEGIN
    PRINT '? La tabla no existe o tiene otro nombre'
    PRINT '? Verifica el nombre exacto de la tabla en tu base de datos'
END
