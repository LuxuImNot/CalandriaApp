# ==========================================
# BUILD RELEASE - CALANDRIA v1.3.7-H (PowerShell)
# ==========================================

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "BUILD RELEASE - CALANDRIA v1.3.7-H" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Función para mostrar errores
function Show-Error {
    param([string]$message)
    Write-Host "ERROR: $message" -ForegroundColor Red
    Read-Host "Presiona Enter para salir"
    exit 1
}

# Limpiar builds anteriores
Write-Host "[1/6] Limpiando builds anteriores..." -ForegroundColor Yellow
if (Test-Path "DynamicSepticSystem\bin\Release") { Remove-Item "DynamicSepticSystem\bin\Release" -Recurse -Force }
if (Test-Path "DynamicSepticSystem\obj\Release") { Remove-Item "DynamicSepticSystem\obj\Release" -Recurse -Force }
if (Test-Path "Updater\bin\Release") { Remove-Item "Updater\bin\Release" -Recurse -Force }
if (Test-Path "Updater\obj\Release") { Remove-Item "Updater\obj\Release" -Recurse -Force }
Write-Host "? Limpieza completada" -ForegroundColor Green

# Compilar DynamicSepticSystem
Write-Host ""
Write-Host "[2/6] Compilando DynamicSepticSystem..." -ForegroundColor Yellow
try {
    & msbuild "DynamicSepticSystem\DynamicSepticSystem.csproj" /p:Configuration=Release /p:Platform="Any CPU" /t:Rebuild /v:minimal /nologo
    if ($LASTEXITCODE -ne 0) { throw "Build falló" }
    Write-Host "? DynamicSepticSystem compilado" -ForegroundColor Green
}
catch {
    Show-Error "Fallo al compilar DynamicSepticSystem: $_"
}

# Compilar Updater
Write-Host ""
Write-Host "[3/6] Compilando Updater..." -ForegroundColor Yellow
try {
    & msbuild "Updater\Updater.csproj" /p:Configuration=Release /p:Platform="Any CPU" /t:Rebuild /v:minimal /nologo
    if ($LASTEXITCODE -ne 0) { throw "Build falló" }
    Write-Host "? Updater compilado" -ForegroundColor Green
}
catch {
    Show-Error "Fallo al compilar Updater: $_"
}

# Actualizar version.txt
Write-Host ""
Write-Host "[4/6] Actualizando version.txt..." -ForegroundColor Yellow
"1.3.7-H" | Out-File -FilePath "DynamicSepticSystem\bin\Release\version.txt" -Encoding ASCII -NoNewline
Write-Host "? version.txt actualizado: 1.3.7-H" -ForegroundColor Green

# Copiar Updater.exe
Write-Host ""
Write-Host "[5/6] Copiando Updater.exe..." -ForegroundColor Yellow
Copy-Item "Updater\bin\Release\Updater.exe" "DynamicSepticSystem\bin\Release\Updater.exe" -Force
Write-Host "? Updater.exe copiado" -ForegroundColor Green

# Crear ZIP de distribución
Write-Host ""
Write-Host "[6/6] Creando ZIP de distribución..." -ForegroundColor Yellow
$zipPath = "CalandriaApp-v1.3.7-H.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory(
    "DynamicSepticSystem\bin\Release",
    $zipPath,
    [System.IO.Compression.CompressionLevel]::Optimal,
    $false
)
Write-Host "? ZIP creado: $zipPath" -ForegroundColor Green

# Mostrar resumen
Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "BUILD COMPLETADO EXITOSAMENTE" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Archivos generados:" -ForegroundColor Cyan
Write-Host "  Binarios: DynamicSepticSystem\bin\Release\" -ForegroundColor White
Write-Host "  ZIP:      $zipPath" -ForegroundColor White
Write-Host ""

# Verificar archivos importantes
Write-Host "Verificando archivos críticos:" -ForegroundColor Cyan
$criticalFiles = @(
    "DynamicSepticSystem\bin\Release\DynamicSepticSystem.exe",
    "DynamicSepticSystem\bin\Release\Updater.exe",
    "DynamicSepticSystem\bin\Release\version.txt",
    "DynamicSepticSystem\bin\Release\App.config"
)

foreach ($file in $criticalFiles) {
    if (Test-Path $file) {
        $size = (Get-Item $file).Length
        Write-Host "  ? $(Split-Path $file -Leaf) ($([math]::Round($size/1KB, 2)) KB)" -ForegroundColor Green
    }
    else {
        Write-Host "  ? $(Split-Path $file -Leaf) NO ENCONTRADO" -ForegroundColor Red
    }
}

# Mostrar contenido version.txt
Write-Host ""
$versionContent = Get-Content "DynamicSepticSystem\bin\Release\version.txt" -Raw
Write-Host "Contenido de version.txt: '$versionContent'" -ForegroundColor Cyan

Write-Host ""
Write-Host "Siguiente paso:" -ForegroundColor Yellow
Write-Host "  1. Probar la aplicación desde bin\Release\" -ForegroundColor White
Write-Host "  2. Compilar Setup1 en Visual Studio (modo Release)" -ForegroundColor White
Write-Host "  3. O distribuir el ZIP: $zipPath" -ForegroundColor White
Write-Host "  4. Subir a GitHub Release con tag 1.3.7-H" -ForegroundColor White
Write-Host ""

# Preguntar si abrir carpeta
$openFolder = Read-Host "¿Abrir carpeta Release? (S/N)"
if ($openFolder -eq "S" -or $openFolder -eq "s") {
    explorer "DynamicSepticSystem\bin\Release"
}
