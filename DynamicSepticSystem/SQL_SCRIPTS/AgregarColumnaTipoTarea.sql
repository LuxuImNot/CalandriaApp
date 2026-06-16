/*
==============================================================================
SCRIPT: Agregar Columna TipoTarea a TreeListData
==============================================================================
Descripción: Agrega la columna TipoTarea para etiquetar nodos hijo como
            Material (rojo) o Mano de Obra (verde)
Autor: Sistema Calandria Residencial
Fecha: Enero 2025
Versión: 1.0
==============================================================================
*/

USE [BaseDatosCalandria]
GO

PRINT ''
PRINT '============================================================================'
PRINT '         AGREGANDO COLUMNA TipoTarea A TreeListData'
PRINT '============================================================================'
PRINT ''

-- Verificar si la columna ya existe
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[TreeListData]') 
    AND name = 'TipoTarea'
)
BEGIN
    PRINT '? Agregando columna TipoTarea...'
    
    ALTER TABLE [dbo].[TreeListData]
    ADD [TipoTarea] [int] NOT NULL DEFAULT 0
    
    PRINT '? Columna TipoTarea agregada correctamente'
    PRINT ''
    PRINT '   Valores posibles:'
    PRINT '   - 0 = Ninguno (sin especificar)'
    PRINT '   - 1 = Material (se muestra en rojo)'
    PRINT '   - 2 = Mano de Obra (se muestra en verde)'
    PRINT ''
END
ELSE
BEGIN
    PRINT '? La columna TipoTarea ya existe en la tabla TreeListData'
    PRINT ''
END
GO

-- Crear índice para mejorar búsquedas por tipo de tarea
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_TreeListData_TipoTarea' 
    AND object_id = OBJECT_ID('TreeListData')
)
BEGIN
    PRINT '? Creando índice IX_TreeListData_TipoTarea...'
    
    CREATE NONCLUSTERED INDEX [IX_TreeListData_TipoTarea] 
    ON [dbo].[TreeListData] ([TipoTarea] ASC)
    
    PRINT '? Índice IX_TreeListData_TipoTarea creado'
    PRINT ''
END
ELSE
BEGIN
    PRINT '? El índice IX_TreeListData_TipoTarea ya existe'
    PRINT ''
END
GO

-- Actualizar la vista vw_TreeList_Completa para incluir TipoTarea
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_TreeList_Completa]'))
BEGIN
    PRINT '? Actualizando vista vw_TreeList_Completa...'
    
    DROP VIEW [dbo].[vw_TreeList_Completa]
END
GO

CREATE VIEW [dbo].[vw_TreeList_Completa]
AS
SELECT 
    t.ID,
    t.ParentID,
    t.Nombre,
    t.Descripcion,
    t.Orden,
    t.Nivel,
    CASE t.Nivel
        WHEN 0 THEN 'Padre'
        WHEN 1 THEN 'Sub-Padre'
        WHEN 2 THEN 'Hijo'
        ELSE 'Nivel ' + CAST(t.Nivel AS nvarchar(10))
    END as TipoNodo,
    t.TipoTarea,
    CASE t.TipoTarea
        WHEN 0 THEN '-'
        WHEN 1 THEN 'Material'
        WHEN 2 THEN 'Mano de Obra'
        ELSE 'Desconocido'
    END as TipoTareaTexto,
    t.FechaCreacion,
    t.FechaModificacion,
    t.UsuarioCreacion,
    (SELECT COUNT(*) FROM TreeListData WHERE ParentID = t.ID) as NumeroHijos,
    -- Columnas personalizadas
    (SELECT Valor FROM TreeListData_Columnas WHERE NodoID = t.ID AND NombreColumna = 'Presupuesto') as Presupuesto,
    (SELECT Valor FROM TreeListData_Columnas WHERE NodoID = t.ID AND NombreColumna = 'Avance') as Avance,
    (SELECT Valor FROM TreeListData_Columnas WHERE NodoID = t.ID AND NombreColumna = 'Responsable') as Responsable
FROM TreeListData t
GO

PRINT '? Vista vw_TreeList_Completa actualizada'
PRINT ''
GO

-- Procedimiento para obtener nodos por tipo de tarea
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_TreeList_ObtenerPorTipoTarea]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_TreeList_ObtenerPorTipoTarea]
GO

CREATE PROCEDURE [dbo].[sp_TreeList_ObtenerPorTipoTarea]
    @TipoTarea int = NULL  -- NULL = todos, 1 = Material, 2 = Mano de Obra
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ID,
        ParentID,
        Nombre,
        Descripcion,
        Nivel,
        TipoTarea,
        CASE TipoTarea
            WHEN 1 THEN 'Material'
            WHEN 2 THEN 'Mano de Obra'
            ELSE 'Sin especificar'
        END as TipoTareaTexto
    FROM TreeListData
    WHERE (@TipoTarea IS NULL OR TipoTarea = @TipoTarea)
        AND Nivel = 2  -- Solo nodos hijo
    ORDER BY ParentID, Orden
END
GO

PRINT '? Procedimiento sp_TreeList_ObtenerPorTipoTarea creado'
PRINT ''
GO

-- Procedimiento para cambiar el tipo de tarea de un nodo
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_TreeList_AsignarTipoTarea]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_TreeList_AsignarTipoTarea]
GO

