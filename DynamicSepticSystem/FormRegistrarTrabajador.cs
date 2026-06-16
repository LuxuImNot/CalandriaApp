using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class FormRegistrarTrabajador : Form
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;

        private byte[] _pdfCURP;
        private byte[] _pdfRFC;
        private byte[] _pdfINE;
        private byte[] _pdfNSS;
        private byte[] _foto;

        private int? _idTrabajadorActual;

        public FormRegistrarTrabajador()
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);

            // Restaurar paleta Calandria que ThemeManager pudo sobrescribir
            RestaurarEstilosUI();

            cmbNacionalidad.Items.AddRange(new object[]
            {
                "Mexicana", "Estadounidense", "Guatemalteca", "Hondureña",
                "Salvadoreña", "Venezolana", "Colombiana", "Cubana", "Otra"
            });
            cmbNacionalidad.SelectedIndex = 0;

            dtpFechaNacimiento.MinDate = new DateTime(1920, 1, 1);
            dtpFechaNacimiento.MaxDate = DateTime.Today;
            dtpFechaNacimiento.Value = new DateTime(1990, 1, 1);

            InicializarTablaTrabajadores();
            CargarListaTrabajadores();
            LimpiarFormulario();

            cmbBuscarTrabajador.SelectedIndexChanged += CmbBuscarTrabajador_SelectedIndexChanged;
        }

        /// <summary>
        /// ThemeManager.AplicarTema sobrescribe BackColor/ForeColor/Font de todos los botones.
        /// Restauramos aquí la paleta Calandria pulida y wire-up de paints decorativos.
        /// </summary>
        private void RestaurarEstilosUI()
        {
            var verdePrimario = Color.FromArgb(39, 174, 96);
            var cafe          = Color.FromArgb(179, 108, 46);
            var azul          = Color.FromArgb(52, 152, 219);
            var rojo          = Color.FromArgb(192, 57, 43);
            var gris          = Color.FromArgb(189, 195, 199);

            ReestilizarBoton(btnGuardar,   verdePrimario, 10F);
            ReestilizarBoton(btnNuevo,     cafe,          10F);
            ReestilizarBoton(btnEliminar,  rojo,          10F);
            ReestilizarBoton(btnCancelar,  gris,          10F);
            ReestilizarBoton(btnCargarFoto,   cafe, 8.75F);
            ReestilizarBoton(btnQuitarFoto,   gris, 8.75F);
            ReestilizarBoton(btnCargarCURP,   cafe, 8.5F);
            ReestilizarBoton(btnVerCURP,      azul, 8.5F);
            ReestilizarBoton(btnCargarRFC,    cafe, 8.5F);
            ReestilizarBoton(btnVerRFC,       azul, 8.5F);
            ReestilizarBoton(btnCargarINE,    cafe, 8.5F);
            ReestilizarBoton(btnVerINE,       azul, 8.5F);
            ReestilizarBoton(btnCargarNSS,    cafe, 8.5F);
            ReestilizarBoton(btnVerNSS,       azul, 8.5F);

            // Placeholder visual cuando el PictureBox de la foto está vacío
            pbFoto.Paint += PintarPlaceholderFoto;

            // Asterisco rojo en labels obligatorios (terminan en " *")
            lblNombre.Paint       += PintarAsteriscoRojo;
            lblNacionalidad.Paint += PintarAsteriscoRojo;
        }

        private static void ReestilizarBoton(Button btn, Color fondo, float tamFuente)
        {
            btn.BackColor = fondo;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.UseVisualStyleBackColor = false;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI Semibold", tamFuente, FontStyle.Bold);
            btn.TextAlign = ContentAlignment.MiddleCenter;
        }

        // Línea de separación arriba del footer
        private void PintarSeparadorFooter(object sender, PaintEventArgs e)
        {
            if (!(sender is Panel p)) return;
            using (var pen = new Pen(Color.FromArgb(220, 210, 195)))
            {
                e.Graphics.DrawLine(pen, 0, 0, p.Width, 0);
            }
        }

        // Marca decorativa cuando aún no hay foto cargada
        private void PintarPlaceholderFoto(object sender, PaintEventArgs e)
        {
            if (_foto != null && _foto.Length > 0) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = pbFoto.ClientRectangle;
            using (var brushFondo = new SolidBrush(Color.FromArgb(250, 246, 240)))
                g.FillRectangle(brushFondo, rect);

            // Silueta sencilla (cabeza + hombros)
            using (var brushSilueta = new SolidBrush(Color.FromArgb(220, 210, 195)))
            {
                int cx = rect.Width / 2;
                int cy = rect.Height / 2 - 4;
                g.FillEllipse(brushSilueta, cx - 10, cy - 18, 20, 20);
                g.FillEllipse(brushSilueta, cx - 22, cy + 2, 44, 30);
            }

            using (var font = new Font("Segoe UI", 7.25F, FontStyle.Italic))
            using (var brushTexto = new SolidBrush(Color.FromArgb(155, 140, 120)))
            using (var sf = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far })
            {
                g.DrawString("sin foto", font, brushTexto,
                    new RectangleF(0, rect.Height - 14, rect.Width, 14), sf);
            }
        }

        // Pinta el " *" final en rojo para indicar obligatoriedad
        private void PintarAsteriscoRojo(object sender, PaintEventArgs e)
        {
            if (!(sender is Label lbl)) return;
            string txt = lbl.Text ?? "";
            if (!txt.EndsWith("*")) return;

            // Borramos el dibujo por defecto del label
            using (var brushFondo = new SolidBrush(lbl.Parent?.BackColor ?? lbl.BackColor))
                e.Graphics.FillRectangle(brushFondo, lbl.ClientRectangle);

            string baseTxt = txt.TrimEnd('*').TrimEnd();
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap
            };
            using (var brushBase = new SolidBrush(lbl.ForeColor))
                e.Graphics.DrawString(baseTxt, lbl.Font, brushBase, lbl.ClientRectangle, sf);

            // Mide el ancho del texto base para colocar el asterisco a su derecha
            SizeF anchoBase = e.Graphics.MeasureString(baseTxt, lbl.Font);
            var rectAst = new RectangleF(
                anchoBase.Width + 4, 0,
                lbl.ClientRectangle.Width - anchoBase.Width - 4,
                lbl.ClientRectangle.Height);
            using (var fontAst = new Font(lbl.Font, FontStyle.Bold))
            using (var brushAst = new SolidBrush(Color.FromArgb(192, 57, 43)))
                e.Graphics.DrawString("*", fontAst, brushAst, rectAst, sf);

            sf.Dispose();
        }

        private void InicializarTablaTrabajadores()
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.TRABAJADORES') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.TRABAJADORES (
        IdTrabajador INT IDENTITY(1,1) PRIMARY KEY,
        ClaveTrabajador NVARCHAR(20) NOT NULL UNIQUE,
        Nombre NVARCHAR(200) NOT NULL,
        Nacionalidad NVARCHAR(50) NOT NULL,
        FechaNacimiento DATE NULL,
        CURP NVARCHAR(18) NULL,
        RFC NVARCHAR(13) NULL,
        INE NVARCHAR(20) NULL,
        NSS NVARCHAR(11) NULL,
        PDF_CURP VARBINARY(MAX) NULL,
        PDF_RFC VARBINARY(MAX) NULL,
        PDF_INE VARBINARY(MAX) NULL,
        PDF_NSS VARBINARY(MAX) NULL,
        Foto VARBINARY(MAX) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT(GETDATE())
    );
