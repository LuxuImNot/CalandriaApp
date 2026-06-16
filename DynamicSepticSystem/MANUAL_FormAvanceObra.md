# ?? FormAvanceObra - Manual de Usuario

## ?? Descripción General

El **FormAvanceObra** es un formulario profesional que permite:
- Visualizar el avance de construcción de cada casa según su prototipo
- Modificar el porcentaje de avance de cada partida del presupuesto
- Ver gráficas dinámicas del progreso (pastel y barras)
- Exportar reportes en PDF con formato profesional
- Calcular automáticamente totales financieros

---

## ?? Características Principales

### ? 1. Carga Automática por Prototipo
- Selecciona la casa (Manzana/Lote)
- El sistema carga automáticamente el presupuesto del prototipo correspondiente
- Cada prototipo tiene su propio presupuesto de obra

### ? 2. Edición en Vivo
- Haz **doble clic** en la columna "Avance %"
- Ingresa un valor entre 0 y 100
- El sistema calcula automáticamente el "Importe Ejecutado"
- Los cambios se guardan instantáneamente en la base de datos

### ? 3. Métricas Financieras en Tiempo Real
- **Total Presupuestado**: Suma del presupuesto completo
- **Total Ejecutado**: Suma del dinero gastado según avances
- **Avance General**: Porcentaje global de la obra
- **Barra de Progreso**: Visualización gráfica del avance

### ? 4. Estadísticas Visuales
- **Completadas**: Partidas al 100%
- **En Progreso**: Partidas entre 1% y 99%
- **Sin Iniciar**: Partidas al 0%

### ? 5. Gráficas Profesionales

#### ?? Gráfica de Pastel
- Muestra el porcentaje general de avance
- Verde: Ejecutado
- Gris: Restante
- Texto central con el porcentaje exacto

#### ?? Gráfica de Barras
- Agrupa por categoría principal (primer nivel WBS)
- Gris: Presupuesto total de la categoría
- Azul: Monto ejecutado de la categoría
- Se actualiza en tiempo real al editar

### ? 6. Código de Colores
- ?? **Rojo** (< 30%): Obra retrasada
- ?? **Naranja** (30-70%): Obra en progreso normal
- ?? **Verde** (> 70%): Obra adelantada

### ? 7. Exportación a PDF
- Genera reporte profesional con:
  - Encabezado con datos de la casa
  - Resumen financiero destacado
  - Tabla detallada de todas las partidas
  - Formato de múltiples páginas automático
- Opción de abrir el PDF inmediatamente

---

## ?? Cómo Usar

### Paso 1: Abrir el Formulario
Desde el menú principal:
```
OBRA ? Avance de Obra por Casa
```

### Paso 2: Seleccionar Casa
1. **Manzana**: Selecciona del combo (ej: M1, M2, M3...)
2. **Lote**: Automáticamente se cargan los lotes disponibles
3. Clic en **"?? Cargar Avance"**

### Paso 3: Ver y Editar Avances
- La tabla muestra todas las partidas del presupuesto
- Columnas visibles:
  - **WBS**: Código de estructura de desglose
  - **Concepto**: Descripción de la partida
  - **Unidad**: Unidad de medida
  - **Cantidad**: Cantidad de unidades
  - **P.U.**: Precio unitario
  - **Importe**: Total presupuestado
  - **Avance %**: ?? EDITABLE (doble clic)
  - **Ejecutado**: Calculado automáticamente

### Paso 4: Modificar Avances
1. Haz **doble clic** en cualquier celda de "Avance %"
2. Ingresa el porcentaje (0-100)
3. Presiona **Enter**
4. ? Se guarda automáticamente
5. ?? Las gráficas se actualizan al instante

### Paso 5: Exportar Reporte
1. Clic en **"?? Exportar PDF"**
2. Selecciona la ubicación y nombre del archivo
3. ? Confirmación de generación exitosa
4. Opción de abrir el PDF directamente

---

## ??? Estructura de Base de Datos

### Tabla: `PresupuestoObra`
Almacena el presupuesto maestro por prototipo:

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Prototipo` | NVARCHAR(50) | Nombre del prototipo (TUNERA, CALANDRA, etc.) |
| `WBS` | NVARCHAR(50) | Código de estructura (1, 1.1, 1.1.1) |
| `Concepto` | NVARCHAR(500) | Descripción de la partida |
| `Unidad` | NVARCHAR(20) | m², pza, m, etc. |
| `Cantidad` | DECIMAL(18,2) | Cantidad de unidades |
| `PrecioUnitario` | DECIMAL(18,2) | Precio por unidad |
| `ImporteTotal` | DECIMAL(18,2) | Cantidad × Precio |

### Tabla: `AvanceManualObra`
Registra los avances capturados por casa:

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | INT | Identificador único |
| `Manzana` | NVARCHAR(10) | Manzana de la casa |
| `Lote` | NVARCHAR(10) | Lote de la casa |
| `Prototipo` | NVARCHAR(50) | Prototipo de la casa |
| `WBS` | NVARCHAR(50) | Código de la partida |
| `AvancePorcentaje` | FLOAT | % de avance (0-100) |
| `FechaActualizacion` | DATETIME | Fecha del último cambio |

---

## ?? Configuración Inicial

### 1. Verificar Tabla de Presupuesto

```sql
-- Ver prototipos disponibles
SELECT DISTINCT Prototipo FROM PresupuestoObra;

-- Ver presupuesto de un prototipo específico
SELECT WBS, Concepto, ImporteTotal 
FROM PresupuestoObra 
WHERE Prototipo = 'TUNERA'
ORDER BY WBS;

