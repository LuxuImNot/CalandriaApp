# FormAvanceConcepto - Documentación Completa

## ?? Descripción General

`FormAvanceConcepto` es un formulario para gestionar el avance de obra **por conceptos** (en lugar de subdivisiones jerárquicas). Está diseñado para trabajar con la estructura de conceptos tal como aparece en la imagen de referencia:

```
| Código | Concepto                              | TOTAL        |
|--------|---------------------------------------|--------------|
| 1      | Preliminares (trazo, cimbra, exc)    | $8,083.38    |
| 2      | Cimentación                           | $189,361.56  |
| 3      | Estructura                            | $1,383,350.28|
| 4      | Inst. Hidraulica - Sanitaria - Gas LP | $239,728.06  |
| 5      | Inst. Electrica                       | $285,229.17  |
| 6      | Albañileria                           | $573,711.08  |
| 7      | Acabados                              | $755,153.18  |
| 8      | Herreria, Aluminio y Vidrio          | $136,126.26  |
| 9      | Carpinteria y Cerrajeria             | $109,827.44  |
| 10     | Muebles y Accesorios                 | $113,779.20  |
| 11     | Limpieza e Instalaciones Especiales  | $96,438.52   |
| 12     | Urbanizacion                          | $0.00        |
|--------|---------------------------------------|--------------|
| TOTAL  |                                       | $3,890,788.14|
```

## ??? Base de Datos

### Tabla Principal: `PresupuestoObra`

El formulario obtiene los conceptos desde esta tabla, agrupando por `Codigo` y `Concepto`:

```sql
SELECT 
    Codigo,
    Concepto,
    SUM(CostoCalandra) as Total  -- o CostoTunera según el prototipo
FROM PresupuestoObra 
GROUP BY Codigo, Concepto
ORDER BY Codigo
```

### Tabla de Avances: `AvanceManualConcepto`

Se crea automáticamente si no existe:

```sql
CREATE TABLE AvanceManualConcepto (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Manzana NVARCHAR(10),
    Lote NVARCHAR(10),
    Prototipo NVARCHAR(50),
    Codigo NVARCHAR(10),
    Concepto NVARCHAR(200),
    AvancePorcentaje FLOAT,
    FechaActualizacion DATETIME DEFAULT GETDATE()
)
```

**Propósito:**
- Almacenar el avance % de cada concepto por casa (Manzana + Lote)
- El sistema calcula automáticamente el monto ejecutado basado en el porcentaje

## ?? Interfaz de Usuario

### Controles Principales

1. **Selección de Casa:**
   - `cmbManzana`: ComboBox para seleccionar la manzana
   - `cmbLote`: ComboBox para seleccionar el lote
   - `btnCargarAvance`: Botón para cargar el avance de la casa seleccionada

2. **Tabla de Conceptos:**
   - `olvAvanceConceptos`: ObjectListView con agrupamiento por concepto
   - **Columnas:**
     - **Código**: Número del concepto (1-12)
     - **Concepto**: Nombre del concepto
     - **Total**: Presupuesto total del concepto ($ MXN)
     - **Avance %**: Porcentaje de avance (EDITABLE)
     - **Ejecutado**: Monto ejecutado (EDITABLE)

3. **Indicadores:**
   - `lblTotalPresupuestado`: Total presupuestado de todos los conceptos
   - `lblTotalEjecutado`: Total ejecutado hasta el momento
   - `lblAvanceGeneral`: Porcentaje de avance general
   - `progressBarAvance`: Barra de progreso visual
   - `lblEstadisticas`: Contadores de conceptos (completados, en progreso, sin iniciar)

4. **Gráficas:**
   - `pictureBoxGrafica`: 
     - **Superior**: Gráfica de pastel mostrando avance general
     - **Inferior**: Gráfica de barras por concepto

5. **Exportación:**
   - `btnExportarPDF`: Genera reporte PDF profesional

## ?? Funcionalidades Principales

### 1. Carga de Datos

```csharp
private List<ItemConcepto> CargarConceptosPorPrototipo(string prototipo)
```

- Determina automáticamente qué columna de costo usar según el prototipo:
  - **CALANDRIA** ? Usa `CostoCalandra`
  - **TUNERA** ? Usa `CostoTunera`
- Agrupa por `Codigo` y `Concepto`
- Suma los totales de todas las partidas de cada concepto

### 2. Edición Bidireccional

