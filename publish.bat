@echo off
REM ========================================================================
REM Script de Publicación y Empaquetado - DynamicSepticSystem
REM Prepara paquete de actualización para GitHub Releases
REM ========================================================================
echo.
echo ========================================
echo  Publicando DynamicSepticSystem
echo ========================================
echo.

REM Configuración
set PROJECT_NAME=DynamicSepticSystem
set SOLUTION_FILE=DynamicSepticSystem.sln
set PUBLISH_DIR=bin\Publish
set BUILD_CONFIG=Release
set GITHUB_OWNER=LuxuImNot
set GITHUB_REPO=CalandriaApp

REM ========================================================================
REM BUSCAR MSBUILD O DOTNET
REM ========================================================================
set MSBUILD_PATH=
set USE_DOTNET=0

REM Intentar encontrar MSBuild (Visual Studio 2022, 2019, 2017)
echo [0/6] Buscando herramientas de compilacion...
for %%v in (2022 2019 2017) do (
    if exist "C:\Program Files\Microsoft Visual Studio\%%v\Community\MSBuild\Current\Bin\MSBuild.exe" (
        set MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\%%v\Community\MSBuild\Current\Bin\MSBuild.exe
        echo Encontrado: Visual Studio %%v Community
        goto :found_msbuild
    )
    if exist "C:\Program Files\Microsoft Visual Studio\%%v\Professional\MSBuild\Current\Bin\MSBuild.exe" (
        set MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\%%v\Professional\MSBuild\Current\Bin\MSBuild.exe
        echo Encontrado: Visual Studio %%v Professional
        goto :found_msbuild
    )
    if exist "C:\Program Files\Microsoft Visual Studio\%%v\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
        set MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\%%v\Enterprise\MSBuild\Current\Bin\MSBuild.exe
        echo Encontrado: Visual Studio %%v Enterprise
        goto :found_msbuild
    )
)

REM Buscar en rutas alternativas
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe
    echo Encontrado: Visual Studio 2019 Community (x86)
    goto :found_msbuild
)

REM Si no se encuentra MSBuild, verificar si dotnet está disponible
:check_dotnet
dotnet --version >nul 2>&1
if %errorlevel% equ 0 (
    set USE_DOTNET=1
    echo Usando: dotnet CLI
    goto :found_tool
)

REM No se encontró ninguna herramienta
echo.
echo ========================================
echo  ERROR: NO SE ENCONTRARON HERRAMIENTAS
echo ========================================
echo.
echo No se pudo encontrar MSBuild ni dotnet CLI.
echo.
echo Soluciones posibles:
echo 1. Instalar Visual Studio 2017 o superior
echo 2. Instalar .NET SDK desde https://dotnet.microsoft.com/download
echo 3. Ejecutar este script desde "Developer Command Prompt for VS"
echo.
pause
exit /b 1

:found_msbuild
set TOOL_NAME=MSBuild
goto :found_tool

:found_tool
echo Herramienta de compilacion configurada: %TOOL_NAME%
echo.

REM Obtener versión y timestamp
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set datetime=%%I
set VERSION=%datetime:~0,8%-%datetime:~8,6%
set ZIP_NAME=%PROJECT_NAME%_v%VERSION%.zip

REM Limpiar directorio de publicación anterior
echo [1/6] Limpiando directorio de publicacion anterior...
if exist "%PUBLISH_DIR%" (
    rmdir /s /q "%PUBLISH_DIR%"
    echo Directorio anterior eliminado.
) else (
    echo No hay directorio anterior que limpiar.
)
echo.

REM Restaurar paquetes NuGet
echo [2/6] Restaurando paquetes NuGet...
if %USE_DOTNET% equ 1 (
    dotnet restore "%SOLUTION_FILE%" --verbosity minimal
) else (
    "%MSBUILD_PATH%" "%SOLUTION_FILE%" /t:Restore /v:m /nologo
)

if %errorlevel% neq 0 (
    echo ADVERTENCIA: No se pudieron restaurar algunos paquetes NuGet.
    echo Continuando con la compilacion...
)
echo Restauracion de paquetes completada.
echo.

REM Compilar el proyecto
echo [3/6] Compilando el proyecto en modo %BUILD_CONFIG%...
if %USE_DOTNET% equ 1 (
    dotnet build "%SOLUTION_FILE%" -c %BUILD_CONFIG% --verbosity minimal --no-restore
) else (
    "%MSBUILD_PATH%" "%SOLUTION_FILE%" /p:Configuration=%BUILD_CONFIG% /p:Platform="Any CPU" /t:Rebuild /v:m /nologo
)

if %errorlevel% neq 0 (
    echo.
    echo ========================================
    echo  ERROR: FALLO LA COMPILACION
    echo ========================================
    echo.
    echo Verifica los errores mostrados arriba.
    echo.
    pause
    exit /b %errorlevel%
)
echo Compilacion exitosa.
echo.

REM Copiar archivos compilados al directorio de publicación
echo [4/6] Copiando archivos compilados...
mkdir "%PUBLISH_DIR%" 2>nul

REM Copiar ejecutable principal y dependencias
echo Copiando binarios...
xcopy /s /y /i "%PROJECT_NAME%\bin\%BUILD_CONFIG%\*.*" "%PUBLISH_DIR%\" >nul

if %errorlevel% neq 0 (
    echo.
    echo ========================================
    echo  ERROR: FALLO AL COPIAR ARCHIVOS
    echo ========================================
    echo.
    pause
    exit /b %errorlevel%
)

echo Archivos binarios copiados.
echo.

