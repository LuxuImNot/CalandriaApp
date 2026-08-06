// UI de tarjetas para la pestana ENTRADAS (consistente con SALIDAS). La grilla
// dgvEntrada se conserva OCULTA como modelo (DataSource = DataTable) que leen
// BtnCapturarEntrada_Click y la conciliacion; estas tarjetas leen/escriben en esa
// misma tabla. Reutiliza AgregarMiniDato de FormAlmacen_Salidas (misma clase parcial).
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class FormAlmacen : Form
    {
        private FlowLayoutPanel flpEntradas;
        private Panel pnlHeaderEntradas;
        private Label lblHeaderEntradas;
        private string _resumenEntrada = "";
        private int _anchoUltimoRenderEntrada = -1;

        // Resultado de la ultima conciliacion (por Id de detalle), para etiquetar tarjetas.
        private Dictionary<int, LineaConciliacionApi> _conciliacionPorId;

        // Paleta
        private static readonly Color EntAzul = Color.FromArgb(41, 128, 185);   // pendiente
        private static readonly Color EntVerde = Color.FromArgb(39, 174, 96);   // cuadra
        private static readonly Color EntAmbar = Color.FromArgb(211, 132, 0);   // diferencia/parcial
        private static readonly Color EntGris = Color.FromArgb(127, 140, 141);
        private static readonly Color EntTinta = Color.FromArgb(44, 62, 80);

        /// <summary>Redibuja las tarjetas de entrada desde el DataTable (modelo de dgvEntrada).</summary>
        private void RenderTarjetasEntrada(string filtro = null)
        {
            if (flpEntradas == null) return;
            _anchoUltimoRenderEntrada = flpEntradas.ClientSize.Width;

            flpEntradas.SuspendLayout();
            foreach (Control c in flpEntradas.Controls.Cast<Control>().ToList()) c.Dispose();
            flpEntradas.Controls.Clear();

            if (lblHeaderEntradas != null)
                lblHeaderEntradas.Text = string.IsNullOrEmpty(_resumenEntrada)
                    ? "Selecciona una orden de compra y captura las cantidades recibidas."
                    : _resumenEntrada;

            var dt = dgvEntrada.DataSource as DataTable;
            if (dt == null || dt.Rows.Count == 0 || !dt.Columns.Contains("CantidadRecibida"))
            {
                flpEntradas.Controls.Add(CrearTarjetaVaciaEntrada(null));
                flpEntradas.ResumeLayout();
                return;
            }

            string f = (filtro ?? "").Trim();
            int mostradas = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (dt.Columns.Contains("Estado") &&
                    string.Equals(row["Estado"]?.ToString(), "ELIMINADO", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (f.Length > 0)
                {
                    string hay = (row["Clave"]?.ToString() ?? "") + " " + (row["Descripcion"]?.ToString() ?? "");
                    if (hay.IndexOf(f, StringComparison.OrdinalIgnoreCase) < 0) continue;
                }
                flpEntradas.Controls.Add(CrearTarjetaEntrada(row));
                mostradas++;
            }
            if (mostradas == 0)
                flpEntradas.Controls.Add(CrearTarjetaVaciaEntrada("Sin coincidencias para la busqueda."));

            flpEntradas.ResumeLayout();
        }

        private Control CrearTarjetaVaciaEntrada(string msg)
        {
            var p = new Panel
            {
                Width = 460,
                Height = 96,
                BackColor = Color.White,
                Margin = new Padding(8),
                BorderStyle = BorderStyle.FixedSingle
            };
            p.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                ForeColor = EntGris,
                Padding = new Padding(16),
                Text = msg ?? "Busca una orden de compra (BUSCAR ORDEN) para capturar su entrada."
            });
            return p;
        }

        private Control CrearTarjetaEntrada(DataRow row)
        {
            int id = row.Table.Columns.Contains("Id") && row["Id"] != DBNull.Value
                ? Convert.ToInt32(row["Id"]) : 0;
            string clave = row["Clave"]?.ToString() ?? "";
            string insumo = row["Descripcion"]?.ToString() ?? "";
            string unidad = row["Unidad"]?.ToString() ?? "";
            decimal comprada = row.Table.Columns.Contains("CantidadComprada") && row["CantidadComprada"] != DBNull.Value
                ? Convert.ToDecimal(row["CantidadComprada"]) : 0m;
            decimal recibida = LeerRecibida(row);

            LineaConciliacionApi conc = null;
            if (_conciliacionPorId != null && id != 0) _conciliacionPorId.TryGetValue(id, out conc);

            var card = new Panel
            {
                Width = 360,
                Height = 168,
                Margin = new Padding(8),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            var accent = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = ColorEstadoEntrada(recibida, comprada) };
            card.Controls.Add(accent);

            var content = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            card.Controls.Add(content);
            content.BringToFront();

            // Estado (arriba) + boton eliminar (x)
            var lblEstado = new Label
            {
                AutoSize = false, Location = new Point(12, 8), Size = new Size(290, 16),
                Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
                ForeColor = accent.BackColor, AutoEllipsis = true,
                Text = TextoEstadoEntrada(recibida, comprada)
            };
            content.Controls.Add(lblEstado);

            var btnQuitar = new Label
            {
                Text = "✕", AutoSize = false, Size = new Size(22, 20), Location = new Point(312, 6),
                TextAlign = ContentAlignment.MiddleCenter, ForeColor = EntGris,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand
            };
            new ToolTip().SetToolTip(btnQuitar, "Quitar esta partida de la orden");
            if (id != 0)
                btnQuitar.Click += (s, e) => EliminarPartidaDeOrden(id, clave);
            content.Controls.Add(btnQuitar);

            content.Controls.Add(new Label
            {
                AutoSize = false, Location = new Point(12, 26), Size = new Size(322, 36),
                Text = string.IsNullOrEmpty(insumo) ? "(sin nombre)" : insumo,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold), ForeColor = EntTinta, AutoEllipsis = true
            });
            content.Controls.Add(new Label
            {
                AutoSize = false, Location = new Point(12, 62), Size = new Size(322, 16),
                Text = $"Codigo: {(string.IsNullOrEmpty(clave) ? "— sin clave —" : clave)}   ·   Unidad: {(string.IsNullOrEmpty(unidad) ? "-" : unidad)}",
                Font = new Font("Segoe UI", 8.25F), ForeColor = EntGris, AutoEllipsis = true
            });

            AgregarMiniDato(content, 12, 84, "COMPRADO", comprada.ToString("N2"), EntTinta);

            // Etiqueta de conciliacion (si la hubo)
            if (conc != null)
            {
                string txt = conc.Origen == "SinMatch"
                    ? "factura: no encontrado"
                    : $"factura: {conc.CantidadFacturada:N2}";
                content.Controls.Add(new Label
                {
                    AutoSize = false, Location = new Point(124, 84), Size = new Size(210, 28),
                    Text = txt, Font = new Font("Segoe UI", 8F),
                    ForeColor = conc.Origen == "SinMatch" ? EntGris : EntAmbar
                });
            }

            // Captura: NumericUpDown de cantidad recibida + delta en vivo
            content.Controls.Add(new Label
            {
                AutoSize = true, Location = new Point(12, 130), Text = "Recibido:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = EntTinta
            });

            var nud = new NumericUpDown
            {
                Location = new Point(76, 127), Width = 92,
                DecimalPlaces = 2, Minimum = 0m, Maximum = 1000000m, Increment = 1m,
                Value = Math.Max(0m, Math.Min(recibida, 1000000m)),
                Font = new Font("Segoe UI", 10F), BackColor = Color.FromArgb(255, 255, 220),
                TextAlign = HorizontalAlignment.Right
            };
            var lblDelta = new Label
            {
                AutoSize = false, Location = new Point(176, 130), Size = new Size(160, 22),
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            ActualizarDelta(lblDelta, nud.Value, comprada);

            nud.ValueChanged += (s, e) =>
            {
                row["CantidadRecibida"] = (double)nud.Value;
                accent.BackColor = ColorEstadoEntrada(nud.Value, comprada);
                lblEstado.ForeColor = accent.BackColor;
                lblEstado.Text = TextoEstadoEntrada(nud.Value, comprada);
                ActualizarDelta(lblDelta, nud.Value, comprada);
            };
            content.Controls.Add(nud);
            content.Controls.Add(lblDelta);

            return card;
        }

        private static decimal LeerRecibida(DataRow row)
        {
            if (!row.Table.Columns.Contains("CantidadRecibida") || row["CantidadRecibida"] == DBNull.Value)
                return 0m;
            return decimal.TryParse(row["CantidadRecibida"].ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out var v)
                ? v : 0m;
        }

        private static Color ColorEstadoEntrada(decimal recibida, decimal comprada)
        {
            if (recibida <= 0m) return EntAzul;           // pendiente
            if (recibida == comprada) return EntVerde;    // cuadra
            return EntAmbar;                               // diferencia (parcial/excepcion)
        }

        private static string TextoEstadoEntrada(decimal recibida, decimal comprada)
        {
            if (recibida <= 0m) return "PENDIENTE";
            if (recibida == comprada) return "✓ COMPLETO";
            if (recibida < comprada) return $"PARCIAL · faltan {(comprada - recibida):N2}";
            return $"EXCEDENTE · +{(recibida - comprada):N2}";
        }

        private void ActualizarDelta(Label lbl, decimal recibida, decimal comprada)
        {
            decimal d = recibida - comprada;
            if (recibida <= 0m) { lbl.Text = ""; return; }
            if (d == 0m) { lbl.Text = "= cuadra"; lbl.ForeColor = EntVerde; }
            else { lbl.Text = (d > 0 ? "+" : "") + d.ToString("N2") + " vs comprado"; lbl.ForeColor = EntAmbar; }
        }

        /// <summary>Marca una partida como ELIMINADA (con justificacion) y refresca.</summary>
        private void EliminarPartidaDeOrden(int id, string clave)
        {
            string justificacion = PromptJustificacion.Pedir(this,
                "Eliminar insumo de la orden",
                $"Vas a eliminar el insumo {clave} de la orden de compra.",
                "Indica el motivo de la eliminacion.", peligro: true);
            if (string.IsNullOrWhiteSpace(justificacion))
            {
                MessageBox.Show("Justificacion obligatoria. No se elimino nada.", "Cancelado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(
                    "UPDATE OrdenesCompraDetalle SET Estado = 'ELIMINADO', Justificacion = @j WHERE Id = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@j", justificacion);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo eliminar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            CargarInsumosDesdeOrdenSeleccionada(); // recarga el detalle pendiente y re-renderiza
        }
    }
}
