/*
==============================================================================
SCRIPT: Almacen <-> Destajos (liberacion de insumos al activar)
==============================================================================
  Soporte para "liberar" los insumos de un destajo desde el almacen hacia la
  Manzana/Lote al activarlo:
    - SalidasAlmacen      : columnas de origen (ruta/nodo/tipo) para vincular la
                            salida con el destajo que la genero e idempotencia.
    - FoliosSalidaAlmacen : columnas de origen para vincular el vale al destajo.
    - ExcepcionesLiberacionAlmacen : bitacora de excepciones (p.ej. desactivar un
                            destajo ya liberado) con justificacion y snapshot.
  Idempotente: se puede correr varias veces. Base: CALANDRIA.
==============================================================================
*/
USE CALANDRIA;
GO
SET NOCOUNT ON;
GO

-- ---------------------------------------------------------------------------
-- 1. SalidasAlmacen: columnas de origen
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.SalidasAlmacen', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.SalidasAlmacen', 'OrigenRuta') IS NULL
        ALTER TABLE dbo.SalidasAlmacen ADD [OrigenRuta] NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.SalidasAlmacen', 'OrigenNodoID') IS NULL
        ALTER TABLE dbo.SalidasAlmacen ADD [OrigenNodoID] INT NULL;
    IF COL_LENGTH('dbo.SalidasAlmacen', 'OrigenTipo') IS NULL
        ALTER TABLE dbo.SalidasAlmacen ADD [OrigenTipo] NVARCHAR(20) NULL;
    PRINT 'SalidasAlmacen: columnas de origen verificadas.';
END
ELSE
    PRINT 'AVISO: no existe dbo.SalidasAlmacen (se crea desde el modulo de almacen).';
GO

-- Indice para la verificacion de idempotencia (OrigenRuta + OrigenNodoID + casa)
IF OBJECT_ID(N'dbo.SalidasAlmacen', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SalidasAlmacen_Origen'
                   AND object_id = OBJECT_ID('dbo.SalidasAlmacen'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SalidasAlmacen_Origen]
        ON dbo.SalidasAlmacen ([OrigenRuta], [OrigenNodoID], [Manzana], [Lote]);
    PRINT 'Indice IX_SalidasAlmacen_Origen creado.';
END
GO

-- ---------------------------------------------------------------------------
-- 2. FoliosSalidaAlmacen: columnas de origen
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.FoliosSalidaAlmacen', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.FoliosSalidaAlmacen', 'OrigenRuta') IS NULL
        ALTER TABLE dbo.FoliosSalidaAlmacen ADD [OrigenRuta] NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.FoliosSalidaAlmacen', 'OrigenNodoID') IS NULL
        ALTER TABLE dbo.FoliosSalidaAlmacen ADD [OrigenNodoID] INT NULL;
    PRINT 'FoliosSalidaAlmacen: columnas de origen verificadas.';
END
ELSE
    PRINT 'AVISO: no existe dbo.FoliosSalidaAlmacen (se crea desde el modulo de almacen).';
GO

-- ---------------------------------------------------------------------------
-- 3. ExcepcionesLiberacionAlmacen: bitacora de excepciones
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.ExcepcionesLiberacionAlmacen', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExcepcionesLiberacionAlmacen(
        [Id]            INT IDENTITY(1,1) NOT NULL,
        [Fecha]         DATETIME      NOT NULL CONSTRAINT DF_ExcLibAlm_Fecha DEFAULT (GETDATE()),
        [Tipo]          NVARCHAR(40)  NOT NULL,   -- p.ej. 'DesactivacionDestajoLiberado'
        [Manzana]       NVARCHAR(10)  NULL,
        [Lote]          NVARCHAR(10)  NULL,
        [Ruta]          NVARCHAR(100) NULL,
        [NodoID]        INT           NULL,
        [NombreDestajo] NVARCHAR(255) NULL,
        [Usuario]       NVARCHAR(100) NULL,
        [Justificacion] NVARCHAR(MAX) NULL,
        [DetalleJson]   NVARCHAR(MAX) NULL,        -- snapshot de insumos liberados
        [TotalImporte]  DECIMAL(18,2) NULL,
        CONSTRAINT PK_ExcepcionesLiberacionAlmacen PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE NONCLUSTERED INDEX IX_ExcLibAlm_Casa
        ON dbo.ExcepcionesLiberacionAlmacen ([Manzana], [Lote], [Ruta], [NodoID]);
    PRINT 'Tabla ExcepcionesLiberacionAlmacen creada.';
END
ELSE
    PRINT 'Tabla ExcepcionesLiberacionAlmacen ya existe.';
GO

PRINT 'Esquema de liberacion Almacen<->Destajos listo.';
GO
