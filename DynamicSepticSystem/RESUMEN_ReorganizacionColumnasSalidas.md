# ?? RESUMEN - Reorganización de Columnas en Salidas de Almacén

## ? Cambios Completados

Se ha reorganizado el orden y renombrado las columnas en el módulo de **Salidas de Almacén** según las especificaciones solicitadas.

---

## ?? Nuevo Orden de Columnas (Izquierda ? Derecha)

| # | Columna Interna | Nombre Mostrado | Ancho | Formato | Editable | Descripción |
|---|----------------|-----------------|-------|---------|----------|-------------|
| 1 | `Clave` | **?? Código** | 100px | Texto | ? No | Código del insumo |
| 2 | `Descripcion` | **?? Insumo** | 280px | Texto | ? No | Descripción del insumo |
| 3 | `Unidad` | **?? Unidad** | 70px | Texto | ? No | Unidad de medida |
| 4 | `PrecioUnitario` | **?? Precio Unitario** | 110px | C2 | ? No | Precio promedio de entradas |
| 5 | `Importe` | **?? Importe** | 100px | C2 | ? No | Calculado: Precio × Cantidad |
| 6 | `Disponible` | **?? Almacén** | 90px | N2 | ? No | Cantidad disponible |
| 7 | `MaximoPermitido` | **?? Máximo Permitido** | 120px | N2 | ? No | De la explosión de insumos |
| 8 | `CantidadSolicitada` | **?? Cantidad a Solicitar** | 140px | N2 | ? Sí | Campo editable por usuario |

---

## ?? Cambios en Nombres de Columnas

### Antes ? Después

| Nombre Anterior | Nombre Nuevo |
|----------------|-------------|
| ?? Clave | ?? **Código** |
| ?? Descripción | ?? **Insumo** |
| ?? P.U. | ?? **Precio Unitario** |
| ?? Disponible | ?? **Almacén** |
| ?? Máximo | ?? **Máximo Permitido** |
| ?? Cantidad Solicitada | ?? **Cantidad a Solicitar** |

---

## ?? Lógica de Columnas

### 1. **Código** (Clave)
- Identificador único del insumo
- Usado para búsquedas y filtros
- Ejemplo: `CEM001`, `VAR001`

### 2. **Insumo** (Descripción)
- Nombre completo del material
- Ejemplo: "Cemento gris 50kg"

### 3. **Unidad**
- Unidad de medida
- Centrado en celda
- Ejemplos: `TON`, `PZA`, `M3`

