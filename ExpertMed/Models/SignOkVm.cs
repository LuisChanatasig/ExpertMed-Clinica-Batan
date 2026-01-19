namespace ExpertMed.Models
{
    public class SignOkVm
    {
        public Guid Token { get; set; }
        public DateTime SignedAtUtc { get; set; }
        public string? ConsentVersion { get; set; }
    }

}

