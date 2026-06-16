# ?? Guía para Crear Setup.exe - Calandria v1.3.7-H

## ?? Opciones Disponibles

Tienes **3 formas** de crear un instalador para Calandria:

---

## ? OPCIÓN 1: Setup.exe con IExpress (RECOMENDADO)

**Ventajas:**
- ? Ejecutable único (.exe)
- ? No requiere software adicional (usa herramienta de Windows)
- ? Interfaz gráfica profesional
- ? Fácil de distribuir

**Desventajas:**
- ?? Archivo grande (45+ MB)

### Pasos:
```powershell
# Ejecutar este script
.\CrearSetupExe.ps1

# El resultado será:
# CalandriaSetup-v1.3.7-H.exe
```

**Para distribuir:**
- Sube el archivo `.exe` a GitHub como asset
- Los usuarios descargan y ejecutan directamente
- UAC solicita permisos automáticamente

---

## ?? OPCIÓN 2: Instalador PowerShell Completo

**Ventajas:**
- ? Más control sobre el proceso
- ? Crea desinstalador automático
- ? Registra en Panel de Control

**Desventajas:**
- ?? Requiere PowerShell (incluido en Windows)
- ?? Los usuarios deben extraer ZIP primero

### Pasos:
```powershell
# Ejecutar este script
.\CrearInstalador.ps1

# El resultado será una carpeta 'Installer' con:
# - Install.ps1
# - payload.zip
# - Instalar.bat (para usuarios)
# - README_INSTALADOR.txt
```

**Para distribuir:**
- Comprimir carpeta `Installer` en ZIP
- Usuarios extraen y ejecutan `Instalar.bat`

---

## ?? OPCIÓN 3: Solo ZIP (Simple)

**Ventajas:**
- ? Archivo más pequeño
- ? Control total del usuario
- ? No requiere permisos de administrador

**Desventajas:**
- ?? Usuario debe copiar archivos manualmente
- ?? No crea accesos directos automáticamente
- ?? No se registra en Panel de Control

### Pasos:
```powershell
# Ya está creado:
CalandriaApp-v1.3.7-H.zip

# Incluir archivo Install.ps1 en el ZIP
Copy-Item "Install.ps1" -Destination ".\DynamicSepticSystem\bin\Release\"
Compress-Archive -Path ".\DynamicSepticSystem\bin\Release\*" -DestinationPath ".\CalandriaApp-v1.3.7-H.zip" -Force
```

**Para distribuir:**
- Sube el ZIP a GitHub
- Los usuarios extraen donde quieran
- Ejecutan DynamicSepticSystem.exe

---

## ?? Pasos Rápidos para Crear Setup.exe

### 1. Verificar Requisitos
```powershell
# Verificar que existe el ZIP
Test-Path ".\CalandriaApp-v1.3.7-H.zip"
# Debe devolver: True
```

### 2. Ejecutar Script
```powershell
# Método A: Doble clic en CrearSetupExe.ps1
# (clic derecho > Ejecutar con PowerShell)

# Método B: Desde PowerShell
.\CrearSetupExe.ps1
```

### 3. Esperar Compilación
- El proceso toma 1-2 minutos
- IExpress comprimirá todos los archivos
- Se mostrará progreso en pantalla

### 4. Verificar Resultado
```powershell
# Verificar que se creó el Setup.exe
Get-Item ".\CalandriaSetup-v1.3.7-H.exe" | Select-Object Name, @{Name="Tamaño (MB)";Expression={[math]::Round($_.Length/1MB,2)}}
```

---

## ?? Subir a GitHub Release

Una vez creado el Setup.exe:

### Archivos a Subir:

1. **CalandriaApp-v1.3.7-H.zip** (Obligatorio)
   - Para usuarios técnicos o actualización automática
   
2. **version.txt** (Obligatorio)
   - Para sistema de actualizaciones

3. **CalandriaSetup-v1.3.7-H.exe** (Opcional pero recomendado)
   - Para usuarios finales que prefieren instalador tradicional

4. **RELEASE_NOTES.txt** (Opcional)
   - Documentación de cambios

### Instrucciones en la Release:

```markdown
## ?? Descargar e Instalar

### Opción 1: Instalador (Recomendado para nuevos usuarios)
1. Descargar: `CalandriaSetup-v1.3.7-H.exe`
2. Ejecutar el archivo
3. Seguir el asistente de instalación
4. ¡Listo!

### Opción 2: ZIP Manual (Para usuarios avanzados)
1. Descargar: `CalandriaApp-v1.3.7-H.zip`
2. Extraer en una carpeta
3. Ejecutar `DynamicSepticSystem.exe`

### Opción 3: Actualización Automática (Para usuarios existentes)
- Abre tu versión anterior de Calandria
- La aplicación detectará y descargará la actualización automáticamente
```

