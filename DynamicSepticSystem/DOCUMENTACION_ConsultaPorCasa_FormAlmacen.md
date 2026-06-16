# ?? DOCUMENTACIÓN - Pestaña CONSULTA POR CASA en FormAlmacen

## ?? Resumen de Implementación

Se ha agregado una nueva pestaña **CONSULTA POR CASA** al módulo de Gestión de Almacén que permite visualizar **TODOS los insumos asignados a una casa específica** mediante búsqueda por Manzana/Lote.

---

## ? Características Implementadas

### 1. Búsqueda por Casa
- ??? **Selector de Manzana**: ComboBox con todas las manzanas que tienen entradas registradas
- ?? **Selector de Lote**: ComboBox dinámico que se actualiza según la manzana seleccionada
- ?? **Cascada automática**: Al seleccionar manzana, se cargan sus lotes
- ? **Validación**: Solo muestra casas con insumos registrados

### 2. Vista Detallada de Insumos por Casa
Muestra todos los insumos asignados a la casa seleccionada con la siguiente información:
- ?? **Fecha de entrada**
- ?? **Estado** (Completo/Discrepancia)
- ?? **Folio de orden de compra**
- ?? **Clave del insumo**
- ?? **Descripción completa**
- ?? **Unidad de medida**
- ?? **Cantidad recibida**
- ?? **Precio unitario**
- ?? **Importe total**
- ??? **Prototipo de la casa**
- ?? **Usuario que registró**
- ?? **Indicador de excepción**
- ?? **Justificación (si aplica)**

### 3. Estadísticas en Tiempo Real
Panel de información con:
- ?? **Casa actual**: "CASA: Manzana X - Lote Y"
- ??? **Prototipo**: Muestra el modelo de la casa
- ?? **Total de insumos**: Contador de insumos asignados
- ?? **Importe total**: Suma de todos los importes
- ?? **Discrepancias**: Contador (solo visible si hay)

### 4. Búsqueda Inteligente dentro de la Casa
- ?? Campo de búsqueda en tiempo real
- ?? Busca en múltiples columnas:
  - Clave de insumo
  - Descripción
  - Folio de OC
  - Prototipo
  - Usuario
  - Justificación
- ?? Mantiene el contexto de la casa seleccionada

### 5. Exportación Personalizada
- ?? Exporta los insumos de la casa específica a Excel
- ?? **Nombre automático**: `Insumos_Casa_M[X]_L[Y]_YYYYMMDD_HHMMSS.xlsx`
- ?? **Formato profesional** con:
  - Título corporativo
  - Información de la casa en el encabezado
  - Fecha de generación
  - Formatos de moneda y números
- ?? Opción de abrir archivo inmediatamente

### 6. Actualización de Datos
- ?? Botón para refrescar los datos de la casa actual
- ? Validación de selección antes de actualizar
- ?? Confirmación visual

---

## ??? Archivos Creados/Modificados

### ? Archivo Nuevo

#### 1. `FormAlmacen_ConsultaCasa.cs` (NUEVO)
**Tamaño**: ~700 líneas de código

**Métodos implementados**:

1. **`CargarManzanasConsulta()`**
   - Consulta las manzanas con entradas en la base de datos
   - Llena el ComboBox de manzanas
   - Agrega opción "-- Seleccionar --"

2. **`CmbManzanaConsulta_SelectedIndexChanged()`**
   - Maneja el cambio de manzana seleccionada
   - Carga los lotes correspondientes
   - Limpia la vista de datos
   - Resetea estadísticas

3. **`CmbLoteConsulta_SelectedIndexChanged()`**
   - Maneja el cambio de lote seleccionado
   - Carga los insumos de la casa
   - Valida la selección

4. **`CargarInsumosPorCasa()`**
   - Consulta todos los insumos de la casa seleccionada
   - Calcula el estado de cada entrada
   - Llena el DataGridView
   - Configura columnas
   - Actualiza estadísticas

5. **`ConfigurarColumnasConsultaCasa()`**
   - Configura encabezados y anchos
   - Aplica formatos (moneda, decimales, fechas)
   - Colorea filas según estado
   - Aplica tema corporativo

6. **`ActualizarEstadisticasConsulta()`**
   - Calcula total de insumos
   - Suma importes totales
   - Cuenta discrepancias
   - Extrae prototipo
   - Actualiza labels informativos

