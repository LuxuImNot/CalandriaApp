using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Clase parcial que contiene el manejo de avances, totales y guardado en BD
    /// </summary>
    public partial class FormEstimacionConceptoMigrado
    {
        private NodoConcepto EncontrarPadre(NodoConcepto hijo)
        {
            foreach (var raiz in nodosRaiz)
            {
                if (raiz.Partidas.Contains(hijo))
                    return raiz;
            }
            return null;
        }

        private void RecalcularAvanceConcepto(NodoConcepto concepto)
        {
            if (!concepto.EsConcepto || concepto.Partidas.Count == 0)
                return;

            // Recalcular totales considerando partidas dinámicas
            double totalPartidas = concepto.Partidas.Sum(p => ObtenerTotalPartida(p));
            double ejecutadoPartidas = concepto.Partidas.Sum(p => p.MontoEjecutado);

            concepto.Total = totalPartidas;
            concepto.MontoEjecutado = ejecutadoPartidas;
            concepto.AvancePorcentaje = totalPartidas > 0 
                ? (ejecutadoPartidas / totalPartidas) * 100.0 
                : 0;
                
            bool todasCompletadas = concepto.Partidas.All(p => p.Completado);
            
            if (todasCompletadas && !concepto.Completado)
            {
                concepto.FechaFinalizacion = DateTime.Now;
                concepto.Completado = true;
            }
            else if (!todasCompletadas && concepto.Completado)
            {
                concepto.FechaFinalizacion = null;
                concepto.Completado = false;
            }
            else
            {
                concepto.Completado = todasCompletadas;
            }
        }

        private void ActualizarTotales()
        {
            // Recalcular los totales de todos los conceptos primero
            foreach (var concepto in nodosRaiz)
            {
                RecalcularAvanceConcepto(concepto);
            }

            double totalPresupuestado = nodosRaiz.Sum(c => c.Total);
            double totalEjecutado = nodosRaiz.Sum(c => c.MontoEjecutado);
            double porEjecutar = totalPresupuestado - totalEjecutado;
            double avanceGeneral = totalPresupuestado > 0 ? (totalEjecutado / totalPresupuestado) * 100 : 0;

            lblTotalPresupuestado.Text = $"Total Presupuestado: {totalPresupuestado:C2}";
            lblTotalEjecutado.Text = $"Total Ejecutado: {totalEjecutado:C2}";
            lblPorEjecutar.Text = $"Por Ejecutar: {porEjecutar:C2}";
            lblAvanceGeneral.Text = $"Avance General: {avanceGeneral:F1}%";

            try { progressBarAvance.Value = Math.Min(100, (int)avanceGeneral); } catch { }

            int completadas = nodosRaiz.Count(i => i.AvancePorcentaje >= 100);
            int enProgreso = nodosRaiz.Count(i => i.AvancePorcentaje > 0 && i.AvancePorcentaje < 100);
            int sinIniciar = nodosRaiz.Count(i => i.AvancePorcentaje == 0);

            lblEstadisticas.Text = $"Completados: {completadas} | En Progreso: {enProgreso} | Sin Iniciar: {sinIniciar}";

            if (avanceGeneral < 30)
                lblAvanceGeneral.ForeColor = Color.FromArgb(231, 76, 60);
            else if (avanceGeneral < 70)
                lblAvanceGeneral.ForeColor = Color.FromArgb(243, 156, 18);
            else
                lblAvanceGeneral.ForeColor = Color.FromArgb(46, 204, 113);
        }

        private void GuardarAvancePartidaEnBD(NodoConcepto partida)
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null) return;

            try
            {
                double montoEjecutado = partida.MontoEjecutado;
                if (partida.EsDinamica && partida.MetrosCuadrados > 0)
                {
                    double valorM2 = ObtenerValorM2(partida);
                    montoEjecutado = partida.MetrosCuadrados * valorM2;
                }

                var req = new GuardarAvancePartidaEstimacionApi
                {
                    Manzana = cmbManzana.SelectedItem.ToString(),
                    Lote = cmbLote.SelectedItem.ToString(),
                    Prototipo = string.IsNullOrEmpty(prototipoActual) ? null : prototipoActual,
                    Wbs = partida.WBS,
                    AvancePorcentaje = partida.AvancePorcentaje,
                    MontoEjecutado = montoEjecutado,
                    MetrosCuadrados = partida.MetrosCuadrados,
                    FechaFinalizacion = partida.FechaFinalizacion
                };

                ApiClient.Post("/api/avances/partida-estimacion", req);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar avance de partida: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Handlers de botones
        private void btnCargarAvance_Click(object sender, EventArgs e)
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
            {
                MessageBox.Show("Selecciona Manzana y Lote primero", "Atención", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string manzana = cmbManzana.SelectedItem.ToString();
            string lote = cmbLote.SelectedItem.ToString();

            try
            {
                var proto = ApiClient.Get<string>(
                    $"/api/avances/prototipo?manzana={Uri.EscapeDataString(manzana)}&lote={Uri.EscapeDataString(lote)}");
                prototipoActual = proto?.Trim() ?? "";

                // Si no hay prototipo, asignar uno por defecto
                if (string.IsNullOrEmpty(prototipoActual))
                {
                    prototipoActual = "TUNERA";
                    System.Diagnostics.Debug.WriteLine($"No se encontró prototipo para M{manzana}-L{lote}, usando '{prototipoActual}' por defecto");
                }

                System.Diagnostics.Debug.WriteLine($"Cargando avance para M{manzana}-L{lote}, Prototipo: {prototipoActual}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener prototipo: {ex.Message}");
                prototipoActual = "TUNERA";
            }
            
            CargarEstimacionJerarquica(manzana, lote);
            
            // Mostrar información del resultado
            int totalConceptos = nodosRaiz.Count;
            int totalPartidas = nodosRaiz.Sum(c => c.Partidas.Count);
            
            MessageBox.Show(
                $"Avance cargado exitosamente\n\n" +
                $"Prototipo: {prototipoActual}\n" +
                $"Conceptos: {totalConceptos}\n" +
                $"Partidas: {totalPartidas}", 
                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnGuardarYExportar_Click(object sender, EventArgs e)
        {
            if (cmbManzana.SelectedItem == null || cmbLote.SelectedItem == null)
            {
                MessageBox.Show("Selecciona Manzana y Lote primero", "Atención", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int partidasSeleccionadas = nodosRaiz.Sum(c => c.Partidas.Count(p => p.Incluir));
            if (partidasSeleccionadas == 0)
            {
                MessageBox.Show("Marca al menos una partida para generar la estimación", "Atención", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string manzana = cmbManzana.SelectedItem.ToString();
                string lote = cmbLote.SelectedItem.ToString();
                
                ActualizarAvancesA100PorCiento();

                int numEstimacion = ObtenerSiguienteNumeroEstimacion(manzana, lote);
                string folio = GenerarFolio(manzana, lote, numEstimacion);

                double totalEjecutado = nodosRaiz.Where(c => c.Partidas.Any(p => p.Incluir))
                    .SelectMany(c => c.Partidas.Where(p => p.Incluir))
                    .Sum(p => ObtenerTotalPartida(p));

                double porcentajeAmortizacion = !string.IsNullOrEmpty(prototipoActual) && prototipoActual.ToUpper().Contains("TUNERA") ? 0.11 : 0.095;
                double amortizacion = totalEjecutado * porcentajeAmortizacion;
                double totalEstimacion = totalEjecutado - amortizacion;

                // El PDF se genera antes del POST: la estimación (cabecera + detalle + PDF)
                // se guarda en una sola transacción en el servidor.
                string rutaPdf = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{folio}.pdf");
                GenerarPDFAvance(rutaPdf);

                int folioId = GuardarEstimacionViaApi(folio, manzana, lote, prototipoActual, numEstimacion,
                    txtProveedor.Text, txtDescripcion.Text, nodosRaiz.Sum(i => i.Total),
                    totalEjecutado, amortizacion, porcentajeAmortizacion, totalEstimacion, rutaPdf);

                if (folioId > 0)
                {
                    MessageBox.Show($"Estimación generada exitosamente\n\nFolio: {folio}\n\nLas partidas se han marcado como completadas.",
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // NUEVO: Preguntar si desea abrir el PDF
                    var resultadoAbrir = MessageBox.Show(
                        "¿Deseas abrir el PDF generado?", 
                        "Abrir PDF", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Question);

                    if (resultadoAbrir == DialogResult.Yes)
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(rutaPdf) { UseShellExecute = true });
                        }
                        catch (Exception exAbrir)
                        {
                            MessageBox.Show($"No se pudo abrir el PDF: {exAbrir.Message}", 
                                "Error al abrir", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }

                    CargarEstimacionJerarquica(manzana, lote);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar estimación: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnMarcarTodos_Click(object sender, EventArgs e)
        {
            foreach (var concepto in nodosRaiz)
            {
                if (!concepto.Completado)
                {
                    concepto.Incluir = true;
                    foreach (var partida in concepto.Partidas)
                    {
                        if (!partida.Completado)
                        {
                            partida.Incluir = true;
                        }
                    }
                }
            }

            olvEstimacionConceptos.RebuildAll(true);
            OnDatosChanged(this, EventArgs.Empty);
        }

        private void btnDesmarcarTodos_Click(object sender, EventArgs e)
        {
            foreach (var concepto in nodosRaiz)
            {
                concepto.Incluir = false;
                foreach (var partida in concepto.Partidas)
                {
                    partida.Incluir = false;
                }
            }

            olvEstimacionConceptos.RebuildAll(true);
            OnDatosChanged(this, EventArgs.Empty);
        }

        private void dgvEvidencias_SelectionChanged(object sender, EventArgs e)
        {
            // Placeholder para compatibilidad
        }
    }
}
