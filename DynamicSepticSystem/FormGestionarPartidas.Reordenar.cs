using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// FormGestionarPartidas - Funcionalidad para reordenar partidas existentes
    /// </summary>
    public partial class FormGestionarPartidas
    {
        #region Campos de control de reordenamiento
        
        /// <summary>
        /// Indica si se han reordenado partidas (para forzar reorganización WBS al guardar)
        /// </summary>
        private bool partidasReordenadas = false;
        
        #endregion
        
        #region Métodos de reordenamiento
        
        /// <summary>
        /// Mueve la partida seleccionada hacia arriba en la lista
        /// </summary>
        private void btnSubir_Click(object sender, EventArgs e)
        {
            MoverPartida(-1); // Mover hacia arriba (índice menor)
        }
        
        /// <summary>
        /// Mueve la partida seleccionada hacia abajo en la lista
        /// </summary>
        private void btnBajar_Click(object sender, EventArgs e)
        {
            MoverPartida(1); // Mover hacia abajo (índice mayor)
        }
        
        /// <summary>
        /// Mueve la partida seleccionada en la dirección indicada
        /// </summary>
        /// <param name="direccion">-1 para subir, 1 para bajar</param>
        private void MoverPartida(int direccion)
        {
            if (dgvPartidas.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Selecciona una partida para mover.",
                    "Atención",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            int indiceActual = dgvPartidas.SelectedRows[0].Index;
            int nuevoIndice = indiceActual + direccion;
            
            // Validar límites
            if (nuevoIndice < 0 || nuevoIndice >= partidasConcepto.Count)
            {
                // Ya está en el límite, no hacer nada
                return;
            }
            
            // Realizar el intercambio
            IntercambiarPartidas(indiceActual, nuevoIndice);
            
            // Mantener la selección en la partida movida
            dgvPartidas.ClearSelection();
            dgvPartidas.Rows[nuevoIndice].Selected = true;
            dgvPartidas.CurrentCell = dgvPartidas.Rows[nuevoIndice].Cells[0];
            
            // Asegurar que la fila sea visible
            dgvPartidas.FirstDisplayedScrollingRowIndex = Math.Max(0, nuevoIndice - 3);
            
            // Marcar que se han reordenado partidas
            partidasReordenadas = true;
            
            Debug.WriteLine($"? Partida movida: índice {indiceActual} -> {nuevoIndice}");
        }
        
        /// <summary>
        /// Intercambia dos partidas en la lista y actualiza sus WBS temporalmente
        /// </summary>
        private void IntercambiarPartidas(int indice1, int indice2)
        {
            // Obtener las partidas
            var partida1 = partidasConcepto[indice1];
            var partida2 = partidasConcepto[indice2];
            
            // Guardar WBS originales
            int wbs1 = partida1.WBS;
            int wbs2 = partida2.WBS;
            
            // Realizar el intercambio en la lista usando RaiseListChangedEvents
            // para evitar múltiples refrescos del DataGridView
            bool raiseEvents = partidasConcepto.RaiseListChangedEvents;
            partidasConcepto.RaiseListChangedEvents = false;
            
            try
            {
                // Remover y reinsertar para mantener el orden
                partidasConcepto.RemoveAt(indice1);
                
                // Ajustar índice si es necesario
                int insertIndex = indice2;
                if (indice1 < indice2)
                    insertIndex = indice2 - 1;
                
                // Insertar en la nueva posición
                if (indice1 < indice2)
                {
                    // Moviendo hacia abajo
                    partidasConcepto.Insert(indice2, partida1);
                }
                else
                {
                    // Moviendo hacia arriba
                    partidasConcepto.Insert(indice2, partida1);
                }
            }
            finally
            {
                partidasConcepto.RaiseListChangedEvents = raiseEvents;
            }
            
            // Refrescar el DataGridView
            partidasConcepto.ResetBindings();
            
            Debug.WriteLine($"   Intercambiadas: '{partida1.Partida}' (WBS:{wbs1}) <-> '{partida2.Partida}' (WBS:{wbs2})");
        }
        
        /// <summary>
        /// Mueve una partida a una posición específica en la lista
        /// </summary>
        /// <param name="indiceOrigen">Índice actual de la partida</param>
        /// <param name="indiceDestino">Índice destino deseado</param>
        private void MoverPartidaAPosicion(int indiceOrigen, int indiceDestino)
        {
            if (indiceOrigen == indiceDestino)
                return;
            
            if (indiceOrigen < 0 || indiceOrigen >= partidasConcepto.Count)
                return;
            
            if (indiceDestino < 0 || indiceDestino >= partidasConcepto.Count)
                return;
            
            var partida = partidasConcepto[indiceOrigen];
            
            bool raiseEvents = partidasConcepto.RaiseListChangedEvents;
            partidasConcepto.RaiseListChangedEvents = false;
            
            try
            {
                partidasConcepto.RemoveAt(indiceOrigen);
                partidasConcepto.Insert(indiceDestino, partida);
            }
            finally
            {
                partidasConcepto.RaiseListChangedEvents = raiseEvents;
            }
            
            partidasConcepto.ResetBindings();
            partidasReordenadas = true;
            
            Debug.WriteLine($"? Partida '{partida.Partida}' movida de {indiceOrigen} a {indiceDestino}");
        }
        
        /// <summary>
        /// Mueve la partida seleccionada al inicio de la lista
        /// </summary>
        private void MoverPartidaAlInicio()
        {
            if (dgvPartidas.SelectedRows.Count == 0)
                return;
            
            int indiceActual = dgvPartidas.SelectedRows[0].Index;
            if (indiceActual == 0)
                return;
            
            MoverPartidaAPosicion(indiceActual, 0);
            
            dgvPartidas.ClearSelection();
            dgvPartidas.Rows[0].Selected = true;
            dgvPartidas.CurrentCell = dgvPartidas.Rows[0].Cells[0];
        }
        
        /// <summary>
        /// Mueve la partida seleccionada al final de la lista
        /// </summary>
        private void MoverPartidaAlFinal()
        {
            if (dgvPartidas.SelectedRows.Count == 0)
                return;
            
            int indiceActual = dgvPartidas.SelectedRows[0].Index;
            int ultimoIndice = partidasConcepto.Count - 1;
            
            if (indiceActual == ultimoIndice)
                return;
            
            MoverPartidaAPosicion(indiceActual, ultimoIndice);
            
            dgvPartidas.ClearSelection();
            dgvPartidas.Rows[ultimoIndice].Selected = true;
            dgvPartidas.CurrentCell = dgvPartidas.Rows[ultimoIndice].Cells[0];
            dgvPartidas.FirstDisplayedScrollingRowIndex = Math.Max(0, ultimoIndice - 5);
        }
        
        /// <summary>
        /// Ordena las partidas alfabéticamente por etapa y luego por nombre de partida
        /// </summary>
        private void OrdenarPartidasAlfabeticamente()
        {
            if (partidasConcepto.Count <= 1)
                return;
            
            var result = MessageBox.Show(
                "¿Deseas ordenar todas las partidas alfabéticamente?\n\n" +
                "• Se ordenarán primero por Etapa\n" +
                "• Luego por nombre de Partida\n\n" +
                "Esta acción reorganizará el WBS al guardar.",
                "Ordenar Partidas",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            
            if (result != DialogResult.Yes)
                return;
            
            var partidasOrdenadas = partidasConcepto
                .OrderBy(p => p.Etapa)
                .ThenBy(p => p.Partida)
                .ToList();
            
            bool raiseEvents = partidasConcepto.RaiseListChangedEvents;
            partidasConcepto.RaiseListChangedEvents = false;
            
            try
            {
                partidasConcepto.Clear();
                foreach (var partida in partidasOrdenadas)
                {
                    partidasConcepto.Add(partida);
                }
            }
            finally
            {
                partidasConcepto.RaiseListChangedEvents = raiseEvents;
            }
            
            partidasConcepto.ResetBindings();
            partidasReordenadas = true;
            
            MessageBox.Show(
                $"? {partidasConcepto.Count} partidas ordenadas alfabéticamente.\n\n" +
                "Al guardar se reorganizará el WBS automáticamente.",
                "Ordenamiento Completado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        
        /// <summary>
        /// Verifica si hay partidas reordenadas que requieran reorganización de WBS
        /// </summary>
        /// <returns>True si se necesita reorganizar WBS</returns>
        private bool RequiereReorganizacionPorReordenamiento()
        {
            return partidasReordenadas;
        }
        
        /// <summary>
        /// Restablece el indicador de reordenamiento
        /// </summary>
        private void ResetearIndicadorReordenamiento()
        {
            partidasReordenadas = false;
        }
        
        #endregion
        
        #region Soporte para Drag & Drop (opcional, para implementación futura)
        
        /// <summary>
        /// Habilita el drag & drop para reordenar partidas con el mouse
        /// </summary>
        private void HabilitarDragDrop()
        {
            dgvPartidas.AllowDrop = true;
            dgvPartidas.MouseDown += DgvPartidas_MouseDown_DragDrop;
            dgvPartidas.MouseMove += DgvPartidas_MouseMove_DragDrop;
            dgvPartidas.DragOver += DgvPartidas_DragOver;
            dgvPartidas.DragDrop += DgvPartidas_DragDrop;
        }
        
        private int indiceFilaArrastre = -1;
        private Rectangle rectanguloArrastre;
        
        private void DgvPartidas_MouseDown_DragDrop(object sender, MouseEventArgs e)
        {
            var hitTest = dgvPartidas.HitTest(e.X, e.Y);
            indiceFilaArrastre = hitTest.RowIndex;
            
            if (indiceFilaArrastre >= 0)
            {
                Size tamañoArrastre = SystemInformation.DragSize;
                rectanguloArrastre = new Rectangle(
                    new Point(e.X - tamañoArrastre.Width / 2, e.Y - tamañoArrastre.Height / 2),
                    tamañoArrastre);
            }
            else
            {
                rectanguloArrastre = Rectangle.Empty;
            }
        }
        
        private void DgvPartidas_MouseMove_DragDrop(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                if (rectanguloArrastre != Rectangle.Empty &&
                    !rectanguloArrastre.Contains(e.X, e.Y))
                {
                    if (indiceFilaArrastre >= 0 && indiceFilaArrastre < partidasConcepto.Count)
                    {
                        dgvPartidas.DoDragDrop(
                            partidasConcepto[indiceFilaArrastre],
                            DragDropEffects.Move);
                    }
                }
            }
        }
        
        private void DgvPartidas_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        
        private void DgvPartidas_DragDrop(object sender, DragEventArgs e)
        {
            Point puntoCliente = dgvPartidas.PointToClient(new Point(e.X, e.Y));
            int indiceDestino = dgvPartidas.HitTest(puntoCliente.X, puntoCliente.Y).RowIndex;
            
            if (indiceDestino >= 0 && indiceDestino != indiceFilaArrastre)
            {
                MoverPartidaAPosicion(indiceFilaArrastre, indiceDestino);
                
                dgvPartidas.ClearSelection();
                dgvPartidas.Rows[indiceDestino].Selected = true;
            }
        }
        
        #endregion
    }
}
