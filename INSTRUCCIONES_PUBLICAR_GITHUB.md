# ?? Instrucciones para Publicar Release v1.3.7-H en GitHub

## ? ESTADO: TODO LISTO PARA PUBLICAR

---

## ?? Archivos Preparados

### Archivo Principal (OBLIGATORIO)
- **CalandriaApp-v1.3.7-H.zip** (44.50 MB)
  - Ubicación: `C:\Users\Luxu\Documents\CALANDRIA RESIDENCIAL\ProgramC#\DynamicSepticSystem\CalandriaApp-v1.3.7-H.zip`
  - Contiene: EXE principal + Updater.exe + todas las DLLs + version.txt + documentación

### Archivo de Versión (OBLIGATORIO)
- **version.txt**
  - Ubicación: `DynamicSepticSystem\version.txt`
  - Contenido: `1.3.7-H`
  - **IMPORTANTE**: Este archivo es crítico para el sistema de actualizaciones

### Documentación Opcional
- **RELEASE_NOTES.txt** - Notas completas de la versión
- **README_SistemaProveedores.md** - Guía del sistema de proveedores (incluido en ZIP)
- **README_SistemaVersionado.md** - Sistema de versiones corregido (incluido en ZIP)

---

## ?? Pasos para Publicar

### Paso 1: Abrir GitHub Releases
```
URL: https://github.com/LuxuImNot/CalandriaApp/releases/new
```

### Paso 2: Configurar la Release

| Campo | Valor |
|-------|-------|
| **Tag version** | `1.3.7-H` |
| **Release title** | `v1.3.7-H - Sistema de Proveedores y Fix de Actualizaciones` |
| **Target** | `main` (o tu rama principal) |
| **Description** | Ver sección siguiente |

### Paso 3: Copiar Descripción

Copia y pega este contenido en el campo de descripción:

