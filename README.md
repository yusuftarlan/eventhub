# EventHub - Etkinlik & Workshop Yönetim Sistemi

ASP.NET Core MVC ile geliştirilmiş, Identity ve Entity Framework Core Code-First kullanan etkinlik yönetim sistemi.

## Özellikler

- ASP.NET Core MVC ve Bootstrap 5 arayüz
- ASP.NET Core Identity ile üyelik sistemi
- Role-based authorization: Admin, Organizer, User
- EF Core Code-First DbContext ve migration yapısı
- Docker MSSQL ile lokal geliştirme desteği
- Otomatik migration ve seed data
- Etkinlik, kategori, katılım ve favori yönetimi
- Organizer sahiplik kontrolü
- Kontenjan doluysa katılım engeli
- Aynı etkinliğe ikinci kez katılım engeli
- Geçmiş tarihli etkinliğe katılım engeli
- Etkinlik kapasitesini mevcut katılımcı sayısının altına düşürmeme
- Kategori adı duplicate kontrolü
- Partial View ile etkinlik kartları: `Views/Shared/_EventCard.cshtml`
- ViewModel ve DataAnnotations validation kullanımı
- Service katmanı ile temel iş kuralları: `Services/IEventService.cs`, `Services/EventService.cs`

## Kurulum

1. Proje klasörüne gidin:

```powershell
cd E:\Yazilim_Projeleri\.Net_projeleri\EventHubP
```

2. SQL Server container'ını başlatın.

Makinenizde SQL Server kurulu değilse Docker kullanmanız yeterlidir:

```powershell
docker run -e "ACCEPT_EULA=Y" `
  -e "MSSQL_SA_PASSWORD=EventHub123!" `
  -p 1433:1433 `
  --name eventhub-sql `
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Container daha önce oluşturulduysa tekrar oluşturmak yerine başlatın:

```powershell
docker start eventhub-sql
```

Container durumunu kontrol edin:

```powershell
docker ps
```

3. Bağlantı bilgisini kontrol edin.

`appsettings.json` içindeki connection string Docker SQL Server'a göre ayarlıdır:

```json
"DefaultConnection": "Server=localhost,1433;Database=EventHubDb;User Id=sa;Password=EventHub123!;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

> Not: `Encrypt=False` lokal geliştirme içindir. Production ortamında daha güvenli bağlantı ayarları kullanılmalıdır.

4. Projeyi çalıştırın:

```powershell
dotnet run
```

Uygulama launch ayarlarına göre şu adreslerde açılır:

```text
http://localhost:5264
https://localhost:7058
```

## Migration ve Seed

Uygulama başlangıcında `Program.cs` içinden `DbInitializer.SeedAsync(app.Services)` çalışır.

Bu akış içinde:

- `Database.MigrateAsync()` ile migration'lar uygulanır.
- `EventHubDb` veritabanı yoksa oluşturulur.
- Identity rolleri oluşturulur: Admin, Organizer, User.
- Test kullanıcıları oluşturulur.
- Başlangıç kategorileri ve örnek etkinlikler eklenir.

Bu yüzden ilk çalıştırmada ayrıca `dotnet ef database update` komutu çalıştırmanız gerekmez. Yine de manuel migration işlemleri için referans komutlar:

```powershell
dotnet ef migrations add MigrationAdi
dotnet ef database update
dotnet ef migrations remove
```

## Test Kullanıcıları

Tüm test kullanıcılarının varsayılan şifresi:

```text
EventHub123!
```

| Rol | E-posta |
| --- | --- |
| Admin | admin@eventhub.com |
| Organizer | organizer@eventhub.com |
| Organizer | organizer2@eventhub.com |
| Organizer | organizer3@eventhub.com |
| User | user@eventhub.com |
| User | user2@eventhub.com |
| User | user3@eventhub.com |
| User | user4@eventhub.com |

## Sayfalar

- Public:
  - `/`
  - `/Events`
  - `/Events/Details/{id}`
  - `/Account/Login`
  - `/Account/Register`

- User:
  - `/MyEvents` - Etkinliklerim
  - `/Favorites` - Favorilerim
  - Etkinliğe katılma, etkinlikten ayrılma ve favori işlemleri

- Organizer:
  - `/Organizer/Events`
  - `/Organizer/Events/Create`
  - `/Organizer/Events/Edit/{id}`
  - `/Organizer/Events/Delete/{id}`
  - `/Organizer/Events/Participants/{id}`

- Admin:
  - `/Admin/Dashboard`
  - `/Admin/Events`
  - `/Admin/Categories`
  - `/Admin/Users`

Admin etkinlik yönetimi `/Admin/Events` üzerinden listelenir. Düzenleme, silme ve katılımcı görme işlemleri mevcut organizer route'ları üzerinden yapılır; Admin rolü bu işlemlere yetkilidir.

## Mimari Notlar

- `ApplicationDbContext`, Identity tabloları ile domain tablolarını aynı DbContext içinde yönetir.
- `DbInitializer`, migration ve başlangıç verisi oluşturma sorumluluğunu taşır.
- `EventsController` HTTP akışını, redirect ve TempData mesajlarını yönetir.
- `EventService`, katılım, ayrılma, favori ve kapasite gibi temel iş kurallarını yönetir.
- ViewModel'ler form ve liste ekranları için entity'lerden ayrı veri taşıma modeli olarak kullanılır.
