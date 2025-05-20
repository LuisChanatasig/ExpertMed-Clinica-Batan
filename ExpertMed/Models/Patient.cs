using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpertMed.Models;

public partial class Patient
{
    public int PatientId { get; set; }

    public DateTime? PatientCreationdate { get; set; }

    public DateTime? PatientModificationdate { get; set; }

    public int? PatientCreationuser { get; set; }

    public int? PatientModificationuser { get; set; }

    public int? PatientDocumenttype { get; set; }

    public string? PatientDocumentnumber { get; set; }

    public string PatientFirstname { get; set; } = null!;

    public string? PatientMiddlename { get; set; }

    public string? PatientSecondlastname { get; set; }

    public int? PatientGender { get; set; }

    public DateOnly? PatientBirthdate { get; set; }

    public int? PatientAge { get; set; }

    public int? PatientBloodtype { get; set; }

    public string? PatientDonor { get; set; }

    public int? PatientMaritalstatus { get; set; }

    public int? PatientVocationalTraining { get; set; }

    public string? PatientLandlinePhone { get; set; }

    public string PatientEmail { get; set; } = null!;

    public int? PatientNationality { get; set; }

    public int? PatientProvince { get; set; }

    public string? PatientAddress { get; set; }

    public string? PatientOcupation { get; set; }
    [NotMapped]
    public string? PatientInsuranceAuthorizationCode { get; set; }

    public string? PatientCompany { get; set; }

    public int? PatientHealtInsurance { get; set; }

    public string PatientCode { get; set; } = null!;

    public int PatientStatus { get; set; }

    public string? PatientCellularPhone { get; set; }

    public string PatientFirstsurname { get; set; } = null!;
    [NotMapped]
    public string? DoctorName { get; set; }
    [NotMapped]
    public string? DoctorFullname { get; set; }


    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

    public virtual ICollection<DoctorPatient> DoctorPatients { get; set; } = new List<DoctorPatient>();

    public virtual Catalog? PatientBloodtypeNavigation { get; set; }

    public virtual User? PatientCreationuserNavigation { get; set; }

    public virtual Catalog? PatientDocumenttypeNavigation { get; set; }

    public virtual Catalog? PatientGenderNavigation { get; set; }

    public virtual Catalog? PatientHealtInsuranceNavigation { get; set; }

    public virtual Catalog? PatientMaritalstatusNavigation { get; set; }

    public virtual User? PatientModificationuserNavigation { get; set; }

    public virtual Country? PatientNationalityNavigation { get; set; }

    public virtual Province? PatientProvinceNavigation { get; set; }

    public virtual Catalog? PatientVocationalTrainingNavigation { get; set; }
}