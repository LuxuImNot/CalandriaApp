# ? CORRECCIÓN: Orden Forzado de Conceptos 1-12

## ?? Objetivo
Forzar el orden correcto de los conceptos en los TreeListView de **FormEstimacionConceptoMigrado** y **FormAvanceObra** para que siempre aparezcan en el orden estándar del 1 al 12, independientemente del orden alfabético.

---

## ?? Orden Estándar de Conceptos

| Código | Concepto |
|--------|----------|
| 1 | Preliminares |
| 2 | Cimentación |
| 3 | Estructura |
| 4 | Inst. Hidráulica, Sanitaria y Gas LP |
| 5 | Inst. Eléctrica |
| 6 | Albañilería |
| 7 | Acabados |
| 8 | Herrería, Aluminio y Vidrio |
| 9 | Carpintería y Cerrajería |
| 10 | Muebles y Accesorios |
| 11 | Inst especiales y Obra Exterior |
| 12 | Urbanización |

---

## ?? Cambios Implementados

### 1?? FormEstimacionConceptoMigrado.cs

#### ? Método `CargarConceptos()` Mejorado
```csharp
// Ahora incluye ORDER BY con prioridad numérica
ORDER BY 
    CASE 
        WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
        THEN TRY_CAST(Codigo AS INT)
        ELSE 999999 
    END,
    Codigo
```

**Características:**
- Si la columna `Codigo` existe en la tabla, usa el valor numérico para ordenar
- Si no existe, genera códigos automáticamente según el orden estándar (1-12)
- Maneja variaciones de acentuación (Cimentación/Cimentacion, Eléctrica/Electrica)

#### ? Método `CargarEstimacionJerarquica()` Mejorado
```csharp
// Diccionario con orden forzado 1-12
var ordenConceptos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
{
    { "Preliminares", 1 },
    { "Cimentación", 2 },
    { "Cimentacion", 2 },
    { "Estructura", 3 },
    // ... hasta 12
};

// Ordenar por código numérico en lugar de alfabéticamente
nodosRaiz = conceptosTemporal.Values
    .OrderBy(c => {
        if (ordenConceptos.TryGetValue(c.Nombre, out int orden))
            return orden;
        return 999; // Los no encontrados van al final
    })
    .ThenBy(c => c.Nombre)
    .ToList();
```

**Ventajas:**
? Orden SIEMPRE correcto (1-12)  
? Ignora mayúsculas/minúsculas  
? Soporta variaciones de acentos  
? Conceptos desconocidos van al final  

---

### 2?? FormAvanceObra.cs

#### ? Método `CargarAvanceJerarquico()` Mejorado
```csharp
// FORZAR ORDEN CORRECTO 1-12
var ordenCategorias = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
{
    { "Preliminares", 1 },
    { "Cimentación", 2 },
    { "Cimentacion", 2 },
    { "Estructura", 3 },
    { "Losas", 3 }, // Sub-categoría de Estructura
    { "Muros", 3 }, // Sub-categoría de Estructura
    // ... incluye TODAS las variaciones posibles
};

// Ordenar NUMÉRICAMENTE en lugar de alfabéticamente
nodosRaiz = categoriasDict.Values
    .OrderBy(c => {
        if (ordenCategorias.TryGetValue(c.Nombre, out int orden))
            return orden;
        return 999;
    })
    .ThenBy(c => c.Nombre)
    .ToList();
```

**Características:**
? Maneja sub-categorías (Losas, Muros ? Estructura)  
? Soporta variaciones de nombres (Inst./Instalacion)  
? Orden consistente con FormEstimacionConceptoMigrado  
? Fácil de expandir si se agregan nuevos conceptos  

---

## ?? Casos de Prueba

### ? Antes (Orden Alfabético - INCORRECTO)
```
1. Acabados
2. Albañilería
3. Carpintería y Cerrajería
4. Cimentación
5. Estructura
6. Herrería, Aluminio y Vidrio
7. Inst especiales y Obra Exterior
8. Inst. Eléctrica
9. Inst. Hidráulica, Sanitaria y Gas LP
10. Muebles y Accesorios
11. Preliminares
12. Urbanización
```

### ? Ahora (Orden Numérico - CORRECTO)
```
1. Preliminares
2. Cimentación
3. Estructura
4. Inst. Hidráulica, Sanitaria y Gas LP
5. Inst. Eléctrica
6. Albañilería
7. Acabados
8. Herrería, Aluminio y Vidrio
9. Carpintería y Cerrajería
10. Muebles y Accesorios
11. Inst especiales y Obra Exterior
12. Urbanización
```

---

## ?? Variaciones Soportadas

El sistema ahora reconoce y ordena correctamente estas variaciones:

| Concepto Estándar | Variaciones Soportadas |
|-------------------|------------------------|
| Cimentación | Cimentacion (sin tilde) |
| Inst. Eléctrica | Inst. Electrica, Instalacion Electrica |
| Albañilería | Albanileria, Albañileria |
| Herrería, Aluminio y Vidrio | Herreria, Aluminio y Vidrio |
| Carpintería y Cerrajería | Carpinteria y Cerrajeria |
| Urbanización | Urbanizacion |

---

## ?? Impacto Visual

### Antes
```
???????????????????????????????????
? ? Acabados                     ?
? ? Albañilería                  ?
? ? Carpintería y Cerrajería     ?
? ? Cimentación                  ?
? ? Estructura                   ?
? ...                             ?
???????????????????????????????????
```

### Ahora
```
???????????????????????????????????
? ? 1. Preliminares              ?
? ? 2. Cimentación               ?
? ? 3. Estructura                ?
? ? 4. Inst. Hidráulica...       ?
? ? 5. Inst. Eléctrica           ?
? ...                             ?
???????????????????????????????????
```

---

## ?? Beneficios

? **Consistencia**: Mismo orden en ambos formularios  
? **Lógica de Construcción**: Orden refleja secuencia real de obra  
? **Facilidad de Uso**: Usuarios encuentran conceptos en posición esperada  
? **Mantenibilidad**: Fácil agregar nuevos conceptos al diccionario  
? **Robustez**: Soporta variaciones de acentuación y nombres  

---

## ?? Cómo Agregar Nuevos Conceptos

Si en el futuro necesitas agregar un nuevo concepto entre el 11 y 12:

```csharp
var ordenConceptos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
{
    // ... conceptos existentes ...
    { "Inst especiales y Obra Exterior", 11 },
    { "Nuevo Concepto Aquí", 12 }, // ? Agregar aquí
    { "Urbanización", 13 },         // ? Renumerar
    { "Urbanizacion", 13 }
};
```

---

## ? Estado Actual

- ? FormEstimacionConceptoMigrado: Orden 1-12 FORZADO
- ? FormAvanceObra: Orden 1-12 FORZADO
- ? Build exitoso
- ? Sin errores de compilación
- ? Soporta variaciones de acentuación
- ? Consistencia entre ambos formularios

---

## ?? Notas Técnicas

1. **StringComparer.OrdinalIgnoreCase**: Ignora mayúsculas/minúsculas
2. **ThenBy(c => c.Nombre)**: Orden alfabético como segundo criterio
3. **return 999**: Conceptos desconocidos van al final
4. **NormalizeForComparison()**: Maneja acentos y caracteres especiales

---

**Fecha de Implementación**: 2025-01-XX  
**Archivos Modificados**:
- `FormEstimacionConceptoMigrado.cs`
- `FormAvanceObra.cs`

---

**Resultado**: Los TreeListView ahora muestran SIEMPRE los conceptos en el orden correcto 1-12, independientemente de cómo estén almacenados en la base de datos. ??
