namespace ExpertMed.Models
{
    public class AppointmentDTO
    {
        public int AppointmentId { get; set; }
        public DateTime? AppointmentCreateDate { get; set; }
        public DateTime? AppointmentModifyDate { get; set; }
        public int? AppointmentCreateUser { get; set; }
        public int? AppointmentModifyUser { get; set; }
        public int? AppointmentConsultationId { get; set; } // con ?
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentHour { get; set; }
        public int? AppointmentPatientId { get; set; }
        public int? AppointmentStatus { get; set; }
        public int? AppointmentPaymentStatus { get; set; }
        public int? AppointmentPaymentStatusLab { get; set; }

        public int? AppointmentMedicalofficeid { get; set; }
        public string? MedicalOfficeName { get; set; }

        public string? PatientName { get; set; }
        public string? PatientInsuranceName { get; set; }

        public bool IsCheckedIn { get; set; } // Representa el campo appointment_asistio
        public string? DoctorName { get; set; }
        public string? DoctorName2 { get; set; }
        public int? DoctorUserId { get; set; }

        public int? AppointmentInsuranceCompanyId { get; set; }
        public string? AppointmentReason { get; set; }
        public string? SpecialtyName { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal AmountToBill { get; set; }

        // Nueva propiedad para los laboratorios
        public bool HasLaboratories { get; set; }
        public List<InsuranceCompanyDto> InsuranceCompanies { get; set; }


        public Patient Patient { get; set; }  // For user details

        public List<User> Users { get; set; }  // For user details
    }

}
