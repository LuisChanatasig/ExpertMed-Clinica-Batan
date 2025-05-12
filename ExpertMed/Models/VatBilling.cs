using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class VatBilling
{
    public int VatbillingId { get; set; }

    public string? VatbillingPercentage { get; set; }

    public string? VatbillingCode { get; set; }

    public string? VatbillingRate { get; set; }

    public int? VatbillingStatus { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
