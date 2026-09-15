using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;
using Calandria.Api.Auth;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Endpoints de Compras · Proveedores (tabla unificada PROVEEDORESCALANDRIA).
    /// Reproduce FormAgregarProveedor (alta) y las cargas de proveedor de
    /// FormCompraMulti / FormCompraIndirecta. El cliente pide la lista completa y
    /// resuelve localmente las búsquedas por clave o por nombre.
    /// </summary>
    [RoutePrefix("api/proveedores"), RequierePermiso("compras.ver")]
    public class ProveedoresController : ApiController
    {
        /// <summary>GET /api/proveedores → catálogo completo (ordenado por nombre).</summary>
        [HttpGet, Route("")]
        public IHttpActionResult Listar()
        {
            var lista = new List<ProveedorDto>();
            using (var conn = Db.Abrir())
            {
                EnsureTabla(conn);

                using (var cmd = new SqlCommand(
                    @"SELECT Folio, ClaveUnica, Nombre, RFC, Direccion, Telefono
                      FROM PROVEEDORESCALANDRIA
                      ORDER BY Nombre", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new ProveedorDto
                        {
                            Folio = reader["Folio"] == DBNull.Value ? "" : reader["Folio"].ToString(),
                            ClaveUnica = reader["ClaveUnica"]?.ToString() ?? "",
                            Nombre = reader["Nombre"]?.ToString() ?? "",
                            Rfc = reader["RFC"] == DBNull.Value ? "" : reader["RFC"].ToString(),
                            Direccion = reader["Direccion"] == DBNull.Value ? "" : reader["Direccion"].ToString(),
                            Telefono = reader["Telefono"] == DBNull.Value ? "" : reader["Telefono"].ToString()
                        });
                    }
                }
            }
            return Ok(lista);
        }

        /// <summary>
        /// POST /api/proveedores · alta de proveedor. Devuelve 409 Conflict si ya
        /// existe la clave única (lo que antes detectaba el cliente por SqlException 2627).
        /// </summary>
        [HttpPost, Route(""), RequierePermiso("compras.editar")]
        public IHttpActionResult Crear([FromBody] CrearProveedorRequest req)
        {
            if (req == null)
                return BadRequest("Cuerpo vacío.");

            string clave = (req.ClaveUnica ?? "").Trim().ToUpperInvariant();
            string nombre = (req.Nombre ?? "").Trim();
            string rfc = (req.Rfc ?? "").Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(clave))
                return BadRequest("La Clave Única es obligatoria.");
            if (string.IsNullOrWhiteSpace(nombre))
                return BadRequest("El Nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(rfc))
                return BadRequest("El RFC es obligatorio.");
            if (rfc.Length < 12 || rfc.Length > 13)
                return BadRequest("El RFC debe tener 12 o 13 caracteres.");

            string direccion = string.IsNullOrWhiteSpace(req.Direccion) ? null : req.Direccion.Trim();
            string telefono = string.IsNullOrWhiteSpace(req.Telefono) ? null : req.Telefono.Trim();

            string folio;
            using (var conn = Db.Abrir())
            {
                EnsureTabla(conn);

                folio = GenerarFolio(conn);
                try
                {
                    using (var cmd = new SqlCommand(
                        @"INSERT INTO PROVEEDORESCALANDRIA (Folio, ClaveUnica, Nombre, RFC, Direccion, Telefono)
                          VALUES (@folio, @clave, @nombre, @rfc, @direccion, @telefono)", conn))
                    {
                        cmd.Parameters.AddWithValue("@folio", folio);
                        cmd.Parameters.AddWithValue("@clave", clave);
                        cmd.Parameters.AddWithValue("@nombre", nombre);
                        cmd.Parameters.AddWithValue("@rfc", rfc);
                        cmd.Parameters.AddWithValue("@direccion", (object)direccion ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@telefono", (object)telefono ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                {
                    return Conflict();
                }
            }
            return Ok(new ProveedorCreadoResponse { Folio = folio });
        }

        // ponytail: COUNT(*)+1 sin lock; dos altas simultáneas podrían repetir folio
        // (igual que GenerarFolio de OrdenesCompraController). Subir a un SEQUENCE si
        // el alta de proveedores deja de ser un evento esporádico.
        /// <summary>Folio consecutivo global PROV-NNNN, según el orden de alta.</summary>
        private static string GenerarFolio(SqlConnection conn, SqlTransaction tx = null)
        {
            int consecutivo;
            using (var cmd = new SqlCommand("SELECT COUNT(*) + 1 FROM PROVEEDORESCALANDRIA", conn, tx))
                consecutivo = (int)cmd.ExecuteScalar();
            return "PROV-" + consecutivo.ToString("D4");
        }

        /// <summary>Crea PROVEEDORESCALANDRIA si no existe, y agrega Folio si falta (idempotente).</summary>
        private static void EnsureTabla(SqlConnection conn)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.PROVEEDORESCALANDRIA') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.PROVEEDORESCALANDRIA (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Folio NVARCHAR(20) NULL,
        ClaveUnica NVARCHAR(50) NOT NULL UNIQUE,
        Nombre NVARCHAR(200) NOT NULL,
        RFC NVARCHAR(13) NOT NULL,
        Direccion NVARCHAR(500) NULL,
        Telefono NVARCHAR(20) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT(GETDATE())
    );
END
ELSE IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'Folio' AND Object_ID = OBJECT_ID(N'dbo.PROVEEDORESCALANDRIA'))
BEGIN
    ALTER TABLE dbo.PROVEEDORESCALANDRIA ADD Folio NVARCHAR(20) NULL;
END";
            using (var cmd = new SqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }
    }
}
