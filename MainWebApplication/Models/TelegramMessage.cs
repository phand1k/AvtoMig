using Microsoft.AspNetCore.Mvc;

namespace MainWebApplication.Models
{
    public class TelegramMessage
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
