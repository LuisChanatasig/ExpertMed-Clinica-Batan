namespace ExpertMed.Models
{
    public class VitalSignInputModel
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }

        public decimal Temperature { get; set; }
        public int RespiratoryRate { get; set; }
        public string BloodPressureAS { get; set; }
        public string BloodPressureDIS { get; set; }
        public string Pulse { get; set; }

        public string Weight { get; set; }
        public string Size { get; set; }

        // Nuevos campos
        public decimal? Bmi { get; set; }
        public decimal? AbdominalPerimeter { get; set; }
        public decimal? CapillaryHemoglobin { get; set; }
        public decimal? CapillaryGlucose { get; set; }
        public decimal? Spo2 { get; set; }

        public int CreatedBy { get; set; }
    }
}
