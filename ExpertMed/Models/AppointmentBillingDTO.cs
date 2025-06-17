namespace ExpertMed.Models
{
    public class AppointmentBillingDTO
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentHour { get; set; }

        public int AppointmentStatus { get; set; }
        public int AppointmentPaymentStatus { get; set; }

        public int? AppointmentConsultationId { get; set; }
        public int? AppointmentMedicalOfficeId { get; set; }
        public int? AppointmentPatientId { get; set; }

        public int? InsuranceCompanyId { get; set; }
        public string? InsuranceCompanyName { get; set; }
        public string? InsuranceAuthCode { get; set; }

        public int PatientId { get; set; }
        public string PatientFullName { get; set; }
    }


}
