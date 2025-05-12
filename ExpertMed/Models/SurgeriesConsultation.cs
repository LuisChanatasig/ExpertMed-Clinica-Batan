using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class SurgeriesConsultation
{
    public int SurgeriesId { get; set; }

    public int? SurgeriesConsultationid { get; set; }

    public DateTime? SurgeriesCreationdate { get; set; }

    public int? SurgeriesCatalogid { get; set; }

    public string? SurgeriesObservation { get; set; }

    public int? SurgeriesStatus { get; set; }

    public virtual Catalog? SurgeriesCatalog { get; set; }

    public virtual Consultation? SurgeriesConsultationNavigation { get; set; }
}
