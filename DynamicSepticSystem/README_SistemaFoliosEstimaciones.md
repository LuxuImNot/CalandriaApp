# ?? ACTUALIZACIÓN: Sistema de Folios para Estimaciones

## ? Archivos Creados

### 1. **SQL_CrearTablasFoliosYPDFs.sql**
Script para crear las tablas necesarias:
- `FoliosEstimacion` - Registro de estimaciones con folio único
- `FoliosEstimacionDetalle` - Detalle de conceptos/partidas por folio
- `PDFsEstimacion` - Almacenamiento de PDFs en la base de datos

### 2. **FormRepositorioPDFs.cs** y **FormRepositorioPDFs.Designer.cs**
Formulario para visualizar el historial de estimaciones por Manzana/Lote:
- Lista de estimaciones generadas
- Ver PDF almacenado
- Ver detalle de conceptos incluidos
- Eliminar estimaciones

### 3. **FormDetalleEstimacion.cs** y **FormDetalleEstimacion.Designer.cs**
Formulario para ver el detalle de una estimación específica:
- Información general del folio
- Lista de conceptos/partidas incluidos
- Totales calculados

### 4. **FormEstimacionConceptoMigrado_ExtensionFolios.cs**
Extensión parcial con métodos para:
- Generar folios únicos
- Guardar folios en BD
- Guardar detalle de conceptos
- Guardar PDF en BD como VARBINARY
- Abrir repositorio de PDFs
- Crear tablas automáticamente si no existen

## ?? Cómo Funciona el Sistema de Folios

### Formato de Folio
```
EST-M{manzana}-L{lote}-{numero:000}-{año}

Ejemplos:
EST-M1-L5-001-2025  (Primera estimación de M1-L5 en 2025)
EST-M1-L5-002-2025  (Segunda estimación de M1-L5 en 2025)
EST-M3-L12-001-2025 (Primera estimación de M3-L12 en 2025)
```

### Flujo de Exportación con Folio

1. **Usuario selecciona conceptos** con checkboxes
2. **Click en "Exportar a PDF"**
3. **Sistema genera folio automático**
   - Calcula siguiente número de estimación
   - Genera folio único
4. **Guarda en base de datos:**
   - Tabla `FoliosEstimacion` (encabezado)
   - Tabla `FoliosEstimacionDetalle` (conceptos seleccionados)
   - Tabla `PDFsEstimacion` (PDF como bytes)
5. **Genera archivo PDF**
6. **Actualiza avances al 100%** (conceptos estimados)
7. **Muestra mensaje de éxito**

## ?? Estructura de Tablas

### FoliosEstimacion
| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT IDENTITY | ID único del folio |
| Folio | NVARCHAR(50) UNIQUE | Folio generado automáticamente |
| Manzana | NVARCHAR(10) | Manzana de la casa |
| Lote | NVARCHAR(10) | Lote de la casa |
| Prototipo | NVARCHAR(50) | Tipo de prototipo |
| FechaGeneracion | DATETIME | Fecha de creación |
| Proveedor | NVARCHAR(200) | Nombre del proveedor |
| Descripcion | NVARCHAR(500) | Descripción del trabajo |
| ImporteContrato | FLOAT | Monto total del contrato |
| TotalRequisicion | FLOAT | Total de la requisición |
| Amortizacion | FLOAT | Monto de amortización |
| PorcentajeAmortizacion | FLOAT | % de amortización aplicado |
| TotalEstimacion | FLOAT | Total de la estimación |
| NumeroEstimacion | INT | Número correlativo por casa |
| Usuario | NVARCHAR(100) | Usuario que generó |
| Observaciones | NVARCHAR(MAX) | Notas adicionales |

### FoliosEstimacionDetalle
| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT IDENTITY | ID único |
| FolioId | INT FK | Referencia a FoliosEstimacion |
| CodigoConcepto | NVARCHAR(10) | Código del concepto |
| NombreConcepto | NVARCHAR(200) | Nombre del concepto |
| WBS | INT | Código WBS de la partida |
| NombrePartida | NVARCHAR(500) | Nombre de la partida |
| MontoPresupuestado | FLOAT | Monto presupuestado |
| MontoEjecutado | FLOAT | Monto ejecutado |
| AvancePorcentaje | FLOAT | Porcentaje de avance |

