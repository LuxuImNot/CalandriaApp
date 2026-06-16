# ?? Datos que Espera FormAvanceObra - Guía Rápida

## ? RESUMEN EJECUTIVO

El **FormAvanceObra** necesita **3 tablas principales**:

| Tabla | Estado | Propósito |
|-------|--------|-----------|
| `InventarioCasas` | ? **OBLIGATORIA** | Lista de casas disponibles (Manzana/Lote) |
| `PresupuestoObra` | ?? **OPCIONAL** | Presupuesto por categoría (usa valores por defecto si no existe) |
| `OrdenesCompra` + `OrdenesCompraDetalle` | ?? **OPCIONAL** | Gastos ejecutados (usa valores simulados si no existe) |

---

## ?? Estructura de Datos

### 1. **InventarioCasas** (OBLIGATORIA)

```sql
-- Mínimo requerido:
Manzana NVARCHAR(50)  -- Ej: "M1", "M2", "M3"
Lote NVARCHAR(50)     -- Ej: "L1", "L2", "L3"
```

**Ejemplo:**
```sql
SELECT * FROM InventarioCasas;

Manzana | Lote | Prototipo
--------|------|------------
M1      | L1   | PLANTA BAJA
M1      | L2   | PLANTA BAJA
M1      | L3   | PLANTA ALTA
M2      | L1   | PLANTA BAJA
M2      | L2   | PLANTA BAJA
```

---

### 2. **PresupuestoObra** (Opcional - usa valores por defecto si no existe)

```sql
Categoria NVARCHAR(100)     -- Debe coincidir con las 17 categorías del sistema
CostoTotal DECIMAL(18,2)    -- Monto del presupuesto
```

**Categorías Esperadas (17 en total):**
1. Preliminares
2. Cimentación
3. Muros Planta Baja
4. Muros Planta Alta
5. Losas
6. Azotea
7. Inst. Hidráulica, Sanitaria y Gas LP
8. Inst. Eléctrica
9. Albañilería Planta Baja
10. Albañilería Planta Alta
11. Acabados Interiores
12. Acabados Exteriores
13. Herrería, Aluminio y Vidrio
14. Carpintería y Cancelería
15. Muebles y Accesorios
16. Obra Exterior
17. Urbanización

**Ejemplo:**
```sql
SELECT Categoria, SUM(CostoTotal) AS Total
FROM PresupuestoObra
GROUP BY Categoria;

Categoria                              | Total
---------------------------------------|-------------
Preliminares                           | $8,223.62
Cimentación                            | $55,324.17
Muros Planta Baja                      | $61,540.18
Losas                                  | $100,484.52
Inst. Eléctrica                        | $33,915.84
Acabados Interiores                    | $206,013.17
...
```

---

### 3. **OrdenesCompra** (Para calcular ejecutado)

```sql
-- Campos requeridos:
FolioOC NVARCHAR(50)          -- Identificador único
Manzana NVARCHAR(50)          -- ? IMPORTANTE: Filtrar por casa
Lote NVARCHAR(50)             -- ? IMPORTANTE: Filtrar por casa
Fecha DATETIME
```

**Ejemplo:**
```sql
SELECT * FROM OrdenesCompra WHERE Manzana = 'M1' AND Lote = 'L1';

FolioOC     | Fecha      | Manzana | Lote | ProveedorClave
------------|------------|---------|------|----------------
OC-001      | 2025-01-15 | M1      | L1   | PROV-001
OC-002      | 2025-01-20 | M1      | L1   | PROV-002
OC-003      | 2025-01-25 | M1      | L1   | PROV-001
```

---

### 4. **OrdenesCompraDetalle** (Desglose de gastos)

```sql
-- Campos requeridos:
FolioOC NVARCHAR(50)          -- Relación con OrdenesCompra
ImporteTotal DECIMAL(18,2)    -- Monto gastado
Categoria NVARCHAR(100)       -- ? IMPORTANTE: Debe coincidir con categorías
```

