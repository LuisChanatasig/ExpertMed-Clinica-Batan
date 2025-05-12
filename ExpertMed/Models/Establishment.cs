using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class Establishment
{
    public int EstablishmentId { get; set; }

    public string EstablishmentName { get; set; } = null!;

    public string? EstablishmentAddress { get; set; }

    public string? EstablishmentEmissionpoint { get; set; }

    public string? EstablishmentPointofsale { get; set; }

    public DateTime? EstablishmentCreationdate { get; set; }

    public DateTime? EstablishmentModificationdate { get; set; }

    public int? EstablishmentSequentialBilling { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
