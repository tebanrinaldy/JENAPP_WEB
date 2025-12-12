namespace Webapi.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public String Client { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Total { get; set; }
        public ICollection<SaleDetail> Details { get; set; } = new List<SaleDetail>();


    }
}
