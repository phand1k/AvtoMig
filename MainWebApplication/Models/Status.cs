using MainWebApplication.Areas.Detailing.Models;
using MainWebApplication.Areas.Wash.Models;
using System.ComponentModel.DataAnnotations;

namespace MainWebApplication.Models
{
    public class Status
    {
        public int Id { get; set; }
        [Display(Name="Статус")]
        public string Name { get; set; }
        public ICollection<RegisterOrder> RegisterOrders { get; set; }
        public ICollection<ServiceOrder> ServiceOrders { get; set; }
        public ICollection<WashOrder> WashOrders { get; set; }
        public Status()
        {
            RegisterOrders = new List<RegisterOrder>();
            ServiceOrders = new List<ServiceOrder>();
            WashOrders = new List<WashOrder>();
        }
    }
}
