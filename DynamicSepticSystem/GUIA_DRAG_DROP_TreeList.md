# ?? Guía de Drag & Drop - Editor TreeList

## ?? Descripción General

El sistema de Drag & Drop permite reorganizar la estructura jerárquica del TreeList arrastrando nodos con el mouse. Soporta tanto arrastre individual como múltiple selección.

---

## ? Características Principales

### 1. **Arrastre Individual**
- Arrastra un solo nodo a la vez
- Cambio automático de nivel según el destino
- Preservación de datos y descendientes

### 2. **Arrastre Múltiple**
- Selecciona múltiples nodos con `Ctrl` o `Shift`
- Arrastra varios nodos simultáneamente
- Validaciones automáticas de compatibilidad

### 3. **Validaciones Inteligentes**
- No permite arrastrar sobre sí mismo
- No permite arrastrar sobre descendientes
- Valida reglas de jerarquía
- Verifica que los nodos sean hermanos (mismo padre)

---

## ?? Cómo Usar

### Arrastre Individual

1. **Seleccionar un nodo**
   - Clic sobre el nodo que deseas mover

2. **Iniciar arrastre**
   - Mantén presionado el botón izquierdo del mouse
   - Arrastra el cursor sobre el nodo destino

3. **Soltar**
   - Suelta el botón del mouse sobre el nodo destino
   - El nodo se moverá automáticamente

### Arrastre Múltiple

1. **Seleccionar múltiples nodos**
   - **Selección continua**: Mantén `Shift` y clic en el último nodo
   - **Selección individual**: Mantén `Ctrl` y clic en cada nodo

2. **Iniciar arrastre**
   - Arrastra cualquiera de los nodos seleccionados
   - Todos los nodos seleccionados se moverán juntos

3. **Validaciones automáticas**
   - El sistema verifica que todos sean del mismo nivel
   - Verifica que todos sean hermanos (mismo padre)
   - Si no cumplen, muestra un mensaje de error

---

## ?? Reglas de Jerarquía

### Regla 1: Padre ? Padre
```
?? Padre A
?  ?? Sub-Padre A1
?  ?? Sub-Padre A2
?
?? Padre B            ?? Arrastra Padre A aquí
   ?? Sub-Padre B1
   ?? ? Padre A (ahora Sub-Padre)  ? Resultado
      ?? Sub-Padre A1 (ahora Hijo)
      ?? Sub-Padre A2 (ahora Hijo)
```
**Resultado**: Padre A se convierte en **Sub-Padre** de Padre B  
Los niveles se ajustan automáticamente (+1 para todos los descendientes)

---

### Regla 2: Sub-Padre ? Sub-Padre
```
Padre A
?? Sub-Padre A1
?  ?? Hijo A1.1
?
?? Sub-Padre A2       ?? Arrastra Sub-Padre A1 aquí
   ?? ? Sub-Padre A1 (ahora Hijo)  ? Resultado
      ?? Hijo A1.1 (ahora nivel 3 - se elimina*)
```
**Resultado**: Sub-Padre A1 se convierte en **Hijo** de Sub-Padre A2  
?? Los descendientes se ajustan automáticamente (+1 nivel)  
?? **Nota**: Si el nodo tiene descendientes, estos pasarán al nivel 3 (o superior)

---

### Regla 3: Hijo ? Sub-Padre
```
Padre A
?? Sub-Padre A1
?  ?? Hijo A1.1       ?? Arrastra este hijo
?
?? Sub-Padre A2       ?? Hacia aquí
   ?? ? Hijo A1.1  ? Resultado (sigue siendo Hijo)
```
**Resultado**: Cambio de padre, el Hijo se mueve a Sub-Padre A2  
Mantiene el mismo nivel (Hijo)

---

### Regla 4: Sub-Padre ? Padre
```
Padre A
?? Sub-Padre A1       ?? Arrastra este Sub-Padre
?  ?? Hijo A1.1
?
Padre B               ?? Hacia aquí
?? ? Sub-Padre A1  ? Resultado (sigue siendo Sub-Padre)
   ?? Hijo A1.1
```
**Resultado**: Cambio de padre, el Sub-Padre se mueve a Padre B  
Mantiene el mismo nivel (Sub-Padre)

---

## ?? Validaciones y Restricciones

### Arrastre Individual

| Validación | Descripción | Mensaje de Error |
|------------|-------------|------------------|
| **Auto-referencia** | No puedes arrastrar un nodo sobre sí mismo | ? Operación no permitida |
| **Descendientes** | No puedes arrastrar un nodo sobre sus propios hijos | ? Operación no permitida |
| **Nivel incompatible** | Solo ciertos niveles pueden combinarse | ? Operación no permitida |

