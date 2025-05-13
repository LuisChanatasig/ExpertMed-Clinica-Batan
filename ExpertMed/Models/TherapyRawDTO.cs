namespace ExpertMed.Models
{
    public class TherapyRawDTO
    {
        public string CedulaPaciente { get; set; }
        public string NombrePaciente { get; set; }
        public string NombreSeguro { get; set; }

        public int RequestId { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string TerapeutaNombre { get; set; }

        public int SessionId { get; set; }
        public DateTime FechaSesion { get; set; }
        public TimeSpan HoraSesion { get; set; }
        public string Tipo { get; set; }
        public string Observaciones { get; set; }
        public int AvancePorcentaje { get; set; }
        public string AvanceNotas { get; set; }
    }

}
