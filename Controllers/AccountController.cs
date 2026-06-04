using EventHub.Data;
using EventHub.Models;
using EventHub.Services;
using EventHub.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventHub.Controllers;

public class AccountController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly DatabaseStatusService _databaseStatus;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        IConfiguration configuration,
        DatabaseStatusService databaseStatus,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _configuration = configuration;
        _databaseStatus = databaseStatus;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (IsSetupAdmin(model.Email, model.Password))
        {
            await SignInSetupAdminAsync(model.Email, model.RememberMe);
            TempData["Success"] = "Kurulum yoneticisi olarak giris yapildi.";
            return RedirectToAction("Database", "Setup");
        }

        try
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            if (result.Succeeded)
            {
                TempData["Success"] = "Giris basarili.";
                return LocalRedirect(model.ReturnUrl ?? Url.Action("Index", "Home")!);
            }
        }
        catch (Exception ex) when (IsDatabaseFailure(ex))
        {
            _databaseStatus.MarkUnavailable(ex.Message);
            ModelState.AddModelError(string.Empty, "Veritabanina baglanilamiyor. Kurulum yoneticisi ile giris yapip SQL Server bilgisini guncelleyin.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "E-posta veya sifre hatali.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, DbInitializer.UserRole);
            await _signInManager.SignInAsync(user, false);
            TempData["Success"] = "Kayit basarili. Hos geldiniz.";
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["Success"] = "Cikis yapildi.";
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private bool IsSetupAdmin(string email, string password)
    {
        var setupEmail = _configuration["SetupAdmin:Email"];
        var setupPassword = _configuration["SetupAdmin:Password"];

        return !string.IsNullOrWhiteSpace(setupEmail) &&
               !string.IsNullOrWhiteSpace(setupPassword) &&
               string.Equals(email, setupEmail, StringComparison.OrdinalIgnoreCase) &&
               password == setupPassword;
    }

    private async Task SignInSetupAdminAsync(string email, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "setup-admin"),
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, DbInitializer.AdminRole),
            new("IsSetupAdmin", "true")
        };

        var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = rememberMe });
    }

    private static bool IsDatabaseFailure(Exception ex)
    {
        return ex is DbUpdateException ||
               ex is InvalidOperationException ||
               ex.GetBaseException().GetType().Namespace?.StartsWith("Microsoft.Data.SqlClient") == true;
    }
}
