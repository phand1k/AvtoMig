using MainWebApplication.Areas.Detailing.Models;
using MainWebApplication.Areas.Wash.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MainWebApplication.Models
{
    public class ModelCar
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Required]
        [ForeignKey("CarId")]
        public int CarId { get; set; }
        public Car Car { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<RegisterOrder> RegisterOrders { get; set; }
        public ICollection<WashOrder> WashOrders { get; set; }
        public ModelCar()
        {
            RegisterOrders = new List<RegisterOrder>();
            WashOrders = new List<WashOrder>();
        }
    }
}
