using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

public class DetalleOrden
{
    public string Manzana { get; set; }
    public string Lote { get; set; }
    public string FolioOC { get; set; }
    public string Detalles { get; set; }
}

public static class DetalleOrdenHelper
{
    private static string rutaJson = @"C:\Users\Luxu\Documents\CALANDRIA RESIDENCIAL\DetallesOrdenes\detalles_casas.json";

    public static void GuardarDetalleOrden(string manzana, string lote, string folioOC, string detalles)
    {
        List<DetalleOrden> lista;

        if (File.Exists(rutaJson))
        {
            var json = File.ReadAllText(rutaJson);
            lista = JsonConvert.DeserializeObject<List<DetalleOrden>>(json) ?? new List<DetalleOrden>();
        }
        else
        {
            lista = new List<DetalleOrden>();
        }

        // Actualiza si ya existe ese folio, si no lo agrega
        var existente = lista.FirstOrDefault(d => d.FolioOC == folioOC);
        if (existente != null)
        {
            existente.Detalles = detalles;
        }
        else
        {
            lista.Add(new DetalleOrden { Manzana = manzana, Lote = lote, FolioOC = folioOC, Detalles = detalles });
        }

        Directory.CreateDirectory(Path.GetDirectoryName(rutaJson));
        File.WriteAllText(rutaJson, JsonConvert.SerializeObject(lista, Formatting.Indented));
    }


    public static string LeerDetalleOrden(string manzana, string lote)
    {
        if (!File.Exists(rutaJson))
            return "(Sin detalles)";

        var lista = JsonConvert.DeserializeObject<List<DetalleOrden>>(File.ReadAllText(rutaJson));
        var detalle = lista?.LastOrDefault(d => d.Manzana == manzana && d.Lote == lote);
        return detalle?.Detalles ?? "(Sin detalles)";
    }
    public static string LeerDetallePorFolio(string folioOC)
    {
        if (!File.Exists(rutaJson))
            return "(Sin detalles)";

        var lista = JsonConvert.DeserializeObject<List<DetalleOrden>>(File.ReadAllText(rutaJson));
        var detalle = lista?.LastOrDefault(d => d.FolioOC == folioOC);
        return detalle?.Detalles ?? "(Sin detalles)";
    }

}
