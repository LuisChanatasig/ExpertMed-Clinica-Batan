namespace ExpertMed.Models
{
    public class MedicalOfficeListDto
    {
        public int MedicalOfficeId { get; set; }
        public int EstablishmentId { get; set; }
        public string? EstablishmentName { get; set; }
        public string Name { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public string? CreatedByName { get; set; }
    }
}
