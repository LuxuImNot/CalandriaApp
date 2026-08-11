using System.Collections.Generic;

namespace Calandria.Api.Models
{
    /// <summary>Una obra, tal como la ve el selector del cliente.</summary>
    public sealed class ObraDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        /// <summary>Nodos en RutaCalandraDestajo + RutaTuneraDestajo. En 0 junto con Insumos y Proveedores, la obra aún no tiene nada configurado.</summary>
        public int NodosArbol { get; set; }
        /// <summary>Filas en InsumosCalandraEXP + InsumosTuneraEXP.</summary>
        public int Insumos { get; set; }
        /// <summary>Filas en PROVEEDORESCALANDRIA.</summary>
        public int Proveedores { get; set; }
    }

    public sealed class CrearObraRequest
    {
        public string Nombre { get; set; }
    }

    public sealed class CrearObraResponseDto
    {
        public int Id { get; set; }
        public string NombreBD { get; set; }
    }

    /// <summary>Confirmación para borrar una obra: debe traer el nombre exacto de la obra.</summary>
    public sealed class EliminarObraRequest
    {
        public string NombreConfirmacion { get; set; }
    }

    /// <summary>Sube el logo de una obra; el servidor sugiere y guarda la paleta de 3 colores.</summary>
    public sealed class PersonalizarObraRequest
    {
        public string LogoBase64 { get; set; }
        public string Extension { get; set; }
    }

    /// <summary>Paleta + estado del logo de una obra, para acentuar sus pantallas web.</summary>
    public sealed class TemaObraDto
    {
        public string ColorPrimario { get; set; }
        public string ColorSecundario { get; set; }
        public string ColorSuave { get; set; }
        public bool TieneLogo { get; set; }
        public string LogoExtension { get; set; }
    }

    /// <summary>Obras asignadas a un usuario, para el checklist de Perfiles.</summary>
    public sealed class AsignacionObrasUsuarioDto
    {
        public string Usuario { get; set; }
        public List<int> ObraIds { get; set; }
    }

    public sealed class AsignarObrasRequest
    {
        public string Usuario { get; set; }
        public List<int> ObraIds { get; set; }
    }
}
