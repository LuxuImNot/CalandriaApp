-- =====================================================
-- SQL_VerificarEstructuraConceptos.sql
-- Verificación de estructura para FormAvanceConcepto
-- =====================================================

PRINT '===== VERIFICACIÓN DE ESTRUCTURA PARA AVANCE POR CONCEPTOS ====='
PRINT ''

-- =====================================================
-- 1. VERIFICAR TABLA PresupuestoObra
-- =====================================================
PRINT '1. Verificando tabla PresupuestoObra...'

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PresupuestoObra')
BEGIN
    PRINT '   ? Tabla PresupuestoObra existe'
    
    -- Verificar columnas necesarias
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PresupuestoObra') AND name = 'Codigo')
        PRINT '   ? Columna Codigo existe'
    ELSE
        PRINT '   ? ERROR: Falta columna Codigo'
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PresupuestoObra') AND name = 'Concepto')
        PRINT '   ? Columna Concepto existe'
    ELSE
        PRINT '   ? ERROR: Falta columna Concepto'
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PresupuestoObra') AND name = 'CostoCalandra')
        PRINT '   ? Columna CostoCalandra existe'
    ELSE
        PRINT '   ? ERROR: Falta columna CostoCalandra'
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PresupuestoObra') AND name = 'CostoTunera')
        PRINT '   ? Columna CostoTunera existe'
    ELSE
        PRINT '   ? ERROR: Falta columna CostoTunera'
    
    -- Mostrar conceptos disponibles
    PRINT ''
    PRINT '   Conceptos encontrados:'
    SELECT 
        Codigo as [Código],
        Concepto,
        COUNT(*) as [Partidas],
        SUM(CostoCalandra) as [Total Calandria],
        SUM(CostoTunera) as [Total Tunera]
    FROM PresupuestoObra
    GROUP BY Codigo, Concepto
    ORDER BY Codigo
    
    DECLARE @TotalConceptos INT
    SELECT @TotalConceptos = COUNT(DISTINCT Codigo) FROM PresupuestoObra
    PRINT ''
    PRINT '   Total de conceptos únicos: ' + CAST(@TotalConceptos AS NVARCHAR(10))
END
ELSE
BEGIN
    PRINT '   ? ERROR: Tabla PresupuestoObra NO EXISTE'
    PRINT '   ? Necesitas crear la tabla PresupuestoObra primero'
END

PRINT ''
PRINT '-- ------------------------------------------------'
PRINT ''

-- =====================================================
-- 2. VERIFICAR TABLA AvanceManualConcepto
-- =====================================================
PRINT '2. Verificando tabla AvanceManualConcepto...'

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AvanceManualConcepto')
BEGIN
    PRINT '   ? Tabla AvanceManualConcepto existe'
    
    -- Contar registros
    DECLARE @TotalRegistros INT
    SELECT @TotalRegistros = COUNT(*) FROM AvanceManualConcepto
    PRINT '   Total de registros: ' + CAST(@TotalRegistros AS NVARCHAR(10))
    
    IF @TotalRegistros > 0
    BEGIN
        PRINT ''
        PRINT '   Últimos 10 avances registrados:'
        SELECT TOP 10
            Manzana,
            Lote,
            Prototipo,
            Codigo,
            Concepto,
            AvancePorcentaje as [Avance %],
            FechaActualizacion as [Fecha]
        FROM AvanceManualConcepto
        ORDER BY FechaActualizacion DESC
    END
END
ELSE
BEGIN
    PRINT '   ? Tabla AvanceManualConcepto NO existe'
    PRINT '   ? Se creará automáticamente al usar el formulario por primera vez'
    PRINT ''
    PRINT '   Script para crear la tabla manualmente:'
    PRINT ''
    PRINT '   CREATE TABLE AvanceManualConcepto ('
    PRINT '       Id INT IDENTITY(1,1) PRIMARY KEY,'
    PRINT '       Manzana NVARCHAR(10),'
    PRINT '       Lote NVARCHAR(10),'
    PRINT '       Prototipo NVARCHAR(50),'
    PRINT '       Codigo NVARCHAR(10),'
    PRINT '       Concepto NVARCHAR(200),'
    PRINT '       AvancePorcentaje FLOAT,'
    PRINT '       FechaActualizacion DATETIME DEFAULT GETDATE()'
    PRINT '   );'
