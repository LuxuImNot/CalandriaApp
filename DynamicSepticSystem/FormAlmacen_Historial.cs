// History tracking functionality - FormAlmacen
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace DynamicSepticSystem
{
    public partial class FormAlmacen : Form
    {
        private void CargarHistorial()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
            var lista = new List<MovimientoHistorial>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM HistorialMovimientos ORDER BY Fecha DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new MovimientoHistorial
                        {
                            Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                            TipoMovimiento = reader["TipoMovimiento"].ToString(),
                            Clave = reader["Clave"].ToString(),
                            Descripcion = reader["Descripcion"].ToString(),
                            Unidad = reader["Unidad"].ToString(),
                            Cantidad = reader.IsDBNull(reader.GetOrdinal("Cantidad")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Cantidad")),
                            Precio = reader.IsDBNull(reader.GetOrdinal("PrecioUnitario")) ? 0 : reader.GetDecimal(reader.GetOrdinal("PrecioUnitario")),
                            Usuario = reader["Usuario"].ToString(),
                            Manzana = reader["Manzana"].ToString(),
                            Lote = reader["Lote"].ToString(),
                            Prototipo = reader["Prototipo"].ToString(),
                            Justificacion = reader["Justificacion"].ToString()
                        });
                    }
                }
            }

            objectListViewHistorial.SetObjects(lista);
        }

        private void ConfigurarColumnasHistorial()
        {
            objectListViewHistorial.Columns.Clear();
            objectListViewHistorial.ShowGroups = false;
            objectListViewHistorial.FullRowSelect = true;
            objectListViewHistorial.OwnerDraw = true;
            objectListViewHistorial.UseTranslucentSelection = true;
            ImageList lista = new ImageList();
            lista.ImageSize = new Size(32, 32);
            lista.Images.Add(MovimientoHistorial.iconEntrada);
            lista.Images.Add(MovimientoHistorial.iconSalida);
            objectListViewHistorial.SmallImageList = lista;

            objectListViewHistorial.Columns.Add(new OLVColumn()
            {
                Width = 36,
                IsEditable = false,
                Text = "", // opcional
                ImageGetter = row => ((MovimientoHistorial)row).TipoMovimiento == "Entrada" ? 0 : 1
            });

            objectListViewHistorial.Columns.Add(new OLVColumn("Fecha", "Fecha") { Width = 120 });
            objectListViewHistorial.Columns.Add(new OLVColumn("Tipo", "TipoMovimiento") { Width = 80 });
            objectListViewHistorial.Columns.Add(new OLVColumn("Clave", "Clave") { Width = 100 });
            objectListViewHistorial.Columns.Add(new OLVColumn("Descripción", "Descripcion") { Width = 250 });
            objectListViewHistorial.Columns.Add(new OLVColumn("Unidad", "Unidad") { Width = 60 });
            objectListViewHistorial.Columns.Add(new OLVColumn("Cantidad", "Cantidad") { Width = 80 });
            objectListViewHistorial.Columns.Add(new OLVColumn("P.U.", "Precio") 
            { 
                Width = 90,
                AspectToStringConverter = obj => ((decimal)obj).ToString("C2")
            });
            objectListViewHistorial.Columns.Add(new OLVColumn("Importe", "Importe") 
            { 
                Width = 100,
                AspectToStringConverter = obj => ((decimal)obj).ToString("C2")
            });
            objectListViewHistorial.Columns.Add(new OLVColumn("Usuario", "Usuario") { Width = 100 });
            objectListViewHistorial.Columns.Add(new OLVColumn("Casa", "Casa") { Width = 100 });
            objectListViewHistorial.Columns.Add(new OLVColumn("Modelo", "Prototipo") { Width = 100 });

            objectListViewHistorial.FormatRow += (sender, e) =>
            {
                var mov = e.Model as MovimientoHistorial;
                if (mov != null)
                {
                    e.Item.BackColor = mov.TipoMovimiento == "Entrada" ? Color.LightGreen : Color.MistyRose;
                }
            };
        }

        private void TabsControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TabsControl.SelectedTab == tabPageHistorial)
            {
                CargarHistorial();
            }
            else if (TabsControl.SelectedTab == tabPage1) // Tab ENTRADAS
            {
                CargarOrdenesDeCompra();
            }
            else if (TabsControl.SelectedTab == tabPageInventario) // ? Tab INVENTARIO
            {
                CargarInventarioCompleto();
            }
            else if (TabsControl.SelectedTab == tabPageConsultaCasa) // ? Tab CONSULTA POR CASA
            {
                CargarManzanasConsulta();
            }
            
            // Ajustar layout al cambiar de tab
            FormAlmacen_Resize(this, EventArgs.Empty);
        }

        public class MovimientoHistorial
        {
            public DateTime Fecha { get; set; }
            public string TipoMovimiento { get; set; }
            public string Clave { get; set; }
            public string Descripcion { get; set; }
            public string Unidad { get; set; }
            public decimal Cantidad { get; set; }
            public string Usuario { get; set; }
            public decimal Precio { get; set; }
            public decimal Importe => Math.Round( Precio * Cantidad, 2 );
            public string Manzana { get; set; }
            public string Lote { get; set; }
            public string Prototipo { get; set; }
            public string Justificacion { get; set; }

            public string Casa => $"M{Manzana}-L{Lote}";
            public Image Icono => TipoMovimiento == "Entrada" ? iconEntrada : iconSalida;

            // Campos estáticos con inicialización segura
            private static Image _iconEntrada;
            private static Image _iconSalida;

            public static Image iconEntrada
            {
                get
                {
                    if ( _iconEntrada == null )
                    {
                        try
                        {
                            string path = Path.Combine( Application.StartupPath, "greenarrow.png" );
                            if ( File.Exists(path) )
                                _iconEntrada = Image.FromFile( path );
                            else
                                _iconEntrada = CreateDefaultIcon( Color.Green );
                        }
                        catch
                        {
                            _iconEntrada = CreateDefaultIcon( Color.Green );
                        }
                    }
                    return _iconEntrada;
                }
            }

            public static Image iconSalida
            {
                get
                {
                    if ( _iconSalida == null )
                    {
                        try
                        {
                            string path = Path.Combine( Application.StartupPath, "arrow.png" );
                            if ( File.Exists(path) )
                                _iconSalida = Image.FromFile( path );
                            else
                                _iconSalida = CreateDefaultIcon( Color.Red );
                        }
                        catch
                        {
                            _iconSalida = CreateDefaultIcon( Color.Red );
                        }
                    }
                    return _iconSalida;
                }
            }

            // Crear un ícono por defecto si no se encuentran las imágenes
            private static Image CreateDefaultIcon(Color color)
            {
                Bitmap bmp = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);
                    
                    // Dibujar una flecha simple
                    using (SolidBrush brush = new SolidBrush(color))
                    {
                        Point[] arrow = new Point[]
                        {
                            new Point(8, 16),
                            new Point(24, 16),
                            new Point(24, 8),
                            new Point(32, 16),
                            new Point(24, 24),
                            new Point(24, 16)
                        };
                        g.FillPolygon(brush, arrow);
                    }
                }
                return bmp;
            }
        }

        private void treeListViewHistorial_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}
