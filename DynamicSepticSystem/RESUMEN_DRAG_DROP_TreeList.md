# ?? Drag & Drop - Resumen Rápido

## ?? Funcionalidades Implementadas

### ? Arrastre Individual
- Arrastrar un nodo con el mouse
- Cambio automático de nivel según destino
- Preservación de todos los datos y descendientes

### ? Arrastre Múltiple
- Seleccionar múltiples nodos: `Ctrl` + Clic o `Shift` + Clic
- Arrastrar varios nodos simultáneamente
- Validación automática de compatibilidad
- Mantiene el orden relativo de los nodos

---

## ?? Reglas Rápidas

| Origen | Destino | Resultado |
|--------|---------|-----------|
| **Padre** | Padre | Se convierte en **Sub-Padre** |
| **Sub-Padre** | Sub-Padre | Se convierte en **Hijo** |
| **Sub-Padre** | Padre | Cambio de padre (sigue siendo Sub-Padre) |
| **Hijo** | Sub-Padre | Cambio de padre (sigue siendo Hijo) |

---

## ?? Restricciones Múltiple

Para arrastrar múltiples nodos:
1. ? Deben ser del **mismo nivel** (Padre, Sub-Padre o Hijo)
2. ? Deben ser **hermanos** (mismo nodo padre)
3. ? Todos deben cumplir las **reglas de jerarquía**

---

## ?? Uso Rápido

### Arrastre Simple
```
1. Clic en un nodo
2. Arrastrarlo sobre otro nodo
3. Soltar
```

### Arrastre Múltiple
```
1. Ctrl + Clic en varios nodos
2. Arrastrar cualquiera de ellos
3. Soltar sobre el destino
```

---

## ?? Cambios en el Código

### Nuevos Campos
```csharp
private List<NodoTree> nodosArrastrados;  // Múltiples nodos
private bool dragEnProgreso;               // Estado del arrastre
```

### Nuevas Configuraciones
```csharp
treeListView.MultiSelect = true;  // Habilitar selección múltiple
```

### Nuevos Métodos
```csharp
TodosMismoNivel()        // Validar mismo nivel
TodosHermanos()          // Validar mismo padre
RealizarDragDropMultiple() // Mover múltiples nodos
```

---

## ? Características Clave

1. **Inteligente**: Detecta automáticamente cuántos nodos arrastrar
2. **Seguro**: Valida todas las reglas antes de permitir el drop
3. **Informativo**: Muestra mensajes claros sobre qué pasó
4. **Transaccional**: Todos los nodos se mueven en una sola operación
5. **Reversible**: Los cambios se marcan como pendientes (puedes cerrar sin guardar)

---

## ?? Ejemplos Visuales

### Mover 3 Hijos a otro Sub-Padre
```
ANTES:
Sub-Padre A          Sub-Padre B
?? Hijo 1 ?         ?? Hijo 4
?? Hijo 2 ?
?? Hijo 3 ?

DESPUÉS:
Sub-Padre A          Sub-Padre B
                     ?? Hijo 4
                     ?? Hijo 1
                     ?? Hijo 2
                     ?? Hijo 3
```

### Convertir 2 Padres en Sub-Padres
```
ANTES:
Padre A ?
Padre B ?
Padre C

DESPUÉS:
Padre C
?? Sub-Padre A (antes Padre)
?? Sub-Padre B (antes Padre)
```

---

## ?? Indicadores Visuales

| Icono | Significado |
|-------|-------------|
| ??? ? | Drop permitido |
| ?? | Drop bloqueado |
| ?? | Nodos seleccionados |
| ? | Operación exitosa |

---

## ?? Problemas Comunes

**"Solo puedes arrastrar nodos del mismo nivel"**
? Has seleccionado Padres y Sub-Padres mezclados

**"Solo puedes arrastrar nodos que sean hermanos"**
? Has seleccionado nodos con diferentes padres

**Cursor prohibido al arrastrar**
? La combinación origen-destino no es válida

---

## ?? Guardar Cambios

**Importante**: Los cambios del drag & drop se marcan como pendientes.  
Debes hacer clic en **"Guardar"** para persistirlos en la base de datos.

---

## ?? Documentación Completa

Ver: `GUIA_DRAG_DROP_TreeList.md` para más detalles y ejemplos.

---

*Implementado en: FormEditorTreeList.cs - Región: Configuración de Drag & Drop*
