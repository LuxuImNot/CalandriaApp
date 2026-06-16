# ?? FORMULARIO ACTIVAR TAREAS TREELIST
## Guía Completa - FormActivarTareasTreeList

### ?? Propósito
Formulario para **activar/desactivar tareas** del TreeList (Ruta Calandria o Tunera) según la **Manzana/Lote** de la casa seleccionada.

Similar a `FormEstimacionConcepto`, pero en lugar de mostrar conceptos, gestiona la **activación/desactivación de tareas** en el árbol jerárquico.

---

## ?? Características

### 1. **Selección de Casa**
- Combo de **Manzana**
- Combo de **Lote**
- Botón **Cargar Tareas**

```
?? Manzana: [5      ?] Lote: [12      ?] [?? Cargar Tareas]
```

### 2. **Visualización de Tareas**
Muestra todas las tareas en una estructura jerárquica con:
- ? **Checkbox** para activar/desactivar
- **Nombre** de la tarea
- **Tipo** (Padre, Sub-Padre, Hijo)
- **Tipo Tarea** (Material o Mano de Obra)
- **Descripción** de la tarea
- **Contador** (solo para Sub-Padres)

```
? | # | Nombre              | Tipo       | Tipo Tarea  | Descripción
? | 1 | Excavación          | Sub-Padre  | Material    | Preparación del terreno
? | 2 | Colocación tuberías | Sub-Padre  | Mano Obra   | Instalación de sistemas
```

### 3. **Controles de Activación**
- **? Marcar Todo**: Activa todas las tareas
- **? Desmarcar Todo**: Desactiva todas las tareas
- **+ Por Nivel**: Marca tareas de un nivel específico
- **- Por Nivel**: Desmarca tareas de un nivel específico

### 4. **Indicadores de Progreso**
- Barra de progreso visual
- Estadísticas en tiempo real: "Activas: 8 | Inactivas: 2 | ..."
- Porcentaje de tareas activas

```
Activas: 8 | Inactivas: 2 | Padres: 3 | Sub-Padres: 5 | Hijos: 2
[??????????] 80% activas
```

### 5. **Guardado de Configuración**
- **?? Guardar**: Guarda el estado de activación en BD para la casa
- **? Cerrar**: Cierra el formulario

---

## ?? Flujo de Uso

### Paso 1: Seleccionar Casa
```
1. Abre FormActivarTareasTreeList
2. Selecciona Manzana: "5"
3. Selecciona Lote: "12"
4. Haz clic en "?? Cargar Tareas"
```

### Paso 2: Revisar Tareas
```
Se carga el TreeList con:
- Todas las tareas de la ruta (Calandria o Tunera)
- Estado guardado anteriormente (si existe)
- Colores según nivel:
  * Azul oscuro: Padres
  * Azul claro: Sub-Padres
  * Rojo/Verde: Hijos (según tipo de tarea)
```

### Paso 3: Activar/Desactivar
```
Opción A: Individual
  - Haz clic en checkbox de cada tarea

Opción B: Todo
  - Haz clic en "? Marcar Todo"
  - O haz clic en "? Desmarcar Todo"

Opción C: Por Nivel
  - Haz clic en "+ Por Nivel"
  - Selecciona el nivel (Padre, Sub-Padre, Hijo)
  - Se marcan solo las tareas de ese nivel
```

### Paso 4: Guardar
```
1. Revisa las estadísticas (barra de progreso)
2. Haz clic en "?? Guardar"
3. Se guarda en BD: ActivacionTareasRuta
4. Confirmación: "? Configuración guardada para M5-L12"
```

---

## ??? Base de Datos

### Tabla: `ActivacionTareasRuta`

```sql
CREATE TABLE ActivacionTareasRuta (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Manzana NVARCHAR(10) NOT NULL,           -- M5
    Lote NVARCHAR(10) NOT NULL,              -- L12
    Prototipo NVARCHAR(50),                  -- Calandria o Tunera
    Ruta NVARCHAR(50) NOT NULL,              -- RutaCalandraDestajo
    NodoID INT NOT NULL,                     -- ID del nodo en la ruta
    NombreTarea NVARCHAR(200),               -- Nombre de la tarea
    Activa BIT NOT NULL DEFAULT 1,           -- 1 = Activa, 0 = Inactiva
    FechaActualizacion DATETIME DEFAULT GETDATE(),
    UsuarioModificacion NVARCHAR(100)
);
```

