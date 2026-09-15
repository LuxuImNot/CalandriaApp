# ?? Sistema de Release para Calandria v1.3.7-H

## ? ARCHIVOS CREADOS EN TU WORKSPACE

### ?? Scripts de Compilación
1. **BuildRelease.bat** - Script de compilación (Windows CMD)
2. **BuildRelease.ps1** - Script de compilación (PowerShell) - MÁS COMPLETO ?
3. **PrepararRelease.bat** - Script todo-en-uno para preparar release

### ?? Documentación
4. **RELEASE_NOTES.txt** - Notas de la versión para usuarios
5. **GITHUB_RELEASE_GUIDE.md** - Guía paso a paso para GitHub Release
6. **README_RESUMEN.md** - Este archivo

### ??? Scripts de Instalación
7. **Install.ps1** - Instalador PowerShell para clientes

### ?? Ya Existentes (Actualizados)
- ? `DynamicSepticSystem\version.txt` ? `1.3.7-H`
- ? `DynamicSepticSystem\Properties\AssemblyInfo.cs` ? v1.3.7.0
- ? `DynamicSepticSystem\Actualizador.cs` ? Corregido (sin duplicación)
- ? `DynamicSepticSystem\README_SistemaProveedores.md`
- ? `DynamicSepticSystem\README_SistemaVersionado.md`
- ? `DynamicSepticSystem\SQL_VerificarProveedores.sql`

---

## ?? CÓMO USAR - PROCESO COMPLETO

### Opción A: Script Todo-en-Uno (RECOMENDADO)

```cmd
# Ejecuta desde la raíz del proyecto
PrepararRelease.bat
```

Esto hará:
1. ? Compilar todo en Release
2. ? Crear ZIP de distribución
3. ? Copiar archivos necesarios a `Release_v1.3.7-H\`
4. ? Generar checksums
5. ? Dejarte listo para subir a GitHub

**Resultado**: Carpeta `Release_v1.3.7-H\` con todo lo necesario

---

### Opción B: Paso a Paso

#### 1?? Compilar
```powershell
# PowerShell (recomendado)
.\BuildRelease.ps1

# O CMD
BuildRelease.bat
```

#### 2?? Verificar
```powershell
# Verificar que existan:
dir DynamicSepticSystem\bin\Release\
# Debe contener:
#  - DynamicSepticSystem.exe
#  - Updater.exe
#  - version.txt (con "1.3.7-H")
#  - Todas las DLLs

# Verificar el ZIP
dir CalandriaApp-v1.3.7-H.zip
```

#### 3?? Compilar Setup1 (Opcional)
```
1. Abrir Visual Studio
2. Abrir Setup1\Setup1.vdproj
3. Cambiar a Release
4. Build ? Rebuild Setup1
5. El MSI estará en Setup1\Release\Setup1.msi
```

#### 4?? Subir a GitHub
```
1. Ir a: https://github.com/LuxuImNot/CalandriaApp/releases/new
2. Tag: 1.3.7-H
3. Title: v1.3.7-H - Sistema de Proveedores y Fix de Actualizaciones
4. Subir archivos:
   - CalandriaApp-v1.3.7-H.zip ? OBLIGATORIO
   - version.txt ? OBLIGATORIO
   - Setup1.msi (opcional)
   - RELEASE_NOTES.txt
   - SQL_VerificarProveedores.sql
   - Todos los README
