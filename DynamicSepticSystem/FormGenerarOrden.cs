using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ExcelDataReader;
using PdfSharp.Drawing;
using ClosedXML.Excel;
using System.Drawing; // para usar imágenes
using PdfSharp.Pdf;
using DynamicSepticSystem;
using System.ComponentModel;
using System.Windows.Media.Media3D;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Configuration;

public partial class FormGenerarOrden : Form
{
    private string rutaExcel;
    private string manzana;
    private string lote;

    public FormGenerarOrden(string rutaExcel, string manzana, string lote)
    {
        InitializeComponent();
        ThemeManager.AplicarTema(this);
        this.rutaExcel = rutaExcel;
        this.manzana = manzana;
        this.lote = lote;
        dgvMateriales.CurrentCellDirtyStateChanged += dgvMateriales_CurrentCellDirtyStateChanged;

        CargarExplosionCompletaDesdeSQL(); // <-- Cambiado aquí
        txtBuscarExplosion.TextChanged += (s, e) =>
        {
            if (dgvMateriales.DataSource is DataTable dt)
            {
                string filtro = txtBuscarExplosion.Text.Trim().Replace("'", "''");
                dt.DefaultView.RowFilter = $"Clave LIKE '%{filtro}%' OR [Descripción] LIKE '%{filtro}%'";
            }
        };
    }