END
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID(N'dbo.TRABAJADORES') AND name = 'Foto')
BEGIN
    ALTER TABLE dbo.TRABAJADORES ADD Foto VARBINARY(MAX) NULL;
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
            catch (Exception ex)
            {
                MessageBox.Show("Error al inicializar tabla TRABAJADORES: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarListaTrabajadores()
        {
            cmbBuscarTrabajador.Items.Clear();

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
                            cmbBuscarTrabajador.Items.Add(new TrabajadorItem
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
                MessageBox.Show("Error al cargar trabajadores: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbBuscarTrabajador_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = cmbBuscarTrabajador.SelectedItem as TrabajadorItem;
            if (item == null) return;
            CargarTrabajador(item.Id);
        }

        private void CargarTrabajador(int idTrabajador)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT * FROM TRABAJADORES WHERE IdTrabajador = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idTrabajador);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read()) return;

                            _idTrabajadorActual = idTrabajador;
                            txtClave.Text = reader["ClaveTrabajador"].ToString();
                            txtNombre.Text = reader["Nombre"].ToString();

                            string nacionalidad = reader["Nacionalidad"].ToString();
                            int idx = cmbNacionalidad.Items.IndexOf(nacionalidad);
                            cmbNacionalidad.SelectedIndex = idx >= 0 ? idx : cmbNacionalidad.Items.Count - 1;

                            if (reader["FechaNacimiento"] != DBNull.Value)
                                dtpFechaNacimiento.Value = Convert.ToDateTime(reader["FechaNacimiento"]);

                            txtCURP.Text = reader["CURP"] == DBNull.Value ? "" : reader["CURP"].ToString();
                            txtRFC.Text = reader["RFC"] == DBNull.Value ? "" : reader["RFC"].ToString();
                            txtINE.Text = reader["INE"] == DBNull.Value ? "" : reader["INE"].ToString();
                            txtNSS.Text = reader["NSS"] == DBNull.Value ? "" : reader["NSS"].ToString();

                            _pdfCURP = LeerBytes(reader, "PDF_CURP");
                            _pdfRFC = LeerBytes(reader, "PDF_RFC");
                            _pdfINE = LeerBytes(reader, "PDF_INE");
                            _pdfNSS = LeerBytes(reader, "PDF_NSS");
                            _foto = ColumnaExiste(reader, "Foto") ? LeerBytes(reader, "Foto") : null;
                        }
                    }
                }

                ActualizarEstadosPDF();
                ActualizarPreviewFoto();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar trabajador: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static byte[] LeerBytes(SqlDataReader reader, string columna)
        {
            return reader[columna] == DBNull.Value ? null : (byte[])reader[columna];
        }

        private static bool ColumnaExiste(SqlDataReader reader, string columna)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (string.Equals(reader.GetName(i), columna, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void ActualizarPreviewFoto()
        {
            if (pbFoto.Image != null)
            {
                var anterior = pbFoto.Image;
                pbFoto.Image = null;
                anterior.Dispose();
            }

            if (_foto != null && _foto.Length > 0)
            {
                try
                {
                    using (var ms = new MemoryStream(_foto))
                    {
                        pbFoto.Image = Image.FromStream(ms);
                    }
                }
                catch
                {
                    pbFoto.Image = null;
                }
            }
        }

        private void btnCargarFoto_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Seleccionar foto del trabajador";
                ofd.Filter = "Imágenes (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    byte[] bytes = File.ReadAllBytes(ofd.FileName);
                    using (var ms = new MemoryStream(bytes))
                    {
                        Image.FromStream(ms).Dispose();
                    }
                    _foto = bytes;
                    ActualizarPreviewFoto();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo cargar la imagen: " + ex.Message,
                        "Foto", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnQuitarFoto_Click(object sender, EventArgs e)
        {
            _foto = null;
            ActualizarPreviewFoto();
        }

        private void ActualizarEstadosPDF()
        {
            ActualizarEstado(lblEstadoCURP, _pdfCURP);
            ActualizarEstado(lblEstadoRFC, _pdfRFC);
            ActualizarEstado(lblEstadoINE, _pdfINE);
            ActualizarEstado(lblEstadoNSS, _pdfNSS);
        }

        private static void ActualizarEstado(Label lbl, byte[] data)
        {
            if (data != null && data.Length > 0)
            {
                int kb = Math.Max(1, data.Length / 1024);
                lbl.Text = "✓ " + kb + " KB";
                lbl.ForeColor = System.Drawing.Color.FromArgb(39, 134, 75);
                lbl.BackColor = System.Drawing.Color.FromArgb(232, 245, 233);
            }
            else
            {
                lbl.Text = "○ sin archivo";
                lbl.ForeColor = System.Drawing.Color.FromArgb(155, 155, 155);
                lbl.BackColor = System.Drawing.Color.FromArgb(247, 244, 238);
            }
        }

        private byte[] CargarPDFDesdeArchivo()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Seleccionar documento PDF";
                ofd.Filter = "Archivos PDF (*.pdf)|*.pdf";
                if (ofd.ShowDialog() != DialogResult.OK) return null;
                try
                {
                    return File.ReadAllBytes(ofd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al leer el PDF: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
        }

        private void VerPDF(byte[] data, string nombreDocumento)
        {
            if (data == null || data.Length == 0)
            {
                MessageBox.Show("No hay PDF cargado para " + nombreDocumento + ".",
                    "Sin documento", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string carpeta = Path.Combine(Path.GetTempPath(), "CalandriaPDFs");
                Directory.CreateDirectory(carpeta);
                string archivo = Path.Combine(carpeta,
                    $"{nombreDocumento}_{Guid.NewGuid():N}.pdf");
                File.WriteAllBytes(archivo, data);

                string sugerido = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    $"{nombreDocumento}_{txtClave.Text}.pdf");

                using (var preview = new FormPdfPreview(archivo, sugerido))
                {
                    preview.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al visualizar PDF: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCargarCURP_Click(object sender, EventArgs e)
        {
            var data = CargarPDFDesdeArchivo();
            if (data != null) { _pdfCURP = data; ActualizarEstadosPDF(); }
        }

        private void btnVerCURP_Click(object sender, EventArgs e) => VerPDF(_pdfCURP, "CURP");

        private void btnCargarRFC_Click(object sender, EventArgs e)
        {
            var data = CargarPDFDesdeArchivo();
            if (data != null) { _pdfRFC = data; ActualizarEstadosPDF(); }
        }

        private void btnVerRFC_Click(object sender, EventArgs e) => VerPDF(_pdfRFC, "RFC");

        private void btnCargarINE_Click(object sender, EventArgs e)
        {
            var data = CargarPDFDesdeArchivo();
            if (data != null) { _pdfINE = data; ActualizarEstadosPDF(); }
        }

        private void btnVerINE_Click(object sender, EventArgs e) => VerPDF(_pdfINE, "INE");

        private void btnCargarNSS_Click(object sender, EventArgs e)
        {
            var data = CargarPDFDesdeArchivo();
            if (data != null) { _pdfNSS = data; ActualizarEstadosPDF(); }
        }

        private void btnVerNSS_Click(object sender, EventArgs e) => VerPDF(_pdfNSS, "NSS");

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            _idTrabajadorActual = null;
            txtClave.Text = GenerarClaveUnica();
            txtNombre.Text = "";
            cmbNacionalidad.SelectedIndex = 0;
            dtpFechaNacimiento.Value = new DateTime(1990, 1, 1);
            txtCURP.Text = "";
            txtRFC.Text = "";
            txtINE.Text = "";
            txtNSS.Text = "";
            _pdfCURP = _pdfRFC = _pdfINE = _pdfNSS = null;
            _foto = null;
            ActualizarEstadosPDF();
            ActualizarPreviewFoto();
            cmbBuscarTrabajador.SelectedIndex = -1;
            txtNombre.Focus();
        }

        private string GenerarClaveUnica()
        {
            var rnd = new Random();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    while (true)
                    {
                        string clave = "TR-" + rnd.Next(0, 1000000).ToString("D6");
                        using (var cmd = new SqlCommand(
                            "SELECT COUNT(*) FROM TRABAJADORES WHERE ClaveTrabajador = @c", conn))
                        {
                            cmd.Parameters.AddWithValue("@c", clave);
                            int count = (int)cmd.ExecuteScalar();
                            if (count == 0) return clave;
                        }
                    }
                }
            }
            catch
            {
                return "TR-" + rnd.Next(0, 1000000).ToString("D6");
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            if (!string.IsNullOrEmpty(txtCURP.Text) && txtCURP.Text.Length != 18)
            {
                MessageBox.Show("El CURP debe tener 18 caracteres.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCURP.Focus();
                return;
            }

            if (!string.IsNullOrEmpty(txtRFC.Text) && (txtRFC.Text.Length < 12 || txtRFC.Text.Length > 13))
            {
                MessageBox.Show("El RFC debe tener 12 o 13 caracteres.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRFC.Focus();
                return;
            }

            if (!string.IsNullOrEmpty(txtNSS.Text) && txtNSS.Text.Length != 11)
            {
                MessageBox.Show("El NSS debe tener 11 dígitos.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNSS.Focus();
                return;
            }

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = _idTrabajadorActual.HasValue
                        ? @"UPDATE TRABAJADORES SET
                                Nombre = @nombre,
                                Nacionalidad = @nacionalidad,
                                FechaNacimiento = @fechaNac,
                                CURP = @curp,
                                RFC = @rfc,
                                INE = @ine,
                                NSS = @nss,
                                PDF_CURP = @pdfCURP,
                                PDF_RFC = @pdfRFC,
                                PDF_INE = @pdfINE,
                                PDF_NSS = @pdfNSS,
                                Foto = @foto
                            WHERE IdTrabajador = @id"
                        : @"INSERT INTO TRABAJADORES
                            (ClaveTrabajador, Nombre, Nacionalidad, FechaNacimiento,
                             CURP, RFC, INE, NSS, PDF_CURP, PDF_RFC, PDF_INE, PDF_NSS, Foto)
                            VALUES (@clave, @nombre, @nacionalidad, @fechaNac,
                                    @curp, @rfc, @ine, @nss, @pdfCURP, @pdfRFC, @pdfINE, @pdfNSS, @foto)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        if (_idTrabajadorActual.HasValue)
                            cmd.Parameters.AddWithValue("@id", _idTrabajadorActual.Value);
                        else
                            cmd.Parameters.AddWithValue("@clave", txtClave.Text.Trim());

                        cmd.Parameters.AddWithValue("@nombre", txtNombre.Text.Trim());
                        cmd.Parameters.AddWithValue("@nacionalidad", cmbNacionalidad.SelectedItem?.ToString() ?? "Mexicana");
                        cmd.Parameters.AddWithValue("@fechaNac", dtpFechaNacimiento.Value.Date);
                        cmd.Parameters.AddWithValue("@curp", ParametroOpcional(txtCURP.Text));
                        cmd.Parameters.AddWithValue("@rfc", ParametroOpcional(txtRFC.Text));
                        cmd.Parameters.AddWithValue("@ine", ParametroOpcional(txtINE.Text));
                        cmd.Parameters.AddWithValue("@nss", ParametroOpcional(txtNSS.Text));
                        AgregarBinario(cmd, "@pdfCURP", _pdfCURP);
                        AgregarBinario(cmd, "@pdfRFC", _pdfRFC);
                        AgregarBinario(cmd, "@pdfINE", _pdfINE);
                        AgregarBinario(cmd, "@pdfNSS", _pdfNSS);
                        AgregarBinario(cmd, "@foto", _foto);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Trabajador guardado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                CargarListaTrabajadores();
                LimpiarFormulario();
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("Ya existe un trabajador con esa matrícula.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static object ParametroOpcional(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? (object)DBNull.Value : texto.Trim();
        }

        private static void AgregarBinario(SqlCommand cmd, string nombre, byte[] data)
        {
            var p = cmd.Parameters.Add(nombre, SqlDbType.VarBinary, -1);
            p.Value = (data != null && data.Length > 0) ? (object)data : DBNull.Value;
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (!_idTrabajadorActual.HasValue)
            {
                MessageBox.Show("Selecciona un trabajador existente para eliminar.", "Eliminar",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"¿Eliminar al trabajador \"{txtNombre.Text}\"?\nEsta acción no se puede deshacer.",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "DELETE FROM TRABAJADORES WHERE IdTrabajador = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", _idTrabajadorActual.Value);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Trabajador eliminado.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarListaTrabajadores();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private class TrabajadorItem
        {
            public int Id { get; set; }
            public string Clave { get; set; }
            public string Nombre { get; set; }
            public override string ToString() => $"{Clave} — {Nombre}";
        }
    }
}
