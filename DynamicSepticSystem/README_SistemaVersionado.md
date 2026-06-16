# Sistema de Versionado - SOLUCIÓN AL BUCLE INFINITO

## ?? Problema Detectado

El sistema entraba en un **bucle infinito de actualizaciones** porque:

1. `Actualizador.cs` tenía la versión hardcodeada: `public static string VersionLocal => "1.3.6-G";`
2. Al actualizar, el Updater.exe copiaba correctamente el `version.txt` con la nueva versión
3. **PERO** el código seguía usando la versión hardcodeada para comparar
4. Resultado: Siempre detectaba que había una "nueva" versión disponible

### Comportamiento Anterior (? INCORRECTO)

```
Inicio de app ? Lee versión: "1.3.6-G" (hardcode)
                     ?
Verifica GitHub ? Versión remota: "1.3.6-G"
                     ?
Compara: "1.3.6-G" vs "1.3.6-G" ? (pero ambas son hardcode)
                     ?
Descarga y actualiza ? Escribe version.txt: "1.3.6-G"
                     ?
Reinicia app ? Lee versión: "1.3.6-G" (sigue leyendo hardcode)
                     ?
BUCLE INFINITO ??
```

## ? Solución Implementada

Se modificó `Actualizador.cs` para que:

1. **Primero intente leer** `version.txt` del directorio de instalación
2. **Si no existe o falla**, use la versión hardcodeada como fallback
3. **Después de actualizar**, el Updater.exe escribe el nuevo `version.txt`
4. **Al reiniciar**, la app lee el `version.txt` actualizado

### Comportamiento Nuevo (? CORRECTO)

```
Inicio de app ? Lee version.txt: "1.3.6-G"
                     ?
Verifica GitHub ? Versión remota: "1.3.7-H"
                     ?
Compara: "1.3.6-G" vs "1.3.7-H" ? (1.3.7 es mayor)
                     ?
Descarga y actualiza ? Escribe version.txt: "1.3.7-H"
                     ?
Reinicia app ? Lee version.txt: "1.3.7-H"
                     ?
Verifica GitHub ? Versión remota: "1.3.7-H"
                     ?
Compara: "1.3.7-H" vs "1.3.7-H" ? (son iguales, no actualiza)
                     ?
FIN ?
```

## ?? Código Modificado

### Antes:
```csharp
public static string VersionLocal => "1.3.6-G";
```

### Después:
```csharp
// Versión hardcodeada como fallback (actualizar manualmente con cada release)
private static readonly string VersionHardcoded = "1.3.6-G";

// Lee la versión desde version.txt si existe, sino usa la hardcodeada
public static string VersionLocal
{
    get
    {
        try
        {
            var versionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.txt");
            if (File.Exists(versionFile))
            {
                var content = File.ReadAllText(versionFile).Trim();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    Log($"Versión leída desde version.txt: {content}");
                    return content;
                }
            }
        }
        catch (Exception ex)
        {
            Log($"Error leyendo version.txt local: {ex.Message}");
        }
        
        Log($"Usando versión hardcodeada: {VersionHardcoded}");
        return VersionHardcoded;
    }
}
```

## ?? Cómo Usar el Sistema de Versiones

### Para Desarrolladores: Crear Nueva Versión

1. **Actualizar versión hardcodeada** en `Actualizador.cs`:
   ```csharp
   private static readonly string VersionHardcoded = "1.3.7-H"; // <-- Cambiar aquí
   ```

2. **Actualizar `AssemblyInfo.cs`**:
   ```csharp
   [assembly: AssemblyVersion("1.3.7.0")]
   [assembly: AssemblyFileVersion("1.3.7.0")]
   ```

3. **Crear archivo `version.txt`** en la raíz del proyecto con el contenido:
   ```
   1.3.7-H
   ```

4. **Compilar el proyecto** en modo Release

5. **Crear release en GitHub** con tag `1.3.7-H` y subir:
   - El ZIP compilado (con todos los archivos)
   - El archivo `version.txt` como asset adicional

