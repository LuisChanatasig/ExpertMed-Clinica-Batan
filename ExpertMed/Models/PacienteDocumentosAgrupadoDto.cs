using System.Text.Json;

namespace ExpertMed.Models
{
    public class PacienteDocumentosAgrupadoDto
    {
        public int PacienteId { get; set; }
        public string Paciente { get; set; }
        public string MisDocumentos { get; set; } // Recibe el string JSON de SQL

        // Propiedad calculada para acceder a los documentos como lista
        // Dentro del DTO, para asegurar que lea bien lo que viene de SQL:
        public List<DocumentoDetalleDto> ListaDocumentos
        {
            get
            {
                if (string.IsNullOrEmpty(MisDocumentos)) return new List<DocumentoDetalleDto>();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<DocumentoDetalleDto>>(MisDocumentos, options);
            }
        }
    }

    public class DocumentoDetalleDto
    {
        public int DocumentoId { get; set; }
        public string NombreArchivo { get; set; }
        public string RutaFisica { get; set; } // <--- AGREGAR ESTA PROPIEDAD
        public string TipoDocumento { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
