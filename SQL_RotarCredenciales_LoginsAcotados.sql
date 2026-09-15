-- ============================================================================
-- Reemplaza el uso de "sa" (control total del servidor) por dos logins
-- acotados, uno por consumidor:
--
--   CalandriaApiSvc        -> lo usa SOLO Calandria.Api (server-side).
--                              Necesita dbcreator (ObrasController crea/borra
--                              BDs de obra) + acceso a CalandriaControl y a
--                              cada BD de obra existente.
--
--   CalandriaClienteLegacy -> lo usa SOLO el cliente WinForms instalado en
--                              cada PC (el que de verdad está expuesto: viaja
--                              en connectionStrings.config junto al .exe).
--                              Solo necesita la BD "CALANDRIA" (es la única
--                              que ese cliente conoce hoy: una sola entrada
--                              CalandriaConn, sin selector de obra) y db_ddladmin
--                              porque varios Form*.cs auto-crean/alteran sus
--                              propias tablas en el primer uso (ver abajo).
--                              NO tiene dbcreator, NO toca master, NO toca
--                              CalandriaControl (Usuarios/Perfiles/Obras).
--
-- Por qué db_ddladmin y no solo datareader/datawriter: el cliente y el API
-- auto-crean/alteran tablas en runtime (ActivacionTareasRuta, PDFsDestajos,
-- MiembrosCuadrilla, TRABAJADORES, RecibosNomina, DestajoFotos,
-- DestajoInsumos, DestajoManoObra, AvanceManualConcepto...). Sin ddladmin la
-- app rompe la primera vez que una tabla nueva no existe todavía. Es un
-- permiso amplio dentro de la BD (puede crear/alterar CUALQUIER tabla ahí),
-- pero ya no es dueño del servidor ni de las demás BDs.
--
-- EJECUTAR EN SSMS/sqlcmd con un login que tenga sysadmin (o el propio sa,
-- por última vez). Cambiar las contraseñas de abajo antes de correr.
-- ============================================================================

USE master;
GO

