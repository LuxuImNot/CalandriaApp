@echo off
setlocal enabledelayedexpansion
title Instalador Calandria Residencial v1.3.7-H

cls
echo.
echo ============================================================
echo   INSTALADOR DE CALANDRIA RESIDENCIAL v1.3.7-H
echo ============================================================
echo.
echo Este instalador copiara los archivos necesarios y creara
echo accesos directos para facilitar el uso de la aplicacion.
echo.

REM Verificar si se ejecuta como administrador
net session >nul 2>&1
if %errorLevel% == 0 (
    echo [OK] Ejecutando con privilegios de Administrador
) else (
    echo [ADVERTENCIA] No se esta ejecutando como Administrador
    echo.
    echo Para una instalacion completa se requieren permisos de administrador.
    echo.
    choice /C SN /M "Deseas continuar de todos modos"
    if errorlevel 2 exit /b
)

echo.
echo ============================================================
echo   CONFIGURACION DE INSTALACION
echo ============================================================
echo.

REM Solicitar directorio de instalacion
set "DEFAULT_DIR=%ProgramFiles%\CalandriaResidencial"
echo Directorio predeterminado: %DEFAULT_DIR%
echo.
set /p "INSTALL_DIR=Directorio de instalacion (Enter para usar predeterminado): "

if "%INSTALL_DIR%"=="" set "INSTALL_DIR=%DEFAULT_DIR%"

echo.
echo Se instalara en: %INSTALL_DIR%
echo.
choice /C SN /M "Deseas continuar con la instalacion"
if errorlevel 2 (
    echo Instalacion cancelada.
    pause
    exit /b
)

echo.
echo ============================================================
echo   INSTALANDO...
echo ============================================================
echo.

REM Paso 1: Crear directorio
echo [1/5] Creando directorio de instalacion...
if not exist "%INSTALL_DIR%" (
    mkdir "%INSTALL_DIR%" 2>nul
    if errorlevel 1 (
        echo       ERROR: No se pudo crear el directorio
        echo       Intenta ejecutar este instalador como Administrador
        pause
        exit /b 1
    )
)
echo       OK

REM Paso 2: Extraer archivos - MEJORADO con manejo de errores
echo [2/5] Extrayendo archivos...
echo       Ubicacion ZIP: %~dp0CalandriaApp.zip
echo       Destino: %INSTALL_DIR%

REM Verificar que el ZIP existe
if not exist "%~dp0CalandriaApp.zip" (
    echo       ERROR: No se encuentra CalandriaApp.zip
    echo       Verifica que el archivo este en la misma carpeta que este instalador
    pause
    exit /b 1
)

REM Método 1: PowerShell con Expand-Archive (Windows 10+)
echo       Intentando metodo 1 (PowerShell Expand-Archive)...
powershell -NoProfile -ExecutionPolicy Bypass -Command "& { try { Expand-Archive -Path '%~dp0CalandriaApp.zip' -DestinationPath '%INSTALL_DIR%' -Force -ErrorAction Stop; Write-Host '      Exito'; exit 0 } catch { Write-Host '      Fallo: ' $_.Exception.Message; exit 1 } }"

if %errorlevel% == 0 (
    echo       OK - Archivos extraidos con Expand-Archive
    goto :ExtraccionExitosa
)

REM Método 2: PowerShell con System.IO.Compression (Windows 7+)
echo       Intentando metodo 2 (System.IO.Compression)...
powershell -NoProfile -ExecutionPolicy Bypass -Command "& { try { Add-Type -AssemblyName System.IO.Compression.FileSystem -ErrorAction Stop; [System.IO.Compression.ZipFile]::ExtractToDirectory('%~dp0CalandriaApp.zip', '%INSTALL_DIR%', $true); Write-Host '      Exito'; exit 0 } catch { Write-Host '      Fallo: ' $_.Exception.Message; exit 1 } }"

if %errorlevel% == 0 (
    echo       OK - Archivos extraidos con System.IO.Compression
    goto :ExtraccionExitosa
)

