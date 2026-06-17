using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace DynamicSepticSystem
{
    public partial class FormCompraMulti : Form
    {
        public FormCompraMulti()
        {
            InitializeComponent();
            
            // Configurar listas ANTES del tema para que las columnas existan
            ConfigurarListas();
            
            // Aplicar tema corporativo
            AplicarTema();
            
            // Cargar proveedores
            CargarProveedores();
        }

        private void AplicarTema()
        {
            // Fondo del formulario
            this.BackColor = ThemeManager.ColorFondo;
            
            // Aplicar tema general
            ThemeManager.AplicarTema(this);
            
            // Estilizar botones específicos
            ThemeManager.EstilizarBotonExito(btnAgregarLote);
            ThemeManager.EstilizarBotonExito(btnGenerarOrden);
            ThemeManager.EstilizarBotonExito(btnAgregarInsumo);
            
            btnAgregarProveedor.BackColor = ThemeManager.ColorInfo;
            btnAgregarProveedor.ForeColor = ThemeManager.ColorTextoClaro;
            btnAgregarProveedor.FlatStyle = FlatStyle.Flat;
            btnAgregarProveedor.FlatAppearance.BorderSize = 0;
            
            btnEliminarCasa.BackColor = ThemeManager.ColorError;
            btnEliminarCasa.ForeColor = ThemeManager.ColorTextoClaro;
            btnEliminarCasa.FlatStyle = FlatStyle.Flat;
            btnEliminarCasa.FlatAppearance.BorderSize = 0;
            
            // Estilizar ObjectListViews (ahora que ya tienen columnas)
            EstilizarObjectListView(olvCatalogo);
            EstilizarObjectListView(olvCarrito);
            
            // Estilizar ListBox de casas
            lstCasas.BackColor = ThemeManager.ColorFondo;
            lstCasas.ForeColor = ThemeManager.ColorTextoOscuro;
            lstCasas.BorderStyle = BorderStyle.FixedSingle;
            lstCasas.Font = new Font("Segoe UI", 9F);
        }
        
        private void EstilizarObjectListView(ObjectListView olv)
        {
            olv.BackColor = ThemeManager.ColorFondo;
            olv.ForeColor = ThemeManager.ColorTextoOscuro;
            olv.BorderStyle = BorderStyle.FixedSingle;
            olv.FullRowSelect = true;
            olv.GridLines = true;
            olv.Font = new Font("Segoe UI", 9F);
            
            // Aplicar estilo de encabezados después de que se configuren las columnas
            olv.HeaderFormatStyle = new HeaderFormatStyle
            {
                Hot = new HeaderStateStyle
                {
                    BackColor = ThemeManager.ColorPrincipalClaro,
                    ForeColor = ThemeManager.ColorTextoClaro
                },
                Normal = new HeaderStateStyle
                {
                    BackColor = ThemeManager.ColorPrincipalMenuBar,
                    ForeColor = ThemeManager.ColorTextoClaro
                },
                Pressed = new HeaderStateStyle
                {
                    BackColor = ThemeManager.ColorPrincipalOscuro,
                    ForeColor = ThemeManager.ColorTextoClaro
                }
            };
            
            olv.UseAlternatingBackColors = true;
            olv.AlternateRowBackColor = ThemeManager.ColorFondoAlterno;
        }

        string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"]?.ConnectionString;

        // Catálogo de proveedores cacheado desde el API (PROVEEDORESCALANDRIA).
        private List<ProveedorApi> _proveedores = new List<ProveedorApi>();


        private string ObtenerPrototipoDesdeBD(string manzana, string lote)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                var result = cmd.ExecuteScalar();
                return result?.ToString();
            }
        }

        // Devuelve la tabla de explosion adecuada según el prototipo
        private string GetExplosionTableForPrototipo(string prototipo)
        {
            // Selecciona la tabla que contiene el catálogo de insumos para el prototipo.
            // Nota: las consultas a la tabla no filtrarán por columna 'Prototipo' porque ya no es necesaria.
            if (string.IsNullOrWhiteSpace(prototipo))
                return "COMPRASCALANDRA";

            var p = prototipo.ToUpperInvariant();
            if (p.Contains("TUNERA"))
                return "COMPRASTUNERA";
            if (p.Contains("CALANDRA") || p.Contains("CALANDRIA"))
                return "COMPRASCALANDRA";

            return "COMPRASCALANDRA"; // fallback seguro
        }

        // Helper: verifica si el reader tiene una columna
        private bool ReaderHasColumn(SqlDataReader reader, string columnName)
        {
            try
            {
                return reader.GetOrdinal(columnName) >= 0;
            }
            catch
            {
                return false;
            }
        }

        // Helper: intenta leer el primer nombre de columna válido y retornar como string
        private string ReadStringFromReader(SqlDataReader reader, params string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                if (ReaderHasColumn(reader, name))
                {
                    var val = reader[name];
                    return val == DBNull.Value ? string.Empty : val.ToString();
                }
            }
            return string.Empty;
        }

        private void ActualizarCatalogoInsumos()
        {
            Dictionary<string, int> casasPorPrototipo = new Dictionary<string, int>();
            List<(string Manzana, string Lote)> casasSeleccionadas = new List<(string, string)>();

            foreach (CasaSeleccionada casa in lstCasas.Items)
            {
                string prototipo = casa.Prototipo;
                string manzana = casa.Manzana;
                string lote = casa.Lote;

                if (!casasPorPrototipo.ContainsKey(prototipo))
                    casasPorPrototipo[prototipo] = 0;
                casasPorPrototipo[prototipo]++;

                casasSeleccionadas.Add((manzana, lote));
            }

            Dictionary<string, InsumoOrdenCompra> insumosTotales = new Dictionary<string, InsumoOrdenCompra>();
            Dictionary<string, decimal> faltantesPorClave = new Dictionary<string, decimal>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Obtener insumos pendientes por casa
                using (SqlCommand cmd = new SqlCommand(@"
SELECT d.Clave, SUM(d.Cantidad) - ISNULL((
    SELECT SUM(e.Cantidad)
    FROM EntradasAlmacen e
    WHERE e.FolioOC = d.FolioOC AND e.Clave = d.Clave
), 0) AS CantidadPendiente
FROM OrdenesCompraDetalle d
INNER JOIN OrdenesCompra_Casas c ON d.FolioOC = c.FolioOC
WHERE c.Manzana = @m AND c.Lote = @l AND d.Estado = 'PENDIENTE'
GROUP BY d.FolioOC, d.Clave, d.Cantidad", conn))
                {
                    foreach (var (manzana, lote) in casasSeleccionadas)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@m", manzana);
                        cmd.Parameters.AddWithValue("@l", lote);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string clave = reader["Clave"].ToString();
                                decimal pendiente = Convert.ToDecimal(reader["CantidadPendiente"]);
                                if (pendiente > 0)
                                {
                                    if (faltantesPorClave.ContainsKey(clave))
                                        faltantesPorClave[clave] += pendiente;
                                    else
                                        faltantesPorClave[clave] = pendiente;
                                }
                            }
                        }
                    }
                }

                // Obtener insumos por prototipo
                foreach (var kvp in casasPorPrototipo)
                {
                    string prototipo = kvp.Key;
                    int cantidadCasas = kvp.Value;
                    string tabla = GetExplosionTableForPrototipo(prototipo);

                    using (SqlCommand cmd = new SqlCommand($"SELECT Clave, Descripcion, Unidad, Cantidad, Familia FROM {tabla}", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string clave = ReadStringFromReader(reader, "Clave");
                                string desc = ReadStringFromReader(reader, "Descripcion");
                                string unidad = ReadStringFromReader(reader, "Unidad");
                                decimal porCasa = 0;
                                string cantidadCol = ReaderHasColumn(reader, "Cantidad") ? "Cantidad" : (ReaderHasColumn(reader, "cantidad") ? "cantidad" : null);
                                object cantidadObj = DBNull.Value;
                                if (cantidadCol != null) cantidadObj = reader[cantidadCol];
                                if (cantidadObj != DBNull.Value) decimal.TryParse(cantidadObj.ToString(), out porCasa);
                                string familia = ReadStringFromReader(reader, "Familia");

                                // Leer columna de precio/costo si existe (no obligatorio)
                                decimal precio = 0m;
                                string precioCol = null;
                                if (ReaderHasColumn(reader, "Costo")) precioCol = "Costo";
                                else if (ReaderHasColumn(reader, "CostoUnitario")) precioCol = "CostoUnitario";
                                else if (ReaderHasColumn(reader, "Precio")) precioCol = "Precio";
                                else if (ReaderHasColumn(reader, "precio")) precioCol = "precio";
                                else if (ReaderHasColumn(reader, "costo")) precioCol = "costo";

                                if (precioCol != null)
                                {
                                    try
                                    {
                                        var precioObj = reader[precioCol];
                                        if (precioObj != DBNull.Value) decimal.TryParse(precioObj.ToString(), out precio);
                                    }
                                    catch { }
                                }

                                decimal cantidadTotal = porCasa * cantidadCasas;

                                bool fueAjustado = false;
                                if (faltantesPorClave.TryGetValue(clave, out decimal pendiente))
                                {
                                    cantidadTotal -= pendiente;
                                    fueAjustado = pendiente > 0;
                                }

                                if (cantidadTotal <= 0)
                                    continue;

                                if (insumosTotales.ContainsKey(clave))
                                {
                                    insumosTotales[clave].Cantidad += Math.Round(cantidadTotal, 3);
                                    insumosTotales[clave].EsModificado |= fueAjustado;
                                }
                                else
                                {
                                    insumosTotales[clave] = new InsumoOrdenCompra
                                    {
                                        Clave = clave,
                                        Descripcion = desc,
                                        Unidad = unidad,
                                        Cantidad = Math.Round(cantidadTotal, 3),
                                        Familia = familia,
                                        EsModificado = fueAjustado,
                                        Precio = precio // asignar precio leido
                                    };
                                }

                            }
                        }
                    }
                }
            }

            listaCatalogoOriginal = insumosTotales.Values.ToList();
            olvCatalogo.SetObjects(listaCatalogoOriginal);
            

            // 🔁 Sincroniza las cantidades máximas del carrito con el catálogo actualizado
            foreach (var insumo in olvCarrito.Objects.Cast<InsumoOrdenCompra>())
            {
                var actualizado = listaCatalogoOriginal.FirstOrDefault(i => i.Clave == insumo.Clave);
                if (actualizado != null)
                {
                    insumo.CantidadMaxima = actualizado.Cantidad;

                    if (insumo.Cantidad > insumo.CantidadMaxima)
                    {
                        insumo.Cantidad = insumo.CantidadMaxima;
                        MessageBox.Show($"Se ajustó {insumo.Clave} al nuevo máximo: {insumo.CantidadMaxima}");
                    }
                }
            }
            olvCarrito.BuildList();


        }

        
        



        private void btnGenerarOrden_Click(object sender, EventArgs e)
        {
            if (lstCasas.Items.Count == 0)
            {
                MessageBox.Show("Agrega al menos una casa.");
                return;
            }

            var lista = olvCarrito.Objects.Cast<InsumoOrdenCompra>().ToList();
            
            if (lista.Count == 0)
            {
                MessageBox.Show("Agrega al menos un insumo al carrito.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(cmbNombreProveedor.Text))
            {
                MessageBox.Show("Selecciona un proveedor.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 📝 SOLICITAR NOMBRE PERSONALIZADO PARA LA ORDEN
            string nombreOrden = string.Empty;
            using (Form formNombre = new Form())
            {
                formNombre.Text = "Nombre de la Orden de Compra";
                formNombre.StartPosition = FormStartPosition.CenterParent;
                formNombre.Size = new Size(450, 180);
                formNombre.FormBorderStyle = FormBorderStyle.FixedDialog;
                formNombre.MaximizeBox = false;
                formNombre.MinimizeBox = false;
                
                Label lblInstruccion = new Label
                {
                    Text = "Ingresa un nombre descriptivo para esta orden de compra:\n(Ejemplo: Materiales Manzana 5, Ferreteríaetc.)",
                    Location = new Point(15, 15),
                    Size = new Size(410, 40),
                    Font = new Font("Segoe UI", 9F)
                };
                
                TextBox txtNombre = new TextBox
                {
                    Location = new Point(15, 65),
                    Size = new Size(410, 25),
                    Font = new Font("Segoe UI", 10F),
                    Text = $"Orden Múltiple - {DateTime.Now:dd/MM/yyyy}" // Sugerencia por defecto
                };
                txtNombre.SelectAll();
                
                Button btnOk = new Button
                {
                    Text = "Aceptar",
                    DialogResult = DialogResult.OK,
                    Location = new Point(225, 105),
                    Size = new Size(100, 30),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                
                Button btnCancelar = new Button
                {
                    Text = "Cancelar",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(335, 105),
                    Size = new Size(90, 30),
                    Font = new Font("Segoe UI", 9F)
                };
                
                formNombre.Controls.AddRange(new Control[] { lblInstruccion, txtNombre, btnOk, btnCancelar });
                formNombre.AcceptButton = btnOk;
                formNombre.CancelButton = btnCancelar;
                
                if (formNombre.ShowDialog() == DialogResult.OK)
                {
                    nombreOrden = txtNombre.Text.Trim();
                    if (string.IsNullOrWhiteSpace(nombreOrden))
                        nombreOrden = $"Orden Múltiple - {DateTime.Now:dd/MM/yyyy}"; // Nombre por defecto
                }
                else
                {
                    return; // Usuario canceló
                }
            }

            // Generar folio
            string folioOC = GenerarFolioOC();
            string rutaPdf = string.Empty;

            // Guardar en SQL (tablas tradicionales)
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(
                        "INSERT INTO OrdenesCompra (FolioOC, Fecha, Usuario, TipoOrden, NombreOrden) VALUES (@folio, @fecha, @usuario, @tipo, @nombre)", conn))
                    {
                        cmd.Parameters.AddWithValue("@folio", folioOC);
                        cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                        cmd.Parameters.AddWithValue("@usuario", Environment.UserName);
                        cmd.Parameters.AddWithValue("@tipo", "MULTIPLE");
                        cmd.Parameters.AddWithValue("@nombre", nombreOrden);
                        cmd.ExecuteNonQuery();
                    }

                    foreach (CasaSeleccionada casa in lstCasas.Items)
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            "INSERT INTO OrdenesCompra_Casas (FolioOC, Manzana, Lote) VALUES (@folio, @m, @l)", conn))
                        {
                            cmd.Parameters.AddWithValue("@folio", folioOC);
                            cmd.Parameters.AddWithValue("@m", casa.Manzana);
                            cmd.Parameters.AddWithValue("@l", casa.Lote);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    foreach (var insumo in lista)
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            "INSERT INTO OrdenesCompraDetalle (FolioOC, Clave, Descripcion, Unidad, Cantidad, PrecioUnitario, ImporteTotal, Familia, Estado) " +
                            "VALUES (@folio, @c, @d, @u, @q, @precio, @importe, @f, 'PENDIENTE')", conn))
                        {
                            cmd.Parameters.AddWithValue("@folio", folioOC);
                            cmd.Parameters.AddWithValue("@c", insumo.Clave);
                            cmd.Parameters.AddWithValue("@d", insumo.Descripcion);
                            cmd.Parameters.AddWithValue("@u", insumo.Unidad);
                            cmd.Parameters.AddWithValue("@q", insumo.Cantidad);
                            cmd.Parameters.AddWithValue("@precio", insumo.Precio);
                            cmd.Parameters.AddWithValue("@importe", insumo.Importe);
                            cmd.Parameters.AddWithValue("@f", insumo.Familia);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            
            // Generar PDF con formato corporativo
            try
            {
                rutaPdf = GenerarPDFCorporativo(folioOC, lista);
                
                // 🔥 GUARDAR EN REPOSITORIO DE PDFs
                try
                {
                    var casas = lstCasas.Items.Cast<CasaSeleccionada>().ToList();
                    GuardarOrdenEnRepositorio(folioOC, rutaPdf, lista, casas);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"⚠ Advertencia: La orden se generó correctamente pero no se pudo guardar en el repositorio de PDFs:\n\n{ex.Message}", 
                        "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerarPDFCorporativo(string folioOC, List<InsumoOrdenCompra> insumos)
        {
            string carpetaPdf = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                "CALANDRIA RESIDENCIAL", "PDFOrdenesCompra");
            Directory.CreateDirectory(carpetaPdf);
            string rutaPdf = Path.Combine(carpetaPdf, $"OrdenCompra_{folioOC}.pdf");

            PdfDocument pdf = new PdfDocument();
            pdf.Info.Title = "Orden de Compra - " + folioOC;
            pdf.Info.Author = "Desarrolladora de Casas Camaney";
            pdf.Info.Subject = "Orden de Compra";
            pdf.Info.Creator = "Sistema Calandria";

            PdfPage page = pdf.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Definir fuentes
            XFont fontBold = new XFont("Arial", 9, XFontStyle.Bold);
            XFont fontNormal = new XFont("Arial", 8, XFontStyle.Regular);
            XFont fontSmall = new XFont("Arial", 7, XFontStyle.Regular);
            XFont fontTableHeader = new XFont("Arial", 8, XFontStyle.Bold);
            XFont fontTitle = new XFont("Arial", 12, XFontStyle.Bold);

            int margin = 30;
            int x = margin;
            int y = margin;
            int pageWidth = (int)page.Width - (margin * 2);

            // ==================== ENCABEZADO CON LOGO ====================
            // INFORMACIÓN DE LA EMPRESA (IZQUIERDA)
            gfx.DrawString("Desarrolladora de Casas Camaney", 
                fontNormal, XBrushes.Black, x, y);
            y += 12;
            
            gfx.DrawString("Blvd. Periférico sur. Y Carretera a la Colorada", 
                fontSmall, XBrushes.Black, x, y);
            y += 10;
            
            gfx.DrawString("Tel. 662xxxxxxxxxx", 
                fontSmall, XBrushes.Black, x, y);
            y += 10;
            
            gfx.DrawString("Email constcas@h.....com", 
                fontSmall, XBrushes.Black, x, y);

            // LOGO CALANDRIA (DERECHA)
            try
            {
                using (var ms = new MemoryStream())
                {
                    DynamicSepticSystem.Properties.Resources.Logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    using (var logo = XImage.FromStream(ms))
                    {
                        int logoWidth = 100;
                        int logoHeight = 67;
                        int logoX = (int)page.Width - margin - logoWidth;
                        gfx.DrawImage(logo, logoX, margin - 5, logoWidth, logoHeight);
                    }
                }
            }
            catch { }

            y = margin + 75;

            // ==================== TÍTULO ====================
            gfx.DrawString("ORDEN DE COMPRA", fontTitle, XBrushes.Black, 
                new XRect(margin, y, pageWidth, 20), XStringFormats.Center);
            y += 30;

            // ==================== INFORMACIÓN DEL PROVEEDOR Y FECHA ====================
            string nombreProv = cmbNombreProveedor.Text;
            string codigoProv = cmbCodigoProveedor.Text;

            // Proveedor (izquierda)
            gfx.DrawString("Proveedor", fontNormal, XBrushes.Black, x, y);
            gfx.DrawLine(XPens.Black, x + 70, y + 5, x + 280, y + 5);
            gfx.DrawString(nombreProv, fontNormal, XBrushes.Black, x + 72, y);

            // Folio (derecha)
            int folioX = (int)page.Width - margin - 180;
            gfx.DrawString("Folio", fontNormal, XBrushes.Black, folioX, y);
            gfx.DrawLine(XPens.Black, folioX + 35, y + 5, folioX + 180, y + 5);
            gfx.DrawString(folioOC, fontNormal, XBrushes.Black, folioX + 37, y);

            y += 20;

            // Código Prov (izquierda)
            gfx.DrawString("Codigo Prov", fontNormal, XBrushes.Black, x, y);
            gfx.DrawLine(XPens.Black, x + 70, y + 5, x + 280, y + 5);
            gfx.DrawString(codigoProv, fontNormal, XBrushes.Black, x + 72, y);

            // Fecha (derecha)
            gfx.DrawString("Fecha", fontNormal, XBrushes.Black, folioX, y);
            gfx.DrawLine(XPens.Black, folioX + 35, y + 5, folioX + 180, y + 5);
            gfx.DrawString(DateTime.Now.ToString("dd/MM/yyyy"), fontNormal, XBrushes.Black, folioX + 37, y);

            y += 35;

            // ==================== TABLA DE INSUMOS ====================
            string[] headers = { "CODIGO", "INSUMO", "UNIDAD", "CANTIDAD", "PRECIO", "IMPORTE" };
            int[] widths = { 70, 200, 55, 70, 75, 80 }; // Total = 550
            int totalTableWidth = 550;
            int headerRowHeight = 20;
            int dataRowHeight = 18;

            // Dibujar encabezado de tabla
            int currentX = x;
            for (int i = 0; i < headers.Length; i++)
            {
                XRect cellRect = new XRect(currentX, y, widths[i], headerRowHeight);
                gfx.DrawRectangle(XPens.Black, XBrushes.White, cellRect);
                gfx.DrawString(headers[i], fontTableHeader, XBrushes.Black, 
                    new XRect(currentX + 2, y + 5, widths[i] - 4, headerRowHeight - 5), 
                    XStringFormats.TopCenter);
                currentX += widths[i];
            }

            y += headerRowHeight;

            // Dibujar filas de datos
            decimal total = 0;

            foreach (var insumo in insumos)
            {
                // Verificar si necesitamos nueva página
                if (y + dataRowHeight > page.Height - 120)
                {
                    page = pdf.AddPage();
                    page.Size = PdfSharp.PageSize.Letter;
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                    
                    // Redibujar encabezado de tabla en nueva página
                    currentX = x;
                    for (int i = 0; i < headers.Length; i++)
                    {
                        XRect cellRect = new XRect(currentX, y, widths[i], headerRowHeight);
                        gfx.DrawRectangle(XPens.Black, XBrushes.White, cellRect);
                        gfx.DrawString(headers[i], fontTableHeader, XBrushes.Black, 
                            new XRect(currentX + 2, y + 5, widths[i] - 4, headerRowHeight - 5), 
                            XStringFormats.TopCenter);
                        currentX += widths[i];
                    }
                    y += headerRowHeight;
                }

                currentX = x;

                // Dibujar bordes de la fila
                XRect rowRect = new XRect(x, y, totalTableWidth, dataRowHeight);
                gfx.DrawRectangle(XPens.Black, XBrushes.White, rowRect);

                // CODIGO
                gfx.DrawString(insumo.Clave, fontSmall, XBrushes.Black, 
                    new XRect(currentX + 2, y + 4, widths[0] - 4, dataRowHeight - 4), 
                    XStringFormats.TopLeft);
                gfx.DrawLine(XPens.Black, currentX + widths[0], y, currentX + widths[0], y + dataRowHeight);
                currentX += widths[0];

                // INSUMO (DESCRIPCIÓN)
                string descCorta = insumo.Descripcion;
                if (descCorta.Length > 40)
                    descCorta = descCorta.Substring(0, 37) + "...";
                gfx.DrawString(descCorta, fontSmall, XBrushes.Black, 
                    new XRect(currentX + 2, y + 4, widths[1] - 4, dataRowHeight - 4), 
                    XStringFormats.TopLeft);
                gfx.DrawLine(XPens.Black, currentX + widths[1], y, currentX + widths[1], y + dataRowHeight);
                currentX += widths[1];

                // UNIDAD
                gfx.DrawString(insumo.Unidad, fontSmall, XBrushes.Black, 
                    new XRect(currentX + 2, y + 4, widths[2] - 4, dataRowHeight - 4), 
                    XStringFormats.TopCenter);
                gfx.DrawLine(XPens.Black, currentX + widths[2], y, currentX + widths[2], y + dataRowHeight);
                currentX += widths[2];

                // CANTIDAD
                gfx.DrawString(insumo.Cantidad.ToString("N2"), fontSmall, XBrushes.Black, 
                    new XRect(currentX + 2, y + 4, widths[3] - 4, dataRowHeight - 4), 
                    XStringFormats.TopCenter);
                gfx.DrawLine(XPens.Black, currentX + widths[3], y, currentX + widths[3], y + dataRowHeight);
                currentX += widths[3];

                // PRECIO
                gfx.DrawString(insumo.Precio.ToString("C2"), fontSmall, XBrushes.Black, 
                    new XRect(currentX + 2, y + 4, widths[4] - 4, dataRowHeight - 4), 
                    XStringFormats.TopRight);
                gfx.DrawLine(XPens.Black, currentX + widths[4], y, currentX + widths[4], y + dataRowHeight);
                currentX += widths[4];

                // IMPORTE
                gfx.DrawString(insumo.Importe.ToString("C2"), fontSmall, XBrushes.Black, 
                    new XRect(currentX + 2, y + 4, widths[5] - 4, dataRowHeight - 4), 
                    XStringFormats.TopRight);

                y += dataRowHeight;
                total += insumo.Importe;
            }

            // Línea final de la tabla
            gfx.DrawLine(XPens.Black, x, y, x + totalTableWidth, y);

            // ==================== TOTALES (SUBTOTAL, IVA, TOTAL) ====================
            y += 20;
            
            // Calcular subtotal, IVA y total
            decimal subtotal = total;
            decimal iva = Math.Round(subtotal * 0.16m, 2); // IVA 16%
            decimal totalConIVA = subtotal + iva;

            // Definir ancho de la caja de totales y posiciones
            int totalesBoxWidth = 250;
            int totalesBoxX = (int)page.Width - margin - totalesBoxWidth;
            int labelWidth = 120;
            int valueWidth = 130;

            // Subtotal
            gfx.DrawString("SUBTOTAL:", fontBold, XBrushes.Black, 
                new XRect(totalesBoxX, y, labelWidth, 15), XStringFormats.CenterLeft);
            gfx.DrawString(subtotal.ToString("C2"), fontNormal, XBrushes.Black, 
                new XRect(totalesBoxX + labelWidth, y, valueWidth, 15), XStringFormats.CenterRight);
            y += 18;

            // IVA 16%
            gfx.DrawString("IVA (16%):", fontBold, XBrushes.Black, 
                new XRect(totalesBoxX, y, labelWidth, 15), XStringFormats.CenterLeft);
            gfx.DrawString(iva.ToString("C2"), fontNormal, XBrushes.Black, 
                new XRect(totalesBoxX + labelWidth, y, valueWidth, 15), XStringFormats.CenterRight);
            y += 18;

            // Línea separadora antes del total
            gfx.DrawLine(XPens.Black, totalesBoxX, y, totalesBoxX + totalesBoxWidth, y);
            y += 5;

            // Total con fondo resaltado
            XRect totalRect = new XRect(totalesBoxX, y, totalesBoxWidth, 22);
            gfx.DrawRectangle(XPens.Black, XBrushes.LightGray, totalRect);
            
            gfx.DrawString("TOTAL:", fontBold, XBrushes.Black, 
                new XRect(totalesBoxX + 5, y + 5, labelWidth - 5, 15), XStringFormats.CenterLeft);
            gfx.DrawString(totalConIVA.ToString("C2"), fontBold, XBrushes.Black, 
                new XRect(totalesBoxX + labelWidth, y + 5, valueWidth - 5, 15), XStringFormats.CenterRight);

            // ==================== FIRMAS ====================
            y = (int)page.Height - margin - 70;
            
            int firmaWidth = 200;
            int firma1X = margin + 50;

            // Línea de firma
            gfx.DrawLine(XPens.Black, firma1X, y, firma1X + firmaWidth, y);
            y += 5;
            
            // Nombre y Firma
            gfx.DrawString("Nombre y Firma", fontSmall, XBrushes.Black, 
                new XRect(firma1X, y, firmaWidth, 15), XStringFormats.TopCenter);
            y += 12;
            
            // Encargado de Compras
            gfx.DrawString("Encargado de Compras", fontSmall, XBrushes.Black, 
                new XRect(firma1X, y, firmaWidth, 15), XStringFormats.TopCenter);

            // Guardar y abrir PDF
            try
            {
                pdf.Save(rutaPdf);
                pdf.Close();
                
                MessageBox.Show($"Orden de compra generada exitosamente.\nFolio: {folioOC}", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo guardar el PDF: " + ex.Message, ex);
            }

            try
            {
                var psi = new ProcessStartInfo(rutaPdf) { UseShellExecute = true };
                Process.Start(psi);
            }
            catch
            {
                try { Process.Start("explorer.exe", carpetaPdf); } catch { }
            }
            
            return rutaPdf;
        }

        private void btnAgregarLote_Click(object sender, EventArgs e)
        {
            string manzana = txtManzana.Text.Trim();
            string lote = txtLote.Text.Trim();

            if (string.IsNullOrEmpty(manzana) || string.IsNullOrEmpty(lote))
            {
                MessageBox.Show("Ingresa manzana y lote.");
                return;
            }

            string prototipo = ObtenerPrototipoDesdeBD(manzana, lote);
            if (string.IsNullOrEmpty(prototipo))
            {
                MessageBox.Show("No se encontró el prototipo para esta casa.");
                return;
            }

            foreach (CasaSeleccionada item in lstCasas.Items)
            {
                if (item.Manzana == manzana && item.Lote == lote)
                {
                    MessageBox.Show("Esta casa ya fue agregada.");
                    return;
                }
            }

            lstCasas.Items.Add(new CasaSeleccionada
            {
                Manzana = manzana,
                Lote = lote,
                Prototipo = prototipo
            });

            txtManzana.Clear();
            txtLote.Clear();
            ActualizarCatalogoInsumos();

        }

        private string GenerarFolioOC()
        {
            string baseFolio = "OC-MULTI-" + DateTime.Now.ToString("yyyyMMdd") + "-";
            int consecutivo = 1;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM OrdenesCompra WHERE FolioOC LIKE @base + '%'", conn))
                {
                    cmd.Parameters.AddWithValue("@base", baseFolio);
                    consecutivo += (int)cmd.ExecuteScalar();
                }
            }

            return baseFolio + consecutivo.ToString("D3");
        }
        private void RecalcularInsumos()
        {
            if (lstCasas.Items.Count == 0)
            {
                olvCatalogo.ClearObjects();
                return;
            }

            Dictionary<string, int> casasPorPrototipo = new Dictionary<string, int>();
            foreach (CasaSeleccionada casa in lstCasas.Items)
            {
                if (!casasPorPrototipo.ContainsKey(casa.Prototipo))
                    casasPorPrototipo[casa.Prototipo] = 0;
                casasPorPrototipo[casa.Prototipo]++;
            }

            Dictionary<string, InsumoOrdenCompra> insumosTotales = new Dictionary<string, InsumoOrdenCompra>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                foreach (var kvp in casasPorPrototipo)
                {
                    string prototipo = kvp.Key;
                    int cantidadCasas = kvp.Value;

                    string tabla = GetExplosionTableForPrototipo(prototipo);

                    using (SqlCommand cmd = new SqlCommand($"SELECT Clave, Descripcion, Unidad, Cantidad, Familia FROM {tabla}", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string clave = ReadStringFromReader(reader, "Clave");
                                string desc = ReadStringFromReader(reader, "Descripcion");
                                string unidad = ReadStringFromReader(reader, "Unidad");
                                string familia = ReadStringFromReader(reader, "Familia");

                                decimal porCasa = 0;
                                string cantidadCol = ReaderHasColumn(reader, "Cantidad") ? "Cantidad" : (ReaderHasColumn(reader, "cantidad") ? "cantidad" : null);
                                object cantidadObj = DBNull.Value;
                                if (cantidadCol != null) cantidadObj = reader[cantidadCol];
                                if (cantidadObj != DBNull.Value) decimal.TryParse(cantidadObj.ToString(), out porCasa);

                                // Leer precio/costo si existe
                                decimal precio = 0m;
                                string precioCol = null;
                                if (ReaderHasColumn(reader, "Costo")) precioCol = "Costo";
                                else if (ReaderHasColumn(reader, "CostoUnitario")) precioCol = "CostoUnitario";
                                else if (ReaderHasColumn(reader, "Precio")) precioCol = "Precio";
                                else if (ReaderHasColumn(reader, "precio")) precioCol = "precio";
                                else if (ReaderHasColumn(reader, "costo")) precioCol = "costo";

                                if (precioCol != null)
                                {
                                    try
                                    {
                                        var precioObj = reader[precioCol];
                                        if (precioObj != DBNull.Value) decimal.TryParse(precioObj.ToString(), out precio);
                                    }
                                    catch { }
                                }

                                if (insumosTotales.ContainsKey(clave))
                                    insumosTotales[clave].Cantidad += porCasa * cantidadCasas;
                                else
                                    insumosTotales[clave] = new InsumoOrdenCompra
                                    {
                                        Clave = clave,
                                        Descripcion = desc,
                                        Unidad = unidad,
                                        Cantidad = porCasa * cantidadCasas,
                                        Familia = familia,
                                        Precio = precio // asignar precio leido
                                    };
                            }
                        }
                    }
                }

                var lista = insumosTotales.Values.ToList();
                olvCatalogo.ShowGroups = true;
                olvCatalogo.SetObjects(lista);
            }
        }

        private void AplicarAgrupamientoPorFamilia()
        {
            var colFamilia = olvCatalogo.AllColumns.FirstOrDefault(c => c.Text == "Familia");
            if (colFamilia != null)
            {
                olvCatalogo.PrimarySortColumn = colFamilia;
                olvCatalogo.PrimarySortOrder = System.Windows.Forms.SortOrder.Ascending;
                olvCatalogo.Sort();
            }
        }

        private void ConfigurarListas()
        {
            // 🧾 Catálogo
            olvCatalogo.FullRowSelect = true;
            olvCatalogo.ShowGroups = true;
            olvCatalogo.Columns.Clear();

            olvCatalogo.FormatRow += (sender, e) =>
            {
                var insumo = e.Model as InsumoOrdenCompra;
                if (insumo != null && insumo.EsModificado)
                {
                    e.Item.BackColor = Color.LightGoldenrodYellow;
                }
            };

            olvCatalogo.Columns.Add(new OLVColumn("Clave", "Clave") { Width = 100 });
            olvCatalogo.Columns.Add(new OLVColumn("Descripción", "Descripcion") { Width = 200 });
            olvCatalogo.Columns.Add(new OLVColumn("Unidad", "Unidad") { Width = 60 });
            var colCantidadCatalogo = new OLVColumn("Cantidad", "Cantidad") { Width = 80, IsEditable = true };
            colCantidadCatalogo.AspectPutter = (row, value) =>
            {
                if (decimal.TryParse(value?.ToString(), out decimal nuevaTotal))
                {
                    var insumo = (InsumoOrdenCompra)row;
                    // Confirmar si desea aplicar la modificación a la explosión
                    var resp = MessageBox.Show($"¿Deseas aplicar el nuevo total ({nuevaTotal}) a la explosión de insumos para el prototipo seleccionado?\n\n" +
                        "Nota: Para evitar ambigüedad, esta operación solo está permitida cuando las casas seleccionadas pertenecen a un único prototipo.",
                        "Aplicar a explosión?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (resp == DialogResult.Cancel) return;

                    // Actualizar valor en memoria
                    insumo.Cantidad = Math.Round(nuevaTotal, 3);

                    if (resp == DialogResult.Yes)
                    {
                        // Verificar que las casas seleccionadas pertenezcan a un único prototipo
                        var prototipos = lstCasas.Items.Cast<CasaSeleccionada>().Select(c => c.Prototipo).Distinct().ToList();
                        if (prototipos.Count != 1)
                        {
                            MessageBox.Show("Selecciona casas de un solo prototipo para aplicar cambios a la explosión.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            string prototipo = prototipos[0];
                            int cantidadCasas = lstCasas.Items.Cast<CasaSeleccionada>().Count(c => c.Prototipo == prototipo);

                            decimal cantidadPorCasa = 0;
                            if (cantidadCasas > 0)
                                cantidadPorCasa = Math.Round(nuevaTotal / cantidadCasas, 6);

                            // Aplicar a la tabla de explosión
                            string tabla = GetExplosionTableForPrototipo(prototipo);
                            try
                            {
                                using (SqlConnection conn = new SqlConnection(connectionString))
                                {
                                    conn.Open();
                                    using (SqlCommand cmdCheck = new SqlCommand($"SELECT COUNT(*) FROM {tabla} WHERE Clave = @clave", conn))
                                    {
                                        cmdCheck.Parameters.AddWithValue("@clave", insumo.Clave);
                                        int existe = (int)cmdCheck.ExecuteScalar();
                                        if (existe > 0)
                                        {
                                            using (SqlCommand cmdUpdate = new SqlCommand($"UPDATE {tabla} SET Cantidad = @cantidad, Descripcion = @desc, Unidad = @unidad, Familia = @familia WHERE Clave = @clave", conn))
                                            {
                                                cmdUpdate.Parameters.AddWithValue("@cantidad", cantidadPorCasa);
                                                cmdUpdate.Parameters.AddWithValue("@desc", insumo.Descripcion ?? string.Empty);
                                                cmdUpdate.Parameters.AddWithValue("@unidad", insumo.Unidad ?? string.Empty);
                                                cmdUpdate.Parameters.AddWithValue("@familia", insumo.Familia ?? "MANUAL");
                                                cmdUpdate.Parameters.AddWithValue("@clave", insumo.Clave);
                                                cmdUpdate.ExecuteNonQuery();
                                            }
                                        }
                                        else
                                        {
                                            using (SqlCommand cmdInsert = new SqlCommand($"INSERT INTO {tabla} (Clave, Descripcion, Unidad, Cantidad, Familia) VALUES (@clave, @desc, @unidad, @cantidad, @familia)", conn))
                                            {
                                                cmdInsert.Parameters.AddWithValue("@clave", insumo.Clave);
                                                cmdInsert.Parameters.AddWithValue("@desc", insumo.Descripcion ?? string.Empty);
                                                cmdInsert.Parameters.AddWithValue("@unidad", insumo.Unidad ?? string.Empty);
                                                cmdInsert.Parameters.AddWithValue("@cantidad", cantidadPorCasa);
                                                cmdInsert.Parameters.AddWithValue("@familia", insumo.Familia ?? "MANUAL");
                                                cmdInsert.ExecuteNonQuery();
                                            }
                                        }
                                    }

                                    // Intentar actualizar Precio si existe la columna (no crítico)
                                    try
                                    {
                                        using (SqlCommand cmdPrecio = new SqlCommand($"UPDATE {tabla} SET Costo = @precio WHERE Clave = @clave", conn))
                                        {
                                            cmdPrecio.Parameters.AddWithValue("@precio", insumo.Precio);
                                            cmdPrecio.Parameters.AddWithValue("@clave", insumo.Clave);
                                            cmdPrecio.ExecuteNonQuery();
                                        }
                                    }
                                    catch { /* ignorar si no existe columna Costo */ }
                                }

                                MessageBox.Show($"Explosión actualizada en {tabla} (cantidad por casa: {cantidadPorCasa}).", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error actualizando explosión: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            };
            olvCatalogo.Columns.Add(colCantidadCatalogo);
            
            // Precio en catálogo (editable)
            var colPrecioCatalogo = new OLVColumn("Precio", "Precio") { Width = 90, IsEditable = true, AspectToStringFormat = "{0:C}" };
            colPrecioCatalogo.AspectPutter = (row, value) =>
            {
                if (decimal.TryParse(value?.ToString(), out decimal nuevoPrecio))
                {
                    var insumo = (InsumoOrdenCompra)row;
                    insumo.Precio = nuevoPrecio;

                    // Preguntar si desea aplicar precio a la explosión (opcional)
                    var resp = MessageBox.Show($"¿Deseas aplicar el nuevo precio ({nuevoPrecio:C}) a la explosión de insumos para el prototipo seleccionado?\n\n" +
                        "Nota: Para evitar ambigüedad, esta operación solo está permitida cuando las casas seleccionadas pertenecen a un único prototipo.",
                        "Aplicar precio a explosión?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (resp == DialogResult.Yes)
                    {
                        var prototipos = lstCasas.Items.Cast<CasaSeleccionada>().Select(c => c.Prototipo).Distinct().ToList();
                        if (prototipos.Count != 1)
                        {
                            MessageBox.Show("Selecciona casas de un prototipo para aplicar el precio en la explosión.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            string prototipo = prototipos[0];
                            string tabla = GetExplosionTableForPrototipo(prototipo);
                            try
                            {
                                using (SqlConnection conn = new SqlConnection(connectionString))
                                {
                                    conn.Open();
                                    // Intentar actualizar o insertar Precio if column exists
                                    using (SqlCommand cmdCheck = new SqlCommand($"SELECT COUNT(*) FROM {tabla} WHERE Clave = @clave", conn))
                                    {
                                        cmdCheck.Parameters.AddWithValue("@clave", insumo.Clave);
                                        int existe = (int)cmdCheck.ExecuteScalar();
                                        if (existe == 0)
                                        {
                                            using (SqlCommand cmdInsert = new SqlCommand($"INSERT INTO {tabla} (Clave, Descripcion, Unidad, Cantidad, Familia) VALUES (@clave, @desc, @unidad, @cantidad, @familia)", conn))
                                            {
                                                cmdInsert.Parameters.AddWithValue("@clave", insumo.Clave);
                                                cmdInsert.Parameters.AddWithValue("@desc", insumo.Descripcion ?? string.Empty);
                                                cmdInsert.Parameters.AddWithValue("@unidad", insumo.Unidad ?? string.Empty);
                                                cmdInsert.Parameters.AddWithValue("@cantidad", 0);
                                                cmdInsert.Parameters.AddWithValue("@familia", insumo.Familia ?? "MANUAL");
                                                cmdInsert.ExecuteNonQuery();
                                            }
                                        }
                                    }

                                    try
                                    {
                                        using (SqlCommand cmdPrecio = new SqlCommand($"UPDATE {tabla} SET Costo = @precio WHERE Clave = @clave", conn))
                                        {
                                            cmdPrecio.Parameters.AddWithValue("@precio", insumo.Precio);
                                            cmdPrecio.Parameters.AddWithValue("@clave", insumo.Clave);
                                            cmdPrecio.ExecuteNonQuery();
                                        }
                                    }
                                    catch { MessageBox.Show("La tabla de explosión no tiene columna 'Costo'. El costo fue actualizado solo en el catálogo.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error aplicando precio en explosión: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            };
            olvCatalogo.Columns.Add(colPrecioCatalogo);
            olvCatalogo.Columns.Add(new OLVColumn("Familia", "Familia")
            {
                Width = 100,
                GroupKeyGetter = row => ((InsumoOrdenCompra)row).Familia
            });

            // 🛒 Carrito
            olvCarrito.FullRowSelect = true;
            olvCarrito.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;
            olvCarrito.ShowGroups = true;
            olvCarrito.Columns.Clear();

            var colCantidad = new OLVColumn("Cantidad", "Cantidad")
            {
                Width = 80,
                IsEditable = true,
                AspectPutter = (row, value) =>
                {
                    if (decimal.TryParse(value?.ToString(), out decimal nuevaCantidad))
                    {
                        var insumo = (InsumoOrdenCompra)row;
                        if (nuevaCantidad <= insumo.CantidadMaxima)
                            insumo.Cantidad = nuevaCantidad;
                        else
                            MessageBox.Show($"La cantidad máxima permitida para {insumo.Clave} es: {insumo.CantidadMaxima}");
                    }
                }
            };

            var colPrecio = new OLVColumn("Precio", "Precio")
            {
                Width = 80,
                IsEditable = true,
                AspectToStringFormat = "{0:C}",
                AspectGetter = row => ((InsumoOrdenCompra)row).Precio,
                AspectPutter = (row, value) =>
                {
                    if (decimal.TryParse(value?.ToString(), out decimal nuevoPrecio))
                    {
                        ((InsumoOrdenCompra)row).Precio = nuevoPrecio;
                    }
                }
            };

            var colImporte = new OLVColumn("Importe", "Importe")
            {
                Width = 100,
                AspectToStringFormat = "{0:C}"
            };

            olvCarrito.Columns.AddRange(new[] {
        new OLVColumn("Clave", "Clave") { Width = 100 },
        new OLVColumn("Descripción", "Descripcion") { Width = 200 },
        new OLVColumn("Unidad", "Unidad") { Width = 60 },
        colCantidad,
        colPrecio,
        colImporte
    });
        }

        private void olvCatalogo_DoubleClick(object sender, EventArgs e)
        {
            var insumo = olvCatalogo.SelectedObject as InsumoOrdenCompra;
            if (insumo == null) return;

            // Evitar duplicados en el carrito
            if (olvCarrito.Objects.Cast<InsumoOrdenCompra>().Any(i => i.Clave == insumo.Clave))
            {
                MessageBox.Show("Este insumo ya está en el carrito.");
                return;
            }

            // Clonar el insumo seleccionado
            var copia = new InsumoOrdenCompra
            {
                Clave = insumo.Clave,
                Descripcion = insumo.Descripcion,
                Unidad = insumo.Unidad,
                Cantidad = insumo.Cantidad,
                CantidadMaxima = insumo.Cantidad, // 👈 AQUÍ se asigna el tope
                Familia = insumo.Familia,
                Precio = insumo.Precio
            };

            var lista = olvCarrito.Objects.Cast<InsumoOrdenCompra>().ToList();
            lista.Add(copia);
            olvCarrito.SetObjects(lista);
        }

        private void btnEliminarCasa_Click(object sender, EventArgs e)
        {
            if (lstCasas.SelectedItem != null)
            {
                lstCasas.Items.Remove(lstCasas.SelectedItem);
                RecalcularInsumos(); // <- actualizar visualización
                ActualizarCatalogoInsumos();
            }
        }

        private List<InsumoOrdenCompra> listaCatalogoOriginal = new List<InsumoOrdenCompra>();

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            var tb = sender as TextBox;
            string filtro = tb.Text.Trim().ToLower();

            if (listaCatalogoOriginal == null || listaCatalogoOriginal.Count == 0)
                return;

            if (string.IsNullOrWhiteSpace(filtro))
            {
                olvCatalogo.SetObjects(listaCatalogoOriginal);
                // 🔁 Actualizar límites en el carrito
                foreach (var insumo in olvCarrito.Objects.Cast<InsumoOrdenCompra>())
                {
                    if (listaCatalogoOriginal.FirstOrDefault(i => i.Clave == insumo.Clave) is InsumoOrdenCompra original)
                    {
                        insumo.CantidadMaxima = original.Cantidad;

                        if (insumo.Cantidad > insumo.CantidadMaxima)
                        {
                            MessageBox.Show($"La cantidad de {insumo.Clave} excede el nuevo límite ({insumo.CantidadMaxima}). Será ajustada.");
                            insumo.Cantidad = insumo.CantidadMaxima;
                        }
                    }
                }
                olvCarrito.BuildList();

                olvCatalogo.ShowGroups = true;
            }
            else
            {
                var filtrados = listaCatalogoOriginal.Where(insumo =>
                    insumo.Clave.ToLower().Contains(filtro) ||
                    insumo.Descripcion.ToLower().Contains(filtro)).ToList();

                olvCatalogo.SetObjects(filtrados);
                olvCatalogo.ShowGroups = false;
            }

            olvCatalogo.BuildList();
        }



        public class InsumoOrdenCompra
        {
            public string Clave { get; set; }
            public string Descripcion { get; set; }
            public string Unidad { get; set; }
            public decimal Cantidad { get; set; }
            public string Familia { get; set; }

            public decimal Precio { get; set; } = 0;
            public decimal Importe => Math.Round(Precio * Cantidad, 2);
            public bool EsModificado { get; set; } = false; // NUEVO
            public decimal CantidadMaxima { get; set; }  // límite para la edición


        }

        public class CasaSeleccionada
        {
            public string Manzana { get; set; }
            public string Lote { get; set; }
            public string Prototipo { get; set; }

            public override string ToString()
            {
                return $"M{Manzana}-L{Lote} ({Prototipo})";
            }
        }
        private void CargarProveedores()
        {
            // Tabla unificada PROVEEDORESCALANDRIA, ahora vía API (ordenada por Nombre).
            cmbCodigoProveedor.Items.Clear();
            cmbNombreProveedor.Items.Clear();

            try
            {
                _proveedores = ApiClient.Get<List<ProveedorApi>>("/api/proveedores")
                               ?? new List<ProveedorApi>();
            }
            catch (Exception ex)
            {
                _proveedores = new List<ProveedorApi>();
                MessageBox.Show("No se pudieron cargar los proveedores: " + ex.Message, "Proveedores",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            foreach (var p in _proveedores)
            {
                cmbCodigoProveedor.Items.Add(p.ClaveUnica ?? "");
                cmbNombreProveedor.Items.Add(p.Nombre ?? "");
            }

            cmbCodigoProveedor.SelectedIndex = -1;
            cmbNombreProveedor.SelectedIndex = -1;

            // conectar manejadores si no están conectados
            cmbCodigoProveedor.SelectedIndexChanged -= cmbCodigoProveedor_SelectedIndexChanged;
            cmbNombreProveedor.SelectedIndexChanged -= cmbNombreProveedor_SelectedIndexChanged;
            cmbCodigoProveedor.SelectedIndexChanged += cmbCodigoProveedor_SelectedIndexChanged;
            cmbNombreProveedor.SelectedIndexChanged += cmbNombreProveedor_SelectedIndexChanged;
        }

        private void cmbCodigoProveedor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCodigoProveedor.SelectedItem == null) return;
            string clave = cmbCodigoProveedor.SelectedItem.ToString();

            var prov = _proveedores.FirstOrDefault(p =>
                string.Equals(p.ClaveUnica, clave, StringComparison.OrdinalIgnoreCase));
            if (prov == null) return;

            SetProveedorSelectionByName(prov.Nombre);
            MostrarDetalleProveedor(prov);
        }

        private void cmbNombreProveedor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbNombreProveedor.SelectedItem == null) return;
            string nombre = cmbNombreProveedor.SelectedItem.ToString();

            // seleccionar código correspondiente si existe
            SetProveedorSelectionByName(nombre);

            var prov = _proveedores.FirstOrDefault(p =>
                string.Equals(p.Nombre, nombre, StringComparison.OrdinalIgnoreCase));
            if (prov == null) return;

            cmbCodigoProveedor.SelectedItem = prov.ClaveUnica;
            MostrarDetalleProveedor(prov);
        }

        private void MostrarDetalleProveedor(ProveedorApi prov)
        {
            lblProvNombre.Text = "Nombre: " + (prov.Nombre ?? "");
            lblProvRFC.Text = "RFC: " + (prov.Rfc ?? "");
            lblProvDireccion.Text = "Dirección: " + (prov.Direccion ?? "");
            lblProvTelefono.Text = "Tel: " + (prov.Telefono ?? "");
        }

        private void SetProveedorSelectionByName(string nombre)
        {
            // buscar índice del nombre en el combo de nombres y sincronizar
            int idx = cmbNombreProveedor.Items.IndexOf(nombre);
            if (idx >= 0)
                cmbNombreProveedor.SelectedIndex = idx;

            // intentar cargar código asociado
            // buscamos en la lista de items por posición (asumimos correspondencia por orden de carga)
            int pos = cmbNombreProveedor.Items.IndexOf(nombre);
            if (pos >= 0 && pos < cmbCodigoProveedor.Items.Count)
            {
                cmbCodigoProveedor.SelectedIndex = pos;
            }
        }

        private void btnAgregarProveedor_Click(object sender, EventArgs e)
        {
            FormAgregarProveedor f = new FormAgregarProveedor();
            f.ShowDialog();

            // Recargar proveedores después de cerrar
            CargarProveedores();
        }

        private void olvCarrito_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hit = olvCarrito.OlvHitTest(e.X, e.Y);
            if (hit?.RowObject != null)
            {
                var insumo = (InsumoOrdenCompra)hit.RowObject;
                olvCarrito.RemoveObject(insumo); // o con confirmación si quieres
            }
        }

        private void btnEliminarInsumo_Click(object sender, EventArgs e)
        {
            // Obtener insumo seleccionado en el carrito
            var seleccionado = olvCarrito.SelectedObject as InsumoOrdenCompra;
            if (seleccionado == null)
            {
                MessageBox.Show("Selecciona un insumo en el carrito para eliminarlo de la explosión.", "Eliminar Insumo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show($"¿Deseas eliminar permanentemente el insumo '{seleccionado.Clave}' de las tablas de explosión para los prototipos seleccionados?\n\nEsta acción no se puede deshacer.",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            // Obtener prototipos únicos de las casas seleccionadas
            var prototiposUnicos = lstCasas.Items.Cast<CasaSeleccionada>()
                .Select(c => c.Prototipo)
                .Distinct()
                .ToList();

            if (prototiposUnicos.Count == 0)
            {
                MessageBox.Show("No hay casas seleccionadas para determinar las tablas de explosión.", "Eliminar Insumo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int eliminados = 0;
            List<string> errores = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                foreach (var prototipo in prototiposUnicos)
                {
                    string tabla = GetExplosionTableForPrototipo(prototipo);
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand($"DELETE FROM {tabla} WHERE Clave = @clave", conn))
                        {
                            cmd.Parameters.AddWithValue("@clave", seleccionado.Clave);
                            int aff = cmd.ExecuteNonQuery();
                            eliminados += aff;
                        }
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"{tabla}: {ex.Message}");
                    }
                }
            }

            // Actualizar catálogo y carrito
            ActualizarCatalogoInsumos();

            if (errores.Count > 0)
            {
                MessageBox.Show($"Se eliminaron {eliminados} filas.\n\nErrores:\n{string.Join("\n", errores)}", "Eliminar Insumo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show($"Insumo '{seleccionado.Clave}' eliminado correctamente de {prototiposUnicos.Count} tabla(s).\nFilas afectadas: {eliminados}", "Eliminar Insumo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Remover del carrito si aún presente
            try
            {
                var lista = olvCarrito.Objects.Cast<InsumoOrdenCompra>().ToList();
                var rem = lista.FirstOrDefault(i => i.Clave == seleccionado.Clave);
                if (rem != null)
                {
                    lista.Remove(rem);
                    olvCarrito.SetObjects(lista);
                }
            }
            catch { }
        }

        private void FormCompraMulti_Load(object sender, EventArgs e)
        {

        }

        private void btnVerRepositorio_Click(object sender, EventArgs e)
        {
            AbrirRepositorioPDFsOrdenesCompra();
        }

        /// <summary>
        /// Abre el formulario para agregar un nuevo insumo ÚNICAMENTE AL CARRITO (no a la explosión)
        /// ACTUALIZADO: Ahora pregunta si desea agregar el insumo a la explosión de insumos
        /// </summary>
        private void btnAgregarInsumo_Click(object sender, EventArgs e)
        {
            using (var form = new FormAgregarInsumoCarrito())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    string clave = form.Clave;
                    string descripcion = form.Descripcion;
                    string unidad = form.Unidad;
                    decimal cantidad = form.Cantidad;
                    decimal precio = form.Precio;

                    // Verificar que no exista ya en el carrito
                    if (olvCarrito.Objects.Cast<InsumoOrdenCompra>().Any(i => i.Clave == clave))
                    {
                        MessageBox.Show($"El insumo '{clave}' ya existe en el carrito.", 
                            "Insumo duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Preguntar si desea agregar a la explosión de insumos
                    if (lstCasas.Items.Count > 0)
                    {
                        var resultado = MessageBox.Show(
                            $"¿Deseas agregar este insumo a la explosión de insumos de los prototipos seleccionados?\n\n" +
                            $"Si seleccionas 'Sí', el insumo '{clave}' se agregará permanentemente a las tablas de explosión " +
                            $"y estará disponible para futuras órdenes de compra de estos prototipos.\n\n" +
                            $"Si seleccionas 'No', el insumo solo se agregará al carrito de esta orden.",
                            "Actualizar explosión de insumos",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (resultado == DialogResult.Cancel)
                        {
                            return; // Usuario canceló
                        }
                        else if (resultado == DialogResult.Yes)
                        {
                            // Agregar a la explosión de insumos
                            AgregarInsumoAExplosion(clave, descripcion, unidad, cantidad);
                        }
                    }

                    // Crear nuevo insumo y agregarlo al carrito
                    var nuevoInsumo = new InsumoOrdenCompra
                    {
                        Clave = clave,
                        Descripcion = descripcion,
                        Unidad = unidad,
                        Cantidad = cantidad,
                        CantidadMaxima = cantidad, // El máximo es lo que el usuario ingresó
                        Familia = "MANUAL", // Marcar como agregado manualmente
                        Precio = precio,
                        EsModificado = true // Marcar como insumo agregado manualmente
                    };

                    var lista = olvCarrito.Objects.Cast<InsumoOrdenCompra>().ToList();
                    lista.Add(nuevoInsumo);
                    olvCarrito.SetObjects(lista);

                    MessageBox.Show($"Insumo '{clave}' agregado al carrito exitosamente.", 
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// Agrega un nuevo insumo a las tablas de explosión de los prototipos de las casas seleccionadas
        /// </summary>
        private void AgregarInsumoAExplosion(string clave, string descripcion, string unidad, decimal cantidadPorCasa)
        {
            // Obtener prototipos únicos de las casas seleccionadas
            var prototiposUnicos = lstCasas.Items.Cast<CasaSeleccionada>()
                .Select(c => c.Prototipo)
                .Distinct()
                .ToList();

            if (prototiposUnicos.Count == 0)
            {
                MessageBox.Show("No hay casas seleccionadas.", "Atención", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int insumosAgregados = 0;
            List<string> errores = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                foreach (string prototipo in prototiposUnicos)
                {
                    string tabla = GetExplosionTableForPrototipo(prototipo);

                    try
                    {
                        // Verificar si el insumo ya existe en esta tabla
                        using (SqlCommand cmdCheck = new SqlCommand(
                            $"SELECT COUNT(*) FROM {tabla} WHERE Clave = @clave", conn))
                        {
                            cmdCheck.Parameters.AddWithValue("@clave", clave);
                            int existe = (int)cmdCheck.ExecuteScalar();

                            if (existe > 0)
                            {
                                // Actualizar la cantidad existente
                                using (SqlCommand cmdUpdate = new SqlCommand(
                                    $"UPDATE {tabla} SET Descripcion = @desc, Unidad = @unidad, Cantidad = @cantidad, Familia = @familia WHERE Clave = @clave", conn))
                                {
                                    cmdUpdate.Parameters.AddWithValue("@clave", clave);
                                    cmdUpdate.Parameters.AddWithValue("@desc", descripcion);
                                    cmdUpdate.Parameters.AddWithValue("@unidad", unidad);
                                    cmdUpdate.Parameters.AddWithValue("@cantidad", cantidadPorCasa);
                                    cmdUpdate.Parameters.AddWithValue("@familia", "MANUAL");
                                    cmdUpdate.ExecuteNonQuery();
                                    insumosAgregados++;
                                }
                            }
                            else
                            {
                                // Insertar nuevo insumo
                                using (SqlCommand cmdInsert = new SqlCommand(
                                    $"INSERT INTO {tabla} (Clave, Descripcion, Unidad, Cantidad, Familia) VALUES (@clave, @desc, @unidad, @cantidad, @familia)", conn))
                                {
                                    cmdInsert.Parameters.AddWithValue("@clave", clave);
                                    cmdInsert.Parameters.AddWithValue("@desc", descripcion);
                                    cmdInsert.Parameters.AddWithValue("@unidad", unidad);
                                    cmdInsert.Parameters.AddWithValue("@cantidad", cantidadPorCasa);
                                    cmdInsert.Parameters.AddWithValue("@familia", "MANUAL");
                                    cmdInsert.ExecuteNonQuery();
                                    insumosAgregados++;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Error en {tabla}: {ex.Message}");
                    }
                }
            }

            // Actualizar el catálogo de insumos para reflejar los cambios
            ActualizarCatalogoInsumos();

            // Mostrar resultado
            if (errores.Count > 0)
            {
                MessageBox.Show(
                    $"✅ Se agregaron {insumosAgregados} insumos a la explosión.\n\n" +
                    $"⚠ Errores encontrados:\n{string.Join("\n", errores)}",
                    "Resultado parcial",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show(
                    $"✅ Insumo '{clave}' agregado exitosamente a {insumosAgregados} tabla(s) de explosión.\n\n" +
                    $"Prototipos actualizados: {string.Join(", ", prototiposUnicos)}",
                    "Éxito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void editarInsumoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var insumo = olvCatalogo.SelectedObject as InsumoOrdenCompra;
            if (insumo == null)
            {
                MessageBox.Show("Selecciona un insumo en el catálogo para editar.", "Editar Insumo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Usar un formulario de edición simple (reusar FormAgregarInsumoCarrito para capturar datos)
            using (var form = new FormAgregarInsumoCarrito())
            {
                // Inicializar valores actuales
                try
                {
                    form.Clave = insumo.Clave;
                    form.Descripcion = insumo.Descripcion;
                    form.Unidad = insumo.Unidad;
                    form.Cantidad = insumo.Cantidad;
                    form.Precio = insumo.Precio;
                }
                catch { }

                if (form.ShowDialog() == DialogResult.OK)
                {
                    // Actualizar objeto en memoria
                    insumo.Descripcion = form.Descripcion;
                    insumo.Unidad = form.Unidad;
                    insumo.Cantidad = form.Cantidad;
                    insumo.Precio = form.Precio;

                    // Preguntar si aplicar cambios a la explosión
                    var aplic = MessageBox.Show("¿Deseas aplicar los cambios a la explosión de insumos (tablas de prototipo)?\n\n" +
                        "(Se aplicará a los prototipos de las casas seleccionadas, si son un único prototipo.)",
                        "Aplicar a explosión?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (aplic == DialogResult.Yes)
                    {
                        var prototipos = lstCasas.Items.Cast<CasaSeleccionada>().Select(c => c.Prototipo).Distinct().ToList();
                        if (prototipos.Count != 1)
                        {
                            MessageBox.Show("Selecciona casas de un solo prototipo para aplicar cambios a la explosión.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            string prototipo = prototipos[0];
                            string tabla = GetExplosionTableForPrototipo(prototipo);
                            try
                            {
                                using (SqlConnection conn = new SqlConnection(connectionString))
                                {
                                    conn.Open();
                                    using (SqlCommand cmdCheck = new SqlCommand($"SELECT COUNT(*) FROM {tabla} WHERE Clave = @clave", conn))
                                    {
                                        cmdCheck.Parameters.AddWithValue("@clave", insumo.Clave);
                                        int existe = (int)cmdCheck.ExecuteScalar();
                                        if (existe > 0)
                                        {
                                            using (SqlCommand cmdUpdate = new SqlCommand($"UPDATE {tabla} SET Descripcion = @desc, Unidad = @unidad, Cantidad = @cantidad, Familia = @familia WHERE Clave = @clave", conn))
                                            {
                                                cmdUpdate.Parameters.AddWithValue("@desc", insumo.Descripcion);
                                                cmdUpdate.Parameters.AddWithValue("@unidad", insumo.Unidad);
                                                cmdUpdate.Parameters.AddWithValue("@cantidad", insumo.Cantidad);
                                                cmdUpdate.Parameters.AddWithValue("@familia", insumo.Familia ?? "MANUAL");
                                                cmdUpdate.Parameters.AddWithValue("@clave", insumo.Clave);
                                                cmdUpdate.ExecuteNonQuery();
                                            }
                                            try
                                            {
                                                using (SqlCommand cmdPrecio = new SqlCommand($"UPDATE {tabla} SET Costo = @precio WHERE Clave = @clave", conn))
                                                {
                                                    cmdPrecio.Parameters.AddWithValue("@precio", insumo.Precio);
                                                    cmdPrecio.Parameters.AddWithValue("@clave", insumo.Clave);
                                                    cmdPrecio.ExecuteNonQuery();
                                                }
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            using (SqlCommand cmdInsert = new SqlCommand($"INSERT INTO {tabla} (Clave, Descripcion, Unidad, Cantidad, Familia) VALUES (@clave, @desc, @unidad, @cantidad, @familia)", conn))
                                            {
                                                cmdInsert.Parameters.AddWithValue("@clave", insumo.Clave);
                                                cmdInsert.Parameters.AddWithValue("@desc", insumo.Descripcion);
                                                cmdInsert.Parameters.AddWithValue("@unidad", insumo.Unidad);
                                                cmdInsert.Parameters.AddWithValue("@cantidad", insumo.Cantidad);
                                                cmdInsert.Parameters.AddWithValue("@familia", insumo.Familia ?? "MANUAL");
                                                cmdInsert.ExecuteNonQuery();
                                            }
                                            try
                                            {
                                                using (SqlCommand cmdPrecio = new SqlCommand($"UPDATE {tabla} SET Costo = @precio WHERE Clave = @clave", conn))
                                                {
                                                    cmdPrecio.Parameters.AddWithValue("@precio", insumo.Precio);
                                                    cmdPrecio.Parameters.AddWithValue("@clave", insumo.Clave);
                                                    cmdPrecio.ExecuteNonQuery();
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                }

                                MessageBox.Show("Explosión actualizada.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error aplicando en BD: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    
                    // Refrescar UI
                    ActualizarCatalogoInsumos();
                    olvCatalogo.RefreshObjects(new[] { insumo });
                }
            }
        }
    }
}
