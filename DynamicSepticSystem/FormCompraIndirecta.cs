using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using BrightIdeasSoftware;
using PdfSharp.Drawing;
using System.Configuration;
using PdfSharp.Pdf;

namespace DynamicSepticSystem
{
    public partial class FormCompraIndirecta : Form
    {
        public FormCompraIndirecta()
        {
            InitializeComponent();
            
            // Aplicar tema corporativo
            AplicarTema();
            
            CargarProveedores();
            ConfigurarListas();
            // La tabla COMPRASINDIRECTAS la asegura el API (ComprasController).
            CargarInsumosDesdeSQL();
        }

        private void AplicarTema()
        {
            // Fondo del formulario
            this.BackColor = ThemeManager.ColorFondo;
            
            // Aplicar tema general
            ThemeManager.AplicarTema(this);
            
            // Estilizar controles espec�ficos
            ThemeManager.EstilizarBotonExito(btnAgregarInsumo);
            ThemeManager.EstilizarBotonExito(btnGenerarOrden);
            
            // Estilizar DataGridViews (ObjectListViews)
            EstilizarObjectListView(olvCatalogo);
            EstilizarObjectListView(olvCarrito);
            
            // GroupBox con colores corporativos
            groupBox1.ForeColor = ThemeManager.ColorPrincipalMenuBar;
            groupBox1.Font = new Font(groupBox1.Font.FontFamily, 10, FontStyle.Bold);
        }
        