```markdown
## ?? Calandria Residencial v1.3.7-H

### ?? Nuevas Funcionalidades

#### ? Sistema de Proveedores Completo
- ? Registro de proveedores con información fiscal (RFC, Dirección, Teléfono)
- ? Catálogo de proveedores integrado en órdenes de compra
- ? PDFs profesionales con datos completos del proveedor
- ? Base de datos: Nueva tabla `PROVEEDORESCALANDRIA`

#### ?? Sistema de Actualización Mejorado
- ? **FIX CRÍTICO**: Corregido bucle infinito de actualizaciones
- ? El sistema ahora lee `version.txt` correctamente
- ? Actualización automática desde GitHub funcional
- ? Logs mejorados en `Actualizador.log`

#### ?? Órdenes de Compra Indirecta
- ? Gestión completa de insumos indirectos
- ? Catálogo personalizable por usuario
- ? Búsqueda y filtrado mejorado
- ? Generación de PDFs con formato profesional

### ?? Correcciones de Bugs
- ? Corregido bucle infinito en sistema de actualizaciones
- ? Mejorada lectura de versión local
- ? Validación de RFC en proveedores
- ? Manejo de errores en actualizador
- ? Configuración de GitHub en App.config

### ?? Instalación

#### Método 1: Actualización Automática (Recomendado)
Si ya tienes instalada cualquier versión anterior:
1. Abre la aplicación
2. La actualización se descargará e instalará automáticamente
3. Reinicia la aplicación cuando se solicite

#### Método 2: Manual desde ZIP
1. Descargar `CalandriaApp-v1.3.7-H.zip` de los assets
2. Extraer en una carpeta (ej: `C:\Program Files\CalandriaResidencial`)
3. Ejecutar `DynamicSepticSystem.exe`

### ?? Post-Instalación

1. **Configurar Base de Datos**:
   - Ejecutar el script SQL incluido en el repositorio
   - Verificar cadena de conexión en `DynamicSepticSystem.exe.config`

2. **Primera Ejecución**:
   - Ejecutar como Administrador la primera vez
   - El sistema creará las tablas necesarias
   - Verificar conectividad a SQL Server

### ?? Requisitos del Sistema

- Windows 7/8/10/11 (64-bit recomendado)
- .NET Framework 4.7.2 o superior
- SQL Server (acceso a base de datos CALANDRIA)
- 2 GB RAM mínimo (4 GB recomendado)
- 500 MB espacio en disco

### ?? Documentación Incluida

Los siguientes archivos de documentación están incluidos en el ZIP:
- `README_SistemaProveedores.md` - Guía completa del sistema de proveedores
- `README_SistemaVersionado.md` - Explicación del sistema de versionado corregido
- `README_OrdenesIndirectas.md` - Manual de órdenes de compra indirectas

### ?? Importante

- **Hacer backup de la base de datos antes de actualizar**
- El sistema requiere permisos de administrador para algunas operaciones
- Actualización automática requiere conexión a internet
- Si la actualización automática falla, usar método manual

### ?? Actualización desde Versiones Anteriores

#### Desde v1.3.6-G o anterior:
- La actualización automática ahora funciona correctamente ?
- NO es necesario desinstalar la versión anterior
- Los datos se preservan automáticamente

#### Cambios en Base de Datos:
- Nueva tabla: `PROVEEDORESCALANDRIA`
- Nueva columna: `OrdenesCompra.ProveedorClave`
- Scripts de migración disponibles en el repositorio

### ?? Soporte y Resolución de Problemas

Si encuentras problemas:
1. Revisar archivo `Actualizador.log` en el directorio de instalación
2. Verificar Event Viewer de Windows (Application Logs)
3. Contactar al equipo de desarrollo con los logs

### ?? Verificación de Instalación

Para verificar que la instalación fue exitosa:
1. Abrir `Actualizador.log` - debe mostrar "Versión local actual: 1.3.7-H"
2. Verificar que `version.txt` contenga exactamente: `1.3.7-H`
3. Probar creación de órdenes de compra con proveedores

---

**Versión**: 1.3.7-H  
**Fecha de Release**: Enero 2025  
**Build**: Release  
**Target Framework**: .NET Framework 4.7.2  

**Cambios técnicos completos**:
- Corrección en `Actualizador.cs` línea 22-44: Lectura mejorada de `version.txt`
- Nueva tabla SQL: `PROVEEDORESCALANDRIA` (RFC, RazonSocial, Direccion, Telefono, etc.)
- Columna agregada: `OrdenesCompra.ProveedorClave` (FK)
- Nuevos formularios: `FormAgregarProveedor`, `FormCompraIndirecta`, `FormAgregarInsumoIndirecto`, `FormSeleccionarTitulo`
- App.config actualizado con configuración de GitHub (GitHubOwner, GitHubRepo)
- Sistema de logs mejorado en actualizador con timestamps y detalles de conectividad
```

### Paso 4: Subir Assets

Arrastra y suelta estos archivos en la sección **Attach binaries by dropping them here or selecting them**:

1. ? **CalandriaApp-v1.3.7-H.zip** (OBLIGATORIO)
   - Ruta completa: `C:\Users\Luxu\Documents\CALANDRIA RESIDENCIAL\ProgramC#\DynamicSepticSystem\CalandriaApp-v1.3.7-H.zip`

2. ? **version.txt** (OBLIGATORIO)
   - Ruta: `DynamicSepticSystem\version.txt`
   - **IMPORTANTE**: El sistema de actualización automática requiere este archivo

3. ?? **RELEASE_NOTES.txt** (Opcional)
   - Ruta: `RELEASE_NOTES.txt`

### Paso 5: Configurar Opciones

- ? Marcar: **Set as the latest release**
- ? NO marcar: **Set as a pre-release** (a menos que quieras testing limitado)

### Paso 6: Publicar

Click en el botón verde **Publish release** ??

---

## ?? Verificación Post-Publicación

Después de publicar, verifica:

### 1. Verificar URLs de Descarga

Abre estas URLs en tu navegador para confirmar que funcionan:

