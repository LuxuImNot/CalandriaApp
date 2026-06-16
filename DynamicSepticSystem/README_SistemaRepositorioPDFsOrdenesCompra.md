# ?? SISTEMA DE REPOSITORIO DE PDFs PARA ÓRDENES DE COMPRA

## ? Sistema Completado

Sistema integral de almacenamiento y consulta de PDFs de órdenes de compra (directas, múltiples, indirectas y administrativas) con consulta por Manzana/Lote.

---

## ?? Archivos Creados

### 1. **SQL_CrearTablasRepositorioPDFsOrdenesCompra.sql**
Script SQL para crear las tablas necesarias:
- `FoliosOrdenCompra` - Registro de órdenes con folio único
- `FoliosOrdenCompraDetalle` - Detalle de insumos por folio
- `FoliosOrdenCompra_Casas` - Casas incluidas en órdenes múltiples
- `PDFsOrdenCompra` - Almacenamiento de PDFs en VARBINARY(MAX)

### 2. **FormCompraMulti_ExtensionPDFRepositorio.cs**
Extensión parcial de `FormCompraMulti` con métodos para:
- Guardar folios de órdenes múltiples
- Guardar detalle de insumos
- Guardar casas incluidas
- Guardar PDF en BD como bytes
- Abrir repositorio de PDFs
- Crear tablas automáticamente

### 3. **FormCompraIndirecta_ExtensionPDFRepositorio.cs**
Extensión parcial de `FormCompraIndirecta` con métodos para:
- Guardar folios de órdenes indirectas/administrativas
- Guardar detalle de insumos
- Guardar PDF en BD
- Abrir repositorio de PDFs indirectas
- Crear tablas automáticamente

### 4. **FormRepositorioPDFsOrdenesCompra.cs** y **.Designer.cs**
Formulario para visualizar y gestionar el repositorio:
- Lista de órdenes generadas (filtrable)
- Ver PDF almacenado
- Ver detalle de insumos
- Eliminar órdenes
- Filtros por fecha y búsqueda
- Consulta por Manzana/Lote o todas las órdenes

### 5. **FormDetalleOrdenCompra.cs** y **.Designer.cs**
Formulario para ver el detalle completo de una orden:
- Información general del folio
- Lista de insumos incluidos
- Casas incluidas (si es orden múltiple)
- Totales calculados (Subtotal, IVA, Total)

---

## ??? Estructura de Tablas

### FoliosOrdenCompra
Tabla principal de folios de órdenes.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT IDENTITY | ID único del folio |
| Folio | NVARCHAR(50) UNIQUE | Folio generado automáticamente |
| Manzana | NVARCHAR(10) NULL | Manzana (NULL para indirectas) |
| Lote | NVARCHAR(10) NULL | Lote (NULL para indirectas) |
| FechaGeneracion | DATETIME | Fecha de creación |
| TipoOrden | NVARCHAR(20) | 'MULTIPLE', 'INDIVIDUAL', 'INDIRECTA', 'ADMINISTRATIVA' |
| NombreProveedor | NVARCHAR(200) | Nombre del proveedor |
| CodigoProveedor | NVARCHAR(50) | Código del proveedor |
| TotalSinIVA | DECIMAL(18,2) | Subtotal sin IVA |
| IVA | DECIMAL(18,2) | IVA calculado (16%) |
| TotalConIVA | DECIMAL(18,2) | Total final con IVA |
| NumeroOrden | INT NULL | Número correlativo por casa |
| Usuario | NVARCHAR(100) | Usuario que generó la orden |
| Estado | NVARCHAR(20) | 'PENDIENTE', 'COMPLETADA', 'CANCELADA' |
| Observaciones | NVARCHAR(MAX) | Notas adicionales |

### FoliosOrdenCompraDetalle
Detalle de insumos incluidos en cada folio.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT IDENTITY | ID único |
| FolioId | INT FK | Referencia a FoliosOrdenCompra |
| Clave | NVARCHAR(100) | Código del insumo |
| Descripcion | NVARCHAR(500) | Descripción del insumo |
| Unidad | NVARCHAR(50) | Unidad de medida |
| Cantidad | DECIMAL(18,3) | Cantidad solicitada |
| PrecioUnitario | DECIMAL(18,2) | Precio por unidad |
| ImporteTotal | DECIMAL(18,2) | Importe = Cantidad × Precio |
| Familia | NVARCHAR(100) | Familia/Categoría del insumo |

### FoliosOrdenCompra_Casas
Casas incluidas en órdenes múltiples.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT IDENTITY | ID único |
| FolioId | INT FK | Referencia a FoliosOrdenCompra |
| Manzana | NVARCHAR(10) | Manzana de la casa |
| Lote | NVARCHAR(10) | Lote de la casa |
| Prototipo | NVARCHAR(50) | Tipo de prototipo |

