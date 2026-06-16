# Guía Rápida: Estado Activado/Desactivado en Destajos

## ¿Qué cambió?

Los destajos (Nivel 1) ahora tienen **dos estados independientes**:

1. **"Activo"** - Checkbox seleccionado (antiguo)
2. **"DesatajoActivado"** - Estado de completitud (NUEVO)

Un destajo está **completamente activado** cuando tiene cuadrilla asignada.

## Estados Posibles para un Destajo

| Estado | Activa | Cuadrilla | DesatajoActivado | Color | Puede PDF? |
|--------|--------|-----------|------------------|-------|-----------|
| Inactivo | ? | - | ? | Gris | ? No |
| Pendiente de cuadrilla | ? | - | ? | Gris | ? No |
| Con cuadrilla pero incompleto | ? | ? | ? | Amarillo | ? No |
| **Completamente Activado** | ? | ? | ? | **Verde** | **? Sí** |

## Columna Nueva en la Interfaz

Se agregó la columna **"Estado"** al TreeList que muestra:
- `? Activado` (verde) - Destajo completamente activado
- `? Desactivado` (gris) - Destajo no completamente activado
- Vacío para otros niveles

## Colores Visuales

### Para Destajos (Nivel 1):

- **?? Verde claro** - Completamente activado (con cuadrilla y DesatajoActivado = true)
- **?? Amarillo suave** - Tiene cuadrilla pero DesatajoActivado = false
- **?? Gris azulado** - Sin cuadrilla

## Flujo de Uso

### Activar un Destajo

```
1. Cargar Manzana/Lote
2. Buscar el destajo (Nivel 1)
3. MARCAR el checkbox ?
   ?? Se abre "Asignar Cuadrilla"
   ?? Seleccionar cuadrilla
   ?? Hacer clic en OK
4. La fila se pone VERDE ? Activado
5. PDF se genera automáticamente
```

### Desactivar un Destajo

```
1. DESMARCAR el checkbox ?
2. Se limpia:
   ?? Cuadrilla asignada
   ?? Estado DesatajoActivado
   ?? Color vuelve a gris
```

### Regenerar PDF de Destajo Activado

```
1. Clic derecho en destajo VERDE
2. Seleccionar "Generar / Regenerar PDF"
3. Se abre preview con PDF
```

> ?? **Solo funciona si el destajo está completamente activado (verde)**

## Estadísticas

La barra de estado ahora muestra:
```
Destajos activos: 3 de 5 · Con cuadrilla: 2 · Completamente activados: 2 · Importe: $500.00
```

## Persistencia en BD

Todos los estados se guardan en `ActivacionTareasRuta`:
- `Activa` - Checkbox seleccionado
- `CuadrillaAsignada` - Código de cuadrilla
- `DesatajoActivado` - TRUE si completamente activado (NEW)

## Validaciones

### Para Generar PDF, el destajo DEBE:
1. ? Estar marcado (Activa = true)
2. ? Tener cuadrilla asignada
3. ? Estar completamente activado (DesatajoActivado = true)

Si falta cualquiera de estos, se muestra:
```
"El destajo no está completamente activado. 
Debe tener cuadrilla asignada."
```

## Botones de Acción Rápida

- **Marcar Todos** - Marca todos los destajos
- **Desmarcar Todos** - Limpia todo (incluyendo DesatajoActivado)
- **Marcar por Nivel** - Marca destajos de un nivel específico
- **Desmarcar por Nivel** - Limpia destajos de un nivel específico
- **Guardar** - Persiste todos los estados en BD

## Casos de Uso

### Caso 1: Activar destajo con cuadrilla rápidamente
```
1. Marcar destajo
2. Asignar cuadrilla
3. ? PDF listo automáticamente
```

### Caso 2: Cambiar cuadrilla de destajo
```
1. Clic derecho en destajo
2. "Asignar / Cambiar Cuadrilla"
3. Seleccionar nueva cuadrilla
4. ? Destajo sigue activado
```

### Caso 3: Dejar pendiente un destajo
```
1. Marcar destajo (queda en amarillo)
2. NO asignar cuadrilla o cancelar
3. ?? No se puede generar PDF
4. Guardar (registra como pendiente)
```

## FAQ

**P: ¿Qué pasa si cargo una casa que ya tiene destajos activados?**
A: Se cargan los estados guardados, incluyendo DesatajoActivado, CuadrillaAsignada, etc.

**P: ¿Se puede desactivar un destajo que ya tiene PDF generado?**
A: Sí. Se limpia la cuadrilla y el estado. El PDF anterior permanece en disco pero no se puede regenerar.

**P: ¿Qué significa "? Desactivado"?**
A: El destajo no está listo para PDF (faltan datos o no está marcado).

**P: ¿Los colores cambian automáticamente?**
A: Sí, cuando se asigna/limpia la cuadrilla o se marca/desmarca.

**P: ¿Se pierden los datos si actualizo la app?**
A: No. Los estados se guardan en BD. La migración agrega la columna automáticamente.
