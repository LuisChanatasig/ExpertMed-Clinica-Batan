using ExpertMed.Models;

namespace ExpertMed.Models
{
    public class UserViewModel
    {
        public int UsersId { get; set; }

        public string UserDocumentNumber { get; set; } = null!;

        public string UserNames { get; set; } = null!;

        public string UserSurnames { get; set; } = null!;

        public string UserPhone { get; set; } = null!;

        public string UserEmail { get; set; } = null!;

        public DateTime? UserCreationdate { get; set; }

        public DateTime? UserModificationdate { get; set; }

        public string UserAddress { get; set; } = null!;

        public byte[]? UserDigitalsignature { get; set; }

        public byte[]? UserProfilephoto { get; set; }

        public string? UserPrfilephoto64 { get; set; }

        public string? UserSenecytcode { get; set; }

        public string? UserXkeytaxo { get; set; }

        public string? UserXpasstaxo { get; set; }

        public int? UserSequentialBilling { get; set; }

        public string UserLogin { get; set; } = null!;

        public string UserPassword { get; set; } = null!;

        public int UserStatus { get; set; }

        public int? UserProfileid { get; set; }

        public int? UserEstablishmentId { get; set; }

        public string? UsersEstablishmentName { get; set; }

        public string? UsersEstablishmentAddress { get; set; }

        public string? UsersEstablishmentEmissionpoint { get; set; }

        public string? UsersEstablishmentPointofsale { get; set; }

        public int? UserSpecialtyid { get; set; }

        public int? UserCountryid { get; set; }

        public string? UserDescription { get; set; }

        public int? UserVatpercentageid { get; set; }


        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public int AppointmentInterval { get; set; }

        public string? WorksDays { get; set; }

        // Estos no son IFormFile, son datos procesados
        public IFormFile? CompanyLogo { get; set; } // <- OJO: nullable
        public IFormFile? CertificateP12 { get; set; } // <- OJO: nullable

        public List<int> SelectedOfficeIds { get; set; }
        public int AssignedBy { get; set; }

        public byte[]? CompanyLogoBytes { get; set; }
        public string? CompanyLogoFileName { get; set; }
        public string? CompanyLogoContentType { get; set; }

        public byte[]? CertificateP12Bytes { get; set; }
        public string? CertificateP12FileName { get; set; }
        public string? CertificateP12ContentType { get; set; }


        public virtual ICollection<AssistantDoctorRelationship> AssistantDoctorRelationshipAssistantUsers { get; set; } = new List<AssistantDoctorRelationship>();

        public virtual ICollection<AssistantDoctorRelationship> AssistantDoctorRelationshipDoctorUsers { get; set; } = new List<AssistantDoctorRelationship>();


        public virtual VatBilling? UserVatpercentage { get; set; }
    }
}
