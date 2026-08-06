// Flujo MANUAL de vale de salida en FormAlmacen. La generacion real (folio +
// PDF + repositorio) vive en SalidaAlmacenService para compartirse con la
// liberacion automatica al activar un destajo. Aqui solo queda la UI.
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class FormAlmacen : Form
    {
        /// <summary>
        /// Genera el PDF del vale de salida (flujo manual) usando el servicio compartido.
        /// </summary>
        private void GenerarValeSalidaPDF()
        {
            if (casaActual == null || casaInventario == null)
            {
                MessageBox.Show("Primero selecciona una casa destino.",
                    "Casa requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataTable dt = dgvInsumos.DataSource as DataTable;
            if (dt == null || dt.Columns.Count == 1)
            {
                MessageBox.Show("No hay insumos cargados para generar vale.",
                    "Sin insumos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Recolectar insumos con cantidad solicitada > 0
            var insumosSalida = new List<InsumoSalida>();
            decimal totalImporte = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (!dt.Columns.Contains("CantidadSolicitada") || row.IsNull("CantidadSolicitada"))
                    continue;

                decimal solicitada = Convert.ToDecimal(row["CantidadSolicitada"]);
                if (solicitada <= 0) continue;

                decimal precioUnitario = Convert.ToDecimal(row["PrecioUnitario"]);
                decimal importe = solicitada * precioUnitario;

                insumosSalida.Add(new InsumoSalida
                {
                    Codigo = row["Clave"].ToString(),
                    Descripcion = row["Descripcion"].ToString(),
                    Unidad = row["Unidad"].ToString(),
                    Cantidad = solicitada,
                    PrecioUnitario = precioUnitario,
                    Importe = importe
                });

                totalImporte += importe;
            }

            if (insumosSalida.Count == 0)
            {
                MessageBox.Show("No se capturo ninguna cantidad para generar el vale.",
                    "Sin cantidades", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Solicitar datos adicionales (UI)
                var datos = SolicitarDatosVale();
                if (datos == null) return; // Usuario cancelo

                var svc = new SalidaAlmacenService(connectionString);
                string folio;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    svc.CrearTablasRepositorioSalidasSiNoExisten(conn, null);
                    svc.EnsureEsquema(conn);

                    folio = svc.GenerarFolioSalida(casaActual.Manzana, casaActual.Lote, conn);
                    string rutaPdf = svc.GenerarPDFValeSalida(
                        folio, insumosSalida, totalImporte, datos,
                        casaActual.Manzana, casaActual.Lote, abrirPdf: true);

                    svc.GuardarValeSalidaEnRepositorio(
                        folio, rutaPdf, insumosSalida, totalImporte, datos,
                        casaActual.Manzana, casaActual.Lote, casaInventario?.Prototipo,
                        null, null, conn);
                }

                MessageBox.Show(
                    $"Vale de salida generado correctamente.\n\n" +
                    $"Folio: {folio}\n" +
                    $"Total insumos: {insumosSalida.Count}\n" +
                    $"Importe total: {totalImporte:C2}\n\n" +
                    $"El PDF se guardo en el repositorio.",
                    "Vale Generado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar vale de salida:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Solicita datos adicionales para el vale (solicitante, residente de obra, etc.)
        /// </summary>
        private DatosVale SolicitarDatosVale()
        {
            using (Form formDatos = new Form())
            {
                formDatos.Text = "Datos del Vale de Salida";
                formDatos.StartPosition = FormStartPosition.CenterParent;
                formDatos.Size = new Size(500, 280);
                formDatos.FormBorderStyle = FormBorderStyle.FixedDialog;
                formDatos.MaximizeBox = false;
                formDatos.MinimizeBox = false;

                int y = 20;

                Label lblSolicitante = new Label
                {
                    Text = "Solicitante:",
                    Location = new Point(20, y),
                    Size = new Size(100, 20),
                    Font = new Font("Segoe UI", 9F)
                };
                TextBox txtSolicitante = new TextBox
                {
                    Location = new Point(130, y),
                    Size = new Size(340, 25),
                    Font = new Font("Segoe UI", 9F)
                };
                y += 35;

                Label lblResidente = new Label
                {
                    Text = "Residente de Obra:",
                    Location = new Point(20, y),
                    Size = new Size(110, 20),
                    Font = new Font("Segoe UI", 9F)
                };
                TextBox txtResidente = new TextBox
                {
                    Location = new Point(130, y),
                    Size = new Size(340, 25),
                    Font = new Font("Segoe UI", 9F)
                };
                y += 35;

                Label lblEncargado = new Label
                {
                    Text = "Encargado de Almacen:",
                    Location = new Point(20, y),
                    Size = new Size(110, 20),
                    Font = new Font("Segoe UI", 9F)
                };
                TextBox txtEncargado = new TextBox
                {
                    Location = new Point(130, y),
                    Size = new Size(340, 25),
                    Font = new Font("Segoe UI", 9F),
                    Text = Global.UsuarioActual?.Nombre ?? Environment.UserName
                };
                y += 35;

                Label lblObservaciones = new Label
                {
                    Text = "Observaciones:",
                    Location = new Point(20, y),
                    Size = new Size(100, 20),
                    Font = new Font("Segoe UI", 9F)
                };
                TextBox txtObservaciones = new TextBox
                {
                    Location = new Point(130, y),
                    Size = new Size(340, 50),
                    Font = new Font("Segoe UI", 9F),
                    Multiline = true
                };
                y += 65;

                Button btnOk = new Button
                {
                    Text = "Generar Vale",
                    DialogResult = DialogResult.OK,
                    Location = new Point(260, y),
                    Size = new Size(110, 35),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    BackColor = ThemeManager.ColorExito,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                Button btnCancelar = new Button
                {
                    Text = "Cancelar",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(380, y),
                    Size = new Size(90, 35),
                    Font = new Font("Segoe UI", 9F)
                };

                formDatos.Controls.AddRange(new Control[] {
                    lblSolicitante, txtSolicitante,
                    lblResidente, txtResidente,
                    lblEncargado, txtEncargado,
                    lblObservaciones, txtObservaciones,
                    btnOk, btnCancelar
                });

                formDatos.AcceptButton = btnOk;
                formDatos.CancelButton = btnCancelar;

                if (formDatos.ShowDialog() == DialogResult.OK)
                {
                    return new DatosVale
                    {
                        Solicitante = txtSolicitante.Text.Trim(),
                        ResidenteObra = txtResidente.Text.Trim(),
                        EncargadoAlmacen = txtEncargado.Text.Trim(),
                        Observaciones = txtObservaciones.Text.Trim()
                    };
                }

                return null;
            }
        }

        /// <summary>
        /// Abre el repositorio de vales de salida
        /// </summary>
        public void AbrirRepositorioValesSalida(string manzana = null, string lote = null)
        {
            try
            {
                using (var formRepo = new FormRepositorioValesSalida(manzana, lote))
                {
                    formRepo.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir repositorio: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Clases auxiliares (compartidas con SalidaAlmacenService)
    public class InsumoSalida
    {
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
    }

    public class DatosVale
    {
        public string Solicitante { get; set; }
        public string ResidenteObra { get; set; }
        public string EncargadoAlmacen { get; set; }
        public string Observaciones { get; set; }
    }
}
