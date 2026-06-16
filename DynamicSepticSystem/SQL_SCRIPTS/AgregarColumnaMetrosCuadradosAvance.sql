-- =============================================
-- Script: Agregar columna MetrosCuadrados a AvanceManualObra
-- Propósito: Permitir persistir metros cuadrados para partidas dinámicas
-- Fecha: Enero 2025
-- Base de Datos: CALANDRIA
-- =============================================

USE CALANDRIA;
GO

-- Verificar si la columna ya existe
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AvanceManualObra' 
    AND COLUMN_NAME = 'MetrosCuadrados'
)
BEGIN
    PRINT '?? Agregando columna MetrosCuadrados a AvanceManualObra...';
    
    ALTER TABLE dbo.AvanceManualObra
    ADD MetrosCuadrados FLOAT NULL DEFAULT 0.0;
    
    PRINT '? Columna MetrosCuadrados agregada exitosamente.';
    PRINT '';
    PRINT '??  Esta columna permite:';
    PRINT '   - Guardar m² ingresados para partidas dinámicas';
    PRINT '   - Recalcular costos dinámicos al recargar datos';
    PRINT '   - Mantener histórico de mediciones por estimación';
END
ELSE
BEGIN
    PRINT '??  La columna MetrosCuadrados ya existe en AvanceManualObra.';
    PRINT '   No se requieren cambios.';
END
GO

-- Verificar la estructura actualizada
PRINT '';
PRINT '?? Estructura actual de AvanceManualObra:';
SELECT 
    COLUMN_NAME AS Columna,
    DATA_TYPE AS Tipo,
    IS_NULLABLE AS Nullable,
    COLUMN_DEFAULT AS ValorPorDefecto
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AvanceManualObra'
ORDER BY ORDINAL_POSITION;
GO

-- Query de ejemplo para ver partidas dinámicas con m²
PRINT '';
PRINT '?? Ejemplo de consulta para partidas dinámicas:';
PRINT '';
PRINT 'SELECT Manzana, Lote, WBS, Concepto, MetrosCuadrados, MontoEjecutado';
PRINT 'FROM AvanceManualObra';
PRINT 'WHERE MetrosCuadrados > 0 AND CAST(WBS AS INT) > 0;';
PRINT '';
GO

-- =============================================
-- FIN DEL SCRIPT
-- =============================================
