using EventHub.Data;
using EventHub.Models;
using EventHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Controllers;

[Authorize(Roles = DbInitializer.AdminRole)]
[Route("Admin/Users")]
public class AdminUsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
        var rows = new List<UserRowViewModel>();

        foreach (var user in users)
        {
            rows.Add(new UserRowViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                CreatedAt = user.CreatedAt,
                Roles = string.Join(", ", await _userManager.GetRolesAsync(user))
            });
        }

        return View("~/Views/Admin/Users/Index.cshtml", rows);
    }
}
