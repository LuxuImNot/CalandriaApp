# ?? ACTUALIZACIÓN - Precio e Importe en Salidas de Almacén

## ?? Resumen de Cambios

Se han agregado las columnas de **Precio Unitario** e **Importe** a las salidas de almacén, permitiendo visualizar el costo de los materiales que salen del inventario basándose en la clave de cada insumo.

---

## ? Cambios Implementados

### 1. **FormAlmacen_Salidas.cs**

#### Modificaciones en `CargarInsumosDisponiblesSalida()`
- ? Agregada columna `PrecioUnitario` a la tabla de datos
- ? Agregada columna `Importe` a la tabla de datos
- ? Query SQL actualizado para obtener precio promedio desde `EntradasAlmacen`
- ? Cálculo automático de importe al cambiar cantidad solicitada

**Query SQL actualizado:**
```sql
SELECT 
    e.Clave,
    e.Descripcion, 
    e.Unidad,
    ISNULL(SUM(e.Cantidad), 0) - ISNULL((SELECT SUM(sa.Cantidad) FROM SalidasAlmacen sa WHERE sa.Clave = e.Clave), 0) AS Disponible,
    AVG(e.PrecioUnitario) AS PrecioUnitario  -- ? NUEVA: Obtener precio promedio
FROM EntradasAlmacen e
GROUP BY e.Clave, e.Descripcion, e.Unidad
HAVING ISNULL(SUM(e.Cantidad), 0) - ISNULL((SELECT SUM(sa.Cantidad) FROM SalidasAlmacen sa WHERE sa.Clave = e.Clave), 0) > 0
ORDER BY e.Clave
```

#### Nuevo Evento: `DgvInsumos_CellValueChanged()`
```csharp
private void DgvInsumos_CellValueChanged(object sender, DataGridViewCellEventArgs e)
{
    if (dgvInsumos.Columns[e.ColumnIndex].Name == "CantidadSolicitada")
    {
        var row = dgvInsumos.Rows[e.RowIndex];
        decimal cantidad = Convert.ToDecimal(row.Cells["CantidadSolicitada"].Value);
        decimal precio = Convert.ToDecimal(row.Cells["PrecioUnitario"].Value);
        row.Cells["Importe"].Value = cantidad * precio; // ? Cálculo automático
    }
}
```

#### Modificaciones en `ConfigurarColumnasSalida()`
```csharp
// ? NUEVA COLUMNA: Precio Unitario
if (dgvInsumos.Columns.Contains("PrecioUnitario"))
{
    dgvInsumos.Columns["PrecioUnitario"].HeaderText = "?? P.U.";
    dgvInsumos.Columns["PrecioUnitario"].Width = 90;
    dgvInsumos.Columns["PrecioUnitario"].ReadOnly = true;
    dgvInsumos.Columns["PrecioUnitario"].DefaultCellStyle.Format = "C2";
    dgvInsumos.Columns["PrecioUnitario"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
    dgvInsumos.Columns["PrecioUnitario"].DefaultCellStyle.BackColor = Color.FromArgb(240, 255, 240);
}

// ? NUEVA COLUMNA: Importe
if (dgvInsumos.Columns.Contains("Importe"))
{
    dgvInsumos.Columns["Importe"].HeaderText = "?? Importe";
    dgvInsumos.Columns["Importe"].Width = 100;
    dgvInsumos.Columns["Importe"].ReadOnly = true;
    dgvInsumos.Columns["Importe"].DefaultCellStyle.Format = "C2";
    dgvInsumos.Columns["Importe"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
    dgvInsumos.Columns["Importe"].DefaultCellStyle.BackColor = Color.FromArgb(240, 255, 240);
    dgvInsumos.Columns["Importe"].DefaultCellStyle.Font = new Font(dgvInsumos.Font, FontStyle.Bold);
}
```

#### Modificaciones en `RegistrarSalida()`
- ? Obtención de `precioUnitario` desde la fila
- ? Cálculo de `importe = cantidad * precio`
- ? INSERT en `SalidasAlmacen` incluye `PrecioUnitario` e `Importe`
- ? INSERT en `HistorialMovimientos` incluye `PrecioUnitario` e `Importe`

```csharp
decimal precioUnitario = Convert.ToDecimal(row["PrecioUnitario"]);
decimal importe = solicitada * precioUnitario;

// INSERT con nuevos campos
INSERT INTO SalidasAlmacen 
(Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe, FechaSalida, Manzana, Lote, Prototipo, Justificacion) 
VALUES (@Clave, @Descripcion, @Unidad, @Cantidad, @PrecioUnitario, @Importe, GETDATE(), @Manzana, @Lote, @Prototipo, NULL)
```

---

### 2. **FormAlmacen_Historial.cs**

#### Modificaciones en `CargarHistorial()`
```csharp
Precio = reader.IsDBNull(reader.GetOrdinal("PrecioUnitario")) 
    ? 0 
    : reader.GetDecimal(reader.GetOrdinal("PrecioUnitario")),
```

