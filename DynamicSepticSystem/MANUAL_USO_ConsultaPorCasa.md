# ?? MANUAL DE USO - Pestaña CONSULTA POR CASA

## ?? Introducción

La nueva pestaña **CONSULTA POR CASA** en el módulo de Gestión de Almacén te permite ver **todos los insumos** que han sido asignados a una casa específica mediante la selección de su Manzana y Lote.

---

## ?? Cómo Acceder

### Paso 1: Abrir el Módulo de Almacén
1. Desde el **Panel Principal**
2. Click en el menú **"ALMACÉN"**
3. Seleccionar **"Gestión de Almacén"**

### Paso 2: Seleccionar la Pestaña
- Click en la pestaña **"CONSULTA POR CASA"** (cuarta pestaña)
- El sistema cargará automáticamente las manzanas disponibles

---

## ?? Interfaz de Usuario

```
???????????????????????????????????????????????????????????????
?  CONSULTA POR CASA                                          ?
???????????????????????????????????????????????????????????????
?                                                              ?
?  MANZANA: [? Combo]  LOTE: [? Combo]  ?? BUSCAR: [______] ?
?                                                              ?
?  ?? Casa seleccionada                ?? Estadísticas       ?
?                                                              ?
?  ????????????????????????????????????????????????????????  ?
?  ?  [Tabla de Insumos]                                  ?  ?
?  ?                                                       ?  ?
?  ?  Fecha ? Estado ? Folio ? Clave ? Descripción ? ...  ?  ?
?  ?  ?????????????????????????????????????????????????????  ?
?  ?  Data rows...                                        ?  ?
?  ????????????????????????????????????????????????????????  ?
?                                                              ?
?  [?? ACTUALIZAR]                    [?? EXPORTAR A EXCEL]  ?
???????????????????????????????????????????????????????????????
```

---

## ?? Guía Paso a Paso

### Consultar Insumos de una Casa

#### 1. Seleccionar Manzana
- Click en el combo **"MANZANA"**
- Aparecerá lista de manzanas disponibles
- Seleccionar la manzana deseada (ej: "5")

**Resultado**: Se cargan automáticamente los lotes de esa manzana

#### 2. Seleccionar Lote
- Click en el combo **"LOTE"** (ahora habilitado)
- Aparecerá lista de lotes de la manzana seleccionada
- Seleccionar el lote deseado (ej: "12")

**Resultado**: Se cargan todos los insumos de esa casa

#### 3. Ver Resultados

La pantalla mostrará:

**Panel de Información (Superior)**:
- ?? **Casa**: "CASA: Manzana 5 - Lote 12"
- ??? **Prototipo**: "Prototipo: TUNERA"
- ?? **Total insumos**: "Total de insumos: 45"
- ?? **Importe total**: "Importe total: $125,450.00"
- ?? **Discrepancias**: "Con discrepancia: 2" (si aplica)

**Tabla de Insumos**:
- Lista completa de todos los insumos asignados
- Información detallada por cada entrada
- Colores indicadores de estado

---

## ?? Buscar Insumo Específico

### Dentro de la Casa Seleccionada

1. **Asegúrate de tener una casa seleccionada** (Manzana + Lote)
2. En el campo **"?? BUSCAR:"**, escribe el término a buscar
   - Ejemplo: "cemento", "varilla", "arena"
3. La tabla se filtrará automáticamente en tiempo real
4. Solo se mostrarán insumos que coincidan con tu búsqueda

**El filtro busca en**:
- ? Clave del insumo
- ? Descripción
- ? Folio de orden de compra
- ? Prototipo
- ? Usuario que registró
- ? Justificación

### Ejemplo:
```
Casa seleccionada: Manzana 5, Lote 12
Buscar: "cemento"

Resultado: Muestra solo las 3 entradas de cemento 
para esa casa específica.
```

---

## ?? Exportar a Excel

### Generar Reporte de Insumos de una Casa

1. **Selecciona la casa** (Manzana + Lote)
2. Click en botón **"?? EXPORTAR A EXCEL"** (esquina inferior derecha)
3. Se abrirá ventana para guardar archivo
4. **Nombre sugerido**: `Insumos_Casa_M5_L12_20250115_143000.xlsx`
5. Selecciona ubicación donde guardar
6. Click en **"Guardar"**
7. Mensaje de confirmación aparecerá
8. **¿Desea abrir el archivo?**
   - **Sí**: Abre el Excel automáticamente
   - **No**: Archivo guardado, puedes abrirlo después

