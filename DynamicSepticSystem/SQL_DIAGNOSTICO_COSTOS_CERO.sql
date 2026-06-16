-- ============================================================================
-- DIAGNÓSTICO Y SOLUCIÓN: Costos en $0.00
-- ============================================================================

USE CALANDRIA;
GO

PRINT '?? DIAGNÓSTICO: ¿Por qué los costos están en $0.00?';
PRINT '========================================';
PRINT '';

-- ============================================================================
-- 1. VERIFICAR SI EXISTE TABLA PresupuestoObra
-- ============================================================================
PRINT '1?? Verificando tabla PresupuestoObra...';
PRINT '';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.PresupuestoObra') AND type in (N'U'))
BEGIN
    PRINT '? ERROR: La tabla PresupuestoObra NO EXISTE';
    PRINT '';
    PRINT '?? SOLUCIÓN:';
    PRINT '   Ejecuta: SQL_CrearEImportarPresupuesto.sql';
    PRINT '';
    PRINT '========================================';
    PRINT '? NO SE PUEDE CONTINUAR SIN LA TABLA';
    PRINT '========================================';
END
ELSE
BEGIN
    PRINT '? La tabla PresupuestoObra existe';
    PRINT '';
    
    -- ============================================================================
    -- 2. VERIFICAR SI HAY DATOS
    -- ============================================================================
    PRINT '2?? Verificando si hay datos en la tabla...';
    PRINT '';
    
    DECLARE @totalRegistros INT = 0;
    SELECT @totalRegistros = COUNT(*) FROM dbo.PresupuestoObra;
    
    IF @totalRegistros = 0
    BEGIN
        PRINT '? ERROR: La tabla PresupuestoObra está VACÍA';
        PRINT '';
        PRINT '?? SOLUCIÓN:';
        PRINT '   Opción 1: Ejecuta SQL_IMPORTAR_SIMPLE.sql (si tienes Excel)';
        PRINT '   Opción 2: Inserta datos manualmente (ver script al final)';
        PRINT '';
    END
    ELSE
    BEGIN
        PRINT '? La tabla tiene ' + CAST(@totalRegistros AS NVARCHAR(10)) + ' registros';
        PRINT '';
        
        -- ============================================================================
        -- 3. VERIFICAR PROTOTIPOS DISPONIBLES
        -- ============================================================================
        PRINT '3?? Prototipos disponibles en PresupuestoObra:';
        PRINT '';
        
        SELECT DISTINCT Prototipo, COUNT(*) AS Conceptos
        FROM dbo.PresupuestoObra
        GROUP BY Prototipo
        ORDER BY Prototipo;
        
        PRINT '';
        
        -- ============================================================================
        -- 4. VERIFICAR SI EXISTE PROTOTIPO "TUNERA"
        -- ============================================================================
        PRINT '4?? ¿Existe el prototipo TUNERA?';
        PRINT '';
        
        DECLARE @tieneTunera INT = 0;
        SELECT @tieneTunera = COUNT(DISTINCT Prototipo) 
        FROM dbo.PresupuestoObra 
        WHERE Prototipo = 'TUNERA';
        
        IF @tieneTunera = 0
        BEGIN
            PRINT '? ERROR: NO existe el prototipo "TUNERA" en PresupuestoObra';
            PRINT '';
            
            -- Buscar prototipos similares
            PRINT '?? Prototipos disponibles:';
            SELECT DISTINCT Prototipo FROM dbo.PresupuestoObra ORDER BY Prototipo;
            
            PRINT '';
            PRINT '?? SOLUCIONES POSIBLES:';
            PRINT '';
            PRINT '   Opción A: Actualizar el prototipo de la casa en InventarioCasas';
            PRINT '   Ejemplo:';
            PRINT '   UPDATE InventarioCasas ';
            PRINT '   SET Prototipo = ''TU_PROTOTIPO_REAL'' ';
            PRINT '   WHERE Manzana = ''5'' AND Lote = ''45'';';
            PRINT '';
            PRINT '   Opción B: Insertar datos de presupuesto para TUNERA';
            PRINT '   (Ver script al final de este diagnóstico)';
            PRINT '';
        END
        ELSE
        BEGIN
            PRINT '? Existe el prototipo TUNERA';
            PRINT '';
            
            -- ============================================================================
            -- 5. VERIFICAR CATEGORÍAS PARA TUNERA
            -- ============================================================================
            PRINT '5?? Categorías disponibles para TUNERA:';
            PRINT '';
            
            SELECT 
                Categoria,
                COUNT(*) AS Conceptos,
                SUM(ISNULL(CostoTotal, 0)) AS CostoTotal
            FROM dbo.PresupuestoObra
            WHERE Prototipo = 'TUNERA'
            GROUP BY Categoria
            ORDER BY Categoria;
            
            PRINT '';
            
            -- Verificar si hay costos
            DECLARE @totalCosto DECIMAL(18,2) = 0;
            SELECT @totalCosto = SUM(ISNULL(CostoTotal, 0))
            FROM dbo.PresupuestoObra
            WHERE Prototipo = 'TUNERA';
            
            IF @totalCosto = 0
            BEGIN
                PRINT '?? ADVERTENCIA: Todas las categorías de TUNERA tienen CostoTotal = 0';
                PRINT '';
                PRINT '?? SOLUCIÓN:';
                PRINT '   Las categorías existen pero no tienen costos asignados.';
                PRINT '   Ejecuta el script de inserción al final de este diagnóstico.';
                PRINT '';
            END
            ELSE
            BEGIN
                PRINT '? El prototipo TUNERA tiene costos asignados: ' + CAST(@totalCosto AS NVARCHAR(20));
                PRINT '';
                PRINT '?? Si aún ves $0.00 en la aplicación, el problema puede ser:';
                PRINT '   1. La casa en InventarioCasas NO tiene Prototipo = ''TUNERA''';
                PRINT '   2. Hay espacios o mayúsculas/minúsculas diferentes';
                PRINT '';
                PRINT '   Verifica:';
                
                -- Verificar casa específica
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.InventarioCasas') AND type in (N'U'))
                BEGIN
                    PRINT '';
                    PRINT '   SELECT Manzana, Lote, Prototipo ';
                    PRINT '   FROM InventarioCasas ';
                    PRINT '   WHERE Manzana = ''5'' AND Lote = ''45'';';
                    PRINT '';
                    
                    SELECT Manzana, Lote, Prototipo, LEN(Prototipo) AS LongitudPrototipo
                    FROM InventarioCasas 
                    WHERE Manzana = '5' AND Lote = '45';
                END
            END
        END
    END
