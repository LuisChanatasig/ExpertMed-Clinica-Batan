namespace ExpertMed.Models
{
    public class VitalSignInputModel
    {
        public int AppointmentId { get; set; } // Nuevo campo para validar que solo se inserte una vez por cita
        public int PatientId { get; set; }
        public decimal Temperature { get; set; }
        public int RespiratoryRate { get; set; }
        public string BloodPressureAS { get; set; }
        public string BloodPressureDIS { get; set; }
        public string Pulse { get; set; }
        public string Weight { get; set; }
        public string Size { get; set; }
        public int CreatedBy { get; set; }
    }


}
