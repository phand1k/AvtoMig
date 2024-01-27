using MainWebApplication.Areas.Detailing.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MainWebApplication.Models
{
    public class Service
    {
        public int Id { get; set; }
        [Display(Name = "Название услуги")]
        public string Name { get; set; }
        [ForeignKey("OrganizationId")]
        public int? OrganizationId { get; set; }
        [Required]
        public DateTime DateOfCreated { get; set; } = DateTime.Now;
        [ForeignKey("AspNetUserId")]
        public string AspNetUserId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<ServiceOrder> ServiceOrders { get; set; }
        public ICollection<SalaryMaster> SalaryMasters { get; set; }
        [Display(Name = "Цена за услугу")]
        public int? Price { get; set; } = 0;
        public Service()
        {
            SalaryMasters = new List<SalaryMaster>();
            ServiceOrders = new List<ServiceOrder>();
        }
    }
}
