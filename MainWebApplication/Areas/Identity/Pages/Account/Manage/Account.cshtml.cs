using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MainWebApplication.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MainWebApplication.Areas.Identity.Pages.Account.Manage
{
    public class AccountModel : PageModel
    {
        ApplicationDbContext db;
        private UserManager<AspNetUser> _userManager;
        public AccountModel(ApplicationDbContext context, UserManager<AspNetUser> userManager)
        {
            db = context;
            _userManager = userManager;
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel : AspNetUser
        {
            public string StatusMessage { get; set; }
        }
        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }
        private Task<AspNetUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
        public async Task<IActionResult> OnPostAsync(AspNetUser user)
        {
                db.Entry(user).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Account");
        }
        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }

    }
}
