using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Webapi.Data;
using Webapi.Hubs;
using Webapi.Models;
using Webapi.Services;

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

        // POST: api/PendingSales
        
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> CreatePendingSale([FromBody] PendingSale pendingSale)
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

            // Código de seguimiento si no viene
            if (string.IsNullOrWhiteSpace(pendingSale.TrackingCode))
                pendingSale.TrackingCode = Guid.NewGuid().ToString("N")[..8];

            _context.PendingSales.Add(pendingSale);
            await _context.SaveChangesAsync();

           
            await _hub.Clients.All.SendAsync("PendingSaleCreated", new
            {
                pendingSale.Id,
                pendingSale.Client,
                pendingSale.Phone,
                pendingSale.Total,
                pendingSale.Date,
                pendingSale.Status,
                pendingSale.TrackingCode
            });

    
            return Ok(new
            {
                pendingSale.Id,
                pendingSale.Client,
                pendingSale.Phone,
                pendingSale.Total,
                pendingSale.Date,
                pendingSale.Status,
                pendingSale.TrackingCode
            });
        }

        // GET: api/PendingSales/search?code=XXX&phone=YYY
      
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchPendingSale(
            [FromQuery] string? code,
            [FromQuery] string? phone)
        {
            if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(phone))
                return BadRequest("Debe enviar un código o teléfono.");

            PendingSale? sale = null;

            if (!string.IsNullOrWhiteSpace(code))
            {
                sale = await _context.PendingSales
                    .FirstOrDefaultAsync(p => p.TrackingCode == code);
            }
            else if (!string.IsNullOrWhiteSpace(phone))
            {
                sale = await _context.PendingSales
                    .Where(p => p.Phone == phone)
                    .OrderByDescending(p => p.Date)
                    .FirstOrDefaultAsync();
            }

            if (sale == null)
                return NotFound("No se encontró el pedido.");

            return Ok(new
            {
                sale.TrackingCode,
                sale.Client,
                sale.Phone,
                sale.Status,
                sale.Date,
                sale.Total
            });
        }

        // GET:

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

        // GET: 
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

        // GET: 
        
        [HttpGet("track/{code}")]
        [AllowAnonymous]
        public async Task<ActionResult> TrackByCode(string code)
        {
            var sale = await _context.PendingSales
                .AsNoTracking()
                .Where(p => p.TrackingCode == code)
                .Select(p => new
                {
                    p.TrackingCode,
                    p.Client,
                    p.Phone,
                    p.Status,
                    p.Date,
                    p.Total
                })
                .FirstOrDefaultAsync();

            if (sale == null)
                return NotFound(new { message = "Pedido no encontrado" });

            return Ok(sale);
        }

        // PUT: 
       
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
                Email = pending.Email,              
                Phone = pending.Phone,              
                Address = pending.Address,          
                PaymentMethod = pending.PaymentMethod, 
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
                pending.Status,
                pending.TrackingCode
            });

            return Ok(createdSale);
        }

        // PUT: 
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
                pending.Status,
                pending.TrackingCode
            });

            return Ok(pending);
        }
    }
}
