using EventHub.Data;
using EventHub.Models;
using EventHub.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var localSettingsPath = Path.Combine(builder.Environment.ContentRootPath, "appsettings.Local.json");
if (File.Exists(localSettingsPath) && new FileInfo(localSettingsPath).Length > 0)
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
}

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddSingleton<DatabaseStatusService>();
builder.Services.AddSingleton<LocalSettingsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var databaseStatus = context.RequestServices.GetRequiredService<DatabaseStatusService>();
    var path = context.Request.Path;
    var isAllowedWhileOffline =
        path.StartsWithSegments("/Account") ||
        path.StartsWithSegments("/Setup") ||
        path.StartsWithSegments("/lib") ||
        path.StartsWithSegments("/css") ||
        path.StartsWithSegments("/js") ||
        path.StartsWithSegments("/favicon.ico");

    if (!databaseStatus.IsAvailable && !isAllowedWhileOffline)
    {
        context.Response.Redirect("/Setup/Database");
        return;
    }

    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

var databaseStatus = app.Services.GetRequiredService<DatabaseStatusService>();
try
{
    await DbInitializer.SeedAsync(app.Services);
    databaseStatus.MarkAvailable();
}
catch (Exception ex)
{
    databaseStatus.MarkUnavailable(ex.Message);
    app.Logger.LogWarning(ex, "Database is not available. Setup mode is active.");
}

app.Run();
