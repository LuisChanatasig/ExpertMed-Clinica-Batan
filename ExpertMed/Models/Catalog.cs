using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class Catalog
{
    public int CatalogId { get; set; }

    public string CatalogName { get; set; } = null!;

    public string CatalogCategory { get; set; } = null!;

    public int? CategoryStatus { get; set; }

    public virtual ICollection<AllergiesConsultation> AllergiesConsultations { get; set; } = new List<AllergiesConsultation>();

    public virtual ICollection<FamiliaryBackground> FamiliaryBackgroundFamiliaryBackgroundRelatshTuberculosisNavigations { get; set; } = new List<FamiliaryBackground>();

    public virtual ICollection<FamiliaryBackground> FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogCancerNavigations { get; set; } = new List<FamiliaryBackground>();

    public virtual ICollection<FamiliaryBackground> FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogDiabetesNavigations { get; set; } = new List<FamiliaryBackground>();

    public virtual ICollection<FamiliaryBackground> FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogDxcardiovascularNavigations { get; set; } = new List<FamiliaryBackground>();

    public virtual ICollection<FamiliaryBackground> FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogDxinfectiousNavigations { get; set; } = new List<FamiliaryBackground>();

    public virtual ICollection<FamiliaryBackground> FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogDxmentalNavigations { get; set; } = new List<FamiliaryBackground>();

    public virtual ICollection<FamiliaryBackground> FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogHeartdiseaseNavigations { get; set; } = new List<FamiliaryBackground>();

    public virtual ICollection<FamiliaryBackground> FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogHypertensionNavigations { get; set; } = new List<FamiliaryBackground>();

    public virtual ICollection<FamiliaryBackground> FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogMalformationNavigations { get; set; } = new List<FamiliaryBackground>();

    public virtual ICollection<FamiliaryBackground> FamiliaryBackgroundFamiliaryBackgroundRelatshcatalogOtherNavigations { get; set; } = new List<FamiliaryBackground>();

    public virtual ICollection<Patient> PatientPatientBloodtypeNavigations { get; set; } = new List<Patient>();

    public virtual ICollection<Patient> PatientPatientDocumenttypeNavigations { get; set; } = new List<Patient>();

    public virtual ICollection<Patient> PatientPatientGenderNavigations { get; set; } = new List<Patient>();

    public virtual ICollection<Patient> PatientPatientHealtInsuranceNavigations { get; set; } = new List<Patient>();

    public virtual ICollection<Patient> PatientPatientMaritalstatusNavigations { get; set; } = new List<Patient>();

    public virtual ICollection<Patient> PatientPatientVocationalTrainingNavigations { get; set; } = new List<Patient>();

    public virtual ICollection<SurgeriesConsultation> SurgeriesConsultations { get; set; } = new List<SurgeriesConsultation>();
}
