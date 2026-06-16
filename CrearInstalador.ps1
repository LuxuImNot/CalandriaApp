# Script para Crear Instalador de Calandria v1.3.7-H
# Este script crea un instalador ejecutable autoextraíble

param(
    [string]$Version = "1.3.7-H",
    [string]$OutputDir = ".\Installer"
)

Write-Host "?????????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?   CREADOR DE INSTALADOR - CALANDRIA v$Version              ?" -ForegroundColor Green
Write-Host "?????????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""

# Verificar que exista el ZIP
$zipPath = ".\CalandriaApp-v$Version.zip"
if (-not (Test-Path $zipPath)) {
    Write-Host "? ERROR: No se encuentra $zipPath" -ForegroundColor Red
    Write-Host "   Ejecuta primero BuildRelease.ps1" -ForegroundColor Yellow
    exit 1
}

Write-Host "? ZIP encontrado: $zipPath" -ForegroundColor Green

# Crear directorio de salida
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# Script de instalación embebido
$installerScript = @'
# Instalador de Calandria Residencial
# Este script se autoejecuta desde el instalador

param([string]$ExtractPath = "$env:ProgramFiles\CalandriaResidencial")

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  INSTALADOR DE CALANDRIA RESIDENCIAL v{VERSION}" -ForegroundColor Cyan
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Verificar privilegios de administrador
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "??  Este instalador requiere permisos de Administrador" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Reintentando con elevación..." -ForegroundColor Yellow
    
    $scriptPath = $MyInvocation.MyCommand.Path
    Start-Process powershell.exe "-ExecutionPolicy Bypass -File `"$scriptPath`"" -Verb RunAs
    exit
}

Write-Host "? Ejecutando como Administrador" -ForegroundColor Green
Write-Host ""

# Solicitar confirmación de instalación
Write-Host "?? Directorio de instalación:" -ForegroundColor Cyan
Write-Host "   $ExtractPath" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "¿Deseas continuar con la instalación? (S/N)"
if ($confirm -notmatch '^[Ss]$') {
    Write-Host "? Instalación cancelada" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "?? Extrayendo archivos..." -ForegroundColor Cyan

try {
    # Crear directorio si no existe
    if (-not (Test-Path $ExtractPath)) {
        New-Item -ItemType Directory -Path $ExtractPath -Force | Out-Null
        Write-Host "   ? Directorio creado" -ForegroundColor Green
    }

    # El ZIP está embebido en este script, extraerlo
    $zipBytes = [System.IO.File]::ReadAllBytes("$PSScriptRoot\payload.zip")
    $tempZip = [System.IO.Path]::Combine($env:TEMP, "calandria_install.zip")
    [System.IO.File]::WriteAllBytes($tempZip, $zipBytes)
    
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory($tempZip, $ExtractPath, $true)
    
    Remove-Item $tempZip -Force
    
    Write-Host "   ? Archivos extraídos correctamente" -ForegroundColor Green
    
} catch {
    Write-Host "   ? Error al extraer archivos: $($_.Exception.Message)" -ForegroundColor Red
    Read-Host "Presiona Enter para salir"
    exit 1
}

Write-Host ""
Write-Host "?? Creando acceso directo..." -ForegroundColor Cyan

try {
    # Crear acceso directo en el escritorio
    $WshShell = New-Object -ComObject WScript.Shell
    $desktopPath = [System.Environment]::GetFolderPath('Desktop')
    $shortcutPath = Join-Path $desktopPath "Calandria Residencial.lnk"
    $shortcut = $WshShell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = Join-Path $ExtractPath "DynamicSepticSystem.exe"
    $shortcut.WorkingDirectory = $ExtractPath
    $shortcut.Description = "Sistema de Gestión Calandria Residencial"
    $shortcut.Save()
    
    Write-Host "   ? Acceso directo creado en el Escritorio" -ForegroundColor Green
    
    # Crear en menú inicio
    $startMenuPath = "$env:ProgramData\Microsoft\Windows\Start Menu\Programs"
    $startShortcutPath = Join-Path $startMenuPath "Calandria Residencial.lnk"
    $startShortcut = $WshShell.CreateShortcut($startShortcutPath)
    $startShortcut.TargetPath = Join-Path $ExtractPath "DynamicSepticSystem.exe"
    $startShortcut.WorkingDirectory = $ExtractPath
    $startShortcut.Description = "Sistema de Gestión Calandria Residencial"
    $startShortcut.Save()
    
    Write-Host "   ? Acceso directo creado en Menú Inicio" -ForegroundColor Green
    
} catch {
    Write-Host "   ??  No se pudo crear acceso directo: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "?? Configurando registro..." -ForegroundColor Cyan

try {
    # Registrar en Panel de Control > Programas
    $regPath = "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\CalandriaResidencial"
    
    if (-not (Test-Path $regPath)) {
        New-Item -Path $regPath -Force | Out-Null
    }
    
    Set-ItemProperty -Path $regPath -Name "DisplayName" -Value "Calandria Residencial"
    Set-ItemProperty -Path $regPath -Name "DisplayVersion" -Value "{VERSION}"
    Set-ItemProperty -Path $regPath -Name "Publisher" -Value "Calandria Residencial"
    Set-ItemProperty -Path $regPath -Name "InstallLocation" -Value $ExtractPath
    Set-ItemProperty -Path $regPath -Name "UninstallString" -Value "powershell.exe -ExecutionPolicy Bypass -File `"$ExtractPath\Uninstall.ps1`""
    Set-ItemProperty -Path $regPath -Name "DisplayIcon" -Value "$ExtractPath\DynamicSepticSystem.exe"
    Set-ItemProperty -Path $regPath -Name "NoModify" -Value 1
    Set-ItemProperty -Path $regPath -Name "NoRepair" -Value 1
    
    Write-Host "   ? Aplicación registrada en el sistema" -ForegroundColor Green
    
} catch {
    Write-Host "   ??  No se pudo registrar: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "???  Creando desinstalador..." -ForegroundColor Cyan

$uninstallScript = @"
# Desinstalador de Calandria Residencial
`$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  DESINSTALADOR DE CALANDRIA RESIDENCIAL" -ForegroundColor Cyan
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

`$confirm = Read-Host "¿Estás seguro de desinstalar Calandria Residencial? (S/N)"
if (`$confirm -notmatch '^[Ss]$') {
    Write-Host "Desinstalación cancelada" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "???  Eliminando archivos..." -ForegroundColor Cyan

try {
    # Eliminar acceso directo del escritorio
    `$desktopPath = [System.Environment]::GetFolderPath('Desktop')
    `$shortcutPath = Join-Path `$desktopPath "Calandria Residencial.lnk"
    if (Test-Path `$shortcutPath) {
        Remove-Item `$shortcutPath -Force
        Write-Host "   ? Acceso directo eliminado del Escritorio" -ForegroundColor Green
    }
    
    # Eliminar del menú inicio
    `$startMenuPath = "`$env:ProgramData\Microsoft\Windows\Start Menu\Programs"
    `$startShortcutPath = Join-Path `$startMenuPath "Calandria Residencial.lnk"
    if (Test-Path `$startShortcutPath) {
        Remove-Item `$startShortcutPath -Force
        Write-Host "   ? Acceso directo eliminado del Menú Inicio" -ForegroundColor Green
    }
    
    # Eliminar entrada del registro
    `$regPath = "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\CalandriaResidencial"
    if (Test-Path `$regPath) {
        Remove-Item -Path `$regPath -Force -Recurse
        Write-Host "   ? Registro del sistema limpiado" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "? Desinstalación completada" -ForegroundColor Green
    Write-Host ""
    Write-Host "??  NOTA: Los archivos de instalación deben eliminarse manualmente:" -ForegroundColor Yellow
    Write-Host "   $ExtractPath" -ForegroundColor White
    Write-Host ""
    Write-Host "Presiona Enter para abrir la carpeta..." -ForegroundColor Cyan
    Read-Host
    
    # Abrir carpeta padre para que el usuario elimine
    `$parentPath = Split-Path `$PSScriptRoot -Parent
    Start-Process explorer.exe `$parentPath
    
} catch {
    Write-Host "   ? Error durante desinstalación: `$(`$_.Exception.Message)" -ForegroundColor Red
    Read-Host "Presiona Enter para salir"
    exit 1
}
"@

try {
    $uninstallScript | Out-File -FilePath (Join-Path $ExtractPath "Uninstall.ps1") -Encoding UTF8 -Force
    Write-Host "   ? Desinstalador creado" -ForegroundColor Green
} catch {
    Write-Host "   ??  No se pudo crear desinstalador: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "  ? INSTALACIÓN COMPLETADA EXITOSAMENTE" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "?? Ubicación de instalación:" -ForegroundColor Cyan
Write-Host "   $ExtractPath" -ForegroundColor White
Write-Host ""
Write-Host "?? Para ejecutar la aplicación:" -ForegroundColor Cyan
Write-Host "   • Usa el acceso directo del Escritorio" -ForegroundColor White
Write-Host "   • O busca 'Calandria' en el Menú Inicio" -ForegroundColor White
Write-Host ""

$launch = Read-Host "¿Deseas ejecutar Calandria ahora? (S/N)"
if ($launch -match '^[Ss]$') {
    Start-Process (Join-Path $ExtractPath "DynamicSepticSystem.exe")
}

Write-Host ""
Write-Host "Presiona Enter para salir..." -ForegroundColor Gray
Read-Host
'@

# Reemplazar placeholders
$installerScript = $installerScript -replace '{VERSION}', $Version

Write-Host ""
Write-Host "?? Generando script de instalación..." -ForegroundColor Cyan

$installerScriptPath = Join-Path $OutputDir "Install.ps1"
$installerScript | Out-File -FilePath $installerScriptPath -Encoding UTF8 -Force

Write-Host "   ? Script generado: $installerScriptPath" -ForegroundColor Green

# Copiar el ZIP como payload
Write-Host ""
Write-Host "?? Copiando archivos de instalación..." -ForegroundColor Cyan

$payloadPath = Join-Path $OutputDir "payload.zip"
Copy-Item $zipPath $payloadPath -Force

Write-Host "   ? Payload copiado" -ForegroundColor Green

# Crear archivo README para el instalador
$readmeContent = @"
# INSTALADOR DE CALANDRIA RESIDENCIAL v$Version

## ?? Contenido del Instalador

- Install.ps1 - Script de instalación
- payload.zip - Archivos de la aplicación

## ?? Instrucciones de Instalación

### Opción 1: Ejecutar directamente (Recomendado)
1. Haz clic derecho en 'Install.ps1'
2. Selecciona 'Ejecutar con PowerShell'
3. Si aparece advertencia de seguridad, selecciona 'Ejecutar de todos modos'
4. Acepta el UAC (solicitud de permisos de administrador)
5. Sigue las instrucciones en pantalla

### Opción 2: Desde PowerShell
``````powershell
# Abrir PowerShell como Administrador
cd "$OutputDir"
Set-ExecutionPolicy Bypass -Scope Process -Force
.\Install.ps1
``````

## ?? Ubicación de Instalación

Por defecto: ``C:\Program Files\CalandriaResidencial``

## ?? Características de la Instalación

? Copia todos los archivos necesarios
? Crea acceso directo en el Escritorio
? Crea acceso directo en el Menú Inicio
? Registra la aplicación en Panel de Control
? Crea desinstalador automático

## ??? Desinstalación

1. Panel de Control > Programas y características > Calandria Residencial
2. O ejecuta: ``C:\Program Files\CalandriaResidencial\Uninstall.ps1``

## ?? Requisitos

- Windows 7/8/10/11
- .NET Framework 4.7.2+
- Permisos de Administrador
- SQL Server accesible (configurado en App.config)

## ?? Notas

- La instalación requiere permisos de administrador
- Asegúrate de tener configurada la cadena de conexión a SQL Server
- Backup de datos antes de actualizar desde versiones anteriores

## ?? Soporte

Para problemas con la instalación, revisa los logs en:
``C:\Program Files\CalandriaResidencial\Actualizador.log``

---
Versión: $Version
Fecha: $(Get-Date -Format "yyyy-MM-dd")
"@

$readmePath = Join-Path $OutputDir "README_INSTALADOR.txt"
$readmeContent | Out-File -FilePath $readmePath -Encoding UTF8 -Force

Write-Host "   ? README creado" -ForegroundColor Green

# Crear archivo BAT para facilitar ejecución
$batchContent = @"
@echo off
echo ========================================
echo   INSTALADOR DE CALANDRIA v$Version
echo ========================================
echo.
echo Iniciando instalacion...
echo.
powershell.exe -ExecutionPolicy Bypass -File "%~dp0Install.ps1"
pause
"@

$batchPath = Join-Path $OutputDir "Instalar.bat"
$batchContent | Out-File -FilePath $batchPath -Encoding ASCII -Force

Write-Host "   ? Instalador BAT creado" -ForegroundColor Green

Write-Host ""
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "  ? INSTALADOR CREADO EXITOSAMENTE" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "?? Ubicación: $OutputDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Archivos generados:" -ForegroundColor Cyan
Get-ChildItem $OutputDir | ForEach-Object {
    $size = if ($_.PSIsContainer) { "" } else { " ({0:N2} MB)" -f ($_.Length / 1MB) }
    Write-Host "   • $($_.Name)$size" -ForegroundColor White
}
Write-Host ""
Write-Host "?? Para distribuir el instalador:" -ForegroundColor Cyan
Write-Host "   1. Comprime la carpeta '$OutputDir' en un ZIP" -ForegroundColor White
Write-Host "   2. Distribuye el ZIP a los usuarios" -ForegroundColor White
Write-Host "   3. Los usuarios descomprimen y ejecutan 'Instalar.bat'" -ForegroundColor White
Write-Host ""
Write-Host "?? O crea un Setup.exe autoextraíble con IExpress:" -ForegroundColor Yellow
Write-Host "   .\CrearSetupExe.ps1" -ForegroundColor White
Write-Host ""

# Preguntar si quiere crear Setup.exe
$createExe = Read-Host "¿Deseas crear un Setup.exe autoextraíble ahora? (S/N)"
if ($createExe -match '^[Ss]$') {
    Write-Host ""
    Write-Host "Creando Setup.exe con IExpress..." -ForegroundColor Cyan
    
    # Crear SED para IExpress
    $sedContent = @"
[Version]
Class=IEXPRESS
SEDVersion=3
[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=0
HideExtractAnimation=1
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
InstallPrompt=¿Deseas instalar Calandria Residencial v$Version?
DisplayLicense=
FinishMessage=Instalación completada. Ejecuta Calandria desde el Menú Inicio o Escritorio.
TargetName=$OutputDir\CalandriaSetup-v$Version.exe
FriendlyName=Calandria Residencial v$Version
AppLaunched=cmd /c "powershell.exe -ExecutionPolicy Bypass -File Install.ps1"
PostInstallCmd=<None>
AdminQuietInstCmd=
UserQuietInstCmd=
FILE0="Install.ps1"
FILE1="payload.zip"
FILE2="README_INSTALADOR.txt"
[SourceFiles]
SourceFiles0=$OutputDir\
[SourceFiles0]
%FILE0%=
%FILE1%=
%FILE2%=
"@
    
    $sedPath = Join-Path $OutputDir "setup.sed"
    $sedContent | Out-File -FilePath $sedPath -Encoding ASCII -Force
    
    # Ejecutar IExpress
    try {
        Start-Process "iexpress.exe" -ArgumentList "/N `"$sedPath`"" -Wait -NoNewWindow
        
        $setupExePath = Join-Path $OutputDir "CalandriaSetup-v$Version.exe"
        if (Test-Path $setupExePath) {
            Write-Host ""
            Write-Host "? Setup.exe creado exitosamente!" -ForegroundColor Green
            Write-Host "   Ubicación: $setupExePath" -ForegroundColor White
            $exeSize = (Get-Item $setupExePath).Length / 1MB
            Write-Host "   Tamaño: $($exeSize.ToString('F2')) MB" -ForegroundColor White
            Write-Host ""
            Write-Host "?? Este archivo .exe puede distribuirse directamente" -ForegroundColor Cyan
        } else {
            Write-Host "??  No se pudo crear el Setup.exe" -ForegroundColor Yellow
            Write-Host "   Distribuye la carpeta '$OutputDir' comprimida" -ForegroundColor White
        }
    } catch {
        Write-Host "??  Error al crear Setup.exe: $($_.Exception.Message)" -ForegroundColor Yellow
        Write-Host "   Distribuye la carpeta '$OutputDir' comprimida" -ForegroundColor White
    }
}

Write-Host ""
Write-Host "Presiona Enter para finalizar..." -ForegroundColor Gray
Read-Host
