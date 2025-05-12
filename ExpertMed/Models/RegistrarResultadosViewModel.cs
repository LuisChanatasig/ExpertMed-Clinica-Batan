namespace ExpertMed.Models
{
    public class RegistrarResultadosViewModel
    {
        public int ConsultationId { get; set; }
        public List<ResultadoExamenInput> Resultados { get; set; } = new();
    }
    public class ResultadoExamenInput
    {
        public int ExamenId { get; set; }
        public string Resultado { get; set; }
        public IFormFile? Archivo { get; set; } // Para el archivo que se sube
    }
}
