namespace ExpertMed.Models
{
    public class TherapyHistoryDTO
    {
        public string CedulaPaciente { get; set; }
        public string NombrePaciente { get; set; }
        public string NombreSeguro { get; set; }
        public List<TherapyRequestDTO> Solicitudes { get; set; }
    }

}
