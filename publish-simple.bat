@echo off
REM ========================================================================
REM Script de Publicación SIMPLIFICADO - DynamicSepticSystem
REM Usa Visual Studio Developer Command Prompt
REM ========================================================================
echo.
echo ========================================
echo  Publicacion Simple - DynamicSepticSystem
echo ========================================
echo.

REM Configuración
set PROJECT_NAME=DynamicSepticSystem
set SOLUTION_FILE=DynamicSepticSystem.sln
set PUBLISH_DIR=bin\Publish
set BUILD_CONFIG=Release

REM Obtener timestamp para versión
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set datetime=%%I
set VERSION=%datetime:~0,8%-%datetime:~8,6%
set ZIP_NAME=%PROJECT_NAME%_v%VERSION%.zip

echo Limpiando directorio anterior...
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
mkdir "%PUBLISH_DIR%"
echo.

echo ========================================
echo INSTRUCCIONES DE USO:
echo ========================================
echo.
echo Este script requiere Visual Studio Developer Command Prompt.
echo.
echo OPCION 1: Ejecutar desde Visual Studio Developer Command Prompt
echo   1. Abre "Developer Command Prompt for VS"
echo   2. Navega a: %CD%
echo   3. Ejecuta: publish-simple.bat
echo.
echo OPCION 2: Compilar desde Visual Studio
echo   1. Abre el proyecto en Visual Studio
echo   2. Menu: Build ^> Rebuild Solution
echo   3. Copia los archivos de: DynamicSepticSystem\bin\Release\
echo   4. Pegalos en: %PUBLISH_DIR%\
echo   5. Ejecuta: zip-only.bat
echo.
echo OPCION 3: Usar dotnet CLI
echo   dotnet build -c Release
echo   dotnet publish -c Release -o %PUBLISH_DIR%
echo.

choice /C 123 /M "Selecciona una opcion (1-3)"
set OPTION=%errorlevel%

if %OPTION% equ 1 goto :build_with_msbuild
if %OPTION% equ 2 goto :manual_copy
if %OPTION% equ 3 goto :build_with_dotnet

:build_with_msbuild
echo.
echo Compilando con MSBuild...
msbuild "%SOLUTION_FILE%" /p:Configuration=%BUILD_CONFIG% /p:Platform="Any CPU" /t:Rebuild /v:minimal
if %errorlevel% neq 0 (
    echo ERROR: MSBuild no esta disponible o fallo la compilacion.
    echo Intenta con otra opcion.
    pause
    exit /b 1
)

echo Copiando archivos...
xcopy /s /y "DynamicSepticSystem\bin\%BUILD_CONFIG%\*.*" "%PUBLISH_DIR%\"
goto :create_zip

:build_with_dotnet
echo.
echo Compilando con dotnet...
dotnet publish "DynamicSepticSystem\DynamicSepticSystem.csproj" -c %BUILD_CONFIG% -o "%PUBLISH_DIR%" --verbosity minimal
if %errorlevel% neq 0 (
    echo ERROR: dotnet CLI no esta disponible o fallo la compilacion.
    echo Intenta con otra opcion.
    pause
    exit /b 1
)
goto :create_zip

:manual_copy
echo.
echo Por favor, copia manualmente los archivos de:
echo   DynamicSepticSystem\bin\Release\
echo.
echo A la carpeta:
echo   %CD%\%PUBLISH_DIR%\
echo.
pause
if not exist "%PUBLISH_DIR%\*.exe" (
    echo ERROR: No se encontraron archivos .exe en %PUBLISH_DIR%
    echo Asegurate de haber copiado los archivos correctamente.
    pause
    exit /b 1
)
goto :create_zip

:create_zip
echo.
echo Creando archivo ZIP...
if exist "%ZIP_NAME%" del /f /q "%ZIP_NAME%"

powershell -NoProfile -Command "Compress-Archive -Path '%PUBLISH_DIR%\*' -DestinationPath '%ZIP_NAME%' -Force"

if %errorlevel% neq 0 (
    echo ERROR: No se pudo crear el ZIP.
    pause
    exit /b 1
)

echo.
echo ========================================
echo  PUBLICACION COMPLETADA
echo ========================================
echo.
echo Archivo creado: %ZIP_NAME%
echo Ubicacion: %CD%
echo.

explorer /select,"%CD%\%ZIP_NAME%"

echo.
pause
