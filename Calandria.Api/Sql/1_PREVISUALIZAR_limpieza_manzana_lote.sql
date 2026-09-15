-- ============================================================================
-- SOLO LECTURA. No modifica nada. Córrelo primero para ver EXACTAMENTE qué
-- filas tocaría el script de limpieza (2_APLICAR_limpieza_manzana_lote.sql)
-- antes de correr ese.
--
-- Qué hace: cuenta y muestra las filas donde Manzana/Lote/Ruta tienen espacios
-- de más al inicio o al final (ej. "A1 " en vez de "A1"). Nada más.
-- ============================================================================

-- Cuántas filas de ActivacionTareasRuta tienen espacios de sobra:
SELECT COUNT(*) AS Filas_ActivacionTareasRuta_con_espacios
FROM dbo.ActivacionTareasRuta
WHERE Manzana <> LTRIM(RTRIM(Manzana))
   OR Lote    <> LTRIM(RTRIM(Lote))
   OR Ruta    <> LTRIM(RTRIM(Ruta));

-- Cuáles son (hasta 50, para ver el patrón):
SELECT TOP 50 NodoID,
       Manzana AS Manzana_actual, '[' + Manzana + ']' AS Manzana_con_limites,
       Lote    AS Lote_actual,    '[' + Lote    + ']' AS Lote_con_limites,
       Ruta    AS Ruta_actual,    '[' + Ruta    + ']' AS Ruta_con_limites
FROM dbo.ActivacionTareasRuta
WHERE Manzana <> LTRIM(RTRIM(Manzana))
   OR Lote    <> LTRIM(RTRIM(Lote))
   OR Ruta    <> LTRIM(RTRIM(Ruta));

-- Lo mismo para SalidasAlmacen:
SELECT COUNT(*) AS Filas_SalidasAlmacen_con_espacios
FROM dbo.SalidasAlmacen
WHERE Manzana <> LTRIM(RTRIM(Manzana))
   OR Lote    <> LTRIM(RTRIM(Lote));

SELECT TOP 50 *,
       '[' + Manzana + ']' AS Manzana_con_limites,
       '[' + Lote    + ']' AS Lote_con_limites
FROM dbo.SalidasAlmacen
WHERE Manzana <> LTRIM(RTRIM(Manzana))
   OR Lote    <> LTRIM(RTRIM(Lote));
