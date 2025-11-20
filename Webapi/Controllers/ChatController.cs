using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Webapi.Services;

namespace Webapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ChatController : ControllerBase
    {
        private readonly ChatbotService _chatbotService;

        public ChatController(ChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        public record ChatRequest(string Mensaje);

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Mensaje))
                return BadRequest("Mensaje vacío");

            var respuesta = await _chatbotService.ResponderAsync(req.Mensaje);

            return Ok(new { respuesta });
        }
    }
}
