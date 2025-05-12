using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class Medication
{
    public int MedicationsId { get; set; }

    public string MedicationsName { get; set; } = null!;

    public string? MedicationsDescription { get; set; }

    public string? MedicationsCategory { get; set; }

    public string? MedicationsDistinctive { get; set; }

    public string? MedicationsConcentration { get; set; }

    public string? MedicationsCie10 { get; set; }

    public int? MedicationsStatus { get; set; }

    public virtual ICollection<MedicationsConsultation> MedicationsConsultations { get; set; } = new List<MedicationsConsultation>();
}
