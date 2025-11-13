namespace Webapi.Models
{
    public class InventoryMovement
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public  Product ? Product { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Reason { get; set; }
    }
}
