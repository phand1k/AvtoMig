namespace MainWebApplication.Models
{
    public class SmsActivate
    {
        public int Id { get; set; }
        public string? PhoneNumber { get; set; }
        public string? SMSCode { get; set; }
        public DateTime DateOfStart { get; set; }
        public DateTime DateOfEnd { get; set; }
        public bool IsUsed { get; set; } = false;
    }
}
