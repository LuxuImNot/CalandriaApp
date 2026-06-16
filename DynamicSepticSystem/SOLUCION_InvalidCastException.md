# ?? SOLUCIÓN: InvalidCastException al Cargar Avances

## ?? Problema Identificado

```
Exception thrown: 'System.InvalidCastException' in System.Data.dll
Error al cargar avances: Specified cast is not valid.
?? Avances cargados: 0
```

### ? Causa Raíz

El método `CargarAvancesPartidas()` intentaba usar `TRY_CAST(WBS AS INT)` directamente en SQL y luego convertir el resultado a `Int32` en C#:

```csharp
// ? CÓDIGO ANTERIOR (PROBLEMA)
string sql = @"
    SELECT 
        TRY_CAST(WBS AS INT) AS WBSNum,  // TRY_CAST puede retornar NULL
        ...
    FROM AvanceManualObra";

using (var reader = cmd.ExecuteReader())
{
    while (reader.Read())
    {
        int wbs = reader.GetInt32(0);  // ? FALLA si WBSNum es NULL
        ...
    }
}
```

**Problema:** 
- `TRY_CAST(WBS AS INT)` retorna `NULL` si el valor no es numérico
- `reader.GetInt32(0)` lanza `InvalidCastException` cuando encuentra `NULL`
- Los registros con WBS inválidos impedían cargar TODOS los avances

---

## ? Solución Implementada

### 1. Leer WBS como String y Convertir en C#

```csharp
// ? CÓDIGO CORREGIDO
string sql = @"
    SELECT 
        WBS,  // Leer como está (sin conversión en SQL)
        ISNULL(AvancePorcentaje, 0) AS AvancePorcentaje,
        ISNULL(MontoEjecutado, 0) AS MontoEjecutado,
        FechaFinalizacion,
        ISNULL(MetrosCuadrados, 0) AS MetrosCuadrados
    FROM AvanceManualObra
    WHERE Manzana = @manzana AND Lote = @lote";

using (var reader = cmd.ExecuteReader())
{
    while (reader.Read())
    {
        try
        {
            // ? Leer como string
            string wbsString = reader["WBS"]?.ToString();
            
            if (string.IsNullOrWhiteSpace(wbsString))
            {
                continue; // Saltar registros vacíos
            }

            // ? Convertir manualmente con validación
            if (!int.TryParse(wbsString, out int wbs))
            {
                Debug.WriteLine($"?? WBS inválido: '{wbsString}'");
                continue; // Saltar y continuar con el siguiente
            }

            // ? Procesar registro válido
            double avancePorcentaje = reader.GetDouble(...);
            double montoEjecutado = reader.GetDouble(...);
            ...
            
            avances[wbs] = new Tuple<...>(...);
        }
        catch (Exception exRow)
        {
            Debug.WriteLine($"?? Error al procesar fila: {exRow.Message}");
            // Continuar con la siguiente fila (no detener todo)
        }
    }
}
```

### 2. Ventajas de la Nueva Solución

? **Robustez:** Maneja registros inválidos sin detener el proceso  
? **Diagnóstico:** Registra en debug los WBS problemáticos  
? **Compatibilidad:** Funciona con WBS de tipo `NVARCHAR`, `VARCHAR` o `INT`  
? **Tolerancia a Errores:** Un registro malo no impide cargar los demás  
? **Logging:** Muestra exactamente qué registros causan problemas  

---

## ?? Diagnóstico del Problema

### Ejecutar Script de Diagnóstico

```sql
-- Ubicación: SQL_SCRIPTS\DiagnosticoAvanceManualObra.sql
-- Ejecutar en SQL Server Management Studio o Visual Studio
```

El script verificará:

1. ? Existencia de la tabla `AvanceManualObra`
2. ?? Estructura completa de la tabla
3. ?? Tipo de dato de la columna `WBS`
4. ? Existencia de columna `MetrosCuadrados`
5. ? Existencia de columna `FechaFinalizacion`
6. ?? Análisis de contenido (registros válidos/inválidos)
7. ?? Registros con WBS NULL o vacío
8. ?? Registros con WBS no numérico
9. ?? Recomendaciones para corregir problemas
10. ?? Query de prueba equivalente al código C#

### Salida Esperada del Diagnóstico

