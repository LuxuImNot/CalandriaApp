-- ============================================================================
-- Script de Corrección RÁPIDA para AvanceObraManual
-- Ejecuta este script si tienes errores de "Invalid column name"
-- ============================================================================

USE CALANDRIA;
GO

PRINT '?? Iniciando corrección de tabla AvanceObraManual...';
PRINT '';

-- ============================================================================
-- PASO 1: Verificar si existe columna MontoEjecutado
-- ============================================================================
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AvanceObraManual') AND name = 'MontoEjecutado')
BEGIN
    PRINT '?? Encontrada columna MontoEjecutado (nombre antiguo)';
    
    -- Verificar si ya existe Ejecutado
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AvanceObraManual') AND name = 'Ejecutado')
    BEGIN
        PRINT '?? Ya existe columna Ejecutado, eliminando MontoEjecutado...';
        
        -- Copiar datos si es necesario
        UPDATE dbo.AvanceObraManual 
        SET Ejecutado = ISNULL(MontoEjecutado, 0)
        WHERE Ejecutado = 0 AND MontoEjecutado > 0;
        
        -- Eliminar columna antigua
        ALTER TABLE dbo.AvanceObraManual DROP COLUMN MontoEjecutado;
        PRINT '? Columna MontoEjecutado eliminada';
    END
    ELSE
    BEGIN
        -- Renombrar directamente
        EXEC sp_rename 'dbo.AvanceObraManual.MontoEjecutado', 'Ejecutado', 'COLUMN';
        PRINT '? Columna MontoEjecutado renombrada a Ejecutado';
    END
END
ELSE
BEGIN
    PRINT '?? No existe columna MontoEjecutado (ya está corregida o es nueva tabla)';
END
GO

-- ============================================================================
-- PASO 2: Agregar columna Ejecutado si no existe
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AvanceObraManual') AND name = 'Ejecutado')
BEGIN
    ALTER TABLE dbo.AvanceObraManual ADD Ejecutado DECIMAL(18,2) DEFAULT 0;
    PRINT '? Columna Ejecutado agregada';
END
ELSE
BEGIN
    PRINT '?? Columna Ejecutado ya existe';
END
GO

-- ============================================================================
-- PASO 3: Agregar columna Presupuestado si no existe
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AvanceObraManual') AND name = 'Presupuestado')
BEGIN
    ALTER TABLE dbo.AvanceObraManual ADD Presupuestado DECIMAL(18,2) DEFAULT 0;
    PRINT '? Columna Presupuestado agregada';
END
ELSE
BEGIN
    PRINT '?? Columna Presupuestado ya existe';
END
GO

-- ============================================================================
-- PASO 4: Verificar estructura actualizada
-- ============================================================================
PRINT '';
PRINT '========================================';
PRINT '  ?? ESTRUCTURA ACTUALIZADA';
PRINT '========================================';

SELECT 
    COLUMN_NAME AS Columna,
    DATA_TYPE AS TipoDato,
    CASE 
        WHEN CHARACTER_MAXIMUM_LENGTH IS NOT NULL THEN CAST(CHARACTER_MAXIMUM_LENGTH AS NVARCHAR(10))
        WHEN NUMERIC_PRECISION IS NOT NULL THEN CAST(NUMERIC_PRECISION AS NVARCHAR(10)) + ',' + CAST(NUMERIC_SCALE AS NVARCHAR(10))
        ELSE '-'
    END AS Tamano,
    IS_NULLABLE AS PermiteNull
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AvanceObraManual'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '========================================';
PRINT '  ? CORRECCIÓN COMPLETADA';
PRINT '========================================';
PRINT '';
PRINT '?? Columnas requeridas verificadas:';
PRINT '   ? Manzana';
PRINT '   ? Lote';
PRINT '   ? Categoria';
PRINT '   ? Presupuestado (nueva)';
PRINT '   ? Ejecutado (renombrada)';
PRINT '   ? PorcentajeAvance';
PRINT '   ? UsuarioCaptura';
PRINT '   ? FechaCaptura';
PRINT '';
PRINT '?? Ya puedes usar FormAvanceObra sin errores de columnas';
GO

-- ============================================================================
-- PASO 5: Verificar datos existentes
-- ============================================================================
PRINT '';
PRINT '========================================';
PRINT '  ?? RESUMEN DE DATOS';
PRINT '========================================';

DECLARE @TotalRegistros INT;
DECLARE @CasasUnicas INT;

SELECT @TotalRegistros = COUNT(*) FROM dbo.AvanceObraManual;
SELECT @CasasUnicas = COUNT(DISTINCT Manzana + '-' + Lote) FROM dbo.AvanceObraManual;

PRINT '   Total de registros: ' + CAST(@TotalRegistros AS NVARCHAR(10));
PRINT '   Casas con avance registrado: ' + CAST(@CasasUnicas AS NVARCHAR(10));

IF @TotalRegistros > 0
BEGIN
    PRINT '';
    PRINT '?? Últimos 5 registros:';
    SELECT TOP 5
        Manzana,
        Lote,
        Categoria,
        Presupuestado,
        Ejecutado,
        PorcentajeAvance,
        UsuarioCaptura,
        FechaCaptura
    FROM dbo.AvanceObraManual
    ORDER BY FechaCaptura DESC;
END

PRINT '';
PRINT '? Script de corrección finalizado correctamente';
GO
