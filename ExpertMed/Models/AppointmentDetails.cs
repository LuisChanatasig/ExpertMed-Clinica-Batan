namespace ExpertMed.Models
{
    public class AppointmentDetails
    {

        public int AppointmentId { get; set; }
        public DateTime AppointmentCreatedate { get; set; }
        public DateTime AppointmentModifydate { get; set; }
        public int AppointmentCreateuser { get; set; }
        public int AppointmentModifyuser { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeOnly AppointmentHour { get; set; }
        public int AppointmentPatientid { get; set; }
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public string DoctorFirstName { get; set; }
        public string DoctorLastName { get; set; }
        public int DoctorSpecialty { get; set; }
        public int AppointmentStatus { get; set; }
        public int AppointmentConsultationid { get; set; }
    }
}
