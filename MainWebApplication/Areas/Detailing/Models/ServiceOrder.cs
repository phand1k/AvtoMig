using MainWebApplication.Areas.Identity.Data;
using MainWebApplication.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MainWebApplication.Areas.Detailing.Models
{
    public class ServiceOrder
    {
        [BindNever]
        public int Id { get; set; }
        [Display(Name = "Услуга")]
        [ForeignKey("ServiceId")]
        public int? ServiceId { get; set; }
        public Service? Service { get; set; }
        [Display(Name = "Ответсвенный")]
        [Required]
        [ForeignKey("AspNetUserId")]
        public string? AspNetUserId { get; set; }
        public AspNetUser? AspNetUser { get; set; }
        [Display(Name = "Дата добавления услуги на заказ-наряд")]
        public DateTime DateOfCreateServiceOrder { get; set; } = DateTime.Now;
        [Display(Name = "Статус")]
        [ForeignKey("StatusId")]
        public int? StatusId { get; set; } = 1;
        public Status? Status { get; set; }
        [ForeignKey("OrganizationId")]
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public bool IsDeleted { get; set; } = false;
        [ForeignKey("RegisterOrderId")]
        public int? RegisterOrderId { get; set; }
        public RegisterOrder? RegisterOrder { get; set; }
        [Display(Name = "Цена за услугу")]
        public int? ServiceOrderPrice { get; set; }
        [Display(Name = "Цена за услугу")]
        public string? Price { get; set; }
        [Display(Name = "Зарплата мастера")]
        public string? SalaryMaster { get; set; }
        [Display(Name = "Цена за услугу")]
        public int? Salary { get; set; }
    }
}
