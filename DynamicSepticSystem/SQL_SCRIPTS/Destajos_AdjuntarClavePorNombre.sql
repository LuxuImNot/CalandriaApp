/*
==============================================================================
  Destajos · ADJUNTAR CLAVE DEL CATALOGO POR NOMBRE
==============================================================================
  Compara los insumos de DESTAJO (nodos Material) contra el catalogo canonico
  Insumos*EXP y, cuando el nombre coincide, adjunta la Clave del catalogo al
  nodo de destajo (columna 'Clave' en *_Columnas).

  Modelo:
    - Insumo de destajo = nodo con TipoTarea = 1 en RutaTuneraDestajo /
      RutaCalandraDestajo. Su nombre vive en r.Nombre y su clave (si la tiene)
      en *_Columnas con NombreColumna = 'Clave'.
    - Catalogo canonico  = InsumosTuneraEXP / InsumosCalandraEXP. El nombre del
      insumo es la columna [Descripcion] (acentuada) y la clave es [Clave].
    - Emparejamiento por NOMBRE normalizado: mayusculas, sin acentos y espacios
      colapsados. Aqui se logra con la colacion Latin1_General_CI_AI (case- e
      acento-insensitive) + colapso de espacios. Es la misma regla que usa el
      API (ComprasController.NormNombre / CatalogoDestajos).

  Alcance (decidido): SOLO nodos SIN clave (sin fila 'Clave' o con Valor vacio).
    NO se tocan nodos que ya tienen clave, ni siquiera si fuera huerfana.

  Ambiguos: si un nombre mapea a VARIAS claves distintas del catalogo, NO se
    adjunta nada; solo se reporta para revision manual.

  Seguridad: @Aplicar = 0 (PREVIEW) por defecto -> solo muestra que haria.
             Pon @Aplicar = 1 para escribir (en transaccion). Idempotente.

  Ejecutar conectado a la BD CALANDRIA (igual que los scripts de Centralizacion,
  p.ej. sqlcmd -d CALANDRIA -f 65001).
  Archivo UTF-8 con BOM (contiene la columna acentuada [Descripcion]).
==============================================================================
*/
SET NOCOUNT ON;

-- >>> Cambia a 1 para ESCRIBIR las claves. 0 = solo vista previa. <<<
DECLARE @Aplicar BIT = 0;

-- Colacion de cotejo: case-insensitive + accent-insensitive.
-- (No hace falta UPPER ni quitar acentos a mano; lo resuelve la colacion.)

IF OBJECT_ID(N'dbo.InsumosTuneraEXP')  IS NULL
   AND OBJECT_ID(N'dbo.InsumosCalandraEXP') IS NULL
BEGIN
    PRINT 'No existe InsumosTuneraEXP ni InsumosCalandraEXP. Nada que comparar.';
    RETURN;
END

-- ----------------------------------------------------------------------------
-- 1) Catalogo normalizado (ambos EXP). NK = nombre normalizado (colacion CI_AI).
--    El colapso de espacios usa NCHAR(1)/NCHAR(2) como centinelas (no aparecen
--    en datos): ' ' -> <1><2>, luego <2><1> -> '', luego <1><2> -> ' '.
-- ----------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#Cat') IS NOT NULL DROP TABLE #Cat;
CREATE TABLE #Cat (
    Cat   NVARCHAR(10)   NOT NULL,
    NK    NVARCHAR(1000) COLLATE Latin1_General_CI_AI NOT NULL,
    Clave NVARCHAR(50)   NOT NULL
);

IF OBJECT_ID(N'dbo.InsumosTuneraEXP') IS NOT NULL
    INSERT INTO #Cat (Cat, NK, Clave)
    SELECT 'TUNERA',
           LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(CAST([Descripción] AS NVARCHAR(1000)),
                 N' ', NCHAR(1)+NCHAR(2)), NCHAR(2)+NCHAR(1), N''), NCHAR(1)+NCHAR(2), N' '))),
           LTRIM(RTRIM(Clave))
    FROM dbo.InsumosTuneraEXP
    WHERE Clave IS NOT NULL AND LTRIM(RTRIM(Clave)) <> ''
      AND [Descripción] IS NOT NULL AND LTRIM(RTRIM([Descripción])) <> '';

IF OBJECT_ID(N'dbo.InsumosCalandraEXP') IS NOT NULL
    INSERT INTO #Cat (Cat, NK, Clave)
    SELECT 'CALANDRA',
           LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(CAST([Descripción] AS NVARCHAR(1000)),
                 N' ', NCHAR(1)+NCHAR(2)), NCHAR(2)+NCHAR(1), N''), NCHAR(1)+NCHAR(2), N' '))),
           LTRIM(RTRIM(Clave))
    FROM dbo.InsumosCalandraEXP
    WHERE Clave IS NOT NULL AND LTRIM(RTRIM(Clave)) <> ''
      AND [Descripción] IS NOT NULL AND LTRIM(RTRIM([Descripción])) <> '';

