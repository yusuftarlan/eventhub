using EventHub.Data;
using EventHub.Models;
using EventHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Controllers;

[Authorize]
public class MyEventsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MyEventsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var events = await _context.EventParticipants
            .Include(p => p.Event)!.ThenInclude(e => e!.Category)
            .Include(p => p.Event)!.ThenInclude(e => e!.Participants)
            .Include(p => p.Event)!.ThenInclude(e => e!.FavoriteEvents)
            .Where(p => p.UserId == userId && p.Event != null)
            .OrderBy(p => p.Event!.Date)
            .Select(p => new EventCardViewModel
            {
                Id = p.Event!.Id,
                Title = p.Event.Title,
                Description = p.Event.Description,
                Date = p.Event.Date,
                Location = p.Event.Location,
                Capacity = p.Event.Capacity,
                ParticipantCount = p.Event.Participants.Count,
                ImageUrl = p.Event.ImageUrl,
                CategoryName = p.Event.Category!.Name,
                IsJoined = true,
                IsFavorite = p.Event.FavoriteEvents.Any(f => f.UserId == userId)
            })
            .ToListAsync();

        return View(events);
    }
}
