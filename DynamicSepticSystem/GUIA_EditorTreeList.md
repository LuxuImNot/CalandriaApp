# ?? EDITOR DE TREELIST - GUÍA COMPLETA

## ?? Descripción General

El **Editor de TreeList** es un componente visual completo que permite crear, editar y gestionar estructuras jerárquicas de datos (árboles) con 3 niveles:
- **Nivel 0**: Nodos Padre (raíz)
- **Nivel 1**: Nodos Sub-Padre
- **Nivel 2**: Nodos Hijo

Características principales:
- ? **CRUD completo** (Crear, Leer, Actualizar, Eliminar)
- ? **Jerarquía visual** con TreeListView (BrightIdeasSoftware)
- ? **Reordenamiento** de nodos (subir/bajar)
- ? **Columnas personalizables** dinámicas
- ? **Guardado en base de datos** SQL Server
- ? **Interfaz moderna** con colores Material Design
- ? **Validaciones** y confirmaciones
- ? **Menú contextual** (clic derecho)

---

## ?? Instalación

### 1?? Ejecutar el Script SQL

```sql
-- Ubicación: DynamicSepticSystem\SQL_SCRIPTS\CrearTablasTreeList.sql
-- Ejecutar en SQL Server Management Studio
```

El script crea automáticamente:
- ? Tabla `TreeListData` (nodos del árbol)
- ? Tabla `TreeListData_Columnas` (valores de columnas personalizadas)
- ? Tabla `TreeListData_ColumnasDefinicion` (definición de columnas)
- ? Procedimientos almacenados útiles
- ? Vistas para consultas
- ? Datos de ejemplo (opcional)

### 2?? Agregar Archivos al Proyecto

Los siguientes archivos ya están creados en `DynamicSepticSystem`:
- ? `NodoTree.cs` - Clase modelo del nodo
- ? `FormEditorTreeList.cs` - Formulario principal
- ? `FormEditorTreeList.Designer.cs` - Diseño del formulario
- ? `FormEditorTreeList.Dialogos.cs` - Diálogos auxiliares

### 3?? Compilar el Proyecto

```bash
# Desde Visual Studio: Build > Build Solution
# O presiona Ctrl+Shift+B
```

---

## ?? Uso Básico

### Abrir el Editor

```csharp
using DynamicSepticSystem;

// Desde cualquier formulario de tu aplicación
private void btnAbrirEditorTreeList_Click(object sender, EventArgs e)
{
    string connectionString = @"Server=tu_servidor;Database=BaseDatosCalandria;Trusted_Connection=True;";
    
    using (var form = new FormEditorTreeList(connectionString, "TreeListData"))
    {
        form.ShowDialog();
    }
}
```

### Parámetros del Constructor

```csharp
FormEditorTreeList(string connectionString, string nombreTabla = "TreeListData")
```

- **connectionString**: Cadena de conexión a SQL Server
- **nombreTabla**: Nombre de la tabla principal (por defecto: "TreeListData")

---

## ?? Interfaz del Usuario

### Panel Izquierdo - Botones

#### ?? Gestión de Nodos

| Botón | Color | Función | Atajo |
|-------|-------|---------|-------|
| ? **Agregar Padre** | Verde | Crea un nodo raíz | - |
| ? **Agregar Sub-Padre** | Azul | Crea un hijo del padre seleccionado | Requiere padre seleccionado |
| ? **Agregar Hijo** | Morado | Crea un hijo del sub-padre seleccionado | Requiere sub-padre seleccionado |
| ?? **Renombrar** | Amarillo | Edita nombre y descripción | Requiere nodo seleccionado |
| ??? **Eliminar** | Rojo | Elimina nodo y descendientes | Requiere nodo seleccionado |

#### ?? Orden

| Botón | Función |
|-------|---------|
| ?? **Subir** | Mueve el nodo una posición arriba |
| ?? **Bajar** | Mueve el nodo una posición abajo |

#### ?? Columnas

| Botón | Función |
|-------|---------|
| **Gestionar Columnas** | Abre el editor de columnas personalizadas |

#### ?? Acciones

| Botón | Función | Estado |
|-------|---------|--------|
| **Guardar** | Guarda todos los cambios en BD | Solo activo si hay cambios |
| **Cerrar** | Cierra el formulario | Siempre activo |

### Panel Central - TreeListView

Muestra el árbol jerárquico con:
- ?? **Padres** - Fondo azul claro, texto en negrita
- ?? **Sub-Padres** - Fondo azul muy claro, texto en negrita
- ? **Hijos** - Fondo blanco/alternado

**Columnas base:**
- **Nombre**: Nombre del nodo (editable)
- **Tipo**: Padre / Sub-Padre / Hijo
- **Descripción**: Descripción opcional (editable)
- **Orden**: Orden de visualización

