using System.ComponentModel.DataAnnotations;
using MainWebApplication.Models;
using MainWebApplication.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace MainWebApplication.Areas.Wash.Models
{
    public class WashOrder
    {
        public int Id { get; set; }
        public int? StatusId { get; set; }
        public Status? Status { get; set; }
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        [Display(Name = "Марка авто")]
        [Required(ErrorMessage = "Выберите марку авто")]
        public int? CarId { get; set; }
        [ForeignKey("CarId")]
        public Car? Cars { get; set; }
        [Display(Name ="Создал")]
        public string? AspNetUserId { get; set; }
        public AspNetUser? AspNetUser { get; set; }
        [Required]
        [Display(Name ="Гос номер авто")]
        public string? CarNumber { get; set; }
        public bool IsDeleted { get; set; } = false;
        [Required(ErrorMessage = "Выберите модель машины")]
        [Display(Name = "Модель авто")]
        [ForeignKey("ModelCarId")]
        public int? ModelCarId { get; set; }
        public ModelCar? ModelCar { get; set; }
        [Display(Name = "Мобильный номер клиента")]
        public string? PhoneNumberClient { get; set; }
        [Display(Name = "ФИО клиента")]
        public string? ClientName { get; set; }
        public ICollection<WashService> WashServices { get; set; }
        public bool FullDelete { get; set; } = false;
        public ICollection<SalaryMasterList> SalaryMasterLists { get; set; }
        public ICollection<SubmitWash> SubmitWashes { get; set; }
        public DateTime DateOfCreateWashOrder { get; set; } = DateTime.Now;
        [ForeignKey("PaymentId")]
        [Display(Name = "Способ оплаты")]
        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }
        public WashOrder()
        {
            SubmitWashes = new List<SubmitWash>();
            SalaryMasterLists = new List<SalaryMasterList>();
            WashServices = new List<WashService>();
        }
    }
}
