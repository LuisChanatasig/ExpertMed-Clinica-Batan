namespace ExpertMed.Models
{
    public class TherapyRequestDTO
    {
        public int RequestId { get; set; }
        public string TerapeutaNombre { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public List<TherapySessionDTO> Sesiones { get; set; }
    }
}
