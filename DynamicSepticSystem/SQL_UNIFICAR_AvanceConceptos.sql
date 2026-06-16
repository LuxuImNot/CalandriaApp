-- =============================================
-- ?? UNIFICACIÓN DE TABLAS DE AVANCE POR CONCEPTOS
-- =============================================
-- Propósito: Migrar datos de AvanceEstimacionConcepto
--            hacia AvanceManualConcepto para tener 
--            UNA SOLA fuente de verdad
-- =============================================

-- PASO 1: Verificar si existen datos en la tabla antigua
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AvanceEstimacionConcepto')
BEGIN
    PRINT '? Tabla AvanceEstimacionConcepto existe'
    
    SELECT COUNT(*) as TotalRegistros 
    FROM AvanceEstimacionConcepto
    
    -- PASO 2: Migrar datos globales con marca especial
    -- Usar Manzana='GLOBAL', Lote='GLOBAL' para diferenciarlos
    INSERT INTO AvanceManualConcepto 
        (Manzana, Lote, Prototipo, Codigo, Concepto, AvancePorcentaje, FechaActualizacion)
    SELECT 
        'GLOBAL' as Manzana,
        'GLOBAL' as Lote,
        'GLOBAL' as Prototipo,
        Codigo,
        Concepto,
        AvancePorcentaje,
        COALESCE(FechaActualizacion, GETDATE())
    FROM AvanceEstimacionConcepto
    WHERE Codigo NOT IN (
        -- No migrar si ya existe un registro específico de casa
        SELECT DISTINCT Codigo 
        FROM AvanceManualConcepto 
        WHERE Manzana <> 'GLOBAL' AND Lote <> 'GLOBAL'
    )
    
    PRINT '? Datos migrados a AvanceManualConcepto con marca GLOBAL'
    
    -- PASO 3: Renombrar tabla antigua (por si se necesita recuperar)
    EXEC sp_rename 'AvanceEstimacionConcepto', 'AvanceEstimacionConcepto_BACKUP'
    
    PRINT '? Tabla antigua renombrada a AvanceEstimacionConcepto_BACKUP'
    PRINT '??  Revisar los datos migrados antes de eliminar el backup'
END
ELSE
BEGIN
    PRINT '??  Tabla AvanceEstimacionConcepto no existe (ya fue migrada o no se creó)'
END

-- PASO 4: Verificar migración
PRINT ''
PRINT '?? RESUMEN DE DATOS EN AvanceManualConcepto:'
PRINT '--------------------------------------------'

SELECT 
    CASE 
        WHEN Manzana = 'GLOBAL' AND Lote = 'GLOBAL' THEN 'Globales (migrados)'
        ELSE 'Específicos por casa'
    END as TipoDato,
    COUNT(*) as TotalRegistros
FROM AvanceManualConcepto
GROUP BY 
    CASE 
        WHEN Manzana = 'GLOBAL' AND Lote = 'GLOBAL' THEN 'Globales (migrados)'
        ELSE 'Específicos por casa'
    END

-- PASO 5: Crear vista para consultas consolidadas
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_AvanceConceptosConsolidado')
    DROP VIEW vw_AvanceConceptosConsolidado
GO

CREATE VIEW vw_AvanceConceptosConsolidado AS
SELECT 
    amc.Manzana,
    amc.Lote,
    amc.Prototipo,
    amc.Codigo,
    amc.Concepto,
    amc.AvancePorcentaje,
    CASE 
        WHEN amc.Manzana = 'GLOBAL' AND amc.Lote = 'GLOBAL' THEN 'Global'
        ELSE 'Específico'
    END as TipoAvance,
    amc.FechaActualizacion
FROM AvanceManualConcepto amc
GO

PRINT '? Vista vw_AvanceConceptosConsolidado creada'

-- PASO 6: Ejemplo de consulta para ver avance de una casa específica
PRINT ''
PRINT '?? EJEMPLO: Consultar avance de una casa específica'
PRINT '----------------------------------------------------'
PRINT 'SELECT * FROM vw_AvanceConceptosConsolidado'
PRINT 'WHERE Manzana = ''tu_manzana'' AND Lote = ''tu_lote'''
PRINT 'ORDER BY CAST(Codigo AS INT)'
PRINT ''

-- PASO 7: Para eliminar el backup (SOLO después de verificar)
PRINT '??  IMPORTANTE: Antes de eliminar el backup, verifica que todos los datos'
PRINT '   se migraron correctamente. Para eliminar ejecuta:'
PRINT '   DROP TABLE AvanceEstimacionConcepto_BACKUP'
PRINT ''
