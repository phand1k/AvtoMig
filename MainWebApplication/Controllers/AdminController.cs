using Microsoft.AspNetCore.Mvc;
using MainWebApplication.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using MainWebApplication.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MainWebApplication.Controllers
{
    [Authorize(Roles = "Администратор")]
    public class AdminController : Controller
    {
        ApplicationDbContext db;

        public AdminController(ApplicationDbContext context)
        {
            db = context;
        }
        public IActionResult SmsActivates()
        {
            var list = db.SmsActivates.Where(x=>!x.IsUsed).ToList();
            return View(list);
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Organizations()
        {
            var list = await db.Organizations.ToListAsync();
            return View(list);
        }
        public async Task<IActionResult> EditOrganization(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Organization organization = await db.Organizations.FirstOrDefaultAsync(x=>x.Id == id);
            if (organization == null)
            {
                return BadRequest();
            }
            return View(organization);
        }
        [HttpPost]
        public async Task<IActionResult> EditOrganization(Organization organization)
        {
            if (ModelState.IsValid)
            {
                db.Entry(organization).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Organizations");
            }
            ModelState.AddModelError(string.Empty, "");
            return View(organization);
        }
        public IActionResult AddStatus()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddStatus(Status status)
        {
            var checkStatus = db.Statuses.FirstOrDefault(x=>x.Name == status.Name);
            if (ModelState.IsValid)
            {
                if (checkStatus == null)
                {
                    await db.Statuses.AddAsync(status);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("Name", "Такой статус уже существует");
                return View(status);
            }
            ModelState.AddModelError("", "");
            return View(status);
        }
        public IActionResult CreateCar()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateCar(Car car)
        {
            await db.Cars.AddAsync(car);
            await db.SaveChangesAsync();
            return RedirectToAction("ListCars");
        }
        public IActionResult CreateModelCar()
        {
            ViewData["CarId"] = new SelectList(db.Cars, "Id", "Name");
            return View();
        }
        public async Task<IActionResult> Users()
        {
            var allUsers = await db.AspNetUsers.ToListAsync();
            return View(allUsers);
        }
        public async Task<IActionResult> ListCars()
        {
            var listCars = await db.Cars.Where(x=>!x.IsDeleted).ToListAsync();
            return View(listCars);
        }
        [HttpPost]
        public async Task<IActionResult> CreateModelCar(ModelCar modelCar)
        {
            ViewData["CarId"] = new SelectList(db.Cars, "Id", "Name");
            ViewData["Success"] = null;
            var isHave = await db.ModelCars.FirstOrDefaultAsync(x=>x.CarId == modelCar.CarId && x.Name == modelCar.Name);
            if (isHave == null)
            {
                ViewData["Success"] = "Модель успешно добавлена";
                await db.ModelCars.AddAsync(modelCar);
                await db.SaveChangesAsync();
                return View(modelCar);
            }
            ModelState.AddModelError("Name", "Данная модель уже существует");
            return View(modelCar);
        }
        public async Task<IActionResult> EditModelCar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ModelCar modelCar = await db.ModelCars.FirstOrDefaultAsync(x=>x.Id == id);
            ViewData["CarId"] = new SelectList(db.Cars, "Id", "Name");
            if (modelCar == null)
            {
                return NotFound();
            }
            return View(modelCar);
        }
        public async Task<IActionResult> EditModelCar(ModelCar modelCar)
        {
            ViewData["CarId"] = new SelectList(db.Cars, "Id", "Name");
            if (ModelState.IsValid)
            {
                db.Entry(modelCar).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Orders");
            }
            ModelState.AddModelError("", "");
            return View(modelCar);
        }
        public async Task<IActionResult> ListModelCars()
        {
            var listModels = await db.ModelCars.Include(x=>x.Car).
                Where(x=>!x.IsDeleted).ToListAsync();
            return View(listModels);
        }
        public async Task<IActionResult> PaymentTypes()
        {
            var listPrepaymentTypes = db.Payments;
            return View(await listPrepaymentTypes.ToListAsync());
        }
        public IActionResult AddPayment()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddPayment(Payment payment)
        {
            var check = await db.Payments.FirstOrDefaultAsync(x=>x.Name == payment.Name);
            if (check == null)
            {
                if (ModelState.IsValid)
                {
                    await db.Payments.AddAsync(payment);
                    await db.SaveChangesAsync();
                    return RedirectToAction("PaymentTypes");
                }
                return View(payment);
            }
            ModelState.AddModelError("Name", "Такой способ оплаты уже существует");
            return View(payment);
        }
    }
}