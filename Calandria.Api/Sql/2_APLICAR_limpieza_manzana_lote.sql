-- ============================================================================
-- Antes de correr esto: corre 1_PREVISUALIZAR_limpieza_manzana_lote.sql (solo
-- lectura) y mira cuántas filas y cuáles saldrían afectadas. Si el número te
-- parece razonable, sigue con este.
--
-- Qué SÍ hace (y nada más que esto):
--   UPDATE de 2 tablas (ActivacionTareasRuta, SalidasAlmacen): reescribe SOLO
--   las columnas Manzana/Lote/Ruta quitándoles espacios de más al inicio/final
--   (ej. "A1 " -> "A1"). No toca ninguna otra columna ni ninguna otra tabla.
--
-- No hay ningún DROP, DELETE ni TRUNCATE en este archivo. Ninguna fila se
-- elimina; solo se les recorta el espacio sobrante a 2-3 columnas de texto.
--
-- CÓMO CORRERLO DE FORMA SEGURA (recomendado en SSMS):
--   1. Selecciona y ejecuta SOLO hasta la línea "-- FIN DE LOS UPDATE" (no todo
--      el archivo de un jalón). Vas a ver "(N row(s) affected)" para cada
--      UPDATE.
--   2. Compara esos números contra lo que viste en la previsualización.
--   3. Si coinciden: selecciona y ejecuta la línea "COMMIT TRAN;" (al final,
--      comentada) para confirmar.
--      Si algo no cuadra: ejecuta "ROLLBACK TRAN;" en su lugar y queda todo
--      exactamente como estaba, como si nunca hubieras corrido nada.
--   La transacción se queda abierta hasta que hagas COMMIT o ROLLBACK a
--   propósito — no hay paso automático que confirme el cambio por ti.
--
-- El índice (paso separado, opcional, siempre seguro: solo agrega una
-- estructura de búsqueda, no toca datos) está en
-- 3_CREAR_INDICE_activacion_tareas_ruta.sql.
-- ============================================================================

BEGIN TRAN;

UPDATE dbo.ActivacionTareasRuta
   SET Manzana = LTRIM(RTRIM(Manzana)),
       Lote    = LTRIM(RTRIM(Lote)),
       Ruta    = LTRIM(RTRIM(Ruta))
 WHERE Manzana <> LTRIM(RTRIM(Manzana))
    OR Lote    <> LTRIM(RTRIM(Lote))
    OR Ruta    <> LTRIM(RTRIM(Ruta));

UPDATE dbo.SalidasAlmacen
   SET Manzana = LTRIM(RTRIM(Manzana)),
       Lote    = LTRIM(RTRIM(Lote))
 WHERE Manzana <> LTRIM(RTRIM(Manzana))
    OR Lote    <> LTRIM(RTRIM(Lote));

-- FIN DE LOS UPDATE. Revisa los "row(s) affected" de arriba contra la
-- previsualización antes de seguir.

-- Descomenta UNA de las dos líneas siguientes y corre solo esa:
-- COMMIT TRAN;
-- ROLLBACK TRAN;
