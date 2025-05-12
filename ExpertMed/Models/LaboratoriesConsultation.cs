using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class LaboratoriesConsultation
{
    public int LaboratoriesId { get; set; }

    public int? LaboratoriesConsultationid { get; set; }

    public int? LaboratoriesLaboratoriesid { get; set; }

    public string? LaboratoriesAmount { get; set; }

    public string? LaboratoriesObservation { get; set; }

    public int? LaboratoriesSequential { get; set; }

    public int? LaboratoriesStatus { get; set; }

    public virtual Consultation? LaboratoriesConsultationNavigation { get; set; }

    public virtual Laboratory? LaboratoriesLaboratories { get; set; }
}
