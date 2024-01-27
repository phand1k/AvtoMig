// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MainWebApplication.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MainWebApplication.Models;

namespace MainWebApplication.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly IEmailSender _sender;
        ApplicationDbContext db;

        public RegisterConfirmationModel(UserManager<AspNetUser> userManager, IEmailSender sender, ApplicationDbContext context, SignInManager<AspNetUser> signInManager)
        {
            db = context;
            _userManager = userManager;
            _sender = sender;
            this.signInManager = signInManager;
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [Display(Name = "Код подтверждения")]
            public string Code { get; set; }
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string PhoneNumber { get; set; }


        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool DisplayConfirmAccountLink { get; set; }

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
        private readonly SignInManager<AspNetUser> signInManager;

        public async Task<IActionResult> OnPostAsync(string phoneNumber)
        {
            var Sms = await db.SmsActivates.
                Where(x => x.PhoneNumber == phoneNumber && !x.IsUsed).OrderByDescending(x => x.DateOfEnd).
                LastOrDefaultAsync();
            PhoneNumber = phoneNumber;
            if (Sms == null)
            {
                ModelState.AddModelError("Input.Code", "Истек срок действия или код уже был использован");
                return Page();
            }
            if (Input.Code == Sms.SMSCode)
            {
                AspNetUser user = await db.AspNetUsers.
                    Where(x => x.UserName == phoneNumber).FirstOrDefaultAsync();
                user.EmailConfirmed = true;

                db.Entry(user).State = EntityState.Modified;
                await db.SaveChangesAsync();

                SmsActivate activate = await db.SmsActivates.
                    Where(x => x.PhoneNumber == phoneNumber).OrderByDescending(x => x.DateOfEnd).
                    LastOrDefaultAsync();
                activate.IsUsed = true;
                db.Entry(activate).State = EntityState.Modified;
                await db.SaveChangesAsync();
                await signInManager.SignInAsync(user, isPersistent: false);
                return Redirect("/Home/Index");
            }
            ModelState.AddModelError("Input.Code", "Не верный код подтверждения");
            return Page();
        }
    }
}
