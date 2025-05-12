using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class Profile
{
    public int ProfileId { get; set; }

    public string ProfileName { get; set; } = null!;

    public string? ProfileDescription { get; set; }

    public int ProfileStatus { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
