# Instalador PowerShell para Calandria Residencial v1.3.7-H
# Requiere ejecutarse como Administrador

#Requires -RunAsAdministrator

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Instalador de Calandria Residencial v1.3.7-H" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar .NET Framework
Write-Host "Verificando .NET Framework 4.7.2..." -ForegroundColor Yellow
try {
    $dotNetVersion = Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full' -ErrorAction Stop
    if ($dotNetVersion.Release -lt 461808) {
        Write-Host "ERROR: .NET Framework 4.7.2 o superior no está instalado." -ForegroundColor Red
        Write-Host "Descargalo desde: https://dotnet.microsoft.com/download/dotnet-framework" -ForegroundColor Yellow
        Read-Host "Presiona Enter para salir"
        exit 1
    }
    Write-Host "? .NET Framework correcto (Release: $($dotNetVersion.Release))" -ForegroundColor Green
}
catch {
    Write-Host "? No se pudo verificar .NET Framework" -ForegroundColor Red
    $continue = Read-Host "¿Continuar de todos modos? (S/N)"
    if ($continue -ne "S" -and $continue -ne "s") {
        exit 1
    }
}

# Directorio de instalación
$installDir = "C:\Program Files\CalandriaResidencial"
Write-Host ""
Write-Host "Directorio de instalación: $installDir" -ForegroundColor Yellow

# Preguntar confirmación
$confirm = Read-Host "¿Continuar con la instalación? (S/N)"
if ($confirm -ne "S" -and $confirm -ne "s") {
    Write-Host "Instalación cancelada por el usuario." -ForegroundColor Yellow
    exit 0
}

# Crear backup si existe instalación anterior
if (Test-Path $installDir) {
    Write-Host ""
    Write-Host "Se detectó una instalación anterior" -ForegroundColor Yellow
    $backup = Read-Host "¿Crear backup antes de actualizar? (S/N)"
    
    if ($backup -eq "S" -or $backup -eq "s") {
        $backupDir = "$installDir`_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
        Write-Host "Creando backup en: $backupDir" -ForegroundColor Yellow
        Copy-Item -Path $installDir -Destination $backupDir -Recurse -Force
        Write-Host "? Backup creado exitosamente" -ForegroundColor Green
    }
}

# Crear directorio de instalación
Write-Host ""
Write-Host "Creando directorio de instalación..." -ForegroundColor Yellow
if (!(Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
    Write-Host "? Directorio creado" -ForegroundColor Green
}
else {
    Write-Host "? Directorio ya existe, se sobrescribirán los archivos" -ForegroundColor Green
}

# Copiar archivos
Write-Host ""
Write-Host "Copiando archivos de la aplicación..." -ForegroundColor Yellow
$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path

try {
    # Copiar todos los archivos excepto este script
    Get-ChildItem -Path $sourceDir -File | Where-Object { $_.Name -ne "Install.ps1" } | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination $installDir -Force
        Write-Host "  ? $($_.Name)" -ForegroundColor Gray
    }
    
    # Copiar subdirectorios si existen
    Get-ChildItem -Path $sourceDir -Directory | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination $installDir -Recurse -Force
        Write-Host "  ? $($_.Name)\" -ForegroundColor Gray
    }
    
    Write-Host "? Archivos copiados exitosamente" -ForegroundColor Green
}
catch {
    Write-Host "? Error al copiar archivos: $_" -ForegroundColor Red
    Read-Host "Presiona Enter para salir"
    exit 1
}

# Crear acceso directo en escritorio
Write-Host ""
Write-Host "Creando acceso directo en el escritorio..." -ForegroundColor Yellow
try {
    $WshShell = New-Object -comObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut("$env:Public\Desktop\Calandria Residencial.lnk")
    $Shortcut.TargetPath = "$installDir\DynamicSepticSystem.exe"
    $Shortcut.WorkingDirectory = $installDir
    $Shortcut.Description = "Calandria Residencial - Sistema de Gestión"
    $Shortcut.IconLocation = "$installDir\DynamicSepticSystem.exe,0"
    $Shortcut.Save()
    Write-Host "? Acceso directo creado" -ForegroundColor Green
}
catch {
    Write-Host "? No se pudo crear el acceso directo: $_" -ForegroundColor Yellow
}

# Crear entrada en menú inicio
Write-Host ""
Write-Host "Creando entrada en el menú de inicio..." -ForegroundColor Yellow
try {
    $startMenuPath = "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\Calandria Residencial"
    if (!(Test-Path $startMenuPath)) {
        New-Item -ItemType Directory -Path $startMenuPath -Force | Out-Null
    }
    
    $Shortcut = $WshShell.CreateShortcut("$startMenuPath\Calandria Residencial.lnk")
    $Shortcut.TargetPath = "$installDir\DynamicSepticSystem.exe"
    $Shortcut.WorkingDirectory = $installDir
    $Shortcut.Description = "Calandria Residencial - Sistema de Gestión"
    $Shortcut.IconLocation = "$installDir\DynamicSepticSystem.exe,0"
    $Shortcut.Save()
    Write-Host "? Entrada en menú de inicio creada" -ForegroundColor Green
}
catch {
    Write-Host "? No se pudo crear entrada en menú de inicio: $_" -ForegroundColor Yellow
}

