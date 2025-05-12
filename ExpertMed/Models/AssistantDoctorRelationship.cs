using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class AssistantDoctorRelationship
{
    public int AssistandoctorId { get; set; }

    public DateTime? AssitandoctorDate { get; set; }

    public int RelationshipStatus { get; set; }

    public int? AssistantUserid { get; set; }

    public int? DoctorUserid { get; set; }

    public virtual User? AssistantUser { get; set; }

    public virtual User? DoctorUser { get; set; }
}