CREATE PROCEDURE [dbo].[sp_TreeList_AsignarTipoTarea]
    @NodoID int,
    @TipoTarea int  -- 0 = Ninguno, 1 = Material, 2 = Mano de Obra
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validar que el nodo existe
    IF NOT EXISTS (SELECT 1 FROM TreeListData WHERE ID = @NodoID)
    BEGIN
        RAISERROR('El nodo especificado no existe', 16, 1)
        RETURN
    END
    
    -- Validar que es un nodo hijo (nivel 2)
    IF NOT EXISTS (SELECT 1 FROM TreeListData WHERE ID = @NodoID AND Nivel = 2)
    BEGIN
        RAISERROR('Solo se puede asignar tipo de tarea a nodos hijo (nivel 2)', 16, 1)
        RETURN
    END
    
    -- Validar tipo de tarea
    IF @TipoTarea NOT IN (0, 1, 2)
    BEGIN
        RAISERROR('Tipo de tarea inválido. Use: 0 = Ninguno, 1 = Material, 2 = Mano de Obra', 16, 1)
        RETURN
    END
    
    -- Actualizar el tipo de tarea
    UPDATE TreeListData
    SET TipoTarea = @TipoTarea,
        FechaModificacion = GETDATE()
    WHERE ID = @NodoID
    
    PRINT '? Tipo de tarea actualizado para nodo ID: ' + CAST(@NodoID AS nvarchar(10))
END
GO

PRINT '? Procedimiento sp_TreeList_AsignarTipoTarea creado'
PRINT ''
GO

-- ============================================================================
-- VERIFICACIÓN FINAL
-- ============================================================================

PRINT '============================================================================'
PRINT '                    VERIFICACIÓN DE ACTUALIZACIÓN'
PRINT '============================================================================'
PRINT ''

-- Verificar que la columna existe
IF EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[TreeListData]') 
    AND name = 'TipoTarea'
)
BEGIN
    PRINT '? Columna TipoTarea: OK'
    
    -- Mostrar estadísticas
    DECLARE @TotalNodosHijo int
    DECLARE @ConMaterial int
    DECLARE @ConManoObra int
    DECLARE @SinAsignar int
    
    SELECT @TotalNodosHijo = COUNT(*) FROM TreeListData WHERE Nivel = 2
    SELECT @ConMaterial = COUNT(*) FROM TreeListData WHERE Nivel = 2 AND TipoTarea = 1
    SELECT @ConManoObra = COUNT(*) FROM TreeListData WHERE Nivel = 2 AND TipoTarea = 2
    SELECT @SinAsignar = COUNT(*) FROM TreeListData WHERE Nivel = 2 AND TipoTarea = 0
    
    PRINT ''
    PRINT '?? Estadísticas de Nodos Hijo:'
    PRINT '   - Total de nodos hijo: ' + CAST(@TotalNodosHijo AS nvarchar(10))
    PRINT '   - ?? Material: ' + CAST(@ConMaterial AS nvarchar(10))
    PRINT '   - ?? Mano de Obra: ' + CAST(@ConManoObra AS nvarchar(10))
    PRINT '   - ? Sin asignar: ' + CAST(@SinAsignar AS nvarchar(10))
END
ELSE
BEGIN
    PRINT '? ERROR: La columna TipoTarea no fue creada'
END
PRINT ''

-- Verificar vista
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_TreeList_Completa]'))
    PRINT '? Vista vw_TreeList_Completa: OK'
ELSE
    PRINT '? ERROR: Vista vw_TreeList_Completa no encontrada'
PRINT ''

-- Verificar procedimientos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_TreeList_ObtenerPorTipoTarea]'))
    PRINT '? Procedimiento sp_TreeList_ObtenerPorTipoTarea: OK'
ELSE
    PRINT '? ERROR: Procedimiento sp_TreeList_ObtenerPorTipoTarea no encontrado'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_TreeList_AsignarTipoTarea]'))
    PRINT '? Procedimiento sp_TreeList_AsignarTipoTarea: OK'
ELSE
    PRINT '? ERROR: Procedimiento sp_TreeList_AsignarTipoTarea no encontrado'

PRINT ''
PRINT '============================================================================'
PRINT '? ACTUALIZACIÓN COMPLETADA EXITOSAMENTE'
PRINT '============================================================================'
PRINT ''
PRINT '?? Ejemplos de uso:'
PRINT ''
PRINT '   -- Asignar tipo Material al nodo 100'
PRINT '   EXEC sp_TreeList_AsignarTipoTarea @NodoID = 100, @TipoTarea = 1'
PRINT ''
PRINT '   -- Asignar tipo Mano de Obra al nodo 101'
PRINT '   EXEC sp_TreeList_AsignarTipoTarea @NodoID = 101, @TipoTarea = 2'
PRINT ''
PRINT '   -- Obtener todos los nodos tipo Material'
PRINT '   EXEC sp_TreeList_ObtenerPorTipoTarea @TipoTarea = 1'
PRINT ''
PRINT '   -- Obtener todos los nodos tipo Mano de Obra'
PRINT '   EXEC sp_TreeList_ObtenerPorTipoTarea @TipoTarea = 2'
PRINT ''
PRINT '   -- Ver todos los nodos con su tipo de tarea'
PRINT '   SELECT * FROM vw_TreeList_Completa ORDER BY Nivel, Orden'
PRINT ''
PRINT '============================================================================'
GO

/*
==============================================================================
NOTAS:
==============================================================================

1. Esta actualización es compatible con versiones anteriores
   - Los nodos existentes tendrán TipoTarea = 0 (Ninguno) por defecto

2. Solo los nodos hijo (Nivel = 2) deben tener tipo de tarea asignado
   - Los nodos padre y sub-padre mantienen TipoTarea = 0

3. Colores en la interfaz:
   - Material (1): Texto rojo (#C0392B)
   - Mano de Obra (2): Texto verde (#27AE60)

4. El cambio de tipo de tarea actualiza automáticamente FechaModificacion

==============================================================================
*/