# Registrar en Agregar o quitar programas
Write-Host ""
Write-Host "Registrando en 'Agregar o quitar programas'..." -ForegroundColor Yellow
try {
    $regPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\CalandriaResidencial"
    
    if (!(Test-Path $regPath)) {
        New-Item -Path $regPath -Force | Out-Null
    }
    
    Set-ItemProperty -Path $regPath -Name "DisplayName" -Value "Calandria Residencial"
    Set-ItemProperty -Path $regPath -Name "DisplayVersion" -Value "1.3.7-H"
    Set-ItemProperty -Path $regPath -Name "Publisher" -Value "Calandria Residencial"
    Set-ItemProperty -Path $regPath -Name "InstallLocation" -Value $installDir
    Set-ItemProperty -Path $regPath -Name "DisplayIcon" -Value "$installDir\DynamicSepticSystem.exe"
    Set-ItemProperty -Path $regPath -Name "UninstallString" -Value "powershell.exe -ExecutionPolicy Bypass -File `"$installDir\Uninstall.ps1`""
    Set-ItemProperty -Path $regPath -Name "NoModify" -Value 1
    Set-ItemProperty -Path $regPath -Name "NoRepair" -Value 1
    
    Write-Host "? Registro en sistema completado" -ForegroundColor Green
}
catch {
    Write-Host "? No se pudo registrar en el sistema: $_" -ForegroundColor Yellow
}

# Crear desinstalador
Write-Host ""
Write-Host "Creando script de desinstalación..." -ForegroundColor Yellow
$uninstallScript = @"
#Requires -RunAsAdministrator
Write-Host "Desinstalando Calandria Residencial..." -ForegroundColor Yellow
Remove-Item -Path "$installDir" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$env:Public\Desktop\Calandria Residencial.lnk" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\Calandria Residencial" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\CalandriaResidencial" -Force -ErrorAction SilentlyContinue
Write-Host "Desinstalación completada." -ForegroundColor Green
"@
$uninstallScript | Out-File -FilePath "$installDir\Uninstall.ps1" -Encoding UTF8
Write-Host "? Script de desinstalación creado" -ForegroundColor Green

# Verificar archivos críticos
Write-Host ""
Write-Host "Verificando instalación..." -ForegroundColor Yellow
$criticalFiles = @(
    "$installDir\DynamicSepticSystem.exe",
    "$installDir\Updater.exe",
    "$installDir\version.txt"
)

$allOk = $true
foreach ($file in $criticalFiles) {
    if (Test-Path $file) {
        Write-Host "  ? $(Split-Path $file -Leaf)" -ForegroundColor Green
    }
    else {
        Write-Host "  ? $(Split-Path $file -Leaf) NO ENCONTRADO" -ForegroundColor Red
        $allOk = $false
    }
}

# Verificar version.txt
if (Test-Path "$installDir\version.txt") {
    $version = Get-Content "$installDir\version.txt" -Raw
    Write-Host "  ? Versión instalada: $version" -ForegroundColor Cyan
}

# Resumen final
Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "INSTALACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""

if ($allOk) {
    Write-Host "? Todos los archivos se instalaron correctamente" -ForegroundColor Green
}
else {
    Write-Host "? Algunos archivos no se encontraron" -ForegroundColor Yellow
    Write-Host "  Verifica que el paquete esté completo" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Ubicación de instalación:" -ForegroundColor Cyan
Write-Host "  $installDir" -ForegroundColor White
Write-Host ""
Write-Host "Puedes ejecutar la aplicación desde:" -ForegroundColor Cyan
Write-Host "  - Acceso directo en el escritorio" -ForegroundColor White
Write-Host "  - Menú de inicio > Calandria Residencial" -ForegroundColor White
Write-Host "  - $installDir\DynamicSepticSystem.exe" -ForegroundColor White
Write-Host ""
Write-Host "IMPORTANTE:" -ForegroundColor Yellow
Write-Host "  1. Verifica la cadena de conexión en App.config" -ForegroundColor White
Write-Host "  2. Ejecuta SQL_VerificarProveedores.sql en la base de datos" -ForegroundColor White
Write-Host "  3. El sistema se actualizará automáticamente desde GitHub" -ForegroundColor White
Write-Host ""

# Preguntar si desea ejecutar ahora
$ejecutar = Read-Host "¿Deseas ejecutar la aplicación ahora? (S/N)"
if ($ejecutar -eq "S" -or $ejecutar -eq "s") {
    Write-Host ""
    Write-Host "Iniciando aplicación..." -ForegroundColor Yellow
    Start-Process "$installDir\DynamicSepticSystem.exe"
}

Write-Host ""
Write-Host "Instalación finalizada. Presiona Enter para salir." -ForegroundColor Green
Read-Host
