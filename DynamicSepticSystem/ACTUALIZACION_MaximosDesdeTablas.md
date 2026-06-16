# ?? ACTUALIZACIÓN - Máximos Permitidos desde Tablas de Compras

## ? Cambio Implementado

Se ha modificado el sistema de **Salidas de Almacén** para que obtenga los **máximos permitidos** directamente desde las tablas `COMPRASCALANDRA` y `COMPRASTUNERA` en lugar de usar la explosión de insumos en memoria.

---

## ?? Problema Anterior

### Antes:
```csharp
// Obtenía el máximo desde el diccionario explosion en memoria
double maximoPermitido = 0;
if (explosion != null && explosion.ContainsKey(clave))
{
    maximoPermitido = explosion[clave];
}
```

**Limitaciones:**
- ? Dependía de cargar la explosión de insumos en memoria
- ? Requería el método `CargarExplosionParaSalida(prototipo)`
- ? Datos en memoria podían no estar sincronizados con la base de datos

---

## ? Solución Implementada

### Ahora:
```csharp
// Determina la tabla según el prototipo
string tablaCompras = "";
if (casaInventario.Prototipo.ToUpper().Contains("CALANDRIA"))
{
    tablaCompras = "COMPRASCALANDRA";
}
else if (casaInventario.Prototipo.ToUpper().Contains("TUNERA"))
{
    tablaCompras = "COMPRASTUNERA";
}

// Consulta SQL con subconsulta para obtener el máximo
SELECT 
    e.Clave,
    e.Descripcion, 
    e.Unidad,
    ISNULL(SUM(e.Cantidad), 0) - ISNULL((SELECT SUM(sa.Cantidad) FROM SalidasAlmacen sa WHERE sa.Clave = e.Clave), 0) AS Disponible,
    AVG(e.PrecioUnitario) AS PrecioUnitario,
    ISNULL((SELECT TOP 1 Cantidad FROM COMPRASCALANDRA WHERE Clave = e.Clave), 0) AS MaximoPermitido
FROM EntradasAlmacen e
GROUP BY e.Clave, e.Descripcion, e.Unidad
HAVING ISNULL(SUM(e.Cantidad), 0) - ISNULL((SELECT SUM(sa.Cantidad) FROM SalidasAlmacen sa WHERE sa.Clave = e.Clave), 0) > 0
ORDER BY e.Clave
```

**Ventajas:**
- ? Datos siempre actualizados desde la base de datos
- ? No requiere cargar explosión en memoria
- ? Consulta única obtiene todos los datos necesarios
- ? Más eficiente y mantenible

---

## ??? Lógica de Selección de Tabla

### Criterio de Selección

```csharp
if (casaInventario.Prototipo.ToUpper().Contains("CALANDRIA"))
{
    tablaCompras = "COMPRASCALANDRA";
}
else if (casaInventario.Prototipo.ToUpper().Contains("TUNERA"))
{
    tablaCompras = "COMPRASTUNERA";
}
else
{
    // Si no es ninguno de los dos, MaximoPermitido = 0
    tablaCompras = "";
}
```

### Ejemplos de Prototipos

| Prototipo | Tabla Usada | Máximos Desde |
|-----------|-------------|---------------|
| CALANDRIA | `COMPRASCALANDRA` | ? Tabla específica |
| CALANDRIA PLUS | `COMPRASCALANDRA` | ? Tabla específica |
| TUNERA | `COMPRASTUNERA` | ? Tabla específica |
| TUNERA PLUS | `COMPRASTUNERA` | ? Tabla específica |
| Otro prototipo | (ninguna) | ?? MaximoPermitido = 0 |

---

## ?? Estructura de las Tablas

### Tabla: COMPRASCALANDRA
```sql
CREATE TABLE COMPRASCALANDRA (
    Clave NVARCHAR(50),
    Descripcion NVARCHAR(255),
    Unidad NVARCHAR(20),
    Cantidad DECIMAL(18, 2),  -- ? Este es el máximo permitido
    ...
)
```

