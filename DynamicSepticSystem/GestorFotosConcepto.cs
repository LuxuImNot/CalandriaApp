using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Gestiona las fotos adjuntas a un concepto o partida de una casa específica
    /// (Manzana + Lote + Identificador del nodo).
    ///
    /// La tabla FotosConcepto se crea automáticamente la primera vez que se usa,
    /// por lo que no requiere migración manual de base de datos.
    /// </summary>
    public class GestorFotosConcepto
    {
        private readonly string connectionString;
        private bool tablaVerificada = false;

        public GestorFotosConcepto()
        {
            connectionString = ConfigurationManager.ConnectionStrings["CalandriaConn"].ConnectionString;
        }

        public GestorFotosConcepto(string connString)
        {
            connectionString = connString;
        }

        /// <summary>
        /// Crea la tabla FotosConcepto si aún no existe. Idempotente.
        /// </summary>
        public void AsegurarTabla()
        {
            if (tablaVerificada)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FotosConcepto')
                    BEGIN
                        CREATE TABLE FotosConcepto (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Manzana NVARCHAR(50) NOT NULL,
                            Lote NVARCHAR(50) NOT NULL,
                            Identificador NVARCHAR(100) NOT NULL,
                            EsConcepto BIT NOT NULL DEFAULT(1),
                            NombreNodo NVARCHAR(255) NULL,
                            Descripcion NVARCHAR(255) NULL,
                            Foto VARBINARY(MAX) NOT NULL,
                            Extension NVARCHAR(10) NULL,
                            TamanioKB FLOAT NULL,
                            FechaCaptura DATETIME NOT NULL DEFAULT(GETDATE()),
                            UsuarioCaptura NVARCHAR(100) NULL
                        );
                        CREATE INDEX IX_FotosConcepto_Nodo
                            ON FotosConcepto (Manzana, Lote, Identificador, EsConcepto);
                    END";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            tablaVerificada = true;
        }

        /// <summary>
        /// Guarda una foto asociada a un concepto/partida de una casa.
        /// </summary>
        public int GuardarFoto(string manzana, string lote, string identificador, bool esConcepto,
            string nombreNodo, string descripcion, byte[] foto, string extension, string usuario = "Sistema")
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote))
                throw new ArgumentException("Manzana y Lote son obligatorios");

            if (string.IsNullOrWhiteSpace(identificador))
                throw new ArgumentException("El identificador del concepto es obligatorio");

            if (foto == null || foto.Length == 0)
                throw new ArgumentException("La foto no puede estar vacía");

            if (foto.Length > 10 * 1024 * 1024) // 10 MB
                throw new ArgumentException("La foto no puede superar los 10 MB");

            AsegurarTabla();

            double tamanioKB = foto.Length / 1024.0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                    INSERT INTO FotosConcepto
                        (Manzana, Lote, Identificador, EsConcepto, NombreNodo, Descripcion, Foto, Extension, TamanioKB, UsuarioCaptura)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@Manzana, @Lote, @Identificador, @EsConcepto, @NombreNodo, @Descripcion, @Foto, @Extension, @TamanioKB, @Usuario)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Manzana", manzana);
                    cmd.Parameters.AddWithValue("@Lote", lote);
                    cmd.Parameters.AddWithValue("@Identificador", identificador);
                    cmd.Parameters.AddWithValue("@EsConcepto", esConcepto);
                    cmd.Parameters.AddWithValue("@NombreNodo", (object)nombreNodo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", (object)descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Foto", foto);
                    cmd.Parameters.AddWithValue("@Extension", extension ?? "jpg");
                    cmd.Parameters.AddWithValue("@TamanioKB", tamanioKB);
                    cmd.Parameters.AddWithValue("@Usuario", usuario ?? "Sistema");

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Lista las fotos (sin el binario) asociadas a un concepto/partida.
        /// </summary>
        public List<FotoConceptoInfo> ListarFotos(string manzana, string lote, string identificador, bool esConcepto)
        {
            var lista = new List<FotoConceptoInfo>();

            AsegurarTabla();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                    SELECT Id, Manzana, Lote, Identificador, EsConcepto, NombreNodo,
                           Descripcion, Extension, TamanioKB, FechaCaptura, UsuarioCaptura
                    FROM FotosConcepto
                    WHERE Manzana = @Manzana AND Lote = @Lote
                      AND Identificador = @Identificador AND EsConcepto = @EsConcepto
                    ORDER BY FechaCaptura ASC, Id ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Manzana", manzana);
                    cmd.Parameters.AddWithValue("@Lote", lote);
                    cmd.Parameters.AddWithValue("@Identificador", identificador);
                    cmd.Parameters.AddWithValue("@EsConcepto", esConcepto);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new FotoConceptoInfo
                            {
                                Id = reader.GetInt32(0),
                                Manzana = reader.GetString(1),
                                Lote = reader.GetString(2),
                                Identificador = reader.GetString(3),
                                EsConcepto = reader.GetBoolean(4),
                                NombreNodo = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                Descripcion = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                Extension = reader.IsDBNull(7) ? "jpg" : reader.GetString(7),
                                TamanioKB = reader.IsDBNull(8) ? 0 : reader.GetDouble(8),
                                Fecha = reader.GetDateTime(9),
                                Usuario = reader.IsDBNull(10) ? "Sistema" : reader.GetString(10)
                            });
                        }
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Obtiene el binario completo de una foto.
        /// </summary>
        public byte[] ObtenerFoto(int id)
        {
            AsegurarTabla();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT Foto FROM FotosConcepto WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    return cmd.ExecuteScalar() as byte[];
                }
            }
        }

        /// <summary>
        /// Cuenta las fotos asociadas a un concepto/partida.
        /// </summary>
        public int ContarFotos(string manzana, string lote, string identificador, bool esConcepto)
        {
            AsegurarTabla();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                    SELECT COUNT(*) FROM FotosConcepto
                    WHERE Manzana = @Manzana AND Lote = @Lote
                      AND Identificador = @Identificador AND EsConcepto = @EsConcepto";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Manzana", manzana);
                    cmd.Parameters.AddWithValue("@Lote", lote);
                    cmd.Parameters.AddWithValue("@Identificador", identificador);
                    cmd.Parameters.AddWithValue("@EsConcepto", esConcepto);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Actualiza la descripción de una foto.
        /// </summary>
        public void ActualizarDescripcion(int id, string descripcion)
        {
            AsegurarTabla();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE FotosConcepto SET Descripcion = @Descripcion WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Descripcion", (object)descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Elimina una foto.
        /// </summary>
        public bool EliminarFoto(int id)
        {
            AsegurarTabla();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("DELETE FROM FotosConcepto WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }

    /// <summary>
    /// Información de una foto adjunta a un concepto/partida.
    /// </summary>
    public class FotoConceptoInfo
    {
        public int Id { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Identificador { get; set; }
        public bool EsConcepto { get; set; }
        public string NombreNodo { get; set; }
        public string Descripcion { get; set; }
        public string Extension { get; set; }
        public double TamanioKB { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }

        public string TamanioFormateado =>
            TamanioKB < 1024 ? $"{TamanioKB:F1} KB" : $"{TamanioKB / 1024:F2} MB";

        public string FechaFormateada => Fecha.ToString("dd/MM/yyyy HH:mm");
    }
}