        private void EstilizarObjectListView(ObjectListView olv)
        {
            olv.BackColor = ThemeManager.ColorFondo;
            olv.ForeColor = ThemeManager.ColorTextoOscuro;
            olv.BorderStyle = BorderStyle.FixedSingle;
            olv.FullRowSelect = true;
            olv.GridLines = true;
            olv.Font = new Font("Segoe UI", 9F);
            
            // Aplicar estilo de encabezados despu�s de que se configuren las columnas
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
        private List<InsumoIndirecto> listaCatalogoOriginal = new List<InsumoIndirecto>();

        // Catálogo de proveedores cacheado desde el API (PROVEEDORESCALANDRIA).
        private List<ProveedorApi> _proveedores = new List<ProveedorApi>();

        /// <summary>
        /// Carga los insumos del catálogo COMPRASINDIRECTAS vía API.
        /// </summary>
        private void CargarInsumosDesdeSQL()
        {
            listaCatalogoOriginal.Clear();

            try
            {
                var insumos = ApiClient.Get<List<InsumoIndirectoApi>>("/api/compras/indirectas")
                              ?? new List<InsumoIndirectoApi>();
                foreach (var i in insumos)
                {
                    listaCatalogoOriginal.Add(new InsumoIndirecto
                    {
                        Clave = i.Clave ?? "",
                        Descripcion = i.Descripcion ?? "",
                        Unidad = i.Unidad ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo cargar el catálogo de compras indirectas: " + ex.Message,
                    "Compras indirectas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            olvCatalogo.SetObjects(listaCatalogoOriginal);
        }

        private void ConfigurarListas()
        {
            // ?? Cat�logo
            olvCatalogo.FullRowSelect = true;
            olvCatalogo.ShowGroups = false;
            olvCatalogo.Columns.Clear();

            olvCatalogo.Columns.Add(new OLVColumn("Clave", "Clave") { Width = 120 });
            olvCatalogo.Columns.Add(new OLVColumn("Descripci?n", "Descripcion") { Width = 300 });
            olvCatalogo.Columns.Add(new OLVColumn("Unidad", "Unidad") { Width = 80 });

            // ?? Carrito
            olvCarrito.FullRowSelect = true;
            olvCarrito.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;
            olvCarrito.ShowGroups = false;
            olvCarrito.Columns.Clear();

            var colCantidad = new OLVColumn("Cantidad", "Cantidad")
            {
                Width = 80,
                IsEditable = true,
                AspectPutter = (row, value) =>
                {
                    if (decimal.TryParse(value?.ToString(), out decimal nuevaCantidad) && nuevaCantidad >= 0)
                    {
                        ((InsumoIndirecto)row).Cantidad = nuevaCantidad;
                        olvCarrito.RefreshObject(row);
                    }
                }
            };

            var colCosto = new OLVColumn("Costo", "Costo")
            {
                Width = 100,
                IsEditable = true,
                AspectToStringFormat = "{0:C}",
                AspectGetter = row => ((InsumoIndirecto)row).Costo,
                AspectPutter = (row, value) =>
                {
                    if (decimal.TryParse(value?.ToString().Replace("$", "").Replace(",", ""), out decimal nuevoCosto) && nuevoCosto >= 0)
                    {
                        ((InsumoIndirecto)row).Costo = nuevoCosto;
                        olvCarrito.RefreshObject(row);
                    }
                }
            };

            var colImporte = new OLVColumn("Importe", "Importe")
            {
                Width = 120,
                AspectToStringFormat = "{0:C}",
                AspectGetter = row => ((InsumoIndirecto)row).Importe
            };

            olvCarrito.Columns.AddRange(new[] {
                new OLVColumn("Clave", "Clave") { Width = 120 },
                new OLVColumn("Descripci?n", "Descripcion") { Width = 300 },
                new OLVColumn("Unidad", "Unidad") { Width = 80 },
                colCantidad,
                colCosto,
                colImporte
            });
        }

        private void CargarProveedores()
        {
            txtClaveProveedor.Items.Clear();

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

            foreach (var clave in _proveedores
                         .Select(p => p.ClaveUnica ?? "")
                         .OrderBy(c => c, StringComparer.OrdinalIgnoreCase))
            {
                txtClaveProveedor.Items.Add(clave);
            }

            txtClaveProveedor.SelectedIndex = -1;
        }

        private void txtClaveProveedor_SelectedIndexChanged(object sender, EventArgs e)
        {
            string claveSeleccionada = txtClaveProveedor.Text.Trim();

            if (string.IsNullOrWhiteSpace(claveSeleccionada))
            {
                LimpiarInfoProveedor();
                return;
            }

            var prov = _proveedores.FirstOrDefault(p =>
                string.Equals(p.ClaveUnica, claveSeleccionada, StringComparison.OrdinalIgnoreCase));
            if (prov != null)
            {
                lblNombreProveedor.Text = "Nombre: " + (prov.Nombre ?? "");
                lblRFCProveedor.Text = "RFC: " + (prov.Rfc ?? "");
                lblDireccionProveedor.Text = "Direcci?n: " + (string.IsNullOrEmpty(prov.Direccion) ? "N/A" : prov.Direccion);
                lblTelefonoProveedor.Text = "Tel?fono: " + (string.IsNullOrEmpty(prov.Telefono) ? "N/A" : prov.Telefono);
            }
            else
            {
                LimpiarInfoProveedor();
            }
        }

        private void LimpiarInfoProveedor()
        {
            lblNombreProveedor.Text = "Nombre: ";
            lblRFCProveedor.Text = "RFC: ";
            lblDireccionProveedor.Text = "Direcci?n: ";
            lblTelefonoProveedor.Text = "Tel?fono: ";
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(filtro))
            {
                olvCatalogo.SetObjects(listaCatalogoOriginal);
            }
            else
            {
                var filtrados = listaCatalogoOriginal.Where(insumo =>
                    insumo.Clave.ToLower().Contains(filtro) ||
                    insumo.Descripcion.ToLower().Contains(filtro)).ToList();

                olvCatalogo.SetObjects(filtrados);
            }

            olvCatalogo.BuildList();
        }

        private void olvCatalogo_DoubleClick(object sender, EventArgs e)
        {
            var insumo = olvCatalogo.SelectedObject as InsumoIndirecto;
            if (insumo == null) return;

            // Evitar duplicados en el carrito
            if (olvCarrito.Objects.Cast<InsumoIndirecto>().Any(i => i.Clave == insumo.Clave))
            {
                MessageBox.Show("Este insumo ya est? en el carrito.", "Atenci?n", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Clonar el insumo seleccionado
            var copia = new InsumoIndirecto
            {
                Clave = insumo.Clave,
                Descripcion = insumo.Descripcion,
                Unidad = insumo.Unidad,
                Cantidad = 1,
                Costo = 0
            };

            var lista = olvCarrito.Objects.Cast<InsumoIndirecto>().ToList();
            lista.Add(copia);
            olvCarrito.SetObjects(lista);
        }

        private void olvCarrito_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hit = olvCarrito.OlvHitTest(e.X, e.Y);
            if (hit?.RowObject != null)
            {
                var result = MessageBox.Show("?Eliminar este insumo del carrito?", 
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    olvCarrito.RemoveObject(hit.RowObject);
                }
            }
        }

        private void btnAgregarInsumo_Click(object sender, EventArgs e)
        {
            using (var form = new FormAgregarInsumoIndirecto())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // Insertar en la base de datos
                    string clave = form.Clave;
                    string descripcion = form.Descripcion;
                    string unidad = form.Unidad;

                    // Verificar que la clave termine con "-A"
                    if (!clave.EndsWith("-A"))
                    {
                        clave += "-A";
                    }

                    try
                    {
                        ApiClient.Post("/api/compras/indirectas", new
                        {
                            Clave = clave,
                            Descripcion = descripcion,
                            Unidad = unidad
                        });

                        MessageBox.Show("Insumo agregado exitosamente.", "?xito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Recargar cat?logo
                        CargarInsumosDesdeSQL();
                    }
                    catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        MessageBox.Show("Ya existe un insumo con esa clave.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al guardar: " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnGenerarOrden_Click(object sender, EventArgs e)
        {
            var insumosCarrito = olvCarrito.Objects.Cast<InsumoIndirecto>().ToList();

            if (insumosCarrito.Count == 0)
            {
                MessageBox.Show("Agrega al menos un insumo al carrito.", "Atenci�n", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtClaveProveedor.Text))
            {
                MessageBox.Show("Selecciona un proveedor.", "Atenci�n", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ?? SOLICITAR NOMBRE PERSONALIZADO PARA LA ORDEN
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
                    Text = "Ingresa un nombre descriptivo para esta orden de compra:\n(Ejemplo: Compra Administrativa, Material de Oficina, etc.)",
                    Location = new Point(15, 15),
                    Size = new Size(410, 40),
                    Font = new Font("Segoe UI", 9F)
                };
                
                TextBox txtNombre = new TextBox
                {
                    Location = new Point(15, 65),
                    Size = new Size(410, 25),
                    Font = new Font("Segoe UI", 10F),
                    Text = $"Compra Indirecta - {DateTime.Now:dd/MM/yyyy}" // Sugerencia por defecto
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
                        nombreOrden = $"Compra Indirecta - {DateTime.Now:dd/MM/yyyy}"; // Nombre por defecto
                }
                else
                {
                    return; // Usuario cancel�
                }
            }

            // Preguntar el tipo de t�tulo
            string tipoTitulo = "";
            using (var formTitulo = new FormSeleccionarTitulo())
            {
                if (formTitulo.ShowDialog() == DialogResult.OK)
                {
                    tipoTitulo = formTitulo.TituloSeleccionado;
                }
                else
                {
                    return; // Usuario cancel�
                }
            }

            // Guardar la orden vía API (folio + cabecera + detalle, en una transacción)
            string folioOC;
            string claveProveedor = txtClaveProveedor.Text;
            string rutaPdf = string.Empty;

            try
            {
                var resp = ApiClient.Post<FolioOrdenApi>("/api/ordenescompra/indirecta", new
                {
                    Usuario = Environment.UserName,
                    NombreOrden = nombreOrden,
                    ProveedorClave = claveProveedor,
                    Detalles = insumosCarrito.Select(i => new DetalleOrdenApi
                    {
                        Clave = i.Clave,
                        Descripcion = i.Descripcion,
                        Unidad = i.Unidad,
                        Cantidad = i.Cantidad,
                        PrecioUnitario = i.Costo,
                        ImporteTotal = i.Importe
                    }).ToList()
                });

                folioOC = resp?.FolioOC;
                if (string.IsNullOrEmpty(folioOC))
                    throw new Exception("El servidor no devolvió un folio.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar orden: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Generar PDF
            try
            {
                rutaPdf = GenerarPDF(folioOC, tipoTitulo, insumosCarrito, claveProveedor);
                
                // ?? GUARDAR EN REPOSITORIO DE PDFs
                try
                {
                    GuardarOrdenIndirectaEnRepositorio(folioOC, rutaPdf, insumosCarrito, tipoTitulo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"? Advertencia: La orden se gener� correctamente pero no se pudo guardar en el repositorio de PDFs:\n\n{ex.Message}", 
                        "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                
                MessageBox.Show($"Orden de compra generada exitosamente.\nFolio: {folioOC}", 
                    "�xito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // El folio ahora lo genera el servidor al guardar la orden (OrdenesCompraController).

        private string GenerarPDF(string folioOC, string tipoTitulo, List<InsumoIndirecto> insumos, string claveProveedor)
        {
            string carpetaPdf = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                "CALANDRIA RESIDENCIAL", "PDFOrdenesCompra");
            Directory.CreateDirectory(carpetaPdf);
            string rutaPdf = Path.Combine(carpetaPdf, $"OrdenCompraIndirecta_{folioOC}.pdf");

            PdfDocument pdf = new PdfDocument();
            pdf.Info.Title = "Orden de Compra - " + folioOC;
            pdf.Info.Author = "Desarrolladora de Casas Camaney";
            pdf.Info.Subject = "Orden de Compra";
            pdf.Info.Creator = "Sistema Calandria";

            PdfPage page = pdf.AddPage();
            page.Size = PdfSharp.PageSize.Letter;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Definir fuentes
            XFont fontTitle = new XFont("Arial", 14, XFontStyle.Bold);
            XFont fontBold = new XFont("Arial", 9, XFontStyle.Bold);
            XFont fontNormal = new XFont("Arial", 8, XFontStyle.Regular);
            XFont fontSmall = new XFont("Arial", 7, XFontStyle.Regular);
            XFont fontTableHeader = new XFont("Arial", 8, XFontStyle.Bold);

            int margin = 30;
            int x = margin;
            int y = margin;
            int pageWidth = (int)page.Width - (margin * 2);

            // ==================== ENCABEZADO ====================
            // INFORMACI�N DE LA EMPRESA (IZQUIERDA)
            int leftColumnWidth = 380;
            
            gfx.DrawString("Desarrolladora de Casas Camaney", 
                new XFont("Arial", 9, XFontStyle.Bold), XBrushes.Black, x, y);
            y += 12;
            
            gfx.DrawString("Blvd. Perif�rico sur. Y Carretera a la Colorada", 
                fontSmall, XBrushes.Black, x, y);
            y += 10;
            
            gfx.DrawString("Tel. 662xxxxxxxxxx", 
                fontSmall, XBrushes.Black, x, y);
            y += 10;
            
            gfx.DrawString("Email constcas@h.....com", 
                fontSmall, XBrushes.Black, x, y);

            // LOGO CALANDRIA (DERECHA) - Alineado con la info de empresa
            try
            {
                using (var ms = new MemoryStream())
                {
                    DynamicSepticSystem.Properties.Resources.Logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    using (var logo = XImage.FromStream(ms))
                    {
                        int logoWidth = 120;
                        int logoHeight = 80;
                        int logoX = (int)page.Width - margin - logoWidth;
                        gfx.DrawImage(logo, logoX, margin - 5, logoWidth, logoHeight);
                    }
                }
            }
            catch { }

            y = margin + 85;

            // ==================== T�TULO ====================
            gfx.DrawString("ORDEN DE COMPRA", fontTitle, XBrushes.Black, 
                new XRect(margin, y, pageWidth, 20), XStringFormats.Center);
            y += 30;

            // ==================== FOLIO Y FECHA (DERECHA) ====================
            int folioBoxWidth = 180;
            int folioBoxX = (int)page.Width - margin - folioBoxWidth;
            
            // L�nea para Folio
            int folioY = y;
            gfx.DrawString("Folio_________", fontNormal, XBrushes.Black, folioBoxX, folioY);
            gfx.DrawString(folioOC, fontNormal, XBrushes.Black, folioBoxX + 60, folioY);
            
            // L�nea para Fecha
            folioY += 15;
            gfx.DrawString("Fecha_________", fontNormal, XBrushes.Black, folioBoxX, folioY);
            gfx.DrawString(DateTime.Now.ToString("dd/MM/yyyy"), fontNormal, XBrushes.Black, folioBoxX + 60, folioY);

            y += 45;

            // ==================== INFORMACI�N DEL PROVEEDOR ====================
            // Obtener datos del proveedor
            string nombreProv = "";
            string rfcProv = "";
            string direccionProv = "";
            string telefonoProv = "";

            var provPdf = _proveedores.FirstOrDefault(p =>
                string.Equals(p.ClaveUnica, claveProveedor, StringComparison.OrdinalIgnoreCase));
            if (provPdf != null)
            {
                nombreProv = provPdf.Nombre ?? "";
                rfcProv = provPdf.Rfc ?? "";
                direccionProv = provPdf.Direccion ?? "";
                telefonoProv = provPdf.Telefono ?? "";
            }

            int provY = y;
            gfx.DrawString("Proveedor_____________________________", fontNormal, XBrushes.Black, x, provY);
            gfx.DrawString(nombreProv, fontNormal, XBrushes.Black, x + 100, provY);
            
            provY += 15;
            gfx.DrawString("Codigo Prov___________________________", fontNormal, XBrushes.Black, x, provY);
            gfx.DrawString(claveProveedor, fontNormal, XBrushes.Black, x + 100, provY);

            y = provY + 30;

            // ==================== TABLA DE INSUMOS ====================
            // Definir columnas: CODIGO | INSUMO | UNIDAD | CANTIDAD | PRECIO | IMPORTE
            string[] headers = { "CODIGO", "INSUMO", "UNIDAD", "CANTIDAD", "PRECIO", "IMPORTE" };
            int[] widths = { 70, 200, 55, 70, 75, 80 }; // Total = 550
            int totalTableWidth = 550; // Suma manual de widths
            int headerRowHeight = 20;
            int dataRowHeight = 18;

            // Dibujar encabezado de tabla con bordes
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
                // Verificar si necesitamos nueva p�gina
                if (y + dataRowHeight > page.Height - 120)
                {
                    page = pdf.AddPage();
                    page.Size = PdfSharp.PageSize.Letter;
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                    
                    // Redibujar encabezado de tabla en nueva p�gina
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

                // INSUMO (DESCRIPCI�N)
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
                gfx.DrawString(insumo.Costo.ToString("C2"), fontSmall, XBrushes.Black, 
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

            // L�nea final de la tabla
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

            // L�nea separadora antes del total
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
            y = (int)page.Height - margin - 80;
            
            int firmaWidth = 200;
            int firma1X = margin + 50;

            // Nombre y Firma - Encargado de Compras
            gfx.DrawLine(XPens.Black, firma1X, y, firma1X + firmaWidth, y);
            y += 5;
            gfx.DrawString("Nombre y Firma", fontSmall, XBrushes.Black, 
                new XRect(firma1X, y, firmaWidth, 15), XStringFormats.TopCenter);
            y += 12;
            gfx.DrawString("Encargado de Compras", fontSmall, XBrushes.Black, 
                new XRect(firma1X, y, firmaWidth, 15), XStringFormats.TopCenter);

            // Guardar y abrir PDF
            try
            {
                pdf.Save(rutaPdf);
                pdf.Close();
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
                // Si no se puede abrir el archivo directamente, abrir la carpeta contenedora
                try { Process.Start("explorer.exe", carpetaPdf); } catch { }
            }
            
            return rutaPdf;
        }

        private void btnAgregarProveedor_Click(object sender, EventArgs e)
        {
            FormAgregarProveedor f = new FormAgregarProveedor();
            f.ShowDialog();
            CargarProveedores();
        }

        private void btnVerRepositorio_Click(object sender, EventArgs e)
        {
            AbrirRepositorioPDFsOrdenesIndirectas();
        }
    }

    public class InsumoIndirecto
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Costo { get; set; }
        public decimal Importe => Math.Round(Cantidad * Costo, 2);
    }
}
