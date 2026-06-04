using EventHub.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Controllers;

[Authorize(Roles = DbInitializer.AdminRole)]
[Route("Admin/Events")]
public class AdminEventsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminEventsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var events = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Organizer)
            .Include(e => e.Participants)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return View("~/Views/Admin/Events/Index.cshtml", events);
    }
}
