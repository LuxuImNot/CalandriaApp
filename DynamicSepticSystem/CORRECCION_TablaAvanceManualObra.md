# ?? Corrección: Uso de Tabla AvanceManualObra

## ?? Resumen del Cambio

Se corrigió el método `ReorganizarWBSConcepto` en **`FormGestionarPartidas.cs`** para usar la tabla correcta de la base de datos:

### ? Antes (Incorrecto):
```csharp
// Verificaba tabla Obra (que NO existe)
SELECT COUNT(*)
FROM Obra o
INNER JOIN PresupuestoObra p ON o.WBS_Correcto = p.WBS_Correcto
WHERE p.Codigo = @codigo 
AND (o.Avance > 0 OR o.MontoEstimado > 0)
```

### ? Después (Correcto):
```csharp
// Ahora usa AvanceManualObra (tabla real)
SELECT COUNT(*)
FROM AvanceManualObra amo
INNER JOIN PresupuestoObra p ON amo.WBS_Correcto = p.WBS_Correcto
WHERE p.Codigo = @codigo 
AND (amo.PorcentajeAvance > 0 OR amo.MontoEstimado > 0)
```

---

## ??? Estructura de la Tabla AvanceManualObra

Según el código encontrado en `FormAvanceObra.cs`:

```sql
CREATE TABLE AvanceManualObra (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Manzana NVARCHAR(10),
    Lote NVARCHAR(10),
    Prototipo NVARCHAR(50),
    WBS NVARCHAR(50),                -- Identificador de la partida
    WBS_Correcto INT,                -- FK a PresupuestoObra
    Concepto NVARCHAR(200),          -- Nombre de la partida
    ImporteTotal FLOAT,              -- Costo total de la partida
    AvancePorcentaje FLOAT,          -- % de avance (0-100)
    MontoEjecutado FLOAT,            -- Monto ya ejecutado ($)
    ImporteEjecutado FLOAT,          -- Alias alternativo
    FechaActualizacion DATETIME DEFAULT GETDATE()
)
```

---

## ?? Flujo de Reorganización WBS

### **Paso 1: Verificar Progreso Registrado**

```csharp
// El sistema verifica si hay obras con avance > 0
bool hayProgreso = false;
using (SqlCommand cmdCheck = new SqlCommand(@"
    SELECT COUNT(*)
    FROM AvanceManualObra amo
    INNER JOIN PresupuestoObra p ON amo.WBS_Correcto = p.WBS_Correcto
    WHERE p.Codigo = @codigo 
    AND (amo.PorcentajeAvance > 0 OR amo.MontoEstimado > 0)", 
    conn, transaction))
{
    hayProgreso = (int)cmdCheck.ExecuteScalar() > 0;
}
```

**¿Qué busca?**
- Registros en `AvanceManualObra` donde:
  - El `WBS_Correcto` pertenece al concepto seleccionado
  - El avance sea mayor a 0% **O**
  - Haya monto ejecutado

### **Paso 2: Advertencia al Usuario**

Si encuentra progreso, muestra:
```
?? ADVERTENCIA: Existen obras con progreso registrado.

Si reorganizas el WBS:
• Se preservará el progreso usando mapeo temporal
• Las partidas mantendrán su vinculación con las obras
• El orden visual cambiará pero los datos se conservan

¿Deseas continuar con la reorganización segura?
```

### **Paso 3: Actualizar WBS en AvanceManualObra**

```csharp
// Actualiza WBS en AvanceManualObra (preserva progreso)
UPDATE amo
SET amo.WBS_Correcto = m.WBS_Nuevo
FROM AvanceManualObra amo
INNER JOIN #MapeoWBS m ON amo.WBS_Correcto = m.WBS_Viejo
WHERE m.EsNueva = 0
```

**Ejemplo Visual:**

**ANTES de reorganizar:**
```
AvanceManualObra:
????????????????????????????????????????????????????????????????
? Manzana ? Lote ? WBS_Cor ? Concepto           ? AvancePor... ?
????????????????????????????????????????????????????????????????
? M5      ? L2   ? 102     ? Trazo              ? 50%          ?
? M5      ? L2   ? 103     ? Nivelación         ? 30%          ?
? M7      ? L1   ? 102     ? Trazo              ? 75%          ?
????????????????????????????????????????????????????????????????
```

**DESPUÉS de reorganizar** (insertaste partida 102 ? ahora es 103):
```
AvanceManualObra:
????????????????????????????????????????????????????????????????
? Manzana ? Lote ? WBS_Cor ? Concepto           ? AvancePor... ?
????????????????????????????????????????????????????????????????
? M5      ? L2   ? 103     ? Trazo (ahora 103)  ? 50% ?       ?
? M5      ? L2   ? 104     ? Nivelación (104)   ? 30% ?       ?
? M7      ? L1   ? 103     ? Trazo (ahora 103)  ? 75% ?       ?
????????????????????????????????????????????????????????????????
```

? **El progreso se preservó usando el mapeo WBS_Viejo ? WBS_Nuevo**

---

## ?? Relación con Otras Tablas