### Contenido del Excel

El archivo incluye:
- ?? **Encabezado corporativo**: "INSUMOS POR CASA - CALANDRIA RESIDENCIAL"
- ?? **Información de casa**: "Casa: Manzana 5 - Lote 12"
- ?? **Fecha de generación**: "Generado: 15/01/2025 14:30"
- ?? **Tabla completa** con 13 columnas:
  - Fecha
  - Estado
  - Folio OC
  - Clave
  - Descripción
  - Unidad
  - Cantidad
  - Precio Unitario
  - Importe
  - Prototipo
  - Usuario
  - Excepción
  - Justificación

---

## ?? Actualizar Datos

### Refrescar Información de la Casa

Si otro usuario ha registrado nuevas entradas:

1. Asegúrate de tener una casa seleccionada
2. Click en botón **"?? ACTUALIZAR"** (esquina inferior izquierda)
3. El sistema recargará los datos desde la base de datos
4. Mensaje de confirmación: "? Consulta actualizada correctamente"

---

## ?? Código de Colores

### Estados Visuales

| Color de Fondo | Significado | Explicación |
|----------------|-------------|-------------|
| ?? **Verde claro** | COMPLETO | Entrada sin problemas, cantidad correcta |
| ?? **Rojo claro** | DISCREPANCIA | Hay diferencia entre lo ordenado y recibido |

### En la Columna "Estado"
- ? **"COMPLETO"** (verde): Todo en orden
- ?? **"DISCREPANCIA"** (rojo): Revisar justificación

---

## ?? Panel de Estadísticas

### Información Mostrada

#### Casa Seleccionada
```
?? CASA: Manzana 5 - Lote 12
```
- Muestra la casa que estás consultando
- Formato en **negrita y color corporativo**

#### Prototipo
```
??? Prototipo: TUNERA
```
- Modelo de la casa
- Extraído automáticamente de los insumos

#### Total de Insumos
```
?? Total de insumos: 45
```
- Contador de entradas registradas para esta casa

#### Importe Total
```
?? Importe total: $125,450.00
```
- Suma de todos los importes de insumos
- Formato de moneda con dos decimales

#### Discrepancias (Condicional)
```
?? Con discrepancia: 2
```
- Solo aparece si hay entradas con problemas
- Color naranja/rojo para llamar la atención

---

## ?? Información de la Tabla

### Columnas Disponibles

1. **Fecha Entrada** (130px)
   - Fecha y hora de registro
   - Formato: dd/MM/yyyy HH:mm
   - Ejemplo: "15/01/2025 14:30"

2. **Estado** (100px)
   - COMPLETO o DISCREPANCIA
   - Colorea la fila completa
   - Texto en negrita

3. **Folio OC** (120px)
   - Número de orden de compra
   - Ejemplo: "OC-001-2025"

4. **Clave** (100px)
   - Código del insumo
   - Ejemplo: "CEM-001"

5. **Descripción** (300px)
   - Nombre completo del material
   - Ejemplo: "Cemento Portland Gris 50kg"

6. **Unidad** (70px)
   - Unidad de medida
   - Ejemplo: "BULTO", "M3", "PIEZA"

7. **Cantidad** (90px)
   - Cantidad recibida
   - Formato numérico: 0.00
   - Centrado

8. **P.U.** (90px)
   - Precio Unitario
   - Formato moneda: $0.00
   - Alineado a la derecha

9. **Importe** (100px)
   - Cantidad × Precio Unitario
   - Formato moneda: $0.00
   - Alineado a la derecha

10. **Prototipo** (120px)
    - Modelo de la casa
    - Ejemplo: "TUNERA", "COCUYO"

11. **Usuario** (100px)
    - Quién registró la entrada
    - Ejemplo: "admin", "almacen1"

12. **Excepción** (80px)
    - True/False
    - Indica si hay discrepancia
    - Centrado

