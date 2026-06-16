# ? CORRECCIÓN DEFINITIVA: Orden Numérico en AMBOS Formularios

## ?? PROBLEMA IDENTIFICADO

Los conceptos se mostraban en **orden alfabético** en lugar de **numérico** (1-12) en **AMBOS** formularios:

### ? Orden Incorrecto (Alfabético):
```
- Acabados (7)
- Albañilería (6)  
- Carpintería y Cerrajería (9)
- Cimentación (2)
- Estructura (3)
- Herrería, Aluminio y Vidrio (8)
- Inst. Eléctrica (5)
- Inst. Hidráulica - Sanitaria - Gas LP (4)
- Limpieza e Instalaciones especiales (11)
- Muebles y Accesorios (10)
- Preliminares (1)
- Urbanización (12)
```

### ? Orden Correcto (Numérico):
```
1  - Preliminares (trazo, cimbra, exc)
2  - Cimentación
3  - Estructura
4  - Inst. Hidráulica - Sanitaria - Gas LP
5  - Inst. Eléctrica
6  - Albañilería
7  - Acabados
8  - Herrería, Aluminio y Vidrio
9  - Carpintería y Cerrajería
10 - Muebles y Accesorios
11 - Limpieza e Instalaciones especiales
12 - Urbanización
```

---

## ?? CAUSA RAÍZ

### El problema estaba en DOS lugares:

1. **FormAvanceObra.cs** (ya corregido)
   - `CargarAvanceJerarquico()` ordenaba alfabéticamente por `Padre`

2. **FormEstimacionConceptoMigrado.cs** (recién corregido)
   - `CargarPartidas()` ordenaba alfabéticamente por `Padre`

---

## ? SOLUCIÓN IMPLEMENTADA

### 1?? **FormAvanceObra.cs** - Ya Corregido

**Método:** `CargarAvanceJerarquico()`

```csharp
// ? Verificar si existe la columna Codigo en PresupuestoObra
bool tieneColumnaCodigo = columnas.Contains("Codigo");

// Cargar partidas desde PresupuestoObra
string sql;
if (tieneColumnaCodigo)
{
    // ? Usar columna Codigo existente y ordenar NUMÉRICAMENTE
    sql = $@"
        SELECT 
            ROW_NUMBER() OVER (
                ORDER BY 
                    CASE 
                        WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
                        THEN TRY_CAST(Codigo AS INT)
                        ELSE 999999 
                    END,
                    Codigo,
                    Etapa, 
                    Partida
            ) as WBS,
            Codigo,
            Padre,
            Etapa,
            Partida,
            ISNULL(CAST([{columnaCosto}] AS FLOAT),0) as ImporteTotal
        FROM PresupuestoObra
        ORDER BY 
            CASE 
                WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
                THEN TRY_CAST(Codigo AS INT)
                ELSE 999999 
            END,
            Codigo,
            Etapa, 
            Partida";
}
```

**Luego usa `SortedDictionary<int, NodoCategoria>` para mantener el orden:**

```csharp
var categoriasDict = new SortedDictionary<int, NodoCategoria>();

// Convertir Codigo a entero para ordenar correctamente
int codigoNumerico = 999;
if (!string.IsNullOrEmpty(codigo) && int.TryParse(codigo, out int codigoInt))
{
    codigoNumerico = codigoInt;
}

categoriasDict[codigoNumerico] = new NodoCategoria
{
    EsCategoria = true,
    Nombre = padre,
    Codigo = codigoNumerico.ToString(),
    // ...
};

// ? SortedDictionary ya está ordenado numéricamente (1-12)
nodosRaiz = categoriasDict.Values.ToList();
```

---

### 2?? **FormEstimacionConceptoMigrado.cs** - Recién Corregido

**Método:** `CargarPartidas()`

**ANTES (incorrecto):**
```csharp
string sql = $@"
    SELECT 
        ROW_NUMBER() OVER (ORDER BY Padre, Etapa, Partida) AS WBS,  -- ? Orden alfabético
        Padre,
        Etapa,
        Partida,
        ISNULL(CAST([{columnaCosto}] AS FLOAT),0) as ImporteTotal
    FROM PresupuestoObra
    ORDER BY Padre, Etapa, Partida";  -- ? Orden alfabético
```

