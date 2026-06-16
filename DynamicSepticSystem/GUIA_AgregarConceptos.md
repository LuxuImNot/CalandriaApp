# ?? GUÍA DE USO: AGREGAR CONCEPTOS

## ?? Descripción General

La funcionalidad **"? Agregar Concepto"** permite agregar nuevos conceptos personalizados al catálogo de estimaciones con **selección visual de posición** y renumeración automática.

---

## ?? Ubicación

**Formulario:** `FormEstimacionConceptoMigrado`  
**Botón:** `? Agregar Concepto` (esquina inferior derecha, color azul #3498db)  
**Acceso:** Panel de botones inferior

---

## ?? Pasos para Agregar un Concepto

### 1?? **Abrir el Diálogo**
- Click en el botón **"? Agregar Concepto"**
- Se abre el formulario modal **"Agregar Nuevo Concepto"**

### 2?? **Ingresar Nombre del Concepto** (Sección A)
- Escribe el nombre del nuevo concepto en el campo de texto
- Ejemplo: `"Instalación de Paneles Solares"`

### 3?? **Seleccionar Posición** (Sección B)
El sistema muestra **todos los conceptos existentes** en la base de datos:

```
[1] Preliminares
[2] Cimentación
[3] Estructura
[4] Ins. Hidraulica, Sanitaria y Gas LP
[5] Inst. Eléctrica
[6] Albañilería
[7] Acabados
[8] Herrería, Aluminio y Vidrio
[9] Carpintería y Cerrajería
[10] Muebles y Accesorios
[11] Inst especiales y Obra Exterior
[12] Urbanización
? [Insertar al FINAL]
```

**Opciones:**
- **Seleccionar un concepto existente:** El nuevo concepto se insertará **ANTES** de ese concepto
- **Seleccionar "[Insertar al FINAL]":** El nuevo concepto se agregará al final de la lista

**Indicador visual:**
- ? **Verde:** "Se insertará al FINAL de la lista"
- ? **Azul:** "Se insertará ANTES de: [X] Concepto"

#### ?? **NUEVO: Partidas de Referencia**

Cuando seleccionas un concepto existente, el sistema **automáticamente muestra** las partidas de ese concepto en la sección derecha (grid de partidas).

**Características de las Partidas de Referencia:**
- ?? **Color gris claro** - Indica que son solo informativas
- ?? **Solo lectura** - No se pueden editar ni eliminar
- ?? **Propósito** - Sirven como referencia para las partidas del nuevo concepto
- ? **Uso de Etapas** - Al agregar una nueva partida, las etapas del concepto de referencia aparecen en el ComboBox

**Ejemplo Visual:**
```
???????????????????????????????????????????????????????
? ?? Partidas del Concepto                            ?
???????????????????????????????????????????????????????
? Etapa              ? Partida          ? Tunera ? Cal?
???????????????????????????????????????????????????????
? ?? Yesos           ? Falso plafón     ? $500   ?$550? ? Gris (Referencia)
? ?? Yesos           ? Tirol en muros   ? $300   ?$330? ? Gris (Referencia)
? ?? Enjarres        ? Repellado        ? $400   ?$440? ? Gris (Referencia)
? ????????????????????????????????????????????????????? ? Separador
? ?? Yesos           ? Plafón decorativo? $800   ?$880? ? Blanco (Nueva)
? ?? Acabados        ? Texturizado      ? $600   ?$660? ? Blanco (Nueva)
???????????????????????????????????????????????????????
Total: 2 partida(s) - Referencia: 3
```

**Ventajas:**
- ? Facilita la creación de conceptos similares
- ? Mantiene consistencia en nombres de etapas
- ? Referencia rápida de costos del concepto base
- ? No interfiere con las partidas nuevas

### 4?? **Agregar Partidas** (Sección C)

#### ? Agregar una Partida
1. Click en **"? Agregar Partida"**
2. Se abre un diálogo con los siguientes campos:
   - **Etapa:** ComboBox editable con etapas sugeridas del concepto de referencia
   - **Partida:** Nombre de la partida (obligatorio)
   - **Costo Tunera:** Costo para prototipo Tunera
   - **Costo Calandra:** Costo para prototipo Calandra
3. Click en **"? Agregar"**

**?? Etapas Sugeridas:**
Si hay un concepto de referencia seleccionado, el ComboBox de "Etapa" mostrará:
- Todas las etapas del concepto de referencia
- Etapas de partidas ya agregadas
- Permite escribir una etapa nueva manualmente

**Ejemplo:**
```
Etapa: Yesos ? Sugerida del concepto de referencia
Partida: Plafón con molduras
Costo Tunera: $15,000.00
Costo Calandra: $15,000.00
```

#### ??? Eliminar una Partida
1. Selecciona la fila de la partida en la tabla
2. Click en **"??? Eliminar"**

**?? IMPORTANTE:**
- ? Puedes eliminar **partidas nuevas** (fondo blanco/alternado)
- ? NO puedes eliminar **partidas de referencia** (fondo gris)
- Si intentas eliminar una partida de referencia, aparecerá: 
  ```
  ?? No puedes eliminar partidas de referencia del concepto existente.
  ```

**Contador:** 
- Muestra partidas nuevas: `"Total: 2 partida(s)"`
- Si hay referencia: `"Total: 2 partida(s) - Referencia: 3"`

### 5?? **Guardar el Concepto**

Click en **"?? Guardar Concepto"**

#### Validaciones Automáticas:
- ? Nombre del concepto vacío
- ? Sin partidas agregadas
- ? Sin posición seleccionada

#### Confirmación:
Si seleccionaste **insertar ANTES de un concepto existente**, aparecerá:

```
?? ADVERTENCIA: Los códigos de los conceptos posteriores 
se renumerarán automáticamente.

¿Continuar?
```

Si seleccionaste **insertar al FINAL**, aparecerá:

```
Se agregará el concepto 'Nombre' al FINAL de la lista.
Partidas: X

¿Continuar?
```

**?? Nota Importante:**
- Solo se guardan las **partidas nuevas** (las que agregaste)
- Las **partidas de referencia** NO se guardan (solo son informativas)

### 6?? **Resultado**

Mensaje de éxito:
```
? Concepto agregado exitosamente

Código: 13
Nombre: Instalación de Paneles Solares
Partidas: 3

Recarga el avance para ver los cambios.
```

### 7?? **Ver los Cambios**

Para ver el nuevo concepto en la lista:
1. Selecciona **Manzana** y **Lote**
2. Click en **"?? Cargar Avance"**
3. El nuevo concepto aparecerá en la lista jerárquica

---

## ?? NUEVA CARACTERÍSTICA: Selector de Posición de Partidas

Cuando hay partidas de referencia, al hacer clic en "? Agregar" aparece un diálogo adicional:

**"¿Dónde deseas insertar la nueva partida?"**

```
???????????????????????????????????????????????????????
? Selecciona una partida existente del concepto o     ?
? inserta al final:                                   ?
???????????????????????????????????????????????????????
? [1] Yesos - Falso plafón                            ?
? [2] Yesos - Tirol en muros                          ?
? [3] Enjarres - Repellado                            ?
? [NUEVA 1] Yesos - Plafón decorativo                 ?
? [NUEVA 2] Acabados - Texturizado                    ?
? ? [Insertar al FINAL]                               ?
???????????????????????????????????????????????????????
```

**Opciones:**
- Seleccionar una partida ? La nueva se insertará ANTES de ella
- Seleccionar "? [Insertar al FINAL]" ? Se agrega al final

---

## ?? Características Técnicas

### ?? Base de Datos

El sistema guarda el nuevo concepto en **DOS tablas**:

1. **`Estimacion(Concepto)`** - Catálogo de conceptos con costos
2. **`PresupuestoObra`** - Presupuesto detallado por partida

**Columnas insertadas:**
- `Codigo` - Código numérico del concepto
- `Concepto` - Nombre del concepto
- `Padre` - Mismo que Concepto (para jerarquía)
- `Etapa` - Etapa de la partida
- `Partida` - Nombre de la partida
- `TOTAL` - Costo total (usa CostoTunera)
- `CostoTunera` - Costo para prototipo Tunera
- `CostoCalandra` - Costo para prototipo Calandra

### ?? Renumeración Automática

**Ejemplo de inserción en medio:**

**Antes:**
```
[1] Preliminares
[2] Cimentación
[3] Estructura
```

**Insertar "Demolición" ANTES de [2]:**

**Después:**
```
[1] Preliminares
[2] Demolición       ? NUEVO
[3] Cimentación      ? Renumerado (era 2)
[4] Estructura       ? Renumerado (era 3)
```

### ?? Compatibilidad

El sistema **detecta automáticamente** si las tablas tienen columna `Codigo`:

- ? **CON columna Codigo:** Usa el código existente y renumera
- ?? **SIN columna Codigo:** Genera códigos según orden estándar

---

## ?? Advertencias y Consideraciones

### ?? Renumeración
- La renumeración es **PERMANENTE** en la base de datos
- Afecta a **TODOS los conceptos** posteriores al punto de inserción
- Se recomienda hacer **respaldo de BD** antes de usar esta función

### ?? Partidas de Referencia
- ?? Las partidas de referencia son **SOLO INFORMATIVAS**
- ?? NO se pueden editar ni eliminar
- ? Solo se guardan las **partidas nuevas** que agregues
- ?? Sirven como guía para crear partidas similares

### ?? Validaciones
- ? No se pueden agregar conceptos sin partidas
- ? Las etapas y partidas no pueden estar vacías
- ? Los costos pueden ser $0.00

### ?? Relaciones
- El nuevo concepto **NO tendrá avances guardados** hasta que se registren
- Aparecerá en la lista **después de recargar el avance**
- Se puede estimar **igual que cualquier otro concepto**

---

## ?? Solución de Problemas

### Problema: No aparecen conceptos existentes
**Causa:** Base de datos vacía o error de conexión  
**Solución:** 
- Verificar que existan datos en `Estimacion(Concepto)`
- Revisar la cadena de conexión
- Ver la ventana "Output" de Visual Studio para logs

### Problema: No aparecen partidas de referencia
**Causa:** Tabla PresupuestoObra vacía o sin columna Codigo  
**Solución:**
- Verificar que existan datos en `PresupuestoObra`
- Verificar que la tabla tenga la columna `Codigo`
- Ver logs en Output: "? Cargadas X partidas del concepto [Y]"

### Problema: Error al guardar
**Causa:** Permisos de BD o estructura incorrecta  
**Solución:**
- Verificar permisos de escritura en SQL Server
- Verificar que existan las tablas requeridas
- Revisar el mensaje de error detallado

### Problema: Concepto no aparece después de guardar
**Causa:** No se recargó el avance  
**Solución:**
- Click en **"?? Cargar Avance"** después de agregar el concepto
- Verificar que esté seleccionada Manzana y Lote

---

## ?? Ejemplo Completo

### Caso de Uso: Agregar "Sistema de Riego"

1. **Click en** `? Agregar Concepto`

2. **Nombre:** `Sistema de Riego Automatizado`

3. **Posición:** Seleccionar `[11] Inst especiales y Obra Exterior`  
   ? Se insertará ANTES, quedando como [11]
   
4. **Partidas de Referencia:** Se cargan automáticamente las partidas del concepto [11]
   ```
   ?? Instalaciones - Gas LP          $8,500.00
   ?? Instalaciones - Drenaje pluvial $12,000.00
   ...
   ```

5. **Agregar Partidas:**

   | Etapa | Partida | Costo Tunera | Costo Calandra |
   |-------|---------|--------------|----------------|
   | Instalaciones | Tuberías PVC 1" | $5,000.00 | $5,200.00 |
   | Instalaciones | Aspersores Pop-Up | $3,500.00 | $3,500.00 |
   | Instalaciones | Programador Digital | $2,800.00 | $2,800.00 |
   | Instalaciones | Válvulas Solenoide | $4,200.00 | $4,200.00 |

6. **Total:** 4 partidas nuevas (+ 2 de referencia mostradas)

7. **Guardar** ? Confirmar renumeración

8. **Resultado:**
   ```
   [11] Sistema de Riego Automatizado  ? NUEVO
   [12] Inst especiales y Obra Exterior ? Renumerado
   [13] Urbanización                     ? Renumerado
   ```

9. **Recargar avance** para ver el nuevo concepto

---

## ?? Depuración

El sistema incluye **logs automáticos** en la ventana **Output** de Visual Studio:

```
? Conexión a BD abierta correctamente
? Tabla tiene columna 'Codigo': True
? Ejecutando consulta SQL...
  ? Leído: [1] Preliminares
  ? Leído: [2] Cimentación
  ...
? Total conceptos cargados: 12
? Conceptos cargados: 12
  - [1] Preliminares
  - [2] Cimentación
  ...
? Cargadas 15 partidas del concepto [6]
?? Grid actualizado: 17 filas visibles (15 ref + 2 nuevas)
```

---

## ?? Archivos Relacionados

- `FormEstimacionConceptoMigrado.cs` - Formulario principal
- `FormEstimacionConceptoMigrado.AgregarConcepto.cs` - Lógica de agregar concepto
- `FormAgregarConcepto.cs` - Diálogo modal
- `FormAgregarConcepto.Designer.cs` - Diseño del diálogo
- `RESUMEN_CORRECCIONES_FormAgregarConcepto.md` - Documentación técnica de correcciones

---

## ?? Paleta de Colores

- **Botón Principal:** `#3498db` (Azul)
- **Agregar Partida:** `#2ecc71` (Verde)
- **Eliminar Partida:** `#e74c3c` (Rojo)
- **Guardar:** `#3498db` (Azul)
- **Cancelar:** `#95a5a6` (Gris)
- **Partidas de Referencia:** `#F5F5F5` (Gris claro)
- **Separador:** `LightBlue` (Azul claro)

---

## ?? Mejoras Futuras Sugeridas

- [ ] Importar conceptos desde Excel
- [ ] Clonar conceptos existentes (copiar todas las partidas)
- [ ] Editar conceptos existentes
- [ ] Eliminar conceptos
- [ ] Mover conceptos (drag & drop)
- [ ] Historial de cambios en conceptos
- [ ] Plantillas de conceptos predefinidas
- [x] ? Mostrar partidas de referencia al seleccionar concepto
- [x] ? Selector de posición para nuevas partidas
- [x] ? Validación para evitar eliminar partidas de referencia

---

**Última actualización:** Enero 2025  
**Versión:** 1.1 (con partidas de referencia)  
**Compatibilidad:** .NET Framework 4.7.2, C# 7.3  
**Estado:** ? Totalmente funcional
