namespace ExpertMed.Models
{
    public class ReporteChartDto
    {
        public List<EstadoChartItem> Estados { get; set; }
        public List<string> Meses { get; set; }
        public List<int> ValoresPorMes { get; set; }
        public List<MedicoChartItem> TopMedicos { get; set; }
        public List<int> Pendientes { get; set; }
        public List<int> Finalizados { get; set; }
    }

    public class EstadoChartItem
    {
        public string Nombre { get; set; }
        public int Valor { get; set; }
    }

    public class MedicoChartItem
    {
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
    }

}
