# Script Rápido para Crear Instalador con BAT mejorado

param(
    [string]$Version = "1.3.7-H"
)

Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "  CREADOR DE INSTALADOR - CALANDRIA v$Version" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""

# Verificar ZIP
$zipPath = ".\CalandriaApp-v$Version.zip"
if (-not (Test-Path $zipPath)) {
    Write-Host "? ERROR: No se encuentra $zipPath" -ForegroundColor Red
    exit 1
}

Write-Host "? ZIP encontrado" -ForegroundColor Green
Write-Host ""

# Crear carpeta de instalador
$installerDir = ".\InstaladorCalandria"
if (Test-Path $installerDir) {
    Remove-Item $installerDir -Recurse -Force
}
New-Item -ItemType Directory -Path $installerDir -Force | Out-Null

Write-Host "?? Creando paquete de instalación..." -ForegroundColor Cyan

# Copiar ZIP
Copy-Item $zipPath "$installerDir\CalandriaApp.zip" -Force

# Leer el instalador mejorado que acabamos de crear
$installBatContent = Get-Content ".\InstaladorMejorado.bat" -Raw

$installBatPath = "$installerDir\Instalar.bat"
$installBatContent | Out-File -FilePath $installBatPath -Encoding ASCII -Force

Write-Host "   ? Script de instalación mejorado creado" -ForegroundColor Green

# Crear README
$readme = @"
??????????????????????????????????????????????????????????????????
?                                                                ?
?     INSTALADOR DE CALANDRIA RESIDENCIAL v$Version             ?
?                                                                ?
??????????????????????????????????????????????????????????????????

?? CONTENIDO DEL PAQUETE

  • Instalar.bat ................ Script de instalación
  • CalandriaApp.zip ............ Archivos de la aplicación
  • README.txt .................. Este archivo


?? INSTRUCCIONES DE INSTALACIÓN

  1. Cierra cualquier instancia de Calandria que esté ejecutándose

  2. Haz doble clic en: Instalar.bat

  3. Si aparece "Windows protegió tu PC":
     - Haz clic en "Más información"
     - Luego en "Ejecutar de todos modos"

  4. Si aparece UAC (Control de cuentas de usuario):
     - Haz clic en "Sí" para permitir cambios

  5. Sigue las instrucciones en pantalla

  6. ¡Listo! La aplicación está instalada


?? UBICACIÓN DE INSTALACIÓN

  Por defecto: C:\Program Files\CalandriaResidencial

  Puedes cambiar esta ubicación durante la instalación.


? QUÉ HACE EL INSTALADOR

  ? Copia todos los archivos necesarios
  ? Crea acceso directo en el Escritorio
  ? Crea acceso directo en el Menú Inicio
  ? Registra la app en Panel de Control (si es administrador)


?? REQUISITOS DEL SISTEMA

  • Windows 7, 8, 10 u 11
  • .NET Framework 4.7.2 o superior
  • 2 GB de RAM (mínimo)
  • 500 MB de espacio en disco
  • SQL Server accesible (configurado en App.config)


?? NOTAS IMPORTANTES

  • Se recomienda ejecutar como Administrador para instalación completa
  • Si ya tienes Calandria instalado, se actualizará automáticamente
  • Asegúrate de tener configurada la conexión a SQL Server
  • Haz backup de tus datos antes de actualizar


??? DESINSTALACIÓN

  Opción 1: Panel de Control
    1. Panel de Control > Programas y características
    2. Buscar "Calandria Residencial"
    3. Clic derecho > Desinstalar

  Opción 2: Manual
    1. Eliminar carpeta: C:\Program Files\CalandriaResidencial
    2. Eliminar accesos directos del Escritorio y Menú Inicio


?? SOLUCIÓN DE PROBLEMAS

  Problema: "No se puede extraer el archivo"
  Solución: Instala .NET Framework 4.7.2 o superior desde:
           https://dotnet.microsoft.com/download/dotnet-framework

  Problema: "Acceso denegado"
  Solución: Ejecuta Instalar.bat como Administrador
           (clic derecho > Ejecutar como administrador)

  Problema: "La aplicación no inicia"
  Solución: 
    1. Verifica la conexión a SQL Server
    2. Revisa el archivo: Actualizador.log
    3. Instala .NET Framework 4.7.2+


?? SOPORTE

  Para problemas de instalación:
  1. Revisa el archivo Actualizador.log en la carpeta de instalación
  2. Verifica Event Viewer de Windows (Logs de aplicación)
  3. Contacta al equipo de desarrollo


