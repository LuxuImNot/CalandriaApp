# ?? GUÍA RÁPIDA - Pestaña INVENTARIO

## ? Acceso Rápido

```
Menú Principal ? ALMACÉN ? Gestión de Almacén ? Pestaña INVENTARIO
```

---

## ?? ¿Qué Hace?

Muestra **TODAS** las entradas de almacén registradas con:
- ? Búsqueda en tiempo real
- ? Estadísticas automáticas
- ? Exportación a Excel profesional

---

## ?? Búsqueda Rápida

| Escribe | Para Buscar |
|---------|------------|
| `CEM001` | Por clave de insumo |
| `Cemento` | Por descripción |
| `OC-123` | Por folio de orden |
| `M5` | Por manzana |
| `L45` | Por lote |
| `TUNERA` | Por prototipo |
| `Juan` | Por usuario |

---

## ?? Exportación Excel

1. **Click**: `?? EXPORTAR A EXCEL`
2. **Guardar**: Selecciona ubicación
3. **Listo**: Archivo `Inventario_Almacen_YYYYMMDD_HHMMSS.xlsx`

### Formato del Excel
- ? Encabezado corporativo Calandria
- ? Colores corporativos (#B36C2E)
- ? Formato moneda automático
- ? Anchos de columna optimizados

---

## ?? Columnas Principales

| Columna | Info |
|---------|------|
| ?? **Fecha** | Cuándo se registró |
| **Estado** | ? Completo / ?? Parcial / ?? Discrepancia |
| ?? **Folio OC** | Orden de compra |
| ?? **Clave** | Código del insumo |
| ?? **Descripción** | Nombre completo |
| ?? **P.U.** | Precio unitario |
| ?? **Importe** | Total (Cantidad × P.U.) |
| ??? **Manzana** | Destino |
| ?? **Lote** | Destino |
| ?? **Usuario** | Quién lo registró |

---

## ?? Estadísticas

### Label Inferior Muestra:
```
?? Total de entradas: 150 | ?? Importe total: $450,000.00
```

### Si Hay Discrepancias:
```
?? Total: 150 | ?? Importe: $450,000.00 | ?? Con discrepancia: 5
```
(Texto en color naranja)

---

## ?? Estados de Entrada

| Icono | Estado | Significado |
|-------|--------|-------------|
| ? | COMPLETO | Cantidad recibida = Cantidad ordenada |
| ?? | PARCIAL | Recibido menos de lo ordenado |
| ?? | DISCREPANCIA | Hay justificación registrada |

---

## ?? Actualizar Datos

**Click**: `?? ACTUALIZAR`

Útil cuando:
- Otro usuario registró nuevas entradas
- Necesitas los datos más recientes
- Después de modificar desde otra pestaña

---

## ?? Consejos de Uso

### ? Auditoría Rápida
```
1. Buscar por mes: "01/2025"
2. Ver totales en estadísticas
3. Exportar a Excel
4. Enviar a contabilidad
```

### ? Encontrar Discrepancias
```
1. Observar contador de discrepancias
2. Buscar: "??"
3. Revisar columna Justificación
4. Exportar reporte
```

### ? Verificar Prototipo
```
1. Buscar: "TUNERA"
2. Ver todas las entradas
3. Verificar importes
4. Comparar con presupuesto
```

---

## ?? Solución Rápida de Problemas

### Problema: No hay datos
**Solución**: Primero registra entradas desde pestaña ENTRADAS

### Problema: El filtro no actualiza
**Solución**: Borra el texto de búsqueda y escribe de nuevo

### Problema: Error al exportar
**Solución**: Verifica que tengas permisos de escritura en la carpeta destino

---

## ?? Atajos de Teclado

| Tecla | Acción |
|-------|--------|
| `Ctrl + F` | Foco en búsqueda (si implementado) |
| `F5` | Actualizar (via botón) |
| `Enter` | Confirmar exportación |

---

## ?? Layout Responsivo

? **Mínimo**: 1024×600  
? **Recomendado**: 1280×720  
? **Óptimo**: 1920×1080

El DataGridView se expande automáticamente con la ventana.

---

## ?? Permisos Necesarios

- ?? Lectura: Tabla `EntradasAlmacen`
- ?? Escritura: Carpeta destino (solo para exportar)

---

## ?? Rendimiento

| Operación | Tiempo |
|-----------|--------|
| Cargar datos | < 2 seg |
| Buscar | Instantáneo |
| Exportar Excel | < 5 seg |

---

## ?? Tema Visual

**Colores Corporativos Calandria**:
- ?? Principal: `#B36C2E`
- ?? Éxito: Verde
- ?? Advertencia: Naranja

**Tipografía**: Segoe UI

---

## ?? Documentación Completa

Para más detalles, ver:
- `DOCUMENTACION_Inventario_FormAlmacen.md` (Completa)
- `RESUMEN_Inventario_FormAlmacen.md` (Resumen)

---

## ? Checklist de Uso

- [ ] Abrir FormAlmacen
- [ ] Ir a pestaña INVENTARIO
- [ ] Verificar que carguen datos
- [ ] Probar búsqueda
- [ ] Ver estadísticas
- [ ] Exportar Excel de prueba
- [ ] Verificar formato del Excel

---

**¡LISTO PARA USAR!** ??

---

**Versión**: 1.1  
**Fecha**: Enero 2025  
**Estado**: ? PRODUCCIÓN