**Columnas personalizadas:**
- Se agregan dinámicamente según configuración

---

## ?? Funcionalidades Detalladas

### 1?? Agregar Nodos

#### Agregar Padre
1. Click en **"? Agregar Padre"**
2. Ingresar:
   - **Nombre** (obligatorio)
   - **Descripción** (opcional)
3. Click en **"? Aceptar"**

**Resultado:** Se crea un nuevo nodo raíz al final de la lista

#### Agregar Sub-Padre
1. **Seleccionar** un nodo **Padre** en el árbol
2. Click en **"? Agregar Sub-Padre"**
3. Ingresar nombre y descripción
4. Click en **"? Aceptar"**

**Resultado:** Se crea un nuevo sub-padre como hijo del padre seleccionado

#### Agregar Hijo
1. **Seleccionar** un nodo **Sub-Padre** en el árbol
2. Click en **"? Agregar Hijo"**
3. Ingresar nombre y descripción
4. Click en **"? Aceptar"**

**Resultado:** Se crea un nuevo hijo del sub-padre seleccionado

### 2?? Renombrar Nodos

1. Seleccionar un nodo
2. Click en **"?? Renombrar"** (o clic derecho > Renombrar)
3. Modificar nombre y/o descripción
4. Click en **"? Aceptar"**

**Nota:** También puedes hacer **doble clic** en la celda de nombre/descripción para editar directamente

### 3?? Eliminar Nodos

1. Seleccionar un nodo
2. Click en **"??? Eliminar"**
3. **Confirmar** la eliminación

**?? ADVERTENCIA:**
- Si el nodo tiene hijos, se eliminarán **TODOS los descendientes**
- El sistema muestra el número de nodos que se eliminarán
- La eliminación es **PERMANENTE** al guardar

### 4?? Reordenar Nodos

**Subir:**
1. Seleccionar un nodo
2. Click en **"?? Subir"**
3. El nodo se mueve una posición arriba entre sus hermanos

**Bajar:**
1. Seleccionar un nodo
2. Click en **"?? Bajar"**
3. El nodo se mueve una posición abajo entre sus hermanos

**Notas:**
- Solo se pueden reordenar nodos del mismo nivel
- El orden se guarda automáticamente

---

## ?? Gestión de Columnas Personalizadas

### Abrir el Gestor de Columnas

1. Click en **"?? Gestionar Columnas"**
2. Se abre el diálogo de gestión

### Agregar una Columna

1. Click en **"? Agregar"**
2. Configurar:

| Campo | Descripción | Ejemplo |
|-------|-------------|---------|
| **Nombre** | Clave interna (sin espacios) | `Presupuesto` |
| **Título** | Encabezado visible | `Presupuesto Total` |
| **Ancho** | Ancho en pixels (50-500) | `120` |
| **Tipo de dato** | String, Int32, Decimal, DateTime, Boolean | `Decimal` |
| **Editable** | ? Si permite edición | ? |
| **Formato** | Formato de visualización | `{0:C2}` para moneda |

3. Click en **"? Aceptar"**

### Formatos Comunes

| Tipo | Formato | Ejemplo |
|------|---------|---------|
| Moneda | `{0:C2}` | $1,234.56 |
| Número decimal | `{0:N2}` | 1,234.56 |
| Porcentaje | `{0:F1}%` | 45.5% |
| Fecha corta | `{0:dd/MM/yyyy}` | 31/01/2025 |
| Fecha larga | `{0:dd/MM/yyyy HH:mm}` | 31/01/2025 14:30 |

### Editar una Columna

1. Seleccionar la columna en la lista
2. Click en **"?? Editar"**
3. Modificar propiedades
4. Click en **"? Aceptar"**

### Eliminar una Columna

1. Seleccionar la columna
2. Click en **"??? Eliminar"**
3. Confirmar eliminación

**?? ADVERTENCIA:** Se perderán todos los datos de esa columna

### Reordenar Columnas

- **?? Subir**: Mueve la columna a la izquierda
- **?? Bajar**: Mueve la columna a la derecha

---

## ?? Guardado en Base de Datos

### Guardado Manual

1. Realizar cambios en el árbol
2. El botón **"?? Guardar"** se activa (color verde)
3. Click en **"?? Guardar"**
4. Confirmación: "? Datos guardados correctamente"

### Guardado Automático al Cerrar

Si hay cambios sin guardar:
```
????????????????????????????????????????????
? ?? Cambios sin guardar                   ?
?                                          ?
? Hay cambios sin guardar.                ?
? ¿Deseas guardar antes de salir?         ?
?                                          ?
?   [Sí]    [No]    [Cancelar]            ?
????????????????????????????????????????????
```

- **Sí**: Guarda y cierra
- **No**: Cierra sin guardar (se pierden cambios)
- **Cancelar**: Regresa al editor

