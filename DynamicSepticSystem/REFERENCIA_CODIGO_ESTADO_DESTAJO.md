# Referencia de Código: Estado Activado/Desactivado

## Clases y Propiedades

### ItemTareaActivacion
```csharp
public class ItemTareaActivacion
{
    public int ID { get; set; }
    public int ParentId { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int Nivel { get; set; }
    public int Contador { get; set; }
    public string Tipo { get; set; }
    public string TipoTarea { get; set; }
    public TipoTarea TipoTareaEnum { get; set; }
    
    // Estados
    public bool Activa { get; set; }                           // Checkbox seleccionado
    public bool DesatajoActivado { get; set; } = false;        // Destajo con cuadrilla (NEW)
    public string CuadrillaAsignada { get; set; } = "";        // Código de cuadrilla
    
    // Datos
    public decimal Cantidad { get; set; }
    public string Unidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    public decimal Total => Cantidad * PrecioUnitario;
}
```

---

## Métodos Clave

### 1. Configurar Columna de Estado
```csharp
// Columna Estado Destajo (Activado/Desactivado)
var colEstadoDestajo = new OLVColumn("Estado", "DesatajoActivado")
{
    Width = 90,
    IsEditable = false,
    TextAlign = HorizontalAlignment.Center,
    Sortable = false,
    AspectGetter = obj =>
    {
        var it = obj as ItemTareaActivacion;
        if (it?.Nivel == 1)
        {
            return it.DesatajoActivado ? "? Activado" : "? Desactivado";
        }
        return "";
    }
};
```

### 2. Aplicar Colores Según Estado
```csharp
private void OlvTareas_FormatRow(object sender, FormatRowEventArgs e)
{
    var item = e.Model as ItemTareaActivacion;
    if (item?.Nivel == 1)  // Solo destajos
    {
        if (item.DesatajoActivado && !string.IsNullOrEmpty(item.CuadrillaAsignada))
        {
            // Verde: completamente activado
            e.Item.BackColor = Color.FromArgb(200, 230, 201);
        }
        else if (!string.IsNullOrEmpty(item.CuadrillaAsignada))
        {
            // Amarillo: con cuadrilla pero no completamente activado
            e.Item.BackColor = Color.FromArgb(255, 243, 224);
        }
        else
        {
            // Gris: sin cuadrilla
            e.Item.BackColor = Color.FromArgb(243, 247, 252);
        }
    }
}
```

### 3. Al Asignar Cuadrilla
```csharp
private void EjecutarFlujoActivacionDestajo(ItemTareaActivacion destajo, bool esActivacion = true)
{
    // ... mostrar FormAsignarCuadrilla ...
    
    if (dr == DialogResult.OK && formCuadrilla.CuadrillaAsignada != null)
    {
        destajo.Activa = true;
        destajo.CuadrillaAsignada = formCuadrilla.CuadrillaAsignada.CodigoCuadrilla;
        destajo.DesatajoActivado = true;  // ? MARCAR COMO COMPLETAMENTE ACTIVADO
        
        PersistirActivacionDestajo(destajo);
        olvTareas.RefreshObject(destajo);
        ActualizarEstadisticas();
        GenerarPdfDestajo(destajo);
    }
    else if (esActivacion)
    {
        // Revertir
        destajo.Activa = false;
        destajo.CuadrillaAsignada = "";
        destajo.DesatajoActivado = false;  // ? LIMPIAR ESTADO
        olvTareas.RefreshObject(destajo);
        ActualizarEstadisticas();
    }
}
```

### 4. Al Desmarcar Destajo
```csharp
else if (!e.Item.Checked && item.Nivel == 1)
{
    // Al desactivar, limpia todo
    item.CuadrillaAsignada = "";
    item.DesatajoActivado = false;  // ? LIMPIAR ESTADO
    olvTareas.RefreshObject(item);
}
```

### 5. Validación Antes de Generar PDF
```csharp
private void GenerarPdfDestajo(ItemTareaActivacion destajo)
{
    if (destajo == null || destajo.Nivel != 1) return;
    
    // ? Validación estricta
    if (!destajo.DesatajoActivado || string.IsNullOrEmpty(destajo.CuadrillaAsignada))
    {
        MessageBox.Show("El destajo no está completamente activado. " +
                       "Debe tener cuadrilla asignada.",
                       "PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
    }
    
    // ... generar PDF ...
}
```

