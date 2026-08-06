// Database operations for entries (entradas) - FormAlmacen
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    public partial class FormAlmacen : Form
    {
        // Clase auxiliar para almacenar datos de �rdenes en el ComboBox
        private class OrdenComboItem
        {
            public string Folio { get; set; }
            public string NombreOrden { get; set; }
            
            public override string ToString()
            {
                if (!string.IsNullOrWhiteSpace(NombreOrden))
                    return $"{NombreOrden} ({Folio})";
                return Folio;
            }
        }

        private void TxtBuscarEntrada_TextChanged(object sender, EventArgs e)
        {
            // Filtra las TARJETAS (no la grilla) por clave / descripcion.
            RenderTarjetasEntrada(txtBuscarEntrada?.Text);
        }

        private void CargarOrdenesDeCompra()
        {
            cmbOrdenesCompra.Items.Clear();

            string sql = @"
SELECT DISTINCT d.FolioOC, o.NombreOrden
FROM OrdenesCompraDetalle d
INNER JOIN OrdenesCompra o ON d.FolioOC = o.FolioOC
WHERE d.Estado = 'PENDIENTE'
ORDER BY d.FolioOC DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string folio = reader["FolioOC"].ToString();
                        string nombreOrden = reader["NombreOrden"]?.ToString();
                        
                        cmbOrdenesCompra.Items.Add(new OrdenComboItem 
                        { 
                            Folio = folio, 
                            NombreOrden = nombreOrden 
                        });
                    }
                }
            }

            // Autocompletado
            var autoComplete = new AutoCompleteStringCollection();
            foreach (OrdenComboItem item in cmbOrdenesCompra.Items)
            {
                autoComplete.Add(item.ToString());
            }

            cmbOrdenesCompra.AutoCompleteCustomSource = autoComplete;
        }

        private void dgvEntrada_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            dgvEntrada.CurrentCellDirtyStateChanged += (s, ev) =>
            {
                if (dgvEntrada.IsCurrentCellDirty)
                {
                    dgvEntrada.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
        }

        private void BtnCapturarEntrada_Click(object sender, EventArgs e)
        {
            if (casaActual == null || casaInventario == null)
            {
                MessageBox.Show("?? Selecciona una casa (lote) antes de capturar entrada.", 
                    "Casa requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dt = dgvEntrada.DataSource as DataTable;
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("?? No hay datos para registrar.", 
                    "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dt.AsEnumerable().All(r => r["Estado"].ToString() != "PENDIENTE"))
            {
                MessageBox.Show("? No hay insumos pendientes para registrar.", 
                    "Orden completa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool hayAlgoParaRegistrar = false;

            foreach (DataRow row in dt.Rows)
            {
                if (row["Estado"].ToString() != "PENDIENTE")
                    continue;

                if (row["CantidadRecibida"] != DBNull.Value &&
                    double.TryParse(row["CantidadRecibida"].ToString(), out double capturada) &&
                    capturada > 0)
                {
                    hayAlgoParaRegistrar = true;

                    double cantidadComprada = Convert.ToDouble(row["CantidadComprada"]);
                    if (capturada != cantidadComprada && string.IsNullOrWhiteSpace(row["Justificacion"]?.ToString()))
                    {
                        string clave = row["Clave"].ToString();
                        string justificacion = PromptJustificacion.Pedir(this,
                            "Justificación requerida",
                            $"La cantidad capturada de {clave} no coincide con la comprada.",
                            "Explica por qué la entrada difiere de la orden de compra.");

                        if (string.IsNullOrWhiteSpace(justificacion))
                        {
                            MessageBox.Show("La justificación es obligatoria; no se registró nada.", 
                                "Operación cancelada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        row["Justificacion"] = justificacion;
                    }
                }
            }

            if (!hayAlgoParaRegistrar)
            {
                MessageBox.Show("?? No hay cantidades v�lidas capturadas para registrar.", 
                    "Sin cantidades", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("�Deseas confirmar la captura de esta entrada?", 
                "Confirmar entrada", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            string folioOC = GetFolioFromCombo();
            if (string.IsNullOrWhiteSpace(folioOC))
            {
                MessageBox.Show("?? Ingresa un folio de orden.", 
                    "Folio requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string usuario = Global.UsuarioActual?.Nombre ?? Environment.UserName;
            var insumosParaPdf = new List<MovimientoHistorial>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                foreach (DataRow row in dt.Rows)
                {
                    if (row["Estado"].ToString() != "PENDIENTE")
                        continue;

                    if (row["CantidadRecibida"] == DBNull.Value ||
                        !double.TryParse(row["CantidadRecibida"].ToString(), out double cantidadRecibida) ||
                        cantidadRecibida <= 0)
                        continue;

                    int idDetalle = Convert.ToInt32(row["Id"]);
                    string clave = row["Clave"].ToString();
                    string descripcion = row["Descripcion"].ToString();
                    string unidad = row["Unidad"].ToString();
                    double cantidadComprada = Convert.ToDouble(row["CantidadComprada"]);
                    bool esExcepcion = cantidadRecibida != cantidadComprada;
                    string justificacion = row["Justificacion"]?.ToString();

                    // OBTENER PRECIO UNITARIO
                    decimal precioUnitario = 0;
                    using (SqlCommand cmdPrecio = new SqlCommand("SELECT PrecioUnitario FROM OrdenesCompraDetalle WHERE Id = @id", conn))
                    {
                        cmdPrecio.Parameters.AddWithValue("@id", idDetalle);
                        var result = cmdPrecio.ExecuteScalar();
                        precioUnitario = (result != null && result != DBNull.Value) ? Convert.ToDecimal(result) : 0;
                    }

                    decimal importe = precioUnitario * (decimal)cantidadRecibida;

                    // INSERTAR EN EntradasAlmacen
                    using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO EntradasAlmacen 
(FolioOC, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe, FechaEntrada, Manzana, Lote, Prototipo, Usuario, EsExcepcion, Justificacion)
VALUES 
(@FolioOC, @Clave, @Descripcion, @Unidad, @Cantidad, @Precio, @Importe, GETDATE(), @Manzana, @Lote, @Prototipo, @Usuario, @EsExcepcion, @Justificacion)", conn))
                    {
                        cmd.Parameters.AddWithValue("@FolioOC", folioOC);
                        cmd.Parameters.AddWithValue("@Clave", clave);
                        cmd.Parameters.AddWithValue("@Descripcion", descripcion);
                        cmd.Parameters.AddWithValue("@Unidad", unidad);
                        cmd.Parameters.AddWithValue("@Cantidad", cantidadRecibida);
                        cmd.Parameters.AddWithValue("@Precio", precioUnitario);
                        cmd.Parameters.AddWithValue("@Importe", importe);
                        cmd.Parameters.AddWithValue("@Manzana", casaActual.Manzana);
                        cmd.Parameters.AddWithValue("@Lote", casaActual.Lote);
                        cmd.Parameters.AddWithValue("@Prototipo", casaInventario.Prototipo);
                        cmd.Parameters.AddWithValue("@Usuario", usuario);
                        cmd.Parameters.AddWithValue("@EsExcepcion", esExcepcion);
                        cmd.Parameters.AddWithValue("@Justificacion", (object)justificacion ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }

                    // INSERTAR EN HistorialMovimientos
                    using (SqlCommand cmdHist = new SqlCommand(@"
INSERT INTO HistorialMovimientos 
(Fecha, TipoMovimiento, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, Importe, Usuario, Manzana, Lote, Prototipo, Justificacion)
VALUES 
(GETDATE(), 'Entrada', @Clave, @Descripcion, @Unidad, @Cantidad, @Precio, @Importe, @Usuario, @Manzana, @Lote, @Prototipo, @Justificacion)", conn))
                    {
                        cmdHist.Parameters.AddWithValue("@Clave", clave);
                        cmdHist.Parameters.AddWithValue("@Descripcion", descripcion);
                        cmdHist.Parameters.AddWithValue("@Unidad", unidad);
                        cmdHist.Parameters.AddWithValue("@Cantidad", cantidadRecibida);
                        cmdHist.Parameters.AddWithValue("@Precio", precioUnitario);
                        cmdHist.Parameters.AddWithValue("@Importe", importe);
                        cmdHist.Parameters.AddWithValue("@Usuario", usuario);
                        cmdHist.Parameters.AddWithValue("@Manzana", casaActual.Manzana);
                        cmdHist.Parameters.AddWithValue("@Lote", casaActual.Lote);
                        cmdHist.Parameters.AddWithValue("@Prototipo", casaInventario.Prototipo);
                        cmdHist.Parameters.AddWithValue("@Justificacion", (object)justificacion ?? DBNull.Value);
                        cmdHist.ExecuteNonQuery();
                    }

                    // ACTUALIZAR ESTADO DE INSUMO EN ORDEN
                    double recibidoAntes = 0;
                    using (SqlCommand cmdCheck = new SqlCommand(@"
SELECT ISNULL(SUM(Cantidad), 0)
FROM EntradasAlmacen
WHERE FolioOC = @FolioOC AND Clave = @Clave", conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@FolioOC", folioOC);
                        cmdCheck.Parameters.AddWithValue("@Clave", clave);
                        recibidoAntes = Convert.ToDouble(cmdCheck.ExecuteScalar());
                    }

                    double totalRecibido = recibidoAntes;
                    string nuevoEstado = totalRecibido >= cantidadComprada ? "RECIBIDO" : "PENDIENTE";

                    using (SqlCommand cmdUpdate = new SqlCommand("UPDATE OrdenesCompraDetalle SET Estado = @estado WHERE Id = @Id", conn))
                    {
                        cmdUpdate.Parameters.AddWithValue("@estado", nuevoEstado);
                        cmdUpdate.Parameters.AddWithValue("@Id", idDetalle);
                        cmdUpdate.ExecuteNonQuery();
                    }

                    // AGREGAR A LISTA PDF
                    insumosParaPdf.Add(new MovimientoHistorial
                    {
                        Clave = clave,
                        Descripcion = descripcion,
                        Unidad = unidad,
                        Cantidad = (decimal)cantidadRecibida,
                        Precio = precioUnitario,
                        Usuario = usuario,
                        Manzana = casaActual.Manzana,
                        Lote = casaActual.Lote,
                        Prototipo = casaInventario.Prototipo,
                        Justificacion = justificacion,
                        Fecha = DateTime.Now,
                        TipoMovimiento = "Entrada"
                    });
                }
            }

            MessageBox.Show("? Entrada capturada correctamente.", 
                "Operación exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // GENERAR PDF
            GenerarPDFEntrada(folioOC, usuario, casaActual.Manzana, casaActual.Lote, casaInventario.Prototipo, DateTime.Now, insumosParaPdf);

            // LIMPIAR UI
            var selectedItem = cmbOrdenesCompra.SelectedItem as OrdenComboItem;
            if (selectedItem != null)
            {
                cmbOrdenesCompra.Items.Remove(selectedItem);
                cmbOrdenesCompra.SelectedIndex = -1;
                dgvEntrada.DataSource = null;
            }

            // Limpia las tarjetas tras capturar.
            _conciliacionPorId = null;
            _resumenEntrada = "";
            RenderTarjetasEntrada();

            CargarOrdenesDeCompra();
            CargarHistorial();
        }

        public static void GenerarPDFEntrada(string folio, string usuario, string manzana, string lote, string prototipo, DateTime fecha, List<MovimientoHistorial> insumos)
        {
            string carpetaPdf = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CALANDRIA RESIDENCIAL", "PDFOrdenesCompra");
            Directory.CreateDirectory(carpetaPdf);
            string rutaPdf = Path.Combine(carpetaPdf, $"EntradaAlmacen_{folio}.pdf");

            PdfDocument pdf = new PdfDocument();
            PdfPage page = pdf.AddPage();
            page.Size = PdfSharp.PageSize.Letter;

            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Segoe UI", 9, XFontStyle.Regular);
            XFont bold = new XFont("Segoe UI", 9, XFontStyle.Bold);
            XFont titleFont = new XFont("Segoe UI", 14, XFontStyle.Bold);

            int x = 40;
            int y = 40;

            // LOGO
            using (var ms = new MemoryStream())
            {
                DynamicSepticSystem.Properties.Resources.Logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                var logo = XImage.FromStream(ms);
                gfx.DrawImage(logo, page.Width - 150, 20, 100, 50);
            }

            // Encabezado
            gfx.DrawString("Desarrolladora de Casas Camaney", bold, XBrushes.Black, new XPoint(x, y)); y += 15;
            gfx.DrawString("Blvd. Perif�rico sur y Carretera a la Colorada", font, XBrushes.Black, new XPoint(x, y)); y += 15;
            gfx.DrawString("Tel. 662-XXX-XXXX | Email: constcas@empresa.com", font, XBrushes.Black, new XPoint(x, y)); y += 25;
            gfx.DrawLine(XPens.Gray, x, y, page.Width - x, y); y += 25;

            // T�tulo centrado
            gfx.DrawString("ENTRADA DE ALMAC�N", titleFont, XBrushes.Black, new XRect(0, y, page.Width, 30), XStringFormats.TopCenter); y += 40;

            // Datos generales
            gfx.DrawString($"Folio: {folio}", bold, XBrushes.Black, new XPoint(x, y));
            gfx.DrawString($"Fecha: {fecha:dd/MM/yyyy}", bold, XBrushes.Black, new XPoint(page.Width / 2 + 20, y)); y += 20;
            gfx.DrawString($"Casa: Manzana {manzana} - Lote {lote} | Prototipo: {prototipo}", font, XBrushes.Black, new XPoint(x, y)); y += 20;
            gfx.DrawString($"Capturado por: {usuario}", font, XBrushes.Black, new XPoint(x, y)); y += 30;
            gfx.DrawLine(XPens.LightGray, x, y, page.Width - x, y); y += 15;

            // Tabla: columnas y medidas
            string[] headers = { "CODIGO", "INSUMO", "UNIDAD", "CANTIDAD", "PRECIO", "IMPORTE" };
            int[] widths = { 60, 200, 60, 70, 70, 80 };
            int rowHeight = 20;
            int tableX = x;
            decimal totalImporte = 0;

            // CABECERA
            XSolidBrush headerBrush = new XSolidBrush(XColor.FromArgb(240, 240, 240));
            gfx.DrawRectangle(headerBrush, tableX, y, widths.Sum(), rowHeight);
            int colX = tableX;
            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawRectangle(XPens.Black, colX, y, widths[i], rowHeight);
                gfx.DrawString(headers[i], bold, XBrushes.Black, new XRect(colX, y, widths[i], rowHeight), XStringFormats.Center);
                colX += widths[i];
            }
            y += rowHeight;

            // FILAS
            foreach (var insumo in insumos)
            {
                int alto = rowHeight;
                var size = gfx.MeasureString(insumo.Descripcion, font);
                int lineas = (int)Math.Ceiling(size.Width / widths[1]);
                alto = Math.Max(rowHeight * lineas, rowHeight);

                string[] datos = {
            insumo.Clave,
            insumo.Descripcion,
            insumo.Unidad,
            insumo.Cantidad.ToString("0.##"),
            insumo.Precio.ToString("C"),
            insumo.Importe.ToString("C")
        };

                colX = tableX;
                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawRectangle(XPens.Black, colX, y, widths[i], alto);
                    gfx.DrawString(datos[i], font, XBrushes.Black, new XRect(colX + 2, y + 4, widths[i] - 4, alto - 4), XStringFormats.TopLeft);
                    colX += widths[i];
                }

                totalImporte += insumo.Importe;
                y += alto;

                if (y > page.Height - 100)
                {
                    page = pdf.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            // FILA TOTAL
            colX = tableX;
            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawRectangle(XPens.Black, colX, y, widths[i], rowHeight);
                if (i == 4)
                    gfx.DrawString("TOTAL", bold, XBrushes.Black, new XRect(colX, y, widths[i], rowHeight), XStringFormats.Center);
                else if (i == 5)
                    gfx.DrawString(totalImporte.ToString("C"), bold, XBrushes.Black, new XRect(colX, y, widths[i], rowHeight), XStringFormats.Center);
                colX += widths[i];
            }
            y += rowHeight + 20;

            // Firma
            gfx.DrawString("__________________________________________", font, XBrushes.Black, x, y); y += 15;
            gfx.DrawString("Encargado de Almac�n - Nombre y Firma", font, XBrushes.Black, x, y); y += 30;

            // Pie de p�gina
            gfx.DrawLine(XPens.LightGray, x, page.Height - 40, page.Width - x, page.Height - 40);
            gfx.DrawString($"CALANDRIA RESIDENCIAL - Control Interno | Generado: {DateTime.Now:dd/MM/yyyy HH:mm}", font, XBrushes.Gray, new XPoint(x, page.Height - 25));

            // GUARDAR Y ABRIR
            pdf.Save(rutaPdf);
            Process.Start(rutaPdf);
        }

        private void CargarInsumosDesdeOrdenSeleccionada()
        {
            string folio = GetFolioFromCombo();
            if (string.IsNullOrWhiteSpace(folio)) return;

            string sql = @"
    SELECT Id, Clave, Descripcion, Unidad, Cantidad AS CantidadComprada, Estado, Justificacion
    FROM OrdenesCompraDetalle
    WHERE FolioOC = @folio AND Estado = 'PENDIENTE'";

            DataTable dt = new DataTable();
            using (var conn = new SqlConnection(connectionString))
            using (var da = new SqlDataAdapter(sql, conn))
            {
                da.SelectCommand.Parameters.AddWithValue("@folio", folio);
                da.Fill(dt);
            }

            if (!dt.Columns.Contains("CantidadRecibida"))
                dt.Columns.Add("CantidadRecibida", typeof(double));

            dgvEntrada.DataSource = dt; // grilla OCULTA = modelo

            // Nueva orden cargada: limpia la conciliacion previa y refresca tarjetas.
            _conciliacionPorId = null;
            string nombre = (cmbOrdenesCompra.SelectedItem as OrdenComboItem)?.NombreOrden;
            _resumenEntrada = $"Orden {folio}" +
                (string.IsNullOrWhiteSpace(nombre) ? "" : $" · {nombre}") +
                $" · {dt.Rows.Count} partida(s) pendientes. Captura lo recibido y pulsa \"Capturar entrada\".";
            RenderTarjetasEntrada(txtBuscarEntrada?.Text);

            MostrarLotesDeOrden(folio);
        }

        private void BtnBuscarOrden_Click(object sender, EventArgs e)
        {
            CargarOrdenesDeCompra();

            if (cmbOrdenesCompra.Items.Count > 0)
            {
                cmbOrdenesCompra.SelectedIndex = 0;
                CargarInsumosDesdeOrdenSeleccionada();
            }
            else
            {
                MessageBox.Show("No hay �rdenes de compra pendientes.");
            }
        }

        /// <summary>
        /// Conciliar con factura: sube el CFDI (XML) de la OC seleccionada, el servidor lo
        /// compara contra el detalle PENDIENTE y devuelve cuánto se facturó por partida.
        /// Aquí se PRECARGA CantidadRecibida y se resaltan las diferencias; el usuario
        /// revisa y confirma con "Capturar entrada" (la diferencia deja la orden parcial).
        /// </summary>
        private void BtnConciliarFactura_Click(object sender, EventArgs e)
        {
            string folio = GetFolioFromCombo();
            if (string.IsNullOrWhiteSpace(folio))
            {
                MessageBox.Show("Selecciona y carga una orden de compra primero.", "Conciliar factura",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Asegura que el detalle de la orden esté cargado en la grilla.
            var dt = dgvEntrada.DataSource as DataTable;
            if (dt == null || !dt.Columns.Contains("CantidadRecibida"))
            {
                CargarInsumosDesdeOrdenSeleccionada();
                dt = dgvEntrada.DataSource as DataTable;
            }
            if (dt == null)
            {
                MessageBox.Show("No hay partidas cargadas para esta orden.", "Conciliar factura",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string xmlBase64;
            using (var ofd = new OpenFileDialog
            {
                Title = "Selecciona el CFDI (XML) de la factura",
                Filter = "Factura CFDI (*.xml)|*.xml|Todos los archivos|*.*"
            })
            {
                if (ofd.ShowDialog() != DialogResult.OK) return;
                try { xmlBase64 = Convert.ToBase64String(File.ReadAllBytes(ofd.FileName)); }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo leer el archivo: " + ex.Message, "Conciliar factura",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            ConciliarFacturaResponseApi resp;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                resp = ApiClient.Post<ConciliarFacturaResponseApi>(
                    "/api/ordenescompra/conciliar-factura",
                    new { FolioOC = folio, XmlBase64 = xmlBase64 });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conciliar: " + ex.Message, "Conciliar factura",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally { Cursor.Current = Cursors.Default; }

            if (resp == null)
            {
                MessageBox.Show("El servidor no devolvió conciliación.", "Conciliar factura",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Precargar CantidadRecibida con lo facturado emparejado y guardar la
            // conciliacion por Id para etiquetar/colorear las tarjetas.
            _conciliacionPorId = new Dictionary<int, LineaConciliacionApi>();
            foreach (var l in resp.Lineas) _conciliacionPorId[l.IdDetalle] = l;

            int emparejadas = 0, conDiferencia = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (row["Id"] == DBNull.Value) continue;
                int id = Convert.ToInt32(row["Id"]);
                if (!_conciliacionPorId.TryGetValue(id, out var linea) || linea.Origen == "SinMatch") continue;
                row["CantidadRecibida"] = (double)linea.CantidadFacturada;
                emparejadas++;
                if (linea.Diferencia != 0m) conDiferencia++;
            }

            string sinAsignar = resp.SinAsignar != null && resp.SinAsignar.Count > 0
                ? $" · {resp.SinAsignar.Count} concepto(s) sin asignar"
                : "";
            _resumenEntrada = $"Factura {resp.Emisor} · Total {resp.TotalFactura:C2} · " +
                $"{emparejadas} emparejada(s), {conDiferencia} con diferencia{sinAsignar}";
            RenderTarjetasEntrada(txtBuscarEntrada?.Text);

            string sinAsignarMsg = resp.SinAsignar != null && resp.SinAsignar.Count > 0
                ? $"\n\n⚠ {resp.SinAsignar.Count} concepto(s) de la factura no se asignaron a la orden."
                : "";
            string ia = resp.UsoIA
                ? "\n\nSe emparejaron automaticamente los conceptos de la factura cuya redaccion " +
                  "no coincidia exactamente con las partidas de la orden, y se precargaron las " +
                  "cantidades recibidas para tu revision."
                : "";
            string aviso = string.IsNullOrEmpty(resp.Aviso) ? "" : "\n" + resp.Aviso;
            MessageBox.Show(
                $"Factura de: {resp.Emisor} ({resp.Rfc})\nUUID: {resp.Uuid}\nTotal factura: {resp.TotalFactura:C2}\n\n" +
                $"Partidas emparejadas: {emparejadas}\nCon diferencia (parcial): {conDiferencia}{sinAsignarMsg}{ia}{aviso}\n\n" +
                "Revisa las cantidades precargadas en las tarjetas y confirma con \"Capturar entrada\".",
                "Conciliación de factura", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MostrarLotesDeOrden(string folioOC)
        {
            flwCasasAsociadas.Controls.Clear();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT Manzana, Lote FROM OrdenesCompra_Casas WHERE FolioOC = @folio", conn))
            {
                cmd.Parameters.AddWithValue("@folio", folioOC);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string manzana = reader["Manzana"].ToString();
                        string lote = reader["Lote"].ToString();

                        Button btnCasa = new Button
                        {
                            Text = $"M{manzana}-L{lote}",
                            Tag = (manzana, lote),
                            AutoSize = true,
                            Margin = new Padding(3)
                        };

                        btnCasa.Click += BtnCasa_Click;
                        flwCasasAsociadas.Controls.Add(btnCasa);
                    }
                }
            }
        }

        private void BtnCasa_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag is ValueTuple<string, string> casaTag)
            {
                string manzana = casaTag.Item1;
                string lote = casaTag.Item2;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l", conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzana);
                        cmd.Parameters.AddWithValue("@l", lote);
                        string prototipo = cmd.ExecuteScalar()?.ToString();

                        if (string.IsNullOrEmpty(prototipo))
                        {
                            MessageBox.Show("No se encontr� la casa en InventarioCasas.");
                            return;
                        }

                        casaActual = new PanelPrincipal.Casa { Manzana = manzana, Lote = lote };
                        casaInventario = new DynamicSepticSystem.CasaInventario { Manzana = manzana, Lote = lote, Prototipo = prototipo };

                        lblCasa.Text = $"M{manzana}-L{lote} ({prototipo})";
                    }
                }
            }
        }

        private void cmbOrdenesCompra_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Carga unificada (con Id/Estado/CantidadComprada) para las tarjetas.
            CargarInsumosDesdeOrdenSeleccionada();
        }

        private void btnEliminardeOrden_Click(object sender, EventArgs e)
        {
            if (dgvEntrada.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona al menos un insumo.", 
                    "Selección requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (DataGridViewRow row in dgvEntrada.SelectedRows)
            {
                int id = Convert.ToInt32(row.Cells["Id"].Value);
                string clave = row.Cells["Clave"].Value.ToString();

                string justificacion = PromptJustificacion.Pedir(this,
                    "Eliminar insumo de la orden",
                    $"Vas a eliminar el insumo {clave} de la orden de compra.",
                    "Indica el motivo de la eliminación.", peligro: true);

                if (string.IsNullOrWhiteSpace(justificacion))
                {
                    MessageBox.Show("La justificación es obligatoria.", 
                        "Operación cancelada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("UPDATE OrdenesCompraDetalle SET Estado = 'ELIMINADO', Justificacion = @Justificacion WHERE Id = @Id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@Justificacion", justificacion);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("? Insumo(s) eliminados correctamente.", 
                "Operación exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            CargarInsumosDesdeOrdenSeleccionada(); // Refresca la tabla
        }

        // M�todo auxiliar para obtener el folio desde el ComboBox
        private string GetFolioFromCombo()
        {
            var selectedItem = cmbOrdenesCompra.SelectedItem;
            
            if (selectedItem is OrdenComboItem ordenItem)
                return ordenItem.Folio;
            
            if (selectedItem != null)
                return selectedItem.ToString();
            
            return string.Empty;
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {
        }
    }
}
