namespace ExpertMed.Models
{
    public class EstablishmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string EmissionPoint { get; set; }
        public string PointOfSale { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public int? SequentialBilling { get; set; }
        public byte[] Logo { get; set; }
    }
}
