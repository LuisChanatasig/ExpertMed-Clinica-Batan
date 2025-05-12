using System.Threading.Tasks;
using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Crmf;
using static ExpertMed.Services.ChatGPTService;

namespace ExpertMed.Controllers
{
    public class ChatController : Controller
    {
        private readonly ChatGPTService _chatService;

        public ChatController(ChatGPTService chatService)
        {
            _chatService = chatService;
        }
        [HttpPost("ask")]
        public IActionResult Ask([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest("Pregunta vacía.");

            // Simulación de respuesta
            string simulatedResponse = $" Recibí tu mensaje: \"{request.Prompt}\". Te responderé pronto,sigo aprendiendo.";

            return Ok(new { response = simulatedResponse });
        }

    }


    public class ChatRequest
    {
        public string Prompt { get; set; }
    }
}
