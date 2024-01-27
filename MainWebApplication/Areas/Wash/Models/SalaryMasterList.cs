using MainWebApplication.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MainWebApplication.Areas.Wash.Models
{
    public class SalaryMasterList
    {
        [BindNever]
        [Key]
        public int Id { get; set; }
        [Display(Name = "Мастер")]
        [ForeignKey("AspNetUserId")]
        public string? AspNetUserId { get; set; }
        public AspNetUser? AspNetUser { get; set; }
        [Display(Name = "Заказ-наряд")]
        [ForeignKey("WashOrderId")]
        public int? WashOrderId { get; set; }
        public WashOrder? WashOrder { get; set; }
        [Display(Name = "Услуга")]
        [ForeignKey("WashServiceId")]
        public int? WashServiceId { get; set; }
        public WashService? WashService { get; set; }

        [Display(Name = "Дата выполнения услуги")]
        public DateTime DateOfWash { get; set; } = DateTime.Now;
        [Display(Name = "Зарплата")]
        public int? Salary { get; set; }
        [ForeignKey("OrganizationId")]
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
