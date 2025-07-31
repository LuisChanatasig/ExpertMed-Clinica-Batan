namespace ExpertMed.Models
{
    public class FacturaEmitidaDTO
    {
        public int FacturaId { get; set; }
        public int Secuencial { get; set; }
        public string SecuencialFormateado
        {
            get
            {
                return $"001-003-{Secuencial.ToString("D9")}";
            }
        }
        public DateTime Fecha { get; set; }
        public string Paciente { get; set; }
        public string Medico { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalAseguradora { get; set; }
        public decimal TotalCopago { get; set; }
        public string MetodoPago { get; set; }
        public string Aseguradora { get; set; }
        public int TotalItems { get; set; }

        public string Origen { get; set; } // "LOCAL" o "DATIL"
    }


}
