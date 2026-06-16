/*
==============================================================================
SCRIPT: Crear Tablas para RutaCalandraDestajo
==============================================================================
Descripción: Crea las tablas necesarias para almacenar la estructura jerárquica
            de Ruta Calandra Destajo (TreeList independiente)
Autor: Sistema Calandria Residencial
Fecha: Enero 2025
Versión: 1.0
==============================================================================
*/

USE [BaseDatosCalandria]
GO

PRINT ''
PRINT '============================================================================'
PRINT '         CREANDO TABLAS PARA RUTA CALANDRA DESTAJO'
PRINT '============================================================================'
PRINT ''

-- ============================================================================
-- 1. TABLA PRINCIPAL: RutaCalandraDestajo
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RutaCalandraDestajo]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RutaCalandraDestajo](
        [ID] [int] NOT NULL,
        [ParentID] [int] NULL,
        [Nombre] [nvarchar](255) NOT NULL,
        [Descripcion] [nvarchar](500) NULL,
        [Orden] [int] NOT NULL DEFAULT 0,
        [Nivel] [int] NOT NULL DEFAULT 0,
        [TipoTarea] [int] NOT NULL DEFAULT 0,
        [FechaCreacion] [datetime] NOT NULL DEFAULT GETDATE(),
        [FechaModificacion] [datetime] NULL,
        [UsuarioCreacion] [nvarchar](100) NULL DEFAULT 'Sistema',
        
        CONSTRAINT [PK_RutaCalandraDestajo] PRIMARY KEY CLUSTERED ([ID] ASC)
    )
    
    PRINT '? Tabla RutaCalandraDestajo creada correctamente'
END
ELSE
BEGIN
    PRINT '? Tabla RutaCalandraDestajo ya existe'
END
GO

-- Crear índices
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RutaCalandraDestajo_ParentID' AND object_id = OBJECT_ID('RutaCalandraDestajo'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RutaCalandraDestajo_ParentID] ON [dbo].[RutaCalandraDestajo]
    (
        [ParentID] ASC
    )
    PRINT '? Índice IX_RutaCalandraDestajo_ParentID creado'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RutaCalandraDestajo_Nivel_Orden' AND object_id = OBJECT_ID('RutaCalandraDestajo'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RutaCalandraDestajo_Nivel_Orden] ON [dbo].[RutaCalandraDestajo]
    (
        [Nivel] ASC,
        [Orden] ASC
    )
    PRINT '? Índice IX_RutaCalandraDestajo_Nivel_Orden creado'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RutaCalandraDestajo_TipoTarea' AND object_id = OBJECT_ID('RutaCalandraDestajo'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RutaCalandraDestajo_TipoTarea] ON [dbo].[RutaCalandraDestajo]
    (
        [TipoTarea] ASC
    )
    PRINT '? Índice IX_RutaCalandraDestajo_TipoTarea creado'
END
GO

-- ============================================================================
-- 2. TABLA DE VALORES: RutaCalandraDestajo_Columnas
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RutaCalandraDestajo_Columnas]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RutaCalandraDestajo_Columnas](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [NodoID] [int] NOT NULL,
        [NombreColumna] [nvarchar](100) NOT NULL,
        [Valor] [nvarchar](max) NULL,
        
        CONSTRAINT [PK_RutaCalandraDestajo_Columnas] PRIMARY KEY CLUSTERED ([ID] ASC),
        CONSTRAINT [FK_RutaCalandraDestajo_Columnas_Nodo] FOREIGN KEY ([NodoID])
            REFERENCES [dbo].[RutaCalandraDestajo] ([ID])
            ON DELETE CASCADE
    )
    
    PRINT '? Tabla RutaCalandraDestajo_Columnas creada correctamente'
END
ELSE
BEGIN
    PRINT '? Tabla RutaCalandraDestajo_Columnas ya existe'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RutaCalandraDestajo_Columnas_NodoID' AND object_id = OBJECT_ID('RutaCalandraDestajo_Columnas'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RutaCalandraDestajo_Columnas_NodoID] ON [dbo].[RutaCalandraDestajo_Columnas]
    (
        [NodoID] ASC,
        [NombreColumna] ASC
    )
    PRINT '? Índice IX_RutaCalandraDestajo_Columnas_NodoID creado'
END
GO

