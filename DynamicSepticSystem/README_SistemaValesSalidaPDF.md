# ?? SISTEMA DE VALES DE SALIDA DE ALMACÉN CON PDF Y REPOSITORIO

## ? Sistema Completado

Sistema integral para generar vales de salida de almacén en formato PDF con almacenamiento en base de datos y repositorio consultable.

---

## ?? Archivos Creados

### 1. **SQL_SCRIPTS/CrearTablasRepositorioSalidasAlmacen.sql**
Script SQL para crear las tablas necesarias:
- `FoliosSalidaAlmacen` - Registro de vales con folio único
- `FoliosSalidaAlmacenDetalle` - Detalle de insumos por vale
- `PDFsSalidaAlmacen` - Almacenamiento de PDFs en VARBINARY(MAX)
- `VW_FoliosSalidaResumen` - Vista de resumen
- `SP_ObtenerSiguienteNumeroSalida` - Procedimiento almacenado

### 2. **FormAlmacen_ExtensionPDFSalidas.cs**
Extensión parcial de `FormAlmacen` con métodos para:
- Generar folio único automático
- Solicitar datos del vale (solicitante, residente, encargado)
- Generar PDF con formato corporativo
- Guardar en repositorio de la base de datos
- Abrir repositorio de vales

### 3. **FormRepositorioValesSalida.cs** y **.Designer.cs**
Formulario para visualizar y gestionar el repositorio:
- Lista de vales generados (filtrable por fecha y texto)
- Ver PDF almacenado
- Ver detalle de insumos
- Eliminar vales
- Filtros por Manzana/Lote

### 4. **FormDetalleValeSalida.cs** y **.Designer.cs**
Formulario para ver el detalle completo de un vale:
- Información general del vale
- Lista de insumos incluidos con precios
- Total del vale

---

## ??? Estructura de Tablas

### FoliosSalidaAlmacen
Tabla principal de folios de vales.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT IDENTITY | ID único del folio |
| Folio | NVARCHAR(50) UNIQUE | Folio generado automáticamente |
| Manzana | NVARCHAR(10) | Manzana de la casa |
| Lote | NVARCHAR(10) | Lote de la casa |
| Prototipo | NVARCHAR(50) | Tipo de prototipo |
| Obra | NVARCHAR(200) | Identificador de obra |
| Edificacion | NVARCHAR(200) | Edificación |
| Urbanizacion | NVARCHAR(200) | Urbanización |
| FechaSolicitud | DATETIME | Fecha y hora de solicitud |
| DiaSolicitud | INT | Día de solicitud |
| MesSolicitud | INT | Mes de solicitud |
| AnioSolicitud | INT | Año de solicitud |
| TotalImporte | DECIMAL(18,2) | Total del vale |
| NumeroSalida | INT | Número correlativo por casa |
| Usuario | NVARCHAR(100) | Usuario que generó el vale |
| Solicitante | NVARCHAR(200) | Persona que solicita |
| ResidenteObra | NVARCHAR(200) | Residente de obra |
| EncargadoAlmacen | NVARCHAR(200) | Encargado de almacén |
| Observaciones | NVARCHAR(MAX) | Notas adicionales |
| Estado | NVARCHAR(20) | PENDIENTE, ENTREGADO, CANCELADO |

### FoliosSalidaAlmacenDetalle
Detalle de insumos incluidos en cada vale.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT IDENTITY | ID único |
| FolioId | INT FK | Referencia a FoliosSalidaAlmacen |
| NumeroFila | INT | Número de fila en el PDF (1-10) |
| Codigo | NVARCHAR(100) | Código del insumo |
| Descripcion | NVARCHAR(500) | Descripción del insumo |
| Unidad | NVARCHAR(50) | Unidad de medida |
| Cantidad | DECIMAL(18,3) | Cantidad solicitada |
| PrecioUnitario | DECIMAL(18,2) | Precio por unidad |
| Importe | DECIMAL(18,2) | Importe = Cantidad × Precio |
| Observaciones | NVARCHAR(500) | Observaciones del insumo |

