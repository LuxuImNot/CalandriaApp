using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
                        
                        // Renumerar conceptos si es necesario (inserción en medio)
                        if (posicionInsercion >= 0 && posicionInsercion < conceptosExistentes.Count)
                        {
                            RenumerarConceptosPosteriores(nuevoCodigo);
                        }
                        
                        // Guardar nuevo concepto en BD
                        GuardarNuevoConceptoEnBD(nuevoCodigo, nombreConcepto, partidas);
                        
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
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine("Conexión a BD abierta correctamente");
                    
                    // Verificar si existe la columna Codigo
                    bool tieneCodigoColumn = false;
                    using (SqlCommand cmdCheck = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'Estimacion(Concepto)' 
                        AND COLUMN_NAME = 'Codigo'", conn))
                    {
                        tieneCodigoColumn = (int)cmdCheck.ExecuteScalar() > 0;
                        System.Diagnostics.Debug.WriteLine($"Tabla tiene columna 'Codigo': {tieneCodigoColumn}");
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
                                FROM [dbo].[Estimacion(Concepto)]
                                WHERE Codigo IS NOT NULL AND Concepto IS NOT NULL
                            ) AS Conceptos
                            ORDER BY CodigoNumerico, Codigo";
                    }
                    else
                    {
                        // Si NO existe Codigo, generar uno según orden estándar
                        sql = @"
                            WITH ConceptosOrdenados AS (
                                SELECT DISTINCT
                                    Concepto,
                                    CASE Concepto
                                        WHEN 'Preliminares' THEN 1
                                        WHEN 'Cimentación' THEN 2
                                        WHEN 'Cimentacion' THEN 2
                                        WHEN 'Estructura' THEN 3
                                        WHEN 'Ins. Hidraulica, Sanitaria y Gas LP' THEN 4
                                        WHEN 'Inst. Hidraulica, Sanitaria y Gas LP' THEN 4
                                        WHEN 'Inst. Eléctrica' THEN 5
                                        WHEN 'Inst. Electrica' THEN 5
                                        WHEN 'Albañilería' THEN 6
                                        WHEN 'AlbanILERIA' THEN 6
                                        WHEN 'Albañileria' THEN 6
                                        WHEN 'Acabados' THEN 7
                                        WHEN 'Herrería, Aluminio y Vidrio' THEN 8
                                        WHEN 'Herreria, Aluminio y Vidrio' THEN 8
                                        WHEN 'Carpintería y Cerrajería' THEN 9
                                        WHEN 'Carpinteria y Cerrajeria' THEN 9
                                        WHEN 'Muebles y Accesorios' THEN 10
                                        WHEN 'Inst especiales y Obra Exterior' THEN 11
                                        WHEN 'Urbanización' THEN 12
                                        WHEN 'Urbanizacion' THEN 12
                                        ELSE 999
                                    END AS OrdenConcepto
                                FROM [dbo].[Estimacion(Concepto)]
                                WHERE Concepto IS NOT NULL
                            )
                            SELECT 
                                CAST(OrdenConcepto AS NVARCHAR(10)) AS Codigo,
                                Concepto
                            FROM ConceptosOrdenados
                            ORDER BY OrdenConcepto";
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Ejecutando consulta SQL...");
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            string codigoStr = reader["Codigo"]?.ToString() ?? "0";
                            string nombreConcepto = reader["Concepto"]?.ToString() ?? "";
                            
                            System.Diagnostics.Debug.WriteLine($"  Leído: [{codigoStr}] {nombreConcepto}");
                            
                            if (int.TryParse(codigoStr, out int codigo))
                            {
                                conceptos.Add(new ConceptoExistente
                                {
                                    Codigo = codigo,
                                    Nombre = nombreConcepto
                                });
                                count++;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"  No se pudo parsear código: {codigoStr}");
                            }
                        }
                        System.Diagnostics.Debug.WriteLine($"Total conceptos cargados: {count}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar conceptos existentes: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
                
                // MOSTRAR ERROR AL USUARIO (no silencioso)
                MessageBox.Show(
                    $"Error al cargar conceptos desde la base de datos:\n\n{ex.Message}\n\n" +
                    $"Verifica la conexión y la estructura de la tabla.",
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
        /// Renumera los conceptos posteriores al código especificado (incrementa +1)
        /// </summary>
        private void RenumerarConceptosPosteriores(int codigoDesde)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Verificar si existe la columna Codigo
                    bool tieneCodigoColumn = false;
                    using (SqlCommand cmdCheck = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'Estimacion(Concepto)' 
                        AND COLUMN_NAME = 'Codigo'", conn))
                    {
                        tieneCodigoColumn = (int)cmdCheck.ExecuteScalar() > 0;
                    }
                    
                    if (!tieneCodigoColumn)
                    {
                        // Si no existe la columna, no se puede renumerar
                        return;
                    }
                    
                    // Renumerar en orden DESCENDENTE para evitar conflictos de clave única
                    string sqlEstimacion = @"
                        UPDATE [dbo].[Estimacion(Concepto)]
                        SET Codigo = CAST((TRY_CAST(Codigo AS INT) + 1) AS NVARCHAR(10))
                        WHERE TRY_CAST(Codigo AS INT) >= @codigoDesde
                        AND TRY_CAST(Codigo AS INT) IS NOT NULL";
                    
                    using (SqlCommand cmd = new SqlCommand(sqlEstimacion, conn))
                    {
                        cmd.Parameters.AddWithValue("@codigoDesde", codigoDesde);
                        cmd.ExecuteNonQuery();
                    }
                    
                    // Renumerar también en PresupuestoObra si existe la columna Codigo
                    using (SqlCommand cmdCheckPresupuesto = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'PresupuestoObra' 
                        AND COLUMN_NAME = 'Codigo'", conn))
                    {
                        bool tieneCodigoPresupuesto = (int)cmdCheckPresupuesto.ExecuteScalar() > 0;
                        
                        if (tieneCodigoPresupuesto)
                        {
                            string sqlPresupuesto = @"
                                UPDATE PresupuestoObra
                                SET Codigo = CAST((TRY_CAST(Codigo AS INT) + 1) AS NVARCHAR(10))
                                WHERE TRY_CAST(Codigo AS INT) >= @codigoDesde
                                AND TRY_CAST(Codigo AS INT) IS NOT NULL";
                            
                            using (SqlCommand cmd = new SqlCommand(sqlPresupuesto, conn))
                            {
                                cmd.Parameters.AddWithValue("@codigoDesde", codigoDesde);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al renumerar conceptos: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Guarda el nuevo concepto y sus partidas en la base de datos
        /// </summary>
        private void GuardarNuevoConceptoEnBD(int codigo, string nombreConcepto, List<PartidaConcepto> partidas)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    // Verificar si existe la columna Codigo en ambas tablas
                    bool tieneCodigoEstimacion = false;
                    bool tieneCodigoPresupuesto = false;
                    
                    using (SqlCommand cmdCheck = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'Estimacion(Concepto)' 
                        AND COLUMN_NAME = 'Codigo'", conn))
                    {
                        tieneCodigoEstimacion = (int)cmdCheck.ExecuteScalar() > 0;
                    }
                    
                    using (SqlCommand cmdCheck = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'PresupuestoObra' 
                        AND COLUMN_NAME = 'Codigo'", conn))
                    {
                        tieneCodigoPresupuesto = (int)cmdCheck.ExecuteScalar() > 0;
                    }
                    
                    // Insertar en Estimacion(Concepto)
                    foreach (var partida in partidas)
                    {
                        string sqlEstimacion = "";
                        
                        if (tieneCodigoEstimacion)
                        {
                            sqlEstimacion = @"
                                INSERT INTO [dbo].[Estimacion(Concepto)] 
                                (Codigo, Concepto, Padre, Etapa, Partida, TOTAL, CostoTunera, CostoCalandra)
                                VALUES (@codigo, @concepto, @padre, @etapa, @partida, @total, @costoTunera, @costoCalandra)";
                        }
                        else
                        {
                            sqlEstimacion = @"
                                INSERT INTO [dbo].[Estimacion(Concepto)] 
                                (Concepto, Padre, Etapa, Partida, TOTAL, CostoTunera, CostoCalandra)
                                VALUES (@concepto, @padre, @etapa, @partida, @total, @costoTunera, @costoCalandra)";
                        }
                        
                        using (SqlCommand cmd = new SqlCommand(sqlEstimacion, conn))
                        {
                            if (tieneCodigoEstimacion)
                                cmd.Parameters.AddWithValue("@codigo", codigo.ToString());
                            cmd.Parameters.AddWithValue("@concepto", nombreConcepto);
                            cmd.Parameters.AddWithValue("@padre", nombreConcepto);
                            cmd.Parameters.AddWithValue("@etapa", partida.Etapa);
                            cmd.Parameters.AddWithValue("@partida", partida.Partida);
                            cmd.Parameters.AddWithValue("@total", partida.CostoTunera); // Usar Tunera como TOTAL
                            cmd.Parameters.AddWithValue("@costoTunera", partida.CostoTunera);
                            cmd.Parameters.AddWithValue("@costoCalandra", partida.CostoCalandra);
                            
                            cmd.ExecuteNonQuery();
                        }
                    }
                    
                    // Insertar en PresupuestoObra
                    foreach (var partida in partidas)
                    {
                        string sqlPresupuesto = "";
                        
                        if (tieneCodigoPresupuesto)
                        {
                            sqlPresupuesto = @"
                                INSERT INTO PresupuestoObra 
                                (Codigo, Padre, Etapa, Partida, CostoTunera, CostoCalandra)
                                VALUES (@codigo, @padre, @etapa, @partida, @costoTunera, @costoCalandra)";
                        }
                        else
                        {
                            sqlPresupuesto = @"
                                INSERT INTO PresupuestoObra 
                                (Padre, Etapa, Partida, CostoTunera, CostoCalandra)
                                VALUES (@padre, @etapa, @partida, @costoTunera, @costoCalandra)";
                        }
                        
                        using (SqlCommand cmd = new SqlCommand(sqlPresupuesto, conn))
                        {
                            if (tieneCodigoPresupuesto)
                                cmd.Parameters.AddWithValue("@codigo", codigo.ToString());
                            cmd.Parameters.AddWithValue("@padre", nombreConcepto);
                            cmd.Parameters.AddWithValue("@etapa", partida.Etapa);
                            cmd.Parameters.AddWithValue("@partida", partida.Partida);
                            cmd.Parameters.AddWithValue("@costoTunera", partida.CostoTunera);
                            cmd.Parameters.AddWithValue("@costoCalandra", partida.CostoCalandra);
                            
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar concepto en BD: {ex.Message}", ex);
            }
        }
    }
}
