using MainWebApplication.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MainWebApplication.Areas.Wash.Models;
namespace MainWebApplication.Areas.Wash.Controllers
{
    [Area("Wash")]
    public class MasterController : Controller
    {
        private UserManager<AspNetUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        private UserManager<AspNetUser> _userManager;
        private ApplicationDbContext db;
        private Task<AspNetUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
        public MasterController(RoleManager<IdentityRole> roleMgr, UserManager<AspNetUser> userMrg, ApplicationDbContext context)
        {
            roleManager = roleMgr;
            userManager = userMrg;
            db = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Tasks()
        {
            var result = await db.WashServices.Include(x => x.Service).Include(x => x.WashOrder.Cars.ModelCars).
                Where(x => x.AspNetUserId == GetCurrentUserAsync().Result.Id && x.Status.Name == "Не готово").
                Where(x=>!x.WashOrder.IsDeleted && !x.IsDeleted).
                ToListAsync();
            return View(result);
        }
        public async Task<IActionResult> CompletedTasks()
        {
            var result = await db.WashServices.Include(x => x.Service).Include(x => x.WashOrder.Cars.ModelCars).
                Where(x => x.AspNetUserId == GetCurrentUserAsync().Result.Id && x.Status.Name == "Готово").
                 Where(x => !x.WashOrder.IsDeleted).
                ToListAsync();
            return View(result);
        }
        public async Task<IActionResult> WashCar(int? id, string s)
        {
            if (id == null)
            {
                return NotFound();
            }
            WashService? service = await db.WashServices.
                Include(x=>x.WashOrder.Cars.ModelCars).
                Include(x=>x.AspNetUser).
                Include(x=>x.Service).Include(x=>x.Status).
                Where(x=>x.AspNetUserId == GetCurrentUserAsync().Result.Id)
                .Where(x=>!x.IsDeleted && x.Status.Name == "Не готово").
                FirstOrDefaultAsync(x=>x.Id == id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }
        [HttpPost]
        public async Task<IActionResult> WashCar(int? id, SalaryMasterList salary)
        {
            if (id == null)
            {
                return NotFound();
            }
            WashService service = await db.WashServices.
                Where(x=>x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
            if (service == null)
            {
                return NotFound();
            }
            service.StatusId = 2;
            db.Entry(service).State = EntityState.Modified;

            salary.WashOrderId = service.WashOrderId;
            salary.WashServiceId = service.Id;
            salary.AspNetUserId = service.AspNetUserId;
            salary.OrganizationId = service.OrganizationId;
            salary.Salary = service.MasterSalaryInt;
            await db.SalaryMasterLists.AddAsync(salary);

            await db.SaveChangesAsync();

            return RedirectToAction("Tasks");
        }

    }
}
