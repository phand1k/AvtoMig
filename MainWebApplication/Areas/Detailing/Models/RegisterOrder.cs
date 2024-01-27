using MainWebApplication.Areas.Identity.Data;
using MainWebApplication.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MainWebApplication.Areas.Detailing.Models
{
    public class RegisterOrder
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Display(Name = "Гос номер авто")]
        public string CarNumber { get; set; }
        [Display(Name = "Дата создания заказ-наряда")]
        public DateTime? DateofCreatedOrder { get; set; } = DateTime.Now;
        [Display(Name = "Создал заказ-наряд")]
        [ForeignKey("AspNetUserId")]
        public string? AspNetUserId { get; set; }
        [ForeignKey("OrganizationId")]
        public int? OrganizationId { get; set; }
        [Display(Name = "Предоплата")]
        public int? Prepayment { get; set; } = 0;
        [Display(Name = "Марка авто")]
        [Required(ErrorMessage = "Выберите марку авто")]
        public int? CarId { get; set; }
        [ForeignKey("CarId")]
        public Car? Cars { get; set; }
        public AspNetUser? AspNetUser { get; set; }
        public bool IsDeleted { get; set; } = false;
        [Display(Name = "Модель авто")]
        [ForeignKey("ModelCarId")]
        public int? ModelCarId { get; set; }
        public ModelCar? ModelCar { get; set; }
        [Required(ErrorMessage = "Введите номер клиента")]
        [Display(Name = "Номер клиента")]
        public string PhoneNumberClient { get; set; }
        [Required(ErrorMessage = "Введите ФИО клиента")]
        [Display(Name = "ФИО клиента")]
        public string ClientName { get; set; }
        [Required(ErrorMessage = "Выберите способ предоплаты")]
        [Display(Name = "Способ предоплаты")]
        [ForeignKey("PrepaymentId")]
        public int PrepaymentTypeId { get; set; }
        public Payment PrepaymentType { get; set; }
        public bool FullDelete { get; set; } = false;
        [Display(Name = "Статус")]
        [ForeignKey("StatusId")]
        public int? StatusId { get; set; }
        public Status Status { get; set; }
    }
}
