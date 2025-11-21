using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Webapi.Data;
using Webapi.Models;

namespace Webapi.Services
{
    public class Reportsservice
    {
        private readonly Connectioncontextdb _context;

        public Reportsservice(Connectioncontextdb context)
        {
            _context = context;
        }

        public async Task<ReportResult> GetSalesReportAsync(DateTime from, DateTime to)
        {
            var ventasDb = await _context.Sales
                .Include(s => s.Details)
                .ThenInclude(d => d.Product)
                .Where(s => s.Date.Date >= from.Date && s.Date.Date <= to.Date)
                .ToListAsync();

            var result = new ReportResult
            {
                Desde = from,
                Hasta = to,
                TotalGeneral = ventasDb.Sum(v => v.Details.Sum(d => d.TotalPrice)),
                CantidadVentas = ventasDb.Count,
                PromedioVentas = ventasDb.Count == 0 ? 0 : ventasDb.Average(v => v.Details.Sum(d => d.TotalPrice))

            };

            foreach (var v in ventasDb)
            {
                var rv = new ReportVenta
                {
                    Id = v.Id,
                    Cliente = v.Client,
                    Fecha = v.Date,
                    TotalVenta = v.Details.Sum(d => d.TotalPrice)
                };

                foreach (var d in v.Details)
                {
                    rv.Detalles.Add(new ReportVentaDetalle
                    {
                        Producto = d.Product!.Name,
                        Cantidad = d.Quantity,
                        PrecioUnit = d.UnitPrice,
                        Subtotal = d.TotalPrice
                    });
                }

                result.Ventas.Add(rv);
            }

            return result;
        }

        public async Task<byte[]> GetSalesReportPdfAsync(DateTime from, DateTime to)
        {
            var data = await GetSalesReportAsync(from, to);

            return Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);

                    page.Header()
                        .Text("Reporte de Ventas")
                        .FontSize(20)
                        .SemiBold()
                        .FontColor(Colors.Blue.Darken2);

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Desde: {data.Desde:dd/MM/yyyy}  -  Hasta: {data.Hasta:dd/MM/yyyy}");
                        col.Item().Text($"Total vendido: {data.TotalGeneral:C}");
                        col.Item().Text($"Cantidad ventas: {data.CantidadVentas}");
                        col.Item().Text($"Promedio por venta: {data.PromedioVentas:C}");

                        col.Item()
                            .PaddingVertical(10)
                            .LineHorizontal(1);

                        foreach (var venta in data.Ventas)
                        {
                            col.Item().Text($"Venta #{venta.Id} - {venta.Fecha:dd/MM/yyyy}")
                                .FontSize(14)
                                .SemiBold();

                            col.Item().Text($"Cliente: {venta.Cliente}");

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(3);
                                    c.RelativeColumn(1);
                                    c.RelativeColumn(1);
                                    c.RelativeColumn(1);
                                });

                                table.Header(h =>
                                {
                                    h.Cell().Text("Producto").SemiBold();
                                    h.Cell().Text("Cant.").SemiBold();
                                    h.Cell().Text("P.Unit").SemiBold();
                                    h.Cell().Text("Subt.").SemiBold();
                                });

                                foreach (var d in venta.Detalles)
                                {
                                    table.Cell().Text(d.Producto);
                                    table.Cell().Text(d.Cantidad.ToString());
                                    table.Cell().Text($"{d.PrecioUnit:C}");
                                    table.Cell().Text($"{d.Subtotal:C}");
                                }
                            });

                            col.Item().AlignRight().Text($"TOTAL: {venta.TotalVenta:C}")
                                .SemiBold();

                            col.Item()
                                .PaddingVertical(10)
                                .LineHorizontal(1);
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GetSaleTicketPdfAsync(int saleId)
        {
            var sale = await _context.Sales
                .Include(s => s.Details)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(s => s.Id == saleId);

            if (sale == null)
                throw new Exception("La venta no existe.");

            return Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A6);
                    page.Margin(20);

                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text("JenApp").FontSize(16).SemiBold();
                        col.Item().AlignCenter().Text("TICKET DE VENTA").FontSize(12);

                        col.Item().Text($"Venta No: {sale.Id}");
                        col.Item().Text($"Fecha: {sale.Date:dd/MM/yyyy HH:mm}");
                        col.Item().Text($"Cliente: {sale.Client}");

                        col.Item()
                            .PaddingVertical(10)
                            .LineHorizontal(1);

                        foreach (var d in sale.Details)
                        {
                            col.Item().Text($"{d.Product!.Name}  x{d.Quantity}  =  {d.TotalPrice:C}");
                        }

                        col.Item().AlignRight().Text($"TOTAL: {sale.Total:C}")
                            .FontSize(14)
                            .SemiBold();

                        col.Item().AlignCenter().Text("¡Gracias por su compra!");
                    });
                });
            }).GeneratePdf();
        }
    }
}
