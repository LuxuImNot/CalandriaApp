using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace DynamicSepticSystem
{
    public partial class FormRepositorioPDFs : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
        private string manzanaFiltro;
        private string loteFiltro;

        public FormRepositorioPDFs(string manzana, string lote)
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            manzanaFiltro = manzana;
            loteFiltro = lote;
            this.Load += FormRepositorioPDFs_Load;
        }

        private void FormRepositorioPDFs_Load(object sender, EventArgs e)
        {
            this.Text = $"Repositorio de Estimaciones - M{manzanaFiltro} L{loteFiltro}";
            ConfigurarListView();
            CargarEstimaciones();
        }

        private void ConfigurarListView()
        {
            olvEstimaciones.FullRowSelect = true;
            olvEstimaciones.UseAlternatingBackColors = true;
            olvEstimaciones.AlternateRowBackColor = Color.FromArgb(240, 248, 255);
            olvEstimaciones.GridLines = true;
            olvEstimaciones.View = View.Details;

            var colFolio = new OLVColumn("Folio", "Folio") { Width = 120, IsEditable = false };
            var colFecha = new OLVColumn("Fecha", "FechaGeneracion") 
            { 
                Width = 150, 
                IsEditable = false,
                AspectToStringFormat = "{0:dd/MM/yyyy HH:mm}"
            };
            var colProveedor = new OLVColumn("Proveedor", "Proveedor") { Width = 200, IsEditable = false, FillsFreeSpace = true };
            var colTotal = new OLVColumn("Total Estimaci�n", "TotalEstimacion") 
            { 
                Width = 120, 
                IsEditable = false,
                TextAlign = HorizontalAlignment.Right,
                AspectToStringFormat = "{0:C2}"
            };
            var colNumero = new OLVColumn("No. Est.", "NumeroEstimacion") { Width = 70, IsEditable = false, TextAlign = HorizontalAlignment.Center };
            var colPartidas = new OLVColumn("Partidas", "TotalPartidas") { Width = 70, IsEditable = false, TextAlign = HorizontalAlignment.Center };

            olvEstimaciones.AllColumns.AddRange(new[] { colFolio, colNumero, colFecha, colProveedor, colTotal, colPartidas });
            olvEstimaciones.RebuildColumns();

            olvEstimaciones.DoubleClick += OlvEstimaciones_DoubleClick;
        }

        private void CargarEstimaciones()
        {
            try
            {
                var estimaciones = new List<EstimacionInfo>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT 
                            f.Id,
                            f.Folio,
                            f.Manzana,
                            f.Lote,
                            f.Prototipo,
                            f.FechaGeneracion,
                            f.Proveedor,
                            f.Descripcion,
                            f.TotalEstimacion,
                            f.NumeroEstimacion,
                            f.Observaciones,
                            COUNT(DISTINCT d.Id) AS TotalPartidas,
                            CASE WHEN EXISTS(SELECT 1 FROM PDFsEstimacion WHERE FolioId = f.Id) THEN 1 ELSE 0 END AS TienePDF
                        FROM FoliosEstimacion f
                        LEFT JOIN FoliosEstimacionDetalle d ON f.Id = d.FolioId
                        WHERE f.Manzana = @manzana AND f.Lote = @lote
                        GROUP BY f.Id, f.Folio, f.Manzana, f.Lote, f.Prototipo, f.FechaGeneracion, 
                                 f.Proveedor, f.Descripcion, f.TotalEstimacion, f.NumeroEstimacion, f.Observaciones
                        ORDER BY f.FechaGeneracion DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@manzana", manzanaFiltro);
                        cmd.Parameters.AddWithValue("@lote", loteFiltro);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                estimaciones.Add(new EstimacionInfo
                                {
                                    Id = reader.GetInt32(0),
                                    Folio = reader.GetString(1),
                                    Manzana = reader.GetString(2),
                                    Lote = reader.GetString(3),
                                    Prototipo = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    FechaGeneracion = reader.GetDateTime(5),
                                    Proveedor = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                    Descripcion = reader.IsDBNull(7) ? "" : reader.GetString(7),
                                    TotalEstimacion = reader.IsDBNull(8) ? 0 : reader.GetDouble(8),
                                    NumeroEstimacion = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                                    Observaciones = reader.IsDBNull(10) ? "" : reader.GetString(10),
                                    TotalPartidas = reader.GetInt32(11),
                                    TienePDF = reader.GetInt32(12) == 1
                                });
                            }
                        }
                    }
                }

                olvEstimaciones.SetObjects(estimaciones);
                lblTotal.Text = $"Total de estimaciones: {estimaciones.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar estimaciones: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OlvEstimaciones_DoubleClick(object sender, EventArgs e)
        {
            if (olvEstimaciones.SelectedObject == null) return;
            
            var estimacion = (EstimacionInfo)olvEstimaciones.SelectedObject;
            AbrirPDF(estimacion);
        }

        private void btnVerPDF_Click(object sender, EventArgs e)
        {
            if (olvEstimaciones.SelectedObject == null)
            {
                MessageBox.Show("Selecciona una estimaci�n", "Atenci�n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var estimacion = (EstimacionInfo)olvEstimaciones.SelectedObject;
            AbrirPDF(estimacion);
        }

        private void AbrirPDF(EstimacionInfo estimacion)
        {
            try
            {
                byte[] pdfBytes = null;
                string nombreArchivo = "";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT ContenidoPDF, NombreArchivo FROM PDFsEstimacion WHERE FolioId = @folioId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@folioId", estimacion.Id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                pdfBytes = (byte[])reader["ContenidoPDF"];
                                nombreArchivo = reader["NombreArchivo"].ToString();
                            }
                        }
                    }
                }

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    MessageBox.Show("No se encontr� el PDF para esta estimaci�n", "Atenci�n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Crear archivo temporal y abrirlo
                string tempPath = Path.Combine(Path.GetTempPath(), nombreArchivo);
                File.WriteAllBytes(tempPath, pdfBytes);
                Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnVerDetalle_Click(object sender, EventArgs e)
        {
            if (olvEstimaciones.SelectedObject == null)
            {
                MessageBox.Show("Selecciona una estimaci�n", "Atenci�n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var estimacion = (EstimacionInfo)olvEstimaciones.SelectedObject;
            MostrarDetalle(estimacion);
        }

        private void MostrarDetalle(EstimacionInfo estimacion)
        {
            try
            {
                var detalles = new List<DetalleEstimacion>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT 
                            CodigoConcepto,
                            NombreConcepto,
                            WBS,
                            NombrePartida,
                            MontoPresupuestado,
                            MontoEjecutado,
                            AvancePorcentaje
                        FROM FoliosEstimacionDetalle
                        WHERE FolioId = @folioId
                        ORDER BY CodigoConcepto, WBS";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@folioId", estimacion.Id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                detalles.Add(new DetalleEstimacion
                                {
                                    CodigoConcepto = reader.IsDBNull(0) ? "" : reader.GetString(0),
                                    NombreConcepto = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                    WBS = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                    NombrePartida = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                    MontoPresupuestado = reader.IsDBNull(4) ? 0 : reader.GetDouble(4),
                                    MontoEjecutado = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
                                    AvancePorcentaje = reader.IsDBNull(6) ? 0 : reader.GetDouble(6)
                                });
                            }
                        }
                    }
                }

                using (var formDetalle = new FormDetalleEstimacion(estimacion, detalles))
                {
                    formDetalle.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar detalle: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarEstimaciones();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (olvEstimaciones.SelectedObject == null)
            {
                MessageBox.Show("Selecciona una estimaci�n para eliminar", "Atenci�n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var estimacion = (EstimacionInfo)olvEstimaciones.SelectedObject;
            
            var result = MessageBox.Show(
                $"�Est�s seguro de eliminar la estimaci�n {estimacion.Folio}?\n\n" +
                $"ADVERTENCIA: Esta acci�n revertir� el avance de las partidas incluidas.\n\n" +
                $"Esta acci�n no se puede deshacer.",
                "Confirmar eliminaci�n",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // ?? CR�TICO: Revertir avances ANTES de eliminar
                    // Si falla la reversi�n, NO se debe eliminar la estimaci�n
                    bool reversionExitosa = RevertirAvancesEstimacion(conn, estimacion);
                    
                    if (!reversionExitosa)
                    {
                        MessageBox.Show(
                            "? ERROR: No se pudo revertir el avance de las partidas.\n\n" +
                            "La estimaci�n NO se eliminar� para mantener la integridad de los datos.\n\n" +
                            "Por favor, verifica:\n" +
                            "1. Que la tabla FoliosEstimacionDetalle tenga la columna WBS\n" +
                            "2. Que los registros tengan valores WBS v�lidos\n" +
                            "3. Que la tabla AvanceManualObra est� accesible\n\n" +
                            "Contacta al administrador del sistema si el problema persiste.",
                            "Error en Reversi�n de Avances",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return; // ?? DETENER la eliminaci�n
                    }
                    
                    // ? Solo si la reversi�n fue exitosa, eliminar el folio
                    // El CASCADE eliminar� autom�ticamente el detalle y el PDF
                    string sql = "DELETE FROM FoliosEstimacion WHERE Id = @id";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", estimacion.Id);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "? Estimaci�n eliminada correctamente y avances revertidos",
                    "�xito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                
                CargarEstimaciones();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"? Error al eliminar estimaci�n:\n\n{ex.Message}\n\n" +
                    $"La estimaci�n NO fue eliminada.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool RevertirAvancesEstimacion(SqlConnection conn, EstimacionInfo estimacion)
        {
            try
            {
                // 1. Primero verificar que la columna WBS existe en FoliosEstimacionDetalle
                bool tieneColumnaWBS = false;
                string sqlCheckWBS = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'FoliosEstimacionDetalle' 
                    AND COLUMN_NAME = 'WBS'";
                
                using (SqlCommand cmdCheck = new SqlCommand(sqlCheckWBS, conn))
                {
                    int count = (int)cmdCheck.ExecuteScalar();
                    tieneColumnaWBS = count > 0;
                }

                if (!tieneColumnaWBS)
                {
                    System.Diagnostics.Debug.WriteLine("?? La tabla FoliosEstimacionDetalle NO tiene la columna WBS. No se puede revertir avances.");
                    return false; // ? Fall�: columna WBS no existe
                }

                // ? NUEVO: Verificar si existe columna IdPresupuestoObra (v�nculo permanente)
                bool tieneIdPresupuestoObra = false;
                string sqlCheckId = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'FoliosEstimacionDetalle' 
                    AND COLUMN_NAME = 'IdPresupuestoObra'";
                
                using (SqlCommand cmdCheckId = new SqlCommand(sqlCheckId, conn))
                {
                    tieneIdPresupuestoObra = (int)cmdCheckId.ExecuteScalar() > 0;
                }

                System.Diagnostics.Debug.WriteLine(
                    $"?? M�todo de reversi�n: {(tieneIdPresupuestoObra ? "V�NCULO PERMANENTE (IdPresupuestoObra)" : "LEGACY (WBS)")}");

                // ? VERIFICAR si AvanceManualObra tambi�n tiene IdPresupuestoObra
                bool avanceTieneIdPresupuesto = false;
                using (SqlCommand cmdCheckAvance = new SqlCommand(@"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'AvanceManualObra' 
                    AND COLUMN_NAME = 'IdPresupuestoObra'", conn))
                {
                    avanceTieneIdPresupuesto = (int)cmdCheckAvance.ExecuteScalar() > 0;
                }

                bool usarVinculoPermanente = tieneIdPresupuestoObra && avanceTieneIdPresupuesto;
                
                if (usarVinculoPermanente)
                {
                    System.Diagnostics.Debug.WriteLine("? Usando V�NCULO PERMANENTE - Reversi�n 100% segura contra cambios de WBS");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("?? Usando m�todo LEGACY - Vulnerable a cambios de WBS posteriores");
                }

                // 2. Obtener todas las partidas incluidas en esta estimaci�n
                var partidasDetalle = new List<(int? WBS, int? IdPresupuestoObra, string NombrePartida)>();
                var codigosConceptos = new List<string>();
                
                string sqlDetalle;
                if (tieneIdPresupuestoObra)
                {
                    sqlDetalle = @"
                        SELECT DISTINCT 
                            WBS, 
                            IdPresupuestoObra,
                            NombrePartida,
                            CodigoConcepto 
                        FROM FoliosEstimacionDetalle 
                        WHERE FolioId = @folioId";
                }
                else
                {
                    sqlDetalle = @"
                        SELECT DISTINCT 
                            WBS, 
                            NULL AS IdPresupuestoObra,
                            NombrePartida,
                            CodigoConcepto 
                        FROM FoliosEstimacionDetalle 
                        WHERE FolioId = @folioId 
                        AND WBS IS NOT NULL";
                }
                
                using (SqlCommand cmd = new SqlCommand(sqlDetalle, conn))
                {
                    cmd.Parameters.AddWithValue("@folioId", estimacion.Id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int? wbs = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0);
                            int? idPresupuesto = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1);
                            string nombrePartida = reader.IsDBNull(2) ? "" : reader.GetString(2);
							
                            if (wbs.HasValue || idPresupuesto.HasValue)
                            {
                                partidasDetalle.Add((wbs, idPresupuesto, nombrePartida));
                            }
							
                            if (!reader.IsDBNull(3))
                            {
                                string codigo = reader.GetString(3);
                                if (!codigosConceptos.Contains(codigo))
                                {
                                    codigosConceptos.Add(codigo);
                                }
                            }
                        }
                    }
                }

                if (partidasDetalle.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("?? No hay partidas para revertir en esta estimaci�n");
                    return true;
                }

                System.Diagnostics.Debug.WriteLine($"?? Iniciando reversi�n de {partidasDetalle.Count} partidas de estimaci�n {estimacion.Folio}");

                // 3. Para cada partida, verificar si tiene otras estimaciones POSTERIORES
                int partidasRevertidas = 0;
                int partidasConM2Revertidos = 0;
                int erroresReversion = 0;

                // Detectar columna WBS en AvanceManualObra
                bool tieneWBSCorrectoAvance = false;
                using (SqlCommand cmdCheck = new SqlCommand(@"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'AvanceManualObra' AND COLUMN_NAME = 'WBS_Correcto'", conn))
                {
                    tieneWBSCorrectoAvance = (int)cmdCheck.ExecuteScalar() > 0;
                }
                string colWBSAvance = tieneWBSCorrectoAvance ? "WBS_Correcto" : "WBS";

                foreach (var (wbs, idPresupuesto, nombrePartida) in partidasDetalle)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"\n--- PROCESANDO PARTIDA: {nombrePartida} ---");
                        System.Diagnostics.Debug.WriteLine($"    WBS: {(wbs.HasValue ? wbs.Value.ToString() : "NULL")}");
                        System.Diagnostics.Debug.WriteLine($"    IdPresupuesto: {(idPresupuesto.HasValue ? idPresupuesto.Value.ToString() : "NULL")}");
                        
                        // ? PRIORIZAR v�nculo permanente si est� disponible
                        string sqlVerificar;
                        SqlCommand cmdVerificar;
                        
                        if (usarVinculoPermanente && idPresupuesto.HasValue)
                        {
                            // M�TODO SEGURO: Usar IdPresupuestoObra
                            System.Diagnostics.Debug.WriteLine($"    M�todo: V�NCULO PERMANENTE (IdPresupuestoObra={idPresupuesto.Value})");
                            
                            sqlVerificar = @"
                                SELECT COUNT(*) 
                                FROM FoliosEstimacionDetalle d
                                INNER JOIN FoliosEstimacion f ON d.FolioId = f.Id
                                WHERE d.IdPresupuestoObra = @idPresupuesto
                                AND f.Manzana = @manzana 
                                AND f.Lote = @lote
                                AND f.FechaGeneracion > @fecha";
							
                            cmdVerificar = new SqlCommand(sqlVerificar, conn);
                            cmdVerificar.Parameters.AddWithValue("@idPresupuesto", idPresupuesto.Value);
                        }
                        else if (wbs.HasValue)
                        {
                            // M�TODO LEGACY: Usar WBS (vulnerable a reorganizaciones)
                            System.Diagnostics.Debug.WriteLine($"    M�todo: LEGACY (WBS={wbs.Value})");
							
                            sqlVerificar = @"
                                SELECT COUNT(*) 
                                FROM FoliosEstimacionDetalle d
                                INNER JOIN FoliosEstimacion f ON d.FolioId = f.Id
                                WHERE d.WBS = @wbs 
                                AND d.WBS IS NOT NULL
                                AND f.Manzana = @manzana 
                                AND f.Lote = @lote
                                AND f.FechaGeneracion > @fecha";
							
                            cmdVerificar = new SqlCommand(sqlVerificar, conn);
                            cmdVerificar.Parameters.AddWithValue("@wbs", wbs.Value);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"?? Partida sin WBS ni IdPresupuestoObra: {nombrePartida}");
                            continue;
                        }
						
                        cmdVerificar.Parameters.AddWithValue("@manzana", estimacion.Manzana);
                        cmdVerificar.Parameters.AddWithValue("@lote", estimacion.Lote);
                        cmdVerificar.Parameters.AddWithValue("@fecha", estimacion.FechaGeneracion);
                        
                        System.Diagnostics.Debug.WriteLine($"    Par�metros b�squeda:");
                        System.Diagnostics.Debug.WriteLine($"       Manzana: {estimacion.Manzana}");
                        System.Diagnostics.Debug.WriteLine($"       Lote: {estimacion.Lote}");
                        System.Diagnostics.Debug.WriteLine($"       Fecha: {estimacion.FechaGeneracion:yyyy-MM-dd HH:mm:ss}");
						
                        int estimacionesPosteriores = (int)cmdVerificar.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine($"    Estimaciones posteriores encontradas: {estimacionesPosteriores}");

                        if (estimacionesPosteriores == 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"    ? NO hay estimaciones posteriores - Procediendo a revertir...");
                            
                            // Verificar columnas disponibles
                            bool tieneMetrosCuadrados = false;
                            bool tieneFechaFinalizacion = false;
							
                            using (SqlCommand cmdCheckCols = new SqlCommand(@"
                                SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS 
                                WHERE TABLE_NAME = 'AvanceManualObra' 
                                AND COLUMN_NAME IN ('MetrosCuadrados', 'FechaFinalizacion')", conn))
                            using (var readerCheck = cmdCheckCols.ExecuteReader())
                            {
                                while (readerCheck.Read())
                                {
                                    string colName = readerCheck.GetString(0);
                                    if (colName == "MetrosCuadrados") tieneMetrosCuadrados = true;
                                    if (colName == "FechaFinalizacion") tieneFechaFinalizacion = true;
                                }
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"    Columnas disponibles: MetrosCuadrados={tieneMetrosCuadrados}, FechaFinalizacion={tieneFechaFinalizacion}");
							
                            // ? CONSTRUIR UPDATE usando v�nculo permanente si est� disponible
                            string sqlRevertir;
                            SqlCommand cmdRevertir;
							
                            if (avanceTieneIdPresupuesto && idPresupuesto.HasValue && wbs.HasValue)
                            {
                                // M�TODO H�BRIDO: Buscar por IdPresupuestoObra O por WBS (compatibilidad con datos antiguos)
                                // Esto permite revertir registros que fueron guardados antes de implementar IdPresupuestoObra
                                sqlRevertir = @"
                                    UPDATE AvanceManualObra 
                                    SET AvancePorcentaje = 0.0, 
                                        MontoEjecutado = 0.0" +
                                        (tieneMetrosCuadrados ? ", MetrosCuadrados = 0.0" : "") +
                                        (tieneFechaFinalizacion ? ", FechaFinalizacion = NULL" : "") + @",
                                        IdPresupuestoObra = @idPresupuesto,
                                        FechaActualizacion = GETDATE()
                                    WHERE Manzana = @m 
                                    AND Lote = @l 
                                    AND (IdPresupuestoObra = @idPresupuesto OR " + colWBSAvance + @" = @wbs)";
                                
                                System.Diagnostics.Debug.WriteLine($"    SQL UPDATE (H�BRIDO - IdPresupuesto O WBS):");
                                System.Diagnostics.Debug.WriteLine($"       IdPresupuestoObra = {idPresupuesto.Value} O {colWBSAvance} = {wbs.Value}");
                                System.Diagnostics.Debug.WriteLine($"       ? Actualizar� IdPresupuestoObra si el registro solo ten�a WBS");
							
                                cmdRevertir = new SqlCommand(sqlRevertir, conn);
                                cmdRevertir.Parameters.AddWithValue("@idPresupuesto", idPresupuesto.Value);
                                cmdRevertir.Parameters.AddWithValue("@wbs", wbs.Value.ToString());
                                
                                // ?? DIAGN�STICO: Verificar qu� registros existen
                                string sqlVerificarExiste = $@"
                                    SELECT COUNT(*) as Total,
                                           SUM(CASE WHEN IdPresupuestoObra = @idPresupuesto THEN 1 ELSE 0 END) as ConId,
                                           SUM(CASE WHEN IdPresupuestoObra IS NULL AND {colWBSAvance} = @wbs THEN 1 ELSE 0 END) as SoloWBS,
                                           ISNULL(AVG(AvancePorcentaje), 0) as AvanceActual,
                                           ISNULL(SUM(MontoEjecutado), 0) as MontoActual
                                    FROM AvanceManualObra 
                                    WHERE Manzana = @m AND Lote = @l 
                                    AND (IdPresupuestoObra = @idPresupuesto OR {colWBSAvance} = @wbs)";
                                
                                using (SqlCommand cmdVerifExiste = new SqlCommand(sqlVerificarExiste, conn))
                                {
                                    cmdVerifExiste.Parameters.AddWithValue("@m", estimacion.Manzana);
                                    cmdVerifExiste.Parameters.AddWithValue("@l", estimacion.Lote);
                                    cmdVerifExiste.Parameters.AddWithValue("@idPresupuesto", idPresupuesto.Value);
                                    cmdVerifExiste.Parameters.AddWithValue("@wbs", wbs.Value.ToString());
                                    
                                    using (var readerExiste = cmdVerifExiste.ExecuteReader())
                                    {
                                        if (readerExiste.Read())
                                        {
                                            int total = readerExiste.GetInt32(0);
                                            int conId = readerExiste.GetInt32(1);
                                            int soloWBS = readerExiste.GetInt32(2);
                                            double avanceActual = Convert.ToDouble(readerExiste.GetValue(3));
                                            double montoActual = Convert.ToDouble(readerExiste.GetValue(4));
                                            
                                            System.Diagnostics.Debug.WriteLine($"    ?? Registros encontrados en AvanceManualObra:");
                                            System.Diagnostics.Debug.WriteLine($"       Total: {total}");
                                            System.Diagnostics.Debug.WriteLine($"       Con IdPresupuesto={idPresupuesto.Value}: {conId}");
                                            System.Diagnostics.Debug.WriteLine($"       Solo con WBS={wbs.Value}: {soloWBS}");
                                            System.Diagnostics.Debug.WriteLine($"       Avance actual: {avanceActual}%");
                                            System.Diagnostics.Debug.WriteLine($"       Monto actual: ${montoActual:N2}");
                                            
                                            if (total == 0)
                                            {
                                                System.Diagnostics.Debug.WriteLine($"    ?? NO HAY REGISTROS para revertir en M{estimacion.Manzana}-L{estimacion.Lote}");
                                            }
                                            else if (soloWBS > 0)
                                            {
                                                System.Diagnostics.Debug.WriteLine($"    ?? Se encontraron {soloWBS} registro(s) antiguo(s) (solo WBS)");
                                                System.Diagnostics.Debug.WriteLine($"       Se actualizar� su IdPresupuestoObra para futuras operaciones");
                                            }
                                        }
                                    }
                                }
                            }
                            else if (avanceTieneIdPresupuesto && idPresupuesto.HasValue)
                            {
                                // Solo IdPresupuestoObra disponible (no hay WBS confiable)
                                sqlRevertir = @"
                                    UPDATE AvanceManualObra 
                                    SET AvancePorcentaje = 0.0, 
                                        MontoEjecutado = 0.0" +
                                        (tieneMetrosCuadrados ? ", MetrosCuadrados = 0.0" : "") +
                                        (tieneFechaFinalizacion ? ", FechaFinalizacion = NULL" : "") + @",
                                        FechaActualizacion = GETDATE()
                                    WHERE Manzana = @m 
                                    AND Lote = @l 
                                    AND IdPresupuestoObra = @idPresupuesto";
                                
                                System.Diagnostics.Debug.WriteLine($"    SQL UPDATE (solo IdPresupuestoObra):");
                                System.Diagnostics.Debug.WriteLine($"       IdPresupuestoObra = {idPresupuesto.Value}");
							
                                cmdRevertir = new SqlCommand(sqlRevertir, conn);
                                cmdRevertir.Parameters.AddWithValue("@idPresupuesto", idPresupuesto.Value);
                            }
                            else if (wbs.HasValue)
                            {
                                // M�TODO LEGACY: Usar WBS (vulnerable a reorganizaciones)
                                // Este caso solo deber�a ocurrir en sistemas sin IdPresupuestoObra
                                sqlRevertir = @"
                                    UPDATE AvanceManualObra 
                                    SET AvancePorcentaje = 0.0, 
                                        MontoEjecutado = 0.0" +
                                        (tieneMetrosCuadrados ? ", MetrosCuadrados = 0.0" : "") +
                                        (tieneFechaFinalizacion ? ", FechaFinalizacion = NULL" : "") + @",
                                        FechaActualizacion = GETDATE()
                                    WHERE Manzana = @m 
                                    AND Lote = @l 
                                    AND " + colWBSAvance + @" = @wbs";
                                
                                System.Diagnostics.Debug.WriteLine($"    SQL UPDATE (LEGACY - solo WBS):");
                                System.Diagnostics.Debug.WriteLine($"       {colWBSAvance} = {wbs.Value}");
                                System.Diagnostics.Debug.WriteLine($"       ?? ADVERTENCIA: M�todo legacy, vulnerable a cambios de WBS");
							
                                cmdRevertir = new SqlCommand(sqlRevertir, conn);
                                cmdRevertir.Parameters.AddWithValue("@wbs", wbs.Value.ToString());
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"    ?? No se puede construir UPDATE (sin WBS ni IdPresupuesto)");
                                continue;
                            }
							
                            cmdRevertir.Parameters.AddWithValue("@m", estimacion.Manzana);
                            cmdRevertir.Parameters.AddWithValue("@l", estimacion.Lote);
                            
                            System.Diagnostics.Debug.WriteLine($"    Ejecutando UPDATE...");
							
                            int rowsAffected = cmdRevertir.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine($"    Filas afectadas: {rowsAffected}");
                            
                            if (rowsAffected > 0)
                            {
                                partidasRevertidas++;
                                
                                string metodoUsado = avanceTieneIdPresupuesto && idPresupuesto.HasValue && wbs.HasValue 
                                    ? "H�BRIDO (actualiz� IdPresupuesto si faltaba)" 
                                    : avanceTieneIdPresupuesto && idPresupuesto.HasValue 
                                        ? "PERMANENTE" 
                                        : "LEGACY";
                                
                                System.Diagnostics.Debug.WriteLine(
                                    $"? Partida revertida: {nombrePartida} " +
                                    $"(WBS={wbs}, IdPresupuesto={idPresupuesto}) - M�todo: {metodoUsado}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine(
                                    $"?? UPDATE no afect� ninguna fila para: {nombrePartida} " +
                                    $"(WBS={wbs}, IdPresupuesto={idPresupuesto})");
                                System.Diagnostics.Debug.WriteLine($"    Posibles causas:");
                                System.Diagnostics.Debug.WriteLine($"    1. No existe registro en AvanceManualObra para esta casa");
                                System.Diagnostics.Debug.WriteLine($"    2. Manzana/Lote no coincide (verificar formato)");
                                System.Diagnostics.Debug.WriteLine($"    3. El progreso nunca fue guardado para esta partida");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"?? Partida NO revertida (tiene {estimacionesPosteriores} estimaci�n(es) posterior(es)): {nombrePartida}");
                        }
                    }
                    catch (SqlException ex)
                    {
                        erroresReversion++;
                        System.Diagnostics.Debug.WriteLine($"? Error SQL al revertir {nombrePartida}: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"   Error Number: {ex.Number}");
                        System.Diagnostics.Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
                    }
                    catch (Exception ex)
                    {
                        erroresReversion++;
                        System.Diagnostics.Debug.WriteLine($"? Error general al revertir {nombrePartida}: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
                    }
                }

                // Log resumen de partidas
                System.Diagnostics.Debug.WriteLine(
                    $"?? RESUMEN REVERSI�N PARTIDAS:\n" +
                    $"   - M�todo usado: {(usarVinculoPermanente ? "V�NCULO PERMANENTE ?" : "LEGACY (WBS) ??")}\n" +
                    $"   - Total partidas en estimaci�n: {partidasDetalle.Count}\n" +
                    $"   - Partidas revertidas a 0%: {partidasRevertidas}\n" +
                    $"   - Errores durante reversi�n: {erroresReversion}");

                // ? Si hubo errores, retornar falso
                if (erroresReversion > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"? REVERSI�N FALLIDA: {erroresReversion} errores encontrados");
                    return false;
                }

                // 4. Revertir conceptos (WBS negativo)
                int conceptosRevertidos = 0;
                int erroresConceptos = 0;
                
                foreach (string codigoConcepto in codigosConceptos)
                {
                    try
                    {
                        if (int.TryParse(codigoConcepto, out int codigo))
                        {
                            int wbsConcepto = -codigo; // WBS negativo para conceptos
							
                            // Verificar si hay estimaciones posteriores con este concepto
                            string sqlVerificar = @"
                                SELECT COUNT(*) 
                                FROM FoliosEstimacionDetalle d
                                INNER JOIN FoliosEstimacion f ON d.FolioId = f.Id
                                WHERE d.CodigoConcepto = @codigo 
                                AND f.Manzana = @manzana 
                                AND f.Lote = @lote
                                AND f.FechaGeneracion > @fecha";
							
                            int estimacionesPosteriores = 0;
                            using (SqlCommand cmd = new SqlCommand(sqlVerificar, conn))
                            {
                                cmd.Parameters.AddWithValue("@codigo", codigoConcepto);
                                cmd.Parameters.AddWithValue("@manzana", estimacion.Manzana);
                                cmd.Parameters.AddWithValue("@lote", estimacion.Lote);
                                cmd.Parameters.AddWithValue("@fecha", estimacion.FechaGeneracion);
                                estimacionesPosteriores = (int)cmd.ExecuteScalar();
                            }

                            if (estimacionesPosteriores == 0)
                            {
                                // No hay estimaciones posteriores, eliminar el registro del concepto
                                string sqlRevertir = $@"
                                    DELETE FROM AvanceManualObra 
                                    WHERE Manzana = @m 
                                    AND Lote = @l 
                                    AND {colWBSAvance} = @wbs";
							
                                using (SqlCommand cmd = new SqlCommand(sqlRevertir, conn))
                                {
                                    cmd.Parameters.AddWithValue("@m", estimacion.Manzana);
                                    cmd.Parameters.AddWithValue("@l", estimacion.Lote);
                                    cmd.Parameters.AddWithValue("@wbs", wbsConcepto.ToString());
							
                                    int rowsAffected = cmd.ExecuteNonQuery();
                                    if (rowsAffected > 0)
                                    {
                                        conceptosRevertidos++;
                                        System.Diagnostics.Debug.WriteLine(
                                            $"? Concepto eliminado de avance: C�digo={codigoConcepto} (WBS={wbsConcepto})");
                                    }
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine(
                                    $"?? Concepto NO eliminado (tiene estimaciones posteriores): C�digo={codigoConcepto}");
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        erroresConceptos++;
                        System.Diagnostics.Debug.WriteLine($"? Error al revertir concepto {codigoConcepto}: {ex.Message}");
                    }
                }

                // ? Si hubo errores en conceptos, retornar falso
                if (erroresConceptos > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"? REVERSI�N DE CONCEPTOS FALLIDA: {erroresConceptos} errores encontrados");
                    return false;
                }

                // ? Log final exitoso
                System.Diagnostics.Debug.WriteLine(
                    $"? REVERSI�N COMPLETADA EXITOSAMENTE para estimaci�n {estimacion.Folio}:\n" +
                    $"   - M�todo: {(usarVinculoPermanente ? "V�NCULO PERMANENTE (100% seguro)" : "LEGACY (WBS)")}\n" +
                    $"   - {partidasRevertidas} partidas revertidas a 0%\n" +
                    $"   - {conceptosRevertidos} conceptos eliminados del avance");
                
                return true; // ? Reversi�n exitosa
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error cr�tico al revertir avances: {ex.Message}\n{ex.StackTrace}");
                return false; // ? Error cr�tico, no se debe eliminar
            }
        }
    }

    public class EstimacionInfo
    {
        public int Id { get; set; }
        public string Folio { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string Proveedor { get; set; }
        public string Descripcion { get; set; }
        public double TotalEstimacion { get; set; }
        public int NumeroEstimacion { get; set; }
        public string Observaciones { get; set; }
        public int TotalPartidas { get; set; }
        public bool TienePDF { get; set; }
    }

    public class DetalleEstimacion
    {
        public string CodigoConcepto { get; set; }
        public string NombreConcepto { get; set; }
        public int WBS { get; set; }
        public string NombrePartida { get; set; }
        public double MontoPresupuestado { get; set; }
        public double MontoEjecutado { get; set; }
        public double AvancePorcentaje { get; set; }
    }
}
