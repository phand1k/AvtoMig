using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MainWebApplication.Areas.Detailing.Models;
using MainWebApplication.Areas.Wash.Models;
using MainWebApplication.Models;

namespace MainWebApplication.Areas.Identity.Data;

// Add profile data for application users by adding properties to the AspNetUser class
public class AspNetUser : IdentityUser
{
    [Display(Name = "Имя")]
    public string? FirstName { get; set; }
    [Display(Name = "Фамилия")]
    public string? LastName { get; set; }
    [Display(Name="Дата регистрации")]
    public DateTime DateOfCreated { get; set; } = DateTime.Now;
    public int OrganizationId { get; set; }
    public bool IsDeleted { get; set; } = false;
    [StringLength(12, ErrorMessage = "БИН/ИИН Организации должен содержать в себе 12 сиволов")]
    [ForeignKey("OrganizationId")]
    [Display(Name = "Рабочая организация")]
    public string? OrganizationNumber { get; set; }
    public ICollection<Service> Services { get; set; }
    public ICollection<RegisterOrder> RegisterOrders { get; set; }
    public ICollection<ServiceOrder> ServiceOrders { get; set; }
    public Organization? Organization { get; set; }
    public ICollection<WashOrder> WashOrders { get; set; }
    public ICollection<SalaryMaster> SalaryMasters { get; set; }
    public ICollection<SalaryMasterList> SalaryMasterLists { get; set; }
    public AspNetUser()
    {
        SalaryMasterLists = new List<SalaryMasterList>();
        SalaryMasters = new HashSet<SalaryMaster>();
        RegisterOrders = new List<RegisterOrder>();
        Services = new List<Service>();
        ServiceOrders = new List<ServiceOrder>();
        WashOrders = new List<WashOrder>();
    }
    [Display(Name ="ФИО")]
    public string FullName
    {
        get
        {
            return FirstName + " " + LastName;
        }
    }
}

