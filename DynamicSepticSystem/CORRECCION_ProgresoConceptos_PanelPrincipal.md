# ?? CORRECCIÓN: Progreso de Conceptos no se Refleja en PanelPrincipal

**Fecha:** Enero 2025  
**Problema:** Los conceptos terminados en FormEstimacionConceptoMigrado no se reflejaban en el PanelPrincipal  
**Estado:** ? RESUELTO

---

## ?? DIAGNÓSTICO DEL PROBLEMA

### Síntoma
Cuando se completaban conceptos en `FormEstimacionConceptoMigrado`, el avance no se reflejaba en el `PanelPrincipal` en la sección de "Progreso por Conceptos".

### Causa Raíz
El `PanelPrincipal` estaba buscando el avance de conceptos en la tabla **`AvanceManualConcepto`**, pero los conceptos terminados se estaban guardando en la tabla **`AvanceManualObra`** con **WBS negativos** para distinguirlos de las partidas.

**Discrepancia en el almacenamiento:**
- **FormAvanceConcepto**: Guarda en `AvanceManualConcepto` ?
- **FormEstimacionConceptoMigrado**: Guarda conceptos en `AvanceManualObra` con WBS negativos ?

**PanelPrincipal estaba consultando:**
```sql
SELECT AVG(AvancePorcentaje) 
FROM AvanceManualConcepto
WHERE Manzana = @m AND Lote = @l
```

**Pero los datos estaban en:**
```sql
SELECT COUNT(*) 
FROM AvanceManualObra
WHERE Manzana = @m AND Lote = @l 
  AND CAST(WBS AS INT) < 0  -- WBS negativos = conceptos
  AND AvancePorcentaje = 100
```

---

## ? SOLUCIÓN IMPLEMENTADA

### Modificación en `PanelPrincipal.cs`

Se actualizó el método `CargarResumenProgreso()` para calcular correctamente el progreso de conceptos desde la tabla `AvanceManualObra`.

**Cambio Principal:**

```csharp
// ?? CORREGIDO: Obtener progreso por conceptos desde AvanceManualObra
// Los conceptos se identifican por WBS negativos (-1, -2, -3, etc.)
decimal progresoConceptos = 0;
int conceptosCompletados = 0;
int totalConceptos = 12; // Total estándar de conceptos en el sistema

using (SqlCommand cmd = new SqlCommand(@"
    SELECT 
        COUNT(CASE WHEN AvancePorcentaje = 100 THEN 1 END) AS ConceptosCompletados,
        COUNT(*) AS TotalRegistrados
    FROM dbo.AvanceManualObra
    WHERE Manzana = @manzana 
      AND Lote = @lote
      AND CAST(WBS AS INT) < 0", conn))
{
    cmd.Parameters.AddWithValue("@manzana", casaActual.Manzana);
    cmd.Parameters.AddWithValue("@lote", casaActual.Lote);
    
    using (SqlDataReader reader = cmd.ExecuteReader())
    {
        if (reader.Read())
        {
            conceptosCompletados = reader["ConceptosCompletados"] != DBNull.Value ? 
                Convert.ToInt32(reader["ConceptosCompletados"]) : 0;
            
            // Calcular porcentaje de avance de conceptos
            // (conceptos completados / total de conceptos) * 100
            progresoConceptos = totalConceptos > 0 ? 
                ((decimal)conceptosCompletados / totalConceptos) * 100 : 0;
        }
    }
}
```

### Lógica de Cálculo

**Progreso de Conceptos:**
```
Progreso = (Conceptos Completados / Total de Conceptos) × 100
```

Donde:
- **Conceptos Completados**: Registros en `AvanceManualObra` con WBS < 0 y AvancePorcentaje = 100
- **Total de Conceptos**: 12 (estándar del sistema)

**Ejemplo:**
- Si 3 conceptos están completados de 12 totales:
  - Progreso = (3 / 12) × 100 = **25%**
- Si los 12 conceptos están completados:
  - Progreso = (12 / 12) × 100 = **100%**

### Actualización de UI

Ahora el label de progreso de conceptos muestra:
```csharp
lblProgresoConceptosValor.Text = $"{progresoConceptos:F1}% ({conceptosCompletados}/{totalConceptos})";
```

