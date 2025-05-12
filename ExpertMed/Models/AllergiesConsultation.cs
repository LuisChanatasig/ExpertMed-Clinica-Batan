using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class AllergiesConsultation
{
    public int AllergiesId { get; set; }

    public int? AllergiesConsultationid { get; set; }

    public DateTime? AllergiesCreationdate { get; set; }

    public int AllergiesCatalogid { get; set; }

    public string? AllergiesObservation { get; set; }

    public int? AllergiesStatus { get; set; }

    public virtual Catalog AllergiesCatalog { get; set; } = null!;

    public virtual Consultation? AllergiesConsultationNavigation { get; set; }
}
