# ?? INSTALADOR CALANDRIA v1.3.7-H

## ?? UBICACIÓN

**Archivo**: `CalandriaInstalador-v1.3.7-H.zip`  
**Tamaño**: 44.02 MB  
**Ubicación**: En la carpeta raíz del proyecto

---

## ?? CÓMO USAR EL INSTALADOR

### Para Distribuir a Usuarios:

1. **Envía el archivo**:
   - `CalandriaInstalador-v1.3.7-H.zip`

2. **Instrucciones para el usuario**:
   ```
   1. Descargar CalandriaInstalador-v1.3.7-H.zip
   2. Extraer el ZIP en cualquier carpeta
   3. Hacer doble clic en "Instalar.bat"
   4. Seguir las instrucciones en pantalla
   ```

---

## ?? QUÉ CONTIENE EL INSTALADOR

### Archivos Incluidos:

1. **Instalar.bat** (Mejorado)
   - 3 métodos de extracción para máxima compatibilidad
   - Manejo robusto de errores
   - Solicita permisos de administrador automáticamente

2. **CalandriaApp.zip**
   - Todos los archivos de la aplicación
   - DynamicSepticSystem.exe
   - Updater.exe
   - Todas las DLLs necesarias

3. **README.txt**
   - Instrucciones completas
   - Requisitos del sistema
   - Solución de problemas

---

## ? CARACTERÍSTICAS DEL INSTALADOR

### ? Funcionalidades:

- **Instalación guiada**: El usuario solo sigue el asistente
- **Múltiples métodos**: 3 técnicas diferentes de extracción
- **Manejo de errores**: Si un método falla, prueba el siguiente
- **Accesos directos**: Crea automáticamente en Escritorio y Menú Inicio
- **Registro en sistema**: Aparece en Panel de Control > Programas
- **Elevación UAC**: Solicita permisos de administrador si es necesario
- **Ubicación personalizable**: El usuario puede elegir dónde instalar

### ?? Métodos de Extracción (en orden):

1. **PowerShell Expand-Archive** (Windows 10+)
2. **System.IO.Compression** (Windows 7+)
3. **VBScript Shell.Application** (Máxima compatibilidad)

Si los 3 fallan, muestra instrucciones claras al usuario.

---

## ?? FLUJO DE INSTALACIÓN

### Lo que hace el instalador:

```
1. Verifica permisos de administrador
2. Solicita directorio de instalación
   (Por defecto: C:\Program Files\CalandriaResidencial)
3. Extrae archivos con el mejor método disponible
4. Crea acceso directo en el Escritorio
5. Crea acceso directo en el Menú Inicio
6. Registra en Panel de Control (si tiene permisos admin)
7. Ofrece ejecutar la aplicación inmediatamente
```

---

## ?? PARA SUBIR A GITHUB

### Como Asset en la Release:

Este instalador es **OPCIONAL** pero **RECOMENDADO** para:
- ? Nuevos usuarios que no conocen la aplicación
- ? Usuarios finales no técnicos
- ? Instalaciones corporativas/empresariales
- ? Usuarios que prefieren instaladores tradicionales

### Instrucciones en la Release:

```markdown
### ¿Eres nuevo usuario?
?? Descarga: `CalandriaInstalador-v1.3.7-H.zip`
- Extraer y ejecutar `Instalar.bat`
- El asistente te guiará paso a paso
- No requiere conocimientos técnicos
```

---

## ?? DIFERENCIA CON EL ZIP NORMAL

### CalandriaApp-v1.3.7-H.zip (ZIP Normal)
- ? Para actualización automática
- ? Para usuarios técnicos
- ? Solo extraer y ejecutar
- ? No crea accesos directos
- ? No registra en sistema

### CalandriaInstalador-v1.3.7-H.zip (Este)
- ? Instalación guiada
- ? Crea accesos directos
- ? Registra en Panel de Control
- ? Solicita permisos automáticamente
- ? Mejor para nuevos usuarios

---

## ?? CÓMO PROBAR EL INSTALADOR

### Antes de distribuir:

1. **Extraer el ZIP**:
   ```
   Descomprimir CalandriaInstalador-v1.3.7-H.zip
   ```

2. **Ejecutar Instalar.bat**:
   ```
   Doble clic en Instalar.bat
   ```

3. **Verificar**:
   - ? Se instaló en la ubicación correcta
   - ? Se creó acceso directo en Escritorio
   - ? Se creó acceso directo en Menú Inicio
   - ? La aplicación ejecuta correctamente
   - ? Aparece en Panel de Control > Programas

4. **Probar desinstalación**:
   ```
   Panel de Control > Programas > Calandria Residencial > Desinstalar
   ```

---

## ?? CONTENIDO DEL README.txt

El instalador incluye un README.txt con:

- ?? Instrucciones detalladas de instalación
- ?? Requisitos del sistema
- ?? Solución de problemas comunes
- ??? Instrucciones de desinstalación
- ?? Información de soporte

---

## ?? SOLUCIÓN DE PROBLEMAS

### Si el instalador falla:

**Problema**: "No se pueden extraer los archivos"

**Soluciones**:
1. Ejecutar `Instalar.bat` como Administrador
   - Clic derecho > Ejecutar como administrador
   
2. Verificar .NET Framework 4.7.2+ instalado
   - Descargar desde: https://dotnet.microsoft.com/download/dotnet-framework

3. Extraer manualmente:
   - Extraer `CalandriaApp.zip` del instalador
   - Copiar contenido a `C:\Program Files\CalandriaResidencial`

---

## ?? RECOMENDACIÓN FINAL

### Para la Release de GitHub:

**Sube AMBOS archivos**:

1. **CalandriaApp-v1.3.7-H.zip** (Obligatorio)
   - Para actualización automática
   - Para usuarios técnicos

2. **CalandriaInstalador-v1.3.7-H.zip** (Este - Recomendado)
   - Para nuevos usuarios
   - Experiencia profesional

Así cubres todos los casos de uso y tipos de usuarios.

---

## ?? RESUMEN

- ? **Archivo**: CalandriaInstalador-v1.3.7-H.zip
- ? **Tamaño**: 44.02 MB
- ? **Ubicación**: Carpeta raíz del proyecto
- ? **Listo para**: Distribuir o subir a GitHub
- ? **Estado**: Verificado y funcional

---

**Preparado**: Enero 2025  
**Versión**: 1.3.7-H  
**Autor**: Calandria Development Team
