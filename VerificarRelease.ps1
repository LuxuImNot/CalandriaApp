# Script de Verificación Final - Release v1.3.7-H
# Ejecutar antes de publicar en GitHub

Write-Host "========================================" -ForegroundColor Green
Write-Host "  VERIFICACIÓN DE RELEASE v1.3.7-H" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

$errors = @()
$warnings = @()
$success = @()

# 1. Verificar ZIP principal
Write-Host "?? Verificando archivo ZIP..." -ForegroundColor Cyan
$zipPath = ".\CalandriaApp-v1.3.7-H.zip"
if (Test-Path $zipPath) {
    $zipSize = (Get-Item $zipPath).Length / 1MB
    $success += "? ZIP encontrado: $([math]::Round($zipSize, 2)) MB"
} else {
    $errors += "? ZIP no encontrado: $zipPath"
}

# 2. Verificar contenido del ZIP
Write-Host "?? Verificando contenido del ZIP..." -ForegroundColor Cyan
if (Test-Path $zipPath) {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
    
    $requiredFiles = @(
        "DynamicSepticSystem.exe",
        "Updater.exe", 
        "version.txt",
        "DynamicSepticSystem.exe.config"
    )
    
    foreach ($file in $requiredFiles) {
        $found = $zip.Entries | Where-Object { $_.Name -eq $file }
        if ($found) {
            $success += "? Archivo incluido: $file"
        } else {
            $errors += "? Archivo faltante en ZIP: $file"
        }
    }
    
    $zip.Dispose()
}

# 3. Verificar version.txt
Write-Host "?? Verificando version.txt..." -ForegroundColor Cyan
$versionFile = ".\DynamicSepticSystem\bin\Release\version.txt"
if (Test-Path $versionFile) {
    $version = (Get-Content $versionFile -Raw).Trim()
    if ($version -eq "1.3.7-H") {
        $success += "? version.txt correcto: $version"
    } else {
        $errors += "? version.txt incorrecto. Esperado: '1.3.7-H', Encontrado: '$version'"
    }
} else {
    $errors += "? version.txt no encontrado"
}

# 4. Verificar Updater.exe
Write-Host "?? Verificando Updater.exe..." -ForegroundColor Cyan
$updaterPaths = @(
    ".\DynamicSepticSystem\bin\Release\Updater.exe",
    ".\Updater\bin\Release\net472\Updater.exe"
)

$updaterFound = $false
foreach ($path in $updaterPaths) {
    if (Test-Path $path) {
        $updaterFound = $true
        $success += "? Updater.exe encontrado: $path"
        break
    }
}

if (-not $updaterFound) {
    $errors += "? Updater.exe no encontrado en ninguna ubicación"
}

# 5. Verificar DynamicSepticSystem.exe
Write-Host "?? Verificando ejecutable principal..." -ForegroundColor Cyan
$exePath = ".\DynamicSepticSystem\bin\Release\DynamicSepticSystem.exe"
if (Test-Path $exePath) {
    $exeSize = (Get-Item $exePath).Length / 1MB
    $success += "? DynamicSepticSystem.exe encontrado: $([math]::Round($exeSize, 2)) MB"
    
    # Verificar versión del ensamblado
    try {
        $assembly = [System.Reflection.Assembly]::LoadFrom((Resolve-Path $exePath))
        $assemblyVersion = $assembly.GetName().Version
        $success += "? Versión del ensamblado: $assemblyVersion"
    } catch {
        $warnings += "?? No se pudo leer la versión del ensamblado"
    }
} else {
    $errors += "? DynamicSepticSystem.exe no encontrado"
}

# 6. Verificar archivos de documentación
Write-Host "?? Verificando documentación..." -ForegroundColor Cyan
$docs = @(
    "RELEASE_NOTES.txt",
    "GITHUB_RELEASE_GUIDE.md",
    "README_SistemaProveedores.md",
    "README_SistemaVersionado.md"
)

foreach ($doc in $docs) {
    if (Test-Path $doc) {
        $success += "? Documentación encontrada: $doc"
    } else {
        $warnings += "?? Documentación faltante: $doc"
    }
}

# 7. Verificar configuración de GitHub
Write-Host "?? Verificando configuración..." -ForegroundColor Cyan
$configPath = ".\DynamicSepticSystem\bin\Release\DynamicSepticSystem.exe.config"
if (Test-Path $configPath) {
    $config = Get-Content $configPath -Raw
    if ($config -match "GitHubOwner.*LuxuImNot") {
        $success += "? GitHubOwner configurado correctamente"
    } else {
        $warnings += "?? Verificar configuración de GitHubOwner en App.config"
    }
    
    if ($config -match "GitHubRepo.*CalandriaApp") {
        $success += "? GitHubRepo configurado correctamente"
    } else {
        $warnings += "?? Verificar configuración de GitHubRepo en App.config"
    }
}

# RESUMEN FINAL
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "           RESUMEN DE VERIFICACIÓN" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

if ($success.Count -gt 0) {
    Write-Host "? ÉXITOS ($($success.Count)):" -ForegroundColor Green
    foreach ($msg in $success) {
        Write-Host "  $msg" -ForegroundColor White
    }
    Write-Host ""
}

if ($warnings.Count -gt 0) {
    Write-Host "?? ADVERTENCIAS ($($warnings.Count)):" -ForegroundColor Yellow
    foreach ($msg in $warnings) {
        Write-Host "  $msg" -ForegroundColor White
    }
    Write-Host ""
}

if ($errors.Count -gt 0) {
    Write-Host "? ERRORES ($($errors.Count)):" -ForegroundColor Red
    foreach ($msg in $errors) {
        Write-Host "  $msg" -ForegroundColor White
    }
    Write-Host ""
    Write-Host "? NO PUBLICAR EN GITHUB - Corregir errores primero" -ForegroundColor Red
    exit 1
} else {
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "?? LISTO PARA PUBLICAR EN GITHUB ??" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Siguiente paso:" -ForegroundColor Cyan
    Write-Host "1. Ve a: https://github.com/LuxuImNot/CalandriaApp/releases/new" -ForegroundColor White
    Write-Host "2. Tag: 1.3.7-H" -ForegroundColor White
    Write-Host "3. Título: v1.3.7-H - Sistema de Proveedores y Fix de Actualizaciones" -ForegroundColor White
    Write-Host "4. Arrastra estos archivos:" -ForegroundColor White
    Write-Host "   • CalandriaApp-v1.3.7-H.zip" -ForegroundColor Yellow
    Write-Host "   • DynamicSepticSystem\version.txt" -ForegroundColor Yellow
    Write-Host "   • RELEASE_NOTES.txt (opcional)" -ForegroundColor Yellow
    Write-Host "5. Copia la descripción desde GITHUB_RELEASE_GUIDE.md" -ForegroundColor White
    Write-Host "6. Marca como 'Latest release'" -ForegroundColor White
    Write-Host "7. ¡Publicar!" -ForegroundColor White
    Write-Host ""
    exit 0
}
