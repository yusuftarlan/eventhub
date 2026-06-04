using EventHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Data;

public static class DbInitializer
{
    public const string AdminRole = "Admin";
    public const string OrganizerRole = "Organizer";
    public const string UserRole = "User";
    private const string DefaultPassword = "EventHub123!";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();
        await EnsureRolesAsync(roleManager);

        var admin = await EnsureUserAsync(userManager, "admin@eventhub.com", "EventHub Admin", AdminRole);
        var organizer = await EnsureUserAsync(userManager, "organizer@eventhub.com", "EventHub Organizer", OrganizerRole);
        await EnsureUserAsync(userManager, "organizer2@eventhub.com", "EventHub Organizer 2", OrganizerRole);
        await EnsureUserAsync(userManager, "organizer3@eventhub.com", "EventHub Organizer 3", OrganizerRole);
        await EnsureUserAsync(userManager, "user@eventhub.com", "EventHub User", UserRole);
        await EnsureUserAsync(userManager, "user2@eventhub.com", "EventHub User 2", UserRole);
        await EnsureUserAsync(userManager, "user3@eventhub.com", "EventHub User 3", UserRole);
        await EnsureUserAsync(userManager, "user4@eventhub.com", "EventHub User 4", UserRole);

        if (!await context.Categories.AnyAsync())
        {
            context.Categories.AddRange(
                new Category { Name = "Yazilim", Description = "Programlama, web ve yazilim gelistirme etkinlikleri." },
                new Category { Name = "Tasarim", Description = "UI/UX, urun tasarimi ve yaratici atolyeler." },
                new Category { Name = "Girisimcilik", Description = "Is modeli, pazarlama ve startup bulusmalari." },
                new Category { Name = "Kariyer", Description = "CV, mulakat ve profesyonel gelisim etkinlikleri." }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Events.AnyAsync())
        {
            var categories = await context.Categories.ToListAsync();
            var software = categories.First(c => c.Name == "Yazilim");
            var design = categories.First(c => c.Name == "Tasarim");
            var entrepreneurship = categories.First(c => c.Name == "Girisimcilik");

            context.Events.AddRange(
                new Event
                {
                    Title = "ASP.NET Core MVC Workshop",
                    Description = "Code-first veri modeli, Identity ve MVC katmanlariyla gercek bir web uygulamasi gelistirme workshopu.",
                    Date = DateTime.Now.AddDays(10),
                    Location = "Istanbul Teknokent",
                    Capacity = 40,
                    ImageUrl = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=1200&q=80",
                    CategoryId = software.Id,
                    OrganizerId = organizer.Id,
                    IsActive = true
                },
                new Event
                {
                    Title = "UI/UX Tasarim Sprinti",
                    Description = "Kullanici arastirmasi, wireframe ve prototipleme adimlarini uygulamali ogreten yogun tasarim sprinti.",
                    Date = DateTime.Now.AddDays(18),
                    Location = "Kadikoy Tasarim Merkezi",
                    Capacity = 25,
                    ImageUrl = "https://images.unsplash.com/photo-1559028012-481c04fa702d?auto=format&fit=crop&w=1200&q=80",
                    CategoryId = design.Id,
                    OrganizerId = organizer.Id,
                    IsActive = true
                },
                new Event
                {
                    Title = "Startup Fikirden Sunuma",
                    Description = "Bir girisim fikrini dogrulama, is modeline donusturme ve yatirimci sunumu hazirlama etkinligi.",
                    Date = DateTime.Now.AddDays(25),
                    Location = "Ankara Kulucka Merkezi",
                    Capacity = 60,
                    ImageUrl = "https://images.unsplash.com/photo-1556761175-b413da4baf72?auto=format&fit=crop&w=1200&q=80",
                    CategoryId = entrepreneurship.Id,
                    OrganizerId = admin.Id,
                    IsActive = true
                }
            );
            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { AdminRole, OrganizerRole, UserRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(user, DefaultPassword);
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        return user;
    }
}
