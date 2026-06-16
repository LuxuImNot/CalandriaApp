# ?? PRUEBAS DE VALIDACIÓN - CORRECCIÓN DE AVANCES

## ?? Objetivo

Validar que **PanelPrincipal** y **FormEstimacionConceptoMigrado** calculan **EXACTAMENTE** los mismos avances después de la corrección.

---

## ? Corrección Implementada

### Cambios en `PanelPrincipal.CargarResumenProgreso`:

1. ? **Carga completa de información de partidas**:
   - Ahora incluye: `EsDinamica`, `ValorM2Tunera`, `ValorM2Calandra`, `PrototiposAplicables`
   
2. ? **Carga de metros cuadrados desde `AvanceManualObra`**:
   - Lee columna `MetrosCuadrados` si existe
   
3. ? **Recalcula costo de partidas dinámicas**:
   - Si `EsDinamica = true` y `MetrosCuadrados > 0`:
     ```csharp
     decimal costoRecalculado = metrosCuadrados * valorM2;
     ```
   
4. ? **Ajusta total presupuestado**:
   - Suma las diferencias de partidas dinámicas al total
   
5. ? **Recalcula monto ejecutado para partidas dinámicas**:
   - Usa el costo recalculado en lugar del fijo

---

## ?? Plan de Pruebas

### Prueba 1: **Casa SIN Partidas Dinámicas**

**Objetivo**: Verificar que el cálculo básico sigue funcionando correctamente.

#### Pasos:
1. Seleccionar una casa en **PanelPrincipal** (ej: M1-L1)
2. Anotar valores mostrados:
   - Total Presupuestado: `___________`
   - Total Ejecutado: `___________`
   - Avance General: `___________`
3. Abrir **FormEstimacionConceptoMigrado**
4. Cargar la misma casa (M1-L1)
5. Comparar valores en `lblTotalPresupuestado`, `lblTotalEjecutado`, `lblAvanceGeneral`

#### Criterio de Éxito:
- ? Valores **idénticos** con máximo ±0.1% de diferencia por redondeos

---

### Prueba 2: **Casa CON Partidas Dinámicas**

**Objetivo**: Verificar que las partidas dinámicas se calculan correctamente.

#### Pre-requisitos:
- Al menos una partida con `EsDinamica = 1` en `PresupuestoObra`
- Metros cuadrados registrados en `AvanceManualObra.MetrosCuadrados` > 0

#### Pasos:
1. Identificar una casa con partidas dinámicas:
   ```sql
   SELECT TOP 1 Manzana, Lote 
   FROM AvanceManualObra 
   WHERE MetrosCuadrados > 0
   ```
2. Seleccionar esa casa en **PanelPrincipal**
3. Anotar valores:
   - Total Presupuestado: `___________`
   - Total Ejecutado: `___________`
   - Avance General: `___________`
4. Abrir **FormEstimacionConceptoMigrado**
5. Cargar la misma casa
6. Comparar valores

#### Criterio de Éxito:
- ? Valores **idénticos** en ambos formularios
- ? Total Presupuestado es **MAYOR** que si se usara costo fijo (si m² > límite)

---

### Prueba 3: **Cambio de Metros Cuadrados**

**Objetivo**: Verificar que los cambios en m² se reflejan correctamente.

#### Pasos:
1. Abrir **FormEstimacionConceptoMigrado**
2. Seleccionar una casa (ej: M1-L1)
3. Modificar m² de una partida dinámica
4. Guardar cambios
5. Volver a **PanelPrincipal**
6. Seleccionar la misma casa
7. Verificar que el **Total Ejecutado** cambió

#### Criterio de Éxito:
- ? Los cambios en m² se reflejan **inmediatamente** en PanelPrincipal
- ? Total Ejecutado se actualiza correctamente

---

### Prueba 4: **Prototipos Diferentes (Tunera vs Calandra)**

**Objetivo**: Verificar que se usa la columna de costo correcta según prototipo.

#### Pasos:
1. Seleccionar una casa **Tunera** (ej: M1-L1)
2. Anotar valores de PanelPrincipal y FormEstimacionConceptoMigrado
3. Seleccionar una casa **Calandra** (ej: M2-L1)
4. Anotar valores de ambos formularios

#### Criterio de Éxito:
- ? Casa **Tunera** usa `CostoTunera` / `ValorM2Tunera`
- ? Casa **Calandra** usa `CostoCalandra` / `ValorM2Calandra`
- ? Valores son **idénticos** en ambos formularios para cada casa

