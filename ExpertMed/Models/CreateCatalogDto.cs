using System.Text.Json.Serialization;

namespace ExpertMed.Models
{
    public class CreateCatalogDto
    {
        [JsonPropertyName("catalog_name")]
        public string CatalogName { get; set; }

        [JsonPropertyName("catalog_category")]
        public string CatalogCategory { get; set; }

        [JsonPropertyName("category_status")]
        public int? CategoryStatus { get; set; } = 1;
    }

    public class CatalogResponseDto
    {
        public int CatalogId { get; set; }
        public string CatalogName { get; set; }
        public string CatalogCategory { get; set; }
        public int CategoryStatus { get; set; }
    }
}
