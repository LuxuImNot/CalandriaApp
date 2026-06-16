@echo off
REM ==========================================
REM BUILD RELEASE - CALANDRIA v1.3.7-H
REM ==========================================
echo.
echo ==========================================
echo BUILD RELEASE - CALANDRIA v1.3.7-H
echo ==========================================
echo.

cd /d "%~dp0"

REM Limpiar builds anteriores
echo [1/5] Limpiando builds anteriores...
if exist "DynamicSepticSystem\bin\Release" rmdir /s /q "DynamicSepticSystem\bin\Release"
if exist "DynamicSepticSystem\obj\Release" rmdir /s /q "DynamicSepticSystem\obj\Release"
if exist "Updater\bin\Release" rmdir /s /q "Updater\bin\Release"
if exist "Updater\obj\Release" rmdir /s /q "Updater\obj\Release"
echo Limpieza completada.

REM Compilar DynamicSepticSystem en Release
echo.
echo [2/5] Compilando DynamicSepticSystem en Release...
msbuild "DynamicSepticSystem.sln" /p:Configuration=Release /p:Platform="Any CPU" /t:Rebuild /v:minimal
if %errorlevel% neq 0 (
    echo ERROR: Fallo al compilar DynamicSepticSystem
    pause
    exit /b 1
)
echo Compilacion de DynamicSepticSystem completada.

REM Compilar Updater ya está incluido en la solución
echo [3/5] Verificando Updater...
if not exist "Updater\bin\Release\Updater.exe" (
    echo ERROR: Updater.exe no se compiló
    pause
    exit /b 1
)
echo Updater.exe encontrado.

REM Actualizar version.txt
echo.
echo [4/5] Actualizando version.txt...
echo 1.3.7-H > "DynamicSepticSystem\bin\Release\version.txt"
echo version.txt actualizado.

REM Copiar Updater.exe a la carpeta Release de DynamicSepticSystem
echo.
echo [5/5] Copiando Updater.exe...
copy /Y "Updater\bin\Release\Updater.exe" "DynamicSepticSystem\bin\Release\Updater.exe"
if %errorlevel% neq 0 (
    echo ADVERTENCIA: No se pudo copiar Updater.exe
)
echo Copia completada.

echo.
echo ==========================================
echo BUILD COMPLETADO EXITOSAMENTE
echo ==========================================
echo.
echo Archivos generados en:
echo   DynamicSepticSystem\bin\Release\
echo.
echo Contenido:
dir /b "DynamicSepticSystem\bin\Release"
echo.
echo Siguiente paso:
echo   1. Verificar que version.txt contiene: 1.3.7-H
echo   2. Compilar Setup1 en Visual Studio (Release)
echo   3. O crear ZIP manualmente de la carpeta Release
echo.
pause
