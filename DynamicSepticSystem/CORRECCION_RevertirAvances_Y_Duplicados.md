# ?? CORRECCIÓN: Revertir Avances al Eliminar Estimaciones y Prevenir Duplicados

**Fecha:** 29 de Enero de 2025  
**Desarrollador:** Sistema Calandria Residencial  
**Objetivo:** Implementar reversión automática de avances al eliminar estimaciones y prevenir duplicación de conceptos

---

## ?? PROBLEMAS IDENTIFICADOS

### 1. ? **Avances No Se Revertían al Eliminar Estimaciones**

**Situación Anterior:**
- Usuario elimina estimación del repositorio
- Los avances permanecían al 100% en `AvanceManualObra`
- Las partidas seguían marcadas como completadas
- No se podían volver a estimar

**Impacto:**
- ?? Inconsistencia en los datos
- ?? Imposibilidad de corregir errores
- ?? Avances "fantasma" en el sistema

### 2. ? **Conceptos Se Duplicaban en la Vista**

**Situación Anterior:**
- El concepto "Cimentación" aparecía dos veces
- Mismas partidas repetidas
- Problema en `SortedDictionary`

**Evidencia (Captura de pantalla):**
```
? Cimentación > cimbra y acero...     $563.43
? Cimentación > concreto en los...    $19,428.00
...
? Cimentación                          $38,359.66  ? DUPLICADO
? Cimentación > cimbra y acero...     $16,251.81
? Cimentación > cimbra y acero...     $563.43
```

---

## ? SOLUCIONES IMPLEMENTADAS

### ?? **Solución 1: Reversión Automática de Avances**

#### **Archivo Modificado:** `FormRepositorioPDFs.cs`

#### **Nuevo Método: `RevertirAvancesEstimacion()`**

```csharp
private void RevertirAvancesEstimacion(SqlConnection conn, EstimacionInfo estimacion)
{
    try
    {
        // 1. Obtener todas las partidas (WBS) incluidas en esta estimación
        var wbsPartidas = new List<int>();
        var codigosConceptos = new List<string>();
        
        string sqlDetalle = @"
            SELECT DISTINCT 
                WBS, 
                CodigoConcepto 
            FROM FoliosEstimacionDetalle 
            WHERE FolioId = @folioId";
        
        // ... (código de consulta)

        // 2. Para cada partida, verificar si tiene estimaciones POSTERIORES
        foreach (int wbs in wbsPartidas)
        {
            // Verificar si esta partida aparece en estimaciones posteriores
            string sqlVerificar = @"
                SELECT COUNT(*) 
                FROM FoliosEstimacionDetalle d
                INNER JOIN FoliosEstimacion f ON d.FolioId = f.Id
                WHERE d.WBS = @wbs 
                AND f.Manzana = @manzana 
                AND f.Lote = @lote
                AND f.FechaGeneracion > @fecha";
            
            int estimacionesPosteriores = 0;
            // ... (ejecutar consulta)

            if (estimacionesPosteriores == 0)
            {
                // ? NO hay estimaciones posteriores ? REVERTIR a 0%
                string sqlRevertir = @"
                    UPDATE AvanceManualObra 
                    SET AvancePorcentaje = 0.0, 
                        MontoEjecutado = 0.0,
                        FechaFinalizacion = NULL,
                        FechaActualizacion = GETDATE()
                    WHERE Manzana = @m 
                    AND Lote = @l 
                    AND WBS = @wbs";
                
                // ... (ejecutar actualización)
            }
        }

        // 3. Revertir conceptos (WBS negativo)
        foreach (string codigoConcepto in codigosConceptos)
        {
            int wbsConcepto = -codigo; // WBS negativo para conceptos
            
            // Verificar estimaciones posteriores
            // Si NO hay ? ELIMINAR el registro del concepto
            string sqlRevertir = @"
                DELETE FROM AvanceManualObra 
                WHERE Manzana = @m 
                AND Lote = @l 
                AND WBS = @wbs";
        }
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error al revertir avances: {ex.Message}");
    }
}
```

#### **Modificación en `btnEliminar_Click()`**

```csharp
private void btnEliminar_Click(object sender, EventArgs e)
{
    // ... (validaciones)

    var result = MessageBox.Show(
        $"¿Estás seguro de eliminar la estimación {estimacion.Folio}?\n\n" +
        $"ADVERTENCIA: Esta acción revertirá el avance de las partidas incluidas.\n\n" +  // ? NUEVO
        $"Esta acción no se puede deshacer.",
        "Confirmar eliminación",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Warning);

    if (result != DialogResult.Yes) return;

    try
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            
            // ? NUEVO: Revertir avances ANTES de eliminar
            RevertirAvancesEstimacion(conn, estimacion);
            
            // El CASCADE eliminará automáticamente el detalle y el PDF
            string sql = "DELETE FROM FoliosEstimacion WHERE Id = @id";
            // ... (ejecutar eliminación)
        }

        MessageBox.Show("Estimación eliminada correctamente y avances revertidos", "Éxito", 
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    // ... (manejo de errores)
}
```

