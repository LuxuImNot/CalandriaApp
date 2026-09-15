using System;
using System.ComponentModel;
using System.IO;
using ClosedXML.Excel;

namespace DynamicSepticSystem
{
    public static class FolioManager
    {
        private static readonly string rutaFolios = @"C:\Users\Luxu\Documents\CALANDRIA RESIDENCIAL\FoliosOrdenCompra.xlsx";
        private const string hoja = "Folios";

        public static string GenerarFolioOrdenCompra(string manzana, string lote)
        {
            int nuevoNumero = 1;
            string fecha = DateTime.Now.ToString("yyyyMMdd");

            if (!File.Exists(rutaFolios))
            {
                using (var nuevo = new XLWorkbook())
                {
                    var ws = nuevo.Worksheets.Add(hoja);
                    ws.Cell(1, 1).Value = "Fecha";
                    ws.Cell(1, 2).Value = "Folio";
                    ws.Cell(2, 1).Value = fecha;
                    ws.Cell(2, 2).Value = nuevoNumero;
                    nuevo.SaveAs(rutaFolios);
                }
            }
            else
            {
                using (var package = new XLWorkbook(rutaFolios))
                {
                    if (!package.Worksheets.TryGetWorksheet(hoja, out var wsFolios))
                        wsFolios = package.Worksheets.Add(hoja);
                    int lastRow = wsFolios.LastRowUsed()?.RowNumber() ?? 1;
                    string ultimaFecha = wsFolios.Cell(lastRow, 1).GetString();

                    if (ultimaFecha == fecha)
                    {
                        nuevoNumero = int.Parse(wsFolios.Cell(lastRow, 2).GetString()) + 1;
                    }

                    wsFolios.Cell(lastRow + 1, 1).Value = fecha;
                    wsFolios.Cell(lastRow + 1, 2).Value = nuevoNumero;
                    package.Save();
                }
            }

            return $"OC-M{manzana}-L{lote}-{fecha}-{nuevoNumero.ToString("D3")}";
        }
    }
}
