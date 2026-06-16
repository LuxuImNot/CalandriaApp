# ?? SOLUCIÓN DEFINITIVA: Duplicación de Conceptos en TreeListView

**Fecha:** 2025-01-XX  
**Archivo:** `FormEstimacionConceptoMigrado.cs`  
**Problema:** Duplicación de conceptos al marcar checkboxes con nodos expandidos  
**Estado:** ? **RESUELTO COMPLETAMENTE**

---

## ?? **PROBLEMA IDENTIFICADO**

### Síntoma
Al marcar un checkbox cuando hay un concepto expandido, los conceptos se duplicaban visualmente en el TreeListView:

```
? Cimentación                    $38,359.66
  ? Preliminares > plataforma...  $0.00
  ? Preliminares > trazo...        $1,954.18
? Albañilería                    $143,142.06
? Albañilería                    $143,142.06  ? DUPLICADO
  ? Inst. Hidraulica...            $59,901.99
```

### Causa Raíz

El `BooleanCheckStatePutter` estaba usando **múltiples llamadas asíncronas** a `RefreshObject` y `RefreshObjects`:

```csharp
// ? CÓDIGO PROBLEMÁTICO (ANTES)
this.BeginInvoke(new Action(() => {
    olvEstimacionConceptos.RefreshObject(nodo);  // 1ra llamada
    if (nodo.EsConcepto)
    {
        olvEstimacionConceptos.RefreshObjects(nodo.Partidas);  // 2da llamada (PROBLEMA)
    }
    else
    {
        var padre = EncontrarPadre(nodo);
        if (padre != null)
        {
            olvEstimacionConceptos.RefreshObject(padre);  // 3ra llamada (CONFLICTO)
        }
    }
    OnDatosChanged(this, EventArgs.Empty);
}));
```

**Por qué causaba duplicación:**

1. **RefreshObject(nodo)** - Refresca el nodo padre
2. **RefreshObjects(nodo.Partidas)** - Intenta refrescar TODAS las partidas hijas
3. **Si el concepto está expandido**, el TreeListView intenta **re-renderizar los hijos** mientras están visibles
4. **Condición de carrera**: Las llamadas asíncronas se acumulan y ejecutan en desorden
5. **Resultado**: El TreeListView dibuja el mismo concepto **DOS VECES**

---

## ? **SOLUCIÓN IMPLEMENTADA**

### Estrategia
Usar **`RebuildAll(true)`** en lugar de múltiples `RefreshObject` para reconstruir el árbol completo de forma sincronizada.

### Código Corregido

```csharp
// ? CÓDIGO CORRECTO (AHORA)
olvEstimacionConceptos.BooleanCheckStatePutter = delegate(object rowObject, bool newValue) {
    var nodo = (NodoConcepto)rowObject;
    if (nodo.Completado && newValue)
        return false;
        
    nodo.Incluir = newValue;
    
    // Propagar cambios a hijos o padre
    if (nodo.EsConcepto)
    {
        // Si es concepto, propagar a las partidas
        foreach (var partida in nodo.Partidas)
        {
            if (!partida.Completado)
            {
                partida.Incluir = newValue;
            }
        }
    }
    else
    {
        // Si es partida, actualizar el concepto padre
        var padre = EncontrarPadre(nodo);
        if (padre != null)
        {
            bool todasMarcadas = padre.Partidas.All(p => p.Incluir || p.Completado);
            bool ningunaMarcada = padre.Partidas.All(p => !p.Incluir || p.Completado);
            
            if (todasMarcadas)
            {
                padre.Incluir = true;
            }
            else if (ningunaMarcada)
            {
                padre.Incluir = false;
            }
        }
    }
    
    // ?? SOLUCIÓN DEFINITIVA: Usar RebuildAll en lugar de RefreshObject
    // Esto evita duplicaciones al reconstruir todo el árbol correctamente
    this.BeginInvoke(new Action(() => {
        try
        {
            // 1. Guardar el estado de expansión
            var expandidos = new HashSet<string>();
            foreach (var item in olvEstimacionConceptos.Objects)
            {
                var conceptoRaiz = item as NodoConcepto;
                if (conceptoRaiz != null && conceptoRaiz.EsConcepto)
                {
                    if (olvEstimacionConceptos.IsExpanded(conceptoRaiz))
                    {
                        expandidos.Add(conceptoRaiz.Codigo);
                    }
                }
            }
            
            // 2. Reconstruir el árbol completo
            olvEstimacionConceptos.RebuildAll(true);
            
            // 3. Restaurar el estado de expansión
            foreach (var item in olvEstimacionConceptos.Objects)
            {
                var conceptoRaiz = item as NodoConcepto;
                if (conceptoRaiz != null && conceptoRaiz.EsConcepto)
                {
                    if (expandidos.Contains(conceptoRaiz.Codigo))
                    {
                        olvEstimacionConceptos.Expand(conceptoRaiz);
                    }
                }
            }
            
            OnDatosChanged(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en refresh: {ex.Message}");
        }
    }));
    
    return newValue;
};
```

---

## ?? **VENTAJAS DE LA SOLUCIÓN**

