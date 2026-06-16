using BrightIdeasSoftware;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    public partial class FormHardProgress : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
        private List<NodoCategoria> nodosRaiz = new List<NodoCategoria>();
        private string prototipoActual = "";

        public FormHardProgress()
        {
            InitializeComponent();
            ConfigurarTreeListView();
            CargarManzanas();
            this.Load += FormHardProgress_Load;
            
            // Aplicar tema
            ThemeManager.AplicarTema(this);
        }

        private void FormHardProgress_Load(object sender, EventArgs e)
        {
            this.Text = "Avance Hard Progress - Sistema Calandria";
        }

        private void ConfigurarTreeListView()
        {
            // Configurar como TreeListView
            olvAvance.CanExpandGetter = delegate(object x) {
                var nodo = (NodoCategoria)x;
                return nodo.EsCategoria && nodo.Partidas.Count > 0;
            };
            
            olvAvance.ChildrenGetter = delegate(object x) {
                return ((NodoCategoria)x).Partidas;
            };

            olvAvance.FullRowSelect = true;
            olvAvance.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.DoubleClick;
            olvAvance.UseAlternatingBackColors = true;
            olvAvance.AlternateRowBackColor = Color.FromArgb(240, 248, 255);

            // Columnas
            var colNombre = new OLVColumn("Categoría / Partida", "Nombre") 
            { 
                Width = 350,
                IsEditable = false, 
                Sortable = false,
                FillsFreeSpace = true
            };

            var colCosto = new OLVColumn("Costo", "ImporteTotal")
            {
                Width = 130,
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                AspectToStringFormat = "{0:C2}",
                Sortable = false
            };

            // Columna Avance % (EDITABLE)
            var colAvance = new OLVColumn("Avance %", "AvancePorcentaje")
            {
                Width = 120,
                IsEditable = true,
                TextAlign = HorizontalAlignment.Center,
                AspectToStringFormat = "{0:N1}%",
                Sortable = false
            };
            
            colAvance.AspectPutter = (row, val) =>
            {
                var nodo = (NodoCategoria)row;
                if (double.TryParse(val.ToString(), out double porcentaje))
                {
                    porcentaje = Math.Max(0, Math.Min(100, porcentaje));
                    
                    if (nodo.EsCategoria)
                    {
                        // Aplicar a todas las partidas de la categoría
                        foreach (var partida in nodo.Partidas)
                        {
                            partida.AvancePorcentaje = porcentaje;
                            partida.ImporteEjecutado = partida.ImporteTotal * (porcentaje / 100.0);
                            GuardarAvancePartidaEnBD(partida);
                        }
                        RecalcularAvanceCategoria(nodo);
                    }
                    else
                    {
                        // Aplicar solo a la partida
                        nodo.AvancePorcentaje = porcentaje;
                        nodo.ImporteEjecutado = nodo.ImporteTotal * (porcentaje / 100.0);
                        GuardarAvancePartidaEnBD(nodo);
                        
                        // Recalcular el concepto padre
                        var padre = EncontrarPadre(nodo);
                        if (padre != null)
                        {
                            RecalcularAvanceCategoria(padre);
                            olvAvance.RefreshObject(padre);
                        }
                    }
                    
                    olvAvance.RefreshObject(nodo);
                    ActualizarTotales();
                    DibujarGraficas();
                }
            };

            // Columna Ejecutado (EDITABLE)
            var colEjecutado = new OLVColumn("Ejecutado", "ImporteEjecutado")
            {
                Width = 140,
                IsEditable = true,
                TextAlign = HorizontalAlignment.Right,
                AspectToStringFormat = "{0:C2}",
                Sortable = false
            };
            
            colEjecutado.AspectPutter = (row, val) =>
            {
                var nodo = (NodoCategoria)row;
                string valorStr = val.ToString().Replace("$", "").Replace(",", "").Trim();

                if (double.TryParse(valorStr, out double montoEjecutado))
                {
                    if (montoEjecutado > nodo.ImporteTotal)
                    {
                        MessageBox.Show($"El monto ejecutado (${montoEjecutado:N2}) no puede ser mayor al presupuestado (${nodo.ImporteTotal:N2})",
                            "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (montoEjecutado < 0)
                    {
                        MessageBox.Show("El monto ejecutado no puede ser negativo",
                            "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (nodo.EsCategoria)
                    {
                        // Distribuir proporcionalmente entre partidas
                        double totalPartidas = nodo.Partidas.Sum(p => p.ImporteTotal);
                        if (totalPartidas > 0)
                        {
                            foreach (var partida in nodo.Partidas)
                            {
                                double proporcion = partida.ImporteTotal / totalPartidas;
                                partida.ImporteEjecutado = montoEjecutado * proporcion;
                                partida.AvancePorcentaje = partida.ImporteTotal > 0 
                                    ? (partida.ImporteEjecutado / partida.ImporteTotal) * 100.0 
                                    : 0;
                                GuardarAvancePartidaEnBD(partida);
                            }
                        }
                        RecalcularAvanceCategoria(nodo);
                    }
                    else
                    {
                        nodo.ImporteEjecutado = montoEjecutado;
                        nodo.AvancePorcentaje = nodo.ImporteTotal > 0 
                            ? (montoEjecutado / nodo.ImporteTotal) * 100.0 
                            : 0;
                        GuardarAvancePartidaEnBD(nodo);
                        
                        var padre = EncontrarPadre(nodo);
                        if (padre != null)
                        {
                            RecalcularAvanceCategoria(padre);
                            olvAvance.RefreshObject(padre);
                        }
                    }

                    olvAvance.RefreshObject(nodo);
                    ActualizarTotales();
                    DibujarGraficas();
                }
            };

            olvAvance.AllColumns.AddRange(new[] { colNombre, colCosto, colAvance, colEjecutado });
            olvAvance.RebuildColumns();

            // Aplicar formato condicional para partidas completadas
            olvAvance.RowFormatter = delegate(OLVListItem item)
            {
                var nodo = (NodoCategoria)item.RowObject;
                
                // Si la partida está completada al 100%, ponerla en gris
                if (!nodo.EsCategoria && nodo.AvancePorcentaje >= 100)
                {
                    item.BackColor = Color.FromArgb(220, 220, 220);
                    item.ForeColor = Color.FromArgb(100, 100, 100);
                    
                    // Hacer que el texto se vea "tachado" visualmente
                    Font currentFont = item.Font ?? olvAvance.Font;
                    item.Font = new Font(currentFont, FontStyle.Strikeout);
                }
            };

            // Validación en CellEditFinishing
            olvAvance.CellEditFinishing += (s, e) =>
            {
                if (e.Column.Text == "Avance %")
                {
                    if (double.TryParse(e.NewValue?.ToString(), out double val))
                    {
                        if (val < 0 || val > 100)
                        {
                            MessageBox.Show("El avance debe estar entre 0 y 100%", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            e.Cancel = true;
                        }
                    }
                }
                else if (e.Column.Text == "Ejecutado")
                {
                    string valorStr = e.NewValue?.ToString().Replace("$", "").Replace(",", "").Trim();
                    if (double.TryParse(valorStr, out double val))
                    {
                        var nodo = (NodoCategoria)e.RowObject;
                        if (val > nodo.ImporteTotal || val < 0)
                        {
                            e.Cancel = true;
                        }
                    }
                }
            };
            
            // Prevenir edición de partidas completadas
            olvAvance.CellEditStarting += (s, e) =>
            {
                var nodo = (NodoCategoria)e.RowObject;
                
                // Si la partida está completada (100%), no permitir edición
                if (!nodo.EsCategoria && nodo.AvancePorcentaje >= 100)
                {
                    MessageBox.Show("Esta partida ya fue marcada como completada (100%) desde Estimación por Conceptos.\n\nNo se puede editar.", 
                        "Partida Completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Cancel = true;
                }
            };
        }

        private NodoCategoria EncontrarPadre(NodoCategoria hijo)
        {
            foreach (var raiz in nodosRaiz)
            {
                if (raiz.Partidas.Contains(hijo))
                    return raiz;
            }
            return null;
        }

        private void RecalcularAvanceCategoria(NodoCategoria categoria)
        {
            if (!categoria.EsCategoria || categoria.Partidas.Count == 0)
                return;

            double totalPartidas = categoria.Partidas.Sum(p => p.ImporteTotal);
            double ejecutadoPartidas = categoria.Partidas.Sum(p => p.ImporteEjecutado);

            categoria.ImporteTotal = totalPartidas;
            categoria.ImporteEjecutado = ejecutadoPartidas;
            categoria.AvancePorcentaje = totalPartidas > 0 
                ? (ejecutadoPartidas / totalPartidas) * 100.0 
                : 0;
        }

        private void CargarManzanas()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT DISTINCT Manzana FROM InventarioCasas ORDER BY Manzana";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        cmbManzana.Items.Clear();
                        while (reader.Read())
                        {
                            cmbManzana.Items.Add(reader["Manzana"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar manzanas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbManzana_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbManzana.SelectedItem == null) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT DISTINCT Lote FROM InventarioCasas WHERE Manzana = @m ORDER BY Lote";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", cmbManzana.SelectedItem.ToString());
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            cmbLote.Items.Clear();
                            while (reader.Read())
                            {
                                cmbLote.Items.Add(reader["Lote"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar lotes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCargarAvance_Click(object sender, EventArgs e)
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
            {
                MessageBox.Show("Por favor selecciona Manzana y Lote", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();

            prototipoActual = ObtenerPrototipo(manzana, lote);
            if (string.IsNullOrEmpty(prototipoActual))
            {
                MessageBox.Show($"No se encontró el prototipo para M{manzana}-L{lote}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.Text = $"Avance Hard Progress - M{manzana} L{lote} ({prototipoActual})";

            CargarAvanceJerarquico(manzana, lote);
        }

        private string ObtenerPrototipo(string manzana, string lote)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzana);
                        cmd.Parameters.AddWithValue("@l", lote);
                        var result = cmd.ExecuteScalar();
                        return (result?.ToString() ?? "").Trim();
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        private void CargarAvanceJerarquico(string manzana, string lote)
        {
            nodosRaiz = new List<NodoCategoria>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Determinar columna de costo
                    string columnaCosto = prototipoActual.ToUpper().Contains("TUNERA") ? "CostoTunera" : "CostoCalandra";
                    
                    // Verificar si existe la columna
                    var columnas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    using (SqlCommand cmdCols = new SqlCommand(@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PresupuestoObra'", conn))
                    using (SqlDataReader rc = cmdCols.ExecuteReader())
                    {
                        while (rc.Read())
                        {
                            columnas.Add(rc.GetString(0));
                        }
                    }

                    if (!columnas.Contains(columnaCosto))
                    {
                        columnaCosto = columnas.Contains("TOTAL") ? "TOTAL" : 
                                     columnas.Contains("CostoCalandra") ? "CostoCalandra" : "CostoTunera";
                    }

                    // Verificar si existe la columna Codigo en PresupuestoObra
                    bool tieneColumnaCodigo = columnas.Contains("Codigo");

                    // Cargar partidas desde PresupuestoObra
                    string sql;
                    if (tieneColumnaCodigo)
                    {
                        // Usar columna Codigo existente y ordenar NUMÉRICAMENTE
                        sql = $@"
                            SELECT 
                                ROW_NUMBER() OVER (
                                    ORDER BY 
                                        CASE 
                                            WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
                                            THEN TRY_CAST(Codigo AS INT)
                                            ELSE 999999 
                                        END,
                                        Codigo,
                                        Etapa, 
                                        Partida
                                ) as WBS,
                                Codigo,
                                Padre,
                                Etapa,
                                Partida,
                                ISNULL(CAST([{columnaCosto}] AS FLOAT),0) as ImporteTotal
                            FROM PresupuestoObra
                            ORDER BY 
                                CASE 
                                    WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
                                    THEN TRY_CAST(Codigo AS INT)
                                    ELSE 999999 
                                END,
                                Codigo,
                                Etapa, 
                                Partida";
                    }
                    else
                    {
                        // Si NO existe Codigo, usar orden alfabético por Padre (fallback)
                        sql = $@"
                            SELECT 
                                ROW_NUMBER() OVER (ORDER BY Padre, Etapa, Partida) as WBS,
                                Padre,
                                Etapa,
                                Partida,
                                ISNULL(CAST([{columnaCosto}] AS FLOAT),0) as ImporteTotal
                            FROM PresupuestoObra
                            ORDER BY Padre, Etapa, Partida";
                    }

                    var partidas = new List<Tuple<int, string, string, string, string, double>>();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int wbs = Convert.ToInt32(reader["WBS"]);
                            string codigo = tieneColumnaCodigo && !reader.IsDBNull(reader.GetOrdinal("Codigo")) 
                                ? reader["Codigo"].ToString() 
                                : "";
                            string padre = reader["Padre"]?.ToString() ?? "";
                            string etapa = reader["Etapa"]?.ToString() ?? "";
                            string partida = reader["Partida"]?.ToString() ?? "";
                            double importe = Convert.ToDouble(reader["ImporteTotal"]);

                            partidas.Add(Tuple.Create(wbs, codigo, padre, etapa, partida, importe));
                        }
                    }

                    // Cargar avances guardados
                    var avancesPartidas = CargarAvancesGuardadosJerarquico(conn, manzana, lote);

                    // ? CORRECCIÓN: Agrupar por PADRE (nombre de categoría) en lugar de Codigo
                    var categoriasDict = new Dictionary<string, NodoCategoria>(StringComparer.OrdinalIgnoreCase);

                    foreach (var partida in partidas)
                    {
                        string codigo = partida.Item2;
                        string padre = partida.Item3;
                        
                        // Crear o recuperar categoría usando PADRE como clave
                        if (!categoriasDict.ContainsKey(padre))
                        {
                            categoriasDict[padre] = new NodoCategoria
                            {
                                EsCategoria = true,
                                Nombre = padre,
                                Codigo = codigo,  // Guardar codigo solo para referencia
                                ImporteTotal = 0,
                                AvancePorcentaje = 0,
                                ImporteEjecutado = 0,
                                Partidas = new List<NodoCategoria>()
                            };
                        }

                        // Crear nodo partida
                        var nodoPartida = new NodoCategoria
                        {
                            EsCategoria = false,
                            WBS = partida.Item1,
                            Codigo = codigo,
                            Nombre = $"{partida.Item4} > {partida.Item5}",
                            Padre = padre,
                            Etapa = partida.Item4,
                            Partida = partida.Item5,
                            ImporteTotal = partida.Item6,
                            AvancePorcentaje = 0,
                            ImporteEjecutado = 0
                        };

                        // Aplicar avance guardado si existe
                        if (avancesPartidas.TryGetValue(partida.Item1, out var avance))
                        {
                            nodoPartida.AvancePorcentaje = avance.Item1;
                            nodoPartida.ImporteEjecutado = !double.IsNaN(avance.Item2) 
                                ? avance.Item2 
                                : nodoPartida.ImporteTotal * (nodoPartida.AvancePorcentaje / 100.0);
                        }

                        categoriasDict[padre].Partidas.Add(nodoPartida);
                    }

                    // Calcular totales por categoría
                    foreach (var categoria in categoriasDict.Values)
                    {
                        RecalcularAvanceCategoria(categoria);
                    }

                    // ? Ordenar las categorías por Codigo numérico (1-12)
                    nodosRaiz = categoriasDict.Values
                        .OrderBy(c => 
                        {
                            if (int.TryParse(c.Codigo, out int codigoNum))
                                return codigoNum;
                            return 999;
                        })
                        .ThenBy(c => c.Nombre)
                        .ToList();
                }

                // Establecer raíces y COLAPSAR todos los nodos
                olvAvance.Roots = nodosRaiz;
                olvAvance.CollapseAll();

                ActualizarTotales();
                DibujarGraficas();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar avance: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Dictionary<int, Tuple<double, double>> CargarAvancesGuardadosJerarquico(SqlConnection conn, string manzana, string lote)
        {
            var avances = new Dictionary<int, Tuple<double, double>>();

            try
            {
                // Crear tabla si no existe
                using (SqlCommand cmdCheck = new SqlCommand(@"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AvanceManualObra')
                    BEGIN
                        CREATE TABLE AvanceManualObra (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Manzana NVARCHAR(10), Lote NVARCHAR(10),
                            Prototipo NVARCHAR(50), WBS NVARCHAR(50),
                            Concepto NVARCHAR(200), ImporteTotal FLOAT,
                            AvancePorcentaje FLOAT,
                            FechaActualizacion DATETIME DEFAULT GETDATE()
                        );
                    END", conn))
                {
                    cmdCheck.ExecuteNonQuery();
                }

                // Detectar columnas disponibles
                bool tieneMonto = false;
                using (SqlCommand cmdCols = new SqlCommand(@"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AvanceManualObra' AND (COLUMN_NAME = 'MontoEjecutado' OR COLUMN_NAME = 'ImporteEjecutado')", conn))
                {
                    tieneMonto = (int)cmdCols.ExecuteScalar() > 0;
                }

                string sql = tieneMonto
                    ? "SELECT WBS, AvancePorcentaje, MontoEjecutado FROM AvanceManualObra WHERE Manzana = @m AND Lote = @l"
                    : "SELECT WBS, AvancePorcentaje FROM AvanceManualObra WHERE Manzana = @m AND Lote = @l";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@m", manzana);
                    cmd.Parameters.AddWithValue("@l", lote);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (int.TryParse(reader["WBS"]?.ToString(), out int wbs))
                            {
                                double avance = Convert.ToDouble(reader["AvancePorcentaje"]);
                                double monto = double.NaN;

                                if (tieneMonto && !reader.IsDBNull(reader.GetOrdinal("MontoEjecutado")))
                                {
                                    monto = Convert.ToDouble(reader["MontoEjecutado"]);
                                }

                                avances[wbs] = Tuple.Create(avance, monto);
                            }
                        }
                    }
                }
            }
            catch { }

            return avances;
        }

        private void GuardarAvancePartidaEnBD(NodoCategoria partida)
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null) return;

            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Verificar columnas disponibles
                    bool tieneImporteTotal = false;
                    bool tieneMonto = false;
                    using (SqlCommand cmdCols = new SqlCommand(@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AvanceManualObra'", conn))
                    using (SqlDataReader rc = cmdCols.ExecuteReader())
                    {
                        while (rc.Read())
                        {
                            var col = rc.GetString(0);
                            if (string.Equals(col, "ImporteTotal", StringComparison.OrdinalIgnoreCase))
                                tieneImporteTotal = true;
                            if (string.Equals(col, "MontoEjecutado", StringComparison.OrdinalIgnoreCase) || string.Equals(col, "ImporteEjecutado", StringComparison.OrdinalIgnoreCase))
                                tieneMonto = true;
                        }
                    }

                    string sql;
                    if (tieneImporteTotal && tieneMonto)
                    {
                        sql = @"IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                                   UPDATE AvanceManualObra SET AvancePorcentaje=@avance, Concepto=@concepto, ImporteTotal=@importe, MontoEjecutado=@monto, FechaActualizacion=GETDATE()
                                   WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                               ELSE
                                   INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, Concepto, ImporteTotal, MontoEjecutado, AvancePorcentaje)
                                   VALUES (@m, @l, @proto, @wbs, @concepto, @importe, @monto, @avance)";
                    }
                    else if (tieneImporteTotal && !tieneMonto)
                    {
                        sql = @"IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                                   UPDATE AvanceManualObra SET AvancePorcentaje=@avance, Concepto=@concepto, ImporteTotal=@importe, FechaActualizacion=GETDATE()
                                   WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                               ELSE
                                   INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, Concepto, ImporteTotal, AvancePorcentaje)
                                   VALUES (@m, @l, @proto, @wbs, @concepto, @importe, @avance)";
                    }
                    else
                    {
                        sql = @"IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                                   UPDATE AvanceManualObra SET AvancePorcentaje=@avance, Concepto=@concepto, FechaActualizacion=GETDATE()
                                   WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                               ELSE
                                   INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, Concepto, AvancePorcentaje)
                                   VALUES (@m, @l, @proto, @wbs, @concepto, @avance)";
                    }

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzana);
                        cmd.Parameters.AddWithValue("@l", lote);
                        cmd.Parameters.AddWithValue("@proto", prototipoActual ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@wbs", partida.WBS.ToString());
                        cmd.Parameters.AddWithValue("@avance", partida.AvancePorcentaje);
                        cmd.Parameters.AddWithValue("@concepto", partida.Padre ?? "");

                        if (tieneImporteTotal)
                        {
                            var pImp = cmd.Parameters.Add("@importe", SqlDbType.Decimal);
                            pImp.Precision = 18;
                            pImp.Scale = 2;
                            pImp.Value = Math.Round(partida.ImporteTotal, 2);
                        }

                        if (tieneMonto)
                        {
                            var pMont = cmd.Parameters.Add("@monto", SqlDbType.Decimal);
                            pMont.Precision = 18;
                            pMont.Scale = 2;
                            pMont.Value = Math.Round(partida.ImporteEjecutado, 2);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }

                // Actualizar avances por concepto
                try
                {
                    ActualizarAvanceConceptosEnBD(manzana, lote);
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar avance: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarAvanceConceptosEnBD(string manzana, string lote)
        {
            // Método simplificado - se puede expandir según necesidades
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AvanceManualConcepto')
                                  BEGIN
                                      UPDATE c SET 
                                          c.AvancePorcentaje = (
                                              SELECT AVG(p.AvancePorcentaje) 
                                              FROM AvanceManualObra p 
                                              WHERE p.Manzana = @m AND p.Lote = @l AND p.Concepto = c.Concepto
                                          ),
                                          c.FechaActualizacion = GETDATE()
                                      FROM AvanceManualConcepto c
                                      WHERE c.Manzana = @m AND c.Lote = @l
                                  END";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@m", manzana);
                        cmd.Parameters.AddWithValue("@l", lote);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        private void ActualizarTotales()
        {
            double totalPresupuestado = nodosRaiz.Sum(i => i.ImporteTotal);
            double totalEjecutado = nodosRaiz.Sum(i => i.ImporteEjecutado);
            double avanceGeneral = totalPresupuestado > 0 ? (totalEjecutado / totalPresupuestado) * 100 : 0;

            lblTotalPresupuestado.Text = $"Total Presupuestado: {totalPresupuestado:C2}";
            lblTotalEjecutado.Text = $"Total Ejecutado: {totalEjecutado:C2}";
            lblAvanceGeneral.Text = $"Avance General: {avanceGeneral:F1}%";

            progressBarAvance.Value = Math.Min(100, (int)avanceGeneral);

            // Contar partidas (no categorías)
            var todasPartidas = nodosRaiz.SelectMany(c => c.Partidas).ToList();
            int completadas = todasPartidas.Count(i => i.AvancePorcentaje >= 100);
            int enProgreso = todasPartidas.Count(i => i.AvancePorcentaje > 0 && i.AvancePorcentaje < 100);
            int sinIniciar = todasPartidas.Count(i => i.AvancePorcentaje == 0);

            lblEstadisticas.Text = $"Completadas: {completadas} | En Progreso: {enProgreso} | Sin Iniciar: {sinIniciar}";

            // Usar colores del tema
            if (avanceGeneral < 30)
                lblAvanceGeneral.ForeColor = ThemeManager.ColorError;
            else if (avanceGeneral < 70)
                lblAvanceGeneral.ForeColor = ThemeManager.ColorAdvertencia;
            else
                lblAvanceGeneral.ForeColor = ThemeManager.ColorExito;
        }

        private void DibujarGraficas()
        {
            if (nodosRaiz.Count == 0) return;

            int width = pictureBoxGrafica.Width;
            int height = pictureBoxGrafica.Height;

            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                DibujarGraficaPastel(g, new Rectangle(20, 20, 180, 180));
                DibujarGraficaBarras(g, new Rectangle(20, 220, width - 40, height - 240));
            }

            pictureBoxGrafica.Image?.Dispose();
            pictureBoxGrafica.Image = bmp;
        }

        private void DibujarGraficaPastel(Graphics g, Rectangle rect)
        {
            double totalPresupuestado = nodosRaiz.Sum(i => i.ImporteTotal);
            double totalEjecutado = nodosRaiz.Sum(i => i.ImporteEjecutado);
            double avanceGeneral = totalPresupuestado > 0 ? (totalEjecutado / totalPresupuestado) * 100 : 0;

            float anguloEjecutado = (float)(avanceGeneral * 3.6);
            float anguloRestante = 360 - anguloEjecutado;

            // Usar color principal del tema
            using (Brush brushEjecutado = new SolidBrush(ThemeManager.ColorPrincipal))
            {
                g.FillPie(brushEjecutado, rect, 0, anguloEjecutado);
            }

            using (Brush brushRestante = new SolidBrush(Color.FromArgb(220, 220, 220)))
            {
                g.FillPie(brushRestante, rect, anguloEjecutado, anguloRestante);
            }

            using (Pen pen = new Pen(ThemeManager.ColorSecundario, 2))
            {
                g.DrawEllipse(pen, rect);
            }

            string textoAvance = $"{avanceGeneral:F1}%";
            using (Font font = new Font("Segoe UI", 20, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(ThemeManager.ColorTextoOscuro))
            {
                SizeF textSize = g.MeasureString(textoAvance, font);
                PointF textPos = new PointF(rect.X + (rect.Width - textSize.Width) / 2, rect.Y + (rect.Height - textSize.Height) / 2);
                g.DrawString(textoAvance, font, textBrush, textPos);
            }

            using (Font fontLeyenda = new Font("Segoe UI", 9))
            using (Brush textBrush = new SolidBrush(ThemeManager.ColorTextoOscuro))
            {
                g.FillRectangle(new SolidBrush(ThemeManager.ColorPrincipal), 220, 40, 15, 15);
                g.DrawString("Ejecutado", fontLeyenda, textBrush, 240, 38);
                g.FillRectangle(new SolidBrush(Color.FromArgb(220, 220, 220)), 220, 65, 15, 15);
                g.DrawString("Restante", fontLeyenda, textBrush, 240, 63);
            }
        }

        private void DibujarGraficaBarras(Graphics g, Rectangle rect)
        {
            // Las categorías ya están en nodosRaiz
            var categorias = nodosRaiz
                .Select(c => new
                {
                    Categoria = c.Nombre,
                    Total = c.ImporteTotal,
                    Ejecutado = c.ImporteEjecutado
                })
                .OrderByDescending(c => c.Total)
                .ToList();

            if (categorias.Count == 0) return;

            double maxValor = categorias.Max(c => c.Total);
            if (maxValor == 0) return;

            int barWidth = Math.Max(40, (rect.Width - 60) / categorias.Count);
            int spacing = 10;

            using (Font font = new Font("Segoe UI", 7))
            using (Brush textBrush = new SolidBrush(ThemeManager.ColorTextoOscuro))
            using (Pen gridPen = new Pen(Color.LightGray, 1))
            {
                for (int i = 0; i <= 5; i++)
                {
                    int y = rect.Y + (rect.Height * i / 5);
                    g.DrawLine(gridPen, rect.X, y, rect.Right, y);
                }

                int x = rect.X + 30;
                foreach (var cat in categorias)
                {
                    int alturaTotal = (int)((cat.Total / maxValor) * (rect.Height - 60));
                    int alturaEjecutado = (int)((cat.Ejecutado / maxValor) * (rect.Height - 60));
                    int yBase = rect.Bottom - 40;

                    using (Brush brushFondo = new SolidBrush(Color.FromArgb(220, 220, 220)))
                    {
                        g.FillRectangle(brushFondo, x, yBase - alturaTotal, barWidth - spacing, alturaTotal);
                    }

                    // Usar color principal para la barra ejecutada
                    using (Brush brushEjecutado = new SolidBrush(ThemeManager.ColorPrincipal))
                    {
                        g.FillRectangle(brushEjecutado, x, yBase - alturaEjecutado, barWidth - spacing, alturaEjecutado);
                    }

                    string etiqueta = cat.Categoria;
                    if (etiqueta.Length > 15)
                        etiqueta = etiqueta.Substring(0, 12) + "...";

                    g.TranslateTransform(x + (barWidth - spacing) / 2, yBase + 5);
                    g.RotateTransform(-45);
                    g.DrawString(etiqueta, font, textBrush, 0, 0);
                    g.ResetTransform();

                    if (cat.Total > 0)
                    {
                        double porcentaje = (cat.Ejecutado / cat.Total) * 100;
                        string textoPorcentaje = $"{porcentaje:F0}%";
                        SizeF textSize = g.MeasureString(textoPorcentaje, font);

                        using (Font fontPorcentaje = new Font("Segoe UI", 8, FontStyle.Bold))
                        {
                            g.DrawString(textoPorcentaje, fontPorcentaje, textBrush,
                                x + (barWidth - spacing - textSize.Width) / 2,
                                yBase - alturaTotal - 15);
                        }
                    }

                    x += barWidth;
                }
            }
        }

        private void btnExportarPDF_Click(object sender, EventArgs e)
        {
            if (nodosRaiz.Count == 0)
            {
                MessageBox.Show("Primero carga el avance de una casa", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                FileName = $"HardProgress_M{cmbManzana.SelectedItem}_L{cmbLote.SelectedItem}_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    GenerarPDFAvance(sfd.FileName);
                    MessageBox.Show("PDF generado exitosamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    var result = MessageBox.Show("¿Deseas abrir el PDF?", "Abrir PDF", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al generar PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GenerarPDFAvance(string rutaPdf)
        {
            // Método simplificado que solo genera un PDF básico
            PdfDocument pdf = new PdfDocument();
            PdfPage page = pdf.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Arial", 12);

            gfx.DrawString($"Hard Progress - M{cmbManzana.SelectedItem} L{cmbLote.SelectedItem}", 
                font, XBrushes.Black, new XRect(0, 20, page.Width, 30), XStringFormats.TopCenter);

            double y = 60;
            foreach (var categoria in nodosRaiz)
            {
                gfx.DrawString($"{categoria.Nombre}: {categoria.AvancePorcentaje:F1}%", 
                    font, XBrushes.Black, 40, y);
                y += 20;
            }

            pdf.Save(rutaPdf);
        }

        private void FormHardProgress_Resize(object sender, EventArgs e)
        {
            if (nodosRaiz.Count > 0)
            {
                DibujarGraficas();
            }
        }

        private static string NormalizeForComparison(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            string normalized = input.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (char c in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC).ToLowerInvariant();
        }

        public void ShowEvidenciaPreview(Image image)
        {
            if (pictureBoxGrafica == null) return;

            if (pictureBoxGrafica.InvokeRequired)
            {
                pictureBoxGrafica.Invoke(new Action(() => ShowEvidenciaPreview(image)));
                return;
            }

            try
            {
                if (pictureBoxGrafica.Image != null)
                {
                    try { pictureBoxGrafica.Image.Dispose(); } catch { }
                    pictureBoxGrafica.Image = null;
                }

                if (image != null)
                {
                    pictureBoxGrafica.Image = new Bitmap(image);
                    pictureBoxGrafica.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (pictureBoxGrafica != null && pictureBoxGrafica.Image != null)
                {
                    try { pictureBoxGrafica.Image.Dispose(); } catch { }
                    pictureBoxGrafica.Image = null;
                }
            }
            catch { }

            base.OnFormClosing(e);
        }
    }
}