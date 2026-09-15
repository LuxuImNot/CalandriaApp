@echo off
setlocal enabledelayedexpansion

REM ============================================================
REM  Corre Calandria.Api EN PRIMER PLANO (modo consola) para
REM  SUPERVISAR las transacciones en vivo. La ventana permanece
REM  abierta mostrando cada peticion/respuesta (y el cuerpo de
REM  las de escritura si LogDetallado=true).
REM
REM  Detiene el servicio (y ESPERA a que quede STOPPED para
REM  liberar el puerto), corre el exe en esta ventana, y al
REM  cerrarlo (ENTER) ofrece reiniciar el servicio.
REM  USO: doble clic en el servidor (NO ejecutes el .exe directo).
REM ============================================================

set "SERVICIO=CalandriaApi"
set "EXE=%~dp0Calandria.Api.exe"

REM --- Auto-elevacion a Administrador (HttpListener en + necesita admin) ---
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Solicitando privilegios de administrador...
    powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    exit /b
)

if not exist "%EXE%" (
    echo [ERROR] No se encontro "%EXE%".
    echo Copia este .bat a la carpeta donde esta Calandria.Api.exe.
    pause
    exit /b 1
)

echo Deteniendo el servicio para liberar el puerto %SERVICIO%...
sc stop %SERVICIO% >nul 2>&1

REM --- Esperar a que el servicio quede STOPPED (o no exista) hasta ~30s ---
set /a _intentos=0
:esperar_stop
REM Si el servicio no existe (1060), no hay nada que esperar.
sc query %SERVICIO% 2>nul | find "1060" >nul && goto puerto_libre
sc query %SERVICIO% 2>nul | find "STOPPED" >nul && goto puerto_libre
set /a _intentos+=1
if !_intentos! GEQ 30 (
    echo.
    echo [AVISO] El servicio no confirmo STOPPED tras 30s. El puerto puede seguir
    echo         ocupado y el exe fallara con "conflicts with an existing registration".
    echo         Detenlo a mano con detener-servicio.bat y vuelve a intentar.
    echo.
    pause
    goto fin
)
echo   ...esperando que el servicio se detenga (!_intentos!/30)
ping -n 2 127.0.0.1 >nul
goto esperar_stop

:puerto_libre
REM --- Matar instancias SUELTAS del exe (consolas previas que aun retienen el
REM     puerto en HTTP.SYS). El servicio ya esta STOPPED, asi que esto solo afecta
REM     procesos huerfanos, no al servicio. ---
tasklist /fi "imagename eq Calandria.Api.exe" 2>nul | find /i "Calandria.Api.exe" >nul
if %errorlevel%==0 (
    echo Cerrando instancias previas del API que aun retienen el puerto...
    taskkill /F /IM Calandria.Api.exe >nul 2>&1
    ping -n 3 127.0.0.1 >nul
)

echo Servicio detenido. Puerto %SERVICIO% libre.
echo.
echo ================================================================
echo   CALANDRIA API - MODO CONSOLA (supervision en vivo)
echo   Se imprime cada peticion ^>^> y respuesta ^<^< con su tiempo.
echo   Con LogDetallado=true tambien el cuerpo de POST/PUT y el user.
echo   Tambien se guarda en logs\api-YYYYMMDD.log
echo   --> Presiona ENTER en esta ventana para detener.
echo ================================================================
echo.

"%EXE%"

echo.
echo La consola del API se cerro.
choice /C SN /M "Quieres volver a iniciar el SERVICIO CalandriaApi ahora"
if errorlevel 2 goto fin
sc start %SERVICIO%
echo.
sc query %SERVICIO%
:fin
echo.
pause
endlocal
