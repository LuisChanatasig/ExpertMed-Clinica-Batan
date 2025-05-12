namespace ExpertMed.Models
{
    public class Facturacions
    {
        public int? CitaId { get; set; }
        public DateTime FechaFacturacion { get; set; }
        public decimal TotalFactura { get; set; }
        public string? MetodoPago { get; set; }

        public byte[]? ComprobantePagoFacturacion { get; set; }

        // Nuevas propiedades agregadas para la facturación
        public string BillingDetailsNames { get; set; }
        public string BillingDetailsCiNumber { get; set; }
        public string BillingDetailsDocumentType { get; set; }
        public string BillingDetailsAddress { get; set; }
        public string BillingDetailsPhone { get; set; }
        public string BillingDetailsEmail { get; set; }
    }

}
