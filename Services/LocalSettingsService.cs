using System.Text.Json;
using EventHub.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EventHub.Services;

public class LocalSettingsService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public LocalSettingsService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public DatabaseSetupViewData GetDatabaseSetupViewData()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        var builder = string.IsNullOrWhiteSpace(connectionString)
            ? new SqlConnectionStringBuilder()
            : new SqlConnectionStringBuilder(connectionString);

        var server = builder.DataSource;
        var port = 1433;

        if (!string.IsNullOrWhiteSpace(server))
        {
            var parts = server.Split(',', 2);
            server = parts[0];
            if (parts.Length == 2 && int.TryParse(parts[1], out var parsedPort))
            {
                port = parsedPort;
            }
        }

        return new DatabaseSetupViewData
        {
            Server = string.IsNullOrWhiteSpace(server) ? "localhost" : server,
            Port = port,
            Database = string.IsNullOrWhiteSpace(builder.InitialCatalog) ? "EventHubDb" : builder.InitialCatalog,
            UserId = string.IsNullOrWhiteSpace(builder.UserID) ? "sa" : builder.UserID
        };
    }

    public string BuildConnectionString(DatabaseSetupViewData data)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = $"{data.Server},{data.Port}",
            InitialCatalog = data.Database,
            UserID = data.UserId,
            Password = data.Password,
            Encrypt = false,
            TrustServerCertificate = true,
            MultipleActiveResultSets = true
        };

        return builder.ConnectionString;
    }

    public void SaveConnectionString(string connectionString)
    {
        var path = Path.Combine(_environment.ContentRootPath, "appsettings.Local.json");
        LocalSettingsFile settings;

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            settings = JsonSerializer.Deserialize<LocalSettingsFile>(json) ?? new LocalSettingsFile();
        }
        else
        {
            settings = new LocalSettingsFile();
        }

        settings.ConnectionStrings["DefaultConnection"] = connectionString;

        var output = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, output);

        if (_configuration is IConfigurationRoot root)
        {
            root.Reload();
        }
    }

    private sealed class LocalSettingsFile
    {
        public Dictionary<string, string> ConnectionStrings { get; set; } = new();
    }
}
