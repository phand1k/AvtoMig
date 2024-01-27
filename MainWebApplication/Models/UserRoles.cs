using System.ComponentModel.DataAnnotations;

namespace MainWebApplication.Models
{
    public class UserRoles
    {
        [Display(Name = "Название роли")]
        public string RoleName { get; set; }
    }
}
