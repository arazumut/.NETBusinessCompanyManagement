# Şirket Yönetim Sistemi

Bu proje, şirketlerin insan kaynakları, proje yönetimi ve şirket içi operasyonlarını tek bir platformdan yönetebilmesini sağlayan modern bir ASP.NET Core MVC uygulamasıdır.

## Özellikler

- **Kullanıcı Yönetimi**
  - Rol tabanlı yetkilendirme (Admin, Yönetici, Çalışan)
  - Güvenli kimlik doğrulama
  - Personel profilleri

- **İnsan Kaynakları Yönetimi**
  - Personel bilgilerinin yönetimi
  - Departman ve pozisyon yönetimi
  - İzin talep ve onay sistemi

- **Proje Yönetimi**
  - Proje oluşturma ve takibi
  - Görev atama ve durum takibi
  - Proje zaman çizelgeleri

## Teknolojiler

- ASP.NET Core 8.0
- Entity Framework Core
- SQLite Veritabanı
- Identity Framework
- Bootstrap 5

## Kurulum

1. Projeyi klonlayın
   ```bash
   git clone https://github.com/kullanıcıadı/CompanyManagementSystem.git
   cd CompanyManagementSystem
   ```

2. Projeyi derleyin ve çalıştırın
   ```bash
   dotnet build
   dotnet run
   ```

3. Tarayıcınızda uygulamayı açın
   ```
   http://localhost:5011
   ```

## Varsayılan Kullanıcılar

Uygulama ilk çalıştırıldığında otomatik olarak aşağıdaki kullanıcı oluşturulur:

- **Admin**
  - Email: admin@company.com
  - Şifre: Admin@123

## Veritabanı Şeması

Uygulama aşağıdaki temel veritabanı yapısına sahiptir:

- **Employees**: Personel bilgileri
- **Departments**: Departman bilgileri
- **Positions**: Pozisyon bilgileri
- **Leaves**: İzin talepleri
- **Projects**: Proje bilgileri
- **WorkTasks**: Görevler

## Ekran Görüntüleri

*(Uygulama ekran görüntüleri burada yer alacak)*

## Gelecek Özellikler

- Raporlama sistemi
- Ücret yönetimi
- Performans değerlendirme
- Mobil uygulama desteği

