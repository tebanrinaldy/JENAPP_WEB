using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.ComponentModel.DataAnnotations;
using Webapi.Data;
using Webapi.DTOs;
using Webapi.Models;

namespace Webapi.Services
{
    public class Reportsservice
    {
        private readonly Connectioncontextdb _context;
        private readonly IWebHostEnvironment _env;

        public Reportsservice(Connectioncontextdb context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<Reportresponsedto> GetDetailAsync(DateTime from, DateTime? to)
        {
            var fromDate = from.Date;
            var toDate = (to ?? from).Date.AddDays(1);
            var detalles = await _context.SaleDetails
                .Include(d => d.Sale)
                .Include(d => d.Product)
                .Where(d => d.Sale!.Date >= fromDate && d.Sale.Date < toDate)
                .ToListAsync();

            var filas = detalles.Select(d => new Reportrowdto
            {
                SaleId = d.SaleId,
                Fecha = d.Sale!.Date,
                Cliente = d.Sale.Client,
                ProductId = d.ProductId,
                Producto = d.Product?.Name ?? "",
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                TotalPrice = d.Quantity * d.UnitPrice
            })
                .OrderBy(f => f.Fecha)
                .ThenBy(f => f.SaleId)
                .ToList();

            var total = filas.Sum(f => f.TotalPrice);
            var ventasDistintas = filas.Select(f => f.SaleId).Distinct().Count();
            var promedio = ventasDistintas == 0 ? 0 : total / ventasDistintas;

            return new Reportresponsedto
            {
                DateFrom = fromDate,
                DateTo = (to ?? from).Date,
                TotalAmount = total,
                SalesCount = ventasDistintas,
                AverageAmount = promedio,
                Rows = filas
            };
        }

        public async Task<byte[]> GetDetailPdfAsync(DateTime from, DateTime? to)
        {
            var data = await GetDetailAsync(from, to);
            return GenerateDetailsPdf(
                data.DateFrom,
                data.DateTo,
                data.TotalAmount,
                data.SalesCount,
                data.AverageAmount,
                data.Rows
                );
        }

        public async Task<byte[]> GetSaleTicketPdfAsync(int saleId)
        {
            var sale = await _context.Sales
                .Include(s => s.Details)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(s => s.Id == saleId);
            if (sale == null)
                throw new Exception("Venta no encontrada");
            return GenerateTicketPdf(sale);
        }
        private byte[] GenerateDetailsPdf(
            DateTime desde,
            DateTime hasta,
            decimal total,
            int ventasDistintias,
            decimal promedio,
            List<Reportrowdto> filas)
        {
            byte[]? logoData = LoadLogo();
            return Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);

                    page.Header().Row(row =>
                    {
                        if (logoData != null)
                            row.ConstantItem(60).Image(logoData);
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Reporte de Ventas").FontSize(20).SemiBold();
                            col.Item().Text($"Desde {desde:dd/MM/yyyy} - Hasta {hasta:dd/MM/yyyy} ")
                            .FontSize(10);
                        });
                    });
                    page.Content().Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            Card(r, "Total vendido", total.ToString("C2"));
                            Card(r, "Ventas", ventasDistintias.ToString());
                            Card(r, "Promedio", promedio.ToString("C2"));
                        });
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(50);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                                c.ConstantColumn(40);
                                c.ConstantColumn(60);
                                c.ConstantColumn(60);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Element(HeadingCell).Text("Venta");
                                header.Cell().Element(HeadingCell).Text("Fecha");
                                header.Cell().Element(HeadingCell).Text("Cliente");
                                header.Cell().Element(HeadingCell).Text("Cant.");
                                header.Cell().Element(HeadingCell).Text("P. Unit");
                                header.Cell().Element(HeadingCell).Text("Total");
                            });
                            foreach (var f in filas)
                            {
                                table.Cell().Text(f.SaleId.ToString());
                                table.Cell().Text(f.Fecha.ToString("dd/MM/yyyy"));
                                table.Cell().Text(f.Cliente);
                                table.Cell().Text(f.Quantity.ToString());
                                table.Cell().Text(f.UnitPrice.ToString("C2"));
                                table.Cell().Text(f.TotalPrice.ToString("C2"));
                            }
                        });
                    });
                });
            }).GeneratePdf();
        }
        private IContainer HeadingCell(IContainer container)
        {
            return container
                .Background(Colors.Blue.Medium)
                .Padding(3)
                .DefaultTextStyle(x => x
                    .FontSize(9)
                    .FontColor(Colors.White)
                    .SemiBold());
        }
        private void Card(RowDescriptor row, string tittle, string value)
        {
            row.RelativeItem().Border(1).Padding(8).Padding(8).Column(c =>
            {
                c.Item().Text(tittle).FontSize(10);
                c.Item().Text(value).FontSize(14).SemiBold();
            });
        }
        private byte[] GenerateTicketPdf(Sale sale)
        {
            byte[]? logoData = LoadLogo();

            return Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A6);
                    page.Margin(15);

                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text("JenApp").FontSize(16).SemiBold();
                        col.Item().Text($"Venta #{sale.Id}");
                        col.Item().Text($"Fecha: {sale.Date:dd/MM/yyyy HH:mm}");
                        col.Item().Text($"Cliente: {sale.Client}");

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Text("Prod").SemiBold();
                                h.Cell().Text("Cant").SemiBold();
                                h.Cell().Text("Sub").SemiBold();
                            });

                            foreach (var d in sale.Details)
                            {
                                table.Cell().Text(d.Product?.Name ?? "");
                                table.Cell().Text(d.Quantity.ToString());
                                table.Cell().Text(d.TotalPrice.ToString("C0"));
                            }
                        });

                        col.Item().AlignRight().Text($"TOTAL: {sale.Total:C0}").SemiBold();
                    });
                });
            }).GeneratePdf();
        }

        private byte[]? LoadLogo()
        {
            try
            {
                var path = Path.Combine(_env.ContentRootPath, "Images", "jenapp-logo.jpeg");
                return File.Exists(path) ? File.ReadAllBytes(path) : null;
            }
            catch
            {
                return null;
            }
        }
    }
}