---

## ??? Estructura de Base de Datos

### Tabla Principal: TreeListData

```sql
CREATE TABLE TreeListData (
    ID int NOT NULL PRIMARY KEY,
    ParentID int NULL,                  -- ID del padre (NULL si es raíz)
    Nombre nvarchar(255) NOT NULL,
    Descripcion nvarchar(500) NULL,
    Orden int NOT NULL DEFAULT 0,       -- Orden de visualización
    Nivel int NOT NULL DEFAULT 0,       -- 0=Padre, 1=Sub-Padre, 2=Hijo
    FechaCreacion datetime NOT NULL,
    FechaModificacion datetime NULL,
    UsuarioCreacion nvarchar(100) NULL
)
```

### Tabla de Valores: TreeListData_Columnas

```sql
CREATE TABLE TreeListData_Columnas (
    ID int IDENTITY(1,1) PRIMARY KEY,
    NodoID int NOT NULL,               -- FK a TreeListData.ID
    NombreColumna nvarchar(100) NOT NULL,
    Valor nvarchar(max) NULL
)
```

### Tabla de Definición: TreeListData_ColumnasDefinicion

```sql
CREATE TABLE TreeListData_ColumnasDefinicion (
    ID int IDENTITY(1,1) PRIMARY KEY,
    Nombre nvarchar(100) NOT NULL UNIQUE,
    Titulo nvarchar(100) NOT NULL,
    Ancho int NOT NULL DEFAULT 100,
    TipoDato nvarchar(200) NOT NULL,   -- System.String, System.Decimal, etc.
    EsEditable bit NOT NULL DEFAULT 1,
    Formato nvarchar(50) NULL
)
```

---

## ?? Consultas SQL Útiles

### Ver todos los nodos con jerarquía

```sql
SELECT 
    ID,
    ParentID,
    Nombre,
    Nivel,
    CASE Nivel
        WHEN 0 THEN 'Padre'
        WHEN 1 THEN 'Sub-Padre'
        WHEN 2 THEN 'Hijo'
    END as TipoNodo,
    Orden
FROM TreeListData
ORDER BY 
    CASE WHEN ParentID IS NULL THEN ID ELSE ParentID END,
    Orden
```

### Ver nodos con valores de columnas personalizadas

```sql
SELECT * FROM vw_TreeList_Completa
```

### Eliminar un nodo y todos sus descendientes

```sql
EXEC sp_TreeList_EliminarNodo @NodoID = 1
```

### Contar nodos por nivel

```sql
SELECT 
    Nivel,
    CASE Nivel
        WHEN 0 THEN 'Padres'
        WHEN 1 THEN 'Sub-Padres'
        WHEN 2 THEN 'Hijos'
    END as Tipo,
    COUNT(*) as Cantidad
FROM TreeListData
GROUP BY Nivel
ORDER BY Nivel
```

---

## ?? Personalización

### Cambiar el Nombre de la Tabla

```csharp
// Al abrir el editor, especifica el nombre de tabla personalizado
using (var form = new FormEditorTreeList(connectionString, "MiTablaCustom"))
{
    form.ShowDialog();
}
```

**Nota:** El script SQL debe ejecutarse modificando el nombre de tabla

### Agregar Más Niveles

Por defecto soporta 3 niveles (0, 1, 2). Para agregar más:

1. Modificar la clase `NodoTree.cs`:
```csharp
public string TipoNodo
{
    get
    {
        switch (Nivel)
        {
            case 0: return "Padre";
            case 1: return "Sub-Padre";
            case 2: return "Hijo";
            case 3: return "Sub-Hijo";  // NUEVO
            default: return $"Nivel {Nivel}";
        }
    }
}
```

2. Agregar botón en `FormEditorTreeList.Designer.cs`
3. Implementar lógica en `FormEditorTreeList.cs`

---

## ?? Solución de Problemas

### ? Error: "No se puede conectar a la base de datos"

**Causa:** Cadena de conexión incorrecta

**Solución:**
```csharp
// Verificar la cadena de conexión
string connString = @"Server=.\SQLEXPRESS;Database=BaseDatosCalandria;Trusted_Connection=True;";
```

### ? Error: "Tabla TreeListData no existe"

**Causa:** Script SQL no ejecutado

**Solución:**
1. Abrir SQL Server Management Studio
2. Ejecutar `SQL_SCRIPTS\CrearTablasTreeList.sql`
3. Verificar que las tablas se crearon correctamente

### ? Los cambios no se guardan

**Causa:** No se llamó al método `btnGuardar_Click`

**Solución:**
- Click en el botón **"?? Guardar"** antes de cerrar
- O confirmar el guardado al cerrar el formulario

### ? Las columnas personalizadas no aparecen

