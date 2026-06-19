using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Clase parcial que contiene la funcionalidad de "Agregar Concepto" y "Gestionar Partidas"
    /// </summary>
    public partial class FormEstimacionConceptoMigrado
    {
        /// <summary>
        /// Handler del botón "Agregar Concepto"
        /// </summary>
        private void btnAgregarConcepto_Click(object sender, EventArgs e)
        {
            try
            {
                // Cargar conceptos existentes desde la BD
                var conceptosExistentes = CargarConceptosExistentesParaSelector();
                
                // DEPURACIÓN: Mostrar cuántos conceptos se cargaron
                System.Diagnostics.Debug.WriteLine($"Conceptos cargados: {conceptosExistentes.Count}");
                foreach (var c in conceptosExistentes)
                {
                    System.Diagnostics.Debug.WriteLine($"  - [{c.Codigo}] {c.Nombre}");
                }
                
                // Si no hay conceptos, informar al usuario
                if (conceptosExistentes.Count == 0)
                {
                    var result = MessageBox.Show(
                        "No se encontraron conceptos existentes en la base de datos.\n\n" +
                        "El nuevo concepto se agregará como el primero.\n\n" +
                        "¿Deseas continuar?",
                        "Sin Conceptos Existentes", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Warning);
                    
                    if (result != DialogResult.Yes)
                        return;
                }
                
                // Abrir formulario modal
                using (var formAgregar = new FormAgregarConcepto(conceptosExistentes, prototipoActual))
                {
                    if (formAgregar.ShowDialog() == DialogResult.OK)
                    {
                        // Obtener datos del formulario
                        string nombreConcepto = formAgregar.NombreConcepto;
                        int posicionInsercion = formAgregar.PosicionInsercion;
                        var partidas = formAgregar.Partidas;
                        
                        // Calcular el código del nuevo concepto
                        int nuevoCodigo = CalcularNuevoCodigoConcepto(posicionInsercion, conceptosExistentes);

                        // Si la inserción es en medio, el servidor renumera los conceptos posteriores.
                        bool renumerar = posicionInsercion >= 0 && posicionInsercion < conceptosExistentes.Count;

                        // Guardar nuevo concepto (renumeración + inserción en una transacción)
                        GuardarNuevoConceptoEnBD(nuevoCodigo, nombreConcepto, partidas, renumerar);
                        
                        MessageBox.Show(
                            $"Concepto agregado exitosamente\n\n" +
                            $"Código: {nuevoCodigo}\n" +
                            $"Nombre: {nombreConcepto}\n" +
                            $"Partidas: {partidas.Count}\n\n" +
                            $"Recarga el avance para ver los cambios.",
                            "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Recargar datos si hay manzana y lote seleccionados
                        if (cmbManzana.SelectedItem != null && cmbLote.SelectedItem != null)
                        {
                            string manzana = cmbManzana.SelectedItem.ToString();
                            string lote = cmbLote.SelectedItem.ToString();
                            CargarEstimacionJerarquica(manzana, lote);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar concepto:\n\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Handler del botón "Gestionar Partidas" - NUEVO
        /// Permite agregar/editar/eliminar partidas de conceptos existentes
        /// </summary>
        private void btnGestionarPartidas_Click(object sender, EventArgs e)
        {
            try
            {
                using (var formGestionar = new FormGestionarPartidas(connectionString))
                {
                    if (formGestionar.ShowDialog() == DialogResult.OK)
                    {
                        // Recargar datos si hay manzana y lote seleccionados
                        if (cmbManzana.SelectedItem != null && cmbLote.SelectedItem != null)
                        {
                            string manzana = cmbManzana.SelectedItem.ToString();
                            string lote = cmbLote.SelectedItem.ToString();
                            CargarEstimacionJerarquica(manzana, lote);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al gestionar partidas:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Carga los conceptos existentes desde la BD para el selector de posición
        /// </summary>
        private List<ConceptoExistente> CargarConceptosExistentesParaSelector()
        {
            var conceptos = new List<ConceptoExistente>();

            try
            {
                var api = ApiClient.Get<List<ConceptoExistenteApi>>("/api/avances/conceptos-selector");
                if (api != null)
                    foreach (var c in api)
                        conceptos.Add(new ConceptoExistente { Codigo = c.Codigo, Nombre = c.Nombre });

                System.Diagnostics.Debug.WriteLine($"Total conceptos cargados: {conceptos.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar conceptos existentes: {ex.Message}");

                // MOSTRAR ERROR AL USUARIO (no silencioso)
                MessageBox.Show(
                    $"Error al cargar conceptos desde la base de datos:\n\n{ex.Message}",
                    "Error de Base de Datos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return conceptos;
        }
        
        /// <summary>
        /// Calcula el código del nuevo concepto según la posición de inserción
        /// </summary>
        private int CalcularNuevoCodigoConcepto(int posicionInsercion, List<ConceptoExistente> conceptosExistentes)
        {
            if (posicionInsercion < 0 || posicionInsercion >= conceptosExistentes.Count)
            {
                // Insertar al final
                if (conceptosExistentes.Count > 0)
                {
                    return conceptosExistentes.Max(c => c.Codigo) + 1;
                }
                return 1;
            }
            else
            {
                // Insertar antes del concepto seleccionado
                return conceptosExistentes[posicionInsercion].Codigo;
            }
        }
        
        /// <summary>
        /// Guarda el nuevo concepto y sus partidas vía API. El servidor renumera los
        /// conceptos posteriores (si <paramref name="renumerar"/>) e inserta las partidas
        /// en Estimacion(Concepto) y PresupuestoObra dentro de una transacción.
        /// </summary>
        private void GuardarNuevoConceptoEnBD(int codigo, string nombreConcepto, List<PartidaConcepto> partidas, bool renumerar)
        {
            try
            {
                var req = new ConceptoNuevoApi
                {
                    Codigo = codigo,
                    Nombre = nombreConcepto,
                    Renumerar = renumerar,
                    Partidas = partidas.Select(p => new PartidaNuevaApi
                    {
                        Etapa = p.Etapa,
                        Partida = p.Partida,
                        CostoTunera = p.CostoTunera,
                        CostoCalandra = p.CostoCalandra
                    }).ToList()
                };

                ApiClient.Post("/api/avances/concepto-nuevo", req);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar concepto en BD: {ex.Message}", ex);
            }
        }
    }
}
