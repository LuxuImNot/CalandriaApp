/*
  Centralizacion de insumos · FASE 0 — DIAGNOSTICO (solo lectura).

  El catalogo canonico es el de DESTAJOS: InsumosTuneraEXP / InsumosCalandraEXP.
  Compras (COMPRASTUNERA/COMPRASCALANDRA/COMPRASINDIRECTAS) y Almacen se van a
  reapuntar a el. Regla: Insumos*EXP DOMINA (ante divergencia gana Insumos*EXP).

  Este script NO modifica nada. Solo reporta el grado de desorden para dimensionar
  el backfill (FASE 1). Ejecutar y revisar cada result set.

  Archivo UTF-8 con BOM (contiene la columna acentuada [Descripción]).
*/
-- Ejecutar conectado a la BD CALANDRIA (el .bat lo hace con: sqlcmd -d CALANDRIA).
SET NOCOUNT ON;

PRINT '=== 1. Claves en COMPRAS* que NO existen en Insumos*EXP (entran al backfill) ===';

IF OBJECT_ID(N'dbo.COMPRASTUNERA') IS NOT NULL AND OBJECT_ID(N'dbo.InsumosTuneraEXP') IS NOT NULL
    SELECT 'TUNERA' AS Catalogo, c.Clave, c.Descripcion, c.Unidad
    FROM dbo.COMPRASTUNERA c
    WHERE NOT EXISTS (SELECT 1 FROM dbo.InsumosTuneraEXP i WHERE i.Clave = c.Clave)
    ORDER BY c.Clave;

IF OBJECT_ID(N'dbo.COMPRASCALANDRA') IS NOT NULL AND OBJECT_ID(N'dbo.InsumosCalandraEXP') IS NOT NULL
    SELECT 'CALANDRA' AS Catalogo, c.Clave, c.Descripcion, c.Unidad
    FROM dbo.COMPRASCALANDRA c
    WHERE NOT EXISTS (SELECT 1 FROM dbo.InsumosCalandraEXP i WHERE i.Clave = c.Clave)
    ORDER BY c.Clave;

PRINT '=== 2. Claves con costo/unidad/descripcion DIVERGENTE (informativo: gana Insumos*EXP) ===';

IF OBJECT_ID(N'dbo.COMPRASTUNERA') IS NOT NULL AND OBJECT_ID(N'dbo.InsumosTuneraEXP') IS NOT NULL
    SELECT 'TUNERA' AS Catalogo, i.Clave,
           i.[Descripción] AS Desc_EXP, c.Descripcion AS Desc_COMPRAS,
           i.Costo AS Costo_EXP, c.Costo AS Costo_COMPRAS,
           i.Unidad AS Unidad_EXP, c.Unidad AS Unidad_COMPRAS
    FROM dbo.InsumosTuneraEXP i
    INNER JOIN dbo.COMPRASTUNERA c ON c.Clave = i.Clave
    WHERE ISNULL(i.Costo,0) <> ISNULL(c.Costo,0)
       OR ISNULL(i.Unidad,'') <> ISNULL(c.Unidad,'')
       OR ISNULL(i.[Descripción],'') <> ISNULL(c.Descripcion,'')
    ORDER BY i.Clave;

IF OBJECT_ID(N'dbo.COMPRASCALANDRA') IS NOT NULL AND OBJECT_ID(N'dbo.InsumosCalandraEXP') IS NOT NULL
    SELECT 'CALANDRA' AS Catalogo, i.Clave,
           i.[Descripción] AS Desc_EXP, c.Descripcion AS Desc_COMPRAS,
           i.Costo AS Costo_EXP, c.Costo AS Costo_COMPRAS,
           i.Unidad AS Unidad_EXP, c.Unidad AS Unidad_COMPRAS
    FROM dbo.InsumosCalandraEXP i
    INNER JOIN dbo.COMPRASCALANDRA c ON c.Clave = i.Clave
    WHERE ISNULL(i.Costo,0) <> ISNULL(c.Costo,0)
       OR ISNULL(i.Unidad,'') <> ISNULL(c.Unidad,'')
       OR ISNULL(i.[Descripción],'') <> ISNULL(c.Descripcion,'')
    ORDER BY i.Clave;

PRINT '=== 3. COMPRASINDIRECTAS no representadas en Insumos*EXP (entran al backfill como Tipo=Indirecto) ===';

