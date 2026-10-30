using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using SonGun.Models;
using SonGun.Models.ViewModels;

namespace SonGun.Controllers;

public class AccountController : Controller
{
    private readonly AuthSettings _authSettings;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IOptions<AuthSettings> authSettings, ILogger<AccountController> logger)
    {
        _authSettings = authSettings.Value;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Redirects already authenticated users away from the login page to the target URL or home
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login-limiter")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Re-renders the login view with validation error messages if input constraints fail
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? role = null;

        // Assigns the 'Admin' role if submitted credentials match administrator configuration
        if (SafeEqualsUsername(model.Username, _authSettings.AdminUsername) &&
            SafeEqualsPassword(model.Password, _authSettings.AdminPassword))
        {
            role = "Admin";
        }
        // Assigns the 'Editor' role if submitted credentials match editor configuration
        else if (SafeEqualsUsername(model.Username, _authSettings.EditorUsername) &&
                 SafeEqualsPassword(model.Password, _authSettings.EditorPassword))
        {
            role = "Editor";
        }

        // Issues an encrypted authentication cookie and signs the user in upon successful role match
        if (role != null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, model.Username.Trim()),
                new(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("Kullanıcı {Username} ({Role}) başarıyla giriş yaptı.", model.Username, role);
            return RedirectToLocal(model.ReturnUrl);
        }

        _logger.LogWarning("Başarısız giriş denemesi: {Username}", model.Username);
        ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    // Validates target redirect URL to prevent Open Redirect phishing vulnerabilities
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        // Ensures the return URL belongs to the local site before performing redirect
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    // Compares usernames in a case-insensitive manner while rejecting null/whitespace input
    private static bool SafeEqualsUsername(string? a, string? b)
    {
        // Rejects comparison immediately if either username string is null or whitespace
        if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
        {
            return false;
        }

        return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    // Compares passwords using constant-time byte equality to prevent side-channel timing attacks
    private static bool SafeEqualsPassword(string? input, string? actual)
    {
        // Returns false immediately if either password argument is null or empty
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(actual))
        {
            return false;
        }

        var aBytes = Encoding.UTF8.GetBytes(input.Trim());
        var bBytes = Encoding.UTF8.GetBytes(actual.Trim());

        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}
