<#
.SINOPSIS
    Bumpea la version del ensamblado (Properties\AssemblyInfo.cs), compila
    DynamicSepticSystem en Release, arma el ZIP (para el auto-updater) y el
    instalador CalandriaSetup.exe (para equipos nuevos, si Inno Setup esta
    instalado), genera el .sha256 del ZIP y publica todo como una release de
    GitHub (usa la sesion de "gh" ya logueada en esta maquina - no guarda
    ningun token en este script).

.USO
    .\publicar-release.ps1 -Version 1.9.5.0
    .\publicar-release.ps1 -Version 1.9.5.0 -Notas "Fix bug en Almacen"

    No compila nada por su cuenta si algo falla a medias: cada paso se
    detiene con un mensaje claro antes de tocar GitHub.
#>
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+\.\d+$')]
    [string]$Version,

    [string]$Notas = "",

    # Por si alguna vez cambian de cuenta/repo.
    [string]$Owner = "LuxuImNot",
    [string]$Repo = "CalandriaApp"
)

$ErrorActionPreference = "Stop"
$raiz = $PSScriptRoot
$tag = "v$Version"
$nombreZip = "CalandriaApp-$tag.zip"
$rutaZip = Join-Path $raiz $nombreZip
$rutaSha = "$rutaZip.sha256"
$rutaAssemblyInfo = Join-Path $raiz "DynamicSepticSystem\Properties\AssemblyInfo.cs"
# Estos NO deben viajar en el ZIP ni en el instalador: secrets.config trae el
# token personal de GitHub del desarrollador (la app funciona sin el, solo baja
# el limite de peticiones a la API), y los .example no le sirven de nada a un
# usuario final. connectionStrings.config SI se incluye (todavia hace falta
# mientras dure la migracion a la Web API), pero NO el del desarrollador:
# ver el paso 5.2.
$excluirDeRelease = @("secrets.config", "secrets.config.example", "connectionStrings.config.example")
$rutaConnRelease = Join-Path $raiz "connectionStrings.release.config"

Write-Host "=== Publicar release $tag ===" -ForegroundColor Cyan

# 1) Verificar que "gh" este disponible y logueado ANTES de compilar nada.
gh auth status 2>$null | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Error "gh no esta autenticado. Corre 'gh auth login' primero."
}

# 2) Verificar que la version no exista ya como release (evita pisar una).
#    "gh" escribe a stderr cuando NO encuentra la release (el caso normal, una
#    version nueva): con $ErrorActionPreference="Stop" eso se vuelve un error
#    terminante pese al "2>$null", así que se baja la preferencia solo para
#    esta llamada puntual.
$prevEAP = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
gh release view $tag --repo "$Owner/$Repo" 2>$null | Out-Null
$ErrorActionPreference = $prevEAP
if ($LASTEXITCODE -eq 0) {
    Write-Error "Ya existe una release '$tag' en $Owner/$Repo. Usa otra version o borra esa release primero."
}

# 3) Bumpear AssemblyVersion/AssemblyFileVersion en AssemblyInfo.cs. Esta es la
#    UNICA fuente de verdad de la version: el .exe la trae embebida y
#    Actualizador.VersionLocal la lee directo del binario (ya no hay
#    version.txt suelto que se pueda desincronizar).
if (-not (Test-Path $rutaAssemblyInfo)) { Write-Error "No se encontro $rutaAssemblyInfo" }
$contenido = Get-Content -Path $rutaAssemblyInfo -Raw
$contenido = $contenido -replace 'AssemblyVersion\("[\d.]+"\)', "AssemblyVersion(`"$Version`")"
$contenido = $contenido -replace 'AssemblyFileVersion\("[\d.]+"\)', "AssemblyFileVersion(`"$Version`")"
Set-Content -Path $rutaAssemblyInfo -Value $contenido -NoNewline -Encoding utf8
Write-Host "AssemblyInfo.cs -> $Version"

# 4) Ubicar MSBuild con vswhere (no asume una ruta fija de Visual Studio).
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path $vswhere)) { Write-Error "No se encontro vswhere.exe. Instala Visual Studio o ajusta la ruta de MSBuild a mano." }
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1
if (-not $msbuild) { Write-Error "No se encontro MSBuild.exe via vswhere." }
Write-Host "MSBuild: $msbuild"

