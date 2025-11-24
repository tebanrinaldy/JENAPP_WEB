using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Webapi.Data;
using Webapi.Hubs;
using Webapi.Models;
using Webapi.Services;
using Microsoft.EntityFrameworkCore;

namespace Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PendingSalesController : ControllerBase
    {
        private readonly Connectioncontextdb _context;
        private readonly Saleservice _saleService;
        private readonly IHubContext<NotificationsHub> _hub;

        public PendingSalesController(
            Connectioncontextdb context,
            Saleservice saleService,
            IHubContext<NotificationsHub> hub)
        {
            _context = context;
            _saleService = saleService;
            _hub = hub;
        }

        [HttpPost]
        public async Task<ActionResult<PendingSale>> CreatePendingSale([FromBody] PendingSale pendingSale)
        {
            if (pendingSale == null || pendingSale.Details == null || !pendingSale.Details.Any())
                return BadRequest("La venta debe tener al menos un detalle.");

            foreach (var d in pendingSale.Details)
            {
                d.PendingSale = null;
                d.Product = null;
                d.TotalPrice = d.Quantity * d.UnitPrice;
            }

            pendingSale.Total = pendingSale.Details.Sum(d => d.TotalPrice);
            pendingSale.Status = "Pendiente";
            pendingSale.Date = DateTime.Now;

            _context.PendingSales.Add(pendingSale);
            await _context.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("PendingSaleCreated", new
            {
                pendingSale.Id,
                pendingSale.Client,
                pendingSale.Total,
                pendingSale.Date,
                pendingSale.Status
            });



            return CreatedAtAction(nameof(GetPendingSaleById), new { id = pendingSale.Id }, pendingSale);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PendingSale>>> GetPendingSales()
        {
            var sales = await _context.PendingSales
                .Include(p => p.Details)
                .ThenInclude(d => d.Product)
                .Where(p => p.Status == "Pendiente")
                .OrderByDescending(p => p.Date)
                .ToListAsync();

            return Ok(sales);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<PendingSale>> GetPendingSaleById(int id)
        {
            var sale = await _context.PendingSales
                .Include(p => p.Details)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (sale == null)
                return NotFound();

            return Ok(sale);
        }

        [HttpPut("confirm/{id}")]
        [Authorize]
        public async Task<IActionResult> ConfirmPendingSale(int id)
        {
            var pending = await _context.PendingSales
                .Include(p => p.Details)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pending == null)
                return NotFound();

            if (pending.Status != "Pendiente")
                return BadRequest("La venta ya fue procesada.");

            var sale = new Sale
            {
                Client = pending.Client,
                Date = DateTime.Now,
                Total = pending.Total,
                Details = pending.Details.Select(d => new SaleDetail
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    TotalPrice = d.TotalPrice
                }).ToList()
            };

            var createdSale = await _saleService.CreateSaleAsync(sale);

            pending.Status = "Confirmada";
            await _context.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("PendingSaleUpdated", new
            {
                pending.Id,
                pending.Status
            });


            return Ok(createdSale);
        }

        [HttpPut("reject/{id}")]
        [Authorize]
        public async Task<IActionResult> RejectPendingSale(int id)
        {
            var pending = await _context.PendingSales.FindAsync(id);

            if (pending == null)
                return NotFound();

            if (pending.Status != "Pendiente")
                return BadRequest("La venta ya fue procesada.");

            pending.Status = "Rechazada";
            await _context.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("PendingSaleUpdated", new
            {
                pending.Id,
                pending.Status
            });
    
            return Ok(pending);
        }
    }
}