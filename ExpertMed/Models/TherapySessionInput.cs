namespace ExpertMed.Models
{
    public class TherapySessionInput
    {
        public string Tipo { get; set; }
        public string Fecha { get; set; } // Cambiado de DateTime
        public string Hora { get; set; }  // Cambiado de TimeSpan
        public int SesionesCont { get; set; }
        public string Observaciones { get; set; }
    }

}
