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

namespace Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SaleDetailsController : ControllerBase
    {
        private readonly Connectioncontextdb _context;

        public SaleDetailsController(Connectioncontextdb context)
        {
            _context = context;
        }

        // GET: api/SaleDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleDetail>>> GetSaleDetails()
        {
            return await _context.SaleDetails.ToListAsync();
        }

        // GET: api/SaleDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SaleDetail>> GetSaleDetail(int id)
        {
            var saleDetail = await _context.SaleDetails.FindAsync(id);

            if (saleDetail == null)
            {
                return NotFound();
            }

            return saleDetail;
        }

        // PUT: api/SaleDetails/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSaleDetail(int id, SaleDetail saleDetail)
        {
            if (id != saleDetail.Id)
            {
                return BadRequest();
            }

            _context.Entry(saleDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SaleDetailExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SaleDetails
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SaleDetail>> PostSaleDetail(SaleDetail saleDetail)
        {
            _context.SaleDetails.Add(saleDetail);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSaleDetail", new { id = saleDetail.Id }, saleDetail);
        }

        // DELETE: api/SaleDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSaleDetail(int id)
        {
            var saleDetail = await _context.SaleDetails.FindAsync(id);
            if (saleDetail == null)
            {
                return NotFound();
            }

            _context.SaleDetails.Remove(saleDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SaleDetailExists(int id)
        {
            return _context.SaleDetails.Any(e => e.Id == id);
        }
    }
}
