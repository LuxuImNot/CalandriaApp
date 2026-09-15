@echo off
setlocal
REM ============================================================
REM  Centralizacion de insumos - ejecuta las 3 fases EN ORDEN.
REM    01 Diagnostico (solo lectura)
REM    02 Backfill   (Insumos*EXP DOMINA; absorbe indirectos)
REM    03 Vistas     (COMPRAS* -> vistas sobre Insumos*EXP)
REM
REM  Catalogo canonico: Insumos*EXP (el de Destajos).
REM  Requiere 'sqlcmd' (SQL Server Command Line Utilities) en el PATH.
REM
REM  Uso:
REM    Centralizacion_RUN.bat                 (usa los valores de abajo)
REM    Centralizacion_RUN.bat SERVIDOR BD USUARIO CONTRASENA
REM ============================================================

REM ---- Conexion (tomada de App.config; editar si cambia el entorno) ----
set "SERVER=100.75.234.9"
set "DB=CALANDRIA"
set "DBUSER=sa"
set "DBPASS=<CONTRASENA-PURGADA>"

REM ---- Override opcional por argumentos ----
if not "%~1"=="" set "SERVER=%~1"
if not "%~2"=="" set "DB=%~2"
if not "%~3"=="" set "DBUSER=%~3"
if not "%~4"=="" set "DBPASS=%~4"

set "SCRIPTDIR=%~dp0"
set "LOG=%SCRIPTDIR%Centralizacion_RUN.log"

REM ---- Verificar que sqlcmd exista ----
where sqlcmd >nul 2>&1
if errorlevel 1 (
  echo ERROR: no se encontro 'sqlcmd' en el PATH.
  echo Instala "SQL Server Command Line Utilities" o ejecuta los .sql desde SSMS.
  exit /b 1
)

echo ============================================================ > "%LOG%"
echo  Centralizacion de insumos  %DATE% %TIME%                   >> "%LOG%"
echo  Servidor=%SERVER%  BD=%DB%  Usuario=%DBUSER%               >> "%LOG%"
echo ============================================================ >> "%LOG%"

echo.
echo Servidor=%SERVER%  BD=%DB%  Usuario=%DBUSER%
echo Log: %LOG%
echo.

call :run "Centralizacion_01_Diagnostico_Insumos.sql"        "FASE 0 - Diagnostico (solo lectura)"
if errorlevel 1 goto :error
call :run "Centralizacion_02_Backfill_CatalogoInsumos.sql"   "FASE 1 - Backfill del catalogo canonico"
if errorlevel 1 goto :error
call :run "Centralizacion_03_Vistas_Compatibilidad_COMPRAS.sql" "FASE 2 - Vistas de compatibilidad"
if errorlevel 1 goto :error

echo.
echo === TODO OK. Revisa el detalle en: %LOG% ===
echo Siguiente paso: redeploy del servicio CalandriaApi (endpoints de indirectos).
endlocal
exit /b 0

:run
echo --- %~2
echo.                                                          >> "%LOG%"
echo ===== %~2 ^(%~1^) =====                                   >> "%LOG%"
sqlcmd -S "%SERVER%" -d "%DB%" -U "%DBUSER%" -P "%DBPASS%" -b -f 65001 -i "%SCRIPTDIR%%~1" >> "%LOG%" 2>&1
exit /b %errorlevel%

:error
echo.
echo *** ERROR durante la ejecucion. Revisa el log: %LOG%
echo *** Las fases posteriores NO se ejecutaron. Corrige y vuelve a correr.
endlocal
exit /b 1