**Ejemplo:**
```sql
SELECT * FROM OrdenesCompraDetalle WHERE FolioOC = 'OC-001';

FolioOC | Descripcion        | Cantidad | PrecioUnit | ImporteTotal | Categoria
--------|--------------------| ---------|------------|--------------|-------------------
OC-001  | Block 15x20x40     | 2500     | $12.50     | $31,250.00   | Muros Planta Baja
OC-001  | Cemento gris       | 150      | $185.50    | $27,825.00   | Muros Planta Baja
OC-001  | Arena cernida      | 25       | $450.00    | $11,250.00   | Cimentación
```

---

## ?? Cómo Funciona el Cálculo

### Fórmula del % de Avance:

```
% Avance = (Total Ejecutado / Total Presupuestado) × 100
```

### Ejemplo de Cálculo:

**Para la categoría "Muros Planta Baja" en M1-L1:**

1. **Presupuestado** (de tabla `PresupuestoObra`):
   ```sql
   SELECT SUM(CostoTotal) FROM PresupuestoObra 
   WHERE Categoria = 'Muros Planta Baja'
   ? $61,540.18
   ```

2. **Ejecutado** (de órdenes de compra):
   ```sql
   SELECT SUM(d.ImporteTotal)
   FROM OrdenesCompra o
   JOIN OrdenesCompraDetalle d ON o.FolioOC = d.FolioOC
   WHERE o.Manzana = 'M1' AND o.Lote = 'L1'
     AND d.Categoria = 'Muros Planta Baja'
   ? $59,075.00
   ```

3. **% Avance**:
   ```
   (59,075.00 / 61,540.18) × 100 = 96.0%
   ```

4. **Estado**: ? Completado (?80%)

---

## ?? Inicialización Rápida

### Opción 1: Ejecutar Script SQL

```sql
-- Ejecuta este archivo:
SQL_InicializarDatosAvanceObra.sql
```

Este script:
- ? Crea tabla `PresupuestoObra`
- ? Agrega columnas `Manzana`, `Lote` y `Categoria`
- ? Inserta presupuesto completo ($776,772.85)
- ? Crea 3 casas de prueba con diferentes avances

### Opción 2: Datos Mínimos Manuales

```sql
-- 1. Asegurar que tienes casas en inventario
SELECT * FROM InventarioCasas;

-- 2. (Opcional) Insertar presupuesto básico
-- El sistema usa valores por defecto si no existe

-- 3. (Opcional) Crear órdenes de compra con Manzana/Lote
UPDATE OrdenesCompra SET Manzana = 'M1', Lote = 'L1' WHERE FolioOC = 'OC-XXX';

-- 4. (Opcional) Agregar categoría a detalles
UPDATE OrdenesCompraDetalle 
SET Categoria = 'Muros Planta Baja' 
WHERE Clave LIKE 'BLOCK%';
```

---

## ?? Problemas Comunes

### Problema 1: No se cargan manzanas/lotes

**Causa**: Tabla `InventarioCasas` vacía  
**Solución**:
```sql
SELECT COUNT(*) FROM InventarioCasas;
-- Si es 0, inserta al menos una casa de prueba
INSERT INTO InventarioCasas (Manzana, Lote, Prototipo)
VALUES ('M1', 'L1', 'PLANTA BAJA');
```

---

### Problema 2: Avance muestra 0% en todo

**Causa**: No hay órdenes de compra con `Manzana` y `Lote`  
**Solución**:
```sql
-- Verificar si existen órdenes para esa casa
SELECT * FROM OrdenesCompra 
WHERE Manzana = 'M1' AND Lote = 'L1';

-- Si no hay, el sistema usará valores simulados (30-70%)
-- Para datos reales, asegúrate de llenar Manzana/Lote en órdenes existentes
```

---

### Problema 3: Categorías no coinciden

**Causa**: Campo `Categoria` en `OrdenesCompraDetalle` tiene valores diferentes  
**Solución**:
```sql
-- Ver categorías actuales
SELECT DISTINCT Categoria FROM OrdenesCompraDetalle;

-- Actualizar a categorías correctas
UPDATE OrdenesCompraDetalle 
SET Categoria = 'Muros Planta Baja'
WHERE Clave LIKE 'BLOCK%' OR Descripcion LIKE '%block%';

UPDATE OrdenesCompraDetalle 
SET Categoria = 'Cimentación'
WHERE Clave LIKE 'ZAP%' OR Descripcion LIKE '%zapata%';
```

