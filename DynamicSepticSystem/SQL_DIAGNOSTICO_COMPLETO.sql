-- ============================================================================
-- Script de Verificación y Diagnóstico COMPLETO
-- Ejecuta este para identificar problemas en la estructura de las tablas
-- ============================================================================

USE CALANDRIA;
GO

PRINT '';
PRINT '========================================';
PRINT '  ?? DIAGNÓSTICO COMPLETO DE TABLAS';
PRINT '========================================';
PRINT '';

-- ============================================================================
-- 1. VERIFICAR TABLA AvanceObraManual
-- ============================================================================
PRINT '?? 1. TABLA: AvanceObraManual';
PRINT '----------------------------------------';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.AvanceObraManual') AND type in (N'U'))
BEGIN
    PRINT '? La tabla existe';
    PRINT '';
    PRINT 'Columnas encontradas:';
    
    SELECT 
        COLUMN_NAME AS Columna,
        DATA_TYPE AS Tipo,
        CASE 
            WHEN CHARACTER_MAXIMUM_LENGTH IS NOT NULL THEN CAST(CHARACTER_MAXIMUM_LENGTH AS NVARCHAR(10))
            WHEN NUMERIC_PRECISION IS NOT NULL THEN CAST(NUMERIC_PRECISION AS NVARCHAR(10)) + ',' + CAST(NUMERIC_SCALE AS NVARCHAR(10))
            ELSE '-'
        END AS Tamano
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AvanceObraManual'
    ORDER BY ORDINAL_POSITION;
    
    -- Verificar columnas críticas
    DECLARE @tieneEjecutado INT = 0;
    DECLARE @tienePresupuestado INT = 0;
    
    SELECT @tieneEjecutado = COUNT(*) FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.AvanceObraManual') AND name = 'Ejecutado';
    
    SELECT @tienePresupuestado = COUNT(*) FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.AvanceObraManual') AND name = 'Presupuestado';
    
    PRINT '';
    IF @tieneEjecutado = 1
        PRINT '? Columna Ejecutado: OK';
    ELSE
        PRINT '? Columna Ejecutado: FALTA - Ejecuta SQL_CORREGIR_TablaAvanceManual.sql';
    
    IF @tienePresupuestado = 1
        PRINT '? Columna Presupuestado: OK';
    ELSE
        PRINT '? Columna Presupuestado: FALTA - Ejecuta SQL_CORREGIR_TablaAvanceManual.sql';
    
    -- Verificar si existe MontoEjecutado (columna antigua)
    DECLARE @tieneMontoEjecutado INT = 0;
    SELECT @tieneMontoEjecutado = COUNT(*) FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.AvanceObraManual') AND name = 'MontoEjecutado';
    
    IF @tieneMontoEjecutado = 1
        PRINT '?? Columna MontoEjecutado: ENCONTRADA (debería ser Ejecutado) - Ejecuta SQL_CORREGIR_TablaAvanceManual.sql';
    
    PRINT '';
    PRINT 'Registros en tabla:';
    SELECT COUNT(*) AS TotalRegistros FROM dbo.AvanceObraManual;
END
ELSE
BEGIN
    PRINT '? La tabla NO existe';
    PRINT '?? SOLUCIÓN: Ejecuta SQL_CrearTablaAvanceManual.sql';
END

PRINT '';
PRINT '';

