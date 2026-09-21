using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;
using Calandria.Api.Auth;
using Calandria.Api.Data;

namespace Calandria.Api.Controllers
{
    /// <summary>
    /// Inversión general de la obra: presupuesto ("Importe") y lo ya ejercido
    /// ("Aplicado") por categoría, más la bitácora de cómo llegó a ese número.
    ///
    /// El Aplicado se mueve de dos formas: registrando una acción (concepto +
    /// cantidad, que se suma) o ajustando el campo a mano. Las dos quedan en
    /// InversionMovimientos con usuario y fecha, igual que los cambios de
    /// presupuesto, así que el historial siempre explica el total.
    ///
    /// Las categorías son fijas (Categorias) y viven en la BD de la obra activa.
    /// Las tablas se crean y se siembran solas en la primera llamada — mismo
    /// patrón que ProveedoresController.EnsureTabla — para no depender de correr
    /// un script en cada BD de obra ni de tocar ObraPlantilla.sql.
    /// </summary>
    [RoutePrefix("api/inversion"), RequierePermiso("sistema.administrador")]
    public class InversionController : ApiController
    {
        /// <summary>Categorías fijas con su presupuesto y aplicado iniciales, en orden de captura.</summary>
        private static readonly Tuple<string, decimal, decimal>[] Categorias =
        {
            Tuple.Create("Terreno",        45000000m, 2000000m),
            Tuple.Create("BIM",             1000000m,  723064.49m),
            Tuple.Create("Operación",      15000000m, 4169687.36m),
            Tuple.Create("Permisos",        9500000m, 6198911.28m),
            Tuple.Create("Urba Exterior",   4500000m, 2381043.74m),
            Tuple.Create("Urba Interior",  14000000m, 1020568.55m),
            Tuple.Create("Electricidad",    8000000m,  700096.14m),
            Tuple.Create("Acceso",          2500000m,       0m),
            Tuple.Create("Bardas",          4000000m,       0m),
            Tuple.Create("Parque",          2000000m,       0m),
            Tuple.Create("Mobiliario",      1000000m,  946088.30m),
            Tuple.Create("Vivienda",              0m,       0m),
            Tuple.Create("Otros",            500000m,  439013.54m)
        };

        // Tipos de renglón del historial. Se guardan sin acentos: son claves, no texto de UI.
        private const string TipoAccion = "Accion";
        private const string TipoAjusteImporte = "AjusteImporte";
        private const string TipoAjusteAplicado = "AjusteAplicado";

        public class RenglonInversion
        {
            public string Categoria { get; set; }
            public int Orden { get; set; }
            public decimal Importe { get; set; }
            public decimal Aplicado { get; set; }
            public DateTime? FechaActualizacion { get; set; }
            public string UsuarioModificacion { get; set; }
        }

        public class MovimientoInversion
        {
            public int Id { get; set; }
            public string Categoria { get; set; }
            public string Tipo { get; set; }
            public string Concepto { get; set; }
            public decimal Cantidad { get; set; }
            public decimal? Anterior { get; set; }
            public string Usuario { get; set; }
            public DateTime Fecha { get; set; }
        }

        public class GuardarInversionDto
        {
            public string Categoria { get; set; }
            public decimal Importe { get; set; }
            public decimal Aplicado { get; set; }
        }

        public class AccionInversionDto
        {
            public string Categoria { get; set; }
            public string Concepto { get; set; }
            public decimal Cantidad { get; set; }
        }

        /// <summary>GET /api/inversion — las 13 categorías con importe y aplicado.</summary>
        [HttpGet, Route("")]
        public IHttpActionResult Listar()
        {
            var lista = new List<RenglonInversion>();
            using (var conn = Db.Abrir())
            {
                EnsureTablas(conn);
                using (var cmd = new SqlCommand(
                    "SELECT Categoria, Orden, Importe, Aplicado, FechaActualizacion, UsuarioModificacion " +
                    "FROM dbo.InversionObra ORDER BY Orden", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        lista.Add(new RenglonInversion
                        {
                            Categoria = (string)reader["Categoria"],
                            Orden = (int)reader["Orden"],
                            Importe = (decimal)reader["Importe"],
                            Aplicado = (decimal)reader["Aplicado"],
                            FechaActualizacion = reader["FechaActualizacion"] as DateTime?,
                            UsuarioModificacion = reader["UsuarioModificacion"] as string
                        });
                }
            }
            return Ok(lista);
        }

        /// <summary>
        /// POST /api/inversion/guardar — ajuste directo de los montos de una
        /// categoría. Cada monto que cambie deja su renglón en el historial.
        /// </summary>
        [HttpPost, Route("guardar")]
        public IHttpActionResult Guardar(GuardarInversionDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Categoria))
                return BadRequest("Falta la categoría.");
            if (dto.Importe < 0 || dto.Aplicado < 0)
                return BadRequest("Los montos no pueden ser negativos.");