-- Calcular total por prototipo
SELECT Prototipo, SUM(ImporteTotal) AS TotalPresupuesto
FROM PresupuestoObra
GROUP BY Prototipo;
```

### 2. Asignar Prototipos a Casas

```sql
-- Ver casas y sus prototipos
SELECT Manzana, Lote, Prototipo 
FROM InventarioCasas
ORDER BY Manzana, Lote;

-- Actualizar prototipo de una casa
UPDATE InventarioCasas 
SET Prototipo = 'TUNERA' 
WHERE Manzana = 'M5' AND Lote = 'L45';
```

### 3. Crear Tabla de Avances (automático)
La tabla `AvanceManualObra` se crea automáticamente al cargar el primer avance.

---

## ?? Casos de Uso

### Caso 1: Iniciar Avance de Casa Nueva
1. Selecciona la casa que acaba de iniciar construcción
2. Carga el avance (todo estará en 0%)
3. Edita las primeras partidas (ej: Preliminares = 50%)
4. El sistema calcula automáticamente el dinero ejecutado

### Caso 2: Actualizar Avance Semanal
1. Carga la casa que estás supervisando
2. Se muestran los avances anteriores
3. Actualiza los porcentajes según el progreso de la semana
4. Exporta PDF para la reunión de avance

### Caso 3: Reporte para Cliente
1. Carga la casa del cliente
2. Verifica que los avances estén actualizados
3. Exporta PDF profesional
4. Comparte con el cliente vía email

### Caso 4: Análisis Financiero
1. Carga varias casas y compara:
   - Total Presupuestado vs Ejecutado
   - Porcentaje de avance general
   - Categorías con mayor gasto
2. Identifica desviaciones del presupuesto

---

## ?? Personalización de Colores

Los colores se ajustan automáticamente según el avance:

```csharp
// Código de colores en el sistema
if (avance < 30) 
    color = Rojo;      // Alerta de retraso
else if (avance < 70) 
    color = Naranja;   // Progreso normal
else 
    color = Verde;     // Adelantado
```

---

## ?? Solución de Problemas

### Problema: "No se encontró el prototipo"

**Causa**: La casa no tiene prototipo asignado o el prototipo no existe en `PresupuestoObra`

**Solución**:
```sql
-- Ver prototipo de la casa
SELECT Prototipo FROM InventarioCasas 
WHERE Manzana = 'TU_MANZANA' AND Lote = 'TU_LOTE';

-- Actualizar prototipo
UPDATE InventarioCasas 
SET Prototipo = 'TUNERA' 
WHERE Manzana = 'TU_MANZANA' AND Lote = 'TU_LOTE';
```

### Problema: "Presupuesto en $0.00"

**Causa**: El prototipo no tiene datos en `PresupuestoObra`

**Solución**:
1. Verifica que existan registros:
```sql
SELECT COUNT(*) FROM PresupuestoObra 
WHERE Prototipo = 'TU_PROTOTIPO';
```

2. Si es 0, importa el presupuesto desde Excel usando los scripts SQL proporcionados

### Problema: Las gráficas no se muestran

**Causa**: El control `pictureBoxGrafica` no está visible o es muy pequeño

**Solución**:
- Maximiza la ventana del formulario
- Las gráficas se redibujan automáticamente al cambiar el tamaño

### Problema: Error al guardar avance

**Causa**: Problema de conexión a base de datos

**Solución**:
1. Verifica la cadena de conexión en `App.config`
2. Asegúrate de tener permisos de escritura en la BD
3. Comprueba que SQL Server esté ejecutándose

---

## ?? Ejemplo de Flujo Completo

```
1. Usuario abre "Avance de Obra"
2. Selecciona: Manzana = "M5", Lote = "L45"
3. Clic en "Cargar Avance"
4. Sistema muestra:
   - Prototipo: TUNERA
   - Total Presupuestado: $1,560,000.00
   - Total Ejecutado: $450,000.00
   - Avance General: 28.8%
   
5. Usuario edita:
   - Preliminares: 100%
   - Cimentación: 100%
   - Muros PB: 75%
   
6. Sistema recalcula:
   - Total Ejecutado: $545,000.00
   - Avance General: 34.9%
   - Color cambia de Rojo a Naranja
   
7. Usuario exporta PDF
8. Comparte reporte con cliente
```

---

## ?? Seguridad y Auditoría

- Todos los cambios se registran con fecha y hora
- Se puede rastrear quién modificó cada avance
- Los datos se guardan inmediatamente en la base de datos
- Backup automático mediante las políticas de SQL Server

---

## ?? Métricas Clave

El formulario calcula automáticamente:

1. **Avance General**: `(Total Ejecutado / Total Presupuestado) × 100`
2. **Importe Ejecutado**: `Importe Total × (Avance % / 100)`
3. **Partidas Completadas**: `COUNT(Avance = 100%)`
4. **Partidas en Progreso**: `COUNT(0% < Avance < 100%)`
5. **Partidas Sin Iniciar**: `COUNT(Avance = 0%)`

---

## ?? Soporte

Si tienes dudas o encuentras errores:
1. Verifica los scripts SQL de diagnóstico incluidos
2. Revisa la configuración de conexión a base de datos
3. Consulta los archivos de documentación adicionales
4. Ejecuta las consultas de verificación proporcionadas

---

**Desarrollado para**: Sistema Calandria Residencial  
**Versión**: 1.0  
**Fecha**: Enero 2025  
**Tecnologías**: C# .NET Framework 4.7.2, SQL Server, BrightIdeasSoftware, PdfSharp