REM Método 3: VBScript (compatibilidad máxima)
echo       Intentando metodo 3 (VBScript)...
echo Set objShell = CreateObject("Shell.Application") > "%TEMP%\extract.vbs"
echo Set objSource = objShell.NameSpace("%~dp0CalandriaApp.zip") >> "%TEMP%\extract.vbs"
echo Set objTarget = objShell.NameSpace("%INSTALL_DIR%") >> "%TEMP%\extract.vbs"
echo intOptions = 16 + 4 >> "%TEMP%\extract.vbs"
echo objTarget.CopyHere objSource.Items, intOptions >> "%TEMP%\extract.vbs"
echo WScript.Sleep 2000 >> "%TEMP%\extract.vbs"

cscript //nologo "%TEMP%\extract.vbs" >nul 2>&1
del "%TEMP%\extract.vbs" >nul 2>&1

if exist "%INSTALL_DIR%\DynamicSepticSystem.exe" (
    echo       OK - Archivos extraidos con VBScript
    goto :ExtraccionExitosa
)

REM Si todos los métodos fallan
echo       ERROR: No se pudieron extraer los archivos con ningun metodo
echo.
echo       Puedes intentar:
echo       1. Instalar .NET Framework 4.7.2 desde:
echo          https://dotnet.microsoft.com/download/dotnet-framework
echo       2. Extraer manualmente CalandriaApp.zip a: %INSTALL_DIR%
echo       3. Ejecutar este instalador como Administrador
echo.
pause
exit /b 1

:ExtraccionExitosa

REM Paso 3: Crear acceso directo en Escritorio
echo [3/5] Creando acceso directo en Escritorio...
powershell -NoProfile -ExecutionPolicy Bypass -Command "& { try { $WshShell = New-Object -ComObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%USERPROFILE%\Desktop\Calandria Residencial.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\DynamicSepticSystem.exe'; $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; $Shortcut.Description = 'Sistema de Gestion Calandria Residencial'; $Shortcut.Save(); exit 0 } catch { exit 1 } }" >nul 2>&1

if errorlevel 1 (
    echo       ADVERTENCIA: No se pudo crear acceso directo en Escritorio
) else (
    echo       OK
)

REM Paso 4: Crear acceso directo en Menu Inicio
echo [4/5] Creando acceso directo en Menu Inicio...
powershell -NoProfile -ExecutionPolicy Bypass -Command "& { try { $WshShell = New-Object -ComObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%ProgramData%\Microsoft\Windows\Start Menu\Programs\Calandria Residencial.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\DynamicSepticSystem.exe'; $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; $Shortcut.Description = 'Sistema de Gestion Calandria Residencial'; $Shortcut.Save(); exit 0 } catch { exit 1 } }" >nul 2>&1

if errorlevel 1 (
    echo       ADVERTENCIA: No se pudo crear acceso directo en Menu Inicio
) else (
    echo       OK
)

REM Paso 5: Registrar en Panel de Control (solo si es admin)
echo [5/5] Registrando aplicacion...
net session >nul 2>&1
if %errorLevel% == 0 (
    reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\CalandriaResidencial" /v "DisplayName" /t REG_SZ /d "Calandria Residencial" /f >nul 2>&1
    reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\CalandriaResidencial" /v "DisplayVersion" /t REG_SZ /d "1.3.7-H" /f >nul 2>&1
    reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\CalandriaResidencial" /v "Publisher" /t REG_SZ /d "Calandria Residencial" /f >nul 2>&1
    reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\CalandriaResidencial" /v "InstallLocation" /t REG_SZ /d "%INSTALL_DIR%" /f >nul 2>&1
    reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\CalandriaResidencial" /v "DisplayIcon" /t REG_SZ /d "%INSTALL_DIR%\DynamicSepticSystem.exe" /f >nul 2>&1
    echo       OK
) else (
    echo       OMITIDO (requiere permisos de administrador)
)

echo.
echo ============================================================
echo   INSTALACION COMPLETADA
echo ============================================================
echo.
echo Calandria Residencial se ha instalado correctamente en:
echo %INSTALL_DIR%
echo.
echo Puedes ejecutar la aplicacion desde:
echo  - Acceso directo en el Escritorio
echo  - Menu Inicio ^> Calandria Residencial
echo  - Directamente desde: %INSTALL_DIR%\DynamicSepticSystem.exe
echo.
echo ============================================================
echo.

choice /C SN /M "Deseas ejecutar Calandria ahora"
if not errorlevel 2 (
    start "" "%INSTALL_DIR%\DynamicSepticSystem.exe"
)

echo.
echo Gracias por instalar Calandria Residencial!
echo.
pause
