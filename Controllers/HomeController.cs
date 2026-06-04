using System.Diagnostics;
using EventHub.Data;
using EventHub.Models;
using EventHub.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var events = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Participants)
            .Include(e => e.FavoriteEvents)
            .Where(e => e.IsActive && e.Date > DateTime.Now)
            .OrderBy(e => e.Date)
            .Take(6)
            .Select(e => new EventCardViewModel
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Date = e.Date,
                Location = e.Location,
                Capacity = e.Capacity,
                ParticipantCount = e.Participants.Count,
                ImageUrl = e.ImageUrl,
                CategoryName = e.Category!.Name,
                IsJoined = userId != null && e.Participants.Any(p => p.UserId == userId),
                IsFavorite = userId != null && e.FavoriteEvents.Any(f => f.UserId == userId)
            })
            .ToListAsync();

        return View(events);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
