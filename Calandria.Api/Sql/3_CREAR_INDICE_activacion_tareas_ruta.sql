-- ============================================================================
-- Paso separado y opcional. Solo AGREGA una estructura de búsqueda (índice)
-- sobre ActivacionTareasRuta(Manzana, Lote, Ruta); no modifica, no borra y no
-- depende de haber corrido el paso 2 (aunque el índice ayuda más una vez que
-- esos valores ya no tienen espacios de sobra).
-- Se puede correr y no pasa nada malo si ya existe (el IF NOT EXISTS lo evita).
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ActivacionTareasRuta_Manzana_Lote_Ruta'
      AND object_id = OBJECT_ID('dbo.ActivacionTareasRuta')
)
BEGIN
    CREATE INDEX IX_ActivacionTareasRuta_Manzana_Lote_Ruta
        ON dbo.ActivacionTareasRuta (Manzana, Lote, Ruta)
        INCLUDE (NodoID, Activa, CuadrillaAsignada, DesatajoActivado, Finalizado,
                 FechaActivacion, FechaFinalizacion, FechaActualizacion);
END
