using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Vista de perfil de un trabajador con sus datos personales, foto, documentos
    /// y el historial de recibos de nómina emitidos (tabla RecibosNomina).
    /// </summary>
    public class FormPerfilTrabajador : Form
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;

        private ComboBox cmbTrabajador;
        private Button btnRefrescar;

        private PictureBox pbFoto;
        private Label lblNombre;
        private Label lblClave;
        private Label lblRol;
        private Label lblNacionalidad;
        private Label lblFechaNac;
        private Label lblCURP;
        private Label lblRFC;
        private Label lblINE;
        private Label lblNSS;

        private Button btnVerCURP;
        private Button btnVerRFC;
        private Button btnVerINE;
        private Button btnVerNSS;

        private DataGridView dgvRecibos;
        private Label lblResumenRecibos;
        private Button btnVerRecibo;
        private Button btnCerrar;

        private byte[] _pdfCURP;
        private byte[] _pdfRFC;
        private byte[] _pdfINE;
        private byte[] _pdfNSS;

        private int? _idTrabajadorActual;
        private readonly BindingList<ReciboItem> _recibos = new BindingList<ReciboItem>();

        public FormPerfilTrabajador() : this(null) { }

        public FormPerfilTrabajador(int? idTrabajadorInicial)
        {
            BuildUI();
            ThemeManager.AplicarTema(this);
            EnsureTablaRecibos();
            CargarTrabajadores();

            if (idTrabajadorInicial.HasValue)
                SeleccionarTrabajador(idTrabajadorInicial.Value);
        }

        #region UI

        private void BuildUI()
        {
            this.Text = "Perfil del Trabajador";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(1040, 680);
            this.ClientSize = new Size(1080, 720);
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);

            var panelBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(41, 60, 88)
            };
            var lblTitulo = new Label
            {
                Text = "PERFIL DEL TRABAJADOR",
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 8)
            };
            var lblSub = new Label
            {
                Text = "Datos personales, documentos e historial de recibos de nómina",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(189, 200, 215),
                AutoSize = true,
                Location = new Point(22, 32)
            };
            panelBanner.Controls.Add(lblTitulo);
            panelBanner.Controls.Add(lblSub);

            // Selector
            var lblSelector = new Label
            {
                Text = "Trabajador:",
                Location = new Point(22, 74),
                Size = new Size(80, 22),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(99, 110, 114)
            };
            cmbTrabajador = new ComboBox
            {
                Location = new Point(110, 70),
                Size = new Size(420, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cmbTrabajador.SelectedIndexChanged += (s, e) =>
            {
                var item = cmbTrabajador.SelectedItem as TrabajadorItem;
                if (item != null) CargarTrabajador(item.Id);
            };
            btnRefrescar = new Button
            {
                Text = "Refrescar",
                Location = new Point(540, 69),
                Size = new Size(110, 28),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnRefrescar.Click += (s, e) =>
            {
                int? id = _idTrabajadorActual;
                CargarTrabajadores();
                if (id.HasValue) SeleccionarTrabajador(id.Value);
            };

            // Ficha (lado izquierdo)
            var panelFicha = new Panel
            {
                Location = new Point(22, 108),
                Size = new Size(380, 560),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
                BackColor = Color.FromArgb(248, 250, 253),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(14)
            };

            pbFoto = new PictureBox
            {
                Location = new Point(120, 14),
                Size = new Size(140, 160),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            lblNombre = new Label
            {
                Location = new Point(14, 184),
                Size = new Size(352, 28),
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 47, 67),
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblClave = new Label
            {
                Location = new Point(14, 214),
                Size = new Size(352, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(99, 110, 114),
                TextAlign = ContentAlignment.MiddleCenter
            };

            int yCampo = 248;
            lblRol = AgregarCampo(panelFicha, "Rol / cuadrilla:", ref yCampo);
            lblNacionalidad = AgregarCampo(panelFicha, "Nacionalidad:", ref yCampo);
            lblFechaNac = AgregarCampo(panelFicha, "Fecha de nacimiento:", ref yCampo);
            lblCURP = AgregarCampo(panelFicha, "CURP:", ref yCampo);
            lblRFC = AgregarCampo(panelFicha, "RFC:", ref yCampo);
            lblINE = AgregarCampo(panelFicha, "INE:", ref yCampo);
            lblNSS = AgregarCampo(panelFicha, "NSS:", ref yCampo);

            // Botones documentos
            int yBtns = yCampo + 12;
            btnVerCURP = BotonDocumento("Ver CURP", 14, yBtns);
            btnVerRFC = BotonDocumento("Ver RFC", 100, yBtns);
            btnVerINE = BotonDocumento("Ver INE", 186, yBtns);
            btnVerNSS = BotonDocumento("Ver NSS", 272, yBtns);
            btnVerCURP.Click += (s, e) => VerPDF(_pdfCURP, "CURP");
            btnVerRFC.Click += (s, e) => VerPDF(_pdfRFC, "RFC");
            btnVerINE.Click += (s, e) => VerPDF(_pdfINE, "INE");
            btnVerNSS.Click += (s, e) => VerPDF(_pdfNSS, "NSS");

            panelFicha.Controls.Add(pbFoto);
            panelFicha.Controls.Add(lblNombre);
            panelFicha.Controls.Add(lblClave);
            panelFicha.Controls.Add(btnVerCURP);
            panelFicha.Controls.Add(btnVerRFC);
            panelFicha.Controls.Add(btnVerINE);
            panelFicha.Controls.Add(btnVerNSS);

            // Historial (lado derecho)
            var lblHistorial = new Label
            {
                Text = "HISTORIAL DE RECIBOS DE NÓMINA",
                Location = new Point(420, 108),
                Size = new Size(640, 22),
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 47, 67),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            dgvRecibos = new DataGridView
            {
                Location = new Point(420, 134),
                Size = new Size(640, 488),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true
            };
            dgvRecibos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvRecibos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRecibos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvRecibos.EnableHeadersVisualStyles = false;
            dgvRecibos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);

            dgvRecibos.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Fecha",
                DataPropertyName = "FechaTexto",
                Width = 100
            });
            dgvRecibos.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Cuadrilla",
                DataPropertyName = "Cuadrilla",
                Width = 100
            });
            dgvRecibos.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Concepto",
                DataPropertyName = "Concepto",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    WrapMode = DataGridViewTriState.True
                }
            });
            dgvRecibos.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Monto",
                DataPropertyName = "MontoTexto",
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });
            dgvRecibos.DataSource = _recibos;
            dgvRecibos.CellDoubleClick += (s, e) => VerReciboSeleccionado();

            lblResumenRecibos = new Label
            {
                Location = new Point(420, 628),
                Size = new Size(420, 22),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 47, 67)
            };

            btnVerRecibo = new Button
            {
                Text = "Ver / Guardar Recibo",
                Location = new Point(844, 628),
                Size = new Size(160, 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnVerRecibo.Click += (s, e) => VerReciboSeleccionado();

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Location = new Point(1010, 628),
                Size = new Size(60, 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnCerrar.Click += (s, e) => this.Close();

            this.Controls.Add(lblSelector);
            this.Controls.Add(cmbTrabajador);
            this.Controls.Add(btnRefrescar);
            this.Controls.Add(panelFicha);
            this.Controls.Add(lblHistorial);
            this.Controls.Add(dgvRecibos);
            this.Controls.Add(lblResumenRecibos);
            this.Controls.Add(btnVerRecibo);
            this.Controls.Add(btnCerrar);
            this.Controls.Add(panelBanner);
        }

        private Label AgregarCampo(Panel padre, string etiqueta, ref int y)
        {
            var lblE = new Label
            {
                Text = etiqueta,
                Location = new Point(14, y),
                Size = new Size(140, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(99, 110, 114)
            };
            var lblV = new Label
            {
                Location = new Point(154, y),
                Size = new Size(212, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(33, 47, 67)
            };
            padre.Controls.Add(lblE);
            padre.Controls.Add(lblV);
            y += 24;
            return lblV;
        }

        private static Button BotonDocumento(string texto, int x, int y)
        {
            return new Button
            {
                Text = texto,
                Location = new Point(x, y),
                Size = new Size(80, 28),
                BackColor = Color.FromArgb(99, 110, 114),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
        }

        #endregion

        #region Datos

        private void EnsureTablaRecibos()
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RecibosNomina')
BEGIN
    CREATE TABLE RecibosNomina (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdTrabajador INT NULL,
        NombreTrabajador NVARCHAR(200) NOT NULL,
        Rol NVARCHAR(80) NULL,
        CodigoCuadrilla NVARCHAR(20) NULL,
        Concepto NVARCHAR(MAX) NULL,
        Monto DECIMAL(18,2) NOT NULL,
        FechaRecibo DATE NOT NULL,
        PeriodoDesde DATE NULL,
        PeriodoHasta DATE NULL,
        TotalCuadrilla DECIMAL(18,2) NULL,
        Pdf VARBINARY(MAX) NULL,
        Usuario NVARCHAR(120) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
    );
END";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                        cmd.ExecuteNonQuery();
                }
            }
            catch { /* silencioso */ }
        }

        private void CargarTrabajadores()
        {
            cmbTrabajador.Items.Clear();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT IdTrabajador, ClaveTrabajador, Nombre FROM TRABAJADORES ORDER BY Nombre", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbTrabajador.Items.Add(new TrabajadorItem
                            {
                                Id = Convert.ToInt32(reader["IdTrabajador"]),
                                Clave = reader["ClaveTrabajador"].ToString(),
                                Nombre = reader["Nombre"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando trabajadores: " + ex.Message,
                    "Trabajadores", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SeleccionarTrabajador(int id)
        {
            for (int i = 0; i < cmbTrabajador.Items.Count; i++)
            {
                var it = cmbTrabajador.Items[i] as TrabajadorItem;
                if (it != null && it.Id == id)
                {
                    cmbTrabajador.SelectedIndex = i;
                    return;
                }
            }
        }

        private void CargarTrabajador(int id)
        {
            _idTrabajadorActual = id;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT * FROM TRABAJADORES WHERE IdTrabajador = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read()) return;

                            lblNombre.Text = (reader["Nombre"] ?? "").ToString();
                            lblClave.Text = "Clave: " + (reader["ClaveTrabajador"] ?? "").ToString();
                            lblNacionalidad.Text = (reader["Nacionalidad"] ?? "").ToString();
                            lblFechaNac.Text = reader["FechaNacimiento"] == DBNull.Value
                                ? "-"
                                : Convert.ToDateTime(reader["FechaNacimiento"]).ToString(
                                    "dd/MM/yyyy", CultureInfo.GetCultureInfo("es-MX"));
                            lblCURP.Text = (reader["CURP"] == DBNull.Value ? "-" : reader["CURP"].ToString());
                            lblRFC.Text = (reader["RFC"] == DBNull.Value ? "-" : reader["RFC"].ToString());
                            lblINE.Text = (reader["INE"] == DBNull.Value ? "-" : reader["INE"].ToString());
                            lblNSS.Text = (reader["NSS"] == DBNull.Value ? "-" : reader["NSS"].ToString());

                            _pdfCURP = LeerBytes(reader, "PDF_CURP");
                            _pdfRFC = LeerBytes(reader, "PDF_RFC");
                            _pdfINE = LeerBytes(reader, "PDF_INE");
                            _pdfNSS = LeerBytes(reader, "PDF_NSS");

                            byte[] foto = ColumnaExiste(reader, "Foto") ? LeerBytes(reader, "Foto") : null;
                            MostrarFoto(foto);
                        }
                    }
                }
                lblRol.Text = ObtenerRolYCuadrilla(id);
                CargarRecibos(id);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando datos del trabajador: " + ex.Message,
                    "Perfil", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string ObtenerRolYCuadrilla(int idTrabajador)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"
                        SELECT TOP 1 Rol, CodigoCuadrilla, EsJefe
                        FROM MiembrosCuadrilla
                        WHERE IdTrabajador = @id
                        ORDER BY EsJefe DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idTrabajador);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string rol = (reader["Rol"] ?? "-").ToString();
                                string codigo = (reader["CodigoCuadrilla"] ?? "-").ToString();
                                bool jefe = Convert.ToBoolean(reader["EsJefe"]);
                                return rol + (jefe ? " (jefe) " : " ") + " · " + codigo;
                            }
                        }
                    }
                }
            }
            catch { }
            return "(sin cuadrilla asignada)";
        }

        private void CargarRecibos(int idTrabajador)
        {
            _recibos.Clear();
            decimal total = 0m;
            int cuenta = 0;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"
                        SELECT Id, FechaRecibo, CodigoCuadrilla, Concepto, Monto,
                               CASE WHEN Pdf IS NULL THEN 0 ELSE 1 END AS TienePdf
                        FROM RecibosNomina
                        WHERE IdTrabajador = @id
                           OR (IdTrabajador IS NULL AND NombreTrabajador = @nombre)
                        ORDER BY FechaRecibo DESC, Id DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idTrabajador);
                        cmd.Parameters.AddWithValue("@nombre", lblNombre.Text ?? "");
                        using (var reader = cmd.ExecuteReader())
                        {
                            var cul = CultureInfo.GetCultureInfo("es-MX");
                            while (reader.Read())
                            {
                                decimal monto = Convert.ToDecimal(reader["Monto"]);
                                _recibos.Add(new ReciboItem
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Fecha = Convert.ToDateTime(reader["FechaRecibo"]),
                                    FechaTexto = Convert.ToDateTime(reader["FechaRecibo"])
                                        .ToString("dd/MM/yyyy", cul),
                                    Cuadrilla = (reader["CodigoCuadrilla"] ?? "").ToString(),
                                    Concepto = (reader["Concepto"] ?? "").ToString(),
                                    Monto = monto,
                                    MontoTexto = monto.ToString("C2", cul),
                                    TienePdf = Convert.ToInt32(reader["TienePdf"]) == 1
                                });
                                total += monto;
                                cuenta++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando recibos: " + ex.Message,
                    "Recibos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            lblResumenRecibos.Text = string.Format("Recibos: {0}   ·   Total histórico: {1}",
                cuenta, total.ToString("C2", CultureInfo.GetCultureInfo("es-MX")));
        }

        private void VerReciboSeleccionado()
        {
            if (dgvRecibos.CurrentRow == null) return;
            var item = dgvRecibos.CurrentRow.DataBoundItem as ReciboItem;
            if (item == null) return;

            if (!item.TienePdf)
            {
                MessageBox.Show("Este recibo no tiene PDF almacenado.",
                    "Recibos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                byte[] pdf;
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT Pdf FROM RecibosNomina WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", item.Id);
                        var res = cmd.ExecuteScalar();
                        if (res == null || res == DBNull.Value)
                        {
                            MessageBox.Show("El PDF ya no está disponible.",
                                "Recibos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        pdf = (byte[])res;
                    }
                }

                string nombreSeguro = string.IsNullOrEmpty(lblNombre.Text)
                    ? "Trabajador"
                    : lblNombre.Text;
                foreach (var c in Path.GetInvalidFileNameChars())
                    nombreSeguro = nombreSeguro.Replace(c, '_');

                string archivoTemp = Path.Combine(Path.GetTempPath(),
                    string.Format("Recibo_{0}_{1:yyyyMMdd_HHmmss}.pdf", nombreSeguro, DateTime.Now));
                File.WriteAllBytes(archivoTemp, pdf);

                string sugerido = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    string.Format("Recibo_{0}_{1:yyyyMMdd}.pdf", nombreSeguro, item.Fecha));

                using (var preview = new FormPdfPreview(archivoTemp, sugerido))
                {
                    preview.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error abriendo el recibo: " + ex.Message,
                    "Recibos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void VerPDF(byte[] data, string tipo)
        {
            if (data == null || data.Length == 0)
            {
                MessageBox.Show("No hay PDF cargado para " + tipo + ".",
                    "Documento", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                string carpeta = Path.Combine(Path.GetTempPath(), "CalandriaPDFs");
                Directory.CreateDirectory(carpeta);
                string archivo = Path.Combine(carpeta,
                    string.Format("{0}_{1:N}.pdf", tipo, Guid.NewGuid()));
                File.WriteAllBytes(archivo, data);

                string sugerido = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    string.Format("{0}_{1}.pdf", tipo,
                        (lblClave.Text ?? "").Replace("Clave: ", "")));
                using (var preview = new FormPdfPreview(archivo, sugerido))
                {
                    preview.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error visualizando el PDF: " + ex.Message,
                    "Documento", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarFoto(byte[] foto)
        {
            if (pbFoto.Image != null)
            {
                var ant = pbFoto.Image;
                pbFoto.Image = null;
                ant.Dispose();
            }
            if (foto == null || foto.Length == 0) return;
            try
            {
                using (var ms = new MemoryStream(foto))
                    pbFoto.Image = Image.FromStream(ms);
            }
            catch { pbFoto.Image = null; }
        }

        private static byte[] LeerBytes(SqlDataReader reader, string columna)
        {
            return reader[columna] == DBNull.Value ? null : (byte[])reader[columna];
        }

        private static bool ColumnaExiste(SqlDataReader reader, string columna)
        {
            for (int i = 0; i < reader.FieldCount; i++)
                if (string.Equals(reader.GetName(i), columna, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        #endregion

        #region Modelos

        private class TrabajadorItem
        {
            public int Id { get; set; }
            public string Clave { get; set; }
            public string Nombre { get; set; }
            public override string ToString() { return Clave + " — " + Nombre; }
        }

        private class ReciboItem
        {
            public int Id { get; set; }
            public DateTime Fecha { get; set; }
            public string FechaTexto { get; set; }
            public string Cuadrilla { get; set; }
            public string Concepto { get; set; }
            public decimal Monto { get; set; }
            public string MontoTexto { get; set; }
            public bool TienePdf { get; set; }
        }

        #endregion
    }
}
