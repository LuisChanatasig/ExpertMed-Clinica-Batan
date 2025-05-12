namespace ExpertMed.Models
{
    public class ResultadoImagenInput
    {
        public int ImageId { get; set; }
        public string? Resultado { get; set; }
        public IFormFile? Archivo { get; set; }
    }

}
