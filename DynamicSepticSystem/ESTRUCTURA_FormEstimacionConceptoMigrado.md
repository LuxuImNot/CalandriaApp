# Estructura de FormEstimacionConceptoMigrado

## Descripción

El formulario `FormEstimacionConceptoMigrado` ha sido dividido en múltiples archivos parciales (`partial class`) para facilitar el mantenimiento y la edición del código.

## Archivos y sus responsabilidades

### 1. FormEstimacionConceptoMigrado.cs (Principal)
**Responsabilidad:** Constructor, campos privados e inicialización básica.

Contenido:
- Campos privados (connectionString, nodosRaiz, prototipoActual, etc.)
- Constructor
- `CrearLabelFolio()`
- `ActualizarLabelFolio()`
- `FormEstimacionConcepto_Load()`

---

### 2. FormEstimacionConceptoMigrado.MenuContextual.cs
**Responsabilidad:** Menú contextual de clic derecho para partidas.

Contenido:
- `ConfigurarMenuContextual()`
- `ContextMenuPartidas_Opening()`
- `MenuCapturarM2_Click()`
- `MostrarDialogoCapturarM2()`
- `MenuMarcar_Click()`
- `MenuDetalles_Click()`
- `MenuResetear_Click()`

---

### 3. FormEstimacionConceptoMigrado.TreeListView.cs
**Responsabilidad:** Configuración del TreeListView y sus columnas.

Contenido:
- `ConfigurarTreeListView()`
- `ConfigurarColumnasTreeListView()`
- `ConfigurarEventosFormato()`
- `MostrarDialogoM2EnCheck()`

---

### 4. FormEstimacionConceptoMigrado.CargaDatos.cs
**Responsabilidad:** Carga de datos desde la base de datos.

Contenido:
- `CargarManzanas()`
- `cmbManzana_SelectedIndexChanged()`
- `CargarLotes()`
- `cmbLote_SelectedIndexChanged()`
- `CargarEstimacionJerarquica()`
- `CargarConceptos()`
- `CargarPartidasConDinamicas()`
- `CrearNodoPartidaDinamica()`
- `CargarAvancesPartidas()`
- `NormalizeForComparison()`

---

### 5. FormEstimacionConceptoMigrado.Preview.cs
**Responsabilidad:** Vista previa del PDF en el formulario.

Contenido:
- `OnDatosChanged()`
- `TimerPreview_Tick()`
- `ActualizarPreviewPDF()`
- `GenerarPDFPreview()`
- `GenerarImagenDesdePDFReal()`
- `DibujarContenidoPreview()`

---

### 6. FormEstimacionConceptoMigrado.Avances.cs
**Responsabilidad:** Manejo de avances, totales y guardado en BD.

Contenido:
- `EncontrarPadre()`
- `RecalcularAvanceConcepto()`
- `ActualizarTotales()`
- `GuardarAvancePartidaEnBD()`
- `btnCargarAvance_Click()`
- `btnGuardarYExportar_Click()`
- `btnMarcarTodos_Click()`
- `btnDesmarcarTodos_Click()`

---

### 7. FormEstimacionConceptoMigrado.AgregarConcepto.cs (Existente)
**Responsabilidad:** Funcionalidad para agregar nuevos conceptos.

Contenido:
- `btnAgregarConcepto_Click()`
- `btnGestionarPartidas_Click()`
- `CargarConceptosExistentesParaSelector()`
- `CalcularNuevoCodigoConcepto()`
- `RenumerarConceptosPosteriores()`
- `GuardarNuevoConceptoEnBD()`

---

### 8. FormEstimacionConceptoMigrado_ExtensionFolios.cs (Existente)
**Responsabilidad:** Manejo de folios y generación de PDFs.

Contenido:
- `ObtenerSiguienteNumeroEstimacion()`
- `GenerarFolio()`
- `GuardarFolioEstimacion()`
- `GuardarDetalleFolio()`
- `GuardarPDFEnBD()`
- `btnRepositorioPDFs_Click()`
- `CrearTablasSiNoExisten()`
- `ActualizarAvancesA100PorCiento()`
- `GenerarPDFAvance()`
- `DibujarContenidoPDF()`

---

### 9. FormEstimacionConceptoMigrado_ExtensionPDF.cs (Existente)
**Responsabilidad:** Formato y diseño del PDF.

Contenido:
- `DibujarPDFNuevoFormato()`
- `DibujarFirmasPDF()`

---

### 10. FormEstimacionConceptoMigrado_ExtensionEvidencias.cs (Existente)
**Responsabilidad:** Manejo de evidencias fotográficas.

Contenido:
- `btnSeleccionarEvidencias_Click()`
- `ActualizarLabelEvidencias()`
- `DibujarEvidenciasPDF()`
- `LimpiarEvidenciasSeleccionadas()`
- Clase `FormSeleccionEvidenciasPDF`

---

## Diagrama de dependencias

```
FormEstimacionConceptoMigrado.cs (Principal)
    ?
    ??? FormEstimacionConceptoMigrado.Designer.cs (Generado automáticamente)
    ?
    ??? FormEstimacionConceptoMigrado.MenuContextual.cs
    ?
    ??? FormEstimacionConceptoMigrado.TreeListView.cs
    ?
    ??? FormEstimacionConceptoMigrado.CargaDatos.cs
    ?
    ??? FormEstimacionConceptoMigrado.Preview.cs
    ?
    ??? FormEstimacionConceptoMigrado.Avances.cs
    ?
    ??? FormEstimacionConceptoMigrado.AgregarConcepto.cs
    ?
    ??? FormEstimacionConceptoMigrado_ExtensionFolios.cs
    ?
    ??? FormEstimacionConceptoMigrado_ExtensionPDF.cs
    ?
    ??? FormEstimacionConceptoMigrado_ExtensionEvidencias.cs
```

## Cómo agregar nueva funcionalidad

1. **Identifica la categoría** de la funcionalidad que deseas agregar.
2. **Abre el archivo parcial correspondiente** según la responsabilidad.
3. **Agrega el nuevo método** manteniendo la consistencia con el resto del código.
4. Si necesitas crear una **nueva categoría**, crea un nuevo archivo con el patrón:
   - `FormEstimacionConceptoMigrado.NuevaCategoria.cs`

## Notas importantes

- Todos los archivos usan `partial class FormEstimacionConceptoMigrado`.
- Los campos privados se definen **solo** en el archivo principal.
- El archivo `.Designer.cs` no debe modificarse manualmente.
- Los métodos de eventos de botones deben estar vinculados en el Designer.

## Fecha de reorganización
- **Fecha:** $(date)
- **Archivos creados:** 6 nuevos archivos parciales
- **Motivo:** Facilitar edición y mantenimiento del código
