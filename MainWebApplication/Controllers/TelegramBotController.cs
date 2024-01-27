using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MainWebApplication.Models;
using MainWebApplication.Areas.Identity.Data;
namespace MainWebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TelegramBotController : Controller
    {
        private readonly ILogger<TelegramBotController> _logger;

        public TelegramBotController(ILogger<TelegramBotController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveMessage([FromBody] string message, [FromServices] ApplicationDbContext db)
        {
            _logger.LogInformation($"Received message from Telegram Bot: {message}");

            var telegramMessage = new TelegramMessage
            {
                Content = message,
                CreatedAt = DateTime.Now
            };

            db.TelegramMessages.Add(telegramMessage);
            await db.SaveChangesAsync();

            return Ok();
        }

    }
}
