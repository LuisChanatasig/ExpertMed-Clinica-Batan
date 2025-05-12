using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class Image
{
    public int ImagesId { get; set; }

    public string ImagesName { get; set; } = null!;

    public string? ImagesDescription { get; set; }

    public string? ImagesCategory { get; set; }

    public string? ImagesCie10 { get; set; }

    public int? ImagesStatus { get; set; }

    public virtual ICollection<ImagesConsutlation> ImagesConsutlations { get; set; } = new List<ImagesConsutlation>();
}