13. **Justificación** (220px)
    - Explicación de discrepancias
    - Solo tiene valor si hay excepción
    - Ejemplo: "Proveedor envió 48 en lugar de 50"

---

## ?? Mensajes del Sistema

### Mensajes de Validación

#### "Seleccione una casa para ver sus insumos"
- **Cuándo aparece**: Al abrir la pestaña por primera vez
- **Qué hacer**: Seleccionar manzana y lote

#### "-- Seleccionar --"
- **Cuándo aparece**: Opción por defecto en combos
- **Qué hacer**: Elegir una opción real de la lista

#### "?? No hay datos para exportar"
- **Cuándo aparece**: Intentas exportar sin seleccionar casa
- **Qué hacer**: Primero selecciona manzana y lote

#### "?? Debe seleccionar una manzana y lote primero"
- **Cuándo aparece**: Intentas actualizar sin selección
- **Qué hacer**: Selecciona casa completa (manzana + lote)

### Mensajes de Éxito

#### "? Consulta actualizada correctamente"
- Datos refrescados exitosamente

#### "? Insumos exportados exitosamente"
- Excel generado sin problemas
- Incluye opción de abrir archivo

### Mensajes de Error

#### "? Error al cargar manzanas"
- Problema con base de datos o conexión
- Contactar soporte técnico

#### "? Error al cargar lotes"
- Problema al consultar lotes de la manzana
- Verificar conexión

#### "? Error al cargar insumos"
- Problema al traer datos de la casa
- Verificar conexión o contactar soporte

#### "? Error al exportar a Excel"
- Problema al generar archivo
- Posibles causas:
  - Sin permisos de escritura en carpeta destino
  - Disco lleno
  - Archivo abierto en Excel

---

## ?? Casos de Uso Comunes

### Caso 1: Verificación de Materiales Recibidos

**Situación**: Necesitas verificar qué materiales han llegado para la casa que estás construyendo.

**Pasos**:
1. Abre pestaña CONSULTA POR CASA
2. Selecciona tu manzana
3. Selecciona tu lote
4. Revisa la lista completa de insumos
5. Verifica cantidades y fechas

**Resultado**: Sabes exactamente qué tienes disponible.

---

### Caso 2: Buscar Insumo Específico

**Situación**: ¿Ya llegó el cemento para mi casa?

**Pasos**:
1. Selecciona tu casa (Manzana 3, Lote 8)
2. En campo de búsqueda escribe: "cemento"
3. Revisa los resultados filtrados

**Resultado**: Ves solo las entradas de cemento para tu casa.

---

### Caso 3: Generar Reporte para Supervisor

**Situación**: Tu supervisor pide un reporte de materiales de una casa.

**Pasos**:
1. Selecciona la casa solicitada
2. Click en "EXPORTAR A EXCEL"
3. Guarda el archivo
4. Envía el Excel al supervisor

**Resultado**: Reporte profesional generado en segundos.

---

### Caso 4: Detectar Problemas en Entregas

**Situación**: Quieres saber si hubo algún problema con las entregas de una casa.

**Pasos**:
1. Selecciona la casa
2. Observa el panel de estadísticas
3. Si aparece "Con discrepancia: X", hay problemas
4. Busca las filas con fondo rojo en la tabla
5. Lee la columna "Justificación"

**Resultado**: Identificas y entiendes los problemas.

---

### Caso 5: Comparar Costos entre Casas

**Situación**: Comparar cuánto han costado los materiales de dos casas similares.

**Pasos**:
1. Consulta primera casa (M3-L5)
2. Anota importe total: $118,000
3. Consulta segunda casa (M8-L12)
4. Anota importe total: $125,450
5. Calcula diferencia: $7,450

**Resultado**: Detectas variación de costos para investigar.

---

## ? Preguntas Frecuentes (FAQ)

### ¿Por qué no aparece mi manzana en el combo?

**Respuesta**: Solo se muestran manzanas que tienen entradas de insumos registradas. Si no aparece:
- Verifica que se hayan registrado entradas para esa manzana
- Usa la pestaña "ENTRADAS" para registrar insumos primero

---

### ¿Puedo ver varias casas al mismo tiempo?

**Respuesta**: No, esta pestaña muestra una casa a la vez. Para:
- Ver todo el inventario: Usa pestaña "INVENTARIO"
- Comparar casas: Exporta Excel de cada una y compara manualmente

