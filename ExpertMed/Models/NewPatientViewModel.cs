namespace ExpertMed.Models
{
    public class NewPatientViewModel
    {
        public List<Catalog> GenderTypes { get; set; }
        public List<Catalog> BloodTypes { get; set; }
        public List<Catalog> DocumentTypes { get; set; }
        public List<Catalog> CivilTypes { get; set; }
        public List<Catalog> ProfessionalTrainingTypes { get; set; }
        public List<Catalog> SureHealthTypes { get; set; }
        public List<Catalog> RelationshipTypes { get; set; }
        public List<Catalog> AllergiesTypes { get; set; }
        public List<Catalog> SurgeriesTypes { get; set; }
        public List<Catalog> FamilyMember { get; set; }
        public List<Country> Countries { get; set; }
        public List<Catalog> Parents { get; set; }
        public List<Province> Provinces { get; set; }
        public List<Diagnosis> Diagnoses { get; set; }
        public List<Medication> Medications { get; set; }
        public List<Image> Images { get; set; }
        public List<Laboratory> Laboratories { get; set; }
        public List<InsuranceCompanyDto> InsuranceCompanies { get; set; }


        public Patient Patient { get; set; }  // For user details

        public  List <User> Users { get; set; }  // For user details
        public  List <MedicDetails> UsersP { get; set; }  // For user details

        public DetailsPatientConsult DetailsPatient { get; set; }
        public List<DoctorPatient> Doctors { get; set; } // Lista de médicos asociados al paciente

        public Consulta Consultation { get; set; }

        public virtual Catalog? PatientBloodtypeNavigation { get; set; }

        public virtual User? PatientCreationuserNavigation { get; set; }

        public virtual Catalog? PatientDocumenttypeNavigation { get; set; }

        public virtual Catalog? PatientGenderNavigation { get; set; }

        public virtual Catalog? PatientHealthInsuranceNavigation { get; set; }

        public virtual Catalog? PatientMaritalstatusNavigation { get; set; }

        public virtual User? PatientModificationuserNavigation { get; set; }

        public virtual Country? PatientNationalityNavigation { get; set; }

        public virtual Province? PatientProvinceNavigation { get; set; }

        public virtual Catalog? PatientVocationalTrainingNavigation { get; set; }
    }
}
