using EventHub.Data;
using EventHub.Models;
using EventHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Controllers;

[Authorize]
public class FavoritesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public FavoritesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var events = await _context.FavoriteEvents
            .Include(f => f.Event)!.ThenInclude(e => e!.Category)
            .Include(f => f.Event)!.ThenInclude(e => e!.Participants)
            .Include(f => f.Event)!.ThenInclude(e => e!.FavoriteEvents)
            .Where(f => f.UserId == userId && f.Event != null)
            .OrderBy(f => f.Event!.Date)
            .Select(f => new EventCardViewModel
            {
                Id = f.Event!.Id,
                Title = f.Event.Title,
                Description = f.Event.Description,
                Date = f.Event.Date,
                Location = f.Event.Location,
                Capacity = f.Event.Capacity,
                ParticipantCount = f.Event.Participants.Count,
                ImageUrl = f.Event.ImageUrl,
                CategoryName = f.Event.Category!.Name,
                IsJoined = f.Event.Participants.Any(p => p.UserId == userId),
                IsFavorite = true
            })
            .ToListAsync();

        return View(events);
    }
}
