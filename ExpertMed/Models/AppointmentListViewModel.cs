using iText.StyledXmlParser.Jsoup.Nodes;

namespace ExpertMed.Models
{

    public class AppointmentListViewModel
    {
        public List<AppointmentDTO> Appointments { get; set; } = new List<AppointmentDTO>();

        // Para el modal de agendar cita - datos del formulario de paciente
        public List<User> Users { get; set; } = new List<User>();
        public List<InsuranceCompanyDto> InsuranceCompanies { get; set; } = new List<InsuranceCompanyDto>();
        public Patient Patient { get; set; }

        // Datos adicionales para los selects del formulario (si los necesitas)
        public List<Catalog> GenderTypes { get; set; } = new List<Catalog>();
        public List<Catalog> BloodTypes { get; set; } = new List<Catalog>();
        public List<Catalog> CivilTypes { get; set; } = new List<Catalog>();
        public List<Catalog> ProfessionalTrainingTypes { get; set; } = new List<Catalog>();
        public List<Catalog> SureHealthTypes { get; set; } = new List<Catalog>();
        public List<Country> Countries { get; set; } = new List<Country>();
        public List<Province> Provinces { get; set; } = new List<Province>();
        public List<MedicDetails> UsersP { get; set; } = new List<MedicDetails>();
    }
}
