using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Clase parcial que contiene la funcionalidad del men� contextual para partidas
    /// </summary>
    public partial class FormEstimacionConceptoMigrado
    {
        // Referencia a la opci�n de terminar sin estimaci�n (solo admin)
        private ToolStripMenuItem menuTerminarSinEstimacion;
        
        /// <summary>
        /// Configura el men� contextual para partidas din�micas
        /// </summary>
        private void ConfigurarMenuContextual()
        {
            contextMenuPartidas = new ContextMenuStrip();
            contextMenuPartidas.Opening += ContextMenuPartidas_Opening;
            
            // Opci�n para capturar m�
            var menuCapturarM2 = new ToolStripMenuItem
            {
                Text = "?? Capturar Metros Cuadrados (m�)",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Image = null
            };
            menuCapturarM2.Click += MenuCapturarM2_Click;
            
            // Separador
            var separador1 = new ToolStripSeparator();
            
            // Opci�n para marcar/desmarcar
            var menuMarcar = new ToolStripMenuItem
            {
                Text = "? Marcar/Desmarcar Partida",
                Font = new Font("Segoe UI", 9F),
            };
            menuMarcar.Click += MenuMarcar_Click;
            
            // Opci�n para ver detalles
            var menuDetalles = new ToolStripMenuItem
            {
                Text = "? Ver Detalles",
                Font = new Font("Segoe UI", 9F),
            };
            menuDetalles.Click += MenuDetalles_Click;
            
            // Separador
            var separador2 = new ToolStripSeparator();
            
            // ?? OPCI�N ADMIN: Terminar partida sin generar estimaci�n
            menuTerminarSinEstimacion = new ToolStripMenuItem
            {
                Text = "?? Terminar sin Estimaci�n [ADMIN]",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(142, 68, 173), // Color p�rpura para indicar admin
                Visible = false // Se mostrar� solo para admin
            };
            menuTerminarSinEstimacion.Click += MenuTerminarSinEstimacion_Click;
            
            // Separador para opciones admin
            var separadorAdmin = new ToolStripSeparator();
            separadorAdmin.Name = "separadorAdmin";
            
            // Opci�n para resetear progreso
            var menuResetear = new ToolStripMenuItem
            {
                Text = "?? Resetear Progreso",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(231, 76, 60)
            };
            menuResetear.Click += MenuResetear_Click;

            // Separador para Propiedades
            var separadorPropiedades = new ToolStripSeparator();

            // Opci�n Propiedades: adjuntar fotos al concepto/partida
            var menuPropiedades = new ToolStripMenuItem
            {
                Text = "?? Propiedades (Fotos)",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185)
            };
            menuPropiedades.Click += MenuPropiedades_Click;

            // NOTA: las opciones de Propiedades se agregan al FINAL para no alterar
            // los �ndices usados en ContextMenuPartidas_Opening.
            contextMenuPartidas.Items.AddRange(new ToolStripItem[] {
                menuCapturarM2,
                separador1,
                menuMarcar,
                menuDetalles,
                separador2,
                menuTerminarSinEstimacion,
                separadorAdmin,
                menuResetear,
                separadorPropiedades,
                menuPropiedades
            });
            
            // Aplicar tema al men� contextual
            contextMenuPartidas.BackColor = Color.White;
            contextMenuPartidas.ForeColor = Color.FromArgb(52, 73, 94);
            
            // Asignar el men� contextual al TreeListView
            olvEstimacionConceptos.ContextMenuStrip = contextMenuPartidas;
        }
        
        /// <summary>
        /// Se ejecuta antes de mostrar el men� contextual para ajustar las opciones
        /// </summary>
        private void ContextMenuPartidas_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var nodoSeleccionado = olvEstimacionConceptos.SelectedObject as NodoConcepto;
            
            if (nodoSeleccionado == null)
            {
                e.Cancel = true;
                return;
            }
            
            // Verificar si es admin para mostrar opciones exclusivas
            bool esAdmin = EsUsuarioAdmin();
            
            // Obtener los items del men�
            var menuCapturarM2 = contextMenuPartidas.Items[0] as ToolStripMenuItem;
            var menuMarcar = contextMenuPartidas.Items[2] as ToolStripMenuItem;
            var menuResetear = contextMenuPartidas.Items[7] as ToolStripMenuItem;
            var separadorAdmin = contextMenuPartidas.Items[6] as ToolStripSeparator;
            
            // ?? Mostrar/ocultar opciones de admin
            menuTerminarSinEstimacion.Visible = esAdmin;
            separadorAdmin.Visible = esAdmin;
            
            if (nodoSeleccionado.EsConcepto)
            {
                menuCapturarM2.Visible = false;
                menuMarcar.Text = "? Marcar/Desmarcar Concepto";
                menuResetear.Text = "?? Resetear Progreso del Concepto";
                menuResetear.Enabled = nodoSeleccionado.Partidas.Any(p => p.Completado || p.AvancePorcentaje > 0);
                
                // Para conceptos, la opci�n admin termina todas las partidas
                menuTerminarSinEstimacion.Text = "?? Terminar Concepto sin Estimaci�n [ADMIN]";
                menuTerminarSinEstimacion.Enabled = esAdmin && !nodoSeleccionado.Completado && 
                    nodoSeleccionado.Partidas.Any(p => !p.Completado);
            }
            else
            {
                menuCapturarM2.Visible = nodoSeleccionado.EsDinamica && !nodoSeleccionado.Completado;
                menuMarcar.Text = "? Marcar/Desmarcar Partida";
                menuResetear.Text = "?? Resetear Progreso de la Partida";
                
                if (nodoSeleccionado.EsDinamica && nodoSeleccionado.MetrosCuadrados > 0)
                {
                    menuCapturarM2.Text = $"?? Editar m� (actual: {nodoSeleccionado.MetrosCuadrados:F2} m�)";
                }
                else
                {
                    menuCapturarM2.Text = "?? Capturar Metros Cuadrados (m�)";
                }
                
                menuResetear.Enabled = nodoSeleccionado.Completado || nodoSeleccionado.AvancePorcentaje > 0;
                
                // Para partidas individuales
                menuTerminarSinEstimacion.Text = "?? Terminar sin Estimaci�n [ADMIN]";
                menuTerminarSinEstimacion.Enabled = esAdmin && !nodoSeleccionado.Completado;
            }
            
            if (nodoSeleccionado.Completado)
            {
                menuCapturarM2.Enabled = false;
                menuMarcar.Enabled = false;
            }
            else
            {
                menuCapturarM2.Enabled = true;
                menuMarcar.Enabled = true;
            }
        }
        
        /// <summary>
        /// ?? ADMIN ONLY: Muestra di�logo para solicitar fecha de finalizaci�n
        /// </summary>
        /// <returns>La fecha seleccionada o null si se cancel�</returns>
        private DateTime? MostrarDialogoFechaFinalizacion(string nombrePartidaOConcepto, bool esConcepto)
        {
            using (var formFecha = new Form
            {
                Text = "?? Fecha de Finalizaci�n [ADMIN]",
                Size = new Size(420, 280),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            })
            {
                // T�tulo
                var lblTitulo = new Label
                {
                    Text = "?? Seleccionar Fecha de Finalizaci�n",
                    Location = new Point(20, 20),
                    AutoSize = false,
                    Width = 370,
                    Height = 30,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(142, 68, 173),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Descripci�n
                var lblDescripcion = new Label
                {
                    Text = esConcepto 
                        ? $"Concepto: {nombrePartidaOConcepto}\n\nSelecciona la fecha en que se termin� este concepto:"
                        : $"Partida: {nombrePartidaOConcepto}\n\nSelecciona la fecha en que se termin� esta partida:",
                    Location = new Point(20, 55),
                    AutoSize = false,
                    Width = 370,
                    Height = 60,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(52, 73, 94)
                };

                // DateTimePicker
                var dtpFecha = new DateTimePicker
                {
                    Location = new Point(20, 125),
                    Width = 200,
                    Format = DateTimePickerFormat.Short,
                    Value = DateTime.Today,
                    MaxDate = DateTime.Today, // No permitir fechas futuras
                    Font = new Font("Segoe UI", 10F)
                };

                // Label informativo
                var lblInfo = new Label
                {
                    Text = "?? No se permiten fechas futuras",
                    Location = new Point(230, 128),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                    ForeColor = Color.FromArgb(149, 165, 166)
                };

                // Checkbox para usar fecha actual
                var chkHoy = new CheckBox
                {
                    Text = "Usar fecha de hoy",
                    Location = new Point(20, 160),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9F),
                    Checked = true
                };

                chkHoy.CheckedChanged += (s, ev) =>
                {
                    dtpFecha.Enabled = !chkHoy.Checked;
                    if (chkHoy.Checked)
                    {
                        dtpFecha.Value = DateTime.Today;
                    }
                };

                // Inicialmente deshabilitar el DateTimePicker
                dtpFecha.Enabled = false;

                // Bot�n Aceptar
                var btnOk = new Button
                {
                    Text = "? Aceptar",
                    Location = new Point(150, 200),
                    Width = 120,
                    Height = 40,
                    DialogResult = DialogResult.OK,
                    BackColor = Color.FromArgb(142, 68, 173),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnOk.FlatAppearance.BorderSize = 0;

                // Bot�n Cancelar
                var btnCancel = new Button
                {
                    Text = "? Cancelar",
                    Location = new Point(280, 200),
                    Width = 100,
                    Height = 40,
                    DialogResult = DialogResult.Cancel,
                    BackColor = Color.FromArgb(149, 165, 166),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnCancel.FlatAppearance.BorderSize = 0;

                formFecha.Controls.AddRange(new Control[] {
                    lblTitulo, lblDescripcion, dtpFecha, lblInfo, chkHoy, btnOk, btnCancel
                });

                formFecha.AcceptButton = btnOk;
                formFecha.CancelButton = btnCancel;

                if (formFecha.ShowDialog(this) == DialogResult.OK)
                {
                    return dtpFecha.Value.Date;
                }
                
                return null;
            }
        }

        /// <summary>
        /// ?? ADMIN ONLY: Termina una partida o concepto al 100% sin generar estimaci�n
        /// </summary>
        private void MenuTerminarSinEstimacion_Click(object sender, EventArgs e)
        {
            // Verificar que sea admin
            if (!EsUsuarioAdmin())
            {
                MessageBox.Show(
                    "Esta funci�n solo est� disponible para el usuario administrador.",
                    "Acceso Restringido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            var nodoSeleccionado = olvEstimacionConceptos.SelectedObject as NodoConcepto;
            
            if (nodoSeleccionado == null)
            {
                return;
            }
            
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
            {
                MessageBox.Show("Selecciona Manzana y Lote primero", "Atenci�n", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();
            
            // Confirmaci�n
            string mensaje;
            if (nodoSeleccionado.EsConcepto)
            {
                int partidasPendientes = nodoSeleccionado.Partidas.Count(p => !p.Completado);
                mensaje = $"�Deseas marcar como TERMINADAS las {partidasPendientes} partidas pendientes del concepto '{nodoSeleccionado.Nombre}'?\n\n" +
                         $"?? ADVERTENCIA:\n" +
                         $"� Esta acci�n NO generar� una estimaci�n/PDF.\n" +
                         $"� Las partidas se marcar�n al 100% directamente en la base de datos.\n" +
                         $"� Esta acci�n es solo para correcciones administrativas.\n\n" +
                         $"�Continuar?";
            }
            else
            {
                double totalPartida = ObtenerTotalPartida(nodoSeleccionado);
                mensaje = $"�Deseas marcar como TERMINADA la partida '{nodoSeleccionado.Nombre}'?\n\n" +
                         $"WBS: {nodoSeleccionado.WBS}\n" +
                         $"Monto: {totalPartida:C2}\n\n" +
                         $"?? ADVERTENCIA:\n" +
                         $"� Esta acci�n NO generar� una estimaci�n/PDF.\n" +
                         $"� La partida se marcar� al 100% directamente en la base de datos.\n" +
                         $"� Esta acci�n es solo para correcciones administrativas.\n\n" +
                         $"�Continuar?";
            }
            
            var result = MessageBox.Show(
                mensaje,
                "?? Terminar sin Estimaci�n [ADMIN]",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            
            if (result != DialogResult.Yes)
            {
                return;
            }
            
            // ?? Solicitar fecha de finalizaci�n
            DateTime? fechaFinalizacion = MostrarDialogoFechaFinalizacion(
                nodoSeleccionado.Nombre, 
                nodoSeleccionado.EsConcepto);
            
            if (!fechaFinalizacion.HasValue)
            {
                // El usuario cancel�
                MessageBox.Show("Operaci�n cancelada.", "Cancelado", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Verificar si existe columna MetrosCuadrados
                    bool tieneMetrosCuadrados = false;
                    using (SqlCommand cmdCheck = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'AvanceManualObra' 
                        AND COLUMN_NAME = 'MetrosCuadrados'", conn))
                    {
                        tieneMetrosCuadrados = (int)cmdCheck.ExecuteScalar() > 0;
                    }
                    
                    int partidasTerminadas = 0;
                    
                    if (nodoSeleccionado.EsConcepto)
                    {
                        // Terminar todas las partidas del concepto
                        foreach (var partida in nodoSeleccionado.Partidas.Where(p => !p.Completado))
                        {
                            TerminarPartidaSinEstimacion(conn, manzana, lote, partida, tieneMetrosCuadrados, fechaFinalizacion.Value);
                            partidasTerminadas++;
                        }
                        
                        // Tambi�n registrar el concepto como completado (WBS negativo)
                        RegistrarConceptoCompletado(conn, manzana, lote, nodoSeleccionado, fechaFinalizacion.Value);
                    }
                    else
                    {
                        // Terminar solo la partida seleccionada
                        TerminarPartidaSinEstimacion(conn, manzana, lote, nodoSeleccionado, tieneMetrosCuadrados, fechaFinalizacion.Value);
                        partidasTerminadas = 1;
                        
                        // Verificar si el concepto padre qued� completo
                        var padre = EncontrarPadre(nodoSeleccionado);
                        if (padre != null)
                        {
                            bool todasCompletadas = padre.Partidas.All(p => p.Completado);
                            if (todasCompletadas)
                            {
                                RegistrarConceptoCompletado(conn, manzana, lote, padre, fechaFinalizacion.Value);
                            }
                        }
                    }
                    
                    // Recargar datos
                    CargarEstimacionJerarquica(manzana, lote);
                    
                    MessageBox.Show(
                        $"? {partidasTerminadas} partida(s) marcada(s) como TERMINADA(s).\n\n" +
                        $"?? Fecha de finalizaci�n: {fechaFinalizacion.Value:dd/MM/yyyy}\n\n" +
                        $"Los cambios se han guardado directamente en la base de datos.\n" +
                        $"No se gener� ninguna estimaci�n.",
                        "Operaci�n Completada",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Operaci�n cancelada.", "Cancelado", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al terminar partida(s): {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Termina una partida individual sin generar estimaci�n
        /// </summary>
        private void TerminarPartidaSinEstimacion(SqlConnection conn, string manzana, string lote, 
            NodoConcepto partida, bool tieneMetrosCuadrados, DateTime fechaFinalizacion)
        {
            // Calcular monto ejecutado
            double montoEjecutado = ObtenerTotalPartida(partida);
            
            // Si es din�mica y no tiene m�, usar el l�mite o un valor predeterminado
            if (partida.EsDinamica && partida.MetrosCuadrados <= 0)
            {
                double limite = partida.ObtenerLimiteEfectivo(prototipoActual);
                if (limite > 0)
                {
                    partida.MetrosCuadrados = limite;
                }
                else
                {
                    // Si no hay l�mite, pedir al admin que ingrese los m�
                    using (var inputForm = new Form
                    {
                        Text = "Ingresar m� para partida din�mica",
                        Size = new Size(350, 180),
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        MaximizeBox = false,
                        MinimizeBox = false
                    })
                    {
                        var lbl = new Label
                        {
                            Text = $"La partida '{partida.Nombre}' es din�mica.\nIngresa los m� ejecutados:",
                            Location = new Point(20, 20),
                            AutoSize = true
                        };
                        
                        var numM2 = new NumericUpDown
                        {
                            Location = new Point(20, 60),
                            Width = 150,
                            DecimalPlaces = 2,
                            Minimum = 0M,  // ? CAMBIADO: Permite 0
                            Maximum = 99999M,
                            Value = 1M
                        };
                        
                        var btnOk = new Button
                        {
                            Text = "Aceptar",
                            DialogResult = DialogResult.OK,
                            Location = new Point(100, 100),
                            Width = 80
                        };
                        
                        var btnCancel = new Button
                        {
                            Text = "Cancelar",
                            DialogResult = DialogResult.Cancel,
                            Location = new Point(190, 100),
                            Width = 80
                        };
                        
                        inputForm.Controls.AddRange(new Control[] { lbl, numM2, btnOk, btnCancel });
                        inputForm.AcceptButton = btnOk;
                        inputForm.CancelButton = btnCancel;
                        
                        if (inputForm.ShowDialog() == DialogResult.OK)
                        {
                            partida.MetrosCuadrados = (double)numM2.Value;
                        }
                        else
                        {
                            throw new OperationCanceledException("Operaci�n cancelada por el usuario.");
                        }
                    }
                }
                
                // Recalcular monto con los m� actualizados
                double valorM2 = ObtenerValorM2(partida);
                montoEjecutado = partida.MetrosCuadrados * valorM2;
            }
            
            string sql;
            if (tieneMetrosCuadrados)
            {
                sql = @"
                    IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                        UPDATE AvanceManualObra 
                        SET AvancePorcentaje = 100.0, 
                            MontoEjecutado = @monto, 
                            MetrosCuadrados = @metrosC,
                            FechaActualizacion = GETDATE(),
                            FechaFinalizacion = @fechaFin
                        WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                    ELSE
                        INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, AvancePorcentaje, MontoEjecutado, MetrosCuadrados, FechaFinalizacion)
                        VALUES (@m, @l, @proto, @wbs, 100.0, @monto, @metrosC, @fechaFin)";
            }
            else
            {
                sql = @"
                    IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                        UPDATE AvanceManualObra 
                        SET AvancePorcentaje = 100.0, 
                            MontoEjecutado = @monto, 
                            FechaActualizacion = GETDATE(),
                            FechaFinalizacion = @fechaFin
                        WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                    ELSE
                        INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, AvancePorcentaje, MontoEjecutado, FechaFinalizacion)
                        VALUES (@m, @l, @proto, @wbs, 100.0, @monto, @fechaFin)";
            }
            
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                cmd.Parameters.AddWithValue("@proto", string.IsNullOrEmpty(prototipoActual) ? (object)DBNull.Value : prototipoActual);
                cmd.Parameters.AddWithValue("@wbs", partida.WBS.ToString());
                cmd.Parameters.AddWithValue("@monto", montoEjecutado);
                cmd.Parameters.AddWithValue("@fechaFin", fechaFinalizacion);
                
                if (tieneMetrosCuadrados)
                {
                    cmd.Parameters.AddWithValue("@metrosC", partida.MetrosCuadrados);
                }
                
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException)
                {
                    // Fallback sin FechaFinalizacion si la columna no existe
                    string fallback = tieneMetrosCuadrados
                        ? @"
                            IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                                UPDATE AvanceManualObra 
                                SET AvancePorcentaje = 100.0, MontoEjecutado = @monto, MetrosCuadrados = @metrosC, FechaActualizacion = GETDATE()
                                WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                            ELSE
                                INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, AvancePorcentaje, MontoEjecutado, MetrosCuadrados)
                                VALUES (@m, @l, @proto, @wbs, 100.0, @monto, @metrosC)"
                        : @"
                            IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                                UPDATE AvanceManualObra 
                                SET AvancePorcentaje = 100.0, MontoEjecutado = @monto, FechaActualizacion = GETDATE()
                                WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                            ELSE
                                INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, AvancePorcentaje, MontoEjecutado)
                                VALUES (@m, @l, @proto, @wbs, 100.0, @monto)";
                    
                    using (SqlCommand cmd2 = new SqlCommand(fallback, conn))
                    {
                        cmd2.Parameters.AddWithValue("@m", manzana);
                        cmd2.Parameters.AddWithValue("@l", lote);
                        cmd2.Parameters.AddWithValue("@proto", string.IsNullOrEmpty(prototipoActual) ? (object)DBNull.Value : prototipoActual);
                        cmd2.Parameters.AddWithValue("@wbs", partida.WBS.ToString());
                        cmd2.Parameters.AddWithValue("@monto", montoEjecutado);
                        if (tieneMetrosCuadrados)
                        {
                            cmd2.Parameters.AddWithValue("@metrosC", partida.MetrosCuadrados);
                        }
                        cmd2.ExecuteNonQuery();
                    }
                }
            }
            
            // Actualizar el objeto en memoria
            partida.Completado = true;
            partida.AvancePorcentaje = 100;
            partida.MontoEjecutado = montoEjecutado;
            partida.FechaFinalizacion = fechaFinalizacion;
            
            System.Diagnostics.Debug.WriteLine(
                $"[ADMIN] Partida terminada sin estimaci�n: WBS={partida.WBS}, " +
                $"Monto={montoEjecutado:C2}, m�={partida.MetrosCuadrados:F2}, " +
                $"Fecha={fechaFinalizacion:dd/MM/yyyy}");
        }
        
        /// <summary>
        /// Registra un concepto como completado (con WBS negativo)
        /// </summary>
        private void RegistrarConceptoCompletado(SqlConnection conn, string manzana, string lote, NodoConcepto concepto, DateTime fechaFinalizacion)
        {
            if (!int.TryParse(concepto.Codigo, out int codigoNumerico))
            {
                return;
            }
            
            int wbsConcepto = -codigoNumerico; // WBS negativo para conceptos
            double totalConcepto = concepto.Partidas.Sum(p => p.MontoEjecutado);
            
            string sql = @"
                IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                    UPDATE AvanceManualObra 
                    SET AvancePorcentaje = 100.0, 
                        MontoEjecutado = @monto, 
                        Concepto = @concepto,
                        FechaActualizacion = GETDATE(),
                        FechaFinalizacion = @fechaFin
                    WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                ELSE
                    INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, Concepto, AvancePorcentaje, MontoEjecutado, FechaFinalizacion)
                    VALUES (@m, @l, @proto, @wbs, @concepto, 100.0, @monto, @fechaFin)";
            
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                cmd.Parameters.AddWithValue("@proto", string.IsNullOrEmpty(prototipoActual) ? (object)DBNull.Value : prototipoActual);
                cmd.Parameters.AddWithValue("@wbs", wbsConcepto.ToString());
                cmd.Parameters.AddWithValue("@concepto", concepto.Nombre);
                cmd.Parameters.AddWithValue("@monto", totalConcepto);
                cmd.Parameters.AddWithValue("@fechaFin", fechaFinalizacion);
                
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException)
                {
                    // Fallback sin FechaFinalizacion
                    string fallback = @"
                        IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                            UPDATE AvanceManualObra 
                            SET AvancePorcentaje = 100.0, MontoEjecutado = @monto, Concepto = @concepto, FechaActualizacion = GETDATE()
                            WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                        ELSE
                            INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, Concepto, AvancePorcentaje, MontoEjecutado)
                            VALUES (@m, @l, @proto, @wbs, @concepto, 100.0, @monto)";
                    
                    using (SqlCommand cmd2 = new SqlCommand(fallback, conn))
                    {
                        cmd2.Parameters.AddWithValue("@m", manzana);
                        cmd2.Parameters.AddWithValue("@l", lote);
                        cmd2.Parameters.AddWithValue("@proto", string.IsNullOrEmpty(prototipoActual) ? (object)DBNull.Value : prototipoActual);
                        cmd2.Parameters.AddWithValue("@wbs", wbsConcepto.ToString());
                        cmd2.Parameters.AddWithValue("@concepto", concepto.Nombre);
                        cmd2.Parameters.AddWithValue("@monto", totalConcepto);
                        cmd2.ExecuteNonQuery();
                    }
                }
            }
            
            System.Diagnostics.Debug.WriteLine(
                $"[ADMIN] Concepto registrado como completado: WBS={wbsConcepto}, " +
                $"Concepto={concepto.Nombre}, Total={totalConcepto:C2}, " +
                $"Fecha={fechaFinalizacion:dd/MM/yyyy}");
        }
        
        /// <summary>
        /// Manejador para la opci�n de capturar m�
        /// </summary>
        private void MenuCapturarM2_Click(object sender, EventArgs e)
        {
            var nodoSeleccionado = olvEstimacionConceptos.SelectedObject as NodoConcepto;
            
            if (nodoSeleccionado == null || !nodoSeleccionado.EsDinamica || nodoSeleccionado.EsConcepto)
            {
                return;
            }
            
            if (nodoSeleccionado.Completado)
            {
                MessageBox.Show("Esta partida ya est� completada y no puede modificarse.", 
                    "Partida Completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            MostrarDialogoCapturarM2(nodoSeleccionado);
        }
        
        /// <summary>
        /// Muestra el di�logo para capturar metros cuadrados
        /// </summary>
        private void MostrarDialogoCapturarM2(NodoConcepto partida)
        {
            using (var formM2 = new Form
            {
                Text = "Partida Din�mica - Capturar m�",
                Size = new Size(480, 280),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            })
            {
                // T�tulo
                var lblTitulo = new Label
                {
                    Text = "?? Captura de Metros Cuadrados",
                    Location = new Point(20, 20),
                    AutoSize = false,
                    Width = 430,
                    Height = 30,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(41, 128, 185),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Nombre de la partida
                var lblPartida = new Label
                {
                    Text = $"Partida: {partida.Nombre}",
                    Location = new Point(20, 55),
                    AutoSize = false,
                    Width = 430,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(52, 73, 94)
                };

                // Informaci�n del valor por m�
                double valorM2 = prototipoActual.ToUpper().Contains("TUNERA") ? partida.ValorM2Tunera : partida.ValorM2Calandra;
                var lblInfo = new Label
                {
                    Text = $"?? Valor por m� ({prototipoActual}): {valorM2:C2}",
                    Location = new Point(20, 80),
                    AutoSize = false,
                    Width = 430,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(39, 174, 96)
                };

                // Panel para entrada de m�
                var panelEntrada = new Panel
                {
                    Location = new Point(20, 115),
                    Size = new Size(430, 60),
                    BackColor = Color.FromArgb(236, 240, 241),
                    BorderStyle = BorderStyle.FixedSingle
                };

                var lblM2 = new Label
                {
                    Text = "Metros cuadrados (m�):",
                    Location = new Point(10, 10),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };

                var numM2 = new NumericUpDown
                {
                    Location = new Point(10, 30),
                    Width = 150,
                    DecimalPlaces = 2,
                    Minimum = 0M,  // ? CAMBIADO: Ahora permite 0
                    Maximum = 99999M,
                    Value = partida.MetrosCuadrados > 0 ? (decimal)partida.MetrosCuadrados : 0M,
                    Font = new Font("Segoe UI", 11F),
                    TextAlign = HorizontalAlignment.Right
                };

                var lblCostoCalculado = new Label
                {
                    Location = new Point(180, 30),
                    AutoSize = false,
                    Width = 240,
                    Height = 30,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(230, 126, 34),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                EventHandler actualizarCosto = (s, ev) =>
                {
                    double costoCalculado = (double)numM2.Value * valorM2;
                    lblCostoCalculado.Text = $"= {costoCalculado:C2}";
                };
                numM2.ValueChanged += actualizarCosto;
                actualizarCosto(null, EventArgs.Empty);

                panelEntrada.Controls.Add(lblM2);
                panelEntrada.Controls.Add(numM2);
                panelEntrada.Controls.Add(lblCostoCalculado);

                // Botones
                var btnOk = new Button
                {
                    Text = "? Guardar",
                    Location = new Point(200, 190),
                    Width = 120,
                    Height = 40,
                    DialogResult = DialogResult.OK,
                    BackColor = Color.FromArgb(46, 204, 113),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text = "? Cancelar",
                    Location = new Point(330, 190),
                    Width = 120,
                    Height = 40,
                    DialogResult = DialogResult.Cancel,
                    BackColor = Color.FromArgb(149, 165, 166),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnCancel.FlatAppearance.BorderSize = 0;

                formM2.Controls.AddRange(new Control[] {
                    lblTitulo, lblPartida, lblInfo, panelEntrada, btnOk, btnCancel
                });

                formM2.AcceptButton = btnOk;
                formM2.CancelButton = btnCancel;
                numM2.Select();
                numM2.Select(0, numM2.Text.Length);

                if (formM2.ShowDialog(this) == DialogResult.OK)
                {
                    double nuevoM2 = (double)numM2.Value;
                    
                    // ? CAMBIADO: Ahora permite 0 (�til para resetear)
                    if (nuevoM2 < 0)
                    {
                        MessageBox.Show("Los metros cuadrados no pueden ser negativos.", 
                            "Validaci�n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    partida.MetrosCuadrados = nuevoM2;
                    partida.Total = nuevoM2 * valorM2;

                    System.Diagnostics.Debug.WriteLine(
                        $"? m� capturados: WBS={partida.WBS}, m�={nuevoM2:F2}, " +
                        $"Valor/m�={valorM2:C2}, Total={partida.Total:C2}");

                    var padre = EncontrarPadre(partida);
                    if (padre != null)
                    {
                        RecalcularAvanceConcepto(padre);
                        olvEstimacionConceptos.RefreshObject(padre);
                    }

                    olvEstimacionConceptos.RefreshObject(partida);
                    ActualizarTotales();
                    OnDatosChanged(this, EventArgs.Empty);

                    string mensaje = nuevoM2 > 0 
                        ? $"? Metros cuadrados capturados exitosamente\n\n" +
                          $"Partida: {partida.Nombre}\n" +
                          $"m�: {nuevoM2:F2}\n" +
                          $"Costo calculado: {partida.Total:C2}"
                        : $"? Metros cuadrados reseteados a 0\n\n" +
                          $"Partida: {partida.Nombre}\n" +
                          $"La partida no tendr� costo hasta que se capturen metros cuadrados.";

                    MessageBox.Show(
                        mensaje,
                        "Captura Exitosa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }
        
        /// <summary>
        /// Manejador para la opci�n de marcar/desmarcar
        /// </summary>
        private void MenuMarcar_Click(object sender, EventArgs e)
        {
            var nodoSeleccionado = olvEstimacionConceptos.SelectedObject as NodoConcepto;
            
            if (nodoSeleccionado == null || nodoSeleccionado.Completado)
            {
                return;
            }
            
            nodoSeleccionado.Incluir = !nodoSeleccionado.Incluir;
            
            if (!nodoSeleccionado.EsConcepto && nodoSeleccionado.EsDinamica && 
                nodoSeleccionado.Incluir && nodoSeleccionado.MetrosCuadrados == 0)
            {
                MostrarDialogoCapturarM2(nodoSeleccionado);
            }
            
            if (nodoSeleccionado.EsConcepto)
            {
                foreach (var partida in nodoSeleccionado.Partidas)
                {
                    if (!partida.Completado)
                    {
                        partida.Incluir = nodoSeleccionado.Incluir;
                    }
                }
            }
            else
            {
                var padre = EncontrarPadre(nodoSeleccionado);
                if (padre != null)
                {
                    bool todasMarcadas = padre.Partidas.All(p => p.Incluir || p.Completado);
                    bool ningunaMarcada = padre.Partidas.All(p => !p.Incluir || p.Completado);
                    
                    if (todasMarcadas)
                        padre.Incluir = true;
                    else if (ningunaMarcada)
                        padre.Incluir = false;
                }
            }
            
            olvEstimacionConceptos.RebuildAll(true);
            OnDatosChanged(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Manejador para la opci�n de ver detalles
        /// </summary>
        private void MenuDetalles_Click(object sender, EventArgs e)
        {
            var nodoSeleccionado = olvEstimacionConceptos.SelectedObject as NodoConcepto;
            
            if (nodoSeleccionado == null)
            {
                return;
            }
            
            string detalles;
            
            if (nodoSeleccionado.EsConcepto)
            {
                double totalPresupuestado = nodoSeleccionado.Partidas.Sum(p => p.Total);
                double totalEjecutado = nodoSeleccionado.Partidas.Sum(p => p.MontoEjecutado);
                double porcentaje = totalPresupuestado > 0 ? (totalEjecutado / totalPresupuestado) * 100 : 0;
                
                int partidasCompletadas = nodoSeleccionado.Partidas.Count(p => p.Completado);
                int totalPartidas = nodoSeleccionado.Partidas.Count;
                int partidasDinamicas = nodoSeleccionado.Partidas.Count(p => p.EsDinamica);
                
                detalles = $"?? DETALLES DEL CONCEPTO\n\n" +
                          $"C�digo: {nodoSeleccionado.Codigo}\n" +
                          $"Nombre: {nodoSeleccionado.Nombre}\n\n" +
                          $"?? AVANCE\n" +
                          $"Total Presupuestado: {totalPresupuestado:C2}\n" +
                          $"Total Ejecutado: {totalEjecutado:C2}\n" +
                          $"Avance: {porcentaje:F1}%\n\n" +
                          $"?? PARTIDAS\n" +
                          $"Completadas: {partidasCompletadas}/{totalPartidas}\n" +
                          $"Din�micas: {partidasDinamicas}";
            }
            else
            {
                double valorM2 = prototipoActual.ToUpper().Contains("TUNERA") ? 
                    nodoSeleccionado.ValorM2Tunera : nodoSeleccionado.ValorM2Calandra;
                
                detalles = $"?? DETALLES DE LA PARTIDA\n\n" +
                          $"WBS: {nodoSeleccionado.WBS}\n" +
                          $"Nombre: {nodoSeleccionado.Nombre}\n\n";
                
                if (nodoSeleccionado.EsDinamica)
                {
                    detalles += $"?? PARTIDA DIN�MICA\n" +
                               $"Prototipo: {prototipoActual}\n" +
                               $"Valor/m�: {valorM2:C2}\n" +
                               $"Metros cuadrados: {nodoSeleccionado.MetrosCuadrados:F2} m�\n" +
                               $"Costo calculado: {(nodoSeleccionado.MetrosCuadrados * valorM2):C2}\n\n";
                }
                else
                {
                    detalles += $"?? COSTO\n" +
                               $"Total Presupuestado: {nodoSeleccionado.Total:C2}\n\n";
                }
                
                detalles += $"?? AVANCE\n" +
                           $"Avance: {nodoSeleccionado.AvancePorcentaje:F1}%\n" +
                           $"Monto Ejecutado: {nodoSeleccionado.MontoEjecutado:C2}\n" +
                           $"Estado: {(nodoSeleccionado.Completado ? "TERMINADO" : "EN PROCESO")}";
                
                if (nodoSeleccionado.FechaFinalizacion.HasValue)
                {
                    detalles += $"\nFecha Finalizaci�n: {nodoSeleccionado.FechaFinalizacion.Value:dd/MM/yyyy}";
                }
            }
            
            MessageBox.Show(detalles, "Detalles", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Manejador para la opci�n de resetear progreso
        /// </summary>
        private void MenuResetear_Click(object sender, EventArgs e)
        {
            var nodoSeleccionado = olvEstimacionConceptos.SelectedObject as NodoConcepto;
            
            if (nodoSeleccionado == null)
            {
                return;
            }
            
            var result = MessageBox.Show(
                $"�Est�s seguro de que deseas resetear el progreso de {(nodoSeleccionado.EsConcepto ? "este concepto" : "esta partida")}?\n\n" +
                "Esto eliminar� todos los avances y metros cuadrados capturados.",
                "Confirmar Reset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            
            if (result != DialogResult.Yes)
            {
                return;
            }
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    string sql = @"
                        UPDATE AvanceManualObra
                        SET AvancePorcentaje = 0, MontoEjecutado = 0, MetrosCuadrados = 0, FechaFinalizacion = NULL
                        WHERE Manzana = @m AND Lote = @l AND WBS = @wbs";
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", cmbManzana.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@l", cmbLote.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@wbs", nodoSeleccionado.WBS);
                        
                        cmd.ExecuteNonQuery();
                    }
                }
                
                if (nodoSeleccionado.EsConcepto)
                {
                    foreach (var partida in nodoSeleccionado.Partidas)
                    {
                        partida.AvancePorcentaje = 0;
                        partida.MontoEjecutado = 0;
                        partida.MetrosCuadrados = 0;
                        partida.Completado = false;
                    }
                }
                else
                {
                    nodoSeleccionado.AvancePorcentaje = 0;
                    nodoSeleccionado.MontoEjecutado = 0;
                    nodoSeleccionado.MetrosCuadrados = 0;
                    nodoSeleccionado.Completado = false;
                }
                
                olvEstimacionConceptos.RefreshObject(nodoSeleccionado);
                ActualizarTotales();
                
                MessageBox.Show("El progreso ha sido reseteado exitosamente", "�xito", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al resetear progreso: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
