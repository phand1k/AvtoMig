using System.ComponentModel.DataAnnotations;

namespace MainWebApplication.ViewModels
{
    public class UsersViewModel
    {
        public string UserId { get; set; }
        [Display(Name = "Номер телефона")]
        public string PhoneNumber { get; set; }
        [Display(Name = "Роли")]
        public string Role { get; set; }
        [Display(Name = "ФИО")]
        public string FullName { get; set; }
    }
}
