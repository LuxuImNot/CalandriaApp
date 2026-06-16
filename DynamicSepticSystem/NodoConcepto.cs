using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Clase que representa un nodo en el árbol de conceptos y partidas
    /// Puede ser un concepto (nodo padre) o una partida (nodo hijo)
    /// </summary>
    public class NodoConcepto
    {
        /// <summary>
        /// Indica si este nodo es un concepto (true) o una partida (false)
        /// </summary>
        public bool EsConcepto { get; set; }
        
        /// <summary>
        /// WBS (Work Breakdown Structure) - Código numérico único de la partida
        /// </summary>
        public int WBS { get; set; }
        
        /// <summary>
        /// Código del concepto o partida
        /// </summary>
        public string Codigo { get; set; }
        
        /// <summary>
        /// Nombre del concepto o partida
        /// </summary>
        public string Nombre { get; set; }
        
        /// <summary>
        /// Total presupuestado (costo total de la partida o suma de partidas del concepto)
        /// </summary>
        public double Total { get; set; }
        
        /// <summary>
        /// Indica si este nodo debe incluirse en la estimación actual
        /// </summary>
        public bool Incluir { get; set; }
        
        /// <summary>
        /// Indica si la partida o concepto está completado (100%)
        /// </summary>
        public bool Completado { get; set; }
        
        /// <summary>
        /// Porcentaje de avance (0-100)
        /// </summary>
        public double AvancePorcentaje { get; set; }
        
        /// <summary>
        /// Monto ejecutado hasta el momento
        /// </summary>
        public double MontoEjecutado { get; set; }
        
        /// <summary>
        /// Fecha de finalización de la partida o concepto
        /// </summary>
        public DateTime? FechaFinalizacion { get; set; }
        
        /// <summary>
        /// Lista de partidas hijas (solo para nodos concepto)
        /// </summary>
        public List<NodoConcepto> Partidas { get; set; }
        
        /// <summary>
        /// Indica si esta partida tiene costo dinámico (calculado por m²)
        /// </summary>
        public bool EsDinamica { get; set; }
        
        /// <summary>
        /// Valor por metro cuadrado para prototipo Tunera
        /// </summary>
        public double ValorM2Tunera { get; set; }
        
        /// <summary>
        /// Valor por metro cuadrado para prototipo Calandra
        /// </summary>
        public double ValorM2Calandra { get; set; }
        
        /// <summary>
        /// Metros cuadrados para cálculo dinámico
        /// </summary>
        public double MetrosCuadrados { get; set; }
        
        /// <summary>
        /// Límite máximo de m² para partidas dinámicas.
        /// Si es 0 o negativo, no hay límite (usa todos los m² ingresados).
        /// El costo se calculará con el mínimo entre m² ingresados y este límite.
        /// </summary>
        public double LimiteM2 { get; set; }
        
        /// <summary>
        /// Límite máximo de m² para Tunera en partidas dinámicas.
        /// Si es 0 o negativo, no hay límite.
        /// </summary>
        public double LimiteM2Tunera { get; set; }
        
        /// <summary>
        /// Límite máximo de m² para Calandra en partidas dinámicas.
        /// Si es 0 o negativo, no hay límite.
        /// </summary>
        public double LimiteM2Calandra { get; set; }
        
        /// <summary>
        /// Lista de prototipos a los que aplica esta partida.
        /// Almacenado como string separado por comas (ej: "TUNERA,CALANDRA").
        /// Si es null o vacío, aplica a todos los prototipos.
        /// </summary>
        public string PrototiposAplicables { get; set; }
        
        /// <summary>
        /// Lista de prototipos como array para facilitar el manejo
        /// </summary>
        public string[] PrototiposArray
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PrototiposAplicables))
                    return new[] { "TUNERA", "CALANDRA" }; // Por defecto aplica a todos
                
                return PrototiposAplicables
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim().ToUpperInvariant())
                    .ToArray();
            }
        }
        
        /// <summary>
        /// Verifica si la partida aplica a un prototipo específico
        /// </summary>
        /// <param name="prototipo">Nombre del prototipo a verificar</param>
        /// <returns>True si la partida aplica al prototipo, False en caso contrario</returns>
        public bool AplicaAPrototipo(string prototipo)
        {
            if (string.IsNullOrWhiteSpace(prototipo))
                return true;
            
            if (string.IsNullOrWhiteSpace(PrototiposAplicables))
                return true; // Si no se especifica, aplica a todos
            
            string prototipoNormalizado = prototipo.Trim().ToUpperInvariant();
            return PrototiposArray.Contains(prototipoNormalizado);
        }
        
        /// <summary>
        /// Obtiene una descripción de los prototipos aplicables
        /// </summary>
        public string PrototiposDescripcion
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PrototiposAplicables))
                    return "Todos";
                
                var prototipos = PrototiposArray;
                if (prototipos.Length == 0)
                    return "Todos";
                
                if (prototipos.Length == 1)
                    return $"Solo {prototipos[0]}";
                
                return string.Join(", ", prototipos);
            }
        }
        
        /// <summary>
        /// Obtiene el límite efectivo para un prototipo dado
        /// </summary>
        /// <param name="prototipo">Nombre del prototipo (TUNERA o CALANDRA)</param>
        /// <returns>Límite efectivo de m²</returns>
        public double ObtenerLimiteEfectivo(string prototipo)
        {
            if (string.IsNullOrEmpty(prototipo))
                return LimiteM2;
            
            if (prototipo.ToUpper().Contains("TUNERA"))
            {
                return LimiteM2Tunera > 0 ? LimiteM2Tunera : LimiteM2;
            }
            else
            {
                return LimiteM2Calandra > 0 ? LimiteM2Calandra : LimiteM2;
            }
        }
        
        /// <summary>
        /// Calcula el porcentaje de avance basado en los m² ingresados vs el límite
        /// </summary>
        /// <param name="prototipo">Nombre del prototipo para obtener el límite correcto</param>
        /// <returns>Porcentaje de avance (0-100), o -1 si no aplica</returns>
        public double CalcularAvancePorLimite(string prototipo)
        {
            if (!EsDinamica || MetrosCuadrados <= 0)
                return -1; // No aplica
            
            double limite = ObtenerLimiteEfectivo(prototipo);
            
            if (limite <= 0)
                return -1; // Sin límite definido, no se puede calcular avance
            
            double avance = (MetrosCuadrados / limite) * 100.0;
            return Math.Min(avance, 100.0); // Máximo 100%
        }
        
        /// <summary>
        /// Calcula los metros cuadrados efectivos aplicando el límite si existe
        /// </summary>
        public double MetrosCuadradosEfectivos
        {
            get
            {
                if (!EsDinamica || MetrosCuadrados <= 0)
                    return MetrosCuadrados;
                
                // Aplicar límite si está definido
                if (LimiteM2 > 0 && MetrosCuadrados > LimiteM2)
                    return LimiteM2;
                
                return MetrosCuadrados;
            }
        }
        
        /// <summary>
        /// Calcula los metros cuadrados efectivos para un prototipo específico
        /// </summary>
        /// <param name="prototipo">Nombre del prototipo</param>
        /// <returns>Metros cuadrados efectivos considerando el límite del prototipo</returns>
        public double ObtenerMetrosCuadradosEfectivos(string prototipo)
        {
            if (!EsDinamica || MetrosCuadrados <= 0)
                return MetrosCuadrados;
            
            double limite = ObtenerLimiteEfectivo(prototipo);
            
            if (limite > 0 && MetrosCuadrados > limite)
                return limite;
            
            return MetrosCuadrados;
        }
        
        /// <summary>
        /// Calcula el costo dinámico aplicando el límite de m² si corresponde
        /// </summary>
        /// <param name="valorM2">Valor por metro cuadrado según el prototipo</param>
        /// <returns>Costo calculado con límite aplicado</returns>
        public double CalcularCostoDinamico(double valorM2)
        {
            if (!EsDinamica || valorM2 <= 0 || MetrosCuadrados <= 0)
                return Total;
            
            return valorM2 * MetrosCuadradosEfectivos;
        }
        
        public NodoConcepto()
        {
            Partidas = new List<NodoConcepto>();
            EsConcepto = false;
            WBS = 0;
            Codigo = string.Empty;
            Nombre = string.Empty;
            Total = 0;
            Incluir = false;
            Completado = false;
            AvancePorcentaje = 0;
            MontoEjecutado = 0;
            FechaFinalizacion = null;
            EsDinamica = false;
            ValorM2Tunera = 0;
            ValorM2Calandra = 0;
            MetrosCuadrados = 0;
            LimiteM2 = 0;
            LimiteM2Tunera = 0;
            LimiteM2Calandra = 0;
            PrototiposAplicables = null; // null = aplica a todos
        }
    }
}
