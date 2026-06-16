# Script Rápido para Crear Setup.exe usando IExpress (nativo de Windows)
# Este método crea un ejecutable autoextraíble sin software adicional

param(
    [string]$Version = "1.3.7-H"
)

Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "  CREADOR DE SETUP.EXE - CALANDRIA v$Version" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""

# Verificar ZIP
$zipPath = ".\CalandriaApp-v$Version.zip"
if (-not (Test-Path $zipPath)) {
    Write-Host "? ERROR: No se encuentra $zipPath" -ForegroundColor Red
    Write-Host "   Ejecuta primero: .\BuildRelease.ps1" -ForegroundColor Yellow
    Read-Host "Presiona Enter para salir"
    exit 1
}

Write-Host "? ZIP encontrado: $zipPath" -ForegroundColor Green

# Crear carpeta temporal para IExpress
$tempDir = ".\SetupTemp"
if (Test-Path $tempDir) {
    Remove-Item $tempDir -Recurse -Force
}
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

Write-Host "?? Creando archivos temporales..." -ForegroundColor Cyan

# Copiar ZIP
Copy-Item $zipPath "$tempDir\CalandriaApp.zip" -Force

# Crear script de instalación embebido
$installBatch = @"
@echo off
title Instalador de Calandria Residencial v$Version

echo.
echo =========================================================
echo   INSTALADOR DE CALANDRIA RESIDENCIAL v$Version
echo =========================================================
echo.

REM Verificar privilegios de administrador
net session >nul 2>&1
if %errorLevel% NEQ 0 (
    echo [ADVERTENCIA] Este instalador requiere permisos de Administrador
    echo.
    echo Solicitando permisos...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit
)

echo [OK] Ejecutando como Administrador
echo.

REM Solicitar directorio de instalacion
set "INSTALL_DIR=%ProgramFiles%\CalandriaResidencial"
echo Directorio de instalacion: %INSTALL_DIR%
echo.
set /p "CONFIRM=Deseas continuar con la instalacion? (S/N): "
if /i not "%CONFIRM%"=="S" (
    echo Instalacion cancelada.
    pause
    exit
)

echo.
echo [1/4] Creando directorio de instalacion...
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
echo       OK

echo [2/4] Extrayendo archivos...
powershell -Command "Expand-Archive -Path '%~dp0CalandriaApp.zip' -DestinationPath '%INSTALL_DIR%' -Force"
if %errorLevel% NEQ 0 (
    echo       ERROR: No se pudo extraer el archivo
    pause
    exit /b 1
)
echo       OK

echo [3/4] Creando accesos directos...

REM Acceso directo en el Escritorio
powershell -Command "$WshShell = New-Object -ComObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%USERPROFILE%\Desktop\Calandria Residencial.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\DynamicSepticSystem.exe'; $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; $Shortcut.Description = 'Sistema de Gestion Calandria Residencial'; $Shortcut.Save()"
echo       Escritorio: OK

REM Acceso directo en Menu Inicio
powershell -Command "$WshShell = New-Object -ComObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%ProgramData%\Microsoft\Windows\Start Menu\Programs\Calandria Residencial.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\DynamicSepticSystem.exe'; $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; $Shortcut.Description = 'Sistema de Gestion Calandria Residencial'; $Shortcut.Save()"
echo       Menu Inicio: OK

echo [4/4] Registrando aplicacion...
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\CalandriaResidencial" /v "DisplayName" /t REG_SZ /d "Calandria Residencial" /f >nul
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\CalandriaResidencial" /v "DisplayVersion" /t REG_SZ /d "$Version" /f >nul
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\CalandriaResidencial" /v "Publisher" /t REG_SZ /d "Calandria Residencial" /f >nul
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\CalandriaResidencial" /v "InstallLocation" /t REG_SZ /d "%INSTALL_DIR%" /f >nul
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\CalandriaResidencial" /v "DisplayIcon" /t REG_SZ /d "%INSTALL_DIR%\DynamicSepticSystem.exe" /f >nul
echo       OK

echo.
echo =========================================================
echo   INSTALACION COMPLETADA EXITOSAMENTE
echo =========================================================
echo.
echo Ubicacion: %INSTALL_DIR%
echo.
echo Puedes ejecutar Calandria desde:
echo   - Acceso directo en el Escritorio
echo   - Menu Inicio
echo.

set /p "LAUNCH=Deseas ejecutar Calandria ahora? (S/N): "
if /i "%LAUNCH%"=="S" (
    start "" "%INSTALL_DIR%\DynamicSepticSystem.exe"
)

echo.
echo Presiona cualquier tecla para salir...
pause >nul
"@

$installBatch | Out-File -FilePath "$tempDir\install.bat" -Encoding ASCII -Force

Write-Host "   ? Scripts creados" -ForegroundColor Green

# Crear archivo SED para IExpress
Write-Host "?? Generando configuración de IExpress..." -ForegroundColor Cyan

