using System.Collections.Generic;

namespace DynamicSepticSystem
{
    public class CasaInventario
    {
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Prototipo { get; set; }
        public string FotoPath { get; set; }
        // Lista de WBS marcados como entregados (persistida en DB como JSON)
        public List<string> DestajosTerminadosWBS { get; set; } = new List<string>();
    }
}
