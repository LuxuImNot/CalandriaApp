using System;

namespace Calandria.Api.Models
{
    /// <summary>
    /// Fila de la lista de trabajadores (combos de selección). Incluye los campos
    /// de texto que FormAsignarCuadrilla muestra al elegir un trabajador, para no
    /// necesitar una segunda llamada.
    /// </summary>
    public sealed class TrabajadorOpcionDto
    {
        public int Id { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Nacionalidad { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Curp { get; set; }
        public string Rfc { get; set; }
        public string Ine { get; set; }
        public string Nss { get; set; }
    }

    /// <summary>
    /// Detalle completo de un trabajador (FormRegistrarTrabajador / FormPerfilTrabajador).
    /// La foto viaja en base64 (preview inmediato); los PDF se descargan aparte
    /// vía /api/trabajadores/{id}/documento/{tipo} para no inflar esta respuesta.
    /// </summary>
    public sealed class TrabajadorDetalleDto
    {
        public int Id { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Nacionalidad { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Curp { get; set; }
        public string Rfc { get; set; }
        public string Ine { get; set; }
        public string Nss { get; set; }
        public string FotoBase64 { get; set; }
        public bool TieneCurp { get; set; }
        public int CurpKb { get; set; }
        public bool TieneRfc { get; set; }
        public int RfcKb { get; set; }
        public bool TieneIne { get; set; }
        public int IneKb { get; set; }
        public bool TieneNss { get; set; }
        public int NssKb { get; set; }
        /// <summary>Texto tipo "OFICIAL (jefe) · CD-000123" u "(sin cuadrilla asignada)".</summary>
        public string RolCuadrilla { get; set; }
    }

    /// <summary>
    /// Cuerpo de POST /api/trabajadores. Los campos binarios usan tri-estado:
    /// null = no tocar, "" = quitar el archivo, base64 = reemplazar.
    /// </summary>
    public sealed class GuardarTrabajadorRequest
    {
        public int? Id { get; set; }
        public string Nombre { get; set; }
        public string Nacionalidad { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Curp { get; set; }
        public string Rfc { get; set; }
        public string Ine { get; set; }
        public string Nss { get; set; }
        public string FotoBase64 { get; set; }
        public string PdfCurpBase64 { get; set; }
        public string PdfRfcBase64 { get; set; }
        public string PdfIneBase64 { get; set; }
        public string PdfNssBase64 { get; set; }
    }

    public sealed class ClaveNuevaDto
    {
        public string Clave { get; set; }
    }

    /// <summary>
    /// Cuerpo de POST /api/trabajadores/cuadrillas. Codigo vacío/null = cuadrilla
    /// nueva (se genera matrícula); si trae valor, reemplaza sus miembros.
    /// Reproduce FormAsignarCuadrilla.BtnGuardarCuadrilla_Click.
    /// </summary>
    public sealed class GuardarCuadrillaRequest
    {
        public string Codigo { get; set; }
        public System.Collections.Generic.List<MiembroCuadrillaDto> Miembros { get; set; }
    }

    public sealed class CuadrillaGuardadaDto
    {
        public string Codigo { get; set; }
    }
}