### Tabla: COMPRASTUNERA
```sql
CREATE TABLE COMPRASTUNERA (
    Clave NVARCHAR(50),
    Descripcion NVARCHAR(255),
    Unidad NVARCHAR(20),
    Cantidad DECIMAL(18, 2),  -- ? Este es el máximo permitido
    ...
)
```

---

## ?? Consulta SQL Detallada

### Para Casas CALANDRIA
```sql
SELECT 
    e.Clave,
    e.Descripcion, 
    e.Unidad,
    -- Disponible en almacén
    ISNULL(SUM(e.Cantidad), 0) - ISNULL((
        SELECT SUM(sa.Cantidad) 
        FROM SalidasAlmacen sa 
        WHERE sa.Clave = e.Clave
    ), 0) AS Disponible,
    
    -- Precio promedio
    AVG(e.PrecioUnitario) AS PrecioUnitario,
    
    -- ? Máximo permitido desde COMPRASCALANDRA
    ISNULL((
        SELECT TOP 1 Cantidad 
        FROM COMPRASCALANDRA 
        WHERE Clave = e.Clave
    ), 0) AS MaximoPermitido
FROM EntradasAlmacen e
GROUP BY e.Clave, e.Descripcion, e.Unidad
HAVING ISNULL(SUM(e.Cantidad), 0) - ISNULL((
    SELECT SUM(sa.Cantidad) 
    FROM SalidasAlmacen sa 
    WHERE sa.Clave = e.Clave
), 0) > 0
ORDER BY e.Clave
```

### Para Casas TUNERA
```sql
-- Misma consulta pero con:
ISNULL((
    SELECT TOP 1 Cantidad 
    FROM COMPRASTUNERA  -- ? Tabla diferente
    WHERE Clave = e.Clave
), 0) AS MaximoPermitido
```

---

## ?? Flujo de Trabajo Actualizado

### 1. Usuario Selecciona Casa
```
Casa: M1-L5
Prototipo: CALANDRIA
```

### 2. Sistema Determina Tabla
```csharp
if (Prototipo.Contains("CALANDRIA"))
    ? tablaCompras = "COMPRASCALANDRA"
```

### 3. Consulta SQL con Máximos
```sql
SELECT ..., 
    (SELECT TOP 1 Cantidad FROM COMPRASCALANDRA WHERE Clave = e.Clave) AS MaximoPermitido
FROM EntradasAlmacen e
```

### 4. Resultados Mostrados

| Código | Insumo | Unidad | Precio Unitario | Importe | Almacén | **Máximo Permitido** | Cantidad a Solicitar |
|--------|--------|--------|----------------|---------|---------|---------------------|---------------------|
| CEM001 | Cemento gris 50kg | TON | $150.00 | $0.00 | 100.00 | **50.00** ? | [editable] |
| VAR001 | Varilla 3/8 | TON | $22,500.00 | $0.00 | 25.00 | **10.00** ? | [editable] |

**Nota**: Los máximos (50.00 y 10.00) vienen directamente de `COMPRASCALANDRA.Cantidad`

---

## ?? Código Modificado

### Método: `CargarInsumosDisponiblesSalida()`

```csharp
// Determinar la tabla de compras según el prototipo
string tablaCompras = "";
if (casaInventario.Prototipo.ToUpper().Contains("CALANDRIA"))
{
    tablaCompras = "COMPRASCALANDRA";
}
else if (casaInventario.Prototipo.ToUpper().Contains("TUNERA"))
{
    tablaCompras = "COMPRASTUNERA";
}

// Construcción dinámica de la consulta SQL
string sqlInventario = @"
    SELECT 
        e.Clave,
        e.Descripcion, 
        e.Unidad,
        ISNULL(SUM(e.Cantidad), 0) - ISNULL((SELECT SUM(sa.Cantidad) FROM SalidasAlmacen sa WHERE sa.Clave = e.Clave), 0) AS Disponible,
        AVG(e.PrecioUnitario) AS PrecioUnitario,
        " + (string.IsNullOrEmpty(tablaCompras) 
            ? "0" 
            : $"ISNULL((SELECT TOP 1 Cantidad FROM {tablaCompras} WHERE Clave = e.Clave), 0)") + @" AS MaximoPermitido
    FROM EntradasAlmacen e
    GROUP BY e.Clave, e.Descripcion, e.Unidad
    HAVING ISNULL(SUM(e.Cantidad), 0) - ISNULL((SELECT SUM(sa.Cantidad) FROM SalidasAlmacen sa WHERE sa.Clave = e.Clave), 0) > 0
    ORDER BY e.Clave";
```

