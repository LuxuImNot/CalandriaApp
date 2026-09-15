using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using Calandria.Api.Auth;
using Calandria.Api.Data;
using Calandria.Api.Models;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Editor del árbol maestro de 3 niveles (Padre/Sub-Padre/Hijo) que alimenta
    /// el módulo Destajos (ver DestajosController). Reproduce el SQL de
    /// FormEditorTreeList.cs, pero unifica el guardado del árbol y de las
    /// definiciones de columnas en UNA sola transacción atómica (el cliente
    /// clásico comprometía las columnas por separado, sin transacción).
    /// Requiere Admin en escritura: esta pantalla reescribe la estructura que
    /// ya usan todas las casas en producción.
    /// </summary>
    [RoutePrefix("api/editor-tareas"), RequierePermiso("destajos.ver")]
    public class EditorTareasController : ApiController
    {
        private const string TablaRutaTunera = "RutaTuneraDestajo";
        private const string TablaRutaCalandra = "RutaCalandraDestajo";

        private static string ValidarRuta(string ruta)
        {
            if (ruta == TablaRutaTunera) return TablaRutaTunera;
            if (ruta == TablaRutaCalandra) return TablaRutaCalandra;
            return null;
        }

        private static bool EsAdmin(System.Security.Principal.IPrincipal user)
        {
            return user.IsInRole("Admin") ||
                   string.Equals(user.Identity?.Name, "admin", StringComparison.OrdinalIgnoreCase);
        }

        // ==================================================================
        // Lectura
        // ==================================================================

        /// <summary>GET /api/editor-tareas/arbol?ruta=RutaTuneraDestajo|RutaCalandraDestajo</summary>
        [HttpGet, Route("arbol")]
        public IHttpActionResult Arbol(string ruta)
        {
            string tabla = ValidarRuta(ruta);
            if (tabla == null) return BadRequest("Ruta inválida.");

            using (var conn = Db.Abrir())
            {
                var nodos = CargarNodos(conn, tabla);
                var columnas = CargarColumnas(conn, tabla);
                return Ok(new ArbolEditorDto { Ruta = tabla, Nodos = nodos, Columnas = columnas });
            }
        }

        private static List<NodoEditorDto> CargarNodos(SqlConnection conn, string tabla)
        {
            var nodos = new Dictionary<int, NodoEditorDto>();
            using (var cmd = new SqlCommand($@"
                SELECT ID, ParentID, Nombre, Descripcion, Orden, Nivel,
                       FechaCreacion, FechaModificacion, UsuarioCreacion, TipoTarea
                FROM [{tabla}]
                ORDER BY Nivel, Orden", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int tipoTarea = reader["TipoTarea"] != DBNull.Value ? Convert.ToInt32(reader["TipoTarea"]) : 0;
                    var n = new NodoEditorDto
                    {
                        Id = Convert.ToInt32(reader["ID"]),
                        ParentId = reader["ParentID"] != DBNull.Value ? (int?)Convert.ToInt32(reader["ParentID"]) : null,
                        Nombre = reader["Nombre"].ToString(),
                        Descripcion = reader["Descripcion"] == DBNull.Value ? "" : reader["Descripcion"].ToString(),
                        Orden = Convert.ToInt32(reader["Orden"]),
                        Nivel = Convert.ToInt32(reader["Nivel"]),
                        FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                        FechaModificacion = reader["FechaModificacion"] == DBNull.Value
                            ? (DateTime?)null : Convert.ToDateTime(reader["FechaModificacion"]),
                        UsuarioCreacion = reader["UsuarioCreacion"] == DBNull.Value ? "" : reader["UsuarioCreacion"].ToString(),
                        TipoTarea = tipoTarea,
                        TipoTareaTexto = tipoTarea == 1 ? "Material" : tipoTarea == 2 ? "Mano de Obra" : "-"
                    };
                    nodos[n.Id] = n;
                }
            }

            using (var cmd = new SqlCommand($"SELECT NodoID, NombreColumna, Valor FROM [{tabla}_Columnas]", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int nodoId = Convert.ToInt32(reader["NodoID"]);
                    if (!nodos.TryGetValue(nodoId, out var n)) continue;
                    string col = reader["NombreColumna"].ToString();
                    n.Valores[col] = reader["Valor"] == DBNull.Value ? null : reader["Valor"].ToString();
                }
            }

            return nodos.Values.OrderBy(n => n.Nivel).ThenBy(n => n.Orden).ToList();
        }

        /// <summary>
        /// Igual que la carga clásica de FormEditorTreeList.CargarColumnasPersonalizadas:
        /// si la tabla de definiciones aún no existe para esta ruta, el árbol
        /// simplemente no tiene columnas personalizadas todavía.
        /// </summary>
        private static List<ColumnaDefDto> CargarColumnas(SqlConnection conn, string tabla)
        {
            var columnas = new List<ColumnaDefDto>();
            string sql = $@"
                SELECT Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato,
                       EsCalculada, TipoOperacion, ColumnaOrigen1, ColumnaOrigen2
                FROM [{tabla}_ColumnasDefinicion]
                ORDER BY ID";
            try
            {
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columnas.Add(new ColumnaDefDto
                        {
                            Nombre = reader["Nombre"].ToString(),
                            Titulo = reader["Titulo"].ToString(),
                            Ancho = Convert.ToInt32(reader["Ancho"]),
                            TipoDato = reader["TipoDato"].ToString(),
                            EsEditable = Convert.ToBoolean(reader["EsEditable"]),
                            Formato = reader["Formato"] == DBNull.Value ? null : reader["Formato"].ToString(),
                            EsCalculada = reader["EsCalculada"] != DBNull.Value && Convert.ToBoolean(reader["EsCalculada"]),
                            TipoOperacion = reader["TipoOperacion"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TipoOperacion"]),
                            ColumnaOrigen1 = reader["ColumnaOrigen1"] == DBNull.Value ? null : reader["ColumnaOrigen1"].ToString(),
                            ColumnaOrigen2 = reader["ColumnaOrigen2"] == DBNull.Value ? null : reader["ColumnaOrigen2"].ToString()
                        });
                    }
                }
            }
            catch (SqlException ex) when (ex.Message.IndexOf("Invalid object name", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Sin columnas personalizadas definidas todavía para esta ruta.
            }
            return columnas;
        }

        // ==================================================================
        // Escritura — guardado atómico de árbol + columnas · [ADMIN]
        // ==================================================================

        /// <summary>POST /api/editor-tareas/guardar · [ADMIN]</summary>
        [HttpPost, Route("guardar"), RequierePermiso("destajos.editar")]
        public IHttpActionResult Guardar([FromBody] GuardarArbolRequest req)
        {
            if (req == null) return BadRequest("Falta el cuerpo de la petición.");
            if (!EsAdmin(User)) return StatusCode(HttpStatusCode.Forbidden);
            string tabla = ValidarRuta(req.Ruta);
            if (tabla == null) return BadRequest("Ruta inválida.");

            var nodos = req.Nodos ?? new List<NodoEditorRequest>();
            var dup = nodos.GroupBy(n => n.Id).FirstOrDefault(g => g.Count() > 1);
            if (dup != null)
                return BadRequest($"Hay nodos con el mismo ID={dup.Key} en la petición. No se guardó para no corromper el árbol.");

            string usuario = User?.Identity?.Name ?? "Sistema";
            var remap = new Dictionary<int, int>();

            using (var conn = Db.Abrir())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    var idsExistentes = LeerIdsExistentes(conn, tx, tabla);

                    // Salvaguarda contra ediciones concurrentes de otra sesión: si un ID
                    // "nuevo" ya existe en BD (otra sesión insertó mientras tanto), se
                    // reasigna aquí ANTES de tocar la tabla.
                    ReasignarIdsEnConflicto(nodos, idsExistentes, remap);

                    var idsFinales = new HashSet<int>(nodos.Select(n => n.Id));

                    foreach (var nodo in OrdenarTopDown(nodos))
                    {
                        if (idsExistentes.Contains(nodo.Id))
                            ActualizarNodo(nodo, tabla, conn, tx);
                        else
                            InsertarNodo(nodo, tabla, conn, tx, usuario);
                    }

                    SincronizarColumnasValor(nodos, tabla, conn, tx);

                    foreach (var id in idsExistentes.Where(id => !idsFinales.Contains(id)))
                        EliminarNodo(id, tabla, conn, tx);

                    ReemplazarColumnasDefinicion(req.Columnas ?? new List<ColumnaDefDto>(), tabla, conn, tx);

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }

            return Ok(new GuardarArbolResponseDto
            {
                Ok = true,
                Mensaje = "Guardado correctamente.",
                IdsReasignados = remap
            });
        }

        private static HashSet<int> LeerIdsExistentes(SqlConnection conn, SqlTransaction tx, string tabla)
        {
            var ids = new HashSet<int>();
            using (var cmd = new SqlCommand($"SELECT ID FROM [{tabla}]", conn, tx))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read()) ids.Add(reader.GetInt32(0));
            return ids;
        }

        /// <summary>Reasigna el ID de los nodos NUEVOS cuyo número ya exista en BD (otra sesión insertó mientras tanto); reapunta el ParentId de sus hijos. Puerto de FormEditorTreeList.ReasignarIdsEnConflicto.</summary>
        private static void ReasignarIdsEnConflicto(List<NodoEditorRequest> nodos, HashSet<int> idsExistentes, Dictionary<int, int> remapSalida)
        {
            var ocupados = new HashSet<int>(idsExistentes);
            foreach (var n in nodos) ocupados.Add(n.Id);
            int siguiente = ocupados.Count > 0 ? ocupados.Max() + 1 : 1;

            var remap = new Dictionary<int, int>();
            foreach (var n in nodos)
            {
                if (n.EsNuevo && idsExistentes.Contains(n.Id))
                {
                    int nuevo = siguiente++;
                    while (ocupados.Contains(nuevo)) nuevo = siguiente++;
                    ocupados.Add(nuevo);
                    remap[n.Id] = nuevo;
                    n.Id = nuevo;
                }
            }

            if (remap.Count == 0) return;

            foreach (var n in nodos)
                if (n.ParentId.HasValue && remap.TryGetValue(n.ParentId.Value, out int nuevoPadre))
                    n.ParentId = nuevoPadre;

            foreach (var kv in remap) remapSalida[kv.Key] = kv.Value;
        }

        /// <summary>Ordena para que un padre siempre se procese antes que sus hijos (no hay FK en ParentID, pero mantiene la intención del árbol si algo falla a la mitad).</summary>
        private static List<NodoEditorRequest> OrdenarTopDown(List<NodoEditorRequest> nodos)
        {
            var porId = nodos.ToDictionary(n => n.Id);
            var porPadre = nodos.GroupBy(n => n.ParentId ?? -1).ToDictionary(g => g.Key, g => g.ToList());
            var resultado = new List<NodoEditorRequest>(nodos.Count);
            var visitados = new HashSet<int>();

            void Visitar(NodoEditorRequest n)
            {
                if (!visitados.Add(n.Id)) return;
                resultado.Add(n);
                if (porPadre.TryGetValue(n.Id, out var hijos))
                    foreach (var h in hijos) Visitar(h);
            }

            foreach (var raiz in nodos.Where(n => !n.ParentId.HasValue || !porId.ContainsKey(n.ParentId.Value)))
                Visitar(raiz);
            foreach (var n in nodos) Visitar(n); // red de seguridad para cualquier nodo no alcanzado

            return resultado;
        }

        private static void InsertarNodo(NodoEditorRequest nodo, string tabla, SqlConnection conn, SqlTransaction tx, string usuario)
        {
            string sql = $@"
                INSERT INTO [{tabla}]
                    (ID, ParentID, Nombre, Descripcion, Orden, Nivel, FechaCreacion, FechaModificacion, UsuarioCreacion, TipoTarea)
                VALUES
                    (@id, @parentId, @nombre, @descripcion, @orden, @nivel, @fechaCreacion, NULL, @usuarioCreacion, @tipoTarea)";
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", nodo.Id);
                cmd.Parameters.AddWithValue("@parentId", (object)nodo.ParentId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@nombre", nodo.Nombre ?? string.Empty);
                cmd.Parameters.AddWithValue("@descripcion", (object)nodo.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@orden", nodo.Orden);
                cmd.Parameters.AddWithValue("@nivel", nodo.Nivel);
                cmd.Parameters.AddWithValue("@fechaCreacion", DateTime.Now);
                cmd.Parameters.AddWithValue("@usuarioCreacion", usuario);
                cmd.Parameters.AddWithValue("@tipoTarea", nodo.TipoTarea);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>No toca UsuarioCreacion: es el creador original, no quien edita después.</summary>
        private static void ActualizarNodo(NodoEditorRequest nodo, string tabla, SqlConnection conn, SqlTransaction tx)
        {
            string sql = $@"
                UPDATE [{tabla}]
                   SET ParentID = @parentId, Nombre = @nombre, Descripcion = @descripcion,
                       Orden = @orden, Nivel = @nivel, FechaModificacion = @fechaModificacion, TipoTarea = @tipoTarea
                 WHERE ID = @id";
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", nodo.Id);
                cmd.Parameters.AddWithValue("@parentId", (object)nodo.ParentId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@nombre", nodo.Nombre ?? string.Empty);
                cmd.Parameters.AddWithValue("@descripcion", (object)nodo.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@orden", nodo.Orden);
                cmd.Parameters.AddWithValue("@nivel", nodo.Nivel);
                cmd.Parameters.AddWithValue("@fechaModificacion", DateTime.Now);
                cmd.Parameters.AddWithValue("@tipoTarea", nodo.TipoTarea);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>El FK de "{tabla}_Columnas" es ON DELETE CASCADE: limpia solo las columnas de este nodo.</summary>
        private static void EliminarNodo(int id, string tabla, SqlConnection conn, SqlTransaction tx)
        {
            using (var cmd = new SqlCommand($"DELETE FROM [{tabla}] WHERE ID = @id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Resincroniza en bloque los valores de columnas personalizadas (Precio/Unidad/etc.) de todos los nodos.</summary>
        private static void SincronizarColumnasValor(List<NodoEditorRequest> nodos, string tabla, SqlConnection conn, SqlTransaction tx)
        {
            using (var cmdDel = new SqlCommand($"DELETE FROM [{tabla}_Columnas]", conn, tx))
                cmdDel.ExecuteNonQuery();

            var filas = new List<(int NodoId, string Nombre, string Valor)>();
            foreach (var nodo in nodos)
                if (nodo.Valores != null)
                    foreach (var kv in nodo.Valores)
                        if (!string.IsNullOrEmpty(kv.Value))
                            filas.Add((nodo.Id, kv.Key, kv.Value));

            const int filasPorLote = 500; // 3 parámetros/fila; margen bajo el límite de 2100 de SQL Server.
            for (int inicio = 0; inicio < filas.Count; inicio += filasPorLote)
            {
                int conteo = Math.Min(filasPorLote, filas.Count - inicio);
                var sb = new StringBuilder();
                sb.Append($"INSERT INTO [{tabla}_Columnas] (NodoID, NombreColumna, Valor) VALUES ");
                using (var cmd = new SqlCommand { Connection = conn, Transaction = tx })
                {
                    for (int j = 0; j < conteo; j++)
                    {
                        var fila = filas[inicio + j];
                        if (j > 0) sb.Append(',');
                        sb.Append($"(@n{j},@c{j},@v{j})");
                        cmd.Parameters.AddWithValue($"@n{j}", fila.NodoId);
                        cmd.Parameters.AddWithValue($"@c{j}", fila.Nombre);
                        cmd.Parameters.Add($"@v{j}", SqlDbType.NVarChar, -1).Value = (object)fila.Valor ?? DBNull.Value;
                    }
                    cmd.CommandText = sb.ToString();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>Reemplazo transaccional (a diferencia del cliente clásico, que hacía DELETE+INSERT sin transacción): el orden de inserción = el orden que se muestra, no hay columna Orden explícita.</summary>
        private static void ReemplazarColumnasDefinicion(List<ColumnaDefDto> columnas, string tabla, SqlConnection conn, SqlTransaction tx)
        {
            using (var cmdDel = new SqlCommand($"DELETE FROM [{tabla}_ColumnasDefinicion]", conn, tx))
                cmdDel.ExecuteNonQuery();

            foreach (var col in columnas)
            {
                string sql = $@"
                    INSERT INTO [{tabla}_ColumnasDefinicion]
                        (Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato, EsCalculada, TipoOperacion, ColumnaOrigen1, ColumnaOrigen2)
                    VALUES
                        (@nombre, @titulo, @ancho, @tipoDato, @esEditable, @formato, @esCalculada, @tipoOperacion, @origen1, @origen2)";
                using (var cmd = new SqlCommand(sql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@nombre", col.Nombre ?? "");
                    cmd.Parameters.AddWithValue("@titulo", col.Titulo ?? "");
                    cmd.Parameters.AddWithValue("@ancho", col.Ancho);
                    cmd.Parameters.AddWithValue("@tipoDato", col.TipoDato ?? "System.String");
                    cmd.Parameters.AddWithValue("@esEditable", col.EsEditable);
                    cmd.Parameters.AddWithValue("@formato", (object)col.Formato ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@esCalculada", col.EsCalculada);
                    cmd.Parameters.AddWithValue("@tipoOperacion", col.TipoOperacion);
                    cmd.Parameters.AddWithValue("@origen1", (object)col.ColumnaOrigen1 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@origen2", (object)col.ColumnaOrigen2 ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
