using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Webapi.Data;
using Webapi.Models; // 👈 asegúrate que aquí está tu entidad Sale
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

        // =========================
        // 1) ENDPOINT: DETALLE POR DÍA
        // =========================
        [HttpGet("daily-details")]
        public Task<IActionResult> GetDailyDetails([FromQuery] DateTime date)
        {
            // Reutilizamos el endpoint general usando el mismo día como from y to
            return GetDetails(date, date);
        }

        // =========================
        // 2) ENDPOINT: DETALLES POR RANGO (JSON)
        // =========================
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

        // =========================
        // 3) ENDPOINT: DETALLES POR RANGO (PDF)
        // =========================
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

        // =========================
        // 4) ENDPOINT: TICKET DE UNA VENTA (PDF TIPO POS)
        // =========================
        // GET: api/reports/ticket/5
        [HttpGet("ticket/{saleId}")]
        public async Task<IActionResult> GetSaleTicket(int saleId)
        {
            // Traer la venta con sus detalles y producto
            var sale = await _context.Sales
                .Include(s => s.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(s => s.Id == saleId);

            if (sale == null)
                return NotFound(new { message = "Venta no encontrada" });

            // Generar el PDF tipo ticket
            var pdfBytes = GenerarTicketPdf(sale);

            var fileName = $"ticket-venta-{saleId}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // =========================
        // 5) CLASE INTERNA PARA EL REPORTE GENERAL
        // =========================
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

        // =========================
        // 6) GENERACIÓN PDF REPORTE GENERAL
        // =========================
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
                               .AlignMiddle()
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

        // =========================
        // 7) GENERACIÓN PDF TIPO TICKET POS
        // =========================
        private byte[] GenerarTicketPdf(Sale sale)
        {
            // Intentar cargar el mismo logo (opcional)
            byte[]? logoData = null;
            try
            {
                var logoPath = Path.Combine(_env.ContentRootPath, "Images", "jenapp-logo.jpeg");
                if (System.IO.File.Exists(logoPath))
                    logoData = System.IO.File.ReadAllBytes(logoPath);
            }
            catch
            {
                // si falla, sin logo
            }

            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    // Tamaño tipo ticket (puedes ajustar)
                    page.Size(PageSizes.A6);
                    page.Margin(20);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(t => t.FontSize(9));

                    page.Content().Column(col =>
                    {
                        // HEADER
                        col.Item().AlignCenter().Column(c =>
                        {
                            if (logoData != null)
                            {
                                c.Item().Height(40).Width(40).AlignCenter().Image(logoData);
                            }

                            c.Item().Text("Jenapp")
                                .FontSize(14)
                                .SemiBold();

                            c.Item().Text("Factura de venta")
                                .FontSize(10);
                        });

                        col.Item().PaddingVertical(5).LineHorizontal(0.5f);

                        // INFO VENTA
                        col.Item().Text(txt =>
                        {
                            txt.Span("Venta N°: ").SemiBold();
                            txt.Span(sale.Id.ToString());
                        });

                        col.Item().Text(txt =>
                        {
                            txt.Span("Fecha: ").SemiBold();
                            txt.Span(sale.Date.ToString("dd/MM/yyyy HH:mm"));
                        });

                        col.Item().Text(txt =>
                        {
                            txt.Span("Cliente: ").SemiBold();
                            txt.Span(sale.Client ?? "Sin nombre");
                        });

                        col.Item().PaddingVertical(5).LineHorizontal(0.5f);

                        // DETALLES
                        col.Item().Text("Detalles").SemiBold().FontSize(10);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Producto
                                columns.RelativeColumn(1); // Cant
                                columns.RelativeColumn(1); // P. U.
                                columns.RelativeColumn(1); // Subtotal
                            });

                            // Encabezados
                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Prod");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Cant");
                                header.Cell().Element(HeaderCell).AlignRight().Text("P.U.");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Sub");

                                static IContainer HeaderCell(IContainer container) =>
                                    container.DefaultTextStyle(x => x.SemiBold())
                                             .PaddingBottom(2);
                            });

                            // Filas
                            foreach (var d in sale.Details)
                            {
                                var nombre = d.Product?.Name ?? "Producto";
                                var cant = d.Quantity;
                                var unit = d.UnitPrice;
                                var sub = d.TotalPrice;

                                table.Cell().Text(nombre).FontSize(9);
                                table.Cell().AlignRight().Text(cant.ToString()).FontSize(9);
                                table.Cell().AlignRight().Text(unit.ToString("N0")).FontSize(9);
                                table.Cell().AlignRight().Text(sub.ToString("N0")).FontSize(9);
                            }
                        });

                        col.Item().PaddingVertical(5).LineHorizontal(0.5f);

                        // TOTAL
                        col.Item().AlignRight().Text(txt =>
                        {
                            txt.Span("TOTAL: ").SemiBold();
                            txt.Span(sale.Total.ToString("N0")).SemiBold();
                        });

                        col.Item().PaddingTop(10).AlignCenter().Text("¡Gracias por su compra!")
                            .FontSize(9);
                    });
                });
            }).GeneratePdf();
        }
    }
}
