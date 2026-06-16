using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Formulario para gestionar (agregar/editar/eliminar) partidas de conceptos existentes
    /// Archivo principal - Configuraci�n e inicializaci�n
    /// </summary>
    public partial class FormGestionarPartidas : Form
    {
        #region Campos privados
        
        private string connectionString;
        private List<ConceptoExistente> conceptosDisponibles;
        private ConceptoExistente conceptoSeleccionado;
        private BindingList<PartidaConcepto> partidasConcepto;
        
        #endregion
        
        #region Constructor
        
        public FormGestionarPartidas(string connString)
        {
            InitializeComponent();
            ThemeManager.AplicarTema(this);
            connectionString = connString;
            partidasConcepto = new BindingList<PartidaConcepto>();
            
            ConfigurarFormulario();
            CargarConceptos();
        }
        
        #endregion
        
        #region Configuraci�n del formulario
        
        private void ConfigurarFormulario()
        {
            // Configuraci�n de DataGridView
            dgvPartidas.AutoGenerateColumns = false;
            dgvPartidas.AllowUserToAddRows = false;
            dgvPartidas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPartidas.MultiSelect = false;
            dgvPartidas.RowHeadersVisible = false;
            dgvPartidas.BackgroundColor = Color.White;
            dgvPartidas.BorderStyle = BorderStyle.None;
            
            // Columnas
            dgvPartidas.Columns.Clear();
            
            var colEtapa = new DataGridViewTextBoxColumn 
            { 
                Name = "Etapa", 
                HeaderText = "Etapa", 
                Width = 100,
                DataPropertyName = "Etapa"
            };
            
            var colPartida = new DataGridViewTextBoxColumn 
            { 
                Name = "Partida", 
                HeaderText = "Partida", 
                Width = 180,
                DataPropertyName = "Partida"
            };
            
            // Columna de tipo de costo
            var colTipoCosto = new DataGridViewTextBoxColumn 
            { 
                Name = "TipoCosto", 
                HeaderText = "Tipo", 
                Width = 120,
                DataPropertyName = "TipoCosto",
                ReadOnly = true
            };
            
            var colCostoTunera = new DataGridViewTextBoxColumn 
            { 
                Name = "CostoTunera", 
                HeaderText = "Costo Tunera", 
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" },
                DataPropertyName = "CostoTunera"
            };
            
            var colCostoCalandra = new DataGridViewTextBoxColumn 
            { 
                Name = "CostoCalandra", 
                HeaderText = "Costo Calandra", 
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" },
                DataPropertyName = "CostoCalandra"
            };
            
            // Columna de L�mite M2 Tunera
            var colLimiteM2Tunera = new DataGridViewTextBoxColumn 
            { 
                Name = "LimiteM2Tunera", 
                HeaderText = "L�m. m\u00B2 Tunera", 
                Width = 95,
                DataPropertyName = "LimiteEfectivoTunera",
                ReadOnly = true
            };
            
            // Columna de L�mite M2 Calandra
            var colLimiteM2Calandra = new DataGridViewTextBoxColumn 
            { 
                Name = "LimiteM2Calandra", 
                HeaderText = "L�m. m\u00B2 Calandra", 
                Width = 95,
                DataPropertyName = "LimiteEfectivoCalandra",
                ReadOnly = true
            };
            
            // Columna de Prototipos Aplicables
            var colPrototipos = new DataGridViewTextBoxColumn 
            { 
                Name = "Prototipos", 
                HeaderText = "Prototipos", 
                Width = 100,
                DataPropertyName = "PrototiposDescripcion",
                ReadOnly = true
            };
            
            dgvPartidas.Columns.AddRange(new DataGridViewColumn[] 
            { 
                colEtapa, colPartida, colTipoCosto, colCostoTunera, colCostoCalandra, 
                colLimiteM2Tunera, colLimiteM2Calandra, colPrototipos 
            });
            
            // Estilo alternado
            dgvPartidas.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
            
            // Evento para formatear las celdas de l�mite m�
            dgvPartidas.CellFormatting += DgvPartidas_CellFormatting;
            
            // Establecer el DataSource una sola vez
            dgvPartidas.DataSource = partidasConcepto;
        }
        
        /// <summary>
        /// Formatea las celdas de l�mite m� para mostrar valores legibles
        /// </summary>
        private void DgvPartidas_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= partidasConcepto.Count)
                return;
            
            var partida = partidasConcepto[e.RowIndex];
            string columnName = dgvPartidas.Columns[e.ColumnIndex].Name;
            
            // Formatear columnas de l�mite m�
            if (columnName == "LimiteM2Tunera" || columnName == "LimiteM2Calandra")
            {
                // Si no es partida din�mica, mostrar "-"
                if (!partida.EsDinamica)
                {
                    e.Value = "-";
                    e.FormattingApplied = true;
                    return;
                }
                
                // Obtener el valor del l�mite seg�n la columna
                double limite = columnName == "LimiteM2Tunera" 
                    ? partida.LimiteEfectivoTunera 
                    : partida.LimiteEfectivoCalandra;
                
                // Si el l�mite es 0, mostrar "Sin l�mite"
                if (limite <= 0)
                {
                    e.Value = "Sin l\u00EDmite";
                    e.FormattingApplied = true;
                }
                else
                {
                    e.Value = limite.ToString("N2");
                    e.FormattingApplied = true;
                }
            }
        }
        
        #endregion
        
        #region Carga de conceptos
        
        private void CargarConceptos()
        {
            conceptosDisponibles = new List<ConceptoExistente>();
            cmbConceptos.Items.Clear();
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Verificar si existe columna Codigo
                    bool tieneCodigoColumn = false;
                    using (SqlCommand cmdCheck = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'Estimacion(Concepto)' 
                        AND COLUMN_NAME = 'Codigo'", conn))
                    {
                        tieneCodigoColumn = (int)cmdCheck.ExecuteScalar() > 0;
                    }
                    
                    string sql;
                    if (tieneCodigoColumn)
                    {
                        // CORREGIDO: Usar subquery para evitar conflicto DISTINCT + ORDER BY
                        sql = @"
                            SELECT Codigo, Concepto
                            FROM (
                                SELECT DISTINCT
                                    Codigo,
                                    Concepto,
                                    CASE 
                                        WHEN TRY_CAST(Codigo AS INT) IS NOT NULL 
                                        THEN TRY_CAST(Codigo AS INT)
                                        ELSE 999999 
                                    END AS CodigoNumerico
                                FROM [Estimacion(Concepto)]
                                WHERE Codigo IS NOT NULL AND Concepto IS NOT NULL
                            ) AS Conceptos
                            ORDER BY CodigoNumerico, Codigo";
                    }
                    else
                    {
                        // Generar c�digos seg�n orden est�ndar
                        sql = @"
                            WITH ConceptosOrdenados AS (
                                SELECT DISTINCT
                                    Concepto,
                                    CASE Concepto
                                        WHEN 'Preliminares' THEN 1
                                        WHEN 'Cimentaci�n' THEN 2
                                        WHEN 'Cimentacion' THEN 2
                                        WHEN 'Estructura' THEN 3
                                        WHEN 'Ins. Hidraulica, Sanitaria y Gas LP' THEN 4
                                        WHEN 'Inst. El�ctrica' THEN 5
                                        WHEN 'Alba�iler�a' THEN 6
                                        WHEN 'Acabados' THEN 7
                                        WHEN 'Herrer�a, Aluminio y Vidrio' THEN 8
                                        WHEN 'Carpinter�a y Cerrajer�a' THEN 9
                                        WHEN 'Muebles y Accesorios' THEN 10
                                        WHEN 'Inst especiales y Obra Exterior' THEN 11
                                        WHEN 'Urbanizaci�n' THEN 12
                                        ELSE 999
                                    END AS OrdenConcepto
                                FROM [Estimacion(Concepto)]
                            )
                            SELECT 
                                CAST(OrdenConcepto AS NVARCHAR(10)) AS Codigo,
                                Concepto
                            FROM ConceptosOrdenados
                            ORDER BY OrdenConcepto";
                    }
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var concepto = new ConceptoExistente
                            {
                                Codigo = int.TryParse(reader["Codigo"].ToString(), out int codigo) ? codigo : 0,
                                Nombre = reader["Concepto"].ToString()
                            };
                            
                            conceptosDisponibles.Add(concepto);
                            cmbConceptos.Items.Add($"[{concepto.Codigo}] {concepto.Nombre}");
                        }
                    }
                }
                
                if (cmbConceptos.Items.Count > 0)
                {
                    cmbConceptos.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar conceptos:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
        #endregion
        
        #region Evento de selecci�n de concepto
        
        private void cmbConceptos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbConceptos.SelectedIndex < 0 || cmbConceptos.SelectedIndex >= conceptosDisponibles.Count)
                return;
            
            conceptoSeleccionado = conceptosDisponibles[cmbConceptos.SelectedIndex];
            CargarPartidasConcepto();
        }
        
        #endregion
        
        #region Bot�n Cancelar
        
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "�Deseas cancelar sin guardar los cambios?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }
        
        #endregion
    }
}
