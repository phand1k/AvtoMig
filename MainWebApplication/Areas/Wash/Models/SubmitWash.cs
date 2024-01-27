using MainWebApplication.Areas.Identity.Data;
using MainWebApplication.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MainWebApplication.Areas.Wash.Models
{
    public class SubmitWash
    {
        [BindNever]
        [Key]
        public int Id { get; set; }
        public int? WashOrderId { get; set; }
        public WashOrder? WashOrder { get; set; }
        [Display(Name = "Общая сумма услуг")]
        public int? SummServices { get; set; }
        [Display(Name = "Дата сдачи авто клиенту")]
        public DateTime DateOfAutoComplete { get; set; } = DateTime.Now;
        [Display(Name = "Сдал авто")]
        public string? AspNetUserId { get; set; }
        public AspNetUser? AspNetUser { get; set; }
        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }

    }
}