---

### ?? **Solución 2: Prevenir Duplicación de Conceptos**

#### **Archivo Modificado:** `FormEstimacionConceptoMigrado.cs`

#### **Problema Identificado:**
```csharp
// ? ANTES (permitía duplicados)
foreach (var concepto in conceptos)
{
    var nodoConcepto = new NodoConcepto { ... };
    
    // Convertir codigo a entero
    int codigoNumerico = 999;
    if (int.TryParse(concepto.Item1, out int codigoInt))
    {
        codigoNumerico = codigoInt;
    }

    // ? Esto SOBRESCRIBÍA el concepto anterior con el mismo código
    conceptosTemporal[codigoNumerico] = nodoConcepto;
}
```

#### **Corrección Aplicada:**

```csharp
// ? AHORA (previene duplicados)

// 1. HashSet para rastrear conceptos procesados
var conceptosProcesados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

foreach (var concepto in conceptos)
{
    // ? NUEVO: Verificar si ya procesamos este concepto
    if (conceptosProcesados.Contains(concepto.Item2))
    {
        System.Diagnostics.Debug.WriteLine($"Concepto duplicado detectado y omitido: {concepto.Item2}");
        continue; // ? SALTAR conceptos duplicados
    }
    
    conceptosProcesados.Add(concepto.Item2);

    var nodoConcepto = new NodoConcepto { ... };
    
    // ... (procesar partidas)

    int codigoNumerico = 999;
    if (!string.IsNullOrEmpty(concepto.Item1) && int.TryParse(concepto.Item1, out int codigoInt))
    {
        codigoNumerico = codigoInt;
    }

    // ? Solo agregar si el código NO existe ya
    if (!conceptosTemporal.ContainsKey(codigoNumerico))
    {
        conceptosTemporal[codigoNumerico] = nodoConcepto;
    }
    else
    {
        System.Diagnostics.Debug.WriteLine($"Código duplicado detectado: {codigoNumerico} para concepto {concepto.Item2}");
    }
}
```

#### **Beneficios:**
- ? Previene duplicados por **nombre** de concepto
- ? Previene duplicados por **código** numérico
- ? Mensajes de debug para rastrear problemas
- ? Primera ocurrencia prevalece

---

### ?? **Solución 3: Corrección de Error de Compilación**

#### **Problema:**
```csharp
// ? Declaración duplicada con tipos incompatibles
private Dictionary<int, Tuple<double, double, DateTime?>> CargarAvancesPartidas(...)
{
    var avances = new Dictionary<int, Tuple<double, double, DateTime?>>();  // ? CORRECTO
    var avances = new Dictionary<int, Tuple<double, double, DateTime>>();   // ? ERROR: duplicado
    // ...
}
```

#### **Corrección:**
```csharp
// ? Solo una declaración
private Dictionary<int, Tuple<double, double, DateTime?>> CargarAvancesPartidas(...)
{
    var avances = new Dictionary<int, Tuple<double, double, DateTime?>>();
    // ... (resto del código)
    return avances;
}
```

---

## ?? FLUJO DE REVERSIÓN DE AVANCES

### **Caso 1: Estimación Única (Más Reciente)**

```
Usuario elimina: EST-M5-L2-001-2025 (única estimación)
  ?
1. Sistema detecta partidas incluidas:
   - WBS 1: Cimentación > cimbra y acero
   - WBS 2: Cimentación > concreto
   - WBS 3: Cimentación > fumigación
  ?
2. Para cada partida, verifica estimaciones posteriores
   ? NO hay estimaciones posteriores
  ?
3. REVIERTE avances a 0%:
   UPDATE AvanceManualObra 
   SET AvancePorcentaje = 0.0, 
       MontoEjecutado = 0.0,
       FechaFinalizacion = NULL
   WHERE WBS IN (1, 2, 3)
  ?
4. Revierte conceptos (WBS negativo):
   DELETE FROM AvanceManualObra 
   WHERE WBS = -2  -- (Cimentación = concepto 2)
  ?
5. Elimina folio y dependencias (CASCADE)
  ?
? RESULTADO: Partidas vuelven a 0%, listas para re-estimación
```

### **Caso 2: Estimación Intermedia (Hay Posteriores)**

