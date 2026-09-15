-- ============================================================================
-- Aislamiento multi-cliente: agrega el concepto de "Cliente" (empresa dueña
-- de una o más obras) arriba de Obras/Usuarios/Perfiles. Hasta ahora todo
-- vivía en una sola BD maestra compartida sin distinguir de qué empresa era
-- cada obra/usuario/perfil -- correcto mientras solo Calandria Residencial
-- usaba el sistema, pero un hueco real si el mismo servidor hospeda a otras
-- constructoras: cualquier admin con "sistema.perfiles" podía ver/editar
-- obras, usuarios y perfiles de TODAS las empresas, incluida la asignación
-- de acceso (ver ObrasController.AsignarObrasUsuario).
--
-- Este script solo prepara los datos existentes (todo pasa a pertenecer al
-- Cliente 1 = "Calandria Residencial"). El filtrado real vive en el código
-- (TokenService agrega el claim "cliente"; ObrasController/PerfilesController
-- ya filtran por él).
--
-- Corre contra CalandriaControl (la BD maestra). Idempotente: se puede correr
-- varias veces sin duplicar nada. Aplícalo ANTES de desplegar el Calandria.Api
-- nuevo (el código nuevo asume que estas columnas ya existen).
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Clientes')
BEGIN
    CREATE TABLE Clientes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(150) NOT NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT Clientes ON;
    INSERT INTO Clientes (Id, Nombre, Activo) VALUES (1, 'Calandria Residencial', 1);
    SET IDENTITY_INSERT Clientes OFF;
END
GO

-- ---- Usuarios ----
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Usuarios') AND name = 'ClienteId')
    ALTER TABLE Usuarios ADD ClienteId INT NULL;
GO

UPDATE Usuarios SET ClienteId = 1 WHERE ClienteId IS NULL;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Usuarios') AND name = 'ClienteId' AND is_nullable = 1)
    ALTER TABLE Usuarios ALTER COLUMN ClienteId INT NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Usuarios_Clientes')
    ALTER TABLE Usuarios ADD CONSTRAINT FK_Usuarios_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(Id);
GO

-- ---- Obras ----
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Obras') AND name = 'ClienteId')
    ALTER TABLE Obras ADD ClienteId INT NULL;
GO

UPDATE Obras SET ClienteId = 1 WHERE ClienteId IS NULL;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Obras') AND name = 'ClienteId' AND is_nullable = 1)
    ALTER TABLE Obras ALTER COLUMN ClienteId INT NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Obras_Clientes')
    ALTER TABLE Obras ADD CONSTRAINT FK_Obras_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(Id);
GO

-- ---- Perfiles ----
-- Cada cliente maneja su propio catálogo de perfiles (incluidos los que hoy
-- son EsSistema=1); un cliente nuevo arranca sin perfiles y necesita que se le
-- cree al menos un perfil "Administrador" con todos los permisos (a mano, ver
-- nota al final) para poder autogestionarse desde ahí.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Perfiles') AND name = 'ClienteId')
    ALTER TABLE Perfiles ADD ClienteId INT NULL;
GO

UPDATE Perfiles SET ClienteId = 1 WHERE ClienteId IS NULL;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Perfiles') AND name = 'ClienteId' AND is_nullable = 1)
    ALTER TABLE Perfiles ALTER COLUMN ClienteId INT NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Perfiles_Clientes')
    ALTER TABLE Perfiles ADD CONSTRAINT FK_Perfiles_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(Id);
GO

-- ============================================================================
-- Nota: Obras.Nombre y Perfiles.Nombre siguen siendo UNIQUE a nivel de TODO el
-- servidor (no solo por cliente) -- dos clientes no pueden ambos tener una
-- obra o un perfil con el nombre exactamente igual. Es una limitación menor
-- de UX (mensaje "ya existe", hay que variar el nombre), no un problema de
-- seguridad; se puede resolver después si molesta en la práctica.
--
-- Alta de un cliente nuevo (manual, hasta que exista una pantalla):
--   INSERT INTO Clientes (Nombre) VALUES ('Constructora XYZ');
--   -- toma el Id nuevo, crea su primer perfil "Administrador" con todos los
--   -- permisos de PermisosCatalogo.Todos vía INSERT directo a Perfiles/
--   -- PerfilPermisos, y su primer usuario vía Calandria.Api.PasswordHasher.Hash
--   -- (o pide a ese admin que inicie sesión una vez creado con clave temporal).
-- ============================================================================
