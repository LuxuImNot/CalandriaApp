using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Calandria.Api.Models;
using Calandria.Api.Services;
using Xunit;

namespace Calandria.Api.Tests
{
    // Pruebas puras (sin BD ni red): CfdiParser.Parse solo lee el XML, y
    // ConciliadorFactura.Conciliar recibe las líneas de la OC ya cargadas. La IA
    // opcional queda desactivada por IaProveedor=None en App.config, así que el
    // camino "sin match" no dispara ninguna llamada de red.
    public class ConciliacionFacturaTests
    {
        private const string CfdiEjemplo = @"
<cfdi:Comprobante xmlns:cfdi=""http://www.sat.gob.mx/cfd/4"" Version=""4.0"" Total=""11800.00"">
  <cfdi:Emisor Rfc=""ABC010101AAA"" Nombre=""Proveedor de Prueba SA de CV"" />
  <cfdi:Conceptos>
    <cfdi:Concepto NoIdentificacion=""BLK-12x20x40"" ClaveProdServ=""30111601"" Descripcion=""Bloque entero 12x20x40cm"" Unidad=""Pieza"" Cantidad=""1000"" ValorUnitario=""10.00"" Importe=""10000.00"" />
    <cfdi:Concepto NoIdentificacion="""" ClaveProdServ=""30111601"" Descripcion=""Cemento Gris 50Kg"" ClaveUnidad=""H87"" Cantidad=""50"" ValorUnitario=""36.00"" Importe=""1800.00"" />
  </cfdi:Conceptos>
  <cfdi:Complemento>
    <tfd:TimbreFiscalDigital xmlns:tfd=""http://www.sat.gob.mx/TimbreFiscalDigital"" UUID=""11111111-2222-3333-4444-555555555555"" />
  </cfdi:Complemento>
</cfdi:Comprobante>";

        [Fact]
        public void CfdiParser_LeeCabeceraYConceptos()
        {
            CfdiParseado cfdi = CfdiParser.Parse(CfdiEjemplo);

            Assert.Equal(11800.00m, cfdi.Total);
            Assert.Equal("Proveedor de Prueba SA de CV", cfdi.Emisor);
            Assert.Equal("ABC010101AAA", cfdi.Rfc);
            Assert.Equal("11111111-2222-3333-4444-555555555555", cfdi.Uuid);
            Assert.Equal(2, cfdi.Conceptos.Count);
            Assert.Equal("BLK-12x20x40", cfdi.Conceptos[0].NoIdentificacion);
            Assert.Equal(1000m, cfdi.Conceptos[0].Cantidad);
        }

        [Fact]
        public void CfdiParser_SinAtributoUnidad_UsaClaveUnidadComoRespaldo()
        {
            CfdiParseado cfdi = CfdiParser.Parse(CfdiEjemplo);

            // El 2do concepto no trae Unidad, solo ClaveUnidad="H87" (ver Parse: respaldo).
            Assert.Equal("H87", cfdi.Conceptos[1].Unidad);
        }

        [Fact]
        public void CfdiParser_XmlConDoctype_LanzaEnVezDeProcesarEntidades()
        {
            // Protección anti XXE / billion-laughs documentada en CfdiParser: un CFDI
            // real nunca trae DOCTYPE, así que prohibirlo es seguro y no rompe nada real.
            string xmlConDoctype = "<!DOCTYPE foo [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]>" + CfdiEjemplo;

            Assert.Throws<XmlException>(() => CfdiParser.Parse(xmlConDoctype));
        }

        [Theory]
        [InlineData("Válvula   Compresión", "VALVULA COMPRESION")]
        [InlineData("  bloque entero  12x20x40cm ", "BLOQUE ENTERO 12X20X40CM")]
        public void NormNombre_QuitaAcentosYColapsaEspacios(string entrada, string esperado)
        {
            Assert.Equal(esperado, ConciliadorFactura.NormNombre(entrada));
        }

        [Fact]
        public void Conciliar_EmparejaPorClaveYPorNombre_YDejaSinMatchLoQueNoAparece()
        {
            var cfdi = CfdiParser.Parse(CfdiEjemplo);
            // Concepto extra en la factura que ninguna línea de la OC reclama.
            cfdi.Conceptos.Add(new FacturaConceptoDto { NoIdentificacion = "", Descripcion = "Flete de material", Cantidad = 1, Importe = 500m });

            var lineasOC = new List<OcLinea>
            {
                new OcLinea { IdDetalle = 1, Clave = "BLK-12x20x40", Descripcion = "Bloque entero 12x20x40cm", Unidad = "Pieza", Cantidad = 1000 },
                new OcLinea { IdDetalle = 2, Clave = "", Descripcion = "cemento gris 50kg", Unidad = "Pieza", Cantidad = 50 },
                new OcLinea { IdDetalle = 3, Clave = "ZZZ-NOMATCH", Descripcion = "Varilla 3/8 (no está en la factura)", Unidad = "Pieza", Cantidad = 10 },
            };

            var resp = ConciliadorFactura.Conciliar("OC-TEST-001", cfdi, lineasOC);

            var l1 = resp.Lineas.Single(l => l.IdDetalle == 1);
            Assert.Equal("Clave", l1.Origen);
            Assert.Equal(1.0, l1.Confianza);
            Assert.Equal(1000m, l1.CantidadFacturada);
            Assert.Equal(0m, l1.Diferencia);

            var l2 = resp.Lineas.Single(l => l.IdDetalle == 2);
            Assert.Equal("Nombre", l2.Origen);
            Assert.Equal(0.9, l2.Confianza);
            Assert.Equal(50m, l2.CantidadFacturada);

            var l3 = resp.Lineas.Single(l => l.IdDetalle == 3);
            Assert.Equal("SinMatch", l3.Origen);
            Assert.Equal(0m, l3.CantidadFacturada);
            Assert.Equal(-10m, l3.Diferencia);

            Assert.False(resp.UsoIA);
            Assert.Single(resp.SinAsignar);
            Assert.Equal("Flete de material", resp.SinAsignar[0].Descripcion);
        }
    }
}
