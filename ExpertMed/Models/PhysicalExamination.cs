using System;
using System.Collections.Generic;

namespace ExpertMed.Models
{
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

        // ✅ Nuevos campos agregados por tu SP

        public bool? PhysicalexaminationSkinfaneras { get; set; }
        public string? PhysicalexaminationSkinfanerasObs { get; set; }

        public bool? PhysicalexaminationEyes { get; set; }
        public string? PhysicalexaminationEyesObs { get; set; }

        public bool? PhysicalexaminationEars { get; set; }
        public string? PhysicalexaminationEarsObs { get; set; }

        public bool? PhysicalexaminationNose { get; set; }
        public string? PhysicalexaminationNoseObs { get; set; }

        public bool? PhysicalexaminationMouth { get; set; }
        public string? PhysicalexaminationMouthObs { get; set; }

        public bool? PhysicalexaminationOropharynx { get; set; }
        public string? PhysicalexaminationOropharynxObs { get; set; }

        public bool? PhysicalexaminationAxilasmamas { get; set; }
        public string? PhysicalexaminationAxilasmamasObs { get; set; }

        public bool? PhysicalexaminationSpine { get; set; }
        public string? PhysicalexaminationSpineObs { get; set; }

        public bool? PhysicalexaminationIngleperine { get; set; }
        public string? PhysicalexaminationIngleperineObs { get; set; }

        public bool? PhysicalexaminationUpperlimbs { get; set; }
        public string? PhysicalexaminationUpperlimbsObs { get; set; }

        public bool? PhysicalexaminationLowerlimbs { get; set; }
        public string? PhysicalexaminationLowerlimbsObs { get; set; }

        public virtual Consultation? PhysicalexaminationConsultation { get; set; }
    }
}
