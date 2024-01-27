using MainWebApplication.Areas.Detailing.Models;
using MainWebApplication.Areas.Wash.Models;
using System.ComponentModel.DataAnnotations;

namespace MainWebApplication.Models
{
    public class Payment
    {
        public int Id { get; set; }
        [Display(Name="Название способа оплаты")]
        public string Name { get; set; }
        public ICollection<RegisterOrder> RegisterOrders { get; set; }
        public Payment()
        {
            RegisterOrders = new List<RegisterOrder>();
        }
        public bool IsDeleted { get; set; } = false;
    }
}
