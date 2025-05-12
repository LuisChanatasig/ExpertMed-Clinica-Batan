using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class DoctorPatient
{
    public int DoctorPatientId { get; set; }

    public int DoctorUserid { get; set; }

    public int PatientId { get; set; }

    public DateTime? CreationDate { get; set; }

    public int CreatedBy { get; set; }

    public int RelationshipStatus { get; set; }

    public virtual User DoctorUser { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;
}
