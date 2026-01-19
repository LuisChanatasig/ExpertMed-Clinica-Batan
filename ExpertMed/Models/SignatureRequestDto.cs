namespace ExpertMed.Models
{
    public sealed class SignatureRequestDto
    {
        public Guid Token { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }

    public sealed class SignatureStatusDto
    {
        public byte Status { get; set; } = 0;       // 0 Pending, 1 Signed, 2 Expired, 3 Consumed/Cancelled
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? SignedAtLocal { get; set; }
        public string? SignatureDataUrl { get; set; }
    }

    public sealed class SignVm
    {
        public Guid Token { get; set; }
        public DateTime ExpiresAt { get; set; }   // se puede mostrar local
        public byte Status { get; set; }
    }

}
