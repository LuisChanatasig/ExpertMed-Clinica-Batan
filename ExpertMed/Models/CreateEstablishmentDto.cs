namespace ExpertMed.Models
{
    public class CreateEstablishmentDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string EmissionPoint { get; set; }
        public string PointOfSale { get; set; }
        public int? SequentialBilling { get; set; }
        public IFormFile Logo { get; set; } // 👈 archivo del logo
    }

}
