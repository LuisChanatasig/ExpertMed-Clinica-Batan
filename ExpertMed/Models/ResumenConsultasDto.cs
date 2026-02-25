namespace ExpertMed.Models
{
    public class ResumenConsultasDto
    {
        // Dataset 1: KPIs
        public DashboardKpi Kpi { get; set; } = new DashboardKpi();

        // Dataset 2: Evolución Diaria
        public List<DashboardEvolutionItem> EvolucionDiaria { get; set; } = new List<DashboardEvolutionItem>();

        // Dataset 3: Estado Citas (Pastel)
        public List<DashboardStatusItem> EstadoCitas { get; set; } = new List<DashboardStatusItem>();

        // Dataset 4: Ranking Médicos
        public List<DashboardDoctorItem> RankingMedicos { get; set; } = new List<DashboardDoctorItem>();

        // Dataset 5: Pacientes por Seguro
        public List<DashboardInsuranceItem> PacientesPorSeguro { get; set; } = new List<DashboardInsuranceItem>();

        // Clases anidadas para estructura interna
        public class DashboardKpi
        {
            public int TotalCitas { get; set; }
            public int TotalConsultas { get; set; }
            public int TotalPagadas { get; set; }
            public int TotalPacientesHistorico { get; set; }
            public int TotalAsistidos { get; set; } // Añadir esta
            public int TotalNoAsistidos { get; set; } // Añadir esta
        }

        public class DashboardEvolutionItem
        {
            public DateTime Fecha { get; set; }
            public int CantidadCitas { get; set; }
        }

        public class DashboardStatusItem
        {
            public string Estado { get; set; } = string.Empty;
            public int Cantidad { get; set; }
        }

        public class DashboardDoctorItem
        {
            public string Medico { get; set; } = string.Empty;
            public int ConsultasRealizadas { get; set; }
        }

        public class DashboardInsuranceItem
        {
            public string Seguro { get; set; } = string.Empty;
            public int CantidadPacientesUnicos { get; set; }
        }
    }
}