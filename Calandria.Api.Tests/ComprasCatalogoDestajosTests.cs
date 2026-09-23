using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Calandria.Api.Data;
using Xunit;

namespace Calandria.Api.Tests
{
    /// <summary>
    /// GET api/compras/catalogo-destajos es, desde ahora, el ÚNICO catálogo de compras:
    /// la explosión (COMPRAS*) se dejó de leer porque estaba desfasada del árbol.
    /// Lo que fija esta prueba son las dos reglas que hacen que el catálogo sea veraz:
    ///   - salen TODAS las tareas TipoTarea=1 del árbol, sin exigir destajo activado
    ///     (antes un gate global devolvía catálogo vacío),
    ///   - de COMPRAS* sólo entran las filas Familia='MANUAL' (insumos capturados a
    ///     mano, que no existen en ningún destajo); el resto de la explosión se ignora.
    ///
    /// Misma mecánica que las otras integraciones: transacción propia y Rollback
    /// siempre, así que no deja nada escrito en la obra de prueba.
    /// </summary>
    public sealed class ComprasCatalogoDestajosTests : IDisposable
    {
        private const string ObraPrueba = "Prueba_numero_1";
        private const string Ruta = "RutaTuneraDestajo";
        private const string TablaExplosion = "COMPRASTUNERA";

        // Las mismas consultas que ejecuta ComprasController.CatalogoDestajos.
        private const string SqlArbol = @"
            SELECT
                r.Nombre AS Nombre,
                ISNULL(MAX(CASE WHEN c.NombreColumna = 'Clave'    THEN c.Valor END), '') AS Clave,
                ISNULL(MAX(CASE WHEN c.NombreColumna = 'Cantidad' THEN c.Valor END), '0') AS Cantidad
            FROM [RutaTuneraDestajo] r
            LEFT JOIN [RutaTuneraDestajo_Columnas] c ON c.NodoID = r.ID
            WHERE r.TipoTarea = 1
            GROUP BY r.ID, r.Nombre";

        private const string SqlManuales =
            "SELECT Clave, Descripcion FROM [COMPRASTUNERA] WHERE Familia = 'MANUAL'";

        private readonly SqlConnection _conn;
        private readonly SqlTransaction _tx;

        public ComprasCatalogoDestajosTests()
        {
            ObraContext.CadenaActual = Configuracion.CadenaConexionObra(ObraPrueba);
            _conn = Db.Abrir();
            _tx = _conn.BeginTransaction();
        }

        public void Dispose()
        {
            _tx.Rollback();
            _conn.Dispose();
        }

        private int SiguienteIdNodo()
        {
            using (var cmd = new SqlCommand($"SELECT ISNULL(MAX(ID), 0) + 1 FROM [{Ruta}]", _conn, _tx))
                return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private void InsertarNodoMaterial(int id, string nombre)
        {
            using (var cmd = new SqlCommand($@"
                INSERT INTO [{Ruta}] (ID, ParentID, Nombre, Descripcion, Orden, Nivel,
                                      FechaCreacion, FechaModificacion, UsuarioCreacion, TipoTarea)
                VALUES (@id, NULL, @nombre, '', 0, 2, GETDATE(), NULL, 'test', 1)", _conn, _tx))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertarFilaExplosion(string clave, string descripcion, string familia)
        {
            using (var cmd = new SqlCommand($@"
                INSERT INTO [{TablaExplosion}] (Clave, Descripcion, Unidad, Cantidad, Familia)
                VALUES (@c, @d, 'PZA', 1, @f)", _conn, _tx))
            {
                cmd.Parameters.AddWithValue("@c", clave);
                cmd.Parameters.AddWithValue("@d", descripcion);
                cmd.Parameters.AddWithValue("@f", familia);
                cmd.ExecuteNonQuery();
            }
        }

        private List<string> Nombres(string sql, string columna)
        {
            var lista = new List<string>();
            using (var cmd = new SqlCommand(sql, _conn, _tx))
            using (var rd = cmd.ExecuteReader())
                while (rd.Read())
                    lista.Add((rd[columna]?.ToString() ?? "").Trim());
            return lista;
        }

        [Fact]
        public void Catalogo_TraeLaTareaMaterial_AunqueNingunDestajoEsteActivado()
        {
            // El gate viejo miraba ActivacionTareasRuta.DesatajoActivado; ya no existe.
            // Este nodo no está activado en ninguna casa y aun así debe salir.
            string nombre = "INSUMO PRUEBA CATALOGO " + Guid.NewGuid().ToString("N").Substring(0, 8);
            InsertarNodoMaterial(SiguienteIdNodo(), nombre);

            Assert.Contains(nombre, Nombres(SqlArbol, "Nombre"));
        }

        [Fact]
        public void Catalogo_DeLaExplosionSoloEntraLoMarcadoComoManual()
        {
            string sufijo = Guid.NewGuid().ToString("N").Substring(0, 8);
            string claveManual = "MAN-" + sufijo;
            string claveVieja = "EXP-" + sufijo;

            InsertarFilaExplosion(claveManual, "Insumo capturado a mano " + sufijo, "MANUAL");
            InsertarFilaExplosion(claveVieja, "Insumo de la explosion vieja " + sufijo, "Acero");

            var manuales = Nombres(SqlManuales, "Clave");

            Assert.Contains(claveManual, manuales);
            Assert.DoesNotContain(claveVieja, manuales);
        }
    }
}