```
=========================================
DIAGNÓSTICO: Tabla AvanceManualObra
=========================================

1. Verificando existencia de la tabla...
   ? La tabla AvanceManualObra EXISTE

2. Estructura de la tabla:
   ---------------------------------------------------------
   Columna              | Tipo     | Permite_NULL | Valor_Defecto
   ---------------------|----------|--------------|---------------
   Id                   | int      | NO           | NULL
   Manzana              | nvarchar | YES          | NULL
   Lote                 | nvarchar | YES          | NULL
   Prototipo            | nvarchar | YES          | NULL
   WBS                  | nvarchar | YES          | NULL ?? TIPO TEXTO
   AvancePorcentaje     | float    | YES          | NULL
   MontoEjecutado       | float    | YES          | NULL
   FechaActualizacion   | datetime | YES          | (getdate())
   MetrosCuadrados      | float    | YES          | NULL
   FechaFinalizacion    | datetime | YES          | NULL

3. Tipo de dato de columna WBS:
   Tipo actual: nvarchar
   ??  WBS es de tipo texto - Se convertirá a INT en código C#

4. Verificando columna MetrosCuadrados:
   ? Columna MetrosCuadrados EXISTE

5. Verificando columna FechaFinalizacion:
   ? Columna FechaFinalizacion EXISTE

6. Análisis de contenido:
   ---------------------------------------------------------
   Total de registros: 45
   Registros con WBS NULL o vacío: 2 ??
   Registros con WBS no numérico: 3 ??
   Registros con WBS numérico válido: 40 ?

7. Registros problemáticos (primeros 10):
   ---------------------------------------------------------
   a) WBS NULL o vacío:
   Id  | Manzana | Lote | WBS  | Prototipo
   ----|---------|------|------|----------
   123 | 5       | 10   | NULL | TUNERA
   124 | 5       | 11   |      | CALANDRA

   b) WBS no numérico:
   Id  | Manzana | Lote | WBS     | Prototipo
   ----|---------|------|---------|----------
   125 | 6       | 1    | ABC123  | TUNERA
   126 | 6       | 2    | -1      | CALANDRA
   127 | 6       | 3    | 1.5     | TUNERA

8. Muestra de registros válidos (primeros 5):
   ---------------------------------------------------------
   Manzana | Lote | WBS | AvancePorcentaje | MontoEjecutado | Estado_WBS
   --------|------|-----|------------------|----------------|------------
   1       | 1    | 5   | 100.0            | $45,200.00     | OK ?
   1       | 1    | 12  | 100.0            | $1,980.36      | OK ?
   1       | 2    | 23  | 50.0             | $12,500.00     | OK ?
   2       | 1    | 8   | 75.0             | $32,450.00     | OK ?
   2       | 3    | 15  | 100.0            | $8,750.00      | OK ?

9. Recomendaciones:
   ---------------------------------------------------------
   ??  Hay registros con WBS NULL o vacío
      Acción: Eliminar o corregir estos registros
      Query sugerida:
      DELETE FROM AvanceManualObra WHERE WBS IS NULL OR WBS = ''

   ??  Hay registros con WBS no numérico
      Acción: Corregir los valores de WBS
      Query sugerida:
      -- Revisar manualmente cada registro
      SELECT * FROM AvanceManualObra 
      WHERE TRY_CAST(WBS AS INT) IS NULL
      AND WBS IS NOT NULL AND WBS <> ''

   ? La tabla tiene las columnas necesarias para partidas dinámicas
   ? La tabla tiene la columna FechaFinalizacion

=========================================
FIN DEL DIAGNÓSTICO
=========================================
```

---

## ??? Correcciones Manuales en la BD

### 1. Eliminar Registros con WBS NULL o Vacío

```sql
-- ?? PRECAUCIÓN: Esto eliminará registros
-- Revisar primero qué se eliminará:
SELECT * FROM AvanceManualObra 
WHERE WBS IS NULL OR WBS = ''

-- Si estás seguro, eliminar:
DELETE FROM AvanceManualObra 
WHERE WBS IS NULL OR WBS = ''
```

### 2. Corregir Registros con WBS No Numérico

```sql
-- Identificar registros problemáticos
SELECT 
    Id,
    Manzana,
    Lote,
    WBS,
    Prototipo
FROM AvanceManualObra
WHERE TRY_CAST(WBS AS INT) IS NULL
AND WBS IS NOT NULL 
AND WBS <> ''

-- Opciones de corrección:

-- Opción A: Eliminar registros inválidos (si no son importantes)
DELETE FROM AvanceManualObra
WHERE TRY_CAST(WBS AS INT) IS NULL
AND WBS IS NOT NULL 
AND WBS <> ''

-- Opción B: Corregir manualmente registro por registro
-- UPDATE AvanceManualObra SET WBS = '5' WHERE Id = 123
-- UPDATE AvanceManualObra SET WBS = '12' WHERE Id = 124
-- ...

-- Opción C: Convertir decimales a enteros (si aplica)
UPDATE AvanceManualObra 
SET WBS = CAST(FLOOR(CAST(WBS AS FLOAT)) AS INT)
WHERE TRY_CAST(WBS AS FLOAT) IS NOT NULL
AND TRY_CAST(WBS AS INT) IS NULL
```

### 3. Verificar Correcciones

