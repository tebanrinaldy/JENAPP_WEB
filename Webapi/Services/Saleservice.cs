using Webapi.Data;
using Webapi.Models;
using Microsoft.EntityFrameworkCore;

namespace Webapi.Services
{
    public class Saleservice
    {
        private readonly Connectioncontextdb _context;
        public  Saleservice(Connectioncontextdb context)
        {
            _context = context;
        }

        public async Task<List<Sale>> GetAllSalesAsync()
        {
            return await _context.Sales
                .Include(s => s.Details)
                .ThenInclude(d => d.Product)
                .ToListAsync();
        }

        public async Task<Sale?> GetSaleByIdAsync(int id)
        {
            return await _context.Sales
                .Include(s => s.Details)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Sale> CreateSaleAsync(Sale sale)
        {
            if (sale == null || sale .Details == null || sale.Details.Count == 0)
                throw new ArgumentException("La venta debe tener al menos un detalle.");

            foreach (var detail in sale.Details)
                detail.TotalPrice = detail.Quantity * detail.UnitPrice;
            sale.Total = sale.Details.Sum(d => d.TotalPrice);
            sale.Date = DateTime.Now;
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            return await _context.Sales
                .Include(s => s.Details)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(s => s.Id == sale.Id) 
                ?? throw new Exception("Error al crear la venta.");

        }

        public async Task DeleteSaleAsync(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Details)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
                throw new KeyNotFoundException("Venta no encontrada.");

            _context.SaleDetails.RemoveRange(sale.Details);

            _context.Sales.Remove(sale);

            await _context.SaveChangesAsync();
        }
    }
}
