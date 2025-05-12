namespace ExpertMed.Models
{
    public class ResumenConsultasDto
    {
        public string? NombreEstablecimiento { get; set; }

        public List<OcupacionConsultorioItem> OcupacionConsultorios { get; set; } = new();
        public MedicoTopItem MedicoTop { get; set; }
        public List<TipoPacienteItem> TiposPacientes { get; set; } = new();

        public class OcupacionConsultorioItem
        {
            public string Consultorio { get; set; }
            public int MedicosAsignados { get; set; }
        }

        public class MedicoTopItem
        {
            public string Medico { get; set; }
            public int TotalConsultas { get; set; }
        }

        public class TipoPacienteItem
        {
            public string TipoPaciente { get; set; }
            public int Total { get; set; }
        }
    }

}
