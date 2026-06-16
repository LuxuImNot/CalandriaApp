# ?? GUÍA RÁPIDA - FormAlmacen Modular

## ?? Estructura de Archivos

```
FormAlmacen/
??? FormAlmacen.cs                    [Principal - Solo declaración]
??? FormAlmacen_Core.cs               [Núcleo - Inicialización]
??? FormAlmacen_Theme.cs              [Tema - Estilos visuales]
??? FormAlmacen_Layout.cs             [Layout - Diseño responsivo]
??? FormAlmacen_Entradas.cs           [Entradas - Órdenes de compra]
??? FormAlmacen_Salidas.cs            [Salidas - Despacho de materiales]
??? FormAlmacen_Historial.cs          [Historial - Registro de movimientos]
??? FormAlmacen.Designer.cs           [Diseñador - Controles UI]
```

---

## ?? Búsqueda Rápida de Funcionalidades

### ?? Cambiar Colores o Fuentes
**Archivo**: `FormAlmacen_Theme.cs`
- Método: `AplicarTemaCorporativo()`
- Líneas: ~50-180

### ?? Ajustar Posiciones o Tamaños
**Archivo**: `FormAlmacen_Layout.cs`
- Método: `ConfigurarLayoutEntradas()`
- Método: `ConfigurarLayoutSalidas()`
- Líneas: ~30-250

### ?? Modificar Captura de Entradas
**Archivo**: `FormAlmacen_Entradas.cs`
- Método: `BtnCapturarEntrada_Click()`
- Líneas: ~50-280

### ?? Modificar Captura de Salidas
**Archivo**: `FormAlmacen_Salidas.cs`
- Método: `RegistrarSalida()`
- Líneas: ~50-130

### ?? Modificar Historial
**Archivo**: `FormAlmacen_Historial.cs`
- Método: `CargarHistorial()`
- Método: `ConfigurarColumnasHistorial()`
- Líneas: ~20-100

### ?? Agregar Controles Nuevos
**Archivo**: `FormAlmacen.Designer.cs`
- Método: `InitializeComponent()`
- Declarar control como campo privado
- Inicializar en InitializeComponent()

---

## ??? Escenarios Comunes de Edición

### ?? Cambiar Texto de un Botón
1. Ir a: `FormAlmacen.Designer.cs`
2. Buscar: `btnCapturarEntrada.Text = "..."`
3. Modificar el texto entre comillas

### ?? Cambiar Color de un Label
1. Ir a: `FormAlmacen_Theme.cs`
2. Buscar el label (ej: `lblCasa`)
3. Modificar: `lblCasa.ForeColor = ThemeManager.ColorX`

### ?? Agregar Validación a Entrada
1. Ir a: `FormAlmacen_Entradas.cs`
2. Buscar: `BtnCapturarEntrada_Click`
3. Agregar validación antes de `using (SqlConnection conn...)`

### ?? Cambiar Consulta SQL
1. Ir al archivo correspondiente:
   - Entradas: `FormAlmacen_Entradas.cs`
   - Salidas: `FormAlmacen_Salidas.cs`
   - Historial: `FormAlmacen_Historial.cs`
2. Buscar la variable `sql = @"..."`
3. Modificar la consulta

### ?? Ajustar Ancho del Panel Lateral
1. Ir a: `FormAlmacen_Layout.cs`
2. Buscar: `int panelWidth = 250;`
3. Cambiar el valor (ej: 280, 300, etc.)

---

## ?? Métodos Principales por Archivo

### FormAlmacen_Core.cs
```csharp
? FormAlmacen()                           // Constructor
? FormAlmacen_Load()                      // Carga inicial
? SetCasaActual()                         // Establece casa actual
? ActualizarResumenOrden()                // Actualiza contador
? CargarExplosionParaSalida()             // Carga explosión de insumos
? CargarExplosionDesdeSQL()               // Lee explosión desde BD
```

### FormAlmacen_Theme.cs
```csharp
? AplicarTemaCorporativo()                // Aplica tema visual
? EstilizarDataGridViewAlmacen()          // Estiliza grillas
? dgvEntrada_RowPrePaint()                // Colorea filas
```

### FormAlmacen_Layout.cs
```csharp
? ConfigurarLayoutResponsivo()            // Configura layout general
? ConfigurarLayoutEntradas()              // Layout tab entradas
? ConfigurarLayoutSalidas()               // Layout tab salidas
? ConfigurarLayoutHistorial()             // Layout tab historial
? FormAlmacen_Resize()                    // Maneja redimensionamiento
? AjustarLayoutTabEntradas()              // Ajustes dinámicos entradas
? AjustarLayoutTabSalidas()               // Ajustes dinámicos salidas
```

