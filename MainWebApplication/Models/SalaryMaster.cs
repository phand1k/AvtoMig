using MainWebApplication.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace MainWebApplication.Models
{
    public class SalaryMaster
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Пользователь" )]
        public string? AspNetUserId { get; set; }
        [Required]
        [Display(Name = "Зарплата мастера")]
        public string? SalaryMasterStr { get; set; }
        public int? SalaryMasterInt { get; set; }
        [Display(Name ="Услуга")]
        public int? ServiceId { get; set; }
        public Service? Service { get; set; }
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public AspNetUser? AspNetUser { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
