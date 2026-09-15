// Theme and UI styling for FormAlmacen
// Aplica el sistema corporativo Calandria (ThemeManager) y agrega refinamientos
// específicos del módulo de almacén: badges de estado, variantes de botón,
// tipografía jerárquica y DataGridView pulido.
using System;
using System.Drawing;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace DynamicSepticSystem
{
    public partial class FormAlmacen : Form
    {
        /// <summary>
        /// Aplica el tema corporativo de Calandria al formulario, partiendo del
        /// sistema global de ThemeManager y agregando capas específicas del módulo.
        /// </summary>
        private void AplicarTemaCorporativo()
        {
            // 1) Base corporativa global (paleta, fuentes, double buffer, tabs, etc.)
            this.SuspendLayout();
            this.BackColor = ThemeManager.ColorFondoApp;
            this.ForeColor = ThemeManager.ColorTextoOscuro;
            this.Font = ThemeManager.FuenteRegular;
            this.Text = "Gestión de Almacén · Pilaris";

            // 2) TabControl con look corporativo (acento café en selección)
            if (TabsControl != null)
            {
                ThemeManager.AplicarTemaControl(TabsControl);
                TabsControl.BackColor = ThemeManager.ColorFondoApp;
                foreach (TabPage tab in TabsControl.TabPages)
                {
                    tab.BackColor = ThemeManager.ColorFondoApp;
                    tab.ForeColor = ThemeManager.ColorTextoOscuro;
                    tab.Padding = new Padding(16);
                }
            }

            // 3) Botones — variantes según rol
            EstilizarBotonAccionPrimaria(btnCapturarEntrada, "CAPTURAR ENTRADA");
            EstilizarBotonAccionPrimaria(btnRegistrarSalida, "CAPTURAR SALIDA");

            EstilizarBotonOutlineSeguro(btnBuscarOrden, "BUSCAR ORDEN");
            EstilizarBotonOutlineSeguro(btnSeleccionarCasaSalida, "SELECCIONAR CASA");
            EstilizarBotonOutlineSeguro(btnActualizarInventario, "ACTUALIZAR");
            EstilizarBotonOutlineSeguro(btnActualizarConsultaCasa, "ACTUALIZAR");

            EstilizarBotonExitoCompacto(btnExportarInventario, "EXPORTAR A EXCEL");
            EstilizarBotonExitoCompacto(btnExportarConsultaCasa, "EXPORTAR A EXCEL");

            EstilizarBotonPeligroCompacto(btnEliminardeOrden, "ELIMINAR SELECCIONADOS");

            // Ver vales: acento secundario (info)
            if (btnVerRepositorioVales != null)
            {
                btnVerRepositorioVales.Tag = "secundario";
                btnVerRepositorioVales.FlatStyle = FlatStyle.Flat;
                btnVerRepositorioVales.FlatAppearance.BorderSize = 0;
                btnVerRepositorioVales.BackColor = ThemeManager.ColorInfo;
                btnVerRepositorioVales.ForeColor = ThemeManager.ColorTextoClaro;
                btnVerRepositorioVales.FlatAppearance.MouseOverBackColor = Color.FromArgb(62, 145, 207);
                btnVerRepositorioVales.FlatAppearance.MouseDownBackColor = Color.FromArgb(34, 109, 168);
                btnVerRepositorioVales.Cursor = Cursors.Hand;
                btnVerRepositorioVales.Font = ThemeManager.FuenteBotones;
                btnVerRepositorioVales.Text = "VER VALES DE SALIDA";
                btnVerRepositorioVales.UseVisualStyleBackColor = false;
            }

            // 4) Labels de sección y encabezados
            EstilizarLabelSeccion(label3);            // "Orden de compra"
            EstilizarLabelSeccion(label6);            // "Buscar en orden"
            EstilizarLabelSeccion(label7);            // "Buscar insumo"
            EstilizarLabelSeccion(lblInstrucciones);  // "Selecciona casa destino"
            EstilizarLabelSeccion(lblBuscarInventario);
            EstilizarLabelSeccion(lblBuscarConsultaCasa);
            EstilizarLabelSeccion(lblManzanaConsulta);
            EstilizarLabelSeccion(lblLoteConsulta);

            EstilizarLabelCampo(label1); // "Lote"
            EstilizarLabelCampo(label2); // "Manzana"

            // 5) Badges de "casa seleccionada"
            EstilizarLabelBadge(lblCasa, "Sin casa seleccionada");
            EstilizarLabelBadge(lblCasaSalida, "Sin casa seleccionada");
            EstilizarLabelBadge(lblCasaSeleccionada, "Seleccione una casa para ver sus insumos");

            // 6) Labels métricos del inventario y consulta
            EstilizarLabelMetrico(lblTotalInventario);
            EstilizarLabelMetrico(lblTotalInsumosConsulta);
            EstilizarLabelMetrico(lblTotalImporteConsulta);
            EstilizarLabelMetrico(lblPrototipoConsulta);
            EstilizarLabelMetrico(lblSummary);
            if (lblDiscrepanciasConsulta != null)
            {
                lblDiscrepanciasConsulta.Font = ThemeManager.FuenteSemi;
                lblDiscrepanciasConsulta.ForeColor = ThemeManager.ColorError;
                lblDiscrepanciasConsulta.BackColor = Color.Transparent;
            }

            // 7) Inputs (TextBox / ComboBox)
            EstilizarInputTexto(txtBuscarEntrada);
            EstilizarInputTexto(txtBuscarSalida);
            EstilizarInputTexto(txtBuscarInventario);
            EstilizarInputTexto(txtBuscarConsultaCasa);
            EstilizarInputTexto(txtTrimManzanaSalida);
            EstilizarInputTexto(txtTrimLoteSalida);
            EstilizarInputCombo(cmbOrdenesCompra);
            EstilizarInputCombo(cmbManzanaConsulta);
            EstilizarInputCombo(cmbLoteConsulta);

            // 8) Grids
            EstilizarDataGridViewAlmacen(dgvEntrada);
            EstilizarDataGridViewAlmacen(dgvInsumos);
            EstilizarDataGridViewAlmacen(dgvInventario);
            EstilizarDataGridViewAlmacen(dgvConsultaCasa);

            // 9) ObjectListView (Historial) — alineado con el tema
            if (objectListViewHistorial != null)
            {
                objectListViewHistorial.BackColor = ThemeManager.ColorFondo;
                objectListViewHistorial.ForeColor = ThemeManager.ColorTextoOscuro;
                objectListViewHistorial.Font = ThemeManager.FuenteGrilla;
                objectListViewHistorial.GridLines = false;
                objectListViewHistorial.UseAlternatingBackColors = true;
                objectListViewHistorial.AlternateRowBackColor = ThemeManager.ColorFondoAlterno;
                objectListViewHistorial.FullRowSelect = true;
                objectListViewHistorial.HeaderStyle = ColumnHeaderStyle.Nonclickable;
                objectListViewHistorial.BorderStyle = BorderStyle.None;
                objectListViewHistorial.HighlightBackgroundColor = Color.FromArgb(232, 220, 200);
                objectListViewHistorial.HighlightForegroundColor = ThemeManager.ColorPrincipalOscuro;
                ThemeManager.HabilitarDoubleBuffer(objectListViewHistorial);
            }

            // 10) FlowLayout de casas asociadas
            if (flwCasasAsociadas != null)
            {
                flwCasasAsociadas.BackColor = ThemeManager.ColorFondoAlterno;
                flwCasasAsociadas.BorderStyle = BorderStyle.None;
                flwCasasAsociadas.Padding = new Padding(6);
                ThemeManager.HabilitarDoubleBuffer(flwCasasAsociadas);
            }

            this.ResumeLayout(false);
        }

        // ===================================================================
        // VARIANTES DE BOTÓN — ENVOLTORIOS LOCALES
        // ===================================================================
        private static void EstilizarBotonAccionPrimaria(Button btn, string texto)
        {
            if (btn == null) return;
            ThemeManager.EstilizarBotonExito(btn);
            btn.Font = new Font(ThemeManager.FuenteBotones.FontFamily, 10.5F, FontStyle.Bold);
            btn.Text = texto;
            btn.Height = Math.Max(btn.Height, 46);
            btn.TextAlign = ContentAlignment.MiddleCenter;
            btn.ImageAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(8, 0, 8, 0);
        }

        private static void EstilizarBotonOutlineSeguro(Button btn, string texto)
        {
            if (btn == null) return;
            ThemeManager.EstilizarBotonOutline(btn);
            btn.Font = ThemeManager.FuenteBotones;
            btn.Text = texto;
            btn.Height = Math.Max(btn.Height, 36);
            btn.TextAlign = ContentAlignment.MiddleCenter;
            btn.Padding = new Padding(6, 0, 6, 0);
        }

        private static void EstilizarBotonExitoCompacto(Button btn, string texto)
        {
            if (btn == null) return;
            ThemeManager.EstilizarBotonExito(btn);
            btn.Font = ThemeManager.FuenteBotones;
            btn.Text = texto;
            btn.Height = Math.Max(btn.Height, 40);
            btn.TextAlign = ContentAlignment.MiddleCenter;
        }

        private static void EstilizarBotonPeligroCompacto(Button btn, string texto)
        {
            if (btn == null) return;
            ThemeManager.EstilizarBotonPeligro(btn);
            btn.Font = ThemeManager.FuenteBotones;
            btn.Text = texto;
            btn.Height = Math.Max(btn.Height, 38);
            btn.TextAlign = ContentAlignment.MiddleCenter;
        }

        // ===================================================================
        // VARIANTES DE LABEL
        // ===================================================================
        private static void EstilizarLabelSeccion(Label lbl)
        {
            if (lbl == null) return;
            lbl.Font = new Font(ThemeManager.FuenteSemi.FontFamily, 8.5F, FontStyle.Bold);
            lbl.ForeColor = ThemeManager.ColorTextoSecundario;
            lbl.BackColor = Color.Transparent;
            lbl.AutoSize = true;
            lbl.Padding = new Padding(0, 2, 0, 2);
        }

        private static void EstilizarLabelCampo(Label lbl)
        {
            if (lbl == null) return;
            lbl.Font = new Font(ThemeManager.FuenteSemi.FontFamily, 8.5F, FontStyle.Bold);
            lbl.ForeColor = ThemeManager.ColorTextoSecundario;
            lbl.BackColor = Color.Transparent;
            lbl.AutoSize = true;
        }

        private static void EstilizarLabelMetrico(Label lbl)
        {
            if (lbl == null) return;
            lbl.Font = ThemeManager.FuenteSubtitulo;
            lbl.ForeColor = ThemeManager.ColorPrincipalOscuro;
            lbl.BackColor = Color.Transparent;
            lbl.AutoSize = true;
        }

        /// <summary>
        /// Convierte un Label en badge informativo (fondo suave + texto oscuro).
        /// Inicia en estado "neutral"; <see cref="ActualizarBadgeCasa"/> cambia el color
        /// según haya selección o no.
        /// </summary>
        private static void EstilizarLabelBadge(Label lbl, string textoInicial)
        {
            if (lbl == null) return;
            lbl.AutoSize = false;
            lbl.Height = 32;
            lbl.MinimumSize = new Size(0, 32);
            lbl.Font = ThemeManager.FuenteSemi;
            lbl.TextAlign = ContentAlignment.MiddleLeft;
            lbl.Padding = new Padding(12, 0, 12, 0);
            lbl.Text = string.IsNullOrWhiteSpace(lbl.Text) ? textoInicial : lbl.Text;
            ActualizarBadgeCasa(lbl, false);
        }

        /// <summary>
        /// Cambia el estilo del badge según si hay casa seleccionada o no.
        /// </summary>
        public static void ActualizarBadgeCasa(Label badge, bool seleccionada)
        {
            if (badge == null) return;
            if (seleccionada)
            {
                badge.BackColor = ThemeManager.ColorExitoSuave;
                badge.ForeColor = ThemeManager.ColorExito;
            }
            else
            {
                badge.BackColor = ThemeManager.ColorFondoAlterno;
                badge.ForeColor = ThemeManager.ColorTextoSecundario;
            }
        }

        // ===================================================================
        // INPUTS
        // ===================================================================
        private static void EstilizarInputTexto(TextBox txt)
        {
            if (txt == null) return;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.BackColor = ThemeManager.ColorFondo;
            txt.ForeColor = ThemeManager.ColorTextoOscuro;
            txt.Font = ThemeManager.FuenteRegular;
            // Altura mínima para sensación más profesional
            try { txt.MinimumSize = new Size(0, 26); } catch { }
        }

        private static void EstilizarInputCombo(ComboBox cbo)
        {
            if (cbo == null) return;
            cbo.BackColor = ThemeManager.ColorFondo;
            cbo.ForeColor = ThemeManager.ColorTextoOscuro;
            cbo.FlatStyle = FlatStyle.Flat;
            cbo.Font = ThemeManager.FuenteRegular;
        }

        // ===================================================================
        // DATAGRIDVIEW — wrapper compatible con código existente
        // ===================================================================
        /// <summary>
        /// Estiliza un DataGridView con el tema corporativo. Conserva la firma
        /// que ya usan FormAlmacen_Inventario y FormAlmacen_ConsultaCasa.
        /// </summary>
        private void EstilizarDataGridViewAlmacen(DataGridView dgv)
        {
            if (dgv == null) return;
            ThemeManager.AplicarTemaControl(dgv);
            // El estilo corporativo deja MultiSelect=false; el almacén requiere
            // selección múltiple para acciones de lote (eliminar/registrar varios).
            dgv.MultiSelect = true;
        }

        // ===================================================================
        // PINTADO CONDICIONAL DE FILAS — ENTRADAS
        // ===================================================================
        private void dgvEntrada_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvEntrada.Rows.Count) return;
            var row = dgvEntrada.Rows[e.RowIndex];
            if (!dgvEntrada.Columns.Contains("Estado")) return;

            var estado = row.Cells["Estado"].Value?.ToString();
            switch (estado)
            {
                case "RECIBIDO":
                    row.DefaultCellStyle.BackColor = ThemeManager.ColorExitoSuave;
                    row.DefaultCellStyle.ForeColor = ThemeManager.ColorTextoOscuro;
                    break;
                case "ELIMINADO":
                    row.DefaultCellStyle.BackColor = ThemeManager.ColorErrorSuave;
                    row.DefaultCellStyle.ForeColor = ThemeManager.ColorTextoOscuro;
                    break;
                case "PENDIENTE":
                    row.DefaultCellStyle.BackColor = ThemeManager.ColorFondo;
                    row.DefaultCellStyle.ForeColor = ThemeManager.ColorTextoOscuro;
                    break;
            }
        }
    }
}