### PDFsOrdenCompra
Almacenamiento de PDFs como VARBINARY.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT IDENTITY | ID único |
| FolioId | INT FK | Referencia a FoliosOrdenCompra |
| Folio | NVARCHAR(50) | Copia del folio (índice) |
| Manzana | NVARCHAR(10) NULL | Manzana (filtro) |
| Lote | NVARCHAR(10) NULL | Lote (filtro) |
| TipoOrden | NVARCHAR(20) | Tipo de orden |
| NombreArchivo | NVARCHAR(255) | Nombre del archivo PDF |
| **ContenidoPDF** | **VARBINARY(MAX)** | **PDF almacenado como bytes** |
| TamanioBytes | BIGINT | Tamaño del archivo |
| FechaAlmacenamiento | DATETIME | Fecha de almacenamiento |

---

## ?? Flujo de Guardado

### Para Órdenes Múltiples (FormCompraMulti)

1. **Usuario genera orden de compra**
   - Agrega casas
   - Selecciona insumos del carrito
   - Selecciona proveedor
   - Click en "Generar Orden"

2. **Sistema genera folio automático**
   ```
   OC-MULTI-YYYYMMDD-###
   Ejemplo: OC-MULTI-20250128-001
   ```

3. **Se guarda en base de datos:**
   - Tabla `FoliosOrdenCompra` (encabezado)
   - Tabla `FoliosOrdenCompraDetalle` (insumos)
   - Tabla `FoliosOrdenCompra_Casas` (casas incluidas)
   - Tabla `PDFsOrdenCompra` (PDF como bytes)

4. **Se genera archivo PDF**
   - En: `Mis Documentos\CALANDRIA RESIDENCIAL\PDFOrdenesCompra\`
   - Se abre automáticamente

5. **Mensaje de confirmación**
   - Muestra folio, tipo, totales
   - Confirma almacenamiento en BD

### Para Órdenes Indirectas (FormCompraIndirecta)

1. **Usuario genera orden indirecta/administrativa**
   - Selecciona insumos del carrito
   - Define cantidades y costos
   - Selecciona proveedor
   - Elige tipo de título (Indirecta o Administrativa)
   - Click en "Generar Orden"

2. **Sistema genera folio automático**
   ```
   OC-IND-YYYYMMDD-###
   Ejemplo: OC-IND-20250128-001
   ```

3. **Se guarda en base de datos:**
   - Tabla `FoliosOrdenCompra` (sin Manzana/Lote)
   - Tabla `FoliosOrdenCompraDetalle` (insumos)
   - Tabla `PDFsOrdenCompra` (PDF como bytes)

4. **Se genera archivo PDF**
   - Con el título seleccionado
   - Se abre automáticamente

---

## ?? Métodos Principales

### En FormCompraMulti_ExtensionPDFRepositorio.cs

```csharp
// Guardar orden completa en repositorio
public void GuardarOrdenEnRepositorio(
    string folioOC, 
    string rutaPdf, 
    List<InsumoOrdenCompra> insumos,
    List<CasaSeleccionada> casas = null)

// Abrir repositorio de PDFs
public void AbrirRepositorioPDFsOrdenesCompra(
    string manzana = null, 
    string lote = null)

// Crear tablas si no existen
private void CrearTablasRepositorioSiNoExisten()
```

### En FormCompraIndirecta_ExtensionPDFRepositorio.cs

```csharp
// Guardar orden indirecta en repositorio
public void GuardarOrdenIndirectaEnRepositorio(
    string folioOC, 
    string rutaPdf, 
    List<InsumoIndirecto> insumos, 
    string tipoTitulo)

