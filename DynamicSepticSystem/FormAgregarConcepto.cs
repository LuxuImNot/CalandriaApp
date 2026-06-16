using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Formulario modal para agregar un nuevo concepto con selección visual de posición
    /// </summary>
    public partial class FormAgregarConcepto : Form
    {
        // Propiedades públicas para obtener los datos ingresados
        public string NombreConcepto { get; private set; }
        public int PosicionInsercion { get; private set; } // -1 = al final, >= 0 = antes del índice
        public List<PartidaConcepto> Partidas { get; private set; }
        
        private List<ConceptoExistente> conceptosExistentes;
        private string prototipo;
        private List<PartidaConcepto> partidasConceptoSeleccionado; // Partidas del concepto seleccionado (solo para referencia)
        
        /// <summary>
        /// Constructor del formulario
        /// </summary>
        /// <param name="conceptosActuales">Lista de conceptos existentes en el sistema</param>
        /// <param name="prototipoActual">Prototipo actual (CALANDRA o TUNERA)</param>
        public FormAgregarConcepto(List<ConceptoExistente> conceptosActuales, string prototipoActual)
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            conceptosExistentes = conceptosActuales ?? new List<ConceptoExistente>();
            prototipo = prototipoActual ?? "CALANDRA";
            Partidas = new List<PartidaConcepto>();
            partidasConceptoSeleccionado = new List<PartidaConcepto>();
            
            ConfigurarFormulario();
            CargarConceptosExistentes();
        }
        
        private void ConfigurarFormulario()
        {
            // Configuración de DataGridView
            dgvPartidas.AutoGenerateColumns = false;
            dgvPartidas.AllowUserToAddRows = false;
            dgvPartidas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPartidas.MultiSelect = false;
            dgvPartidas.RowHeadersVisible = false;
            dgvPartidas.BackgroundColor = Color.White;
            dgvPartidas.BorderStyle = BorderStyle.None;
            
            // Columnas
            dgvPartidas.Columns.Clear();
            dgvPartidas.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "Etapa", 
                HeaderText = "Etapa", 
                Width = 150 
            });
            dgvPartidas.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "Partida", 
                HeaderText = "Partida", 
                Width = 200 
            });
            dgvPartidas.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "CostoTunera", 
                HeaderText = "Costo Tunera", 
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
            });
            dgvPartidas.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "CostoCalandra", 
                HeaderText = "Costo Calandra", 
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
            });
            
            // Estilo alternado
            dgvPartidas.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
            
            // Configuración de ListBox
            lstPosicion.SelectionMode = SelectionMode.One;
            lstPosicion.SelectedIndexChanged += LstPosicion_SelectedIndexChanged;
        }
        
        private void CargarConceptosExistentes()
        {
            lstPosicion.Items.Clear();
            
            // Agregar conceptos existentes
            foreach (var concepto in conceptosExistentes.OrderBy(c => c.Codigo))
            {
                lstPosicion.Items.Add($"[{concepto.Codigo}] {concepto.Nombre}");
            }
            
            // Agregar opción "Al final"
            lstPosicion.Items.Add("\u23EC [Insertar al FINAL]");
            
            // Seleccionar "Al final" por defecto
            if (lstPosicion.Items.Count > 0)
            {
                lstPosicion.SelectedIndex = lstPosicion.Items.Count - 1;
            }
        }
        
        private void LstPosicion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstPosicion.SelectedIndex < 0)
                return;
                
            // Actualizar label indicador
            if (lstPosicion.SelectedIndex == lstPosicion.Items.Count - 1)
            {
                lblIndicadorPosicion.Text = "\u2713 Se insertará al FINAL de la lista";
                lblIndicadorPosicion.ForeColor = Color.FromArgb(46, 204, 113); // Verde
                
                // Limpiar partidas del concepto seleccionado
                partidasConceptoSeleccionado.Clear();
                
                // \u{1F527} FIX: Actualizar el grid para que NO muestre partidas del concepto anterior
                ActualizarGridPartidas();
            }
            else
            {
                string textoSeleccionado = lstPosicion.SelectedItem.ToString();
                lblIndicadorPosicion.Text = $"\u2B06 Se insertará ANTES de: {textoSeleccionado}";
                lblIndicadorPosicion.ForeColor = Color.FromArgb(41, 128, 185); // Azul
                
                // \u{1F527} FIX: Cargar Y MOSTRAR partidas del concepto seleccionado
                int codigoConcepto = conceptosExistentes[lstPosicion.SelectedIndex].Codigo;
                CargarPartidasConceptoSeleccionado(codigoConcepto);
            }
        }
        
        /// <summary>
        /// Carga las partidas del concepto seleccionado desde la BD y las muestra en el grid
        /// </summary>
        private void CargarPartidasConceptoSeleccionado(int codigoConcepto)
        {
            partidasConceptoSeleccionado.Clear();
            
            try
            {
                // Obtener connection string
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
                
                using (var conn = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // NO iniciar transacción aquí - solo leer datos
                    // Las transacciones se manejan solo cuando hay modificaciones                    
                    // Consultar partidas del concepto desde PresupuestoObra
                    string sql = @"
                        SELECT 
                            Etapa,
                            Partida,
                            ISNULL(CostoTunera, 0) AS CostoTunera,
                            ISNULL(CostoCalandra, 0) AS CostoCalandra
                        FROM PresupuestoObra
                        WHERE Codigo = @codigo
                        ORDER BY Etapa, Partida";
                    
                    using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                    {
                        // NO asignar transacción para una lectura - solo si estuviéramos modificando datos
                        cmd.Parameters.AddWithValue("@codigo", codigoConcepto.ToString());
                        cmd.CommandTimeout = 30; // Agregar timeout explícito
                        
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var partida = new PartidaConcepto
                                {
                                    Etapa = reader["Etapa"]?.ToString() ?? "",
                                    Partida = reader["Partida"]?.ToString() ?? "",
                                    CostoTunera = Convert.ToDouble(reader["CostoTunera"]),
                                    CostoCalandra = Convert.ToDouble(reader["CostoCalandra"])
                                };
                                
                                partidasConceptoSeleccionado.Add(partida);
                            }
                        }
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"\u2705 Cargadas {partidasConceptoSeleccionado.Count} partidas del concepto [{codigoConcepto}]");
                
                // \u{1F527} FIX CRÍTICO: Mostrar las partidas en el DataGridView
                MostrarPartidasConceptoEnGrid();
            }
            catch (System.Data.SqlClient.SqlException exSql)
            {
                System.Diagnostics.Debug.WriteLine($"\u274C Error SQL al cargar partidas del concepto: {exSql.Message}\nNumber: {exSql.Number}");
                MessageBox.Show(
                    $"Error al cargar partidas del concepto:\n\n{exSql.Message}",
                    "Error SQL", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"\u274C Error al cargar partidas del concepto: {ex.Message}");
                MessageBox.Show(
                    $"Error al cargar partidas del concepto:\n\n{ex.Message}",
                    "Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// \u{1F527} NUEVO: Muestra las partidas del concepto seleccionado en el DataGridView (solo lectura/referencia)
        /// </summary>
        private void MostrarPartidasConceptoEnGrid()
        {
            dgvPartidas.Rows.Clear();
            
            // Mostrar partidas del concepto seleccionado (en color diferente para indicar que son de referencia)
            foreach (var partida in partidasConceptoSeleccionado)
            {
                int rowIndex = dgvPartidas.Rows.Add(
                    partida.Etapa,
                    partida.Partida,
                    partida.CostoTunera,
                    partida.CostoCalandra
                );
                
                // \u{1F3A8} Marcar con color gris claro para indicar que son partidas de referencia (no editables)
                dgvPartidas.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
                dgvPartidas.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Gray;
            }
            
            // Agregar separador visual si hay partidas de referencia
            if (partidasConceptoSeleccionado.Count > 0)
            {
                int separadorIndex = dgvPartidas.Rows.Add("═══════════", "═══════════", 0, 0);
                dgvPartidas.Rows[separadorIndex].DefaultCellStyle.BackColor = Color.LightBlue;
                dgvPartidas.Rows[separadorIndex].DefaultCellStyle.Font = new Font(dgvPartidas.Font, FontStyle.Bold);
            }
            
            // Ahora agregar las partidas que el usuario está creando (Partidas)
            foreach (var partida in Partidas)
            {
                dgvPartidas.Rows.Add(
                    partida.Etapa,
                    partida.Partida,
                    partida.CostoTunera,
                    partida.CostoCalandra
                );
            }
            
            // Actualizar contador (solo cuenta las partidas nuevas del usuario)
            lblContadorPartidas.Text = $"Total: {Partidas.Count} partida(s) - Referencia: {partidasConceptoSeleccionado.Count}";
            
            System.Diagnostics.Debug.WriteLine($"\u1F4CA Grid actualizado: {dgvPartidas.Rows.Count} filas visibles ({partidasConceptoSeleccionado.Count} ref + {Partidas.Count} nuevas)");
        }
        
        private void btnAgregarPartida_Click(object sender, EventArgs e)
        {
            // Verificar si hay partidas del concepto seleccionado para mostrar selector de posición
            int posicionInsercionPartida = -1; // -1 = al final
            
            if (partidasConceptoSeleccionado.Count > 0)
            {
                // Mostrar diálogo de selección de posición
                posicionInsercionPartida = MostrarSelectorPosicionPartida();
                
                if (posicionInsercionPartida == -2) // Usuario canceló
                    return;
            }
            
            // Crear formulario simple para agregar partida
            using (var formPartida = new Form
            {
                Text = "Agregar Partida",
                Size = new Size(500, 250),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var lblEtapa = new Label { Text = "Etapa:", Location = new Point(20, 20), AutoSize = true };
                
                // ComboBox editable para Etapa
                var cmbEtapa = new ComboBox 
                { 
                    Location = new Point(150, 17), 
                    Width = 300,
                    DropDownStyle = ComboBoxStyle.DropDown,
                    AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                    AutoCompleteSource = AutoCompleteSource.ListItems
                };
                
                // Cargar etapas únicas del concepto seleccionado
                var etapasUnicas = partidasConceptoSeleccionado
                    .Select(p => p.Etapa)
                    .Distinct()
                    .OrderBy(etapa => etapa)
                    .ToList();
                
                // Agregar también etapas de las partidas ya agregadas
                var etapasAgregadas = Partidas
                    .Select(p => p.Etapa)
                    .Distinct()
                    .Where(etapa => !etapasUnicas.Contains(etapa))
                    .OrderBy(etapa => etapa);
                
                etapasUnicas.AddRange(etapasAgregadas);
                
                // Poblar el ComboBox
                foreach (var etapa in etapasUnicas)
                {
                    if (!string.IsNullOrWhiteSpace(etapa))
                        cmbEtapa.Items.Add(etapa);
                }
                
                var lblPartida = new Label { Text = "Partida:", Location = new Point(20, 55), AutoSize = true };
                var txtPartida = new TextBox { Location = new Point(150, 52), Width = 300 };
                
                var lblCostoTunera = new Label { Text = "Costo Tunera:", Location = new Point(20, 90), AutoSize = true };
                var numCostoTunera = new NumericUpDown { 
                    Location = new Point(150, 87), 
                    Width = 150, 
                    DecimalPlaces = 2, 
                    Maximum = 9999999, 
                    ThousandsSeparator = true 
                };
                
                var lblCostoCalandra = new Label { Text = "Costo Calandra:", Location = new Point(20, 125), AutoSize = true };
                var numCostoCalandra = new NumericUpDown { 
                    Location = new Point(150, 122), 
                    Width = 150, 
                    DecimalPlaces = 2, 
                    Maximum = 9999999, 
                    ThousandsSeparator = true 
                };
                
                var btnOk = new Button { 
                    Text = "\u2795 Agregar", 
                    DialogResult = DialogResult.OK, 
                    Location = new Point(150, 165),
                    Width = 100,
                    BackColor = Color.FromArgb(46, 204, 113),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnOk.FlatAppearance.BorderSize = 0;
                
                var btnCancel = new Button { 
                    Text = "\u274C Cancelar", 
                    DialogResult = DialogResult.Cancel, 
                    Location = new Point(260, 165),
                    Width = 100,
                    BackColor = Color.FromArgb(231, 76, 60),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnCancel.FlatAppearance.BorderSize = 0;
                
                formPartida.Controls.AddRange(new Control[] { 
                    lblEtapa, cmbEtapa,
                    lblPartida, txtPartida, 
                    lblCostoTunera, numCostoTunera, 
                    lblCostoCalandra, numCostoCalandra, 
                    btnOk, btnCancel 
                });
                
                formPartida.AcceptButton = btnOk;
                formPartida.CancelButton = btnCancel;
                
                if (formPartida.ShowDialog() == DialogResult.OK)
                {
                    string etapaTexto = cmbEtapa.Text.Trim();
                    
                    if (string.IsNullOrWhiteSpace(etapaTexto) || string.IsNullOrWhiteSpace(txtPartida.Text))
                    {
                        MessageBox.Show("La Etapa y Partida son obligatorias.", "Validación", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    // Crear la nueva partida
                    var partida = new PartidaConcepto
                    {
                        Etapa = etapaTexto,
                        Partida = txtPartida.Text.Trim(),
                        CostoTunera = (double)numCostoTunera.Value,
                        CostoCalandra = (double)numCostoCalandra.Value
                    };
                    
                    // Insertar en la posición seleccionada
                    if (posicionInsercionPartida >= 0 && posicionInsercionPartida <= Partidas.Count)
                    {
                        Partidas.Insert(posicionInsercionPartida, partida);
                    }
                    else
                    {
                        Partidas.Add(partida);
                    }
                    
                    // \u{1F527} FIX: Actualizar el grid para mostrar la nueva partida
                    if (partidasConceptoSeleccionado.Count > 0)
                    {
                        MostrarPartidasConceptoEnGrid(); // Usa el nuevo método que muestra ambas listas
                    }
                    else
                    {
                        ActualizarGridPartidas(); // Usa el método original si no hay concepto seleccionado
                    }
                }
            }
        }
        
        /// <summary>
        /// Muestra un diálogo para seleccionar dónde insertar la nueva partida
        /// </summary>
        private int MostrarSelectorPosicionPartida()
        {
            using (var formSelector = new Form
            {
                Text = "Seleccionar Posición de la Partida",
                Size = new Size(900, 550),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            })
            {
                // Título
                var lblTitulo = new Label
                {
                    Text = "¿Dónde deseas insertar la nueva partida?",
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    Location = new Point(20, 20),
                    Size = new Size(840, 30),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                
                // Instrucción
                var lblInstruccion = new Label
                {
                    Text = "Selecciona una partida existente del concepto o inserta al final:",
                    Font = new Font("Segoe UI", 9.5F),
                    Location = new Point(20, 55),
                    Size = new Size(840, 25),
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                
                var lstPartidas = new ListBox
                {
                    Location = new Point(20, 90),
                    Size = new Size(840, 340),
                    Font = new Font("Consolas", 9.5F),
                    HorizontalScrollbar = true
                };
                
                // Agregar partidas existentes del concepto seleccionado
                for (int i = 0; i < partidasConceptoSeleccionado.Count; i++)
                {
                    var p = partidasConceptoSeleccionado[i];
                    string texto = $"[{i + 1}] {p.Etapa} - {p.Partida}";
                    if (texto.Length > 90)
                        texto = texto.Substring(0, 87) + "...";
                    lstPartidas.Items.Add(texto);
                }
                
                // Agregar partidas ya agregadas por el usuario
                for (int i = 0; i < Partidas.Count; i++)
                {
                    var p = Partidas[i];
                    string texto = $"[NUEVA {i + 1}] {p.Etapa} - {p.Partida}";
                    if (texto.Length > 90)
                        texto = texto.Substring(0, 87) + "...";
                    lstPartidas.Items.Add(texto);
                }
                
                // Opción "Al final"
                lstPartidas.Items.Add("\u23EC [Insertar al FINAL]");
                lstPartidas.SelectedIndex = lstPartidas.Items.Count - 1;
                
                // Label indicador
                var lblIndicador = new Label
                {
                    Location = new Point(20, 440),
                    Size = new Size(840, 30),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 204, 113),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = "\u2713 Se insertará al FINAL de la lista"
                };
                
                // Evento de cambio de selección
                lstPartidas.SelectedIndexChanged += (sender2, eventArgs2) =>
                {
                    if (lstPartidas.SelectedIndex < 0)
                        return;
                    
                    if (lstPartidas.SelectedIndex == lstPartidas.Items.Count - 1)
                    {
                        lblIndicador.Text = "\u2713 Se insertará al FINAL de la lista";
                        lblIndicador.ForeColor = Color.FromArgb(46, 204, 113);
                    }
                    else
                    {
                        string textoSeleccionado = lstPartidas.SelectedItem.ToString();
                        lblIndicador.Text = $"\u2B06 Se insertará ANTES de: {textoSeleccionado}";
                        lblIndicador.ForeColor = Color.FromArgb(41, 128, 185);
                    }
                };
                
                // Botones
                var btnAceptar = new Button
                {
                    Text = "\u2713 Continuar",
                    DialogResult = DialogResult.OK,
                    Location = new Point(650, 480),
                    Size = new Size(130, 42),
                    BackColor = Color.FromArgb(46, 204, 113),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };
                btnAceptar.FlatAppearance.BorderSize = 0;
                
                var btnCancelar = new Button
                {
                    Text = "\u274C Cancelar",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(790, 480),
                    Size = new Size(90, 42),
                    BackColor = Color.FromArgb(231, 76, 60),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };
                btnCancelar.FlatAppearance.BorderSize = 0;
                
                formSelector.Controls.AddRange(new Control[] {
                    lblTitulo, lblInstruccion, lstPartidas, lblIndicador, btnAceptar, btnCancelar
                });
                
                formSelector.AcceptButton = btnAceptar;
                formSelector.CancelButton = btnCancelar;
                
                if (formSelector.ShowDialog() == DialogResult.OK)
                {
                    if (lstPartidas.SelectedIndex == lstPartidas.Items.Count - 1)
                    {
                        return -1; // Al final
                    }
                    else
                    {
                        return lstPartidas.SelectedIndex; // Antes de la seleccionada
                    }
                }
                
                return -2; // Cancelado
            }
        }
        
        private void btnEliminarPartida_Click(object sender, EventArgs e)
        {
            if (dgvPartidas.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona una partida para eliminar.", "Atención", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            int indice = dgvPartidas.SelectedRows[0].Index;
            
            // \u{1F527} FIX: Calcular el índice real en la lista Partidas (sin contar las de referencia)
            int indiceReal = indice;
            if (partidasConceptoSeleccionado.Count > 0)
            {
                // Si hay partidas de referencia, restar su cantidad + 1 (separador)
                indiceReal = indice - partidasConceptoSeleccionado.Count - 1;
            }
            
            if (indiceReal >= 0 && indiceReal < Partidas.Count)
            {
                Partidas.RemoveAt(indiceReal);
                
                // Actualizar grid
                if (partidasConceptoSeleccionado.Count > 0)
                {
                    MostrarPartidasConceptoEnGrid();
                }
                else
                {
                    ActualizarGridPartidas();
                }
            }
            else
            {
                MessageBox.Show("No puedes eliminar partidas de referencia del concepto existente.", 
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        /// <summary>
        /// Handler para editar una partida existente
        /// </summary>
        private void btnEditarPartida_Click(object sender, EventArgs e)
        {
            if (dgvPartidas.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona una partida para editar.", "Atención", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            int indice = dgvPartidas.SelectedRows[0].Index;
            
            // Determinar si es una partida de referencia o del usuario
            bool esPartidaReferencia = false;
            PartidaConcepto partidaEditar = null;
            int indiceReal = indice;
            
            if (partidasConceptoSeleccionado.Count > 0)
            {
                // Verificar si es partida de referencia
                if (indice < partidasConceptoSeleccionado.Count)
                {
                    esPartidaReferencia = true;
                    partidaEditar = partidasConceptoSeleccionado[indice];
                }
                else
                {
                    // Es una partida del usuario (restar referencia + separador)
                    indiceReal = indice - partidasConceptoSeleccionado.Count - 1;
                    if (indiceReal >= 0 && indiceReal < Partidas.Count)
                    {
                        partidaEditar = Partidas[indiceReal];
                    }
                }
            }
            else
            {
                // No hay partidas de referencia, todas son del usuario
                if (indice >= 0 && indice < Partidas.Count)
                {
                    partidaEditar = Partidas[indice];
                }
            }
            
            // Validar que se haya encontrado una partida válida
            if (partidaEditar == null)
            {
                MessageBox.Show("No se puede editar el separador visual.", 
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Crear formulario de edición
            using (var formEditar = new Form
            {
                Text = esPartidaReferencia ? "Editar Partida (Referencia)" : "Editar Partida",
                Size = new Size(500, 250),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var lblEtapa = new Label { Text = "Etapa:", Location = new Point(20, 20), AutoSize = true };
                
                // ComboBox editable para Etapa
                var cmbEtapa = new ComboBox 
                { 
                    Location = new Point(150, 17), 
                    Width = 300,
                    DropDownStyle = ComboBoxStyle.DropDown,
                    AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                    AutoCompleteSource = AutoCompleteSource.ListItems,
                    Text = partidaEditar.Etapa
                };
                
                // Cargar etapas únicas
                var etapasUnicas = partidasConceptoSeleccionado
                    .Select(p => p.Etapa)
                    .Distinct()
                    .OrderBy(etapa => etapa)
                    .ToList();
                
                var etapasAgregadas = Partidas
                    .Select(p => p.Etapa)
                    .Distinct()
                    .Where(etapa => !etapasUnicas.Contains(etapa))
                    .OrderBy(etapa => etapa);
                
                etapasUnicas.AddRange(etapasAgregadas);
                
                foreach (var etapa in etapasUnicas)
                {
                    if (!string.IsNullOrWhiteSpace(etapa))
                        cmbEtapa.Items.Add(etapa);
                }
                
                var lblPartida = new Label { Text = "Partida:", Location = new Point(20, 55), AutoSize = true };
                var txtPartida = new TextBox { 
                    Location = new Point(150, 52), 
                    Width = 300,
                    Text = partidaEditar.Partida
                };
                
                var lblCostoTunera = new Label { Text = "Costo Tunera:", Location = new Point(20, 90), AutoSize = true };
                var numCostoTunera = new NumericUpDown { 
                    Location = new Point(150, 87), 
                    Width = 150, 
                    DecimalPlaces = 2, 
                    Maximum = 9999999, 
                    ThousandsSeparator = true,
                    Value = (decimal)partidaEditar.CostoTunera
                };
                
                var lblCostoCalandra = new Label { Text = "Costo Calandra:", Location = new Point(20, 125), AutoSize = true };
                var numCostoCalandra = new NumericUpDown { 
                    Location = new Point(150, 122), 
                    Width = 150, 
                    DecimalPlaces = 2, 
                    Maximum = 9999999, 
                    ThousandsSeparator = true,
                    Value = (decimal)partidaEditar.CostoCalandra
                };
                
                var btnOk = new Button { 
                    Text = "\u2705 Guardar", 
                    DialogResult = DialogResult.OK, 
                    Location = new Point(150, 165),
                    Width = 100,
                    BackColor = Color.FromArgb(46, 204, 113),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnOk.FlatAppearance.BorderSize = 0;
                
                var btnCancel = new Button { 
                    Text = "\u274C Cancelar", 
                    DialogResult = DialogResult.Cancel, 
                    Location = new Point(260, 165),
                    Width = 100,
                    BackColor = Color.FromArgb(231, 76, 60),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnCancel.FlatAppearance.BorderSize = 0;
                
                formEditar.Controls.AddRange(new Control[] { 
                    lblEtapa, cmbEtapa,
                    lblPartida, txtPartida, 
                    lblCostoTunera, numCostoTunera, 
                    lblCostoCalandra, numCostoCalandra, 
                    btnOk, btnCancel 
                });
                
                formEditar.AcceptButton = btnOk;
                formEditar.CancelButton = btnCancel;
                
                if (formEditar.ShowDialog() == DialogResult.OK)
                {
                    string etapaTexto = cmbEtapa.Text.Trim();
                    
                    if (string.IsNullOrWhiteSpace(etapaTexto) || string.IsNullOrWhiteSpace(txtPartida.Text))
                    {
                        MessageBox.Show("La Etapa y Partida son obligatorias.", "Validación", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    // Actualizar la partida
                    partidaEditar.Etapa = etapaTexto;
                    partidaEditar.Partida = txtPartida.Text.Trim();
                    partidaEditar.CostoTunera = (double)numCostoTunera.Value;
                    partidaEditar.CostoCalandra = (double)numCostoCalandra.Value;
                    
                    // Si es partida de referencia, agregarla a Partidas si no está ya
                    if (esPartidaReferencia)
                    {
                        // La partida de referencia se está editando, crear una copia en Partidas
                        var nuevaPartida = new PartidaConcepto
                        {
                            Etapa = partidaEditar.Etapa,
                            Partida = partidaEditar.Partida,
                            CostoTunera = partidaEditar.CostoTunera,
                            CostoCalandra = partidaEditar.CostoCalandra
                        };
                        
                        // Verificar si ya existe en Partidas para no duplicar
                        bool yaExiste = Partidas.Any(p => 
                            p.Etapa == nuevaPartida.Etapa && 
                            p.Partida == nuevaPartida.Partida);
                        
                        if (!yaExiste)
                        {
                            Partidas.Add(nuevaPartida);
                        }
                    }
                    
                    // Actualizar el grid
                    if (partidasConceptoSeleccionado.Count > 0)
                    {
                        MostrarPartidasConceptoEnGrid();
                    }
                    else
                    {
                        ActualizarGridPartidas();
                    }
                    
                    string mensajeExito = esPartidaReferencia 
                        ? "Partida de referencia editada. Se ha creado una copia en tus partidas." 
                        : "Partida actualizada exitosamente.";
                    
                    MessageBox.Show(mensajeExito, "Éxito", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        
        private void ActualizarGridPartidas()
        {
            dgvPartidas.Rows.Clear();
            
            foreach (var partida in Partidas)
            {
                dgvPartidas.Rows.Add(
                    partida.Etapa,
                    partida.Partida,
                    partida.CostoTunera,
                    partida.CostoCalandra
                );
            }
            
            // Actualizar contador
            lblContadorPartidas.Text = $"Total: {Partidas.Count} partida(s)";
            
            System.Diagnostics.Debug.WriteLine($"\u1F4CA Grid actualizado: {dgvPartidas.Rows.Count} filas visibles");
        }
        
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            // Validar nombre del concepto
            if (string.IsNullOrWhiteSpace(txtNombreConcepto.Text))
            {
                MessageBox.Show("Ingresa el nombre del concepto.", "Validación", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombreConcepto.Focus();
                return;
            }
            
            // Validar que tenga al menos una partida
            if (Partidas.Count == 0)
            {
                MessageBox.Show("Agrega al menos una partida al concepto.", "Validación", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnAgregarPartida.Focus();
                return;
            }
            
            // Validar selección de posición
            if (lstPosicion.SelectedIndex < 0)
            {
                MessageBox.Show("Selecciona la posición donde se insertará el concepto.", "Validación", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                lstPosicion.Focus();
                return;
            }
            
            // Mostrar confirmación
            string mensaje = "";
            if (lstPosicion.SelectedIndex == lstPosicion.Items.Count - 1)
            {
                mensaje = $"Se agregará el concepto '{txtNombreConcepto.Text}' al FINAL de la lista.\n\n" +
                         $"Partidas: {Partidas.Count}\n\n" +
                         $"¿Continuar?";
            }
            else
            {
                string conceptoAntes = lstPosicion.SelectedItem.ToString();
                mensaje = $"Se agregará el concepto '{txtNombreConcepto.Text}' ANTES de:\n{conceptoAntes}\n\n" +
                         $"\u26A0 ADVERTENCIA: Los códigos de los conceptos posteriores se renumerarán automáticamente.\n\n" +
                         $"Partidas: {Partidas.Count}\n\n" +
                         $"¿Continuar?";
            }
            
            var result = MessageBox.Show(mensaje, "Confirmar", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result != DialogResult.Yes)
                return;
            
            NombreConcepto = txtNombreConcepto.Text.Trim();
            
            if (lstPosicion.SelectedIndex == lstPosicion.Items.Count - 1)
            {
                PosicionInsercion = -1; // Al final
            }
            else
            {
                PosicionInsercion = lstPosicion.SelectedIndex;
            }
            
            DialogResult = DialogResult.OK;
            Close();
        }
        
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
