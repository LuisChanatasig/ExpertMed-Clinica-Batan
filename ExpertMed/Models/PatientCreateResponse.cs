namespace ExpertMed.Models
{
    public class PatientCreateResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public int PatientId { get; set; }
        public string? PatientCode { get; set; }

        public string? SecurityToken { get; set; }

        public string? SignatureData { get; set; }
        public string? SignedAt { get; set; }
    }
}