### Arrastre Múltiple

| Validación | Descripción | Mensaje de Error |
|------------|-------------|------------------|
| **Mismo nivel** | Todos deben ser Padre, Sub-Padre o Hijo | ? "Solo puedes arrastrar nodos del mismo nivel" |
| **Mismos hermanos** | Todos deben tener el mismo padre | ? "Solo puedes arrastrar nodos que sean hermanos" |
| **Todas las reglas individuales** | Cada nodo debe cumplir las reglas | ? "Todos los nodos deben cumplir las mismas reglas" |

---

## ?? Ejemplos Prácticos

### Ejemplo 1: Reorganizar Sub-Padres entre Padres

**Escenario**: Tienes 2 Padres y quieres mover Sub-Padres de uno a otro

```
ANTES:
Padre A
?? Sub-Padre A1
?? Sub-Padre A2

Padre B
?? Sub-Padre B1

ACCIÓN: Arrastrar Sub-Padre A2 ? Padre B

DESPUÉS:
Padre A
?? Sub-Padre A1

Padre B
?? Sub-Padre B1
?? Sub-Padre A2  ?
```

---

### Ejemplo 2: Mover múltiples Hijos a otro Sub-Padre

**Escenario**: Reorganizar hijos entre diferentes Sub-Padres

```
ANTES:
Padre A
?? Sub-Padre A1
?  ?? Hijo 1
?  ?? Hijo 2
?  ?? Hijo 3
?
?? Sub-Padre A2
   ?? Hijo 4

ACCIÓN: 
1. Seleccionar Hijo 1, Hijo 2 (Ctrl+Clic)
2. Arrastrar ? Sub-Padre A2

DESPUÉS:
Padre A
?? Sub-Padre A1
?  ?? Hijo 3
?
?? Sub-Padre A2
   ?? Hijo 4
   ?? Hijo 1  ?
   ?? Hijo 2  ?
```

---

### Ejemplo 3: Convertir Padre en Sub-Padre

**Escenario**: Reorganizar la estructura principal

```
ANTES:
Padre A
?? Sub-Padre A1
?  ?? Hijo A1.1
?? Sub-Padre A2

Padre B
?? Sub-Padre B1

ACCIÓN: Arrastrar Padre A ? Padre B

DESPUÉS:
Padre B
?? Sub-Padre B1
?? Sub-Padre A  ? (antes era Padre)
   ?? Hijo A1  ? (antes era Sub-Padre)
   ?  ?? (Hijo A1.1 se elimina - nivel 3)
   ?? Hijo A2  ? (antes era Sub-Padre)
```

?? **Importante**: Al cambiar niveles, los descendientes también se ajustan.  
Si un nodo pasaría al nivel 3 o superior, se eliminaría de la jerarquía.

---

## ?? Indicadores Visuales

### Durante el Arrastre

| Estado | Cursor | Descripción |
|--------|--------|-------------|
| ? **Drop Permitido** | ??? ? | Cursor de movimiento, nodo destino resaltado |
| ? **Drop Bloqueado** | ?? | Cursor prohibido, no se puede soltar |
| ?? **Arrastrando** | ????? | Arrastrando uno o múltiples nodos |

### Selección Múltiple

- **Nodos seleccionados**: Resaltados en azul
- **Contador**: Se muestra en el mensaje de confirmación
- **Listado**: Se muestran los nombres de todos los nodos movidos

---

## ?? Configuración Técnica

### Campos Privados

```csharp
private List<NodoTree> nodosArrastrados;  // Lista de nodos en arrastre
private bool dragEnProgreso;               // Indicador de arrastre activo
```

### Eventos Principales

```csharp
// Iniciar arrastre
treeListView.ItemDrag += TreeListView_ItemDrag;

// Validar durante arrastre
treeListView.DragOver += TreeListView_DragOver;

// Completar arrastre
treeListView.DragDrop += TreeListView_DragDrop;
```

### Métodos Clave

```csharp
TodosMismoNivel()        // Valida mismo nivel
TodosHermanos()          // Valida mismo padre
TodosDropPermitidos()    // Valida reglas para todos
RealizarDragDropMultiple() // Ejecuta el movimiento
```

---

## ?? Métodos de Validación

### `TodosMismoNivel(List<NodoTree> nodos)`
Verifica que todos los nodos tengan el mismo nivel (0, 1 o 2)

### `TodosHermanos(List<NodoTree> nodos)`
Verifica que todos los nodos tengan el mismo ParentID

### `EsDropPermitido(NodoTree origen, NodoTree destino)`
Verifica que la combinación origen-destino sea válida según las reglas