-- Mapa: NK -> #claves distintas y la clave (si es unica).
IF OBJECT_ID('tempdb..#CatMap') IS NOT NULL DROP TABLE #CatMap;
SELECT Cat, NK,
       Distintas = COUNT(DISTINCT Clave),
       Clave     = MIN(Clave)
INTO #CatMap
FROM #Cat
GROUP BY Cat, NK;

-- ----------------------------------------------------------------------------
-- 2) Nodos de destajo Material (TipoTarea=1) SIN clave, de ambas rutas.
-- ----------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#Nodos') IS NOT NULL DROP TABLE #Nodos;
CREATE TABLE #Nodos (
    Ruta   NVARCHAR(40)   NOT NULL,
    Cat    NVARCHAR(10)   NOT NULL,
    NodoID INT            NOT NULL,
    Nombre NVARCHAR(255)  NOT NULL,
    NK     NVARCHAR(1000) COLLATE Latin1_General_CI_AI NOT NULL
);

IF OBJECT_ID(N'dbo.RutaTuneraDestajo') IS NOT NULL
   AND OBJECT_ID(N'dbo.RutaTuneraDestajo_Columnas') IS NOT NULL
    INSERT INTO #Nodos (Ruta, Cat, NodoID, Nombre, NK)
    SELECT 'RutaTuneraDestajo', 'TUNERA', r.ID, r.Nombre,
           LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(CAST(r.Nombre AS NVARCHAR(1000)),
                 N' ', NCHAR(1)+NCHAR(2)), NCHAR(2)+NCHAR(1), N''), NCHAR(1)+NCHAR(2), N' ')))
    FROM dbo.RutaTuneraDestajo r
    WHERE r.TipoTarea = 1
      AND LTRIM(RTRIM(ISNULL(r.Nombre,''))) <> ''
      AND NOT EXISTS (
            SELECT 1 FROM dbo.RutaTuneraDestajo_Columnas c
            WHERE c.NodoID = r.ID AND c.NombreColumna = 'Clave'
              AND c.Valor IS NOT NULL AND LTRIM(RTRIM(c.Valor)) <> '');

IF OBJECT_ID(N'dbo.RutaCalandraDestajo') IS NOT NULL
   AND OBJECT_ID(N'dbo.RutaCalandraDestajo_Columnas') IS NOT NULL
    INSERT INTO #Nodos (Ruta, Cat, NodoID, Nombre, NK)
    SELECT 'RutaCalandraDestajo', 'CALANDRA', r.ID, r.Nombre,
           LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(CAST(r.Nombre AS NVARCHAR(1000)),
                 N' ', NCHAR(1)+NCHAR(2)), NCHAR(2)+NCHAR(1), N''), NCHAR(1)+NCHAR(2), N' ')))
    FROM dbo.RutaCalandraDestajo r
    WHERE r.TipoTarea = 1
      AND LTRIM(RTRIM(ISNULL(r.Nombre,''))) <> ''
      AND NOT EXISTS (
            SELECT 1 FROM dbo.RutaCalandraDestajo_Columnas c
            WHERE c.NodoID = r.ID AND c.NombreColumna = 'Clave'
              AND c.Valor IS NOT NULL AND LTRIM(RTRIM(c.Valor)) <> '');

-- ----------------------------------------------------------------------------
-- 3) Plan: clasifica cada nodo en MATCH / AMBIGUO / SIN MATCH.
-- ----------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#Plan') IS NOT NULL DROP TABLE #Plan;
SELECT n.Ruta, n.Cat, n.NodoID, n.Nombre, n.NK,
       ClaveNueva = CASE WHEN m.Distintas = 1 THEN m.Clave END,
       Estado     = CASE WHEN m.NK IS NULL    THEN 'SIN MATCH'
                         WHEN m.Distintas = 1 THEN 'MATCH'
                         ELSE 'AMBIGUO' END
INTO #Plan
FROM #Nodos n
LEFT JOIN #CatMap m ON m.Cat = n.Cat AND m.NK = n.NK;

-- ----------------------------------------------------------------------------
-- 4) Reportes (siempre).
-- ----------------------------------------------------------------------------
PRINT '=== Resumen por ruta y estado ===';
SELECT Ruta, Estado, Nodos = COUNT(*)
FROM #Plan GROUP BY Ruta, Estado ORDER BY Ruta, Estado;

