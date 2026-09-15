-- ============================================================================
-- BD de pruebas LOCAL, desde cero (sin depender de Tailscale ni de la BD de
-- producción). Crea CalandriaControl con el mismo esquema que producción
-- (reconstruido a partir de SQL_CrearBDMaestraYMigrar.sql + Calandria.Api/Sql/
-- 4_CREAR_TablasPerfiles.sql, 5_PERSONALIZACION_Obras.sql,
-- 6_PERFIL_USUARIO_Foto.sql, 9_APLICAR_TerminosAceptados.sql) y siembra:
--   - un perfil "Admin" con TODOS los permisos vigentes (Calandria.Api/Auth/
--     PermisosCatalogo.cs), incluidos sistema.obras/sistema.facturacion que el
--     script de perfiles original no traía todavía.
--   - un usuario admin/admin123 (hash SHA-256 heredado; el primer login lo
--     migra solo a PBKDF2, ver DynamicSepticSystem/PasswordHasher.cs).
--
-- La obra de prueba (su propia BD) NO se crea aquí: se aprovisiona llamando a
-- POST /api/obras con el Calandria.Api corriendo local, para reutilizar el
-- mismo código que aplica Sql/ObraPlantilla.sql en producción (ver
-- ObrasController.Crear) en vez de reimplementarlo a mano.
--
-- Ejecutar con: sqlcmd -S "localhost\SQLEXPRESS" -E -C -i SQL_Local_CrearBDMaestra.sql
-- Idempotente: se puede correr varias veces sin duplicar nada.
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Usuarios')
BEGIN
    CREATE TABLE Usuarios (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre VARCHAR(50) NOT NULL UNIQUE,
        ClaveHash VARCHAR(256) NULL,
        Rol VARCHAR(20) NULL,
        PerfilId INT NULL REFERENCES Perfiles(Id),
        FotoBytes VARBINARY(MAX) NULL,
        FotoExtension NVARCHAR(10) NULL,
        FechaAlta DATETIME NOT NULL DEFAULT GETDATE(),
        FechaAltaConfirmada BIT NOT NULL DEFAULT 0
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
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        LogoBytes VARBINARY(MAX) NULL,
        LogoExtension NVARCHAR(10) NULL,
        ColorPrimario NVARCHAR(9) NULL,
        ColorSecundario NVARCHAR(9) NULL,
        ColorSuave NVARCHAR(9) NULL
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TerminosAceptados')
BEGIN
    CREATE TABLE TerminosAceptados (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Usuario NVARCHAR(100) NOT NULL,
        VersionHash NVARCHAR(64) NOT NULL,
        FechaAceptacion DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_TerminosAceptados_Usuario_Version UNIQUE (Usuario, VersionHash)
    );
END
GO

-- ---- Siembra: perfil Admin con TODOS los permisos vigentes ----
IF NOT EXISTS (SELECT 1 FROM Perfiles WHERE Nombre = 'Admin')
    INSERT INTO Perfiles (Nombre, Descripcion, EsSistema) VALUES ('Admin', 'Acceso total al sistema (local de pruebas).', 1);
GO

INSERT INTO PerfilPermisos (PerfilId, Permiso)
SELECT p.Id, x.Permiso
FROM Perfiles p
CROSS JOIN (VALUES
    ('sistema.administrador'), ('sistema.perfiles'), ('sistema.usuarios'),
    ('sistema.obras'), ('sistema.facturacion'),
    ('almacen.ver'), ('almacen.editar'),
    ('compras.ver'), ('compras.editar'),
    ('nomina.ver'), ('nomina.editar'),
    ('trabajadores.ver'), ('trabajadores.editar'),
    ('destajos.ver'), ('destajos.editar'),
    ('estimaciones.ver'), ('estimaciones.editar'),
    ('errores.ver')
) x(Permiso)
WHERE p.Nombre = 'Admin'
  AND NOT EXISTS (SELECT 1 FROM PerfilPermisos pp WHERE pp.PerfilId = p.Id AND pp.Permiso = x.Permiso);
GO

-- ---- Siembra: usuario admin / admin123 ----
-- Hash SHA-256 heredado de "admin123" (formato de 64 hex que PasswordHasher.Verificar
-- todavía acepta); el primer login lo reemplaza solo por PBKDF2. Cambia la clave
-- después de tu primer login si vas a dejar este entorno corriendo un tiempo.
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Nombre = 'admin')
BEGIN
    INSERT INTO Usuarios (Nombre, ClaveHash, PerfilId, FechaAlta, FechaAltaConfirmada)
    SELECT 'admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', p.Id, GETDATE(), 1
    FROM Perfiles p WHERE p.Nombre = 'Admin';
END
GO

PRINT 'CalandriaControl lista. Usuario: admin / Clave: admin123 (cámbiala tras el primer login).';
GO
