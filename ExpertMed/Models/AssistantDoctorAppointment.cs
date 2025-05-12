using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class AssistantDoctorAppointment
{
    public int AssistantUserid { get; set; }

    public int DoctorUserid { get; set; }

    public int AppointmentId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreationDate { get; set; }

    public int? RelationshipStatus { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual User AssistantUser { get; set; } = null!;

    public virtual User DoctorUser { get; set; } = null!;
}
