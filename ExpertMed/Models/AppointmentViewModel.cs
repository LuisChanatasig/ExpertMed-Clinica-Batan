namespace ExpertMed.Models
{
    public class AppointmentViewModel
    {
        public int appointment_id { get; set; }
        public DateTime? appointment_createdate { get; set; }
        public DateTime? appointment_modifydate { get; set; }
        public int? appointment_createuser { get; set; }
        public int? appointment_modifyuser { get; set; }
        public DateTime appointment_date { get; set; }
        public TimeSpan appointment_hour { get; set; }
        public int? appointment_patientid { get; set; }
        public int? appointment_consultationid { get; set; }
        public int? appointment_status { get; set; }
        public string DoctorName { get; set; }
        public string DoctorName2 { get; set; }
        public int DoctorUserId { get; set; }
        public string PatientName { get; set; }
        public string PatientLastName { get; set; }
    }

}
