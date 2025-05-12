namespace ExpertMed.Models
{
    public class PatientDTO
    {
        public int PatientId { get; set; }
        public DateTime? PatientCreationDate { get; set; }
        public DateTime? PatientModificationDate { get; set; }
        public int? PatientCreationUser { get; set; }
        public int? PatientModificationUser { get; set; }
        public int? PatientDocumentType { get; set; }
        public string? PatientDocumentNumber { get; set; }
        public string? PatientFirstname { get; set; }
        public string? PatientMiddlename { get; set; }
        public string? PatientFirstsurname { get; set; }
        public string? PatientSecondlastname { get; set; }
        public int? PatientGender { get; set; }
        public DateTime? PatientBirthdate { get; set; }
        public int? PatientAge { get; set; }
        public int? PatientBloodtype { get; set; }
        public string? PatientDonor { get; set; }
        public int? PatientMaritalStatus { get; set; }
        public int? PatientVocationalTraining { get; set; }
        public string? PatientLandlinePhone { get; set; }
        public string? PatientCellularPhone { get; set; }
        public string? PatientEmail { get; set; }
        public int? PatientNationality { get; set; }
        public string? NationalityName { get; set; }
        public int? PatientProvince { get; set; }
        public string? PatientAddress { get; set; }
        public string? PatientOcupation { get; set; }
        public string? PatientCompany { get; set; }
        public int? PatientHealtInsurance { get; set; }
        public string? PatientInsuranceName { get; set; }
        public string? PatientCode { get; set; }
        public int PatientStatus { get; set; }
        public string? DoctorFullname { get; set; }
    }


}
