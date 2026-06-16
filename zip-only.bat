@echo off
REM ========================================================================
REM Script SOLO para empaquetar archivos ya compilados
REM Usar cuando ya compilaste desde Visual Studio
REM ========================================================================
echo.
echo ========================================
echo  Empaquetado Rapido - DynamicSepticSystem
echo ========================================
echo.

REM Configuración
set PROJECT_NAME=DynamicSepticSystem
set PUBLISH_DIR=bin\Publish
set BUILD_CONFIG=Release
set SOURCE_DIR=%PROJECT_NAME%\bin\%BUILD_CONFIG%

REM Obtener timestamp para versión
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set datetime=%%I
set VERSION=%datetime:~0,8%-%datetime:~8,6%
set ZIP_NAME=%PROJECT_NAME%_v%VERSION%.zip

REM Verificar que existan archivos compilados
if not exist "%SOURCE_DIR%\%PROJECT_NAME%.exe" (
    echo.
    echo ========================================
    echo  ERROR: NO SE ENCONTRARON ARCHIVOS
    echo ========================================
    echo.
    echo No se encontro el ejecutable en: %SOURCE_DIR%
    echo.
    echo SOLUCION:
    echo 1. Abre el proyecto en Visual Studio
    echo 2. Selecciona la configuracion: Release
    echo 3. Menu: Build ^> Rebuild Solution
    echo 4. Vuelve a ejecutar este script
    echo.
    pause
    exit /b 1
)

echo [1/3] Archivos encontrados en: %SOURCE_DIR%
echo.

REM Limpiar y crear directorio de publicación
echo [2/3] Preparando directorio de publicacion...
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
mkdir "%PUBLISH_DIR%"

REM Copiar archivos
echo Copiando archivos...
xcopy /s /y /i "%SOURCE_DIR%\*.*" "%PUBLISH_DIR%\" >nul

if %errorlevel% neq 0 (
    echo ERROR: No se pudieron copiar los archivos.
    pause
    exit /b 1
)

echo Archivos copiados exitosamente.
echo.

REM Crear ZIP
echo [3/3] Creando archivo ZIP...
if exist "%ZIP_NAME%" del /f /q "%ZIP_NAME%"

powershell -NoProfile -Command "Compress-Archive -Path '%PUBLISH_DIR%\*' -DestinationPath '%ZIP_NAME%' -Force"

if %errorlevel% neq 0 (
    echo ERROR: No se pudo crear el ZIP.
    pause
    exit /b 1
)

echo.
echo ========================================
echo  EMPAQUETADO COMPLETADO
echo ========================================
echo.
echo Archivo creado: %ZIP_NAME%
for %%A in ("%ZIP_NAME%") do echo Tamaño: %%~zA bytes
echo.

REM Mostrar contenido
echo Contenido del paquete:
dir /b "%PUBLISH_DIR%\*.exe" 2>nul
echo.

explorer /select,"%CD%\%ZIP_NAME%"

echo.
pause
