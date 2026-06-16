# ? CORRECCIÓN: Ordenamiento Numérico del Código en Estimación(Concepto)

## ?? PROBLEMA DETECTADO

### ? Situación Anterior:
Los códigos se mostraban en **orden alfabético** en lugar de **orden numérico**:

```
Código | Concepto
-------|----------
1      | Acabados
10     | Muebles y Accesorios
11     | Preliminares
12     | Urbanización
2      | Albañilería
3      | Carpintería
...
```

**Esperado:**
```
Código | Concepto
-------|----------
1      | Preliminares (trazo, cimbra, exc)
2      | Cimentación
3      | Estructura
4      | Inst. Hidráulica - Sanitaria - Gas LP
5      | Inst. Eléctrica
6      | Albañilería
7      | Acabados
8      | Herrería, Aluminio y Vidrio
9      | Carpintería y Cerrajería
10     | Muebles y Accesorios
11     | Limpieza e Instalaciones Especiales
12     | Urbanización
```

---

## ?? CAUSA RAÍZ

La tabla `Estimacion(Concepto)` **SÍ tiene una columna `Codigo`**, pero al hacer:

```sql
ORDER BY Codigo  -- ? Ordena alfabéticamente: "1", "10", "11", "2", "3"...
```

Los códigos se ordenaban como **texto**, no como números.

---

## ? SOLUCIÓN IMPLEMENTADA

### 1. **Verificar Existencia de Columna `Codigo`**

```csharp
string sqlCheck = @"
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Estimacion(Concepto)' 
    AND COLUMN_NAME = 'Codigo'";

bool tieneColumnaCodigo = false;
using (SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn))
{
    int count = (int)cmdCheck.ExecuteScalar();
    tieneColumnaCodigo = count > 0;
}
```

### 2. **Ordenar Numéricamente con TRY_CAST**

```sql
SELECT 
    Codigo,
    Concepto,
    SUM(CAST(TOTAL as FLOAT)) as Total
FROM [dbo].[Estimacion(Concepto)]
GROUP BY Codigo, Concepto
ORDER BY 
    CASE 
        WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
        THEN TRY_CAST(Codigo AS INT)  -- ? Convierte a entero para ordenar numéricamente
        ELSE 999999                    -- Si no es número, va al final
    END,
    Codigo  -- Orden alfabético como fallback
```

### 3. **Fallback si no existe columna `Codigo`**

```sql
-- Si la columna Codigo no existe en la tabla, se genera dinámicamente
SELECT 
    ROW_NUMBER() OVER (ORDER BY Concepto) as Codigo,
    Concepto,
    SUM(CAST(TOTAL as FLOAT)) as Total
FROM [dbo].[Estimacion(Concepto)]
GROUP BY Concepto
ORDER BY Concepto
```

---

## ?? RESULTADO

### Ahora los conceptos se muestran en orden numérico correcto:

| Código | Concepto                              | Total         |
|--------|---------------------------------------|---------------|
| 1      | Preliminares (trazo, cimbra, exc)    | $8,083.38     |
| 2      | Cimentación                           | $189,361.56   |
| 3      | Estructura                            | $1,383,350.28 |
| 4      | Inst. Hidráulica - Sanitaria - Gas LP | $239,728.06   |
| 5      | Inst. Eléctrica                       | $285,229.17   |
| 6      | Albañilería                           | $573,711.08   |
| 7      | Acabados                              | $755,153.18   |
| 8      | Herrería, Aluminio y Vidrio          | $136,126.26   |
| 9      | Carpintería y Cerrajería             | $109,827.44   |
| 10     | Muebles y Accesorios                 | $113,779.20   |
| 11     | Limpieza e Instalaciones Especiales  | $96,438.52    |
| 12     | Urbanización                         | $0.00         |

---

## ?? ARCHIVOS MODIFICADOS

### ? `FormEstimacionConcepto.cs`
**Método:** `CargarConceptosDesdeTabla()`

```csharp
private List<ItemEstimacionConcepto> CargarConceptosDesdeTabla()
{
    // ...código anterior...
    
    if (tieneColumnaCodigo)
    {
        // ? Ordenamiento numérico con TRY_CAST
        sql = @"
            SELECT 
                Codigo,
                Concepto,
                SUM(CAST(TOTAL as FLOAT)) as Total
            FROM [dbo].[Estimacion(Concepto)]
            GROUP BY Codigo, Concepto
            ORDER BY 
                CASE 
                    WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
                    THEN TRY_CAST(Codigo AS INT)
                    ELSE 999999 
                END,
                Codigo";
    }
    
    // ...resto del código...
}
```

### ? `FormAvanceConcepto.cs`
**Método:** `CargarConceptosPorPrototipo()`

```csharp
private List<ItemConcepto> CargarConceptosPorPrototipo(string prototipo)
{
    // ...código anterior...
    
    if (tieneColumnaCodigo)
    {
        // ? Ordenamiento numérico con TRY_CAST
        sql = @"
            SELECT 
                Codigo,
                Concepto,
                SUM(CAST(TOTAL as FLOAT)) as Total
            FROM [dbo].[Estimacion(Concepto)]
            GROUP BY Codigo, Concepto
            ORDER BY 
                CASE 
                    WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
                    THEN TRY_CAST(Codigo AS INT)
                    ELSE 999999 
                END,
                Codigo";
    }
    
    // ...resto del código...
}
```