### PDFsEstimacion
| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT IDENTITY | ID único |
| FolioId | INT FK | Referencia a FoliosEstimacion |
| Folio | NVARCHAR(50) | Copia del folio (búsqueda) |
| Manzana | NVARCHAR(10) | Manzana (filtro) |
| Lote | NVARCHAR(10) | Lote (filtro) |
| NombreArchivo | NVARCHAR(255) | Nombre del archivo PDF |
| ContenidoPDF | VARBINARY(MAX) | **PDF almacenado como bytes** |
| TamanioBytes | BIGINT | Tamaño del archivo |
| FechaAlmacenamiento | DATETIME | Fecha de almacenamiento |

## ?? Métodos Agregados

### En FormEstimacionConceptoMigrado

```csharp
// Obtener siguiente número de estimación
private int ObtenerSiguienteNumeroEstimacion(string manzana, string lote)

// Generar folio único
private string GenerarFolio(string manzana, string lote, int numeroEstimacion)

// Guardar folio en BD
private int GuardarFolioEstimacion(...)

// Guardar detalle de conceptos
private void GuardarDetalleFolio(int folioId)

// Guardar PDF como VARBINARY en BD
private void GuardarPDFEnBD(int folioId, string folio, string manzana, string lote, string rutaPdf)

// Abrir repositorio
private void btnRepositorioPDFs_Click(object sender, EventArgs e)

// Crear tablas si no existen
private void CrearTablasSiNoExisten()
```

## ??? Botón "Ver Estimaciones"

### Ubicación
Panel de filtros, junto al botón "Cargar Avance"

### Funcionalidad
- Abre `FormRepositorioPDFs` con el filtro de Manzana/Lote actual
- Muestra historial de estimaciones generadas
- Permite ver PDFs almacenados
- Permite ver detalle de conceptos
- Permite eliminar estimaciones

### Validación
- Requiere que se haya seleccionado Manzana y Lote

## ?? Repositorio de PDFs

### Características
- **Lista de estimaciones** por Manzana/Lote
- **Columnas:**
  - Folio
  - No. Estimación
  - Fecha generación
  - Proveedor
  - Total estimación
  - Partidas incluidas

### Acciones Disponibles
1. **?? Ver PDF** - Abre el PDF almacenado en la BD
2. **?? Detalle** - Muestra conceptos incluidos
3. **?? Actualizar** - Recarga la lista
4. **??? Eliminar** - Borra el folio completo (con confirmación)

## ?? Actualización en Designer

### Agregar al FormEstimacionConceptoMigrado.Designer.cs

```csharp
// Agregar botón en panelFiltros
private System.Windows.Forms.Button btnRepositorioPDFs;

// En InitializeComponent(), después de btnCargarAvance:
// 
// btnRepositorioPDFs
// 
this.btnRepositorioPDFs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
this.btnRepositorioPDFs.FlatAppearance.BorderSize = 0;
this.btnRepositorioPDFs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
this.btnRepositorioPDFs.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
this.btnRepositorioPDFs.ForeColor = System.Drawing.Color.White;
this.btnRepositorioPDFs.Location = new System.Drawing.Point(680, 18);
this.btnRepositorioPDFs.Name = "btnRepositorioPDFs";
this.btnRepositorioPDFs.Size = new System.Drawing.Size(180, 35);
this.btnRepositorioPDFs.TabIndex = 5;
this.btnRepositorioPDFs.Text = "?? Ver Estimaciones";
this.btnRepositorioPDFs.UseVisualStyleBackColor = false;
this.btnRepositorioPDFs.Click += new System.EventHandler(this.btnRepositorioPDFs_Click);

// Agregar al panelFiltros.Controls
this.panelFiltros.Controls.Add(this.btnRepositorioPDFs);
```

## ? Pasos de Instalación

### 1. Ejecutar Script SQL
```sql
-- Ejecutar: SQL_CrearTablasFoliosYPDFs.sql
-- Esto crea las 3 tablas necesarias
```

