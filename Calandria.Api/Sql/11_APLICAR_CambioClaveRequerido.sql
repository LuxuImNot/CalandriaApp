-- ============================================================================
-- Restablecimiento de contraseña con cambio obligatorio al primer ingreso.
--
-- Usuarios.CambioClaveRequerido = 1 marca una cuenta cuya contraseña la fijó un
-- administrador (PerfilesController.RestablecerClave). Mientras siga en 1, el
-- JWT del usuario lleva el claim "cambioClave" y JwtMessageHandler le rechaza
-- todo salvo api/auth/cambiar-clave: la cuenta entra pero no opera hasta poner
-- una contraseña propia. Cambiarla la vuelve a 0 y emite un token limpio.
--
-- Corre contra CalandriaControl (la BD maestra, tabla Usuarios).
-- Idempotente: se puede correr varias veces sin duplicar nada.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'CambioClaveRequerido')
BEGIN
    ALTER TABLE Usuarios
        ADD CambioClaveRequerido BIT NOT NULL
            CONSTRAINT DF_Usuarios_CambioClaveRequerido DEFAULT 0;
END
GO

-- Las cuentas que ya existen no se tocan: siguen con su contraseña y con la
-- marca en 0. Solo un restablecimiento hecho por un administrador la sube a 1.

-- Verificación (debe devolver la columna con DEFAULT 0 y ningún usuario marcado):
-- SELECT COUNT(*) AS PendientesDeCambio FROM Usuarios WHERE CambioClaveRequerido = 1;