// Abrir repositorio de órdenes indirectas
public void AbrirRepositorioPDFsOrdenesIndirectas()
```

---

## ?? Repositorio de PDFs

### Características

- **Lista de órdenes** con información resumida
- **Columnas:**
  - Folio
  - Tipo de orden
  - Fecha de generación
  - Manzana/Lote (si aplica)
  - Proveedor
  - Total con IVA
  - Total de insumos
  - Casas incluidas (para órdenes múltiples)
  - Estado

### Acciones Disponibles

1. **?? Ver PDF** - Extrae y abre el PDF almacenado
2. **?? Detalle** - Muestra formulario con información completa
3. **??? Eliminar** - Borra el folio completo (con confirmación)
4. **?? Actualizar** - Recarga la lista desde BD
5. **? Cerrar** - Cierra el repositorio

### Filtros

- **Búsqueda por texto:** Folio, proveedor, casas incluidas
- **Rango de fechas:** Desde/Hasta con checkboxes
- **Por Manzana/Lote:** Automático si se abre desde una casa específica
- **Solo indirectas:** Filtro automático para órdenes sin casa

---

## ?? Integración en Formularios Existentes

### FormCompraMulti

Después de generar la orden y el PDF, agregar:

```csharp
// En btnGenerarOrden_Click, después de GenerarPDFCorporativo():
try
{
    var casas = lstCasas.Items.Cast<CasaSeleccionada>().ToList();
    var insumos = olvCarrito.Objects.Cast<InsumoOrdenCompra>().ToList();
    
    GuardarOrdenEnRepositorio(folioOC, rutaPdf, insumos, casas);
}
catch (Exception ex)
{
    MessageBox.Show($"Error al guardar en repositorio: {ex.Message}", 
        "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
```

**Agregar botón "Ver Repositorio":**
```csharp
private void btnVerRepositorio_Click(object sender, EventArgs e)
{
    AbrirRepositorioPDFsOrdenesCompra();
}
```

### FormCompraIndirecta

Después de generar la orden y el PDF, agregar:

```csharp
// En btnGenerarOrden_Click, después de GenerarPDF():
try
{
    var insumos = insumosCarrito;
    
    GuardarOrdenIndirectaEnRepositorio(folioOC, rutaPdf, insumos, tipoTitulo);
}
catch (Exception ex)
{
    MessageBox.Show($"Error al guardar en repositorio: {ex.Message}", 
        "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
```

**Agregar botón "Ver Repositorio de Indirectas":**
```csharp
private void btnVerRepositorio_Click(object sender, EventArgs e)
{
    AbrirRepositorioPDFsOrdenesIndirectas();
}
```

---

## ?? Consultas SQL Útiles

### Ver todas las órdenes generadas
```sql
SELECT * FROM VW_FoliosOrdenCompraResumen 
ORDER BY FechaGeneracion DESC
```

### Ver órdenes de una casa específica
```sql
SELECT * FROM VW_OrdenesCompraPorCasa 
WHERE Manzana = '1' AND Lote = '1' 
ORDER BY FechaGeneracion DESC
```

### Ver detalle completo de una orden
```sql
SELECT 
    f.Folio,
    f.TipoOrden,
    f.FechaGeneracion,
    f.NombreProveedor,
    f.TotalConIVA,
    d.Clave,
    d.Descripcion,
    d.Cantidad,
    d.PrecioUnitario,
    d.ImporteTotal
FROM FoliosOrdenCompra f
INNER JOIN FoliosOrdenCompraDetalle d ON f.Id = d.FolioId
WHERE f.Folio = 'OC-MULTI-20250128-001'
```

### Ver órdenes indirectas/administrativas
```sql
SELECT * FROM FoliosOrdenCompra 
WHERE TipoOrden IN ('INDIRECTA', 'ADMINISTRATIVA') 
ORDER BY FechaGeneracion DESC
```

### Ver tamaño de PDFs almacenados
```sql
SELECT 
    Folio,
    TipoOrden,
    NombreArchivo,
    TamanioBytes / 1024.0 / 1024.0 AS TamanioMB,
    FechaAlmacenamiento
FROM PDFsOrdenCompra 
ORDER BY TamanioBytes DESC
```

### Total de órdenes por tipo
```sql
SELECT 
    TipoOrden,
    COUNT(*) AS TotalOrdenes,
    SUM(TotalConIVA) AS SumaTotal
FROM FoliosOrdenCompra
GROUP BY TipoOrden
ORDER BY TotalOrdenes DESC
```

### Órdenes por proveedor
```sql
SELECT 
    NombreProveedor,
    COUNT(*) AS TotalOrdenes,
    SUM(TotalConIVA) AS MontoTotal
FROM FoliosOrdenCompra
GROUP BY NombreProveedor
ORDER BY MontoTotal DESC
```

---

## ?? Pasos de Instalación

### 1. Ejecutar Script SQL
```sql
-- Ejecutar: SQL_CrearTablasRepositorioPDFsOrdenesCompra.sql
-- Esto crea las 4 tablas y 2 vistas necesarias
```

### 2. Agregar archivos al proyecto
Asegurarse de que los siguientes archivos estén en el proyecto:
- `FormCompraMulti_ExtensionPDFRepositorio.cs`
- `FormCompraIndirecta_ExtensionPDFRepositorio.cs`
- `FormRepositorioPDFsOrdenesCompra.cs`
- `FormRepositorioPDFsOrdenesCompra.Designer.cs`
- `FormDetalleOrdenCompra.cs`
- `FormDetalleOrdenCompra.Designer.cs`
- `SQL_CrearTablasRepositorioPDFsOrdenesCompra.sql`

### 3. Integrar en FormCompraMulti
Agregar llamada a `GuardarOrdenEnRepositorio()` después de generar PDF.

### 4. Integrar en FormCompraIndirecta
Agregar llamada a `GuardarOrdenIndirectaEnRepositorio()` después de generar PDF.

### 5. Compilar y Probar

#### Test 1: Orden Múltiple
1. Abrir `FormCompraMulti`
2. Agregar varias casas
3. Seleccionar insumos
4. Generar orden de compra
5. Verificar que se guarde en repositorio
6. Abrir repositorio y verificar que aparezca
7. Ver PDF desde repositorio
8. Ver detalle

#### Test 2: Orden Indirecta
1. Abrir `FormCompraIndirecta`
2. Agregar insumos al carrito
3. Seleccionar proveedor
4. Generar orden (elegir tipo de título)
5. Verificar que se guarde en repositorio
6. Abrir repositorio de indirectas
7. Ver PDF y detalle

#### Test 3: Consulta por Manzana/Lote
1. Desde cualquier formulario que tenga Manzana/Lote
2. Abrir repositorio con filtro de M/L específico
3. Verificar que solo aparezcan órdenes de esa casa
4. Probar filtros de búsqueda y fecha

#### Test 4: Eliminar Orden
1. En el repositorio, seleccionar una orden
2. Click en "Eliminar"
3. Confirmar eliminación
4. Verificar que se elimine (CASCADE borra detalle, casas y PDF)

---

## ? Ventajas del Sistema

1. **Trazabilidad completa** - Cada orden tiene un folio único
2. **Histórico consultable** - Ver todas las órdenes por casa o globalmente
3. **PDFs accesibles** - Almacenados en BD, no se pierden
4. **Detalle registrado** - Saber exactamente qué se incluyó en cada orden
5. **Numeración automática** - No hay duplicados ni errores manuales
6. **Eliminación en cascada** - Borrar un folio elimina todo su detalle
7. **Búsqueda flexible** - Por folio, proveedor, fecha, casa
8. **Soporte multi-tipo** - Órdenes múltiples, individuales, indirectas, administrativas
9. **Consulta por casa** - Filtro automático por Manzana/Lote
10. **Sin duplicación** - El PDF se guarda solo en BD, evitando archivos dispersos

---

## ?? Diseño de Interfaz

### Colores Corporativos
- **Header**: `RGB(41, 128, 185)` - Azul corporativo
- **Tabla Headers**: `RGB(52, 152, 219)` - Azul claro
- **Filas alternadas**: `RGB(245, 245, 245)` - Gris muy claro
- **Bordes**: `RGB(189, 195, 199)` - Gris

### Iconos en Botones
- ?? Ver PDF
- ?? Detalle
- ??? Eliminar
- ?? Actualizar
- ? Cerrar
- ?? Repositorio

---

## ?? Mejoras Futuras (Opcional)

- [ ] Agregar campo "Firmado" (bool) para marcar órdenes firmadas
- [ ] Exportar múltiples PDFs en ZIP
- [ ] Reportes consolidados por período
- [ ] Filtros avanzados (rango de monto, estado)
- [ ] Gráficas de órdenes por mes/proveedor
- [ ] Comparativa de costos entre órdenes
- [ ] Notificaciones de nuevas órdenes
- [ ] Historial de cambios de estado
- [ ] Integración con sistema de almacén para tracking de entradas
- [ ] Firma digital de órdenes
- [ ] Envío de PDFs por correo electrónico
- [ ] Generación de reportes Excel desde el repositorio

---

## ?? Notas Importantes

### Rendimiento
- Los PDFs se almacenan como `VARBINARY(MAX)` en SQL Server
- Tamaño máximo por PDF: ~2 GB (límite de VARBINARY(MAX))
- Se recomienda crear índices en las columnas de filtro frecuente

### Seguridad
- Los PDFs están seguros en la BD (respaldos automáticos)
- Se registra el usuario que generó cada orden
- Las eliminaciones requieren confirmación

### Mantenimiento
- Revisar periódicamente el tamaño de la tabla `PDFsOrdenCompra`
- Considerar archivar órdenes antiguas si el volumen crece mucho
- Mantener backups regulares de la base de datos

---

**Desarrollado para**: Sistema Calandria Residencial  
**Fecha**: Enero 2025  
**Versión**: 1.0  
**Estado**: ? LISTO PARA COMPILAR

---

## ?? ¡Sistema Completo!

Ahora tienes un sistema robusto de gestión de PDFs para órdenes de compra con:
- ? Folios únicos automáticos
- ? Almacenamiento de PDFs en BD
- ? Repositorio visual navegable
- ? Detalle completo de cada orden
- ? Trazabilidad total por casa o global
- ? Soporte para órdenes múltiples e indirectas
- ? Consultas SQL poderosas
- ? Interfaz moderna con tema corporativo