**Editar Avance %:**
```csharp
// Usuario ingresa: 75%
item.AvancePorcentaje = 75.0;
item.MontoEjecutado = item.Total * 0.75;  // Calcula monto automáticamente
```

**Editar Monto Ejecutado:**
```csharp
// Usuario ingresa: $150,000
item.MontoEjecutado = 150000;
item.AvancePorcentaje = (150000 / item.Total) * 100;  // Calcula % automáticamente
```

**Validaciones:**
- ? Avance % debe estar entre 0-100
- ? Monto ejecutado no puede ser mayor al presupuestado
- ? No se permiten valores negativos
- ? Guardado automático en BD tras cada cambio

### 3. Persistencia de Datos

```csharp
private void GuardarAvanceEnBD(ItemConcepto item)
```

- **INSERT** si es la primera vez que se registra avance
- **UPDATE** si ya existe un registro previo
- Almacena: Manzana, Lote, Prototipo, Código, Concepto, Avance %
- Actualiza `FechaActualizacion` automáticamente

### 4. Agrupamiento Visual

El ObjectListView agrupa los conceptos automáticamente:

```
? Preliminares (trazo, cimbra, exc)
  ?? 1 | Preliminares... | $8,083.38 | 0% | $0.00

? Cimentación
  ?? 2 | Cimentación | $189,361.56 | 100% | $189,361.56

? Estructura
  ?? 3 | Estructura | $1,383,350.28 | 50% | $691,675.14
```

### 5. Totalizadores en Tiempo Real

Cada vez que se edita un valor:

1. ? Recalcula totales generales
2. ? Actualiza indicadores visuales
3. ? Redibuja gráficas
4. ? Cambia colores según el progreso:
   - ?? Rojo (< 30%): Muy bajo
   - ?? Naranja (30-70%): En progreso
   - ?? Verde (> 70%): Avanzado

### 6. Gráficas Automáticas

**Gráfica de Pastel:**
- Muestra % de avance general
- Colores dinámicos según el progreso
- Efecto de "dona" (hueco en el centro)
- Porcentaje grande en el centro

**Gráfica de Barras:**
- Una barra por concepto
- Altura proporcional al presupuesto
- Barra gris = presupuesto total
- Barra azul = monto ejecutado
- Etiquetas con % sobre cada barra
- Nombres de conceptos rotados 45°

### 7. Exportación a PDF

Genera un reporte profesional con:

**Página 1 - Portada:**
- ? Encabezado corporativo
- ? Datos del proyecto (Manzana, Lote, Prototipo)
- ? Métricas principales (Presupuestado, Ejecutado, Avance %)
- ? Estadísticas (Completados, En progreso, Sin iniciar)
- ? Gráfica de pastel grande
- ? Nota: "Este reporte muestra X de Y conceptos (solo los que tienen avance)"

**Página 2+ - Detalle:**
- ? Tabla con todos los conceptos que tienen avance > 0%
- ? Columnas: Código, Concepto, Presupuesto, Avance %, Ejecutado
- ? Colores según estado:
  - ?? Verde: Completado (100%)
  - ?? Naranja: En progreso (0-99%)
- ? Filas alternadas (fondo gris/blanco)
- ? Paginación automática
- ? Encabezados de continuación

**Pie de Página:**
- Nombre del sistema
- Número de página
- Fecha de generación

## ?? Clase ItemConcepto

```csharp
public class ItemConcepto
{
    public string Codigo { get; set; }          // Ej: "1", "2", "3"...
    public string Concepto { get; set; }        // Ej: "Preliminares (trazo, cimbra, exc)"
    public double Total { get; set; }           // Presupuesto total del concepto
    public double AvancePorcentaje { get; set; } // 0-100
    public double MontoEjecutado { get; set; }  // Monto en $
}
```

## ?? Flujo de Trabajo

### Uso Normal

1. **Seleccionar casa:**
   ```
   Usuario: Selecciona Manzana ? Se cargan Lotes
   Usuario: Selecciona Lote ? Click "Cargar Avance"
   ```

2. **El sistema:**
   ```
   ? Obtiene el prototipo de la casa
   ? Carga los 12 conceptos desde PresupuestoObra
   ? Consulta avances guardados en AvanceManualConcepto
   ? Muestra la tabla con agrupamiento
   ? Dibuja gráficas iniciales
   ```

