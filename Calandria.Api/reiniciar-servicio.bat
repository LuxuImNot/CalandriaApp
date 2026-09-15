@echo off
setlocal

REM ============================================================
REM  REINICIA el Servicio de Windows Calandria.Api (stop + start).
REM
REM  USO: doble clic en el servidor. Util tras copiar una
REM       version nueva del exe a C:\CalandriaApi\.
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
echo === Reiniciando servicio %SERVICIO% ===
echo [1/2] Deteniendo...
sc stop %SERVICIO%
REM espera a que el SCM libere el proceso
ping -n 3 127.0.0.1 >nul

echo [2/2] Iniciando...
sc start %SERVICIO%

echo.
echo === Estado actual ===
sc query %SERVICIO%

echo.
echo Listo. Prueba:  curl http://localhost:%PUERTO%/api/health
echo.
pause
endlocal