-- ============================================================================
-- 3. TABLA DE DEFINICIÓN: RutaCalandraDestajo_ColumnasDefinicion
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RutaCalandraDestajo_ColumnasDefinicion]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RutaCalandraDestajo_ColumnasDefinicion](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [Nombre] [nvarchar](100) NOT NULL,
        [Titulo] [nvarchar](100) NOT NULL,
        [Ancho] [int] NOT NULL DEFAULT 100,
        [TipoDato] [nvarchar](200) NOT NULL DEFAULT 'System.String',
        [EsEditable] [bit] NOT NULL DEFAULT 1,
        [Formato] [nvarchar](50) NULL,
        
        CONSTRAINT [PK_RutaCalandraDestajo_ColumnasDefinicion] PRIMARY KEY CLUSTERED ([ID] ASC),
        CONSTRAINT [UQ_RutaCalandraDestajo_ColumnasDefinicion_Nombre] UNIQUE ([Nombre])
    )
    
    PRINT '? Tabla RutaCalandraDestajo_ColumnasDefinicion creada correctamente'
END
ELSE
BEGIN
    PRINT '? Tabla RutaCalandraDestajo_ColumnasDefinicion ya existe'
END
GO

-- ============================================================================
-- 4. DATOS DE EJEMPLO (OPCIONAL)
-- ============================================================================

IF NOT EXISTS (SELECT * FROM RutaCalandraDestajo)
BEGIN
    PRINT '?? Insertando datos de ejemplo para Ruta Calandra Destajo...'
    
    -- Nodos padre (nivel 0)
    INSERT INTO RutaCalandraDestajo (ID, ParentID, Nombre, Descripcion, Orden, Nivel, UsuarioCreacion)
    VALUES 
        (1, NULL, 'Calandria - Fase 1', 'Primera fase del proyecto Calandria', 0, 0, 'admin'),
        (2, NULL, 'Calandria - Fase 2', 'Segunda fase del proyecto Calandria', 1, 0, 'admin')
    
    -- Sub-padres (nivel 1)
    INSERT INTO RutaCalandraDestajo (ID, ParentID, Nombre, Descripcion, Orden, Nivel, UsuarioCreacion)
    VALUES 
        (10, 1, 'Urbanización', 'Trabajos de urbanización', 0, 1, 'admin'),
        (11, 1, 'Construcción', 'Trabajos de construcción', 1, 1, 'admin'),
        (20, 2, 'Acabados', 'Trabajos de acabados', 0, 1, 'admin')
    
    -- Hijos (nivel 2) con tipos de tarea
    INSERT INTO RutaCalandraDestajo (ID, ParentID, Nombre, Descripcion, Orden, Nivel, TipoTarea, UsuarioCreacion)
    VALUES 
        (100, 10, 'Concreto hidráulico', 'Material para pavimentos', 0, 2, 1, 'admin'),
        (101, 10, 'Colocación de pavimento', 'Mano de obra especializada', 1, 2, 2, 'admin'),
        (110, 11, 'Block', 'Material de construcción', 0, 2, 1, 'admin'),
        (111, 11, 'Albañilería', 'Mano de obra de construcción', 1, 2, 2, 'admin'),
        (200, 20, 'Pintura', 'Material de acabados', 0, 2, 1, 'admin'),
        (201, 20, 'Aplicación de pintura', 'Mano de obra de pintor', 1, 2, 2, 'admin')
    
    -- Columnas personalizadas de ejemplo
    INSERT INTO RutaCalandraDestajo_ColumnasDefinicion (Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato)
    VALUES 
        ('Presupuesto', 'Presupuesto', 120, 'System.Decimal', 1, '{0:C2}'),
        ('Avance', 'Avance %', 100, 'System.Decimal', 1, '{0:F1}%'),
        ('Responsable', 'Responsable', 150, 'System.String', 1, NULL),
        ('FechaInicio', 'Inicio', 100, 'System.DateTime', 1, '{0:dd/MM/yyyy}'),
        ('FechaFin', 'Fin', 100, 'System.DateTime', 1, '{0:dd/MM/yyyy}')
    
    PRINT '? Datos de ejemplo insertados correctamente'
END
ELSE
BEGIN
    PRINT '? La tabla ya contiene datos, no se insertarán ejemplos'
