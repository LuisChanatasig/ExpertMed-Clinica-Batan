namespace ExpertMed.Models
{
    public class Users
    {
        public int UserId { get; set; }
        public string DocumentNumber { get; set; }
        public string Names { get; set; }
        public string Surnames { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ModificationDate { get; set; }
        public string Address { get; set; }
        public string ProfilePhoto { get; set; }
        public string ProfilePhoto64 { get; set; }
        public string SenecytCode { get; set; }
        public string XKeyTaxo { get; set; }
        public string XPassTaxo { get; set; }
        public int SequentialBilling { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int Status { get; set; }
        public int ProfileId { get; set; }
        public string EstablishmentName { get; set; }
        public string EstablishmentAddress { get; set; }
        public int VatPercentageId { get; set; }
        public int SpecialityId { get; set; }
        public int CountryId { get; set; }
        public string Description { get; set; }
        public string ProfileName { get; set; }
        public string SpecialityName { get; set; }
        public string CountryName { get; set; }
        public string VatBillingPercentage { get; set; }
    }
}
