using MainWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MainWebApplication.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MainWebApplication.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;
using MainWebApplication.Methods;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MainWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private static string MANAGER = "Менеджер";
        private static string DIRECTOR = "Руководитель";
        private static string MASTER = "Мастер";
        private static string MASTERROLE = "c6122f47-74d1-4dfd-a11e-bc372bad0cf4";
        private readonly ILogger<HomeController> _logger;
        ApplicationDbContext db;
        private UserManager<AspNetUser> _userManager;
        private readonly IUserStore<AspNetUser> _userStore;
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "404, Requested resource was not found";
                    break;
            }

            return View("PageNotFound");
        }

        public async Task<IActionResult> ListUsers()
        {
            var currentUser = await GetCurrentUserAsync();
            var users = await _userManager.Users
                .Where(x => x.OrganizationId == currentUser.OrganizationId)
                .Select(c => new UsersViewModel()
                {
                    UserId = c.Id,
                    FullName = c.FirstName + " " + c.LastName,
                    PhoneNumber = c.NormalizedUserName
                })
                .ToListAsync();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(currentUser);
                user.Role = string.Join(",", roles);
            }

            return View(users);
        }


        public IActionResult GetServicePrice(int? id)
        {
            var service = db.Services.FirstOrDefault(x => x.Id == id);
            if (service == null)
            {
                return NotFound();
            }
            var price = service.Price;
            return Json(new { price });
        }
        /*public IActionResult GetSalaryMaster(int? id, int? AspNetUserId)
        {
            var salary = db.SalaryMasters.Where(x=>x.ServiceId == id && ).FirstOrDefault(x => x.Id == id);
        }*/
        public async Task<IActionResult> EditRole(string? userId)
        {
            var roles = await db.Roles.ToListAsync();
            var excludedRoles = new List<string> { "Администратор" }; // Список ролей, которые нужно исключить

            var filteredRoles = roles.Where(r => !excludedRoles.Contains(r.Name));

            ViewData["Roles"] = new SelectList(filteredRoles, "Name", "Name");
            AspNetUser? user = await db.AspNetUsers.
                Where(x => x.Id == userId).FirstOrDefaultAsync();
            return View();
        }
        private UserManager<AspNetUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        [HttpPost]
        public async Task<IActionResult> EditRole(string? id, RoleModification role)
        {
            var user = await db.AspNetUsers.
                Where(x => x.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return BadRequest();
            }
            await _userManager.AddToRoleAsync(user, role.RoleName);
            return RedirectToAction("ListUsers");
        }
        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
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
        public async Task<IActionResult> CreateSalaryMasterAsync(SalaryMaster salary)
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
                    else
                    {
                        salary.SalaryMasterStr = result + " тенге";
                        salary.SalaryMasterInt = salaryMasterForParse;
                        if (salary.SalaryMasterInt >= await db.Services.Where(x => x.Id == salary.ServiceId).Select(x => x.Price).FirstOrDefaultAsync())
                        {
                            ModelState.AddModelError("SalaryMasterStr", "Зарплата мастера не может быть больше цены на услугу!");
                            return View(salary);
                        }
                    }
                    salary.OrganizationId = GetCurrentUserAsync().Result.OrganizationId;
                    db.SalaryMasters.Add(salary);
                    db.SaveChanges();
                    return RedirectToAction("SalaryList");
                }
                ModelState.AddModelError("ServiceId", "Такая услуга уже создана на данного мастера");
                return View(salary);
            }
            ModelState.AddModelError("", "");
            return View(salary);
        }
        public async Task<IActionResult> SalaryList()
        {
            var list = await db.SalaryMasters.Include(x => x.AspNetUser).
                Include(x => x.Service).
                Where(x => x.OrganizationId == GetCurrentUserAsync().Result.OrganizationId && !x.IsDeleted).
                ToListAsync();
            return View(list);
        }
        [Authorize(Roles = "Руководитель")]
        public async Task<IActionResult> MyOrganization()
        {
            var organization = await db.Organizations.Include(x => x.TypeOrganization).
                Where(x => x.Id == GetCurrentUserAsync().Result.OrganizationId).FirstOrDefaultAsync();
            return View(organization);
        }
        public async Task<IActionResult> Profile()
        {
            var currentUser = await GetCurrentUserAsync();
            var user = await _userManager.Users
                .Where(x => x.Id == currentUser.Id)
                .Select(c => new ProfileViewModel()
                {
                    ProfileId = c.Id,
                    FullName = c.FirstName + " " + c.LastName,
                    Email = c.Email,
                    PhoneNumber = c.UserName,
                    DateOfRegister = c.DateOfCreated,
                    OrganizationName = c.Organization.FullOrganizationName
                })
                .FirstOrDefaultAsync();

            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(currentUser);
                user.Roles = string.Join(", ", roles);
            }

            return View(user);
        }
        public async Task<IActionResult> EditProfile(string? id)
        {
            AspNetUser? user = await db.AspNetUsers.FindAsync(id);
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> EditProfile(AspNetUser user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Profile");
            }
            return View(user);
        }
        public async Task<IActionResult> ChangeOrganization(int? id)
        {
            Organization? organization = await db.Organizations.FindAsync(id);
            return View(organization);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeOrganization(AspNetUser user)
        {
            if (ModelState.IsValid)
            {
                var checkOrganization = await db.Organizations.
                    FirstOrDefaultAsync(x => x.OrganizationNumber == user.OrganizationNumber);
                if (checkOrganization != null)
                {
                    user.OrganizationId = checkOrganization.Id;
                    db.Entry(user).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Profile");
                }
                ModelState.AddModelError("OrganizationNumber", "Организация с таким ИИН/БИН не существует");
                return View(user);
            }
            ModelState.AddModelError("", "");
            return View(user);
        }
        public IActionResult PageNotFound()
        {
            return View();
        }
        public HomeController(IUserStore<AspNetUser> userStore, ApplicationDbContext context, UserManager<AspNetUser> userManager, RoleManager<IdentityRole> roleMgr)
        {
            roleManager = roleMgr;
            db = context;
            _userManager = userManager;
            _userStore = userStore;
        }
        private async Task<AspNetUser> GetCurrentUserAsync() => await _userManager.GetUserAsync(HttpContext.User);
        [AllowAnonymous]
        [HttpGet]
        public IActionResult RegisterOrganization()
        {
            ViewData["TypeOfOrganization"] = new SelectList(db.TypeOrganizations, "Id", "Name");
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RegisterOrganization(Organization organization)
        {
            var checkOrganization = await db.
                Organizations.
                FirstOrDefaultAsync(x => x.OrganizationNumber == organization.OrganizationNumber);
            ViewData["TypeOfOrganization"] = new SelectList(db.TypeOrganizations, "Id", "Name");
            if (checkOrganization == null)
            {
                organization.CreatedDate = DateTime.Now;
                organization.StartDateSub = organization.CreatedDate;
                organization.EndDateSub = organization.StartDateSub.AddDays(14);
                await db.Organizations.AddAsync(organization);
                await db.SaveChangesAsync();

                var masterUser = CreateUser();
                await _userManager.AddToRoleAsync(masterUser, "Мастер");
                masterUser.OrganizationId = organization.Id;
                masterUser.EmailConfirmed = false;
                masterUser.FirstName = "Неопределенный";
                masterUser.LastName = "Мастер";

                var phoneNumber = GenerateRandomString(12);
                var userId = await _userManager.GetUserIdAsync(masterUser);

                await _userStore.SetUserNameAsync(masterUser, phoneNumber, CancellationToken.None);
                var result = await _userManager.CreateAsync(masterUser, GenerateRandomString(20));
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("OrganizationNumber", "Организация с таким ИИН/БИН уже существует");
            return View(organization);
        }
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var guid = Guid.NewGuid().ToString("N");
            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                if (i < 32)
                {
                    result[i] = guid[i];
                }
                else
                {
                    result[i] = chars[random.Next(chars.Length)];
                }
            }

            return new string(result);
        }
        private AspNetUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AspNetUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AspNetUser)}'. " +
                    $"Ensure that '{nameof(AspNetUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var currentUser = await db.AspNetUsers.
                Include(x => x.Organization.TypeOrganization).
                Where(x => x.Id == GetCurrentUserAsync().Result.Id).FirstOrDefaultAsync();

                if (User.IsInRole("Администратор"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                if (currentUser.Organization.TypeOrganization.Name == "Детейлинг центр")
                {
                    if (User.IsInRole(DIRECTOR))
                    {
                        return RedirectToAction("Index", "Director", new { area = "Detailing" });
                    }
                    else if (User.IsInRole(MANAGER))
                    {
                        return RedirectToAction("Index", "Manager", new { area = "Detailing" });
                    }
                    else if (User.IsInRole(MASTER))
                    {
                        return RedirectToAction("Master", "Index", new { area = "Detailing" });
                    }
                }

                else if (currentUser.Organization.TypeOrganization.Name == "Автомойка")
                {
                    if (User.IsInRole("Руководитель"))
                    {
                        return RedirectToAction("Index", "Director", new { area = "Wash" });
                    }
                    else if (User.IsInRole("Менеджер"))
                    {

                    }
                    else if (User.IsInRole("Мастер"))
                    {
                        return RedirectToAction("Tasks", "Master", new { area = "Wash" });
                    }
                }
                return View(currentUser);
            }
            return View();
        }
        [Authorize(Roles = "Менеджер")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}