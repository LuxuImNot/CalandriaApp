@echo off
setlocal

REM ============================================================
REM  Instala Calandria.Api como Servicio de Windows con arranque
REM  automatico en cada encendido del equipo.
REM
REM  USO: copia este .bat a la MISMA carpeta donde esta
REM       Calandria.Api.exe (en el servidor) y haz doble clic.
REM       Se vuelve a ejecutar sin problema para actualizar.
REM ============================================================

set "SERVICIO=CalandriaApi"
set "PUERTO=8733"
set "EXE=%~dp0Calandria.Api.exe"

REM --- Auto-elevacion a Administrador ---
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Solicitando privilegios de administrador...
    powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    exit /b
)

REM --- Verificar que el exe existe junto al .bat ---
if not exist "%EXE%" (
    echo [ERROR] No se encontro "%EXE%".
    echo Copia este .bat a la carpeta donde esta Calandria.Api.exe.
    pause
    exit /b 1
)

echo.
echo === Instalando servicio %SERVICIO% ===
echo Exe:    "%EXE%"
echo Puerto: %PUERTO%
echo.

REM --- Reserva de URL para la cuenta del servicio (LocalSystem) ---
echo [1/5] Reservando URL http://+:%PUERTO%/ ...
netsh http add urlacl url=http://+:%PUERTO%/ user="NT AUTHORITY\SYSTEM" >nul 2>&1
REM (si ya existe, se ignora el error)

REM --- Regla de firewall de entrada ---
echo [2/5] Abriendo el puerto %PUERTO% en el firewall...
netsh advfirewall firewall delete rule name="%SERVICIO% %PUERTO%" >nul 2>&1
netsh advfirewall firewall add rule name="%SERVICIO% %PUERTO%" dir=in action=allow protocol=TCP localport=%PUERTO% >nul

REM --- Si ya existe el servicio, detener y borrar para reinstalar ---
echo [3/5] Eliminando instalacion previa (si existe)...
sc stop %SERVICIO% >nul 2>&1
sc delete %SERVICIO% >nul 2>&1
REM pequena espera para que el SCM libere el nombre
ping -n 3 127.0.0.1 >nul

REM --- Crear el servicio con arranque automatico ---
echo [4/5] Creando el servicio (arranque automatico)...
sc create %SERVICIO% binPath= "%EXE%" start= auto DisplayName= "Calandria API"
sc description %SERVICIO% "Web API de Calandria (OWIN self-host). Escucha en el puerto %PUERTO%."
REM Reinicia el servicio si se cae: a los 5s, hasta 3 veces; contador se resetea cada dia.
sc failure %SERVICIO% reset= 86400 actions= restart/5000/restart/5000/restart/5000

REM --- Arrancar ---
echo [5/5] Iniciando el servicio...
sc start %SERVICIO%

echo.
echo === Estado actual ===
sc query %SERVICIO%

echo.
echo Listo. El servicio arrancara solo en cada encendido.
echo Prueba:  curl http://localhost:%PUERTO%/api/health
echo.
pause
endlocal
