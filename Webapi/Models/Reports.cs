namespace Webapi.Models
{
    public class ReportVentaDetalle
    {
        public string Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnit { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class ReportVenta
    {
        public int Id { get; set; }
        public string Cliente { get; set; }
        public DateTime Fecha { get; set; }
        public decimal TotalVenta { get; set; }
        public List<ReportVentaDetalle> Detalles { get; set; } = new();
    }

    public class ReportResult
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public decimal TotalGeneral { get; set; }
        public int CantidadVentas { get; set; }
        public decimal PromedioVentas { get; set; }
        public List<ReportVenta> Ventas { get; set; } = new();
    }
}