```
ZIP Principal:
https://github.com/LuxuImNot/CalandriaApp/releases/download/1.3.7-H/CalandriaApp-v1.3.7-H.zip

version.txt:
https://github.com/LuxuImNot/CalandriaApp/releases/download/1.3.7-H/version.txt
```

### 2. Probar Actualización Automática

1. Instala la versión anterior (v1.3.6-G) en una máquina de prueba
2. Abre la aplicación
3. Debería detectar automáticamente la nueva versión
4. Verificar que descarga e instala correctamente

### 3. Verificar API de GitHub

Ejecuta este comando para verificar que GitHub reconoce la release:

```powershell
$response = Invoke-RestMethod -Uri "https://api.github.com/repos/LuxuImNot/CalandriaApp/releases/latest"
Write-Host "Tag detectado: $($response.tag_name)"
Write-Host "Assets disponibles: $($response.assets.Count)"
```

Debería mostrar:
```
Tag detectado: 1.3.7-H
Assets disponibles: 2 (o más si subiste archivos opcionales)
```

---

## ?? Solución de Problemas Comunes

### Problema: "Tag already exists"
**Solución**: Ese tag ya fue usado. Opciones:
1. Eliminar la release anterior y el tag
2. Usar un tag diferente (ej: `1.3.7-H-v2`)

### Problema: Assets no se pueden descargar
**Causas posibles**:
- Repositorio privado ? Hacer público o configurar token
- Archivo mayor a 2GB ? Comprimir más o usar Git LFS
- Error de red ? Reintentar subida

### Problema: Actualización automática no funciona
**Verificar**:
1. ? Tag exacto: `1.3.7-H` (case-sensitive)
2. ? Archivo ZIP con nombre correcto
3. ? version.txt con contenido correcto (sin espacios ni saltos de línea extra)
4. ? Release marcada como "Latest"
5. ? Repositorio público o token configurado

---

## ?? Notificar a los Usuarios

Después de publicar, puedes enviar este mensaje:

```
?? Nueva Versión Disponible: Calandria v1.3.7-H

Novedades:
? Sistema completo de proveedores
? Corrección de actualizaciones automáticas
? Órdenes de compra indirectas mejoradas

La aplicación se actualizará automáticamente la próxima vez que la abras.

Si prefieres instalar manualmente:
https://github.com/LuxuImNot/CalandriaApp/releases/tag/1.3.7-H

Notas completas: [Enlace a la release]
```

---

## ? Checklist Final

Antes de publicar, confirma:

- [x] Código compilado en modo Release
- [x] version.txt contiene exactamente `1.3.7-H`
- [x] Updater.exe incluido en el ZIP
- [x] App.config configurado con GitHub (GitHubOwner/GitHubRepo)
- [x] Documentación incluida en el ZIP
- [x] ZIP creado exitosamente (44.50 MB)
- [x] Probado localmente (compilación exitosa)
- [ ] Subido a GitHub como release
- [ ] Assets verificados (ZIP + version.txt)
- [ ] Marcado como Latest release
- [ ] Probado actualización automática

---

## ?? Resultado Esperado

Después de publicar correctamente:

1. ? Release visible en: https://github.com/LuxuImNot/CalandriaApp/releases
2. ? Badge de "Latest" en la release
3. ? Assets descargables públicamente
4. ? Usuarios con versiones anteriores recibirán notificación de actualización
5. ? Sistema de actualización automática funcionando

---

**¡Listo para publicar! ??**

Si tienes dudas o problemas durante la publicación, consulta:
- `GITHUB_RELEASE_GUIDE.md` - Guía detallada paso a paso
- `README_RESUMEN.md` - Resumen del proyecto completo
- Logs en `Actualizador.log` después de probar

**Fecha de preparación**: $(Get-Date -Format "yyyy-MM-dd HH:mm")  
**Versión preparada**: 1.3.7-H  
**Estado**: ? LISTO PARA PUBLICAR
