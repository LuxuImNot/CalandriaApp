using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// FormGestionarPartidas - Operaciones CRUD (Agregar, Editar, Eliminar)
    /// </summary>
    public partial class FormGestionarPartidas
    {
        #region Agregar Partida
        
        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (conceptoSeleccionado == null)
            {
                MessageBox.Show(
                    "Selecciona un concepto primero.",
                    "Atencion",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            using (var formPartida = new Form
            {
                Text = "Agregar Partida",
                Size = new Size(520, 700),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                // Crear controles del formulario
                var controles = CrearControlesFormularioPartida(null);
                formPartida.Controls.AddRange(controles.TodosLosControles);
                formPartida.AcceptButton = controles.BtnOk;
                formPartida.CancelButton = controles.BtnCancel;
                
                if (formPartida.ShowDialog() == DialogResult.OK)
                {
                    ProcesarAgregarPartida(controles);
                }
            }
        }
        
        private void ProcesarAgregarPartida(ControlesFormularioPartida controles)
        {
            string etapaTexto = controles.CmbEtapa.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(etapaTexto) || string.IsNullOrWhiteSpace(controles.TxtPartida.Text))
            {
                MessageBox.Show(
                    "La Etapa y Partida son obligatorias.",
                    "Validacion",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            // Validar que al menos un prototipo este seleccionado
            if (!controles.ChkTunera.Checked && !controles.ChkCalandra.Checked)
            {
                MessageBox.Show(
                    "Debe seleccionar al menos un prototipo aplicable.",
                    "Validacion",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            // Construir la cadena de prototipos aplicables
            var prototiposList = new List<string>();
            if (controles.ChkTunera.Checked) prototiposList.Add("TUNERA");
            if (controles.ChkCalandra.Checked) prototiposList.Add("CALANDRA");
            string prototiposAplicables = string.Join(",", prototiposList);
            
            // Obtener costos segun los prototipos seleccionados
            double costoTunera = 0;
            double costoCalandra = 0;
            double valorM2Tunera = 0;
            double valorM2Calandra = 0;
            double limiteM2Tunera = 0;
            double limiteM2Calandra = 0;
            
            if (controles.ChkEsDinamica.Checked)
            {
                // Partida dinamica
                if (controles.ChkTunera.Checked)
                {
                    valorM2Tunera = (double)controles.NumValorM2Tunera.Value;
                    limiteM2Tunera = (double)controles.NumLimiteM2Tunera.Value;
                }
                if (controles.ChkCalandra.Checked)
                {
                    valorM2Calandra = (double)controles.NumValorM2Calandra.Value;
                    limiteM2Calandra = (double)controles.NumLimiteM2Calandra.Value;
                }
            }
            else
            {
                // Partida con costo fijo
                if (controles.ChkTunera.Checked)
                    costoTunera = (double)controles.NumCostoTunera.Value;
                if (controles.ChkCalandra.Checked)
                    costoCalandra = (double)controles.NumCostoCalandra.Value;
            }
            
            var partida = new PartidaConcepto
            {
                Etapa = etapaTexto,
                Partida = controles.TxtPartida.Text.Trim(),
                EsDinamica = controles.ChkEsDinamica.Checked,
                CostoTunera = costoTunera,
                CostoCalandra = costoCalandra,
                ValorM2Tunera = valorM2Tunera,
                ValorM2Calandra = valorM2Calandra,
                LimiteM2Tunera = limiteM2Tunera,
                LimiteM2Calandra = limiteM2Calandra,
                LimiteM2 = 0, // Legacy, ya no se usa
                PrototiposAplicables = prototiposAplicables,
                WBS = 0 // Nueva partida
            };
            
            // Insertar segun la opcion seleccionada
            int opcionSeleccionada = controles.CmbPosicion.SelectedIndex;
            
            switch (opcionSeleccionada)
            {
                case 0: // Al final
                    partidasConcepto.Add(partida);
                    break;
                    
                case 1: // En orden alfabetico
                    partidasConcepto.Add(partida);
                    var partidasOrdenadas = partidasConcepto
                        .OrderBy(p => p.Etapa)
                        .ThenBy(p => p.Partida)
                        .ToList();
                
                    partidasConcepto.Clear();
                    foreach (var p in partidasOrdenadas)
                    {
                        partidasConcepto.Add(p);
                    }
                    break;
                    
                case 2: // Antes de la seleccionada
                    if (dgvPartidas.SelectedRows.Count > 0)
                    {
                        int indice = dgvPartidas.SelectedRows[0].Index;
                        partidasConcepto.Insert(indice, partida);
                    }
                    else
                    {
                        partidasConcepto.Add(partida);
                    }
                    break;
                    
                case 3: // Despues de la seleccionada
                    if (dgvPartidas.SelectedRows.Count > 0)
                    {
                        int indice = dgvPartidas.SelectedRows[0].Index;
                        partidasConcepto.Insert(indice + 1, partida);
                    }
                    else
                    {
                        partidasConcepto.Add(partida);
                    }
                    break;
            }
            
            ActualizarGrid();
            ActualizarContador();
            
            // Mensaje informativo
            MostrarMensajeExitoAgregar(partida, prototiposList, opcionSeleccionada);
        }
        
        private void MostrarMensajeExitoAgregar(PartidaConcepto partida, List<string> prototiposList, int opcionSeleccionada)
        {
            string mensajePrototipos = prototiposList.Count == 2 
                ? "Aplica a todos los prototipos"
                : $"\u26A1 PARTIDA EXCLUSIVA para: {string.Join(", ", prototiposList)}";
            
            string mensajeLimites = "";
            if (partida.EsDinamica)
            {
                var limites = new List<string>();
                if (partida.LimiteM2Tunera > 0)
                    limites.Add($"TUNERA: max {partida.LimiteM2Tunera:N2} m\u00B2");
                if (partida.LimiteM2Calandra > 0)
                    limites.Add($"CALANDRA: max {partida.LimiteM2Calandra:N2} m\u00B2");
                
                if (limites.Count > 0)
                    mensajeLimites = $"\n\n\u26A0 Limites configurados:\n   \u2022 {string.Join("\n   \u2022 ", limites)}";
            }
            
            string mensajeExito = partida.EsDinamica 
                ? $"\U0001F4D0 Partida dinamica agregada exitosamente.\n\n" +
                  $"El costo se calculara automaticamente segun los m\u00B2 del proyecto." +
                  mensajeLimites +
                  $"\n\n\U0001F3E0 {mensajePrototipos}"
                : $"\u2713 Partida agregada exitosamente.\n\n\U0001F3E0 {mensajePrototipos}";
            
            if (opcionSeleccionada == 1)
            {
                mensajeExito += "\n\nAl guardar, se reorganizara el WBS para mantener\n" +
                               "el orden. El progreso de las obras se preservara.";
            }
            
            MessageBox.Show(
                mensajeExito,
                "Informacion",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        
        #endregion
        
        #region Editar Partida
        
        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (dgvPartidas.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Selecciona una partida para editar.",
                    "Atencion",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            
            int indice = dgvPartidas.SelectedRows[0].Index;
            
            if (indice < 0 || indice >= partidasConcepto.Count)
                return;
            
            var partidaEditar = partidasConcepto[indice];
            
            using (var formEditar = new Form
            {
                Text = "Editar Partida",
                Size = new Size(520, 620),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var controles = CrearControlesFormularioPartida(partidaEditar);
                
                // Remover controles de posicion para edicion
                formEditar.Controls.AddRange(controles.ControlesSinPosicion);
                formEditar.AcceptButton = controles.BtnOk;
                formEditar.CancelButton = controles.BtnCancel;
                
                // Ajustar posicion de botones para edicion
                controles.BtnOk.Location = new Point(150, 530);
                controles.BtnCancel.Location = new Point(260, 530);
                
                if (formEditar.ShowDialog() == DialogResult.OK)
                {
                    ProcesarEditarPartida(partidaEditar, controles);
                }
            }
        }
        
        private void ProcesarEditarPartida(PartidaConcepto partidaEditar, ControlesFormularioPartida controles)
        {
            string etapaTexto = controles.CmbEtapa.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(etapaTexto) || string.IsNullOrWhiteSpace(controles.TxtPartida.Text))
            {
                MessageBox.Show(
                    "La Etapa y Partida son obligatorias.",
                    "Validacion",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            // Validar que al menos un prototipo este seleccionado
            if (!controles.ChkTunera.Checked && !controles.ChkCalandra.Checked)
            {
                MessageBox.Show(
                    "Debe seleccionar al menos un prototipo aplicable.",
                    "Validacion",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            // Construir la cadena de prototipos aplicables
            var prototiposList = new List<string>();
            if (controles.ChkTunera.Checked) prototiposList.Add("TUNERA");
            if (controles.ChkCalandra.Checked) prototiposList.Add("CALANDRA");
            string prototiposAplicables = string.Join(",", prototiposList);
            
            // Obtener costos segun los prototipos seleccionados
            double costoTunera = 0;
            double costoCalandra = 0;
            double valorM2Tunera = 0;
            double valorM2Calandra = 0;
            double limiteM2Tunera = 0;
            double limiteM2Calandra = 0;
            
            if (controles.ChkEsDinamica.Checked)
            {
                // Partida dinamica
                if (controles.ChkTunera.Checked)
                {
                    valorM2Tunera = (double)controles.NumValorM2Tunera.Value;
                    limiteM2Tunera = (double)controles.NumLimiteM2Tunera.Value;
                }
                if (controles.ChkCalandra.Checked)
                {
                    valorM2Calandra = (double)controles.NumValorM2Calandra.Value;
                    limiteM2Calandra = (double)controles.NumLimiteM2Calandra.Value;
                }
            }
            else
            {
                // Partida con costo fijo
                if (controles.ChkTunera.Checked)
                    costoTunera = (double)controles.NumCostoTunera.Value;
                if (controles.ChkCalandra.Checked)
                    costoCalandra = (double)controles.NumCostoCalandra.Value;
            }
            
            partidaEditar.Etapa = etapaTexto;
            partidaEditar.Partida = controles.TxtPartida.Text.Trim();
            partidaEditar.EsDinamica = controles.ChkEsDinamica.Checked;
            partidaEditar.CostoTunera = costoTunera;
            partidaEditar.CostoCalandra = costoCalandra;
            partidaEditar.ValorM2Tunera = valorM2Tunera;
            partidaEditar.ValorM2Calandra = valorM2Calandra;
            partidaEditar.LimiteM2Tunera = limiteM2Tunera;
            partidaEditar.LimiteM2Calandra = limiteM2Calandra;
            partidaEditar.PrototiposAplicables = prototiposAplicables;
            
            ActualizarGrid();
            
            string mensajePrototipos = prototiposList.Count == 2 
                ? "Aplica a todos los prototipos"
                : $"\u26A1 PARTIDA EXCLUSIVA para: {prototiposAplicables}";
            
            string mensajeLimites = "";
            if (partidaEditar.EsDinamica)
            {
                var limites = new List<string>();
                if (partidaEditar.LimiteM2Tunera > 0)
                    limites.Add($"TUNERA: max {partidaEditar.LimiteM2Tunera:N2} m\u00B2");
                if (partidaEditar.LimiteM2Calandra > 0)
                    limites.Add($"CALANDRA: max {partidaEditar.LimiteM2Calandra:N2} m\u00B2");
                
                if (limites.Count > 0)
                    mensajeLimites = $"\n\n\u26A0 Limites configurados:\n   \u2022 {string.Join("\n   \u2022 ", limites)}";
            }
            
            string mensaje = partidaEditar.EsDinamica
                ? $"\U0001F4D0 Partida dinamica actualizada exitosamente.\n\n" +
                  $"El costo se calculara automaticamente segun los m\u00B2 del proyecto." +
                  mensajeLimites +
                  $"\n\n\U0001F3E0 {mensajePrototipos}"
                : $"\u2713 Partida actualizada exitosamente.\n\n\U0001F3E0 {mensajePrototipos}";
            
            MessageBox.Show(
                mensaje,
                "Exito",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        
        #endregion
        
        #region Eliminar Partida
        
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvPartidas.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Selecciona una partida para eliminar.",
                    "Atencion",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            
            int indice = dgvPartidas.SelectedRows[0].Index;
            
            if (indice < 0 || indice >= partidasConcepto.Count)
                return;
            
            var partida = partidasConcepto[indice];
            
            var result = MessageBox.Show(
                $"\u00BFEstas seguro de eliminar esta partida?\n\n" +
                $"Etapa: {partida.Etapa}\n" +
                $"Partida: {partida.Partida}\n" +
                $"Prototipos: {partida.PrototiposDescripcion}",
                "Confirmar Eliminacion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                partidasConcepto.RemoveAt(indice);
                ActualizarGrid();
                ActualizarContador();
            }
        }
        
        #endregion
        
        #region Helpers para crear controles del formulario
        
        /// <summary>
        /// Clase auxiliar para agrupar los controles del formulario de partida
        /// </summary>
        private class ControlesFormularioPartida
        {
            public ComboBox CmbEtapa { get; set; }
            public TextBox TxtPartida { get; set; }
            public CheckBox ChkEsDinamica { get; set; }
            public NumericUpDown NumCostoTunera { get; set; }
            public NumericUpDown NumCostoCalandra { get; set; }
            public NumericUpDown NumValorM2Tunera { get; set; }
            public NumericUpDown NumValorM2Calandra { get; set; }
            public NumericUpDown NumLimiteM2Tunera { get; set; }
            public NumericUpDown NumLimiteM2Calandra { get; set; }
            public CheckBox ChkTunera { get; set; }
            public CheckBox ChkCalandra { get; set; }
            public ComboBox CmbPosicion { get; set; }
            public Button BtnOk { get; set; }
            public Button BtnCancel { get; set; }
            public Control[] TodosLosControles { get; set; }
            public Control[] ControlesSinPosicion { get; set; }
        }
        
        private ControlesFormularioPartida CrearControlesFormularioPartida(PartidaConcepto partidaExistente)
        {
            bool esEdicion = partidaExistente != null;
            bool esDinamica = esEdicion && partidaExistente.EsDinamica;
            
            // Determinar que prototipos estan marcados
            var prototiposActuales = esEdicion ? partidaExistente.PrototiposArray : new[] { "TUNERA", "CALANDRA" };
            bool tuneraMarcada = prototiposActuales.Contains("TUNERA");
            bool calandraMarcada = prototiposActuales.Contains("CALANDRA");
            
            var lblEtapa = new Label { Text = "Etapa:", Location = new Point(20, 20), AutoSize = true };
            
            var cmbEtapa = new ComboBox 
            { 
                Location = new Point(150, 17), 
                Width = 320,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems,
                Text = esEdicion ? partidaExistente.Etapa : ""
            };
            
            // Cargar etapas existentes
            var etapasUnicas = partidasConcepto
                .Select(p => p.Etapa)
                .Distinct()
                .OrderBy(etapa => etapa)
                .ToList();
            
            foreach (var etapa in etapasUnicas)
            {
                if (!string.IsNullOrWhiteSpace(etapa))
                    cmbEtapa.Items.Add(etapa);
            }
            
            var lblPartida = new Label { Text = "Partida:", Location = new Point(20, 55), AutoSize = true };
            var txtPartida = new TextBox 
            { 
                Location = new Point(150, 52), 
                Width = 320,
                Text = esEdicion ? partidaExistente.Partida : ""
            };
            
            // === SECCION DE PROTOTIPOS (MOVIDA ARRIBA) ===
            var lblPrototipos = new Label 
            { 
                Text = "Prototipos aplicables:", 
                Location = new Point(20, 90), 
                AutoSize = true,
                Font = new Font(Control.DefaultFont, FontStyle.Bold)
            };
            
            var grpPrototipos = new GroupBox
            {
                Location = new Point(150, 80),
                Size = new Size(320, 55),
                Text = "Selecciona los prototipos para esta partida"
            };
            
            var chkTunera = new CheckBox 
            { 
                Text = "\U0001F3E0 TUNERA", 
                Location = new Point(15, 22),
                AutoSize = true,
                Checked = tuneraMarcada,
                Font = new Font(Control.DefaultFont, FontStyle.Bold)
            };
            
            var chkCalandra = new CheckBox 
            { 
                Text = "\U0001F3E1 CALANDRA", 
                Location = new Point(160, 22),
                AutoSize = true,
                Checked = calandraMarcada,
                Font = new Font(Control.DefaultFont, FontStyle.Bold)
            };
            
            grpPrototipos.Controls.Add(chkTunera);
            grpPrototipos.Controls.Add(chkCalandra);
            
            // === CHECKBOX DINAMICA ===
            var chkEsDinamica = new CheckBox 
            { 
                Text = "\U0001F4D0 Partida Dinamica ($/m\u00B2)", 
                Location = new Point(150, 145),
                Width = 320,
                AutoSize = false,
                Checked = esDinamica
            };
            
            // === SECCION DE COSTOS FIJOS ===
            var lblSeccionFijos = new Label
            {
                Text = "\U0001F4B0 Costos Fijos:",
                Location = new Point(20, 175),
                AutoSize = true,
                Font = new Font(Control.DefaultFont, FontStyle.Italic),
                ForeColor = Color.DarkBlue,
                Visible = !esDinamica
            };
            
            var lblCostoTunera = new Label 
            { 
                Text = "Costo Tunera:", 
                Location = new Point(40, 200), 
                AutoSize = true,
                Enabled = !esDinamica && tuneraMarcada,
                Visible = !esDinamica
            };
            var numCostoTunera = new NumericUpDown 
            { 
                Location = new Point(170, 197), 
                Width = 150, 
                DecimalPlaces = 2, 
                Maximum = 9999999, 
                ThousandsSeparator = true,
                Value = esEdicion ? (decimal)partidaExistente.CostoTunera : 0,
                Enabled = !esDinamica && tuneraMarcada,
                Visible = !esDinamica
            };
            
            var lblCostoCalandra = new Label 
            { 
                Text = "Costo Calandra:", 
                Location = new Point(40, 235), 
                AutoSize = true,
                Enabled = !esDinamica && calandraMarcada,
                Visible = !esDinamica
            };
            var numCostoCalandra = new NumericUpDown 
            { 
                Location = new Point(170, 232), 
                Width = 150, 
                DecimalPlaces = 2, 
                Maximum = 9999999, 
                ThousandsSeparator = true,
                Value = esEdicion ? (decimal)partidaExistente.CostoCalandra : 0,
                Enabled = !esDinamica && calandraMarcada,
                Visible = !esDinamica
            };
            
            // === SECCION DE COSTOS DINAMICOS ===
            var lblSeccionDinamicos = new Label
            {
                Text = "\U0001F4D0 Valores por m\u00B2:",
                Location = new Point(20, 175),
                AutoSize = true,
                Font = new Font(Control.DefaultFont, FontStyle.Italic),
                ForeColor = Color.DarkGreen,
                Visible = esDinamica
            };
            
            // -- TUNERA --
            var lblValorM2Tunera = new Label 
            { 
                Text = "Valor/m\u00B2 Tunera:", 
                Location = new Point(40, 200), 
                AutoSize = true,
                Enabled = esDinamica && tuneraMarcada,
                Visible = esDinamica
            };
            var numValorM2Tunera = new NumericUpDown 
            { 
                Location = new Point(170, 197), 
                Width = 120, 
                DecimalPlaces = 2, 
                Maximum = 9999999, 
                ThousandsSeparator = true,
                Value = esEdicion ? (decimal)partidaExistente.ValorM2Tunera : 0,
                Enabled = esDinamica && tuneraMarcada,
                Visible = esDinamica
            };
            
            var lblLimiteM2Tunera = new Label 
            { 
                Text = "Limite m\u00B2:", 
                Location = new Point(300, 200), 
                AutoSize = true,
                Enabled = esDinamica && tuneraMarcada,
                Visible = esDinamica
            };
            var numLimiteM2Tunera = new NumericUpDown 
            { 
                Location = new Point(370, 197), 
                Width = 100, 
                DecimalPlaces = 2, 
                Maximum = 9999999, 
                ThousandsSeparator = true,
                Value = esEdicion ? (decimal)partidaExistente.LimiteEfectivoTunera : 0,
                Enabled = esDinamica && tuneraMarcada,
                Visible = esDinamica
            };
            
            // -- CALANDRA --
            var lblValorM2Calandra = new Label 
            { 
                Text = "Valor/m\u00B2 Calandra:", 
                Location = new Point(40, 235), 
                AutoSize = true,
                Enabled = esDinamica && calandraMarcada,
                Visible = esDinamica
            };
            var numValorM2Calandra = new NumericUpDown 
            { 
                Location = new Point(170, 232), 
                Width = 120, 
                DecimalPlaces = 2, 
                Maximum = 9999999, 
                ThousandsSeparator = true,
                Value = esEdicion ? (decimal)partidaExistente.ValorM2Calandra : 0,
                Enabled = esDinamica && calandraMarcada,
                Visible = esDinamica
            };
            
            var lblLimiteM2Calandra = new Label 
            { 
                Text = "Limite m\u00B2:", 
                Location = new Point(300, 235), 
                AutoSize = true,
                Enabled = esDinamica && calandraMarcada,
                Visible = esDinamica
            };
            var numLimiteM2Calandra = new NumericUpDown 
            { 
                Location = new Point(370, 232), 
                Width = 100, 
                DecimalPlaces = 2, 
                Maximum = 9999999, 
                ThousandsSeparator = true,
                Value = esEdicion ? (decimal)partidaExistente.LimiteEfectivoCalandra : 0,
                Enabled = esDinamica && calandraMarcada,
                Visible = esDinamica
            };
            
            // Nota sobre limites
            var lblNotaLimite = new Label
            {
                Text = "\U0001F4A1 Limite = 0 significa sin limite (usa todos los m\u00B2 del proyecto)",
                Location = new Point(40, 265),
                Size = new Size(430, 20),
                ForeColor = Color.Gray,
                Font = new Font(Control.DefaultFont.FontFamily, 8f, FontStyle.Italic),
                Visible = esDinamica
            };
            
            // === INFORMACION DE PARTIDA EXCLUSIVA ===
            var lblInfoExclusiva = new Label
            {
                Text = "",
                Location = new Point(20, 295),
                Size = new Size(460, 40),
                ForeColor = Color.DarkOrange,
                Font = new Font(Control.DefaultFont, FontStyle.Italic),
                Visible = false
            };
            
            // Funcion para actualizar la UI segun los prototipos seleccionados
            Action actualizarUI = () =>
            {
                bool tunera = chkTunera.Checked;
                bool calandra = chkCalandra.Checked;
                bool dinamica = chkEsDinamica.Checked;
                
                // Actualizar visibilidad de secciones
                lblSeccionFijos.Visible = !dinamica;
                lblCostoTunera.Visible = !dinamica;
                numCostoTunera.Visible = !dinamica;
                lblCostoCalandra.Visible = !dinamica;
                numCostoCalandra.Visible = !dinamica;
                
                lblSeccionDinamicos.Visible = dinamica;
                lblValorM2Tunera.Visible = dinamica;
                numValorM2Tunera.Visible = dinamica;
                lblLimiteM2Tunera.Visible = dinamica;
                numLimiteM2Tunera.Visible = dinamica;
                lblValorM2Calandra.Visible = dinamica;
                numValorM2Calandra.Visible = dinamica;
                lblLimiteM2Calandra.Visible = dinamica;
                numLimiteM2Calandra.Visible = dinamica;
                lblNotaLimite.Visible = dinamica;
                
                // Habilitar/deshabilitar segun prototipo seleccionado
                lblCostoTunera.Enabled = tunera && !dinamica;
                numCostoTunera.Enabled = tunera && !dinamica;
                lblCostoCalandra.Enabled = calandra && !dinamica;
                numCostoCalandra.Enabled = calandra && !dinamica;
                
                lblValorM2Tunera.Enabled = tunera && dinamica;
                numValorM2Tunera.Enabled = tunera && dinamica;
                lblLimiteM2Tunera.Enabled = tunera && dinamica;
                numLimiteM2Tunera.Enabled = tunera && dinamica;
                
                lblValorM2Calandra.Enabled = calandra && dinamica;
                numValorM2Calandra.Enabled = calandra && dinamica;
                lblLimiteM2Calandra.Enabled = calandra && dinamica;
                numLimiteM2Calandra.Enabled = calandra && dinamica;
                
                // Actualizar texto de los labels segun seleccion
                if (!tunera)
                {
                    lblCostoTunera.Text = "Costo Tunera: (no aplica)";
                    lblValorM2Tunera.Text = "Valor/m\u00B2 Tunera: (no aplica)";
                }
                else
                {
                    lblCostoTunera.Text = "Costo Tunera:";
                    lblValorM2Tunera.Text = "Valor/m\u00B2 Tunera:";
                }
                
                if (!calandra)
                {
                    lblCostoCalandra.Text = "Costo Calandra: (no aplica)";
                    lblValorM2Calandra.Text = "Valor/m\u00B2 Calandra: (no aplica)";
                }
                else
                {
                    lblCostoCalandra.Text = "Costo Calandra:";
                    lblValorM2Calandra.Text = "Valor/m\u00B2 Calandra:";
                }
                
                // Mostrar informacion de partida exclusiva
                if (tunera && !calandra)
                {
                    lblInfoExclusiva.Text = "\u26A1 Esta partida sera EXCLUSIVA para TUNERA.\n" +
                                           "No aparecera en presupuestos de CALANDRA.";
                    lblInfoExclusiva.Visible = true;
                }
                else if (!tunera && calandra)
                {
                    lblInfoExclusiva.Text = "\u26A1 Esta partida sera EXCLUSIVA para CALANDRA.\n" +
                                           "No aparecera en presupuestos de TUNERA.";
                    lblInfoExclusiva.Visible = true;
                }
                else
                {
                    lblInfoExclusiva.Visible = false;
                }
            };
            
            // Eventos para actualizar la UI
            chkTunera.CheckedChanged += (s, ev) => actualizarUI();
            chkCalandra.CheckedChanged += (s, ev) => actualizarUI();
            chkEsDinamica.CheckedChanged += (s, ev) => actualizarUI();
            
            // Ejecutar actualizacion inicial
            actualizarUI();
            
            // Tooltips
            var tooltip = new ToolTip();
            tooltip.SetToolTip(chkEsDinamica, 
                "\u2713 Marcado: Costo = Valor/m\u00B2 x m\u00B2 del proyecto\n" +
                "\u2717 Desmarcado: Costo fijo tradicional");
            tooltip.SetToolTip(numLimiteM2Tunera,
                "Limite maximo de m\u00B2 para TUNERA.\n" +
                "\u2022 0 = Sin limite (usa todos los m\u00B2 del proyecto)\n" +
                "\u2022 >0 = El costo se calcula con el MINIMO entre:\n" +
                "       m\u00B2 del proyecto y este limite.");
            tooltip.SetToolTip(numLimiteM2Calandra,
                "Limite maximo de m\u00B2 para CALANDRA.\n" +
                "\u2022 0 = Sin limite (usa todos los m\u00B2 del proyecto)\n" +
                "\u2022 >0 = El costo se calcula con el MINIMO entre:\n" +
                "       m\u00B2 del proyecto y este limite.");
            tooltip.SetToolTip(grpPrototipos,
                "\u26A1 PARTIDA EXCLUSIVA:\n" +
                "Si solo seleccionas UN prototipo, la partida\n" +
                "solo aparecera en presupuestos de ese tipo.\n\n" +
                "Util para partidas especificas de cada modelo.");
            
            // Selector de posicion (solo para agregar)
            var lblPosicion = new Label 
            { 
                Text = "Insertar:", 
                Location = new Point(20, 350), 
                AutoSize = true 
            };
            
            var cmbPosicion = new ComboBox 
            { 
                Location = new Point(150, 347), 
                Width = 320,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            
            cmbPosicion.Items.Add("Al final (sin reorganizar)");
            cmbPosicion.Items.Add("En orden alfabetico (reorganizar WBS)");
            
            if (dgvPartidas.SelectedRows.Count > 0)
            {
                int indiceSeleccionado = dgvPartidas.SelectedRows[0].Index;
                cmbPosicion.Items.Add($"Antes de la partida seleccionada (fila {indiceSeleccionado + 1})");
                cmbPosicion.Items.Add($"Despues de la partida seleccionada (fila {indiceSeleccionado + 1})");
            }
            
            cmbPosicion.SelectedIndex = 0;
            
            tooltip.SetToolTip(cmbPosicion, 
                "\u2022 Al final: Mas rapido, no reorganiza WBS\n" +
                "\u2022 En orden alfabetico: Reorganiza todo el concepto\n" +
                "\u2022 Antes/Despues: Inserta en posicion especifica");
            
            var btnOk = new Button 
            { 
                Text = esEdicion ? "\U0001F4BE Guardar" : "\u2713 Agregar", 
                DialogResult = DialogResult.OK, 
                Location = new Point(150, 610),
                Width = 100,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOk.FlatAppearance.BorderSize = 0;
            
            var btnCancel = new Button 
            { 
                Text = "\u2717 Cancelar", 
                DialogResult = DialogResult.Cancel, 
                Location = new Point(260, 610),
                Width = 100,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            
            var todosLosControles = new Control[] 
            { 
                lblEtapa, cmbEtapa,
                lblPartida, txtPartida,
                lblPrototipos, grpPrototipos,
                chkEsDinamica,
                lblSeccionFijos,
                lblCostoTunera, numCostoTunera, 
                lblCostoCalandra, numCostoCalandra,
                lblSeccionDinamicos,
                lblValorM2Tunera, numValorM2Tunera,
                lblLimiteM2Tunera, numLimiteM2Tunera,
                lblValorM2Calandra, numValorM2Calandra,
                lblLimiteM2Calandra, numLimiteM2Calandra,
                lblNotaLimite,
                lblInfoExclusiva,
                lblPosicion, cmbPosicion,
                btnOk, btnCancel 
            };
            
            var controlesSinPosicion = new Control[] 
            { 
                lblEtapa, cmbEtapa,
                lblPartida, txtPartida,
                lblPrototipos, grpPrototipos,
                chkEsDinamica,
                lblSeccionFijos,
                lblCostoTunera, numCostoTunera, 
                lblCostoCalandra, numCostoCalandra,
                lblSeccionDinamicos,
                lblValorM2Tunera, numValorM2Tunera,
                lblLimiteM2Tunera, numLimiteM2Tunera,
                lblValorM2Calandra, numValorM2Calandra,
                lblLimiteM2Calandra, numLimiteM2Calandra,
                lblNotaLimite,
                lblInfoExclusiva,
                btnOk, btnCancel 
            };
            
            return new ControlesFormularioPartida
            {
                CmbEtapa = cmbEtapa,
                TxtPartida = txtPartida,
                ChkEsDinamica = chkEsDinamica,
                NumCostoTunera = numCostoTunera,
                NumCostoCalandra = numCostoCalandra,
                NumValorM2Tunera = numValorM2Tunera,
                NumValorM2Calandra = numValorM2Calandra,
                NumLimiteM2Tunera = numLimiteM2Tunera,
                NumLimiteM2Calandra = numLimiteM2Calandra,
                ChkTunera = chkTunera,
                ChkCalandra = chkCalandra,
                CmbPosicion = cmbPosicion,
                BtnOk = btnOk,
                BtnCancel = btnCancel,
                TodosLosControles = todosLosControles,
                ControlesSinPosicion = controlesSinPosicion
            };
        }
        
        #endregion
    }
}