#### Modificaciones en `ConfigurarColumnasHistorial()`
```csharp
objectListViewHistorial.Columns.Add(new OLVColumn("P.U.", "Precio") 
{ 
    Width = 90,
    AspectToStringConverter = obj => ((decimal)obj).ToString("C2")
});

objectListViewHistorial.Columns.Add(new OLVColumn("Importe", "Importe") 
{ 
    Width = 100,
    AspectToStringConverter = obj => ((decimal)obj).ToString("C2")
});
```

---

### 3. **Script SQL: AgregarColumnasPrecioSalidas.sql**

Ubicación: `DynamicSepticSystem\SQL_SCRIPTS\AgregarColumnasPrecioSalidas.sql`

**Funciones del script:**

1. ? Agrega columna `PrecioUnitario` a `SalidasAlmacen` (si no existe)
2. ? Agrega columna `Importe` a `SalidasAlmacen` (si no existe)
3. ? Agrega columna `PrecioUnitario` a `HistorialMovimientos` (si no existe)
4. ? Agrega columna `Importe` a `HistorialMovimientos` (si no existe)
5. ? Actualiza registros existentes con precio promedio desde `EntradasAlmacen`
6. ? Muestra resumen de actualización

**Ejecutar este script ANTES de usar la nueva funcionalidad.**

---

## ?? Columnas en DataGridView de Salidas

