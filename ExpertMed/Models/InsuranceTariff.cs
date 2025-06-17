using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ExpertMed.Models
{
    [Table("insurance_tariff")]
    public class InsuranceTariff
    {
        [Key]
        [Column("insurance_tariff_id")]
        public int insurance_tariff_id { get; set; }

        [Column("insurance_company_id")]
        public int insurance_company_id { get; set; }

        [Column("insurance_tariff_code")]
        public string insurance_tariff_code { get; set; }

        [Column("insurance_tariff_description")]
        public string insurance_tariff_description { get; set; }

        [Column("insurance_tariff_price")]
        public decimal insurance_tariff_price { get; set; }
    }

}