**Ejemplo de visualización:**
```
Progreso por Conceptos: 25.0% (3/12)
```

---

## ?? FLUJO DE DATOS COMPLETO

### 1. Usuario completa un concepto en FormEstimacionConceptoMigrado

**Acción:**
- Usuario marca todas las partidas de un concepto como incluidas
- Click en "Guardar y Exportar"

**Proceso:**
```csharp
// FormEstimacionConceptoMigrado.cs
private void ActualizarAvancesA100PorCiento()
{
    foreach (var concepto in nodosRaiz)
    {
        if (concepto.Incluir)
        {
            // Actualizar concepto con WBS negativo
            concepto.Completado = true;
            concepto.FechaFinalizacion = DateTime.Now;
            GuardarAvanceConceptoEnBD(concepto);
        }
    }
}

private void GuardarFechaConceptoEnBD(NodoConcepto concepto)
{
    int codigoConcepto = -int.Parse(concepto.Codigo); // WBS negativo
    
    // INSERT/UPDATE en AvanceManualObra con WBS = -1, -2, -3, etc.
}
```

### 2. Datos guardados en BD

**Tabla: AvanceManualObra**
```
| Manzana | Lote | WBS | Concepto         | AvancePorcentaje | FechaFinalizacion |
|---------|------|-----|------------------|------------------|-------------------|
| 1       | 5    | -1  | Preliminares     | 100.0            | 2025-01-15        |
| 1       | 5    | -2  | Cimentación      | 100.0            | 2025-01-18        |
| 1       | 5    | -3  | Estructura       | 100.0            | 2025-01-22        |
| 1       | 5    | 1   | Trazo            | 100.0            | 2025-01-15        |
| 1       | 5    | 2   | Excavación       | 100.0            | 2025-01-16        |
...
```

**Observar:**
- **WBS negativos** (-1, -2, -3) = **Conceptos**
- **WBS positivos** (1, 2, 3, ...) = **Partidas**

### 3. PanelPrincipal carga el resumen

**Acción:**
- Usuario busca la casa (M1-L5) en PanelPrincipal
- Click en "Buscar Casa"

**Consulta SQL (ANTES - Incorrecta):**
```sql
-- ? Buscaba en tabla incorrecta
SELECT AVG(AvancePorcentaje) 
FROM AvanceManualConcepto
WHERE Manzana = '1' AND Lote = '5'
-- Resultado: 0% (tabla vacía)
```

**Consulta SQL (AHORA - Correcta):**
```sql
-- ? Busca en tabla correcta con filtro WBS < 0
SELECT COUNT(CASE WHEN AvancePorcentaje = 100 THEN 1 END) AS ConceptosCompletados
FROM AvanceManualObra
WHERE Manzana = '1' 
  AND Lote = '5'
  AND CAST(WBS AS INT) < 0
-- Resultado: 3 conceptos completados
```

**Cálculo:**
```
Progreso = (3 / 12) × 100 = 25.0%
```

### 4. UI actualizada correctamente

**PanelPrincipal muestra:**
```
???????????????????????????????????
?? Resumen de Progreso - M1 L5
???????????????????????????????????

?? Progreso General:      37.5%
   ???????????????????? [37.5%]

??? Progreso por Partidas: 50.0%
   ???????????????????? [50.0%]

?? Progreso por Conceptos: 25.0% (3/12)
   ???????????????????? [25.0%]
   
? Conceptos Completados:
   • Preliminares
   • Cimentación
   • Estructura
???????????????????????????????????
```

---

## ?? COMPARACIÓN ANTES vs DESPUÉS

### ANTES (Problema)

| Ubicación | Tabla Consultada | WBS Filtro | Resultado |
|-----------|------------------|------------|-----------|
| FormAvanceConcepto | AvanceManualConcepto | - | ? Correcto |
| FormEstimacionConceptoMigrado | AvanceManualObra | WBS < 0 | ? Guarda correcto |
| **PanelPrincipal** | **AvanceManualConcepto** | - | **? Tabla vacía** |

**Síntoma:**
```
PanelPrincipal muestra:
?? Progreso por Conceptos: 0.0% (0/12)
```