**Causa:** No hay columnas definidas en `TreeListData_ColumnasDefinicion`

**Solución:**
1. Click en **"?? Gestionar Columnas"**
2. Agregar columnas
3. Guardar cambios

---

## ?? Ejemplos de Uso

### Ejemplo 1: Estructura de Proyectos

```
?? Proyecto Residencial Calandria      [Padre]
?? ?? Fase 1 - Preliminares            [Sub-Padre]
?  ?? ?? Limpieza del terreno          [Hijo]
?  ?? ?? Trazo y nivelación            [Hijo]
?  ?? ?? Excavaciones                  [Hijo]
?? ?? Fase 2 - Cimentación             [Sub-Padre]
?  ?? ?? Plantilla de concreto         [Hijo]
?  ?? ?? Armado de zapatas             [Hijo]
?  ?? ?? Colado de cimentación         [Hijo]
?? ?? Fase 3 - Estructura              [Sub-Padre]
   ?? ?? Columnas                      [Hijo]
   ?? ?? Trabes                        [Hijo]
   ?? ?? Losas                         [Hijo]
```

**Columnas personalizadas:**
- Presupuesto (Decimal)
- Avance % (Decimal)
- Responsable (String)
- Fecha Inicio (DateTime)
- Fecha Fin (DateTime)

### Ejemplo 2: Catálogo de Productos

```
??? Materiales de Construcción         [Padre]
?? ?? Albañilería                     [Sub-Padre]
?  ?? Cemento Portland Tipo I         [Hijo]
?  ?? Arena de río                    [Hijo]
?  ?? Grava 3/4"                      [Hijo]
?? ? Eléctricos                       [Sub-Padre]
?  ?? Cable calibre 12                [Hijo]
?  ?? Apagadores sencillos            [Hijo]
?  ?? Contactos polarizados           [Hijo]
?? ?? Hidráulicos                     [Sub-Padre]
   ?? Tubería PVC 2"                  [Hijo]
   ?? Codos de 90°                    [Hijo]
   ?? Válvulas de paso                [Hijo]
```

**Columnas personalizadas:**
- Precio Unitario (Decimal)
- Unidad (String)
- Stock (Int32)
- Proveedor (String)

---

## ?? Mejoras Futuras Sugeridas

- [ ] Importar desde Excel
- [ ] Exportar a PDF
- [ ] Drag & Drop para reordenar
- [ ] Búsqueda y filtrado
- [ ] Duplicar nodos
- [ ] Historial de cambios
- [ ] Permisos por usuario
- [ ] Validaciones personalizadas
- [ ] Fórmulas en columnas
- [ ] Gráficas de jerarquía

---

## ?? Notas Importantes

1. **Respaldos:** Siempre respalda la BD antes de eliminar nodos masivos
2. **IDs únicos:** Los IDs se generan automáticamente y son únicos
3. **Relaciones:** Al eliminar un padre, se eliminan todos sus descendientes
4. **Orden:** El orden se actualiza automáticamente al reordenar
5. **Columnas:** Los cambios en columnas afectan a todos los nodos existentes

---

## ????? Código de Integración

### Abrir desde un botón

```csharp
private void btnEditorTreeList_Click(object sender, EventArgs e)
{
    try
    {
        string connectionString = Properties.Settings.Default.ConnectionString;
        
        using (var form = new FormEditorTreeList(connectionString))
        {
            if (form.ShowDialog() == DialogResult.OK)
            {
                // Recargar datos si es necesario
                CargarDatos();
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error al abrir el editor:\n\n{ex.Message}", 
            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

### Abrir desde menú

```csharp
private void menuItemEditorTreeList_Click(object sender, EventArgs e)
{
    var form = new FormEditorTreeList(Global.ConnectionString);
    form.Show(); // Modo no modal
}
```

---

## ? Checklist de Implementación

- [ ] Ejecutar script SQL `CrearTablasTreeList.sql`
- [ ] Verificar que las tablas se crearon correctamente
- [ ] Agregar archivos al proyecto de Visual Studio
- [ ] Compilar sin errores
- [ ] Probar crear un nodo padre
- [ ] Probar crear un sub-padre
- [ ] Probar crear un hijo
- [ ] Probar renombrar nodos
- [ ] Probar eliminar nodos
- [ ] Probar reordenar nodos
- [ ] Agregar columnas personalizadas
- [ ] Editar valores en columnas
- [ ] Guardar cambios en BD
- [ ] Verificar persistencia de datos

---

**Desarrollado para:** Sistema Calandria Residencial  
**Fecha:** Enero 2025  
**Versión:** 1.0  
**Compatibilidad:** .NET Framework 4.7.2  
**Dependencias:** BrightIdeasSoftware (ObjectListView)

---

**¡Editor de TreeList listo para usar! ??**
