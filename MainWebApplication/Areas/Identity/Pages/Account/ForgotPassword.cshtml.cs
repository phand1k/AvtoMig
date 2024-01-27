// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MainWebApplication.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MainWebApplication.Methods;
using MainWebApplication.Models;

namespace MainWebApplication.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly IEmailSender _emailSender;
        private ApplicationDbContext db;

        public ForgotPasswordModel(ApplicationDbContext _db, UserManager<AspNetUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            db = _db;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [Display(Name = "Номер телефона")]
            public string PhoneNumber { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(Input.PhoneNumber);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    Random codeSMS = new Random();
                    float key = codeSMS.Next(1000, 9999);
                    SmsActivate activate = new SmsActivate();
                    // Don't reveal that the user does not exist or is not confirmed
                    var phoneNumber = CorrectSymbols.CorrectMethod(Input.PhoneNumber);
                    activate.DateOfStart = DateTime.Now;
                    activate.DateOfEnd = activate.DateOfStart.AddMinutes(5);
                    activate.SMSCode = Convert.ToString(key);
                    activate.PhoneNumber = phoneNumber;
                    await db.SmsActivates.AddAsync(activate);
                    await db.SaveChangesAsync();
                    return RedirectToPage("ForgotPasswordConfirmation", new {phoneNumber = phoneNumber});
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.PhoneNumber,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