7. **`LimpiarEstadisticasConsulta()`**
   - Resetea todos los labels
   - Muestra mensaje de instrucción
   - Oculta contador de discrepancias

8. **`TxtBuscarConsultaCasa_TextChanged()`**
   - Filtra el DataGridView en tiempo real
   - Busca en 6 columnas diferentes
   - Maneja errores automáticamente

9. **`BtnExportarConsultaCasa_Click()`**
   - Valida que haya datos
   - Muestra diálogo de guardar
   - Llama al método de exportación
   - Ofrece abrir el archivo

10. **`ExportarConsultaCasaExcel()`**
    - Crea libro Excel con ClosedXML
    - Aplica formato corporativo
    - Exporta con información de la casa
    - Ajusta anchos y formatos

11. **`BtnActualizarConsultaCasa_Click()`**
    - Valida la selección
    - Recarga los datos
    - Muestra confirmación

### ? Archivos Modificados

#### 1. `FormAlmacen.Designer.cs`
**Cambios**:
- Agregado `TabPage tabPageConsultaCasa`
- Agregado `ComboBox cmbManzanaConsulta`
- Agregado `ComboBox cmbLoteConsulta`
- Agregado `DataGridView dgvConsultaCasa`
- Agregado `TextBox txtBuscarConsultaCasa`
- Agregado 8 Labels para información y estadísticas
- Agregado `Button btnExportarConsultaCasa`
- Agregado `Button btnActualizarConsultaCasa`
- Configuración completa de controles con posicionamiento

**Ubicación de controles**:
```
???????????????????????????????????????????????????????????
? MANZANA: [Combo?]  LOTE: [Combo?]  BUSCAR: [_______] ?
?                                                          ?
? CASA: Manzana X - Lote Y    ?  Total: 45 insumos      ?
? Prototipo: TUNERA            ?  Importe: $125,450.00   ?
?                                                          ?
? ?????????????????????????????????????????????????????? ?
? ?  DataGridView (Insumos de la casa)                 ? ?
? ?  [Datos con scroll]                                 ? ?
? ?                                                      ? ?
? ?                                                      ? ?
? ?????????????????????????????????????????????????????? ?
?                                                          ?
? [?? ACTUALIZAR]                  [?? EXPORTAR A EXCEL] ?
???????????????????????????????????????????????????????????
```

#### 2. `FormAlmacen_Layout.cs`
**Cambios**:
- Agregado método `ConfigurarLayoutConsultaCasa()`
- Agregado método `AjustarLayoutTabConsultaCasa()`
- Configuración de anclajes responsivos
- Llamada desde `ConfigurarLayoutResponsivo()`
- Integración con `FormAlmacen_Resize()`

#### 3. `FormAlmacen_Historial.cs`
**Cambios**:
- Actualizado `TabsControl_SelectedIndexChanged`
- Agregado llamado a `CargarManzanasConsulta()` cuando se selecciona la pestaña

---

## ?? Diseño Visual

### Paleta de Colores
- ?? **Principal**: `#B36C2E` (Color corporativo Calandria)
- ? **Fondo**: Blanco
- ?? **Completo**: Verde claro (`#E6FFE6`)
- ?? **Discrepancia**: Rojo claro (`#FFE6E6`)
- ?? **Selección**: Azul corporativo

### Tipografía
- **Títulos**: Segoe UI, 11pt, Bold
- **Labels**: Segoe UI, 10pt, Regular/Bold
- **Combos/TextBox**: Segoe UI, 10pt
- **Botones**: Segoe UI, 9pt, Bold
- **DataGridView**: Segoe UI, 9pt

### Estados Visuales
- **Sin selección**: Label gris en cursiva "Seleccione una casa..."
- **Casa seleccionada**: Label color corporativo, negrita
- **Filas completas**: Fondo verde claro
- **Filas con discrepancia**: Fondo rojo claro

---

## ?? Base de Datos

### Consulta Principal: Insumos por Casa

```sql
SELECT 
    e.FechaEntrada,
    e.FolioOC,
    e.Clave,
    e.Descripcion,
    e.Unidad,
    e.Cantidad,
    e.PrecioUnitario,
    e.Importe,
    e.Prototipo,
    e.Usuario,
    e.EsExcepcion,
    e.Justificacion,
    CASE 
        WHEN e.EsExcepcion = 1 THEN 'DISCREPANCIA'
        ELSE 'COMPLETO'
    END AS Estado
FROM EntradasAlmacen e
WHERE e.Manzana = @manzana AND e.Lote = @lote
ORDER BY e.FechaEntrada DESC, e.Clave
```

