using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Tipo de tarea para nodos hijo
    /// </summary>
    public enum TipoTarea
    {
        /// <summary>
        /// No especificado
        /// </summary>
        Ninguno = 0,

        /// <summary>
        /// Material (se muestra en rojo)
        /// </summary>
        Material = 1,

        /// <summary>
        /// Mano de Obra (se muestra en verde)
        /// </summary>
        ManoDeObra = 2
    }

    /// <summary>
    /// Representa un nodo en la estructura de �rbol jer�rquica
    /// Puede ser padre, sub-padre o hijo
    /// </summary>
    public class NodoTree : INotifyPropertyChanged
    {
        #region Campos privados

        private int _id;
        private int? _parentId;
        private string _nombre;
        private string _descripcion;
        private int _orden;
        private int _nivel;
        private bool _esExpandible;
        private List<NodoTree> _hijos;
        private TipoTarea _tipoTarea;

        #endregion

        #region Propiedades

        /// <summary>
        /// ID �nico del nodo
        /// </summary>
        public int ID
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(ID));
                }
            }
        }

        /// <summary>
        /// True si el nodo fue creado en esta sesión y todavía no se ha guardado en
        /// la BD. Permite re-asignar su ID al guardar si otra sesión insertó un nodo
        /// con el mismo número mientras tanto (evita PK duplicada / sobrescritura).
        /// </summary>
        public bool EsNuevo { get; set; }

        /// <summary>
        /// ID del nodo padre (null si es ra�z)
        /// </summary>
        public int? ParentID
        {
            get { return _parentId; }
            set
            {
                if (_parentId != value)
                {
                    _parentId = value;
                    OnPropertyChanged(nameof(ParentID));
                }
            }
        }

        /// <summary>
        /// Nombre del nodo
        /// </summary>
        public string Nombre
        {
            get { return _nombre; }
            set
            {
                if (_nombre != value)
                {
                    _nombre = value;
                    OnPropertyChanged(nameof(Nombre));
                }
            }
        }

        /// <summary>
        /// Descripci�n del nodo
        /// </summary>
        public string Descripcion
        {
            get { return _descripcion; }
            set
            {
                if (_descripcion != value)
                {
                    _descripcion = value;
                    OnPropertyChanged(nameof(Descripcion));
                }
            }
        }

        /// <summary>
        /// Orden de visualizaci�n del nodo
        /// </summary>
        public int Orden
        {
            get { return _orden; }
            set
            {
                if (_orden != value)
                {
                    _orden = value;
                    OnPropertyChanged(nameof(Orden));
                }
            }
        }

        /// <summary>
        /// Nivel en la jerarqu�a (0 = ra�z, 1 = sub-padre, 2 = hijo)
        /// </summary>
        public int Nivel
        {
            get { return _nivel; }
            set
            {
                if (_nivel != value)
                {
                    _nivel = value;
                    OnPropertyChanged(nameof(Nivel));
                    OnPropertyChanged(nameof(TipoNodo));
                }
            }
        }

        /// <summary>
        /// Indica si el nodo puede tener hijos
        /// </summary>
        public bool EsExpandible
        {
            get { return _esExpandible; }
            set
            {
                if (_esExpandible != value)
                {
                    _esExpandible = value;
                    OnPropertyChanged(nameof(EsExpandible));
                }
            }
        }

        /// <summary>
        /// Lista de nodos hijos
        /// </summary>
        public List<NodoTree> Hijos
        {
            get { return _hijos ?? (_hijos = new List<NodoTree>()); }
            set
            {
                _hijos = value;
                OnPropertyChanged(nameof(Hijos));
                OnPropertyChanged(nameof(TieneHijos));
            }
        }

        /// <summary>
        /// Indica si el nodo tiene hijos
        /// </summary>
        public bool TieneHijos
        {
            get { return _hijos != null && _hijos.Count > 0; }
        }

        /// <summary>
        /// Tipo de nodo (Padre, Sub-Padre, Hijo)
        /// </summary>
        public string TipoNodo
        {
            get
            {
                switch (Nivel)
                {
                    case 0: return "Padre";
                    case 1: return "Sub-Padre";
                    case 2: return "Hijo";
                    default: return $"Nivel {Nivel}";
                }
            }
        }

        /// <summary>
        /// Tipo de tarea (solo para nodos hijo - nivel 2)
        /// </summary>
        public TipoTarea TipoTarea
        {
            get { return _tipoTarea; }
            set
            {
                if (_tipoTarea != value)
                {
                    _tipoTarea = value;
                    OnPropertyChanged(nameof(TipoTarea));
                    OnPropertyChanged(nameof(TipoTareaTexto));
                }
            }
        }

        /// <summary>
        /// Texto descriptivo del tipo de tarea
        /// </summary>
        public string TipoTareaTexto
        {
            get
            {
                switch (TipoTarea)
                {
                    case TipoTarea.Material:
                        return "Material";
                    case TipoTarea.ManoDeObra:
                        return "Mano de Obra";
                    default:
                        return "-";
                }
            }
        }

        /// <summary>
        /// Columnas adicionales personalizables (Diccionario din�mico)
        /// </summary>
        public Dictionary<string, object> ColumnasAdicionales { get; set; }

        /// <summary>
        /// Fecha de creaci�n del nodo
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha de �ltima modificaci�n
        /// </summary>
        public DateTime? FechaModificacion { get; set; }

        /// <summary>
        /// Usuario que cre� el nodo
        /// </summary>
        public string UsuarioCreacion { get; set; }

        #endregion

        #region Constructor

        public NodoTree()
        {
            ColumnasAdicionales = new Dictionary<string, object>();
            FechaCreacion = DateTime.Now;
            TipoTarea = TipoTarea.Ninguno;
        }

        #endregion

        #region M�todos

        /// <summary>
        /// Obtiene el valor de una columna adicional
        /// </summary>
        public object ObtenerValorColumna(string nombreColumna)
        {
            if (ColumnasAdicionales.ContainsKey(nombreColumna))
                return ColumnasAdicionales[nombreColumna];
            return null;
        }

        /// <summary>
        /// Calcula el valor de una columna calculada
        /// </summary>
        public object CalcularValorColumna(ColumnaTreeList columna)
        {
            if (!columna.EsCalculada || columna.TipoOperacion == TipoOperacion.Ninguna)
                return ObtenerValorColumna(columna.Nombre);

            // Obtener valores de las columnas de origen
            object valor1 = ObtenerValorColumna(columna.ColumnaOrigen1);
            object valor2 = ObtenerValorColumna(columna.ColumnaOrigen2);

            // Si alg�n valor es nulo, retornar null
            if (valor1 == null || valor2 == null)
                return null;

            // Convertir a decimal para realizar operaciones
            decimal? num1 = ConvertirADecimal(valor1);
            decimal? num2 = ConvertirADecimal(valor2);

            if (!num1.HasValue || !num2.HasValue)
                return null;

            // Realizar la operaci�n seg�n el tipo
            decimal resultado = 0;
            switch (columna.TipoOperacion)
            {
                case TipoOperacion.Multiplicacion:
                    resultado = num1.Value * num2.Value;
                    break;
                case TipoOperacion.Suma:
                    resultado = num1.Value + num2.Value;
                    break;
                case TipoOperacion.Resta:
                    resultado = num1.Value - num2.Value;
                    break;
                case TipoOperacion.Division:
                    if (num2.Value == 0)
                        return null; // Evitar divisi�n por cero
                    resultado = num1.Value / num2.Value;
                    break;
                default:
                    return null;
            }

            // Guardar el resultado calculado en el diccionario para futuras referencias
            EstablecerValorColumna(columna.Nombre, resultado);

            return resultado;
        }

        /// <summary>
        /// Convierte un objeto a decimal
        /// </summary>
        private decimal? ConvertirADecimal(object valor)
        {
            if (valor == null)
                return null;

            if (valor is decimal)
                return (decimal)valor;

            if (valor is int)
                return (decimal)(int)valor;

            if (valor is double)
                return (decimal)(double)valor;

            if (valor is float)
                return (decimal)(float)valor;

            if (valor is long)
                return (decimal)(long)valor;

            // Intentar convertir desde string
            if (decimal.TryParse(valor.ToString(), out decimal result))
                return result;

            return null;
        }

        /// <summary>
        /// Establece el valor de una columna adicional
        /// </summary>
        public void EstablecerValorColumna(string nombreColumna, object valor)
        {
            if (ColumnasAdicionales.ContainsKey(nombreColumna))
                ColumnasAdicionales[nombreColumna] = valor;
            else
                ColumnasAdicionales.Add(nombreColumna, valor);

            OnPropertyChanged(nombreColumna);
        }

        /// <summary>
        /// Agrega un nodo hijo
        /// </summary>
        public void AgregarHijo(NodoTree hijo)
        {
            hijo.ParentID = this.ID;
            hijo.Nivel = this.Nivel + 1;
            Hijos.Add(hijo);
            OnPropertyChanged(nameof(Hijos));
            OnPropertyChanged(nameof(TieneHijos));
        }

        /// <summary>
        /// Elimina un nodo hijo
        /// </summary>
        public bool EliminarHijo(NodoTree hijo)
        {
            bool resultado = Hijos.Remove(hijo);
            if (resultado)
            {
                OnPropertyChanged(nameof(Hijos));
                OnPropertyChanged(nameof(TieneHijos));
            }
            return resultado;
        }

        /// <summary>
        /// Obtiene todos los descendientes del nodo (recursivo)
        /// </summary>
        public List<NodoTree> ObtenerTodosLosDescendientes()
        {
            var lista = new List<NodoTree>();
            foreach (var hijo in Hijos)
            {
                lista.Add(hijo);
                lista.AddRange(hijo.ObtenerTodosLosDescendientes());
            }
            return lista;
        }

        /// <summary>
        /// Clona el nodo (sin hijos)
        /// </summary>
        public NodoTree Clonar()
        {
            return new NodoTree
            {
                Nombre = this.Nombre,
                Descripcion = this.Descripcion,
                Nivel = this.Nivel,
                EsExpandible = this.EsExpandible,
                ColumnasAdicionales = new Dictionary<string, object>(this.ColumnasAdicionales)
            };
        }

        public override string ToString()
        {
            return $"{Nombre} ({TipoNodo})";
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// Definici�n de una columna personalizada del TreeList
    /// </summary>
    public class ColumnaTreeList
    {
        public string Nombre { get; set; }
        public string Titulo { get; set; }
        public int Ancho { get; set; }
        public Type TipoDato { get; set; }
        public bool EsEditable { get; set; }
        public string Formato { get; set; }

        /// <summary>
        /// Indica si la columna es calculada (resultado de una operaci�n)
        /// </summary>
        public bool EsCalculada { get; set; }

        /// <summary>
        /// Tipo de operaci�n para columnas calculadas
        /// </summary>
        public TipoOperacion TipoOperacion { get; set; }

        /// <summary>
        /// Nombre de la primera columna de origen para el c�lculo
        /// </summary>
        public string ColumnaOrigen1 { get; set; }

        /// <summary>
        /// Nombre de la segunda columna de origen para el c�lculo
        /// </summary>
        public string ColumnaOrigen2 { get; set; }

        public ColumnaTreeList()
        {
            Ancho = 100;
            TipoDato = typeof(string);
            EsEditable = true;
            EsCalculada = false;
            TipoOperacion = TipoOperacion.Ninguna;
        }
    }

    /// <summary>
    /// Tipo de operaci�n para columnas calculadas
    /// </summary>
    public enum TipoOperacion
    {
        /// <summary>
        /// Sin operaci�n (columna normal)
        /// </summary>
        Ninguna = 0,

        /// <summary>
        /// Multiplicaci�n de dos columnas
        /// </summary>
        Multiplicacion = 1,

        /// <summary>
        /// Suma de dos columnas
        /// </summary>
        Suma = 2,

        /// <summary>
        /// Resta de dos columnas
        /// </summary>
        Resta = 3,

        /// <summary>
        /// Divisi�n de dos columnas
        /// </summary>
        Division = 4
    }
}
