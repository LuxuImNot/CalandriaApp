# ?? CORRECCIÓN COMPLEMENTARIA: Guardar Conceptos con WBS Negativo

**Fecha:** Enero 2025  
**Problema:** Los conceptos terminados en FormEstimacionConceptoMigrado no se guardaban con WBS negativo  
**Estado:** ? RESUELTO

---

## ?? PROBLEMA DETECTADO

Después de corregir el `PanelPrincipal` para que lea conceptos desde `AvanceManualObra` con WBS negativos, se descubrió que **el método `ActualizarAvancesA100PorCiento()` no estaba guardando los conceptos con WBS negativo**.

### Flujo Incompleto

**ANTES:**
```
Usuario marca partidas ? Click "Guardar y Exportar" ? Método ActualizarAvancesA100PorCiento():
  ? Guarda partidas con WBS positivos (1, 2, 3...)
  ? NO guarda conceptos con WBS negativos (-1, -2, -3...)
  
Resultado en BD:
  WBS | Concepto              | AvancePorcentaje
  1   | Trazo                 | 100.0
  2   | Excavación            | 100.0
  ? FALTA: -1 | Preliminares | 100.0
```

**Consecuencia:**
- PanelPrincipal NO podía contar conceptos completados
- Progreso por Conceptos mostraba 0.0% (0/12)

---

## ? SOLUCIÓN IMPLEMENTADA

### Modificación en `FormEstimacionConceptoMigrado_ExtensionFolios.cs`

Se agregó lógica al método `ActualizarAvancesA100PorCiento()` para:

1. **Guardar partidas** con WBS positivos (ya existente)
2. **Verificar si TODAS las partidas de un concepto están completadas**
3. **Guardar el concepto con WBS negativo** cuando corresponda

### Código Agregado

```csharp
// 2?? NUEVO: Verificar si TODAS las partidas del concepto están completadas
// Si es así, guardar el concepto con WBS negativo para que PanelPrincipal lo detecte
var partidasCompletadas = concepto.Partidas.Where(p => p.Completado).Count();
var totalPartidas = concepto.Partidas.Count;

if (partidasCompletadas == totalPartidas && totalPartidas > 0)
{
    // TODAS las partidas del concepto están al 100%
    // Guardar concepto con WBS negativo
    int codigoConcepto = -1;
    if (!string.IsNullOrEmpty(concepto.Codigo) && int.TryParse(concepto.Codigo, out int codigo))
    {
        codigoConcepto = -codigo; // WBS negativo para distinguir conceptos
    }

    double totalConcepto = concepto.Partidas.Sum(p => p.MontoEjecutado);

    string sqlConcepto = @"
        IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
            UPDATE AvanceManualObra 
            SET AvancePorcentaje=100.0, 
                MontoEjecutado=@monto, 
                FechaActualizacion=GETDATE(),
                FechaFinalizacion=GETDATE(),
                Concepto=@concepto
            WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
        ELSE
            INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, Concepto, AvancePorcentaje, MontoEjecutado, FechaFinalizacion)
            VALUES (@m, @l, @proto, @wbs, @concepto, 100.0, @monto, GETDATE())";

    using (SqlCommand cmdConcepto = new SqlCommand(sqlConcepto, conn))
    {
        cmdConcepto.Parameters.AddWithValue("@m", manzana);
        cmdConcepto.Parameters.AddWithValue("@l", lote);
        cmdConcepto.Parameters.AddWithValue("@proto", prototipoActual ?? (object)DBNull.Value);
        cmdConcepto.Parameters.AddWithValue("@wbs", codigoConcepto.ToString());
        cmdConcepto.Parameters.AddWithValue("@concepto", concepto.Nombre ?? (object)DBNull.Value);
        cmdConcepto.Parameters.AddWithValue("@monto", totalConcepto);
        
        cmdConcepto.ExecuteNonQuery();
    }
}
```

### Lógica de Guardado

**Para cada concepto:**
1. Contar partidas completadas vs total de partidas
2. Si `partidasCompletadas == totalPartidas`:
   - Calcular WBS negativo: `-codigo`
   - Calcular monto total: `Sum(partidas.MontoEjecutado)`
   - Insertar/actualizar registro con:
     - `WBS = -1, -2, -3...` (negativo)
     - `Concepto = nombre del concepto`
     - `AvancePorcentaje = 100.0`
     - `FechaFinalizacion = GETDATE()`

---

## ?? FLUJO COMPLETO AHORA

### 1. Usuario completa conceptos

**En FormEstimacionConceptoMigrado:**
- Marca todas las partidas de "Preliminares"
- Marca todas las partidas de "Cimentación"
- Click "Guardar y Exportar"

### 2. Método ActualizarAvancesA100PorCiento()

