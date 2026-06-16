# ?? Sistema de Avance de Obra por Manzana/Lote

## ?? Descripción

El **FormAvanceObra** es un formulario que permite visualizar el avance de construcción de cada casa (manzana/lote) basándose en las categorías de presupuesto de obra.

## ?? Características Principales

### 1. **Categorías de Obra**
El sistema rastrea el avance de las siguientes categorías principales:

- ? **Preliminares** - Trabajos iniciales del terreno
- ? **Cimentación** - Base y zapatas
- ? **Muros Planta Baja** - Construcción de muros primer nivel
- ? **Muros Planta Alta** - Construcción de muros segundo nivel
- ? **Losas** - Entrepisos y techos
- ? **Azotea** - Terminado de azotea
- ? **Inst. Hidráulica, Sanitaria y Gas LP** - Instalaciones
- ? **Inst. Eléctrica** - Sistema eléctrico
- ? **Albañilería Planta Baja** - Acabados primer nivel
- ? **Albañilería Planta Alta** - Acabados segundo nivel
- ? **Acabados Interiores** - Pisos, azulejos, pintura
- ? **Acabados Exteriores** - Fachadas
- ? **Herrería, Aluminio y Vidrio** - Ventanas, puertas metálicas
- ? **Carpintería y Cancelería** - Puertas de madera
- ? **Muebles y Accesorios** - Cocina, baños
- ? **Obra Exterior** - Banquetas, jardines
- ? **Urbanización** - Servicios externos

### 2. **Vista de Avance**

#### Tabla Detallada
Muestra para cada categoría:
- **Categoría**: Nombre del rubro de obra
- **Presupuestado**: Monto total planificado ($)
- **Ejecutado**: Monto gastado/ejecutado hasta la fecha ($)
- **% Avance**: Porcentaje de avance (Ejecutado / Presupuestado × 100)
- **Estado**: Indicador visual del estado
  - ? **Completado** (verde): 100% o más
  - ? **En Progreso** (naranja): 50% - 99%
  - ? **Iniciado** (naranja oscuro): 1% - 49%
  - ? **Sin Iniciar** (gris): 0%

#### Colores por Avance
- ?? **Verde**: ? 80% de avance
- ?? **Naranja**: 50% - 79% de avance
- ?? **Rojo**: < 50% de avance (con algo ejecutado)
- ? **Gris**: Sin iniciar (0%)

### 3. **Panel de Resumen**

Muestra información general de la casa:

```
????????????????????????????????????????????????????
?  Total Presupuestado: $776,772.85               ?
?  Total Ejecutado: $423,215.42                   ?
?  AVANCE GENERAL: 54.5%                          ?
?                                                  ?
?  [????????????????????] 54.5%                   ?
?                                                  ?
?  Completadas: 5 | En Progreso: 8 | Sin Iniciar: 4?
????????????????????????????????????????????????????
```

### 4. **Gráfica Visual**

Muestra un gráfico de barras con:
- Una barra por categoría
- Altura proporcional al porcentaje de avance
- Colores según el estado
- Etiqueta con el porcentaje en cada barra

### 5. **Exportación a PDF**

Genera un reporte profesional en PDF con:

#### Estructura del PDF:

```
???????????????????????????????????????????
? HEADER AZUL                             ?
?  REPORTE DE AVANCE DE OBRA              ?
?  Manzana: XX | Lote: XX                 ?
???????????????????????????????????????????
? RESUMEN GENERAL                         ?
?  • Total Presupuestado                  ?
?  • Total Ejecutado                      ?
?  • AVANCE GENERAL: XX%                  ?
???????????????????????????????????????????
? DETALLE POR CATEGORÍA                   ?
? ?????????????????????????????????????   ?
? ? Tabla con todas las categorías    ?   ?
? ? (colores por estado)              ?   ?
? ?????????????????????????????????????   ?
???????????????????????????????????????????
? Pie de página con fecha y hora          ?
???????????????????????????????????????????
```

## ?? Cómo Usar

### Paso 1: Acceder al Formulario

Desde el **PanelPrincipal**:
1. Click en el menú **OBRA** (barra superior)
2. Seleccionar **"Avance de Obra por Casa"**

### Paso 2: Seleccionar Casa

1. Seleccionar **Manzana** del combo box
2. Seleccionar **Lote** (se cargan automáticamente según la manzana)
3. Click en **?? Cargar Avance**

### Paso 3: Visualizar Información

El sistema mostrará:
- ? Tabla con todas las categorías y sus avances
- ? Resumen general en el panel superior
- ? Gráfica de barras en el panel inferior
- ? Estadísticas de categorías completadas/en progreso/sin iniciar

### Paso 4: Exportar Reporte (Opcional)

Click en **?? Exportar PDF** para generar el reporte completo.

## ?? Estructura de Datos

### Tabla: `PresupuestoObra`

```sql
CREATE TABLE PresupuestoObra (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Categoria NVARCHAR(100) NOT NULL,
    Subcategoria NVARCHAR(200),
    Descripcion NVARCHAR(500),
    Unidad NVARCHAR(50),
    Cantidad DECIMAL(18,4),
    PrecioUnitario DECIMAL(18,2),
    CostoTotal DECIMAL(18,2),
    Prototipo NVARCHAR(50) -- Ej: "PLANTA BAJA", "PLANTA ALTA"
);
```

### Obtención de Datos

El sistema obtiene los datos de dos fuentes:

#### 1. **Presupuestado**
- Desde tabla `PresupuestoObra`
- Agrupado por `Categoria`
- Suma de `CostoTotal`

