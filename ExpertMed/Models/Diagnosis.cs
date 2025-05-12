using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class Diagnosis
{
    public int DiagnosisId { get; set; }

    public string DiagnosisName { get; set; } = null!;

    public string? DiagnosisDescription { get; set; }

    public string? DiagnosisCategory { get; set; }

    public string? DiagnosisCie10 { get; set; }

    public int? DiagnosisStatus { get; set; }

    public virtual ICollection<DiagnosisConsultation> DiagnosisConsultations { get; set; } = new List<DiagnosisConsultation>();
}
