using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Consulta por Casa (Dinero): ventana dedicada que hospeda en WebView2 la
    /// página Calandria.Api/ui/administrativos.html (Estimaciones, Destajos,
    /// Compras, Salidas de Almacén y Nómina detallada de una casa) — ver
    /// FormAdministrativos.Web.cs, mismo patrón que FormPerfilesWeb.cs /
    /// FormTrabajadoresWeb.cs. Ya no tiene interfaz WinForms clásica: la
    /// migración se completó y no hay panel al que volver si WebView2 falla.
    ///
    /// Lo que queda en este archivo es lo que el puente web todavía necesita:
    /// los campos de estado de la última consulta y la generación del PDF con
    /// PdfSharp (que no depende de WinForms, sólo dibuja lo que ya está en
    /// memoria — ver FormAdministrativos.Web.cs § Exportar PDF).
    /// </summary>
    public partial class FormAdministrativos : Form
    {
        private readonly CultureInfo culturaMx = CultureInfo.GetCultureInfo("es-MX");

        private string manzanaActual = "";
        private string loteActual = "";
        private string prototipoActual = "";

        private readonly List<RegistroEstimacion> registrosEstimacion = new List<RegistroEstimacion>();
        private readonly List<RegistroDestajo> registrosDestajo = new List<RegistroDestajo>();

        private decimal totalEstimaciones = 0m;
        private decimal totalManoObra = 0m;
        private decimal totalMaterial = 0m;
        private int conteoMO = 0;
        private int conteoMat = 0;

        public FormAdministrativos()
        {
            Text = "Administrativos — Consulta por Casa";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1200, 702);
            MinimumSize = new Size(1000, 650);
            WindowState = FormWindowState.Maximized;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);
            ThemeManager.AplicarTema(this);
            InicializarPanelWeb();
        }

        #region Exportación PDF

        private string TipoTareaTexto(TipoTarea t)
        {
            switch (t)
            {
                case TipoTarea.ManoDeObra: return "M. de Obra";
                case TipoTarea.Material: return "Material";
                default: return "—";
            }
        }

        private void GenerarPDF(string rutaArchivo,
            List<CompraCasaApi> comprasExtra = null,
            List<SalidaAlmacenCasaApi> salidasExtra = null,
            List<NominaCasaApi> nominaExtra = null)
        {
            using (var pdf = new PdfDocument())
            {
                pdf.Info.Title = $"Administrativos — Mz {manzanaActual} Lt {loteActual}";
                pdf.Info.Author = "Sistema Pilaris";
                pdf.Info.Subject = "Concentrado financiero por casa";
                pdf.Info.Creator = "DynamicSepticSystem";

                var page = pdf.AddPage();
                page.Size = PdfSharp.PageSize.Letter;
                page.Orientation = PdfSharp.PageOrientation.Landscape;
                var gfx = XGraphics.FromPdfPage(page);

                var fontTitulo = new XFont("Arial", 16, XFontStyle.Bold);
                var fontSub = new XFont("Arial", 10, XFontStyle.Regular);
                var fontSeccion = new XFont("Arial", 12, XFontStyle.Bold);
                var fontHeader = new XFont("Arial", 9, XFontStyle.Bold);
                var fontCell = new XFont("Arial", 8, XFontStyle.Regular);
                var fontGrupo = new XFont("Arial", 9.5, XFontStyle.Bold);
                var fontTotal = new XFont("Arial", 11, XFontStyle.Bold);

                var brushCafe = new XSolidBrush(XColor.FromArgb(88, 53, 23));
                var brushCafeBar = new XSolidBrush(XColor.FromArgb(179, 108, 46));
                var brushBlanco = XBrushes.White;
                var brushTexto = XBrushes.Black;
                var brushVerde = new XSolidBrush(XColor.FromArgb(46, 134, 75));
                var brushRojo = new XSolidBrush(XColor.FromArgb(155, 41, 28));
                var brushAlt = new XSolidBrush(XColor.FromArgb(250, 247, 242));
                var brushGrupo = new XSolidBrush(XColor.FromArgb(235, 222, 200));

                double margin = 30;
                double pageW = page.Width - 2 * margin;
                double y = margin;

                // Header
                gfx.DrawRectangle(brushCafe, margin, y, pageW, 50);
                gfx.DrawString("CONCENTRADO ADMINISTRATIVO", fontTitulo, brushBlanco,
                    new XRect(margin + 10, y + 6, pageW - 20, 22), XStringFormats.TopLeft);
                gfx.DrawString($"Mz {manzanaActual} · Lt {loteActual} · Prototipo: {(string.IsNullOrEmpty(prototipoActual) ? "—" : prototipoActual)}",
                    fontSub, brushBlanco,
                    new XRect(margin + 10, y + 28, pageW - 20, 18), XStringFormats.TopLeft);
                gfx.DrawString($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}",
                    fontSub, brushBlanco,
                    new XRect(margin, y + 28, pageW - 10, 18), XStringFormats.TopRight);
                y += 60;

                // ====== Resumen tipo HUD en el PDF ======
                double cardW = (pageW - 20) / 3.0;
                DibujarTarjetaPDF(gfx, margin, y, cardW, "ESTIMACIONES", totalEstimaciones.ToString("C2", culturaMx),
                    $"{registrosEstimacion.Count} concepto(s)", brushCafe, brushVerde, fontHeader, fontGrupo, fontCell);
                DibujarTarjetaPDF(gfx, margin + cardW + 10, y, cardW, "DESTAJOS · MANO DE OBRA", totalManoObra.ToString("C2", culturaMx),
                    $"{conteoMO} destajo(s)", brushCafe, brushVerde, fontHeader, fontGrupo, fontCell);
                DibujarTarjetaPDF(gfx, margin + 2 * (cardW + 10), y, cardW, "DESTAJOS · MATERIAL", totalMaterial.ToString("C2", culturaMx),
                    $"{conteoMat} destajo(s)", brushCafe, brushRojo, fontHeader, fontGrupo, fontCell);
                y += 70;

                // ============ ESTIMACIONES agrupado ============
                gfx.DrawRectangle(brushCafeBar, margin, y, pageW, 24);
                gfx.DrawString("ESTIMACIONES — Agrupadas por Etapa", fontSeccion, brushBlanco,
                    new XRect(margin + 8, y + 4, pageW - 16, 18), XStringFormats.TopLeft);
                y += 24;

                double[] anchoEst = { 70, 90, 360, 90, 130, 90 };
                string[] hdrEst = { "WBS", "Código", "Partida", "Avance %", "Monto Ejec.", "Fecha" };
                y = DibujarFilaTabla(gfx, margin, y, anchoEst, hdrEst, fontHeader, brushCafe, brushBlanco, true);

                var grupos = registrosEstimacion
                    .GroupBy(r => string.IsNullOrEmpty(r.Etapa) ? "(Sin etapa)" : r.Etapa)
                    .OrderBy(g => g.Key);

                foreach (var grupo in grupos)
                {
                    if (y > page.Height - 100)
                    {
                        page = pdf.AddPage();
                        page.Size = PdfSharp.PageSize.Letter;
                        page.Orientation = PdfSharp.PageOrientation.Landscape;
                        gfx.Dispose();
                        gfx = XGraphics.FromPdfPage(page);
                        y = margin;
                        y = DibujarFilaTabla(gfx, margin, y, anchoEst, hdrEst, fontHeader, brushCafe, brushBlanco, true);
                    }

                    decimal subtotal = grupo.Sum(r => r.MontoEjecutado);
                    gfx.DrawRectangle(brushGrupo, margin, y, pageW, 18);
                    gfx.DrawString($"  {grupo.Key}   ·   {grupo.Count()} concepto(s)",
                        fontGrupo, brushCafe,
                        new XRect(margin + 4, y + 1, pageW - 200, 16), XStringFormats.CenterLeft);
                    gfx.DrawString($"Subtotal: {subtotal.ToString("C2", culturaMx)}",
                        fontGrupo, brushVerde,
                        new XRect(margin, y + 1, pageW - 6, 16), XStringFormats.CenterRight);
                    y += 18;

                    int fila = 0;
                    foreach (var r in grupo)
                    {
                        if (y > page.Height - 60)
                        {
                            page = pdf.AddPage();
                            page.Size = PdfSharp.PageSize.Letter;
                            page.Orientation = PdfSharp.PageOrientation.Landscape;
                            gfx.Dispose();
                            gfx = XGraphics.FromPdfPage(page);
                            y = margin;
                            y = DibujarFilaTabla(gfx, margin, y, anchoEst, hdrEst, fontHeader, brushCafe, brushBlanco, true);
                        }

                        string[] valores =
                        {
                            r.WBS,
                            r.Codigo,
                            Truncar(r.Partida, 75),
                            r.AvancePorcentaje.ToString("N2"),
                            r.MontoEjecutado.ToString("C2", culturaMx),
                            r.FechaFinalizacion.HasValue ? r.FechaFinalizacion.Value.ToString("dd/MM/yyyy") : "—"
                        };
                        XBrush fondo = (fila % 2 == 0) ? (XBrush)brushAlt : XBrushes.White;
                        y = DibujarFilaTabla(gfx, margin, y, anchoEst, valores, fontCell, fondo, brushTexto, false);
                        fila++;
                    }
                }

                y += 4;
                gfx.DrawString("TOTAL ESTIMADO EJECUTADO:", fontTotal, brushCafe,
                    new XRect(margin, y, pageW - 130, 18), XStringFormats.TopRight);
                gfx.DrawString(totalEstimaciones.ToString("C2", culturaMx), fontTotal, brushVerde,
                    new XRect(margin, y, pageW, 18), XStringFormats.TopRight);
                y += 28;

                // ============ DESTAJOS ============
                if (y > page.Height - 140)
                {
                    page = pdf.AddPage();
                    page.Size = PdfSharp.PageSize.Letter;
                    page.Orientation = PdfSharp.PageOrientation.Landscape;
                    gfx.Dispose();
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                }

                gfx.DrawRectangle(brushCafeBar, margin, y, pageW, 24);
                gfx.DrawString("DESTAJOS — Agrupados por Categoría (M.O. gastado al asignar nómina · Material gastado al finalizar)",
                    fontSeccion, brushBlanco,
                    new XRect(margin + 8, y + 4, pageW - 16, 18), XStringFormats.TopLeft);
                y += 24;

                double[] anchoDes = { 35, 70, 215, 45, 45, 75, 85, 85, 65, 65 };
                string[] hdrDes = { "ID", "Tipo", "Destajo", "Cant.", "Unid.", "P. Unit.", "Importe", "Gastado", "Cuadr.", "Estado" };
                y = DibujarFilaTabla(gfx, margin, y, anchoDes, hdrDes, fontHeader, brushCafe, brushBlanco, true);

                var gruposDes = registrosDestajo
                    .GroupBy(r => string.IsNullOrEmpty(r.Categoria) ? "(Sin categoría)" : r.Categoria)
                    .OrderBy(g => g.Key);

                foreach (var grupo in gruposDes)
                {
                    if (y > page.Height - 100)
                    {
                        page = pdf.AddPage();
                        page.Size = PdfSharp.PageSize.Letter;
                        page.Orientation = PdfSharp.PageOrientation.Landscape;
                        gfx.Dispose();
                        gfx = XGraphics.FromPdfPage(page);
                        y = margin;
                        y = DibujarFilaTabla(gfx, margin, y, anchoDes, hdrDes, fontHeader, brushCafe, brushBlanco, true);
                    }

                    decimal subMO = grupo.Where(r => r.Tipo == TipoTarea.ManoDeObra).Sum(r => r.MontoGastado);
                    decimal subMat = grupo.Where(r => r.Tipo == TipoTarea.Material && r.Finalizado).Sum(r => r.Importe);

                    gfx.DrawRectangle(brushGrupo, margin, y, pageW, 18);
                    gfx.DrawString($"  {grupo.Key}   ·   {grupo.Count()} destajo(s)",
                        fontGrupo, brushCafe,
                        new XRect(margin + 4, y + 1, pageW - 300, 16), XStringFormats.CenterLeft);
                    gfx.DrawString($"M.O.: {subMO.ToString("C2", culturaMx)}   ·   Material: {subMat.ToString("C2", culturaMx)}",
                        fontGrupo, brushVerde,
                        new XRect(margin, y + 1, pageW - 6, 16), XStringFormats.CenterRight);
                    y += 18;

                    int filaDes = 0;
                    foreach (var r in grupo)
                    {
                        if (y > page.Height - 60)
                        {
                            page = pdf.AddPage();
                            page.Size = PdfSharp.PageSize.Letter;
                            page.Orientation = PdfSharp.PageOrientation.Landscape;
                            gfx.Dispose();
                            gfx = XGraphics.FromPdfPage(page);
                            y = margin;
                            y = DibujarFilaTabla(gfx, margin, y, anchoDes, hdrDes, fontHeader, brushCafe, brushBlanco, true);
                        }

                        string gastadoTxt = r.MontoGastado == 0m ? "—" : r.MontoGastado.ToString("C2", culturaMx);
                        string[] valores =
                        {
                            r.NodoID.ToString(),
                            TipoTareaTexto(r.Tipo),
                            Truncar(r.Nombre, 50),
                            r.Cantidad.ToString("N2"),
                            r.Unidad,
                            r.PrecioUnitario.ToString("C2", culturaMx),
                            r.Importe.ToString("C2", culturaMx),
                            gastadoTxt,
                            r.Cuadrilla,
                            r.EstadoTexto
                        };
                        XBrush fondo = r.GastoComputado ? (XBrush)new XSolidBrush(XColor.FromArgb(232, 245, 233))
                                                        : ((filaDes % 2 == 0) ? (XBrush)brushAlt : XBrushes.White);
                        y = DibujarFilaTabla(gfx, margin, y, anchoDes, valores, fontCell, fondo, brushTexto, false);
                        filaDes++;
                    }
                }

                y += 4;
                decimal gastoDestajos = totalManoObra + totalMaterial;
                gfx.DrawString("TOTAL GASTADO EN DESTAJOS:", fontTotal, brushCafe,
                    new XRect(margin, y, pageW - 130, 18), XStringFormats.TopRight);
                gfx.DrawString(gastoDestajos.ToString("C2", culturaMx), fontTotal, brushVerde,
                    new XRect(margin, y, pageW, 18), XStringFormats.TopRight);
                y += 28;

                // ============ Secciones informativas adicionales (sólo si vienen datos —
                // el flujo clásico de FormAdministrativos no las trae) ============
                if (comprasExtra != null && comprasExtra.Count > 0)
                {
                    y = DibujarSeccionPlanaPDF(pdf, ref page, ref gfx, margin, pageW, y,
                        "COMPRAS LIGADAS A LA CASA",
                        new double[] { 100, 70, 80, 220, 70, 90 },
                        new[] { "Folio OC", "Fecha", "Tipo", "Proveedor", "Partidas", "Importe" },
                        comprasExtra.Select(c => new[]
                        {
                            c.FolioOC, c.Fecha.HasValue ? c.Fecha.Value.ToString("dd/MM/yyyy") : "—", c.TipoOrden,
                            Truncar(c.Proveedor, 40), c.NumPartidas.ToString(), c.Importe.ToString("C2", culturaMx)
                        }).ToList(),
                        "TOTAL COMPRAS:", comprasExtra.Sum(c => c.Importe),
                        fontHeader, fontCell, fontTotal, brushCafeBar, brushCafe, brushBlanco, brushTexto, brushVerde, brushAlt);
                }

                if (salidasExtra != null && salidasExtra.Count > 0)
                {
                    y = DibujarSeccionPlanaPDF(pdf, ref page, ref gfx, margin, pageW, y,
                        "SALIDAS DE ALMACÉN",
                        new double[] { 80, 220, 60, 70, 80, 90, 80 },
                        new[] { "Clave", "Descripción", "Unidad", "Cantidad", "P. Unit.", "Importe", "Fecha" },
                        salidasExtra.Select(s => new[]
                        {
                            s.Clave, Truncar(s.Descripcion, 40), s.Unidad, s.Cantidad.ToString("N2"),
                            s.PrecioUnitario.ToString("C2", culturaMx), s.Importe.ToString("C2", culturaMx),
                            s.FechaSalida.HasValue ? s.FechaSalida.Value.ToString("dd/MM/yyyy") : "—"
                        }).ToList(),
                        "TOTAL SALIDAS DE ALMACÉN:", salidasExtra.Sum(s => s.Importe),
                        fontHeader, fontCell, fontTotal, brushCafeBar, brushCafe, brushBlanco, brushTexto, brushVerde, brushAlt);
                }

                if (nominaExtra != null && nominaExtra.Count > 0)
                {
                    y = DibujarSeccionPlanaPDF(pdf, ref page, ref gfx, margin, pageW, y,
                        "NÓMINA DETALLADA",
                        new double[] { 180, 70, 180, 90, 90, 90 },
                        new[] { "Tarea", "Cuadrilla", "Trabajador", "Rol", "Monto", "Actualizado" },
                        nominaExtra.Select(n => new[]
                        {
                            Truncar(n.NombreTarea, 30), n.CodigoCuadrilla, Truncar(n.NombreTrabajador, 30), n.Rol,
                            n.Monto.ToString("C2", culturaMx),
                            n.FechaActualizacion.HasValue ? n.FechaActualizacion.Value.ToString("dd/MM/yyyy") : "—"
                        }).ToList(),
                        "TOTAL NÓMINA:", nominaExtra.Sum(n => n.Monto),
                        fontHeader, fontCell, fontTotal, brushCafeBar, brushCafe, brushBlanco, brushTexto, brushVerde, brushAlt);
                }

                // Gran total
                if (y > page.Height - 50)
                {
                    page = pdf.AddPage();
                    page.Size = PdfSharp.PageSize.Letter;
                    page.Orientation = PdfSharp.PageOrientation.Landscape;
                    gfx.Dispose();
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                }
                gfx.DrawRectangle(brushCafe, margin, y, pageW, 36);
                gfx.DrawString("GRAN TOTAL DE LA CASA:", new XFont("Arial", 13, XFontStyle.Bold), brushBlanco,
                    new XRect(margin + 10, y + 9, pageW - 180, 22), XStringFormats.TopLeft);
                gfx.DrawString((totalEstimaciones + totalManoObra + totalMaterial).ToString("C2", culturaMx),
                    new XFont("Arial", 15, XFontStyle.Bold), brushBlanco,
                    new XRect(margin + 10, y + 7, pageW - 20, 24), XStringFormats.TopRight);

                gfx.Dispose();
                pdf.Save(rutaArchivo);
            }
        }

        private void DibujarTarjetaPDF(XGraphics gfx, double x, double y, double w,
            string titulo, string valor, string info,
            XBrush brushTitulo, XBrush brushValor, XFont fontHdr, XFont fontVal, XFont fontInfo)
        {
            // Card body
            gfx.DrawRectangle(XBrushes.White, x, y, w, 60);
            gfx.DrawRectangle(new XPen(XColor.FromArgb(220, 210, 195)), x, y, w, 60);
            // Acento café
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(179, 108, 46)), x, y, w, 3);

            gfx.DrawString(titulo, fontHdr, brushTitulo,
                new XRect(x + 10, y + 6, w - 20, 16), XStringFormats.CenterLeft);
            gfx.DrawString(valor, new XFont("Arial", 16, XFontStyle.Bold), brushValor,
                new XRect(x + 10, y + 22, w - 20, 22), XStringFormats.CenterLeft);
            gfx.DrawString(info, fontInfo, XBrushes.Gray,
                new XRect(x + 10, y + 44, w - 20, 14), XStringFormats.CenterLeft);
        }

        private double DibujarFilaTabla(XGraphics gfx, double xInicial, double y, double[] anchos,
            string[] valores, XFont font, XBrush fondo, XBrush texto, bool isHeader)
        {
            double altura = isHeader ? 22 : 16;
            double x = xInicial;
            double totalW = 0;
            foreach (var a in anchos) totalW += a;

            gfx.DrawRectangle(fondo, xInicial, y, totalW, altura);

            for (int i = 0; i < valores.Length && i < anchos.Length; i++)
            {
                var rect = new XRect(x + 4, y + 2, anchos[i] - 8, altura - 4);
                XStringFormat format;
                if (isHeader) format = XStringFormats.Center;
                else if (i == valores.Length - 1) format = XStringFormats.Center;
                else if (i >= valores.Length - 3) format = XStringFormats.CenterRight;
                else format = XStringFormats.CenterLeft;
                gfx.DrawString(valores[i] ?? "", font, texto, rect, format);
                x += anchos[i];
            }
            gfx.DrawLine(XPens.LightGray, xInicial, y + altura, xInicial + totalW, y + altura);
            return y + altura;
        }

        /// <summary>
        /// Sección de tabla plana sin agrupar (a diferencia de Estimaciones/Destajos),
        /// para las 3 secciones informativas nuevas (Compras/Salidas de Almacén/Nómina):
        /// barra de título + encabezado + filas con salto de página + línea de subtotal.
        /// </summary>
        private double DibujarSeccionPlanaPDF(PdfDocument pdf, ref PdfPage page, ref XGraphics gfx,
            double margin, double pageW, double y, string titulo,
            double[] anchos, string[] encabezados, List<string[]> filas,
            string totalLabel, decimal totalValor,
            XFont fontHeader, XFont fontCell, XFont fontTotal,
            XBrush brushBarra, XBrush brushTitulo, XBrush brushBlanco, XBrush brushTexto, XBrush brushValor, XBrush brushAlt)
        {
            if (y > page.Height - 140)
            {
                page = pdf.AddPage();
                page.Size = PdfSharp.PageSize.Letter;
                page.Orientation = PdfSharp.PageOrientation.Landscape;
                gfx.Dispose();
                gfx = XGraphics.FromPdfPage(page);
                y = margin;
            }

            gfx.DrawRectangle(brushBarra, margin, y, pageW, 24);
            gfx.DrawString(titulo, new XFont("Arial", 12, XFontStyle.Bold), brushBlanco,
                new XRect(margin + 8, y + 4, pageW - 16, 18), XStringFormats.TopLeft);
            y += 24;

            y = DibujarFilaTabla(gfx, margin, y, anchos, encabezados, fontHeader, brushTitulo, brushBlanco, true);

            int fila = 0;
            foreach (var valores in filas)
            {
                if (y > page.Height - 60)
                {
                    page = pdf.AddPage();
                    page.Size = PdfSharp.PageSize.Letter;
                    page.Orientation = PdfSharp.PageOrientation.Landscape;
                    gfx.Dispose();
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                    y = DibujarFilaTabla(gfx, margin, y, anchos, encabezados, fontHeader, brushTitulo, brushBlanco, true);
                }

                XBrush fondo = (fila % 2 == 0) ? brushAlt : XBrushes.White;
                y = DibujarFilaTabla(gfx, margin, y, anchos, valores, fontCell, fondo, brushTexto, false);
                fila++;
            }

            y += 4;
            gfx.DrawString(totalLabel, fontTotal, brushTitulo,
                new XRect(margin, y, pageW - 130, 18), XStringFormats.TopRight);
            gfx.DrawString(totalValor.ToString("C2", culturaMx), fontTotal, brushValor,
                new XRect(margin, y, pageW, 18), XStringFormats.TopRight);
            y += 28;

            return y;
        }

        private string Truncar(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
        }

        #endregion

        #region Modelos internos

        public class RegistroEstimacion
        {
            public string WBS { get; set; }
            public string Codigo { get; set; }
            public string Etapa { get; set; }
            public string Partida { get; set; }
            public decimal AvancePorcentaje { get; set; }
            public decimal MontoEjecutado { get; set; }
            public DateTime? FechaFinalizacion { get; set; }
        }

        public class RegistroDestajo
        {
            public int NodoID { get; set; }
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string Categoria { get; set; }
            public decimal Cantidad { get; set; }
            public string Unidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public string Cuadrilla { get; set; }
            public bool Finalizado { get; set; }
            public DateTime? FechaFinalizacion { get; set; }
            public TipoTarea Tipo { get; set; }
            /// <summary>Total acumulado en NominaTareasAsignada para este destajo.</summary>
            public decimal NominaAsignada { get; set; }
            public decimal Importe => Cantidad * PrecioUnitario;

            /// <summary>
            /// Monto considerado "gastado" según reglas de negocio:
            ///   M.O.: total asignado en nómina · Material: importe sólo si está finalizado.
            /// </summary>
            public decimal MontoGastado
            {
                get
                {
                    if (Tipo == TipoTarea.ManoDeObra) return NominaAsignada;
                    if (Tipo == TipoTarea.Material) return Finalizado ? Importe : 0m;
                    return 0m;
                }
            }

            public bool GastoComputado => MontoGastado > 0m;

            public string TipoTexto
            {
                get
                {
                    switch (Tipo)
                    {
                        case TipoTarea.ManoDeObra: return "M. de Obra";
                        case TipoTarea.Material: return "Material";
                        default: return "—";
                    }
                }
            }

            public string EstadoTexto
            {
                get
                {
                    if (Finalizado) return "Finalizado";
                    if (!string.IsNullOrEmpty(Cuadrilla)) return "Con cuadrilla";
                    return "Activado";
                }
            }
        }

        #endregion
    }
}
