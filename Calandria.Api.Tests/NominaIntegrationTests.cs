using System;
using System.Collections.Generic;
using System.Linq;
using Calandria.Api.Controllers;
using Calandria.Api.Data;
using Calandria.Api.Models;
using Xunit;
using OkNegotiatedResult = System.Web.Http.Results.OkNegotiatedContentResult<Calandria.Api.Models.AsignacionNominaDto>;

namespace Calandria.Api.Tests
{
    /// <summary>
    /// Integración contra la obra de prueba real "Prueba_numero_1". A diferencia de
    /// EditorTareas/OrdenesCompra, GuardarAsignacion hace su propio Commit interno
    /// (abre su propia conexión con Db.Abrir()), así que no hay una transacción
    /// externa que hacer Rollback: la limpieza es explícita, vía el propio endpoint
    /// EliminarAsignacion, con claves (Manzana/Lote/Ruta/NodoId) que no pueden
    /// colisionar con datos reales.
    /// </summary>
    public sealed class NominaIntegrationTests : IDisposable
    {
        private const string ObraPrueba = "Prueba_numero_1";
        private const string Manzana = "ZZTEST";
        private const string Lote = "ZZTEST";
        private const string Ruta = "RutaPonytailTest";
        private const int NodoId = -999002;

        public NominaIntegrationTests()
        {
            ObraContext.CadenaActual = Configuracion.CadenaConexionObra(ObraPrueba);
        }

        public void Dispose()
        {
            new NominaController().EliminarAsignacion(new EliminarAsignacionRequest
            {
                Manzana = Manzana,
                Lote = Lote,
                Ruta = Ruta,
                NodoIds = new List<int> { NodoId }
            });
        }

        [Fact]
        public void GuardarYLeerAsignacion_MontosSeConservanExactos()
        {
            var controller = new NominaController();
            var req = new GuardarAsignacionRequest
            {
                Manzana = Manzana,
                Lote = Lote,
                Ruta = Ruta,
                NodoId = NodoId,
                NombreTarea = "TEST_ponytail",
                CodigoCuadrilla = "ZZTEST-CUADRILLA",
                TotalDistribuir = 5000.00m,
                Lineas = new List<LineaAsignacionNomina>
                {
                    new LineaAsignacionNomina { Nombre = "Trabajador Uno", Rol = "Albañil", EsJefe = true, Monto = 3000.75m },
                    new LineaAsignacionNomina { Nombre = "Trabajador Dos", Rol = "Ayudante", EsJefe = false, Monto = 1999.25m },
                }
            };

            var guardado = controller.GuardarAsignacion(req);
            Assert.IsType<System.Web.Http.Results.OkResult>(guardado);

            var leido = (OkNegotiatedResult)controller.Asignacion(Manzana, Lote, Ruta, NodoId);

            Assert.Equal("ZZTEST-CUADRILLA", leido.Content.CodigoCuadrilla);
            Assert.Equal(2, leido.Content.Montos.Count);
            Assert.Equal(3000.75m, leido.Content.Montos.Single(m => m.NombreTrabajador == "Trabajador Uno").Monto);
            Assert.Equal(1999.25m, leido.Content.Montos.Single(m => m.NombreTrabajador == "Trabajador Dos").Monto);
            Assert.Equal(5000.00m, leido.Content.Montos.Sum(m => m.Monto));
        }

        [Fact]
        public void GuardarAsignacion_AlGuardarDeNuevo_ReemplazaEnVezDeAcumular()
        {
            var controller = new NominaController();
            var primera = new GuardarAsignacionRequest
            {
                Manzana = Manzana,
                Lote = Lote,
                Ruta = Ruta,
                NodoId = NodoId,
                CodigoCuadrilla = "ZZTEST-CUADRILLA",
                TotalDistribuir = 1000m,
                Lineas = new List<LineaAsignacionNomina>
                {
                    new LineaAsignacionNomina { Nombre = "Trabajador Viejo", Monto = 1000m }
                }
            };
            controller.GuardarAsignacion(primera);

            var segunda = new GuardarAsignacionRequest
            {
                Manzana = Manzana,
                Lote = Lote,
                Ruta = Ruta,
                NodoId = NodoId,
                CodigoCuadrilla = "ZZTEST-CUADRILLA",
                TotalDistribuir = 5000m,
                Lineas = new List<LineaAsignacionNomina>
                {
                    new LineaAsignacionNomina { Nombre = "Trabajador Nuevo", Monto = 5000m }
                }
            };
            controller.GuardarAsignacion(segunda);

            var leido = (OkNegotiatedResult)controller.Asignacion(Manzana, Lote, Ruta, NodoId);

            Assert.Single(leido.Content.Montos);
            Assert.Equal("Trabajador Nuevo", leido.Content.Montos[0].NombreTrabajador);
            Assert.Equal(5000m, leido.Content.Montos[0].Monto);
        }

        [Fact]
        public void EliminarAsignacion_BorraLaAsignacionGuardada()
        {
            var controller = new NominaController();
            controller.GuardarAsignacion(new GuardarAsignacionRequest
            {
                Manzana = Manzana,
                Lote = Lote,
                Ruta = Ruta,
                NodoId = NodoId,
                CodigoCuadrilla = "ZZTEST-CUADRILLA",
                TotalDistribuir = 1000m,
                Lineas = new List<LineaAsignacionNomina> { new LineaAsignacionNomina { Nombre = "X", Monto = 1000m } }
            });

            int borrados = (int)((System.Web.Http.Results.OkNegotiatedContentResult<int>)
                controller.EliminarAsignacion(new EliminarAsignacionRequest
                {
                    Manzana = Manzana,
                    Lote = Lote,
                    Ruta = Ruta,
                    NodoIds = new List<int> { NodoId }
                })).Content;

            Assert.Equal(1, borrados);
            var leido = (OkNegotiatedResult)controller.Asignacion(Manzana, Lote, Ruta, NodoId);
            Assert.Empty(leido.Content.Montos);
        }
    }
}
