namespace Webapi.Models
{
    public class PendingSale
    {
        public int Id { get; set; }
        public string Client { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Total { get; set; }
        public string Status { get; set; } = "Pendiente";
        public ICollection<PendingSaleDetail> Details { get; set; } = new List<PendingSaleDetail>();
    }
}