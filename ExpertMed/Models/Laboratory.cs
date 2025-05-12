using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class Laboratory
{
    public int LaboratoriesId { get; set; }

    public string LaboratoriesName { get; set; } = null!;

    public string? LaboratoriesDescription { get; set; }

    public string? LaboratoriesCategory { get; set; }

    public string? LaboratoriesCie10 { get; set; }

    public int? LaboratoriesStatus { get; set; }

    public virtual ICollection<LaboratoriesConsultation> LaboratoriesConsultations { get; set; } = new List<LaboratoriesConsultation>();
}