### 2. Compilar Proyecto
Los archivos ya están listos, solo compilar.

### 3. Probar Funcionalidad

#### Test 1: Generar Estimación con Folio
1. Abrir FormEstimacionConceptoMigrado
2. Seleccionar M1, L1
3. Cargar avance
4. Seleccionar conceptos con checkboxes
5. Click "Exportar a PDF"
6. Verificar que se genera el folio automáticamente
7. Verificar que se guarda en las tablas

#### Test 2: Ver Repositorio
1. Click "?? Ver Estimaciones"
2. Verificar que aparece la estimación generada
3. Hacer doble-clic o "Ver PDF"
4. Verificar que se abre el PDF

#### Test 3: Ver Detalle
1. En el repositorio, seleccionar una estimación
2. Click "?? Detalle"
3. Verificar que muestra los conceptos incluidos

#### Test 4: Eliminar
1. Seleccionar una estimación
2. Click "??? Eliminar"
3. Confirmar
4. Verificar que se elimina (CASCADE borra detalle y PDF también)

## ?? Consultas Útiles

### Ver folios generados
```sql
SELECT * FROM FoliosEstimacion
ORDER BY FechaGeneracion DESC
```

### Ver detalle de un folio
```sql
SELECT 
    f.Folio,
    f.Manzana,
    f.Lote,
    f.TotalEstimacion,
    fd.NombreConcepto,
    fd.NombrePartida,
    fd.MontoEjecutado
FROM FoliosEstimacion f
INNER JOIN FoliosEstimacionDetalle fd ON f.Id = fd.FolioId
WHERE f.Folio = 'EST-M1-L1-001-2025'
```

### Ver estimaciones por casa
```sql
SELECT 
    Folio,
    NumeroEstimacion,
    FechaGeneracion,
    Proveedor,
    TotalEstimacion,
    (SELECT COUNT(*) FROM FoliosEstimacionDetalle WHERE FolioId = f.Id) AS TotalPartidas
FROM FoliosEstimacion f
WHERE Manzana = '1' AND Lote = '1'
ORDER BY NumeroEstimacion DESC
```

### Ver tamaño de PDFs almacenados
```sql
SELECT 
    Folio,
    NombreArchivo,
    TamanioBytes / 1024 AS TamanioKB,
    TamanioBytes / 1024 / 1024 AS TamanioMB,
    FechaAlmacenamiento
FROM PDFsEstimacion
ORDER BY TamanioBytes DESC
```

## ?? Ventajas del Sistema

1. **Trazabilidad completa** - Cada estimación tiene un folio único
2. **Histórico consultable** - Ver todas las estimaciones de una casa
3. **PDFs accesibles** - Almacenados en BD, no se pierden
4. **Detalle registrado** - Saber exactamente qué se incluyó en cada estimación
5. **Numeración automática** - No hay duplicados ni errores manuales
6. **Eliminación en cascada** - Borrar un folio elimina todo su detalle
7. **Búsqueda por casa** - Filtro por Manzana/Lote
8. **Fecha de generación** - Registro automático de cuándo se generó

## ?? Mejoras Futuras (Opcional)

- [ ] Agregar campo "Firmado" (bool) para marcar estimaciones firmadas
- [ ] Exportar múltiples PDFs en ZIP
- [ ] Reportes consolidados por período
- [ ] Filtros avanzados en repositorio (fecha, proveedor, rango de monto)
- [ ] Gráficas de estimaciones por mes
- [ ] Comparativa entre estimaciones (evolución de avance)
- [ ] Notificaciones de nuevas estimaciones

---

**Desarrollado para**: Sistema Calandria Residencial  
**Fecha**: Enero 2025  
**Versión**: 1.0  
**Estado**: ? LISTO PARA COMPILAR

---

## ?? ¡Sistema Completo!

Ahora tienes un sistema robusto de gestión de estimaciones con:
- ? Folios únicos automáticos
- ? Almacenamiento de PDFs en BD
- ? Repositorio visual navegable
- ? Detalle completo de cada estimación
- ? Trazabilidad total