    private void CargarExplosionCompletaDesdeSQL()
    {
        string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString; var dt = new DataTable();

        dt.Columns.Add("Seleccionar", typeof(bool));
        dt.Columns.Add("Clave");
        dt.Columns.Add("Descripción");
        dt.Columns.Add("Unidad");
        dt.Columns.Add("Cantidad");
        dt.Columns.Add("Costo", typeof(double));
        dt.Columns.Add("Importe", typeof(double));
        dt.Columns.Add("Porcentaje");
        dt.Columns.Add("Familia");

        using (var conn = new SqlConnection(connectionString))
        using (var cmd = new SqlCommand("SELECT Clave, Descripcion, Unidad, Cantidad, 0 AS Costo, 0 AS Importe, '' AS Porcentaje, '' AS Familia FROM ExplosionInsumos", conn))
        {
            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    dt.Rows.Add(false,
                                reader["Clave"],
                                reader["Descripcion"],
                                reader["Unidad"],
                                reader["Cantidad"],
                                0, // Costo
                                0, // Importe
                                "", // Porcentaje
                                ""); // Familia
                }
            }
        }

        dgvMateriales.DataSource = dt;
        ActualizarResumenOrden();
        
    }


    private void ActualizarResumenOrden()
    {
        var dt = dgvMateriales.DataSource as DataTable;
        if (dt == null)
        {
            lblResumenOrden.Text = "";
            return;
        }

        var filasSeleccionadas = dt.AsEnumerable().Where(r => r.Field<bool>("Seleccionar")).ToList();

        if (!filasSeleccionadas.Any())
        {
            lblResumenOrden.Text = "No hay materiales seleccionados.";
            return;
        }

        double totalImporte = 0;
        List<string> lineas = new List<string>();

        foreach (var row in filasSeleccionadas)
        {
            string desc = row["Descripción"].ToString();
            double importe = 0;
            double.TryParse(row["Importe"].ToString(), out importe);
            totalImporte += importe;

            lineas.Add($"• {desc}: ${importe:N2}");
        }

        lineas.Add("-----------------------------");
        lineas.Add($"Total estimado: ${totalImporte:N2}");

        lblResumenOrden.Text = string.Join(Environment.NewLine, lineas);
    }
    string fechaHora = DateTime.Now.ToString("yyyyMMdd_HHmmss");

    private void RegistrarMaterialesComprados(
    string manzana, string lote, string folioOC, List<Material> materiales,
    string proveedor, string usuario, string detalles, string rutaExcel)
    {
        using (var package = new XLWorkbook())
        {
            var ws = package.Worksheets.Add("Compras");
            string[] headers = { "Folio OC", "Fecha", "Clave", "Descripción", "Unidad", "Cantidad", "Proveedor", "Usuario", "Detalles" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];

            int fila = 2;
            foreach (var mat in materiales)
            {
                ws.Cell(fila, 1).Value = folioOC;
                ws.Cell(fila, 2).Value = DateTime.Now;
                ws.Cell(fila, 3).Value = mat.Clave;
                ws.Cell(fila, 4).Value = mat.Descripcion;
                ws.Cell(fila, 5).Value = mat.Unidad;
                ws.Cell(fila, 6).Value = mat.Cantidad;
                ws.Cell(fila, 7).Value = proveedor;
                ws.Cell(fila, 8).Value = usuario;
                ws.Cell(fila, 9).Value = detalles;
                fila++;
            }

            package.SaveAs(rutaExcel);
        }
    }

    


    

    private void btnGenerarPDF_Click_1(object sender, EventArgs e)
    {
        string proveedorCodigo = txtCodigoProveedor.Text.Trim();
        string proveedorNombre = txtNombreProveedor.Text.Trim();
        string detalles = txtDetallesBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(proveedorCodigo) || string.IsNullOrWhiteSpace(proveedorNombre))
        {
            MessageBox.Show("Debes ingresar código y nombre del proveedor.");
            return;
        }

        if (!DateTime.TryParse(dtpFechaOrden.Text, out DateTime fechaOrden))
        {
            MessageBox.Show("Fecha inválida.");
            return;
        }

        var filasSeleccionadas = ((DataTable)dgvMateriales.DataSource).AsEnumerable()
            .Where(r => r.Field<bool>("Seleccionar")).ToList();

        if (!filasSeleccionadas.Any())
        {
            MessageBox.Show("Selecciona al menos un material.");
            return;
        }

        // 1. Generar folio y timestamp
        string folio = FolioManager.GenerarFolioOrdenCompra(manzana, lote);
        string fechaHora = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 2. Guardar los detalles en JSON maestro
        DetalleOrdenHelper.GuardarDetalleOrden(manzana, lote, folio, detalles);

        // 3. Rutas únicas para Excel y PDF
        string carpetaExcel = @"C:\Users\Luxu\Documents\CALANDRIA RESIDENCIAL\ExcelOrdenesCompra";
        Directory.CreateDirectory(carpetaExcel);
        string rutaExcel = Path.Combine(carpetaExcel, $"OrdenCompra_{folio}_{fechaHora}.xlsx");

        string carpetaPdf = @"C:\Users\Luxu\Documents\CALANDRIA RESIDENCIAL\PDFOrdenesCompra";
        Directory.CreateDirectory(carpetaPdf);
        string rutaPdf = Path.Combine(carpetaPdf, $"OrdenCompra_{folio}_{fechaHora}.pdf");

        // 4. Mostrar SaveFileDialog para PDF (opcional, puedes comentar si siempre guardas en rutaPdf)
        SaveFileDialog sfd = new SaveFileDialog
        {
            Filter = "PDF Files|*.pdf",
            FileName = Path.GetFileName(rutaPdf)
        };

        if (sfd.ShowDialog() != DialogResult.OK)
            return;


        List<Material> materialesRegistrados = filasSeleccionadas.Select(row => new Material
        {
            Clave = row["Clave"].ToString(),
            Descripcion = row["Descripción"].ToString(),
            Unidad = row["Unidad"].ToString(),
            Cantidad = double.TryParse(row["Cantidad"].ToString(), out double c) ? c : 0
        }).ToList();

        string usuario = Global.UsuarioActual?.Nombre ?? Environment.UserName;


        // 5. Generar PDF
        PdfDocument pdf = new PdfDocument();
        PdfPage page = pdf.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);

        using (var ms = new MemoryStream())
        {
            DynamicSepticSystem.Properties.Resources.Logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            using (var logo = XImage.FromStream(ms))
            {
                gfx.DrawImage(logo, page.Width - 150, 20, 100, 89); // alto ajustado a la proporción del logo Pilaris
            }
        }

        XFont fontTitulo = new XFont("Arial", 14, XFontStyle.Bold);
        XFont font = new XFont("Arial", 10, XFontStyle.Regular);

        gfx.DrawString("Desarrolladora de Casas Camaney", font, XBrushes.Black, new XPoint(40, 30));
        gfx.DrawString("Blvd. Periférico sur. Y Carretera a la Colorada", font, XBrushes.Black, new XPoint(40, 45));
        gfx.DrawString("Tel. 662xxxxxxxx", font, XBrushes.Black, new XPoint(40, 60));
        gfx.DrawString("Email constcas@h......com", font, XBrushes.Black, new XPoint(40, 75));

        gfx.DrawString("ORDEN DE COMPRA", fontTitulo, XBrushes.Black, new XPoint(200, 100));

        gfx.DrawString("Proveedor:", font, XBrushes.Black, new XPoint(40, 130));
        gfx.DrawString(proveedorNombre, font, XBrushes.Black, new XPoint(110, 130));
        gfx.DrawString("Código Prov:", font, XBrushes.Black, new XPoint(40, 145));
        gfx.DrawString(proveedorCodigo, font, XBrushes.Black, new XPoint(110, 145));
        gfx.DrawString("Folio:", font, XBrushes.Black, new XPoint(350, 130));
        gfx.DrawString(folio, font, XBrushes.Black, new XPoint(400, 130));
        gfx.DrawString("Fecha:", font, XBrushes.Black, new XPoint(350, 145));
        gfx.DrawString(fechaOrden.ToShortDateString(), font, XBrushes.Black, new XPoint(400, 145));



        // ---- 1. Prepara datos y fuentes ----
        string[] headers = { "CODIGO", "INSUMO", "UNIDAD", "CANTIDAD", "PRECIO", "IMPORTE" };
        int columnas = headers.Length;
        int lineHeight = 20;
        int yStart = 180;

        // ---- 2. Calcula anchos de columna ----
        int[] anchos = new int[columnas];

        // Inicializa con el ancho de los encabezados
        for (int i = 0; i < columnas; i++)
        {
            anchos[i] = (int)gfx.MeasureString(headers[i], font).Width + 10;
        }

        // Mide el ancho de cada dato
        foreach (var row in filasSeleccionadas)
        {
            string[] valores = {
        row["Clave"].ToString(),
        row["Descripción"].ToString(),
        row["Unidad"].ToString(),
        row["Cantidad"].ToString(),
        row["Costo"].ToString(),
        row["Importe"].ToString()
    };
            for (int i = 0; i < columnas; i++)
            {
                int anchoDato = (int)gfx.MeasureString(valores[i], font).Width + 10;
                if (anchoDato > anchos[i])
                    anchos[i] = anchoDato;
            }
        }

        // ---- 3. Calcula posición X de cada columna ----
        int[] xPoints = new int[columnas + 1];
        xPoints[0] = 40; // Margen izquierdo
        for (int i = 1; i <= columnas; i++)
        {
            xPoints[i] = xPoints[i - 1] + anchos[i - 1];
        }

        // ---- 4. Dibuja encabezados ----
        for (int i = 0; i < columnas; i++)
        {
            gfx.DrawString(headers[i], font, XBrushes.Black, new XPoint(xPoints[i] + 2, yStart));
        }

        // ---- 5. Dibuja filas de datos ----
        int yLine = yStart + lineHeight;
        foreach (var row in filasSeleccionadas)
        {
            string[] valores = {
        row["Clave"].ToString(),
        row["Descripción"].ToString(),
        row["Unidad"].ToString(),
        row["Cantidad"].ToString(),
        row["Costo"].ToString(),
        row["Importe"].ToString()
    };
            for (int i = 0; i < columnas; i++)
            {
                gfx.DrawString(valores[i], font, XBrushes.Black, new XPoint(xPoints[i] + 2, yLine));
            }
            yLine += lineHeight;
        }

        // ---- 6. Dibuja líneas de tabla ----
        // Líneas verticales
        for (int i = 0; i < xPoints.Length; i++)
        {
            gfx.DrawLine(XPens.Black, xPoints[i], yStart, xPoints[i], yLine);
        }
        // Líneas horizontales
        int filasTotales = filasSeleccionadas.Count + 1;
        for (int i = 0; i <= filasTotales; i++)
        {
            gfx.DrawLine(XPens.Black, xPoints[0], yStart + lineHeight * i, xPoints[columnas], yStart + lineHeight * i);
        }

        // ---- 7. Dibuja totales y pie ----
        gfx.DrawString("TOTAL", font, XBrushes.Black, new XPoint(xPoints[4], yLine + 10));
        gfx.DrawString("Nombre y Firma", font, XBrushes.Black, new XPoint(xPoints[0], yLine + 50));
        gfx.DrawString("Encargado de Compras", font, XBrushes.Black, new XPoint(xPoints[0], yLine + 65));


        // Guarda el PDF con el nombre único
        pdf.Save(sfd.FileName);

        // 6. Lista de materiales para registrar en Excel
       

        string nombre = Global.UsuarioActual?.Nombre ?? Environment.UserName;

        // 7. Guarda los materiales en Excel, archivo único por orden
        RegistrarMaterialesComprados(manzana, lote, folio, materialesRegistrados, proveedorNombre, usuario, detalles, rutaExcel);
        materialesRegistrados = filasSeleccionadas.Select(row => new Material
        {
            Clave = row["Clave"].ToString(),
            Descripcion = row["Descripción"].ToString(),
            Unidad = row["Unidad"].ToString(),
            Cantidad = double.TryParse(row["Cantidad"].ToString(), out double c) ? c : 0
        }).ToList();
        using (var conn = new SqlConnection(@"Server=(localdb)\CALANDRIA;Database=CALANDRIA;Trusted_Connection=True;"))
        {
            conn.Open();
            foreach (var mat in materialesRegistrados)
            {
                using (var cmd = new SqlCommand(@"INSERT INTO OrdenesCompra 
        (FolioOC, Fecha, Manzana, Lote, Clave, Descripcion, Unidad, Cantidad, Proveedor, Usuario, Detalles) 
        VALUES (@Folio, @Fecha, @Manzana, @Lote, @Clave, @Descripcion, @Unidad, @Cantidad, @Proveedor, @Usuario, @Detalles)", conn))
                {
                    cmd.Parameters.AddWithValue("@Folio", folio);
                    cmd.Parameters.AddWithValue("@Fecha", fechaOrden);
                    cmd.Parameters.AddWithValue("@Manzana", manzana);
                    cmd.Parameters.AddWithValue("@Lote", lote);
                    cmd.Parameters.AddWithValue("@Clave", mat.Clave);
                    cmd.Parameters.AddWithValue("@Descripcion", mat.Descripcion);
                    cmd.Parameters.AddWithValue("@Unidad", mat.Unidad);
                    cmd.Parameters.AddWithValue("@Cantidad", mat.Cantidad);
                    cmd.Parameters.AddWithValue("@Proveedor", proveedorNombre);
                    cmd.Parameters.AddWithValue("@Usuario", usuario);
                    cmd.Parameters.AddWithValue("@Detalles", detalles);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        MessageBox.Show("Orden de compra generada correctamente.");
    }




    private void dgvMateriales_CurrentCellDirtyStateChanged(object sender, EventArgs e)
    {
        if (dgvMateriales.IsCurrentCellDirty)
        {
            dgvMateriales.CommitEdit(DataGridViewDataErrorContexts.Commit);
            ActualizarResumenOrden();
        }
    }

    
    private void btnCancelar_Click_1(object sender, EventArgs e)
    {
        this.Close();
    }

    private void FormGenerarOrden_Load(object sender, EventArgs e)
    {

    }

    private void button2_Click(object sender, EventArgs e)
    {

    }

    private DataGridView dgvMateriales;


    private void InitializeComponent()
    {
            this.dgvMateriales = new System.Windows.Forms.DataGridView();
            this.btnGenerarPDF = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.dtpFechaOrden = new System.Windows.Forms.DateTimePicker();
            this.txtCodigoProveedor = new System.Windows.Forms.TextBox();
            this.txtNombreProveedor = new System.Windows.Forms.TextBox();
            this.lblProveedor = new System.Windows.Forms.Label();
            this.lblCodigoProveedor = new System.Windows.Forms.Label();
            this.lblResumenOrden = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblDetallesGen = new System.Windows.Forms.Label();
            this.txtDetallesBox = new System.Windows.Forms.TextBox();
            this.txtBuscarExplosion = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMateriales)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvMateriales
            // 
            this.dgvMateriales.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMateriales.Location = new System.Drawing.Point(410, 12);
            this.dgvMateriales.Name = "dgvMateriales";
            this.dgvMateriales.Size = new System.Drawing.Size(495, 405);
            this.dgvMateriales.TabIndex = 0;
            this.dgvMateriales.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMateriales_CellContentClick);
            // 
            // btnGenerarPDF
            // 
            this.btnGenerarPDF.Location = new System.Drawing.Point(696, 530);
            this.btnGenerarPDF.Name = "btnGenerarPDF";
            this.btnGenerarPDF.Size = new System.Drawing.Size(75, 23);
            this.btnGenerarPDF.TabIndex = 1;
            this.btnGenerarPDF.Text = "GENERAR";
            this.btnGenerarPDF.UseVisualStyleBackColor = true;
            this.btnGenerarPDF.Click += new System.EventHandler(this.btnGenerarPDF_Click_1);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Location = new System.Drawing.Point(830, 530);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(75, 23);
            this.btnCancelar.TabIndex = 2;
            this.btnCancelar.Text = "CANCELAR";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click_1);
            // 
            // dtpFechaOrden
            // 
            this.dtpFechaOrden.Location = new System.Drawing.Point(696, 422);
            this.dtpFechaOrden.Name = "dtpFechaOrden";
            this.dtpFechaOrden.Size = new System.Drawing.Size(200, 20);
            this.dtpFechaOrden.TabIndex = 3;
            // 
            // txtCodigoProveedor
            // 
            this.txtCodigoProveedor.Location = new System.Drawing.Point(696, 492);
            this.txtCodigoProveedor.Name = "txtCodigoProveedor";
            this.txtCodigoProveedor.Size = new System.Drawing.Size(75, 20);
            this.txtCodigoProveedor.TabIndex = 4;
            // 
            // txtNombreProveedor
            // 
            this.txtNombreProveedor.Location = new System.Drawing.Point(830, 492);
            this.txtNombreProveedor.Name = "txtNombreProveedor";
            this.txtNombreProveedor.Size = new System.Drawing.Size(75, 20);
            this.txtNombreProveedor.TabIndex = 5;
            // 
            // lblProveedor
            // 
            this.lblProveedor.AutoSize = true;
            this.lblProveedor.Location = new System.Drawing.Point(830, 476);
            this.lblProveedor.Name = "lblProveedor";
            this.lblProveedor.Size = new System.Drawing.Size(75, 13);
            this.lblProveedor.TabIndex = 6;
            this.lblProveedor.Text = "PROVEEDOR";
            // 
            // lblCodigoProveedor
            // 
            this.lblCodigoProveedor.AutoSize = true;
            this.lblCodigoProveedor.Location = new System.Drawing.Point(693, 476);
            this.lblCodigoProveedor.Name = "lblCodigoProveedor";
            this.lblCodigoProveedor.Size = new System.Drawing.Size(82, 13);
            this.lblCodigoProveedor.TabIndex = 7;
            this.lblCodigoProveedor.Text = "CODIGO PROV";
            // 
            // lblResumenOrden
            // 
            this.lblResumenOrden.AutoSize = true;
            this.lblResumenOrden.Location = new System.Drawing.Point(3, 0);
            this.lblResumenOrden.Name = "lblResumenOrden";
            this.lblResumenOrden.Size = new System.Drawing.Size(35, 13);
            this.lblResumenOrden.TabIndex = 8;
            this.lblResumenOrden.Text = "label1";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblResumenOrden);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(392, 405);
            this.panel1.TabIndex = 9;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint_1);
            // 
            // lblDetallesGen
            // 
            this.lblDetallesGen.AutoSize = true;
            this.lblDetallesGen.Location = new System.Drawing.Point(15, 428);
            this.lblDetallesGen.Name = "lblDetallesGen";
            this.lblDetallesGen.Size = new System.Drawing.Size(65, 13);
            this.lblDetallesGen.TabIndex = 10;
            this.lblDetallesGen.Text = "DETALLES:";
            // 
            // txtDetallesBox
            // 
            this.txtDetallesBox.Location = new System.Drawing.Point(12, 444);
            this.txtDetallesBox.Multiline = true;
            this.txtDetallesBox.Name = "txtDetallesBox";
            this.txtDetallesBox.Size = new System.Drawing.Size(337, 109);
            this.txtDetallesBox.TabIndex = 11;
            this.txtDetallesBox.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // txtBuscarExplosion
            // 
            this.txtBuscarExplosion.Location = new System.Drawing.Point(379, 530);
            this.txtBuscarExplosion.Name = "txtBuscarExplosion";
            this.txtBuscarExplosion.Size = new System.Drawing.Size(156, 20);
            this.txtBuscarExplosion.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(376, 514);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(219, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "BUSCAR INSUMO (CLAVE, DESCRIPCION)";
            // 
            // FormGenerarOrden
            // 
            this.ClientSize = new System.Drawing.Size(917, 565);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtBuscarExplosion);
            this.Controls.Add(this.txtDetallesBox);
            this.Controls.Add(this.lblDetallesGen);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblCodigoProveedor);
            this.Controls.Add(this.lblProveedor);
            this.Controls.Add(this.txtNombreProveedor);
            this.Controls.Add(this.txtCodigoProveedor);
            this.Controls.Add(this.dtpFechaOrden);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnGenerarPDF);
            this.Controls.Add(this.dgvMateriales);
            this.Name = "FormGenerarOrden";
            ((System.ComponentModel.ISupportInitialize)(this.dgvMateriales)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }
    public class DetalleOrden
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string FolioOC { get; set; }
        public string Detalles { get; set; }
    }

    public class Material
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public double Cantidad { get; set; }
    }

    private Button btnGenerarPDF;
    private Button btnCancelar;
    private DateTimePicker dtpFechaOrden;
    private TextBox txtCodigoProveedor;
    private TextBox txtNombreProveedor;
    private Label lblProveedor;
    private Label lblCodigoProveedor;
    private Label lblResumenOrden;

    private void dgvMateriales_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {

    }

    private void panel1_Paint(object sender, PaintEventArgs e)
    {

    }

    private Panel panel1;

    private void panel1_Paint_1(object sender, PaintEventArgs e)
    {

    }

    private Label lblDetallesGen;
    private TextBox txtDetallesBox;

    private void textBox1_TextChanged(object sender, EventArgs e)
    {

    }

    private TextBox txtBuscarExplosion;
    private Label label1;
}