### Consulta Auxiliar 1: Manzanas Disponibles

```sql
SELECT DISTINCT Manzana 
FROM EntradasAlmacen 
WHERE Manzana IS NOT NULL AND Manzana != ''
ORDER BY Manzana
```

### Consulta Auxiliar 2: Lotes por Manzana

```sql
SELECT DISTINCT Lote 
FROM EntradasAlmacen 
WHERE Manzana = @manzana AND Lote IS NOT NULL AND Lote != ''
ORDER BY Lote
```

---

## ?? Cómo Usar

### Flujo de Trabajo Completo

#### Paso 1: Abrir el Formulario de Almacén
```
Menú Principal ? ALMACÉN ? Gestión de Almacén
```

#### Paso 2: Ir a la Pestaña CONSULTA POR CASA
- Hacer clic en la pestaña **"CONSULTA POR CASA"** (cuarta pestaña)
- El sistema cargará automáticamente las manzanas disponibles

#### Paso 3: Seleccionar Casa
1. **Seleccionar Manzana** del primer ComboBox
2. Automáticamente se cargarán los lotes de esa manzana
3. **Seleccionar Lote** del segundo ComboBox
4. Automáticamente se cargarán los insumos de esa casa

#### Paso 4: Revisar Información
- Ver el panel de estadísticas:
  - Casa seleccionada
  - Prototipo
  - Total de insumos
  - Importe total
  - Discrepancias (si hay)
- Revisar la tabla de insumos

#### Paso 5: Buscar Insumo Específico (Opcional)
1. Escribir en el campo de búsqueda
2. El sistema filtrará automáticamente
3. Los resultados se actualizan en tiempo real

#### Paso 6: Exportar (Opcional)
1. Clic en **"?? EXPORTAR A EXCEL"**
2. Seleccionar ubicación
3. Confirmar
4. Opcionalmente abrir el archivo

#### Paso 7: Actualizar Datos (Opcional)
- Clic en **"?? ACTUALIZAR"** para refrescar desde la base de datos

---

## ?? Casos de Uso

### Caso 1: Verificar Insumos de una Casa
**Objetivo**: Ver qué materiales se han recibido para Manzana 5, Lote 12

**Pasos**:
1. Abrir pestaña CONSULTA POR CASA
2. Seleccionar Manzana = "5"
3. Seleccionar Lote = "12"
4. Revisar la lista de insumos
5. Verificar el importe total

**Resultado**: Se muestra lista completa con 45 insumos, importe total $125,450.00

---

### Caso 2: Buscar un Insumo Específico en una Casa
**Objetivo**: Verificar si ya se recibió el cemento para Manzana 3, Lote 8

**Pasos**:
1. Seleccionar Manzana = "3", Lote = "8"
2. Escribir "cemento" en el campo de búsqueda
3. Revisar los resultados filtrados

**Resultado**: Muestra 3 entradas de cemento con fechas y cantidades

---

### Caso 3: Detectar Discrepancias en una Casa
**Objetivo**: Identificar si hay problemas con los insumos de una casa

**Pasos**:
1. Seleccionar la casa
2. Observar el panel de estadísticas
3. Si aparece "Con discrepancia: X", revisar la tabla
4. Identificar filas con fondo rojo claro
5. Leer la justificación

**Resultado**: Se identifican 2 insumos con discrepancias justificadas

---

### Caso 4: Generar Reporte de Insumos por Casa
**Objetivo**: Crear reporte en Excel para revisión de supervisor

**Pasos**:
1. Seleccionar la casa
2. Clic en "EXPORTAR A EXCEL"
3. Guardar como `Insumos_M5_L12_Revision.xlsx`
4. Abrir archivo
5. Enviar por correo electrónico

**Resultado**: Excel profesional con todos los detalles de la casa

---

### Caso 5: Comparar Insumos entre Casas del Mismo Prototipo
**Objetivo**: Ver diferencias entre dos casas TUNERA

**Pasos**:
1. Consultar Manzana 3, Lote 5 (TUNERA)
2. Anotar total de insumos e importe
3. Consultar Manzana 8, Lote 12 (TUNERA)
4. Anotar total de insumos e importe
5. Comparar

**Resultado**: Casa 1: 42 insumos, $118,000. Casa 2: 45 insumos, $125,450. Diferencia detectada.

