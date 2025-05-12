using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class DiagnosisConsultation
{
    public int DiagnosisId { get; set; }

    public int? DiagnosisConsultationid { get; set; }

    public int? DiagnosisDiagnosisid { get; set; }

    public string? DiagnosisObservation { get; set; }

    public bool? DiagnosisPresumptive { get; set; }

    public bool? DiagnosisDefinitive { get; set; }

    public int? DiagnosisSequential { get; set; }

    public int? DiagnosisStatus { get; set; }

    public virtual Consultation? DiagnosisConsultationNavigation { get; set; }

    public virtual Diagnosis? DiagnosisDiagnosis { get; set; }
}
