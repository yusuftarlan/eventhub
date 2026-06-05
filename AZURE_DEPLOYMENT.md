# Azure Deployment Notes

This project is prepared for the simplest Azure production-style setup:

- Public web app: Azure App Service
- Private database: Azure SQL Database with Private Endpoint
- Private database access from web: App Service VNet Integration

## 1. Create Azure Resources

Create these resources in the same region:

- Resource Group: `rg-eventhub-prod`
- Virtual Network: `vnet-eventhub`
- App Service integration subnet: `snet-appservice-integration`
- Private endpoint subnet: `snet-private-endpoints`
- Azure SQL Server
- Azure SQL Database: `EventHubDb`
- App Service for the ASP.NET Core web app

Use Azure SQL Database instead of a self-hosted SQL Server container unless you explicitly need to manage SQL Server yourself.

## 2. Configure Azure SQL

Create the SQL Server and database, then add a Private Endpoint for the SQL Server.

Recommended final database posture:

- Public network access: Disabled
- Private Endpoint: Enabled
- Private DNS zone: Linked to `vnet-eventhub`

During first setup, you may temporarily allow public access from your own IP to inspect the database. Disable it after the App Service can connect privately.

## 3. Configure App Service Networking

Enable VNet Integration on the App Service.

Use:

```text
snet-appservice-integration
```

The SQL Private Endpoint should use:

```text
snet-private-endpoints
```

Do not use the same subnet for both.

## 4. Configure App Service Settings

Set these App Service application settings:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=tcp:<sql-server-name>.database.windows.net,1433;Initial Catalog=EventHubDb;User ID=<sql-user>;Password=<sql-password>;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True;Connection Timeout=30;
SetupAdmin__Email=<setup-admin-email>
SetupAdmin__Password=<strong-setup-admin-password>
```

If deploying the Docker container to App Service, also set:

```text
WEBSITES_PORT=8080
```

## 5. Deploy

The existing GitHub Actions workflow deploys the published ASP.NET Core app to Azure App Service.

Before pushing, confirm that local-only settings are not tracked:

```powershell
git ls-files appsettings.Local.json
```

The command should print nothing.

## 6. Verify

After deployment, check:

- App Service opens publicly.
- App Service logs do not show `Database is not available`.
- Azure SQL contains migrated tables.
- Login works with the seeded admin user:

```text
admin@eventhub.com
EventHub123!
```

If login fails with a database connection error, first inspect the App Service logs and verify the private endpoint, private DNS, and VNet Integration settings.
