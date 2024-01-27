using MainWebApplication.Areas.Detailing.Models;
using MainWebApplication.Areas.Wash.Models;
using System.ComponentModel.DataAnnotations;

namespace MainWebApplication.Models
{
    public class Car
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Машина")]
        public string? Name { get; set; }
        public ICollection<ModelCar> ModelCars { get; set; }
        public ICollection<RegisterOrder> RegisterOrders { get; set; }
        public ICollection<WashOrder> WashOrders { get; set; }
        public Car()
        {
            RegisterOrders = new List<RegisterOrder>();
            ModelCars = new List<ModelCar>();
            WashOrders = new List<WashOrder>();
        }
        public bool IsDeleted { get; set; } = false;
    }
}
