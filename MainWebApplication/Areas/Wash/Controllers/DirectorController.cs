using EllipticCurve.Utils;
using MainWebApplication.Areas.Detailing.Models;
using MainWebApplication.Areas.Identity.Data;
using MainWebApplication.Areas.Wash.Models;
using MainWebApplication.Areas.Wash.ViewModels;
using MainWebApplication.Models;
using MainWebApplication.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.RegularExpressions;
namespace MainWebApplication.Areas.Wash.Controllers
{
    [Authorize(Roles="Руководитель")]
    [Area("Wash")]
    public class DirectorController : Controller
    {
        private UserManager<AspNetUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        private UserManager<AspNetUser> _userManager;
        private ApplicationDbContext db;
        private static string MASTERROLE = "c6122f47-74d1-4dfd-a11e-bc372bad0cf4";
        private Task<AspNetUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
        public DirectorController(RoleManager<IdentityRole> roleMgr, UserManager<AspNetUser> userMrg, ApplicationDbContext context)
        {
            roleManager = roleMgr;
            userManager = userMrg;
            db = context;
            _userManager = userManager;
        }
        [HttpGet]
        public IEnumerable<WashOrder> GetUsers()
        {
            return db.WashOrders.ToList();
        }
        public async Task<IActionResult> Index()
        {
            DateTime today = DateTime.Now.Date;
            IndexModel model = new IndexModel
            {
                CountOfAutosForThisDay = await db.WashOrders.
                Where(x => x.DateOfCreateWashOrder.Date.Equals(today)).CountAsync()
            };
            var organizationType = await db.AspNetUsers.
                Where(x => x.Id == GetCurrentUserAsync().Result.Id).
                Include(x => x.Organization.TypeOrganization).
                FirstOrDefaultAsync();
            return View(model);
        }
        public IActionResult CreateService()
        {
            return View();
        }
        [HttpPost]
        public IActionResult FullDelete(int? id)
        {
            WashOrder order = db.WashOrders.FirstOrDefault(x => x.Id == id);
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
        public IActionResult CreateSalaryMaster()
        {
            ViewData["Services"] = new SelectList(db.Services.
               Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted), "Id", "Name");
            ViewData["Users"] = new SelectList(db.AspNetUsers.Where(x => x.Id == db.UserRoles.Where(x => x.RoleId == MASTERROLE).Select(x => x.UserId).FirstOrDefault()).
                 Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId), "Id", "FullName");
            return View();
        }
        [HttpPost]
        public IActionResult CreateSalaryMaster(SalaryMaster salary)
        {
            ViewData["Services"] = new SelectList(db.Services.
               Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted), "Id", "Name");
            ViewData["Users"] = new SelectList(db.AspNetUsers.Where(x => x.Id == db.UserRoles.Where(x => x.RoleId == MASTERROLE).Select(x => x.UserId).FirstOrDefault()).
                 Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId), "Id", "FullName");
            var check = db.SalaryMasters.Where(x => x.ServiceId == salary.ServiceId && x.AspNetUserId == salary.AspNetUserId).FirstOrDefault();
            if (ModelState.IsValid)
            {
                if (check == null)
                {
                    var result = Regex.Replace(salary.SalaryMasterStr, @"\D+", "", RegexOptions.ECMAScript);
                    int salaryMasterForParse = int.Parse(result);
                    if (salaryMasterForParse <= 100)
                    {
                        if (!salary.SalaryMasterStr.Contains("%"))
                        {
                            salary.SalaryMasterStr = salary.SalaryMasterStr + " %";
                        }
                        salary.SalaryMasterInt = salaryMasterForParse;
                    }
                    salary.SalaryMasterStr = result + " тенге";
                    salary.SalaryMasterInt = salaryMasterForParse;
                    salary.OrganizationId = GetCurrentUserAsync().Result.OrganizationId;
                    db.SalaryMasters.Add(salary);
                    db.SaveChanges();
                    return RedirectToAction("ServicesList");
                }
                ModelState.AddModelError("ServiceId", "Такая услуга уже создана на данного мастера");
                return View(salary);
            }
            ModelState.AddModelError("", "");
            return View(salary);
        }
        public async Task<IActionResult> WashComplete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WashOrder order = await db.WashOrders.Include(x=>x.AspNetUser).
                Include(x=>x.ModelCar).Include(x=>x.Cars).Include(x=>x.Status).
                Where(x => x.Id == id && !x.IsDeleted).
                Where(x=>x.Status.Name == "Не готово").
                Where(x=>x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }
            ViewBag.Orders = db.WashServices.Include(x => x.Status).
                Where(x => x.WashOrderId == id && !x.IsDeleted).
                Select(x => new ModelForWashAddOrder()
            {
                Id = x.Id,
                NameService = x.Service.Name,
                NameMaster = x.AspNetUser.FirstName + " " + x.AspNetUser.LastName,
                Price = x.WashServicePrice,
                SalaryMaster = x.MasterSalaryInt,
                Status = x.Status.Name
            });

            ViewData["PaymentId"] = new SelectList(db.Payments.Where(x => !x.IsDeleted), "Id", "Name");
            ViewData["HaveNotReadyServices"] = null;

            var washServices = await db.WashServices.Include(x => x.Status).
                Where(x => x.WashOrderId == id).ToListAsync();

            var checkDoneServices = washServices.
                Where(x=>x.WashOrderId == id).FirstOrDefault(x => x.Status.Name == "Не готово");
            ViewData["PaymentSumm"] = washServices
                .Where(x=>!x.IsDeleted).Sum(x=>x.WashServicePrice); // проверка все ли услуги в заказ-наряде завершены
            // Если не все услуги в заказ-наряде завершены выводим в представлении сообщение об этом

            if (checkDoneServices == null)
            {
                ViewData["HaveNotReadyServices"] = "Some message";
            }

            ViewData["Services"] = new SelectList(db.Services.
               Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted), "Id", "Name");
            ViewData["Users"] = new SelectList(db.AspNetUsers.Where(x => x.Id == db.UserRoles.Where(x => x.RoleId == MASTERROLE).Select(x => x.UserId).FirstOrDefault()).
                Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId), "Id", "FullName");
            return View(order);
        }
        [HttpPost]
        public async Task<IActionResult> WashComplete(int? id, SubmitWash submit, WashOrder order)
        {
            if (id == null)
            {
                return NotFound();
            }
            var services = db.WashServices.Where(x=>x.WashOrderId == id && !x.IsDeleted);
            ViewBag.Orders = services.Select(x => new ModelForWashAddOrder()
            {
                Id = x.Id,
                NameService = x.Service.Name,
                NameMaster = x.AspNetUser.FirstName + " " + x.AspNetUser.LastName,
                Price = x.WashServicePrice,
                SalaryMaster = x.MasterSalaryInt
            });
            ViewData["PaymentId"] = new SelectList(db.Payments.Where(x => !x.IsDeleted), "Id", "Name");
            ViewData["HaveNotReadyServices"] = null;

            order = await db.WashOrders.Include(x=>x.Payment).
                Include(x=>x.Status).Where(x=>x.Status.Name == "Не готово").
                Where(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }
            int countOfServices = await db.WashServices.
                Where(x=>x.WashOrderId == id && !x.IsDeleted).CountAsync();
            if (countOfServices<1)
            {
                ModelState.AddModelError("CarNumber", "На заказ-наряд не назначена услуга");
                return View(order);
            }
            var washServices = await services.Include(x=>x.Status).
                Where(x => x.WashOrderId == id && !x.IsDeleted).ToListAsync();
            var checkDoneServices = washServices.FirstOrDefault(x=>x.Status.Name == "Не готово");
            if (checkDoneServices == null)
            {
                ViewData["HaveNotReadyServices"] = "Some message";
            }
            foreach (var item in washServices)
            {
                item.StatusId = 2;
            }

            order.StatusId = 2;
            order.PaymentId = order.PaymentId;
            db.Entry(order).State = EntityState.Modified;
            submit.WashOrderId = order.Id;
            submit.PaymentId = order.PaymentId;
            submit.AspNetUserId = GetCurrentUserAsync().Result.Id;
            submit.SummServices = await services.Select(x => x.WashServicePrice).SumAsync();
            foreach (var item in washServices.Where(x=>x.Status.Name == "Не готово" && !x.IsDeleted))
            {
                SalaryMasterList salaryMaster = new SalaryMasterList();
                salaryMaster.WashServiceId = item.Id;
                salaryMaster.WashOrderId = item.WashOrderId;
                salaryMaster.AspNetUserId = item.AspNetUserId;
                salaryMaster.Salary = item.MasterSalaryInt;
                salaryMaster.OrganizationId = item.OrganizationId;
                await db.SalaryMasterLists.AddAsync(salaryMaster);
            }
            await db.SubmitWashes.AddAsync(submit);

            await db.SaveChangesAsync();
            return RedirectToAction("Orders");
        }
        public async Task<IActionResult> ListOfCompleteWashes(string? search, string searchBy, string? carNumber)
        {
            var list = db.SubmitWashes.Include(x=>x.WashOrder.Cars.ModelCars).
                Include(x=>x.WashOrder.WashServices).
                Include(x=>x.AspNetUser).
                Where(x=>!x.WashOrder.IsDeleted && x.WashOrder.OrganizationId == GetCurrentUserAsync().Result.OrganizationId);
            if(!string.IsNullOrEmpty(search) && searchBy == "Гос номер авто")
            {
                list = list.Where(x => x.WashOrder.CarNumber.Contains(search));
            }
            return View(await list.ToListAsync());
        }
        public async Task<IActionResult> DetailsOfCompleteAuto(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var auto = await db.SubmitWashes.
                Include(x=>x.AspNetUser).
                Include(x=>x.WashOrder.AspNetUser).
                Include(x=>x.WashOrder.ModelCar).
                Include(x=>x.WashOrder.Cars).
                Where(x=>x.Id == id && x.WashOrder.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).FirstOrDefaultAsync();
            if (auto == null)
            {
                return BadRequest();
            }
            ViewBag.Orders = db.WashServices.Where(x => x.WashOrderId == auto.WashOrderId && !x.IsDeleted).Select(x => new ModelForWashAddOrder()
            {
                Id = x.Id,
                NameService = x.Service.Name,
                NameMaster = x.AspNetUser.FirstName + " " + x.AspNetUser.LastName,
                Price = x.WashServicePrice,
                SalaryMaster = x.MasterSalaryInt,
                Profit = x.WashServicePrice - x.MasterSalaryInt
            });
            return View(auto);
        }
        [HttpPost]
        public async Task<IActionResult> ReturnOder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            WashOrder order = await db.WashOrders.FirstOrDefaultAsync(x => x.Id == id);
            if (order != null)
            {
                var servicesOrder = await db.WashServices.
                    Where(x => x.WashOrderId == id).ToListAsync();

                foreach (var item in servicesOrder)
                {
                    item.IsDeleted = false;
                }
                var salarys = await db.SalaryMasterLists.
                    Where(x => x.WashOrderId == id).ToListAsync();
                foreach (var item in salarys)
                {
                    item.IsDeleted = false;
                }

                order.IsDeleted = false;
                db.Entry(order).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("DeletedOrders");
            }
            ModelState.AddModelError("", "");
            return View(order);
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
        public async Task<IActionResult> ServicesList(string? search)
        {
            var listServices = db.Services.Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId);
            if (!string.IsNullOrEmpty(search))
            {
                listServices = listServices.Where(x => x.Name.Contains(search));
            }
            return View(await listServices.ToListAsync());
        }
        public async Task<IActionResult> EditServiceWash(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var usersInMasterRole = await _userManager.GetUsersInRoleAsync("Мастер");

            var users = usersInMasterRole
                .Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId)
                .Select(x => new SelectListItem { Value = x.Id, Text = x.FullName })
                .ToList();

            ViewData["Services"] = new SelectList(db.Services.
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted), "Id", "Name");
            ViewData["Users"] = new SelectList(users, "Value", "Text");

            WashService? service = db.WashServices.
                Where(x=>!x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).Where(x=>x.Status.Name == "Не готово").
                Include(x=>x.Status).FirstOrDefault(x=>x.Id == id);

            ViewData["ServiceIsReady"] = null;
            if (service.Status.Name == "Готово")
            {
                ViewData["ServiceIsReady"] = "IsNotNull";
                return View(service);
            }
            return View(service);
        }
        [HttpPost]
        public IActionResult EditServiceWash(WashService washService)
        {
            ViewData["Services"] = new SelectList(db.Services.
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted), "Id", "Name");
            ViewData["Users"] = new SelectList(db.AspNetUsers.
                Where(x => x.Id == db.UserRoles.Where(x => x.RoleId == MASTERROLE). // 23bd70e1-2a61-4ba7-9d07-fefda7888cf3
                Select(x => x.UserId).FirstOrDefault()).
                Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId), "Id", "FullName");
            if (ModelState.IsValid)
            {
                db.Entry(washService).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Orders");
            }
            ModelState.AddModelError("", "");
            return View(washService);
        }
        public IActionResult SaveData()
        {
            return View();
        }
        public async Task<IActionResult> EditOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            WashOrder order = await db.WashOrders.
                Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).
                FirstOrDefaultAsync(x => x.Id == id);
            if (order == null)
            {
                return BadRequest();
            }
            ViewData["CarId"] = new SelectList(db.Cars.Where(x => !x.IsDeleted), "Id", "Name");
            ViewData["ModelCarId"] = new SelectList(db.ModelCars.Where(x => !x.IsDeleted && x.CarId == order.CarId), "Id", "Name");
            return View(order);
        }
        [HttpPost]
        public IActionResult EditOrder(WashOrder order)
        {
            ViewData["CarId"] = new SelectList(db.Cars.Where(x => !x.IsDeleted), "Id", "Name");
            ViewData["ModelCarId"] = new SelectList(db.ModelCars.Where(x => !x.IsDeleted && x.CarId == order.CarId), "Id", "Name");
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Orders");
            }
            ModelState.AddModelError("", "");
            return View(order);
        }
        public async Task<IActionResult> DetailsOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            WashOrder? order = await db.WashOrders.Include(x=>x.ModelCar.Car).
                Where(x=>x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted).FirstOrDefaultAsync();
            if (order == null)
            {
                return BadRequest();
            }
            ViewBag.Orders = db.WashServices.Where(x => x.WashOrderId == order.Id && !x.IsDeleted).Select(x => new ModelForWashAddOrder()
            {
                Id = x.Id,
                NameService = x.Service.Name,
                NameMaster = x.AspNetUser.FirstName + " " + x.AspNetUser.LastName,
                Price = x.WashServicePrice,
                SalaryMaster = x.MasterSalaryInt
            });
            return View(order);
        }
        public async Task<IActionResult> CreateOrder()
        {
            ViewData["CarId"] = new SelectList(db.Cars.Where(x => !x.IsDeleted), "Id", "Name");
            return View();
        }
        [HttpPost]
        public IActionResult CreateOrder(WashOrder order)
        {
            ViewData["CarId"] = new SelectList(db.Cars.Where(x => !x.IsDeleted), "Id", "Name");
            ViewData["ModelCarId"] = new SelectList(db.ModelCars.Where(x => !x.IsDeleted && x.CarId == order.CarId), "Id", "Name");
            if (ModelState.IsValid)
            {
                var checkAutoInOrder = db.WashOrders.Where(x=>x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).
                Where(x => x.CarNumber == order.CarNumber && x.Status.Name == "Не готово").Where(x=>!x.IsDeleted).FirstOrDefault();
                if (checkAutoInOrder == null)
                {
                    order.StatusId = 1;
                    order.IsDeleted = false;
                    order.AspNetUserId = GetCurrentUserAsync().Result.Id;
                    order.OrganizationId = GetCurrentUserAsync().Result.OrganizationId;
                    db.WashOrders.Add(order);
                    db.SaveChanges();
                    return RedirectToAction("AddServiceWash", new { id = order.Id });
                }
                ModelState.AddModelError("CarNumber", "Авто с таким гос номером уже в работе");
                return View(order);
            }
            ModelState.AddModelError("", "");
            return View(order);
        }
        public async Task<IActionResult> DeletedOrders()
        {
            var orders = await db.WashOrders.Include(x => x.Cars.ModelCars)
                .Include(x => x.AspNetUser).Where(x => x.IsDeleted && !x.FullDelete && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).ToListAsync();
            return View(orders);
        }
        public async Task<IActionResult> Orders(string? search, string? searchBy)
        {
            var list = db.WashOrders.Include(x => x.Status).OrderBy(x=>x.DateOfCreateWashOrder).
                Where(x => x.Status.Name == "Не готово").Include(x => x.Cars.ModelCars).
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted);
            if (!string.IsNullOrEmpty(search) && searchBy == "Гос номер авто")
            {
                list = list.Where(x => x.CarNumber.Contains(search));
            }
            if (!string.IsNullOrEmpty(search) && searchBy == "Марка")
            {
                list = list.Where(s => s.ModelCar.Name.Contains(search) || s.Cars.Name.Contains(search));
            }
            return View(await list.ToListAsync());
        }
        public async Task<IActionResult> SubmitedOrders(string? search, string? searchBy)
        {
            var list = db.WashOrders.Include(x => x.Status).
                Where(x => x.Status.Name == "Не готово").Include(x => x.Cars.ModelCars).
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted);
            if (!string.IsNullOrEmpty(search) && searchBy == "Гос номер авто")
            {
                list = list.Where(x => x.CarNumber.Contains(search));
            }
            if (!string.IsNullOrEmpty(search) && searchBy == "Марка")
            {
                list = list.Where(s => s.ModelCar.Name.Contains(search) || s.Cars.Name.Contains(search));
            }
            return View(await list.ToListAsync());
        }
        public async Task<IActionResult> MasterSalaryList()
        {
            var salaryList = await db.SalaryMasterLists.
                Include(x=>x.WashOrder.Cars).
                Include(x=>x.WashService.Service).
                Include(x=>x.WashOrder.ModelCar).
                Include(x=>x.AspNetUser).
                Where(x=>x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).ToListAsync();
            return View(salaryList);
        }
        public async Task<IActionResult> MasterSalaryDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            SalaryMasterList? salary = await db.SalaryMasterLists.
                Include(x => x.WashOrder.Cars).
                Include(x => x.WashService.Service).
                Include(x => x.WashOrder.ModelCar).
                Include(x => x.AspNetUser).FirstOrDefaultAsync(x=>x.Id == id);
            if (salary == null)
            {
                return NotFound();
            }
            return View(salary);
        }
        private string ConvertSalaryMasterStr(string? salaryMasterStr)
        {
            if (salaryMasterStr == null)
            {
                // Do something
            }
            string? result = Regex.Replace(salaryMasterStr, @"\D+", "", RegexOptions.ECMAScript);
            return result;
        }
        public JsonResult LoadPrice(int? id)
        {
            var prices = db.Services.Where(x => x.Id == id).ToList();
            return Json(new SelectList(prices, "Id", "Name"));
        }
        [HttpPost]
        public async Task<IActionResult> CreateSalaryMasterFromAddServiceWash(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            WashService? serviceForAddSalary = await db.WashServices.
                Where(x => x.WashOrderId == id).FirstOrDefaultAsync();

            if (serviceForAddSalary == null)
            {
                return BadRequest();
            }
            SalaryMaster salary = new SalaryMaster();
            salary.SalaryMasterStr = serviceForAddSalary.SalaryMasterStr;
            salary.SalaryMasterInt = serviceForAddSalary.MasterSalaryInt;
            salary.AspNetUserId = serviceForAddSalary.AspNetUserId;
            salary.IsDeleted = false;
            salary.OrganizationId = serviceForAddSalary.OrganizationId;
            salary.ServiceId = serviceForAddSalary.ServiceId;

            await db.SalaryMasters.AddAsync(salary);
            await db.SaveChangesAsync();
            return RedirectToAction("qwe", new { id });
        }
        public async Task<IActionResult> AddServiceWash(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            /*WashService service = await db.WashServices.
               Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted).
               FirstOrDefaultAsync();*/
            WashOrder? order = await db.WashOrders.Where(x=>x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).
                FirstOrDefaultAsync(x=>x.Id == id);
            if (order == null)
            {
                return BadRequest();
            }
            WashService service = new WashService();
            ViewBag.Orders = db.WashServices.Where(x => x.WashOrderId == id && !x.IsDeleted).Select(x => new ModelForWashAddOrder()
            {
                Id = x.Id,
                NameService = x.Service.Name,
                NameMaster = x.AspNetUser.FirstName + " " + x.AspNetUser.LastName,
                Price = x.WashServicePrice,
                SalaryMaster = x.MasterSalaryInt
            });
            var usersInMasterRole = await _userManager.GetUsersInRoleAsync("Мастер");

            var users = usersInMasterRole
                .Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId)
                .Select(x => new SelectListItem { Value = x.Id, Text = x.FullName })
                .ToList();
            ViewData["Services"] = new SelectList(db.Services.
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted), "Id", "Name");
            ViewData["Users"] = new SelectList(users, "Value", "Text");

            return View(service);
        }
        [HttpPost]
        public async Task<IActionResult> AddServiceWash(WashService? service, int? id)
        {
            ViewBag.Orders = db.WashServices.Where(x => x.WashOrderId == id && !x.IsDeleted).Select(x => new ModelForWashAddOrder()
            {
                Id = x.Id,
                NameService = x.Service.Name,
                NameMaster = x.AspNetUser.FirstName + " " + x.AspNetUser.LastName,
                Price = x.WashServicePrice,
                SalaryMaster = x.MasterSalaryInt
            });
            var usersInMasterRole = await _userManager.GetUsersInRoleAsync("Мастер");

            var users = usersInMasterRole
                .Where(x => !x.IsDeleted && x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId)
                .Select(x => new SelectListItem { Value = x.Id, Text = x.FullName })
                .ToList();
            ViewData["Services"] = new SelectList(db.Services.
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted), "Id", "Name");
            ViewData["Users"] = new SelectList(users, "Value", "Text");
            var checkServiceInWashOrder = await db.WashServices.Where(x => x.WashOrderId == id && !x.IsDeleted).
                FirstOrDefaultAsync(x => x.ServiceId == service.ServiceId);
            if (checkServiceInWashOrder == null)
            {
                service.StatusId = 1;
                service.WashServicePrice = db.Services.Where(x => x.Id == service.ServiceId).Select(x => x.Price).FirstOrDefault();
                service.WashOrderId = id;
                service.OrganizationId = GetCurrentUserAsync().Result.OrganizationId;
                service.IsDeleted = false;
                ViewData["SalaryIsNull"] = null;


                if (ModelState.IsValid)
                {
                    int? salary = db.SalaryMasters.
                        Where(x => x.AspNetUserId == service.AspNetUserId && x.ServiceId == service.ServiceId).
                        Select(x => x.SalaryMasterInt).
                        FirstOrDefault();

                    if (salary == null)
                    {
                        ViewData["SalaryIsNull"] = "True";
                        if (service.SalaryMasterStr == null)
                        {
                            ModelState.AddModelError("SalaryMasterStr", "Введите зарплату для мастера");
                            return View(service);
                        }
                        else
                        {
                            string? result = ConvertSalaryMasterStr(service.SalaryMasterStr);
                            salary = int.Parse(result);
                            if (salary <= 100)
                            {
                                if (!service.SalaryMasterStr.Contains("%"))
                                {
                                    service.SalaryMasterStr = service.SalaryMasterStr + " %";
                                }
                                service.MasterSalaryInt = (service.WashServicePrice * salary) / 100;
                            }
                            else
                            {
                                service.SalaryMasterStr = result + " тенге";
                                service.MasterSalaryInt = salary;
                                if (service.MasterSalaryInt >= await db.Services.
                                    Where(x => x.Id == service.ServiceId).Select(x => x.Price).
                                    FirstOrDefaultAsync())
                                {
                                    ModelState.AddModelError("SalaryMasterStr", "Зарплата мастера не может быть больше цены на услугу!");
                                    return View(service);
                                }
                                return View(service);
                            }
                        }
                        await db.WashServices.AddAsync(service);
                        await db.SaveChangesAsync();
                        return RedirectToAction("AddServiceWash", new { id });
                    }


                    else
                    {
                        if (salary <= 100)
                        {
                            service.MasterSalaryInt = (service.WashServicePrice * salary) / 100;
                        }
                        else
                        {
                            service.MasterSalaryInt = salary;
                        }
                        await db.WashServices.AddAsync(service);
                        await db.SaveChangesAsync();
                        return RedirectToAction("AddServiceWash", new { id });
                    }
                }
                ModelState.AddModelError("", "");
                return View(service);
            }
            ModelState.AddModelError("ServiceId", "Эта услуга уже назначена на заказ-наряд");
            return View(service);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteServiceWash(int? id)
        {
            WashService service = await db.WashServices.Where(x=>!x.IsDeleted).
                FirstOrDefaultAsync(x => x.Id == id);
            if (service != null || id == null)
            {
                var salaryMaster = await db.SalaryMasterLists.
                    Where(x=>x.WashServiceId == id).FirstOrDefaultAsync();
                if (salaryMaster != null)
                {
                    salaryMaster.IsDeleted = true;
                }
                service.IsDeleted = true;
                db.Entry(service).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("AddServiceWash", new { id = service.WashOrderId });
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteService(int? id)
        {
            Service service = await 
                db.Services.FirstOrDefaultAsync(x => x.Id == id);
            if (service != null)
            {
                var salarySettings = await db.
                    SalaryMasters.
                    Where(x=>x.ServiceId == id).
                    ToListAsync();
                service.IsDeleted = true;
                foreach (var item in salarySettings)
                {
                    item.IsDeleted = true;
                }
                db.Entry(service).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("ServicesList");
            }
            ModelState.AddModelError("", "");
            return View(service);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int? id)
        {
            WashOrder order = await db.WashOrders.Where(x=>x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId).
                FirstOrDefaultAsync(x => x.Id == id);
            if (order != null)
            {
                var servicesOrder = await db.WashServices.
                    Where(x => x.WashOrderId == id).ToListAsync();

                foreach (var item in servicesOrder)
                {
                    item.IsDeleted = true;
                }
                var salarys = await db.SalaryMasterLists.
                    Where(x => x.WashOrderId == id).ToListAsync();
                foreach (var item in salarys)
                {
                    item.IsDeleted = true;
                }

                order.IsDeleted = true;
                db.Entry(order).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Orders");
            }
            ModelState.AddModelError("", "");
            return View(order);
        }
    }
}
