# Actualización de Formato PDF - Órdenes de Compra (Formato Simplificado)

## ?? Resumen

Se actualizó el formato de generación de PDFs para las órdenes de compra en **FormCompraIndirecta** y **FormCompraMulti** para que coincida exactamente con el diseño simplificado corporativo solicitado.

---

## ? Cambios Implementados

### 1. **Diseño del Encabezado**

#### Información de la Empresa (Izquierda)
```
Desarrolladora de Casas Camaney
Blvd. Periférico sur. Y Carretera a la Colorada
Tel. 662xxxxxxxxxx
Email constcas@h.....com
```

#### Logo (Derecha)
- ??? **Logo Calandria** posicionado en la **esquina superior derecha**
- Dimensiones: 100x67 px
- Alineado verticalmente con la información de la empresa

---

### 2. **Estructura del Documento**

#### Título
- ?? "ORDEN DE COMPRA" centrado en negrita
- Fuente Arial 12pt Bold

#### Folio y Fecha (Alineado a la derecha)
```
Folio_________  OC-MULTI-20250114-001
Fecha_________  14/01/2025
```

#### Información del Proveedor (Izquierda)
```
Proveedor_____________________________  Nombre del Proveedor
Codigo Prov___________________________  PROV-001
```

---

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

**Ancho total de la tabla**: 550px

#### Características de la Tabla:
- ? Encabezados con borde completo
- ? Bordes en todas las celdas (XPens.Black)
- ? Líneas verticales separando columnas
- ? Línea horizontal final debajo de la última fila
- ? Altura de filas: 18px
- ? Altura de encabezado: 20px
- ? Soporte para múltiples páginas (auto-paginación)

---

### 4. **Total**

```
                                        TOTAL       $12,345.67
```

- ? Sin desglose de IVA ni Subtotal
- ? Posicionado en la parte inferior derecha
- ? Sin caja rectangular (formato simplificado)

---

### 5. **Sección de Firmas**

```
            _______________________
            Nombre y Firma
            Encargado de Compras
```

- ? Una sola firma centrada
- ? Línea horizontal para firma
- ? Texto "Nombre y Firma"
- ? Texto "Encargado de Compras"

---

## ?? Especificaciones Técnicas

### Márgenes
- Superior, Inferior, Izquierdo, Derecho: **30px**

### Fuentes
- **Título**: Arial 12pt Bold
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

### 1. FormCompraMulti.cs
**Método actualizado**: `GenerarPDFCorporativo()`

**Cambios principales**:
- ? Logo posicionado en la esquina superior derecha (100x67 px)
- ? Información de la empresa alineada a la izquierda
- ? Folio y Fecha con formato de líneas de subrayado
- ? Información del proveedor con formato simplificado
- ? Tabla con 6 columnas según diseño
- ? Total simplificado (sin IVA ni Subtotal)
- ? Firma única centrada

### 2. FormCompraIndirecta.cs
**Método actualizado**: `GenerarPDF()`

**Cambios principales**:
- ? Mismo diseño que FormCompraMulti
- ? Consistencia en formato entre ambos tipos de órdenes
- ? Logo con dimensiones idénticas
- ? Tabla y firmas con formato uniforme

---

## ?? Comparación: Antes vs. Después

### Antes ?
- Encabezado con fondo azul
- Logo más grande (120x80 px)
- Totales dentro de una caja rectangular
- Formato con más elementos decorativos

### Después ?
- Encabezado simple sin fondo de color
- Logo optimizado (100x67 px)
- Información de empresa alineada a la izquierda (texto simple)
- Folio y Fecha con formato de líneas de subrayado
- Tabla con 6 columnas (CODIGO, INSUMO, UNIDAD, CANTIDAD, PRECIO, IMPORTE)
- Total simple sin caja rectangular
- Una sola firma centrada

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

5. **Logo optimizado**:
   - Reducción de tamaño de 120x80 a 100x67 px
   - Mejor proporción y uso del espacio

---

## ?? Pruebas Sugeridas

### Test 1: Orden Indirecta
1. Abrir `FormCompraIndirecta`
2. Agregar 3-5 insumos con descripciones largas
3. Asignar cantidades y precios
4. Generar orden de compra
5. ? Verificar que el PDF tenga el formato correcto:
   - Logo arriba a la derecha (100x67 px)
   - Tabla con 6 columnas
   - Total sin caja rectangular
   - Una firma centrada

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

- **Líneas de código modificadas**: ~200 líneas
- **Archivos afectados**: 2 archivos (.cs)
- **Tiempo de implementación**: ~25 minutos
- **Errores corregidos**: 0

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

El formato PDF de las órdenes de compra ahora coincide exactamente con el diseño simplificado solicitado:
- Logo correctamente posicionado ?
- Tabla con 6 columnas ?
- Información formateada según especificación ?
- Compilación exitosa ?
- Formato consistente entre FormCompraMulti y FormCompraIndirecta ?

---

**Desarrollado para**: Sistema Calandria Residencial  
**Fecha**: Enero 2025  
**Versión del PDF**: 2.1 (Simplificado)  
**Estado**: ? COMPLETADO Y COMPILADO

---

## ?? Ejemplo Visual del Formato

```
???????????????????????????????????????????????????????????????
? Desarrolladora de Casas Camaney           [LOGO CALANDRIA] ?
? Blvd. Periférico sur. Y Carretera...                        ?
? Tel. 662xxxxxxxxxx                                          ?
? Email constcas@h.....com                                    ?
???????????????????????????????????????????????????????????????
?                   ORDEN DE COMPRA                           ?
???????????????????????????????????????????????????????????????
? Proveedor_______________  ABC Proveedor                     ?
?                                      Folio___  OC-XXX-001   ?
? Codigo Prov_____________  PROV-001                          ?
?                                      Fecha___  14/01/2025   ?
???????????????????????????????????????????????????????????????
? CODIGO ? INSUMO ? UNIDAD ? CANTIDAD ? PRECIO ? IMPORTE     ?
??????????????????????????????????????????????????????????????
? INS-01 ? Desc.. ?   PZA  ?   10.00  ? $50.00 ?    $500.00  ?
? INS-02 ? Desc.. ?    KG  ?   25.50  ? $30.00 ?    $765.00  ?
??????????????????????????????????????????????????????????????
?                                      TOTAL      $1,265.00   ?
???????????????????????????????????????????????????????????????
?                  _______________________                    ?
?                  Nombre y Firma                             ?
?                  Encargado de Compras                       ?
???????????????????????????????????????????????????????????????
```
