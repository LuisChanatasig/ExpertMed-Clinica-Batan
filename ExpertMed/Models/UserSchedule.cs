using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class UserSchedule
{
    public int SchudelsId { get; set; }

    public int UsersId { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int AppointmentInterval { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public int? SchudelsStatus { get; set; }

    public virtual ICollection<UserScheduleDay> UserScheduleDays { get; set; } = new List<UserScheduleDay>();
}
