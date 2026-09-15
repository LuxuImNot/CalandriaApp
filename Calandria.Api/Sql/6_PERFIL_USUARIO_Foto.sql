-- ============================================================================
-- Desglose del perfil activo (rail del panel web): foto del usuario y fecha de
-- alta de la cuenta, para el modal que se abre al hacer clic en el avatar.
-- FechaAlta no existía antes: a los usuarios ya creados les queda la fecha en
-- que se corre este script (no su ingreso real, que nunca se registró).
-- Corre contra CalandriaControl (la BD maestra, tabla Usuarios).
-- Idempotente: se puede correr varias veces sin duplicar nada.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'FotoBytes')
    ALTER TABLE Usuarios ADD FotoBytes VARBINARY(MAX) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'FotoExtension')
    ALTER TABLE Usuarios ADD FotoExtension NVARCHAR(10) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'FechaAlta')
    ALTER TABLE Usuarios ADD FechaAlta DATETIME NOT NULL DEFAULT GETDATE();

-- Distingue una FechaAlta real (capturada al crear el usuario o confirmada por
-- un admin desde "Perfiles y Permisos") de la fecha de respaldo de arriba, que
-- es solo el momento en que se corrió este script. El gestor de usuarios
-- muestra un aviso mientras esto siga en 0.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'FechaAltaConfirmada')
    ALTER TABLE Usuarios ADD FechaAltaConfirmada BIT NOT NULL DEFAULT 0;
