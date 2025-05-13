namespace ExpertMed.Models
{
    public class FavoriteMedicationDto
    {
        public int DoctorUserId { get; set; }
        public int MedicationId { get; set; }
        public string Amount { get; set; }
        public string? Observation { get; set; }
    }

}
