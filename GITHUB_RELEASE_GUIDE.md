# Guía para Crear GitHub Release v1.3.7-H

## ?? Pasos para Publicar la Release

### 1. Preparar los Archivos

Ejecuta el script de compilación:

```powershell
# Opción A: PowerShell (Recomendado)
.\BuildRelease.ps1

# Opción B: Batch
.\BuildRelease.bat
```

Esto generará:
- `CalandriaApp-v1.3.7-H.zip` - Paquete completo
- `DynamicSepticSystem\bin\Release\` - Archivos compilados

### 2. Verificar Archivos Críticos

Antes de subir, verifica que existan:
- ? `DynamicSepticSystem.exe`
- ? `Updater.exe`
- ? `version.txt` (con contenido: `1.3.7-H`)
- ? `App.config`
- ? Todas las DLLs necesarias

### 3. Crear Release en GitHub

#### 3.1 Ir a GitHub Releases
1. Abre tu repositorio: https://github.com/LuxuImNot/CalandriaApp
2. Click en **Releases** (lado derecho)
3. Click en **Create a new release**

#### 3.2 Configurar la Release
```
Tag version:    1.3.7-H
Release title:  v1.3.7-H - Sistema de Proveedores y Fix de Actualizaciones
Target:         main (o tu rama principal)
```

#### 3.3 Descripción de la Release

Copia y pega esto en el campo de descripción:

```markdown
## ?? Calandria Residencial v1.3.7-H

### ?? Nuevas Funcionalidades

#### ? Sistema de Proveedores Completo
- Registro de proveedores con información fiscal (RFC, Dirección, Teléfono)
- Catálogo de proveedores integrado en órdenes de compra
- PDFs profesionales con datos completos del proveedor
- Base de datos: Nueva tabla `PROVEEDORESCALANDRIA`

#### ?? Sistema de Actualización Mejorado
- **FIX CRÍTICO**: Corregido bucle infinito de actualizaciones
- El sistema ahora lee `version.txt` correctamente
- Actualización automática desde GitHub funcional
- Logs mejorados en `Actualizador.log`

#### ?? Órdenes de Compra Indirecta
- Gestión completa de insumos indirectos
- Catálogo personalizable por usuario
- Búsqueda y filtrado mejorado
- Generación de PDFs con formato profesional

### ?? Correcciones de Bugs
- ? Corregido bucle infinito en sistema de actualizaciones
- ? Mejorada lectura de versión local
- ? Validación de RFC en proveedores
- ? Manejo de errores en actualizador

### ?? Instalación

#### Método 1: Instalador MSI (Recomendado)
1. Descargar `Setup1.msi` de los assets
2. Ejecutar como Administrador
3. Seguir el asistente de instalación

#### Método 2: Manual desde ZIP
1. Descargar `CalandriaApp-v1.3.7-H.zip`
2. Extraer en una carpeta
3. Ejecutar `Install.ps1` como Administrador
4. O copiar manualmente a `C:\Program Files\CalandriaResidencial`

### ?? Post-Instalación

1. **Configurar Base de Datos**:
   - Ejecutar `SQL_VerificarProveedores.sql` en SQL Server
   - Verificar cadena de conexión en `App.config`

2. **Proveedores de Prueba**:
   - El script SQL incluye 5 proveedores de ejemplo
   - Puedes eliminarlos o editarlos según necesites

3. **Primera Ejecución**:
   - Ejecutar como Administrador la primera vez
   - El sistema creará las tablas necesarias
   - Verificar conectividad a SQL Server

### ?? Requisitos

- Windows 7/8/10/11
- .NET Framework 4.7.2+
- SQL Server (acceso a base de datos CALANDRIA)
- 2 GB RAM mínimo
- 500 MB espacio en disco

### ?? Documentación

Ver archivos incluidos:
- `RELEASE_NOTES.txt` - Notas completas de la versión
- `README_SistemaProveedores.md` - Guía del sistema de proveedores
- `README_SistemaVersionado.md` - Sistema de versiones corregido
- `README_OrdenesIndirectas.md` - Manual de órdenes indirectas

### ?? Importante

- **Hacer backup de la base de datos antes de actualizar**
- El sistema requiere permisos de administrador
- Actualización automática solo funciona con conexión a internet

### ?? Actualización desde v1.3.6-G