END
GO

-- ============================================================================
-- 5. RENOMBRAR TABLAS EXISTENTES (TreeListData -> RutaTuneraDestajo)
-- ============================================================================

PRINT ''
PRINT '============================================================================'
PRINT '         RENOMBRANDO TABLAS EXISTENTES'
PRINT '============================================================================'
PRINT ''

-- Renombrar tabla principal
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TreeListData]') AND type in (N'U'))
   AND NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RutaTuneraDestajo]') AND type in (N'U'))
BEGIN
    EXEC sp_rename 'TreeListData', 'RutaTuneraDestajo'
    PRINT '? Tabla TreeListData renombrada a RutaTuneraDestajo'
END
ELSE IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RutaTuneraDestajo]') AND type in (N'U'))
BEGIN
    PRINT '? Tabla RutaTuneraDestajo ya existe'
END
GO

-- Renombrar tabla de columnas
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TreeListData_Columnas]') AND type in (N'U'))
   AND NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RutaTuneraDestajo_Columnas]') AND type in (N'U'))
BEGIN
    EXEC sp_rename 'TreeListData_Columnas', 'RutaTuneraDestajo_Columnas'
    PRINT '? Tabla TreeListData_Columnas renombrada a RutaTuneraDestajo_Columnas'
END
ELSE IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RutaTuneraDestajo_Columnas]') AND type in (N'U'))
BEGIN
    PRINT '? Tabla RutaTuneraDestajo_Columnas ya existe'
END
GO

-- Renombrar tabla de definición de columnas
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TreeListData_ColumnasDefinicion]') AND type in (N'U'))
   AND NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RutaTuneraDestajo_ColumnasDefinicion]') AND type in (N'U'))
BEGIN
    EXEC sp_rename 'TreeListData_ColumnasDefinicion', 'RutaTuneraDestajo_ColumnasDefinicion'
    PRINT '? Tabla TreeListData_ColumnasDefinicion renombrada a RutaTuneraDestajo_ColumnasDefinicion'
END
ELSE IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RutaTuneraDestajo_ColumnasDefinicion]') AND type in (N'U'))
BEGIN
    PRINT '? Tabla RutaTuneraDestajo_ColumnasDefinicion ya existe'
END
GO

-- Renombrar restricciones y claves foráneas
PRINT ''
PRINT 'Actualizando restricciones...'



-- Primero eliminar la FK existente
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TreeListData_Columnas_Nodo')
BEGIN
    ALTER TABLE RutaTuneraDestajo_Columnas DROP CONSTRAINT FK_TreeListData_Columnas_Nodo
    PRINT '? FK antigua eliminada'
END
GO

-- Eliminar PK antigua si existe
IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_TreeListData')
BEGIN
    ALTER TABLE RutaTuneraDestajo DROP CONSTRAINT PK_TreeListData
    PRINT '? PK antigua eliminada'
END
GO

-- Crear nueva PK
IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_RutaTuneraDestajo')
BEGIN
    ALTER TABLE RutaTuneraDestajo ADD CONSTRAINT PK_RutaTuneraDestajo PRIMARY KEY CLUSTERED (ID)
    PRINT '? Nueva PK creada: PK_RutaTuneraDestajo'
END
GO

-- Crear nueva FK
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RutaTuneraDestajo_Columnas_Nodo')
BEGIN
    ALTER TABLE RutaTuneraDestajo_Columnas 
    ADD CONSTRAINT FK_RutaTuneraDestajo_Columnas_Nodo 
    FOREIGN KEY (NodoID) REFERENCES RutaTuneraDestajo(ID) ON DELETE CASCADE
    PRINT '? Nueva FK creada: FK_RutaTuneraDestajo_Columnas_Nodo'
END
GO

-- Renombrar PK de tabla de definición de columnas si existe
IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_TreeListData_ColumnasDefinicion')
BEGIN
    ALTER TABLE RutaTuneraDestajo_ColumnasDefinicion DROP CONSTRAINT PK_TreeListData_ColumnasDefinicion
    ALTER TABLE RutaTuneraDestajo_ColumnasDefinicion ADD CONSTRAINT PK_RutaTuneraDestajo_ColumnasDefinicion PRIMARY KEY CLUSTERED (ID)
    PRINT '? PK de ColumnasDefinicion actualizada'
