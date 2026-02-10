using System.Text.Json.Serialization;

namespace ExpertMed.Models
{
    public class CreateDiagnosisDto
    {
        [JsonPropertyName("diagnosis_name")]
        public string diagnosis_name { get; set; }

        [JsonPropertyName("diagnosis_description")]
        public string diagnosis_description { get; set; }

        [JsonPropertyName("diagnosis_category")]
        public string diagnosis_category { get; set; } = "Diagnostico";

        [JsonPropertyName("diagnosis_cie10")]
        public string diagnosis_cie10 { get; set; }

        [JsonPropertyName("diagnosis_status")]
        public int? diagnosis_status { get; set; } = 1;
    }

    public class DiagnosisDto
    {
        public int diagnosis_id { get; set; }
        public string diagnosis_name { get; set; }
        public string diagnosis_description { get; set; }
        public string diagnosis_category { get; set; }
        public string diagnosis_cie10 { get; set; }
        public int diagnosis_status { get; set; }
    }
}