# 5) Compilar en Release (Rebuild, no Build: build_target.md ya advierte que
#    Build a secas puede dejar el .exe sin recursos embebidos actualizados).
Write-Host "Compilando (Release)..." -ForegroundColor Cyan
& $msbuild "$raiz\DynamicSepticSystem.sln" /t:Rebuild /p:Configuration=Release /m /nologo /v:minimal
if ($LASTEXITCODE -ne 0) { Write-Error "La compilacion fallo. Revisa los errores arriba." }

# 5.1) Updater es un proyecto aparte del sln: /t:Rebuild SI lo recompila, pero
#      en SU PROPIA carpeta de salida (Updater\bin\Release\net472\), no en
#      DynamicSepticSystem\bin\Release\ (que es lo que se empaqueta abajo).
#      Sin este copiado, el ZIP se queda con cualquier Updater.exe viejo que
#      haya quedado pegado ahi de una compilacion manual anterior.
Write-Host "Copiando Updater.exe recien compilado al output del cliente..." -ForegroundColor Cyan
$updaterSrc = Join-Path $raiz "Updater\bin\Release\net472"
$updaterDst = Join-Path $raiz "DynamicSepticSystem\bin\Release"
if (-not (Test-Path (Join-Path $updaterSrc "Updater.exe"))) { Write-Error "No se encontro Updater.exe compilado en $updaterSrc" }
Copy-Item (Join-Path $updaterSrc "Updater.exe") $updaterDst -Force
$updaterConfig = Join-Path $updaterSrc "Updater.exe.config"
if (Test-Path $updaterConfig) { Copy-Item $updaterConfig $updaterDst -Force }

# 5.2) connectionStrings.config: el que deja la compilacion en bin\Release es
#      el del desarrollador (localhost\SQLEXPRESS). Si viaja asi, en una maquina
#      sin SQLEXPRESS el cliente se congela ~30 s al entrar al panel
#      (InventarioService abre la conexion sincrona en el hilo de UI) y falla
#      toda pantalla que todavia use SQL directo. Se sustituye por la cadena del
#      cliente ANTES de empaquetar, para que la arreglen tanto el ZIP como el
#      instalador (installer.iss tambien lee de bin\Release).
if (-not (Test-Path $rutaConnRelease)) {
    Write-Error "Falta $rutaConnRelease (la cadena de conexion que se le entrega al cliente). Copiala de connectionStrings.config.example, apuntala al servidor de produccion y vuelve a correr."
}
Copy-Item $rutaConnRelease (Join-Path $updaterDst "connectionStrings.config") -Force
$connEmpaquetada = Get-Content (Join-Path $updaterDst "connectionStrings.config") -Raw
if ($connEmpaquetada -match '(?i)(localhost|\(local\)|Integrated Security)') {
    Write-Error "connectionStrings.release.config apunta a una instancia local o usa Integrated Security. Los clientes no tienen esa instancia ni ese acceso: pon el servidor y el usuario SQL de produccion."
}
Write-Host "connectionStrings.config -> cadena del cliente (connectionStrings.release.config)"

# 6) Armar el ZIP desde bin\Release, EXCLUYENDO secrets.config/*.example.
#    Se copia a una carpeta temporal (Compress-Archive no filtra archivos por
#    nombre) y de ahi se comprime.
$origen = Join-Path $raiz "DynamicSepticSystem\bin\Release"
if (-not (Test-Path $origen)) { Write-Error "No existe $origen - la compilacion no genero el output esperado." }

$staging = Join-Path $raiz "bin\ReleaseStaging"
if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
Copy-Item -Path $origen -Destination $staging -Recurse
foreach ($nombre in $excluirDeRelease) {
    $ruta = Join-Path $staging $nombre
    if (Test-Path $ruta) { Remove-Item $ruta -Force }
}

if (Test-Path $rutaZip) { Remove-Item $rutaZip -Force }
Write-Host "Empaquetando $nombreZip..." -ForegroundColor Cyan
Compress-Archive -Path (Join-Path $staging '*') -DestinationPath $rutaZip -CompressionLevel Optimal