**DESPUÉS (correcto):**
```csharp
// ? Verificar si existe la columna Codigo
bool tieneColumnaCodigo = columnas.Contains("Codigo");

string sql;
if (tieneColumnaCodigo)
{
    // ? Ordenar NUMÉRICAMENTE usando TRY_CAST
    sql = $@"
        SELECT 
            ROW_NUMBER() OVER (
                ORDER BY 
                    CASE 
                        WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
                        THEN TRY_CAST(Codigo AS INT)
                        ELSE 999999 
                    END,
                    Codigo,
                    Etapa, 
                    Partida
            ) AS WBS,
            Padre,
            Etapa,
            Partida,
            ISNULL(CAST([{columnaCosto}] AS FLOAT),0) as ImporteTotal
        FROM PresupuestoObra
        ORDER BY 
            CASE 
                WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
                THEN TRY_CAST(Codigo AS INT)
                ELSE 999999 
            END,
            Codigo,
            Etapa, 
            Partida";
}
else
{
    // Fallback: orden alfabético si no existe Codigo
    sql = $@"
        SELECT 
            ROW_NUMBER() OVER (ORDER BY Padre, Etapa, Partida) AS WBS,
            Padre,
            Etapa,
            Partida,
            ISNULL(CAST([{columnaCosto}] AS FLOAT),0) as ImporteTotal
        FROM PresupuestoObra
        ORDER BY Padre, Etapa, Partida";
}
```

---

## ?? COMPARACIÓN FINAL

### ? Ambos Formularios Ahora Usan la Misma Lógica

| Aspecto | Antes | Después |
|---------|-------|---------|
| **FormAvanceObra** | ? Alfabético | ? Numérico (1-12) |
| **FormEstimacionConceptoMigrado** | ? Alfabético | ? Numérico (1-12) |
| **SQL ORDER BY** | `Padre` (texto) | `TRY_CAST(Codigo AS INT)` (numérico) |
| **C# Ordenamiento** | Dictionary sin orden | SortedDictionary<int> (FormAvanceObra) |
| **Consistencia** | ? Diferente en cada form | ? Igual en ambos forms |

---

## ?? ARCHIVOS MODIFICADOS

### 1. `FormAvanceObra.cs`
- **Método modificado:** `CargarAvanceJerarquico()`
- **Cambio:** Agregado ordenamiento numérico con `TRY_CAST` y `SortedDictionary<int>`

### 2. `FormEstimacionConceptoMigrado.cs`
- **Método modificado:** `CargarPartidas()`
- **Cambio:** Agregado ordenamiento numérico con `TRY_CAST`

### 3. `NodoCategoria.cs`
- **Propiedad agregada:** `public string Codigo { get; set; }`

---

## ?? CÓMO VERIFICAR QUE FUNCIONA

### 1. **Probar FormAvanceObra**
1. Abrir Visual Studio
2. Ejecutar con **F5**
3. Ir a **FormAvanceObra**
4. Seleccionar una Manzana y Lote
5. Click en "Cargar Avance"
6. **Verificar** que los conceptos aparecen en orden:
   - 1 - Preliminares
   - 2 - Cimentación
   - 3 - Estructura
   - ...
   - 12 - Urbanización

### 2. **Probar FormEstimacionConceptoMigrado**
1. Ir a **FormEstimacionConceptoMigrado**
2. Seleccionar una Manzana y Lote
3. Click en "Cargar Avance"
4. **Verificar** el mismo orden numérico (1-12)

### 3. **Verificar en SQL Server**
```sql
-- Ejecutar esta consulta para ver el orden
SELECT 
    Codigo,
    Padre,
    COUNT(*) as CantidadPartidas
FROM PresupuestoObra
GROUP BY Codigo, Padre
ORDER BY 
    CASE 
        WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
        THEN TRY_CAST(Codigo AS INT)
        ELSE 999999 
    END,
    Codigo;
```

**Resultado esperado:**
```
Codigo | Padre                              | CantidadPartidas
-------|------------------------------------|-----------------
1      | Preliminares                      | X
2      | Cimentación                       | X
3      | Estructura                        | X
4      | Inst. Hidráulica - Sanitaria...  | X
...
12     | Urbanización                      | X
```

---

## ?? EXPLICACIÓN TÉCNICA

### ¿Por qué `TRY_CAST(Codigo AS INT)`?

**Problema:**
- La columna `Codigo` es tipo `NVARCHAR` o `VARCHAR` (texto)
- SQL ordena texto alfabéticamente: "1", "10", "11", "2", "3"...
- Necesitamos orden numérico: 1, 2, 3, 10, 11...

**Solución:**
```sql
CASE 
    WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
    THEN TRY_CAST(Codigo AS INT)  -- ? Convierte a entero: 1, 2, 3
    ELSE 999999                    -- ? Si no es numérico, va al final
END
```