### 4. **Precio Unitario**
- Precio promedio calculado desde `EntradasAlmacen`
- Formato: `$#,##0.00`
- Fondo verde claro (#F0FFF0)
- **Origen**: Query SQL con `AVG(e.PrecioUnitario)`

### 5. **Importe** ?
- **Cálculo automático**: `PrecioUnitario × CantidadSolicitada`
- Se actualiza en tiempo real al editar cantidad
- Formato: `$#,##0.00` en negrita
- Fondo verde claro (#F0FFF0)

### 6. **Almacén** (Disponible)
- Cantidad actual en inventario
- **Cálculo**: `SUM(Entradas) - SUM(Salidas)`
- Formato: `N2` (dos decimales)

### 7. **Máximo Permitido**
- Cantidad máxima permitida según explosión de insumos
- Depende del prototipo de casa (CALANDRIA o TUNERA)
- Color naranja rojizo para advertencia
- Formato en negrita
- Si no hay explosión: muestra `999,999.00`

### 8. **Cantidad a Solicitar**
- **Única columna editable** ??
- Fondo amarillo claro (#FFFFDC) para destacar
- Al editar, calcula automáticamente el Importe

---

## ?? Implementación Técnica

### Orden de Columnas con DisplayIndex

```csharp
// 1. Código
dgvInsumos.Columns["Clave"].DisplayIndex = 0;

// 2. Insumo
dgvInsumos.Columns["Descripcion"].DisplayIndex = 1;

// 3. Unidad
dgvInsumos.Columns["Unidad"].DisplayIndex = 2;

// 4. Precio Unitario
dgvInsumos.Columns["PrecioUnitario"].DisplayIndex = 3;

// 5. Importe
dgvInsumos.Columns["Importe"].DisplayIndex = 4;

// 6. Almacén
dgvInsumos.Columns["Disponible"].DisplayIndex = 5;

// 7. Máximo Permitido
dgvInsumos.Columns["MaximoPermitido"].DisplayIndex = 6;

// 8. Cantidad a Solicitar
dgvInsumos.Columns["CantidadSolicitada"].DisplayIndex = 7;
```

### Cálculo Automático de Importe

```csharp
private void DgvInsumos_CellValueChanged(object sender, DataGridViewCellEventArgs e)
{
    if (dgvInsumos.Columns[e.ColumnIndex].Name == "CantidadSolicitada")
    {
        var row = dgvInsumos.Rows[e.RowIndex];
        decimal cantidad = Convert.ToDecimal(row.Cells["CantidadSolicitada"].Value);
        decimal precio = Convert.ToDecimal(row.Cells["PrecioUnitario"].Value);
        
        // Cálculo automático
        row.Cells["Importe"].Value = cantidad * precio;
    }
}
```

---

## ?? Vista Previa del Grid

```
????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????
? Código   ? Insumo                 ? Unidad ? Precio Unitario? Importe    ? Almacén    ? Máximo Permitido? Cantidad a Solicitar?
????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????
? CEM001   ? Cemento gris 50kg      ? TON    ? $150.00        ? $0.00      ? 100.00     ? 50.00           ? [editable]         ?
? VAR001   ? Varilla 3/8            ? TON    ? $22,500.00     ? $0.00      ? 25.00      ? 10.00           ? [editable]         ?
? BLK001   ? Block hueco 15x20x40   ? PZA    ? $8.50          ? $0.00      ? 5,000.00   ? 2,000.00        ? [editable]         ?
????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????
```

---

## ?? Estilos Visuales

### Colores de Fondo
- **Blanco**: Columnas de solo lectura (Código, Insumo, Unidad, Almacén)
- **Verde claro (#F0FFF0)**: Columnas monetarias (Precio Unitario, Importe)
- **Amarillo claro (#FFFFDC)**: Columna editable (Cantidad a Solicitar)

### Fuentes Especiales
- **Negrita**: Importe, Máximo Permitido
- **Color Naranja Rojizo**: Máximo Permitido (advertencia)

### Alineación
- **Centro**: Unidad
- **Derecha**: Precio Unitario, Importe, Almacén, Máximo Permitido, Cantidad a Solicitar
- **Izquierda**: Código, Insumo

---

## ?? Flujo de Trabajo del Usuario

1. **Seleccionar Casa** ? Sistema carga explosión de insumos del prototipo
2. **Ver Columnas Ordenadas**:
   - Identifica el insumo por Código e Insumo
   - Ve el Precio Unitario (promedio de entradas)
   - Verifica cuánto hay en Almacén
   - Consulta el Máximo Permitido según explosión
3. **Capturar Cantidad a Solicitar** ??
4. **Ver Importe Calculado Automáticamente** ??
5. **Validaciones**:
   - ?? Si excede Máximo Permitido ? Alerta
   - ?? Si excede Almacén ? Pregunta confirmación
6. **Registrar Salida** ? Guarda en `SalidasAlmacen` y `HistorialMovimientos`

---

## ?? Ejemplo Práctico

### Escenario: Salida de Cemento para Casa TUNERA

| Código | Insumo | Unidad | Precio Unitario | Importe | Almacén | Máximo Permitido | Cantidad a Solicitar |
|--------|--------|--------|----------------|---------|---------|-----------------|---------------------|
| CEM001 | Cemento gris 50kg | TON | $150.00 | **$3,750.00** | 100.00 | 50.00 | **25.00** ?? |

**Cálculo**: `25.00 TON × $150.00 = $3,750.00`

---

## ? Validaciones Implementadas

### 1. Validación de Máximo Permitido
```csharp
if (maximoPermitido < 999999 && solicitada > maximoPermitido)
{
    MessageBox.Show("?? La cantidad solicitada excede el máximo permitido.");
}
```

### 2. Validación de Almacén
```csharp
if (solicitada > disponible)
{
    MessageBox.Show("?? La cantidad excede el inventario disponible.\n¿Desea continuar?");
}
```

---

## ??? Estructura de Datos

### Query SQL Actualizado

```sql
SELECT 
    e.Clave,                    -- Código
    e.Descripcion,              -- Insumo
    e.Unidad,                   -- Unidad
    AVG(e.PrecioUnitario) AS PrecioUnitario,  -- Precio Unitario
    ISNULL(SUM(e.Cantidad), 0) - 
    ISNULL((SELECT SUM(sa.Cantidad) 
            FROM SalidasAlmacen sa 
            WHERE sa.Clave = e.Clave), 0) AS Disponible  -- Almacén
FROM EntradasAlmacen e
GROUP BY e.Clave, e.Descripcion, e.Unidad
HAVING ISNULL(SUM(e.Cantidad), 0) - 
       ISNULL((SELECT SUM(sa.Cantidad) 
               FROM SalidasAlmacen sa 
               WHERE sa.Clave = e.Clave), 0) > 0
ORDER BY e.Clave
```

---

## ?? Notas Importantes

1. **Máximo Permitido**:
   - Se obtiene del diccionario `explosion` cargado desde el prototipo
   - Si no existe el insumo en la explosión: muestra `999,999.00`
   - Depende de `CargarExplosionParaSalida(prototipo)`

2. **Precio Unitario**:
   - Calculado como **promedio** de todas las entradas del insumo
   - Si un insumo tiene múltiples entradas con diferentes precios, usa el promedio
   - Ejemplo: Entrada1=$150, Entrada2=$160 ? Precio=$155

3. **Importe**:
   - Se actualiza **en tiempo real** mientras se edita la cantidad
   - Usa el evento `CellValueChanged`
   - Formato en negrita para mayor visibilidad

4. **Orden de Columnas**:
   - Se establece con `DisplayIndex` para garantizar el orden visual
   - Independiente del orden interno del DataTable

---

## ?? Archivos Modificados

- ? `FormAlmacen_Salidas.cs` - Reorganización completa de columnas

---

## ?? Pruebas Realizadas

- [x] Compilación exitosa
- [x] Sin errores de sintaxis
- [x] Orden de columnas correcto
- [x] Nombres de columnas actualizados
- [x] Cálculo automático de importe funcional

---

## ?? Beneficios de la Reorganización

1. ? **Flujo lógico**: Información de identificación ? Precios ? Disponibilidad ? Captura
2. ? **Mejor visibilidad**: Precio e Importe juntos para análisis rápido
3. ? **Intuitividad**: Cantidad a Solicitar al final (última acción del usuario)
4. ? **Claridad**: Nombres descriptivos ("Almacén" en lugar de "Disponible")

---

**Fecha de Implementación:** Enero 2025  
**Versión:** 1.3 (Reorganización de Columnas)  
**Estado:** ? COMPLETADO Y COMPILADO  
**Sistema:** Gestión de Almacén - Calandria Residencial