???????????????????????????????????????????????????????????????

Versión: $Version
Fecha: $(Get-Date -Format "dd/MM/yyyy")
Target Framework: .NET Framework 4.7.2

???????????????????????????????????????????????????????????????
"@

$readme | Out-File -FilePath "$installerDir\README.txt" -Encoding UTF8 -Force

Write-Host "   ? README creado" -ForegroundColor Green

Write-Host ""
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "  ? PAQUETE DE INSTALACIÓN CREADO" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "?? Ubicación: $installerDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Contenido:" -ForegroundColor Cyan
Get-ChildItem $installerDir | ForEach-Object {
    $size = if ($_.PSIsContainer) { "" } else { " ({0:N2} MB)" -f ($_.Length / 1MB) }
    Write-Host "   • $($_.Name)$size" -ForegroundColor White
}

Write-Host ""
Write-Host "?? OPCIONES PARA DISTRIBUIR:" -ForegroundColor Cyan
Write-Host ""
Write-Host "OPCIÓN 1: Comprimir la carpeta" -ForegroundColor Yellow
Write-Host "  1. Clic derecho en '$installerDir'" -ForegroundColor White
Write-Host "  2. Enviar a > Carpeta comprimida" -ForegroundColor White
Write-Host "  3. Renombrar a: CalandriaInstalador-v$Version.zip" -ForegroundColor White
Write-Host "  4. Distribuir el ZIP" -ForegroundColor White
Write-Host ""
Write-Host "OPCIÓN 2: Comprimir automáticamente ahora" -ForegroundColor Yellow

$compress = Read-Host "¿Comprimir la carpeta ahora? (S/N)"

if ($compress -match '^[Ss]$') {
    Write-Host ""
    Write-Host "?? Comprimiendo instalador..." -ForegroundColor Cyan
    
    $outputZip = ".\CalandriaInstalador-v$Version.zip"
    
    try {
        if (Test-Path $outputZip) {
            Remove-Item $outputZip -Force
        }
        
        Compress-Archive -Path "$installerDir\*" -DestinationPath $outputZip -Force
        
        if (Test-Path $outputZip) {
            $zipSize = (Get-Item $outputZip).Length / 1MB
            Write-Host ""
            Write-Host "? Instalador comprimido exitosamente!" -ForegroundColor Green
            Write-Host ""
            Write-Host "?? Archivo generado:" -ForegroundColor Cyan
            Write-Host "   Nombre: CalandriaInstalador-v$Version.zip" -ForegroundColor White
            Write-Host "   Tamaño: $($zipSize.ToString('F2')) MB" -ForegroundColor White
            Write-Host "   Ubicación: $outputZip" -ForegroundColor White
            Write-Host ""
            Write-Host "?? CÓMO DISTRIBUIR:" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "  1. Sube este ZIP a GitHub como asset (opcional)" -ForegroundColor White
            Write-Host "  2. O envíalo directamente a los usuarios" -ForegroundColor White
            Write-Host ""
            Write-Host "?? INSTRUCCIONES PARA USUARIOS:" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "  1. Descargar CalandriaInstalador-v$Version.zip" -ForegroundColor White
            Write-Host "  2. Extraer el ZIP" -ForegroundColor White
            Write-Host "  3. Ejecutar Instalar.bat" -ForegroundColor White
            Write-Host "  4. Seguir las instrucciones en pantalla" -ForegroundColor White
            Write-Host ""
            
            # Abrir ubicación
            $openLoc = Read-Host "¿Abrir ubicación del archivo? (S/N)"
            if ($openLoc -match '^[Ss]$') {
                Start-Process explorer.exe "/select,`"$outputZip`""
            }
        }
    } catch {
        Write-Host ""
        Write-Host "? Error al comprimir: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "  RESUMEN FINAL" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "? ARCHIVOS LISTOS PARA GITHUB RELEASE:" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1. CalandriaApp-v$Version.zip" -ForegroundColor Yellow
Write-Host "     ? Para actualización automática" -ForegroundColor Gray
Write-Host ""
Write-Host "  2. version.txt" -ForegroundColor Yellow
Write-Host "     ? Obligatorio para sistema de versiones" -ForegroundColor Gray
Write-Host ""
Write-Host "  3. CalandriaInstalador-v$Version.zip (OPCIONAL)" -ForegroundColor Yellow
Write-Host "     ? Instalador completo para nuevos usuarios" -ForegroundColor Gray
Write-Host ""
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "Presiona Enter para finalizar..." -ForegroundColor Gray
Read-Host
