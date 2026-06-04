# Projede Olması Gereken Olmazsa Olmaz Teknik Katmanlar

- Veritabanı:
  Orta ve kapsamlı projelerde EF Core Code-First yaklaşımı ile DbContext ve Migrations kullanımı. Daha küçük ölçekli projelerde, veriler JSON dosyasından okunup JSON dosyasına
  yazılabilecek şekilde geliştirilebilir. Tercih size kalmış.
  JSON kullanılsa dahi temel CRUD işlemleri uygulanmalıdır
- Authentication &amp; Authorization:
  o Kullanıcı girişi ve kayıt işlemleri için ASP.NET Core Identity veya benzeri bir kimlik
  doğrulama yapısı kullanılabilir.
  o Alternatif olarak session/cookie tabanlı basit login sistemi geliştirilebilir.
  o Role-based (Admin/User) yetkilendirme.En az iki farklı rol veya kullanıcı tipi
  tanımlanmalıdır. Örnek: Admin / User, Yönetici / Üye, Öğretmen / Öğrenci

- UI &amp; Layout:
  o \_Layout.cshtml kullanarak ortak Header/Footer yapısı.
  o ViewBag, ViewData veya tercihen ViewModel kullanımı (Veri taşıma için).
- Validations:
  o Data Annotations ile form kontrolü (Required, StringLength vb.).

## Proje: Etkinlik &amp; Workshop Yönetim Sistemi (EventHub)

## Özellikler:

- Kullanıcı: Etkinlikleri görüntüleme, kayıt olma (Join), kendi katıldığı etkinlikleri
  listeleme.
- Admin/Organizatör: Etkinlik oluşturma, kontenjan sınırı koyma, katılımcı listesini
  görme.

## Teknik Detay:

Identity ile Role-based Authorization (Admin vs User), Partial View ile etkinlik kartları.
