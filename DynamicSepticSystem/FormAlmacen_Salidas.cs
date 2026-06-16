// Database operations for exits (salidas) - FormAlmacen
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace DynamicSepticSystem
{
    public partial class FormAlmacen : Form
    {
        private void TxtBuscarSalida_TextChanged(object sender, EventArgs e)
        {
            if (dgvInsumos?.DataSource is DataTable dt)
            {
                string filtro = txtBuscarSalida.Text.Trim().Replace("'", "''");
                dt.DefaultView.RowFilter =
                    $"Clave LIKE '%{filtro}%' OR Descripcion LIKE '%{filtro}%'";
            }
        }

        /// <summary>
        /// Muestra un mensaje inicial en el DataGridView de salidas pidiendo seleccionar insumos
        /// </summary>
        private void MostrarMensajeSeleccionInsumosSalida()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Mensaje", typeof(string));
            
            DataRow row = dt.NewRow();
            row["Mensaje"] = "\U0001F4E6 Seleccione insumo(s) a dirigir a esta casa...";
            dt.Rows.Add(row);
            
            dgvInsumos.DataSource = dt;
            
            // Estilizar el mensaje
            if (dgvInsumos.Columns.Count > 0)
            {
                dgvInsumos.Columns[0].HeaderText = "";
                dgvInsumos.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvInsumos.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvInsumos.Columns[0].DefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                dgvInsumos.Columns[0].DefaultCellStyle.ForeColor = ThemeManager.ColorPrincipalMenuBar;
                dgvInsumos.Columns[0].DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 240);
                dgvInsumos.RowTemplate.Height = 60;
            }
            
            dgvInsumos.ClearSelection();
            dgvInsumos.AllowUserToAddRows = false;
            dgvInsumos.ReadOnly = true;
        }

        /// <summary>
        /// Carga los insumos disponibles del inventario actual para dar salida
        /// </summary>
        private void CargarInsumosDisponiblesSalida()
        {
            if (casaActual == null || casaInventario == null)
            {
                MostrarMensajeSeleccionInsumosSalida();
                return;
            }

            try
            {
                // Crear tabla con estructura completa INCLUYENDO PRECIO E IMPORTE
                DataTable dt = new DataTable();
                dt.Columns.Add("Clave", typeof(string));
                dt.Columns.Add("Descripcion", typeof(string));
                dt.Columns.Add("Unidad", typeof(string));
                dt.Columns.Add("PrecioUnitario", typeof(decimal));
                dt.Columns.Add("Importe", typeof(decimal));
                dt.Columns.Add("Disponible", typeof(decimal));
                dt.Columns.Add("MaximoPermitido", typeof(decimal));
                dt.Columns.Add("CantidadSolicitada", typeof(decimal));

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Determinar la tabla de compras según el prototipo
                    string tablaCompras = "";
                    if (casaInventario.Prototipo.ToUpper().Contains("CALANDRIA"))
                    {
                        tablaCompras = "COMPRASCALANDRA";
                    }
                    else if (casaInventario.Prototipo.ToUpper().Contains("TUNERA"))
                    {
                        tablaCompras = "COMPRASTUNERA";
                    }

                    // Obtener todos los insumos del inventario actual con cantidad disponible, PRECIO Y MÁXIMO PERMITIDO
                    string sqlInventario = @"
                        SELECT 
                            e.Clave,
                            e.Descripcion, 
                            e.Unidad,
                            ISNULL(SUM(e.Cantidad), 0) - ISNULL((SELECT SUM(sa.Cantidad) FROM SalidasAlmacen sa WHERE sa.Clave = e.Clave), 0) AS Disponible,
                            AVG(e.PrecioUnitario) AS PrecioUnitario,
                            " + (string.IsNullOrEmpty(tablaCompras) 
                                ? "0" 
                                : $"ISNULL((SELECT TOP 1 Cantidad FROM {tablaCompras} WHERE Clave = e.Clave), 0)") + @" AS MaximoPermitido
                        FROM EntradasAlmacen e
                        GROUP BY e.Clave, e.Descripcion, e.Unidad
                        HAVING ISNULL(SUM(e.Cantidad), 0) - ISNULL((SELECT SUM(sa.Cantidad) FROM SalidasAlmacen sa WHERE sa.Clave = e.Clave), 0) > 0
                        ORDER BY e.Clave";

                    using (SqlCommand cmd = new SqlCommand(sqlInventario, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string clave = reader["Clave"].ToString();
                                
                                decimal precioUnitario = reader["PrecioUnitario"] != DBNull.Value 
                                    ? Convert.ToDecimal(reader["PrecioUnitario"]) 
                                    : 0;

                                decimal maximoPermitido = reader["MaximoPermitido"] != DBNull.Value
                                    ? Convert.ToDecimal(reader["MaximoPermitido"])
                                    : 0;

                                DataRow row = dt.NewRow();
                                row["Clave"] = clave;
                                row["Descripcion"] = reader["Descripcion"]?.ToString() ?? "Sin descripción";
                                row["Unidad"] = reader["Unidad"]?.ToString() ?? "PZA";
                                row["PrecioUnitario"] = precioUnitario;
                                row["Importe"] = 0; // Se calculará cuando se capture la cantidad
                                row["Disponible"] = reader["Disponible"] != DBNull.Value 
                                    ? Convert.ToDecimal(reader["Disponible"]) 
                                    : 0;
                                row["MaximoPermitido"] = maximoPermitido > 0 ? maximoPermitido : 999999;
                                row["CantidadSolicitada"] = 0;
                                dt.Rows.Add(row);
                            }
                        }
                    }
                }

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("\u26A0\uFE0F No hay insumos disponibles en el inventario.", 
                        "Inventario vacío", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MostrarMensajeSeleccionInsumosSalida();
                    return;
                }

                // Asignar al DataGridView
                dgvInsumos.DataSource = dt;
                
                // Agregar evento para calcular importe cuando cambie la cantidad
                dgvInsumos.CellValueChanged += DgvInsumos_CellValueChanged;
                dgvInsumos.CurrentCellDirtyStateChanged += (s, ev) =>
                {
                    if (dgvInsumos.IsCurrentCellDirty)
                    {
                        dgvInsumos.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    }
                };
                
                ConfigurarColumnasSalida();
                
                // Actualizar label de instrucciones
                if (lblInstrucciones != null)
                {
                    lblInstrucciones.Text = $"\u270F\uFE0F Capture las cantidades a retirar para M{casaActual.Manzana}-L{casaActual.Lote} ({dt.Rows.Count} insumos disponibles)";
                    lblInstrucciones.ForeColor = ThemeManager.ColorExito;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"\u274C Error al cargar inventario:\n\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Calcula el importe cuando cambia la cantidad solicitada
        /// </summary>
        private void DgvInsumos_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            
            if (dgvInsumos.Columns[e.ColumnIndex].Name == "CantidadSolicitada")
            {
                var row = dgvInsumos.Rows[e.RowIndex];
                
                if (row.Cells["CantidadSolicitada"].Value != null && 
                    row.Cells["PrecioUnitario"].Value != null)
                {
                    decimal cantidad = 0;
                    decimal precio = 0;
                    
                    if (decimal.TryParse(row.Cells["CantidadSolicitada"].Value.ToString(), out cantidad) &&
                        decimal.TryParse(row.Cells["PrecioUnitario"].Value.ToString(), out precio))
                    {
                        row.Cells["Importe"].Value = cantidad * precio;
                    }
                }
            }
        }

        /// <summary>
        /// Configura las columnas del DataGridView de salidas
        /// </summary>
        private void ConfigurarColumnasSalida()
        {
            if (dgvInsumos.Columns.Count == 0) return;

            dgvInsumos.AllowUserToAddRows = false;
            dgvInsumos.ReadOnly = false;
            dgvInsumos.RowTemplate.Height = 30;

            // Establecer el orden de las columnas
            int displayIndex = 0;

            // 1. Clave (Código)
            if (dgvInsumos.Columns.Contains("Clave"))
            {
                dgvInsumos.Columns["Clave"].HeaderText = "\U0001F511 Código";
                dgvInsumos.Columns["Clave"].Width = 100;
                dgvInsumos.Columns["Clave"].ReadOnly = true;
                dgvInsumos.Columns["Clave"].DisplayIndex = displayIndex++;
            }

            // 2. Descripcion (Insumo)
            if (dgvInsumos.Columns.Contains("Descripcion"))
            {
                dgvInsumos.Columns["Descripcion"].HeaderText = "\U0001F4E6 Insumo";
                dgvInsumos.Columns["Descripcion"].Width = 280;
                dgvInsumos.Columns["Descripcion"].ReadOnly = true;
                dgvInsumos.Columns["Descripcion"].DisplayIndex = displayIndex++;
            }

            // 3. Unidad
            if (dgvInsumos.Columns.Contains("Unidad"))
            {
                dgvInsumos.Columns["Unidad"].HeaderText = "\U0001F4CF Unidad";
                dgvInsumos.Columns["Unidad"].Width = 70;
                dgvInsumos.Columns["Unidad"].ReadOnly = true;
                dgvInsumos.Columns["Unidad"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvInsumos.Columns["Unidad"].DisplayIndex = displayIndex++;
            }

            // 4. Precio Unitario
            if (dgvInsumos.Columns.Contains("PrecioUnitario"))
            {
                dgvInsumos.Columns["PrecioUnitario"].HeaderText = "\U0001F4B5 Precio Unitario";
                dgvInsumos.Columns["PrecioUnitario"].Width = 110;
                dgvInsumos.Columns["PrecioUnitario"].ReadOnly = true;
                dgvInsumos.Columns["PrecioUnitario"].DefaultCellStyle.Format = "C2";
                dgvInsumos.Columns["PrecioUnitario"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvInsumos.Columns["PrecioUnitario"].DefaultCellStyle.BackColor = Color.FromArgb(240, 255, 240);
                dgvInsumos.Columns["PrecioUnitario"].DisplayIndex = displayIndex++;
            }

            // 5. Importe
            if (dgvInsumos.Columns.Contains("Importe"))
            {
                dgvInsumos.Columns["Importe"].HeaderText = "\U0001F4B0 Importe";
                dgvInsumos.Columns["Importe"].Width = 100;
                dgvInsumos.Columns["Importe"].ReadOnly = true;
                dgvInsumos.Columns["Importe"].DefaultCellStyle.Format = "C2";
                dgvInsumos.Columns["Importe"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvInsumos.Columns["Importe"].DefaultCellStyle.BackColor = Color.FromArgb(240, 255, 240);
                dgvInsumos.Columns["Importe"].DefaultCellStyle.Font = new Font(dgvInsumos.Font, FontStyle.Bold);
                dgvInsumos.Columns["Importe"].DisplayIndex = displayIndex++;
            }

            // 6. Disponible (Almacén)
            if (dgvInsumos.Columns.Contains("Disponible"))
            {
                dgvInsumos.Columns["Disponible"].HeaderText = "\U0001F4CA Almacén";
                dgvInsumos.Columns["Disponible"].Width = 90;
                dgvInsumos.Columns["Disponible"].ReadOnly = true;
                dgvInsumos.Columns["Disponible"].DefaultCellStyle.Format = "N2";
                dgvInsumos.Columns["Disponible"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvInsumos.Columns["Disponible"].DisplayIndex = displayIndex++;
            }

            // 7. Máximo Permitido
            if (dgvInsumos.Columns.Contains("MaximoPermitido"))
            {
                dgvInsumos.Columns["MaximoPermitido"].HeaderText = "\u26A0\uFE0F Máximo Permitido";
                dgvInsumos.Columns["MaximoPermitido"].Width = 120;
                dgvInsumos.Columns["MaximoPermitido"].ReadOnly = true;
                dgvInsumos.Columns["MaximoPermitido"].DefaultCellStyle.Format = "N2";
                dgvInsumos.Columns["MaximoPermitido"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvInsumos.Columns["MaximoPermitido"].DefaultCellStyle.ForeColor = Color.OrangeRed;
                dgvInsumos.Columns["MaximoPermitido"].DefaultCellStyle.Font = new Font(dgvInsumos.Font, FontStyle.Bold);
                dgvInsumos.Columns["MaximoPermitido"].DisplayIndex = displayIndex++;
            }

            // 8. Cantidad a Solicitar
            if (dgvInsumos.Columns.Contains("CantidadSolicitada"))
            {
                dgvInsumos.Columns["CantidadSolicitada"].HeaderText = "\u270F\uFE0F Cantidad a Solicitar";
                dgvInsumos.Columns["CantidadSolicitada"].Width = 140;
                dgvInsumos.Columns["CantidadSolicitada"].ReadOnly = false;
                dgvInsumos.Columns["CantidadSolicitada"].DefaultCellStyle.Format = "N2";
                dgvInsumos.Columns["CantidadSolicitada"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvInsumos.Columns["CantidadSolicitada"].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 220);
                dgvInsumos.Columns["CantidadSolicitada"].DisplayIndex = displayIndex++;
            }
        }

        private void RegistrarSalida()
        {
            if (casaActual == null || casaInventario == null)
            {
                MessageBox.Show("\u26A0\uFE0F Primero selecciona una casa destino.", 
                    "Casa requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataTable dt = dgvInsumos.DataSource as DataTable;
            if (dt == null || dt.Columns.Count == 1) // Si solo tiene la columna de mensaje
            {
                MessageBox.Show("\u26A0\uFE0F No hay insumos cargados para registrar salida.", 
                    "Sin insumos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var usuario = Global.UsuarioActual?.Nombre ?? Environment.UserName;
            bool seRegistroAlgo = false;
            int insumosValidados = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                
                foreach (DataRow row in dt.Rows)
                {
                    if (!dt.Columns.Contains("CantidadSolicitada") || row.IsNull("CantidadSolicitada"))
                        continue;

                    decimal solicitada = Convert.ToDecimal(row["CantidadSolicitada"]);
                    
                    // Ignorar si no se solicitó nada
                    if (solicitada <= 0)
                        continue;

                    string clave = row["Clave"].ToString();
                    string descripcion = row["Descripcion"].ToString();
                    string unidad = row["Unidad"].ToString();
                    decimal disponible = Convert.ToDecimal(row["Disponible"]);
                    decimal maximoPermitido = Convert.ToDecimal(row["MaximoPermitido"]);
                    decimal precioUnitario = Convert.ToDecimal(row["PrecioUnitario"]);
                    decimal importe = solicitada * precioUnitario;

                    insumosValidados++;

                    // Validar que no exceda el máximo permitido (si está definido)
                    if (maximoPermitido < 999999 && solicitada > maximoPermitido)
                    {
                        MessageBox.Show(
                            $"\u26A0\uFE0F La cantidad solicitada para {clave} excede el máximo permitido.\n\n" +
                            $"Solicitado: {solicitada:N2}\n" +
                            $"Máximo permitido: {maximoPermitido:N2}", 
                            "Cantidad excedida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    // Validar que haya suficiente disponible
                    if (solicitada > disponible)
                    {
                        var result = MessageBox.Show(
                            $"\u26A0\uFE0F La cantidad solicitada para {clave} excede el inventario disponible.\n\n" +
                            $"Solicitado: {solicitada:N2}\n" +
                            $"Disponible: {disponible:N2}\n\n" +
                            $"¿Desea registrar la salida de todos modos?", 
                            "Inventario insuficiente", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        
                        if (result != DialogResult.Yes)
                            continue;
                    }

                    // Insertar en SalidasAlmacen CON PRECIO E IMPORTE
                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO SalidasAlmacen 
                        (Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe, FechaSalida, Manzana, Lote, Prototipo, Justificacion) 
                        VALUES (@Clave, @Descripcion, @Unidad, @Cantidad, @PrecioUnitario, @Importe, GETDATE(), @Manzana, @Lote, @Prototipo, NULL)", conn))
                    {
                        cmd.Parameters.AddWithValue("@Clave", clave);
                        cmd.Parameters.AddWithValue("@Descripcion", descripcion);
                        cmd.Parameters.AddWithValue("@Unidad", unidad);
                        cmd.Parameters.AddWithValue("@Cantidad", solicitada);
                        cmd.Parameters.AddWithValue("@PrecioUnitario", precioUnitario);
                        cmd.Parameters.AddWithValue("@Importe", importe);
                        cmd.Parameters.AddWithValue("@Manzana", casaActual.Manzana);
                        cmd.Parameters.AddWithValue("@Lote", casaActual.Lote);
                        cmd.Parameters.AddWithValue("@Prototipo", casaInventario.Prototipo);
                        cmd.ExecuteNonQuery();
                    }

                    // Insertar en HistorialMovimientos CON PRECIO E IMPORTE
                    using (SqlCommand hist = new SqlCommand(@"
                        INSERT INTO HistorialMovimientos 
                        (Fecha, TipoMovimiento, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe, Usuario, Manzana, Lote, Prototipo, Justificacion) 
                        VALUES (GETDATE(), 'Salida', @Clave, @Descripcion, @Unidad, @Cantidad, @PrecioUnitario, @Importe, @Usuario, @Manzana, @Lote, @Prototipo, NULL)", conn))
                    {
                        hist.Parameters.AddWithValue("@Clave", clave);
                        hist.Parameters.AddWithValue("@Descripcion", descripcion);
                        hist.Parameters.AddWithValue("@Unidad", unidad);
                        hist.Parameters.AddWithValue("@Cantidad", solicitada);
                        hist.Parameters.AddWithValue("@PrecioUnitario", precioUnitario);
                        hist.Parameters.AddWithValue("@Importe", importe);
                        hist.Parameters.AddWithValue("@Usuario", usuario);
                        hist.Parameters.AddWithValue("@Manzana", casaActual.Manzana);
                        hist.Parameters.AddWithValue("@Lote", casaActual.Lote);
                        hist.Parameters.AddWithValue("@Prototipo", casaInventario.Prototipo);
                        hist.ExecuteNonQuery();
                    }

                    seRegistroAlgo = true;
                }
            }

            if (seRegistroAlgo)
            {
                // ?? GENERAR PDF DEL VALE DE SALIDA
                try
                {
                    GenerarValeSalidaPDF();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"?? La salida se registró correctamente pero hubo un error al generar el vale PDF:\n\n{ex.Message}",
                        "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                
                CargarHistorial();
                
                // Limpiar el formulario
                txtTrimManzanaSalida.Clear();
                txtTrimLoteSalida.Clear();
                casaActual = null;
                casaInventario = null;
                explosion = null;
                lblCasaSalida.Text = "";
                lblInstrucciones.Text = "\U0001F3E0 SELECCIONA CASA DESTINO";
                lblInstrucciones.ForeColor = ThemeManager.ColorPrincipalMenuBar;
                
                MostrarMensajeSeleccionInsumosSalida();
            }
            else
            {
                if (insumosValidados == 0)
                {
                    MessageBox.Show("\u2139\uFE0F No se capturó ninguna cantidad para registrar salida.", 
                        "Sin cantidades", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("\u26A0\uFE0F No se registró ninguna salida debido a validaciones.", 
                        "Sin salidas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void BtnSeleccionarCasaSalida_Click(object sender, EventArgs e)
        {
            string manzana = txtTrimManzanaSalida.Text.Trim();
            string lote = txtTrimLoteSalida.Text.Trim();

            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote))
            {
                MessageBox.Show("\u26A0\uFE0F Debes ingresar manzana y lote.", 
                    "Datos requeridos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Primero mostrar el mensaje de selección
            MostrarMensajeSeleccionInsumosSalida();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT Prototipo FROM InventarioCasas WHERE Manzana = @Manzana AND Lote = @Lote", conn))
                    {
                        cmd.Parameters.AddWithValue("@Manzana", manzana);
                        cmd.Parameters.AddWithValue("@Lote", lote);

                        var prototipo = cmd.ExecuteScalar()?.ToString();
                        if (string.IsNullOrEmpty(prototipo))
                        {
                            MessageBox.Show(
                                $"\u274C No se encontró la casa M{manzana}-L{lote} en el inventario.", 
                                "Casa no encontrada", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Configurar casaActual
                        casaActual = new PanelPrincipal.Casa { Manzana = manzana, Lote = lote };
                        casaInventario = new DynamicSepticSystem.CasaInventario { Prototipo = prototipo };
                        
                        // Actualizar labels
                        lblCasaSalida.Text = $"\U0001F3E0 M{casaActual.Manzana}-L{casaActual.Lote} ({casaInventario.Prototipo})";
                        lblCasaSalida.ForeColor = ThemeManager.ColorExito;
                        lblCasaSalida.Font = new Font(lblCasaSalida.Font, FontStyle.Bold);
                        
                        // Cargar explosión de insumos (opcional, para los máximos permitidos)
                        CargarExplosionParaSalida(prototipo);
                        
                        // Cargar el inventario actual disponible
                        CargarInsumosDisponiblesSalida();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"\u274C Error al seleccionar casa:\n\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Abre el repositorio de vales de salida
        /// </summary>
        private void BtnVerRepositorioVales_Click(object sender, EventArgs e)
        {
            try
            {
                // Si hay una casa seleccionada, abrir el repositorio filtrado por esa casa
                if (casaActual != null && !string.IsNullOrWhiteSpace(casaActual.Manzana) && !string.IsNullOrWhiteSpace(casaActual.Lote))
                {
                    AbrirRepositorioValesSalida(casaActual.Manzana, casaActual.Lote);
                }
                else
                {
                    // Abrir el repositorio mostrando todos los vales
                    AbrirRepositorioValesSalida();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir repositorio de vales: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvInsumos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Evento para futuras funcionalidades
        }
    }
}
