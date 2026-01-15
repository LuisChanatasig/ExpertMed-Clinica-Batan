using System.Text.Json.Serialization;

namespace ExpertMed.Models
{
    public class CreateImageDto
    {
        [JsonPropertyName("images_name")]
        public string images_name { get; set; }

        [JsonPropertyName("images_description")]
        public string images_description { get; set; }

        [JsonPropertyName("images_category")]
        public string images_category { get; set; }

        [JsonPropertyName("images_cie10")]
        public string images_cie10 { get; set; }

        [JsonPropertyName("images_status")]
        public int? images_status { get; set; } = 1;
    }

    public class ImageResponseDto
    {
        public int images_id { get; set; }
        public string images_name { get; set; }
        public string images_description { get; set; }
        public string images_category { get; set; }
        public string images_cie10 { get; set; }
        public int images_status { get; set; }
    }
}
