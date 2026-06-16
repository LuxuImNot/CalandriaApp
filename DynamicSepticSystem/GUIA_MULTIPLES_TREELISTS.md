# ?? Guía Rápida: Sistema de Múltiples TreeLists

## ?? Descripción General

El sistema ahora soporta **dos TreeLists independientes** que se pueden alternar mediante un ComboBox:

1. **Ruta Tunera Destajo** (anteriormente TreeListData)
2. **Ruta Calandra Destajo** (nueva)

Cada ruta tiene su propia estructura jerárquica, columnas personalizadas y datos independientes.

---

## ?? Instalación

### Paso 1: Ejecutar Script SQL

Ejecuta el siguiente script en tu base de datos:

```
SQL_SCRIPTS\CrearTablasRutaCalandraDestajo.sql
```

Este script:
- ? Renombra `TreeListData` a `RutaTuneraDestajo`
- ? Crea las nuevas tablas para `RutaCalandraDestajo`
- ? Actualiza todas las restricciones y claves foráneas
- ? Inserta datos de ejemplo para Ruta Calandra

### Paso 2: Verificar la Instalación

El script mostrará un resumen al finalizar:

```
? INSTALACIÓN COMPLETADA EXITOSAMENTE
? Tablas creadas:
  - RutaTuneraDestajo (renombrada desde TreeListData)
  - RutaCalandraDestajo (nueva)
```

---

## ?? Uso en la Aplicación

### Abrir el Editor

```csharp
// Abrir con Ruta Tunera (predeterminada)
using (var form = new FormEditorTreeList(connectionString))
{
    form.ShowDialog();
}

// Abrir con Ruta Calandra específicamente
using (var form = new FormEditorTreeList(connectionString, "RutaCalandraDestajo"))
{
    form.ShowDialog();
}

// Abrir con Ruta Tunera específicamente
using (var form = new FormEditorTreeList(connectionString, "RutaTuneraDestajo"))
{
    form.ShowDialog();
}
```

### Cambiar entre Rutas

1. **Ubicar el ComboBox** en la parte superior del panel lateral
2. **Seleccionar la ruta** deseada:
   - Ruta Tunera Destajo
   - Ruta Calandra Destajo
3. Si hay cambios sin guardar, el sistema preguntará si deseas guardar antes de cambiar

```
??????????????????????????
?  Editor TreeList       ?
?  Ruta Tunera Destajo   ?
??????????????????????????
? Seleccionar Ruta:      ?
? ?????????????????????? ?
? ? Ruta Tunera...  ?? ?  ? ComboBox
? ?????????????????????? ?
??????????????????????????
? Gestión de Nodos       ?
? ...                    ?
??????????????????????????
```

---

## ??? Estructura de Tablas

### Ruta Tunera Destajo (antes TreeListData)

| Tabla | Descripción |
|-------|-------------|
| `RutaTuneraDestajo` | Datos principales de nodos |
| `RutaTuneraDestajo_Columnas` | Valores de columnas personalizadas |
| `RutaTuneraDestajo_ColumnasDefinicion` | Definición de columnas |

### Ruta Calandra Destajo (nueva)

| Tabla | Descripción |
|-------|-------------|
| `RutaCalandraDestajo` | Datos principales de nodos |
| `RutaCalandraDestajo_Columnas` | Valores de columnas personalizadas |
| `RutaCalandraDestajo_ColumnasDefinicion` | Definición de columnas |

---

## ?? Flujo de Cambio de Ruta

```
Usuario selecciona nueva ruta en ComboBox
            ?
   ¿Hay cambios pendientes?
            ?
        ?????????
       SÍ       NO
        ?        ?
  Preguntar   Cambiar
  guardar     directamente
        ?        ?
    ??????????????????
    ? Cargar datos   ?
    ? nueva ruta     ?
    ??????????????????
            ?
    Actualizar título
    y estado botones
```

---

## ?? Consultas SQL Útiles

### Ver todas las rutas disponibles

```sql
-- Contar registros en Ruta Tunera
SELECT COUNT(*) AS Total FROM RutaTuneraDestajo

-- Contar registros en Ruta Calandra
SELECT COUNT(*) AS Total FROM RutaCalandraDestajo
```

### Obtener nodos por ruta y tipo de tarea

```sql
-- Materiales en Ruta Tunera
SELECT * FROM RutaTuneraDestajo 
WHERE TipoTarea = 1 AND Nivel = 2

-- Mano de Obra en Ruta Calandra
SELECT * FROM RutaCalandraDestajo 
WHERE TipoTarea = 2 AND Nivel = 2
```