IF OBJECT_ID(N'dbo.COMPRASINDIRECTAS') IS NOT NULL
   AND OBJECT_ID(N'dbo.InsumosTuneraEXP') IS NOT NULL
   AND OBJECT_ID(N'dbo.InsumosCalandraEXP') IS NOT NULL
    SELECT ind.Clave, ind.Descripcion, ind.Unidad,
           EnTunera   = CASE WHEN EXISTS (SELECT 1 FROM dbo.InsumosTuneraEXP   i WHERE i.Clave = ind.Clave) THEN 1 ELSE 0 END,
           EnCalandra = CASE WHEN EXISTS (SELECT 1 FROM dbo.InsumosCalandraEXP i WHERE i.Clave = ind.Clave) THEN 1 ELSE 0 END
    FROM dbo.COMPRASINDIRECTAS ind
    ORDER BY ind.Clave;
ELSE IF OBJECT_ID(N'dbo.COMPRASINDIRECTAS') IS NOT NULL
    PRINT '   (omitido: falta InsumosTuneraEXP o InsumosCalandraEXP; el backfill las crea, vuelve a correr el diagnostico despues.)';

PRINT '=== 4. Nodos de destajo (Nivel 2) sin Clave o con clave huerfana ===';

IF OBJECT_ID(N'dbo.RutaTuneraDestajo') IS NOT NULL
   AND OBJECT_ID(N'dbo.RutaTuneraDestajo_Columnas') IS NOT NULL
    SELECT n.ID, n.Nombre,
           Clave = col.Valor,
           Estado = CASE
                      WHEN col.Valor IS NULL OR LTRIM(RTRIM(col.Valor)) = '' THEN 'SIN CLAVE'
                      WHEN NOT EXISTS (SELECT 1 FROM dbo.InsumosTuneraEXP i WHERE i.Clave = col.Valor) THEN 'CLAVE HUERFANA'
                      ELSE 'OK'
                    END
    FROM dbo.RutaTuneraDestajo n
    LEFT JOIN dbo.RutaTuneraDestajo_Columnas col
           ON col.NodoID = n.ID AND col.NombreColumna = N'Clave'
    WHERE n.Nivel = 2
      AND (col.Valor IS NULL OR LTRIM(RTRIM(col.Valor)) = ''
           OR NOT EXISTS (SELECT 1 FROM dbo.InsumosTuneraEXP i WHERE i.Clave = col.Valor))
    ORDER BY Estado, n.Nombre;

PRINT '=== 5. Movimientos de almacen con Clave vacia o huerfana ===';
-- Requiere ambos catalogos EXP para evaluar "huerfana" contra el catalogo unico.
IF OBJECT_ID(N'dbo.InsumosTuneraEXP') IS NULL OR OBJECT_ID(N'dbo.InsumosCalandraEXP') IS NULL
    PRINT '   (omitido: falta InsumosTuneraEXP o InsumosCalandraEXP; el backfill las crea, vuelve a correr el diagnostico despues.)';
ELSE
BEGIN
    IF OBJECT_ID(N'dbo.SalidasAlmacen') IS NOT NULL
        SELECT 'SalidasAlmacen' AS Tabla, ISNULL(Clave,'') AS Clave,
               MAX(Descripcion) AS Descripcion, COUNT(*) AS Filas
        FROM dbo.SalidasAlmacen
        WHERE Clave IS NULL OR LTRIM(RTRIM(Clave)) = ''
           OR (NOT EXISTS (SELECT 1 FROM dbo.InsumosTuneraEXP   i WHERE i.Clave = SalidasAlmacen.Clave)
           AND NOT EXISTS (SELECT 1 FROM dbo.InsumosCalandraEXP i WHERE i.Clave = SalidasAlmacen.Clave))
        GROUP BY ISNULL(Clave,'');

    IF OBJECT_ID(N'dbo.EntradasAlmacen') IS NOT NULL
        SELECT 'EntradasAlmacen' AS Tabla, ISNULL(Clave,'') AS Clave,
               COUNT(*) AS Filas
        FROM dbo.EntradasAlmacen
        WHERE Clave IS NULL OR LTRIM(RTRIM(Clave)) = ''
           OR (NOT EXISTS (SELECT 1 FROM dbo.InsumosTuneraEXP   i WHERE i.Clave = EntradasAlmacen.Clave)
           AND NOT EXISTS (SELECT 1 FROM dbo.InsumosCalandraEXP i WHERE i.Clave = EntradasAlmacen.Clave))
        GROUP BY ISNULL(Clave,'');
END

PRINT '=== Diagnostico completado. Revisa los result sets antes de correr FASE 1 (backfill). ===';
GO
