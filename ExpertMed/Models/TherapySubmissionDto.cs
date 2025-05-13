namespace ExpertMed.Models { 
public class TherapySubmissionDto
{
    public int PacienteId { get; set; }
    public int TerapeutaId { get; set; }
    public List<TherapySessionInput> Sesiones { get; set; }
}

}
