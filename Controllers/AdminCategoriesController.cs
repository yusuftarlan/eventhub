using EventHub.Data;
using EventHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Controllers;

[Authorize(Roles = DbInitializer.AdminRole)]
[Route("Admin/Categories")]
public class AdminCategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminCategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return View("~/Views/Admin/Categories/Index.cshtml", await _context.Categories.Include(c => c.Events).OrderBy(c => c.Name).ToListAsync());
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/Categories/Create.cshtml", new Category());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        category.Name = (category.Name ?? string.Empty).Trim();

        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/Categories/Create.cshtml", category);
        }

        if (await CategoryNameExistsAsync(category.Name))
        {
            ModelState.AddModelError(nameof(category.Name), "Bu isimde bir kategori zaten var.");
            return View("~/Views/Admin/Categories/Create.cshtml", category);
        }

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Kategori oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        return category is null ? NotFound() : View("~/Views/Admin/Categories/Edit.cshtml", category);
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id)
        {
            return BadRequest();
        }

        category.Name = (category.Name ?? string.Empty).Trim();

        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/Categories/Edit.cshtml", category);
        }

        if (await CategoryNameExistsAsync(category.Name, category.Id))
        {
            ModelState.AddModelError(nameof(category.Name), "Bu isimde bir kategori zaten var.");
            return View("~/Views/Admin/Categories/Edit.cshtml", category);
        }

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Kategori güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.Include(c => c.Events).FirstOrDefaultAsync(c => c.Id == id);
        return category is null ? NotFound() : View("~/Views/Admin/Categories/Delete.cshtml", category);
    }

    [HttpPost("Delete/{id:int}")]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.Include(c => c.Events).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        if (category.Events.Any())
        {
            TempData["Error"] = "Etkinliği olan kategori silinemez.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Kategori silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CategoryNameExistsAsync(string name, int? excludedId = null)
    {
        var normalizedName = name.ToLower();
        return await _context.Categories.AnyAsync(c => c.Name.ToLower() == normalizedName && (!excludedId.HasValue || c.Id != excludedId.Value));
    }
}