---

## ?? Probar el Setup.exe

Antes de distribuir, prueba el instalador:

### 1. Probar Instalación
```powershell
# Ejecutar el Setup.exe
.\CalandriaSetup-v1.3.7-H.exe

# Verificar que:
# ? Se instala en C:\Program Files\CalandriaResidencial
# ? Se crea acceso directo en Escritorio
# ? Se crea acceso directo en Menú Inicio
# ? La aplicación ejecuta correctamente
```

### 2. Verificar Registro en Panel de Control
```
1. Abrir Panel de Control
2. Ir a Programas y características
3. Buscar "Calandria Residencial"
4. Verificar versión: 1.3.7-H
```

### 3. Probar Desinstalación
```
1. Panel de Control > Programas > Calandria Residencial > Desinstalar
2. O ejecutar: C:\Program Files\CalandriaResidencial\Uninstall.ps1
3. Verificar que se eliminan accesos directos
4. Verificar que se elimina de Panel de Control
```

---

## ?? Solución de Problemas

### Problema: "IExpress no encontrado"
**Causa:** Sistema operativo antiguo o corrupto
**Solución:** 
```powershell
# Verificar IExpress
Test-Path "$env:SystemRoot\System32\iexpress.exe"

# Si no existe, usar Opción 2 (Instalador PowerShell)
.\CrearInstalador.ps1
```

### Problema: "Setup.exe muy grande"
**Causa:** IExpress no comprime eficientemente
**Solución:** 
- Es normal (45-50 MB con todos los archivos)
- Alternativamente, distribuir solo el ZIP
- O usar WiX Toolset para crear MSI más eficiente

### Problema: Windows Defender bloquea Setup.exe
**Causa:** Ejecutable sin firma digital
**Solución:**
1. Hacer clic en "Más información"
2. Seleccionar "Ejecutar de todos modos"
3. O firmar digitalmente el ejecutable:
   ```powershell
   # Requiere certificado de código
   signtool.exe sign /f "certificado.pfx" /p "password" "CalandriaSetup-v1.3.7-H.exe"
   ```

### Problema: "Error al ejecutar Setup.exe"
**Causa:** Falta permisos de administrador
**Solución:**
```
1. Clic derecho en Setup.exe
2. Seleccionar "Ejecutar como administrador"
```

---

## ?? Comparación de Métodos

| Característica | Setup.exe | Instalador PS | Solo ZIP |
|----------------|-----------|---------------|----------|
| Fácil de usar | ????? | ???? | ??? |
| Tamaño | 45-50 MB | 45 MB + scripts | 44 MB |
| Requiere admin | Sí | Sí | No |
| Accesos directos | Sí | Sí | No |
| Desinstalador | Sí | Sí | No |
| Panel de Control | Sí | Sí | No |
| Actualización auto | Sí | Sí | Sí |

---

## ? Checklist Final

Antes de distribuir el Setup.exe:

- [ ] Setup.exe creado correctamente
- [ ] Tamaño del archivo razonable (< 60 MB)
- [ ] Probado en máquina limpia o VM
- [ ] Instalación exitosa con permisos de admin
- [ ] Accesos directos creados correctamente
- [ ] Aplicación ejecuta sin errores
- [ ] Registrado en Panel de Control
- [ ] Desinstalación funciona correctamente
- [ ] Windows Defender no lo marca como virus
- [ ] README actualizado con instrucciones

---

## ?? Recomendación Final

**Para GitHub Release, sube ambos:**

1. ? **CalandriaSetup-v1.3.7-H.exe** - Para usuarios finales
2. ? **CalandriaApp-v1.3.7-H.zip** - Para actualizaciones automáticas
3. ? **version.txt** - Para sistema de versiones

Así cubres todos los casos de uso:
- Nuevos usuarios ? Setup.exe
- Usuarios técnicos ? ZIP
- Usuarios existentes ? Actualización automática

---

## ?? Si tienes problemas

Si ninguno de estos métodos funciona:
1. Considera usar **NSIS** (Nullsoft Scriptable Install System)
2. O **Inno Setup** (instalador gratuito muy popular)
3. O **WiX Toolset** (crea MSI profesionales)

Para instalar cualquiera de estos:
```powershell
# NSIS
choco install nsis

# Inno Setup
choco install innosetup

# WiX
choco install wixtoolset
```

---

**Versión**: 1.3.7-H  
**Fecha**: Enero 2025  
**Autor**: Calandria Development Team