---

### ¿Los datos se actualizan automáticamente?

**Respuesta**: No. Si otro usuario registra nuevas entradas mientras tú estás consultando:
- Click en botón "ACTUALIZAR" para refrescar
- O cambia a otra casa y regresa a la anterior

---

### ¿Qué significa "DISCREPANCIA"?

**Respuesta**: Indica que la cantidad recibida fue diferente a la cantidad ordenada:
- Puede ser más o menos
- Siempre debe haber una justificación
- Lee la columna "Justificación" para entender el motivo

---

### ¿Puedo editar los datos desde aquí?

**Respuesta**: No, esta pestaña es solo de **consulta**. Para editar:
- Usa la pestaña "ENTRADAS" para nuevos registros
- Contacta al administrador para modificaciones

---

### ¿El Excel incluye solo lo que veo en pantalla?

**Respuesta**: El Excel incluye **todos** los insumos de la casa seleccionada, incluso si has aplicado un filtro de búsqueda.

---

### ¿Puedo imprimir directamente?

**Respuesta**: No hay botón de impresión directa. Debes:
1. Exportar a Excel
2. Abrir el archivo
3. Usar Ctrl+P en Excel para imprimir

---

### ¿Se puede cambiar el orden de las columnas?

**Respuesta**: No directamente en la aplicación. Pero puedes:
1. Exportar a Excel
2. Reorganizar columnas en Excel
3. Guardar tu versión personalizada

---

## ?? Solución de Problemas

### Problema: No se cargan los lotes

**Síntomas**: 
- Selecciono manzana pero combo de lotes permanece vacío

**Solución**:
1. Verifica que hayas seleccionado una manzana válida (no "-- Seleccionar --")
2. Confirma que esa manzana tenga lotes con insumos registrados
3. Intenta con otra manzana
4. Si persiste, contacta soporte

---

### Problema: La tabla está vacía

**Síntomas**:
- Selecciono manzana y lote pero no aparecen insumos

**Posibles causas**:
1. Esa casa no tiene insumos registrados aún
2. Error de conexión a base de datos

**Solución**:
1. Verifica en pestaña "ENTRADAS" si hay registros para esa casa
2. Intenta con otra casa conocida que sí tenga insumos
3. Click en "ACTUALIZAR"
4. Si persiste, contacta soporte

---

### Problema: La búsqueda no encuentra nada

**Síntomas**:
- Escribo en búsqueda pero no filtra correctamente

**Solución**:
1. Verifica la ortografía de lo que buscas
2. Prueba con términos más generales (ej: "cem" en lugar de "cemento portland")
3. Borra el texto del campo de búsqueda para ver todo
4. Asegúrate de que el insumo exista en esa casa

---

### Problema: Error al exportar a Excel

**Síntomas**:
- Mensaje de error al intentar exportar

**Posibles causas**:
1. No tienes permisos de escritura en la carpeta destino
2. El archivo ya existe y está abierto en Excel
3. Disco lleno

**Solución**:
1. Elige otra carpeta para guardar
2. Cierra Excel si está abierto
3. Verifica espacio en disco
4. Intenta con nombre de archivo diferente

---

### Problema: Las estadísticas no coinciden

**Síntomas**:
- El total de insumos o importe parece incorrecto

**Solución**:
1. Click en "ACTUALIZAR" para refrescar datos
2. Verifica que no tengas un filtro de búsqueda activo (limpia el campo)
3. Si persiste, reporta a soporte técnico

---

## ?? Consejos y Mejores Prácticas

### Consejo 1: Usa la Búsqueda para Trabajo Rápido
En lugar de buscar manualmente en la tabla, usa el campo de búsqueda.

**Ejemplo**:
```
Necesitas: ¿Ya llegó varilla a casa M5-L12?
Rápido: Buscar "varilla"
Lento: Scroll manual en 45 filas
```

---

### Consejo 2: Exporta Excel para Análisis Detallado
Si necesitas hacer cálculos o análisis complejos, exporta a Excel.

**Ventajas**:
- Filtros avanzados de Excel
- Tablas dinámicas
- Gráficas
- Fórmulas personalizadas

---

