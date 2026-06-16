-- Script alternativo que crea la columna Concepto y sugiere backfill
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AvanceManualObra')
BEGIN
    IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='AvanceManualObra' AND COLUMN_NAME='Concepto')
    BEGIN
        ALTER TABLE AvanceManualObra ADD Concepto NVARCHAR(200) NULL;
    END
END

-- Ejemplo de backfill: asignar el Padre basado en WBS
;WITH Pres AS (
    SELECT ROW_NUMBER() OVER (ORDER BY Padre, Etapa, Partida) AS WBS,
           Padre
    FROM PresupuestoObra
)
UPDATE a
SET a.Concepto = p.Padre
FROM AvanceManualObra a
JOIN Pres p ON TRY_CAST(a.WBS AS INT) = p.WBS
WHERE a.Concepto IS NULL OR a.Concepto = '';
