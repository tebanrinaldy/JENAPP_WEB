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

        [HttpGet("sales-report")]
        public async Task<IActionResult> GetSalesReport(DateTime from, DateTime to)
        {
            var result = await _service.GetSalesReportAsync(from, to);
            return Ok(result);
        }

        [HttpGet("sales-report/pdf")]
        public async Task<IActionResult> GetSalesReportPdf(DateTime from, DateTime to)
        {
            var pdf = await _service.GetSalesReportPdfAsync(from, to);
            return File(pdf, "application/pdf", $"reporte_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf");
        }

        [HttpGet("ticket/{saleId}")]
        public async Task<IActionResult> GetTicket(int saleId)
        {
            var pdf = await _service.GetSaleTicketPdfAsync(saleId);
            return File(pdf, "application/pdf", $"ticket_{saleId}.pdf");
        }
    }
}