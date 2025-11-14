using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _env;   // para ruta del logo

        public ReportsController(Connectioncontextdb context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

       
        [HttpGet("daily-details")]
        public Task<IActionResult> GetDailyDetails([FromQuery] DateTime date)
        {
            // Reutilizamos el endpoint general usando el mismo día como from y to
            return GetDetails(date, date);
        }

    
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

        // 🔹 Endpoint para EXPORTAR PDF con el MISMO rango
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

        // 🔹 Generación del PDF con QuestPDF (logo + header + footer)
        private byte[] GenerarReporteDetallesPdf(
            DateTime desde,
            DateTime hasta,
            decimal total,
            int ventasDistintas,
            decimal promedio,
            List<ReportRow> filas
        )
        {
            // Cargar logo desde carpeta Images/jenapp-logo.jpeg
            byte[]? logoData = null;
            try
            {
                var logoPath = Path.Combine(_env.ContentRootPath, "Images", "jenapp-logo.jpeg");
                if (System.IO.File.Exists(logoPath))
                    logoData = System.IO.File.ReadAllBytes(logoPath);
            }
            catch
            {
                // Si falla, simplemente no mostramos logo
            }

            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);

                    // HEADER
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            if (logoData != null)
                            {
                                row.ConstantItem(70).Height(70).AlignLeft()
                                   .Image(logoData);
                            }

                            row.RelativeItem()
                               .AlignMiddle()   // 👈 AQUÍ corregido
                               .Column(c =>
                               {
                                   c.Item().Text("Reporte de Ventas")
                                       .FontSize(20)
                                       .SemiBold()
                                       .FontColor(Colors.Blue.Darken2);

                                   c.Item().Text("Jenapp - Módulo de Reportes")
                                       .FontSize(11)
                                       .FontColor(Colors.Grey.Darken2);

                                   c.Item().Text($"Desde {desde:dd/MM/yyyy} hasta {hasta:dd/MM/yyyy}")
                                       .FontSize(10)
                                       .FontColor(Colors.Grey.Darken3);
                               });
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    // CONTENIDO
                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        // Cards de resumen
                        col.Item().Row(row =>
                        {
                            void Card(string titulo, string valor)
                            {
                                row.RelativeItem().PaddingRight(5).Border(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Background(Colors.Grey.Lighten5)
                                    .Padding(10)
                                    .Column(c =>
                                    {
                                        c.Item().Text(titulo)
                                            .FontSize(10)
                                            .FontColor(Colors.Grey.Darken2);

                                        c.Item().Text(valor)
                                            .FontSize(14)
                                            .SemiBold()
                                            .FontColor(Colors.Blue.Darken3);
                                    });
                            }

                            Card("Total vendido", total.ToString("C2"));
                            Card("Cantidad de ventas", ventasDistintas.ToString());
                            Card("Promedio por venta", promedio.ToString("C2"));
                        });

                        col.Item().PaddingTop(15);

                        // Título tabla
                        col.Item().Text("Detalle de ventas")
                            .FontSize(12)
                            .SemiBold()
                            .FontColor(Colors.Grey.Darken3);

                        col.Item().PaddingBottom(5)
                            .LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);

                        // Tabla
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);   // Venta
                                columns.RelativeColumn(70);   // Fecha
                                columns.RelativeColumn(110);  // Cliente
                                columns.RelativeColumn(110);  // Producto
                                columns.ConstantColumn(40);   // Cant.
                                columns.ConstantColumn(60);   // P. Unit
                                columns.ConstantColumn(70);   // Total
                            });

                            // Encabezados
                            table.Header(header =>
                            {
                                void HeaderCell(string text)
                                {
                                    header.Cell().Background(Colors.Blue.Medium)
                                        .PaddingVertical(5)
                                        .PaddingHorizontal(3)
                                        .Text(text)
                                        .FontSize(9)
                                        .Bold()
                                        .FontColor(Colors.White);
                                }

                                HeaderCell("Venta");
                                HeaderCell("Fecha");
                                HeaderCell("Cliente");
                                HeaderCell("Producto");
                                HeaderCell("Cant.");
                                HeaderCell("P. Unit");
                                HeaderCell("Total");
                            });

                            // Filas
                            foreach (var f in filas)
                            {
                                table.Cell().Padding(3).Text(f.SaleId.ToString()).FontSize(9);
                                table.Cell().Padding(3).Text(f.Fecha.ToString("dd/MM/yyyy")).FontSize(9);
                                table.Cell().Padding(3).Text(f.Cliente).FontSize(9);
                                table.Cell().Padding(3).Text(f.Producto).FontSize(9);
                                table.Cell().Padding(3).AlignRight().Text(f.Quantity.ToString()).FontSize(9);
                                table.Cell().Padding(3).AlignRight().Text(f.UnitPrice.ToString("C2")).FontSize(9);
                                table.Cell().Padding(3).AlignRight().Text(f.TotalPrice.ToString("C2")).FontSize(9);
                            }
                        });
                    });

                    // FOOTER
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Jose Zabaleta y Esteban Rinaldy @ Jenapp")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Medium);

                        text.Span("  |  ").FontSize(9).FontColor(Colors.Grey.Lighten1);

                        text.Span($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Lighten1);
                    });
                });
            }).GeneratePdf();
        }
    }
}
