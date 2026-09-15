-- ============================================================================
-- Registro de aceptación de Términos de Uso / Aviso de Privacidad
-- (Calandria.Api/ui/terminos.html, TerminosController). La "versión vigente"
-- es el hash SHA-256 del propio archivo terminos.html: si se reemplaza ese
-- archivo en el servidor, el hash cambia y todos los usuarios deben volver a
-- aceptar, sin necesidad de tocar esta tabla.
--
-- Corre contra CalandriaControl (la BD maestra, tabla Usuarios).
-- Idempotente: se puede correr varias veces sin duplicar nada.
-- ============================================================================

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
