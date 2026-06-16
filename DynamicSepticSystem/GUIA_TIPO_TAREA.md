# ??? Guía Rápida: Etiquetas de Tipo de Tarea

## ?? Descripción General

La funcionalidad de **Tipo de Tarea** permite clasificar los nodos **hijo** (nivel 2) en dos categorías visuales:

- ?? **Material** - Se muestra en color rojo
- ?? **Mano de Obra** - Se muestra en color verde

Esta clasificación facilita la identificación rápida del tipo de actividad en la estructura jerárquica.

---

## ?? Instalación

### Paso 1: Ejecutar Script SQL

Antes de usar esta funcionalidad, ejecuta el siguiente script en tu base de datos:

```
SQL_SCRIPTS\AgregarColumnaTipoTarea.sql
```

Este script:
- ? Agrega la columna `TipoTarea` a la tabla `TreeListData`
- ? Crea índices para mejorar el rendimiento
- ? Actualiza la vista `vw_TreeList_Completa`
- ? Crea procedimientos almacenados útiles

### Paso 2: Verificar la Instalación

El script mostrará un resumen al finalizar:

```
? Columna TipoTarea: OK
? Vista vw_TreeList_Completa: OK
? Procedimiento sp_TreeList_ObtenerPorTipoTarea: OK
? Procedimiento sp_TreeList_AsignarTipoTarea: OK
```

---

## ?? Uso en la Aplicación

### Asignar Tipo de Tarea

1. **Abrir el Editor de TreeList**
   ```csharp
   using (var form = new FormEditorTreeList(connectionString, "TreeListData"))
   {
       form.ShowDialog();
   }
   ```

2. **Seleccionar un nodo hijo** (nivel 2)
   - Los nodos padre y sub-padre NO pueden tener tipo de tarea

3. **Hacer clic en el botón** `Asignar Tipo de Tarea`

4. **Seleccionar el tipo** en el diálogo:
   - ?? **Material** - Para materiales, suministros, insumos
   - ?? **Mano de Obra** - Para trabajo, personal, servicios
   - ? **Sin especificar** - Para remover la clasificación

5. **Hacer clic en Aceptar**

### Visualización

Los nodos etiquetados se muestran con formato especial:

| Tipo | Color | Estilo |
|------|-------|--------|
| Material | ?? Rojo (#C0392B) | Texto en negrita |
| Mano de Obra | ?? Verde (#27AE60) | Texto en negrita |
| Sin especificar | ? Negro | Texto normal |

La columna **"Tipo Tarea"** muestra el texto correspondiente.

---

## ??? Consultas SQL Útiles

### Ver todos los nodos con su tipo de tarea

```sql
SELECT * FROM vw_TreeList_Completa 
ORDER BY Nivel, Orden
```

### Obtener solo nodos tipo Material

```sql
EXEC sp_TreeList_ObtenerPorTipoTarea @TipoTarea = 1
```

### Obtener solo nodos tipo Mano de Obra

```sql
EXEC sp_TreeList_ObtenerPorTipoTarea @TipoTarea = 2
```

### Asignar tipo mediante SQL

```sql
-- Asignar Material
EXEC sp_TreeList_AsignarTipoTarea @NodoID = 100, @TipoTarea = 1

-- Asignar Mano de Obra
EXEC sp_TreeList_AsignarTipoTarea @NodoID = 101, @TipoTarea = 2

-- Remover asignación
EXEC sp_TreeList_AsignarTipoTarea @NodoID = 102, @TipoTarea = 0
```

### Estadísticas por tipo

```sql
SELECT 
    CASE TipoTarea
        WHEN 0 THEN '? Sin especificar'
        WHEN 1 THEN '?? Material'
        WHEN 2 THEN '?? Mano de Obra'
    END as Tipo,
    COUNT(*) as Cantidad
FROM TreeListData
WHERE Nivel = 2
GROUP BY TipoTarea
ORDER BY TipoTarea
```

---

## ?? Ejemplos de Uso

### Ejemplo 1: Proyecto de Construcción

```
?? Proyecto A (Padre)
  ?? ??? Fase 1: Cimentación (Sub-Padre)
      ?? ?? Cemento Portland (Material)
      ?? ?? Acero de refuerzo (Material)
      ?? ?? Excavación manual (Mano de Obra)
      ?? ?? Armado de acero (Mano de Obra)
```

### Ejemplo 2: Presupuesto de Obra

```
?? Presupuesto General (Padre)
  ?? ?? Instalaciones (Sub-Padre)
      ?? ?? Tubería PVC (Material)
      ?? ?? Conexiones (Material)
      ?? ?? Instalador especializado (Mano de Obra)
      ?? ?? Ayudante general (Mano de Obra)
```

---

## ?? Valores de TipoTarea

| Valor | Constante | Descripción | Color |
|-------|-----------|-------------|-------|
| 0 | `TipoTarea.Ninguno` | Sin especificar | Negro |
| 1 | `TipoTarea.Material` | Material | ?? Rojo |
| 2 | `TipoTarea.ManoDeObra` | Mano de Obra | ?? Verde |

---

## ?? Restricciones

1. **Solo nodos hijo** (nivel 2) pueden tener tipo de tarea asignado
2. El botón **"Asignar Tipo de Tarea"** solo se habilita cuando:
   - Hay un nodo seleccionado
   - El nodo es de nivel 2 (hijo)
3. Los valores son excluyentes (solo un tipo por nodo)

---

## ?? Solución de Problemas

### El botón está deshabilitado

**Causa:** El nodo seleccionado no es de nivel 2

**Solución:** Selecciona un nodo **hijo** (el tercer nivel de la jerarquía)

### Los colores no se muestran

**Causa:** La columna TipoTarea no existe en la base de datos

**Solución:** Ejecuta el script `AgregarColumnaTipoTarea.sql`

### Error al guardar

**Causa:** La columna TipoTarea no existe en la tabla

**Solución:** 
1. Ejecuta el script de actualización
2. Verifica que la tabla tenga la columna:
   ```sql
   SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
   WHERE TABLE_NAME = 'TreeListData' 
   AND COLUMN_NAME = 'TipoTarea'
   ```

---

## ?? Notas Adicionales

- ? **Compatible con versiones anteriores:** Los nodos existentes tendrán `TipoTarea = 0` (Ninguno) por defecto
- ? **Auditoría:** El cambio de tipo actualiza automáticamente `FechaModificacion`
- ? **Rendimiento:** Se crean índices automáticamente para búsquedas rápidas
- ? **Validación:** El sistema valida que solo nodos hijo puedan tener tipo asignado

---

## ?? Soporte

Para dudas o problemas, contactar al equipo de desarrollo del Sistema Calandria Residencial.

**Versión:** 1.0  
**Fecha:** Enero 2025  
**Autor:** Sistema Calandria Residencial
