namespace ExpertMed.Models
{
    public class CreateMedicationDto
    {
        public string medications_name { get; set; }
        public string medications_description { get; set; }
        public string medications_concentration { get; set; }
        public string medications_cie10 { get; set; }
        public int? medications_status { get; set; }
    }

    public class MedicationDto
    {
        public int medications_id { get; set; }
        public string medications_name { get; set; }
        public string medications_description { get; set; }
        public string medications_category { get; set; }
        public string medications_distinctive { get; set; }
        public string medications_concentration { get; set; }
        public string medications_cie10 { get; set; }
        public int medications_status { get; set; }
    }
}
