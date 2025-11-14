using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webapi.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Webapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly Connectioncontextdb _context;

        public ReportsController(Connectioncontextdb context)
        {
            _context = context;
        }

        // ✅ Compatibilidad con el endpoint original por día
        // GET: api/reports/daily-details?date=2025-11-14
        [HttpGet("daily-details")]
        public Task<IActionResult> GetDailyDetails([FromQuery] DateTime date)
        {
            // Reutilizamos el endpoint general usando el mismo día como from y to
            return GetDetails(date, date);
        }

        // ✅ Endpoint general: por día o por rango (JSON)
        // GET: api/reports/details?from=2025-11-14
        // GET: api/reports/details?from=2025-11-14&to=2025-11-16
        [HttpGet("details")]
        public async Task<IActionResult> GetDetails(
            [FromQuery] DateTime from,
            [FromQuery] DateTime? to = null)
        {
            var fromDate = from.Date;
            var toDate = (to ?? from).Date.AddDays(1); // rango [from, to] inclusive

            // 1. Traemos los detalles de venta dentro del rango
            var detalles = await _context.SaleDetails
                .Include(d => d.Sale)
                .Include(d => d.Product)
                .Where(d => d.Sale != null &&
                            d.Sale.Date >= fromDate &&
                            d.Sale.Date < toDate)
                .ToListAsync();

            // 2. Proyección para la tabla (grid)
            var filas = detalles
                .Select(d => new
                {
                    saleId = d.SaleId,
                    fecha = d.Sale!.Date,
                    cliente = d.Sale!.Client,
                    productId = d.ProductId,
                    producto = d.Product != null ? d.Product.Name : string.Empty,
                    quantity = d.Quantity,
                    unitPrice = d.UnitPrice,
                    totalPrice = d.TotalPrice
                })
                .OrderBy(f => f.fecha)
                .ThenBy(f => f.saleId)
                .ToList();

            // 3. Resumen
            var total = filas.Sum(f => f.totalPrice);
            var ventasDistintas = filas
                .Select(f => f.saleId)
                .Distinct()
                .Count();

            var promedio = ventasDistintas == 0 ? 0 : total / ventasDistintas;

            // 4. Resultado final (resumen + filas)
            var resultado = new
            {
                dateFrom = fromDate,
                dateTo = toDate.AddDays(-1).Date, // día final real
                totalAmount = total,
                salesCount = ventasDistintas,
                averageAmount = promedio,
                rows = filas
            };

            return Ok(resultado);
        }

        // 🔹 NUEVO: endpoint para EXPORTAR PDF con el MISMO rango
        // GET: api/reports/details/pdf?from=2025-11-14
        // GET: api/reports/details/pdf?from=2025-11-14&to=2025-11-16
        [HttpGet("details/pdf")]
        public async Task<IActionResult> GetDetailsPdf(
            [FromQuery] DateTime from,
            [FromQuery] DateTime? to = null)
        {
            var fromDate = from.Date;
            var toDate = (to ?? from).Date.AddDays(1); // [from, to] inclusive

            var detalles = await _context.SaleDetails
                .Include(d => d.Sale)
                .Include(d => d.Product)
                .Where(d => d.Sale != null &&
                            d.Sale.Date >= fromDate &&
                            d.Sale.Date < toDate)
                .ToListAsync();

            var filas = detalles
                .Select(d => new ReportRow
                {
                    SaleId = d.SaleId,
                    Fecha = d.Sale!.Date,
                    Cliente = d.Sale!.Client,
                    ProductId = d.ProductId,
                    Producto = d.Product != null ? d.Product.Name : string.Empty,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    TotalPrice = d.TotalPrice
                })
                .OrderBy(f => f.Fecha)
                .ThenBy(f => f.SaleId)
                .ToList();

            var total = filas.Sum(f => f.TotalPrice);
            var ventasDistintas = filas
                .Select(f => f.SaleId)
                .Distinct()
                .Count();

            var promedio = ventasDistintas == 0 ? 0 : total / ventasDistintas;

            var pdfBytes = GenerarReporteDetallesPdf(
                fromDate,
                toDate.AddDays(-1).Date,
                total,
                ventasDistintas,
                promedio,
                filas
            );

            var fileName =
                $"reporte_ventas_{fromDate:yyyyMMdd}_a_{toDate.AddDays(-1).Date:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        // 🔹 Clase interna para representar una fila del reporte
        private class ReportRow
        {
            public int SaleId { get; set; }
            public DateTime Fecha { get; set; }
            public string Cliente { get; set; } = "";
            public int ProductId { get; set; }
            public string Producto { get; set; } = "";
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
        }

        // 🔹 Generación del PDF con QuestPDF
        private byte[] GenerarReporteDetallesPdf(
            DateTime desde,
            DateTime hasta,
            decimal total,
            int ventasDistintas,
            decimal promedio,
            System.Collections.Generic.List<ReportRow> filas
        )
        {
            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);

                    page.Header()
                        .Text($"Reporte de Ventas ({desde:yyyy-MM-dd} al {hasta:yyyy-MM-dd})")
                        .FontSize(18)
                        .Bold()
                        .AlignCenter();

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        // Resumen
                        col.Item().Text(text =>
                        {
                            text.Span("Total vendido: ").SemiBold();
                            text.Span(total.ToString("C2"));
                        });

                        col.Item().Text(text =>
                        {
                            text.Span("Cantidad de ventas: ").SemiBold();
                            text.Span(ventasDistintas.ToString());
                        });

                        col.Item().Text(text =>
                        {
                            text.Span("Promedio por venta: ").SemiBold();
                            text.Span(promedio.ToString("C2"));
                        });

                        col.Item().PaddingTop(10);

                        // Tabla de detalles
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);   // Id venta
                                columns.RelativeColumn(80);   // Fecha
                                columns.RelativeColumn(120);  // Cliente
                                columns.RelativeColumn(120);  // Producto
                                columns.ConstantColumn(40);   // Cantidad
                                columns.ConstantColumn(60);   // Precio unit.
                                columns.ConstantColumn(70);   // Total
                            });

                            // Encabezados
                            table.Header(header =>
                            {
                                header.Cell().Text("Venta").SemiBold();
                                header.Cell().Text("Fecha").SemiBold();
                                header.Cell().Text("Cliente").SemiBold();
                                header.Cell().Text("Producto").SemiBold();
                                header.Cell().Text("Cant.").SemiBold();
                                header.Cell().Text("P. Unit").SemiBold();
                                header.Cell().Text("Total").SemiBold();
                            });

                            foreach (var f in filas)
                            {
                                table.Cell().Text(f.SaleId.ToString());
                                table.Cell().Text(f.Fecha.ToString("yyyy-MM-dd"));
                                table.Cell().Text(f.Cliente);
                                table.Cell().Text(f.Producto);
                                table.Cell().Text(f.Quantity.ToString());
                                table.Cell().Text(f.UnitPrice.ToString("C2"));
                                table.Cell().Text(f.TotalPrice.ToString("C2"));
                            }
                        });
                    });
                });
            }).GeneratePdf();
        }
    }
}