---

## ?? Formato de Exportación Excel

### Estructura del Archivo

```
???????????????????????????????????????????????????????????????
?  INSUMOS POR CASA - CALANDRIA RESIDENCIAL                   ? <- Encabezado (Color corporativo)
???????????????????????????????????????????????????????????????
?  Casa: Manzana 5 - Lote 12                                  ? <- Información de la casa
???????????????????????????????????????????????????????????????
?  Generado: 15/01/2025 14:30                                 ? <- Fecha
???????????????????????????????????????????????????????????????
?                                                              ?
? [Tabla con 13 columnas]                                     ?
? Fecha | Estado | Folio | Clave | Descripción | ...          ?
? ...                                                          ?
???????????????????????????????????????????????????????????????
```

### Columnas Exportadas
1. Fecha
2. Estado
3. Folio OC
4. Clave
5. Descripción
6. Unidad
7. Cantidad
8. P.U. (Precio Unitario)
9. Importe
10. Prototipo
11. Usuario
12. Excepción
13. Justificación

### Formatos Aplicados
- **Moneda**: P.U. e Importe ? `$#,##0.00`
- **Decimal**: Cantidad ? `0.00`
- **Texto**: Todo lo demás
- **Anchos**: Ajustados automáticamente

---

## ??? Validaciones y Seguridad

### Validaciones Implementadas
- ? Verificación de selección de manzana antes de cargar lotes
- ? Verificación de selección de lote antes de cargar insumos
- ? Verificación de datos antes de exportar
- ? Manejo de valores nulos y DBNull
- ? Conversión explícita de tipos
- ? Filtrado seguro (DataTable.DefaultView.RowFilter)
- ? Manejo de excepciones con mensajes descriptivos

### Mensajes de Error
- "? Error al cargar manzanas"
- "? Error al cargar lotes"
- "? Error al cargar insumos"
- "?? No hay datos para exportar"
- "? Error al exportar a Excel"
- "?? Debe seleccionar una manzana y lote primero"

---

## ?? Solución de Problemas

### Problema 1: No aparecen manzanas
**Causa**: No hay entradas registradas con Manzana
**Solución**:
```sql
-- Verificar entradas
SELECT COUNT(*), Manzana 
FROM EntradasAlmacen 
WHERE Manzana IS NOT NULL 
GROUP BY Manzana
```

### Problema 2: Lotes no se cargan
**Causa**: No hay entradas para esa manzana
**Solución**: Verificar que la manzana tenga lotes con entradas registradas

### Problema 3: No se muestran insumos
**Causa**: No hay entradas para esa combinación Manzana/Lote
**Solución**: Verificar en base de datos:
```sql
SELECT * FROM EntradasAlmacen 
WHERE Manzana = 'X' AND Lote = 'Y'
```

### Problema 4: Error al exportar
**Causa**: ClosedXML no instalado o ruta inválida
**Solución**:
- Verificar paquete NuGet ClosedXML
- Seleccionar ruta con permisos de escritura

---

## ?? Rendimiento

### Métricas Esperadas
- ? Carga de manzanas: < 500ms
- ?? Carga de lotes: < 300ms
- ?? Carga de insumos: < 1 segundo (hasta 1,000 insumos por casa)
- ?? Búsqueda: Tiempo real (< 100ms)
- ?? Exportación: < 3 segundos (hasta 500 insumos)

### Optimizaciones
- ? Consultas indexadas por Manzana y Lote
- ? Carga bajo demanda (lazy loading)
- ? Filtrado en memoria (DataTable.DefaultView)
- ? Caché de combos (no se recargan innecesariamente)

---

## ?? Comparación con Otras Pestañas

| Característica | INVENTARIO | CONSULTA POR CASA |
|----------------|------------|-------------------|
| **Alcance** | Todas las entradas | Insumos de una casa |
| **Filtro principal** | Búsqueda libre | Manzana + Lote |
| **Estadísticas** | Totales generales | Totales por casa |
| **Exportación** | Inventario completo | Insumos de la casa |
| **Uso** | Auditoría general | Seguimiento por casa |

---

## ?? Beneficios

### Para el Usuario
- ?? **Vista focalizada**: Solo ve lo relevante para una casa
- ?? **Seguimiento específico**: Rastreo de materiales por vivienda
- ?? **Control de costos**: Importe total por casa
- ?? **Reportes personalizados**: Excel por casa individual
- ?? **Detección rápida**: Identifica discrepancias por casa

