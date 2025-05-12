namespace ExpertMed.Models
{
    public class UserWithDetails
    {
        public int ProfileId { get; set; }
        public int UserId { get; set; }
        public string DocumentNumber { get; set; }
        public string Names { get; set; }
        public string Surnames { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string Address { get; set; }
        public byte[] ProfilePhoto { get; set; }
        public string ProfilePhoto64 { get; set; }
        public string SenecytCode { get; set; }
        public string XKeyTaxo { get; set; }
        public string XPassTaxo { get; set; }
        public int SequentialBilling { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int Status { get; set; }
        public int? profileSelect { get; set; }
        public int? UserEstablishmentid { get; set; }
        public int? UserVatpercentageid { get; set; }
        public int? UserSpecialtyid { get; set; }
        public int? UserCountryid { get; set; }
        public string? UserDescription { get; set; }

        public string ProfileName { get; set; }
        public string EstablishmentName { get; set; }
        public string EstablishmentAddress { get; set; }
        public string EstablishmentPointofsale { get; set; }
        public string EstablishmentEmissionPoint { get; set; }
        public string SpecialtyName { get; set; }
        public string CountryName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int AppointmentInterval { get; set; }

        // 🔗 Asociaciones
        public List<DoctorDto> Doctors { get; set; } = new();
        public List<MedicalOfficeDto> MedicalOffices { get; set; } = new();
        public List<UserFileDto> UserFiles { get; set; } = new();

        // 🖼️ Imágenes y certificados extraídos de archivos
        public string? CompanyLogoBase64 =>
            UserFiles?.FirstOrDefault(f => f.FileType == "logotipo")?.FileContent is byte[] logo
                ? $"data:image/png;base64,{Convert.ToBase64String(logo)}"
                : null;

        public string? CertificateP12Base64 =>
            UserFiles?.FirstOrDefault(f => f.FileType == "certificado_p12")?.FileContent is byte[] cert
                ? $"data:application/x-pkcs12;base64,{Convert.ToBase64String(cert)}"
                : null;
    }

    public class DoctorDto
    {
        public int DoctorId { get; set; }
        public string DoctorNames { get; set; }
        public string DoctorSurnames { get; set; }
        public int DoctorSpecialtyId { get; set; }
        public string DoctorSpecialtyName { get; set; }
    }

    public class MedicalOfficeDto
    {
        public int OfficeId { get; set; }
        public string OfficeName { get; set; }
        public string OfficeLocation { get; set; }
    }

    public class UserFileDto
    {
        public string FileType { get; set; }
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
        public string ContentType { get; set; }
    }
}
