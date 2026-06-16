/*
==============================================================================
SCRIPT: Crear Tablas para Editor de TreeList
==============================================================================
Descripción: Crea las tablas necesarias para almacenar estructuras jerárquicas
            de TreeList con columnas personalizables
Autor: Sistema Calandria Residencial
Fecha: Enero 2025
Versión: 1.0
==============================================================================
*/

USE [BaseDatosCalandria]
GO

-- ============================================================================
-- 1. TABLA PRINCIPAL: TreeListData
-- ============================================================================
-- Almacena los nodos del árbol jerárquico

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TreeListData]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TreeListData](
        [ID] [int] NOT NULL,
        [ParentID] [int] NULL,
        [Nombre] [nvarchar](255) NOT NULL,
        [Descripcion] [nvarchar](500) NULL,
        [Orden] [int] NOT NULL DEFAULT 0,
        [Nivel] [int] NOT NULL DEFAULT 0,
        [FechaCreacion] [datetime] NOT NULL DEFAULT GETDATE(),
        [FechaModificacion] [datetime] NULL,
        [UsuarioCreacion] [nvarchar](100) NULL DEFAULT 'Sistema',
        
        CONSTRAINT [PK_TreeListData] PRIMARY KEY CLUSTERED ([ID] ASC)
    )
    
    PRINT '? Tabla TreeListData creada correctamente'
END
ELSE
BEGIN
    PRINT '? Tabla TreeListData ya existe'
END
GO

-- Crear índices para mejorar rendimiento
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TreeListData_ParentID' AND object_id = OBJECT_ID('TreeListData'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_TreeListData_ParentID] ON [dbo].[TreeListData]
    (
        [ParentID] ASC
    )
    PRINT '? Índice IX_TreeListData_ParentID creado'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TreeListData_Nivel_Orden' AND object_id = OBJECT_ID('TreeListData'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_TreeListData_Nivel_Orden] ON [dbo].[TreeListData]
    (
        [Nivel] ASC,
        [Orden] ASC
    )
    PRINT '? Índice IX_TreeListData_Nivel_Orden creado'
END
GO

-- ============================================================================
-- 2. TABLA DE VALORES: TreeListData_Columnas
-- ============================================================================
-- Almacena los valores de las columnas personalizadas para cada nodo

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TreeListData_Columnas]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TreeListData_Columnas](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [NodoID] [int] NOT NULL,
        [NombreColumna] [nvarchar](100) NOT NULL,
        [Valor] [nvarchar](max) NULL,
        
        CONSTRAINT [PK_TreeListData_Columnas] PRIMARY KEY CLUSTERED ([ID] ASC),
        CONSTRAINT [FK_TreeListData_Columnas_Nodo] FOREIGN KEY ([NodoID])
            REFERENCES [dbo].[TreeListData] ([ID])
            ON DELETE CASCADE
    )
    
    PRINT '? Tabla TreeListData_Columnas creada correctamente'
END
ELSE
BEGIN
    PRINT '? Tabla TreeListData_Columnas ya existe'
END
GO

-- Índice para búsqueda rápida por NodoID
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TreeListData_Columnas_NodoID' AND object_id = OBJECT_ID('TreeListData_Columnas'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_TreeListData_Columnas_NodoID] ON [dbo].[TreeListData_Columnas]
    (
        [NodoID] ASC,
        [NombreColumna] ASC
    )
    PRINT '? Índice IX_TreeListData_Columnas_NodoID creado'
END
GO

-- ============================================================================
-- 3. TABLA DE DEFINICIÓN: TreeListData_ColumnasDefinicion
-- ============================================================================
-- Almacena la definición de las columnas personalizadas

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TreeListData_ColumnasDefinicion]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TreeListData_ColumnasDefinicion](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [Nombre] [nvarchar](100) NOT NULL,
        [Titulo] [nvarchar](100) NOT NULL,
        [Ancho] [int] NOT NULL DEFAULT 100,
        [TipoDato] [nvarchar](200) NOT NULL DEFAULT 'System.String',
        [EsEditable] [bit] NOT NULL DEFAULT 1,
        [Formato] [nvarchar](50) NULL,
        
        CONSTRAINT [PK_TreeListData_ColumnasDefinicion] PRIMARY KEY CLUSTERED ([ID] ASC),
        CONSTRAINT [UQ_TreeListData_ColumnasDefinicion_Nombre] UNIQUE ([Nombre])
    )
    
    PRINT '? Tabla TreeListData_ColumnasDefinicion creada correctamente'
END
ELSE
BEGIN
    PRINT '? Tabla TreeListData_ColumnasDefinicion ya existe'
END
GO

