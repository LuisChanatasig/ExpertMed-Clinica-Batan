namespace ExpertMed.Models
{

    public class TherapySessionDTO
    {
        public int SessionId { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public string Tipo { get; set; }
        public string Observaciones { get; set; }
        public int AvancePorcentaje { get; set; }
        public string AvanceNotas { get; set; }
    }
}
