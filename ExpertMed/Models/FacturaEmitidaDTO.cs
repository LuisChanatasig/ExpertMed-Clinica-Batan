namespace ExpertMed.Models
{
    public class FacturaEmitidaDTO
    {
        public int FacturaId { get; set; }
        public DateTime Fecha { get; set; }
        public string Paciente { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalAseguradora { get; set; }
        public decimal TotalCopago { get; set; }
        public string MetodoPago { get; set; }
        public string Aseguradora { get; set; }
        public int TotalItems { get; set; }
    }

}