### Copiar estructura de una ruta a otra

```sql
-- Copiar columnas de Tunera a Calandra
INSERT INTO RutaCalandraDestajo_ColumnasDefinicion 
    (Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato)
SELECT Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato
FROM RutaTuneraDestajo_ColumnasDefinicion
```

---

## ?? Ejemplos de Uso

### Ejemplo 1: Proyecto con Dos Rutas

```
??? Proyecto Calandria
  ?
  ?? ?? Ruta Tunera Destajo
  ?   ?? Urbanización Tunera
  ?   ?? Construcción Tunera
  ?   ?? Acabados Tunera
  ?
  ?? ?? Ruta Calandra Destajo
      ?? Urbanización Calandria
      ?? Construcción Calandria
      ?? Acabados Calandria
```

### Ejemplo 2: Gestión Independiente

Cada ruta puede tener:
- ? Diferentes estructuras jerárquicas
- ? Diferentes columnas personalizadas
- ? Diferentes tipos de tareas asignadas
- ? Diferentes responsables y fechas

---

## ?? Características del Sistema

### Protección de Datos

- ? Pregunta antes de cambiar si hay cambios sin guardar
- ? Permite guardar antes de cambiar de ruta
- ? Permite cancelar el cambio de ruta

### Independencia de Datos

- ? Cada ruta tiene sus propias tablas
- ? Los cambios en una ruta NO afectan la otra
- ? Se pueden gestionar columnas independientemente

### Interfaz Unificada

- ? Misma interfaz para ambas rutas
- ? Cambio fluido entre rutas
- ? Título actualizado según ruta activa

---

## ?? Configuración Avanzada

### Agregar Más Rutas

Para agregar una tercera ruta (ej: RutaOtraDestajo):

1. **Crear tablas SQL:**
   ```sql
   CREATE TABLE RutaOtraDestajo (...)
   CREATE TABLE RutaOtraDestajo_Columnas (...)
   CREATE TABLE RutaOtraDestajo_ColumnasDefinicion (...)
   ```

2. **Actualizar código C#:**
   ```csharp
   private void ConfigurarComboBoxRutas()
   {
       cboRutas.Items.Add("Ruta Tunera Destajo");
       cboRutas.Items.Add("Ruta Calandra Destajo");
       cboRutas.Items.Add("Ruta Otra Destajo"); // Nueva ruta
   }
   ```

3. **Actualizar evento de cambio:**
   ```csharp
   string nuevaTabla = cboRutas.SelectedIndex == 0 ? "RutaTuneraDestajo" :
                       cboRutas.SelectedIndex == 1 ? "RutaCalandraDestajo" :
                       "RutaOtraDestajo";
   ```

---

## ?? Solución de Problemas

### El ComboBox no muestra las rutas

**Causa:** Las tablas no existen en la base de datos

**Solución:** Ejecuta el script `CrearTablasRutaCalandraDestajo.sql`

### Error al cambiar de ruta

**Causa:** Una de las tablas no existe o tiene estructura incorrecta

**Solución:** Verifica que ambas rutas tengan la estructura completa:
```sql
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE 'Ruta%Destajo%'
```

### Los datos no se guardan al cambiar de ruta

**Causa:** Se canceló el guardado en el diálogo de confirmación

**Solución:** Haz clic en "Sí" cuando se pregunte si deseas guardar

### Las columnas personalizadas no aparecen

**Causa:** La tabla de definición de columnas está vacía

**Solución:** Usa el gestor de columnas para crear columnas personalizadas

---

## ?? Notas Importantes

- ? **Compatibilidad:** Las dos rutas son completamente independientes
- ? **Datos de ejemplo:** Se incluyen datos de ejemplo para ambas rutas
- ? **Migración:** Los datos de TreeListData se migraron a RutaTuneraDestajo
- ? **Validación:** El sistema valida cambios pendientes antes de alternar
- ? **Integridad:** Las restricciones FK aseguran integridad referencial

---

## ?? Soporte

Para dudas o problemas, contactar al equipo de desarrollo del Sistema Calandria Residencial.

**Versión:** 2.0  
**Fecha:** Enero 2025  
**Autor:** Sistema Calandria Residencial

---

## ?? Referencias Relacionadas

- `GUIA_TIPO_TAREA.md` - Guía de etiquetas de tipo de tarea
- `CAMBIOS_TIPO_TAREA.md` - Resumen de cambios de etiquetado
- `SQL_SCRIPTS\CrearTablasRutaCalandraDestajo.sql` - Script de instalación
- `SQL_SCRIPTS\CrearTablasTreeList.sql` - Script original (deprecado)
