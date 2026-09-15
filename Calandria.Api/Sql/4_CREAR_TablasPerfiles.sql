-- ============================================================================
-- Sistema de roles: Perfiles (nombre + descripción) con un conjunto de
-- permisos (PerfilPermisos), y Usuarios.PerfilId apuntando al perfil asignado.
-- Reemplaza el criterio anterior de "Rol = Admin / cualquier otra cosa";
-- Rol se conserva sin usar por compatibilidad (nadie más la lee tras este
-- cambio), no se borra para no romper otras copias de la base.
-- Idempotente: se puede correr varias veces sin duplicar nada.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Perfiles')
BEGIN
    CREATE TABLE Perfiles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL UNIQUE,
        Descripcion NVARCHAR(300) NULL,
        EsSistema BIT NOT NULL DEFAULT 0, -- perfiles Admin/SoloLectura sembrados: no se pueden borrar
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PerfilPermisos')
BEGIN
    CREATE TABLE PerfilPermisos (
        PerfilId INT NOT NULL REFERENCES Perfiles(Id) ON DELETE CASCADE,
        Permiso NVARCHAR(100) NOT NULL,
        PRIMARY KEY (PerfilId, Permiso)
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'PerfilId')
BEGIN
    ALTER TABLE Usuarios ADD PerfilId INT NULL REFERENCES Perfiles(Id);
END

-- Perfiles de sistema, equivalentes a los dos valores de Rol que existían.
IF NOT EXISTS (SELECT 1 FROM Perfiles WHERE Nombre = 'Admin')
    INSERT INTO Perfiles (Nombre, Descripcion, EsSistema) VALUES ('Admin', 'Acceso total al sistema.', 1);

IF NOT EXISTS (SELECT 1 FROM Perfiles WHERE Nombre = 'SoloLectura')
    INSERT INTO Perfiles (Nombre, Descripcion, EsSistema) VALUES ('SoloLectura', 'Solo consulta, sin permisos de edición.', 1);

-- Admin: todos los permisos del catálogo (ver Calandria.Api/Auth/PermisosCatalogo.cs).
INSERT INTO PerfilPermisos (PerfilId, Permiso)
SELECT p.Id, x.Permiso
FROM Perfiles p
CROSS JOIN (VALUES
    ('sistema.administrador'), ('sistema.perfiles'), ('sistema.usuarios'),
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

-- SoloLectura: solo los permisos *.ver.
INSERT INTO PerfilPermisos (PerfilId, Permiso)
SELECT p.Id, x.Permiso
FROM Perfiles p
CROSS JOIN (VALUES
    ('almacen.ver'), ('compras.ver'), ('nomina.ver'),
    ('trabajadores.ver'), ('destajos.ver'), ('estimaciones.ver')
) x(Permiso)
WHERE p.Nombre = 'SoloLectura'
  AND NOT EXISTS (SELECT 1 FROM PerfilPermisos pp WHERE pp.PerfilId = p.Id AND pp.Permiso = x.Permiso);

-- Backfill: usuarios existentes sin PerfilId se asignan por el valor de su Rol actual.
UPDATE u
SET u.PerfilId = p.Id
FROM Usuarios u
JOIN Perfiles p ON p.Nombre = u.Rol
WHERE u.PerfilId IS NULL;

-- Cualquier usuario que no haya calzado (Rol distinto de Admin/SoloLectura) cae en SoloLectura.
UPDATE u
SET u.PerfilId = p.Id
FROM Usuarios u
CROSS JOIN (SELECT Id FROM Perfiles WHERE Nombre = 'SoloLectura') p
WHERE u.PerfilId IS NULL;
