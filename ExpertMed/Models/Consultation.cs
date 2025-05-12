using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class Consultation
{
    public int ConsultationId { get; set; }

    public DateTime? ConsultationCreationdate { get; set; }

    public int? ConsultationUsercreate { get; set; }

    public int ConsultationPatient { get; set; }

    public int? ConsultationSpeciality { get; set; }

    public string ConsultationHistoryclinic { get; set; } = null!;

    public int? ConsultationSequential { get; set; }

    public string? ConsultationReason { get; set; }

    public string? ConsultationDisease { get; set; }

    public string? ConsultationFamiliaryname { get; set; }

    public string? ConsultationWarningsings { get; set; }

    public string? ConsultationNonpharmacologycal { get; set; }

    public int? ConsultationFamiliarytype { get; set; }

    public string? ConsultationFamiliaryphone { get; set; }

    public string? ConsultationTemperature { get; set; }

    public string? ConsultationRespirationrate { get; set; }

    public string? ConsultationBloodpressuredAs { get; set; }

    public string? ConsultationBloodpresuredDis { get; set; }

    public string ConsultationPulse { get; set; } = null!;

    public string ConsultationWeight { get; set; } = null!;

    public string ConsultationSize { get; set; } = null!;

    public string? ConsultationTreatmentplan { get; set; }

    public string? ConsultationObservation { get; set; }

    public string? ConsultationPersonalbackground { get; set; }

    public int? ConsultationDisablilitydays { get; set; }

    public int? ConsultationType { get; set; }

    public int? ConsultationStatus { get; set; }

    public string? ConsultationEvolutionNotes { get; set; }

    public string? ConsultationTherapies { get; set; }

    public virtual ICollection<AllergiesConsultation> AllergiesConsultations { get; set; } = new List<AllergiesConsultation>();

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Patient ConsultationPatientNavigation { get; set; } = null!;

    public virtual Speciality? ConsultationSpecialityNavigation { get; set; }

    public virtual User? ConsultationUsercreateNavigation { get; set; }

    public virtual ICollection<DiagnosisConsultation> DiagnosisConsultations { get; set; } = new List<DiagnosisConsultation>();

    public virtual ICollection<FamiliaryBackground> FamiliaryBackgrounds { get; set; } = new List<FamiliaryBackground>();

    public virtual ICollection<ImagesConsutlation> ImagesConsutlations { get; set; } = new List<ImagesConsutlation>();

    public virtual ICollection<LaboratoriesConsultation> LaboratoriesConsultations { get; set; } = new List<LaboratoriesConsultation>();

    public virtual ICollection<MedicationsConsultation> MedicationsConsultations { get; set; } = new List<MedicationsConsultation>();

    public virtual ICollection<OrgansSystem> OrgansSystems { get; set; } = new List<OrgansSystem>();

    public virtual ICollection<PhysicalExamination> PhysicalExaminations { get; set; } = new List<PhysicalExamination>();

    public virtual ICollection<SurgeriesConsultation> SurgeriesConsultations { get; set; } = new List<SurgeriesConsultation>();
}
