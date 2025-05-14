namespace ExpertMed.Models
{
    public class FavoriteMedicationInputModel
    {
        public int UsersId { get; set; }
        public int MedicationsId { get; set; }
        public string Cantidad { get; set; }
        public string Observacion { get; set; }
    }
}
