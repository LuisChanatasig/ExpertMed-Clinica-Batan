namespace ExpertMed.Models
{
    public class MedicDetails
    {
        public int UsersId { get; set; }

        public string UsersDocumentNumber { get; set; } = null!;

        public string? UsersNames { get; set; }

        public string UsersSurcenames { get; set; } = null!;

        public string UsersPhone { get; set; } = null!;

        public string UsersEmail { get; set; } = null!;

        public DateTime? UsersCreationdate { get; set; }

        public DateTime? UsersModificationdate { get; set; }

        public string UsersAddress { get; set; } = null!;

        public byte[]? UsersProfilephoto { get; set; }

        public string? UsersProfilephoto64 { get; set; }

        public string? UsersSenecytcode { get; set; }

        public string? UsersXkeytaxo { get; set; }

        public string? UsersXpasstaxo { get; set; }

        public int? UsersSequentialBilling { get; set; }

        public string UsersLogin { get; set; } = null!;

        public string? UsersPassword { get; set; }

        public string? UsersDescription { get; set; }
        public string? SpecialityName { get; set; }

        public string? UsersEstablishmentName { get; set; }

        public string? UsersEstablishmentAddress { get; set; }

        public string? UsersEstablishmentEmissionpoint { get; set; }

        public string? UsersEstablishmentPointofsale { get; set; }

        public int? UsersProfileid { get; set; }

        public int? UsersSpecialityid { get; set; }

        public int? UsersCountryid { get; set; }

        public int? UsersVatpercentageid { get; set; }

        public int? UsersStatus { get; set; }

        public virtual ICollection<Appointment> AppointmentAppointmentCreateuserNavigations { get; set; } = new List<Appointment>();

        public virtual ICollection<Appointment> AppointmentAppointmentModifyuserNavigations { get; set; } = new List<Appointment>();

        public virtual ICollection<AssistantDoctorAppointment> AssistantDoctorAppointmentAssistantUsers { get; set; } = new List<AssistantDoctorAppointment>();

        public virtual ICollection<AssistantDoctorAppointment> AssistantDoctorAppointmentDoctorUsers { get; set; } = new List<AssistantDoctorAppointment>();

        public virtual ICollection<AssistantDoctorRelationship> AssistantDoctorRelationshipAssistantUsers { get; set; } = new List<AssistantDoctorRelationship>();

        public virtual ICollection<AssistantDoctorRelationship> AssistantDoctorRelationshipDoctorUsers { get; set; } = new List<AssistantDoctorRelationship>();

        public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

        public virtual ICollection<DoctorPatient> DoctorPatients { get; set; } = new List<DoctorPatient>();

        public virtual ICollection<Patient> PatientPatientCreationuserNavigations { get; set; } = new List<Patient>();

        public virtual ICollection<Patient> PatientPatientModificationuserNavigations { get; set; } = new List<Patient>();

        public virtual Country? UsersCountry { get; set; }

        public virtual Profile? UsersProfile { get; set; }

        public virtual Speciality? UsersSpeciality { get; set; }

        public virtual VatBilling? UsersVatpercentage { get; set; }
    }
}