---

## ?? Tabla de Resultados

| # | Casa | Formulario | Total Presupuestado | Total Ejecutado | Avance General | ¿Coincide? |
|---|------|------------|---------------------|-----------------|----------------|------------|
| 1 | M1-L1 | PanelPrincipal | | | | |
| 1 | M1-L1 | FormEstimacion | | | | ?/? |
| 2 | M2-L3 | PanelPrincipal | | | | |
| 2 | M2-L3 | FormEstimacion | | | | ?/? |
| 3 | M3-L5 | PanelPrincipal | | | | |
| 3 | M3-L5 | FormEstimacion | | | | ?/? |

---

## ?? Verificación en Consola de Debug

Después de cargar una casa en **PanelPrincipal**, buscar en la ventana de Output (Debug) el siguiente mensaje:

```
+----------------------------------------------------------------------+
?  ?????? RESUMEN DE PROGRESO (PanelPrincipal) - CORREGIDO             ?
?----------------------------------------------------------------------?
?  Casa: M1-L1
?  Prototipo: TUNERA
?  Columna de costo usada: CostoTunera
?----------------------------------------------------------------------?
?  ?? Total Presupuestado: $XXX,XXX.XX (ajustado por dinámicas)
?  ? Total Ejecutado:     $XXX,XXX.XX
?  ?? Avance General:      XX.X%
?----------------------------------------------------------------------?
?  ?? Total Partidas:      XXX (filtradas por prototipo)
?  ? Completadas:        XX
?  ? Pendientes:          XX
?  ?? Partidas dinámicas ajustadas: X
?  ?? Total ajuste dinámico: $XXX.XX
+----------------------------------------------------------------------+
```

**Puntos a verificar**:
1. ? `Partidas dinámicas ajustadas` > 0 si hay partidas con m²
2. ? `Total ajuste dinámico` muestra el monto agregado/restado
3. ? `Total Presupuestado` incluye el ajuste

---

## ?? Problemas Comunes

### ? Avances NO coinciden

**Posibles causas**:
1. Partidas dinámicas sin `MetrosCuadrados` en BD
2. Columnas `EsDinamica`, `ValorM2Tunera`, `ValorM2Calandra` no existen
3. Filtro de `PrototiposAplicables` diferente

**Solución**:
- Ejecutar script de migración para agregar columnas faltantes
- Verificar que `MetrosCuadrados` se guarda correctamente en `GuardarAvancePartidaEnBD`

---

### ?? Total Presupuestado negativo o muy alto

**Causa**: Ajustes dinámicos incorrectos

**Solución**:
- Verificar que `ValorM2` sea razonable (no 0, no negativo)
- Validar que `MetrosCuadrados` no exceda límites

---

## ?? Registro de Pruebas

| Fecha | Tester | Casa | Resultado | Observaciones |
|-------|--------|------|-----------|---------------|
| | | | ?/? | |
| | | | ?/? | |
| | | | ?/? | |

---

## ? Conclusión

Después de completar todas las pruebas:

- [ ] Todos los casos de prueba pasaron exitosamente
- [ ] PanelPrincipal y FormEstimacionConceptoMigrado muestran valores idénticos
- [ ] Partidas dinámicas se calculan correctamente
- [ ] Cambios en m² se reflejan en ambos formularios

**Estado**: ? Pendiente / ? Aprobado / ? Requiere correcciones

---

## ?? Soporte

Si encuentras discrepancias después de aplicar la corrección:

1. Capturar screenshot de ambos formularios mostrando valores diferentes
2. Ejecutar query para verificar datos en BD:
   ```sql
   SELECT 
       po.WBS_Correcto,
       po.EsDinamica,
       po.CostoTunera,
       po.CostoCalandra,
       po.ValorM2Tunera,
       po.ValorM2Calandra,
       amo.MetrosCuadrados,
       amo.AvancePorcentaje,
       amo.MontoEjecutado
   FROM PresupuestoObra po
   LEFT JOIN AvanceManualObra amo 
       ON po.WBS_Correcto = amo.WBS 
       AND amo.Manzana = 'X' 
       AND amo.Lote = 'Y'
   WHERE po.EsDinamica = 1
   ```
3. Copiar logs de debug de Output window
4. Reportar en documento `ANALISIS_DISCREPANCIA_AVANCES.md`

---

**Última actualización**: 2024-12-XX
