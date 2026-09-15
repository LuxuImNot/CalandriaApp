using System;
using System.Collections.Generic;

namespace Calandria.Api.Models
{
    /// <summary>
    /// Un nodo del árbol de 3 niveles (Padre=0, Sub-Padre=1, Hijo=2) que edita
    /// FormEditorTreeList. Espejo de NodoTree.cs del cliente clásico; las
    /// columnas personalizadas (Precio, Unidad, etc.) viajan aplanadas en
    /// <see cref="Valores"/> igual que se guardan en "{tabla}_Columnas".
    /// </summary>
    public class NodoEditorDto
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Orden { get; set; }
        public int Nivel { get; set; }

        /// <summary>0=Ninguno, 1=Material, 2=ManoDeObra (solo aplica a Nivel 2).</summary>
        public int TipoTarea { get; set; }
        public string TipoTareaTexto { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string UsuarioCreacion { get; set; }

        /// <summary>Columnas personalizadas del nodo, siempre como texto (mismo formato que "{tabla}_Columnas.Valor").</summary>
        public Dictionary<string, string> Valores { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>Definición de una columna personalizada. Espejo de ColumnaTreeList.cs.</summary>
    public class ColumnaDefDto
    {
        public string Nombre { get; set; }
        public string Titulo { get; set; }
        public int Ancho { get; set; }

        /// <summary>Nombre completo del tipo .NET (p. ej. "System.String", "System.Decimal").</summary>
        public string TipoDato { get; set; }
        public bool EsEditable { get; set; }
        public string Formato { get; set; }

        public bool EsCalculada { get; set; }
        /// <summary>0=Ninguna, 1=Multiplicación, 2=Suma, 3=Resta, 4=División.</summary>
        public int TipoOperacion { get; set; }
        public string ColumnaOrigen1 { get; set; }
        public string ColumnaOrigen2 { get; set; }
    }

    /// <summary>Respuesta de GET api/editor-tareas/arbol: todo lo que la pantalla necesita.</summary>
    public class ArbolEditorDto
    {
        public string Ruta { get; set; }
        public List<NodoEditorDto> Nodos { get; set; } = new List<NodoEditorDto>();
        public List<ColumnaDefDto> Columnas { get; set; } = new List<ColumnaDefDto>();
    }

    /// <summary>Un nodo tal como lo manda el editor web al guardar.</summary>
    public class NodoEditorRequest
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Orden { get; set; }
        public int Nivel { get; set; }
        public int TipoTarea { get; set; }

        /// <summary>true si el nodo se creó en esta sesión y aún no existe en la BD (permite reasignar su ID si hay colisión con otra sesión concurrente).</summary>
        public bool EsNuevo { get; set; }

        public Dictionary<string, string> Valores { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Guardado atómico de TODO el editor (árbol + definiciones de columnas) en
    /// una sola transacción — a diferencia del cliente clásico, donde las
    /// columnas se comprometían solas por cada acción del diálogo.
    /// </summary>
    public class GuardarArbolRequest
    {
        public string Ruta { get; set; }
        public List<NodoEditorRequest> Nodos { get; set; } = new List<NodoEditorRequest>();
        public List<ColumnaDefDto> Columnas { get; set; } = new List<ColumnaDefDto>();
    }

    /// <summary>Respuesta del guardado: si algún ID nuevo chocó con otra sesión, aquí va el mapeo viejo→nuevo para que el cliente actualice su estado.</summary>
    public class GuardarArbolResponseDto
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
        public Dictionary<int, int> IdsReasignados { get; set; } = new Dictionary<int, int>();
    }
}