### Lectura del Resultado

```csharp
decimal maximoPermitido = reader["MaximoPermitido"] != DBNull.Value
    ? Convert.ToDecimal(reader["MaximoPermitido"])
    : 0;

row["MaximoPermitido"] = maximoPermitido > 0 ? maximoPermitido : 999999;
```

---

## ??? Validación en Registro de Salida

```csharp
// Validar que no exceda el máximo permitido (si está definido)
if (maximoPermitido < 999999 && solicitada > maximoPermitido)
{
    MessageBox.Show(
        $"?? La cantidad solicitada para {clave} excede el máximo permitido.\n\n" +
        $"Solicitado: {solicitada:N2}\n" +
        $"Máximo permitido: {maximoPermitido:N2}", 
        "Cantidad excedida", 
        MessageBoxButtons.OK, 
        MessageBoxIcon.Warning
    );
    continue; // No registra esta salida
}
```

---

## ?? Casos de Uso

### Caso 1: Casa CALANDRIA con Insumo Válido

**Entrada:**
- Casa: M1-L5, Prototipo: CALANDRIA
- Insumo: CEM001
- Cantidad en COMPRASCALANDRA: 50.00 TON
- Disponible en Almacén: 100.00 TON
- Cantidad Solicitada: 25.00 TON

**Resultado:**
- ? Máximo Permitido: 50.00 TON (desde COMPRASCALANDRA)
- ? Validación: 25.00 ? 50.00 ? **Permitido**
- ? Salida registrada exitosamente

---

### Caso 2: Casa TUNERA con Insumo Excedido

**Entrada:**
- Casa: M2-L10, Prototipo: TUNERA
- Insumo: VAR001
- Cantidad en COMPRASTUNERA: 10.00 TON
- Disponible en Almacén: 25.00 TON
- Cantidad Solicitada: 15.00 TON

**Resultado:**
- ? Máximo Permitido: 10.00 TON (desde COMPRASTUNERA)
- ? Validación: 15.00 > 10.00 ? **Bloqueado**
- ?? Mensaje: "La cantidad solicitada excede el máximo permitido"
- ? Salida NO registrada

---

### Caso 3: Insumo Sin Máximo Definido

**Entrada:**
- Casa: M3-L15, Prototipo: CALANDRIA
- Insumo: NUEVO001 (no existe en COMPRASCALANDRA)
- Disponible en Almacén: 50.00 PZA
- Cantidad Solicitada: 30.00 PZA

**Resultado:**
- ?? Máximo Permitido: 999,999.00 (valor por defecto)
- ? Validación: Sin límite efectivo
- ? Solo valida disponibilidad en almacén
- ? Salida registrada si hay suficiente inventario

---

### Caso 4: Prototipo No Reconocido

**Entrada:**
- Casa: M4-L20, Prototipo: ESPECIAL
- Insumo: CEM001

**Resultado:**
- ?? Tabla: (ninguna)
- ?? Máximo Permitido: 999,999.00 (sin tabla de referencia)
- ? Solo valida disponibilidad en almacén

---

## ?? Ventajas de la Nueva Implementación

### 1. **Datos Siempre Actualizados**
- ? Consulta directa a la base de datos
- ? No hay riesgo de datos desactualizados en memoria

### 2. **Mayor Rendimiento**
- ? Una sola consulta SQL obtiene todo
- ? No requiere cargar explosión en memoria primero

### 3. **Mantenibilidad**
- ? Más fácil de entender y mantener
- ? Menos dependencias entre métodos

### 4. **Escalabilidad**
- ? Fácil agregar nuevos prototipos
- ? Solo requiere nueva tabla de compras

