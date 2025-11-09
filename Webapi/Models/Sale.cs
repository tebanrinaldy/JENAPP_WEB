namespace Webapi.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public String Client { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Total { get; set; }
        public ICollection<SaleDetail> Details { get; set; }


    }
}