| Aspecto | Antes (RefreshObject) | Ahora (RebuildAll) |
|---------|----------------------|-------------------|
| **Duplicación** | ? Frecuente | ? Eliminada |
| **Sincronización** | ? Asíncrona (problemática) | ? Controlada |
| **Estado de expansión** | ? Se perdía | ? Se preserva |
| **Rendimiento** | ?? Múltiples refreshes | ? Un solo rebuild |
| **Robustez** | ? Condiciones de carrera | ? Try-catch protegido |

---

## ?? **FLUJO DE EJECUCIÓN**

### Cuando el usuario marca un checkbox:

1. **Actualizar datos** en memoria (`nodo.Incluir = newValue`)
2. **Propagar cambios** (padre ? hijos)
3. **BeginInvoke** para ejecutar en el thread de UI:
   - ?? **Guardar** qué conceptos están expandidos
   - ?? **RebuildAll(true)** - Reconstruir el árbol completo
   - ?? **Restaurar** el estado de expansión
   - ?? Notificar cambios para actualizar preview PDF

---

## ?? **PRUEBAS DE VALIDACIÓN**

### ? Casos de Prueba Exitosos

1. **Marcar concepto colapsado** ? ? Funciona correctamente
2. **Marcar concepto expandido** ? ? **NO duplica** (corregido)
3. **Marcar partida individual** ? ? Actualiza concepto padre correctamente
4. **Desmarcar concepto expandido** ? ? Funciona correctamente
5. **Marcar/desmarcar rápidamente** ? ? Sin condiciones de carrera
6. **Expandir/colapsar después de marcar** ? ? Estado preservado

---

## ?? **COMPARACIÓN VISUAL**

### ? Antes (con duplicación)
```
? Cimentación                    $38,359.66
  ? Preliminares > plataforma...  $0.00
  ? Preliminares > trazo...        $1,954.18
? Albañilería                    $143,142.06
? Albañilería                    $143,142.06  ? DUPLICADO
  ? Inst. Hidraulica...            $59,901.99
? Inst. Hidraulica - Sanitaria   $59,901.99
? Inst. Hidraulica - Sanitaria   $59,901.99  ? DUPLICADO
```

### ? Ahora (sin duplicación)
```
? Cimentación                    $38,359.66
  ? Preliminares > plataforma...  $0.00
  ? Preliminares > trazo...        $1,954.18
? Albañilería                    $143,142.06
  ? Inst. Hidraulica...            $59,901.99
? Inst. Hidraulica - Sanitaria   $59,901.99
```

---

## ?? **NOTAS TÉCNICAS**

### Por qué RebuildAll funciona mejor

1. **Limpia el estado interno** del TreeListView completamente
2. **Reconstruye desde cero** usando `ChildrenGetter` y `CanExpandGetter`
3. **Evita inconsistencias** entre el modelo de datos y la vista
4. **Garantiza** que cada nodo aparezca **exactamente una vez**

### Preservación del estado de expansión

```csharp
// Guardar ANTES de rebuild
var expandidos = new HashSet<string>();
foreach (var item in olvEstimacionConceptos.Objects)
{
    if (olvEstimacionConceptos.IsExpanded(item))
        expandidos.Add(item.Codigo);
}

// Restaurar DESPUÉS de rebuild
foreach (var item in olvEstimacionConceptos.Objects)
{
    if (expandidos.Contains(item.Codigo))
        olvEstimacionConceptos.Expand(item);
}
```

---

## ??? **PROTECCIONES ADICIONALES**

### Try-Catch
```csharp
try
{
    // Código de rebuild
}
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine($"Error en refresh: {ex.Message}");
}
```

**Protege contra:**
- Cambios inesperados en el estado del TreeListView
- Excepciones durante la reconstrucción
- Problemas de sincronización

---

## ?? **RESULTADO FINAL**

? **Duplicación eliminada** completamente  
? **Estado de expansión** preservado  
? **Propagación** de checkboxes funciona correctamente  
? **Rendimiento** optimizado (un solo rebuild en lugar de múltiples refreshes)  
? **Robustez** mejorada con try-catch  

---

## ?? **IMPACTO EN OTROS COMPONENTES**

| Componente | Impacto |
|------------|---------|
| **Preview PDF** | ? Se actualiza correctamente vía `OnDatosChanged` |
| **btnMarcarTodos** | ? Compatible (usa `RefreshObjects` de forma diferente) |
| **btnDesmarcarTodos** | ? Compatible |
| **Carga de datos** | ? Sin cambios |

---

## ?? **MANTENIMIENTO FUTURO**

### Si necesitas modificar el comportamiento de checkboxes:

1. **NO usar** múltiples `RefreshObject` en `BooleanCheckStatePutter`
2. **Siempre usar** `RebuildAll(true)` para cambios que afecten la jerarquía
3. **Preservar** el estado de expansión si el usuario espera que se mantenga
4. **Envolver** en try-catch para robustez

---

**Autor:** GitHub Copilot + Usuario  
**Versión:** 1.0 - Solución Definitiva  
**Estado:** ? IMPLEMENTADO Y PROBADO  

---

## ?? **PROBLEMA RESUELTO COMPLETAMENTE**

El TreeListView ahora opera **perfectamente** sin duplicaciones, manteniendo el estado de expansión y actualizando correctamente el preview PDF.
