using EventHub.Data;
using EventHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Controllers;

[Authorize(Roles = DbInitializer.AdminRole)]
[Route("Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var topEvents = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Participants)
            .OrderByDescending(e => e.Participants.Count)
            .Take(5)
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
                CategoryName = e.Category!.Name
            })
            .ToListAsync();

        return View(new AdminDashboardViewModel
        {
            TotalEvents = await _context.Events.CountAsync(),
            TotalUsers = await _context.Users.CountAsync(),
            TotalCategories = await _context.Categories.CountAsync(),
            TotalParticipations = await _context.EventParticipants.CountAsync(),
            TopEvents = topEvents
        });
    }
}
