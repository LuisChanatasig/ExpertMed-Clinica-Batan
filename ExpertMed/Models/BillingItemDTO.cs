namespace ExpertMed.Models
{
    public class BillingItemDTO
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

}