END
GO

-- Renombrar constraint unique si existe
IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'UQ_TreeListData_ColumnasDefinicion_Nombre')
BEGIN
    ALTER TABLE RutaTuneraDestajo_ColumnasDefinicion DROP CONSTRAINT UQ_TreeListData_ColumnasDefinicion_Nombre
    ALTER TABLE RutaTuneraDestajo_ColumnasDefinicion ADD CONSTRAINT UQ_RutaTuneraDestajo_ColumnasDefinicion_Nombre UNIQUE (Nombre)
    PRINT '? Constraint unique de ColumnasDefinicion actualizada'
END
GO

-- Renombrar índices si existen
DECLARE @sql nvarchar(max)

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TreeListData_ParentID')
BEGIN
    EXEC sp_rename 'RutaTuneraDestajo.IX_TreeListData_ParentID', 'IX_RutaTuneraDestajo_ParentID', 'INDEX'
    PRINT '? Índice IX_RutaTuneraDestajo_ParentID renombrado'
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TreeListData_Nivel_Orden')
BEGIN
    EXEC sp_rename 'RutaTuneraDestajo.IX_TreeListData_Nivel_Orden', 'IX_RutaTuneraDestajo_Nivel_Orden', 'INDEX'
    PRINT '? Índice IX_RutaTuneraDestajo_Nivel_Orden renombrado'
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TreeListData_TipoTarea')
BEGIN
    EXEC sp_rename 'RutaTuneraDestajo.IX_TreeListData_TipoTarea', 'IX_RutaTuneraDestajo_TipoTarea', 'INDEX'
    PRINT '? Índice IX_RutaTuneraDestajo_TipoTarea renombrado'
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TreeListData_Columnas_NodoID')
BEGIN
    EXEC sp_rename 'RutaTuneraDestajo_Columnas.IX_TreeListData_Columnas_NodoID', 'IX_RutaTuneraDestajo_Columnas_NodoID', 'INDEX'
    PRINT '? Índice IX_RutaTuneraDestajo_Columnas_NodoID renombrado'
END

PRINT '? Todas las restricciones e índices actualizados'
GO

-- ============================================================================
-- 6. VERIFICACIÓN FINAL
-- ============================================================================

PRINT ''
PRINT '============================================================================'
PRINT '                    VERIFICACIÓN DE INSTALACIÓN'
PRINT '============================================================================'
PRINT ''

DECLARE @CountRutaTunera int
DECLARE @CountRutaCalandra int

SELECT @CountRutaTunera = COUNT(*) FROM RutaTuneraDestajo
SELECT @CountRutaCalandra = COUNT(*) FROM RutaCalandraDestajo

PRINT '?? Registros actuales:'
PRINT '   - RutaTuneraDestajo: ' + CAST(@CountRutaTunera AS nvarchar(10))
PRINT '   - RutaCalandraDestajo: ' + CAST(@CountRutaCalandra AS nvarchar(10))
PRINT ''

-- Verificar tablas creadas
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'RutaTuneraDestajo'))
   AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'RutaCalandraDestajo'))
BEGIN
    PRINT '? INSTALACIÓN COMPLETADA EXITOSAMENTE'
    PRINT ''
    PRINT '? Tablas creadas:'
    PRINT '  - RutaTuneraDestajo (renombrada desde TreeListData)'
    PRINT '  - RutaCalandraDestajo (nueva)'
    PRINT ''
    PRINT '?? Siguiente paso: Actualizar la aplicación para usar ambas TreeLists'
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

1. Para abrir el editor con Ruta Tunera:
   
   using (var form = new FormEditorTreeList(connectionString, "RutaTuneraDestajo"))
   {
       form.ShowDialog();
   }

2. Para abrir el editor con Ruta Calandra:
   
   using (var form = new FormEditorTreeList(connectionString, "RutaCalandraDestajo"))
   {
       form.ShowDialog();
   }

3. Estructura de tablas:
   - RutaTuneraDestajo (antes TreeListData)
   - RutaTuneraDestajo_Columnas
   - RutaTuneraDestajo_ColumnasDefinicion
   - RutaCalandraDestajo
   - RutaCalandraDestajo_Columnas
   - RutaCalandraDestajo_ColumnasDefinicion

==============================================================================
*/
