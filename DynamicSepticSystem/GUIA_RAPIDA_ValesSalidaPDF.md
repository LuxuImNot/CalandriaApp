# ?? GUÍA RÁPIDA - Sistema de Vales de Salida con PDF

## ?? Resumen

Sistema que genera automáticamente vales de salida de almacén en formato PDF con almacenamiento en base de datos y repositorio consultable.

---

## ?? Uso Básico

### 1?? Generar un Vale de Salida

```
1. Abrir FormAlmacen ? Pestaña "Salidas"
2. Ingresar Manzana y Lote
3. Click "Seleccionar Casa"
4. Se cargan insumos disponibles
5. Capturar cantidades en "Cantidad a Solicitar"
6. Click "Registrar Salida"
7. Sistema solicita datos:
   - Solicitante
   - Residente de Obra
   - Encargado de Almacén
   - Observaciones
8. Click "Generar Vale"
9. ? Se genera el PDF automáticamente
10. ? Se guarda en el repositorio
```

### 2?? Ver el Repositorio de Vales

```csharp
// Desde código:
AbrirRepositorioValesSalida();

// Filtrado por casa:
AbrirRepositorioValesSalida("1", "5"); // M1-L5
```

### 3?? Consultar un Vale

```
1. En el repositorio, ubicar el vale
2. Doble-clic en la fila
   O
   Seleccionar y click "?? Ver PDF"
3. El PDF se abre automáticamente
```

### 4?? Ver Detalle de un Vale

```
1. Seleccionar vale en el repositorio
2. Click "?? Detalle"
3. Se muestra:
   - Información general
   - Listado de insumos
   - Totales
```

---

## ?? Formato del Folio

```
VALE-M{manzana}-L{lote}-{numero:000}-{año}

Ejemplos:
? VALE-M1-L5-001-2025
? VALE-M3-L12-002-2025
? VALE-M5-L1-003-2025
```

---

## ??? Estructura del PDF

```
???????????????????????????????????????
?  ?? DESARROLLADORA DE CASAS CAMANEY ?
?  ALMACEN DE INSUMOS Y HERRAMIENTAS  ?
?     VALE DE SALIDA DE ALMACEN       ?
???????????????????????????????????????
?  OBRA: M1-L5    FOLIO: 001          ?
?  FECHA: 28/01/2025                  ?
???????????????????????????????????????
?  TABLA DE INSUMOS (Máximo 10)       ?
?  Nº | Código | Descripción | ...    ?
?  1  | M22357 | Tubo conduit | ...   ?
?  2  | M22305 | Conector     | ...   ?
???????????????????????????????????????
?  TOTAL: $1,850.00                   ?
???????????????????????????????????????
?  FIRMAS:                            ?
?  - Solicitante                      ?
?  - Entrega los insumos              ?
?  - Residente de obra                ?
?  - Encargado de almacén             ?
???????????????????????????????????????
```

---

## ?? Consultas SQL Rápidas

### Ver últimos 10 vales

```sql
SELECT TOP 10 * FROM FoliosSalidaAlmacen 
ORDER BY FechaSolicitud DESC
```

### Vales de una casa

```sql
SELECT * FROM FoliosSalidaAlmacen 
WHERE Manzana = '1' AND Lote = '5'
ORDER BY NumeroSalida DESC
```

### Ver detalle de un vale

```sql
SELECT 
    f.Folio, f.FechaSolicitud, f.Solicitante,
    d.Codigo, d.Descripcion, d.Cantidad, d.Importe
FROM FoliosSalidaAlmacen f
INNER JOIN FoliosSalidaAlmacenDetalle d ON f.Id = d.FolioId
WHERE f.Folio = 'VALE-M1-L5-001-2025'
```

---

## ?? Configuración Inicial

### 1. Ejecutar Script SQL

```sql
-- Archivo: SQL_SCRIPTS\CrearTablasRepositorioSalidasAlmacen.sql
USE CALANDRIA;
GO
-- Ejecutar el script completo
```

### 2. Verificar Tablas Creadas

```sql
-- Verificar
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN (
    'FoliosSalidaAlmacen', 
    'FoliosSalidaAlmacenDetalle', 
    'PDFsSalidaAlmacen'
)
```

---

## ?? Casos de Uso Comunes

### Caso 1: Salida Normal de Insumos

```
Escenario: Se necesitan materiales para M1-L5
1. Seleccionar casa M1-L5
2. Agregar cantidades:
   - Tubo conduit: 30 m
   - Conector: 30 piezas
3. Registrar salida
4. Llenar datos del vale
5. Se genera PDF automáticamente
```

### Caso 2: Consultar Histórico de una Casa

```
Escenario: Ver todos los vales de M1-L5
1. Abrir repositorio filtrado:
   AbrirRepositorioValesSalida("1", "5")
2. Ver lista de vales
3. Filtrar por fechas si es necesario
4. Ver PDFs individuales
```

