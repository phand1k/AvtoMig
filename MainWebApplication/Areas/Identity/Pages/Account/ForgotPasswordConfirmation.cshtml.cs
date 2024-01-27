// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using MainWebApplication.Areas.Identity.Data;
using MainWebApplication.Methods;
using MainWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Xml.Linq;

namespace MainWebApplication.Areas.Identity.Pages.Account
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [AllowAnonymous]
    public class ForgotPasswordConfirmation : PageModel
    {
        ApplicationDbContext db;
        private readonly UserManager<AspNetUser> _userManager;
        public ForgotPasswordConfirmation(ApplicationDbContext context, UserManager<AspNetUser> userManager)
        {
            db = context;
            _userManager = userManager; 
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [Display(Name = "Код подтверждения")]
            public string Code { get; set; }
        }
        public string PhoneNumber { get; set; }
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public async Task<IActionResult> OnGetAsync(string phoneNumber, string returnUrl = null)
        {
            if (phoneNumber == null)
            {
                return RedirectToPage("/Index");
            }
            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _userManager.FindByNameAsync(phoneNumber);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{phoneNumber}'.");
            }
            PhoneNumber = phoneNumber;
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string phoneNumber)
        {
            var Sms = await db.SmsActivates.
                Where(x => x.PhoneNumber == phoneNumber && !x.IsUsed).OrderByDescending(x => x.DateOfEnd).
                LastOrDefaultAsync();
            PhoneNumber = phoneNumber;
            var checkDateOfEndSMS = await db.SmsActivates.Where(x=>!x.IsUsed).OrderBy(x=>x.DateOfEnd).Select(x=>x.DateOfEnd).LastAsync();
            if (Input.Code == Sms.SMSCode)
            {
                AspNetUser user = await db.AspNetUsers.
                    Where(x => x.UserName == phoneNumber).FirstOrDefaultAsync();
                user.EmailConfirmed = true;

                db.Entry(user).State = EntityState.Modified;
                await db.SaveChangesAsync();

                SmsActivate activate = await db.SmsActivates.
                    Where(x => x.PhoneNumber == phoneNumber).OrderBy(x => x.DateOfEnd).
                    LastAsync();
                activate.IsUsed = true;
                db.Entry(activate).State = EntityState.Modified;
                await db.SaveChangesAsync();
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code, PhoneNumber },
                    protocol: Request.Scheme);
                return Redirect($"{HtmlEncoder.Default.Encode(callbackUrl)}");
            }
            ModelState.AddModelError("Input.Code", "Не верный код подтверждения или время смс истекло");
            return Page();
        }
    }
}
