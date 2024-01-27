using System.ComponentModel.DataAnnotations;

namespace MainWebApplication.Models
{
    public class City
    {
        public int Id { get; set; }
        [Required]
        [Display(Name="Город")]
        public string Name { get; set; }
        public City()
        {
        }
    }
}
