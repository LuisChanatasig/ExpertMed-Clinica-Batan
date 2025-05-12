using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class Speciality
{
    public int SpecialityId { get; set; }

    public string SpecialityName { get; set; } = null!;

    public string? SpecialtyDescription { get; set; }

    public string? SpecialityCategory { get; set; }

    public int? SpecialityStatus { get; set; }

    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
