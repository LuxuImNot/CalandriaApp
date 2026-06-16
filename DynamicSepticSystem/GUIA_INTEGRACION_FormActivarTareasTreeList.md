# ?? GUÍA DE INTEGRACIÓN - FormActivarTareasTreeList

## Integración Rápida

### 1. En el menú principal o panel de control

```csharp
// En PanelPrincipal.cs o donde tengas los botones principales

private void AgregarBotonActivarTareas()
{
    var btnActivarTareas = new Button
    {
        Text = "?? Activar Tareas por Casa",
        Width = 200,
        Height = 40,
        BackColor = Color.FromArgb(52, 152, 219),
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        FlatStyle = FlatStyle.Flat,
        Location = new Point(10, 150) // Ajusta según necesidad
    };

    btnActivarTareas.Click += (s, e) =>
    {
        using (var frm = new FormActivarTareasTreeList())
        {
            frm.ShowDialog(this);
        }
    };

    this.Controls.Add(btnActivarTareas);
}

// Llamar en el Load del formulario
private void PanelPrincipal_Load(object sender, EventArgs e)
{
    AgregarBotonActivarTareas();
    // ... resto de inicialización
}
```

---

## 2. En FormEditorTreeList

Agregar un botón para abrir el formulario de activación:

```csharp
// En FormEditorTreeList.cs

private void AgregarBotonActivarTareasFormulario()
{
    var btnActivar = new Button
    {
        Text = "?? Activar Tareas",
        Width = 120,
        Height = 30,
        BackColor = Color.FromArgb(155, 89, 182),
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 9, FontStyle.Bold),
        FlatStyle = FlatStyle.Flat
    };

    btnActivar.Click += (s, e) =>
    {
        using (var frm = new FormActivarTareasTreeList())
        {
            if (frm.ShowDialog(this) == DialogResult.OK)
            {
                // Si es necesario, hacer algo después
                MessageBox.Show("Configuración de tareas guardada", 
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    };

    // Agregar al panel de botones (ajusta según tu layout)
    this.Controls.Add(btnActivar);
}

// En FormEditorTreeList_Load
private void FormEditorTreeList_Load(object sender, EventArgs e)
{
    AgregarBotonActivarTareasFormulario();
    // ... resto de inicialización
}
```

---

## 3. Acceso desde múltiples lugares

```csharp
// Helper para abrir el formulario desde cualquier lugar
public static class FormularioHelper
{
    public static void AbrirActivarTareas(Form owner = null)
    {
        using (var frm = new FormActivarTareasTreeList())
        {
            if (owner != null)
                frm.ShowDialog(owner);
            else
                frm.Show();
        }
    }
}

// Uso desde cualquier formulario
FormularioHelper.AbrirActivarTareas(this);
```

---

## 4. Integración con menú contextual

```csharp
// En FormEditorTreeList.cs o donde manejes menús

private void MostrarMenuContextual(int x, int y)
{
    using (ContextMenuStrip menu = new ContextMenuStrip())
    {
        // Opción 1: Ver tareas por casa
        var itemActivar = new ToolStripMenuItem("?? Activar Tareas por Casa");
        itemActivar.Click += (s, e) =>
        {
            using (var frm = new FormActivarTareasTreeList())
            {
                frm.ShowDialog(this);
            }
        };

        // Opción 2: Editar árbol
        var itemEditar = new ToolStripMenuItem("?? Editar Árbol");
        itemEditar.Click += (s, e) =>
        {
            // Tu lógica de edición
        };

        menu.Items.Add(itemActivar);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(itemEditar);

        menu.Show(this, new Point(x, y));
    }
}
```

---

## 5. Validación previa

```csharp
// Verificar que existe la tabla antes de abrir
private bool VerificarTablaActivacionTareasRuta()
{
    try
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string sql = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'ActivacionTareasRuta'";
            
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
    }
    catch
    {
        return false;
    }
}

// En el evento de clic del botón
private void btnActivarTareas_Click(object sender, EventArgs e)
{
    if (!VerificarTablaActivacionTareasRuta())
    {
        MessageBox.Show(
            "La tabla ActivacionTareasRuta no existe.\n\n" +
            "Ejecuta el script SQL: CrearTablaActivacionTareasRuta.sql\n\n" +
            "Ubicación: SQL_SCRIPTS\\CrearTablaActivacionTareasRuta.sql",
            "Tabla No Encontrada",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
        return;
    }

    using (var frm = new FormActivarTareasTreeList())
    {
        frm.ShowDialog(this);
    }
}
```

---

## 6. Sincronización de datos

```csharp
// Si necesitas sincronizar cambios después de guardar

public class SincronizadorTareas
{
    private string connectionString;

    public SincronizadorTareas(string connString)
    {
        connectionString = connString;
    }

    /// <summary>
    /// Sincroniza las tareas activas de una casa con el editor
    /// </summary>
    public List<int> ObtenerNodosActivos(string manzana, string lote, string ruta)
    {
        var nodosActivos = new List<int>();

        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT NodoID 
                    FROM ActivacionTareasRuta
                    WHERE Manzana = @m AND Lote = @l AND Ruta = @ruta AND Activa = 1";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@m", manzana);
                    cmd.Parameters.AddWithValue("@l", lote);
                    cmd.Parameters.AddWithValue("@ruta", ruta);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            nodosActivos.Add(Convert.ToInt32(reader["NodoID"]));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al obtener nodos activos: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        return nodosActivos;
    }

    /// <summary>
    /// Verifica si un nodo está activo
    /// </summary>
    public bool EsNodoActivo(int nodoId, string manzana, string lote, string ruta)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT COUNT(*) 
                    FROM ActivacionTareasRuta
                    WHERE NodoID = @nodo AND Manzana = @m AND Lote = @l 
                    AND Ruta = @ruta AND Activa = 1";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nodo", nodoId);
                    cmd.Parameters.AddWithValue("@m", manzana);
                    cmd.Parameters.AddWithValue("@l", lote);
                    cmd.Parameters.AddWithValue("@ruta", ruta);

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }
        catch
        {
            return true; // Por defecto, asumir que está activo si hay error
        }
    }
}

// Uso
var sincronizador = new SincronizadorTareas(connectionString);
var nodosActivos = sincronizador.ObtenerNodosActivos("5", "12", "RutaCalandraDestajo");
```