REM Copiar carpeta Resources si existe
echo [5/6] Copiando recursos adicionales...
if exist "%PROJECT_NAME%\Resources\" (
    echo Copiando carpeta Resources...
    xcopy /s /y /i "%PROJECT_NAME%\Resources\*" "%PUBLISH_DIR%\Resources\" >nul
    if errorlevel 1 (
        echo ADVERTENCIA: No se pudo copiar la carpeta Resources completamente.
    ) else (
        echo Carpeta Resources copiada exitosamente.
    )
) else (
    echo No se encontro carpeta Resources - se usaran recursos embebidos
)

REM Copiar archivos de configuración adicionales
if exist "%PROJECT_NAME%\App.config" (
    copy "%PROJECT_NAME%\App.config" "%PUBLISH_DIR%\%PROJECT_NAME%.exe.config" /Y >nul 2>&1
    echo App.config copiado.
)

REM Copiar archivo de coordenadas del mapa
if exist "%PROJECT_NAME%\coordenadas_mapa.json" (
    copy "%PROJECT_NAME%\coordenadas_mapa.json" "%PUBLISH_DIR%\coordenadas_mapa.json" /Y >nul 2>&1
    echo coordenadas_mapa.json copiado.
) else (
    REM Si no existe en la carpeta del proyecto, buscar en bin\Release
    if exist "%PROJECT_NAME%\bin\%BUILD_CONFIG%\coordenadas_mapa.json" (
        copy "%PROJECT_NAME%\bin\%BUILD_CONFIG%\coordenadas_mapa.json" "%PUBLISH_DIR%\coordenadas_mapa.json" /Y >nul 2>&1
        echo coordenadas_mapa.json copiado desde bin\%BUILD_CONFIG%.
    ) else (
        echo ADVERTENCIA: No se encontro coordenadas_mapa.json
    )
)

REM Crear archivo version.txt
echo %VERSION% > "%PUBLISH_DIR%\version.txt"
echo Archivo version.txt creado.

REM Copiar Updater.exe si existe
if exist "Updater\bin\%BUILD_CONFIG%\net472\Updater.exe" (
    copy "Updater\bin\%BUILD_CONFIG%\net472\Updater.exe" "%PUBLISH_DIR%\Updater.exe" /Y >nul 2>&1
    echo Updater.exe copiado.
)

echo.

REM Comprimir a ZIP
echo [6/6] Comprimiendo archivos a ZIP...
echo Creando: %ZIP_NAME%

REM Eliminar ZIP anterior si existe
if exist "%ZIP_NAME%" del /f /q "%ZIP_NAME%"

REM Usar PowerShell para crear el ZIP
powershell -NoProfile -Command "Compress-Archive -Path '%PUBLISH_DIR%\*' -DestinationPath '%ZIP_NAME%' -Force"

if %errorlevel% neq 0 (
    echo.
    echo ========================================
    echo  ERROR: FALLO AL CREAR ZIP
    echo ========================================
    echo.
    pause
    exit /b %errorlevel%
)

echo ZIP creado exitosamente.
echo.

echo ========================================
echo  PAQUETE COMPLETADO EXITOSAMENTE
echo ========================================
echo.
echo Ubicacion publicacion: %CD%\%PUBLISH_DIR%
echo Archivo ZIP: %CD%\%ZIP_NAME%
echo.
echo Contenido del paquete:
echo ----------------------
dir /b "%PUBLISH_DIR%\*.exe" 2>nul
if exist "%PUBLISH_DIR%\Resources\" (
    echo + Carpeta Resources
)
dir /b "%PUBLISH_DIR%\*.dll" 2>nul | find /c ".dll" > nul && echo + Bibliotecas (.dll)
dir /b "%PUBLISH_DIR%\*.config" 2>nul | find /c ".config" > nul && echo + Configuracion (.config)
if exist "%PUBLISH_DIR%\version.txt" (
    echo + version.txt
)
if exist "%PUBLISH_DIR%\coordenadas_mapa.json" (
    echo + coordenadas_mapa.json
)
echo.

REM Información del paquete
echo ----------------------------------------
echo INFORMACION DEL PAQUETE:
echo ----------------------------------------
echo Nombre: %ZIP_NAME%
for %%A in ("%ZIP_NAME%") do set /a SIZE_MB=%%~zA/1024/1024
echo Tamaño: %SIZE_MB% MB aproximadamente
echo GitHub Repo: %GITHUB_OWNER%/%GITHUB_REPO%
echo.

REM Abrir GitHub Releases
echo ----------------------------------------
echo SIGUIENTE PASO: Crear Release en GitHub
echo ----------------------------------------
echo.
echo 1. Sube el archivo: %ZIP_NAME%
echo 2. Crea un nuevo tag (ej: v1.7.%VERSION%)
echo 3. Describe los cambios en el release
echo 4. IMPORTANTE: Sube tambien version.txt como asset adicional
echo.

choice /C SN /M "Desea abrir GitHub Releases ahora"
if %errorlevel% equ 1 (
    echo Abriendo GitHub Releases...
    start https://github.com/%GITHUB_OWNER%/%GITHUB_REPO%/releases/new
    timeout /t 2 /nobreak > nul
)
echo.

REM Opción para abrir la carpeta con el ZIP
choice /C SN /M "Desea abrir la carpeta con el archivo ZIP"
if %errorlevel% equ 1 (
    explorer /select,"%CD%\%ZIP_NAME%"
)

echo.
echo ========================================
echo  PUBLICACION COMPLETADA
echo ========================================
echo Presione cualquier tecla para salir...
pause > nul
