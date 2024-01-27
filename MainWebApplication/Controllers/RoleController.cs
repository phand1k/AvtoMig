using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using MainWebApplication.Models;
using MainWebApplication.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MainWebApplication.Controllers
{
    public class RoleController : Controller
    {
        private UserManager<AspNetUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        public async Task<IActionResult> SendMessage(string message)
        {
            using (var client = new HttpClient())
            {
                var botToken = "5806886891:AAHWpSIY84Eu7FmfgbO1hs0vb9Mf0_dhXgk";
                var botUrl = $"https://api.telegram.org/bot{botToken}/sendMessage?chat_id=your-chat-id&text={message}";
                var response = await client.GetAsync(botUrl);
                response.EnsureSuccessStatusCode();
            }
            return Ok();
        }

        public ViewResult Index() => View(roleManager.Roles);
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
 
        public RoleController(RoleManager<IdentityRole> roleMgr, UserManager<AspNetUser> userMrg)
        {
            roleManager = roleMgr;
            userManager = userMrg;
        }

        // other methods

        public async Task<IActionResult> Update(string id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            List<AspNetUser> members = new List<AspNetUser>();
            List<AspNetUser> nonMembers = new List<AspNetUser>();

            var users = await userManager.Users.ToListAsync();

            foreach (AspNetUser user in users)
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
        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
    }
}
