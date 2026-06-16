-- Agrega columnas Concepto e ImporteTotal a AvanceManualObra y realiza backfill desde PresupuestoObra usando WBS
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AvanceManualObra')
BEGIN
    IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='AvanceManualObra' AND COLUMN_NAME='Concepto')
    BEGIN
        ALTER TABLE AvanceManualObra ADD Concepto NVARCHAR(200) NULL;
    END

    IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='AvanceManualObra' AND COLUMN_NAME='ImporteTotal')
    BEGIN
        ALTER TABLE AvanceManualObra ADD ImporteTotal FLOAT NULL;
    END
END

-- Backfill: reconstruir WBS desde PresupuestoObra y actualizar Concepto (Padre) e ImporteTotal
;WITH Pres AS (
    SELECT ROW_NUMBER() OVER (ORDER BY Padre, Etapa, Partida) AS WBS,
           Padre,
           CostoCalandra AS ImporteCalandria,
           CostoTunera AS ImporteTunera
    FROM PresupuestoObra
)
UPDATE a
SET a.Concepto = p.Padre,
    a.ImporteTotal = CASE WHEN a.Prototipo IS NOT NULL AND UPPER(a.Prototipo) LIKE '%CALANDR%' THEN p.ImporteCalandria ELSE p.ImporteTunera END
FROM AvanceManualObra a
INNER JOIN Pres p ON TRY_CAST(a.WBS AS INT) = p.WBS
WHERE (a.Concepto IS NULL OR a.Concepto = '') OR (a.ImporteTotal IS NULL);

-- Nota: revisa Prototipo valores si tu naming difiere (CALANDRIA / TUNERA)
