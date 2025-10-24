namespace ExpertMed.Models
{
    public class PersonalBackground
    {
        public int PersonalBackgroundId { get; set; }

        public int? PersonalBackgroundConsultationid { get; set; }

        public bool? PersonalBackgroundHeartdisease { get; set; }
        public string? PersonalBackgroundHeartdiseaseObservation { get; set; }

        public bool? PersonalBackgroundHypertension { get; set; }
        public string? PersonalBackgroundHypertensionObservation { get; set; }

        public bool? PersonalBackgroundDxcardiovascular { get; set; }
        public string? PersonalBackgroundDxcardiovascularObservation { get; set; }

        public bool? PersonalBackgroundEndometabolic { get; set; }
        public string? PersonalBackgroundEndometabolicObservation { get; set; }

        public bool? PersonalBackgroundCancer { get; set; }
        public string? PersonalBackgroundCancerObservation { get; set; }

        public bool? PersonalBackgroundTuberculosis { get; set; }
        public string? PersonalBackgroundTuberculosisObservation { get; set; }

        public bool? PersonalBackgroundDxmental { get; set; }
        public string? PersonalBackgroundDxmentalObservation { get; set; }

        public bool? PersonalBackgroundDxinfectious { get; set; }
        public string? PersonalBackgroundDxinfectiousObservation { get; set; }

        public bool? PersonalBackgroundMalformation { get; set; }
        public string? PersonalBackgroundMalformationObservation { get; set; }

        public bool? PersonalBackgroundOther { get; set; }
        public string? PersonalBackgroundOtherObservation { get; set; }
    }
}