### Para el Proyecto
- ?? **Trazabilidad**: Seguimiento completo de materiales
- ?? **Transparencia**: Costos desglosados por casa
- ? **Control de calidad**: Verificación de insumos recibidos
- ?? **Análisis**: Comparación entre casas similares
- ?? **Auditoría**: Evidencia documentada por vivienda

---

## ?? Mejoras Futuras

### Propuestas de Funcionalidad
- [ ] Comparación lado a lado de dos casas
- [ ] Gráfica de distribución de costos por insumo
- [ ] Filtro por rango de fechas de entrada
- [ ] Indicador de completitud vs explosión de insumos
- [ ] Alertas de insumos faltantes
- [ ] Exportación a PDF
- [ ] Historial de cambios en la casa
- [ ] Fotos de entradas por casa
- [ ] Timeline visual de entradas
- [ ] Dashboard comparativo por manzana

### Optimizaciones Propuestas
- [ ] Caché de consultas frecuentes
- [ ] Precarga de lotes en segundo plano
- [ ] Paginación para casas con muchos insumos
- [ ] Índices adicionales en base de datos
- [ ] Compresión de archivos Excel grandes

---

## ? Checklist de Implementación

- [x] Pestaña agregada al TabControl
- [x] Controles creados (2 ComboBox, 1 DataGridView, 1 TextBox, 8 Labels, 2 Buttons)
- [x] Layout responsivo configurado
- [x] Tema corporativo aplicado
- [x] Lógica de consulta implementada (11 métodos)
- [x] Cascada Manzana ? Lote funcionando
- [x] Carga de insumos por casa operativa
- [x] Búsqueda en tiempo real implementada
- [x] Estadísticas calculándose correctamente
- [x] Exportación a Excel funcionando
- [x] Coloreo de filas por estado
- [x] Validaciones de selección
- [x] Manejo de errores completo
- [x] Mensajes descriptivos al usuario
- [x] Integración con evento de tabs
- [x] Documentación completa
- [x] Compilación exitosa
- [x] Testing básico aprobado

---

## ?? Notas de Desarrollo

### Patrón de Diseño
- **Clase parcial**: Mantiene modularización (`FormAlmacen_ConsultaCasa.cs`)
- **Eventos en cascada**: ComboBox ? ComboBox ? DataGridView
- **Filtrado reactivo**: TextChanged actualiza vista instantáneamente
- **Reutilización**: Usa métodos existentes (`EstilizarDataGridViewAlmacen()`)

### Dependencias
- **ClosedXML**: Exportación a Excel
- **System.Data.SqlClient**: Consultas a base de datos
- **ThemeManager**: Colores corporativos

### Compatibilidad
- **.NET Framework**: 4.7.2
- **SQL Server**: 2012+
- **Windows**: 7, 10, 11

---

## ?? Resultado Final

```
?????????????????????????????????????????????????????????????
?  PESTAÑA CONSULTA POR CASA - IMPLEMENTACIÓN COMPLETA      ?
?                                                            ?
?  ? Nueva pestaña agregada                                ?
?  ? 14 controles UI configurados                          ?
?  ? 11 métodos de negocio implementados                   ?
?  ? Búsqueda por Manzana/Lote funcional                   ?
?  ? Carga en cascada operativa                            ?
?  ? Estadísticas dinámicas por casa                       ?
?  ? Búsqueda en tiempo real dentro de casa               ?
?  ? Exportación Excel personalizada                       ?
?  ? Tema corporativo aplicado                             ?
?  ? Layout responsivo configurado                         ?
?  ? Documentación exhaustiva                              ?
?  ? Compilación exitosa                                   ?
?                                                            ?
?  ?? LISTO PARA PRODUCCIÓN                                 ?
?????????????????????????????????????????????????????????????
```

---

**Proyecto**: CALANDRIA RESIDENCIAL  
**Módulo**: FormAlmacen - Gestión de Almacén  
**Funcionalidad**: Consulta de Insumos por Casa  
**Fecha**: Enero 2025  
**Estado**: ? COMPLETADO  
**Versión**: 1.2 (Consulta por Casa agregada)  
**Archivos creados**: 1 nuevo (`FormAlmacen_ConsultaCasa.cs`)  
**Archivos modificados**: 3 (`Designer.cs`, `Layout.cs`, `Historial.cs`)  
**Líneas de código**: ~700 nuevas