-- ============================================================================
-- 2. VERIFICAR TABLA PresupuestoObra
-- ============================================================================
PRINT '?? 2. TABLA: PresupuestoObra';
PRINT '----------------------------------------';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.PresupuestoObra') AND type in (N'U'))
BEGIN
    PRINT '? La tabla existe';
    PRINT '';
    PRINT 'Columnas encontradas:';
    
    SELECT 
        COLUMN_NAME AS Columna,
        DATA_TYPE AS Tipo,
        CASE 
            WHEN CHARACTER_MAXIMUM_LENGTH IS NOT NULL THEN CAST(CHARACTER_MAXIMUM_LENGTH AS NVARCHAR(10))
            WHEN NUMERIC_PRECISION IS NOT NULL THEN CAST(NUMERIC_PRECISION AS NVARCHAR(10)) + ',' + CAST(NUMERIC_SCALE AS NVARCHAR(10))
            ELSE '-'
        END AS Tamano
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'PresupuestoObra'
    ORDER BY ORDINAL_POSITION;
    
    PRINT '';
    PRINT 'Registros en tabla:';
    SELECT COUNT(*) AS TotalConceptos FROM dbo.PresupuestoObra;
    
    PRINT '';
    PRINT 'Prototipos disponibles:';
    SELECT DISTINCT Prototipo FROM dbo.PresupuestoObra ORDER BY Prototipo;
    
    PRINT '';
    PRINT 'Categorías disponibles:';
    SELECT DISTINCT Categoria, COUNT(*) AS Conceptos 
    FROM dbo.PresupuestoObra 
    GROUP BY Categoria 
    ORDER BY Categoria;
    
    -- Verificar si hay datos
    DECLARE @totalPresupuesto INT = 0;
    SELECT @totalPresupuesto = COUNT(*) FROM dbo.PresupuestoObra;
    
    IF @totalPresupuesto = 0
    BEGIN
        PRINT '';
        PRINT '?? ADVERTENCIA: La tabla está VACÍA';
        PRINT '?? SOLUCIÓN: Ejecuta SQL_CrearEImportarPresupuesto.sql';
    END
END
ELSE
BEGIN
    PRINT '? La tabla NO existe';
    PRINT '?? SOLUCIÓN: Ejecuta SQL_CrearEImportarPresupuesto.sql';
END

PRINT '';
PRINT '';

-- ============================================================================
-- 3. VERIFICAR TABLA InventarioCasas
-- ============================================================================
PRINT '?? 3. TABLA: InventarioCasas';
PRINT '----------------------------------------';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.InventarioCasas') AND type in (N'U'))
BEGIN
    PRINT '? La tabla existe';
    
    PRINT '';
    PRINT 'Total de casas:';
    SELECT COUNT(*) AS TotalCasas FROM dbo.InventarioCasas;
    
    PRINT '';
    PRINT 'Casas por prototipo:';
    SELECT 
        Prototipo, 
        COUNT(*) AS Cantidad 
    FROM dbo.InventarioCasas 
    GROUP BY Prototipo 
    ORDER BY Prototipo;
    
    -- Verificar casas sin prototipo
    DECLARE @sinPrototipo INT = 0;
    SELECT @sinPrototipo = COUNT(*) 
    FROM dbo.InventarioCasas 
    WHERE Prototipo IS NULL OR Prototipo = '';
    
    IF @sinPrototipo > 0
    BEGIN
        PRINT '';
        PRINT '?? ADVERTENCIA: ' + CAST(@sinPrototipo AS NVARCHAR(10)) + ' casas sin prototipo';
        PRINT '?? SOLUCIÓN: Ejecuta SQL_CorregirPrototipos.sql';
    END
END
ELSE
BEGIN
    PRINT '? La tabla NO existe';
    PRINT '?? SOLUCIÓN: Verifica la creación de tablas base';
END

PRINT '';
PRINT '';

-- ============================================================================
-- 4. VERIFICAR CONSISTENCIA DE DATOS
-- ============================================================================
PRINT '?? 4. CONSISTENCIA DE DATOS';
PRINT '----------------------------------------';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.InventarioCasas') AND type in (N'U'))
   AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.PresupuestoObra') AND type in (N'U'))