END

PRINT '';
PRINT '========================================';
PRINT '  ?? RESUMEN Y SOLUCIONES';
PRINT '========================================';
PRINT '';

-- ============================================================================
-- RESUMEN FINAL
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.PresupuestoObra') AND type in (N'U'))
BEGIN
    PRINT '? PROBLEMA: Tabla PresupuestoObra no existe';
    PRINT '? SOLUCIÓN: Ejecuta SQL_CrearEImportarPresupuesto.sql';
END
ELSE
BEGIN
    DECLARE @totalReg INT = 0;
    SELECT @totalReg = COUNT(*) FROM dbo.PresupuestoObra;
    
    IF @totalReg = 0
    BEGIN
        PRINT '? PROBLEMA: Tabla PresupuestoObra está vacía';
        PRINT '? SOLUCIÓN: Ejecuta el script de inserción de datos (ver abajo)';
    END
    ELSE
    BEGIN
        DECLARE @tieneTuneraFinal INT = 0;
        SELECT @tieneTuneraFinal = COUNT(*) 
        FROM dbo.PresupuestoObra 
        WHERE Prototipo = 'TUNERA';
        
        IF @tieneTuneraFinal = 0
        BEGIN
            PRINT '? PROBLEMA: No existe prototipo TUNERA en PresupuestoObra';
            PRINT '? SOLUCIÓN 1: Cambiar prototipo de la casa al disponible';
            PRINT '? SOLUCIÓN 2: Insertar datos para TUNERA (ver script abajo)';
        END
        ELSE
        BEGIN
            DECLARE @totalCostoFinal DECIMAL(18,2) = 0;
            SELECT @totalCostoFinal = SUM(ISNULL(CostoTotal, 0))
            FROM dbo.PresupuestoObra
            WHERE Prototipo = 'TUNERA';
            
            IF @totalCostoFinal = 0
            BEGIN
                PRINT '? PROBLEMA: TUNERA existe pero todos los costos son $0.00';
                PRINT '? SOLUCIÓN: Actualizar costos (ver script abajo)';
            END
            ELSE
            BEGIN
                PRINT '? TODO CORRECTO EN BASE DE DATOS';
                PRINT '';
                PRINT '?? El problema puede estar en la aplicación:';
                PRINT '   1. Verifica que el Prototipo en InventarioCasas coincida exactamente';
                PRINT '   2. Reinicia la aplicación completamente';
                PRINT '   3. Verifica la cadena de conexión en App.config';
            END
        END
    END
