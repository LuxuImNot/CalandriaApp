-- ============================================================================
-- Crea la tabla ConciliacionesIaLog en una obra existente (en obras nuevas ya
-- viene incluida en ObraPlantilla.sql). Registra, deduplicado por Uuid, cada
-- factura conciliada con el emparejador automático (IA) en
-- /api/ordenescompra/conciliar-factura — sirve para facturar el add-on
-- "Conciliación de factura con IA" a Pilaris ($18 MXN/factura, mínimo
-- $500 MXN/mes por obra).
--
-- Solo agrega una tabla nueva; no toca ninguna tabla ni fila existente.
-- Idempotente: se puede correr varias veces sin duplicar nada.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ConciliacionesIaLog')
BEGIN
    CREATE TABLE ConciliacionesIaLog (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Uuid NVARCHAR(50) NOT NULL,
        FolioOC NVARCHAR(50) NOT NULL,
        Fecha DATETIME NOT NULL DEFAULT GETDATE(),
        Usuario NVARCHAR(100) NULL,
        CONSTRAINT UQ_ConciliacionesIaLog_Uuid UNIQUE (Uuid)
    );
END
