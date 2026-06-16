-- ============================================================================
-- TABLA: ActivacionTareasRuta
-- DESCRIPCIÓN: Almacena el estado de activación/desactivación de tareas
--              para cada combinación de Manzana/Lote
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ActivacionTareasRuta')
BEGIN
    CREATE TABLE ActivacionTareasRuta (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        
        -- Identificación de la casa
        Manzana NVARCHAR(10) NOT NULL,
        Lote NVARCHAR(10) NOT NULL,
        
        -- Información del prototipo
        Prototipo NVARCHAR(50),
        
        -- Ruta seleccionada (RutaTuneraDestajo o RutaCalandraDestajo)
        Ruta NVARCHAR(50) NOT NULL,
        
        -- Nodo de la ruta
        NodoID INT NOT NULL,
        NombreTarea NVARCHAR(200),
        
        -- Estado de activación
        Activa BIT NOT NULL DEFAULT 1,
        
        -- Auditoría
        FechaActualizacion DATETIME DEFAULT GETDATE(),
        UsuarioModificacion NVARCHAR(100),
        
        -- Índices
        INDEX IX_ActivacionTareasRuta_ManzanaLote (Manzana, Lote),
        INDEX IX_ActivacionTareasRuta_Ruta (Ruta),
        CONSTRAINT FK_ActivacionTareasRuta_NodoID FOREIGN KEY (NodoID) REFERENCES RutaTuneraDestajo(ID) ON DELETE CASCADE
    );
    
    PRINT 'Tabla ActivacionTareasRuta creada correctamente.'
END
ELSE
BEGIN
    PRINT 'La tabla ActivacionTareasRuta ya existe.'
END

-- ============================================================================
-- VISTA: vw_TareasActivasPorCasa
-- DESCRIPCIÓN: Retorna las tareas activas para una casa específica
-- ============================================================================

IF OBJECT_ID('dbo.vw_TareasActivasPorCasa', 'V') IS NOT NULL
    DROP VIEW dbo.vw_TareasActivasPorCasa
GO

CREATE VIEW dbo.vw_TareasActivasPorCasa
AS
SELECT 
    atr.Manzana,
    atr.Lote,
    atr.Prototipo,
    atr.Ruta,
    atr.NodoID,
    atr.NombreTarea,
    atr.Activa,
    atr.FechaActualizacion,
    atr.UsuarioModificacion,
    CASE 
        WHEN atr.Ruta LIKE '%Calandria%' THEN 'Calandria'
        WHEN atr.Ruta LIKE '%Tunera%' THEN 'Tunera'
        ELSE 'Desconocido'
    END AS TipoRuta
FROM ActivacionTareasRuta atr
WHERE atr.Activa = 1
GO

-- ============================================================================
-- STORED PROCEDURES
-- ============================================================================

-- ============================================================================
-- SP: sp_ObtenerTareasActivasPorCasa
-- DESCRIPCIÓN: Obtiene las tareas activas para una casa
-- ============================================================================

IF OBJECT_ID('dbo.sp_ObtenerTareasActivasPorCasa', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ObtenerTareasActivasPorCasa
GO

CREATE PROCEDURE dbo.sp_ObtenerTareasActivasPorCasa
    @Manzana NVARCHAR(10),
    @Lote NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        NodoID,
        NombreTarea,
        Ruta,
        Activa,
        FechaActualizacion
    FROM ActivacionTareasRuta
    WHERE Manzana = @Manzana 
        AND Lote = @Lote
        AND Activa = 1
    ORDER BY Ruta, NodoID;
END
GO

-- ============================================================================
-- SP: sp_GuardarActivacionTarea
-- DESCRIPCIÓN: Guarda o actualiza la activación de una tarea
-- ============================================================================

IF OBJECT_ID('dbo.sp_GuardarActivacionTarea', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GuardarActivacionTarea
GO

CREATE PROCEDURE dbo.sp_GuardarActivacionTarea
    @Manzana NVARCHAR(10),
    @Lote NVARCHAR(10),
    @Prototipo NVARCHAR(50),
    @Ruta NVARCHAR(50),
    @NodoID INT,
    @NombreTarea NVARCHAR(200),
    @Activa BIT,
    @UsuarioModificacion NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM ActivacionTareasRuta 
               WHERE Manzana = @Manzana 
               AND Lote = @Lote 
               AND Ruta = @Ruta
               AND NodoID = @NodoID)
    BEGIN
        UPDATE ActivacionTareasRuta
        SET Activa = @Activa,
            FechaActualizacion = GETDATE(),
            UsuarioModificacion = @UsuarioModificacion
        WHERE Manzana = @Manzana 
            AND Lote = @Lote 
            AND Ruta = @Ruta
            AND NodoID = @NodoID;
    END
    ELSE
    BEGIN
        INSERT INTO ActivacionTareasRuta 
        (Manzana, Lote, Prototipo, Ruta, NodoID, NombreTarea, Activa, UsuarioModificacion)
        VALUES (@Manzana, @Lote, @Prototipo, @Ruta, @NodoID, @NombreTarea, @Activa, @UsuarioModificacion);
    END
END
GO

-- ============================================================================
-- SP: sp_LimpiarActivacionesCasa
-- DESCRIPCIÓN: Limpia todas las activaciones de una casa
-- ============================================================================

IF OBJECT_ID('dbo.sp_LimpiarActivacionesCasa', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_LimpiarActivacionesCasa
GO

CREATE PROCEDURE dbo.sp_LimpiarActivacionesCasa
    @Manzana NVARCHAR(10),
    @Lote NVARCHAR(10),
    @Ruta NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @Ruta IS NULL
    BEGIN
        DELETE FROM ActivacionTareasRuta
        WHERE Manzana = @Manzana AND Lote = @Lote;
    END
    ELSE
    BEGIN
        DELETE FROM ActivacionTareasRuta
        WHERE Manzana = @Manzana AND Lote = @Lote AND Ruta = @Ruta;
    END
END
GO

-- ============================================================================
-- ÍNDICES ADICIONALES
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivacionTareasRuta_Activa')
BEGIN
    CREATE INDEX IX_ActivacionTareasRuta_Activa 
    ON ActivacionTareasRuta (Activa) 
    INCLUDE (Manzana, Lote, NodoID);
    
    PRINT 'Índice IX_ActivacionTareasRuta_Activa creado.'
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivacionTareasRuta_FechaActualizacion')
BEGIN
    CREATE INDEX IX_ActivacionTareasRuta_FechaActualizacion 
    ON ActivacionTareasRuta (FechaActualizacion DESC);
    
    PRINT 'Índice IX_ActivacionTareasRuta_FechaActualizacion creado.'
END

PRINT 'Script de ActivacionTareasRuta completado exitosamente.'