Si tienes la versión anterior:
1. La aplicación se actualizará automáticamente (ahora sí funciona ?)
2. O instalar manualmente esta versión
3. NO es necesario desinstalar la versión anterior

### ?? Soporte

Para problemas:
1. Revisar `Actualizador.log`
2. Verificar Event Viewer de Windows
3. Contactar al equipo de desarrollo

---

**Cambios técnicos**:
- Corrección en `Actualizador.cs` - Lectura de `version.txt` local
- Nueva tabla `PROVEEDORESCALANDRIA` en SQL Server
- Columna `ProveedorClave` en tabla `OrdenesCompra`
- Nuevos formularios: `FormAgregarProveedor`, `FormCompraIndirecta`
```

#### 3.4 Subir Assets

En la sección **Attach binaries**, arrastra y suelta estos archivos:

1. **Obligatorios**:
   - ? `CalandriaApp-v1.3.7-H.zip` (generado por BuildRelease.ps1)
   - ? `version.txt` (debe contener solo: `1.3.7-H`)

2. **Opcionales pero recomendados**:
   - `Setup1.msi` (si lo compilaste en Visual Studio)
   - `RELEASE_NOTES.txt`
   - `README_SistemaProveedores.md`
   - `SQL_VerificarProveedores.sql`

#### 3.5 Publicar

1. Marca como **Latest release** ?
2. Si quieres que solo ciertos usuarios lo vean primero, marca **Pre-release**
3. Click en **Publish release**

### 4. Verificar la Release

Después de publicar:

1. **Verificar que el ZIP está disponible**:
   ```
   https://github.com/LuxuImNot/CalandriaApp/releases/download/1.3.7-H/CalandriaApp-v1.3.7-H.zip
   ```

2. **Verificar version.txt**:
   ```
   https://github.com/LuxuImNot/CalandriaApp/releases/download/1.3.7-H/version.txt
   ```

3. **Probar actualización automática**:
   - Abre la app anterior (v1.3.6-G)
   - Debería detectar la nueva versión
   - Descargar e instalar automáticamente

### 5. Notificar a los Usuarios

Puedes enviar este mensaje:

```
?? Nueva Versión Disponible: v1.3.7-H

Se ha lanzado una nueva versión de Calandria Residencial con:
? Sistema completo de proveedores
? Corrección de actualizaciones automáticas
? Órdenes de compra indirectas

La actualización es automática. Solo abre la aplicación y 
se actualizará desde GitHub.

Si prefieres instalar manualmente:
https://github.com/LuxuImNot/CalandriaApp/releases/tag/1.3.7-H

Notas de la versión:
[Enlace a la release]
```

---

## ?? Solución de Problemas

### El ZIP no se genera

```powershell
# Crear manualmente
Compress-Archive -Path "DynamicSepticSystem\bin\Release\*" -DestinationPath "CalandriaApp-v1.3.7-H.zip" -Force
```

### version.txt no tiene el formato correcto

```powershell
# Crear correctamente (sin salto de línea al final)
"1.3.7-H" | Out-File -FilePath "version.txt" -Encoding ASCII -NoNewline
```

### Los assets no se suben

- Verifica que el archivo no exceda 2GB
- Intenta subir uno por uno
- Asegúrate de estar autenticado en GitHub

### La actualización automática no funciona

Verifica:
1. ? El tag es exactamente `1.3.7-H` (case-sensitive)
2. ? El ZIP se llama `CalandriaApp-v1.3.7-H.zip`
3. ? El `version.txt` contiene solo `1.3.7-H`
4. ? La release está marcada como **Latest**

---

## ? Checklist Final

Antes de publicar, verifica:

- [ ] Código compilado en Release (no Debug)
- [ ] version.txt contiene `1.3.7-H`
- [ ] Updater.exe incluido en el ZIP
- [ ] App.config con cadena de conexión correcta
- [ ] Probado en máquina limpia o VM
- [ ] SQL_VerificarProveedores.sql incluido
- [ ] RELEASE_NOTES.txt incluido
- [ ] Tag en GitHub es `1.3.7-H`
- [ ] Assets subidos correctamente
- [ ] Descripción de release completa
- [ ] Marcado como Latest release

---

**Listo para publicar! ??**

Fecha de release: Enero 2024
Versión: 1.3.7-H
Cambios principales: Sistema de Proveedores + Fix Actualizaciones
