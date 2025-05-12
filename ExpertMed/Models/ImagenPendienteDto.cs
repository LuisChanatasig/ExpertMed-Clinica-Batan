namespace ExpertMed.Models
{
    public class ImagenPendienteDto
    {
        public int ImagesId { get; set; }
        public int ImagesSequential { get; set; }
        public string ImagesAmount { get; set; }
        public string ImagesObservation { get; set; }
        public int ImagesStatus { get; set; }
        public int ImagesConsultationId { get; set; }
        public string EstadoTexto { get; set; }

        public string NombreExamen { get; set; }
        public DateTime FechaSolicitud { get; set; }

        public string NombrePaciente { get; set; }
        public string CedulaPaciente { get; set; }
        public string CodigoPaciente { get; set; }
        public int? SeguroId { get; set; }
        public string NombreSeguro { get; set; }

        public int MedicoId { get; set; }
        public string NombreMedico { get; set; }
    }

}
