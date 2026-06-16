/*
==============================================================================
SCRIPT SIMPLE: Solo Renombrar TreeListData a RutaTuneraDestajo
==============================================================================
Descripción: Renombra las tablas TreeListData a RutaTuneraDestajo
            Ejecuta SOLO este script si ya tienes datos en TreeListData
Autor: Sistema Calandria Residencial
Fecha: Enero 2025
==============================================================================
*/

USE [BaseDatosCalandria]
GO

PRINT ''
PRINT '============================================================================'
PRINT '   RENOMBRANDO TreeListData A RutaTuneraDestajo'
PRINT '============================================================================'
PRINT ''

-- ============================================================================
-- 1. VERIFICAR QUE EXISTAN LAS TABLAS ANTIGUAS
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TreeListData]') AND type in (N'U'))
BEGIN
    PRINT '? ERROR: La tabla TreeListData no existe.'
    PRINT '   Posiblemente ya fue renombrada o nunca existió.'
    PRINT ''
    PRINT '   Verifica con: SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE ''%TreeList%'''
    GOTO FIN
END

PRINT '? Tabla TreeListData encontrada'
PRINT ''

-- ============================================================================
-- 2. RENOMBRAR TABLAS
-- ============================================================================

-- Paso 1: Eliminar FK que referencia la tabla principal
PRINT '? Eliminando claves foráneas...'

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TreeListData_Columnas_Nodo')
BEGIN
    ALTER TABLE TreeListData_Columnas DROP CONSTRAINT FK_TreeListData_Columnas_Nodo
    PRINT '  ? FK_TreeListData_Columnas_Nodo eliminada'
END

-- Paso 2: Renombrar tabla de columnas (valores)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TreeListData_Columnas]') AND type in (N'U'))
BEGIN
    EXEC sp_rename 'TreeListData_Columnas', 'RutaTuneraDestajo_Columnas'
    PRINT '  ? TreeListData_Columnas ? RutaTuneraDestajo_Columnas'
END

-- Paso 3: Renombrar tabla de definición de columnas
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TreeListData_ColumnasDefinicion]') AND type in (N'U'))
BEGIN
    EXEC sp_rename 'TreeListData_ColumnasDefinicion', 'RutaTuneraDestajo_ColumnasDefinicion'
    PRINT '  ? TreeListData_ColumnasDefinicion ? RutaTuneraDestajo_ColumnasDefinicion'
END

-- Paso 4: Renombrar tabla principal
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TreeListData]') AND type in (N'U'))
BEGIN
    EXEC sp_rename 'TreeListData', 'RutaTuneraDestajo'
    PRINT '  ? TreeListData ? RutaTuneraDestajo'
END

PRINT ''
PRINT '============================================================================'
PRINT '   ACTUALIZANDO RESTRICCIONES E ÍNDICES'
PRINT '============================================================================'
PRINT ''

-- ============================================================================
-- 3. ACTUALIZAR RESTRICCIONES
-- ============================================================================

-- Eliminar PK antigua de tabla principal
IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_TreeListData' AND parent_object_id = OBJECT_ID('RutaTuneraDestajo'))
BEGIN
    ALTER TABLE RutaTuneraDestajo DROP CONSTRAINT PK_TreeListData
    PRINT '  ? PK antigua eliminada'
END

-- Crear nueva PK
IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_RutaTuneraDestajo')
BEGIN
    ALTER TABLE RutaTuneraDestajo ADD CONSTRAINT PK_RutaTuneraDestajo PRIMARY KEY CLUSTERED (ID)
    PRINT '  ? Nueva PK creada: PK_RutaTuneraDestajo'
END

-- Crear nueva FK
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RutaTuneraDestajo_Columnas_Nodo')
BEGIN
    ALTER TABLE RutaTuneraDestajo_Columnas 
    ADD CONSTRAINT FK_RutaTuneraDestajo_Columnas_Nodo 
    FOREIGN KEY (NodoID) REFERENCES RutaTuneraDestajo(ID) ON DELETE CASCADE
    PRINT '  ? Nueva FK creada: FK_RutaTuneraDestajo_Columnas_Nodo'
END

-- Actualizar PK de tabla de columnas
IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_TreeListData_Columnas' AND parent_object_id = OBJECT_ID('RutaTuneraDestajo_Columnas'))
BEGIN
    ALTER TABLE RutaTuneraDestajo_Columnas DROP CONSTRAINT PK_TreeListData_Columnas
    ALTER TABLE RutaTuneraDestajo_Columnas ADD CONSTRAINT PK_RutaTuneraDestajo_Columnas PRIMARY KEY CLUSTERED (ID)
    PRINT '  ? PK de Columnas actualizada'
END

-- Actualizar PK de tabla de definición
IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_TreeListData_ColumnasDefinicion' AND parent_object_id = OBJECT_ID('RutaTuneraDestajo_ColumnasDefinicion'))
BEGIN
    ALTER TABLE RutaTuneraDestajo_ColumnasDefinicion DROP CONSTRAINT PK_TreeListData_ColumnasDefinicion
    ALTER TABLE RutaTuneraDestajo_ColumnasDefinicion ADD CONSTRAINT PK_RutaTuneraDestajo_ColumnasDefinicion PRIMARY KEY CLUSTERED (ID)
    PRINT '  ? PK de ColumnasDefinicion actualizada'
