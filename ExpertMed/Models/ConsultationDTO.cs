namespace ExpertMed.Models
{
    public class ConsultationDTO
    {
        public int ConsultationId { get; set; }
        public DateTime ConsultationCreationDate { get; set; }
        public int ConsultationUserCreate { get; set; }
        public string ConsultationUserCreateName { get; set; }  // Nombre del médico
        public string ConsultationCreateLastName { get; set; }
        public int ConsultationPatient { get; set; }
        public string ConsultationPatientName { get; set; }
        public string ConsultationPatientMiddleName { get; set; }
        public string ConsultationPatientSurcename { get; set; }
        public string ConsultationPatientLastName { get; set; }
        public int ConsultationSpeciality { get; set; }
        public string ConsultationSpecialityName { get; set; }
        public string ConsultationHistoryClinic { get; set; }
        public string ConsultationSequential { get; set; }
        public string ConsultationReason { get; set; }
        public string ConsultationDisease { get; set; }
        public string ConsultationFamiliaryName { get; set; }
        public string ConsultationWarningSings { get; set; }
        public string ConsultationNonPharmacologycal { get; set; }
        public string ConsultationFamiliaryType { get; set; }
        public string ConsultationFamiliaryPhone { get; set; }
        public decimal? ConsultationTemperature { get; set; }
        public int? ConsultationRespirationRate { get; set; }
        public int? ConsultationBloodPressureDAS { get; set; }
        public int? ConsultationBloodPressureDIS { get; set; }
        public int? ConsultationPulse { get; set; }
        public decimal? ConsultationWeight { get; set; }
        public decimal? ConsultationSize { get; set; }
        public string ConsultationTreatmentPlan { get; set; }
        public string ConsultationObservation { get; set; }
        public string ConsultationPersonalBackground { get; set; }
        public int? ConsultationDisabilityDays { get; set; }
        public string ConsultationType { get; set; }
        public string ConsultationStatus { get; set; }
    }
}
