using ExpertMed.Migrations;
using System.Data;

namespace ExpertMed.Models
{
    public class Consulta
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
        public string? ConsultationEvolutionNotes { get; set; }

        public string? ConsultationTherapies { get; set; }
        public int? ConsultationType { get; set; }

        public int? ConsultationStatus { get; set; }

        public string? UsersNames { get; set; }
        public string? UsersSurcenames { get; set; }
        public string? UsersEmail { get; set; }
        public string? UsersPhone { get; set; }
        public string? UsersEstablishmentName { get; set; }

        public string? UsersEstablishmentAddress { get; set; }
        public byte[]? UsersProfilephoto { get; set; }
        public string? UsersProfilephoto64 { get; set; }

        public string? SpecialityName { get; set; }

        public string? EstablishmentName { get; set; }


        // Relaciones con otras tablas
        public List<ConsultaAlergiaDTO> AllergiesConsultations { get; set; } // Lista de alergias asociadas a la consulta
        public List<ConsultaCirugiaDTO> SurgeriesConsultations { get; set; } // Lista de cirugías asociadas a la consulta
        public List<ConsultaMedicamentoDTO> MedicationsConsultations { get; set; } // Lista de medicamentos asociados
        public List<ConsultaLaboratorioDTO> LaboratoriesConsultations { get; set; } // Lista de laboratorios asociadosss
        public List<ConsultaImagenDTO> ImagesConsultations { get; set; } // Lista de imágenes asociadas
        public List<ConsultaDiagnosticoDTO> DiagnosisConsultations { get; set; }
       
        public OrgansSystem OrgansSystem { get; set; } // Órganos y sistemas asociados
        public PhysicalExamination PhysicalExamination { get; set; } // Examen físico asociado
        public FamiliaryBackground FamiliaryBackground { get; set; } // Antecedentes familiares asociados

        public virtual User? ConsultationUsercreateNavigation { get; set; }
        public virtual Patient? PacienteConsultaPNavigation { get; set; }
        public virtual Catalog? EstadocivilPacientesCaNavigation { get; set; }
        public virtual Catalog? FormacionprofesionalPacientesCaNavigation { get; set; }
        public virtual Country? NacionalidadPacientesPaNavigation { get; set; }
        public virtual Province? ProvinciaPacientesLNavigation { get; set; }
        public virtual Catalog? SegurosaludPacientesCaNavigation { get; set; }
        public virtual Catalog? SexoPacientesCaNavigation { get; set; }
        public virtual Catalog? TipodocumentoPacientesCaNavigation { get; set; }
        public virtual Catalog? TiposangrePacientesCaNavigation { get; set; }


        // Constructor que inicializa los objetos complejos
        public Consulta()
        {
            FamiliaryBackground = new FamiliaryBackground();
            AllergiesConsultations = new List<ConsultaAlergiaDTO>();
            SurgeriesConsultations = new List<ConsultaCirugiaDTO>();
            MedicationsConsultations = new List<ConsultaMedicamentoDTO>();
            LaboratoriesConsultations = new List<ConsultaLaboratorioDTO>();
            ImagesConsultations = new List<ConsultaImagenDTO>();
            DiagnosisConsultations = new List<ConsultaDiagnosticoDTO>();
            OrgansSystem = new OrgansSystem();
            PhysicalExamination = new PhysicalExamination();

        }
    }
    public class ConsultaAlergiaDTO
    {
       

        public int AllergiesCatalogid { get; set; }

        public string? AllergiesObservation { get; set; }

        public int? AllergiesStatus { get; set; } = 1; // Valor predeterminado
    }
    public class ConsultaCirugiaDTO
    {
        
        public int? SurgeriesCatalogid { get; set; }

        public string? SurgeriesObservation { get; set; }

        public int? SurgeriesStatus { get; set; } = 1; // Valor predeterminado

    }
    public class ConsultaImagenDTO
    {
      
        public int? ImagesImagesid { get; set; }

        public string? ImagesAmount { get; set; }

        public string? ImagesObservation { get; set; }

        public int? ImagesSequential { get; set; }

        public int? ImagesStatus { get; set; } = 1;

    }
    public class ConsultaLaboratorioDTO
    {
       
        public int? LaboratoriesLaboratoriesid { get; set; }

        public string? LaboratoriesAmount { get; set; }

        public string? LaboratoriesObservation { get; set; }

        public int? LaboratoriesSequential { get; set; }

        public int? LaboratoriesStatus { get; set; } = 1;
    }
    public class ConsultaMedicamentoDTO
    {

     
        public int? MedicationsMedicationsid { get; set; }

        public string? MedicationsAmount { get; set; }

        public string? MedicationsObservation { get; set; }

        public int? MedicationsSequential { get; set; }

        public int? MedicationsStatus { get; set; } = 1;

   
    }

    public class ConsultaDiagnosticoDTO
    {

        public int? DiagnosisDiagnosisid { get; set; }

        public string? DiagnosisObservation { get; set; }

        public bool? DiagnosisPresumptive { get; set; }

        public bool? DiagnosisDefinitive { get; set; }


        public int? DiagnosisStatus { get; set; } = 1;

    }
}
