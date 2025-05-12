namespace ExpertMed.Models
{
    public class AppointmentDTO
    {

        public int AppointmentId { get; set; }
        public DateTime AppointmentCreateDate { get; set; }
        public DateTime AppointmentModifyDate { get; set; }
        public int AppointmentCreateUser { get; set; }
        public int AppointmentModifyUser { get; set; }
        public int AppointmentConsultationId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentHour { get; set; }
        public int AppointmentPatientId { get; set; }
        public int AppointmentStatus { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
    }
}
