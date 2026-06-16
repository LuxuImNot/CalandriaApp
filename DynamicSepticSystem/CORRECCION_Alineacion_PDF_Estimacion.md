# ? CORRECCIÓN: Alineación de Texto en PDF - FormEstimacionConceptoMigrado

## ? Problema

El texto en el PDF generado no estaba alineado correctamente. Los textos aparecían desalineados verticalmente y algunos se superponían con los bordes de las celdas.

### **Antes:**
```
???????????????????????
?PROVEEDOR:           ?  ? Texto muy arriba
?Ignacio Durán        ?  ? Pegado al borde superior
???????????????????????
?DESCRIPCIÓN:         ?  ? Desalineado
?Subcontratista       ?  ? Texto cortado
???????????????????????
```

---

## ? Solución

Usar **XStringFormats** correctamente con `XRect` para controlar el alineamiento vertical y horizontal de todos los textos en el PDF.

### **Cambios Realizados:**

#### **1. Alineación con XRect + XStringFormats**

```csharp
// ANTES (texto desalineado):
gfx.DrawString("PROVEEDOR:", fontHeader, XBrushes.Black, margenIzq + 3, y + 4);

// DESPUÉS (texto correctamente alineado):
gfx.DrawString("PROVEEDOR:", fontHeader, XBrushes.Black, 
    new XRect(margenIzq + 3, y + 2, colIzqWidth - 6, altoCelda), 
    XStringFormats.CenterLeft);  // Centrado verticalmente, alineado a la izquierda
```

#### **2. XStringFormats Usados**

| XStringFormat | Uso | Descripción |
|---------------|-----|-------------|
| **XStringFormats.CenterLeft** | Headers, labels, nombres | Centrado verticalmente, alineado a la izquierda |
| **XStringFormats.CenterRight** | Montos, cantidades | Centrado verticalmente, alineado a la derecha |
| **XStringFormats.Center** | Porcentajes, títulos centrados | Centrado vertical y horizontal |
| **XStringFormats.TopCenter** | Firmas, texto centrado arriba | Arriba y centrado horizontalmente |

---

## ?? Detalles de Implementación

### **Sección: Encabezados (PROVEEDOR, DESCRIPCIÓN, etc.)**

```csharp
// Headers con fondo gris
gfx.DrawRectangle(brushGris, margenIzq, y, colIzqWidth, altoCelda);
gfx.DrawString("PROVEEDOR:", fontHeader, XBrushes.Black, 
    new XRect(margenIzq + 3, y + 2, colIzqWidth - 6, altoCelda), 
    XStringFormats.CenterLeft);

// Valores con fondo blanco
y += altoCelda + 2;
gfx.DrawRectangle(XBrushes.White, margenIzq, y, colIzqWidth, altoCelda);
gfx.DrawRectangle(penLinea, margenIzq, y, colIzqWidth, altoCelda);
string proveedor = !string.IsNullOrWhiteSpace(txtProveedor.Text) 
    ? txtProveedor.Text 
    : "Ignacio Durán León";
gfx.DrawString(proveedor, fontNormal, XBrushes.Black, 
    new XRect(margenIzq + 3, y + 2, colIzqWidth - 6, altoCelda), 
    XStringFormats.CenterLeft);
```

### **Sección: Tabla de Conceptos**

```csharp
// Encabezados de tabla
gfx.DrawString("Concepto / Partida", fontHeader, XBrushes.Black, 
    new XRect(margenIzq + 3, y + 3, colNombreWidth - 6, 16), 
    XStringFormats.CenterLeft);

gfx.DrawString("Monto", fontHeader, XBrushes.Black, 
    new XRect(margenIzq + colNombreWidth + 3, y + 3, colMontoWidth - 8, 16), 
    XStringFormats.CenterRight);  // Alineado a la derecha

gfx.DrawString("Avance", fontHeader, XBrushes.Black, 
    new XRect(margenIzq + colNombreWidth + colMontoWidth, y + 3, colPorcentajeWidth, 16), 
    XStringFormats.Center);  // Centrado
```

### **Sección: Partidas**

```csharp
// Nombre de partida (izquierda)
gfx.DrawString("  " + nombrePartida, fontPequena, XBrushes.Black, 
    new XRect(margenIzq + 8, y + 1, colNombreWidth - 16, 13), 
    XStringFormats.CenterLeft);

// Monto ejecutado (derecha)
gfx.DrawString(partida.MontoEjecutado.ToString("C2"), fontPequena, XBrushes.Black,
    new XRect(margenIzq + colNombreWidth + 3, y + 1, colMontoWidth - 8, 13), 
    XStringFormats.CenterRight);

// Porcentaje de avance (centro)
gfx.DrawString($"{partida.AvancePorcentaje:F0}%", fontPequena, XBrushes.Black,
    new XRect(margenIzq + colNombreWidth + colMontoWidth, y + 1, colPorcentajeWidth, 13), 
    XStringFormats.Center);
```

### **Sección: Totales (Requisición, Amortización, Estimación)**

```csharp
// Label (izquierda)
gfx.DrawString("TOTAL DE REQUISICIÓN", fontNegrita, XBrushes.Black, 
    new XRect(margenIzq + 173, y + 2, 127, 14), 
    XStringFormats.CenterLeft);

// Monto (derecha)
gfx.DrawString(totalEjecutadoSeleccionado.ToString("C2"), fontNegrita, XBrushes.Black,
    new XRect(margenIzq + 303, y + 2, 114, 14), 
    XStringFormats.CenterRight);
```

---

## ?? Parámetros de XRect

```csharp
new XRect(x, y, width, height)
```

| Parámetro | Descripción | Uso Típico |
|-----------|-------------|------------|
| **x** | Posición horizontal inicial | `margenIzq + 3` (con padding de 3) |
| **y** | Posición vertical inicial | `y + 2` (con offset de 2 para centrado) |
| **width** | Ancho del rectángulo | `colWidth - 6` (restando padding izq+der) |
| **height** | Alto del rectángulo | `altoCelda` o `13` (altura de la fila) |

---

## ? Resultado

```
????????????????????????????????????
? CONCEPTOS A ESTIMAR               ?  ? Centrado vertical
????????????????????????????????????
??????????????????????????????????????
? Concepto / Part ?  Monto  ? Avance ?  ? Headers centrados
??????????????????????????????????????
? Cimentación     ?         ?        ?
?   Cimbra y acero?$100,000 ?  80%   ?  ? Alineado correctamente
??????????????????????????????????????
```

---

**Estado:** ? Completado  
**Build:** ? Exitoso  
**Fecha:** 2025-01-11
