using System;
using System.Collections.Generic;

namespace ExpertMed.Models;

public partial class FamiliaryBackground
{
    public int FamiliaryBackgroundId { get; set; }

    public int? FamiliaryBackgroundConsultationid { get; set; }

    public bool? FamiliaryBackgroundHeartdisease { get; set; }

    public string? FamiliaryBackgroundHeartdiseaseObservation { get; set; }

    public int? FamiliaryBackgroundRelatshcatalogHeartdisease { get; set; }

    public bool? FamiliaryBackgroundDiabetes { get; set; }

    public string? FamiliaryBackgroundDiabetesObservation { get; set; }

    public int? FamiliaryBackgroundRelatshcatalogDiabetes { get; set; }

    public bool? FamiliaryBackgroundDxcardiovascular { get; set; }

    public string? FamiliaryBackgroundDxcardiovascularObservation { get; set; }

    public int? FamiliaryBackgroundRelatshcatalogDxcardiovascular { get; set; }

    public bool? FamiliaryBackgroundHypertension { get; set; }

    public string? FamiliaryBackgroundHypertensionObservation { get; set; }

    public int? FamiliaryBackgroundRelatshcatalogHypertension { get; set; }

    public bool? FamiliaryBackgroundCancer { get; set; }

    public string? FamiliaryBackgroundCancerObservation { get; set; }

    public int? FamiliaryBackgroundRelatshcatalogCancer { get; set; }

    public bool? FamiliaryBackgroundTuberculosis { get; set; }

    public string? FamiliaryBackgroundTuberculosisObservation { get; set; }

    public int? FamiliaryBackgroundRelatshTuberculosis { get; set; }

    public bool? FamiliaryBackgroundDxmental { get; set; }

    public string? FamiliaryBackgroundDxmentalObservation { get; set; }

    public int? FamiliaryBackgroundRelatshcatalogDxmental { get; set; }

    public bool? FamiliaryBackgroundDxinfectious { get; set; }

    public string? FamiliaryBackgroundDxinfectiousObservation { get; set; }

    public int? FamiliaryBackgroundRelatshcatalogDxinfectious { get; set; }

    public bool? FamiliaryBackgroundMalformation { get; set; }

    public string? FamiliaryBackgroundMalformationObservation { get; set; }

    public int? FamiliaryBackgroundRelatshcatalogMalformation { get; set; }

    public bool? FamiliaryBackgroundOther { get; set; }

    public string? FamiliaryBackgroundOtherObservation { get; set; }

    public int? FamiliaryBackgroundRelatshcatalogOther { get; set; }

    public virtual Consultation? FamiliaryBackgroundConsultation { get; set; }

    public virtual Catalog? FamiliaryBackgroundRelatshTuberculosisNavigation { get; set; }

    public virtual Catalog? FamiliaryBackgroundRelatshcatalogCancerNavigation { get; set; }

    public virtual Catalog? FamiliaryBackgroundRelatshcatalogDiabetesNavigation { get; set; }

    public virtual Catalog? FamiliaryBackgroundRelatshcatalogDxcardiovascularNavigation { get; set; }

    public virtual Catalog? FamiliaryBackgroundRelatshcatalogDxinfectiousNavigation { get; set; }

    public virtual Catalog? FamiliaryBackgroundRelatshcatalogDxmentalNavigation { get; set; }

    public virtual Catalog? FamiliaryBackgroundRelatshcatalogHeartdiseaseNavigation { get; set; }

    public virtual Catalog? FamiliaryBackgroundRelatshcatalogHypertensionNavigation { get; set; }

    public virtual Catalog? FamiliaryBackgroundRelatshcatalogMalformationNavigation { get; set; }

    public virtual Catalog? FamiliaryBackgroundRelatshcatalogOtherNavigation { get; set; }
}
