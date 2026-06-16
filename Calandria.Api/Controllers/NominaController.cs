using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Endpoints de Nómina · asignación de importe de una tarea de Mano de Obra
    /// entre los miembros de una cuadrilla. Reproduce FormAsignarNomina:
    /// cuadrillas y miembros desde MiembrosCuadrilla; la asignación se guarda en
    /// NominaTareasAsignada (DELETE + INSERT por tarea).
    /// </summary>
    [RoutePrefix("api/nomina")]
    public class NominaController : ApiController
    {
        /// <summary>GET /api/nomina/cuadrillas → códigos de cuadrilla.</summary>
        [HttpGet, Route("cuadrillas")]
        public IHttpActionResult Cuadrillas()
        {
            var lista = new List<string>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                "SELECT DISTINCT CodigoCuadrilla FROM MiembrosCuadrilla ORDER BY CodigoCuadrilla", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    lista.Add(reader.GetString(0));
            }
            return Ok(lista);
        }

        /// <summary>GET /api/nomina/cuadrillas/{codigo}/miembros</summary>
        [HttpGet, Route("cuadrillas/{codigo}/miembros")]
        public IHttpActionResult Miembros(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return BadRequest("Falta el código de cuadrilla.");

            var lista = new List<MiembroCuadrillaDto>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(@"
                SELECT IdTrabajador, Nombre, Rol, EsJefe, Telefono
                FROM MiembrosCuadrilla
                WHERE CodigoCuadrilla = @codigo
                ORDER BY EsJefe DESC, Nombre", conn))
            {
                cmd.Parameters.AddWithValue("@codigo", codigo);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new MiembroCuadrillaDto
                        {
                            IdTrabajador = reader["IdTrabajador"] == DBNull.Value
                                ? (int?)null
                                : Convert.ToInt32(reader["IdTrabajador"]),
                            Nombre = reader["Nombre"]?.ToString() ?? "",
                            Rol = reader["Rol"]?.ToString() ?? "",
                            EsJefe = reader["EsJefe"] != DBNull.Value && Convert.ToBoolean(reader["EsJefe"]),
                            Telefono = reader["Telefono"]?.ToString() ?? ""
                        });
                    }
                }
            }
            return Ok(lista);
        }

        /// <summary>
        /// GET /api/nomina/asignacion?manzana=&amp;lote=&amp;ruta=&amp;nodoId=
        /// Devuelve la asignación previa de la tarea (o vacía si no hay).
        /// </summary>
        [HttpGet, Route("asignacion")]
        public IHttpActionResult Asignacion(string manzana, string lote, string ruta, int nodoId)
        {
            var dto = new AsignacionNominaDto();
            using (var conn = Db.Abrir())
            {
                EnsureTabla(conn);

                using (var cmd = new SqlCommand(@"
                    SELECT CodigoCuadrilla, IdTrabajador, NombreTrabajador, Monto
                    FROM NominaTareasAsignada
                    WHERE Manzana = @m AND Lote = @l AND Ruta = @r AND NodoID = @nodo", conn))
                {
                    cmd.Parameters.AddWithValue("@m", manzana ?? "");
                    cmd.Parameters.AddWithValue("@l", lote ?? "");
                    cmd.Parameters.AddWithValue("@r", ruta ?? "");
                    cmd.Parameters.AddWithValue("@nodo", nodoId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dto.CodigoCuadrilla = reader["CodigoCuadrilla"]?.ToString();
                            dto.Montos.Add(new MontoTrabajadorDto
                            {
                                IdTrabajador = reader["IdTrabajador"] == DBNull.Value
                                    ? (int?)null
                                    : Convert.ToInt32(reader["IdTrabajador"]),
                                NombreTrabajador = reader["NombreTrabajador"]?.ToString() ?? "",
                                Monto = reader["Monto"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["Monto"])
                            });
                        }
                    }
                }
            }
            return Ok(dto);
        }

        /// <summary>
        /// POST /api/nomina/asignacion · reemplaza la asignación de la tarea
        /// (DELETE + INSERT en una transacción), igual que FormAsignarNomina.
        /// </summary>
        [HttpPost, Route("asignacion")]
        public IHttpActionResult GuardarAsignacion([FromBody] GuardarAsignacionRequest req)
        {
            if (req == null)
                return BadRequest("Cuerpo vacío.");
            if (string.IsNullOrWhiteSpace(req.CodigoCuadrilla))
                return BadRequest("Falta la cuadrilla.");
            if (req.Lineas == null || req.Lineas.Count == 0)
                return BadRequest("No hay miembros que asignar.");

            using (var conn = Db.Abrir())
            {
                EnsureTabla(conn);

                using (var tx = conn.BeginTransaction())
                {
                    using (var cmdDel = new SqlCommand(@"
                        DELETE FROM NominaTareasAsignada
                        WHERE Manzana = @m AND Lote = @l AND Ruta = @r AND NodoID = @nodo", conn, tx))
                    {
                        cmdDel.Parameters.AddWithValue("@m", req.Manzana ?? "");
                        cmdDel.Parameters.AddWithValue("@l", req.Lote ?? "");
                        cmdDel.Parameters.AddWithValue("@r", req.Ruta ?? "");
                        cmdDel.Parameters.AddWithValue("@nodo", req.NodoId);
                        cmdDel.ExecuteNonQuery();
                    }

                    foreach (var linea in req.Lineas)
                    {
                        using (var cmdIns = new SqlCommand(@"
                            INSERT INTO NominaTareasAsignada
                            (Manzana, Lote, Ruta, NodoID, NombreTarea, CodigoCuadrilla,
                             IdTrabajador, NombreTrabajador, Rol, EsJefe, Monto, TotalAsignado, FechaActualizacion)
                            VALUES
                            (@m, @l, @r, @nodo, @tarea, @codigo,
                             @idTrab, @nombre, @rol, @esJefe, @monto, @total, GETDATE())", conn, tx))
                        {
                            cmdIns.Parameters.AddWithValue("@m", req.Manzana ?? "");
                            cmdIns.Parameters.AddWithValue("@l", req.Lote ?? "");
                            cmdIns.Parameters.AddWithValue("@r", req.Ruta ?? "");
                            cmdIns.Parameters.AddWithValue("@nodo", req.NodoId);
                            cmdIns.Parameters.AddWithValue("@tarea", req.NombreTarea ?? "");
                            cmdIns.Parameters.AddWithValue("@codigo", req.CodigoCuadrilla);
                            cmdIns.Parameters.AddWithValue("@idTrab", (object)linea.IdTrabajador ?? DBNull.Value);
                            cmdIns.Parameters.AddWithValue("@nombre", linea.Nombre ?? "");
                            cmdIns.Parameters.AddWithValue("@rol", linea.Rol ?? "");
                            cmdIns.Parameters.AddWithValue("@esJefe", linea.EsJefe);
                            cmdIns.Parameters.AddWithValue("@monto", linea.Monto);
                            cmdIns.Parameters.AddWithValue("@total", req.TotalDistribuir);
                            cmdIns.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }
            return Ok();
        }

        /// <summary>
        /// Crea NominaTareasAsignada si no existe (idempotente). Antes lo hacía el
        /// cliente en cada apertura del formulario; ahora vive en el servidor.
        /// </summary>
        private static void EnsureTabla(SqlConnection conn, SqlTransaction tx = null)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NominaTareasAsignada')
BEGIN
    CREATE TABLE NominaTareasAsignada (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Manzana NVARCHAR(10),
        Lote NVARCHAR(10),
        Ruta NVARCHAR(50),
        NodoID INT,
        NombreTarea NVARCHAR(300),
        CodigoCuadrilla NVARCHAR(20),
        IdTrabajador INT NULL,
        NombreTrabajador NVARCHAR(200),
        Rol NVARCHAR(50),
        EsJefe BIT,
        Monto DECIMAL(18,2),
        TotalAsignado DECIMAL(18,2),
        FechaActualizacion DATETIME DEFAULT GETDATE()
    );
END";
            using (var cmd = new SqlCommand(sql, conn, tx))
                cmd.ExecuteNonQuery();
        }
    }
}
