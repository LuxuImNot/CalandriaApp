using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public class FormPropiedadesDestajo : Form
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;

        private readonly string _manzana;
        private readonly string _lote;
        private readonly string _prototipo;
        private readonly string _ruta;
        private readonly FormActivarTareasTreeList.ItemTareaActivacion _destajo;
        private readonly List<FormActivarTareasTreeList.ItemTareaActivacion> _hijos;

        private TabControl tabs;
        private FlowLayoutPanel pnlFotos;
        private Button btnAgregarFoto;
        private Button btnActualizarFotos;
        private Label lblTotalFotos;

        public FormPropiedadesDestajo(
            string manzana,
            string lote,
            string prototipo,
            string ruta,
            FormActivarTareasTreeList.ItemTareaActivacion destajo,
            List<FormActivarTareasTreeList.ItemTareaActivacion> hijos)
        {
            _manzana = manzana ?? "";
            _lote = lote ?? "";
            _prototipo = prototipo ?? "";
            _ruta = ruta ?? "";
            _destajo = destajo;
            _hijos = hijos ?? new List<FormActivarTareasTreeList.ItemTareaActivacion>();

            BuildUI();
            ThemeManager.AplicarTema(this);
            EnsureTablaFotos();
            CargarFotos();
        }

        private void BuildUI()
        {
            this.Text = $"Propiedades del Destajo — M{_manzana} L{_lote}";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(820, 600);
            this.ClientSize = new Size(880, 640);
            this.Font = new Font("Segoe UI", 9F);
            this.BackColor = Color.White;

            var lblTitulo = new Label
            {
                Text = $"DESTAJO: {Truncar(_destajo?.Nombre ?? "-", 70)}",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 71, 161),
                Location = new Point(15, 12),
                AutoSize = true
            };

            var lblSub = new Label
            {
                Text = $"M{_manzana} · L{_lote} · {_prototipo} · {_ruta}",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(15, 42),
                AutoSize = true
            };

            tabs = new TabControl
            {
                Location = new Point(12, 70),
                Size = new Size(this.ClientSize.Width - 24, this.ClientSize.Height - 130),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F)
            };

            tabs.TabPages.Add(BuildTabGeneral());
            tabs.TabPages.Add(BuildTabCuadrilla());
            tabs.TabPages.Add(BuildTabSubtareas());
            tabs.TabPages.Add(BuildTabFotos());

            var btnCerrar = new Button
            {
                Text = "Cerrar",
                Size = new Size(120, 36),
                Location = new Point(this.ClientSize.Width - 135, this.ClientSize.Height - 48),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnCerrar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitulo, lblSub, tabs, btnCerrar });
        }

        private TabPage BuildTabGeneral()
        {
            var tab = new TabPage("General");
            tab.BackColor = Color.White;
            tab.Padding = new Padding(10);

            var info = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoScroll = true
            };
            info.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AgregarFila(info, "Manzana:", _manzana);
            AgregarFila(info, "Lote:", _lote);
            AgregarFila(info, "Prototipo:", _prototipo);
            AgregarFila(info, "Ruta:", _ruta);
            AgregarFila(info, "Nodo ID:", _destajo?.ID.ToString() ?? "-");
            AgregarFila(info, "Nombre del destajo:", _destajo?.Nombre ?? "-");
            AgregarFila(info, "Descripción:",
                string.IsNullOrEmpty(_destajo?.Descripcion) ? "—" : _destajo.Descripcion);
            AgregarFila(info, "Cantidad:", (_destajo?.Cantidad ?? 0m).ToString("N2", CultureInfo.CurrentCulture));
            AgregarFila(info, "Unidad:", _destajo?.Unidad ?? "-");
            AgregarFila(info, "Precio unitario:",
                (_destajo?.PrecioUnitario ?? 0m).ToString("C2", CultureInfo.CurrentCulture));
            AgregarFila(info, "Total:",
                (_destajo?.Total ?? 0m).ToString("C2", CultureInfo.CurrentCulture));
            AgregarFila(info, "Cuadrilla asignada:",
                string.IsNullOrEmpty(_destajo?.CuadrillaAsignada) ? "—" : _destajo.CuadrillaAsignada);

            string etapa;
            if (_destajo == null) etapa = "—";
            else if (_destajo.Finalizado) etapa = "Stage 2 — Finalizado";
            else if (_destajo.DesatajoActivado) etapa = "Stage 1 — Activado";
            else etapa = "Pendiente";
            AgregarFila(info, "Etapa:", etapa);

            var fechaActivacion = ObtenerFechaActivacion();
            AgregarFila(info, "Fecha activación:",
                fechaActivacion.HasValue
                    ? fechaActivacion.Value.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture)
                    : "—");
            AgregarFila(info, "Fecha finalización:",
                _destajo?.FechaFinalizacion.HasValue == true
                    ? _destajo.FechaFinalizacion.Value.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture)
                    : "—");

            tab.Controls.Add(info);
            return tab;
        }

        private TabPage BuildTabCuadrilla()
        {
            var tab = new TabPage("Cuadrilla");
            tab.BackColor = Color.White;
            tab.Padding = new Padding(10);

            var miembros = ObtenerMiembrosCuadrilla(_destajo?.CuadrillaAsignada);

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.EnableHeadersVisualStyles = false;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);

            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trabajador", DataPropertyName = "Nombre", Width = 240 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Rol", DataPropertyName = "Rol", Width = 110 });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Jefe", DataPropertyName = "EsJefe", Width = 50 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Teléfono", DataPropertyName = "Telefono", Width = 130 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Clave", DataPropertyName = "Clave", Width = 110 });

            grid.DataSource = miembros;

            var lblHeader = new Label
            {
                Text = string.IsNullOrEmpty(_destajo?.CuadrillaAsignada)
                    ? "Sin cuadrilla asignada"
                    : $"Cuadrilla {_destajo.CuadrillaAsignada} · {miembros.Count} integrantes",
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            tab.Controls.Add(grid);
            tab.Controls.Add(lblHeader);
            return tab;
        }

        private TabPage BuildTabSubtareas()
        {
            var tab = new TabPage("Subtareas");
            tab.BackColor = Color.White;
            tab.Padding = new Padding(10);

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.EnableHeadersVisualStyles = false;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);

            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = "Nombre", Width = 360 });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Cantidad",
                DataPropertyName = "Cantidad",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Unidad", DataPropertyName = "Unidad", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Precio Unitario",
                DataPropertyName = "PrecioUnitario",
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Total",
                DataPropertyName = "Total",
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            grid.DataSource = _hijos;

            var lblHeader = new Label
            {
                Text = $"{_hijos.Count} subtareas registradas",
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            tab.Controls.Add(grid);
            tab.Controls.Add(lblHeader);
            return tab;
        }

        private TabPage BuildTabFotos()
        {
            var tab = new TabPage("Fotos");
            tab.BackColor = Color.White;
            tab.Padding = new Padding(10);

            var pnlBotones = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44
            };

            btnAgregarFoto = new Button
            {
                Text = "Agregar foto...",
                Location = new Point(0, 6),
                Size = new Size(140, 32),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnAgregarFoto.Click += BtnAgregarFoto_Click;

            btnActualizarFotos = new Button
            {
                Text = "Actualizar",
                Location = new Point(150, 6),
                Size = new Size(110, 32),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnActualizarFotos.Click += (s, e) => CargarFotos();

            lblTotalFotos = new Label
            {
                Location = new Point(280, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(80, 80, 80)
            };

            pnlBotones.Controls.Add(btnAgregarFoto);
            pnlBotones.Controls.Add(btnActualizarFotos);
            pnlBotones.Controls.Add(lblTotalFotos);

            pnlFotos = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            tab.Controls.Add(pnlFotos);
            tab.Controls.Add(pnlBotones);
            return tab;
        }

        private static void AgregarFila(TableLayoutPanel tabla, string etiqueta, string valor)
        {
            int row = tabla.RowCount;
            tabla.RowCount = row + 1;
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));

            var lblEt = new Label
            {
                Text = etiqueta,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Anchor = AnchorStyles.Left,
                AutoSize = true
            };
            var lblVal = new Label
            {
                Text = valor ?? "-",
                Font = new Font("Segoe UI", 9F),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = true
            };

            tabla.Controls.Add(lblEt, 0, row);
            tabla.Controls.Add(lblVal, 1, row);
        }

        private DateTime? ObtenerFechaActivacion()
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"
                        SELECT MAX(FechaActualizacion)
                        FROM ActivacionTareasRuta
                        WHERE Manzana = @m AND Lote = @l AND Ruta = @r AND NodoID = @nodo", conn))
                    {
                        cmd.Parameters.AddWithValue("@m", _manzana);
                        cmd.Parameters.AddWithValue("@l", _lote);
                        cmd.Parameters.AddWithValue("@r", _ruta);
                        cmd.Parameters.AddWithValue("@nodo", _destajo?.ID ?? 0);
                        var result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return null;
                        return Convert.ToDateTime(result);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private List<MiembroVista> ObtenerMiembrosCuadrilla(string codigoCuadrilla)
        {
            var lista = new List<MiembroVista>();
            if (string.IsNullOrEmpty(codigoCuadrilla)) return lista;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"
                        SELECT m.Nombre, m.Rol, m.EsJefe, m.Telefono, t.ClaveTrabajador
                        FROM MiembrosCuadrilla m
                        LEFT JOIN TRABAJADORES t ON t.IdTrabajador = m.IdTrabajador
                        WHERE m.CodigoCuadrilla = @codigo
                        ORDER BY m.EsJefe DESC, m.Nombre", conn))
                    {
                        cmd.Parameters.AddWithValue("@codigo", codigoCuadrilla);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new MiembroVista
                                {
                                    Nombre = reader["Nombre"].ToString(),
                                    Rol = reader["Rol"].ToString(),
                                    EsJefe = Convert.ToBoolean(reader["EsJefe"]),
                                    Telefono = reader["Telefono"] == DBNull.Value ? "" : reader["Telefono"].ToString(),
                                    Clave = reader["ClaveTrabajador"] == DBNull.Value ? "" : reader["ClaveTrabajador"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch { }
            return lista;
        }

        private void EnsureTablaFotos()
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DestajoFotos')
BEGIN
    CREATE TABLE DestajoFotos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Manzana NVARCHAR(10) NOT NULL,
        Lote NVARCHAR(10) NOT NULL,
        Ruta NVARCHAR(50) NOT NULL,
        NodoID INT NOT NULL,
        Imagen VARBINARY(MAX) NOT NULL,
        Caption NVARCHAR(300) NULL,
        NombreArchivo NVARCHAR(255) NULL,
        Usuario NVARCHAR(100) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT(GETDATE())
    );
    CREATE INDEX IX_DestajoFotos ON DestajoFotos(Manzana, Lote, Ruta, NodoID);
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
                MessageBox.Show("No se pudo preparar la tabla de fotos: " + ex.Message,
                    "Fotos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CargarFotos()
        {
            // Liberar imágenes previas
            foreach (Control c in pnlFotos.Controls)
            {
                if (c is FotoCard card) card.LiberarImagen();
            }
            pnlFotos.Controls.Clear();

            int total = 0;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"
                        SELECT Id, Imagen, Caption, NombreArchivo, Usuario, FechaCreacion
                        FROM DestajoFotos
                        WHERE Manzana = @m AND Lote = @l AND Ruta = @r AND NodoID = @nodo
                        ORDER BY FechaCreacion DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@m", _manzana);
                        cmd.Parameters.AddWithValue("@l", _lote);
                        cmd.Parameters.AddWithValue("@r", _ruta);
                        cmd.Parameters.AddWithValue("@nodo", _destajo?.ID ?? 0);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = Convert.ToInt32(reader["Id"]);
                                byte[] bytes = (byte[])reader["Imagen"];
                                string caption = reader["Caption"] == DBNull.Value ? "" : reader["Caption"].ToString();
                                string nombre = reader["NombreArchivo"] == DBNull.Value ? "" : reader["NombreArchivo"].ToString();
                                string usuario = reader["Usuario"] == DBNull.Value ? "" : reader["Usuario"].ToString();
                                DateTime fecha = Convert.ToDateTime(reader["FechaCreacion"]);

                                var card = new FotoCard(id, bytes, caption, nombre, usuario, fecha);
                                card.OnVer += FotoCard_Ver;
                                card.OnEliminar += FotoCard_Eliminar;
                                card.OnEditarCaption += FotoCard_EditarCaption;
                                pnlFotos.Controls.Add(card);
                                total++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando fotos: " + ex.Message,
                    "Fotos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            lblTotalFotos.Text = total == 0 ? "Sin fotos registradas" : $"{total} foto(s)";
        }

        private void BtnAgregarFoto_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Seleccionar foto del destajo";
                ofd.Filter = "Imágenes (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
                ofd.Multiselect = true;
                if (ofd.ShowDialog(this) != DialogResult.OK) return;

                string captionGlobal = PromptCaption("Descripción para las fotos (opcional):", "");

                int agregadas = 0;
                foreach (var ruta in ofd.FileNames)
                {
                    try
                    {
                        byte[] bytes = File.ReadAllBytes(ruta);
                        using (var ms = new MemoryStream(bytes))
                        {
                            Image.FromStream(ms).Dispose();
                        }
                        InsertarFoto(bytes, captionGlobal, Path.GetFileName(ruta));
                        agregadas++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("No se pudo agregar " + Path.GetFileName(ruta) + ":\n" + ex.Message,
                            "Foto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                if (agregadas > 0) CargarFotos();
            }
        }

        private void InsertarFoto(byte[] bytes, string caption, string nombreArchivo)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    INSERT INTO DestajoFotos
                        (Manzana, Lote, Ruta, NodoID, Imagen, Caption, NombreArchivo, Usuario, FechaCreacion)
                    VALUES (@m, @l, @r, @nodo, @img, @cap, @arch, @usr, GETDATE())", conn))
                {
                    cmd.Parameters.AddWithValue("@m", _manzana);
                    cmd.Parameters.AddWithValue("@l", _lote);
                    cmd.Parameters.AddWithValue("@r", _ruta);
                    cmd.Parameters.AddWithValue("@nodo", _destajo?.ID ?? 0);
                    cmd.Parameters.Add("@img", SqlDbType.VarBinary, -1).Value = bytes;
                    cmd.Parameters.AddWithValue("@cap",
                        string.IsNullOrEmpty(caption) ? (object)DBNull.Value : caption);
                    cmd.Parameters.AddWithValue("@arch",
                        string.IsNullOrEmpty(nombreArchivo) ? (object)DBNull.Value : nombreArchivo);
                    cmd.Parameters.AddWithValue("@usr",
                        Global.UsuarioActual?.Nombre ?? Environment.UserName ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void FotoCard_Ver(int id, byte[] bytes, string caption)
        {
            using (var visor = new Form())
            {
                visor.Text = string.IsNullOrEmpty(caption) ? "Foto" : caption;
                visor.StartPosition = FormStartPosition.CenterParent;
                visor.ClientSize = new Size(900, 700);
                visor.BackColor = Color.Black;

                var pb = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Black
                };
                try
                {
                    using (var ms = new MemoryStream(bytes))
                    {
                        pb.Image = Image.FromStream(ms);
                    }
                }
                catch { }

                visor.Controls.Add(pb);
                visor.ShowDialog(this);
                pb.Image?.Dispose();
            }
        }

        private void FotoCard_Eliminar(int id)
        {
            var rsp = MessageBox.Show(
                "¿Eliminar esta foto del destajo?\nEsta acción no se puede deshacer.",
                "Eliminar foto", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (rsp != DialogResult.Yes) return;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "DELETE FROM DestajoFotos WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                CargarFotos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error eliminando foto: " + ex.Message,
                    "Foto", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FotoCard_EditarCaption(int id, string captionActual)
        {
            string nuevo = PromptCaption("Editar descripción:", captionActual ?? "");
            if (nuevo == null) return;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "UPDATE DestajoFotos SET Caption = @cap WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@cap",
                            string.IsNullOrEmpty(nuevo) ? (object)DBNull.Value : nuevo);
                        cmd.ExecuteNonQuery();
                    }
                }
                CargarFotos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error actualizando descripción: " + ex.Message,
                    "Foto", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string PromptCaption(string mensaje, string valorInicial)
        {
            using (var dlg = new Form())
            {
                dlg.Text = "Descripción de la foto";
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MinimizeBox = false;
                dlg.MaximizeBox = false;
                dlg.ClientSize = new Size(440, 130);

                var lbl = new Label
                {
                    Text = mensaje,
                    Location = new Point(12, 12),
                    AutoSize = true
                };
                var txt = new TextBox
                {
                    Location = new Point(12, 38),
                    Size = new Size(416, 24),
                    MaxLength = 300,
                    Text = valorInicial ?? ""
                };
                var btnOk = new Button
                {
                    Text = "Aceptar",
                    DialogResult = DialogResult.OK,
                    Location = new Point(266, 78),
                    Size = new Size(80, 30)
                };
                var btnCancel = new Button
                {
                    Text = "Cancelar",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(352, 78),
                    Size = new Size(80, 30)
                };

                dlg.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                return dlg.ShowDialog() == DialogResult.OK ? txt.Text.Trim() : null;
            }
        }

        private static string Truncar(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
        }

        private class MiembroVista
        {
            public string Nombre { get; set; }
            public string Rol { get; set; }
            public bool EsJefe { get; set; }
            public string Telefono { get; set; }
            public string Clave { get; set; }
        }

        private class FotoCard : Panel
        {
            private readonly int _id;
            private readonly byte[] _bytes;
            private string _caption;
            private readonly string _nombreArchivo;
            private readonly string _usuario;
            private readonly DateTime _fecha;
            private PictureBox _pb;
            private Label _lblCaption;
            private Label _lblMeta;

            public event Action<int, byte[], string> OnVer;
            public event Action<int> OnEliminar;
            public event Action<int, string> OnEditarCaption;

            public FotoCard(int id, byte[] bytes, string caption, string nombreArchivo, string usuario, DateTime fecha)
            {
                _id = id;
                _bytes = bytes;
                _caption = caption ?? "";
                _nombreArchivo = nombreArchivo ?? "";
                _usuario = usuario ?? "";
                _fecha = fecha;

                this.Size = new Size(180, 220);
                this.Margin = new Padding(8);
                this.BackColor = Color.White;
                this.BorderStyle = BorderStyle.FixedSingle;

                _pb = new PictureBox
                {
                    Location = new Point(4, 4),
                    Size = new Size(170, 130),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Black,
                    Cursor = Cursors.Hand
                };
                try
                {
                    using (var ms = new MemoryStream(_bytes))
                    {
                        _pb.Image = Image.FromStream(ms);
                    }
                }
                catch { }
                _pb.Click += (s, e) => OnVer?.Invoke(_id, _bytes, _caption);

                _lblCaption = new Label
                {
                    Location = new Point(4, 138),
                    Size = new Size(170, 32),
                    Text = string.IsNullOrEmpty(_caption) ? "(sin descripción)" : _caption,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = string.IsNullOrEmpty(_caption) ? Color.Gray : Color.Black,
                    AutoEllipsis = true
                };

                string meta = $"{_fecha:dd/MM/yyyy HH:mm}";
                if (!string.IsNullOrEmpty(_usuario)) meta += " · " + _usuario;
                _lblMeta = new Label
                {
                    Location = new Point(4, 170),
                    Size = new Size(170, 14),
                    Text = meta,
                    Font = new Font("Segoe UI", 7F, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    AutoEllipsis = true
                };

                var btnVer = new Button
                {
                    Text = "Ver",
                    Location = new Point(4, 188),
                    Size = new Size(50, 26),
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold)
                };
                btnVer.Click += (s, e) => OnVer?.Invoke(_id, _bytes, _caption);

                var btnEditar = new Button
                {
                    Text = "Editar",
                    Location = new Point(58, 188),
                    Size = new Size(60, 26),
                    BackColor = Color.FromArgb(241, 196, 15),
                    ForeColor = Color.Black,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold)
                };
                btnEditar.Click += (s, e) => OnEditarCaption?.Invoke(_id, _caption);

                var btnDel = new Button
                {
                    Text = "X",
                    Location = new Point(122, 188),
                    Size = new Size(52, 26),
                    BackColor = Color.FromArgb(192, 57, 43),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold)
                };
                btnDel.Click += (s, e) => OnEliminar?.Invoke(_id);

                this.Controls.AddRange(new Control[] { _pb, _lblCaption, _lblMeta, btnVer, btnEditar, btnDel });
            }

            public void LiberarImagen()
            {
                if (_pb?.Image != null)
                {
                    var img = _pb.Image;
                    _pb.Image = null;
                    img.Dispose();
                }
            }
        }
    }
}
