using EventHub.Data;
using EventHub.Services;
using EventHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EventHub.Controllers;

[Authorize(Roles = DbInitializer.AdminRole)]
[Route("Setup")]
public class SetupController : Controller
{
    private readonly DatabaseStatusService _databaseStatus;
    private readonly LocalSettingsService _localSettings;
    private readonly ILogger<SetupController> _logger;
    private readonly IServiceProvider _serviceProvider;

    public SetupController(
        DatabaseStatusService databaseStatus,
        LocalSettingsService localSettings,
        ILogger<SetupController> logger,
        IServiceProvider serviceProvider)
    {
        _databaseStatus = databaseStatus;
        _localSettings = localSettings;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    [HttpGet("Database")]
    public IActionResult Database()
    {
        ViewBag.DatabaseAvailable = _databaseStatus.IsAvailable;
        ViewBag.LastError = _databaseStatus.LastError;
        return View(_localSettings.GetDatabaseSetupViewData());
    }

    [HttpPost("Database")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Database(DatabaseSetupViewData model)
    {
        ViewBag.DatabaseAvailable = _databaseStatus.IsAvailable;
        ViewBag.LastError = _databaseStatus.LastError;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var connectionString = _localSettings.BuildConnectionString(model);

        try
        {
            await TestServerConnectionAsync(connectionString);
            _localSettings.SaveConnectionString(connectionString);

            await DbInitializer.SeedAsync(_serviceProvider);
            _databaseStatus.MarkAvailable();

            TempData["Success"] = "SQL Server baglantisi kuruldu, migration ve seed islemleri tamamlandi.";
            return RedirectToAction("Dashboard", "Admin");
        }
        catch (Exception ex)
        {
            _databaseStatus.MarkUnavailable(ex.Message);
            _logger.LogWarning(ex, "Database setup failed.");
            ModelState.AddModelError(string.Empty, $"Baglanti kurulamadi: {ex.GetBaseException().Message}");
            ViewBag.DatabaseAvailable = false;
            ViewBag.LastError = ex.GetBaseException().Message;
            return View(model);
        }
    }

    private static async Task TestServerConnectionAsync(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master",
            ConnectTimeout = 5
        };

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
    }
}
