using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webapi.Data;
using Webapi.Models;
using Webapi.Services;

namespace Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventoryMovementsController : ControllerBase
    {
        private readonly Inventoryservice _inventoryservice;

        public InventoryMovementsController(Inventoryservice inventoryservice)
        {
            _inventoryservice = inventoryservice;
        }

        // GET: api/InventoryMovements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryMovement>>> GetInventoryMovement()
        {
            return Ok(await _inventoryservice.GetAllAsync());
        }

        // POST: api/InventoryMovements
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] InventoryMovement movimiento)
        {
            try
            {
                var result = await _inventoryservice.RegisterMovementAsync(movimiento);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
