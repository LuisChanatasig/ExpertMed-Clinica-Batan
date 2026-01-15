namespace ExpertMed.Models
{
    using System.Text.Json.Serialization;

    public class CreateLaboratoryDto
    {
        [JsonPropertyName("laboratories_name")]
        public string laboratories_name { get; set; }

        [JsonPropertyName("laboratories_description")]
        public string laboratories_description { get; set; }

        [JsonPropertyName("laboratories_cie10")]
        public string laboratories_cie10 { get; set; }
        [JsonPropertyName("laboratories_category")]
        public string laboratories_category { get; set; } = "LABORATORIO";

        [JsonPropertyName("laboratories_status")]
        public int? laboratories_status { get; set; } = 1;
    }

    public class LaboratoryDto
    {
        public int laboratories_id { get; set; }
        public string laboratories_name { get; set; }
        public string laboratories_description { get; set; }
        public string laboratories_category { get; set; }
        public string laboratories_cie10 { get; set; }
        public int laboratories_status { get; set; }
    }
}