3. **Registrar avance:**
   ```
   Usuario: Doble click en columna "Avance %" o "Ejecutado"
   Usuario: Ingresa nuevo valor
   Sistema: Valida ? Calcula el valor complementario ? Guarda en BD
   Sistema: Actualiza totales ? Redibuja gráficas
   ```

4. **Exportar reporte:**
   ```
   Usuario: Click "Exportar PDF"
   Sistema: Genera PDF profesional ? Pregunta si desea abrirlo
   ```

## ?? Diferencias con FormAvanceObra

| Característica | FormAvanceObra | FormAvanceConcepto |
|----------------|----------------|-------------------|
| **Nivel de detalle** | Padre > Etapa > Partida | Solo Concepto |
| **Número de items** | ~200+ partidas | 12 conceptos |
| **Agrupamiento** | Por Padre (categoría) | Por Concepto |
| **Tabla BD** | `AvanceManualObra` | `AvanceManualConcepto` |
| **Identificador único** | WBS | Codigo |
| **PDF - Detalle** | Categorizado por Padre/Etapa | Lista simple por Código |
| **Uso ideal** | Seguimiento detallado | Vista ejecutiva/resumen |

## ?? Ejemplo de Uso

### Registrar avance del concepto "Cimentación"

**Escenario:**
- Casa: M5-L12 (Prototipo: CALANDRIA 120 D)
- Concepto: Cimentación (Código 2)
- Presupuesto: $189,361.56
- Se completó el 100% del trabajo

**Pasos:**

1. Seleccionar Manzana: **5**
2. Seleccionar Lote: **12**
3. Click **Cargar Avance**
4. Ubicar fila: **2 | Cimentación | $189,361.56**
5. Doble click en columna **Avance %**
6. Ingresar: **100**
7. Presionar Enter

**Resultado:**
```
Concepto: Cimentación
Código: 2
Total: $189,361.56
Avance %: 100.0%
Ejecutado: $189,361.56  ? Calculado automáticamente
```

**En BD:**
```sql
INSERT INTO AvanceManualConcepto (
    Manzana, Lote, Prototipo, Codigo, Concepto, AvancePorcentaje
) VALUES (
    '5', '12', 'CALANDRIA 120 D', '2', 'Cimentación', 100.0
)
```

## ??? Mantenimiento

### Agregar nuevos conceptos

Si se agregan nuevos conceptos a `PresupuestoObra`:

1. ? Asignar `Codigo` único (ej: 13, 14...)
2. ? Definir `Concepto` descriptivo
3. ? El formulario los cargará automáticamente
4. ? No requiere cambios en el código

### Modificar estructura

**Para cambiar agrupamiento:**
```csharp
// En ConfigurarObjectListView(), modificar:
colConcepto.GroupKeyGetter = (rowObject) => {
    var item = (ItemConcepto)rowObject;
    return item.AlgunOtroCampo;  // Cambiar lógica de agrupamiento
};
```

**Para agregar columnas:**
```csharp
var colNueva = new OLVColumn("Nombre", "Propiedad") { 
    Width = 100, 
    IsEditable = false 
};
olvAvanceConceptos.AllColumns.Add(colNueva);
```

## ?? Mejoras Futuras Sugeridas

1. **Filtros:**
   - Por rango de códigos
   - Por % de avance
   - Por concepto (búsqueda de texto)

2. **Comparativas:**
   - Comparar avance entre múltiples casas
   - Gráfica de tendencias por concepto

3. **Alertas:**
   - Notificar si un concepto lleva mucho tiempo sin avance
   - Destacar conceptos con retrasos

4. **Exportación adicional:**
   - Excel
   - CSV
   - Imágenes de las gráficas

5. **Historial:**
   - Ver evolución del avance en el tiempo
   - Bitácora de cambios

## ? Notas Importantes

- ?? Los cambios se guardan **inmediatamente** en la BD (sin botón "Guardar")
- ?? El PDF solo incluye conceptos con avance > 0% (filtro automático)
- ?? Los totales se actualizan en **tiempo real**
- ?? El agrupamiento puede colapsarse/expandirse manualmente
- ?? Se debe seleccionar Manzana + Lote antes de cargar avance

## ?? Soporte

Para dudas o problemas:
1. Revisar logs en `Actualizador.log` (si aplica)
2. Verificar conexión a BD en `App.config`
3. Consultar estructura de `PresupuestoObra` y `AvanceManualConcepto`

---

**Desarrollado para:** Sistema Calandria Residencial  
**Versión:** 1.0  
**Fecha:** Enero 2025