-- ============================================================================
-- 4. DATOS DE EJEMPLO (OPCIONAL)
-- ============================================================================
-- Insertar estructura de ejemplo para pruebas

IF NOT EXISTS (SELECT * FROM TreeListData)
BEGIN
    PRINT '?? Insertando datos de ejemplo...'
    
    -- Nodos padre (nivel 0)
    INSERT INTO TreeListData (ID, ParentID, Nombre, Descripcion, Orden, Nivel, UsuarioCreacion)
    VALUES 
        (1, NULL, 'Proyecto A', 'Proyecto de construcción principal', 0, 0, 'admin'),
        (2, NULL, 'Proyecto B', 'Proyecto de urbanización', 1, 0, 'admin'),
        (3, NULL, 'Proyecto C', 'Proyecto de acabados', 2, 0, 'admin')
    
    -- Sub-padres (nivel 1)
    INSERT INTO TreeListData (ID, ParentID, Nombre, Descripcion, Orden, Nivel, UsuarioCreacion)
    VALUES 
        (10, 1, 'Fase 1', 'Cimentación y estructura', 0, 1, 'admin'),
        (11, 1, 'Fase 2', 'Instalaciones', 1, 1, 'admin'),
        (12, 1, 'Fase 3', 'Acabados finales', 2, 1, 'admin'),
        (20, 2, 'Etapa 1', 'Terracerías', 0, 1, 'admin'),
        (21, 2, 'Etapa 2', 'Pavimentos', 1, 1, 'admin')
    
    -- Hijos (nivel 2)
    INSERT INTO TreeListData (ID, ParentID, Nombre, Descripcion, Orden, Nivel, UsuarioCreacion)
    VALUES 
        (100, 10, 'Excavación', 'Excavación de cepas', 0, 2, 'admin'),
        (101, 10, 'Cimbra', 'Cimbra de cimentación', 1, 2, 'admin'),
        (102, 10, 'Armado', 'Armado de acero', 2, 2, 'admin'),
        (103, 10, 'Colado', 'Colado de concreto', 3, 2, 'admin'),
        (110, 11, 'Hidráulica', 'Instalación hidráulica', 0, 2, 'admin'),
        (111, 11, 'Sanitaria', 'Instalación sanitaria', 1, 2, 'admin'),
        (112, 11, 'Eléctrica', 'Instalación eléctrica', 2, 2, 'admin'),
        (200, 20, 'Cortes', 'Cortes de terreno', 0, 2, 'admin'),
        (201, 20, 'Rellenos', 'Rellenos y compactación', 1, 2, 'admin')
    
    -- Columnas personalizadas de ejemplo
    INSERT INTO TreeListData_ColumnasDefinicion (Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato)
    VALUES 
        ('Presupuesto', 'Presupuesto', 120, 'System.Decimal', 1, '{0:C2}'),
        ('Avance', 'Avance %', 100, 'System.Decimal', 1, '{0:F1}%'),
        ('Responsable', 'Responsable', 150, 'System.String', 1, NULL),
        ('FechaInicio', 'Inicio', 100, 'System.DateTime', 1, '{0:dd/MM/yyyy}'),
        ('FechaFin', 'Fin', 100, 'System.DateTime', 1, '{0:dd/MM/yyyy}')
    
    -- Valores de ejemplo para algunas columnas
    INSERT INTO TreeListData_Columnas (NodoID, NombreColumna, Valor)
    VALUES 
        (1, 'Presupuesto', '1500000.00'),
        (1, 'Responsable', 'Ing. García'),
        (1, 'FechaInicio', '2025-01-01'),
        (10, 'Presupuesto', '500000.00'),
        (10, 'Avance', '45.5'),
        (10, 'Responsable', 'Arq. Martínez'),
        (100, 'Presupuesto', '50000.00'),
        (100, 'Avance', '100.0'),
        (101, 'Presupuesto', '80000.00'),
        (101, 'Avance', '75.0')
    
    PRINT '? Datos de ejemplo insertados correctamente'
END
ELSE
BEGIN
    PRINT '? La tabla ya contiene datos, no se insertarán ejemplos'
END
GO

-- ============================================================================
-- 5. PROCEDIMIENTOS ALMACENADOS ÚTILES
-- ============================================================================

-- Procedimiento para obtener todos los nodos con sus hijos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_TreeList_ObtenerJerarquia]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_TreeList_ObtenerJerarquia]
GO

CREATE PROCEDURE [dbo].[sp_TreeList_ObtenerJerarquia]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ID,
        ParentID,
        Nombre,
        Descripcion,
        Orden,
        Nivel,
        FechaCreacion,
        FechaModificacion,
        UsuarioCreacion
    FROM TreeListData
    ORDER BY 
        CASE WHEN ParentID IS NULL THEN ID ELSE ParentID END,
        Orden