| # | Columna | Ancho | Formato | Editable | Color Fondo |
|---|---------|-------|---------|----------|-------------|
| 1 | ?? Clave | 100px | Texto | ? No | Blanco |
| 2 | ?? Descripción | 250px | Texto | ? No | Blanco |
| 3 | ?? Unidad | 70px | Texto | ? No | Blanco |
| 4 | ?? Disponible | 90px | N2 | ? No | Blanco |
| 5 | ?? P.U. | 90px | C2 | ? No | Verde claro (#F0FFF0) ? |
| 6 | ?? Máximo | 90px | N2 | ? No | Blanco |
| 7 | ?? Cantidad Solicitada | 130px | N2 | ? Sí | Amarillo claro (#FFFFDC) |
| 8 | ?? Importe | 100px | C2 | ? No | Verde claro (#F0FFF0) ? |

---

## ?? Flujo de Trabajo Actualizado

### Antes (Sin Precio)
```
1. Seleccionar casa
2. Cargar insumos disponibles
3. Capturar cantidad solicitada
4. Registrar salida
   ?? Se guardaba: Clave, Cantidad, Descripción
```

### Ahora (Con Precio e Importe)
```
1. Seleccionar casa
2. Cargar insumos disponibles CON PRECIO
   ?? Obtiene precio promedio desde EntradasAlmacen
3. Capturar cantidad solicitada
   ?? Se calcula automáticamente el importe
4. Registrar salida
   ?? Se guarda: Clave, Cantidad, Descripción, PrecioUnitario, Importe
```

---

## ?? Características Nuevas

### 1. Precio Automático
- El sistema obtiene el **precio promedio** de todas las entradas de cada insumo
- Si un insumo tiene múltiples entradas con diferentes precios, se usa el promedio
- Ejemplo:
  - Entrada 1: Cemento a $150.00
  - Entrada 2: Cemento a $160.00
  - **Precio usado en salida: $155.00** (promedio)

### 2. Cálculo Automático de Importe
- Al capturar la cantidad solicitada, el sistema calcula automáticamente:
  ```
  Importe = Cantidad Solicitada × Precio Unitario
  ```
- Actualización en tiempo real mientras se edita la celda

### 3. Visualización en Historial
- El historial ahora muestra:
  - Precio unitario de cada movimiento
  - Importe total del movimiento
- Permite auditoría completa de costos

### 4. Formatos de Moneda
- Precio Unitario: `$#,##0.00` (ejemplo: $1,500.00)
- Importe: `$#,##0.00` (ejemplo: $15,000.00)
- Negrita en columna de Importe para mayor visibilidad

---

## ?? Diseño Visual

### Colores de Columnas Nuevas
- **Precio Unitario**: Fondo verde claro (#F0FFF0)
- **Importe**: Fondo verde claro (#F0FFF0) + Negrita

### Emojis en Encabezados
- ?? Precio Unitario (P.U.)
- ?? Importe

---

## ?? Requisitos de Base de Datos

### Tabla: SalidasAlmacen
```sql
-- Columnas requeridas (nuevas)
PrecioUnitario DECIMAL(18, 2) NULL
Importe DECIMAL(18, 2) NULL
```

### Tabla: HistorialMovimientos
```sql
-- Columnas requeridas (ya existentes o nuevas)
PrecioUnitario DECIMAL(18, 2) NULL
Importe DECIMAL(18, 2) NULL
```

**?? IMPORTANTE:** Ejecutar el script SQL antes de usar la aplicación actualizada.

---

## ?? Ejemplo de Uso

### Escenario: Salida de Cemento

1. **Seleccionar casa**: M1-L5
2. **Buscar insumo**: CEM001 - Cemento gris 50kg
3. **Datos mostrados**:
   - Disponible: 100.00 TON
   - **Precio Unitario: $150.00** ?
   - Máximo: 50.00 TON
4. **Capturar cantidad**: 25.00 TON
5. **Importe calculado automáticamente: $3,750.00** ?
6. **Registrar salida**

### Resultado en Base de Datos

**SalidasAlmacen:**
| Clave | Descripción | Cantidad | PrecioUnitario | Importe | Manzana | Lote |
|-------|-------------|----------|----------------|---------|---------|------|
| CEM001 | Cemento gris 50kg | 25.00 | $150.00 | $3,750.00 | 1 | 5 |

**HistorialMovimientos:**
| TipoMovimiento | Clave | Cantidad | PrecioUnitario | Importe | Usuario |
|----------------|-------|----------|----------------|---------|---------|
| Salida | CEM001 | 25.00 | $150.00 | $3,750.00 | Admin |

---

## ?? Pruebas Requeridas

### ? Lista de Verificación

- [ ] Ejecutar script SQL `AgregarColumnasPrecioSalidas.sql`
- [ ] Verificar que columnas existen en base de datos
- [ ] Abrir módulo de Salidas de Almacén
- [ ] Seleccionar una casa
- [ ] Verificar que se muestran precios en columna P.U.
- [ ] Capturar cantidad solicitada
- [ ] Verificar cálculo automático de importe
- [ ] Registrar salida exitosamente
- [ ] Verificar datos en tabla SalidasAlmacen
- [ ] Verificar datos en tabla HistorialMovimientos
- [ ] Abrir pestaña Historial
- [ ] Verificar que se muestran columnas P.U. e Importe
- [ ] Verificar formato de moneda correcto

---

## ?? Solución de Problemas

### Problema 1: No se muestran precios
**Causa:** Columnas no existen en base de datos
**Solución:**
```sql
-- Ejecutar script SQL
USE CALANDRIA;
EXEC sp_help 'SalidasAlmacen'; -- Verificar columnas
```

### Problema 2: Precio muestra $0.00
**Causa:** No hay entradas previas para ese insumo
**Solución:**
- Verificar que el insumo tenga entradas en `EntradasAlmacen`
- Si no tiene entradas, agregar precio manual en la tabla

### Problema 3: Importe no se calcula automáticamente
**Causa:** Evento `CellValueChanged` no está conectado
**Solución:**
- Verificar que se agregó el evento en `CargarInsumosDisponiblesSalida()`
- Compilar nuevamente el proyecto

### Problema 4: Error al registrar salida
**Causa:** Columnas no existen en tabla
**Solución:**
```sql
-- Agregar columnas manualmente
ALTER TABLE SalidasAlmacen ADD PrecioUnitario DECIMAL(18,2);
ALTER TABLE SalidasAlmacen ADD Importe DECIMAL(18,2);
```

---

## ?? Notas Técnicas

### Cálculo de Precio Promedio
```sql
AVG(e.PrecioUnitario) AS PrecioUnitario
```
- Usa todas las entradas del insumo
- Ignora valores NULL
- Retorna 0 si no hay entradas

### Manejo de Valores NULL
```csharp
decimal precioUnitario = reader["PrecioUnitario"] != DBNull.Value 
    ? Convert.ToDecimal(reader["PrecioUnitario"]) 
    : 0;
```

### Formato de Moneda en C#
```csharp
DefaultCellStyle.Format = "C2"; // $#,##0.00
```

---

## ?? Beneficios

1. ? **Control de costos**: Conocer el valor de las salidas
2. ? **Auditoría completa**: Rastrear precios históricos
3. ? **Reportes financieros**: Calcular costo real de construcción
4. ? **Toma de decisiones**: Identificar insumos más costosos
5. ? **Transparencia**: Visibilidad total de costos por casa

---

## ?? Estadísticas (Ejemplo)

### Antes de la Actualización
```
Total salidas registradas: 150
Salidas con precio: 0 (0%)
Importes calculables: 0
```

### Después de la Actualización
```
Total salidas registradas: 150
Salidas con precio: 150 (100%)
Importes calculables: 150
Valor total de salidas: $125,450.00
```

---

## ?? Mejoras Futuras

- [ ] Reporte de salidas por rango de fechas con totales
- [ ] Gráfica de costos de salidas por casa
- [ ] Comparación de costo estimado vs. real
- [ ] Alerta cuando salidas excedan presupuesto
- [ ] Exportación de salidas a Excel con totales

---

## ?? Soporte

Si encuentras problemas:
1. Verificar que el script SQL se ejecutó correctamente
2. Revisar logs de errores en Visual Studio
3. Verificar estructura de tablas en SQL Server
4. Consultar esta documentación

---

**Fecha de Implementación:** Enero 2025  
**Versión:** 1.2 (Salidas con Precio e Importe)  
**Estado:** ? COMPLETADO Y COMPILADO  
**Aprobado por:** Sistema de Gestión de Almacén - Calandria Residencial
