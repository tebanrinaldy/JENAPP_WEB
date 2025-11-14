using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webapi.Data;

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

        // ✅ Endpoint general: por día o por rango
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
    }
}
