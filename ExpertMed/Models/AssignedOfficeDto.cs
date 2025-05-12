namespace ExpertMed.Models
{
    public class AssignedOfficeDto
    {
        public int AssignmentId { get; set; }
        public int UserId { get; set; }
        public string Medico { get; set; }
        public int MedicalOfficeId { get; set; }
        public string Consultorio { get; set; }
        public string Clinica { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime PrimeraAsignacion { get; set; }
        public string EstadoAsignacion { get; set; } // Activo, Inactivo, Mixto

        public List<int> ConsultorioIds { get; set; } = new();
        public bool AssignmentStatus { get; set; }

    }
}