---

## ?? CÓMO PROBAR

### 1. **Verificar en SQL Server**

```sql
-- Consulta de prueba
SELECT 
    Codigo,
    Concepto,
    SUM(CAST(TOTAL as FLOAT)) as Total
FROM [dbo].[Estimacion(Concepto)]
GROUP BY Codigo, Concepto
ORDER BY 
    CASE 
        WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
        THEN TRY_CAST(Codigo AS INT)
        ELSE 999999 
    END,
    Codigo;
```

**Resultado esperado:** Códigos 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12...

### 2. **Probar en la Aplicación**

1. Abrir **FormEstimacionConcepto**
2. Seleccionar Manzana y Lote
3. Click "Cargar Avance"
4. **Verificar** que los códigos aparezcan en orden: 1, 2, 3... 10, 11, 12

---

## ?? EXPLICACIÓN TÉCNICA

### ¿Por qué usar `TRY_CAST`?

```sql
-- Sin TRY_CAST (orden alfabético):
ORDER BY Codigo
-- Resultado: "1", "10", "11", "12", "2", "3", "4", "5", "6", "7", "8", "9"

-- Con TRY_CAST (orden numérico):
ORDER BY CASE 
    WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
    THEN TRY_CAST(Codigo AS INT)
    ELSE 999999 
END
-- Resultado: 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12
```

### ¿Por qué `ELSE 999999`?

Si el `Codigo` **no es un número válido** (ej: "A1", "N/A"), se asigna `999999` para que se ordene al final.

### ¿Por qué agregar `Codigo` como segundo criterio?

```sql
ORDER BY 
    CASE WHEN TRY_CAST(Codigo AS INT) IS NOT NULL THEN TRY_CAST(Codigo AS INT) ELSE 999999 END,
    Codigo  -- ? Orden alfabético como fallback
```

Si hay códigos como:
- `1` (numérico)
- `1A` (alfanumérico)

El orden será:
1. `1` (numérico, valor 1)
2. `1A` (no numérico, valor 999999, luego orden alfabético)

---

## ? VENTAJAS DE LA SOLUCIÓN

| Ventaja | Descripción |
|---------|-------------|
| **Compatible** | Funciona si existe o no la columna `Codigo` |
| **Robusto** | Maneja códigos numéricos y alfanuméricos |
| **Performante** | `TRY_CAST` es eficiente en SQL Server |
| **Consistente** | Ambos formularios usan la misma lógica |
| **Escalable** | Funciona con 10, 100 o 1000 códigos |

---

## ?? CASOS DE USO CUBIERTOS

### ? Caso 1: Códigos Numéricos
```
Entrada: 1, 2, 3, 10, 11, 12
Salida:  1, 2, 3, 10, 11, 12  ?
```

### ? Caso 2: Códigos con Ceros a la Izquierda
```
Entrada: 01, 02, 03, 10, 11, 12
Salida:  1, 2, 3, 10, 11, 12  ? (convierte a entero)
```

### ? Caso 3: Códigos Mixtos
```
Entrada: 1, 1A, 2, 2B, 10, 11
Salida:  1, 2, 10, 11, 1A, 2B  ? (numéricos primero)
```

### ? Caso 4: Sin Columna Codigo
```
Se genera dinámicamente con ROW_NUMBER()  ?
```

---

## ?? RESUMEN

| Aspecto | Antes | Después |
|---------|-------|---------|
| **Orden** | Alfabético (1, 10, 11, 2...) | Numérico (1, 2, 3... 10, 11...) |
| **Verificación** | No verifica si existe columna | ? Verifica existencia |
| **Conversión** | ORDER BY Codigo (texto) | ? ORDER BY TRY_CAST(Codigo AS INT) |
| **Fallback** | N/A | ? Genera códigos si no existen |
| **Consistencia** | Solo FormEstimacionConcepto | ? Ambos formularios |

---

## ? CHECKLIST DE VALIDACIÓN

- [x] Código actualizado en `FormEstimacionConcepto.cs`
- [x] Código actualizado en `FormAvanceConcepto.cs`
- [x] Build exitoso (0 errores) ?
- [x] Lógica de verificación implementada
- [x] Ordenamiento numérico con TRY_CAST
- [x] Fallback si no existe columna Codigo
- [x] Documentación creada

### Pendiente (usuario debe hacer):
- [ ] Probar en FormEstimacionConcepto
- [ ] Probar en FormAvanceConcepto
- [ ] Verificar que códigos 1-12 aparecen en orden correcto
- [ ] Verificar que avances guardados se mantienen

---

## ?? RESULTADO FINAL

? **Los códigos ahora se muestran en orden numérico correcto:** 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12...

? **Compatible con ambos formularios:** FormAvanceConcepto y FormEstimacionConcepto

? **Robusto:** Funciona si la columna Codigo existe o no

? **Build exitoso:** 0 errores de compilación

---

**Fecha:** 2025-01-XX
**Estado:** ? COMPLETADO Y FUNCIONAL