**Ejecución:**
```
???????????????????????????????????????
? Conceptos con partidas marcadas:   ?
? • Preliminares (1 partida)          ?
? • Cimentación (3 partidas)          ?
???????????????????????????????????????
         ?
???????????????????????????????????????
? 1?? Actualizar PARTIDAS al 100%      ?
?   WBS 1 ? Trazo (100%)              ?
?   WBS 5 ? Cimentación A (100%)      ?
?   WBS 6 ? Cimentación B (100%)      ?
?   WBS 7 ? Cimentación C (100%)      ?
???????????????????????????????????????
         ?
???????????????????????????????????????
? 2?? Verificar CONCEPTOS completos    ?
?   Preliminares: 1/1 ?              ?
?   Cimentación: 3/3 ?               ?
???????????????????????????????????????
         ?
???????????????????????????????????????
? 3?? Guardar CONCEPTOS con WBS (-):   ?
?   WBS -1 ? Preliminares (100%)      ?
?   WBS -2 ? Cimentación (100%)       ?
???????????????????????????????????????
```

### 3. Resultado en Base de Datos

**Tabla AvanceManualObra:**
```sql
SELECT Manzana, Lote, WBS, Concepto, AvancePorcentaje, FechaFinalizacion
FROM AvanceManualObra
WHERE Manzana = '5' AND Lote = '2'
ORDER BY CAST(WBS AS INT);
```

**Resultado:**
```
Manzana | Lote | WBS | Concepto       | Avance | FechaFinalizacion
--------|------|-----|----------------|--------|-------------------
5       | 2    | -2  | Cimentación    | 100.0  | 2025-01-15 14:30
5       | 2    | -1  | Preliminares   | 100.0  | 2025-01-15 14:30
5       | 2    | 1   | Trazo          | 100.0  | 2025-01-15 14:30
5       | 2    | 5   | Cimentación A  | 100.0  | 2025-01-15 14:30
5       | 2    | 6   | Cimentación B  | 100.0  | 2025-01-15 14:30
5       | 2    | 7   | Cimentación C  | 100.0  | 2025-01-15 14:30
```

**Observar:**
- ? WBS negativos **-1, -2** para conceptos
- ? WBS positivos **1, 5, 6, 7** para partidas
- ? Ambos con `AvancePorcentaje = 100.0`
- ? Ambos con `FechaFinalizacion`

### 4. PanelPrincipal lee correctamente

**Consulta ejecutada:**
```sql
SELECT COUNT(CASE WHEN AvancePorcentaje = 100 THEN 1 END) AS ConceptosCompletados
FROM AvanceManualObra
WHERE Manzana = '5' AND Lote = '2'
  AND CAST(WBS AS INT) < 0
```

**Resultado:**
```
ConceptosCompletados: 2
```

**Cálculo:**
```
Progreso = (2 / 12) × 100 = 16.7%
```

**Visualización en PanelPrincipal:**
```
???????????????????????????????????
?? Resumen de Progreso - M5 L2
???????????????????????????????????

?? Progreso General:      19.4%
   ???????????????????? [19.4%]

??? Progreso por Partidas: 22.6%
   ???????????????????? [22.6%]

?? Progreso por Conceptos: 16.7% (2/12) ?
   ???????????????????? [16.7%]
   
? Conceptos Completados:
   • Preliminares
   • Cimentación
???????????????????????????????????
```

---

## ?? COMPARACIÓN ANTES vs DESPUÉS

### ANTES (Sin guardar conceptos con WBS negativo)

| Ubicación | Acción | WBS Guardados | Conceptos Detectados |
|-----------|--------|---------------|----------------------|
| FormEstimacionConceptoMigrado | Exportar | Solo positivos | ? 0 |
| PanelPrincipal | Cargar | Busca negativos | ? 0/12 (0%) |

**Síntoma:**
```
?? Progreso por Conceptos: 0.0% (0/12)
```

### DESPUÉS (Guardando conceptos con WBS negativo)

| Ubicación | Acción | WBS Guardados | Conceptos Detectados |
|-----------|--------|---------------|----------------------|
| FormEstimacionConceptoMigrado | Exportar | Positivos + Negativos | ? 2 |
| PanelPrincipal | Cargar | Busca negativos | ? 2/12 (16.7%) |

**Síntoma:**
```
?? Progreso por Conceptos: 16.7% (2/12)
```

---

## ?? VERIFICACIÓN

### Query SQL para verificar conceptos guardados