### Para Usuarios: Actualizar la Aplicación

1. **Abrir la aplicación**
2. El sistema verifica automáticamente si hay actualizaciones
3. **O hacer clic en "Probar Actualización"** (solo visible para admin)
4. Si hay actualización disponible:
   - Clic en "Sí" para aceptar
   - Aceptar permisos de administrador (UAC)
   - La aplicación se cierra automáticamente
   - El Updater.exe aplica los cambios
   - La aplicación se reinicia con la nueva versión

## ?? Flujo de Archivos

```
Instalación Local
??? DynamicSepticSystem.exe
??? version.txt               ? Se lee aquí primero
??? Actualizador.dll
??? ... otros archivos

GitHub Release
??? tag: 1.3.7-H
??? Assets:
?   ??? CalandriaApp.zip     ? Contiene todos los archivos
?   ??? version.txt          ? Versión para verificación
```

## ?? Verificar Versión Actual

Para saber qué versión está corriendo:

1. **Desde la aplicación**: Revisar el log `Actualizador.log`
2. **Desde el archivo**: Abrir `version.txt` en la carpeta de instalación
3. **Desde el código**: Variable `Actualizador.VersionLocal`

### Ejemplo de Log:
```
[2024-01-15 14:30:25.123] Versión leída desde version.txt: 1.3.7-H
[2024-01-15 14:30:25.456] Versión remota detectada: 1.3.7-H
[2024-01-15 14:30:25.789] ¿Es nueva versión? False (Local: 1.3.7-H, Remota: 1.3.7-H)
[2024-01-15 14:30:25.999] No hay actualización necesaria
```

## ?? Solución de Problemas

### Problema: Sigue en bucle infinito

**Causa**: El `version.txt` local no se está actualizando correctamente

**Solución**:
1. Cerrar la aplicación completamente
2. Ir a la carpeta de instalación
3. Verificar que existe `version.txt`
4. Abrir `version.txt` y verificar que tiene la versión correcta
5. Si no existe o está vacío, crear uno manualmente con la versión actual
6. Reiniciar la aplicación

### Problema: No detecta actualizaciones nuevas

**Causa**: La versión hardcodeada es mayor que la remota

**Solución**:
1. Verificar en `Actualizador.cs` la línea:
   ```csharp
   private static readonly string VersionHardcoded = "X.X.X-X";
   ```
2. Asegurar que coincida con el `version.txt` local
3. Recompilar si es necesario

### Problema: Error al leer version.txt

**Causa**: Permisos insuficientes o archivo corrupto

**Solución**:
1. Ejecutar la aplicación como Administrador
2. O eliminar `version.txt` para forzar uso del hardcode
3. Revisar `Actualizador.log` para ver el error exacto

## ?? Notas Importantes

1. **El hardcode es el FALLBACK**, no la fuente principal
2. **Siempre incluir `version.txt`** en el ZIP de la release
3. **El Updater.exe debe copiar `version.txt`** (ya lo hace)
4. **Los logs son tu mejor amigo** - revisar `Actualizador.log` siempre
5. **Formato de versión**: `MAJOR.MINOR.PATCH-LETRA` (ej: `1.3.6-G`)

## ?? Convención de Versionado

- **MAJOR**: Cambios importantes/breaking changes
- **MINOR**: Nuevas funcionalidades
- **PATCH**: Correcciones de bugs
- **LETRA**: Identificador de release (A, B, C... Z)

### Ejemplos:
- `1.3.6-G` ? Sistema de proveedores mejorado
- `1.3.7-H` ? Corrección de bucle de actualizaciones
- `1.4.0-A` ? Nueva funcionalidad mayor
- `2.0.0-A` ? Rediseño completo (breaking changes)

---

**Fecha de corrección**: 15 de Enero, 2024  
**Versión corregida**: 1.3.7-H (próxima release)  
**Problema resuelto**: Bucle infinito de actualizaciones ?