PRINT '=== MATCH: claves que se adjuntarian (o se adjuntaron si @Aplicar=1) ===';
SELECT Ruta, NodoID, Nombre, ClaveNueva
FROM #Plan WHERE Estado = 'MATCH' ORDER BY Ruta, Nombre;

PRINT '=== AMBIGUO: nombre con varias claves en el catalogo (NO se tocan) ===';
SELECT p.Ruta, p.NodoID, p.Nombre, ClaveCandidata = c.Clave
FROM #Plan p
JOIN #Cat c ON c.Cat = p.Cat AND c.NK = p.NK
WHERE p.Estado = 'AMBIGUO'
ORDER BY p.Ruta, p.Nombre, c.Clave;

PRINT '=== SIN MATCH: sin coincidencia de nombre en el catalogo ===';
SELECT Ruta, NodoID, Nombre
FROM #Plan WHERE Estado = 'SIN MATCH' ORDER BY Ruta, Nombre;

-- ----------------------------------------------------------------------------
-- 5) Aplicar (solo si @Aplicar = 1): adjunta la clave a los MATCH.
--    UPDATE de filas 'Clave' vacias + INSERT para nodos sin fila 'Clave'.
-- ----------------------------------------------------------------------------
IF @Aplicar = 1
BEGIN
    SET XACT_ABORT ON;
    BEGIN TRAN;

    DECLARE @uT INT = 0, @iT INT = 0, @uC INT = 0, @iC INT = 0;

    IF OBJECT_ID(N'dbo.RutaTuneraDestajo_Columnas') IS NOT NULL
    BEGIN
        UPDATE c SET c.Valor = p.ClaveNueva
        FROM dbo.RutaTuneraDestajo_Columnas c
        JOIN #Plan p ON p.Ruta = 'RutaTuneraDestajo' AND p.Estado = 'MATCH'
                    AND c.NodoID = p.NodoID AND c.NombreColumna = 'Clave';
        SET @uT = @@ROWCOUNT;

        INSERT INTO dbo.RutaTuneraDestajo_Columnas (NodoID, NombreColumna, Valor)
        SELECT p.NodoID, 'Clave', p.ClaveNueva
        FROM #Plan p
        WHERE p.Ruta = 'RutaTuneraDestajo' AND p.Estado = 'MATCH'
          AND NOT EXISTS (SELECT 1 FROM dbo.RutaTuneraDestajo_Columnas c
                          WHERE c.NodoID = p.NodoID AND c.NombreColumna = 'Clave');
        SET @iT = @@ROWCOUNT;
    END

    IF OBJECT_ID(N'dbo.RutaCalandraDestajo_Columnas') IS NOT NULL
    BEGIN
        UPDATE c SET c.Valor = p.ClaveNueva
        FROM dbo.RutaCalandraDestajo_Columnas c
        JOIN #Plan p ON p.Ruta = 'RutaCalandraDestajo' AND p.Estado = 'MATCH'
                    AND c.NodoID = p.NodoID AND c.NombreColumna = 'Clave';
        SET @uC = @@ROWCOUNT;

        INSERT INTO dbo.RutaCalandraDestajo_Columnas (NodoID, NombreColumna, Valor)
        SELECT p.NodoID, 'Clave', p.ClaveNueva
        FROM #Plan p
        WHERE p.Ruta = 'RutaCalandraDestajo' AND p.Estado = 'MATCH'
          AND NOT EXISTS (SELECT 1 FROM dbo.RutaCalandraDestajo_Columnas c
                          WHERE c.NodoID = p.NodoID AND c.NombreColumna = 'Clave');
        SET @iC = @@ROWCOUNT;
    END

    COMMIT;
    PRINT 'APLICADO. Tunera: ' + CAST(@uT AS NVARCHAR(10)) + ' actualizadas + '
        + CAST(@iT AS NVARCHAR(10)) + ' nuevas. Calandra: '
        + CAST(@uC AS NVARCHAR(10)) + ' actualizadas + '
        + CAST(@iC AS NVARCHAR(10)) + ' nuevas.';
END
ELSE
    PRINT 'PREVIEW: no se escribio nada. Revisa los MATCH y pon @Aplicar = 1 para adjuntar las claves.';

-- Limpieza.
IF OBJECT_ID('tempdb..#Cat')    IS NOT NULL DROP TABLE #Cat;
IF OBJECT_ID('tempdb..#CatMap') IS NOT NULL DROP TABLE #CatMap;
IF OBJECT_ID('tempdb..#Nodos')  IS NOT NULL DROP TABLE #Nodos;
IF OBJECT_ID('tempdb..#Plan')   IS NOT NULL DROP TABLE #Plan;
GO
