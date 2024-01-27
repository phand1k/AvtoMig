using System.ComponentModel.DataAnnotations;

namespace MainWebApplication.ViewModels
{
    public class ProfileViewModel
    {
        public string ProfileId { get; set; }
        [Display(Name = "ФИО")]
        public string FullName { get; set; }
        public DateTime DateOfRegister { get; set; }
        public string Email { get; set; }
        [Display(Name = "Организация")]
        public string OrganizationName { get; set; }
        public string PhoneNumber { get; set; }
        [Display(Name = "Должности")]
        public string Roles { get; set; }
    }
}
