using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace DynamicSepticSystem
{
    public partial class FormRepositorioDestajos : Form
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;

        private readonly string manzana;
        private readonly string lote;
        private readonly string prototipo;
        private List<PdfInfo> pdfs = new List<PdfInfo>();

        public FormRepositorioDestajos(string manzana, string lote, string prototipo)
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            this.manzana = manzana;
            this.lote = lote;
            this.prototipo = prototipo ?? "";

            this.Load += FormRepositorioDestajos_Load;
        }

        private void FormRepositorioDestajos_Load(object sender, EventArgs e)
        {
            this.Text = $"Repositorio de PDFs - M{manzana} L{lote}";
            lblTitulo.Text = $"PDFs de Destajos · M{manzana} L{lote}";
            lblSubtitulo.Text = string.IsNullOrEmpty(prototipo)
                ? "Repositorio almacenado en base de datos"
                : $"Prototipo: {prototipo}  ·  Almacenamiento en BD";

            ConfigurarLista();
            CargarPdfs();
        }

        private void ConfigurarLista()
        {
            olvPdfs.FullRowSelect = true;
            olvPdfs.UseAlternatingBackColors = true;
            olvPdfs.AlternateRowBackColor = Color.FromArgb(248, 250, 253);
            olvPdfs.GridLines = false;
            olvPdfs.View = View.Details;
            olvPdfs.RowHeight = 28;
            olvPdfs.HeaderStyle = ColumnHeaderStyle.Nonclickable;

            olvPdfs.HeaderFormatStyle = new HeaderFormatStyle
            {
                Normal = new HeaderStateStyle
                {
                    BackColor = Color.FromArgb(41, 60, 88),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold)
                },
                Hot = new HeaderStateStyle
                {
                    BackColor = Color.FromArgb(52, 73, 105),
                    ForeColor = Color.White
                },
                Pressed = new HeaderStateStyle
                {
                    BackColor = Color.FromArgb(34, 49, 71),
                    ForeColor = Color.White
                }
            };

            var colId = new OLVColumn("ID", "Id")
            {
                Width = 60,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center
            };

            var colDestajo = new OLVColumn("Destajo", "NombreDestajo")
            {
                Width = 280,
                IsEditable = false,
                FillsFreeSpace = true
            };

            var colCuadrilla = new OLVColumn("Cuadrilla", "CuadrillaAsignada")
            {
                Width = 110,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Center,
                AspectGetter = o =>
                {
                    var p = o as PdfInfo;
                    return string.IsNullOrEmpty(p?.CuadrillaAsignada) ? "—" : p.CuadrillaAsignada;
                }
            };

            var colFecha = new OLVColumn("Fecha", "FechaGeneracion")
            {
                Width = 150,
                IsEditable = false,
                AspectToStringFormat = "{0:dd/MM/yyyy HH:mm}"
            };

            var colTamano = new OLVColumn("Tamaño", "TamanioBytes")
            {
                Width = 90,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                AspectToStringConverter = v => FormatearTamano((long)v)
            };

            var colArchivo = new OLVColumn("Archivo", "NombreArchivo")
            {
                Width = 220,
                IsEditable = false
            };

            olvPdfs.AllColumns.AddRange(new[] { colId, colDestajo, colCuadrilla, colFecha, colTamano, colArchivo });
            olvPdfs.RebuildColumns();

            olvPdfs.DoubleClick += (s, e) => AbrirSeleccionado();
            olvPdfs.SelectionChanged += (s, e) => ActualizarBotones();
        }

        private static string FormatearTamano(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            double kb = bytes / 1024.0;
            if (kb < 1024) return $"{kb:N1} KB";
            double mb = kb / 1024.0;
            return $"{mb:N2} MB";
        }

        private void CargarPdfs()
        {
            try
            {
                pdfs = new List<PdfInfo>();

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    FormActivarTareasTreeList.AsegurarTablaPDFsDestajos(conn);

                    const string sql = @"
                        SELECT Id, Manzana, Lote, Prototipo, Ruta, NodoID, NombreDestajo,
                               CuadrillaAsignada, NombreArchivo, TamanioBytes, Usuario, FechaGeneracion
                        FROM PDFsDestajos
                        WHERE Manzana = @m AND Lote = @l
                        ORDER BY FechaGeneracion DESC";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzana);
                        cmd.Parameters.AddWithValue("@l", lote);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pdfs.Add(new PdfInfo
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Manzana = reader["Manzana"].ToString(),
                                    Lote = reader["Lote"].ToString(),
                                    NombreDestajo = reader["NombreDestajo"] == DBNull.Value
                                        ? "" : reader["NombreDestajo"].ToString(),
                                    CuadrillaAsignada = reader["CuadrillaAsignada"] == DBNull.Value
                                        ? "" : reader["CuadrillaAsignada"].ToString(),
                                    NombreArchivo = reader["NombreArchivo"].ToString(),
                                    TamanioBytes = Convert.ToInt64(reader["TamanioBytes"]),
                                    FechaGeneracion = Convert.ToDateTime(reader["FechaGeneracion"])
                                });
                            }
                        }
                    }
                }

                olvPdfs.SetObjects(pdfs);
                ActualizarEstadisticas();
                ActualizarBotones();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer el repositorio:\n{ex.Message}",
                    "Repositorio", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarEstadisticas()
        {
            int total = pdfs.Count;
            long bytes = pdfs.Sum(p => p.TamanioBytes);
            DateTime? ultima = pdfs.Count > 0 ? pdfs.Max(p => (DateTime?)p.FechaGeneracion) : null;

            lblStatTotal.Text = total.ToString("N0", CultureInfo.CurrentCulture);
            lblStatTamano.Text = FormatearTamano(bytes);
            lblStatUltimo.Text = ultima.HasValue
                ? ultima.Value.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture)
                : "—";

            lblEstado.Text = total == 0
                ? "Aún no se han generado PDFs para esta casa."
                : $"{total} PDF(s) almacenados en la base de datos.";
        }

        private void ActualizarBotones()
        {
            bool hay = olvPdfs.SelectedObject is PdfInfo;
            btnAbrir.Enabled = hay;
            btnGuardarComo.Enabled = hay;
            btnEliminar.Enabled = hay;
        }

        private byte[] LeerContenidoPdf(int id)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT ContenidoPDF FROM PDFsDestajos WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    var obj = cmd.ExecuteScalar();
                    if (obj == null || obj == DBNull.Value) return null;
                    return (byte[])obj;
                }
            }
        }

        private void btnAbrir_Click(object sender, EventArgs e) => AbrirSeleccionado();

        private void AbrirSeleccionado()
        {
            var item = olvPdfs.SelectedObject as PdfInfo;
            if (item == null) return;

            try
            {
                byte[] bytes = LeerContenidoPdf(item.Id);
                if (bytes == null)
                {
                    MessageBox.Show("El PDF está vacío en la base de datos.",
                        "Abrir", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string tempPath = Path.Combine(Path.GetTempPath(), item.NombreArchivo);
                File.WriteAllBytes(tempPath, bytes);

                Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el PDF:\n{ex.Message}",
                    "Abrir", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGuardarComo_Click(object sender, EventArgs e)
        {
            var item = olvPdfs.SelectedObject as PdfInfo;
            if (item == null) return;

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = item.NombreArchivo;
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    byte[] bytes = LeerContenidoPdf(item.Id);
                    if (bytes == null) return;
                    File.WriteAllBytes(sfd.FileName, bytes);
                    MessageBox.Show($"Guardado en:\n{sfd.FileName}",
                        "Guardar copia", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo guardar la copia:\n{ex.Message}",
                        "Guardar copia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            var item = olvPdfs.SelectedObject as PdfInfo;
            if (item == null) return;

            var dr = MessageBox.Show(
                $"¿Eliminar permanentemente el PDF #{item.Id}?\n\n«{item.NombreArchivo}»",
                "Eliminar PDF", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr != DialogResult.Yes) return;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("DELETE FROM PDFsDestajos WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", item.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
                CargarPdfs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo eliminar:\n{ex.Message}",
                    "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e) => CargarPdfs();

        private void btnCerrar_Click(object sender, EventArgs e) => this.Close();

        private class PdfInfo
        {
            public int Id { get; set; }
            public string Manzana { get; set; }
            public string Lote { get; set; }
            public string NombreDestajo { get; set; }
            public string CuadrillaAsignada { get; set; }
            public string NombreArchivo { get; set; }
            public long TamanioBytes { get; set; }
            public DateTime FechaGeneracion { get; set; }
        }
    }
}
