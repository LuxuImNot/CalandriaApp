# Validación de Cambios: Estado Activado/Desactivado en Destajos

## Checklist de Pruebas

### 1. Estructura de Datos
- [ ] Propiedad `DesatajoActivado` existe en `ItemTareaActivacion`
- [ ] Columna `DesatajoActivado` existe en tabla `ActivacionTareasRuta` (BIT DEFAULT 0)
- [ ] Migración SQL se ejecuta correctamente
- [ ] No hay errores de compilación

### 2. Interfaz de Usuario

#### Columna Nueva
- [ ] Columna "Estado" visible en TreeList
- [ ] Muestra "? Activado" cuando DesatajoActivado = true
- [ ] Muestra "? Desactivado" cuando DesatajoActivado = false
- [ ] No muestra nada para niveles 0 y 2
- [ ] Ancho correcto (90 píxeles)

#### Colores
- [ ] Verde claro: Destajos completamente activados (DesatajoActivado=true + Cuadrilla)
- [ ] Amarillo suave: Destajos con cuadrilla pero sin DesatajoActivado
- [ ] Gris azulado: Destajos sin cuadrilla
- [ ] Los colores cambian dinámicamente al asignar/limpiar cuadrilla

#### Estadísticas
- [ ] Barra muestra "Completamente activados: X"
- [ ] ProgressBar refleja cantidad de completamente activados (no solo activos)
- [ ] Porcentaje actualiza correctamente

### 3. Flujo de Activación

#### Marcar Destajo
- [ ] Al marcar checkbox ? abre FormAsignarCuadrilla
- [ ] Si se asigna cuadrilla:
  - [ ] Activa = true
  - [ ] CuadrillaAsignada = "CODIGO"
  - [ ] DesatajoActivado = true ?
  - [ ] Fila se pone VERDE
  - [ ] PDF se genera automáticamente
- [ ] Si se cancela:
  - [ ] Activa = false
  - [ ] DesatajoActivado = false
  - [ ] Cuadrilla se limpia
  - [ ] Fila vuelve a gris

#### Desmarcar Destajo
- [ ] Al desmarcar checkbox:
  - [ ] Activa = false
  - [ ] DesatajoActivado = false ?
  - [ ] CuadrillaAsignada = ""
  - [ ] Fila vuelve a gris

#### Cambiar Cuadrilla
- [ ] Clic derecho ? "Asignar / Cambiar Cuadrilla"
- [ ] Seleccionar nueva cuadrilla:
  - [ ] DesatajoActivado sigue siendo true
  - [ ] CuadrillaAsignada se actualiza
  - [ ] Fila permanece VERDE

### 4. Validación PDF

#### Generar PDF
- [ ] No permite generar PDF si DesatajoActivado = false
- [ ] No permite si CuadrillaAsignada está vacía
- [ ] Muestra error: "El destajo no está completamente activado..."
- [ ] Solo permite si AMBAS condiciones son true

#### Regenerar PDF
- [ ] Botón "Generar / Regenerar PDF" solo visible si DesatajoActivado = true
- [ ] Si se intenta desde destajo "? Desactivado":
  - [ ] Muestra error
  - [ ] No genera PDF

### 5. Botones de Acción Rápida

#### Marcar Todos
- [ ] Marca todos los destajos
- [ ] DesatajoActivado permanece con su valor anterior (no cambia)
- [ ] No abre FormAsignarCuadrilla

#### Desmarcar Todos
- [ ] Desmarca todos
- [ ] DesatajoActivado = false para TODOS ?
- [ ] Limpia cuadrilla para destajos (Nivel 1)
- [ ] Activa = false para TODOS

#### Desmarcar por Nivel
- [ ] Selecciona Nivel 1
- [ ] Desmarca solo destajos de Nivel 1
- [ ] DesatajoActivado = false para esos destajos ?
- [ ] Limpia cuadrilla para esos destajos
- [ ] Otros niveles no se ven afectados

### 6. Persistencia en BD

#### Guardar Configuración
- [ ] Botón "Guardar" persiste DesatajoActivado para cada destajo
- [ ] Se ejecuta INSERT correcto con @desatActivado
- [ ] No hay errores SQL
- [ ] Mensaje de éxito aparece