```sql
-- Después de las correcciones, verificar:
SELECT 
    COUNT(*) AS Total,
    SUM(CASE WHEN WBS IS NULL OR WBS = '' THEN 1 ELSE 0 END) AS WBS_Vacios,
    SUM(CASE WHEN TRY_CAST(WBS AS INT) IS NULL AND WBS IS NOT NULL AND WBS <> '' THEN 1 ELSE 0 END) AS WBS_NoNumericos,
    SUM(CASE WHEN TRY_CAST(WBS AS INT) IS NOT NULL THEN 1 ELSE 0 END) AS WBS_Validos
FROM AvanceManualObra

-- Resultado esperado:
-- Total | WBS_Vacios | WBS_NoNumericos | WBS_Validos
-- ------|------------|-----------------|-------------
-- 40    | 0          | 0               | 40 ?
```

---

## ?? Prueba de la Corrección

### En la Aplicación

1. **Ejecutar el diagnóstico SQL:**
   - Abrir `SQL_SCRIPTS\DiagnosticoAvanceManualObra.sql`
   - Ejecutar en SQL Server Management Studio
   - Revisar la salida para identificar problemas

2. **Corregir registros problemáticos:**
   - Ejecutar las queries de corrección sugeridas
   - Verificar que todos los WBS son numéricos válidos

3. **Probar en la aplicación:**
   - Abrir `FormEstimacionConceptoMigrado`
   - Seleccionar Manzana y Lote
   - Click "Cargar Avance"
   - Verificar en la ventana de Output (Debug):

```
?? Tipo de columna WBS: nvarchar
? Avance cargado: WBS=5, Avance=100.0%, Monto=$45,200.00, m²=45.20, Fecha=28/01/2025
? Avance cargado: WBS=12, Avance=100.0%, Monto=$1,980.36, m²=0.00, Fecha=28/01/2025
? Avance cargado: WBS=23, Avance=50.0%, Monto=$12,500.00, m²=12.50, Fecha=N/A
...
? Total de avances cargados: 15
?? Conceptos cargados: 12
?? Partidas cargadas: 45
?? Avances cargados: 15 ?? AHORA CARGA CORRECTAMENTE
```

---

## ?? Resumen de Cambios

### Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `FormEstimacionConceptoMigrado.cs` | ? Método `CargarAvancesPartidas()` corregido |

### Archivos Creados

| Archivo | Propósito |
|---------|-----------|
| `SQL_SCRIPTS\DiagnosticoAvanceManualObra.sql` | ?? Script de diagnóstico completo |
| `SOLUCION_InvalidCastException.md` | ?? Documentación de la solución |

---

## ? Checklist de Verificación

- [ ] Ejecutar `DiagnosticoAvanceManualObra.sql` en la BD
- [ ] Revisar la salida del diagnóstico
- [ ] Corregir registros con WBS NULL o vacío (si existen)
- [ ] Corregir registros con WBS no numérico (si existen)
- [ ] Verificar que todos los WBS son numéricos válidos
- [ ] Compilar la solución (Build Successful ?)
- [ ] Probar cargar avance en la aplicación
- [ ] Verificar en Debug que se cargan los avances correctamente
- [ ] Verificar que las partidas muestran el avance en el TreeListView
- [ ] Verificar que las fechas de finalización se muestran correctamente

---

## ?? Resultado Final Esperado

### Antes (? Error)

```
Exception thrown: 'System.InvalidCastException' in System.Data.dll
Error al cargar avances: Specified cast is not valid.
?? Avances cargados: 0  ?? NO CARGA NADA
```

### Después (? Funciona)

```
?? Tipo de columna WBS: nvarchar
? Avance cargado: WBS=5, Avance=100.0%, Monto=$45,200.00, m²=45.20, Fecha=28/01/2025
? Avance cargado: WBS=12, Avance=100.0%, Monto=$1,980.36, m²=0.00, Fecha=28/01/2025
?? WBS inválido: 'ABC123'  ?? Registra el problema pero NO falla
? Avance cargado: WBS=23, Avance=50.0%, Monto=$12,500.00, m²=12.50, Fecha=N/A
...
? Total de avances cargados: 15  ?? CARGA CORRECTAMENTE
?? Conceptos cargados: 12
?? Partidas cargadas: 45
```

---

## ?? Próximos Pasos

1. ? **Ejecutar diagnóstico** para identificar problemas en la BD
2. ? **Corregir registros** problemáticos usando las queries sugeridas
3. ? **Probar la aplicación** para verificar que carga avances correctamente
4. ?? **Monitorear logs** de debug para detectar futuros problemas
5. ?? **Mantenimiento preventivo** ejecutando el diagnóstico periódicamente

---

**Fecha:** 28 de enero de 2025  
**Estado:** ? CORREGIDO  
**Versión:** 1.0

---