# 7) Hash del ZIP. Actualizador.cs espera el archivo "<zip>.sha256" con el
#    hash en hex, sin nada mas.
$hash = (Get-FileHash -Path $rutaZip -Algorithm SHA256).Hash.ToLower()
Set-Content -Path $rutaSha -Value $hash -NoNewline -Encoding ascii
Write-Host "SHA256: $hash"

# 8) Instalador CalandriaSetup.exe (opcional): solo si Inno Setup esta
#    instalado. Si no, se avisa y se sigue solo con el ZIP (el auto-updater
#    no depende del instalador para nada).
$rutaInstaladorExe = $null
$iscc = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
if (-not $iscc) {
    $rutaDefault = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
    if (Test-Path $rutaDefault) { $iscc = Get-Item $rutaDefault }
}
if ($iscc) {
    Write-Host "Compilando instalador con Inno Setup..." -ForegroundColor Cyan
    & $iscc.Source "$raiz\installer.iss" "/DMyAppVersion=$Version" "/O$raiz" | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Error "ISCC.exe fallo compilando installer.iss. Revisa el mensaje arriba." }
    $rutaInstaladorExe = Join-Path $raiz "CalandriaSetup-v$Version.exe"
    if (-not (Test-Path $rutaInstaladorExe)) { Write-Error "ISCC.exe no genero $rutaInstaladorExe (revisa OutputBaseFilename en installer.iss)." }
    Write-Host "Instalador generado: $rutaInstaladorExe"
}
else {
    Write-Host "Inno Setup (ISCC.exe) no esta instalado - se publica solo el ZIP, sin instalador." -ForegroundColor Yellow
    Write-Host "  Descargalo de https://jrsoftware.org/isdl.php si quieres generar CalandriaSetup.exe." -ForegroundColor Yellow
}

# 9) Publicar la release en GitHub. Assets: el zip + su .sha256 (lo que el
#    auto-updater necesita) y, si se genero, el instalador para equipos nuevos.
#    "gh release create" interpreta "archivo#etiqueta" en cada asset (para
#    ponerle una etiqueta visible al asset); como esta carpeta vive bajo
#    "...\ProgramC#\..." (un "#" real en la ruta), gh corta la ruta justo ahi
#    y falla con "system cannot find the file". Se copian los assets a una
#    carpeta temporal sin "#" antes de subirlos.
$notasFinal = if ($Notas) { $Notas } else { "Release $tag" }
$assetsOrigen = @($rutaZip, $rutaSha)
if ($rutaInstaladorExe) { $assetsOrigen += $rutaInstaladorExe }

$stagingAssets = Join-Path $env:TEMP "CalandriaRelease_$tag"
if (Test-Path $stagingAssets) { Remove-Item $stagingAssets -Recurse -Force }
New-Item -ItemType Directory -Path $stagingAssets | Out-Null
$assets = foreach ($a in $assetsOrigen) {
    $destino = Join-Path $stagingAssets (Split-Path $a -Leaf)
    Copy-Item -Path $a -Destination $destino -Force
    $destino
}

Write-Host "Publicando release en GitHub..." -ForegroundColor Cyan
# --notes-file, no --notes: PowerShell 5.1 parte una nota de varias lineas en
# varios argumentos, y gh toma los sobrantes como nombres de asset ("no matches
# found for ..."). Por archivo no hay nada que partir.
$rutaNotas = Join-Path $stagingAssets "notas.md"
Set-Content -Path $rutaNotas -Value $notasFinal -Encoding utf8
gh release create $tag @assets `
    --repo "$Owner/$Repo" `
    --title $tag `
    --notes-file $rutaNotas

if ($LASTEXITCODE -ne 0) { Write-Error "gh release create fallo. Revisa el mensaje de arriba; no se dejo nada a medias en GitHub (o revisa manualmente)." }

Remove-Item $staging -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $stagingAssets -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Release $tag publicada: https://github.com/$Owner/$Repo/releases/tag/$tag" -ForegroundColor Green
Write-Host "Recuerda: Properties\AssemblyInfo.cs cambio en tu working tree. Committea/pushea si quieres que el repo quede en sync." -ForegroundColor Yellow
