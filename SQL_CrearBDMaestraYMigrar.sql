-- ============================================================================
-- Multi-obra: crea CalandriaControl (BD maestra) y migra Usuarios/Perfiles/
-- PerfilPermisos desde CALANDRIA. Ejecutar UNA VEZ vía SSMS/sqlcmd, con un
-- login que tenga permiso CREATE DATABASE en el servidor.
--
-- Paso 0 (MANUAL, antes de correr esto): verificar con SSMS que el CREATE
-- TABLE Usuarios de abajo coincide EXACTO con las columnas reales de
-- CALANDRIA.dbo.Usuarios (tipos, nulabilidad, columnas extra que no se ven
-- desde el código C#). Ajustar si no coincide. También verificar que
-- Usuarios.Nombre no tiene duplicados (esta migración le pone UNIQUE).
--
-- Hacer un backup completo de CALANDRIA antes de ejecutar.
-- ============================================================================

IF DB_ID('CalandriaControl') IS NULL
    CREATE DATABASE CalandriaControl;
GO

USE CalandriaControl;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Perfiles')
BEGIN
    CREATE TABLE Perfiles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL UNIQUE,
        Descripcion NVARCHAR(300) NULL,
        EsSistema BIT NOT NULL DEFAULT 0,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PerfilPermisos')
BEGIN
    CREATE TABLE PerfilPermisos (
        PerfilId INT NOT NULL REFERENCES Perfiles(Id) ON DELETE CASCADE,
        Permiso NVARCHAR(100) NOT NULL,
        PRIMARY KEY (PerfilId, Permiso)
    );
END
GO

-- CALANDRIA.dbo.Usuarios NO tiene columna Id (confirmado: ninguna consulta del
-- API la usa, siempre buscan por Nombre) -- la tabla nueva genera su propio Id
-- con IDENTITY, sin intentar preservar nada del origen.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Usuarios')
BEGIN
    CREATE TABLE Usuarios (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre VARCHAR(50) NOT NULL UNIQUE,
        ClaveHash VARCHAR(256) NULL,
        Rol VARCHAR(20) NULL,
        PerfilId INT NULL REFERENCES Perfiles(Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Obras')
BEGIN
    CREATE TABLE Obras (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(150) NOT NULL UNIQUE,
        NombreBD NVARCHAR(128) NOT NULL UNIQUE,
        Activa BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UsuarioObras')
BEGIN
    CREATE TABLE UsuarioObras (
        Usuario NVARCHAR(100) NOT NULL,
        ObraId INT NOT NULL REFERENCES Obras(Id) ON DELETE CASCADE,
        FechaAsignacion DATETIME NOT NULL DEFAULT GETDATE(),
        PRIMARY KEY (Usuario, ObraId)
    );
END
GO

-- Copia de datos: mismo servidor -> nombre de 3 partes, sin linked server.
-- Re-ejecutable: cada INSERT solo trae filas que aún no existen.
SET IDENTITY_INSERT Perfiles ON;
INSERT INTO Perfiles (Id, Nombre, Descripcion, EsSistema, FechaCreacion)
SELECT p.Id, p.Nombre, p.Descripcion, p.EsSistema, p.FechaCreacion
FROM CALANDRIA.dbo.Perfiles p
WHERE NOT EXISTS (SELECT 1 FROM Perfiles WHERE Id = p.Id);
SET IDENTITY_INSERT Perfiles OFF;
GO

INSERT INTO PerfilPermisos (PerfilId, Permiso)
SELECT pp.PerfilId, pp.Permiso
FROM CALANDRIA.dbo.PerfilPermisos pp
WHERE NOT EXISTS (SELECT 1 FROM PerfilPermisos WHERE PerfilId = pp.PerfilId AND Permiso = pp.Permiso);
GO

-- Sin IDENTITY_INSERT: como nada usa Usuarios.Id, dejamos que el Id nuevo se
-- autogenere; el emparejamiento entre CALANDRIA y CalandriaControl es por
-- Nombre (la clave natural que ya usa toda la app).
INSERT INTO Usuarios (Nombre, ClaveHash, Rol, PerfilId)
SELECT u.Nombre, u.ClaveHash, u.Rol, u.PerfilId
FROM CALANDRIA.dbo.Usuarios u
WHERE NOT EXISTS (SELECT 1 FROM Usuarios WHERE Nombre = u.Nombre);
GO

-- CALANDRIA pasa a ser la primera obra; todos los usuarios existentes ya
-- tenían acceso implícito a la única obra que había, así que se les da acceso.
IF NOT EXISTS (SELECT 1 FROM Obras WHERE NombreBD = 'CALANDRIA')
    INSERT INTO Obras (Nombre, NombreBD, Activa) VALUES ('Calandria Residencial', 'CALANDRIA', 1);
GO

INSERT INTO UsuarioObras (Usuario, ObraId)
SELECT u.Nombre, o.Id
FROM Usuarios u
CROSS JOIN (SELECT Id FROM Obras WHERE NombreBD = 'CALANDRIA') o
WHERE NOT EXISTS (SELECT 1 FROM UsuarioObras uo WHERE uo.Usuario = u.Nombre AND uo.ObraId = o.Id);
GO

-- ============================================================================
-- Paso final, MANUAL y APARTE (no ejecutar en la misma pasada): renombrar, NO
-- borrar (reversible si algo salió mal), solo después de confirmar que el
-- nuevo Calandria.Api ya está desplegado y funcionando contra CalandriaControl:
--
--   USE CALANDRIA;
--   EXEC sp_rename 'dbo.Usuarios', 'Usuarios_DEPRECATED_migrado';
--   EXEC sp_rename 'dbo.Perfiles', 'Perfiles_DEPRECATED_migrado';
--   EXEC sp_rename 'dbo.PerfilPermisos', 'PerfilPermisos_DEPRECATED_migrado';
-- ============================================================================
