using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MainWebApplication.Models;
using MainWebApplication.Areas.Detailing.Models;
using MainWebApplication.Areas.Wash.Models;
namespace MainWebApplication.Areas.Identity.Data;

public class ApplicationDbContext : IdentityDbContext<AspNetUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<TelegramMessage> TelegramMessages { get; set; }
    public DbSet<SubmitWash> SubmitWashes { get; set; }
    public DbSet<SalaryMasterList> SalaryMasterLists { get; set; }
    public DbSet<SmsActivate> SmsActivates { get; set; }
    public DbSet<SalaryMaster> SalaryMasters { get; set; }
    public DbSet<WashService> WashServices { get; set; }
    public DbSet<WashOrder> WashOrders { get; set; }
    public DbSet<TypeOrganization> TypeOrganizations { get; set; }
    public DbSet<ServiceOrder> ServiceOrders { get; set; }
    public DbSet<Status> Statuses { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<RegisterOrder> RegisterOrders { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<ModelCar> ModelCars { get; set; }
    public DbSet<Car> Cars { get; set; }
    public DbSet<AspNetUser> AspNetUsers { get; set; }
    public DbSet<City> City { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
