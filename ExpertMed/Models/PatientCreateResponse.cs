namespace ExpertMed.Models
{
    public class PatientCreateResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int PatientId { get; set; }
        public string? PatientCode { get; set; }

    }
}