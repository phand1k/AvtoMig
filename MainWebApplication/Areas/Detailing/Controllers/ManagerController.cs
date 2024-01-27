using MainWebApplication.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using MainWebApplication.ViewModels;
using MainWebApplication.Areas.Detailing.Models;
using MainWebApplication.Models;

namespace MainWebApplication.Areas.Detailing1.Controllers
{
    [Authorize(Roles = "Менеджер")]
    [Authorize(Policy ="")]
    [Area("Detailing")]
    public class ManagerController : Controller
    {
        public ApplicationDbContext db;
        private UserManager<AspNetUser> _userManager;
        private Task<AspNetUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
        public ManagerController(ApplicationDbContext _db, UserManager<AspNetUser> userManager)
        {
            _userManager = userManager;
            db = _db;
        }
        public IActionResult RegisterOrder()
        {
            ViewData["CarId"] = new SelectList(db.Cars, "Id", "Name");
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
            order.StatusId = 1;
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
        public async Task<IActionResult> Orders(string? search, string? searchBy)
        {
            var listOrders = db.RegisterOrders.Include(x => x.Cars).Include(x => x.Cars.ModelCars).
                Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId);
            if (!string.IsNullOrEmpty(search) && searchBy == "Гос номер авто")
            {
                listOrders = listOrders.Where(x => x.CarNumber.Contains(search));
            }
            if (!string.IsNullOrEmpty(search) && searchBy == "Марка")
            {
                listOrders = listOrders.Where(s => s.ModelCar.Name.Contains(search) || s.Cars.Name.Contains(search));
            }
            return View(await listOrders.ToListAsync());
        }
        public async Task<IActionResult> DetailsOrder(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("PageNotFound");
            }
            RegisterOrder? order = await db.RegisterOrders.Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).
                Include(x => x.AspNetUser).Include(x => x.PrepaymentType).
                Include(x => x.Cars.ModelCars).
                FirstOrDefaultAsync(x => x.Id == id);
            if (order == null)
            {
                return RedirectToAction("PageNotFound");
            }
            return View(order);
        }
        public IActionResult Index()
        {
            return View();
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
                if (service.Price == null)
                {
                    service.Price = 0;
                }
                service.AspNetUserId = GetCurrentUserAsync().Result.Id;
                service.OrganizationId = GetCurrentUserAsync().Result.OrganizationId;
                await db.Services.AddAsync(service);
                await db.SaveChangesAsync();
                return RedirectToAction("ServicesList");
            }
            ModelState.AddModelError("", "Такая услуга уже существует");
            return View(service);
        }
        public async Task<IActionResult> ServicesList(string serviceName)
        {
            var listServices = db.Services.Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId);
            if (!string.IsNullOrEmpty(serviceName))
            {
                listServices = listServices.Where(x => x.Name.Contains(serviceName));
            }
            return View(await listServices.ToListAsync());
        }
        [HttpPost]
        public async Task<IActionResult> DeleteServicesOrder(int? id)
        {
            ServiceOrder serviceOrder = await db.ServiceOrders.FirstOrDefaultAsync(x => x.Id == id);
            if (serviceOrder != null || id == null)
            {
                serviceOrder.IsDeleted = true;
                db.Entry(serviceOrder).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("AddServicesOrder", new { id = serviceOrder.RegisterOrderId });
            }
            return RedirectToAction("PageNotFound");
        }
        public async Task<IActionResult> EditServicesOrder(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("PageNotFound");
            }
            ServiceOrder? serviceOrder = await db.ServiceOrders.
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).
                FirstOrDefaultAsync(x => x.Id == id);
            ViewData["CarId"] = new SelectList(db.Cars, "Id", "Name");
            if (serviceOrder == null)
            {
                return RedirectToAction("PageNotFound");
            }
            ViewData["Users"] = new SelectList(db.AspNetUsers.
                Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId), "Id", "FullName");
            ViewData["Services"] = new SelectList(db.Services.
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted), "Id", "Name");
            return View(serviceOrder);
        }
        [HttpPost]
        public IActionResult DeleteService(int? id)
        {
            Service service = db.Services.FirstOrDefault(x => x.Id == id);
            if (service != null || id == null)
            {
                service.IsDeleted = true;
                db.Entry(service).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ServicesList");
            }
            return RedirectToAction("PageNotFound");
        }
        public async Task<IActionResult> EditOrder(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("PageNotFound");
            }
            RegisterOrder order = await db.RegisterOrders.
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).
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
        public async Task<IActionResult> EditService(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("PageNotFound");
            }
            var service = await db.Services.FindAsync(id);
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
                if (service.Price == null)
                {
                    service.Price = 0;
                }
                db.Entry(service).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("ServicesList");
            }
            ModelState.AddModelError("Name", "Такая услуга с таким названием уже имеется в базе. Попробуйте назвать услугу по другому");
            return View(service);
        }
        public async Task<IActionResult> AddServicesOrder(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("PageNotFound");
            }
            var order = await db.RegisterOrders.
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted).
                FirstOrDefaultAsync(x => x.Id == id);
            ServiceOrder serviceOrder = await db.ServiceOrders.
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted).FirstOrDefaultAsync();
            ViewBag.ServicesOrder = db.ServiceOrders.Where(x => x.RegisterOrderId == id && !x.IsDeleted).
                Select(x => new ModelForAddService
                {
                    Id = x.Id,
                    NameService = x.Service.Name,
                    NameMaster = x.AspNetUser.FullName,
                    Price = x.ServiceOrderPrice,
                    PriceForParse = x.Price
                });
            if (order == null)
            {
                return RedirectToAction("PageNotFound");
            }
            ViewData["Users"] = new SelectList(db.AspNetUsers.
                 Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId), "Id", "FullName");
            ViewData["Services"] = new SelectList(db.Services.
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted), "Id", "Name");

            return View(serviceOrder);
        }

        [HttpPost]
        public async Task<IActionResult> AddServicesOrder(ServiceOrder serviceOrder, int? id)
        {
            ViewBag.ServicesOrder = db.ServiceOrders.Where(x => x.RegisterOrderId == id && !x.IsDeleted).
                Select(x => new ModelForAddService { Id = x.Id, NameService = x.Service.Name, NameMaster = x.AspNetUser.FullName });
            var checkAddServicesOrder = await db.ServiceOrders.
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && x.RegisterOrderId == id && !x.IsDeleted).
                FirstOrDefaultAsync(x => x.ServiceId == serviceOrder.ServiceId);
            if (id == null)
            {
                return RedirectToAction("PageNotFound");
            }
            int price;
            int salary;
            ViewData["Users"] = new SelectList(db.AspNetUsers.
                Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId), "Id", "FullName");
            ViewData["Services"] = new SelectList(db.Services.
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted), "Id", "Name");
            serviceOrder.StatusId = 1;
            serviceOrder.RegisterOrderId = id;
            serviceOrder.OrganizationId = GetCurrentUserAsync().Result.OrganizationId;
            serviceOrder.IsDeleted = false;
            if (ModelState.IsValid)
            {
                if (checkAddServicesOrder == null)
                {
                    if (serviceOrder.Price == null)
                    {
                        serviceOrder.ServiceOrderPrice = 0;
                        serviceOrder.Price = "0";
                    }
                    if (serviceOrder.SalaryMaster == null)
                    {
                        serviceOrder.Salary = 0;
                        serviceOrder.SalaryMaster = "0";
                    }
                    serviceOrder.SalaryMaster = serviceOrder.SalaryMaster + " ₸";
                    serviceOrder.Price = serviceOrder.Price + " ₸";
                    int.TryParse(string.Join("", serviceOrder.Price.Where(c => char.IsDigit(c))), out price);
                    int.TryParse(string.Join("", serviceOrder.SalaryMaster.Where(c => char.IsDigit(c))), out salary);
                    serviceOrder.Salary = salary;
                    serviceOrder.ServiceOrderPrice = price;
                    await db.ServiceOrders.AddAsync(serviceOrder);
                    await db.SaveChangesAsync();

                    return RedirectToAction("AddServicesOrder", new { id });
                }
                ModelState.AddModelError("", "Услуга уже назначена на авто");
            }
            return View(serviceOrder);
        }
        public async Task<IActionResult> EditServiceOrder(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("PageNotFound");
            }
            var order = await db.ServiceOrders.FindAsync(id);
            if (order == null)
            {
                return RedirectToAction("PageNotFound");
            }
            ViewData["Users"] = new SelectList(db.AspNetUsers.
                Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId), "Id", "FullName");
            ViewData["Services"] = new SelectList(db.Services.
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted), "Id", "Name");
            return View(order);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditServiceOrder(ServiceOrder order)
        {
            int price;
            int salary;
            if (order.Price == null)
            {
                order.Price = "0";
                order.ServiceOrderPrice = 0;
            }
            if (order.SalaryMaster == null)
            {
                order.SalaryMaster = "0";
                order.Salary = 0;
            }
            int.TryParse(string.Join("", order.Price.Where(c => char.IsDigit(c))), out price);
            int.TryParse(string.Join("", order.SalaryMaster.Where(c => char.IsDigit(c))), out salary);
            order.Salary = salary;
            order.ServiceOrderPrice = price;
            db.Entry(order).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("AddServicesOrder", new { id = order.RegisterOrderId });
        }
        public JsonResult LoadPrice(int? id)
        {
            int? prices = db.Services.Where(x => x.Id == id).Select(x => x.Price).FirstOrDefault();
            return Json(prices);
        }
        public JsonResult LoadModelCar(int? id)
        {
            var modelCars = db.ModelCars.Where(x => x.CarId == id).ToList();
            return Json(new SelectList(modelCars, "Id", "Name"));
        }
        public JsonResult Car()
        {
            var cars = db.Cars.ToList();
            return new JsonResult(cars);
        }
    }
}