END

-- Actualizar constraint UNIQUE
IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'UQ_TreeListData_ColumnasDefinicion_Nombre' AND parent_object_id = OBJECT_ID('RutaTuneraDestajo_ColumnasDefinicion'))
BEGIN
    ALTER TABLE RutaTuneraDestajo_ColumnasDefinicion DROP CONSTRAINT UQ_TreeListData_ColumnasDefinicion_Nombre
    ALTER TABLE RutaTuneraDestajo_ColumnasDefinicion ADD CONSTRAINT UQ_RutaTuneraDestajo_ColumnasDefinicion_Nombre UNIQUE (Nombre)
    PRINT '  ? Constraint UNIQUE actualizada'
END

PRINT ''

-- ============================================================================
-- 4. RENOMBRAR ÍNDICES
-- ============================================================================

PRINT '? Renombrando índices...'

-- Índices de tabla principal
DECLARE @oldIndexName NVARCHAR(128), @newIndexName NVARCHAR(128)

-- IX_TreeListData_ParentID
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TreeListData_ParentID' AND object_id = OBJECT_ID('RutaTuneraDestajo'))
BEGIN
    EXEC sp_rename 'RutaTuneraDestajo.IX_TreeListData_ParentID', 'IX_RutaTuneraDestajo_ParentID', 'INDEX'
    PRINT '  ? IX_RutaTuneraDestajo_ParentID'
END

-- IX_TreeListData_Nivel_Orden
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TreeListData_Nivel_Orden' AND object_id = OBJECT_ID('RutaTuneraDestajo'))
BEGIN
    EXEC sp_rename 'RutaTuneraDestajo.IX_TreeListData_Nivel_Orden', 'IX_RutaTuneraDestajo_Nivel_Orden', 'INDEX'
    PRINT '  ? IX_RutaTuneraDestajo_Nivel_Orden'
END

-- IX_TreeListData_TipoTarea
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TreeListData_TipoTarea' AND object_id = OBJECT_ID('RutaTuneraDestajo'))
BEGIN
    EXEC sp_rename 'RutaTuneraDestajo.IX_TreeListData_TipoTarea', 'IX_RutaTuneraDestajo_TipoTarea', 'INDEX'
    PRINT '  ? IX_RutaTuneraDestajo_TipoTarea'
END

-- Índices de tabla de columnas
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TreeListData_Columnas_NodoID' AND object_id = OBJECT_ID('RutaTuneraDestajo_Columnas'))
BEGIN
    EXEC sp_rename 'RutaTuneraDestajo_Columnas.IX_TreeListData_Columnas_NodoID', 'IX_RutaTuneraDestajo_Columnas_NodoID', 'INDEX'
    PRINT '  ? IX_RutaTuneraDestajo_Columnas_NodoID'
END

PRINT ''

-- ============================================================================
-- 5. VERIFICACIÓN FINAL
-- ============================================================================

PRINT '============================================================================'
PRINT '                    VERIFICACIÓN'
PRINT '============================================================================'
PRINT ''

DECLARE @CountRutaTunera INT

SELECT @CountRutaTunera = COUNT(*) FROM RutaTuneraDestajo

PRINT '?? Registros en RutaTuneraDestajo: ' + CAST(@CountRutaTunera AS NVARCHAR(10))
PRINT ''

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'RutaTuneraDestajo'))
   AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'RutaTuneraDestajo_Columnas'))
   AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'RutaTuneraDestajo_ColumnasDefinicion'))
BEGIN
    PRINT '? RENOMBRAMIENTO COMPLETADO EXITOSAMENTE'
    PRINT ''
    PRINT '? Tablas renombradas:'
    PRINT '  - TreeListData ? RutaTuneraDestajo'
    PRINT '  - TreeListData_Columnas ? RutaTuneraDestajo_Columnas'
    PRINT '  - TreeListData_ColumnasDefinicion ? RutaTuneraDestajo_ColumnasDefinicion'
    PRINT ''
    PRINT '?? Próximo paso: Ejecuta CrearTablasRutaCalandraDestajo.sql'
    PRINT '   para crear la segunda ruta (RutaCalandraDestajo)'
END
ELSE
BEGIN
    PRINT '? ERROR: No todas las tablas fueron renombradas correctamente'
END

PRINT '============================================================================'

FIN:
PRINT ''
GO

/*
==============================================================================
VERIFICACIÓN POST-SCRIPT
==============================================================================

Ejecuta esta consulta para verificar el renombramiento:

    SELECT TABLE_NAME 
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_NAME LIKE '%Ruta%' OR TABLE_NAME LIKE '%TreeList%'
    ORDER BY TABLE_NAME

Deberías ver:
    - RutaTuneraDestajo
    - RutaTuneraDestajo_Columnas
    - RutaTuneraDestajo_ColumnasDefinicion

Y NO deberías ver:
    - TreeListData
    - TreeListData_Columnas
    - TreeListData_ColumnasDefinicion

==============================================================================
*/
