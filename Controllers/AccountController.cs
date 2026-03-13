using InventoryManager.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    return RedirectToAction("Index", "Home");
                }
                if (result.IsLockedOut)
                {
                    return RedirectToPage("/Account/Lockout");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string email, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = email, Email = email };
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // If we got this far, something failed, redisplay form
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string? returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to redirect to home page
                return RedirectToAction("Index", "Home");
            }
        }

        // -----------------------------------------------------------------------
        // POST: Account/ExternalLogin
        // Initiates the OAuth2 challenge with the chosen external provider.
        //
        // WHY POST AND NOT GET?
        // Initiating an OAuth challenge from a GET makes the endpoint susceptible
        // to CSRF-based open-redirect attacks. Using POST + antiforgery token
        // ensures only our own login page can trigger an external login flow.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            // Build the URL the provider will redirect back to after authentication
            var redirectUrl = Url.Action(
                nameof(ExternalLoginCallback),
                "Account",
                new { returnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                provider, redirectUrl);

            // Challenge() triggers the redirect to Google / Facebook
            return Challenge(properties, provider);
        }

        // -----------------------------------------------------------------------
        // GET: Account/ExternalLoginCallback
        // Handles the redirect from the external provider after the user
        // authenticates (or denies) in the provider's UI.
        //
        // Flow:
        //   1. Read the external login info from the authentication cookie.
        //   2. Try to sign in with the existing external login record.
        //   3. If no record yet → create a new ApplicationUser, link the
        //      external login, then sign in.
        //   4. Redirect to returnUrl or Home.
        // -----------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            // The provider reported an error (e.g. user denied access)
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            // Retrieve the external login claims passed back by the provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                // Session expired or the callback was hit directly — restart login
                return RedirectToAction(nameof(Login));
            }

            // --- Attempt sign-in with an existing external login record ---
            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirectOrHome(returnUrl);
            }

            // --- First time with this provider: create a new user account ---
            // Extract the email claim supplied by the provider
            var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                // Provider did not supply an email — cannot create an account
                ModelState.AddModelError(string.Empty,
                    "The external login provider did not supply an email address. " +
                    "Please log in with a different method.");
                return RedirectToAction(nameof(Login));
            }

            // Reuse an existing account if the email is already registered
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                // No matching account — create one seeded from provider claims
                var newUser = new ApplicationUser
                {
                    UserName = email,
                    Email    = email,
                    EmailConfirmed = true   // Provider already verified the email
                };

                var createResult = await _userManager.CreateAsync(newUser);
                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return RedirectToAction(nameof(Login));
                }

                existingUser = newUser;
            }

            // Link the external login to the (new or existing) account
            await _userManager.AddLoginAsync(existingUser, info);

            // Sign the user in
            await _signInManager.SignInAsync(existingUser, isPersistent: false);
            return LocalRedirectOrHome(returnUrl);
        }

        // Small helper — redirects to returnUrl if local, otherwise to Home
        private IActionResult LocalRedirectOrHome(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}