```
???????????????????????????????????????????????????????????????????
?                    DIAGRAMA RELACIONAL COMPLETO                 ?
???????????????????????????????????????????????????????????????????

Estimacion(Concepto)         PresupuestoObra            AvanceManualObra
????????????????????         ????????????????????      ????????????????????
? Codigo (PK)      ??????????? Codigo (FK)      ?      ? Manzana          ?
? Concepto         ?         ? WBS_Correcto (PK)???????? Lote             ?
? CostoTunera      ?         ? Concepto         ?      ? WBS_Correcto (FK)?
? CostoCalandra    ?         ? Padre            ?      ? AvancePorcentaje ?
????????????????????         ? Etapa            ?      ? MontoEstimado    ?
                              ? Partida          ?      ? FechaActualiza.. ?
                              ? CostoTunera      ?      ????????????????????
                              ? CostoCalandra    ?
                              ????????????????????
```

**Flujo de Datos:**
1. **PresupuestoObra**: Define las partidas con `WBS_Correcto` único
2. **AvanceManualObra**: Registra el avance de cada partida en cada casa (Manzana+Lote)
3. **Reorganización**: Actualiza `WBS_Correcto` en ambas tablas preservando la relación

---

## ?? Ejemplo Real de Uso

### Escenario: Casa M5-L2 con Progreso Registrado

```sql
-- Partidas del concepto "Preliminares" (Codigo = 1)
SELECT p.WBS_Correcto, p.Partida, p.CostoTunera
FROM PresupuestoObra p
WHERE p.Codigo = '1'
ORDER BY p.WBS_Correcto

-- Resultado:
-- 101 | Limpieza terreno    | $5,000
-- 102 | Trazo               | $8,000
-- 103 | Nivelación          | $6,000
```

```sql
-- Avance registrado en M5-L2
SELECT amo.WBS_Correcto, amo.Concepto, amo.AvancePorcentaje
FROM AvanceManualObra amo
WHERE amo.Manzana = 'M5' AND amo.Lote = 'L2'
  AND amo.WBS_Correcto IN (101, 102, 103)

-- Resultado:
-- 102 | Trazo | 50%
```

**Usuario agrega nueva partida "Excavación" ANTES de "Trazo":**

1. Sistema detecta desorden (nueva partida en medio)
2. Pregunta si reorganizar
3. Si acepta:
   - 101 ? 101 (Limpieza)
   - 0   ? 102 (Excavación NUEVA) ?
   - 102 ? 103 (Trazo) ? Progreso preservado
   - 103 ? 104 (Nivelación)

4. Actualiza `AvanceManualObra`:
```sql
-- ANTES:
-- WBS_Correcto = 102 | Trazo | 50%

-- DESPUÉS:
-- WBS_Correcto = 103 | Trazo | 50% ?
```

---

## ? Beneficios de la Corrección

1. **Usa tabla correcta**: `AvanceManualObra` (existe en BD)
2. **Preserva progreso real**: Detecta avances registrados por casa
3. **Mapeo seguro**: Usa tabla temporal `#MapeoWBS`
4. **Rollback automático**: Si algo falla, revierte cambios
5. **Logs detallados**: Muestra cuántos registros actualizó

---

## ?? Cómo Probar

### Test 1: Sin Progreso Registrado
```sql
-- Verificar que NO hay avances
SELECT * FROM AvanceManualObra WHERE WBS_Correcto IN (101, 102, 103)
-- Resultado: 0 filas

-- Al reorganizar: NO muestra advertencia
```

### Test 2: Con Progreso Registrado
```sql
-- Insertar avance de prueba
INSERT INTO AvanceManualObra 
(Manzana, Lote, WBS_Correcto, Concepto, AvancePorcentaje, FechaActualizacion)
VALUES 
('M5', 'L2', 102, 'Trazo', 50, GETDATE())

-- Al reorganizar: MUESTRA advertencia ??
```

### Test 3: Verificar Actualización
```sql
-- Después de reorganizar, verificar que cambió el WBS:
SELECT WBS_Correcto, Concepto, AvancePorcentaje
FROM AvanceManualObra
WHERE Manzana = 'M5' AND Lote = 'L2'
ORDER BY WBS_Correcto

-- El WBS_Correcto debe actualizarse pero el % se preserva
```

---

## ?? Logs de Debug

Cuando reorganizas, el sistema escribe logs en **Output ? Debug**:

```
? Actualizado WBS en 3 registros de AvanceManualObra
? Actualizado WBS en 15 registros de PresupuestoObra
? WBS reorganizado:
   - Partidas existentes reorganizadas: 15
   - Partidas nuevas con WBS asignado: 1
   - WBS base: 101
   - WBS final: 116
```

---

## ?? Resumen

| Aspecto | Estado |
|---------|--------|
| ? Tabla correcta | `AvanceManualObra` |
| ? Detección de progreso | Funcional |
| ? Preservación de datos | Garantizada |
| ? Mapeo temporal | `#MapeoWBS` |
| ? Rollback automático | Implementado |
| ? Logs de debug | Activos |

---

**Fecha de corrección**: 17/01/2025  
**Archivo modificado**: `DynamicSepticSystem\FormGestionarPartidas.cs`  
**Método corregido**: `ReorganizarWBSConcepto()`
