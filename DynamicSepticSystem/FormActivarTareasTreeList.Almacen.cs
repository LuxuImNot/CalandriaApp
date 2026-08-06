// Integracion del flujo de activacion de destajos con el ALMACEN:
//  - Al activar un destajo se "liberan" sus insumos Material desde el almacen
//    hacia la Manzana/Lote (SalidasAlmacen + HistorialMovimientos + vale).
//    Si no hay stock suficiente, se BLOQUEA la activacion.
//  - Al desactivar un destajo ya liberado se registra una EXCEPCION (con
//    justificacion); el stock NO se devuelve (los insumos ya salieron).
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DynamicSepticSystem
{
    public partial class FormActivarTareasTreeList
    {
        private SalidaAlmacenService _almacenSvc;
        private SalidaAlmacenService AlmacenSvc =>
            _almacenSvc ?? (_almacenSvc = new SalidaAlmacenService(connectionString));

        /// <summary>
        /// Si el destajo ya libero insumos, pide justificacion y registra una
        /// excepcion (sin devolver stock). Devuelve true si la desactivacion puede
        /// proceder, false si el usuario cancelo la justificacion.
        /// </summary>
        private bool RegistrarExcepcionDesactivacionSiLiberado(ItemTareaActivacion destajo)
        {
            if (destajo == null || destajo.Nivel != 1) return true;
            if (string.IsNullOrEmpty(manzanaActual) || string.IsNullOrEmpty(loteActual)) return true;

            var svc = AlmacenSvc;
            bool liberado;
            try { liberado = svc.YaLiberado(rutaActual, destajo.ID, manzanaActual, loteActual); }
            catch { liberado = false; }
            if (!liberado) return true;

            string justificacion;
            if (!PedirJustificacion(
                    $"Desactivar el destajo \"{destajo.Nombre}\" ya libero insumos del almacen.\n" +
                    "El stock NO se devolvera. Indica el motivo de la desactivacion:",
                    out justificacion))
                return false; // cancelado

            try
            {
                var insumos = svc.ObtenerInsumosLiberados(rutaActual, destajo.ID, manzanaActual, loteActual);
                decimal total = insumos.Sum(i => i.Importe);
                string detalleJson = JsonConvert.SerializeObject(insumos);
                svc.RegistrarExcepcion(
                    "DesactivacionDestajoLiberado", manzanaActual, loteActual, rutaActual,
                    destajo.ID, destajo.Nombre, Global.UsuarioActual?.Nombre ?? Environment.UserName,
                    justificacion, detalleJson, total);
            }
            catch (Exception ex)
            {
                // No bloquear la desactivacion por un fallo de bitacora.
                MessageBox.Show("No se pudo registrar la excepcion de desactivacion:\n\n" + ex.Message,
                    "Almacen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return true;
        }

        /// <summary>
        /// Pide una justificacion obligatoria mediante un dialogo simple.
        /// </summary>
        private bool PedirJustificacion(string mensaje, out string justificacion)
        {
            justificacion = null;
            using (var form = new Form())
            {
                form.Text = "Justificacion requerida";
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.ClientSize = new Size(460, 220);

                var lbl = new Label
                {
                    Text = mensaje,
                    Location = new Point(15, 12),
                    Size = new Size(430, 60),
                    Font = new Font("Segoe UI", 9F)
                };
                var txt = new TextBox
                {
                    Location = new Point(15, 78),
                    Size = new Size(430, 80),
                    Multiline = true,
                    Font = new Font("Segoe UI", 9F)
                };
                var btnOk = new Button
                {
                    Text = "Aceptar",
                    DialogResult = DialogResult.OK,
                    Location = new Point(250, 172),
                    Size = new Size(90, 32),
                    BackColor = GuiaPeligro,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                var btnCancel = new Button
                {
                    Text = "Cancelar",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(350, 172),
                    Size = new Size(95, 32),
                    Font = new Font("Segoe UI", 9F)
                };

                form.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                if (form.ShowDialog(this) != DialogResult.OK)
                    return false;

                justificacion = (txt.Text ?? "").Trim();
                if (string.IsNullOrWhiteSpace(justificacion))
                {
                    MessageBox.Show(
                        "La justificacion es obligatoria para desactivar un destajo ya liberado.",
                        "Justificacion requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                return true;
            }
        }
    }
}
