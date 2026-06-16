using System;
using System.ComponentModel;
using System.IO;
using OfficeOpenXml;

namespace DynamicSepticSystem
{
    public static class FolioManager
    {
        private static readonly string rutaFolios = @"C:\Users\Luxu\Documents\CALANDRIA RESIDENCIAL\FoliosOrdenCompra.xlsx";
        private const string hoja = "Folios";

        public static string GenerarFolioOrdenCompra(string manzana, string lote)
        {
            ExcelPackage.License.SetNonCommercialOrganization("Ren-O-Franc");
            int nuevoNumero = 1;
            string fecha = DateTime.Now.ToString("yyyyMMdd");

            if (!File.Exists(rutaFolios))
            {
                using (var nuevo = new ExcelPackage())
                {
                    var ws = nuevo.Workbook.Worksheets.Add(hoja);
                    ws.Cells[1, 1].Value = "Fecha";
                    ws.Cells[1, 2].Value = "Folio";
                    ws.Cells[2, 1].Value = fecha;
                    ws.Cells[2, 2].Value = nuevoNumero;
                    nuevo.SaveAs(new FileInfo(rutaFolios));
                }
            }
            else
            {
                using (var package = new ExcelPackage(new FileInfo(rutaFolios)))
                {
                    var wsFolios = package.Workbook.Worksheets[hoja] ?? package.Workbook.Worksheets.Add(hoja);
                    int lastRow = wsFolios.Dimension?.End.Row ?? 1;
                    string ultimaFecha = wsFolios.Cells[lastRow, 1].Text;

                    if (ultimaFecha == fecha)
                    {
                        nuevoNumero = int.Parse(wsFolios.Cells[lastRow, 2].Text) + 1;
                    }

                    wsFolios.Cells[lastRow + 1, 1].Value = fecha;
                    wsFolios.Cells[lastRow + 1, 2].Value = nuevoNumero;
                    package.Save();
                }
            }

            return $"OC-M{manzana}-L{lote}-{fecha}-{nuevoNumero.ToString("D3")}";
        }
    }
}
