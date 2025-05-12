using ExpertMed.Models;

namespace ExpertMed.Models
{
    public class NewUserViewModel
    {
        public List<Profile> Profiles { get; set; }
        public List<Speciality> Specialties { get; set; }
        public List<User> Users { get; set; }
        public List<Country> Countries { get; set; }
        public List<VatBilling> VatBillings { get; set; }
        public List<Establishment> Establishments { get; set; }
        public List<DoctorDto> AssociatedDoctors { get; set; }  // Médicos asociados si es asistente
        public List<MedicalOfficeListDto> MedicalOfficeListDtos { get; set; }

        public UserWithDetails User { get; set; }  // For user details
        public UserViewModel Uservm { get; set; }  // For user details

    }
}
