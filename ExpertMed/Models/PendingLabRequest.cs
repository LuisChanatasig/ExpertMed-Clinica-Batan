namespace ExpertMed.Models
{
    public class PendingLabRequest
    {
        public int LaboratoriesId { get; set; }
        public int LaboratoriesSequential { get; set; }
        public int LaboratoriesConsultationId { get; set; }
        public string LaboratoriesAmount { get; set; }
        public string LaboratoriesObservation { get; set; }
        public int LaboratoriesStatus { get; set; }
        public string EstadoTexto { get; set; }

        public string NombreExamen { get; set; }
        public DateTime FechaSolicitud { get; set; }

        public string NombrePaciente { get; set; }
        public string CedulaPaciente { get; set; }

        public int MedicoId { get; set; }

        public string NombreMedico { get; set; }
        public string NombreSeguro { get; set; }
        public string CodigoPaciente { get; set; }
    }

}