### PDFsSalidaAlmacen
Almacenamiento de PDFs como VARBINARY.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | INT IDENTITY | ID único |
| FolioId | INT FK | Referencia a FoliosSalidaAlmacen |
| Folio | NVARCHAR(50) | Copia del folio (índice) |
| Manzana | NVARCHAR(10) | Manzana (filtro) |
| Lote | NVARCHAR(10) | Lote (filtro) |
| NombreArchivo | NVARCHAR(255) | Nombre del archivo PDF |
| **ContenidoPDF** | **VARBINARY(MAX)** | **PDF almacenado como bytes** |
| TamanioBytes | BIGINT | Tamaño del archivo |
| FechaAlmacenamiento | DATETIME | Fecha de almacenamiento |

---

## ?? Formato del PDF

El vale de salida se genera siguiendo el formato proporcionado:

### Encabezado
- Logo de CALANDRIA (izquierda)
- Nombre de la empresa (centro)
- "ALMACEN DE INSUMOS Y HERRAMIENTAS"
- "VALE DE SALIDA DE ALMACEN"

### Información Básica
- **OBRA**: M{manzana}-L{lote}
- **Edificación**: (campo con borde)
- **Urbanización**: (campo con borde)
- **FOLIO**: Número correlativo
- **FECHA DE SOLICITUD**: Casillas separadas (Día/Mes/Año)

### Tabla de Insumos
Máximo 10 filas con las siguientes columnas:
- Nº
- CÓDIGO
- DESCRIPCIÓN
- UNIDAD
- CANTIDAD
- PRECIO UNIT.
- IMPORTE
- OBSERVACIONES

### Totales
- **TOTAL**: Suma de todos los importes

### Firmas
- **SOLICITANTE**: Nombre y línea de firma
- **ENTREGA LOS INSUMOS**: Nombre del encargado y línea
- **RESIDENTE DE OBRA**: Nombre, firma y "Nombre y Firma"
- **Encargado de almacén**: Línea de firma

---

## ?? Flujo de Generación de Vale

### 1. Usuario Registra Salida en FormAlmacen

```csharp
// En FormAlmacen_Salidas.cs - método RegistrarSalida()
1. Selecciona casa destino (Manzana/Lote)
2. Carga insumos disponibles del inventario
3. Captura cantidades a solicitar
4. Click en "Registrar Salida"
5. Sistema registra en SalidasAlmacen
6. Sistema registra en HistorialMovimientos
7. ?? Sistema genera PDF automáticamente
```

### 2. Sistema Genera Folio Automático

```
Formato: VALE-M{manzana}-L{lote}-{numero:000}-{año}

Ejemplos:
VALE-M1-L5-001-2025  (Primer vale de M1-L5 en 2025)
VALE-M1-L5-002-2025  (Segundo vale de M1-L5 en 2025)
VALE-M3-L12-001-2025 (Primer vale de M3-L12 en 2025)
```

### 3. Sistema Solicita Datos Adicionales

Formulario emergente solicitando:
- **Solicitante**: Persona que solicita los insumos
- **Residente de Obra**: Responsable de la obra
- **Encargado de Almacén**: Pre-llenado con usuario actual
- **Observaciones**: Notas adicionales (opcional)

### 4. Sistema Genera PDF

- Crea documento con formato corporativo
- Incluye todos los insumos (máximo 10 por página)
- Calcula totales automáticamente
- Guarda en: `Mis Documentos\CALANDRIA RESIDENCIAL\PDFValesSalida\`
- Abre el PDF automáticamente

### 5. Sistema Guarda en Repositorio

```csharp
// Guardar en base de datos:
? FoliosSalidaAlmacen (encabezado)
? FoliosSalidaAlmacenDetalle (insumos)
? PDFsSalidaAlmacen (PDF como VARBINARY)
```

### 6. Usuario Recibe Confirmación

```
? Vale de salida generado correctamente

Folio: VALE-M1-L5-001-2025
Total insumos: 5
Importe total: $1,850.00