### Índices
- `IX_ActivacionTareasRuta_ManzanaLote`: Búsqueda rápida por casa
- `IX_ActivacionTareasRuta_Ruta`: Búsqueda rápida por ruta
- `IX_ActivacionTareasRuta_Activa`: Búsqueda de tareas activas

---

## ?? Stored Procedures

### 1. `sp_ObtenerTareasActivasPorCasa`
Obtiene las tareas activas de una casa

```sql
EXEC sp_ObtenerTareasActivasPorCasa @Manzana='5', @Lote='12'
```

**Retorna:**
```
NodoID | NombreTarea              | Ruta                  | Activa | FechaActualizacion
1      | Excavación               | RutaCalandraDestajo   | 1      | 2024-01-15 10:30
2      | Colocación tuberías      | RutaCalandraDestajo   | 1      | 2024-01-15 10:30
```

### 2. `sp_GuardarActivacionTarea`
Guarda una tarea individual

```sql
EXEC sp_GuardarActivacionTarea 
    @Manzana='5',
    @Lote='12',
    @Prototipo='Calandria',
    @Ruta='RutaCalandraDestajo',
    @NodoID=1,
    @NombreTarea='Excavación',
    @Activa=1
```

### 3. `sp_LimpiarActivacionesCasa`
Limpia activaciones de una casa

```sql
EXEC sp_LimpiarActivacionesCasa @Manzana='5', @Lote='12'
```

---

## ?? Consultas Útiles

### Ver activaciones guardadas
```sql
SELECT * FROM ActivacionTareasRuta 
WHERE Manzana = '5' AND Lote = '12'
ORDER BY Ruta, NodoID
```

### Ver tareas activas por casa
```sql
SELECT * FROM vw_TareasActivasPorCasa 
WHERE Manzana = '5' AND Lote = '12'
```

### Ver estadísticas por prototipo
```sql
SELECT 
    Prototipo,
    COUNT(*) AS TotalTareas,
    SUM(CASE WHEN Activa = 1 THEN 1 ELSE 0 END) AS TareasActivas,
    CAST(SUM(CASE WHEN Activa = 1 THEN 1 ELSE 0 END) * 100.0 / 
        COUNT(*) AS DECIMAL(5,2)) AS PorcentajeActivas
FROM ActivacionTareasRuta
GROUP BY Prototipo
```

---

## ?? Colores y Estilos

### Niveles de Nodos
| Nivel | Color | Descripción |
|-------|-------|-------------|
| 0 | Azul Oscuro `#C8E6FF` | Padre |
| 1 | Azul Claro `#E6F5FF` | Sub-Padre |
| 2 | Rojo `#C0392B` (Material) | Hijo Material |
| 2 | Verde `#27AE60` (Mano de Obra) | Hijo Mano de Obra |

### Botones
| Botón | Color | Significado |
|-------|-------|-------------|
| ? Marcar Todo | Verde `#2ECC71` | Activar |
| ? Desmarcar Todo | Rojo `#E74C3C` | Desactivar |
| + Por Nivel | Azul `#3498DB` | Marcar por criterio |
| - Por Nivel | Gris `#95A5A6` | Desmarcar por criterio |
| ?? Guardar | Púrpura `#9B59B6` | Guardar cambios |
| ? Cerrar | Gris Oscuro `#7F8C8D` | Cerrar formulario |

---

## ?? Integración

### Agregar al menú principal
```csharp
// En PanelPrincipal.cs o donde corresponda
var btnTareas = new Button();
btnTareas.Text = "?? Activar Tareas";
btnTareas.Click += (s, e) =>
{
    using (var frm = new FormActivarTareasTreeList())
    {
        frm.ShowDialog();
    }
};
```

### Ejemplo de uso desde otro formulario
```csharp
// Desde FormEditorTreeList
private void btnActivarTareas_Click(object sender, EventArgs e)
{
    using (var frm = new FormActivarTareasTreeList())
    {
        frm.ShowDialog();
    }
}
```

