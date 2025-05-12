namespace ExpertMed.Models
{
    public class ReporteLaboratorioDto
    {
        public int LaboratoryId { get; set; }
        public int ConsultationId { get; set; }
        public DateTime FechaConsulta { get; set; }

        public int MedicoId { get; set; }
        public string NombreMedico { get; set; }

        public int PacienteId { get; set; }
        public string NombrePaciente { get; set; }

        public int? SeguroId { get; set; }
        public string NombreSeguro { get; set; }

        public string Resultado { get; set; }
        public DateTime? FechaResultado { get; set; }
    }

}
