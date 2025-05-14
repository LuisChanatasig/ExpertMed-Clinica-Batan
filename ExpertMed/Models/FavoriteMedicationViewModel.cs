namespace ExpertMed.Models
{
    public class FavoriteMedicationViewModel
    {
        public int FavoriteId { get; set; }
        public int MedicationId { get; set; }
        public string MedicationName { get; set; }
        public string Cantidad { get; set; }
        public string Observacion { get; set; }
        public DateTime CreationDate { get; set; }
    }

}
