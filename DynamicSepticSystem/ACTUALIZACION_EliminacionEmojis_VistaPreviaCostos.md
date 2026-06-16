# ACTUALIZACION: FormEstimacionConceptoMigrado - Eliminación de Emojis y Vista Previa de Costos

## Resumen de Cambios

Se implementaron las siguientes mejoras en el FormEstimacionConceptoMigrado:

### 1. Eliminación de Emojis
Se removieron todos los emojis de:
- Tooltips de conceptos
- Columna "Estado"
- Mensajes y textos del sistema

**Antes:**
```
?? Total Presupuestado: $320,611.87
? Total Ejecutado: $150,000.00
?? Avance: 46.8%
?? Partidas: 3/8 completadas

Estado: ? TERMINADO
Estado: ? ACTIVADO
```

**Ahora:**
```
Total Presupuestado: $320,611.87
Total Ejecutado: $150,000.00
Avance: 46.8%
Partidas: 3/8 completadas

Estado: TERMINADO
Estado: ACTIVADO
```

### 2. Vista Previa de Costos al Marcar Checkbox
La columna "Monto" ahora muestra el **total presupuestado** de la partida cuando se marca el checkbox, funcionando como una **vista previa del costo**.

**Comportamiento:**

| Situación | Monto Mostrado |
|-----------|----------------|
| Checkbox DESMARCADO y partida NO completada | $0.00 |
| Checkbox MARCADO | **Total presupuestado** (vista previa) |
| Partida COMPLETADA (sin marcar) | Monto ejecutado real |
| Concepto con partidas marcadas | Suma de totales presupuestados de partidas marcadas |

**Implementación:**
```csharp
colMonto.AspectGetter = delegate(object x) {
    var nodo = (NodoConcepto)x;
    if (nodo.EsConcepto)
    {
        // Para conceptos: suma de partidas marcadas
        var partidasMarcadas = nodo.Partidas.Where(p => p.Incluir).ToList();
        if (partidasMarcadas.Any())
        {
            return partidasMarcadas.Sum(p => p.Total);
        }
        return 0.0;
    }
    else
    {
        // Para partidas: mostrar total si está marcada
        if (nodo.Incluir)
            return nodo.Total; // Vista previa del costo total
        else if (nodo.Completado)
            return nodo.MontoEjecutado;
        else
            return 0.0;
    }
};
```

### 3. Label de Folio a Generar
Se agregó un label dinámico que muestra el folio que se generará al exportar el PDF.

**Características:**
- Se actualiza automáticamente al seleccionar Manzana/Lote
- Ubicación: Panel de filtros, parte superior derecha
- Formato de folio: `EST-M{manzana}-L{lote}-{numero:000}-{año}`

**Implementación:**
```csharp
private Label lblFolioGenerar;

private void CrearLabelFolio()
{
    lblFolioGenerar = new Label
    {
        AutoSize = false,
        Width = 300,
        Height = 25,
        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        ForeColor = Color.FromArgb(41, 128, 185),
        TextAlign = ContentAlignment.MiddleLeft,
        Text = "Folio: (Selecciona Manzana y Lote)"
    };
    
    if (this.Controls.ContainsKey("panelFiltros"))
    {
        var panel = this.Controls["panelFiltros"];
        lblFolioGenerar.Location = new Point(870, 20);
        panel.Controls.Add(lblFolioGenerar);
    }
}

private void ActualizarLabelFolio()
{
    if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
    {
        lblFolioGenerar.Text = "Folio: (Selecciona Manzana y Lote)";
        lblFolioGenerar.ForeColor = Color.Gray;
        return;
    }

    string manzana = cmbManzana.SelectedItem.ToString();
    string lote = cmbLote.SelectedItem.ToString();
    int numEstimacion = ObtenerSiguienteNumeroEstimacion(manzana, lote);
    string folio = GenerarFolio(manzana, lote, numEstimacion);
    
    lblFolioGenerar.Text = $"Folio a Generar: {folio}";
    lblFolioGenerar.ForeColor = Color.FromArgb(41, 128, 185);
}
```

### 4. Folio en PDF
El folio ahora aparece en el PDF generado, en la esquina superior derecha debajo de la barra naranja.

