// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using MainWebApplication.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations.Schema;
using MainWebApplication.Models;
using MainWebApplication.Methods;
using System.Net.Http;
namespace MainWebApplication.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly IUserStore<AspNetUser> _userStore;
        private readonly IUserEmailStore<AspNetUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<AspNetUser> userManager,
            IUserStore<AspNetUser> userStore,
            SignInManager<AspNetUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender, ApplicationDbContext context)
        {
            db = context;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

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
            [Required(ErrorMessage = "Введите номер телефона")]
            [Phone]
            [Display(Name = "Телефон")]
            public string PhoneNumber { get; set; }
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Заполните пароль")]
            [StringLength(100, ErrorMessage = "Минимум 6 символов.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Пароль")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Подтверждение пароля ")]
            [Compare("Password", ErrorMessage = "Пароль и подтверждение пароля не совпадают.")]
            public string ConfirmPassword { get; set; }
            [Required(ErrorMessage = "Введите БИН/ИИН организации")]
            [StringLength(12, ErrorMessage = "Ошибка. Поле должно содержать в себе 12 символов!", MinimumLength = 12)]
            [DataType(DataType.Text)]
            [Display(Name = "ИИН/БИН организации")]
            public string OrganizationNumber { get; set; }
            [Display(Name = "Пароль организации")]
            [DataType(DataType.Password)]
            public string OrganizationPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }
        ApplicationDbContext db;

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            var checkOrganization = db.Organizations.FirstOrDefault(x => x.OrganizationNumber == Input.OrganizationNumber);
            if (checkOrganization != null)
            {
                if (ModelState.IsValid)
                {
                    var user = CreateUser();
                    await _userManager.AddToRoleAsync(user, "Мастер");
                    user.OrganizationId = checkOrganization.Id;

                    var phoneNumber = CorrectSymbols.CorrectMethod(Input.PhoneNumber);


                    await _userStore.SetUserNameAsync(user, phoneNumber, CancellationToken.None);
                    var result = await _userManager.CreateAsync(user, Input.Password);

                    if (result.Succeeded)
                    {
                        Random codeSMS = new Random();
                        float key = codeSMS.Next(1000, 9999);

                        _logger.LogInformation("User created a new account with password.");

                        var userId = await _userManager.GetUserIdAsync(user);
                        var callbackUrl = Url.Page(
                            "/Account/RegisterConfirmation",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = codeSMS, returnUrl = returnUrl },
                            protocol: Request.Scheme);


                        SmsActivate activate = new SmsActivate();
                        activate.DateOfStart = DateTime.Now;
                        activate.DateOfEnd = activate.DateOfStart.AddMinutes(5);
                        activate.SMSCode = Convert.ToString(key);
                        activate.PhoneNumber = phoneNumber;
                        await db.SmsActivates.AddAsync(activate);
                        await db.SaveChangesAsync();

                        var apiUrl = "https://smsc.ru/sys/send.php";
                        var login = "sosarlye";
                        var password = "Ohavizz11";
                        var phones = activate.PhoneNumber.Trim(new char[] { '+' });
                        var message = activate.SMSCode;
                        var url = "https://smsc.ru/sys/send.php?login=exampleLogin&psw=examplePassword&phones=" + phones + "&mes=Код активации для AvtoMig: " + message;
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            using (var client = new HttpClient())
                            {
                                var response = await client.GetAsync(url);
                                if (response.IsSuccessStatusCode)
                                {
                                    return RedirectToPage("RegisterConfirmation", new { phoneNumber = phoneNumber, returnUrl = returnUrl });
                                }
                                else
                                {
                                    // Обработка ошибки при отправке сообщения
                                    // Возможно, вам понадобится добавить соответствующую логику обработки ошибки
                                }
                            }
                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            ModelState.AddModelError(string.Empty, "Неправильный номер организации или организация не зарегистрирована");
            // If we got this far, something failed, redisplay form
            return Page();
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

        private IUserEmailStore<AspNetUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AspNetUser>)_userStore;
        }
    }
}
