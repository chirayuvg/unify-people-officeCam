using Unify.Common.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Unify.Common.Models;

namespace Unify.Common.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountController : Controller
    {

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly DiscoveryService _discoveryService;
        public AccountController(SignInManager<IdentityUser> signInManager,
            ILogger<AccountController> logger, DiscoveryService discoveryService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _discoveryService = discoveryService;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            LoginViewModel model = new LoginViewModel();
            
            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            model.ReturnUrl = returnUrl;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginViewModel model)
        {
            if (!string.IsNullOrEmpty(model.ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, model.ErrorMessage);
            }

            model.ReturnUrl ??= "/";

            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return Redirect(model.ReturnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    string url = _discoveryService.GetSelectApplicationUrl();
                    url = url += $"/Identity/Action/LoginWith2fa?ReturnUrl={model.ReturnUrl}&RememberMe={model.RememberMe}";

                    return Redirect(url);
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    string url = _discoveryService.GetSelectApplicationUrl();
                    url = url += $"/Identity/Action/Lockout";

                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            return View(model);
        }
    }

}