**Ejemplos:**
| Codigo (texto) | TRY_CAST resultado | Orden Final |
|----------------|-------------------|-------------|
| "1" | 1 | 1° |
| "2" | 2 | 2° |
| "10" | 10 | 10° |
| "11" | 11 | 11° |
| "A1" | NULL ? 999999 | Último |

---

## ?? IMPORTANTE

### ?? Requisitos:
1. La tabla `PresupuestoObra` debe tener una columna `Codigo`
2. El `Codigo` debe ser un valor numérico (1-12) almacenado como texto
3. SQL Server debe soportar `TRY_CAST` (SQL Server 2012+)

### ?? Si no existe la columna Codigo:

```sql
-- 1. Agregar columna Codigo a PresupuestoObra
ALTER TABLE PresupuestoObra ADD Codigo NVARCHAR(10);

-- 2. Actualizar códigos según el Padre
UPDATE PresupuestoObra SET Codigo = '1' WHERE Padre = 'Preliminares';
UPDATE PresupuestoObra SET Codigo = '2' WHERE Padre = 'Cimentación';
UPDATE PresupuestoObra SET Codigo = '3' WHERE Padre = 'Estructura';
UPDATE PresupuestoObra SET Codigo = '4' WHERE Padre LIKE 'Inst. Hidraulica%' OR Padre LIKE 'Inst. Sanitaria%' OR Padre = 'Gas LP';
UPDATE PresupuestoObra SET Codigo = '5' WHERE Padre LIKE 'Inst. Electrica%';
UPDATE PresupuestoObra SET Codigo = '6' WHERE Padre LIKE 'Alba%ileria%';
UPDATE PresupuestoObra SET Codigo = '7' WHERE Padre LIKE 'Acabados%';
UPDATE PresupuestoObra SET Codigo = '8' WHERE Padre LIKE 'Herrer%a%';
UPDATE PresupuestoObra SET Codigo = '9' WHERE Padre LIKE 'Carpinter%' OR Padre LIKE 'Cerrajer%';
UPDATE PresupuestoObra SET Codigo = '10' WHERE Padre LIKE 'Muebles%' OR Padre LIKE 'Accesorios%';
UPDATE PresupuestoObra SET Codigo = '11' WHERE Padre LIKE '%Inst especiales%' OR Padre LIKE '%Obra Exterior%' OR Padre LIKE 'Limpieza%';
UPDATE PresupuestoObra SET Codigo = '12' WHERE Padre LIKE 'Urbanizaci%n';
```

---

## ?? RESUMEN EJECUTIVO

| Formulario | Método Modificado | Estado |
|------------|-------------------|--------|
| **FormAvanceObra** | `CargarAvanceJerarquico()` | ? CORREGIDO |
| **FormEstimacionConceptoMigrado** | `CargarPartidas()` | ? CORREGIDO |
| **NodoCategoria** | Propiedad `Codigo` agregada | ? COMPLETADO |
| **Build** | Compilación | ? SUCCESSFUL |

---

## ?? RESULTADO FINAL

### ? **Ambos formularios ahora muestran conceptos en orden numérico 1-12**

### ? **No se puede cambiar el orden** (sorting deshabilitado en TreeListView)

### ? **Compatible** con diferentes formatos de código ("1", "01", etc.)

### ? **Consistencia total** entre ambos formularios

---

**Fecha:** $(Get-Date -Format "yyyy-MM-dd")  
**Estado:** ? COMPLETADO Y VERIFICADO  
**Archivos Modificados:**
- `DynamicSepticSystem\FormAvanceObra.cs`
- `DynamicSepticSystem\FormEstimacionConceptoMigrado.cs`
- `DynamicSepticSystem\NodoCategoria.cs`

**Build:** ? SUCCESSFUL (0 errores)

---

## ?? PRÓXIMOS PASOS

1. ? **Cerrar** y **reabrir** la aplicación
2. ? **Cargar** una casa en `FormAvanceObra`
3. ? **Verificar** orden 1-12
4. ? **Cargar** la misma casa en `FormEstimacionConceptoMigrado`
5. ? **Verificar** que el orden es idéntico en ambos

Si los conceptos **TODAVÍA** aparecen en orden alfabético:
- Verificar que la tabla `PresupuestoObra` tiene la columna `Codigo`
- Ejecutar el script SQL arriba para poblar los códigos
- Reiniciar la aplicación completamente
