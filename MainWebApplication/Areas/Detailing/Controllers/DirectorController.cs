using Microsoft.AspNetCore.Mvc;
using MainWebApplication.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using MainWebApplication.ViewModels;
using MainWebApplication.Areas.Detailing.Models;
using MainWebApplication.Models;

namespace MainWebApplication.Areas.Detailing1.Controllers
{
    [Authorize(Roles = "Руководитель")]
    [Area("Detailing")]
    public class DirectorController : Controller
    {
        private UserManager<AspNetUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        private UserManager<AspNetUser> _userManager;
        private ApplicationDbContext db;
        public DirectorController(RoleManager<IdentityRole> roleMgr, UserManager<AspNetUser> userMrg, ApplicationDbContext context)
        {
            roleManager = roleMgr;
            userManager = userMrg;
            db = context;
            _userManager = userManager;
        }
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "404, Requested resource was not found";
                    break;
            }

            return RedirectToAction("Home", "PageNotFound");
        }
        public IActionResult RegisterOrder()
        {
            ViewData["CarId"] = new SelectList(db.Cars.Where(x=>!x.IsDeleted), "Id", "Name");
            ViewData["PrepaymentTypes"] = new SelectList(db.Payments, "Id", "Name");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterOrder(RegisterOrder order)
        {
            ViewData["CarId"] = new SelectList(db.Cars, "Id", "Name");
            ViewData["PrepaymentTypes"] = new SelectList(db.Payments, "Id", "Name");
            if (order.Prepayment == null)
            {
                order.Prepayment = 0;
            }
            order.AspNetUserId = GetCurrentUserAsync().Result.Id;
            order.OrganizationId = GetCurrentUserAsync().Result.OrganizationId;

            await db.RegisterOrders.AddAsync(order);
            await db.SaveChangesAsync();
            return RedirectToAction("Orders");
        }
        public JsonResult SaveAuto(string data)
        {
            return Json(0);
        }
        public async Task<IActionResult> Orders()
        {
            var listOrders = db.RegisterOrders.Include(x => x.Cars).Include(x => x.Cars.ModelCars).
                Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId);
            return View(await listOrders.ToListAsync());
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> DetailsOrder(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("PageNotFound");
            }
            RegisterOrder order = await db.RegisterOrders.Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).
                Include(x => x.AspNetUser).Include(x => x.PrepaymentType).
                Include(x => x.Cars.ModelCars).
                FirstOrDefaultAsync(x => x.Id == id);
            if (order == null)
            {
                return RedirectToAction("PageNotFound");
            }
            return View(order);
        }
        public IActionResult CreateService()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateService(Service service)
        {
            var checkService = db.Services.
                    Where(x => !x.IsDeleted).Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).
                    FirstOrDefault(x => x.Name == service.Name);
            if (checkService == null)
            {
                service.AspNetUserId = GetCurrentUserAsync().Result.Id;
                service.OrganizationId = GetCurrentUserAsync().Result.OrganizationId;
                await db.Services.AddAsync(service);
                await db.SaveChangesAsync();
                return RedirectToAction("ServicesList");
            }
            ModelState.AddModelError("", "Такая услуга уже существует");
            return View(service);
        }
        public IActionResult ServicesList()
        {
            var listServices = db.Services.Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId);
            return View(listServices.ToList());
        }
        [HttpPost]
        public IActionResult DeleteService(int? id)
        {
            Service service = db.Services.FirstOrDefault(x => x.Id == id);
            if (service != null)
            {
                service.IsDeleted = true;
                db.Entry(service).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ServicesList");
            }
            ModelState.AddModelError("", "");
            return View(service);
        }
        public async Task<IActionResult> EditOrder(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("PageNotFound");
            }
            RegisterOrder order = await db.RegisterOrders.
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted && !x.FullDelete).
                FirstOrDefaultAsync(x => x.Id == id);
            ViewData["CarId"] = new SelectList(db.Cars, "Id", "Name");
            if (order == null)
            {
                return RedirectToAction("PageNotFound");
            }
            return View(order);
        }
        public IActionResult PageNotFound()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditOrder(RegisterOrder order)
        {
            ViewData["CarId"] = new SelectList(db.Cars, "Id", "Name");
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Orders");
            }
            ModelState.AddModelError("", "");
            return View(order);
        }
        [HttpPost]
        public IActionResult DeleteOrder(int? id)
        {
            RegisterOrder order = db.RegisterOrders.FirstOrDefault(x => x.Id == id);
            if (order != null)
            {
                order.IsDeleted = true;
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Orders");
            }
            ModelState.AddModelError("", "");
            return View(order);
        }
        [HttpPost]
        public IActionResult ReturnOder(int? id)
        {
            RegisterOrder order = db.RegisterOrders.FirstOrDefault(x => x.Id == id);
            if (order != null)
            {
                order.IsDeleted = false;
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("DeletedOrders");
            }
            ModelState.AddModelError("", "");
            return View(order);
        }
        [HttpPost]
        public IActionResult FullDelete(int? id)
        {
            RegisterOrder order = db.RegisterOrders.FirstOrDefault(x => x.Id == id);
            if (order != null)
            {
                order.IsDeleted = true;
                order.FullDelete = true;
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("DeletedOrders");
            }
            ModelState.AddModelError("", "");
            return View(order);
        }
        public async Task<IActionResult> DeletedOrders()
        {
            var orders = await db.RegisterOrders.Include(x => x.Cars.ModelCars)
                .Include(x => x.AspNetUser).Where(x => x.IsDeleted && !x.FullDelete && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).ToListAsync();
            return View(orders);
        }
        public async Task<IActionResult> EditService(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("PageNotFound");
            }
            var service = await db.Services.
                Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).
                FirstOrDefaultAsync(x => x.Id == id);
            if (service == null)
            {
                return RedirectToAction("PageNotFound");
            }
            return View(service);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(Service service)
        {
            var checkServiceName = await db.Services.
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted)
                .FirstOrDefaultAsync(x => x.Name == service.Name);
            if (checkServiceName == null)
            {
                db.Entry(service).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("ServicesList");
            }
            ModelState.AddModelError("Name", "Такая услуга с таким названием уже имеется в базе. Попробуйте назвать услугу по другому");
            return View(service);
        }
        public JsonResult LoadModelCar(int? id)
        {
            var modelCars = db.ModelCars.Where(x => x.CarId == id).ToList();
            return Json(new SelectList(modelCars, "Id", "Name"));
        }
        /*public ViewResult Index() => View(roleManager.Roles);
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            if (role != null)
            {
                IdentityResult result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else
                    Errors(result);
            }
            else
                ModelState.AddModelError("", "No role found");
            return View("Index", roleManager.Roles);
        }
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create([Required] string name)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await roleManager.CreateAsync(new IdentityRole(name));
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else
                    Errors(result);
            }
            return View(name);
        }


        // other methods

        public async Task<IActionResult> Update(string id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            List<AspNetUser> members = new List<AspNetUser>();
            List<AspNetUser> nonMembers = new List<AspNetUser>();
            foreach (AspNetUser user in userManager.Users.Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId))
            {
                var list = await userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
                list.Add(user);
            }
            return View(new RoleEdit
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            });
        }
        [HttpPost]
        public async Task<IActionResult> Update(RoleModification model)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                foreach (string userId in model.AddIds ?? new string[] { })
                {
                    AspNetUser user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await userManager.AddToRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
                foreach (string userId in model.DeleteIds ?? new string[] { })
                {
                    AspNetUser user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await userManager.RemoveFromRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
            }

            if (ModelState.IsValid)
                return RedirectToAction(nameof(Index));
            else
                return await Update(model.RoleId);
        }
        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }*/
        private Task<AspNetUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
        public async Task<IActionResult> ListUsers()
        {
            var users = _userManager.Users.Where(x=>x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).Select(c => new UsersViewModel()
            {
                FullName = c.FirstName + " " + c.LastName,
                PhoneNumber = c.PhoneNumber,
                Role = string.Join(",", _userManager.GetRolesAsync(c).Result.ToArray())
            }).ToList();

            return View(users);
        }
        public async Task<IActionResult> MyOrganization()
        {
            var organization = await db.Organizations.Include(x => x.TypeOrganization).
                Where(x => x.Id == GetCurrentUserAsync().Result.OrganizationId).FirstOrDefaultAsync();
            return View(organization);
        }
        public async Task<IActionResult> EditOrganization(int? id)
        {
            var organization = await db.Organizations.Where(x => x.Id == id).FirstOrDefaultAsync();
            return View(organization);
        }
        [HttpPost]
        public async Task<IActionResult> EditOrganization(Organization organization)
        {
            var checkOrganizationNumber = await db.Organizations.
                FirstOrDefaultAsync(x => x.OrganizationNumber == organization.OrganizationNumber);
            if (ModelState.IsValid)
            {
                if (checkOrganizationNumber == null)
                {
                    db.Entry(organization).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("MyOrganization");
                }
                ModelState.AddModelError("OrganizationNumber", "Организация с таким ИИН/БИН уже существует");
                return View(organization);
            }
            ModelState.AddModelError("", "");
            return View(organization);
        }
        public IActionResult ChangeRoleUser(string id)
        {
            if (id == null)
            {

            }
            return View();

        }
    }
}