### Consejo 3: Verifica Discrepancias Inmediatamente
Si ves contador de discrepancias:

1. ? Identifica filas rojas
2. ? Lee justificaciones
3. ? Comunica a supervisor si necesario
4. ? Documenta para auditoría

---

### Consejo 4: Genera Reportes Semanales
Para casas en construcción activa:

1. Cada lunes: Exporta Excel de la casa
2. Archiva con fecha
3. Compara evolución semanal
4. Detecta patrones o problemas

---

### Consejo 5: Combina con Otras Pestañas

**Flujo eficiente**:
1. **ENTRADAS**: Registra nuevos insumos
2. **CONSULTA POR CASA**: Verifica qué tiene cada casa
3. **INVENTARIO**: Vista global de todo
4. **HISTORIAL**: Audita movimientos

---

## ?? Recursos Adicionales

### Documentación Completa
- ?? **DOCUMENTACION_ConsultaPorCasa_FormAlmacen.md**
  - Documentación técnica exhaustiva
  - Detalles de implementación
  - Consultas SQL utilizadas

### Guía Rápida
- ? **GUIA_RAPIDA_ConsultaPorCasa.md**
  - Referencia de 1 página
  - Atajos y tips rápidos
  - Ideal para impresión

### Resumen Ejecutivo
- ?? **RESUMEN_ConsultaPorCasa.md**
  - Visión general de la funcionalidad
  - Estadísticas de implementación
  - Casos de uso

---

## ?? Soporte

### ¿Necesitas Ayuda?

**Soporte Técnico**:
- ?? Email: soporte@calandria.com
- ?? Tel: [Número de soporte]
- ?? Horario: Lunes a Viernes, 8:00 - 18:00

**Preguntas Comunes**:
- Revisa este manual primero
- Consulta la sección FAQ
- Revisa documentación técnica

---

## ? Checklist para Nuevos Usuarios

Antes de usar la funcionalidad por primera vez:

- [ ] Leo este manual completo
- [ ] Entiendo los 3 pasos básicos (Manzana ? Lote ? Ver)
- [ ] Sé usar el campo de búsqueda
- [ ] Puedo exportar a Excel
- [ ] Entiendo los códigos de colores
- [ ] Conozco las estadísticas disponibles
- [ ] Sé cómo actualizar datos
- [ ] Tengo los contactos de soporte

---

## ?? Ejercicios de Práctica

### Ejercicio 1: Consulta Básica
1. Abre la pestaña CONSULTA POR CASA
2. Selecciona cualquier manzana
3. Selecciona cualquier lote
4. Observa los resultados

**¿Pudiste ver los insumos?** ?

---

### Ejercicio 2: Uso de Búsqueda
1. Con una casa seleccionada
2. Busca: "cemento"
3. Luego busca: "varilla"
4. Limpia la búsqueda

**¿Los filtros funcionaron correctamente?** ?

---

### Ejercicio 3: Exportación
1. Selecciona una casa
2. Exporta a Excel
3. Abre el archivo
4. Revisa el contenido

**¿El Excel se generó correctamente?** ?

---

### Ejercicio 4: Detección de Problemas
1. Busca una casa con discrepancias
2. Identifica las filas rojas
3. Lee las justificaciones

**¿Entiendes los problemas detectados?** ?

---

## ?? Resumen de Funciones

| Función | Cómo Hacerlo |
|---------|--------------|
| **Consultar casa** | Manzana ? Lote ? Ver |
| **Buscar insumo** | Escribir en campo búsqueda |
| **Ver estadísticas** | Panel superior automático |
| **Exportar** | Click "EXPORTAR A EXCEL" |
| **Actualizar** | Click "ACTUALIZAR" |
| **Ver discrepancias** | Buscar filas rojas |

---

**¡Felicidades! Ya conoces cómo usar la pestaña CONSULTA POR CASA.**

**Recuerda**: Esta herramienta te ayuda a rastrear fácilmente los materiales de cada casa. ¡Úsala para mejorar el control de tu obra!

---

**Fecha de este manual**: Enero 2025  
**Versión de la funcionalidad**: 1.2  
**Sistema**: CALANDRIA RESIDENCIAL - Gestión de Almacén  
**Estado**: ? OPERATIVO