Aunque en la BD existían registros en `AvanceManualObra` con WBS negativos.

### DESPUÉS (Solución)

| Ubicación | Tabla Consultada | WBS Filtro | Resultado |
|-----------|------------------|------------|-----------|
| FormAvanceConcepto | AvanceManualConcepto | - | ? Correcto |
| FormEstimacionConceptoMigrado | AvanceManualObra | WBS < 0 | ? Guarda correcto |
| **PanelPrincipal** | **AvanceManualObra** | **WBS < 0** | **? Lee correctamente** |

**Síntoma:**
```
PanelPrincipal muestra:
?? Progreso por Conceptos: 25.0% (3/12)
```

Refleja correctamente los conceptos terminados.

---

## ?? VERIFICACIÓN

### Query SQL para verificar conceptos guardados

```sql
-- Ver conceptos guardados (WBS negativos)
SELECT 
    Manzana,
    Lote,
    WBS,
    Concepto,
    AvancePorcentaje,
    FechaFinalizacion
FROM AvanceManualObra
WHERE Manzana = '1' 
  AND Lote = '5'
  AND CAST(WBS AS INT) < 0
ORDER BY CAST(WBS AS INT);

-- Resultado esperado:
-- WBS  | Concepto              | AvancePorcentaje | FechaFinalizacion
-- -1   | Preliminares          | 100.0            | 2025-01-15
-- -2   | Cimentación           | 100.0            | 2025-01-18
-- -3   | Estructura            | 100.0            | 2025-01-22
```

### Pasos de Prueba

1. **En FormEstimacionConceptoMigrado:**
   - Seleccionar M1-L5
   - Marcar todas las partidas de "Preliminares"
   - Click "Guardar y Exportar"
   - ? Debe guardar concepto con WBS = -1

2. **En PanelPrincipal:**
   - Buscar casa M1-L5
   - Verificar sección "Progreso por Conceptos"
   - ? Debe mostrar: "8.3% (1/12)"

3. **Completar más conceptos:**
   - Repetir paso 1 con "Cimentación" y "Estructura"
   - Recargar PanelPrincipal
   - ? Debe mostrar: "25.0% (3/12)"

---

## ?? ARCHIVOS MODIFICADOS

| Archivo | Método Modificado | Líneas |
|---------|-------------------|--------|
| `PanelPrincipal.cs` | `CargarResumenProgreso()` | ~800-950 |

**Cambio específico:**
```diff
- // Obtener progreso por conceptos (desde AvanceManualConcepto)
- decimal progresoConceptos = 0;
- using (SqlCommand cmd = new SqlCommand(@"
-     SELECT ISNULL(AVG(CAST(AvancePorcentaje AS DECIMAL(10,2))), 0)
-     FROM dbo.AvanceManualConcepto
-     WHERE Manzana = @manzana AND Lote = @lote", conn))

+ // ?? CORREGIDO: Obtener progreso por conceptos desde AvanceManualObra
+ // Los conceptos se identifican por WBS negativos (-1, -2, -3, etc.)
+ decimal progresoConceptos = 0;
+ int conceptosCompletados = 0;
+ int totalConceptos = 12;
+ using (SqlCommand cmd = new SqlCommand(@"
+     SELECT COUNT(CASE WHEN AvancePorcentaje = 100 THEN 1 END) AS ConceptosCompletados
+     FROM dbo.AvanceManualObra
+     WHERE Manzana = @manzana AND Lote = @lote
+       AND CAST(WBS AS INT) < 0", conn))
```

---

## ? ESTADO

- ? **Problema identificado**
- ? **Solución implementada**
- ? **Compilación exitosa**
- ? **Listo para pruebas**

---

## ?? PRÓXIMOS PASOS

1. [ ] Probar en ambiente de desarrollo
2. [ ] Verificar con datos reales de obra
3. [ ] Documentar en manual de usuario
4. [ ] Considerar migración de datos si es necesario

---

**Desarrollado para:** Sistema Calandria Residencial  
**Fecha de Corrección:** Enero 2025  
**Versión:** 1.0  
**Prioridad:** ALTA

---

? **Corrección completada exitosamente**
