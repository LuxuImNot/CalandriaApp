@echo off
REM ============================================
REM PREPARAR RELEASE COMPLETA v1.3.7-H
REM ============================================
echo.
echo ============================================
echo PREPARAR RELEASE v1.3.7-H
echo ============================================
echo.

cd /d "%~dp0"

echo Este script hara lo siguiente:
echo   1. Compilar DynamicSepticSystem en Release
echo   2. Compilar Updater en Release
echo   3. Crear ZIP de distribucion
echo   4. Copiar archivos necesarios para GitHub Release
echo.
pause

REM Ejecutar build
echo.
echo ========================================
echo PASO 1: COMPILANDO...
echo ========================================
call BuildRelease.bat
if %errorlevel% neq 0 goto error

REM Crear carpeta de release
echo.
echo ========================================
echo PASO 2: PREPARANDO ARCHIVOS
echo ========================================
set RELEASE_FOLDER=Release_v1.3.7-H
if exist "%RELEASE_FOLDER%" rmdir /s /q "%RELEASE_FOLDER%"
mkdir "%RELEASE_FOLDER%"
echo Carpeta creada: %RELEASE_FOLDER%

REM Copiar ZIP
echo.
echo Copiando ZIP de distribucion...
copy /Y "CalandriaApp-v1.3.7-H.zip" "%RELEASE_FOLDER%\CalandriaApp-v1.3.7-H.zip"

REM Copiar version.txt
echo Copiando version.txt...
echo 1.3.7-H > "%RELEASE_FOLDER%\version.txt"

REM Copiar documentacion
echo Copiando documentacion...
copy /Y "RELEASE_NOTES.txt" "%RELEASE_FOLDER%\RELEASE_NOTES.txt"
copy /Y "GITHUB_RELEASE_GUIDE.md" "%RELEASE_FOLDER%\GITHUB_RELEASE_GUIDE.md"
copy /Y "DynamicSepticSystem\README_SistemaProveedores.md" "%RELEASE_FOLDER%\README_SistemaProveedores.md"
copy /Y "DynamicSepticSystem\README_SistemaVersionado.md" "%RELEASE_FOLDER%\README_SistemaVersionado.md"
copy /Y "DynamicSepticSystem\README_OrdenesIndirectas.md" "%RELEASE_FOLDER%\README_OrdenesIndirectas.md"

REM Copiar scripts SQL
echo Copiando scripts SQL...
copy /Y "DynamicSepticSystem\SQL_VerificarProveedores.sql" "%RELEASE_FOLDER%\SQL_VerificarProveedores.sql"
copy /Y "DynamicSepticSystem\SQL_VerificarComprasIndirectas.sql" "%RELEASE_FOLDER%\SQL_VerificarComprasIndirectas.sql"

REM Copiar instalador si existe
if exist "Setup1\Release\Setup1.msi" (
    echo Copiando instalador MSI...
    copy /Y "Setup1\Release\Setup1.msi" "%RELEASE_FOLDER%\CalandriaSetup-v1.3.7-H.msi"
) else (
    echo NOTA: No se encontro Setup1.msi
    echo       Compila Setup1 en modo Release si quieres incluirlo
)

REM Copiar script de instalacion
echo Copiando script de instalacion...
copy /Y "Install.ps1" "%RELEASE_FOLDER%\Install.ps1"

echo.
echo ========================================
echo PASO 3: GENERANDO CHECKSUMS
echo ========================================
echo Generando checksums MD5...
cd "%RELEASE_FOLDER%"
certutil -hashfile "CalandriaApp-v1.3.7-H.zip" MD5 > checksums.txt
if exist "CalandriaSetup-v1.3.7-H.msi" (
    certutil -hashfile "CalandriaSetup-v1.3.7-H.msi" MD5 >> checksums.txt
)
cd ..
echo Checksums generados

echo.
echo ========================================
echo RELEASE PREPARADA EXITOSAMENTE
echo ========================================
echo.
echo Archivos generados en: %RELEASE_FOLDER%\
echo.
dir /b "%RELEASE_FOLDER%"
echo.
echo ========================================
echo SIGUIENTE PASO: SUBIR A GITHUB
echo ========================================
echo.
echo 1. Ve a: https://github.com/LuxuImNot/CalandriaApp/releases/new
echo 2. Tag: 1.3.7-H
echo 3. Title: v1.3.7-H - Sistema de Proveedores y Fix de Actualizaciones
echo 4. Arrastra TODOS los archivos de %RELEASE_FOLDER%\
echo 5. Copia la descripcion de GITHUB_RELEASE_GUIDE.md
echo 6. Marca como "Latest release"
echo 7. Publish!
echo.
echo Consulta GITHUB_RELEASE_GUIDE.md para mas detalles
echo.
set /p OPEN="Abrir carpeta de release? (S/N): "
if /i "%OPEN%"=="S" explorer "%RELEASE_FOLDER%"
echo.
pause
goto end

:error
echo.
echo ============================================
echo ERROR EN EL PROCESO
echo ============================================
echo Revisa los errores anteriores
pause

:end