5. Copiar descripción de GITHUB_RELEASE_GUIDE.md
6. Marcar "Latest release"
7. Publish!
```

---

## ?? CHECKLIST PRE-RELEASE

### Antes de Compilar
- [ ] Código sin errores de compilación
- [ ] `Actualizador.cs` tiene `VersionHardcoded = "1.3.7-H"`
- [ ] `AssemblyInfo.cs` tiene `AssemblyVersion("1.3.7.0")`
- [ ] `version.txt` contiene `1.3.7-H`
- [ ] Base de datos con tabla `PROVEEDORESCALANDRIA`

### Después de Compilar
- [ ] `DynamicSepticSystem.exe` existe en bin\Release
- [ ] `Updater.exe` existe en bin\Release
- [ ] `version.txt` copiado a bin\Release
- [ ] ZIP creado: `CalandriaApp-v1.3.7-H.zip`
- [ ] Probado en máquina limpia (si es posible)

### GitHub Release
- [ ] Tag es `1.3.7-H` (exacto, case-sensitive)
- [ ] ZIP subido como asset
- [ ] version.txt subido como asset
- [ ] Descripción completa (copia de GITHUB_RELEASE_GUIDE.md)
- [ ] Marcado como "Latest release"
- [ ] URL del ZIP accesible

### Post-Release
- [ ] Probar descarga del ZIP desde GitHub
- [ ] Probar actualización automática desde v1.3.6-G
- [ ] Verificar que version.txt se descarga correctamente
- [ ] Notificar a usuarios

---

## ?? ESTRUCTURA DE ARCHIVOS GENERADOS

```
?? Release_v1.3.7-H\
??? ?? CalandriaApp-v1.3.7-H.zip ? (para GitHub)
??? ?? version.txt ? (para GitHub)
??? ?? CalandriaSetup-v1.3.7-H.msi (opcional)
??? ?? RELEASE_NOTES.txt
??? ?? GITHUB_RELEASE_GUIDE.md
??? ?? README_SistemaProveedores.md
??? ?? README_SistemaVersionado.md
??? ?? README_OrdenesIndirectas.md
??? ?? SQL_VerificarProveedores.sql
??? ?? SQL_VerificarComprasIndirectas.sql
??? ??? Install.ps1
??? ?? checksums.txt
```

---

## ?? VERIFICACIÓN POST-RELEASE

### Verificar URLs de GitHub

Después de publicar, estas URLs deben funcionar:

```
# ZIP principal
https://github.com/LuxuImNot/CalandriaApp/releases/download/1.3.7-H/CalandriaApp-v1.3.7-H.zip

# version.txt
https://github.com/LuxuImNot/CalandriaApp/releases/download/1.3.7-H/version.txt

# API de GitHub (debe mostrar la release)
https://api.github.com/repos/LuxuImNot/CalandriaApp/releases/latest
```

### Probar Actualización Automática

1. Instalar versión anterior (v1.3.6-G)
2. Abrir la aplicación
3. Debe detectar nueva versión automáticamente
4. O desde menú Admin ? Probar Actualización
5. Verificar que descarga y aplica correctamente

---

## ?? SOLUCIÓN DE PROBLEMAS

### "BuildRelease.ps1 no puede ejecutarse"
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### "msbuild no se encuentra"
```powershell
# Usar Developer Command Prompt for VS 2022
# O agregar msbuild al PATH
```

### "El ZIP está corrupto"
```powershell
# Crear manualmente
Compress-Archive -Path "DynamicSepticSystem\bin\Release\*" -DestinationPath "CalandriaApp-v1.3.7-H.zip" -CompressionLevel Optimal -Force
```

### "version.txt tiene formato incorrecto"
```powershell
# Sin salto de línea
"1.3.7-H" | Out-File -FilePath "version.txt" -Encoding ASCII -NoNewline
```

### "Actualización automática no funciona"

Verifica:
1. Tag de GitHub es exactamente `1.3.7-H`
2. Asset ZIP se llama `CalandriaApp-v1.3.7-H.zip`
3. Asset version.txt existe y contiene solo `1.3.7-H`
4. Release marcada como "Latest"
5. Repositorio es público o token configurado

---

## ?? COMANDOS ÚTILES

### Ver versión actual
```powershell
Get-Content DynamicSepticSystem\bin\Release\version.txt
```

### Verificar compilación
```powershell
dir DynamicSepticSystem\bin\Release\*.exe
```

### Limpiar todo y empezar de nuevo
```powershell
Remove-Item -Path "DynamicSepticSystem\bin\Release" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Updater\bin\Release" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "CalandriaApp-v1.3.7-H.zip" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Release_v1.3.7-H" -Recurse -Force -ErrorAction SilentlyContinue
```

### Probar actualización local
```powershell
# Ver logs de actualización
Get-Content "C:\Program Files\CalandriaResidencial\Actualizador.log" -Tail 50
```

---

## ? TODO LISTO!

Has preparado exitosamente:
- ? Sistema de compilación automatizado
- ? Documentación completa
- ? Scripts de instalación
- ? Guías para GitHub Release
- ? Corrección del bucle de actualizaciones

### ?? Siguiente Acción

```cmd
# Ejecuta esto:
PrepararRelease.bat
```

Luego sigue las instrucciones de `GITHUB_RELEASE_GUIDE.md`

---

**¡Buena suerte con la release! ??**

Version: 1.3.7-H
Fecha: Enero 2024
Sistema: Calandria Residencial