END
GO

PRINT '? Procedimiento sp_TreeList_ObtenerJerarquia creado'
GO

-- Procedimiento para eliminar un nodo y todos sus descendientes
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_TreeList_EliminarNodo]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_TreeList_EliminarNodo]
GO

CREATE PROCEDURE [dbo].[sp_TreeList_EliminarNodo]
    @NodoID int
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Eliminar descendientes recursivamente
    ;WITH CTE_Descendientes AS (
        SELECT ID FROM TreeListData WHERE ID = @NodoID
        UNION ALL
        SELECT t.ID 
        FROM TreeListData t
        INNER JOIN CTE_Descendientes c ON t.ParentID = c.ID
    )
    DELETE FROM TreeListData
    WHERE ID IN (SELECT ID FROM CTE_Descendientes)
    
    SELECT @@ROWCOUNT as FilasEliminadas
END
GO

PRINT '? Procedimiento sp_TreeList_EliminarNodo creado'
GO

-- ============================================================================
-- 6. VISTAS ÚTILES
-- ============================================================================

-- Vista con la jerarquía completa y valores de columnas
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_TreeList_Completa]'))
    DROP VIEW [dbo].[vw_TreeList_Completa]
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
    t.FechaCreacion,
    t.FechaModificacion,
    t.UsuarioCreacion,
    (SELECT COUNT(*) FROM TreeListData WHERE ParentID = t.ID) as NumeroHijos,
    -- Columnas personalizadas (ajustar según necesidad)
    (SELECT Valor FROM TreeListData_Columnas WHERE NodoID = t.ID AND NombreColumna = 'Presupuesto') as Presupuesto,
    (SELECT Valor FROM TreeListData_Columnas WHERE NodoID = t.ID AND NombreColumna = 'Avance') as Avance,
    (SELECT Valor FROM TreeListData_Columnas WHERE NodoID = t.ID AND NombreColumna = 'Responsable') as Responsable
FROM TreeListData t
GO

PRINT '? Vista vw_TreeList_Completa creada'
GO

-- ============================================================================
-- 7. VERIFICACIÓN FINAL
-- ============================================================================

PRINT ''
PRINT '============================================================================'
PRINT '                    VERIFICACIÓN DE INSTALACIÓN'
PRINT '============================================================================'

-- Contar registros en cada tabla
DECLARE @CountTreeListData int
DECLARE @CountColumnas int
DECLARE @CountDefinicion int

SELECT @CountTreeListData = COUNT(*) FROM TreeListData
SELECT @CountColumnas = COUNT(*) FROM TreeListData_Columnas
SELECT @CountDefinicion = COUNT(*) FROM TreeListData_ColumnasDefinicion

PRINT '?? Registros actuales:'
PRINT '   - TreeListData: ' + CAST(@CountTreeListData AS nvarchar(10))
PRINT '   - TreeListData_Columnas: ' + CAST(@CountColumnas AS nvarchar(10))
PRINT '   - TreeListData_ColumnasDefinicion: ' + CAST(@CountDefinicion AS nvarchar(10))
PRINT ''

-- Verificar tablas creadas
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'TreeListData'))
   AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'TreeListData_Columnas'))
   AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'TreeListData_ColumnasDefinicion'))
BEGIN
    PRINT '? INSTALACIÓN COMPLETADA EXITOSAMENTE'
    PRINT ''
    PRINT '?? Siguiente paso: Ejecutar FormEditorTreeList desde la aplicación'
END
ELSE
BEGIN
    PRINT '? ERROR: No todas las tablas fueron creadas correctamente'
END

PRINT '============================================================================'
GO

/*
==============================================================================
NOTAS DE USO:
==============================================================================

1. Para abrir el editor desde código C#:
   
   using (var form = new FormEditorTreeList(connectionString, "TreeListData"))
   {
       form.ShowDialog();
   }

2. Para consultar toda la jerarquía:
   
   SELECT * FROM vw_TreeList_Completa ORDER BY Nivel, Orden

3. Para eliminar un nodo y sus descendientes:
   
   EXEC sp_TreeList_EliminarNodo @NodoID = 1

4. Las columnas personalizadas se definen en:
   - TreeListData_ColumnasDefinicion (estructura)
   - TreeListData_Columnas (valores)

5. Estructura de niveles recomendada:
   - Nivel 0: Padres (proyectos, categorías principales)
   - Nivel 1: Sub-Padres (fases, etapas, subcategorías)
   - Nivel 2: Hijos (tareas, partidas, items específicos)

==============================================================================
*/
