using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class Country
{
    public int CountryId { get; set; }

    public string CountryName { get; set; } = null!;

    public string? CountryIso { get; set; }

    public string? CountryCode { get; set; }

    public string? CountryNationality { get; set; }

    public int? CountryStatus { get; set; }

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();

    public virtual ICollection<Province> Provinces { get; set; } = new List<Province>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
