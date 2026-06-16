using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Http;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Endpoints de Almacén · Consulta por casa. Reproducen exactamente las
    /// consultas de FormAlmacen_ConsultaCasa: el "material invertido" son los
    /// destajos finalizados de tipo Material (TipoTarea=1) de cada ruta.
    /// </summary>
    [RoutePrefix("api/almacen")]
    public class AlmacenController : ApiController
    {
        private const string TablaRutaTunera = "RutaTuneraDestajo";
        private const string TablaRutaCalandra = "RutaCalandraDestajo";

        private const string RutaTodas = "Todas";
        private const string RutaTunera = "Tunera";
        private const string RutaCalandra = "Calandra";

        /// <summary>GET /api/almacen/manzanas</summary>
        [HttpGet, Route("manzanas")]
        public IHttpActionResult Manzanas()
        {
            var lista = new List<string>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                @"SELECT DISTINCT Manzana
                  FROM ActivacionTareasRuta
                  WHERE Manzana IS NOT NULL AND Manzana <> ''
                  ORDER BY Manzana", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    lista.Add(reader["Manzana"].ToString());
            }
            return Ok(lista);
        }

        /// <summary>GET /api/almacen/lotes?manzana=X</summary>
        [HttpGet, Route("lotes")]
        public IHttpActionResult Lotes(string manzana)
        {
            if (string.IsNullOrWhiteSpace(manzana))
                return BadRequest("Falta el parámetro 'manzana'.");

            var lista = new List<string>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand(
                @"SELECT DISTINCT Lote
                  FROM ActivacionTareasRuta
                  WHERE Manzana = @manzana
                    AND Lote IS NOT NULL AND Lote <> ''
                  ORDER BY Lote", conn))
            {
                cmd.Parameters.AddWithValue("@manzana", manzana);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        lista.Add(reader["Lote"].ToString());
                }
            }
            return Ok(lista);
        }

        /// <summary>GET /api/almacen/materiales?manzana=X&amp;lote=Y&amp;ruta=Todas|Tunera|Calandra</summary>
        [HttpGet, Route("materiales")]
        public IHttpActionResult Materiales(string manzana, string lote, string ruta = RutaTodas)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote))
                return BadRequest("Faltan los parámetros 'manzana' y/o 'lote'.");

            bool incluirTunera = ruta == RutaTodas || ruta == RutaTunera;
            bool incluirCalandra = ruta == RutaTodas || ruta == RutaCalandra;

            var resultado = new List<MaterialCasaDto>();
            using (var conn = Db.Abrir())
            {
                if (incluirTunera) CargarRuta(conn, resultado, TablaRutaTunera, manzana, lote);
                if (incluirCalandra) CargarRuta(conn, resultado, TablaRutaCalandra, manzana, lote);
            }
            return Ok(resultado);
        }

        private static void CargarRuta(SqlConnection conn, List<MaterialCasaDto> destino,
            string tablaRuta, string manzana, string lote)
        {
            string sql = $@"
                SELECT
                    a.Prototipo,
                    parent.Nombre        AS DestajoNombre,
                    child.Nombre         AS MaterialNombre,
                    child.Descripcion    AS MaterialDescripcion,
                    a.CuadrillaAsignada,
                    a.FechaActivacion,
                    a.FechaFinalizacion,
                    ISNULL(MAX(CASE WHEN c.NombreColumna = 'Cantidad' THEN c.Valor END), '0') AS Cantidad,
                    ISNULL(MAX(CASE WHEN c.NombreColumna = 'Unidad'   THEN c.Valor END), '')  AS Unidad,
                    ISNULL(MAX(CASE WHEN c.NombreColumna = 'Precio'   THEN c.Valor END), '0') AS PrecioUnitario
                FROM [{tablaRuta}] child
                INNER JOIN [{tablaRuta}] parent
                       ON child.ParentId = parent.ID
                INNER JOIN ActivacionTareasRuta a
                       ON a.NodoID = parent.ID
                      AND a.Ruta   = @ruta
                LEFT JOIN [{tablaRuta}_Columnas] c
                       ON c.NodoID = child.ID
                WHERE a.Manzana   = @m
                  AND a.Lote      = @l
                  AND a.Finalizado = 1
                  AND child.TipoTarea = 1   /* TipoTarea.Material */
                GROUP BY a.Prototipo, parent.Nombre, child.Nombre, child.Descripcion,
                         a.CuadrillaAsignada, a.FechaActivacion, a.FechaFinalizacion
                ORDER BY a.FechaFinalizacion DESC, parent.Nombre, child.Nombre";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                cmd.Parameters.AddWithValue("@ruta", tablaRuta);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        decimal cantidad = ParseDecimal(reader["Cantidad"]);
                        decimal precio = ParseDecimal(reader["PrecioUnitario"]);

                        destino.Add(new MaterialCasaDto
                        {
                            Ruta = RutaCorta(tablaRuta),
                            Destajo = reader["DestajoNombre"]?.ToString() ?? string.Empty,
                            Material = reader["MaterialNombre"]?.ToString() ?? string.Empty,
                            Descripcion = reader["MaterialDescripcion"] == DBNull.Value
                                ? "" : reader["MaterialDescripcion"].ToString(),
                            Unidad = reader["Unidad"]?.ToString() ?? "",
                            Cantidad = cantidad,
                            PrecioUnitario = precio,
                            Importe = cantidad * precio,
                            Cuadrilla = reader["CuadrillaAsignada"] == DBNull.Value
                                ? "" : reader["CuadrillaAsignada"].ToString(),
                            FechaActivacion = reader["FechaActivacion"] == DBNull.Value
                                ? (DateTime?)null : Convert.ToDateTime(reader["FechaActivacion"]),
                            FechaFinalizacion = reader["FechaFinalizacion"] == DBNull.Value
                                ? (DateTime?)null : Convert.ToDateTime(reader["FechaFinalizacion"]),
                            Prototipo = reader["Prototipo"] == DBNull.Value
                                ? "" : reader["Prototipo"].ToString()
                        });
                    }
                }
            }
        }

        private static decimal ParseDecimal(object valor)
        {
            if (valor == null || valor == DBNull.Value) return 0m;
            string s = valor.ToString();
            if (string.IsNullOrWhiteSpace(s)) return 0m;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal r)) return r;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal r2)) return r2;
            return 0m;
        }

        private static string RutaCorta(string tablaRuta)
        {
            if (tablaRuta == TablaRutaTunera) return RutaTunera;
            if (tablaRuta == TablaRutaCalandra) return RutaCalandra;
            return tablaRuta;
        }
    }
}
