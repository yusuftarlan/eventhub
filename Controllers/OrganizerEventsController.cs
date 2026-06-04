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

[Authorize(Roles = $"{DbInitializer.OrganizerRole},{DbInitializer.AdminRole}")]
[Route("Organizer/Events")]
public class OrganizerEventsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventService _eventService;

    public OrganizerEventsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEventService eventService)
    {
        _context = context;
        _userManager = userManager;
        _eventService = eventService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var events = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Participants)
            .Where(e => User.IsInRole(DbInitializer.AdminRole) || e.OrganizerId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return View("~/Views/Organizer/Events/Index.cshtml", events);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        return View("~/Views/Organizer/Events/Create.cshtml", await PrepareFormAsync(new EventFormViewModel()));
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Organizer/Events/Create.cshtml", await PrepareFormAsync(model));
        }

        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var eventItem = new Event
        {
            Title = model.Title,
            Description = model.Description,
            Date = model.Date,
            Location = model.Location,
            Capacity = model.Capacity,
            ImageUrl = model.ImageUrl,
            CategoryId = model.CategoryId,
            OrganizerId = userId,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Etkinlik oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var eventItem = await FindAuthorizedEventAsync(id);
        if (eventItem is null)
        {
            return NotFound();
        }

        return View("~/Views/Organizer/Events/Edit.cshtml", await PrepareFormAsync(new EventFormViewModel
        {
            Id = eventItem.Id,
            Title = eventItem.Title,
            Description = eventItem.Description,
            Date = eventItem.Date,
            Location = eventItem.Location,
            Capacity = eventItem.Capacity,
            ImageUrl = eventItem.ImageUrl,
            CategoryId = eventItem.CategoryId,
            IsActive = eventItem.IsActive
        }));
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var eventItem = await FindAuthorizedEventAsync(id);
        if (eventItem is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("~/Views/Organizer/Events/Edit.cshtml", await PrepareFormAsync(model));
        }

        var capacityResult = await _eventService.EnsureCapacityCanBeUpdatedAsync(id, model.Capacity);
        if (!capacityResult.Succeeded)
        {
            ModelState.AddModelError(nameof(model.Capacity), capacityResult.Message);
            return View("~/Views/Organizer/Events/Edit.cshtml", await PrepareFormAsync(model));
        }

        eventItem.Title = model.Title;
        eventItem.Description = model.Description;
        eventItem.Date = model.Date;
        eventItem.Location = model.Location;
        eventItem.Capacity = model.Capacity;
        eventItem.ImageUrl = model.ImageUrl;
        eventItem.CategoryId = model.CategoryId;
        eventItem.IsActive = model.IsActive;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Etkinlik güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eventItem = await FindAuthorizedEventAsync(id);
        if (eventItem is null)
        {
            return NotFound();
        }

        return View("~/Views/Organizer/Events/Delete.cshtml", eventItem);
    }

    [HttpPost("Delete/{id:int}")]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var eventItem = await FindAuthorizedEventAsync(id);
        if (eventItem is null)
        {
            return NotFound();
        }

        _context.Events.Remove(eventItem);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Etkinlik silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Participants/{id:int}")]
    public async Task<IActionResult> Participants(int id)
    {
        var eventItem = await FindAuthorizedEventAsync(id);
        if (eventItem is null)
        {
            return NotFound();
        }

        var participants = await _context.EventParticipants
            .Include(p => p.User)
            .Where(p => p.EventId == id)
            .OrderBy(p => p.JoinDate)
            .Select(p => new ParticipantRowViewModel
            {
                FullName = p.User!.FullName,
                Email = p.User.Email!,
                JoinDate = p.JoinDate
            })
            .ToListAsync();

        return View("~/Views/Organizer/Events/Participants.cshtml", new EventParticipantsViewModel
        {
            EventId = id,
            EventTitle = eventItem.Title,
            Participants = participants
        });
    }

    private async Task<EventFormViewModel> PrepareFormAsync(EventFormViewModel model)
    {
        model.Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", model.CategoryId);
        return model;
    }

    private async Task<Event?> FindAuthorizedEventAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        return await _context.Events
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id && (User.IsInRole(DbInitializer.AdminRole) || e.OrganizerId == userId));
    }
}