$sedContent = @"
[Version]
Class=IEXPRESS
SEDVersion=3
[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=0
HideExtractAnimation=0
UseLongFileName=1
InsideCompressed=0
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=N
InstallPrompt=%InstallPrompt%
DisplayLicense=%DisplayLicense%
FinishMessage=%FinishMessage%
TargetName=%TargetName%
FriendlyName=%FriendlyName%
AppLaunched=%AppLaunched%
PostInstallCmd=%PostInstallCmd%
AdminQuietInstCmd=%AdminQuietInstCmd%
UserQuietInstCmd=%UserQuietInstCmd%
SourceFiles=SourceFiles
[Strings]
InstallPrompt=
DisplayLicense=
FinishMessage=Instalacion de Calandria Residencial completada. Ejecuta la aplicacion desde el Escritorio o Menu Inicio.
TargetName=$PWD\CalandriaSetup-v$Version.exe
FriendlyName=Calandria Residencial v$Version - Instalador
AppLaunched=cmd.exe /c install.bat
PostInstallCmd=<None>
AdminQuietInstCmd=
UserQuietInstCmd=
FILE0="install.bat"
FILE1="CalandriaApp.zip"
[SourceFiles]
SourceFiles0=$tempDir\
[SourceFiles0]
%FILE0%=
%FILE1%=
"@

$sedPath = "$tempDir\setup.sed"
$sedContent | Out-File -FilePath $sedPath -Encoding ASCII -Force

Write-Host "   ? Configuración generada" -ForegroundColor Green

# Ejecutar IExpress
Write-Host ""
Write-Host "?? Compilando Setup.exe..." -ForegroundColor Cyan
Write-Host "   (Esto puede tomar 1-2 minutos)" -ForegroundColor Gray
Write-Host ""

try {
    $iexpressPath = "$env:SystemRoot\System32\iexpress.exe"
    
    if (-not (Test-Path $iexpressPath)) {
        throw "IExpress no encontrado en el sistema"
    }
    
    $process = Start-Process $iexpressPath -ArgumentList "/N `"$sedPath`"" -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -ne 0) {
        throw "IExpress finalizó con código de error: $($process.ExitCode)"
    }
    
    $setupExePath = ".\CalandriaSetup-v$Version.exe"
    
    if (Test-Path $setupExePath) {
        $exeSize = (Get-Item $setupExePath).Length / 1MB
        
        Write-Host ""
        Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
        Write-Host "  ? SETUP.EXE CREADO EXITOSAMENTE" -ForegroundColor Green
        Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
        Write-Host ""
        Write-Host "?? Archivo generado:" -ForegroundColor Cyan
        Write-Host "   Nombre: CalandriaSetup-v$Version.exe" -ForegroundColor White
        Write-Host "   Ubicación: $setupExePath" -ForegroundColor White
        Write-Host "   Tamaño: $($exeSize.ToString('F2')) MB" -ForegroundColor White
        Write-Host ""
        Write-Host "?? Características del instalador:" -ForegroundColor Cyan
        Write-Host "   ? Ejecutable autoextraíble (.exe)" -ForegroundColor Green
        Write-Host "   ? Solicita permisos de administrador automáticamente" -ForegroundColor Green
        Write-Host "   ? Instala en C:\Program Files\CalandriaResidencial" -ForegroundColor Green
        Write-Host "   ? Crea accesos directos (Escritorio + Menú Inicio)" -ForegroundColor Green
        Write-Host "   ? Registra en Panel de Control > Programas" -ForegroundColor Green
        Write-Host "   ? Interfaz gráfica de Windows" -ForegroundColor Green
        Write-Host ""
        Write-Host "?? Distribución:" -ForegroundColor Cyan
        Write-Host "   Este archivo .exe puede distribuirse directamente" -ForegroundColor White
        Write-Host "   Los usuarios solo ejecutan el .exe y siguen el asistente" -ForegroundColor White
        Write-Host ""
        Write-Host "?? Para GitHub Release:" -ForegroundColor Cyan
        Write-Host "   Sube este archivo como asset adicional (opcional)" -ForegroundColor White
        Write-Host "   Usuarios que prefieran instalador tradicional lo usarán" -ForegroundColor White
        Write-Host ""
        
        # Abrir ubicación del archivo
        $openFolder = Read-Host "¿Deseas abrir la carpeta del archivo? (S/N)"
        if ($openFolder -match '^[Ss]$') {
            Start-Process explorer.exe "/select,`"$setupExePath`""
        }
        
        # Probar instalador
        Write-Host ""
        $testInstaller = Read-Host "¿Deseas probar el instalador ahora? (S/N)"
        if ($testInstaller -match '^[Ss]$') {
            Write-Host ""
            Write-Host "??  Esto instalará Calandria en tu sistema" -ForegroundColor Yellow
            Write-Host "   Puedes desinstalarlo después desde Panel de Control" -ForegroundColor Gray
            Write-Host ""
            Start-Process $setupExePath
        }
        
    } else {
        throw "El archivo Setup.exe no fue creado"
    }
    
} catch {
    Write-Host ""
    Write-Host "? ERROR al crear Setup.exe:" -ForegroundColor Red
    Write-Host "   $($_.Exception.Message)" -ForegroundColor White
    Write-Host ""
    Write-Host "?? Alternativas:" -ForegroundColor Yellow
    Write-Host "   1. Distribuye el ZIP: CalandriaApp-v$Version.zip" -ForegroundColor White
    Write-Host "   2. Usa el instalador PowerShell: .\CrearInstalador.ps1" -ForegroundColor White
    Write-Host "   3. Crea un proyecto MSI en Visual Studio" -ForegroundColor White
    Write-Host ""
} finally {
    # Limpiar archivos temporales
    Write-Host ""
    Write-Host "?? Limpiando archivos temporales..." -ForegroundColor Cyan
    try {
        if (Test-Path $tempDir) {
            Remove-Item $tempDir -Recurse -Force
            Write-Host "   ? Limpieza completada" -ForegroundColor Green
        }
    } catch {
        Write-Host "   ??  Algunos archivos temporales no pudieron eliminarse" -ForegroundColor Yellow
        Write-Host "      Elimina manualmente: $tempDir" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "Presiona Enter para finalizar..." -ForegroundColor Gray
Read-Host