            using (var conn = Db.Abrir())
            {
                EnsureTablas(conn);
                using (var tx = conn.BeginTransaction())
                {
                    decimal importeAnterior, aplicadoAnterior;
                    using (var cmd = new SqlCommand(
                        "SELECT Importe, Aplicado FROM dbo.InversionObra WITH (UPDLOCK) WHERE Categoria = @categoria", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@categoria", dto.Categoria);
                        using (var reader = cmd.ExecuteReader())
                        {
                            // Las categorías son fijas: si no está, el nombre no es de la lista.
                            if (!reader.Read()) return BadRequest("Categoría desconocida: " + dto.Categoria);
                            importeAnterior = (decimal)reader["Importe"];
                            aplicadoAnterior = (decimal)reader["Aplicado"];
                        }
                    }

                    if (importeAnterior == dto.Importe && aplicadoAnterior == dto.Aplicado)
                    {
                        tx.Commit();
                        return Ok(new { ok = true, sinCambios = true });
                    }

                    ActualizarMontos(conn, tx, dto.Categoria, dto.Importe, dto.Aplicado);

                    if (importeAnterior != dto.Importe)
                        RegistrarMovimiento(conn, tx, dto.Categoria, TipoAjusteImporte, null, dto.Importe, importeAnterior);
                    if (aplicadoAnterior != dto.Aplicado)
                        RegistrarMovimiento(conn, tx, dto.Categoria, TipoAjusteAplicado, null, dto.Aplicado, aplicadoAnterior);

                    tx.Commit();
                }
            }
            return Ok(new { ok = true });
        }

        /// <summary>
        /// POST /api/inversion/accion — registra un movimiento (concepto +
        /// cantidad) y lo suma al aplicado de la categoría. La cantidad puede ser
        /// negativa para corregir un registro anterior.
        /// </summary>
        [HttpPost, Route("accion")]
        public IHttpActionResult Accion(AccionInversionDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Categoria))
                return BadRequest("Falta la categoría.");
            if (string.IsNullOrWhiteSpace(dto.Concepto))
                return BadRequest("Escribe el concepto del movimiento.");
            if (dto.Cantidad == 0)
                return BadRequest("La cantidad no puede ser cero.");

            string concepto = dto.Concepto.Trim();
            if (concepto.Length > 300) concepto = concepto.Substring(0, 300);

