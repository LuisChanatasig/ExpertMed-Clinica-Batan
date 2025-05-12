using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class ImagesConsutlation
{
    public int ImagesId { get; set; }

    public int? ImagesConsultationid { get; set; }

    public int? ImagesImagesid { get; set; }

    public string? ImagesAmount { get; set; }

    public string? ImagesObservation { get; set; }

    public int? ImagesSequential { get; set; }

    public int? ImagesStatus { get; set; }

    public virtual Consultation? ImagesConsultation { get; set; }

    public virtual Image? ImagesImages { get; set; }
}
