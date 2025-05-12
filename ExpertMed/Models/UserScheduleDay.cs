using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class UserScheduleDay
{
    public int ScheduleDayId { get; set; }

    public int ScheduleId { get; set; }

    public string WorkingDay { get; set; } = null!;

    public virtual UserSchedule Schedule { get; set; } = null!;
}
