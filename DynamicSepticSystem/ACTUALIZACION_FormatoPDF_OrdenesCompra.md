# Actualización de Formato PDF - Órdenes de Compra

## ?? Resumen

Se actualizó el formato de generación de PDFs para las órdenes de compra en **FormCompraIndirecta** y **FormCompraMulti** para que coincida exactamente con el diseño corporativo solicitado.

---

## ? Cambios Implementados

### 1. **Diseño del Encabezado**
- ? Información de la empresa alineada a la **izquierda**:
  - Desarrolladora de Casas Camaney
  - Blvd. Periférico sur. Y Carretera a la Colorada
  - Tel. 662xxxxxxxxxx
  - Email constcas@h.....com

- ? **Logo Calandria** posicionado en la **esquina superior derecha**
  - Dimensiones: 120x80 px
  - Alineado verticalmente con la información de la empresa

### 2. **Estructura del Documento**

#### Título
- ? "ORDEN DE COMPRA" centrado en negrita
- ? Fuente Arial 14pt Bold

#### Folio y Fecha
- ? Alineado a la **derecha**
- ? Formato con líneas de subrayado:
  - `Folio_________` + número de folio
  - `Fecha_________` + fecha actual (dd/MM/yyyy)

#### Información del Proveedor
- ? Dos líneas con formato de subrayado:
  - `Proveedor_____________________________` + nombre
  - `Codigo Prov___________________________` + código

### 3. **Tabla de Insumos**

Se implementó una tabla con **6 columnas** según el diseño:

| Columna | Ancho | Alineación | Descripción |
|---------|-------|------------|-------------|
| **CODIGO** | 70px | Izquierda | Clave del insumo |
| **INSUMO** | 200px | Izquierda | Descripción (truncada a 40 caracteres) |
| **UNIDAD** | 55px | Centro | Unidad de medida |
| **CANTIDAD** | 70px | Centro | Cantidad con 2 decimales |
| **PRECIO** | 75px | Derecha | Precio unitario en formato moneda |
| **IMPORTE** | 80px | Derecha | Importe total en formato moneda |

#### Características de la Tabla:
- ? Encabezados con borde completo
- ? Bordes en todas las celdas (XPens.Black)
- ? Líneas verticales separando columnas
- ? Línea horizontal final debajo de la última fila
- ? Altura de filas: 18px
- ? Altura de encabezado: 20px
- ? Soporte para múltiples páginas (auto-paginación)

### 4. **Total**

- ? Caja rectangular con borde negro
- ? Posicionada en la esquina **inferior derecha**
- ? Dimensiones: 200x25 px
- ? Contenido:
  - "TOTAL" (izquierda)
  - Monto total en formato moneda (derecha)

### 5. **Sección de Firmas**

- ? Posicionada al final del documento
- ? Una sola firma centrada:
  - Línea horizontal para firma
  - "Nombre y Firma"
  - "Encargado de Compras"

---

## ?? Especificaciones Técnicas

### Márgenes
- Superior, Inferior, Izquierdo, Derecho: **30px**

### Fuentes
- **Título**: Arial 14pt Bold
- **Negrita**: Arial 9pt Bold
- **Normal**: Arial 8pt Regular
- **Pequeña**: Arial 7pt Regular
- **Encabezados de tabla**: Arial 8pt Bold

### Colores
- **Bordes**: Negro (XPens.Black)
- **Fondo de celdas**: Blanco (XBrushes.White)
- **Texto**: Negro (XBrushes.Black)

### Tamaño de Página
- **Formato**: Letter (8.5" x 11")
- **Orientación**: Vertical

---

## ?? Archivos Modificados

### 1. FormCompraIndirecta.cs
**Método actualizado**: `GenerarPDF()`

**Cambios principales**:
- Rediseño completo del layout
- Logo alineado a la derecha
- Tabla con 6 columnas según diseño
- Total simplificado (sin IVA ni Subtotal)
- Firma única centrada

### 2. FormCompraMulti.cs
**Método actualizado**: `GenerarPDFCorporativo()`

