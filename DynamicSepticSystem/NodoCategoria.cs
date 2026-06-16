using System.Collections.Generic;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Representa un nodo en la estructura jerárquica de avance de obra.
    /// Puede ser una categoría (concepto) o una partida específica.
    /// </summary>
    public class NodoCategoria
    {
        public bool EsCategoria { get; set; }
        public int WBS { get; set; }
        public string Codigo { get; set; } // ? Código numérico del concepto (1-12)
        public string Nombre { get; set; }
        public string Padre { get; set; }
        public string Etapa { get; set; }
        public string Partida { get; set; }
        public double ImporteTotal { get; set; }
        public double AvancePorcentaje { get; set; }
        public double ImporteEjecutado { get; set; }
        public List<NodoCategoria> Partidas { get; set; } = new List<NodoCategoria>();
    }
}
