using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class PhysicalExamination
{
    public int PhysicalexaminationId { get; set; }

    public int? PhysicalexaminationConsultationid { get; set; }

    public bool? PhysicalexaminationHead { get; set; }

    public string? PhysicalexaminationHeadObs { get; set; }

    public bool? PhysicalexaminationNeck { get; set; }

    public string? PhysicalexaminationNeckObs { get; set; }

    public bool? PhysicalexaminationChest { get; set; }

    public string? PhysicalexaminationChestObs { get; set; }

    public bool? PhysicalexaminationAbdomen { get; set; }

    public string? PhysicalexaminationAbdomenObs { get; set; }

    public bool? PhysicalexaminationPelvis { get; set; }

    public string? PhysicalexaminationPelvisObs { get; set; }

    public bool? PhysicalexaminationLimbs { get; set; }

    public string? PhysicalexaminationLimbsObs { get; set; }

    public virtual Consultation? PhysicalexaminationConsultation { get; set; }
}