### `EsDescendiente(NodoTree posible, NodoTree ancestro)`
Verifica recursivamente si un nodo es descendiente de otro

---

## ?? Flujo de Arrastre Múltiple

```
???????????????????????????????????????
? 1. Usuario selecciona múltiples     ?
?    nodos (Ctrl/Shift + Clic)        ?
???????????????????????????????????????
               ?
???????????????????????????????????????
? 2. Usuario inicia arrastre          ?
?    (Mouse Down + Drag)               ?
???????????????????????????????????????
               ?
???????????????????????????????????????
? 3. Validaciones automáticas:        ?
?    ? Mismo nivel?                   ?
?    ? Mismos hermanos?               ?
?    ? Drop permitido para todos?     ?
???????????????????????????????????????
               ?
         ?????????????
         ?           ?
    ? Error    ? Válido
         ?           ?
         ?    ???????????????
         ?    ? 4. Mover    ?
         ?    ?    todos    ?
         ?    ?    los      ?
         ?    ?    nodos    ?
         ?    ???????????????
         ?           ?
         ?    ???????????????????????
         ?    ? 5. Actualizar vista ?
         ?    ?    y guardar        ?
         ?    ???????????????????????
         ?
    Mostrar mensaje
    de error
```

---

## ? Checklist de Funcionalidad

### Arrastre Individual
- [x] Seleccionar nodo
- [x] Arrastrar con mouse
- [x] Validar destino
- [x] Cambiar nivel automático
- [x] Preservar descendientes
- [x] Actualizar órdenes
- [x] Mostrar confirmación

### Arrastre Múltiple
- [x] Selección con Ctrl
- [x] Selección con Shift
- [x] Validar mismo nivel
- [x] Validar hermanos
- [x] Arrastrar todos juntos
- [x] Mantener orden relativo
- [x] Mostrar contador
- [x] Listar nombres movidos

### Validaciones
- [x] No auto-referencia
- [x] No descendientes
- [x] Reglas de nivel
- [x] Mismo nivel múltiple
- [x] Hermanos múltiple
- [x] Mensajes de error claros

---

## ?? Shortcuts y Atajos

| Acción | Método |
|--------|--------|
| **Seleccionar múltiples continuos** | `Shift` + Clic |
| **Seleccionar individuales** | `Ctrl` + Clic |
| **Arrastrar** | Clic sostenido + Mover mouse |
| **Cancelar arrastre** | `ESC` o soltar fuera |
| **Confirmar drop** | Soltar sobre destino válido |

---

## ?? Solución de Problemas

### Problema: No puedo arrastrar múltiples nodos

**Solución**: Verifica que:
1. Todos los nodos sean del mismo nivel
2. Todos tengan el mismo padre (sean hermanos)
3. No estés arrastrando sobre un nodo inválido

### Problema: El cursor muestra "prohibido" pero debería permitir

**Solución**: Verifica:
1. No estás arrastrando sobre un descendiente
2. La combinación de niveles es válida según las reglas
3. No estás arrastrando sobre el mismo nodo

### Problema: Los descendientes desaparecen después del arrastre

**Explicación**: Cuando un nodo cambia de nivel, sus descendientes se ajustan.  
Si un descendiente pasaría al nivel 3 o superior, la estructura actual no lo soporta.

**Solución**: Considera reorganizar manualmente antes de hacer grandes cambios.

---

## ?? Notas Importantes

1. **Cambios Pendientes**: El drag & drop marca cambios como pendientes. Debes guardar.

2. **Órdenes Automáticos**: Los órdenes se recalculan automáticamente después de mover.

3. **Transaccionalidad**: Todo el movimiento múltiple se realiza en una sola operación.

4. **Preservación de Datos**: Todos los datos personalizados se preservan durante el movimiento.

5. **Fecha de Modificación**: Se actualiza automáticamente en todos los nodos movidos.

---

## ?? Próximas Mejoras

- [ ] Deshacer/Rehacer (Ctrl+Z / Ctrl+Y)
- [ ] Arrastrar entre diferentes TreeLists
- [ ] Preview visual antes de soltar
- [ ] Animaciones de movimiento
- [ ] Historial de cambios
- [ ] Arrastrar al espacio vacío para crear nuevo Padre

---

## ?? Referencias

- **Archivo**: `FormEditorTreeList.cs`
- **Región**: `#region Configuración de Drag & Drop`
- **Componente**: `BrightIdeasSoftware.TreeListView`
- **Documentación**: [ObjectListView Documentation](http://objectlistview.sourceforge.net/cs/index.html)

---

*Última actualización: 2024*
