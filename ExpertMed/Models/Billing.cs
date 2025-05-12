using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class Billing
{
    public int BillingId { get; set; }

    public int? AppointmentId { get; set; }

    public DateTime? BillingCreationdate { get; set; }

    public decimal? BillingTotal { get; set; }

    public string? BillingPaymentMethod { get; set; }

    public byte[]? BillingProofOfPayment { get; set; }

    public int? BillingSequential { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual ICollection<BillingDetail> BillingDetails { get; set; } = new List<BillingDetail>();
}