DECLARE @PwdApi    NVARCHAR(128) = 'CAMBIA_ESTA_PASSWORD_API_1234!';
DECLARE @PwdCliente NVARCHAR(128) = 'CAMBIA_ESTA_PASSWORD_CLIENTE_5678!';

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'CalandriaApiSvc')
    EXEC('CREATE LOGIN CalandriaApiSvc WITH PASSWORD = ''' + @PwdApi + ''', CHECK_POLICY = ON, DEFAULT_DATABASE = CalandriaControl;');

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'CalandriaClienteLegacy')
    EXEC('CREATE LOGIN CalandriaClienteLegacy WITH PASSWORD = ''' + @PwdCliente + ''', CHECK_POLICY = ON, DEFAULT_DATABASE = CALANDRIA;');
GO

-- CalandriaApiSvc necesita poder crear/borrar BDs de obra (ObrasController).
-- Al crear una BD, el rol dbcreator lo hace dueño (db_owner) de esa BD nueva
-- automáticamente, así que las obras futuras no necesitan un paso extra.
ALTER SERVER ROLE dbcreator ADD MEMBER CalandriaApiSvc;
GO

-- ----------------------------------------------------------------------------
-- CalandriaControl: solo CalandriaApiSvc (Usuarios/Perfiles/Obras/UsuarioObras).
-- CalandriaClienteLegacy NO entra aquí.
-- ----------------------------------------------------------------------------
USE CalandriaControl;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'CalandriaApiSvc')
    CREATE USER CalandriaApiSvc FOR LOGIN CalandriaApiSvc;
ALTER ROLE db_datareader ADD MEMBER CalandriaApiSvc;
ALTER ROLE db_datawriter ADD MEMBER CalandriaApiSvc;
ALTER ROLE db_ddladmin   ADD MEMBER CalandriaApiSvc;
GO

-- ----------------------------------------------------------------------------
-- CALANDRIA: la única BD que toca el cliente legacy. También la usa el API
-- (es una obra más, la original pre-multi-obra).
-- ----------------------------------------------------------------------------
USE CALANDRIA;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'CalandriaApiSvc')
    CREATE USER CalandriaApiSvc FOR LOGIN CalandriaApiSvc;
ALTER ROLE db_datareader ADD MEMBER CalandriaApiSvc;
ALTER ROLE db_datawriter ADD MEMBER CalandriaApiSvc;
ALTER ROLE db_ddladmin   ADD MEMBER CalandriaApiSvc;

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'CalandriaClienteLegacy')
    CREATE USER CalandriaClienteLegacy FOR LOGIN CalandriaClienteLegacy;
ALTER ROLE db_datareader ADD MEMBER CalandriaClienteLegacy;
ALTER ROLE db_datawriter ADD MEMBER CalandriaClienteLegacy;
ALTER ROLE db_ddladmin   ADD MEMBER CalandriaClienteLegacy;
GO

-- ----------------------------------------------------------------------------
-- Resto de BDs de obra ya existentes (multi-obra): solo CalandriaApiSvc.
-- Recorre CalandriaControl.dbo.Obras.NombreBD, salta CALANDRIA (ya hecha
-- arriba) y CalandriaControl (ya hecha arriba).
-- ----------------------------------------------------------------------------
DECLARE @nombreBd SYSNAME;
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT DISTINCT NombreBD
    FROM CalandriaControl.dbo.Obras
    WHERE NombreBD NOT IN ('CALANDRIA', 'CalandriaControl')
      AND DB_ID(NombreBD) IS NOT NULL;

OPEN cur;
FETCH NEXT FROM cur INTO @nombreBd;
WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC('USE [' + @nombreBd + '];
          IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = ''CalandriaApiSvc'')
              CREATE USER CalandriaApiSvc FOR LOGIN CalandriaApiSvc;
          ALTER ROLE db_datareader ADD MEMBER CalandriaApiSvc;
          ALTER ROLE db_datawriter ADD MEMBER CalandriaApiSvc;
          ALTER ROLE db_ddladmin   ADD MEMBER CalandriaApiSvc;');
    FETCH NEXT FROM cur INTO @nombreBd;
END
CLOSE cur;
DEALLOCATE cur;
GO

-- ============================================================================
-- Verificación
-- ============================================================================
SELECT name, type_desc, is_disabled FROM sys.server_principals
WHERE name IN ('CalandriaApiSvc', 'CalandriaClienteLegacy', 'sa');

-- Confirma que CalandriaClienteLegacy SOLO aparece en CALANDRIA:
-- (correr esto DESPUÉS del script, conectado a cada BD relevante)
-- SELECT DB_NAME(), name FROM sys.database_principals WHERE name = 'CalandriaClienteLegacy';

-- ============================================================================
-- Siguientes pasos (NO son parte de este script, hacer aparte y en orden):
--
-- 1. Actualizar Calandria.Api\connectionStrings.config:
--      CalandriaConn / CalandriaControlConn -> User Id=CalandriaApiSvc, tu password
--    Reiniciar el servicio CalandriaApi (reiniciar-servicio.bat).
--
-- 2. Actualizar DynamicSepticSystem\connectionStrings.config:
--      CalandriaConn -> User Id=CalandriaClienteLegacy, tu password
--    Repartir la nueva versión a las instalaciones existentes (o vía
--    Actualizador si el .config se empaqueta en el release).
--
-- 3. Probar A FONDO: login del API, pantallas web migradas, y las pantallas
--    WinForms legacy que siguen con SQL directo (Grupo A/B del plan de
--    migración) -- especialmente las que auto-crean tablas la primera vez
--    (ActivacionTareasRuta, TRABAJADORES, etc. en una BD de obra NUEVA).
--
-- 4. Solo cuando 1-3 estén verificados en producción durante unos días:
--    deshabilitar "sa" (no borrarlo todavía, por si hay que revertir rápido):
--      ALTER LOGIN sa DISABLE;
--    Revisar antes si algo más lo usa (SQL Server Agent jobs, otras apps).
-- ============================================================================
