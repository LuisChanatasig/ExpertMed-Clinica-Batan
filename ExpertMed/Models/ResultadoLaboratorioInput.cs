namespace ExpertMed.Models
{
    public class ResultadoLaboratorioInput
    {
        public int LaboratoryId { get; set; }
        public string Resultado { get; set; }
        public IFormFile? Archivo { get; set; }
    }

}
