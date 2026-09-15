using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Calandria.Api.Auth;
using Calandria.Api.Data;
using Calandria.Api.Models;
using Calandria.Api.Services;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Endpoints del módulo de Trabajadores: alta/edición/baja de trabajadores
    /// (FormRegistrarTrabajador), gestión de cuadrillas (FormAsignarCuadrilla) y
    /// perfil con historial de recibos (FormPerfilTrabajador). La lectura de
    /// cuadrillas/miembros existentes reutiliza los endpoints ya expuestos por
    /// NominaController (api/nomina/cuadrillas, .../miembros); aquí sólo vive lo
    /// que ese controlador no necesita: el CRUD de TRABAJADORES y el guardado de
    /// cuadrillas completas.
    /// </summary>
    [RoutePrefix("api/trabajadores"), RequierePermiso("trabajadores.ver")]
    public class TrabajadoresController : ApiController
    {
        private static readonly string[] TiposDocumento = { "curp", "rfc", "ine", "nss" };

        private static string ColumnaDocumento(string tipo)
        {
            switch ((tipo ?? "").ToLowerInvariant())
            {
                case "curp": return "PDF_CURP";
                case "rfc": return "PDF_RFC";
                case "ine": return "PDF_INE";
                case "nss": return "PDF_NSS";
                default: return null;
            }
        }

        // ==================================================================
        // Tablas (idempotente, igual que hacían los formularios clásicos)
        // ==================================================================

        private static void EnsureTablaTrabajadores(SqlConnection conn)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.TRABAJADORES') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.TRABAJADORES (
        IdTrabajador INT IDENTITY(1,1) PRIMARY KEY,
        ClaveTrabajador NVARCHAR(20) NOT NULL UNIQUE,
        Nombre NVARCHAR(200) NOT NULL,
        Nacionalidad NVARCHAR(50) NOT NULL,
        FechaNacimiento DATE NULL,
        CURP NVARCHAR(18) NULL,
        RFC NVARCHAR(13) NULL,
        INE NVARCHAR(20) NULL,
        NSS NVARCHAR(11) NULL,
        PDF_CURP VARBINARY(MAX) NULL,
        PDF_RFC VARBINARY(MAX) NULL,
        PDF_INE VARBINARY(MAX) NULL,
        PDF_NSS VARBINARY(MAX) NULL,
        Foto VARBINARY(MAX) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT(GETDATE())
    );
END
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID(N'dbo.TRABAJADORES') AND name = 'Foto')
BEGIN
    ALTER TABLE dbo.TRABAJADORES ADD Foto VARBINARY(MAX) NULL;
END";
            using (var cmd = new SqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        private static void EnsureMiembrosCuadrilla(SqlConnection conn)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.MiembrosCuadrilla') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.MiembrosCuadrilla (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CodigoCuadrilla NVARCHAR(20) NOT NULL,
        IdTrabajador INT NULL,
        Nombre NVARCHAR(200) NOT NULL,
        Rol NVARCHAR(50) NOT NULL,
        EsJefe BIT NOT NULL DEFAULT(0),
        Telefono NVARCHAR(50) NULL,
        Foto VARBINARY(MAX) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT(GETDATE())
    );
END
ELSE IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'IdTrabajador' AND Object_ID = Object_ID(N'dbo.MiembrosCuadrilla'))
BEGIN
    ALTER TABLE dbo.MiembrosCuadrilla ADD IdTrabajador INT NULL;
