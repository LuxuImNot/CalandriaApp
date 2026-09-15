using System;
using System.Collections.Generic;
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
                return ApiClient.Get<int>("/api/folios-estimacion/siguiente-numero");
            }
            catch
            {
                return 1; // Por defecto, primera estimacion
            }
        }

        // Generar folio unico con formato global
        private string GenerarFolio(string manzana, string lote, int numeroEstimacion)
        {
            // MODIFICADO: Formato global: EST-{numero:000}-{a�o} (M{manzana}-L{lote})
            return $"EST-{numeroEstimacion:000}-{DateTime.Now:yyyy}";
        }

        // M�TODO: Guardar folio en la base de datos (12 par�metros)
        private int GuardarEstimacionViaApi(string folio, string manzana, string lote, string prototipo,
            int numeroEstimacion, string proveedor, string descripcion,
            double importeContrato, double totalRequisicion, double amortizacion,
            double porcentajeAmortizacion, double totalEstimacion, string rutaPdf)
        {
            try
            {
                var detalle = new List<DetalleFolioApi>();
                foreach (var concepto in nodosRaiz)
                    foreach (var partida in concepto.Partidas.Where(p => p.Incluir))
                        detalle.Add(new DetalleFolioApi
                        {
                            CodigoConcepto = concepto.Codigo,
                            NombreConcepto = concepto.Nombre,
                            Wbs = partida.WBS,
                            NombrePartida = partida.Nombre,
                            MontoPresupuestado = partida.Total,
                            MontoEjecutado = partida.MontoEjecutado,
                            AvancePorcentaje = partida.AvancePorcentaje
                        });

                byte[] pdfBytes = File.ReadAllBytes(rutaPdf);

                var req = new GuardarFolioEstimacionApi
                {
                    Folio = folio,
                    Manzana = manzana,
                    Lote = lote,
                    Prototipo = prototipo,
                    NumeroEstimacion = numeroEstimacion,
                    Proveedor = proveedor,
                    Descripcion = descripcion,
                    ImporteContrato = importeContrato,
                    TotalRequisicion = totalRequisicion,
                    Amortizacion = amortizacion,
                    PorcentajeAmortizacion = porcentajeAmortizacion,
                    TotalEstimacion = totalEstimacion,
                    Usuario = Environment.UserName,
                    Detalle = detalle,
                    NombreArchivoPdf = Path.GetFileName(rutaPdf),
                    PdfBase64 = Convert.ToBase64String(pdfBytes)
                };

                var resp = ApiClient.Post<GuardarFolioEstimacionRespApi>("/api/folios-estimacion", req);
                return resp != null ? resp.FolioId : -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar estimacion: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        // Abrir repositorio de PDFs
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
                ApiClient.Post("/api/folios-estimacion/ensure-tablas", new { });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al crear tablas: {ex.Message}");
            }
        }

        // Marcar las partidas seleccionadas (y sus conceptos) al 100% via API
        private void ActualizarAvancesA100PorCiento()
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null) return;

            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();

            try
            {
                var partidas = new List<PartidaCompletadaApi>();
                var conceptos = new List<ConceptoCompletadoApi>();

                foreach (var concepto in nodosRaiz)
                {
                    var partidasSelec = concepto.Partidas.Where(p => p.Incluir).ToList();
                    if (partidasSelec.Count == 0)
                        continue;

                    foreach (var partida in partidasSelec)
                    {
                        partida.AvancePorcentaje = 100.0;
                        partida.MontoEjecutado = partida.Total;
                        partida.Completado = true;
                        partida.FechaFinalizacion = DateTime.Now;

                        double metrosCuadrados = partida.EsDinamica ? partida.MetrosCuadrados : 0.0;
                        partidas.Add(new PartidaCompletadaApi
                        {
                            Wbs = partida.WBS,
                            Monto = partida.Total,
                            MetrosCuadrados = metrosCuadrados
                        });
                    }

                    var partidasCompletadas = concepto.Partidas.Count(p => p.Completado);
                    var totalPartidas = concepto.Partidas.Count;

                    if (partidasCompletadas == totalPartidas && totalPartidas > 0)
                    {
                        int codigoConcepto = -1;
                        if (!string.IsNullOrEmpty(concepto.Codigo) && int.TryParse(concepto.Codigo, out int codigo))
                            codigoConcepto = -codigo;

                        double totalConcepto = concepto.Partidas.Sum(p => p.MontoEjecutado);
                        conceptos.Add(new ConceptoCompletadoApi
                        {
                            Wbs = codigoConcepto,
                            Nombre = concepto.Nombre,
                            Monto = totalConcepto
                        });
                    }
                }

                if (partidas.Count == 0 && conceptos.Count == 0)
                    return;

                ApiClient.Post("/api/avances/marcar-completadas", new MarcarCompletadasApi
                {
                    Manzana = manzana,
                    Lote = lote,
                    Prototipo = string.IsNullOrEmpty(prototipoActual) ? null : prototipoActual,
                    FechaFinalizacion = DateTime.Now,
                    Partidas = partidas,
                    Conceptos = conceptos
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar avances: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Generar PDF de avance
        private void GenerarPDFAvance(string rutaArchivo)
        {
            try
            {
                PdfDocument documento = new PdfDocument();
                documento.Info.Title = $"Estimaci�n de Conceptos - M{cmbManzana.SelectedItem} L{cmbLote.SelectedItem}";
                documento.Info.Author = "Sistema Pilaris";

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
