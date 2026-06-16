using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    // Fila plana que representa un nodo del WBS en el grid
    public class WbsFlatRow
    {
        public PanelPrincipal.TareaRutaCritica Node;
        public int Level;
        public bool Expanded;
        public bool HasChildren;
        public string WBS => Node.WBS;
        public string Concepto => Node.Nombre;
        public string Inicio => Node.Inicio.ToString("dd/MM/yyyy");
        public string Fin => Node.Fin.ToString("dd/MM/yyyy");
        public int Progreso => Node.AvancePorcentaje;
        public string Estado => Node.GetType().GetProperty("EstadoAssignment") != null
            ? Convert.ToString(Node.GetType().GetProperty("EstadoAssignment").GetValue(Node, null))
            : "";
        public string Cuadrilla => Node.GetType().GetProperty("CuadrillaAsignada") != null
            ? Convert.ToString(Node.GetType().GetProperty("CuadrillaAsignada").GetValue(Node, null))
            : "";
    }

    public static class WbsTreeBuilder
    {
        public static List<WbsFlatRow> BuildTopLevel(IEnumerable<PanelPrincipal.TareaRutaCritica> roots)
        {
            var list = new List<WbsFlatRow>();
            foreach (var r in roots)
            {
                list.Add(new WbsFlatRow
                {
                    Node = r,
                    Level = 0,
                    Expanded = false,
                    HasChildren = r.Hijos != null && r.Hijos.Count > 0
                });
            }
            return list;
        }

        public static List<WbsFlatRow> BuildChildren(PanelPrincipal.TareaRutaCritica parent, int level)
        {
            var list = new List<WbsFlatRow>();
            if (parent.Hijos == null) return list;
            foreach (var h in parent.Hijos)
            {
                list.Add(new WbsFlatRow
                {
                    Node = h,
                    Level = level,
                    Expanded = false,
                    HasChildren = h.Hijos != null && h.Hijos.Count > 0
                });
            }
            return list;
        }
    }

    public class TreeGridController
    {
        private readonly DataGridView _grid;
        private readonly BindingList<WbsFlatRow> _rows = new BindingList<WbsFlatRow>();

        public event Action<PanelPrincipal.TareaRutaCritica> OnSelectedNodeChanged;

        public TreeGridController(DataGridView grid)
        {
            _grid = grid;
            _grid.AutoGenerateColumns = false;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.RowHeadersVisible = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.RowTemplate.Height = 24;

            // ==== Columnas ====
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Concepto", HeaderText = "Concepto", DataPropertyName = "Concepto", Width = 260 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "WBS", HeaderText = "WBS", DataPropertyName = "WBS", Width = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Inicio", HeaderText = "Inicio", DataPropertyName = "Inicio", Width = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fin", HeaderText = "Fin", DataPropertyName = "Fin", Width = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Progreso", HeaderText = "Progreso", DataPropertyName = "Progreso", Width = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Estado", HeaderText = "Estado", DataPropertyName = "Estado", Width = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cuadrilla", HeaderText = "Cuadrilla", DataPropertyName = "Cuadrilla", Width = 140 });

            _grid.DataSource = _rows;

            // Eventos visuales e interacción
            _grid.CellPainting += Grid_CellPainting;          // indent + toggle + progreso
            _grid.CellMouseClick += Grid_CellMouseClick;      // expand/collapse
            _grid.SelectionChanged += Grid_SelectionChanged;  // detalle
            _grid.CellToolTipTextNeeded += Grid_CellToolTipTextNeeded;
        }

        public void LoadRoots(IEnumerable<PanelPrincipal.TareaRutaCritica> roots)
        {
            _rows.Clear();
            foreach (var r in WbsTreeBuilder.BuildTopLevel(roots))
                _rows.Add(r);
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            var fr = GetCurrent();
            OnSelectedNodeChanged?.Invoke(fr?.Node);
        }

        private WbsFlatRow GetCurrent()
        {
            if (_grid.CurrentRow == null) return null;
            return _grid.CurrentRow.DataBoundItem as WbsFlatRow;
        }

        private void Grid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != _grid.Columns["Concepto"].Index) return;
            var fr = _grid.Rows[e.RowIndex].DataBoundItem as WbsFlatRow;
            if (fr == null) return;

            // Área del toggle [+] [-]
            var cellRect = _grid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
            var toggleRect = new Rectangle(cellRect.X + 4 + fr.Level * 16, cellRect.Y + (cellRect.Height - 12) / 2, 12, 12);
            if (toggleRect.Contains(e.Location) && fr.HasChildren)
            {
                if (fr.Expanded) Collapse(e.RowIndex);
                else Expand(e.RowIndex);
            }
        }

        private void Expand(int rowIndex)
        {
            var fr = _rows[rowIndex];
            if (!fr.HasChildren || fr.Expanded) return;

            var children = WbsTreeBuilder.BuildChildren(fr.Node, fr.Level + 1);
            int insertPos = rowIndex + 1;
            foreach (var c in children)
                _rows.Insert(insertPos++, c);

            fr.Expanded = true;
            _grid.InvalidateRow(rowIndex);
        }

        private void Collapse(int rowIndex)
        {
            var fr = _rows[rowIndex];
            if (!fr.Expanded) return;
            fr.Expanded = false;

            // Remueve todos los descendientes contiguos
            int i = rowIndex + 1;
            while (i < _rows.Count && _rows[i].Level > fr.Level) _rows.RemoveAt(i);

            _grid.InvalidateRow(rowIndex);
        }

        private void Grid_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var fr = _grid.Rows[e.RowIndex].DataBoundItem as WbsFlatRow;
            if (fr == null) return;

            e.ToolTipText =
                $"WBS: {fr.WBS}\r\n" +
                $"Concepto: {fr.Concepto}\r\n" +
                $"Inicio: {fr.Inicio}  |  Fin: {fr.Fin}\r\n" +
                $"Progreso: {fr.Progreso}%\r\n" +
                (!string.IsNullOrEmpty(fr.Estado) ? $"Estado: {fr.Estado}\r\n" : "") +
                (!string.IsNullOrEmpty(fr.Cuadrilla) ? $"Cuadrilla: {fr.Cuadrilla}\r\n" : "");
        }

        private void Grid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var fr = _grid.Rows[e.RowIndex].DataBoundItem as WbsFlatRow;
            if (fr == null) return;

            // Barra de progreso en la columna Progreso
            if (_grid.Columns[e.ColumnIndex].Name == "Progreso")
            {
                e.PaintBackground(e.CellBounds, true);
                int pct = Math.Max(0, Math.Min(100, Convert.ToInt32(e.Value)));
                var rect = new Rectangle(e.CellBounds.X + 2, e.CellBounds.Y + 6,
                                         (e.CellBounds.Width - 4) * pct / 100,
                                         e.CellBounds.Height - 12);
                using (var br = new SolidBrush(Color.SteelBlue)) e.Graphics.FillRectangle(br, rect);
                TextRenderer.DrawText(e.Graphics, pct + "%", _grid.Font, e.CellBounds, Color.Black,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                e.Handled = true;
                return;
            }

            // Indentación + toggle en la columna Concepto
            if (_grid.Columns[e.ColumnIndex].Name == "Concepto")
            {
                e.PaintBackground(e.CellBounds, true);
                // ident
                int indent = fr.Level * 16;
                // toggle
                if (fr.HasChildren)
                {
                    var tg = new Rectangle(e.CellBounds.X + 4 + indent, e.CellBounds.Y + (e.CellBounds.Height - 12) / 2, 12, 12);
                    using (var p = new Pen(Color.DimGray))
                    using (var b = new SolidBrush(Color.White))
                    {
                        e.Graphics.FillRectangle(b, tg);
                        e.Graphics.DrawRectangle(p, tg);
                        // plus/minus
                        e.Graphics.DrawLine(p, tg.X + 3, tg.Y + tg.Height / 2, tg.Right - 3, tg.Y + tg.Height / 2);
                        if (!fr.Expanded)
                            e.Graphics.DrawLine(p, tg.X + tg.Width / 2, tg.Y + 3, tg.X + tg.Width / 2, tg.Bottom - 3);
                    }
                }
                // texto
                var textRect = new Rectangle(e.CellBounds.X + 24 + indent, e.CellBounds.Y + 4,
                                             e.CellBounds.Width - (24 + indent), e.CellBounds.Height - 8);
                TextRenderer.DrawText(e.Graphics, Convert.ToString(e.FormattedValue), _grid.Font, textRect, Color.Black,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                e.Handled = true;
                return;
            }
        }
    }
}