El PDF se guardó en el repositorio.
```

---

## ?? Repositorio de Vales

### Características

- **Lista de vales** con información resumida
- **Columnas:**
  - Folio
  - Fecha de solicitud
  - Manzana/Lote
  - Prototipo
  - Solicitante
  - Total importe
  - Total de insumos
  - Estado

### Acciones Disponibles

1. **?? Ver PDF** - Extrae y abre el PDF almacenado
2. **?? Detalle** - Muestra formulario con información completa
3. **??? Eliminar** - Borra el vale completo (con confirmación)
4. **?? Actualizar** - Recarga la lista desde BD
5. **? Cerrar** - Cierra el repositorio

### Filtros

- **Búsqueda por texto:** Folio, solicitante, obra
- **Rango de fechas:** Desde/Hasta con checkboxes
- **Por Manzana/Lote:** Automático si se abre desde una casa específica

---

## ?? Integración en FormAlmacen

### Modificación Realizada

En `FormAlmacen_Salidas.cs`, método `RegistrarSalida()`:

```csharp
if (seRegistroAlgo)
{
    // ?? GENERAR PDF DEL VALE DE SALIDA
    try
    {
        GenerarValeSalidaPDF();
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            $"?? La salida se registró correctamente pero hubo un error al generar el vale PDF:\n\n{ex.Message}",
            "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
    
    // ... resto del código ...
}
```

### Abrir Repositorio

Agregar botón en FormAlmacen (pestaña Salidas):

```csharp
private void btnVerValesSalida_Click(object sender, EventArgs e)
{
    // Abrir repositorio de todos los vales
    AbrirRepositorioValesSalida();
    
    // O abrir filtrado por casa actual
    if (casaActual != null)
    {
        AbrirRepositorioValesSalida(casaActual.Manzana, casaActual.Lote);
    }
}
```

---

## ?? Consultas SQL Útiles

### Ver todos los vales generados

```sql
SELECT * FROM VW_FoliosSalidaResumen 
ORDER BY FechaSolicitud DESC
```

### Ver vales de una casa específica

```sql
SELECT * FROM FoliosSalidaAlmacen 
WHERE Manzana = '1' AND Lote = '1' 
ORDER BY NumeroSalida DESC
```

### Ver detalle completo de un vale

```sql
SELECT 
    f.Folio,
    f.FechaSolicitud,
    f.TotalImporte,
    f.Solicitante,
    d.Codigo,
    d.Descripcion,
    d.Cantidad,
    d.PrecioUnitario,
    d.Importe
FROM FoliosSalidaAlmacen f
INNER JOIN FoliosSalidaAlmacenDetalle d ON f.Id = d.FolioId
WHERE f.Folio = 'VALE-M1-L1-001-2025'
```

### Total de salidas por casa

```sql
SELECT 
    Manzana,
    Lote,
    COUNT(*) AS TotalVales,
    SUM(TotalImporte) AS SumaTotal
FROM FoliosSalidaAlmacen
GROUP BY Manzana, Lote
ORDER BY SumaTotal DESC
```

### Vales pendientes de entrega

```sql
SELECT * FROM FoliosSalidaAlmacen 
WHERE Estado = 'PENDIENTE' 
ORDER BY FechaSolicitud ASC
```

### Ver tamaño de PDFs almacenados

```sql
SELECT 
    Folio,
    NombreArchivo,
    TamanioBytes / 1024.0 / 1024.0 AS TamanioMB,
    FechaAlmacenamiento
FROM PDFsSalidaAlmacen 
ORDER BY TamanioBytes DESC
```

---

## ?? Pasos de Instalación

### 1. Ejecutar Script SQL

```sql
-- Ejecutar: SQL_SCRIPTS\CrearTablasRepositorioSalidasAlmacen.sql
-- Esto crea las 3 tablas, la vista y el procedimiento almacenado
```

### 2. Verificar Archivos en el Proyecto

Asegurarse de que los siguientes archivos estén incluidos:
- ? `FormAlmacen_ExtensionPDFSalidas.cs`
- ? `FormRepositorioValesSalida.cs`
- ? `FormRepositorioValesSalida.Designer.cs`
- ? `FormDetalleValeSalida.cs`
- ? `FormDetalleValeSalida.Designer.cs`
- ? `SQL_SCRIPTS\CrearTablasRepositorioSalidasAlmacen.sql`

### 3. Compilar Proyecto

```bash
# El proyecto ya compiló exitosamente ?
```

### 4. Probar Funcionalidad

#### Test 1: Generar Vale de Salida

1. Abrir `FormAlmacen`
2. Ir a pestaña "Salidas"
3. Seleccionar Manzana y Lote (ej: M1, L1)
4. Click en "Seleccionar Casa"
5. Se cargan los insumos disponibles
6. Capturar cantidades a solicitar
7. Click en "Registrar Salida"
8. Sistema solicita datos del vale
9. Llenar formulario y click "Generar Vale"
10. ? Verificar que se genera el PDF
11. ? Verificar que se guarda en BD

#### Test 2: Ver Repositorio

1. Ejecutar consulta SQL para verificar:
   ```sql
   SELECT * FROM FoliosSalidaAlmacen ORDER BY FechaSolicitud DESC
   ```
2. Verificar que aparezca el vale generado
3. Ejecutar:
   ```sql
   SELECT * FROM PDFsSalidaAlmacen ORDER BY FechaAlmacenamiento DESC
   ```
4. Verificar que el PDF esté almacenado

#### Test 3: Consultar desde Repositorio

1. Llamar a `AbrirRepositorioValesSalida()` desde código
2. Verificar que aparezca el vale en la lista
3. Doble-clic o "Ver PDF" para abrir el PDF
4. Click en "Detalle" para ver información completa
5. Verificar filtros de búsqueda y fecha

#### Test 4: Eliminar Vale

1. En el repositorio, seleccionar un vale
2. Click en "??? Eliminar"
3. Confirmar eliminación
4. Verificar que se elimine (CASCADE borra detalle y PDF)

---

## ? Ventajas del Sistema

1. **?? Formato Profesional** - Vale con diseño corporativo
2. **?? Folios Únicos** - Numeración automática correlativa
3. **?? Almacenamiento Seguro** - PDFs en base de datos
4. **?? Trazabilidad Completa** - Historial consultable
5. **?? Responsables Registrados** - Solicitante, residente, encargado
6. **?? Control de Costos** - Precios e importes por insumo
7. **??? Repositorio Organizado** - Filtros y búsqueda avanzada
8. **?? Eliminación en Cascada** - Limpieza automática de registros
9. **?? Reportes Fáciles** - Vistas y consultas predefinidas
10. **?? Respaldo Automático** - Los PDFs no se pierden

---

## ?? Mejoras Futuras (Opcional)

- [ ] Agregar firma digital en los vales
- [ ] Escanear código QR para validar el vale
- [ ] Notificaciones por correo al generar vale
- [ ] Integración con sistema de entregas
- [ ] Historial de cambios de estado
- [ ] Exportar múltiples vales en ZIP
- [ ] Gráficas de salidas por período
- [ ] Comparativa de consumos por casa
- [ ] Alertas de inventario bajo
- [ ] Integración con app móvil

---

## ?? Notas Importantes

### Rendimiento
- Los PDFs se almacenan como `VARBINARY(MAX)` en SQL Server
- Tamaño máximo por PDF: ~2 GB (límite de VARBINARY(MAX))
- Los PDFs típicos de vales pesan entre 50-200 KB
- Se crean índices en las columnas de filtro frecuente

### Seguridad
- Los PDFs están seguros en la BD (respaldos automáticos)
- Se registra el usuario que generó cada vale
- Las eliminaciones requieren confirmación
- Los folios son únicos e irrepetibles

### Mantenimiento
- Revisar periódicamente el tamaño de `PDFsSalidaAlmacen`
- Considerar archivar vales antiguos si el volumen crece
- Mantener backups regulares de la base de datos
- Ejecutar consultas de optimización periódicamente

---

**Desarrollado para**: Sistema Calandria Residencial  
**Fecha**: Enero 2025  
**Versión**: 1.0  
**Estado**: ? LISTO PARA PRODUCCIÓN  
**Compilación**: ? EXITOSA

---

## ?? ¡Sistema Completo!

Ahora tienes un sistema robusto de gestión de vales de salida con:
- ? Folios únicos automáticos
- ? Generación de PDF con formato profesional
- ? Almacenamiento de PDFs en BD
- ? Repositorio visual navegable
- ? Detalle completo de cada vale
- ? Trazabilidad total por casa
- ? Control de costos por insumo
- ? Interfaz moderna con tema corporativo
- ? Integración perfecta con FormAlmacen

**¡Listo para usar en producción!** ??