### Caso 3: Reimprimir un Vale

```
Escenario: Se perdió el vale físico
1. Abrir repositorio
2. Buscar por folio o fecha
3. Doble-clic en el vale
4. El PDF se abre automáticamente
5. Imprimir desde el visor
```

### Caso 4: Eliminar un Vale Erróneo

```
Escenario: Se generó un vale por error
1. Abrir repositorio
2. Seleccionar el vale
3. Click "??? Eliminar"
4. Confirmar eliminación
5. Se eliminan automáticamente:
   - Encabezado
   - Detalle
   - PDF almacenado
```

---

## ?? Personalización del PDF

### Modificar Encabezado

En `FormAlmacen_ExtensionPDFSalidas.cs`, línea ~350:

```csharp
// Cambiar nombre de empresa
gfx.DrawString("DESARROLLADORA DE CASAS CAMANEY",
    fontBold, XBrushes.Black, ...);

// Cambiar subtítulo
gfx.DrawString("ALMACEN DE INSUMOS Y HERRAMIENTAS",
    fontNormal, XBrushes.Black, ...);
```

### Modificar Logo

```csharp
// Línea ~280
using (var logo = XImage.FromStream(ms))
{
    int logoWidth = 90;  // Cambiar ancho
    int logoHeight = 60; // Cambiar alto
    gfx.DrawImage(logo, x, y, logoWidth, logoHeight);
}
```

---

## ?? Reportes Útiles

### Resumen de Salidas del Mes

```sql
SELECT 
    Manzana, Lote,
    COUNT(*) AS TotalVales,
    SUM(TotalImporte) AS SumaTotal
FROM FoliosSalidaAlmacen
WHERE MONTH(FechaSolicitud) = MONTH(GETDATE())
  AND YEAR(FechaSolicitud) = YEAR(GETDATE())
GROUP BY Manzana, Lote
ORDER BY SumaTotal DESC
```

### Insumos Más Solicitados

```sql
SELECT TOP 10
    d.Codigo,
    d.Descripcion,
    COUNT(*) AS VecesRequisado,
    SUM(d.Cantidad) AS CantidadTotal,
    SUM(d.Importe) AS ImporteTotal
FROM FoliosSalidaAlmacenDetalle d
GROUP BY d.Codigo, d.Descripcion
ORDER BY VecesRequisado DESC
```

### Vales Pendientes

```sql
SELECT 
    Folio, Obra, Solicitante, 
    FechaSolicitud, TotalImporte
FROM FoliosSalidaAlmacen
WHERE Estado = 'PENDIENTE'
ORDER BY FechaSolicitud ASC
```

---

## ?? Solución de Problemas

### Problema: No se genera el PDF

**Solución:**
```csharp
// Verificar que la carpeta exista
string carpetaPdf = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
    "CALANDRIA RESIDENCIAL", "PDFValesSalida");

// Si no existe, se crea automáticamente
Directory.CreateDirectory(carpetaPdf);
```

### Problema: No aparecen en el repositorio

**Solución:**
```sql
-- Verificar que las tablas existan
SELECT COUNT(*) FROM FoliosSalidaAlmacen

-- Verificar que se guardaron
SELECT * FROM FoliosSalidaAlmacen 
ORDER BY FechaSolicitud DESC
```

### Problema: Error al abrir PDF

**Solución:**
```csharp
// El PDF se guarda temporalmente en:
Path.GetTempPath() + NombreArchivo

// Verificar que existe:
if (File.Exists(tempPath))
{
    Process.Start(tempPath);
}
```

---

## ?? Permisos Requeridos

```
? Escritura en: Mis Documentos\CALANDRIA RESIDENCIAL\PDFValesSalida\
? Lectura/Escritura en: Tabla FoliosSalidaAlmacen
? Lectura/Escritura en: Tabla FoliosSalidaAlmacenDetalle
? Lectura/Escritura en: Tabla PDFsSalidaAlmacen
? Lectura en: Tabla SalidasAlmacen (para consultar insumos)
```

---

## ?? Soporte

Para dudas o problemas, revisar:
1. `README_SistemaValesSalidaPDF.md` (documentación completa)
2. `SQL_SCRIPTS\CrearTablasRepositorioSalidasAlmacen.sql` (estructura BD)
3. `FormAlmacen_ExtensionPDFSalidas.cs` (código fuente)

---

**Última actualización**: Enero 2025  
**Versión**: 1.0  
**Estado**: ? Producción

---

## ?? Checklist de Verificación

Antes de usar el sistema, verificar:

- [ ] ? Script SQL ejecutado
- [ ] ? Tablas creadas en BD
- [ ] ? Proyecto compilado sin errores
- [ ] ? Permisos de carpeta configurados
- [ ] ? Conexión a BD funcional
- [ ] ? Logo de empresa disponible
- [ ] ? FormAlmacen modificado correctamente

**¡Todo listo para generar vales!** ??