### 6. Persistir en Base de Datos
```csharp
private void PersistirActivacionDestajo(ItemTareaActivacion destajo)
{
    using (var conn = new SqlConnection(connectionString))
    {
        conn.Open();
        
        // Insertar/Actualizar
        using (var cmdIns = new SqlCommand(@"
            INSERT INTO ActivacionTareasRuta
            (Manzana, Lote, Prototipo, Ruta, NodoID, NombreTarea, Activa, 
             CuadrillaAsignada, DesatajoActivado, FechaActualizacion)
            VALUES (@m, @l, @proto, @ruta, @nodo, @nombre, @activa, 
                    @cuadrilla, @desatActivado, GETDATE())", conn))
        {
            cmdIns.Parameters.AddWithValue("@m", manzanaActual);
            cmdIns.Parameters.AddWithValue("@l", loteActual);
            cmdIns.Parameters.AddWithValue("@proto", prototipoActual ?? (object)DBNull.Value);
            cmdIns.Parameters.AddWithValue("@ruta", rutaActual);
            cmdIns.Parameters.AddWithValue("@nodo", destajo.ID);
            cmdIns.Parameters.AddWithValue("@nombre", destajo.Nombre ?? "");
            cmdIns.Parameters.AddWithValue("@activa", destajo.Activa);
            cmdIns.Parameters.AddWithValue("@cuadrilla",
                string.IsNullOrEmpty(destajo.CuadrillaAsignada)
                    ? (object)DBNull.Value
                    : destajo.CuadrillaAsignada);
            cmdIns.Parameters.AddWithValue("@desatActivado", destajo.DesatajoActivado);  // ? GUARDAR
            
            cmdIns.ExecuteNonQuery();
        }
    }
}
```

### 7. Cargar de Base de Datos
```csharp
private void CargarActivacionesGuardadas()
{
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        conn.Open();
        
        // Crear tabla y columnas si no existen (migración automática)
        string sqlCheck = @"
            IF NOT EXISTS (SELECT * FROM sys.columns
                          WHERE Name = N'DesatajoActivado'
                            AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
            BEGIN
                ALTER TABLE ActivacionTareasRuta ADD DesatajoActivado BIT DEFAULT 0;
            END";
        
        using (SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn))
            cmdCheck.ExecuteNonQuery();
        
        // Cargar datos
        string sql = @"
            SELECT NodoID, Activa, CuadrillaAsignada, DesatajoActivado
            FROM ActivacionTareasRuta
            WHERE Manzana = @m AND Lote = @l AND Ruta = @ruta";
        
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@m", manzanaActual);
            cmd.Parameters.AddWithValue("@l", loteActual);
            cmd.Parameters.AddWithValue("@ruta", rutaActual);
            
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int nodoId = Convert.ToInt32(reader["NodoID"]);
                    bool activa = Convert.ToBoolean(reader["Activa"]);
                    string cuadrilla = reader["CuadrillaAsignada"] == DBNull.Value
                        ? "" : reader["CuadrillaAsignada"].ToString();
                    bool desatajoActivado = reader["DesatajoActivado"] == DBNull.Value
                        ? false : Convert.ToBoolean(reader["DesatajoActivado"]);
                    
                    var item = itemsTareas.FirstOrDefault(i => i.ID == nodoId);
                    if (item != null)
                    {
                        item.Activa = activa;
                        item.CuadrillaAsignada = cuadrilla;
                        item.DesatajoActivado = desatajoActivado;  // ? CARGAR
                    }
                }
            }
        }
    }
}
```