END

PRINT ''
PRINT '-- ------------------------------------------------'
PRINT ''

-- =====================================================
-- 3. VERIFICAR TABLA InventarioCasas
-- =====================================================
PRINT '3. Verificando tabla InventarioCasas...'

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'InventarioCasas')
BEGIN
    PRINT '   ? Tabla InventarioCasas existe'
    
    -- Verificar columnas necesarias
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('InventarioCasas') AND name = 'Manzana')
        PRINT '   ? Columna Manzana existe'
    ELSE
        PRINT '   ? ERROR: Falta columna Manzana'
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('InventarioCasas') AND name = 'Lote')
        PRINT '   ? Columna Lote existe'
    ELSE
        PRINT '   ? ERROR: Falta columna Lote'
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('InventarioCasas') AND name = 'Prototipo')
        PRINT '   ? Columna Prototipo existe'
    ELSE
        PRINT '   ? ERROR: Falta columna Prototipo'
    
    -- Mostrar resumen de casas
    DECLARE @TotalCasas INT
    SELECT @TotalCasas = COUNT(*) FROM InventarioCasas
    PRINT ''
    PRINT '   Total de casas en inventario: ' + CAST(@TotalCasas AS NVARCHAR(10))
    
    PRINT ''
    PRINT '   Prototipos disponibles:'
    SELECT 
        Prototipo,
        COUNT(*) as [Casas]
    FROM InventarioCasas
    GROUP BY Prototipo
    ORDER BY Prototipo
END
ELSE
BEGIN
    PRINT '   ? ERROR: Tabla InventarioCasas NO EXISTE'
    PRINT '   ? FormAvanceConcepto necesita esta tabla para seleccionar casas'
END

PRINT ''
PRINT '-- ------------------------------------------------'
PRINT ''

-- =====================================================
-- 4. VERIFICAR CONSISTENCIA DE DATOS
-- =====================================================
PRINT '4. Verificando consistencia de datos...'

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PresupuestoObra')
   AND EXISTS (SELECT * FROM sys.tables WHERE name = 'AvanceManualConcepto')
BEGIN
    -- Verificar si hay avances de conceptos que ya no existen
    IF EXISTS (
        SELECT 1 
        FROM AvanceManualConcepto a
        WHERE NOT EXISTS (
            SELECT 1 
            FROM PresupuestoObra p 
            WHERE p.Codigo = a.Codigo
        )
    )
    BEGIN
        PRINT '   ? ADVERTENCIA: Hay avances de conceptos que ya no existen en PresupuestoObra'
        SELECT DISTINCT 
            Codigo,
            Concepto,
            COUNT(*) as [Registros Huérfanos]
        FROM AvanceManualConcepto
        WHERE NOT EXISTS (
            SELECT 1 
            FROM PresupuestoObra p 
            WHERE p.Codigo = AvanceManualConcepto.Codigo
        )
        GROUP BY Codigo, Concepto
    END
    ELSE
    BEGIN
        PRINT '   ? Todos los avances registrados corresponden a conceptos existentes'
    END
    
    -- Verificar si hay avances de casas que ya no existen
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'InventarioCasas')
    BEGIN
        IF EXISTS (
            SELECT 1 
            FROM AvanceManualConcepto a
            WHERE NOT EXISTS (
                SELECT 1 
                FROM InventarioCasas i
                WHERE i.Manzana = a.Manzana AND i.Lote = a.Lote
            )
        )
        BEGIN
            PRINT '   ? ADVERTENCIA: Hay avances de casas que ya no existen en InventarioCasas'
            SELECT DISTINCT 
                Manzana,
                Lote,
                COUNT(*) as [Registros Huérfanos]
            FROM AvanceManualConcepto
            WHERE NOT EXISTS (
                SELECT 1 
                FROM InventarioCasas i
                WHERE i.Manzana = AvanceManualConcepto.Manzana 
                  AND i.Lote = AvanceManualConcepto.Lote
            )
            GROUP BY Manzana, Lote
        END
        ELSE
        BEGIN
            PRINT '   ? Todos los avances registrados corresponden a casas existentes'
        END
    END