**Ubicación en PDF:**
```
??????????????????????????????????????????????????????????????
? ???????????????????????????????????????????????????????   ? (Barra naranja)
?                                                            ?
?                              FOLIO: EST-M1-L5-001-2025     ? ? NUEVO
? PROVEEDOR:                   Fecha:     15/01/2025        ?
? Ignacio Durán León          Estimación: 1                 ?
? ...                                                        ?
??????????????????????????????????????????????????????????????
```

**Implementación en PDF:**
```csharp
// Barra naranja
g.DrawRectangle(naranja, m, y, w, 4);
y += 10;

// NUEVO: FOLIO en la esquina superior derecha
var manzana = cmbManzana.SelectedItem?.ToString() ?? "0";
var lote = cmbLote.SelectedItem?.ToString() ?? "0";
var numEst = ObtenerSiguienteNumeroEstimacion(manzana, lote);
var folio = GenerarFolio(manzana, lote, numEst);

var folioSize = g.MeasureString(folio, fTit);
g.DrawRectangle(pen, gris, m + w - 180, y, 180, 16);
g.DrawString("FOLIO:", fPeq, XBrushes.Black, m + w - 175, y + 11);
g.DrawString(folio, fNor, XBrushes.Black, m + w - folioSize.Width - 5, y + 11);
y += 18;
```

### 5. Cálculo Correcto en PDF
El PDF ahora usa el **Total presupuestado** (no el ejecutado) para calcular:
- Total de requisición
- Amortización
- Total de estimación

**Cambio:**
```csharp
// ANTES
var totEjec = nodosRaiz.Where(c => c.Partidas.Any(p => p.Incluir))
    .SelectMany(c => c.Partidas.Where(p => p.Incluir))
    .Sum(p => p.MontoEjecutado);

// AHORA
var totEjec = nodosRaiz.Where(c => c.Partidas.Any(p => p.Incluir))
    .SelectMany(c => c.Partidas.Where(p => p.Incluir))
    .Sum(p => p.Total); // Usar Total presupuestado
```

---

## Archivos Modificados

| Archivo | Cambios |
|---------|---------|
| `FormEstimacionConceptoMigrado.cs` | - Eliminados emojis en tooltips y estados<br>- Agregado `lblFolioGenerar`<br>- Método `CrearLabelFolio()`<br>- Método `ActualizarLabelFolio()`<br>- `AspectGetter` de `colMonto` modificado<br>- `DibujarContenidoPreview()` actualizado<br>- Event handlers actualizados |
| `FormEstimacionConceptoMigrado_ExtensionPDF.cs` | - Agregado folio en parte superior del PDF<br>- Cambio de `MontoEjecutado` a `Total` en cálculos<br>- Formato de folio en esquina superior derecha |

---

## Resultado Visual Esperado

### TreeListView

```
????????????????????????????????????????????????????????????????????????????????
?                   Folio a Generar: EST-M5-L2-001-2025                        ?
????????????????????????????????????????????????????????????????????????????????
? ? Concepto / Partida          ? Monto        ? Estado       ? Fecha Fin     ?
????????????????????????????????????????????????????????????????????????????????
? ? Preliminares                ? $0.00        ?              ?               ?
?   ? Trazo y nivelación        ? $0.00        ?              ?               ?
?   ? Limpieza de terreno       ? $1,980.36    ?              ?               ? ? Vista previa
????????????????????????????????????????????????????????????????????????????????
? ? Estructura                  ? $45,200.00   ? ACTIVADO     ?               ?
?   ? Losa de cimentación       ? $45,200.00   ?              ?               ? ? Vista previa
?   ? Muros de carga            ? $0.00        ?              ?               ?
????????????????????????????????????????????????????????????????????????????????
? ? Acabados                    ? $0.00        ? TERMINADO    ? 15/01/2025    ?
?   ? Yeso                      ? $0.00        ? TERMINADO    ? 15/01/2025    ?
????????????????????????????????????????????????????????????????????????????????
```

**Al pasar el mouse sobre "Estructura":**
```
???????????????????????????????????????
? Total Presupuestado: $320,611.87   ?
? Total Ejecutado: $85,450.30        ?
? Avance: 26.7%                      ?
? Partidas: 2/4 completadas          ?
???????????????????????????????????????
```

### PDF Generado