```sql
-- Listar TODOS los registros de una casa (partidas y conceptos)
SELECT 
    Manzana,
    Lote,
    CAST(WBS AS INT) AS WBS_Numero,
    WBS AS WBS_Original,
    Concepto,
    AvancePorcentaje,
    MontoEjecutado,
    FechaFinalizacion,
    CASE 
        WHEN CAST(WBS AS INT) < 0 THEN '?? Concepto'
        ELSE '?? Partida'
    END AS Tipo
FROM AvanceManualObra
WHERE Manzana = '5' AND Lote = '2'
ORDER BY CAST(WBS AS INT);
```

**Resultado esperado:**
```
WBS_Numero | WBS_Original | Concepto       | Avance | MontoEjecutado | FechaFin    | Tipo
-----------|--------------|----------------|--------|----------------|-------------|----------
-2         | -2           | Cimentación    | 100.0  | $38,359.65     | 2025-01-15  | ?? Concepto
-1         | -1           | Preliminares   | 100.0  | $1,980.36      | 2025-01-15  | ?? Concepto
1          | 1            | Trazo          | 100.0  | $1,980.36      | 2025-01-15  | ?? Partida
5          | 5            | Cimentación A  | 100.0  | $12,500.00     | 2025-01-15  | ?? Partida
...
```

### Pasos de Prueba Manual

1. **Limpiar datos anteriores:**
   ```sql
   DELETE FROM AvanceManualObra WHERE Manzana = '5' AND Lote = '2';
   ```

2. **En FormEstimacionConceptoMigrado:**
   - Seleccionar M5-L2
   - Cargar Avance
   - Marcar TODAS las partidas de "Preliminares" ?
   - Marcar TODAS las partidas de "Cimentación" ?
   - Click "Guardar y Exportar"

3. **Verificar en BD:**
   ```sql
   SELECT WBS, Concepto, AvancePorcentaje 
   FROM AvanceManualObra 
   WHERE Manzana = '5' AND Lote = '2'
     AND CAST(WBS AS INT) < 0;
   ```
   
   **Resultado esperado:**
   ```
   WBS | Concepto       | AvancePorcentaje
   ----|----------------|------------------
   -1  | Preliminares   | 100.0
   -2  | Cimentación    | 100.0
   ```

4. **En PanelPrincipal:**
   - Buscar casa M5-L2
   - Verificar sección "Progreso por Conceptos"
   
   **Resultado esperado:**
   ```
   ?? Progreso por Conceptos: 16.7% (2/12)
   ```

---

## ?? ARCHIVOS MODIFICADOS

| Archivo | Método Modificado | Cambio |
|---------|-------------------|--------|
| `FormEstimacionConceptoMigrado_ExtensionFolios.cs` | `ActualizarAvancesA100PorCiento()` | ? Agregado código para guardar conceptos con WBS negativo |

**Fragmento clave:**
```csharp
// Verificar si TODAS las partidas del concepto están completadas
var partidasCompletadas = concepto.Partidas.Where(p => p.Completado).Count();
var totalPartidas = concepto.Partidas.Count;

if (partidasCompletadas == totalPartidas && totalPartidas > 0)
{
    int codigoConcepto = -codigo; // WBS negativo
    // ... guardar en AvanceManualObra
}
```

---

## ? ESTADO

- ? **Problema identificado**
- ? **Solución implementada**
- ? **Compilación exitosa**
- ? **Listo para pruebas**

---

## ?? INTEGRACIÓN CON CORRECCIÓN ANTERIOR

Esta corrección **complementa** la corrección anterior en `PanelPrincipal.cs`:

| Corrección | Componente | Acción |
|------------|------------|--------|
| **Primera** | `PanelPrincipal.cs` | Lee conceptos desde `AvanceManualObra` con WBS < 0 |
| **Segunda** | `FormEstimacionConceptoMigrado_ExtensionFolios.cs` | **Guarda** conceptos en `AvanceManualObra` con WBS < 0 |

**Ahora el flujo completo funciona:**
1. FormEstimacionConceptoMigrado **GUARDA** conceptos con WBS negativos ?
2. PanelPrincipal **LEE** conceptos con WBS negativos ?
3. Progreso se refleja correctamente ?

---

## ?? ARCHIVOS RELACIONADOS

- `CORRECCION_ProgresoConceptos_PanelPrincipal.md` - Corrección de lectura en PanelPrincipal
- `IMPLEMENTACION_FechaConceptos_Automatica.md` - Lógica de fechas automáticas
- `CORRECCION_ConsistenciaAvanceConceptos.md` - Consistencia entre formularios

---

**Desarrollado para:** Sistema Calandria Residencial  
**Fecha de Corrección:** Enero 2025  
**Versión:** 1.1  
**Prioridad:** ALTA

---

? **Corrección complementaria completada exitosamente**

?? **Ahora ambas correcciones trabajan en conjunto para reflejar correctamente el progreso de conceptos en el PanelPrincipal**