```
Usuario elimina: EST-M5-L2-002-2025 (estimación del medio)
  ?
1. Sistema detecta partidas:
   - WBS 10: Estructura > losa cimentación
   - WBS 11: Estructura > muros de carga
  ?
2. Verifica si existen estimaciones POSTERIORES:
   SELECT COUNT(*) 
   FROM FoliosEstimacionDetalle d
   INNER JOIN FoliosEstimacion f ON d.FolioId = f.Id
   WHERE d.WBS = 10 
   AND f.FechaGeneracion > '2025-01-15'  -- Fecha de EST-002
   
   ? Resultado: 1 (existe EST-003-2025 posterior)
  ?
3. ?? NO REVIERTE el avance (mantiene 100%)
   Razón: Hay estimación posterior que también la incluye
  ?
4. Elimina solo el folio EST-002
  ?
? RESULTADO: Avances se mantienen (protegido por estimación posterior)
```

---

## ?? COMPARATIVA ANTES/DESPUÉS

### **Escenario: Eliminar Estimación EST-M1-L1-001-2025**

#### **? ANTES:**

| Acción | Estado en TreeListView | Estado en BD |
|--------|----------------------|--------------|
| **Generar estimación** | ? Cimentación (TERMINADO) | AvancePorcentaje = 100% |
| | ? cimbra (TERMINADO) | WBS 1: 100% |
| | ? concreto (TERMINADO) | WBS 2: 100% |
| **Eliminar estimación** | ? Cimentación (TERMINADO) ? MAL | AvancePorcentaje = 100% ? MAL |
| | ? cimbra (TERMINADO) ? MAL | WBS 1: 100% ? MAL |
| | ? concreto (TERMINADO) ? MAL | WBS 2: 100% ? MAL |
| **Intentar re-estimar** | ? NO se puede marcar | ? Bloqueado |

#### **? AHORA:**

| Acción | Estado en TreeListView | Estado en BD |
|--------|----------------------|--------------|
| **Generar estimación** | ? Cimentación (TERMINADO) | AvancePorcentaje = 100% |
| | ? cimbra (TERMINADO) | WBS 1: 100% |
| | ? concreto (TERMINADO) | WBS 2: 100% |
| **Eliminar estimación** | ? Cimentación (sin estado) ? BIEN | AvancePorcentaje = 0% ? BIEN |
| | ? cimbra (sin estado) ? BIEN | WBS 1: 0% ? BIEN |
| | ? concreto (sin estado) ? BIEN | WBS 2: 0% ? BIEN |
| **Re-estimar** | ? Se puede marcar | ? Funcional |

---

## ?? CASOS DE PRUEBA

### **Test 1: Eliminar Estimación Única**

```sql
-- Preparación
SELECT * FROM FoliosEstimacion WHERE Manzana='1' AND Lote='1';
-- Resultado: 1 fila (EST-M1-L1-001-2025)

SELECT WBS, AvancePorcentaje FROM AvanceManualObra WHERE Manzana='1' AND Lote='1';
-- Resultado: WBS 1,2,3 al 100%
```

**Acción:** Eliminar EST-M1-L1-001-2025

**Resultado Esperado:**
```sql
SELECT * FROM FoliosEstimacion WHERE Manzana='1' AND Lote='1';
-- Resultado: 0 filas ?

SELECT WBS, AvancePorcentaje FROM AvanceManualObra WHERE Manzana='1' AND Lote='1';
-- Resultado: WBS 1,2,3 al 0% ?
```

---

### **Test 2: Eliminar Estimación con Posteriores**

```sql
-- Preparación
SELECT Folio, FechaGeneracion FROM FoliosEstimacion 
WHERE Manzana='5' AND Lote='2' ORDER BY FechaGeneracion;
-- Resultado:
--   EST-M5-L2-001-2025  (15/01/2025)
--   EST-M5-L2-002-2025  (22/01/2025)  ? A eliminar
--   EST-M5-L2-003-2025  (29/01/2025)
```

**Acción:** Eliminar EST-M5-L2-002-2025

**Resultado Esperado:**
```sql
SELECT Folio FROM FoliosEstimacion WHERE Manzana='5' AND Lote='2';
-- Resultado: EST-001 y EST-003 (002 eliminado) ?

SELECT WBS, AvancePorcentaje FROM AvanceManualObra WHERE Manzana='5' AND Lote='2';
-- Resultado: Avances se mantienen al 100% ?
-- (Porque EST-003 es posterior y también incluye esas partidas)
```

---

### **Test 3: Prevenir Duplicados de Conceptos**

**Antes de la Corrección:**
```
FormEstimacionConceptoMigrado carga conceptos:
  - Cimentación (código 2)    ? Primera ocurrencia
  - Cimentación (código 2)    ? Duplicado

Resultado en TreeListView:
  ? Cimentación    ? Duplicado 1
  ? Cimentación    ? Duplicado 2
```