### FormAlmacen_Entradas.cs
```csharp
? TxtBuscarEntrada_TextChanged()          // Filtrado de búsqueda
? CargarOrdenesDeCompra()                 // Carga órdenes pendientes
? BtnCapturarEntrada_Click()              // Captura entrada
? GenerarPDFEntrada()                     // Genera PDF [STATIC]
? CargarInsumosDesdeOrdenSeleccionada()   // Carga insumos de orden
? BtnBuscarOrden_Click()                  // Busca órdenes
? MostrarLotesDeOrden()                   // Muestra casas asociadas
? BtnCasa_Click()                         // Selecciona casa
? cmbOrdenesCompra_SelectedIndexChanged() // Cambio de orden
? btnEliminardeOrden_Click()              // Elimina insumos
```

### FormAlmacen_Salidas.cs
```csharp
? TxtBuscarSalida_TextChanged()           // Filtrado de búsqueda
? CargarInsumosDisponiblesSalida()        // Carga insumos disponibles
? RegistrarSalida()                       // Registra salida
? BtnSeleccionarCasaSalida_Click()        // Selecciona casa destino
? dgvInsumos_CellContentClick()           // Click en celda
```

### FormAlmacen_Historial.cs
```csharp
? CargarHistorial()                       // Carga historial
? ConfigurarColumnasHistorial()           // Configura columnas
? TabsControl_SelectedIndexChanged()      // Cambio de tab
? MovimientoHistorial [clase]             // Modelo de datos
```

---

## ?? Patrones de Código

### Patrón 1: Consulta Simple
```csharp
using (SqlConnection conn = new SqlConnection(connectionString))
using (SqlCommand cmd = new SqlCommand("SELECT ...", conn))
{
    conn.Open();
    // Ejecutar consulta
}
```

### Patrón 2: Inserción con Parámetros
```csharp
using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO Tabla (Col1, Col2) VALUES (@Val1, @Val2)", conn))
{
    cmd.Parameters.AddWithValue("@Val1", valor1);
    cmd.Parameters.AddWithValue("@Val2", valor2);
    cmd.ExecuteNonQuery();
}
```

### Patrón 3: Filtrado de DataTable
```csharp
if (dgv.DataSource is DataTable dt)
{
    string filtro = txtBuscar.Text.Trim().Replace("'", "''");
    dt.DefaultView.RowFilter = $"Columna LIKE '%{filtro}%'";
}
```

### Patrón 4: Validación con MessageBox
```csharp
if (condicionError)
{
    MessageBox.Show("? Mensaje de error", 
        "Título", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    return;
}
```

---

## ? Comandos de Desarrollo

### Compilar Proyecto
```
Ctrl + Shift + B
```

### Ver Errores
```
Ctrl + \, E
```

### Buscar en Archivos
```
Ctrl + Shift + F
```

### Ir a Definición
```
F12
```

### Buscar Referencias
```
Shift + F12
```

---

## ?? Debugging Tips

### Error en Entradas
1. Verificar: `FormAlmacen_Entradas.cs`
2. Breakpoint en: `BtnCapturarEntrada_Click`
3. Revisar valores de DataTable

### Error en Salidas
1. Verificar: `FormAlmacen_Salidas.cs`
2. Breakpoint en: `RegistrarSalida`
3. Revisar diccionario `explosion`

### Error en Layout
1. Verificar: `FormAlmacen_Layout.cs`
2. Revisar valores de: `panelWidth`, `currentY`
3. Verificar `Anchor` y `Dock` de controles

### Error en Tema
1. Verificar: `FormAlmacen_Theme.cs`
2. Revisar existencia de controles (if != null)
3. Verificar ThemeManager.ColorX

---

## ?? Checklist de Modificación

Antes de modificar código:
- [ ] Identificar el archivo correcto
- [ ] Leer el método completo
- [ ] Entender el flujo de datos
- [ ] Hacer backup del código original

Después de modificar código:
- [ ] Compilar sin errores
- [ ] Probar funcionalidad modificada
- [ ] Probar funcionalidades relacionadas
- [ ] Verificar que no se rompió nada más
- [ ] Documentar cambios realizados

---

## ?? Referencias Rápidas

| Necesito... | Ver archivo... |
|------------|---------------|
| Cambiar SQL | `FormAlmacen_Entradas.cs` o `FormAlmacen_Salidas.cs` |
| Cambiar color | `FormAlmacen_Theme.cs` |
| Cambiar posición | `FormAlmacen_Layout.cs` |
| Agregar control | `FormAlmacen.Designer.cs` |
| Cambiar evento | Archivo correspondiente + `.Designer.cs` |
| PDF de entrada | `FormAlmacen_Entradas.cs` ? `GenerarPDFEntrada()` |
| Validación | Archivo de la funcionalidad específica |

---

**Última actualización**: 2024  
**Proyecto**: CALANDRIA RESIDENCIAL  
**Formulario**: FormAlmacen (Gestión de Almacén)
