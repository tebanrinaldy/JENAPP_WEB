using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Webapi.Services;

namespace Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly Reportsservice _service;
        public ReportsController(Reportsservice service)
        {
            _service = service;        
        }

        [HttpGet("details")]
        public async Task<IActionResult> GetDetails(DateTime from, DateTime? to = null)
        {
            var result = await _service.GetDetailAsync(from, to);

            return Ok(result);
        }


        [HttpGet("details/pdf")]
        public async Task<IActionResult> GetDetailsPdf(DateTime from, DateTime? to = null)
        {
            var pdf = await _service.GetDetailPdfAsync(from, to);

            var fileName = $"reporte_{from:yyyyMMdd}.pdf";

            return File(pdf, "application/pdf", fileName);
        }

        [HttpGet("ticket/{saleId}")]
        public async Task<IActionResult> GetTicket(int saleId)
        {
            var pdf = await _service.GetSaleTicketPdfAsync(saleId);

            return File(pdf, "application/pdf", $"ticket-{saleId}.pdf");
        }
    }
}