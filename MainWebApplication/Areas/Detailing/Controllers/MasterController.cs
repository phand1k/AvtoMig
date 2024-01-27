using MainWebApplication.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MainWebApplication.Areas.Detailing1.Controllers
{
    [Area("Detailing")]
    [Authorize(Roles = "Мастер")]
    public class MasterController : Controller
    {
        public ApplicationDbContext db;
        private UserManager<AspNetUser> _userManager;
        private Task<AspNetUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
        public MasterController(ApplicationDbContext _db, UserManager<AspNetUser> userManager)
        {
            _userManager = userManager;
            db = _db;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Tasks()
        {
            var listOfTasks = await db.ServiceOrders.
                Include(x => x.RegisterOrder.ModelCar).
                Include(x => x.RegisterOrder.Cars).Where(x => x.AspNetUserId == GetCurrentUserAsync().Result.Id && x.Status.Name == "В работе").ToListAsync();
            return View(listOfTasks);
        }
        public IActionResult PageNotFound()
        {
            return View();
        }
    }
}
