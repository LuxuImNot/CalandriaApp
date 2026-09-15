/*
==============================================================================
SCRIPT: Duplicar RutaTuneraDestajo -> RutaCalandraDestajo
==============================================================================
  Copia TODO el contenido de la ruta Tunera a la ruta Calandra:
    - RutaTuneraDestajo               -> RutaCalandraDestajo
    - RutaTuneraDestajo_Columnas      -> RutaCalandraDestajo_Columnas
    - RutaTuneraDestajo_ColumnasDefinicion -> RutaCalandraDestajo_ColumnasDefinicion

  Son los mismos insumos/estructura; despues el usuario ajusta las CANTIDADES
  en la ruta Calandra.

  Detalles:
    - Conserva los mismos IDs (PK manual), por lo que ParentID y NodoID siguen
      siendo validos sin necesidad de remapear.
    - LIMPIA primero el destino (incluidos los datos de ejemplo). La FK con
      ON DELETE CASCADE arrastra RutaCalandraDestajo_Columnas.
    - Todo en una transaccion: si algo falla, no deja a medias (ROLLBACK).
    - Requiere que las tablas destino existan (ejecuta antes
      CrearTablasRutaCalandraDestajo.sql).
==============================================================================
*/
USE [BaseDatosCalandria];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

-- ---------------------------------------------------------------------------
-- Validaciones previas
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.RutaTuneraDestajo', N'U') IS NULL
   OR OBJECT_ID(N'dbo.RutaTuneraDestajo_Columnas', N'U') IS NULL
   OR OBJECT_ID(N'dbo.RutaTuneraDestajo_ColumnasDefinicion', N'U') IS NULL
BEGIN
    RAISERROR('No existen las tablas de origen (RutaTuneraDestajo*). Aborta.', 16, 1);
    SET NOEXEC ON;
END
GO

IF OBJECT_ID(N'dbo.RutaCalandraDestajo', N'U') IS NULL
   OR OBJECT_ID(N'dbo.RutaCalandraDestajo_Columnas', N'U') IS NULL
   OR OBJECT_ID(N'dbo.RutaCalandraDestajo_ColumnasDefinicion', N'U') IS NULL
BEGIN
    RAISERROR('No existen las tablas destino (RutaCalandraDestajo*). Ejecuta primero CrearTablasRutaCalandraDestajo.sql.', 16, 1);
    SET NOEXEC ON;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.RutaTuneraDestajo)
BEGIN
    RAISERROR('RutaTuneraDestajo esta vacia: no hay nada que duplicar. Aborta.', 16, 1);
    SET NOEXEC ON;
END
GO

-- ---------------------------------------------------------------------------
-- Copia transaccional
-- ---------------------------------------------------------------------------
BEGIN TRY
    BEGIN TRAN;

    -- 1) Limpiar el destino (hijos antes que padres por la FK).
    DELETE FROM dbo.RutaCalandraDestajo_Columnas;
    DELETE FROM dbo.RutaCalandraDestajo;
    DELETE FROM dbo.RutaCalandraDestajo_ColumnasDefinicion;

    -- 2) Copiar los nodos conservando los IDs. TipoTarea solo si existe en origen
    --    (si no, el destino usa su DEFAULT 0).
    DECLARE @colsNodo NVARCHAR(MAX) =
        N'ID, ParentID, Nombre, Descripcion, Orden, Nivel, FechaCreacion, FechaModificacion, UsuarioCreacion';
    IF COL_LENGTH('dbo.RutaTuneraDestajo', 'TipoTarea') IS NOT NULL
        SET @colsNodo =
        N'ID, ParentID, Nombre, Descripcion, Orden, Nivel, TipoTarea, FechaCreacion, FechaModificacion, UsuarioCreacion';

    DECLARE @sqlNodo NVARCHAR(MAX) =
        N'INSERT INTO dbo.RutaCalandraDestajo (' + @colsNodo + N')
          SELECT ' + @colsNodo + N' FROM dbo.RutaTuneraDestajo;';
    EXEC sp_executesql @sqlNodo;

    -- 3) Copiar los valores de columnas (ID destino es IDENTITY, no se copia).
    INSERT INTO dbo.RutaCalandraDestajo_Columnas (NodoID, NombreColumna, Valor)
    SELECT NodoID, NombreColumna, Valor
    FROM dbo.RutaTuneraDestajo_Columnas;

    -- 4) Copiar las definiciones de columnas (ID destino es IDENTITY, no se copia).
    INSERT INTO dbo.RutaCalandraDestajo_ColumnasDefinicion (Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato)
    SELECT Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato
    FROM dbo.RutaTuneraDestajo_ColumnasDefinicion;

    COMMIT;

    DECLARE @n INT, @c INT, @d INT;
    SELECT @n = COUNT(*) FROM dbo.RutaCalandraDestajo;
    SELECT @c = COUNT(*) FROM dbo.RutaCalandraDestajo_Columnas;
    SELECT @d = COUNT(*) FROM dbo.RutaCalandraDestajo_ColumnasDefinicion;

    PRINT 'Duplicado completado RutaTunera -> RutaCalandra:';
    PRINT '  - Nodos:        ' + CAST(@n AS NVARCHAR(10));
    PRINT '  - Valores:      ' + CAST(@c AS NVARCHAR(10));
    PRINT '  - Definiciones: ' + CAST(@d AS NVARCHAR(10));
    PRINT 'Ahora puedes ajustar las CANTIDADES en la ruta Calandra.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    DECLARE @msg NVARCHAR(2048) = ERROR_MESSAGE();
    RAISERROR('Error al duplicar: %s', 16, 1, @msg);
END CATCH
GO

SET NOEXEC OFF;
GO