BEGIN
    PRINT 'Verificando prototipos...';
    PRINT '';
    
    -- Prototipos en InventarioCasas que NO están en PresupuestoObra
    PRINT 'Prototipos de casas SIN presupuesto:';
    SELECT DISTINCT i.Prototipo
    FROM dbo.InventarioCasas i
    WHERE i.Prototipo NOT IN (SELECT DISTINCT Prototipo FROM dbo.PresupuestoObra)
      AND i.Prototipo IS NOT NULL
      AND i.Prototipo <> '';
    
    DECLARE @prototiposSinPresupuesto INT = 0;
    SELECT @prototiposSinPresupuesto = COUNT(DISTINCT i.Prototipo)
    FROM dbo.InventarioCasas i
    WHERE i.Prototipo NOT IN (SELECT DISTINCT Prototipo FROM dbo.PresupuestoObra)
      AND i.Prototipo IS NOT NULL
      AND i.Prototipo <> '';
    
    IF @prototiposSinPresupuesto = 0
    BEGIN
        PRINT '? Todos los prototipos tienen presupuesto';
    END
    ELSE
    BEGIN
        PRINT '?? ' + CAST(@prototiposSinPresupuesto AS NVARCHAR(10)) + ' prototipos sin presupuesto';
        PRINT '?? SOLUCIÓN: Agrega el presupuesto para estos prototipos';
    END
END

PRINT '';
PRINT '';

-- ============================================================================
-- 5. RESUMEN Y RECOMENDACIONES
-- ============================================================================
PRINT '========================================';
PRINT '  ?? RESUMEN FINAL';
PRINT '========================================';
PRINT '';

DECLARE @erroresEncontrados INT = 0;

-- Verificar AvanceObraManual
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.AvanceObraManual') AND type in (N'U'))
BEGIN
    PRINT '? Falta tabla AvanceObraManual';
    SET @erroresEncontrados = @erroresEncontrados + 1;
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AvanceObraManual') AND name = 'Ejecutado')
    BEGIN
        PRINT '? Falta columna Ejecutado en AvanceObraManual';
        SET @erroresEncontrados = @erroresEncontrados + 1;
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AvanceObraManual') AND name = 'Presupuestado')
    BEGIN
        PRINT '? Falta columna Presupuestado en AvanceObraManual';
        SET @erroresEncontrados = @erroresEncontrados + 1;
    END
END

-- Verificar PresupuestoObra
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.PresupuestoObra') AND type in (N'U'))
BEGIN
    PRINT '? Falta tabla PresupuestoObra';
    SET @erroresEncontrados = @erroresEncontrados + 1;
END
ELSE
BEGIN
    DECLARE @tienePresupuestos INT = 0;
    SELECT @tienePresupuestos = COUNT(*) FROM dbo.PresupuestoObra;
    
    IF @tienePresupuestos = 0
    BEGIN
        PRINT '? Tabla PresupuestoObra está vacía';
        SET @erroresEncontrados = @erroresEncontrados + 1;
    END
END

PRINT '';

IF @erroresEncontrados = 0
BEGIN
    PRINT '? TODAS LAS VERIFICACIONES PASARON';
    PRINT '';
    PRINT 'El sistema está listo para usar.';
    PRINT 'Puedes cargar el avance de obra sin problemas.';
END
ELSE
BEGIN
    PRINT '?? SE ENCONTRARON ' + CAST(@erroresEncontrados AS NVARCHAR(10)) + ' PROBLEMAS';
    PRINT '';
    PRINT '?? SCRIPTS A EJECUTAR EN ORDEN:';
    PRINT '';
    
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.AvanceObraManual') AND type in (N'U'))
        OR NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AvanceObraManual') AND name = 'Ejecutado')
        OR NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AvanceObraManual') AND name = 'Presupuestado')
    BEGIN
        PRINT '   1?? SQL_CORREGIR_TablaAvanceManual.sql';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.PresupuestoObra') AND type in (N'U'))
    BEGIN
        PRINT '   2?? SQL_CrearEImportarPresupuesto.sql';
    END
    ELSE
    BEGIN
        DECLARE @tienePresupuestos2 INT = 0;
        SELECT @tienePresupuestos2 = COUNT(*) FROM dbo.PresupuestoObra;
        
        IF @tienePresupuestos2 = 0
        BEGIN
            PRINT '   2?? SQL_CrearEImportarPresupuesto.sql (para llenar datos)';
        END
    END
END

PRINT '';
PRINT '========================================';
PRINT '  ? DIAGNÓSTICO COMPLETADO';
PRINT '========================================';
GO