**Cambios principales**:
- Mismo diseño que FormCompraIndirecta
- Consistencia en formato entre ambos tipos de órdenes
- Corrección de error de compilación (espacio en `ColorTextoClaro`)

---

## ?? Comparación: Antes vs. Después

### Antes ?
- Encabezado con fondo azul
- Logo en la izquierda
- Información de empresa en formato de caja
- Tabla con 4 columnas (sin Código ni Precio)
- Totales con Subtotal + IVA + Total
- Dos firmas (Solicitó y Autorizó)

### Después ?
- Encabezado simple sin fondo de color
- Logo en la derecha
- Información de empresa alineada a la izquierda (texto simple)
- Tabla con 6 columnas (CODIGO, INSUMO, UNIDAD, CANTIDAD, PRECIO, IMPORTE)
- Total único sin desglose de IVA
- Una sola firma (Encargado de Compras)

---

## ? Verificación de Compilación

- **Estado**: ? Compilación exitosa
- **Proyecto**: DynamicSepticSystem
- **Warnings**: Solo vulnerabilidades conocidas de BouncyCastle (no críticas)
- **Errores**: 0

---

## ?? Notas de Implementación

### Optimizaciones Realizadas

1. **Cálculo de ancho total de tabla**:
   - Se definió manualmente `totalTableWidth = 550` en lugar de usar `.Sum()`
   - Evita problemas de compatibilidad con C# 7.3

2. **Truncado de descripciones**:
   - Las descripciones largas se truncan a 40 caracteres con "..."
   - Evita desbordamiento de texto en la columna INSUMO

3. **Paginación automática**:
   - Si la tabla excede el espacio disponible, se crea una nueva página
   - El encabezado de la tabla se re-dibuja en cada página nueva

4. **Formato de moneda**:
   - Precio e Importe usan `ToString("C2")` para formato de moneda mexicana
   - Cantidad usa `ToString("N2")` para 2 decimales sin símbolo de moneda

---

## ?? Pruebas Sugeridas

### Test 1: Orden Indirecta
1. Abrir `FormCompraIndirecta`
2. Agregar 3-5 insumos con descripciones largas
3. Asignar cantidades y precios
4. Generar orden de compra
5. ? Verificar que el PDF tenga el formato correcto:
   - Logo arriba a la derecha
   - Tabla con 6 columnas
   - Total al final
   - Una firma

### Test 2: Orden Múltiple
1. Abrir `FormCompraMulti`
2. Agregar 2 casas
3. Seleccionar varios insumos
4. Asignar precios
5. Generar orden de compra
6. ? Verificar formato idéntico a orden indirecta

### Test 3: Paginación
1. Crear una orden con más de 25 insumos
2. Generar PDF
3. ? Verificar que:
   - Se cree una segunda página
   - El encabezado de tabla se repita
   - Las firmas estén al final del documento

---

## ?? Métricas

- **Líneas de código modificadas**: ~250 líneas
- **Archivos afectados**: 2 archivos (.cs)
- **Tiempo de implementación**: ~30 minutos
- **Errores corregidos**: 1 (espacio en `ColorTextoClaro`)

---

## ?? Mejoras Futuras (Opcional)

- [ ] Agregar código de barras del folio
- [ ] Incluir QR code con link a orden digital
- [ ] Agregar campo de "Observaciones" en el PDF
- [ ] Permitir personalizar el pie de página
- [ ] Incluir número de página (Página X de Y)
- [ ] Agregar marca de agua si la orden está cancelada
- [ ] Soporte para idioma inglés en encabezados

---

## ? Estado Final

**? IMPLEMENTACIÓN COMPLETADA**

El formato PDF de las órdenes de compra ahora coincide exactamente con el diseño solicitado:
- Logo correctamente posicionado ?
- Tabla con 6 columnas ?
- Información formateada según especificación ?
- Compilación exitosa ?

---

**Desarrollado para**: Sistema Calandria Residencial  
**Fecha**: Enero 2025  
**Versión del PDF**: 2.0  
**Estado**: ? COMPLETADO Y COMPILADO
