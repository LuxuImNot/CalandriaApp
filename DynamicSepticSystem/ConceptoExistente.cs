using System;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Clase que representa un concepto existente en la base de datos
    /// Usado para el selector de posición al agregar nuevos conceptos
    /// </summary>
    public class ConceptoExistente
    {
        /// <summary>
        /// Código numérico del concepto (1-12)
        /// </summary>
        public int Codigo { get; set; }
        
        /// <summary>
        /// Nombre del concepto (ej: "Preliminares", "Cimentación", etc.)
        /// </summary>
        public string Nombre { get; set; }
    }
}
