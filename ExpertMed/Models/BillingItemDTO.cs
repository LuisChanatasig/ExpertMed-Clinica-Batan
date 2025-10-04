namespace ExpertMed.Models
{
    public class BillingItemDTO
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
    public class PaymentMethodDTO
    {
        public string? PaymentMethod { get; set; }
        public decimal PaymentAmount { get; set; }
        public byte[]? PaymentProof { get; set; }
        public string? PaymentNotes { get; set; }
    }
}
