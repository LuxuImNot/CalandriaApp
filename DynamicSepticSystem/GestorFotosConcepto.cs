using System;
using System.Collections.Generic;

namespace DynamicSepticSystem
{
    /// <summary>
    /// Gestiona las fotos adjuntas a un concepto o partida de una casa específica
    /// (Manzana + Lote + Identificador del nodo).
    ///
    /// Migrado a Web API: el acceso a datos vive en FotosConceptoController
    /// (api/fotos-concepto), que también asegura la tabla FotosConcepto. Las firmas
    /// públicas se conservan para no tocar los forms que la consumen. Los
    /// constructores conservan su firma por compatibilidad pero ya no usan la
    /// cadena de conexión.
    /// </summary>
    public class GestorFotosConcepto
    {
        public GestorFotosConcepto()
        {
        }

        public GestorFotosConcepto(string connString)
        {
        }

        /// <summary>
        /// Se conserva por compatibilidad. La tabla la asegura el servidor en cada
        /// operación, así que aquí no hace nada.
        /// </summary>
        public void AsegurarTabla()
        {
            // El servidor (FotosConceptoController) asegura la tabla.
        }

        /// <summary>
        /// Guarda una foto asociada a un concepto/partida de una casa.
        /// </summary>
        public int GuardarFoto(string manzana, string lote, string identificador, bool esConcepto,
            string nombreNodo, string descripcion, byte[] foto, string extension, string usuario = "Sistema")
        {
            if (string.IsNullOrWhiteSpace(manzana) || string.IsNullOrWhiteSpace(lote))
                throw new ArgumentException("Manzana y Lote son obligatorios");

            if (string.IsNullOrWhiteSpace(identificador))
                throw new ArgumentException("El identificador del concepto es obligatorio");

            if (foto == null || foto.Length == 0)
                throw new ArgumentException("La foto no puede estar vacía");

            if (foto.Length > 10 * 1024 * 1024) // 10 MB
                throw new ArgumentException("La foto no puede superar los 10 MB");

            return ApiClient.Post<int>("/api/fotos-concepto", new
            {
                Manzana = manzana,
                Lote = lote,
                Identificador = identificador,
                EsConcepto = esConcepto,
                NombreNodo = nombreNodo,
                Descripcion = descripcion,
                FotoBase64 = Convert.ToBase64String(foto),
                Extension = extension ?? "jpg",
                Usuario = usuario ?? "Sistema"
            });
        }

        /// <summary>
        /// Lista las fotos (sin el binario) asociadas a un concepto/partida.
        /// </summary>
        public List<FotoConceptoInfo> ListarFotos(string manzana, string lote, string identificador, bool esConcepto)
        {
            var dtos = ApiClient.Get<List<FotoConceptoApi>>(
                "/api/fotos-concepto" + QueryNodo(manzana, lote, identificador, esConcepto));

            var lista = new List<FotoConceptoInfo>();
            if (dtos != null)
                foreach (var dto in dtos)
                    lista.Add(MapDto(dto));
            return lista;
        }

        /// <summary>
        /// Obtiene el binario completo de una foto.
        /// </summary>
        public byte[] ObtenerFoto(int id)
        {
            return ApiClient.GetBytes($"/api/fotos-concepto/{id}/foto");
        }

        /// <summary>
        /// Cuenta las fotos asociadas a un concepto/partida.
        /// </summary>
        public int ContarFotos(string manzana, string lote, string identificador, bool esConcepto)
        {
            return ApiClient.Get<int>(
                "/api/fotos-concepto/contar" + QueryNodo(manzana, lote, identificador, esConcepto));
        }

        /// <summary>
        /// Actualiza la descripción de una foto.
        /// </summary>
        public void ActualizarDescripcion(int id, string descripcion)
        {
            ApiClient.Post($"/api/fotos-concepto/{id}/descripcion", new { Descripcion = descripcion });
        }

        /// <summary>
        /// Elimina una foto.
        /// </summary>
        public bool EliminarFoto(int id)
        {
            return ApiClient.Post<bool>($"/api/fotos-concepto/{id}/eliminar", null);
        }

        private static string QueryNodo(string manzana, string lote, string identificador, bool esConcepto)
        {
            return $"?manzana={Uri.EscapeDataString(manzana ?? "")}" +
                   $"&lote={Uri.EscapeDataString(lote ?? "")}" +
                   $"&identificador={Uri.EscapeDataString(identificador ?? "")}" +
                   $"&esConcepto={(esConcepto ? "true" : "false")}";
        }

        private static FotoConceptoInfo MapDto(FotoConceptoApi dto)
        {
            return new FotoConceptoInfo
            {
                Id = dto.Id,
                Manzana = dto.Manzana,
                Lote = dto.Lote,
                Identificador = dto.Identificador,
                EsConcepto = dto.EsConcepto,
                NombreNodo = dto.NombreNodo ?? "",
                Descripcion = dto.Descripcion ?? "",
                Extension = dto.Extension ?? "jpg",
                TamanioKB = dto.TamanioKB,
                Fecha = dto.Fecha,
                Usuario = dto.Usuario ?? "Sistema"
            };
        }
    }

    /// <summary>
    /// Información de una foto adjunta a un concepto/partida.
    /// </summary>
    public class FotoConceptoInfo
    {
        public int Id { get; set; }
        public string Manzana { get; set; }
        public string Lote { get; set; }
        public string Identificador { get; set; }
        public bool EsConcepto { get; set; }
        public string NombreNodo { get; set; }
        public string Descripcion { get; set; }
        public string Extension { get; set; }
        public double TamanioKB { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }

        public string TamanioFormateado =>
            TamanioKB < 1024 ? $"{TamanioKB:F1} KB" : $"{TamanioKB / 1024:F2} MB";

        public string FechaFormateada => Fecha.ToString("dd/MM/yyyy HH:mm");
    }
}
