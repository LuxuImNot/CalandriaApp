using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Concentrado financiero por casa (reproduce FormAdministrativos: Estimaciones +
    /// Destajos, con las mismas consultas defensivas por si la BD de la obra no tiene
    /// todavía alguna columna) más tres secciones nuevas — Compras ligadas a la casa,
    /// Salidas de Almacén y Nómina detallada — para completar el panorama de dinero.
    /// Sólo lectura: no hay endpoints de escritura aquí.
    /// </summary>
    [RoutePrefix("api/administrativos")]
    public class AdministrativosController : ApiController
    {
        /// <summary>GET /api/administrativos/manzanas</summary>
        [HttpGet, Route("manzanas")]
        public IHttpActionResult Manzanas()
        {
            var lista = new List<string>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand("SELECT DISTINCT Manzana FROM InventarioCasas ORDER BY Manzana", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    lista.Add(reader["Manzana"].ToString());
            }
            return Ok(lista);
        }

        /// <summary>GET /api/administrativos/lotes?manzana=X</summary>
        [HttpGet, Route("lotes")]
        public IHttpActionResult Lotes(string manzana)
        {
            if (string.IsNullOrWhiteSpace(manzana))
                return BadRequest("Falta el parámetro 'manzana'.");

            var lista = new List<string>();
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand("SELECT DISTINCT Lote FROM InventarioCasas WHERE Manzana = @m ORDER BY Lote", conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        lista.Add(reader["Lote"].ToString());
                }
            }
            return Ok(lista);
        }

        /// <summary>GET /api/administrativos/consolidado?manzana=X&amp;lote=Y</summary>
        [HttpGet, Route("consolidado")]
        public IHttpActionResult Consolidado(string manzana, string lote)
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote))
                return BadRequest("Faltan 'manzana' y/o 'lote'.");

            using (var conn = Db.Abrir())
            {
                var dto = new ConsolidadoCasaDto { Manzana = manzana, Lote = lote };
                dto.Prototipo = ObtenerPrototipo(conn, manzana, lote);

                dto.Estimaciones = CargarEstimaciones(conn, manzana, lote);
                dto.TotalEstimaciones = dto.Estimaciones.Sum(e => e.MontoEjecutado);

                dto.Destajos = CargarDestajos(conn, manzana, lote, dto.Prototipo);
                dto.TotalDestajosComprometido = dto.Destajos.Sum(d => d.Importe);
                dto.TotalManoObraGastado = dto.Destajos.Where(d => d.Tipo == "M. de Obra").Sum(d => d.MontoGastado);
                dto.TotalMaterialGastado = dto.Destajos.Where(d => d.Tipo == "Material").Sum(d => d.MontoGastado);

                dto.Compras = CargarCompras(conn, manzana, lote);
                dto.TotalCompras = dto.Compras.Sum(c => c.Importe);

                dto.SalidasAlmacen = CargarSalidasAlmacen(conn, manzana, lote);
                dto.TotalSalidasAlmacen = dto.SalidasAlmacen.Sum(s => s.Importe);

                dto.Nomina = CargarNominaDetallada(conn, manzana, lote);
                dto.TotalNomina = dto.Nomina.Sum(n => n.Monto);

                // Gran total = igual que siempre (RefrescarHUD/GenerarPDF en FormAdministrativos):
                // Estimaciones + gasto reconocido de destajos. Compras/SalidasAlmacen/Nómina son
                // bases de costo distintas y se solapan entre sí (una salida de almacén ya viene
                // de una compra previa; el importe de un destajo de Material es lo comprometido en
                // el árbol, no necesariamente lo que costó la compra real) — sumarlas aquí
                // duplicaría el conteo. Se muestran aparte, cada una con su propio subtotal.
                dto.GranTotal = dto.TotalEstimaciones + dto.TotalManoObraGastado + dto.TotalMaterialGastado;

                return Ok(dto);
            }
        }

        private static string ObtenerPrototipo(SqlConnection conn, string manzana, string lote)
        {
            using (var cmd = new SqlCommand("SELECT Prototipo FROM InventarioCasas WHERE Manzana = @m AND Lote = @l", conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                var r = cmd.ExecuteScalar();
                return r == null || r == DBNull.Value ? "" : r.ToString();
            }
        }

        // ------------------------------------------------------------------
        // Estimaciones — port 1:1 de FormAdministrativos.CargarEstimaciones
        // ------------------------------------------------------------------

        private static List<EstimacionItemDto> CargarEstimaciones(SqlConnection conn, string manzana, string lote)
        {
            var lista = new List<EstimacionItemDto>();

            var colsAvance = LeerColumnas(conn, "AvanceManualObra");
            bool tieneFecha = colsAvance.Contains("FechaFinalizacion");

            // PresupuestoObra.WBS_Correcto no existe todavía en algunas BD de obra más
            // antiguas (el JOIN con AvanceManualObra se apoya en esa columna); sin ella
            // no hay forma confiable de correlacionar, así que se omite el enriquecimiento
            // de Código/Etapa/Partida en vez de tronar con "Invalid column name".
            var colsPres = LeerColumnas(conn, "PresupuestoObra");
            bool tieneJoinPresupuesto = colsPres.Contains("WBS_Correcto");
            bool tieneCodigo = tieneJoinPresupuesto && colsPres.Contains("Codigo");
            bool tieneEtapa = tieneJoinPresupuesto && colsPres.Contains("Etapa");
            bool tienePartida = tieneJoinPresupuesto && colsPres.Contains("Partida");

            var sb = new System.Text.StringBuilder();
            sb.Append("SELECT a.WBS, a.AvancePorcentaje, a.MontoEjecutado");
            if (tieneFecha) sb.Append(", a.FechaFinalizacion");
            if (tieneCodigo) sb.Append(", p.Codigo");
            if (tieneEtapa) sb.Append(", p.Etapa");
            if (tienePartida) sb.Append(", p.Partida");
            sb.Append(" FROM AvanceManualObra a ");
            if (tieneJoinPresupuesto)
                sb.Append(" LEFT JOIN PresupuestoObra p ON TRY_CAST(a.WBS AS INT) = p.WBS_Correcto ");
            sb.Append(" WHERE a.Manzana = @m AND a.Lote = @l ");
            sb.Append(" ORDER BY ");
            if (tieneEtapa) sb.Append("p.Etapa, ");
            sb.Append("a.WBS");

            using (var cmd = new SqlCommand(sb.ToString(), conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new EstimacionItemDto
                        {
                            Wbs = reader["WBS"]?.ToString() ?? "",
                            Codigo = tieneCodigo && reader["Codigo"] != DBNull.Value ? reader["Codigo"].ToString() : "",
                            Etapa = tieneEtapa && reader["Etapa"] != DBNull.Value ? reader["Etapa"].ToString() : "",
                            Partida = tienePartida && reader["Partida"] != DBNull.Value ? reader["Partida"].ToString() : "",
                            AvancePorcentaje = LeerDecimalSeguro(reader, "AvancePorcentaje"),
                            MontoEjecutado = LeerDecimalSeguro(reader, "MontoEjecutado"),
                            FechaFinalizacion = tieneFecha && reader["FechaFinalizacion"] != DBNull.Value
                                ? (DateTime?)Convert.ToDateTime(reader["FechaFinalizacion"]) : null
                        });
                    }
                }
            }

            return lista;
        }

        // ------------------------------------------------------------------
        // Destajos — port 1:1 de FormAdministrativos.CargarDestajos + CargarMontosNomina
        // ------------------------------------------------------------------

        private static List<DestajoItemDto> CargarDestajos(SqlConnection conn, string manzana, string lote, string prototipo)
        {
            var lista = new List<DestajoItemDto>();

            string ruta = !string.IsNullOrEmpty(prototipo) && prototipo.ToUpper().Contains("CALANDRA")
                ? "RutaCalandraDestajo"
                : "RutaTuneraDestajo";

            var nominaPorNodo = CargarMontosNomina(conn, manzana, lote, ruta);

            if (!ExisteTabla(conn, "ActivacionTareasRuta")) return lista;
            if (!ExisteTabla(conn, ruta)) return lista;
            bool existeColumnasRuta = ExisteTabla(conn, ruta + "_Columnas");

            var colsRuta = LeerColumnas(conn, ruta);
            bool tieneTipoTarea = colsRuta.Contains("TipoTarea");

            var colsAct = LeerColumnas(conn, "ActivacionTareasRuta");
            bool tieneActFinalizado = colsAct.Contains("Finalizado");
            bool tieneActFechaFin = colsAct.Contains("FechaFinalizacion");
            bool tieneActCuadrilla = colsAct.Contains("CuadrillaAsignada");

            string tipoSelect = tieneTipoTarea ? "ISNULL(r.TipoTarea, 0) AS TipoTarea" : "0 AS TipoTarea";
            string finSelect = tieneActFinalizado ? "ISNULL(a.Finalizado, 0) AS Finalizado" : "0 AS Finalizado";
            string fechaFinSelect = tieneActFechaFin ? "a.FechaFinalizacion" : "CAST(NULL AS DATETIME) AS FechaFinalizacion";
            string cuadrillaSelect = tieneActCuadrilla ? "a.CuadrillaAsignada" : "CAST(NULL AS NVARCHAR(20)) AS CuadrillaAsignada";

            string columnasJoin;
            string cantidadSelect, unidadSelect, precioSelect;
            if (existeColumnasRuta)
            {
                columnasJoin = $"LEFT JOIN {ruta}_Columnas c ON r.ID = c.NodoID";
                cantidadSelect = "ISNULL(MAX(CASE WHEN c.NombreColumna = 'Cantidad' THEN c.Valor END), '0') AS Cantidad";
                unidadSelect = "ISNULL(MAX(CASE WHEN c.NombreColumna = 'Unidad'   THEN c.Valor END), '')  AS Unidad";
                precioSelect = "ISNULL(MAX(CASE WHEN c.NombreColumna = 'Precio'   THEN c.Valor END), '0') AS PrecioUnitario";
            }
            else
            {
                columnasJoin = "";
                cantidadSelect = "'0' AS Cantidad";
                unidadSelect = "'' AS Unidad";
                precioSelect = "'0' AS PrecioUnitario";
            }

            string groupBy = existeColumnasRuta
                ? $@"GROUP BY a.NodoID, a.DesatajoActivado,
                             {(tieneActFinalizado ? "a.Finalizado," : "")}
                             {(tieneActCuadrilla ? "a.CuadrillaAsignada," : "")}
                             {(tieneActFechaFin ? "a.FechaFinalizacion," : "")}
                             r.Nombre, r.Descripcion{(tieneTipoTarea ? ", r.TipoTarea" : "")}"
                : "";

            bool tieneParent = colsRuta.Contains("ParentId");
            string categoriaSelect = tieneParent ? "ISNULL(p_parent.Nombre, '') AS Categoria" : "'' AS Categoria";
            string parentJoin = tieneParent ? $"LEFT JOIN {ruta} p_parent ON p_parent.ID = r.ParentId" : "";
            string categoriaGroup = tieneParent ? ", p_parent.Nombre" : "";

            string sql = $@"
                SELECT  a.NodoID,
                        a.DesatajoActivado,
                        {finSelect},
                        {cuadrillaSelect},
                        {fechaFinSelect},
                        r.Nombre,
                        r.Descripcion,
                        {categoriaSelect},
                        {tipoSelect},
                        {cantidadSelect},
                        {unidadSelect},
                        {precioSelect}
                FROM ActivacionTareasRuta a
                INNER JOIN {ruta} r ON a.NodoID = r.ID
                {parentJoin}
                {columnasJoin}
                WHERE a.Manzana = @m
                  AND a.Lote    = @l
                  AND a.Ruta    = @ruta
                  AND ISNULL(a.DesatajoActivado, 0) = 1
                {(existeColumnasRuta ? groupBy + categoriaGroup : "")}
                ORDER BY {(tieneParent ? "p_parent.Nombre, " : "")}r.Nombre";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                cmd.Parameters.AddWithValue("@ruta", ruta);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        decimal cantidad = ParseDecimalSeguro(reader["Cantidad"]?.ToString());
                        decimal precio = ParseDecimalSeguro(reader["PrecioUnitario"]?.ToString());
                        bool finalizado = reader["Finalizado"] != DBNull.Value && Convert.ToBoolean(reader["Finalizado"]);

                        int tipoRaw = reader["TipoTarea"] != DBNull.Value ? Convert.ToInt32(reader["TipoTarea"]) : 0;
                        string tipoTexto = TipoTareaTexto(tipoRaw);

                        int nodoId = reader["NodoID"] != DBNull.Value ? Convert.ToInt32(reader["NodoID"]) : 0;
                        decimal nominaMonto;
                        nominaPorNodo.TryGetValue(nodoId, out nominaMonto);

                        string categoria;
                        try { categoria = reader["Categoria"]?.ToString() ?? ""; }
                        catch { categoria = ""; }

                        decimal importe = cantidad * precio;
                        // MontoGastado: M.O. = lo asignado en nómina · Material = importe sólo si finalizado.
                        decimal montoGastado = tipoTexto == "M. de Obra" ? nominaMonto
                            : (tipoTexto == "Material" && finalizado ? importe : 0m);

                        string cuadrilla = reader["CuadrillaAsignada"] != DBNull.Value ? reader["CuadrillaAsignada"].ToString() : "";

                        lista.Add(new DestajoItemDto
                        {
                            NodoId = nodoId,
                            Nombre = reader["Nombre"]?.ToString() ?? "",
                            Categoria = categoria,
                            Cantidad = cantidad,
                            Unidad = reader["Unidad"]?.ToString() ?? "",
                            PrecioUnitario = precio,
                            Importe = importe,
                            Tipo = tipoTexto,
                            MontoGastado = montoGastado,
                            Cuadrilla = cuadrilla,
                            Finalizado = finalizado,
                            Estado = finalizado ? "Finalizado" : (!string.IsNullOrEmpty(cuadrilla) ? "Con cuadrilla" : "Activado")
                        });
                    }
                }
            }

            return lista;
        }

        // TipoTarea (DynamicSepticSystem.NodoTree): Ninguno=0, Material=1, ManoDeObra=2.
        private static string TipoTareaTexto(int tipo)
        {
            switch (tipo)
            {
                case 1: return "Material";
                case 2: return "M. de Obra";
                default: return "—";
            }
        }

        private static Dictionary<int, decimal> CargarMontosNomina(SqlConnection conn, string manzana, string lote, string ruta)
        {
            var resultado = new Dictionary<int, decimal>();
            if (!ExisteTabla(conn, "NominaTareasAsignada")) return resultado;

            using (var cmd = new SqlCommand(@"
                SELECT NodoID, SUM(ISNULL(Monto, 0)) AS MontoTotal
                FROM NominaTareasAsignada
                WHERE Manzana = @m AND Lote = @l AND Ruta = @r
                GROUP BY NodoID", conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                cmd.Parameters.AddWithValue("@r", ruta);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int nodoId = Convert.ToInt32(reader["NodoID"]);
                        resultado[nodoId] = LeerDecimalSeguro(reader, "MontoTotal");
                    }
                }
            }
            return resultado;
        }

        // ------------------------------------------------------------------
        // Compras ligadas a la casa (nuevo)
        // ------------------------------------------------------------------

        private static List<CompraCasaDto> CargarCompras(SqlConnection conn, string manzana, string lote)
        {
            var lista = new List<CompraCasaDto>();
            if (!ExisteTabla(conn, "OrdenesCompra") || !ExisteTabla(conn, "OrdenesCompra_Casas") || !ExisteTabla(conn, "OrdenesCompraDetalle"))
                return lista;

            // PROVEEDORESCALANDRIA la crea perezosamente ProveedoresController; puede no
            // existir aún en una obra nueva. Si falta, se resuelve el proveedor con las
            // columnas propias de OrdenesCompra (sin el LEFT JOIN).
            bool tieneProveedores = ExisteTabla(conn, "PROVEEDORESCALANDRIA");
            var colsOc = LeerColumnas(conn, "OrdenesCompra");
            bool tieneProveedorLegacy = colsOc.Contains("Proveedor");
            // NombreOrden se agregó después; algunas BD de obra más antiguas no la tienen.
            bool tieneNombreOrden = colsOc.Contains("NombreOrden");

            string proveedorSelect = tieneProveedores
                ? "COALESCE(prov.Nombre, " + (tieneProveedorLegacy ? "o.Proveedor, " : "") + "o.ProveedorClave, '—') AS Proveedor"
                : "COALESCE(" + (tieneProveedorLegacy ? "o.Proveedor, " : "") + "o.ProveedorClave, '—') AS Proveedor";
            string proveedorJoin = tieneProveedores
                ? "LEFT JOIN PROVEEDORESCALANDRIA prov ON prov.ClaveUnica = o.ProveedorClave"
                : "";
            string nombreOrdenSelect = tieneNombreOrden ? "o.NombreOrden" : "'' AS NombreOrden";
            string nombreOrdenGroup = tieneNombreOrden ? "o.NombreOrden, " : "";

            string sql = $@"
                SELECT o.FolioOC, o.Fecha, o.TipoOrden, {nombreOrdenSelect},
                       {proveedorSelect},
                       COUNT(d.Id) AS NumPartidas,
                       SUM(d.ImporteTotal) AS Importe
                FROM OrdenesCompra o
                INNER JOIN OrdenesCompra_Casas c ON c.FolioOC = o.FolioOC
                INNER JOIN OrdenesCompraDetalle d ON d.FolioOC = o.FolioOC
                {proveedorJoin}
                WHERE c.Manzana = @m AND c.Lote = @l
                GROUP BY o.FolioOC, o.Fecha, o.TipoOrden, {nombreOrdenGroup}
                         {(tieneProveedores ? "prov.Nombre, " : "")}{(tieneProveedorLegacy ? "o.Proveedor, " : "")}o.ProveedorClave
                ORDER BY o.Fecha DESC";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new CompraCasaDto
                        {
                            FolioOC = reader["FolioOC"]?.ToString() ?? "",
                            Fecha = reader["Fecha"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["Fecha"]) : null,
                            TipoOrden = reader["TipoOrden"]?.ToString() ?? "",
                            NombreOrden = reader["NombreOrden"] != DBNull.Value ? reader["NombreOrden"].ToString() : "",
                            Proveedor = reader["Proveedor"]?.ToString() ?? "—",
                            NumPartidas = reader["NumPartidas"] != DBNull.Value ? Convert.ToInt32(reader["NumPartidas"]) : 0,
                            Importe = LeerDecimalSeguro(reader, "Importe")
                        });
                    }
                }
            }
            return lista;
        }

        // ------------------------------------------------------------------
        // Salidas de Almacén hacia la casa (nuevo)
        // ------------------------------------------------------------------

        private static List<SalidaAlmacenCasaDto> CargarSalidasAlmacen(SqlConnection conn, string manzana, string lote)
        {
            var lista = new List<SalidaAlmacenCasaDto>();
            if (!ExisteTabla(conn, "SalidasAlmacen")) return lista;

            // PrecioUnitario/Importe se agregaron después; algunas BD de obra más
            // antiguas solo registraban cantidades, sin costo. Sin esas columnas se
            // reporta el movimiento igual, con importe 0 en vez de tronar.
            var colsSalidas = LeerColumnas(conn, "SalidasAlmacen");
            bool tienePrecio = colsSalidas.Contains("PrecioUnitario");
            bool tieneImporte = colsSalidas.Contains("Importe");
            string precioSelect = tienePrecio ? "PrecioUnitario" : "0 AS PrecioUnitario";
            string importeSelect = tieneImporte ? "Importe" : "0 AS Importe";

            using (var cmd = new SqlCommand($@"
                SELECT Clave, Descripcion, Unidad, Cantidad, {precioSelect}, {importeSelect}, FechaSalida, Justificacion
                FROM SalidasAlmacen
                WHERE Manzana = @m AND Lote = @l
                ORDER BY FechaSalida DESC", conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new SalidaAlmacenCasaDto
                        {
                            Clave = reader["Clave"]?.ToString() ?? "",
                            Descripcion = reader["Descripcion"]?.ToString() ?? "",
                            Unidad = reader["Unidad"]?.ToString() ?? "",
                            Cantidad = LeerDecimalSeguro(reader, "Cantidad"),
                            PrecioUnitario = LeerDecimalSeguro(reader, "PrecioUnitario"),
                            Importe = LeerDecimalSeguro(reader, "Importe"),
                            FechaSalida = reader["FechaSalida"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["FechaSalida"]) : null,
                            Justificacion = reader["Justificacion"] != DBNull.Value ? reader["Justificacion"].ToString() : ""
                        });
                    }
                }
            }
            return lista;
        }

        // ------------------------------------------------------------------
        // Nómina detallada de la casa (nuevo)
        // ------------------------------------------------------------------

        private static List<NominaCasaDto> CargarNominaDetallada(SqlConnection conn, string manzana, string lote)
        {
            var lista = new List<NominaCasaDto>();
            if (!ExisteTabla(conn, "NominaTareasAsignada")) return lista;

            using (var cmd = new SqlCommand(@"
                SELECT NombreTarea, CodigoCuadrilla, NombreTrabajador, Rol, EsJefe, Monto, FechaActualizacion
                FROM NominaTareasAsignada
                WHERE Manzana = @m AND Lote = @l
                ORDER BY FechaActualizacion DESC", conn))
            {
                cmd.Parameters.AddWithValue("@m", manzana);
                cmd.Parameters.AddWithValue("@l", lote);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new NominaCasaDto
                        {
                            NombreTarea = reader["NombreTarea"]?.ToString() ?? "",
                            CodigoCuadrilla = reader["CodigoCuadrilla"]?.ToString() ?? "",
                            NombreTrabajador = reader["NombreTrabajador"]?.ToString() ?? "",
                            Rol = reader["Rol"] != DBNull.Value ? reader["Rol"].ToString() : "",
                            EsJefe = reader["EsJefe"] != DBNull.Value && Convert.ToBoolean(reader["EsJefe"]),
                            Monto = LeerDecimalSeguro(reader, "Monto"),
                            FechaActualizacion = reader["FechaActualizacion"] != DBNull.Value
                                ? (DateTime?)Convert.ToDateTime(reader["FechaActualizacion"]) : null
                        });
                    }
                }
            }
            return lista;
        }

        // ------------------------------------------------------------------
        // Utilidades SQL (mismas que FormAdministrativos)
        // ------------------------------------------------------------------

        private static HashSet<string> LeerColumnas(SqlConnection conn, string tabla)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand(
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @t", conn))
            {
                cmd.Parameters.AddWithValue("@t", tabla);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) set.Add(r.GetString(0));
                }
            }
            return set;
        }

        private static bool ExisteTabla(SqlConnection conn, string tabla)
        {
            using (var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @t", conn))
            {
                cmd.Parameters.AddWithValue("@t", tabla);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static decimal LeerDecimalSeguro(System.Data.IDataReader reader, string columna)
        {
            try
            {
                int idx = reader.GetOrdinal(columna);
                if (reader.IsDBNull(idx)) return 0m;
                object v = reader.GetValue(idx);
                if (v == null) return 0m;
                if (v is decimal d) return d;
                if (v is double dbl) return double.IsNaN(dbl) || double.IsInfinity(dbl) ? 0m : (decimal)dbl;
                if (v is float f) return float.IsNaN(f) || float.IsInfinity(f) ? 0m : (decimal)f;
                if (v is int i) return i;
                if (v is long l) return l;
                return ParseDecimalSeguro(v.ToString());
            }
            catch { return 0m; }
        }

        private static decimal ParseDecimalSeguro(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return 0m;
            decimal d;
            if (decimal.TryParse(valor, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d)) return d;
            if (decimal.TryParse(valor, out d)) return d;
            return 0m;
        }
    }
}
