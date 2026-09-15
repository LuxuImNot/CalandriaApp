@echo off
setlocal

REM ============================================================
REM  DETIENE el Servicio de Windows Calandria.Api.
REM
REM  USO: doble clic en el servidor (donde corre el servicio).
REM       No lo desinstala: queda detenido y se puede iniciar
REM       de nuevo con iniciar-servicio.bat o sc start.
REM ============================================================

set "SERVICIO=CalandriaApi"

REM --- Auto-elevacion a Administrador ---
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Solicitando privilegios de administrador...
    powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    exit /b
)

echo.
echo === Deteniendo servicio %SERVICIO% ===
sc stop %SERVICIO%

echo.
echo === Estado actual ===
sc query %SERVICIO%

echo.
echo Listo. El servicio quedo DETENIDO.
echo Para volver a iniciarlo: iniciar-servicio.bat
echo.
pause
endlocal
