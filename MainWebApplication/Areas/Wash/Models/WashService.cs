using MainWebApplication.Areas.Identity.Data;
using MainWebApplication.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MainWebApplication.Areas.Wash.Models
{
    public class WashService
    {
        [BindNever]
        [Key]
        public int Id { get; set; }
        [ForeignKey("WashOrderId")]
        public int? WashOrderId { get; set; }
        public WashOrder? WashOrder { get; set; }
        [Required(ErrorMessage = "Выберите услугу")]
        [Display(Name = "Услуга")]
        [ForeignKey("ServiceId")]
        public int? ServiceId { get; set; }
        public Service? Service { get; set; }
        [Display(Name = "Ответсвенный")]
        [ForeignKey("AspNetUserId")]
        [Required(ErrorMessage = "Выберите мастера")]
        public string AspNetUserId { get; set; }
        public AspNetUser? AspNetUser { get; set; }
        [Display(Name = "Дата добавления услуги на заказ-наряд")]
        public DateTime DateOfCreateServiceOrder { get; set; } = DateTime.Now;
        [Display(Name = "Статус")]
        [ForeignKey("StatusId")]
        public int? StatusId { get; set; } = 2;
        public Status? Status { get; set; }
        [ForeignKey("OrganizationId")]
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public bool IsDeleted { get; set; } = false;
        [Display(Name = "Цена за услугу")]
        public int? WashServicePrice { get; set; }
        [Display(Name ="Зарплата мастера")]
        public string? SalaryMasterStr { get; set; }
        [Display(Name = "Зарплата мастера")]
        public int? MasterSalaryInt { get; set; }
        public ICollection<SalaryMasterList> SalaryMasterLists { get; set; }
        public WashService()
        {
            SalaryMasterLists = new List<SalaryMasterList>();
        }
    }
}