### 5. **Trazabilidad**
- ? Origen claro de los máximos permitidos
- ? Auditoría más sencilla

---

## ??? Requisitos de Base de Datos

### Tablas Requeridas

1. **COMPRASCALANDRA**
   - ? Debe existir
   - ? Debe tener columna `Clave`
   - ? Debe tener columna `Cantidad`

2. **COMPRASTUNERA**
   - ? Debe existir
   - ? Debe tener columna `Clave`
   - ? Debe tener columna `Cantidad`

### Verificar Existencia

```sql
-- Verificar tabla COMPRASCALANDRA
SELECT COUNT(*) FROM COMPRASCALANDRA;

-- Verificar tabla COMPRASTUNERA
SELECT COUNT(*) FROM COMPRASTUNERA;

-- Verificar estructura
EXEC sp_help 'COMPRASCALANDRA';
EXEC sp_help 'COMPRASTUNERA';
```

---

## ?? Pruebas Requeridas

### Lista de Verificación

- [x] Compilación exitosa
- [ ] Cargar casa con prototipo CALANDRIA
- [ ] Verificar máximos desde COMPRASCALANDRA
- [ ] Validar límite al capturar cantidad
- [ ] Cargar casa con prototipo TUNERA
- [ ] Verificar máximos desde COMPRASTUNERA
- [ ] Validar límite al capturar cantidad
- [ ] Probar insumo sin máximo definido
- [ ] Probar prototipo no reconocido
- [ ] Registrar salida exitosa respetando máximo
- [ ] Intentar exceder máximo y verificar bloqueo

---

## ?? Solución de Problemas

### Problema 1: Máximos siempre en 999,999
**Causa:** Tabla de compras no existe o nombre del prototipo no coincide

**Solución:**
```sql
-- Verificar prototipos en uso
SELECT DISTINCT Prototipo FROM InventarioCasas;

-- Verificar tablas
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('COMPRASCALANDRA', 'COMPRASTUNERA');
```

### Problema 2: Máximos en 0
**Causa:** Insumo no existe en la tabla de compras

**Solución:**
```sql
-- Verificar si el insumo existe
SELECT * FROM COMPRASCALANDRA WHERE Clave = 'CEM001';
SELECT * FROM COMPRASTUNERA WHERE Clave = 'CEM001';
```

### Problema 3: Error en consulta SQL
**Causa:** Sintaxis incorrecta o tabla no existe

**Solución:**
- Verificar permisos de lectura en las tablas
- Ejecutar consulta SQL manualmente para depurar

---

## ?? Comparación: Antes vs. Después

| Aspecto | Antes (Explosión) | Después (Tablas) |
|---------|------------------|------------------|
| Origen de Datos | Diccionario en memoria | Consulta SQL directa |
| Actualización | Manual (cargar explosión) | Automática (cada consulta) |
| Rendimiento | 2 consultas + carga memoria | 1 consulta única |
| Mantenibilidad | Media | Alta |
| Escalabilidad | Limitada | Excelente |
| Trazabilidad | Baja | Alta |

---

## ?? Cambios en Archivos

### Archivos Modificados
- ? `FormAlmacen_Salidas.cs` - Método `CargarInsumosDisponiblesSalida()`

### Archivos NO Modificados
- ?? Ya no se requiere `CargarExplosionParaSalida(prototipo)`
- ?? Variable `explosion` puede eliminarse si no se usa en otro lugar

---

## ?? Próximos Pasos Sugeridos

1. ? Ejecutar pruebas con ambos prototipos
2. ? Verificar que las tablas tienen datos completos
3. ?? Considerar eliminar método `CargarExplosionParaSalida()` si ya no se usa
4. ?? Agregar logs para auditar qué tabla se usa en cada salida
5. ? Documentar estructura de las tablas COMPRASCALANDRA y COMPRASTUNERA

---

**Fecha de Implementación:** Enero 2025  
**Versión:** 1.4 (Máximos desde Tablas de Compras)  
**Estado:** ? COMPLETADO Y COMPILADO  
**Sistema:** Gestión de Almacén - Calandria Residencial
