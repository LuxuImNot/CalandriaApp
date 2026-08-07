using System;
using System.IO;
using System.Reflection;
using Calandria.Api.Controllers;
using Xunit;

namespace Calandria.Api.Tests
{
    // RutaSegura() es privado: se invoca por reflexión para no tener que exponerlo
    // solo para pruebas. Cubre el control anti path-traversal que el propio
    // UiController documenta como necesario ("no basta con validar caracteres").
    public class UiControllerRutaSeguraTests
    {
        private static string RutaSegura(string nombre)
        {
            MethodInfo metodo = typeof(UiController).GetMethod("RutaSegura", BindingFlags.NonPublic | BindingFlags.Static);
            return (string)metodo.Invoke(null, new object[] { nombre });
        }

        private static string CarpetaUiEsperada()
        {
            // Misma resolución que Configuracion.UiRuta con la clave UiRuta sin definir.
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ui"));
        }

        [Fact]
        public void NombreValido_ResuelveDentroDeLaCarpetaUi()
        {
            string resultado = RutaSegura("panel.html");

            Assert.NotNull(resultado);
            Assert.StartsWith(CarpetaUiEsperada(), resultado, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("../secrets.config")]
        [InlineData("..\\..\\Windows\\win.ini")]
        [InlineData("subcarpeta/../../../secrets.config")]
        public void IntentoDeSalirDeLaCarpeta_DevuelveNull(string nombreMalicioso)
        {
            Assert.Null(RutaSegura(nombreMalicioso));
        }

        [Fact]
        public void RutaAbsoluta_DevuelveNull()
        {
            string absoluta = Path.Combine(Path.GetTempPath(), "otraCosa.html");

            Assert.Null(RutaSegura(absoluta));
        }

        [Fact]
        public void CarpetaHermanaConMismoPrefijo_NoSeConfundeConLaCarpetaUi()
        {
            // "ui-otro" no debe colarse por ser prefijo de texto de "ui".
            Assert.Null(RutaSegura("../ui-otro/archivo.html"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void NombreVacio_DevuelveNull(string nombre)
        {
            Assert.Null(RutaSegura(nombre));
        }
    }
}