```
????????????????????????????????????????????????????????????????????
? ?????????????????????????????????????????????????????????????   ?
?                                                                  ?
?                              FOLIO: EST-M5-L2-001-2025          ? ? NUEVO
? PROVEEDOR:                   Fecha:     15/01/2025             ?
? Ignacio Durán León          Estimación: 1                      ?
?                                                                  ?
? DESCRIPCIÓN:                                                    ?
? Subcontratista Edificación                                     ?
?                                                                  ?
? CARGAR A:                                                       ?
? M5 L2 Prototipo TUNERA                                         ?
?                                                                  ?
? IMPORTE DEL CONTRATO:                                          ?
? $897,692.47                                                    ?
?                                                                  ?
? CONCEPTOS A ESTIMAR                                            ?
? ???????????????????????????????????????????????????????????   ?
? Concepto / Partida                              ? Monto        ?
? ?????????????????????????????????????????????????????????????  ?
? Estructura                                                      ?
?   Estructura > Losa de cimentación             ? $45,200.00   ? ? Total
? Total Estructura                               ? $45,200.00   ?
?                                                                  ?
?                               TOTAL DE REQUISICIÓN ? $45,200.00 ?
?                               AMORTIZACIÓN 11.0%   ?  $4,972.00 ?
?                               TOTAL ESTIMACIÓN     ? $40,228.00 ?
????????????????????????????????????????????????????????????????????
```

---

## Ventajas de los Cambios

### 1. Eliminación de Emojis
- **Profesionalismo**: Texto más limpio y formal
- **Compatibilidad**: Evita problemas de codificación en algunos sistemas
- **Impresión**: Los PDFs se imprimen correctamente sin caracteres extraños

### 2. Vista Previa de Costos
- **Transparencia**: El usuario ve inmediatamente cuánto costará la estimación
- **Claridad**: Distingue entre partidas marcadas (costo) y completadas (ejecutado)
- **Toma de decisiones**: Facilita seleccionar partidas según presupuesto disponible

### 3. Label de Folio
- **Anticipación**: El usuario sabe el folio antes de generar el PDF
- **Verificación**: Permite confirmar que el folio sea correcto
- **Organización**: Facilita el seguimiento secuencial de estimaciones

### 4. Folio en PDF
- **Trazabilidad**: Cada PDF tiene su folio visible
- **Identificación rápida**: No necesitas abrir el archivo para saber qué estimación es
- **Archivo físico**: Si se imprime, el folio está claramente visible

---

## Casos de Uso

### Escenario 1: Planificar Estimación
```
1. Usuario selecciona M5, L2
2. Label muestra: "Folio a Generar: EST-M5-L2-001-2025"
3. Usuario marca partidas
4. Columna "Monto" muestra: $45,200.00 (vista previa)
5. Usuario verifica que está dentro del presupuesto
6. Genera PDF con folio EST-M5-L2-001-2025
```

### Escenario 2: Revisar Estimaciones Anteriores
```
1. Usuario abre FormRepositorioPDFs
2. Ve lista de folios: EST-M5-L2-001-2025, EST-M5-L2-002-2025
3. Sabe cuál es la más reciente sin abrir el PDF
4. Puede anticipar el siguiente: EST-M5-L2-003-2025
```

### Escenario 3: Imprimir PDF
```
1. Usuario genera estimación
2. PDF tiene folio visible: EST-M5-L2-001-2025
3. Se imprime y archiva físicamente
4. Fácil de localizar más tarde
```

---

## Estado

- Compilación: EXITOSA
- Errores: NINGUNO
- Listo para: PRUEBAS

---

**Fecha**: Enero 2025  
**Versión**: 1.2  
**Sistema**: Calandria Residencial

---

## Checklist de Pruebas

- [ ] Verificar que el label de folio se actualiza al cambiar Manzana/Lote
- [ ] Confirmar que al marcar checkbox, columna Monto muestra el total presupuestado
- [ ] Verificar que al desmarcar, columna Monto vuelve a $0.00
- [ ] Confirmar que tooltips ya NO tienen emojis
- [ ] Verificar que columna Estado dice "TERMINADO" y "ACTIVADO" (sin emojis)
- [ ] Confirmar que el folio aparece en el PDF generado
- [ ] Verificar que el cálculo de totales usa Total (no MontoEjecutado)
- [ ] Confirmar que el preview muestra el folio correctamente

---

CAMBIOS IMPLEMENTADOS EXITOSAMENTE