#### Cargar Configuración
- [ ] Al cargar Manzana/Lote nuevamente:
  - [ ] DesatajoActivado se carga desde BD
  - [ ] Colores se aplican correctamente
  - [ ] Estados coinciden con lo guardado
  - [ ] CuadrillaAsignada se restaura

#### Migración Automática
- [ ] Al abrir una casa:
  - [ ] Si columna DesatajoActivado no existe, se crea automáticamente
  - [ ] No lanza error
  - [ ] Tabla queda con estructura correcta

### 7. Casos Edge

#### Destajo con Cuadrilla pero DesatajoActivado = false
- [ ] Mostrar en AMARILLO (no verde)
- [ ] "Generar PDF" muestra error
- [ ] Permite cambiar cuadrilla

#### Destajo Activo pero sin Cuadrilla
- [ ] Mostrar en GRIS (no verde)
- [ ] "Generar PDF" muestra error
- [ ] Permite asignar cuadrilla

#### Cargar BD antigua sin DesatajoActivado
- [ ] No lanza error
- [ ] Columna se crea automáticamente
- [ ] Todos quedan con DesatajoActivado = false (default)

### 8. Performance

- [ ] No hay lag al marcar/desmarcar destajos
- [ ] Colores cambian instantáneamente
- [ ] Cargar casa con muchos destajos < 2 segundos
- [ ] Guardar configuración < 1 segundo

### 9. Integración

#### FormAsignarCuadrilla
- [ ] Se abre correctamente al marcar destajo
- [ ] Asignación de cuadrilla funciona como antes
- [ ] Después de asignar: DesatajoActivado = true

#### FormPdfPreview
- [ ] Se abre automáticamente tras asignar cuadrilla
- [ ] PDF contiene información correcta
- [ ] No hay errores al guardar PDF

#### Estadísticas
- [ ] Texto actualiza correctamente tras cada cambio
- [ ] ProgressBar refleja estado real
- [ ] Número de "completamente activados" es preciso

### 10. Regresión

- [ ] Otros niveles (0, 2) no se ven afectados
- [ ] Checkbox de "Activa" sigue funcionando
- [ ] Búsqueda/filtro sigue funcionando
- [ ] Contexto visual de la casa se mantiene
- [ ] Ordenamiento y expansión del árbol funciona

---

## Especificaciones

### Columnas de Estado por Nivel

**Nivel 0 (Padre):**
- Activa: Sí (checkbox)
- DesatajoActivado: No se aplica (vacío)
- Cuadrilla: N/A

**Nivel 1 (Sub-Padre / Destajo):**
- Activa: Sí (checkbox)
- DesatajoActivado: **SÍ (NEW)** - True si tiene cuadrilla
- Cuadrilla: Sí (código)
- Estado: "? Activado" o "? Desactivado"

**Nivel 2 (Hijo):**
- Activa: Sí (checkbox)
- DesatajoActivado: No se aplica (vacío)
- Cuadrilla: N/A

### Validación Lógica

```
PDF_PERMITIDO = DesatajoActivado AND CuadrillaAsignada.IsNotEmpty()

// Para asignar DesatajoActivado = true:
- Nivel DEBE ser 1
- Cuadrilla DEBE estar asignada
- Activa DEBE ser true
```

### Estados de Transición

```
[Inactivo] ? (marcar) ? [Asignando Cuadrilla...]
                         ?? (OK) ? [Activado] ?
                         ?? (Cancel) ? [Inactivo]

[Activado] ? (desmarcar) ? [Inactivo]

[Activado] ? (cambiar cuadrilla) ? [Activado con nueva cuadrilla]
```

---

## Notas

- **DesatajoActivado** es `true` solo cuando hay cuadrilla asignada
- **Activa** es `true` cuando el checkbox está marcado
- Son independientes: un destajo puede estar Activo pero no DesatajoActivado
- La persistencia incluye ambos estados
- El PDF requiere ambos: Activo AND DesatajoActivado AND Cuadrilla
