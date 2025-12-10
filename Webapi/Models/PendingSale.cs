namespace Webapi.Models
{
    public class PendingSale
    {
        public int Id { get; set; }
        public string Client { get; set; }
        public string StatusProduct { get; set; } = "Recibido";
        public string Phone { get; set; }
        public string TrackingCode { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Total { get; set; }
        public string Status { get; set; } = "Pendiente";
        public ICollection<PendingSaleDetail> Details { get; set; } = new List<PendingSaleDetail>();
    }
}