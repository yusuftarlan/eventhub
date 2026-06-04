using EventHub.Data;
using EventHub.Models;
using EventHub.Services;
using EventHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Controllers;

public class EventsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventService _eventService;

    public EventsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEventService eventService)
    {
        _context = context;
        _userManager = userManager;
        _eventService = eventService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(int? categoryId, string? searchTerm)
    {
        var query = _context.Events
            .Include(e => e.Category)
            .Include(e => e.Participants)
            .Include(e => e.FavoriteEvents)
            .Where(e => e.IsActive && e.Date > DateTime.Now);

        if (categoryId.HasValue)
        {
            query = query.Where(e => e.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(e => e.Title.Contains(searchTerm) || e.Description.Contains(searchTerm) || e.Location.Contains(searchTerm));
        }

        var userId = _userManager.GetUserId(User);
        var model = new EventListViewModel
        {
            CategoryId = categoryId,
            SearchTerm = searchTerm,
            Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", categoryId),
            Events = await query.OrderBy(e => e.Date).Select(e => ToCard(e, userId)).ToListAsync()
        };

        return View(model);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var eventItem = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Organizer)
            .Include(e => e.Participants)
            .Include(e => e.FavoriteEvents)
            .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);

        if (eventItem is null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        return View(new EventDetailsViewModel
        {
            Event = ToCard(eventItem, userId),
            OrganizerName = eventItem.Organizer?.FullName ?? "EventHub",
            FullDescription = eventItem.Description
        });
    }

    [Authorize(Roles = $"{DbInitializer.UserRole},{DbInitializer.AdminRole},{DbInitializer.OrganizerRole}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(int id)
    {
        var userId = _userManager.GetUserId(User);
        var result = await _eventService.JoinEventAsync(id, userId);
        if (result.NotFound)
        {
            return NotFound();
        }

        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(int id)
    {
        var userId = _userManager.GetUserId(User);
        var result = await _eventService.LeaveEventAsync(id, userId);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Favorite(int id)
    {
        var userId = _userManager.GetUserId(User);
        var result = await _eventService.ToggleFavoriteAsync(id, userId);
        if (result.NotFound)
        {
            return NotFound();
        }

        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return Redirect(Request.Headers.Referer.ToString() ?? Url.Action(nameof(Index))!);
    }

    private static EventCardViewModel ToCard(Event eventItem, string? userId)
    {
        return new EventCardViewModel
        {
            Id = eventItem.Id,
            Title = eventItem.Title,
            Description = eventItem.Description,
            Date = eventItem.Date,
            Location = eventItem.Location,
            Capacity = eventItem.Capacity,
            ParticipantCount = eventItem.Participants.Count,
            ImageUrl = eventItem.ImageUrl,
            CategoryName = eventItem.Category?.Name ?? "Genel",
            IsJoined = userId != null && eventItem.Participants.Any(p => p.UserId == userId),
            IsFavorite = userId != null && eventItem.FavoriteEvents.Any(f => f.UserId == userId)
        };
    }
}