END

PRINT '';
PRINT '';

-- ============================================================================
-- SCRIPT DE INSERCIÓN DE DATOS DE EJEMPLO PARA TUNERA
-- ============================================================================
PRINT '========================================';
PRINT '  ?? SCRIPT DE INSERCIÓN DE DATOS';
PRINT '========================================';
PRINT '';
PRINT '-- Si necesitas insertar datos para el prototipo TUNERA:';
PRINT '';
PRINT '-- Ejecuta este script:';
PRINT '';

-- Comentado para no ejecutarse automáticamente
PRINT '/*';
PRINT 'USE CALANDRIA;';
PRINT 'GO';
PRINT '';
PRINT '-- Insertar datos de ejemplo para TUNERA';
PRINT 'INSERT INTO PresupuestoObra (Prototipo, Categoria, CostoTotal)';
PRINT 'VALUES ';
PRINT '    (''TUNERA'', ''Preliminares'', 45000.00),';
PRINT '    (''TUNERA'', ''Cimentación'', 120000.00),';
PRINT '    (''TUNERA'', ''Muros Planta Baja'', 180000.00),';
PRINT '    (''TUNERA'', ''Muros Planta Alta'', 150000.00),';
PRINT '    (''TUNERA'', ''Entrepiso'', 95000.00),';
PRINT '    (''TUNERA'', ''Losas'', 110000.00),';
PRINT '    (''TUNERA'', ''Azotea'', 85000.00),';
PRINT '    (''TUNERA'', ''Inst. Hidráulica, Sanitaria y Gas LP'', 75000.00),';
PRINT '    (''TUNERA'', ''Inst. Eléctrica'', 65000.00),';
PRINT '    (''TUNERA'', ''Albañilería Planta Baja'', 55000.00),';
PRINT '    (''TUNERA'', ''Albañilería Planta Alta'', 50000.00),';
PRINT '    (''TUNADOS Interiores'', 125000.00),';
PRINT '    (''TUNERA'', ''Acabados Exteriores'', 95000.00),';
PRINT '    (''TUNERA'', ''Herrería, Aluminio y Vidrio'', 70000.00),';
PRINT '    (''TUNERA'', ''Carpintería y Cancelería'', 60000.00),';
PRINT '    (''TUNERA'', ''Muebles y Accesorios'', 45000.00),';
PRINT '    (''TUNERA'', ''Obra Exterior'', 80000.00),';
PRINT '    (''TUNERA'', ''Urbanización'', 55000.00);';
PRINT '';
PRINT 'PRINT ''? Datos de ejemplo insertados para TUNERA'';';
PRINT 'GO';
PRINT '*/';
PRINT '';

-- ============================================================================
-- SCRIPT ALTERNATIVO: COPIAR DESDE OTRO PROTOTIPO
-- ============================================================================
PRINT '';
PRINT '========================================';
PRINT '  ?? ALTERNATIVA: COPIAR DE OTRO PROTOTIPO';
PRINT '========================================';
PRINT '';
PRINT '-- Si ya tienes datos de otro prototipo y quieres copiarlos a TUNERA:';
PRINT '';
PRINT '/*';
PRINT '-- Reemplaza ''PROTOTIPO_ORIGEN'' con el nombre del prototipo que quieres copiar';
PRINT 'INSERT INTO PresupuestoObra (Prototipo, Categoria, CostoTotal)';
PRINT 'SELECT ''TUNERA'' AS Prototipo, Categoria, CostoTotal';
PRINT 'FROM PresupuestoObra';
PRINT 'WHERE Prototipo = ''PROTOTIPO_ORIGEN'';';
PRINT '';
PRINT 'PRINT ''? Datos copiados desde PROTOTIPO_ORIGEN a TUNERA'';';
PRINT '*/';
PRINT '';

PRINT '========================================';
PRINT '  ? DIAGNÓSTICO COMPLETADO';
PRINT '========================================';
GO