            decimal aplicado;
            using (var conn = Db.Abrir())
            {
                EnsureTablas(conn);
                using (var tx = conn.BeginTransaction())
                {
                    decimal importe;
                    using (var cmd = new SqlCommand(
                        "SELECT Importe, Aplicado FROM dbo.InversionObra WITH (UPDLOCK) WHERE Categoria = @categoria", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@categoria", dto.Categoria);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read()) return BadRequest("Categoría desconocida: " + dto.Categoria);
                            importe = (decimal)reader["Importe"];
                            aplicado = (decimal)reader["Aplicado"];
                        }
                    }

                    aplicado += dto.Cantidad;
                    if (aplicado < 0)
                        return BadRequest("El movimiento dejaría el aplicado en negativo.");

                    ActualizarMontos(conn, tx, dto.Categoria, importe, aplicado);
                    RegistrarMovimiento(conn, tx, dto.Categoria, TipoAccion, concepto, dto.Cantidad, null);
                    tx.Commit();
                }
            }
            return Ok(new { ok = true, categoria = dto.Categoria, aplicado });
        }

        /// <summary>GET /api/inversion/historial?categoria=X — movimientos, del más reciente al más viejo.</summary>
        [HttpGet, Route("historial")]
        public IHttpActionResult Historial(string categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria))
                return BadRequest("Falta el parámetro 'categoria'.");

            var lista = new List<MovimientoInversion>();
            using (var conn = Db.Abrir())
            {
                EnsureTablas(conn);
                using (var cmd = new SqlCommand(
                    "SELECT TOP 200 Id, Categoria, Tipo, Concepto, Cantidad, Anterior, Usuario, Fecha " +
                    "FROM dbo.InversionMovimientos WHERE Categoria = @categoria ORDER BY Fecha DESC, Id DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@categoria", categoria);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            lista.Add(new MovimientoInversion
                            {
                                Id = (int)reader["Id"],
                                Categoria = (string)reader["Categoria"],
                                Tipo = (string)reader["Tipo"],
                                Concepto = reader["Concepto"] as string,
                                Cantidad = (decimal)reader["Cantidad"],
                                Anterior = reader["Anterior"] as decimal?,
                                Usuario = reader["Usuario"] as string,
                                Fecha = (DateTime)reader["Fecha"]
                            });
                    }
                }
            }
            return Ok(lista);
        }

        public class EditarMovimientoDto
        {
            public int Id { get; set; }
            public string Concepto { get; set; }
            public decimal Cantidad { get; set; }
        }

        public class MovimientoIdDto
        {
            public int Id { get; set; }
        }

        /// <summary>Sólo la credencial "admin" edita o borra renglones del historial.</summary>
        private bool EsAdmin => string.Equals(User?.Identity?.Name, "admin", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// POST /api/inversion/movimiento/editar — cambia concepto y cantidad de una
        /// acción ya registrada; la diferencia se aplica al total de la categoría.
        /// </summary>
        [HttpPost, Route("movimiento/editar")]
        public IHttpActionResult EditarMovimiento(EditarMovimientoDto dto)
        {
            if (!EsAdmin) return StatusCode(HttpStatusCode.Forbidden);
            if (dto == null || dto.Id <= 0) return BadRequest("Falta el movimiento a editar.");
            if (string.IsNullOrWhiteSpace(dto.Concepto)) return BadRequest("Escribe el concepto del movimiento.");
            if (dto.Cantidad == 0) return BadRequest("La cantidad no puede ser cero.");

            string concepto = dto.Concepto.Trim();
            if (concepto.Length > 300) concepto = concepto.Substring(0, 300);

            string categoria;
            decimal aplicado;
            using (var conn = Db.Abrir())
            {
                EnsureTablas(conn);
                using (var tx = conn.BeginTransaction())
                {
                    string tipo;
                    decimal cantidadAnterior;
                    if (!LeerMovimiento(conn, tx, dto.Id, out categoria, out tipo, out cantidadAnterior))
                        return BadRequest("El movimiento ya no existe.");
                    if (tipo != TipoAccion)
                        return BadRequest("Sólo se pueden editar los movimientos registrados como acción.");

                    decimal importe;
                    if (!LeerMontos(conn, tx, categoria, out importe, out aplicado))
                        return BadRequest("Categoría desconocida: " + categoria);

                    aplicado += dto.Cantidad - cantidadAnterior;
                    if (aplicado < 0) return BadRequest("El cambio dejaría el aplicado en negativo.");

                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.InversionMovimientos SET Concepto = @concepto, Cantidad = @cantidad, " +
                        "Usuario = @usuario, Fecha = GETDATE() WHERE Id = @id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@concepto", concepto);
                        cmd.Parameters.AddWithValue("@cantidad", dto.Cantidad);
                        cmd.Parameters.AddWithValue("@usuario", (object)User.Identity.Name ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@id", dto.Id);
                        cmd.ExecuteNonQuery();
                    }

                    ActualizarMontos(conn, tx, categoria, importe, aplicado);
                    tx.Commit();
                }
            }
            return Ok(new { ok = true, categoria, aplicado });
        }

        /// <summary>
        /// POST /api/inversion/movimiento/eliminar — borra un renglón del historial.
        /// Si era una acción, su cantidad se descuenta del aplicado; un renglón de
        /// ajuste sólo desaparece de la bitácora (el monto vigente no se toca).
        /// </summary>
        [HttpPost, Route("movimiento/eliminar")]
        public IHttpActionResult EliminarMovimiento(MovimientoIdDto dto)
        {
            if (!EsAdmin) return StatusCode(HttpStatusCode.Forbidden);
            if (dto == null || dto.Id <= 0) return BadRequest("Falta el movimiento a eliminar.");

            string categoria;
            decimal aplicado;
            using (var conn = Db.Abrir())
            {
                EnsureTablas(conn);
                using (var tx = conn.BeginTransaction())
                {
                    string tipo;
                    decimal cantidad;
                    if (!LeerMovimiento(conn, tx, dto.Id, out categoria, out tipo, out cantidad))
                        return BadRequest("El movimiento ya no existe.");

                    decimal importe;
                    if (!LeerMontos(conn, tx, categoria, out importe, out aplicado))
                        return BadRequest("Categoría desconocida: " + categoria);

                    if (tipo == TipoAccion)
                    {
                        aplicado -= cantidad;
                        if (aplicado < 0) return BadRequest("Borrar el movimiento dejaría el aplicado en negativo.");
                        ActualizarMontos(conn, tx, categoria, importe, aplicado);
                    }

                    using (var cmd = new SqlCommand("DELETE FROM dbo.InversionMovimientos WHERE Id = @id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", dto.Id);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
            return Ok(new { ok = true, categoria, aplicado });
        }

        private static bool LeerMovimiento(SqlConnection conn, SqlTransaction tx, int id,
                                           out string categoria, out string tipo, out decimal cantidad)
        {
            categoria = null; tipo = null; cantidad = 0;
            using (var cmd = new SqlCommand(
                "SELECT Categoria, Tipo, Cantidad FROM dbo.InversionMovimientos WITH (UPDLOCK) WHERE Id = @id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return false;
                    categoria = (string)reader["Categoria"];
                    tipo = (string)reader["Tipo"];
                    cantidad = (decimal)reader["Cantidad"];
                    return true;
                }
            }
        }

        private static bool LeerMontos(SqlConnection conn, SqlTransaction tx, string categoria,
                                       out decimal importe, out decimal aplicado)
        {
            importe = 0; aplicado = 0;
            using (var cmd = new SqlCommand(
                "SELECT Importe, Aplicado FROM dbo.InversionObra WITH (UPDLOCK) WHERE Categoria = @categoria", conn, tx))
            {
                cmd.Parameters.AddWithValue("@categoria", categoria);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return false;
                    importe = (decimal)reader["Importe"];
                    aplicado = (decimal)reader["Aplicado"];
                    return true;
                }
            }
        }

        private void ActualizarMontos(SqlConnection conn, SqlTransaction tx, string categoria,
                                      decimal importe, decimal aplicado)
        {
            using (var cmd = new SqlCommand(
                "UPDATE dbo.InversionObra SET Importe = @importe, Aplicado = @aplicado, " +
                "FechaActualizacion = GETDATE(), UsuarioModificacion = @usuario " +
                "WHERE Categoria = @categoria", conn, tx))
            {
                cmd.Parameters.AddWithValue("@importe", importe);
                cmd.Parameters.AddWithValue("@aplicado", aplicado);
                cmd.Parameters.AddWithValue("@usuario", (object)User.Identity.Name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@categoria", categoria);
                cmd.ExecuteNonQuery();
            }
        }

        private void RegistrarMovimiento(SqlConnection conn, SqlTransaction tx, string categoria, string tipo,
                                         string concepto, decimal cantidad, decimal? anterior)
        {
            using (var cmd = new SqlCommand(
                "INSERT INTO dbo.InversionMovimientos (Categoria, Tipo, Concepto, Cantidad, Anterior, Usuario) " +
                "VALUES (@categoria, @tipo, @concepto, @cantidad, @anterior, @usuario)", conn, tx))
            {
                cmd.Parameters.AddWithValue("@categoria", categoria);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.Parameters.AddWithValue("@concepto", (object)concepto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@cantidad", cantidad);
                cmd.Parameters.AddWithValue("@anterior", (object)anterior ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@usuario", (object)User.Identity.Name ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Crea las dos tablas si faltan y siembra las categorías que no estén (idempotente).</summary>
        private static void EnsureTablas(SqlConnection conn)
        {
            const string crear = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.InversionObra') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.InversionObra (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Categoria NVARCHAR(50) NOT NULL UNIQUE,
        Orden INT NOT NULL,
        Importe DECIMAL(18,2) NOT NULL DEFAULT(0),
        Aplicado DECIMAL(18,2) NOT NULL DEFAULT(0),
        FechaActualizacion DATETIME NULL,
        UsuarioModificacion NVARCHAR(100) NULL
    );
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.InversionMovimientos') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.InversionMovimientos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Categoria NVARCHAR(50) NOT NULL,
        Tipo NVARCHAR(20) NOT NULL,
        Concepto NVARCHAR(300) NULL,
        Cantidad DECIMAL(18,2) NOT NULL,
        Anterior DECIMAL(18,2) NULL,
        Usuario NVARCHAR(100) NULL,
        Fecha DATETIME NOT NULL DEFAULT(GETDATE())
    );
    CREATE INDEX IX_InversionMovimientos_Categoria ON dbo.InversionMovimientos (Categoria, Fecha DESC);
END";
            using (var cmd = new SqlCommand(crear, conn))
                cmd.ExecuteNonQuery();

            const string sembrar = @"
IF NOT EXISTS (SELECT 1 FROM dbo.InversionObra WHERE Categoria = @categoria)
    INSERT INTO dbo.InversionObra (Categoria, Orden, Importe, Aplicado)
    VALUES (@categoria, @orden, @importe, @aplicado);";
            for (int i = 0; i < Categorias.Length; i++)
            {
                using (var cmd = new SqlCommand(sembrar, conn))
                {
                    cmd.Parameters.AddWithValue("@categoria", Categorias[i].Item1);
                    cmd.Parameters.AddWithValue("@orden", i + 1);
                    cmd.Parameters.AddWithValue("@importe", Categorias[i].Item2);
                    cmd.Parameters.AddWithValue("@aplicado", Categorias[i].Item3);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
