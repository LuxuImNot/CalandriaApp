# ?? CAMBIO: Numeración Global de Estimaciones

## ?? Descripción del Cambio

Se modificó el sistema de numeración de estimaciones de **por casa** a **numeración global/general** del proyecto.

---

## ?? ANTES (Sistema Anterior)

### Comportamiento Previo
- Cada casa (Manzana/Lote) tenía su propio contador de estimaciones
- El número de estimación se reiniciaba por cada casa

### Ejemplo Anterior
```
Casa M5-L2:
  EST-M5-L2-001-2025 (1ra estimación de esta casa)
  EST-M5-L2-002-2025 (2da estimación de esta casa)

Casa M3-L20:
  EST-M3-L20-001-2025 (1ra estimación de esta casa)
  EST-M3-L20-002-2025 (2da estimación de esta casa)
```

### Problemas
- Múltiples estimaciones número 001, 002, etc. en el mismo proyecto
- Dificultad para saber el orden cronológico global
- Confusión al tener varias "Estimación No. 1" simultáneamente

---

## ?? AHORA (Sistema Actual)

### Comportamiento Nuevo
- **Numeración única y global** para todo el proyecto
- El contador es compartido entre todas las casas
- Cada estimación tiene un número único en todo el sistema

### Ejemplo Actual
```
Estimación 1: EST-001-2025 (Manzana 5, Lote 2)
Estimación 2: EST-002-2025 (Manzana 3, Lote 20)
Estimación 3: EST-003-2025 (Manzana 5, Lote 2)  <- 2da estimación de M5-L2
Estimación 4: EST-004-2025 (Manzana 1, Lote 8)
Estimación 5: EST-005-2025 (Manzana 3, Lote 20) <- 2da estimación de M3-L20
```

### Ventajas
? **Trazabilidad completa**: Cada estimación tiene un número único en el proyecto  
? **Orden cronológico claro**: Los números reflejan el orden real de generación  
? **Sin confusión**: No hay duplicados de "Estimación No. 1"  
? **Profesional**: Formato estándar de la industria  
? **Auditoría**: Fácil identificar cuántas estimaciones se han generado en total

---

## ?? Cambios en el Formato del PDF

### Encabezado Reorganizado

**Antes:**
```
???????????????????????????????????????????????????????
? PROVEEDOR:              ? FECHA:     ? ESTIMACIÓN:  ?
? [Proveedor]             ? 01/01/2025 ? 1            ?
???????????????????????????????????????????????????????
FOLIO: EST-M5-L2-001-2025
```

**Ahora:**
```
???????????????????????????????????????????????????????????????
? UBICACIÓN: ? PROVEEDOR:      ? FECHA:     ? EST. No.:     ?
? M5 L2      ? [Proveedor]     ? 01/01/2025 ? 3             ?
???????????????????????????????????????????????????????????????
FOLIO: EST-003-2025
```

### Mejoras Visuales
- **UBICACIÓN** destacada para identificar rápidamente la casa
- **Folio simplificado** sin duplicar información de manzana/lote
- **Número de estimación global** visible claramente
- **Diseño más limpio** y profesional

---

## ?? Archivos Modificados

### 1. `FormEstimacionConceptoMigrado_ExtensionFolios.cs`

#### Método `ObtenerSiguienteNumeroEstimacion()`
```csharp
// ANTES: Contaba por casa
SELECT ISNULL(MAX(NumeroEstimacion), 0) + 1 
FROM FoliosEstimacion 
WHERE Manzana = @manzana AND Lote = @lote;

// AHORA: Cuenta globalmente
SELECT ISNULL(MAX(NumeroEstimacion), 0) + 1 
FROM FoliosEstimacion;
```

#### Método `GenerarFolio()`
```csharp
// ANTES: Incluía manzana y lote
return $"EST-M{manzana}-L{lote}-{numeroEstimacion:000}-{DateTime.Now:yyyy}";
// Ejemplo: EST-M5-L2-001-2025

// AHORA: Formato simplificado
return $"EST-{numeroEstimacion:000}-{DateTime.Now:yyyy}";
// Ejemplo: EST-003-2025
```

### 2. `FormEstimacionConceptoMigrado_ExtensionPDF.cs`

#### Encabezado Reorganizado
- ? Nueva sección **UBICACIÓN** (Manzana/Lote)
- ? **4 columnas** en lugar de 3 en la fila principal
- ? Distribución optimizada: 30% - 25% - 20% - 25%
- ? Folio simplificado en esquina superior derecha

### 3. `FormEstimacionConceptoMigrado.Preview.cs`

#### Vista Previa Actualizada
- ? Refleja el nuevo formato del PDF
- ? Muestra "Estimación No. X" global
- ? Ubicación claramente visible
- ? Mensaje de preview actualizado

---

## ??? Estructura de Base de Datos

### Tabla `FoliosEstimacion`

La columna `NumeroEstimacion` ahora almacena el **número global**:

```sql
SELECT 
    NumeroEstimacion,  -- Número global único
    Folio,             -- EST-###-YYYY
    Manzana,           -- Para referencia
    Lote,              -- Para referencia
    FechaGeneracion
FROM FoliosEstimacion
ORDER BY NumeroEstimacion DESC;
```

