<#
.SINOPSIS
    Actualiza Calandria.Api EN EL SERVIDOR a partir del zip de deploy.

.USO
    Copia el zip (y su .sha256) al server, abre PowerShell COMO ADMINISTRADOR
    ahi y corre:

        .\desplegar-en-server.ps1 -Zip C:\temp\CalandriaApi-1.9.5.4-20260922-221227.zip

    Hace: verifica el hash -> detiene el servicio -> respalda la carpeta
    actual -> descomprime encima -> arranca -> comprueba /api/health.

.NOTAS
    - El zip NO trae connectionStrings.config ni secrets.config reales, asi
      que los del servidor se conservan (no se sobrescriben).
    - El Calandria.Api.exe.config del zip SI reemplaza al del servidor: trae
      los 7 bindingRedirects que la pila JWT necesita. Nunca lo edites a mano
      (sin ellos, POST /api/auth/login responde 500).
    - Si algo falla, la carpeta anterior queda intacta en el respaldo y se
      restaura con: sc stop CalandriaApi; copiar el respaldo encima; sc start.
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$Zip,

    [string]$Destino = "C:\CalandriaApi",
    [string]$Servicio = "CalandriaApi",
    [int]$Puerto = 8733
)

$ErrorActionPreference = "Stop"

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
        ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "Corre este script como Administrador (detiene/arranca un servicio)."
}
if (-not (Test-Path $Zip)) { Write-Error "No existe el zip: $Zip" }

# 1) Integridad: el .sha256 viaja al lado del zip.
$rutaSha = "$Zip.sha256"
if (Test-Path $rutaSha) {
    $esperado = (Get-Content $rutaSha -Raw).Trim().ToLower()
    $real = (Get-FileHash -Path $Zip -Algorithm SHA256).Hash.ToLower()
    if ($esperado -ne $real) {
        Write-Error "El zip no coincide con su .sha256 (se corrompio al copiar).`n  esperado: $esperado`n  real:     $real"
    }
    Write-Host "SHA256 verificado." -ForegroundColor Green
}
else {
    Write-Host "No se encontro $rutaSha - se sigue sin verificar el hash." -ForegroundColor Yellow
}

# 2) Detener el servicio (si existe y esta corriendo).
$svc = Get-Service -Name $Servicio -ErrorAction SilentlyContinue
if ($svc) {
    if ($svc.Status -ne 'Stopped') {
        Write-Host "Deteniendo $Servicio..." -ForegroundColor Cyan
        Stop-Service -Name $Servicio -Force
        (Get-Service $Servicio).WaitForStatus('Stopped', '00:00:30')
    }
}
else {
    Write-Host "El servicio $Servicio no existe todavia: despues de copiar, corre instalar-servicio.bat." -ForegroundColor Yellow
}

# 3) Respaldo de la carpeta actual.
if (Test-Path $Destino) {
    $respaldo = "$Destino.bak-" + (Get-Date -Format 'yyyyMMdd-HHmmss')
    Write-Host "Respaldando $Destino -> $respaldo" -ForegroundColor Cyan
    Copy-Item $Destino $respaldo -Recurse -Force
}
else {
    New-Item -ItemType Directory -Path $Destino | Out-Null
}

# 4) Descomprimir encima. Expand-Archive -Force sobrescribe lo que viene en el
#    zip y deja intacto lo que no viene (los .config reales del servidor).
Write-Host "Descomprimiendo sobre $Destino..." -ForegroundColor Cyan
Expand-Archive -Path $Zip -DestinationPath $Destino -Force

# 5) Arrancar y comprobar.
if ($svc) {
    Write-Host "Arrancando $Servicio..." -ForegroundColor Cyan
    Start-Service -Name $Servicio
    (Get-Service $Servicio).WaitForStatus('Running', '00:00:30')

    Start-Sleep -Seconds 2
    try {
        $r = Invoke-WebRequest -Uri "http://localhost:$Puerto/api/health" -UseBasicParsing -TimeoutSec 10
        Write-Host "/api/health -> $($r.StatusCode)" -ForegroundColor Green
    }
    catch {
        Write-Host "/api/health NO respondio: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Revisa $Destino\logs\api-$(Get-Date -Format 'yyyyMMdd').log" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Listo. Si algo salio mal, el respaldo esta en $Destino.bak-*" -ForegroundColor Cyan
