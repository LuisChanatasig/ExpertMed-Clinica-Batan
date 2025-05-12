using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class MedicationsConsultation
{
    public int MedicationsId { get; set; }

    public int? MedicationsConsultationid { get; set; }

    public int? MedicationsMedicationsid { get; set; }

    public string? MedicationsAmount { get; set; }

    public string? MedicationsObservation { get; set; }

    public int? MedicationsSequential { get; set; }

    public int? MedicationsStatus { get; set; }

    public virtual Consultation? MedicationsConsultationNavigation { get; set; }

    public virtual Medication? MedicationsMedications { get; set; }
}
