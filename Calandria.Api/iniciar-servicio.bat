@echo off
setlocal

REM ============================================================
REM  INICIA el Servicio de Windows Calandria.Api.
REM
REM  USO: doble clic en el servidor. Requiere que el servicio
REM       ya este instalado (instalar-servicio.bat).
REM ============================================================

set "SERVICIO=CalandriaApi"
set "PUERTO=8733"

REM --- Auto-elevacion a Administrador ---
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Solicitando privilegios de administrador...
    powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    exit /b
)

echo.
echo === Iniciando servicio %SERVICIO% ===
sc start %SERVICIO%

echo.
echo === Estado actual ===
sc query %SERVICIO%

echo.
echo Listo. Prueba:  curl http://localhost:%PUERTO%/api/health
echo.
pause
endlocal