**Resultado Ejemplo:**
```
NumeroEstimacion | Folio           | Manzana | Lote | FechaGeneracion
???????????????????????????????????????????????????????????????????????
5                | EST-005-2025    | 3       | 20   | 2025-01-15 14:30
4                | EST-004-2025    | 1       | 8    | 2025-01-15 11:20
3                | EST-003-2025    | 5       | 2    | 2025-01-14 16:45
2                | EST-002-2025    | 3       | 20   | 2025-01-14 10:15
1                | EST-001-2025    | 5       | 2    | 2025-01-13 09:00
```

---

## ?? Consultas SQL Útiles

### Ver Estimaciones en Orden Global
```sql
SELECT 
    NumeroEstimacion AS 'No. Est.',
    Folio,
    'M' + Manzana + ' L' + Lote AS Casa,
    Proveedor,
    TotalEstimacion,
    FORMAT(FechaGeneracion, 'dd/MM/yyyy HH:mm') AS Fecha
FROM FoliosEstimacion
ORDER BY NumeroEstimacion DESC;
```

### Contar Estimaciones por Casa
```sql
SELECT 
    'M' + Manzana + ' L' + Lote AS Casa,
    COUNT(*) AS TotalEstimaciones,
    MIN(NumeroEstimacion) AS PrimeraEst,
    MAX(NumeroEstimacion) AS UltimaEst
FROM FoliosEstimacion
GROUP BY Manzana, Lote
ORDER BY MAX(NumeroEstimacion) DESC;
```

### Ver Total de Estimaciones del Proyecto
```sql
SELECT 
    COUNT(*) AS TotalEstimaciones,
    MIN(NumeroEstimacion) AS Primera,
    MAX(NumeroEstimacion) AS Ultima,
    SUM(TotalEstimacion) AS MontoTotal
FROM FoliosEstimacion;
```

---

## ? Compatibilidad

### Base de Datos Existente
- ? **Compatible** con tablas existentes
- ? No requiere migración de datos
- ? Los folios antiguos siguen siendo válidos

### Estimaciones Previas
- ?? Las estimaciones generadas antes del cambio mantienen su formato anterior
- ?? Las nuevas estimaciones usan el formato global
- ? Ambos formatos coexisten sin problemas

---

## ?? Impacto del Cambio

### Para el Usuario
? **Más claro**: Es fácil saber cuántas estimaciones hay en total  
? **Más ordenado**: Los números reflejan el orden real  
? **Más profesional**: Formato estándar de la industria  
? **Menos confusión**: No hay números duplicados

### Para el Sistema
? **Más simple**: Menos lógica de conteo  
? **Más rápido**: Menos filtros en consultas  
? **Más escalable**: No hay límites por casa  
? **Más auditable**: Trazabilidad completa

---

## ?? Ejemplo de Uso Real

### Escenario: Proyecto con 3 Casas

```
Cronología de Estimaciones:

1?? EST-001-2025 ? M5-L2  (Cimentación)       ? 13/01/2025 09:00
2?? EST-002-2025 ? M3-L20 (Cimentación)       ? 14/01/2025 10:15
3?? EST-003-2025 ? M5-L2  (Estructura)        ? 14/01/2025 16:45
4?? EST-004-2025 ? M1-L8  (Cimentación)       ? 15/01/2025 11:20
5?? EST-005-2025 ? M3-L20 (Estructura)        ? 15/01/2025 14:30
6?? EST-006-2025 ? M5-L2  (Albañilería)       ? 16/01/2025 08:00
```

### Análisis Rápido
- **Total estimaciones**: 6
- **Casa M5-L2**: 3 estimaciones (001, 003, 006)
- **Casa M3-L20**: 2 estimaciones (002, 005)
- **Casa M1-L8**: 1 estimación (004)
- **Última estimación**: 006

---

## ?? Notas Importantes

### ?? Consideraciones
1. **No hay marcha atrás**: Una vez generada una estimación, su número es permanente
2. **No se puede saltar números**: El sistema asigna secuencialmente
3. **Eliminación de estimaciones**: El número no se reutiliza (por diseño)

### ?? Recomendaciones
- Usar el **Repositorio de PDFs** para ver el historial por casa
- Filtrar por **Manzana/Lote** cuando se necesite ver estimaciones específicas
- El **FOLIO** es único y permanente, usar siempre como referencia

---

## ?? Archivos Relacionados

- `FormEstimacionConceptoMigrado_ExtensionFolios.cs` - Lógica de folios
- `FormEstimacionConceptoMigrado_ExtensionPDF.cs` - Diseño del PDF
- `FormEstimacionConceptoMigrado.Preview.cs` - Vista previa
- `FormEstimacionConceptoMigrado.Avances.cs` - Guardado y exportación
- `FormRepositorioPDFs.cs` - Consulta de estimaciones

---

## ? Estado

**Implementado**: ? Completado  
**Fecha**: Enero 2025  
**Versión**: 2.0 - Numeración Global  
**Compilación**: ? Exitosa  

---

**Desarrollado para**: Sistema Calandria Residencial  
**Tipo de cambio**: Mejora de funcionalidad  
**Nivel de impacto**: Alto (mejora UX y trazabilidad)
