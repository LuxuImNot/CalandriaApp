-- =====================================================
-- Script: AgregarColumnaLimiteM2.sql
-- Descripción: Agrega la columna LimiteM2 a la tabla PresupuestoObra
--              para establecer límites máximos de m² en partidas dinámicas
-- Autor: GitHub Copilot
-- Fecha: 2024
-- =====================================================

-- Verificar si la columna ya existe antes de agregarla
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PresupuestoObra' 
    AND COLUMN_NAME = 'LimiteM2'
)
BEGIN
    -- Agregar columna LimiteM2
    ALTER TABLE PresupuestoObra
    ADD LimiteM2 FLOAT NULL DEFAULT 0;
    
    PRINT '? Columna LimiteM2 agregada exitosamente a PresupuestoObra';
END
ELSE
BEGIN
    PRINT '? La columna LimiteM2 ya existe en PresupuestoObra';
END

GO

-- =====================================================
-- DESCRIPCIÓN DE LA COLUMNA:
-- =====================================================
-- LimiteM2: Límite máximo de metros cuadrados para partidas dinámicas
--
-- Comportamiento:
-- - Si LimiteM2 = 0 o NULL: Sin límite, usa todos los m² del proyecto
-- - Si LimiteM2 > 0: El costo se calcula con el MÍNIMO entre:
--                    * m² del proyecto
--                    * LimiteM2
--
-- Ejemplo de uso:
-- Si una partida tiene:
--   - ValorM2Tunera = $100/m²
--   - LimiteM2 = 50 m²
--   - Proyecto tiene 120 m²
--
-- El costo se calculará como: $100 × 50 = $5,000 (no $12,000)
--
-- =====================================================
-- VERIFICACIÓN
-- =====================================================
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PresupuestoObra'
AND COLUMN_NAME IN ('EsDinamica', 'ValorM2Tunera', 'ValorM2Calandra', 'LimiteM2')
ORDER BY COLUMN_NAME;
