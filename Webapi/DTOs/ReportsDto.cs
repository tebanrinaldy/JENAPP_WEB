namespace Webapi.DTOs
{
    public class Reportrowdto
    {
        public int SaleId { get; set; }
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; }
        public int ProductId { get; set; }
        public string Producto { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

    }
    public class Reportresponsedto
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public decimal TotalAmount { get; set; }
        public int SalesCount { get; set; }
        public decimal AverageAmount { get; set; }
        public List<Reportrowdto> Rows { get; set; } = new();
    }
}
