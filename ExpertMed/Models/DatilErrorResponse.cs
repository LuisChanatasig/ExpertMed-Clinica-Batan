namespace ExpertMed.Models
{
    public class DatilErrorResponse
    {
        public List<DatilErrorDetail> Errors { get; set; }
    }

    public class DatilErrorDetail
    {
        public string Details { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }
        public string Parameter { get; set; }
    }
}