---

## ?? Configuración

### Ruta automática según prototipo
- **Calandria** ? `RutaCalandraDestajo`
- **Tunera** ? `RutaTuneraDestajo`

Se determina automáticamente consultando `InventarioCasas.Prototipo`

---

## ?? Notas Importantes

### 1. Estructura Jerárquica
```
Nivel 0: PADRE (Ej: Instalación)
?? Nivel 1: SUB-PADRE (Ej: Tuberías)
?  ?? Nivel 2: HIJO (Ej: Colocación de tubería 50mm)
?  ?? Nivel 2: HIJO (Ej: Colocación de tubería 75mm)
?? Nivel 1: SUB-PADRE (Ej: Válvulas)
   ?? Nivel 2: HIJO (Ej: Válvula de control)
```

### 2. Tipo de Tarea
Solo los **Hijos (Nivel 2)** tienen tipo de tarea:
- **Material**: Se muestra en rojo
- **Mano de Obra**: Se muestra en verde

### 3. Persistencia
Las activaciones se guardan **por casa** (Manzana/Lote):
- Diferentes casas pueden tener diferentes activaciones
- Al reabrir, se cargan las activaciones guardadas

---

## ?? Solución de Problemas

### "No se encuentran tareas"
**Causa**: Manzana/Lote no existe en `InventarioCasas`
**Solución**: Verifica que la casa exista en inventario

### "El prototipo no se detecta"
**Causa**: `InventarioCasas.Prototipo` es NULL
**Solución**: Revisa que la casa tenga prototipo asignado

### "Los cambios no se guardan"
**Causa**: La tabla `ActivacionTareasRuta` no existe
**Solución**: Ejecuta `CrearTablaActivacionTareasRuta.sql`

### "Cargar Tareas está deshabilitado"
**Causa**: No hay Manzana/Lote seleccionados
**Solución**: Selecciona ambos combos antes de hacer clic

---

## ?? Archivos Relacionados

- `FormActivarTareasTreeList.cs`: Lógica principal
- `FormActivarTareasTreeList.Designer.cs`: Interfaz gráfica
- `DialogSeleccionarNivel.cs`: Diálogo para seleccionar nivel
- `CrearTablaActivacionTareasRuta.sql`: Script SQL
- `NodoTree.cs`: Modelo de datos para nodos

---

## ? Checklist

- [ ] Ejecutar script SQL `CrearTablaActivacionTareasRuta.sql`
- [ ] Compilar proyecto
- [ ] Abrir FormActivarTareasTreeList
- [ ] Seleccionar Manzana y Lote
- [ ] Haz clic en "?? Cargar Tareas"
- [ ] Prueba botones: Marcar Todo, Desmarcar Todo, Por Nivel
- [ ] Prueba Guardar
- [ ] Verifica BD: `SELECT * FROM ActivacionTareasRuta`
- [ ] Recarga la casa y verifica que persisten los cambios

---

## ?? Casos de Uso

### Caso 1: Activar solo Sub-Padres
```
1. Cargar Tareas
2. Haz clic en "? Desmarcar Todo"
3. Haz clic en "+ Por Nivel"
4. Selecciona "Sub-Padres (Nivel 1)"
5. Resultado: Solo Sub-Padres están activos
6. Guardar
```

### Caso 2: Activar solo Materiales
```
1. Cargar Tareas
2. Desmarcar todos
3. Hacer click en cada hijo de tipo "Material"
4. O hacerlo manual si no hay mucha cantidad
5. Guardar
```

### Caso 3: Cambiar configuración de una casa
```
1. Cargar Tareas (cargan activaciones previas)
2. Modificar según necesidad
3. Guardar (sobrescribe activaciones anteriores)
```

---

## ?? Soporte

Para preguntas sobre la implementación, consulta:
- `FormEstimacionConcepto.cs` (diseño similar)
- `FormEditorTreeList.cs` (información de rutas)
- Documentación en `IMPLEMENTACION_COMPLETADA_TreeList.md`

---

**Estado**: ? Completado y Funcional
**Versión**: 1.0
**Última actualización**: 2024

