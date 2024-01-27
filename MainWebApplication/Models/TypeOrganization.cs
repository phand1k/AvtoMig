using MainWebApplication.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace MainWebApplication.Models
{
    public class TypeOrganization
    {
        public int Id { get; set; }
        [Display(Name = "Вид деятельности")]
        public string Name { get; set; } 
        public ICollection<Organization> Organizations { get; set; }
        public TypeOrganization()
        {
            Organizations = new List<Organization>();
        }
    }
}