---

## ?? Verificación Pre-Uso

Antes de usar el formulario, ejecuta estas consultas:

```sql
-- ? Verificar casas disponibles
SELECT COUNT(*) AS TotalCasas FROM InventarioCasas;
-- Debe ser > 0

-- ? Verificar presupuesto (opcional)
SELECT COUNT(*) AS Items, SUM(CostoTotal) AS Total 
FROM PresupuestoObra;
-- Si es 0, se usarán valores por defecto

-- ? Verificar órdenes con Manzana/Lote
SELECT COUNT(*) AS Ordenes 
FROM OrdenesCompra 
WHERE Manzana IS NOT NULL AND Lote IS NOT NULL;
-- Si es 0, el avance será simulado

-- ? Verificar categorías en detalles
SELECT DISTINCT Categoria 
FROM OrdenesCompraDetalle 
WHERE Categoria IS NOT NULL;
-- Debe mostrar algunas de las 17 categorías
```

---

## ?? Flujo de Datos Completo

```
???????????????????????????????????????
? 1. Usuario selecciona Manzana/Lote ?
???????????????????????????????????????
                ?
                ?
???????????????????????????????????????
? 2. Cargar Presupuestado             ?
?    (de PresupuestoObra)             ?
?    ?                                ?
?    Si no existe ? Usar valores      ?
?    por defecto del documento        ?
???????????????????????????????????????
                ?
                ?
???????????????????????????????????????
? 3. Cargar Ejecutado                 ?
?    (de OrdenesCompra+Detalle)       ?
?    ?                                ?
?    Filtrar por Manzana y Lote       ?
?    Agrupar por Categoria            ?
?    ?                                ?
?    Si no hay datos ? Simular        ?
?    valores (30-70% del presup.)     ?
???????????????????????????????????????
                ?
                ?
???????????????????????????????????????
? 4. Calcular % Avance                ?
?    (Ejecutado / Presupuestado × 100)?
???????????????????????????????????????
                ?
                ?
???????????????????????????????????????
? 5. Mostrar en interfaz:             ?
?    • Tabla detallada                ?
?    • Resumen general                ?
?    • Gráfica de barras              ?
???????????????????????????????????????
```

---

## ?? Recomendaciones

### Para Datos Reales:

1. **Ejecutar script de inicialización primero**
   ```bash
   SQL_InicializarDatosAvanceObra.sql
   ```

2. **Agregar columnas si usas BD existente**
   - `Manzana` y `Lote` en `OrdenesCompra`
   - `Categoria` en `OrdenesCompraDetalle`

3. **Actualizar órdenes existentes**
   ```sql
   -- Asociar órdenes a casas específicas
   UPDATE OrdenesCompra SET Manzana = 'M1', Lote = 'L1'
   WHERE FolioOC IN ('OC-001', 'OC-002', 'OC-003');
   ```

4. **Clasificar gastos por categoría**
   ```sql
   -- Actualizar categorías en base a descripción
   UPDATE OrdenesCompraDetalle 
   SET Categoria = CASE
       WHEN Descripcion LIKE '%block%' THEN 'Muros Planta Baja'
       WHEN Descripcion LIKE '%zapata%' THEN 'Cimentación'
       WHEN Descripcion LIKE '%losa%' THEN 'Losas'
       -- ... más casos
   END
   WHERE Categoria IS NULL;
   ```

---

## ?? ¿Necesitas Ayuda?

Si el formulario no muestra datos:

1. ? Verifica que `InventarioCasas` tenga registros
2. ? Ejecuta el script `SQL_InicializarDatosAvanceObra.sql`
3. ? Revisa que las órdenes tengan `Manzana` y `Lote`
4. ? Verifica que los detalles tengan `Categoria`

**El sistema funcionará con valores simulados si no hay datos reales**, pero para reportes precisos necesitas las tablas configuradas correctamente.

---

**Última Actualización**: Enero 2025  
**Script de Inicialización**: `SQL_InicializarDatosAvanceObra.sql`  
**Documentación Completa**: `README_AvanceObra.md`