### 8. Actualizar Estadísticas
```csharp
private void ActualizarEstadisticas()
{
    int totalDestajos = itemsTareas.Count(i => i.Nivel == 1);
    int destajosActivos = itemsTareas.Count(i => i.Nivel == 1 && i.Activa);
    int conCuadrilla = itemsTareas.Count(i => i.Nivel == 1 && i.Activa && 
                                              !string.IsNullOrEmpty(i.CuadrillaAsignada));
    int completameteActivados = itemsTareas.Count(i => i.Nivel == 1 && i.DesatajoActivado);
    
    lblEstadisticas.Text = string.Format(
        "Destajos activos: {0} de {1}   ·   Con cuadrilla: {2}   ·   " +
        "Completamente activados: {3}   ·   Importe: {4}",
        destajosActivos, totalDestajos, conCuadrilla, completameteActivados,
        montoActivo.ToString("C2", CultureInfo.CurrentCulture));
    
    progressBarActivacion.Maximum = Math.Max(totalDestajos, 1);
    progressBarActivacion.Value = Math.Min(completameteActivados, totalDestajos);
    
    lblPorcentaje.Text = totalDestajos > 0
        ? $"{(completameteActivados * 100 / totalDestajos):F0}% completamente activados"
        : "—";
}
```

---

## Patrones Comunes

### Verificar si Destajo Está Completamente Activado
```csharp
bool estaCompletameteActivado = destajo.Nivel == 1 && 
                                 destajo.DesatajoActivado && 
                                 !string.IsNullOrEmpty(destajo.CuadrillaAsignada);

if (estaCompletameteActivado)
{
    // Destajo listo para PDF
}
```

### Limpiar Destajo Completamente
```csharp
void LimpiarDestajo(ItemTareaActivacion destajo)
{
    if (destajo.Nivel == 1)
    {
        destajo.Activa = false;
        destajo.CuadrillaAsignada = "";
        destajo.DesatajoActivado = false;
    }
}
```

### Contar Destajos por Estado
```csharp
var inactivos = itemsTareas.Count(i => i.Nivel == 1 && !i.Activa);
var pendientes = itemsTareas.Count(i => i.Nivel == 1 && i.Activa && !i.DesatajoActivado);
var activados = itemsTareas.Count(i => i.Nivel == 1 && i.DesatajoActivado);
```

---

## SQL Migración

### Crear Tabla Completa
```sql
CREATE TABLE ActivacionTareasRuta (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Manzana NVARCHAR(10),
    Lote NVARCHAR(10),
    Prototipo NVARCHAR(50),
    Ruta NVARCHAR(50),
    NodoID INT,
    NombreTarea NVARCHAR(200),
    Activa BIT,
    CuadrillaAsignada NVARCHAR(20) NULL,
    DesatajoActivado BIT DEFAULT 0,  -- ? NUEVO
    FechaActualizacion DATETIME DEFAULT GETDATE()
);
```

### Agregar Columna a Tabla Existente
```sql
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE Name = N'DesatajoActivado'
      AND Object_ID = Object_ID(N'dbo.ActivacionTareasRuta'))
BEGIN
    ALTER TABLE ActivacionTareasRuta 
    ADD DesatajoActivado BIT DEFAULT 0;
END
```

### Actualizar Registros Existentes
```sql
-- Marcar como activados aquellos que tienen cuadrilla
UPDATE ActivacionTareasRuta
SET DesatajoActivado = 1
WHERE CuadrillaAsignada IS NOT NULL 
  AND CuadrillaAsignada != '';
```

---

## Flujo de Ejecución

```
USUARIO ABRE APLICACIÓN
    ?
    ?
CargarManzanas()
    ?
    ?
USUARIO SELECCIONA Manzana/Lote
    ?
    ?
btnCargar_Click()
    ?? ObtenerPrototipo()
    ?? CargarTareas()
    ?   ?? CargarTareasDesdeRuta()
    ?   ?   ?? Llena lista itemsTareas
    ?   ?? CargarActivacionesGuardadas()
    ?       ?? Lee DesatajoActivado desde BD ?
    ?
    ?
USUARIO MARCA CHECKBOX DESTAJO
    ?
    ?
OlvTareas_ItemChecked()
    ?? EjecutarFlujoActivacionDestajo()
       ?? Abre FormAsignarCuadrilla
       ?? Si OK:
       ?   ?? destajo.DesatajoActivado = true ?
       ?   ?? PersistirActivacionDestajo()
       ?   ?   ?? Guarda DesatajoActivado en BD ?
       ?   ?? Cambia color a VERDE
       ?   ?? GenerarPdfDestajo()
       ?       ?? Valida DesatajoActivado ?
       ?? Si Cancelar:
           ?? destajo.DesatajoActivado = false ?
```