END";
            using (var cmd = new SqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        private static void EnsureTablaRecibos(SqlConnection conn)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RecibosNomina')
BEGIN
    CREATE TABLE RecibosNomina (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdTrabajador INT NULL,
        NombreTrabajador NVARCHAR(200) NOT NULL,
        Rol NVARCHAR(80) NULL,
        CodigoCuadrilla NVARCHAR(20) NULL,
        Concepto NVARCHAR(MAX) NULL,
        Monto DECIMAL(18,2) NOT NULL,
        FechaRecibo DATE NOT NULL,
        PeriodoDesde DATE NULL,
        PeriodoHasta DATE NULL,
        TotalCuadrilla DECIMAL(18,2) NULL,
        Pdf VARBINARY(MAX) NULL,
        Usuario NVARCHAR(120) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
    );
END";
            using (var cmd = new SqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        // ==================================================================
        // Lectura
        // ==================================================================

        /// <summary>GET /api/trabajadores → lista completa, para combos.</summary>
        [HttpGet, Route("")]
        public IHttpActionResult Lista()
        {
            var lista = new List<TrabajadorOpcionDto>();
            using (var conn = Db.Abrir())
            {
                EnsureTablaTrabajadores(conn);
                using (var cmd = new SqlCommand(@"
                    SELECT IdTrabajador, ClaveTrabajador, Nombre, Nacionalidad,
                           FechaNacimiento, CURP, RFC, INE, NSS
                    FROM TRABAJADORES
                    ORDER BY Nombre", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new TrabajadorOpcionDto
                        {
                            Id = Convert.ToInt32(reader["IdTrabajador"]),
                            Clave = reader["ClaveTrabajador"].ToString(),
                            Nombre = reader["Nombre"].ToString(),
                            Nacionalidad = reader["Nacionalidad"].ToString(),
                            FechaNacimiento = reader["FechaNacimiento"] == DBNull.Value
                                ? (DateTime?)null : Convert.ToDateTime(reader["FechaNacimiento"]),
                            Curp = reader["CURP"] == DBNull.Value ? "" : reader["CURP"].ToString(),
                            Rfc = reader["RFC"] == DBNull.Value ? "" : reader["RFC"].ToString(),
                            Ine = reader["INE"] == DBNull.Value ? "" : reader["INE"].ToString(),
                            Nss = reader["NSS"] == DBNull.Value ? "" : reader["NSS"].ToString()
                        });
                    }
                }
            }
            return Ok(lista);
        }

        /// <summary>GET /api/trabajadores/clave-nueva → matrícula propuesta para un alta.</summary>
        [HttpGet, Route("clave-nueva")]
        public IHttpActionResult ClaveNueva()
        {
            using (var conn = Db.Abrir())
            {
                EnsureTablaTrabajadores(conn);
                return Ok(new ClaveNuevaDto { Clave = GenerarClaveUnica(conn) });
            }
        }

        /// <summary>GET /api/trabajadores/{id} → ficha completa (foto en base64, PDFs aparte).</summary>
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult Detalle(int id)
        {
            using (var conn = Db.Abrir())
            {
                EnsureTablaTrabajadores(conn);
                var dto = ObtenerDetalle(conn, id);
                if (dto == null) return NotFound();
                return Ok(dto);
            }
        }

        private static TrabajadorDetalleDto ObtenerDetalle(SqlConnection conn, int id)
        {
            TrabajadorDetalleDto dto = null;
            using (var cmd = new SqlCommand("SELECT * FROM TRABAJADORES WHERE IdTrabajador = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return null;

                    byte[] foto = LeerBytes(reader, "Foto");
                    byte[] curp = LeerBytes(reader, "PDF_CURP");
                    byte[] rfc = LeerBytes(reader, "PDF_RFC");
                    byte[] ine = LeerBytes(reader, "PDF_INE");
                    byte[] nss = LeerBytes(reader, "PDF_NSS");

                    dto = new TrabajadorDetalleDto
                    {
                        Id = id,
                        Clave = reader["ClaveTrabajador"].ToString(),
                        Nombre = reader["Nombre"].ToString(),
                        Nacionalidad = reader["Nacionalidad"].ToString(),
                        FechaNacimiento = reader["FechaNacimiento"] == DBNull.Value
                            ? (DateTime?)null : Convert.ToDateTime(reader["FechaNacimiento"]),
                        Curp = reader["CURP"] == DBNull.Value ? "" : reader["CURP"].ToString(),
                        Rfc = reader["RFC"] == DBNull.Value ? "" : reader["RFC"].ToString(),
                        Ine = reader["INE"] == DBNull.Value ? "" : reader["INE"].ToString(),
                        Nss = reader["NSS"] == DBNull.Value ? "" : reader["NSS"].ToString(),
                        FotoBase64 = foto != null ? Convert.ToBase64String(foto) : null,
                        TieneCurp = curp != null, CurpKb = KbDe(curp),
                        TieneRfc = rfc != null, RfcKb = KbDe(rfc),
                        TieneIne = ine != null, IneKb = KbDe(ine),
                        TieneNss = nss != null, NssKb = KbDe(nss)
                    };
                }
            }

            dto.RolCuadrilla = ObtenerRolYCuadrilla(conn, id);
            return dto;
        }

        private static int KbDe(byte[] data) => data == null || data.Length == 0 ? 0 : Math.Max(1, data.Length / 1024);

        private static string ObtenerRolYCuadrilla(SqlConnection conn, int idTrabajador)
        {
            using (var cmd = new SqlCommand(@"
                SELECT TOP 1 Rol, CodigoCuadrilla, EsJefe
                FROM MiembrosCuadrilla
                WHERE IdTrabajador = @id
                ORDER BY EsJefe DESC", conn))
            {
                cmd.Parameters.AddWithValue("@id", idTrabajador);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string rol = (reader["Rol"] ?? "-").ToString();
                        string codigo = (reader["CodigoCuadrilla"] ?? "-").ToString();
                        bool jefe = Convert.ToBoolean(reader["EsJefe"]);
                        return rol + (jefe ? " (jefe) " : " ") + " · " + codigo;
                    }
                }
            }
            return "(sin cuadrilla asignada)";
        }

        private static byte[] LeerBytes(SqlDataReader reader, string columna)
        {
            for (int i = 0; i < reader.FieldCount; i++)
                if (string.Equals(reader.GetName(i), columna, StringComparison.OrdinalIgnoreCase))
                    return reader[columna] == DBNull.Value ? null : (byte[])reader[columna];
            return null;
        }

        /// <summary>GET /api/trabajadores/{id}/documento/{tipo} → PDF binario (curp|rfc|ine|nss).</summary>
        [HttpGet, Route("{id:int}/documento/{tipo}")]
        public HttpResponseMessage Documento(int id, string tipo)
        {
            string columna = ColumnaDocumento(tipo);
            if (columna == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            byte[] bytes = null;
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand($"SELECT {columna} FROM TRABAJADORES WHERE IdTrabajador = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                var obj = cmd.ExecuteScalar();
                if (obj != null && obj != DBNull.Value) bytes = (byte[])obj;
            }

            if (bytes == null || bytes.Length == 0) return Request.CreateResponse(HttpStatusCode.NotFound);
            bytes = CifradoDocumentos.Descifrar(bytes);

            var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            return resp;
        }

        /// <summary>GET /api/trabajadores/{id}/recibos → historial de nómina del trabajador.</summary>
        [HttpGet, Route("{id:int}/recibos")]
        public IHttpActionResult Recibos(int id)
        {
            var lista = new List<ReciboNominaDto>();
            using (var conn = Db.Abrir())
            {
                EnsureTablaRecibos(conn);

                string nombre = "";
                using (var cmdN = new SqlCommand("SELECT Nombre FROM TRABAJADORES WHERE IdTrabajador = @id", conn))
                {
                    cmdN.Parameters.AddWithValue("@id", id);
                    var obj = cmdN.ExecuteScalar();
                    if (obj == null) return NotFound();
                    nombre = obj.ToString();
                }

                using (var cmd = new SqlCommand(@"
                    SELECT Id, IdTrabajador, NombreTrabajador, Rol, CodigoCuadrilla, Concepto, Monto,
                           FechaRecibo, PeriodoDesde, PeriodoHasta, TotalCuadrilla,
                           CASE WHEN Pdf IS NULL THEN 0 ELSE 1 END AS TienePdf
                    FROM RecibosNomina
                    WHERE IdTrabajador = @id
                       OR (IdTrabajador IS NULL AND NombreTrabajador = @nombre)
                    ORDER BY FechaRecibo DESC, Id DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@nombre", nombre ?? "");
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new ReciboNominaDto
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                IdTrabajador = reader["IdTrabajador"] == DBNull.Value
                                    ? (int?)null : Convert.ToInt32(reader["IdTrabajador"]),
                                NombreTrabajador = reader["NombreTrabajador"]?.ToString() ?? "",
                                Rol = reader["Rol"] == DBNull.Value ? "" : reader["Rol"].ToString(),
                                CodigoCuadrilla = reader["CodigoCuadrilla"] == DBNull.Value ? "" : reader["CodigoCuadrilla"].ToString(),
                                Concepto = reader["Concepto"] == DBNull.Value ? "" : reader["Concepto"].ToString(),
                                Monto = reader["Monto"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["Monto"]),
                                FechaRecibo = Convert.ToDateTime(reader["FechaRecibo"]),
                                PeriodoDesde = reader["PeriodoDesde"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PeriodoDesde"]),
                                PeriodoHasta = reader["PeriodoHasta"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PeriodoHasta"]),
                                TotalCuadrilla = reader["TotalCuadrilla"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["TotalCuadrilla"]),
                                TienePdf = Convert.ToInt32(reader["TienePdf"]) == 1
                            });
                        }
                    }
                }
            }
            return Ok(lista);
        }

        // ==================================================================
        // Escritura
        // ==================================================================

        /// <summary>POST /api/trabajadores → crea o actualiza (Id nulo = alta).</summary>
        [HttpPost, Route(""), RequierePermiso("trabajadores.editar")]
        public IHttpActionResult Guardar([FromBody] GuardarTrabajadorRequest req)
        {
            if (req == null) return BadRequest("Cuerpo vacío.");
            if (string.IsNullOrWhiteSpace(req.Nombre)) return BadRequest("El nombre es obligatorio.");
            if (!string.IsNullOrEmpty(req.Curp) && req.Curp.Length != 18) return BadRequest("El CURP debe tener 18 caracteres.");
            if (!string.IsNullOrEmpty(req.Rfc) && (req.Rfc.Length < 12 || req.Rfc.Length > 13)) return BadRequest("El RFC debe tener 12 o 13 caracteres.");
            if (!string.IsNullOrEmpty(req.Nss) && req.Nss.Length != 11) return BadRequest("El NSS debe tener 11 dígitos.");

            using (var conn = Db.Abrir())
            {
                EnsureTablaTrabajadores(conn);

                bool esNuevo = !req.Id.HasValue;
                if (!esNuevo)
                {
                    using (var cmd = new SqlCommand("SELECT 1 FROM TRABAJADORES WHERE IdTrabajador = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", req.Id.Value);
                        if (cmd.ExecuteScalar() == null) return NotFound();
                    }
                }

                byte[] foto = ResolverBinario(req.FotoBase64, esNuevo ? null : LeerBinarioActual(conn, req.Id.Value, "Foto"));
                byte[] curp = ResolverBinario(req.PdfCurpBase64, esNuevo ? null : LeerBinarioActual(conn, req.Id.Value, "PDF_CURP"), cifrar: true);
                byte[] rfc = ResolverBinario(req.PdfRfcBase64, esNuevo ? null : LeerBinarioActual(conn, req.Id.Value, "PDF_RFC"), cifrar: true);
                byte[] ine = ResolverBinario(req.PdfIneBase64, esNuevo ? null : LeerBinarioActual(conn, req.Id.Value, "PDF_INE"), cifrar: true);
                byte[] nss = ResolverBinario(req.PdfNssBase64, esNuevo ? null : LeerBinarioActual(conn, req.Id.Value, "PDF_NSS"), cifrar: true);

                int id;
                try
                {
                    if (esNuevo)
                    {
                        string clave = GenerarClaveUnica(conn);
                        using (var cmd = new SqlCommand(@"
                            INSERT INTO TRABAJADORES
                            (ClaveTrabajador, Nombre, Nacionalidad, FechaNacimiento,
                             CURP, RFC, INE, NSS, PDF_CURP, PDF_RFC, PDF_INE, PDF_NSS, Foto)
                            OUTPUT INSERTED.IdTrabajador
                            VALUES (@clave, @nombre, @nacionalidad, @fechaNac,
                                    @curp, @rfc, @ine, @nss, @pdfCurp, @pdfRfc, @pdfIne, @pdfNss, @foto)", conn))
                        {
                            cmd.Parameters.AddWithValue("@clave", clave);
                            AgregarComunes(cmd, req);
                            AgregarBinario(cmd, "@pdfCurp", curp);
                            AgregarBinario(cmd, "@pdfRfc", rfc);
                            AgregarBinario(cmd, "@pdfIne", ine);
                            AgregarBinario(cmd, "@pdfNss", nss);
                            AgregarBinario(cmd, "@foto", foto);
                            id = (int)cmd.ExecuteScalar();
                        }
                    }
                    else
                    {
                        id = req.Id.Value;
                        using (var cmd = new SqlCommand(@"
                            UPDATE TRABAJADORES SET
                                Nombre = @nombre, Nacionalidad = @nacionalidad, FechaNacimiento = @fechaNac,
                                CURP = @curp, RFC = @rfc, INE = @ine, NSS = @nss,
                                PDF_CURP = @pdfCurp, PDF_RFC = @pdfRfc, PDF_INE = @pdfIne, PDF_NSS = @pdfNss, Foto = @foto
                            WHERE IdTrabajador = @id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            AgregarComunes(cmd, req);
                            AgregarBinario(cmd, "@pdfCurp", curp);
                            AgregarBinario(cmd, "@pdfRfc", rfc);
                            AgregarBinario(cmd, "@pdfIne", ine);
                            AgregarBinario(cmd, "@pdfNss", nss);
                            AgregarBinario(cmd, "@foto", foto);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (SqlException ex) when (ex.Number == 2627)
                {
                    return Content(HttpStatusCode.Conflict, "Ya existe un trabajador con esa matrícula.");
                }

                return Ok(ObtenerDetalle(conn, id));
            }
        }

        private static byte[] LeerBinarioActual(SqlConnection conn, int id, string columna)
        {
            using (var cmd = new SqlCommand($"SELECT {columna} FROM TRABAJADORES WHERE IdTrabajador = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                var obj = cmd.ExecuteScalar();
                return obj == null || obj == DBNull.Value ? null : (byte[])obj;
            }
        }

        /// <summary>
        /// Tri-estado: null = conserva lo actual (ya cifrado si aplica, no se toca),
        /// "" = lo quita, base64 = lo reemplaza (recién decodificado, en claro).
        /// cifrar=true para las columnas PDF_* (documentos de identidad); Foto no la cifra.
        /// </summary>
        private static byte[] ResolverBinario(string base64, byte[] actual, bool cifrar = false)
        {
            if (base64 == null) return actual;
            if (base64.Length == 0) return null;
            byte[] bytes = Convert.FromBase64String(base64);
            return cifrar ? CifradoDocumentos.Cifrar(bytes) : bytes;
        }

        private static void AgregarComunes(SqlCommand cmd, GuardarTrabajadorRequest req)
        {
            cmd.Parameters.AddWithValue("@nombre", req.Nombre.Trim());
            cmd.Parameters.AddWithValue("@nacionalidad", string.IsNullOrWhiteSpace(req.Nacionalidad) ? "Mexicana" : req.Nacionalidad.Trim());
            cmd.Parameters.AddWithValue("@fechaNac", (object)req.FechaNacimiento ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@curp", ParametroOpcional(req.Curp));
            cmd.Parameters.AddWithValue("@rfc", ParametroOpcional(req.Rfc));
            cmd.Parameters.AddWithValue("@ine", ParametroOpcional(req.Ine));
            cmd.Parameters.AddWithValue("@nss", ParametroOpcional(req.Nss));
        }

        private static object ParametroOpcional(string texto) =>
            string.IsNullOrWhiteSpace(texto) ? (object)DBNull.Value : texto.Trim();

        private static void AgregarBinario(SqlCommand cmd, string nombre, byte[] data)
        {
            var p = cmd.Parameters.Add(nombre, SqlDbType.VarBinary, -1);
            p.Value = (data != null && data.Length > 0) ? (object)data : DBNull.Value;
        }

        /// <summary>POST /api/trabajadores/{id}/eliminar</summary>
        [HttpPost, Route("{id:int}/eliminar"), RequierePermiso("trabajadores.editar")]
        public IHttpActionResult Eliminar(int id)
        {
            using (var conn = Db.Abrir())
            using (var cmd = new SqlCommand("DELETE FROM TRABAJADORES WHERE IdTrabajador = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                int filas = cmd.ExecuteNonQuery();
                return Ok(filas);
            }
        }

        private static string GenerarClaveUnica(SqlConnection conn)
        {
            var rnd = new Random();
            while (true)
            {
                string clave = "TR-" + rnd.Next(0, 1000000).ToString("D6");
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM TRABAJADORES WHERE ClaveTrabajador = @c", conn))
                {
                    cmd.Parameters.AddWithValue("@c", clave);
                    if ((int)cmd.ExecuteScalar() == 0) return clave;
                }
            }
        }

        // ==================================================================
        // Cuadrillas — la lectura (códigos existentes / miembros de una
        // cuadrilla) ya la sirve NominaController; aquí sólo el guardado.
        // ==================================================================

        /// <summary>POST /api/trabajadores/cuadrillas → crea o reemplaza los miembros de una cuadrilla.</summary>
        [HttpPost, Route("cuadrillas"), RequierePermiso("trabajadores.editar")]
        public IHttpActionResult GuardarCuadrilla([FromBody] GuardarCuadrillaRequest req)
        {
            if (req == null) return BadRequest("Cuerpo vacío.");
            if (req.Miembros == null || req.Miembros.Count == 0) return BadRequest("Agrega al menos un miembro.");

            int jefes = 0;
            foreach (var m in req.Miembros) if (m.EsJefe) jefes++;
            if (jefes != 1) return BadRequest("Debe haber exactamente un miembro marcado como jefe de cuadrilla.");

            using (var conn = Db.Abrir())
            {
                EnsureMiembrosCuadrilla(conn);

                bool esNueva = string.IsNullOrEmpty(req.Codigo);
                string codigoFinal = esNueva ? GenerarCodigoCuadrillaUnico(conn) : req.Codigo;

                using (var tx = conn.BeginTransaction())
                {
                    if (!esNueva)
                    {
                        using (var cmdDel = new SqlCommand("DELETE FROM MiembrosCuadrilla WHERE CodigoCuadrilla = @codigo", conn, tx))
                        {
                            cmdDel.Parameters.AddWithValue("@codigo", codigoFinal);
                            cmdDel.ExecuteNonQuery();
                        }
                    }

                    foreach (var m in req.Miembros)
                    {
                        using (var cmdIns = new SqlCommand(@"
                            INSERT INTO MiembrosCuadrilla
                            (CodigoCuadrilla, IdTrabajador, Nombre, Rol, EsJefe, Telefono, Foto)
                            VALUES (@codigo, @idTrab, @nombre, @rol, @esJefe, @tel, NULL)", conn, tx))
                        {
                            cmdIns.Parameters.AddWithValue("@codigo", codigoFinal);
                            cmdIns.Parameters.AddWithValue("@idTrab", (object)m.IdTrabajador ?? DBNull.Value);
                            cmdIns.Parameters.AddWithValue("@nombre", m.Nombre ?? "");
                            cmdIns.Parameters.AddWithValue("@rol", m.Rol ?? "");
                            cmdIns.Parameters.AddWithValue("@esJefe", m.EsJefe);
                            cmdIns.Parameters.AddWithValue("@tel", string.IsNullOrWhiteSpace(m.Telefono) ? (object)DBNull.Value : m.Telefono);
                            cmdIns.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }

                return Ok(new CuadrillaGuardadaDto { Codigo = codigoFinal });
            }
        }

        private static string GenerarCodigoCuadrillaUnico(SqlConnection conn)
        {
            var rnd = new Random();
            while (true)
            {
                string codigo = "CD-" + rnd.Next(0, 1000000).ToString("D6");
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM MiembrosCuadrilla WHERE CodigoCuadrilla = @codigo", conn))
                {
                    cmd.Parameters.AddWithValue("@codigo", codigo);
                    if ((int)cmd.ExecuteScalar() == 0) return codigo;
                }
            }
        }
    }
}
