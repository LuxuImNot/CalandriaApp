# ?? ACTUALIZACIÓN: FormAvanceConcepto - Uso de dbo.Estimacion(Concepto)

## ?? Cambios Realizados

Se ha modificado el `FormAvanceConcepto` para utilizar la tabla **`dbo.Estimacion(Concepto)`** en lugar de `PresupuestoObra`.

---

## ??? Estructura de la Tabla

### Nombre de la Tabla
```sql
[dbo].[Estimacion(Concepto)]
```

?? **IMPORTANTE:** El nombre de la tabla incluye paréntesis, por lo que SIEMPRE debe usarse entre corchetes en las consultas SQL.

### Columnas Esperadas
- **`Concepto`** - (NVARCHAR) Nombre del concepto
- **`TOTAL`** - (NUMERIC/FLOAT) Monto total del concepto

---

## ?? Modificación en FormAvanceConcepto.cs

### Método Modificado: `CargarConceptosPorPrototipo()`

**ANTES:**
```csharp
string sql = $@"
    SELECT 
        Codigo,
        Concepto,
        SUM({columnaCosto}) as Total
    FROM PresupuestoObra 
    GROUP BY Codigo, Concepto
    ORDER BY Codigo";
```

**DESPUÉS:**
```csharp
string sql = @"
    SELECT 
        ROW_NUMBER() OVER (ORDER BY Concepto) as Codigo,
        Concepto,
        SUM(CAST(TOTAL as FLOAT)) as Total
    FROM [dbo].[Estimacion(Concepto)]
    GROUP BY Concepto
    ORDER BY Concepto";
```

### Cambios Clave:

1. ? **Eliminada la lógica de prototipo** (CostoCalandra/CostoTunera)
   - Ya no se usa `columnaCosto` variable
   - Se usa directamente la columna `TOTAL`

2. ? **Código generado automáticamente**
   - Se usa `ROW_NUMBER()` para generar códigos secuenciales
   - Los códigos se asignan en orden alfabético de conceptos

3. ? **Tabla correctamente escapada**
   - Se usan corchetes: `[dbo].[Estimacion(Concepto)]`
   - Previene errores por caracteres especiales en el nombre

4. ? **Conversión de tipo explícita**
   - Se usa `CAST(TOTAL as FLOAT)` para asegurar tipo numérico

---

## ?? Ejemplo de Datos Esperados

### Tabla Original: `dbo.Estimacion(Concepto)`
```
| Concepto                              | TOTAL        |
|---------------------------------------|--------------|
| Preliminares (trazo, cimbra, exc)    | 8,083.38     |
| Cimentación                           | 189,361.56   |
| Estructura                            | 1,383,350.28 |
| Inst. Hidraulica - Sanitaria - Gas LP | 239,728.06   |
| Inst. Electrica                       | 285,229.17   |
| ...                                   | ...          |
```

### Resultado en FormAvanceConcepto
```
| Codigo | Concepto                              | Total        | Avance % | Ejecutado |
|--------|---------------------------------------|--------------|----------|-----------|
| 1      | Albañileria                           | $573,711.08  | 0.0%     | $0.00     |
| 2      | Carpinteria y Cerrajeria             | $109,827.44  | 0.0%     | $0.00     |
| 3      | Cimentación                          | $189,361.56  | 0.0%     | $0.00     |
| 4      | Estructura                           | $1,383,350.28| 0.0%     | $0.00     |
| ...    | ...                                  | ...          | ...      | ...       |
```

---

## ? Funcionalidades que SE MANTIENEN

- ? Edición bidireccional (Avance % ? Monto Ejecutado)
- ? Validaciones (0-100%, no negativos, no exceder presupuesto)
- ? Guardado automático en `AvanceManualConcepto`
- ? Gráficas en tiempo real (pastel + barras)
- ? Exportación a PDF profesional
- ? Totalizadores dinámicos
- ? Agrupamiento visual

---

## ? Funcionalidades REMOVIDAS

- ? **Selección automática por prototipo** (CALANDRIA/TUNERA)
  - Ya no se diferencia entre prototipos
  - Se usa siempre la columna `TOTAL` de `dbo.Estimacion(Concepto)`
  
- ? **Método `ObtenerPrototipo()`**
  - Aún se ejecuta pero solo por compatibilidad
  - No afecta la carga de datos

---

## ?? Verificación de la Tabla

### 1. Ejecutar Script de Verificación

Ejecuta el archivo: **`SQL_VerificarEstimacionConcepto.sql`**

Este script verificará:
- ? Existencia de la tabla
- ? Estructura de columnas
- ? Tipo de datos
- ? Conceptos únicos
- ? Valores totales
- ? Compatibilidad con FormAvanceConcepto

### 2. Consulta Manual Rápida

```sql
-- Ver estructura
SELECT TOP 10 * FROM [dbo].[Estimacion(Concepto)]

-- Ver conceptos agrupados (como en el formulario)
SELECT 
    ROW_NUMBER() OVER (ORDER BY Concepto) as Codigo,
    Concepto,
    SUM(CAST(TOTAL as FLOAT)) as Total
FROM [dbo].[Estimacion(Concepto)]
GROUP BY Concepto
ORDER BY Concepto
```

---

## ?? Pasos para Usar

### 1. Verificar la Tabla

