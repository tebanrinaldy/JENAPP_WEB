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
    public class SalesController : ControllerBase
    {
        private readonly Saleservice _saleservice;

        public SalesController(Saleservice saleservice)
        {
            _saleservice = saleservice;
        }

        // GET: api/Sales
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sale>>> GetSales()
        {
            var sales = await _saleservice.GetAllSalesAsync();

            return Ok(sales);
        }

        // GET: api/Sales/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Sale>> GetSale(int id)
        {
            var sale = await _saleservice.GetSaleByIdAsync(id);

            if (sale == null)
            {
                return NotFound();
            }

            return Ok(sale);
        }

        // POST: api/Sales
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Sale>> PostSale([FromBody] Sale sale)
        {
            try
            {
                var createdSale = await _saleservice.CreateSaleAsync(sale);
                return CreatedAtAction(nameof(GetSale), new { id = createdSale.Id }, createdSale);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        

        // DELETE: api/Sales/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSale(int id)
            {
                try
                {
                    await _saleservice.DeleteSaleAsync(id);
                    return NoContent();
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }

            }
    }
}
