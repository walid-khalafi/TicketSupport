using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TicketSupport.DAL;
using TicketSupport.WEB.Models.AccountViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using TicketSupport.Controllers;



// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TicketSupport.WEB.Controllers
{

    public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    //private readonly Gazelle.AzmaBoton.Services.Messaging.IEmailSender _emailSender;
    private readonly ILogger _logger;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public AccountController(

        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        //  Gazelle.AzmaBoton.Services.Messaging.IEmailSender emailSender,
        ILogger<AccountController> logger,
        ApplicationDbContext context, IWebHostEnvironment hostingEnvironment

        )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        //  _emailSender = emailSender;
        _logger = logger;
        _context = context;
        _hostingEnvironment = hostingEnvironment;
        // _userService = userService;
        // _orgService = orgService;

    }

    [TempData]
    public string ErrorMessage { get; set; }

    // GET: /<controller>/
    public IActionResult Index()
    {
        return View();
    }
    [AllowAnonymous]
    public async Task<IActionResult> Login(string returnUrl = "")
    {
        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ViewData["ReturnUrl"] = returnUrl;
        return View();

    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        // if (ModelState.IsValid)
        //  {
        // This doesn't count login failures towards account lockout
        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
        var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in.");
            return RedirectToLocal(returnUrl);
        }
        if (result.RequiresTwoFactor)
        {
            return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");
            return RedirectToAction(nameof(Lockout));
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");

            return RedirectToAction("Login", "Account");
        }

        // }
        _logger.LogWarning("Invalid Model State.");
        return RedirectToAction("Login", "Account");

    }
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
    {
        // Ensure the user has gone through the username & password screen first
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new ApplicationException($"Unable to load two-factor authentication user.");
        }

        var model = new LoginWith2faViewModel { RememberMe = rememberMe };
        ViewData["ReturnUrl"] = returnUrl;

        return View(model);
    }
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
            return RedirectToLocal(returnUrl);
        }
        else if (result.IsLockedOut)
        {
            _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
            return RedirectToAction(nameof(Lockout));
        }
        else
        {
            _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
            ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
            return View();
        }
    }
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
    {
        // Ensure the user has gone through the username & password screen first
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new ApplicationException($"Unable to load two-factor authentication user.");
        }

        ViewData["ReturnUrl"] = returnUrl;

        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new ApplicationException($"Unable to load two-factor authentication user.");
        }

        var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
            return RedirectToLocal(returnUrl);
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
            return RedirectToAction(nameof(Lockout));
        }
        else
        {
            _logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
            ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
            return View();
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Lockout()
    {
        return View();
    }
    public static bool IsPhoneNumber(string number)
    {
        string pat1 = "(09[0-9]{9}$)";
        //return Regex.Match(number, @"^(\+[0-9]{9})$").Success;
        return Regex.Match(number, pat1).Success;
    }
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Register(string returnUrl = "")
    {

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = "")
    {
        ViewData["ReturnUrl"] = returnUrl;
        // if (ModelState.IsValid)
        // {
        bool isPhoneNumber = IsPhoneNumber(model.PhoneNumber);
        if (!isPhoneNumber)
        {
            TempData["error_msg"] = "شماره تلفن صحیح نمی باشد";
            return View(model);
        }


        var db_user = _context.Users.FirstOrDefault(x => x.PhoneNumber == model.PhoneNumber);
        if (db_user != null)
        {
            TempData["error_msg"] = "ایمیل یا شماره تماس قبلا در سیستم ثبت شده است درصورتی که رمز عبور خود را فراموش کرده اید از بخش فراموشی رمز عبور اقدام به بازیابی حساب کاربری خود نمایید.";
            return View(model);
        }

        var user = new IdentityUser { UserName = model.PhoneNumber, Email = "notset@notset.com", PhoneNumber = model.PhoneNumber };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            // check if customer role exist

            var role = _context.Roles.FirstOrDefault(x => x.Name == "Client");
            if (role == null)
            {
                _context.Roles.Add(new IdentityRole
                {
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    Id = Guid.NewGuid().ToString(),
                    Name = "Client",
                    NormalizedName = "CLIENT",
                });

                _context.SaveChanges();
            }
            await _userManager.AddToRoleAsync(user, "Client");

            _logger.LogInformation("User created a new account with password.");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //  var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);

            //StringBuilder template = new StringBuilder();
            // load templates
            //var email_template_folder = _hostingEnvironment.ContentRootFileProvider.GetDirectoryContents("/EmailTemplates");
            //try
            //{
            //    var tempalte_path = email_template_folder.FirstOrDefault(x => x.Name == "ConfirmEmailTemplate.html");
            //    if (tempalte_path != null)
            //    {
            //        // Open the text file using a stream reader.
            //        using (StreamReader sr = new StreamReader(Path.Combine(tempalte_path.PhysicalPath)))
            //        {
            //            // Read the stream to a string, and write the string to the console.
            //            template.Append(sr.ReadToEnd());
            //            // String line = sr.ReadToEnd();
            //            //  Console.WriteLine(line);
            //        }
            //    }

            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("The file could not be read:");
            //    Console.WriteLine(e.Message);
            //}
            //template = template.Replace("[[Email]]", user.Email);
            //template = template.Replace("[[Link]]", callbackUrl);

            //try
            //{
            //    await _emailSender.SendEmailConfirmationVer1Async(model.Email, template.ToString());
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}

            //  await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("User created a new account with password.");
            TempData["success_msg"] = "لطفا جهت فعال سازی اشتراک بر روی لینک ارسال شده به ایمیل کلیک نمایید";
            return RedirectToAction("Login");
            //   return RedirectToLocal(returnUrl);
        }
        AddErrors(result);
        //  }
        TempData["error_msg"] = "لطفا فرم ثبت نام را با دقت تکمیل نمایید";
        // If we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction(nameof(AccountController.Login), "Account");
    }
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string returnUrl = null)
    {
        // Request a redirect to the external login provider.
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
    {
        if (remoteError != null)
        {
            ErrorMessage = $"Error from external provider: {remoteError}";
            return RedirectToAction(nameof(Login));
        }
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return RedirectToAction(nameof(Login));
        }

        // Sign in the user with this external login provider if the user already has a login.
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
            return RedirectToLocal(returnUrl);
        }
        if (result.IsLockedOut)
        {
            return RedirectToAction(nameof(Lockout));
        }
        else
        {
            // If the user does not have an account, then ask the user to create an account.
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            return View("ExternalLogin", new ExternalLoginViewModel { Email = email });
        }
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                throw new ApplicationException("Error loading external login information during confirmation.");
            }
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                var role = _context.Roles.FirstOrDefault(x => x.Name == "Client");
                if (role == null)
                {
                    _context.Roles.Add(new IdentityRole
                    {
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        Id = Guid.NewGuid().ToString(),
                        Name = "Client",
                        NormalizedName = "CLIENT",
                    });
                    _context.SaveChanges();
                }
                await _userManager.AddToRoleAsync(user, "Client");
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                    return RedirectToLocal(returnUrl);
                }
            }
            AddErrors(result);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(nameof(ExternalLogin), model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userId}'.");
        }
        var result = await _userManager.ConfirmEmailAsync(user, code);
        return View(result.Succeeded ? "ConfirmEmail" : "Error");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            // var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
            // await _emailSender.SendEmailAsync("support@chaptak.com", model.Email, "Reset Password",
            // $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string code = null)
    {
        if (code == null)
        {
            throw new ApplicationException("A code must be supplied for password reset.");
        }
        var model = new ResetPasswordViewModel { Code = code };
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }
        var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }
        AddErrors(result);
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }


    [HttpGet]
    public IActionResult AccessDenied()
    {

        return View();
    }

    #region Helpers

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }

    #endregion



}

}