# ?? Guía de Publicación - CalandriaApp

Esta guía explica cómo compilar y publicar una nueva versión de DynamicSepticSystem para GitHub Releases.

---

## ?? Índice

1. [Scripts Disponibles](#scripts-disponibles)
2. [Proceso de Publicación Completo](#proceso-de-publicación-completo)
3. [Solución de Problemas](#solución-de-problemas)
4. [Estructura del Paquete](#estructura-del-paquete)

---

## ??? Scripts Disponibles

### 1. `publish.bat` (Recomendado)
**Script completo de publicación automática**

- ? Busca automáticamente MSBuild o dotnet
- ? Restaura paquetes NuGet
- ? Compila el proyecto en modo Release
- ? Copia archivos binarios
- ? Copia carpeta Resources
- ? Copia Updater.exe
- ? Crea version.txt
- ? Genera archivo ZIP para distribución
- ? Opción para abrir GitHub Releases

**Uso:**
```cmd
.\publish.bat
```

### 2. `publish-simple.bat`
**Script con opciones manuales**

Útil si:
- MSBuild no está en el PATH
- Prefieres compilar desde Visual Studio
- Necesitas diagnosticar problemas

**Opciones:**
1. Compilar con MSBuild (requiere Developer Command Prompt)
2. Copiar archivos manualmente
3. Usar dotnet CLI

**Uso:**
```cmd
.\publish-simple.bat
```

### 3. `zip-only.bat`
**Solo empaqueta archivos ya compilados**

Útil si:
- Ya compilaste desde Visual Studio
- Solo necesitas crear el ZIP
- Quieres hacer pruebas rápidas

**Requisito previo:** Compilar en Visual Studio (Release mode)

**Uso:**
```cmd
.\zip-only.bat
```

---

## ?? Proceso de Publicación Completo

### Paso 1: Preparar el Código

1. **Actualizar versión en el código:**

   Edita `DynamicSepticSystem\Actualizador.cs`:
   ```csharp
   private static readonly string VersionHardcoded = "1.7.0-A"; // ?? Cambiar aquí
   ```

2. **Actualizar AssemblyInfo:**

   Edita `DynamicSepticSystem\Properties\AssemblyInfo.cs`:
   ```csharp
   [assembly: AssemblyVersion("1.7.0.0")]
   [assembly: AssemblyFileVersion("1.7.0.0")]
   ```

   Edita `Updater\Properties\AssemblyInfo.cs`:
   ```csharp
   [assembly: AssemblyVersion("1.7.0.0")]
   [assembly: AssemblyFileVersion("1.7.0.0")]
   ```

3. **Commit y Push:**
   ```cmd
   git add .
   git commit -m "Preparar versión 1.7.0-A"
   git push origin main
   ```

### Paso 2: Compilar y Empaquetar

**Opción A: Script Automático (Recomendado)**
```cmd
.\publish.bat
```

**Opción B: Desde Visual Studio + ZIP**
1. Abrir el proyecto en Visual Studio
2. Cambiar a configuración `Release`
3. Menu: `Build > Rebuild Solution`
4. Ejecutar: `.\zip-only.bat`

### Paso 3: Crear Release en GitHub

1. **Ir a GitHub Releases:**
   ```
   https://github.com/LuxuImNot/CalandriaApp/releases/new
   ```

2. **Configurar el Release:**
   - **Tag version**: `1.7.0-A`
   - **Release title**: `v1.7.0-A - [Descripción breve]`
   - **Description**: Descripción detallada de cambios

3. **Subir Archivos:**
   - ? Arrastrar el archivo ZIP generado (ej: `DynamicSepticSystem_v20250115-143000.zip`)
   - ? Crear archivo `version.txt` con contenido `1.7.0-A`
   - ? Subir `version.txt` como asset adicional

4. **Publicar:**
   - Marcar como "Latest release" si es la versión más reciente
   - Click en "Publish release"

### Paso 4: Verificar Actualización

1. **Probar en local:**
   - Abrir DynamicSepticSystem
   - Click en "Probar Actualización" (solo visible para admin)
   - Verificar que detecta la nueva versión

2. **Verificar en instalación de usuario:**
   - El sistema debería detectar automáticamente la actualización al iniciar

---

## ?? Solución de Problemas

### ? "MSBuild no encontrado"

**Solución 1: Usar Developer Command Prompt**
```cmd
1. Abrir "Developer Command Prompt for VS 2022"
2. Navegar a: cd "C:\...\DynamicSepticSystem"
3. Ejecutar: .\publish.bat
```

**Solución 2: Usar dotnet CLI**
```cmd
dotnet build -c Release
dotnet publish -c Release -o bin\Publish
.\zip-only.bat
```

**Solución 3: Compilar desde Visual Studio**
```cmd
1. Visual Studio > Build > Rebuild Solution
2. Ejecutar: .\zip-only.bat
```

### ? "No se encuentra Updater.exe"

**Verificar ubicación:**
```cmd
dir Updater\bin\Release\net472\Updater.exe
```

Si existe pero no se copia, editar `publish.bat` línea ~186:
```batch
if exist "Updater\bin\%BUILD_CONFIG%\net472\Updater.exe" (
```

### ? "Falta carpeta Resources en el ZIP"

**Verificar que existe:**
```cmd
dir DynamicSepticSystem\Resources
```

**Verificar que se copió:**
```cmd
dir bin\Publish\Resources
```

**Solución manual:**
```cmd
xcopy /s /i DynamicSepticSystem\Resources\* bin\Publish\Resources\
.\zip-only.bat
```

### ? "El ZIP está corrupto"

**Eliminar ZIP anterior y regenerar:**
```cmd
del DynamicSepticSystem_v*.zip
.\publish.bat
```

### ? "Error de compilación CS7034"

Problema con versión del Updater. Ya está corregido en `Updater\Properties\AssemblyInfo.cs`.

Si persiste:
```cmd
1. Cerrar Visual Studio
2. Eliminar carpetas: bin\ y obj\
3. Ejecutar: .\publish.bat
```

---

## ?? Estructura del Paquete

Después de ejecutar `publish.bat`, se crea la siguiente estructura:

```
bin\Publish\                          (Carpeta de publicación)
??? DynamicSepticSystem.exe          (Ejecutable principal)
??? DynamicSepticSystem.exe.config   (Configuración)
??? Updater.exe                       (Sistema de actualización)
??? version.txt                       (Versión actual)
??? Resources\                        (Recursos de la aplicación)
?   ??? Logo.png
?   ??? LogoCamaney.png
?   ??? exit.ico
?   ??? ...
??? *.dll                             (Bibliotecas)
??? ...

DynamicSepticSystem_v20250115-143000.zip  (Archivo para distribución)
```

### Contenido del ZIP para GitHub:

El ZIP debe contener:
- ? Todos los archivos .exe
- ? Todas las bibliotecas .dll
- ? Archivos de configuración .config
- ? Carpeta Resources completa
- ? version.txt
- ? Updater.exe

### Assets en GitHub Release:

- ? `DynamicSepticSystem_v20250115-143000.zip` (paquete completo)
- ? `version.txt` (archivo separado con la versión)

---

## ?? Convención de Versionado

Formato: `MAJOR.MINOR.PATCH-LETRA`

Ejemplos:
- `1.6.1-G` ? Versión actual
- `1.7.0-A` ? Nueva versión con funcionalidades
- `1.7.1-A` ? Corrección de bugs
- `2.0.0-A` ? Cambio mayor (breaking changes)

### Incrementar Versión:

| Tipo de Cambio | Incremento | Ejemplo |
|----------------|------------|---------|
| Corrección de bugs menores | PATCH | 1.6.1 ? 1.6.2 |
| Nueva funcionalidad menor | MINOR | 1.6.1 ? 1.7.0 |
| Cambio importante/breaking | MAJOR | 1.6.1 ? 2.0.0 |
| Release iterativa | LETRA | A ? B ? C |

---

## ?? Checklist Pre-Release

Antes de publicar, verificar:

- [ ] ? Versión actualizada en `Actualizador.cs`
- [ ] ? AssemblyVersion actualizado en ambos proyectos
- [ ] ? Código compilado sin errores
- [ ] ? Updater.exe incluido en el paquete
- [ ] ? Carpeta Resources incluida
- [ ] ? version.txt creado con versión correcta
- [ ] ? ZIP generado exitosamente
- [ ] ? Commit y push realizados
- [ ] ? Release creado en GitHub
- [ ] ? Archivos ZIP y version.txt subidos como assets
- [ ] ? Release marcado como "Latest"
- [ ] ? Actualización probada en local

---

## ?? Soporte

Si encuentras problemas:

1. **Revisar logs de compilación**
2. **Verificar que MSBuild/Visual Studio estén instalados**
3. **Consultar la sección de Solución de Problemas**
4. **Revisar issues en GitHub**

---

## ?? Historial de Cambios

### v1.7.0 (Enero 2025)
- ? Sistema de publicación automatizado
- ? Corrección de versiones en Updater
- ? Inclusión de carpeta Resources
- ? Scripts mejorados de publicación

---

**Última actualización:** 15 de Enero, 2025
**Versión de esta guía:** 1.0
