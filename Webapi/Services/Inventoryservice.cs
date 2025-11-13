using Microsoft.EntityFrameworkCore;
using Webapi.Data;
using Webapi.Models;

namespace Webapi.Services
{
    public class Inventoryservice
    {
        private readonly Connectioncontextdb _context;
        public Inventoryservice(Connectioncontextdb context)
        {
            _context = context;
        }
        public async Task<List<InventoryMovement>> GetAllAsync()
        {
            return await _context.InventoryMovement
                .Include(m => m.Product)
                .OrderByDescending(m => m.Date)
                .ToListAsync();
        }

        public async Task<InventoryMovement?> RegisterMovementAsync(InventoryMovement movement)
        {
            var product = await _context.Products.FindAsync(movement.ProductId);

            if (product == null)
                throw new ArgumentException("El producto no existe.");

            if (movement.Type == "Salida")
            {
                if (movement.Quantity > product.Stock)
                    throw new InvalidOperationException("Stock insuficiente.");

                product.Stock -= movement.Quantity;
            }
            else if (movement.Type == "Entrada")
            {
                product.Stock += movement.Quantity;
            }
            else
            {
                throw new ArgumentException("Tipo inválido.");
            }

            _context.InventoryMovement.Add(movement);
            _context.Products.Update(product);

            await _context.SaveChangesAsync();
            return movement;
        }
    }
}