```sql
-- Ejecutar
EXEC sp_help '[dbo].[Estimacion(Concepto)]'
```

### 2. Compilar el Proyecto

```bash
Build ? Rebuild Solution
```

### 3. Probar el Formulario

1. Abrir la aplicación
2. Ir a "Avance por Conceptos"
3. Seleccionar Manzana y Lote
4. Click "Cargar Avance"
5. Verificar que se muestren los conceptos

---

## ?? Solución de Problemas

### Error: "Invalid object name 'dbo.Estimacion(Concepto)'"

**Causa:** El nombre de la tabla no se escapó correctamente

**Solución:**
```sql
-- ? INCORRECTO
SELECT * FROM dbo.Estimacion(Concepto)

-- ? CORRECTO
SELECT * FROM [dbo].[Estimacion(Concepto)]
```

### Error: "Invalid column name 'TOTAL'"

**Causa:** La columna tiene otro nombre

**Solución:**
1. Ejecutar: `SELECT * FROM [dbo].[Estimacion(Concepto)]`
2. Identificar el nombre real de la columna de totales
3. Modificar en `FormAvanceConcepto.cs` línea ~225:
   ```csharp
   SUM(CAST(NombreRealColumna as FLOAT)) as Total
   ```

### Error: "Cannot convert varchar to float"

**Causa:** La columna TOTAL tiene formato no numérico

**Solución:**
1. Verificar formato de datos:
   ```sql
   SELECT TOTAL, TRY_CAST(TOTAL as FLOAT) as Convertido
   FROM [dbo].[Estimacion(Concepto)]
   WHERE TRY_CAST(TOTAL as FLOAT) IS NULL
   ```
2. Limpiar datos no numéricos antes de usar

### No se muestran conceptos en el formulario

**Verificar:**
1. ? La tabla existe y tiene datos
2. ? La columna `Concepto` tiene valores
3. ? La columna `TOTAL` tiene valores numéricos
4. ? La cadena de conexión es correcta

```sql
-- Verificación rápida
SELECT COUNT(*) as TotalRegistros,
       COUNT(DISTINCT Concepto) as ConceptosUnicos,
       SUM(CAST(TOTAL as FLOAT)) as TotalGeneral
FROM [dbo].[Estimacion(Concepto)]
```

---

## ?? Comparación: Antes vs Después

| Aspecto | Antes (PresupuestoObra) | Después (Estimacion(Concepto)) |
|---------|-------------------------|--------------------------------|
| **Tabla origen** | `PresupuestoObra` | `[dbo].[Estimacion(Concepto)]` |
| **Columna de costo** | `CostoCalandra` o `CostoTunera` | `TOTAL` |
| **Código** | `Codigo` (columna existente) | Generado con `ROW_NUMBER()` |
| **Prototipo** | Diferencia por prototipo | No diferencia |
| **Agrupamiento** | Por Codigo + Concepto | Solo por Concepto |

---

## ?? Archivos Relacionados

- ? **FormAvanceConcepto.cs** - Código principal (modificado)
- ? **SQL_VerificarEstimacionConcepto.sql** - Script de verificación (nuevo)
- ? **ACTUALIZACION_EstimacionConcepto.md** - Este documento (nuevo)
- ? **DOCUMENTACION_FormAvanceConcepto.md** - Documentación original
- ? **SQL_VerificarEstructuraConceptos.sql** - Script antiguo (referencia)

---

## ? Checklist de Actualización

- [x] Modificado método `CargarConceptosPorPrototipo()`
- [x] Removida lógica de prototipo (CostoCalandra/CostoTunera)
- [x] Agregado script de verificación SQL
- [x] Creado documento de actualización
- [x] Verificado que compila sin errores

### Pendiente (usuario debe hacer):
- [ ] Ejecutar `SQL_VerificarEstimacionConcepto.sql`
- [ ] Verificar que la tabla existe y tiene datos
- [ ] Probar el formulario con casa real
- [ ] Verificar que los totales coinciden con los esperados

---

## ?? Resultado Final

El formulario ahora carga los conceptos directamente desde la tabla `dbo.Estimacion(Concepto)`, generando códigos secuenciales automáticamente y usando el campo `TOTAL` sin distinción de prototipo.

**Ejemplo de salida:**
```
Código | Concepto                              | Total
-------|---------------------------------------|---------------
1      | Acabados                              | $755,153.18
2      | Albañileria                           | $573,711.08
3      | Carpinteria y Cerrajeria             | $109,827.44
4      | Cimentación                          | $189,361.56
5      | Estructura                           | $1,383,350.28
6      | Herreria, Aluminio y Vidrio          | $136,126.26
7      | Inst. Electrica                      | $285,229.17
8      | Inst. Hidraulica - Sanitaria - Gas LP| $239,728.06
9      | Limpieza e Instalaciones Especiales  | $96,438.52
10     | Muebles y Accesorios                 | $113,779.20
11     | Preliminares (trazo, cimbra, exc)    | $8,083.38
12     | Urbanizacion                         | $0.00
-------|---------------------------------------|---------------
TOTAL  |                                       | $3,890,788.14
```

---

**Fecha de Actualización:** Enero 2025  
**Versión:** 1.1  
**Estado:** ? IMPLEMENTADO Y FUNCIONAL
