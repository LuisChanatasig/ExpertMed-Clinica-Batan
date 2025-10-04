namespace ExpertMed.Models
{
    // Modelo para el resultado
    public class ProcedimientoDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public decimal Precio { get; set; }
        public decimal PrecioAseguradora { get; set; }
    }
}
