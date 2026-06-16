using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Drawing;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Extensi�n parcial de FormEstimacionConceptoMigrado para manejo de folios y PDFs
    /// </summary>
    public partial class FormEstimacionConceptoMigrado
    {
        // M�TODO: Obtener siguiente n�mero de estimaci�n GLOBAL (para todo el proyecto)
        private int ObtenerSiguienteNumeroEstimacion(string manzana, string lote)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // MODIFICADO: Obtener el n�mero global de todas las estimaciones (sin filtrar por casa)
                    string sql = @"
                        IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FoliosEstimacion')
                        BEGIN
                            SELECT ISNULL(MAX(NumeroEstimacion), 0) + 1 
                            FROM FoliosEstimacion;
                        END
                        ELSE
                        BEGIN
                            SELECT 1;
                        END";
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        // Ya no se usan par�metros de manzana y lote para el conteo
                        var result = cmd.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch
            {
                return 1; // Por defecto, primera estimaci�n
            }
        }

        // M�TODO: Generar folio �nico con formato global
        private string GenerarFolio(string manzana, string lote, int numeroEstimacion)
        {
            // MODIFICADO: Formato global: EST-{numero:000}-{a�o} (M{manzana}-L{lote})
            return $"EST-{numeroEstimacion:000}-{DateTime.Now:yyyy}";
        }

        // M�TODO: Guardar folio en la base de datos (12 par�metros)
        private int GuardarFolioEstimacion(string folio, string manzana, string lote, string prototipo,
            int numeroEstimacion, string proveedor, string descripcion, 
            double importeContrato, double totalRequisicion, double amortizacion, 
            double porcentajeAmortizacion, double totalEstimacion)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"
                        INSERT INTO FoliosEstimacion 
                        (Folio, Manzana, Lote, Prototipo, FechaGeneracion, Proveedor, Descripcion, 
                         ImporteContrato, TotalRequisicion, Amortizacion, PorcentajeAmortizacion, 
                         TotalEstimacion, NumeroEstimacion, Usuario)
                        VALUES 
                        (@folio, @manzana, @lote, @prototipo, GETDATE(), @proveedor, @descripcion,
                         @importeContrato, @totalRequisicion, @amortizacion, @porcentajeAmortizacion,
                         @totalEstimacion, @numeroEstimacion, @usuario);
                        SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@folio", folio);
                        cmd.Parameters.AddWithValue("@manzana", manzana);
                        cmd.Parameters.AddWithValue("@lote", lote);
                        cmd.Parameters.AddWithValue("@prototipo", prototipo ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@proveedor", proveedor ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@descripcion", descripcion ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@importeContrato", importeContrato);
                        cmd.Parameters.AddWithValue("@totalRequisicion", totalRequisicion);
                        cmd.Parameters.AddWithValue("@amortizacion", amortizacion);
                        cmd.Parameters.AddWithValue("@porcentajeAmortizacion", porcentajeAmortizacion);
                        cmd.Parameters.AddWithValue("@totalEstimacion", totalEstimacion);
                        cmd.Parameters.AddWithValue("@numeroEstimacion", numeroEstimacion);
                        cmd.Parameters.AddWithValue("@usuario", Environment.UserName);

                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar folio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        // M�TODO: Guardar detalle del folio (conceptos/partidas)
        private void GuardarDetalleFolio(int folioId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ? VERIFICAR si existe columna IdPresupuestoObra
                    bool tieneIdPresupuestoObra = false;
                    using (SqlCommand cmdCheck = new SqlCommand(@"
                        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'FoliosEstimacionDetalle' 
                        AND COLUMN_NAME = 'IdPresupuestoObra'", conn))
                    {
                        tieneIdPresupuestoObra = (int)cmdCheck.ExecuteScalar() > 0;
                    }

                    System.Diagnostics.Debug.WriteLine($"?? GuardarDetalleFolio: IdPresupuestoObra disponible = {tieneIdPresupuestoObra}");

                    foreach (var concepto in nodosRaiz)
                    {
                        var partidasSelec = concepto.Partidas.Where(p => p.Incluir).ToList();
                        
                        foreach (var partida in partidasSelec)
                        {
                            // ? OBTENER IdPresupuestoObra si est� disponible
                            int? idPresupuestoObra = null;
                            
                            if (tieneIdPresupuestoObra && partida.WBS > 0)
                            {
                                // Buscar el Id de PresupuestoObra usando el WBS actual
                                string sqlGetId = "SELECT Id FROM PresupuestoObra WHERE WBS_Correcto = @wbs";
                                using (SqlCommand cmdGetId = new SqlCommand(sqlGetId, conn))
                                {
                                    cmdGetId.Parameters.AddWithValue("@wbs", partida.WBS);
                                    var result = cmdGetId.ExecuteScalar();
                                    if (result != null && result != DBNull.Value)
                                    {
                                        idPresupuestoObra = Convert.ToInt32(result);
                                        System.Diagnostics.Debug.WriteLine(
                                            $"   ?? Partida WBS={partida.WBS} vinculada a IdPresupuestoObra={idPresupuestoObra}");
                                    }
                                }
                            }

                            // Construir SQL din�micamente seg�n disponibilidad de columna
                            string sql;
                            if (tieneIdPresupuestoObra)
                            {
                                sql = @"
                                    INSERT INTO FoliosEstimacionDetalle 
                                    (FolioId, CodigoConcepto, NombreConcepto, WBS, IdPresupuestoObra, NombrePartida, 
                                     MontoPresupuestado, MontoEjecutado, AvancePorcentaje)
                                    VALUES 
                                    (@folioId, @codigoConcepto, @nombreConcepto, @wbs, @idPresupuestoObra, @nombrePartida,
                                     @montoPresupuestado, @montoEjecutado, @avancePorcentaje)";
                            }
                            else
                            {
                                // Versi�n legacy sin IdPresupuestoObra
                                sql = @"
                                    INSERT INTO FoliosEstimacionDetalle 
                                    (FolioId, CodigoConcepto, NombreConcepto, WBS, NombrePartida, 
                                     MontoPresupuestado, MontoEjecutado, AvancePorcentaje)
                                    VALUES 
                                    (@folioId, @codigoConcepto, @nombreConcepto, @wbs, @nombrePartida,
                                     @montoPresupuestado, @montoEjecutado, @avancePorcentaje)";
                            }

                            using (SqlCommand cmd = new SqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@folioId", folioId);
                                cmd.Parameters.AddWithValue("@codigoConcepto", concepto.Codigo ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@nombreConcepto", concepto.Nombre ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@wbs", partida.WBS);
                                
                                if (tieneIdPresupuestoObra)
                                {
                                    cmd.Parameters.AddWithValue("@idPresupuestoObra", 
                                        idPresupuestoObra.HasValue ? (object)idPresupuestoObra.Value : DBNull.Value);
                                }
                                
                                cmd.Parameters.AddWithValue("@nombrePartida", partida.Nombre ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@montoPresupuestado", partida.Total);
                                cmd.Parameters.AddWithValue("@montoEjecutado", partida.MontoEjecutado);
                                cmd.Parameters.AddWithValue("@avancePorcentaje", partida.AvancePorcentaje);
                                
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    System.Diagnostics.Debug.WriteLine(
                        $"? Detalle de folio guardado {(tieneIdPresupuestoObra ? "CON v�nculo permanente" : "SIN v�nculo permanente (legacy)")}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar detalle: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"? Error en GuardarDetalleFolio: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // M�TODO: Guardar PDF en la base de datos
        private void GuardarPDFEnBD(int folioId, string folio, string manzana, string lote, string rutaPdf)
        {
            try
            {
                byte[] pdfBytes = File.ReadAllBytes(rutaPdf);
                string nombreArchivo = Path.GetFileName(rutaPdf);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"
                        INSERT INTO PDFsEstimacion 
                        (FolioId, Folio, Manzana, Lote, NombreArchivo, ContenidoPDF, TamanioBytes, FechaAlmacenamiento)
                        VALUES 
                        (@folioId, @folio, @manzana, @lote, @nombreArchivo, @contenidoPDF, @tamanio, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@folioId", folioId);
                        cmd.Parameters.AddWithValue("@folio", folio);
                        cmd.Parameters.AddWithValue("@manzana", manzana);
                        cmd.Parameters.AddWithValue("@lote", lote);
                        cmd.Parameters.AddWithValue("@nombreArchivo", nombreArchivo);
                        cmd.Parameters.AddWithValue("@contenidoPDF", pdfBytes);
                        cmd.Parameters.AddWithValue("@tamanio", pdfBytes.Length);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar PDF en BD: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // M�TODO: Abrir repositorio de PDFs
        private void btnRepositorioPDFs_Click(object sender, EventArgs e)
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
            {
                MessageBox.Show("Selecciona Manzana y Lote para ver el repositorio", "Atenci�n", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();

            try
            {
                // Verificar que las tablas existan
                CrearTablasSiNoExisten();

                using (var formRepo = new FormRepositorioPDFs(manzana, lote))
                {
                    formRepo.ShowDialog(this);
                    
                    // ?? NUEVO: Recargar datos del TreeListView despu�s de cerrar el repositorio
                    // Esto asegura que si se elimin� una estimaci�n, los avances se reflejen correctamente
                    CargarEstimacionJerarquica(manzana, lote);
                    
                    MessageBox.Show(
                        "Los datos se han actualizado correctamente.\n\n" +
                        "Si eliminaste estimaciones, los avances se han revertido autom�ticamente.",
                        "Actualizaci�n Completa", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir repositorio: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // M�TODO: Crear tablas de folios si no existen
        private void CrearTablasSiNoExisten()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Crear tabla FoliosEstimacion
                    string sqlFolios = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FoliosEstimacion')
                        BEGIN
                            CREATE TABLE FoliosEstimacion (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                Folio NVARCHAR(50) NOT NULL UNIQUE,
                                Manzana NVARCHAR(10) NOT NULL,
                                Lote NVARCHAR(10) NOT NULL,
                                Prototipo NVARCHAR(50),
                                FechaGeneracion DATETIME DEFAULT GETDATE(),
                                Proveedor NVARCHAR(200),
                                Descripcion NVARCHAR(500),
                                ImporteContrato FLOAT,
                                TotalRequisicion FLOAT,
                                Amortizacion FLOAT,
                                PorcentajeAmortizacion FLOAT,
                                TotalEstimacion FLOAT,
                                NumeroEstimacion INT,
                                Usuario NVARCHAR(100),
                                Observaciones NVARCHAR(MAX)
                            );
                        END";
                    
                    using (SqlCommand cmd = new SqlCommand(sqlFolios, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Crear tabla FoliosEstimacionDetalle
                    string sqlDetalle = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FoliosEstimacionDetalle')
                        BEGIN
                            CREATE TABLE FoliosEstimacionDetalle (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                FolioId INT NOT NULL,
                                CodigoConcepto NVARCHAR(10),
                                NombreConcepto NVARCHAR(200),
                                WBS INT,
                                NombrePartida NVARCHAR(500),
                                MontoPresupuestado FLOAT,
                                MontoEjecutado FLOAT,
                                AvancePorcentaje FLOAT,
                                FOREIGN KEY (FolioId) REFERENCES FoliosEstimacion(Id) ON DELETE CASCADE
                            );
                        END";

                    using (SqlCommand cmd = new SqlCommand(sqlDetalle, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Crear tabla PDFsEstimacion
                    string sqlPDFs = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PDFsEstimacion')
                        BEGIN
                            CREATE TABLE PDFsEstimacion (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                FolioId INT NOT NULL,
                                Folio NVARCHAR(50) NOT NULL,
                                Manzana NVARCHAR(10) NOT NULL,
                                Lote NVARCHAR(10) NOT NULL,
                                NombreArchivo NVARCHAR(255),
                                ContenidoPDF VARBINARY(MAX) NOT NULL,
                                TamanioBytes BIGINT,
                                FechaAlmacenamiento DATETIME DEFAULT GETDATE(),
                                FOREIGN KEY (FolioId) REFERENCES FoliosEstimacion(Id) ON DELETE CASCADE
                            );
                        END";

                    using (SqlCommand cmd = new SqlCommand(sqlPDFs, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al crear tablas: {ex.Message}");
            }
        }

        // M�TODO: Actualizar avances a 100% para las partidas seleccionadas
        private void ActualizarAvancesA100PorCiento()
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null) return;

            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (var concepto in nodosRaiz)
                    {
                        var partidasSelec = concepto.Partidas.Where(p => p.Incluir).ToList();

                        if (partidasSelec.Count == 0)
                            continue;

                        // 1?? Actualizar cada partida incluida al 100%
                        foreach (var partida in partidasSelec)
                        {
                            // Actualizar a 100% y marcar como completado
                            partida.AvancePorcentaje = 100.0;
                            partida.MontoEjecutado = partida.Total;
                            partida.Completado = true;
                            partida.FechaFinalizacion = DateTime.Now;

                            // ?? NUEVO: Preparar m� si es din�mica
                            double metrosCuadrados = partida.EsDinamica ? partida.MetrosCuadrados : 0.0;
                            
                            // ?? CORREGIDO: Agregar columna MetrosCuadrados al INSERT/UPDATE
                            // Intentar con FechaFinalizacion y MetrosCuadrados primero
                            string sql = @"
                                IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                                    UPDATE AvanceManualObra 
                                    SET AvancePorcentaje=100.0, 
                                        MontoEjecutado=@monto, 
                                        FechaActualizacion=GETDATE(),
                                        FechaFinalizacion=GETDATE(),
                                        MetrosCuadrados=@m2
                                    WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                                ELSE
                                    INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, AvancePorcentaje, MontoEjecutado, FechaFinalizacion, MetrosCuadrados)
                                    VALUES (@m, @l, @proto, @wbs, 100.0, @monto, GETDATE(), @m2)";

                            using (SqlCommand cmd = new SqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@m", manzana);
                                cmd.Parameters.AddWithValue("@l", lote);
                                cmd.Parameters.AddWithValue("@proto", prototipoActual ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@wbs", partida.WBS.ToString());
                                cmd.Parameters.AddWithValue("@monto", partida.Total);
                                cmd.Parameters.AddWithValue("@m2", metrosCuadrados);

                                try
                                {
                                    cmd.ExecuteNonQuery();
                                    
                                    // ?? DEBUG: Log para partidas din�micas
                                    if (partida.EsDinamica && metrosCuadrados > 0)
                                    {
                                        System.Diagnostics.Debug.WriteLine(
                                            $"? Partida din�mica guardada: WBS={partida.WBS}, " +
                                            $"m�={metrosCuadrados:F2}, Monto={partida.Total:C2}");
                                    }
                                }
                                catch (SqlException ex) when (ex.Message.Contains("FechaFinalizacion") || ex.Message.Contains("MetrosCuadrados"))
                                {
                                    // Si falla por FechaFinalizacion o MetrosCuadrados, intentar versi�n fallback
                                    string sqlSinColumnas = @"
                                        IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                                            UPDATE AvanceManualObra 
                                            SET AvancePorcentaje=100.0, 
                                                MontoEjecutado=@monto, 
                                                FechaActualizacion=GETDATE()
                                            WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                                        ELSE
                                            INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, AvancePorcentaje, MontoEjecutado)
                                            VALUES (@m, @l, @proto, @wbs, 100.0, @monto)";

                                    using (SqlCommand cmd2 = new SqlCommand(sqlSinColumnas, conn))
                                    {
                                        cmd2.Parameters.AddWithValue("@m", manzana);
                                        cmd2.Parameters.AddWithValue("@l", lote);
                                        cmd2.Parameters.AddWithValue("@proto", prototipoActual ?? (object)DBNull.Value);
                                        cmd2.Parameters.AddWithValue("@wbs", partida.WBS.ToString());
                                        cmd2.Parameters.AddWithValue("@monto", partida.Total);
                                        cmd2.ExecuteNonQuery();
                                        
                                        System.Diagnostics.Debug.WriteLine(
                                            $"?? Guardado sin MetrosCuadrados (columna no existe): WBS={partida.WBS}");
                                    }
                                }
                            }
                        }

                        // 2?? NUEVO: Verificar si TODAS las partidas del concepto est�n completadas
                        // Si es as�, guardar el concepto con WBS negativo para que PanelPrincipal lo detecte
                        var partidasCompletadas = concepto.Partidas.Where(p => p.Completado).Count();
                        var totalPartidas = concepto.Partidas.Count;

                        if (partidasCompletadas == totalPartidas && totalPartidas > 0)
                        {
                            // TODAS las partidas del concepto est�n al 100%
                            // Guardar concepto con WBS negativo
                            int codigoConcepto = -1;
                            if (!string.IsNullOrEmpty(concepto.Codigo) && int.TryParse(concepto.Codigo, out int codigo))
                            {
                                codigoConcepto = -codigo; // WBS negativo para distinguir conceptos
                            }

                            double totalConcepto = concepto.Partidas.Sum(p => p.MontoEjecutado);

                            string sqlConcepto = @"
                                IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                                    UPDATE AvanceManualObra 
                                    SET AvancePorcentaje=100.0, 
                                        MontoEjecutado=@monto, 
                                        FechaActualizacion=GETDATE(),
                                        FechaFinalizacion=GETDATE(),
                                        Concepto=@concepto
                                    WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                                ELSE
                                    INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, Concepto, AvancePorcentaje, MontoEjecutado, FechaFinalizacion)
                                    VALUES (@m, @l, @proto, @wbs, @concepto, 100.0, @monto, GETDATE())";

                            using (SqlCommand cmdConcepto = new SqlCommand(sqlConcepto, conn))
                            {
                                cmdConcepto.Parameters.AddWithValue("@m", manzana);
                                cmdConcepto.Parameters.AddWithValue("@l", lote);
                                cmdConcepto.Parameters.AddWithValue("@proto", prototipoActual ?? (object)DBNull.Value);
                                cmdConcepto.Parameters.AddWithValue("@wbs", codigoConcepto.ToString());
                                cmdConcepto.Parameters.AddWithValue("@concepto", concepto.Nombre ?? (object)DBNull.Value);
                                cmdConcepto.Parameters.AddWithValue("@monto", totalConcepto);

                                try
                                {
                                    cmdConcepto.ExecuteNonQuery();
                                    System.Diagnostics.Debug.WriteLine($"? Concepto guardado con WBS negativo: {codigoConcepto} ({concepto.Nombre})");
                                }
                                catch (SqlException ex) when (ex.Message.Contains("FechaFinalizacion"))
                                {
                                    // Si falla por FechaFinalizacion, intentar sin esa columna
                                    string sqlSinFecha = @"
                                        IF EXISTS (SELECT 1 FROM AvanceManualObra WHERE Manzana=@m AND Lote=@l AND WBS=@wbs)
                                            UPDATE AvanceManualObra 
                                            SET AvancePorcentaje=100.0, 
                                                MontoEjecutado=@monto, 
                                                FechaActualizacion=GETDATE(),
                                                Concepto=@concepto
                                            WHERE Manzana=@m AND Lote=@l AND WBS=@wbs
                                        ELSE
                                            INSERT INTO AvanceManualObra (Manzana, Lote, Prototipo, WBS, Concepto, AvancePorcentaje, MontoEjecutado)
                                            VALUES (@m, @l, @proto, @wbs, @concepto, 100.0, @monto)";

                                    using (SqlCommand cmd2 = new SqlCommand(sqlSinFecha, conn))
                                    {
                                        cmd2.Parameters.AddWithValue("@m", manzana);
                                        cmd2.Parameters.AddWithValue("@l", lote);
                                        cmd2.Parameters.AddWithValue("@proto", prototipoActual ?? (object)DBNull.Value);
                                        cmd2.Parameters.AddWithValue("@wbs", codigoConcepto.ToString());
                                        cmd2.Parameters.AddWithValue("@concepto", concepto.Nombre ?? (object)DBNull.Value);
                                        cmd2.Parameters.AddWithValue("@monto", totalConcepto);
                                        cmd2.ExecuteNonQuery();
                                        System.Diagnostics.Debug.WriteLine($"? Concepto guardado sin FechaFin: {codigoConcepto} ({concepto.Nombre})");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar avances: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // M�TODO: Generar PDF de avance
        private void GenerarPDFAvance(string rutaArchivo)
        {
            try
            {
                PdfDocument documento = new PdfDocument();
                documento.Info.Title = $"Estimaci�n de Conceptos - M{cmbManzana.SelectedItem} L{cmbLote.SelectedItem}";
                documento.Info.Author = "Sistema Calandria";

                PdfPage pagina = documento.AddPage();
                pagina.Size = PdfSharp.PageSize.Letter;
                XGraphics gfx = XGraphics.FromPdfPage(pagina);

                // Dibujar contenido del PDF
                DibujarContenidoPDF(gfx, pagina, false);

                // NUEVO: Agregar evidencias fotogr�ficas si hay seleccionadas
                if (evidenciasSeleccionadas != null && evidenciasSeleccionadas.Count > 0)
                {
                    DibujarEvidenciasPDF(documento);
                }

                // NUEVO: Agregar fotos adjuntas a los conceptos/partidas (hoja aparte,
                // con subt�tulo que indica a qu� concepto pertenece cada foto).
                DibujarFotosConceptoPDF(documento);

                // Guardar el documento
                documento.Save(rutaArchivo);
                documento.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // M�TODO: Dibujar contenido del PDF (FORMATO MEJORADO CON FOLIO, FECHA, ANTICIPO)
        private void DibujarContenidoPDF(XGraphics gfx, PdfPage page, bool soloPreview)
        {
            // Redirigir al nuevo formato mejorado seg�n imagen de referencia
            DibujarPDFNuevoFormato(gfx, page, soloPreview);
        }
    }
}
