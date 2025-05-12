using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class Province
{
    public int ProvinceId { get; set; }

    public string ProvinceName { get; set; } = null!;

    public string? ProvinceDemony { get; set; }

    public string? ProvincePrefix { get; set; }

    public string? ProvinceCode { get; set; }

    public string? ProvinceIso { get; set; }

    public int ProvinceStatus { get; set; }

    public int? ProvinceCountryid { get; set; }

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();

    public virtual Country? ProvinceCountry { get; set; }
}
