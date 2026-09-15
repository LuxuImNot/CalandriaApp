using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Distribuye el importe total de una tarea de Mano de Obra entre
    /// los miembros de una cuadrilla seleccionada.
    /// </summary>
    public class FormAsignarNomina : Form
    {
        private readonly string _manzana;
        private readonly string _lote;
        private readonly string _ruta;
        private readonly int _nodoId;
        private readonly string _nombreTarea;
        private readonly decimal _totalDistribuir;

        private readonly BindingList<MiembroNomina> _miembros = new BindingList<MiembroNomina>();
        private bool _ajustandoMontos;

        private Label lblTitulo;
        private Label lblTarea;
        private Label lblTotal;
        private Label lblCuadrilla;
        private ComboBox cmbCuadrilla;
        private DataGridView dgvMiembros;
        private Label lblResumen;
        private Button btnDistribuirIgual;
        private Button btnLimpiar;
        private Button btnGuardar;
        private Button btnCerrar;

        public FormAsignarNomina(
            string manzana,
            string lote,
            string ruta,
            int nodoId,
            string nombreTarea,
            decimal totalDistribuir)
        {
            _manzana = manzana ?? "";
            _lote = lote ?? "";
            _ruta = ruta ?? "";
            _nodoId = nodoId;
            _nombreTarea = nombreTarea ?? "";
            _totalDistribuir = totalDistribuir;

            BuildUI();
            ThemeManager.AplicarTema(this);

            // La tabla NominaTareasAsignada la asegura el servidor (API) bajo demanda.
            CargarCuadrillas();

            this.Load += (s, e) => CargarAsignacionExistente();
        }

        #region Construcción de UI

        private void BuildUI()
        {
            this.Text = "Asignar Nómina";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ClientSize = new Size(720, 540);
            this.Font = new Font("Segoe UI", 9);
            this.BackColor = Color.White;

            lblTitulo = new Label
            {
                Text = "ASIGNACIÓN DE NÓMINA",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(15, 12),
                AutoSize = true
            };

            lblTarea = new Label
            {
                Text = $"Tarea: {_nombreTarea}",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(18, 48),
                Size = new Size(680, 20)
            };

            lblTotal = new Label
            {
                Text = "Total a distribuir: " + _totalDistribuir.ToString("C2", CultureInfo.CurrentCulture),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 174, 96),
                Location = new Point(18, 72),
                Size = new Size(680, 24)
            };

            lblCuadrilla = new Label
            {
                Text = "Cuadrilla:",
                Location = new Point(18, 110),
                Size = new Size(70, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            cmbCuadrilla = new ComboBox
            {
                Location = new Point(90, 107),
                Size = new Size(280, 24),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCuadrilla.SelectedIndexChanged += CmbCuadrilla_SelectedIndexChanged;

            dgvMiembros = new DataGridView
            {
                Location = new Point(18, 145),
                Size = new Size(684, 290),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EditMode = DataGridViewEditMode.EditOnEnter
            };
            dgvMiembros.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvMiembros.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvMiembros.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvMiembros.EnableHeadersVisualStyles = false;
            dgvMiembros.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);

            dgvMiembros.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Trabajador",
                DataPropertyName = "Nombre",
                Width = 240,
                ReadOnly = true
            });
            dgvMiembros.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Rol",
                DataPropertyName = "Rol",
                Width = 100,
                ReadOnly = true
            });
            dgvMiembros.Columns.Add(new DataGridViewCheckBoxColumn
            {
                HeaderText = "Jefe",
                DataPropertyName = "EsJefe",
                Width = 50,
                ReadOnly = true
            });
            dgvMiembros.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Teléfono",
                DataPropertyName = "Telefono",
                Width = 110,
                ReadOnly = true
            });
            var colMonto = new DataGridViewTextBoxColumn
            {
                HeaderText = "Monto",
                DataPropertyName = "Monto",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    BackColor = Color.FromArgb(255, 252, 220)
                }
            };
            dgvMiembros.Columns.Add(colMonto);

            dgvMiembros.DataSource = _miembros;
            dgvMiembros.CellValidating += DgvMiembros_CellValidating;
            dgvMiembros.CellEndEdit += (s, e) => ActualizarResumen();

            lblResumen = new Label
            {
                Location = new Point(18, 445),
                Size = new Size(684, 24),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            btnDistribuirIgual = new Button
            {
                Text = "Distribuir Igualmente",
                Location = new Point(18, 480),
                Size = new Size(160, 38),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnDistribuirIgual.Click += BtnDistribuirIgual_Click;

            btnLimpiar = new Button
            {
                Text = "Limpiar Montos",
                Location = new Point(186, 480),
                Size = new Size(130, 38),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnLimpiar.Click += (s, e) =>
            {
                foreach (var m in _miembros) m.Monto = 0m;
                RefrescarGrid();
                ActualizarResumen();
            };

            btnGuardar = new Button
            {
                Text = "Guardar Nómina",
                Location = new Point(440, 480),
                Size = new Size(140, 38),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnGuardar.Click += BtnGuardar_Click;

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Location = new Point(588, 480),
                Size = new Size(114, 38),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnCerrar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                lblTitulo, lblTarea, lblTotal,
                lblCuadrilla, cmbCuadrilla,
                dgvMiembros, lblResumen,
                btnDistribuirIgual, btnLimpiar,
                btnGuardar, btnCerrar
            });
        }

        #endregion

        #region Cuadrillas

        private void CargarCuadrillas()
        {
            cmbCuadrilla.Items.Clear();
            try
            {
                // Migrado a API: GET /api/nomina/cuadrillas
                var cuadrillas = ApiClient.Get<List<string>>("/api/nomina/cuadrillas");
                foreach (var c in cuadrillas)
                    cmbCuadrilla.Items.Add(c);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar las cuadrillas: " + ex.Message,
                    "Cuadrillas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CmbCuadrilla_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCuadrilla.SelectedItem == null) return;
            CargarMiembros(cmbCuadrilla.SelectedItem.ToString(), conservarMontos: false);
            ActualizarResumen();
        }

        private void CargarMiembros(string codigoCuadrilla, bool conservarMontos)
        {
            var montosPrevios = conservarMontos
                ? _miembros.ToDictionary(m => m.Clave(), m => m.Monto)
                : null;

            _miembros.Clear();

            try
            {
                // Migrado a API: GET /api/nomina/cuadrillas/{codigo}/miembros
                var miembros = ApiClient.Get<List<MiembroNomina>>(
                    "/api/nomina/cuadrillas/" + Uri.EscapeDataString(codigoCuadrilla) + "/miembros");

                foreach (var m in miembros)
                {
                    m.Monto = 0m;
                    if (montosPrevios != null && montosPrevios.TryGetValue(m.Clave(), out decimal monto))
                        m.Monto = monto;

                    _miembros.Add(m);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando miembros de la cuadrilla: " + ex.Message,
                    "Cuadrilla", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            RefrescarGrid();
        }

        #endregion

        #region Distribución y validación

        private void BtnDistribuirIgual_Click(object sender, EventArgs e)
        {
            if (_miembros.Count == 0)
            {
                MessageBox.Show("Selecciona una cuadrilla con miembros primero.",
                    "Distribuir", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal porPersona = Math.Round(_totalDistribuir / _miembros.Count, 2);
            decimal asignado = porPersona * _miembros.Count;
            decimal diferencia = _totalDistribuir - asignado;

            _ajustandoMontos = true;
            for (int i = 0; i < _miembros.Count; i++)
                _miembros[i].Monto = porPersona;

            // Compensar el centavo residual en el primer miembro (idealmente el jefe).
            if (diferencia != 0m && _miembros.Count > 0)
            {
                int idxAjuste = 0;
                for (int i = 0; i < _miembros.Count; i++)
                {
                    if (_miembros[i].EsJefe) { idxAjuste = i; break; }
                }
                _miembros[idxAjuste].Monto += diferencia;
            }
            _ajustandoMontos = false;

            RefrescarGrid();
            ActualizarResumen();
        }

        private void DgvMiembros_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (_ajustandoMontos) return;
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var col = dgvMiembros.Columns[e.ColumnIndex];
            if (col.DataPropertyName != "Monto") return;

            string txt = (e.FormattedValue ?? "").ToString().Trim();
            if (string.IsNullOrEmpty(txt))
            {
                dgvMiembros.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0m;
                return;
            }

            decimal valor;
            if (!decimal.TryParse(txt, NumberStyles.Number | NumberStyles.AllowCurrencySymbol,
                    CultureInfo.CurrentCulture, out valor) &&
                !decimal.TryParse(txt, NumberStyles.Number | NumberStyles.AllowCurrencySymbol,
                    CultureInfo.InvariantCulture, out valor))
            {
                MessageBox.Show("El monto debe ser un número válido.",
                    "Monto inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }

            if (valor < 0)
            {
                MessageBox.Show("El monto no puede ser negativo.",
                    "Monto inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }
        }

        private void ActualizarResumen()
        {
            decimal asignado = _miembros.Sum(m => m.Monto);
            decimal diferencia = _totalDistribuir - asignado;

            string txt = $"Asignado: {asignado.ToString("C2", CultureInfo.CurrentCulture)}   |   " +
                         $"Total: {_totalDistribuir.ToString("C2", CultureInfo.CurrentCulture)}   |   " +
                         $"Diferencia: {diferencia.ToString("C2", CultureInfo.CurrentCulture)}";

            lblResumen.Text = txt;

            if (Math.Abs(diferencia) < 0.01m)
                lblResumen.ForeColor = Color.FromArgb(39, 174, 96);
            else if (asignado > _totalDistribuir)
                lblResumen.ForeColor = Color.FromArgb(192, 57, 43);
            else
                lblResumen.ForeColor = Color.FromArgb(211, 84, 0);
        }

        private void RefrescarGrid()
        {
            dgvMiembros.DataSource = null;
            dgvMiembros.DataSource = _miembros;
        }

        #endregion

        #region Persistencia

        private void CargarAsignacionExistente()
        {
            try
            {
                // Migrado a API: GET /api/nomina/asignacion?manzana=&lote=&ruta=&nodoId=
                var asignacion = ApiClient.Get<AsignacionNominaApi>(
                    "/api/nomina/asignacion"
                    + "?manzana=" + Uri.EscapeDataString(_manzana)
                    + "&lote=" + Uri.EscapeDataString(_lote)
                    + "&ruta=" + Uri.EscapeDataString(_ruta)
                    + "&nodoId=" + _nodoId);

                string codigoExistente = asignacion?.CodigoCuadrilla;
                var montos = new Dictionary<string, decimal>();
                if (asignacion?.Montos != null)
                {
                    foreach (var mt in asignacion.Montos)
                    {
                        string clave = MiembroNomina.ClaveDe(mt.IdTrabajador, mt.NombreTrabajador);
                        montos[clave] = mt.Monto;
                    }
                }

                if (!string.IsNullOrEmpty(codigoExistente))
                {
                    int idx = cmbCuadrilla.Items.IndexOf(codigoExistente);
                    if (idx < 0)
                    {
                        cmbCuadrilla.Items.Add(codigoExistente);
                        idx = cmbCuadrilla.Items.IndexOf(codigoExistente);
                    }

                    cmbCuadrilla.SelectedIndexChanged -= CmbCuadrilla_SelectedIndexChanged;
                    cmbCuadrilla.SelectedIndex = idx;
                    cmbCuadrilla.SelectedIndexChanged += CmbCuadrilla_SelectedIndexChanged;

                    CargarMiembros(codigoExistente, conservarMontos: false);

                    foreach (var m in _miembros)
                    {
                        if (montos.TryGetValue(m.Clave(), out decimal monto))
                            m.Monto = monto;
                    }
                    RefrescarGrid();
                    ActualizarResumen();
                }
            }
            catch
            {
                // Sin asignación previa: estado inicial vacío.
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (cmbCuadrilla.SelectedItem == null)
            {
                MessageBox.Show("Selecciona una cuadrilla.", "Nómina",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_miembros.Count == 0)
            {
                MessageBox.Show("La cuadrilla seleccionada no tiene miembros.", "Nómina",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal asignado = _miembros.Sum(m => m.Monto);
            decimal diferencia = _totalDistribuir - asignado;

            if (Math.Abs(diferencia) >= 0.01m)
            {
                var rsp = MessageBox.Show(
                    $"El total asignado ({asignado.ToString("C2", CultureInfo.CurrentCulture)}) " +
                    $"no coincide con el total de la tarea ({_totalDistribuir.ToString("C2", CultureInfo.CurrentCulture)}).\n\n" +
                    "¿Deseas guardar de todas formas?",
                    "Nómina", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (rsp != DialogResult.Yes) return;
            }

            string codigoCuadrilla = cmbCuadrilla.SelectedItem.ToString();

            try
            {
                // Migrado a API: POST /api/nomina/asignacion (DELETE + INSERT en el servidor)
                var request = new GuardarAsignacionRequestApi
                {
                    Manzana = _manzana,
                    Lote = _lote,
                    Ruta = _ruta,
                    NodoId = _nodoId,
                    NombreTarea = _nombreTarea,
                    CodigoCuadrilla = codigoCuadrilla,
                    TotalDistribuir = _totalDistribuir
                };
                foreach (var m in _miembros)
                {
                    request.Lineas.Add(new LineaAsignacionNominaApi
                    {
                        IdTrabajador = m.IdTrabajador,
                        Nombre = m.Nombre ?? "",
                        Rol = m.Rol ?? "",
                        EsJefe = m.EsJefe,
                        Monto = m.Monto
                    });
                }

                ApiClient.Post("/api/nomina/asignacion", request);

                MessageBox.Show("Nómina guardada correctamente.", "Nómina",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                GenerarPdfNomina(codigoCuadrilla);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error guardando la nómina: " + ex.Message, "Nómina",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerarPdfNomina(string codigoCuadrilla)
        {
            try
            {
                string nombreArchivo =
                    $"Nomina_M{_manzana}-L{_lote}_Nodo{_nodoId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string archivoTemp = Path.Combine(Path.GetTempPath(), nombreArchivo);

                CrearPdfNomina(archivoTemp, codigoCuadrilla);

                string sugerido = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    nombreArchivo);

                using (var preview = new FormPdfPreview(archivoTemp, sugerido))
                {
                    preview.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "La nómina se guardó pero no fue posible generar el PDF:\n" + ex.Message,
                    "PDF Nómina", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CrearPdfNomina(string archivo, string codigoCuadrilla)
        {
            var doc = new PdfDocument();
            doc.Info.Title = "Asignación de Nómina";
            doc.Info.Author = "Pilaris";

            var page = doc.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            var gfx = XGraphics.FromPdfPage(page);

            var fuenteTitulo = new XFont("Arial", 16, XFontStyle.Bold);
            var fuenteSubtitulo = new XFont("Arial", 11, XFontStyle.Bold);
            var fuenteSeccion = new XFont("Arial", 10, XFontStyle.Bold);
            var fuenteNormal = new XFont("Arial", 9, XFontStyle.Regular);
            var fuenteEncabezado = new XFont("Arial", 9, XFontStyle.Bold);
            var fuentePequena = new XFont("Arial", 8, XFontStyle.Regular);

            var colorPrincipal = XColor.FromArgb(13, 71, 161);
            var colorSecundario = XColor.FromArgb(33, 150, 243);
            var colorBorde = XColor.FromArgb(189, 189, 189);
            var colorTexto = XColor.FromArgb(33, 33, 33);
            var colorGrisClaro = XColor.FromArgb(245, 245, 245);
            var colorVerde = XColor.FromArgb(39, 174, 96);
            var colorRojo = XColor.FromArgb(192, 57, 43);

            var brochaPrincipal = new XSolidBrush(colorPrincipal);
            var brochaSecundario = new XSolidBrush(colorSecundario);
            var brochaTexto = new XSolidBrush(colorTexto);
            var brochaBlanca = XBrushes.White;
            var brochaGrisClaro = new XSolidBrush(colorGrisClaro);
            var brochaVerde = new XSolidBrush(colorVerde);
            var brochaRojo = new XSolidBrush(colorRojo);

            var penBorde = new XPen(colorBorde, 0.5);
            var penPrincipal = new XPen(colorPrincipal, 1.5);

            double margen = 30;
            double ancho = page.Width - 2 * margen;
            double y = margen;

            // === ENCABEZADO ===
            gfx.DrawRectangle(brochaPrincipal, margen, y, ancho, 50);

            try
            {
                using (var ms = new MemoryStream())
                {
                    DynamicSepticSystem.Properties.Resources.Logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    var logo = XImage.FromStream(ms);
                    gfx.DrawImage(logo, margen + 6, y + 6, 38, 38);
                }
            }
            catch { }

            gfx.DrawString("CALANDRIA RESIDENCIAL", fuenteSubtitulo, brochaBlanca,
                new XRect(margen + 50, y + 6, ancho - 50, 14), XStringFormats.TopLeft);
            gfx.DrawString("ASIGNACIÓN DE NÓMINA", fuenteTitulo, brochaBlanca,
                new XRect(margen + 50, y + 22, ancho - 50, 18), XStringFormats.TopLeft);

            y += 60;

            // === INFO CASA ===
            gfx.DrawRectangle(brochaGrisClaro, margen, y, ancho, 38);
            gfx.DrawRectangle(penBorde, margen, y, ancho, 38);

            double colInfo = ancho / 4;
            string[] etiquetas = { "MANZANA", "LOTE", "NODO ID", "FECHA" };
            string[] valores = {
                $"M{_manzana}",
                $"L{_lote}",
                _nodoId.ToString(),
                DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.CreateSpecificCulture("es-MX"))
            };

            for (int i = 0; i < 4; i++)
            {
                double xCol = margen + i * colInfo;
                gfx.DrawString(etiquetas[i], fuenteEncabezado, brochaSecundario,
                    new XRect(xCol + 6, y + 4, colInfo - 12, 10), XStringFormats.TopLeft);
                gfx.DrawString(valores[i], fuenteSubtitulo, brochaPrincipal,
                    new XRect(xCol + 6, y + 16, colInfo - 12, 16), XStringFormats.TopLeft);
                if (i > 0)
                    gfx.DrawLine(penBorde, xCol, y + 4, xCol, y + 34);
            }

            y += 46;

            // === DATOS DE LA TAREA Y CUADRILLA ===
            gfx.DrawRectangle(brochaSecundario, margen, y, ancho, 16);
            gfx.DrawString("INFORMACIÓN DE LA TAREA", fuenteSeccion, brochaBlanca,
                new XRect(margen + 4, y + 2, ancho - 8, 13), XStringFormats.TopLeft);
            y += 16;

            double altoDatos = 50;
            gfx.DrawRectangle(brochaGrisClaro, margen, y, ancho, altoDatos);
            gfx.DrawRectangle(penBorde, margen, y, ancho, altoDatos);

            gfx.DrawString("Tarea: " + TruncarN(_nombreTarea, 95), fuenteNormal, brochaTexto,
                new XRect(margen + 6, y + 6, ancho - 12, 12), XStringFormats.TopLeft);
            gfx.DrawString("Ruta: " + (_ruta ?? "-"), fuenteNormal, brochaTexto,
                new XRect(margen + 6, y + 20, ancho - 12, 12), XStringFormats.TopLeft);
            gfx.DrawString("Cuadrilla asignada: " + (codigoCuadrilla ?? "-"), fuenteNormal, brochaTexto,
                new XRect(margen + 6, y + 34, ancho - 12, 12), XStringFormats.TopLeft);

            y += altoDatos + 8;

            // === TOTAL A DISTRIBUIR ===
            gfx.DrawRectangle(brochaVerde, margen, y, ancho, 22);
            gfx.DrawString(
                "TOTAL A DISTRIBUIR: " + _totalDistribuir.ToString("C2", CultureInfo.CurrentCulture),
                fuenteSubtitulo, brochaBlanca,
                new XRect(margen + 6, y + 4, ancho - 12, 16), XStringFormats.TopLeft);

            y += 30;

            // === TABLA DE TRABAJADORES ===
            gfx.DrawRectangle(brochaSecundario, margen, y, ancho, 16);
            gfx.DrawString("DISTRIBUCIÓN POR TRABAJADOR", fuenteSeccion, brochaBlanca,
                new XRect(margen + 4, y + 2, ancho - 8, 13), XStringFormats.TopLeft);
            y += 16;

            double colTrabajador = ancho * 0.45;
            double colRol = ancho * 0.18;
            double colTel = ancho * 0.17;
            double colMonto = ancho - colTrabajador - colRol - colTel;

            gfx.DrawRectangle(brochaPrincipal, margen, y, ancho, 14);
            gfx.DrawString("Trabajador", fuenteEncabezado, brochaBlanca,
                new XRect(margen + 4, y + 2, colTrabajador - 4, 10), XStringFormats.TopLeft);
            gfx.DrawString("Rol", fuenteEncabezado, brochaBlanca,
                new XRect(margen + colTrabajador + 4, y + 2, colRol - 4, 10), XStringFormats.TopLeft);
            gfx.DrawString("Teléfono", fuenteEncabezado, brochaBlanca,
                new XRect(margen + colTrabajador + colRol + 4, y + 2, colTel - 4, 10), XStringFormats.TopLeft);
            gfx.DrawString("Monto", fuenteEncabezado, brochaBlanca,
                new XRect(margen + colTrabajador + colRol + colTel + 4, y + 2, colMonto - 8, 10),
                XStringFormats.TopRight);

            y += 14;

            decimal asignadoTotal = 0m;
            bool altern = false;

            foreach (var m in _miembros)
            {
                XSolidBrush brochaFila = m.EsJefe
                    ? new XSolidBrush(XColor.FromArgb(255, 243, 224))
                    : (altern ? brochaGrisClaro : new XSolidBrush(XColor.FromArgb(255, 255, 255)));

                gfx.DrawRectangle(brochaFila, margen, y, ancho, 14);
                gfx.DrawRectangle(penBorde, margen, y, ancho, 14);

                string prefijo = m.EsJefe ? "[JEFE] " : "";
                gfx.DrawString(TruncarN(prefijo + (m.Nombre ?? "-"), 55), fuenteNormal, brochaTexto,
                    new XRect(margen + 4, y + 2, colTrabajador - 4, 10), XStringFormats.TopLeft);
                gfx.DrawString(TruncarN(m.Rol ?? "-", 18), fuenteNormal, brochaTexto,
                    new XRect(margen + colTrabajador + 4, y + 2, colRol - 4, 10), XStringFormats.TopLeft);
                gfx.DrawString(TruncarN(m.Telefono ?? "-", 18), fuenteNormal, brochaTexto,
                    new XRect(margen + colTrabajador + colRol + 4, y + 2, colTel - 4, 10), XStringFormats.TopLeft);
                gfx.DrawString(m.Monto.ToString("C2", CultureInfo.CurrentCulture),
                    fuenteNormal, brochaTexto,
                    new XRect(margen + colTrabajador + colRol + colTel + 4, y + 2, colMonto - 8, 10),
                    XStringFormats.TopRight);

                y += 14;
                asignadoTotal += m.Monto;
                altern = !altern;
            }

            // Fila de totales
            gfx.DrawRectangle(brochaPrincipal, margen, y, ancho, 16);
            gfx.DrawString("TOTAL ASIGNADO", fuenteEncabezado, brochaBlanca,
                new XRect(margen + 4, y + 3, ancho - colMonto - 8, 12), XStringFormats.TopLeft);
            gfx.DrawString(asignadoTotal.ToString("C2", CultureInfo.CurrentCulture),
                fuenteEncabezado, brochaBlanca,
                new XRect(margen + colTrabajador + colRol + colTel + 4, y + 3, colMonto - 8, 12),
                XStringFormats.TopRight);

            y += 22;

            decimal diferencia = _totalDistribuir - asignadoTotal;
            if (Math.Abs(diferencia) >= 0.01m)
            {
                gfx.DrawString(
                    "Diferencia respecto al total: " + diferencia.ToString("C2", CultureInfo.CurrentCulture),
                    fuenteNormal, brochaRojo,
                    new XRect(margen, y, ancho, 12), XStringFormats.TopLeft);
                y += 14;
            }

            // === FIRMAS ===
            y += 30;
            double anchoFirma = (ancho / 3) - 6;
            double altoFirma = 50;
            double xFirma1 = margen;
            double xFirma2 = margen + anchoFirma + 9;
            double xFirma3 = margen + (anchoFirma + 9) * 2;
            double yLinea = y + altoFirma - 14;

            gfx.DrawLine(penBorde, xFirma1, yLinea, xFirma1 + anchoFirma, yLinea);
            gfx.DrawString("Jefe de Cuadrilla", fuentePequena, brochaTexto,
                new XRect(xFirma1, yLinea + 4, anchoFirma, 10), XStringFormats.TopCenter);

            gfx.DrawLine(penBorde, xFirma2, yLinea, xFirma2 + anchoFirma, yLinea);
            gfx.DrawString("Supervisor de Obra", fuentePequena, brochaTexto,
                new XRect(xFirma2, yLinea + 4, anchoFirma, 10), XStringFormats.TopCenter);

            gfx.DrawLine(penBorde, xFirma3, yLinea, xFirma3 + anchoFirma, yLinea);
            gfx.DrawString("Administración", fuentePequena, brochaTexto,
                new XRect(xFirma3, yLinea + 4, anchoFirma, 10), XStringFormats.TopCenter);

            // === PIE ===
            double yPie = page.Height - margen - 10;
            gfx.DrawLine(penPrincipal, margen, yPie, margen + ancho, yPie);
            yPie += 2;
            string usuario = Global.UsuarioActual?.Nombre ?? Environment.UserName;
            string fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm",
                CultureInfo.CreateSpecificCulture("es-MX"));
            gfx.DrawString($"Generado por: {usuario}  ·  {fecha}", fuentePequena, brochaTexto,
                new XRect(margen, yPie, ancho / 2, 10), XStringFormats.TopLeft);
            gfx.DrawString("© Pilaris", fuentePequena, brochaTexto,
                new XRect(margen + ancho / 2, yPie, ancho / 2, 10), XStringFormats.TopRight);

            doc.Save(archivo);
        }

        private static string TruncarN(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
        }

        #endregion

        #region Modelo

        public class MiembroNomina
        {
            public int? IdTrabajador { get; set; }
            public string Nombre { get; set; }
            public string Rol { get; set; }
            public bool EsJefe { get; set; }
            public string Telefono { get; set; }
            public decimal Monto { get; set; }

            public string Clave() => ClaveDe(IdTrabajador, Nombre);

            public static string ClaveDe(int? idTrabajador, string nombre)
            {
                return idTrabajador.HasValue
                    ? "ID:" + idTrabajador.Value
                    : "N:" + (nombre ?? "").Trim().ToUpperInvariant();
            }
        }

        #endregion
    }
}