END

PRINT ''
PRINT '-- ------------------------------------------------'
PRINT ''

-- =====================================================
-- 5. RESUMEN ESTADÍSTICO
-- =====================================================
PRINT '5. Resumen estadístico...'

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AvanceManualConcepto')
   AND EXISTS (SELECT * FROM sys.tables WHERE name = 'PresupuestoObra')
BEGIN
    PRINT ''
    PRINT '   Conceptos con mayor cantidad de avances registrados:'
    SELECT TOP 5
        a.Codigo,
        a.Concepto,
        COUNT(DISTINCT CONCAT(a.Manzana, '-', a.Lote)) as [Casas con Avance],
        AVG(a.AvancePorcentaje) as [Promedio Avance %]
    FROM AvanceManualConcepto a
    GROUP BY a.Codigo, a.Concepto
    ORDER BY COUNT(DISTINCT CONCAT(a.Manzana, '-', a.Lote)) DESC
    
    PRINT ''
    PRINT '   Casas con más conceptos registrados:'
    SELECT TOP 5
        Manzana,
        Lote,
        Prototipo,
        COUNT(*) as [Conceptos con Avance],
        AVG(AvancePorcentaje) as [Promedio Avance %]
    FROM AvanceManualConcepto
    GROUP BY Manzana, Lote, Prototipo
    ORDER BY COUNT(*) DESC
END

PRINT ''
PRINT '-- ------------------------------------------------'
PRINT ''

-- =====================================================
-- 6. RECOMENDACIONES
-- =====================================================
PRINT '6. Recomendaciones:'
PRINT ''

-- Verificar índices
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AvanceManualConcepto')
BEGIN
    IF NOT EXISTS (
        SELECT * FROM sys.indexes 
        WHERE object_id = OBJECT_ID('AvanceManualConcepto') 
        AND name = 'IX_AvanceManualConcepto_ManzanaLote'
    )
    BEGIN
        PRINT '   ? Crear índice para mejorar rendimiento:'
        PRINT '   CREATE INDEX IX_AvanceManualConcepto_ManzanaLote'
        PRINT '   ON AvanceManualConcepto(Manzana, Lote);'
        PRINT ''
    END
    
    IF NOT EXISTS (
        SELECT * FROM sys.indexes 
        WHERE object_id = OBJECT_ID('AvanceManualConcepto') 
        AND name = 'IX_AvanceManualConcepto_Codigo'
    )
    BEGIN
        PRINT '   ? Crear índice para búsquedas por código:'
        PRINT '   CREATE INDEX IX_AvanceManualConcepto_Codigo'
        PRINT '   ON AvanceManualConcepto(Codigo);'
        PRINT ''
    END
END

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PresupuestoObra')
BEGIN
    IF NOT EXISTS (
        SELECT * FROM sys.indexes 
        WHERE object_id = OBJECT_ID('PresupuestoObra') 
        AND name = 'IX_PresupuestoObra_Codigo'
    )
    BEGIN
        PRINT '   ? Crear índice en PresupuestoObra:'
        PRINT '   CREATE INDEX IX_PresupuestoObra_Codigo'
        PRINT '   ON PresupuestoObra(Codigo, Concepto);'
        PRINT ''
    END
END

PRINT ''
PRINT '===== FIN DE VERIFICACIÓN ====='
PRINT ''
PRINT 'Estado del sistema:'
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PresupuestoObra')
   AND EXISTS (SELECT * FROM sys.tables WHERE name = 'InventarioCasas')
BEGIN
    PRINT '? Sistema listo para usar FormAvanceConcepto'
END
ELSE
BEGIN
    PRINT '? Sistema NO está listo - Revisa los errores anteriores'
END
