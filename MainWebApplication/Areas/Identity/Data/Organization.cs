using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MainWebApplication.Areas.Detailing.Models;
using MainWebApplication.Areas.Wash.Models;
using MainWebApplication.Models;

namespace MainWebApplication.Areas.Identity.Data
{
    public class Organization
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Название сервиса")]
        [StringLength(30, ErrorMessage = "Введите корректно название сервиса", MinimumLength = 3)]
        public string Name { get; set; }
        [Required(ErrorMessage = "Введите БИН или ИИН организации")]
        [StringLength(12, ErrorMessage = "Ошибка. Поле должно содержать в себе 12 символов!", MinimumLength = 12)]
        [Display(Name = "БИН/ИИН организации")]
        public string OrganizationNumber { get; set; }
        [Required(ErrorMessage = "Введите название организации")]
        [StringLength(30, ErrorMessage = "Введите корректно название", MinimumLength = 5)]
        [Display(Name = "Название организации")]
        public string FullOrganizationName { get; set; }
        [Display(Name = "Дата создания")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [Display(Name="Дата начала подписки")]
        public DateTime StartDateSub { get; set; }
        [Display(Name = "Дата окончания подписки")]
        public DateTime EndDateSub { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Кодовое слово")]
        public string Password { get; set; }
        [ForeignKey("OrganizationId")]
        [Display(Name = "Тип организации")]
        public int? TypeOrganizationId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public TypeOrganization TypeOrganization { get; set; }
        public ICollection<AspNetUser> AspNetUsers { get; set; }
        public ICollection<Service> Services { get; set; }
        public ICollection<RegisterOrder> RegisterOrders { get; set; }
        public ICollection<ServiceOrder> ServiceOrders { get; set; }
        public ICollection<WashOrder> WashOrders { get; set; }
        public ICollection<SalaryMaster> SalaryMasters { get; set; }
        public ICollection<SalaryMasterList> SalaryMasterLists { get; set; }
        public Organization()
        {
            SalaryMasterLists = new List<SalaryMasterList>();
            RegisterOrders = new List<RegisterOrder>();
            Services = new List<Service>();
            AspNetUsers = new List<AspNetUser>();
            ServiceOrders = new List<ServiceOrder>();
            WashOrders = new List<WashOrder>();
            SalaryMasters = new List<SalaryMaster>();
        }
    }
}