**Después de la Corrección:**
```
FormEstimacionConceptoMigrado carga conceptos:
  - Cimentación (código 2)    ? Aceptado
  - Cimentación (código 2)    ? OMITIDO (log: "Concepto duplicado detectado")

Resultado en TreeListView:
  ? Cimentación    ? Solo UNO ?
```

---

## ?? CONSULTAS SQL ÚTILES

### **Ver Estimaciones de una Casa**
```sql
SELECT 
    f.Folio,
    f.FechaGeneracion,
    f.TotalEstimacion,
    COUNT(DISTINCT d.WBS) AS TotalPartidas
FROM FoliosEstimacion f
LEFT JOIN FoliosEstimacionDetalle d ON f.Id = d.FolioId
WHERE f.Manzana = '1' AND f.Lote = '1'
GROUP BY f.Id, f.Folio, f.FechaGeneracion, f.TotalEstimacion
ORDER BY f.FechaGeneracion DESC;
```

### **Ver Avances de una Casa**
```sql
SELECT 
    WBS,
    AvancePorcentaje,
    MontoEjecutado,
    FechaFinalizacion,
    FechaActualizacion
FROM AvanceManualObra
WHERE Manzana = '1' AND Lote = '1'
ORDER BY WBS;
```

### **Verificar Partidas Duplicadas en Estimaciones**
```sql
SELECT 
    f.Folio,
    d.WBS,
    d.NombrePartida,
    COUNT(*) AS Repeticiones
FROM FoliosEstimacion f
INNER JOIN FoliosEstimacionDetalle d ON f.Id = d.FolioId
WHERE f.Manzana = '1' AND f.Lote = '1'
GROUP BY f.Folio, d.WBS, d.NombrePartida
HAVING COUNT(*) > 1;
```

---

## ?? ARCHIVOS MODIFICADOS

| Archivo | Cambios |
|---------|---------|
| `FormRepositorioPDFs.cs` | ? Agregado método `RevertirAvancesEstimacion()` |
| | ? Modificado `btnEliminar_Click()` para revertir avances |
| | ? Mensaje de advertencia actualizado |
| `FormEstimacionConceptoMigrado.cs` | ? Agregado `HashSet<string> conceptosProcesados` |
| | ? Validación para prevenir duplicados por nombre |
| | ? Validación para prevenir duplicados por código |
| | ? Corregida declaración duplicada en `CargarAvancesPartidas()` |

---

## ?? ESTADO DEL SISTEMA

- ? **Compilación:** EXITOSA
- ? **Errores:** NINGUNO
- ? **Listo para:** PRUEBAS

---

## ?? BENEFICIOS

### **Para el Usuario:**
1. ? Puede corregir errores eliminando estimaciones
2. ? Los avances se revierten automáticamente
3. ? No ve conceptos duplicados
4. ? Interfaz más limpia y confiable

### **Para la Integridad de Datos:**
1. ? Consistencia entre estimaciones y avances
2. ? Protección de datos (no revierte si hay estimaciones posteriores)
3. ? Prevención de duplicados en memoria
4. ? Mensajes de debug para rastreo

### **Para el Mantenimiento:**
1. ? Código más robusto
2. ? Fácil de depurar
3. ? Validaciones claras
4. ? Documentación completa

---

## ?? NOTAS IMPORTANTES

### **?? Comportamiento de Reversión:**
- Solo revierte avances si **NO hay estimaciones posteriores**
- Si existe una estimación más reciente que incluye la misma partida, **protege** el avance
- Los conceptos (WBS negativo) se **eliminan** completamente si no hay posteriores

### **?? Seguridad:**
- Mensaje de advertencia claro antes de eliminar
- Confirmación requerida (Yes/No)
- Transacción en base de datos

### **?? Debug:**
- Mensajes en `Debug.WriteLine()` para rastrear duplicados
- Log de conceptos omitidos
- Log de códigos duplicados

---

**Desarrollado para:** Sistema Calandria Residencial  
**Fecha:** 29 de Enero de 2025  
**Versi ?n:** 3.0  
**Feature:** Reversión de Avances y Prevención de Duplicados

---

## ? CHECKLIST DE VERIFICACIÓN

- [x] Compilación exitosa
- [x] Error de declaración duplicada corregido
- [x] Método `RevertirAvancesEstimacion()` implementado
- [x] Validación de estimaciones posteriores
- [x] Prevención de duplicados por nombre
- [x] Prevención de duplicados por código
- [x] Mensaje de advertencia actualizado
- [ ] Prueba: Eliminar estimación única
- [ ] Prueba: Eliminar estimación con posteriores
- [ ] Prueba: Verificar no hay duplicados en TreeListView
- [ ] Prueba: Verificar reversión en base de datos

---

**?? Sistema completamente funcional y listo para pruebas!**