#### 2. **Ejecutado**
- Desde tabla `OrdenesCompra` y `OrdenesCompraDetalle`
- Filtrado por `Manzana` y `Lote`
- Suma de `ImporteTotal` por categoría

### Ejemplo de Consulta

```sql
-- Obtener ejecutado para una casa específica
SELECT 
    d.Categoria,
    SUM(d.ImporteTotal) AS TotalEjecutado
FROM OrdenesCompra o
INNER JOIN OrdenesCompraDetalle d ON o.FolioOC = d.FolioOC
WHERE o.Manzana = 'M1' AND o.Lote = 'L5'
GROUP BY d.Categoria
```

## ?? Valores de Presupuesto Base

Si no existe la tabla `PresupuestoObra`, el sistema usa valores de ejemplo del documento proporcionado:

| Categoría | Presupuesto |
|-----------|-------------|
| Preliminares | $8,223.62 |
| Cimentación | $55,324.17 |
| Muros Planta Baja | $61,540.18 |
| Muros Planta Alta | $30,197.35 |
| Losas | $100,484.52 |
| Azotea | $9,240.85 |
| Inst. Hidráulica... | $27,625.27 |
| Inst. Eléctrica | $33,915.84 |
| Albañilería PB | $85,521.06 |
| Albañilería PA | $57,220.55 |
| Acabados Interiores | $206,013.17 |
| Acabados Exteriores | $9,074.29 |
| Herrería... | $9,363.12 |
| Carpintería... | $21,676.37 |
| Muebles y Accesorios | $39,705.35 |
| Obra Exterior | $21,653.13 |
| Urbanización | $0.00 |
| **TOTAL** | **$776,772.85** |

## ?? Colores del Sistema

### Colores Principales
- **Header**: `#2980B9` (Azul corporativo)
- **Éxito**: `#2ECC71` (Verde)
- **Advertencia**: `#F39C12` (Naranja)
- **Peligro**: `#E74C3C` (Rojo)
- **Gris**: `#95A5A6` (Sin iniciar)

### Estados de Categoría

```csharp
public enum EstadoCategoria
{
    SinIniciar,    // 0%
    Iniciado,      // 1-49%
    EnProgreso,    // 50-99%
    Atrasado,      // (futuro: comparar con cronograma)
    Completado     // 100%+
}
```

## ?? Archivos Generados

### Carpeta de PDFs
```
C:\Users\{Usuario}\Documents\CALANDRIA RESIDENCIAL\PDFAvanceObra\
```

### Nombre del PDF
```
AvanceObra_M{Manzana}_L{Lote}_{yyyyMMdd}.pdf

Ejemplo:
AvanceObra_M1_L5_20250131.pdf
```

## ?? Personalización

### Agregar Nuevas Categorías

1. Agregar en el array `categoriasPrincipales` en el método `CargarDatosAvance()`:

```csharp
var categoriasPrincipales = new[]
{
    "Preliminares",
    "Cimentación",
    // ... existentes ...
    "TU_NUEVA_CATEGORIA" // ? Agregar aquí
};
```

2. Agregar el presupuesto en `ObtenerPresupuestoEjemplo()`:

```csharp
var presupuestos = new Dictionary<string, decimal>
{
    // ... existentes ...
    { "TU_NUEVA_CATEGORIA", 10000.00m } // ? Agregar aquí
};
```

### Modificar Colores

En el método `DibujarGraficaAvance()` o `GenerarPDFAvance()`:

```csharp
Color barColor = cat.PorcentajeAvance >= 80 ? Color.FromArgb(46, 204, 113) :  // Verde
                cat.PorcentajeAvance >= 50 ? Color.FromArgb(243, 156, 18) :   // Naranja
                cat.PorcentajeAvance > 0 ? Color.FromArgb(230, 126, 34) :     // Naranja oscuro
                Color.LightGray;                                               // Gris
```

## ?? Solución de Problemas

### Problema: No se muestran datos

**Causa**: No hay órdenes de compra asociadas a la casa  
**Solución**: 
1. Verifica que existan registros en `OrdenesCompra` con esa Manzana/Lote
2. Verifica que `OrdenesCompraDetalle` tenga el campo `Categoria` lleno

### Problema: Avance muestra 0% en todas las categorías

**Causa**: El campo `Categoria` en `OrdenesCompraDetalle` no coincide con las categorías definidas  
**Solución**: 
1. Verifica los valores en la columna `Categoria` de la tabla
2. Asegúrate de que coincidan exactamente con los nombres del array `categoriasPrincipales`

### Problema: PDF no se abre

**Causa**: Permisos de escritura o visor PDF no configurado  
**Solución**:
1. Verifica permisos en la carpeta `Documents\CALANDRIA RESIDENCIAL\PDFAvanceObra`
2. Instala un visor de PDF (Adobe Reader, etc.)

## ?? Futuras Mejoras

### Versión 2.0 (Planeado)
- [ ] Comparación con cronograma (fechas programadas vs reales)
- [ ] Alertas de retrasos
- [ ] Gráfica de evolución temporal
- [ ] Filtros por rango de fechas
- [ ] Exportar a Excel
- [ ] Fotografías de avance por categoría
- [ ] Comparación entre casas del mismo prototipo
- [ ] Dashboard ejecutivo con múltiples casas

## ?? Soporte

Para problemas o sugerencias:
1. Revisar logs de la aplicación
2. Verificar estructura de base de datos
3. Contactar al administrador del sistema

---

**Desarrollado por**: Calandria Development Team  
**Versión**: 1.0  
**Fecha**: Enero 2025  
**Framework**: .NET Framework 4.7.2
