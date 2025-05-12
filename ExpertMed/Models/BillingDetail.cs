using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class BillingDetail
{
    public int BillingDetailsId { get; set; }

    public int? BillingId { get; set; }

    public string? BillingDetailsNames { get; set; }

    public string? BillingDetailsCinumber { get; set; }

    public string? BillingDetailsDocumenttype { get; set; }

    public string? BillingDetailsAddress { get; set; }

    public string? BillingDetailsPhone { get; set; }

    public string? BillingDetailsEmail { get; set; }

    public virtual Billing? Billing { get; set; }
}