---

## 7. Filtrado dinámico en FormEditorTreeList

```csharp
// Mostrar solo tareas activas según configuración de casa

private void MostrarSoloTareasActivas(string manzana, string lote)
{
    var sincronizador = new SincronizadorTareas(connectionString);
    var nodosActivos = sincronizador.ObtenerNodosActivos(manzana, lote, nombreTabla);

    // Ocultar nodos inactivos
    foreach (var nodo in todosLosNodos)
    {
        bool esActivo = nodosActivos.Contains(nodo.ID);
        // Tu lógica para mostrar/ocultar nodos
    }
}
```

---

## 8. Exportación de configuración

```csharp
// Exportar configuración de tareas a Excel u otro formato

private void ExportarConfiguracionTareas(string manzana, string lote, string rutaArchivo)
{
    try
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string sql = @"
                SELECT 
                    Manzana,
                    Lote,
                    NombreTarea,
                    CASE WHEN Activa = 1 THEN 'Activa' ELSE 'Inactiva' END AS Estado,
                    FechaActualizacion
                FROM ActivacionTareasRuta
                WHERE Manzana = @m AND Lote = @l
                ORDER BY Ruta, NodoID";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Guardar a Excel (requiere interop)
                    // O guardar a CSV
                    ExportarACSV(dt, rutaArchivo);
                }
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error al exportar: {ex.Message}",
            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

private void ExportarACSV(DataTable dt, string rutaArchivo)
{
    using (StreamWriter writer = new StreamWriter(rutaArchivo, false, System.Text.Encoding.UTF8))
    {
        // Encabezados
        for (int i = 0; i < dt.Columns.Count; i++)
        {
            writer.Write(dt.Columns[i].ColumnName);
            if (i < dt.Columns.Count - 1)
                writer.Write(",");
        }
        writer.WriteLine();

        // Datos
        foreach (DataRow row in dt.Rows)
        {
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                writer.Write($"\"{row[i]}\"");
                if (i < dt.Columns.Count - 1)
                    writer.Write(",");
            }
            writer.WriteLine();
        }
    }
}
```

---

## 9. Logs de actividad

```csharp
// Registrar cambios de activación

public class LogActivacionTareas
{
    private string connectionString;
    private string rutaLog;

    public LogActivacionTareas(string connString, string ruta)
    {
        connectionString = connString;
        rutaLog = ruta ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogActivacionTareas.log");
    }

    public void RegistrarCambio(string manzana, string lote, string tarea, bool activa, string usuario)
    {
        string estado = activa ? "ACTIVÓ" : "DESACTIVÓ";
        string mensaje = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {usuario} {estado} la tarea '{tarea}' en M{manzana}-L{lote}";

        try
        {
            File.AppendAllText(rutaLog, mensaje + Environment.NewLine);
        }
        catch { /* Ignorar errores de log */ }
    }

    public void RegistrarGuardado(string manzana, string lote, string usuario, int tareasActivas)
    {
        string mensaje = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {usuario} guardó configuración para M{manzana}-L{lote}. Tareas activas: {tareasActivas}";

        try
        {
            File.AppendAllText(rutaLog, mensaje + Environment.NewLine);
        }
        catch { /* Ignorar errores de log */ }
    }
}
```

---

## 10. Instalación Completa

### Paso 1: Ejecutar SQL
```bash
sqlcmd -S SERVER -d DATABASE -i "CrearTablaActivacionTareasRuta.sql"
```

### Paso 2: Compilar
```bash
cd "DynamicSepticSystem"
dotnet build
```

### Paso 3: Verificar archivos
- ? `FormActivarTareasTreeList.cs`
- ? `FormActivarTareasTreeList.Designer.cs`
- ? `DialogSeleccionarNivel.cs`
- ? `SQL_SCRIPTS/CrearTablaActivacionTareasRuta.sql`

### Paso 4: Agregar referencia (si falta)
```csharp
using BrightIdeasSoftware;
using System.Data.SqlClient;
```

### Paso 5: Probar
```
1. Abrir aplicación
2. Navegar a FormActivarTareasTreeList
3. Seleccionar M5-L12
4. Haz clic "?? Cargar Tareas"
5. Activar/desactivar tareas
6. Guardar
7. Reabrir para verificar persistencia
```

---

## Checklist de Integración

- [ ] Copiar archivos `.cs` a proyecto
- [ ] Ejecutar script SQL
- [ ] Compilar sin errores
- [ ] Agregar botón en menú principal o panel
- [ ] Probar con una casa
- [ ] Verificar que se guardan datos
- [ ] Verificar que se cargan datos al reabrir
- [ ] Probar botones de marcado (Todo, Por Nivel)
- [ ] Documentación actualizada
- [ ] Pruebas en producción

---

**Estado**: ? Listo para integrar
**Versión**: 1.0

