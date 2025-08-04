# 🚌 Otobüs Bilet Sistemi

Modern ve kullanıcı dostu bir otobüs bilet rezervasyon sistemi. ASP.NET Core 9.0 ve Entity Framework Core ile geliştirilmiştir.

## 📋 İçindekiler

## 🎯 Proje Özeti

Bu proje, otobüs firmaları için kapsamlı bir bilet satış ve yönetim sistemidir. Kullanıcılar sefer aramadan bilet satın alımına kadar tüm süreçleri dijital ortamda gerçekleştirebilir. Admin paneli ile firma yöneticileri tüm operasyonları yönetebilir ve detaylı raporlar alabilir.

## Özellikler

### 👥 Kullanıcı Özellikleri

- 🔐 **Kullanıcı Yönetimi**: Kayıt olma, giriş yapma, profil yönetimi
- 🔍 **Sefer Arama**: Nereden-nereye, tarih ve yolcu sayısına göre sefer arama
- 💺 **Koltuk Seçimi**: Interaktif koltuk haritası ile koltuk seçimi
- 👤 **Yolcu Bilgileri**: Her yolcu için detaylı bilgi girişi
- 💳 **Online Ödeme**: Güvenli kredi kartı ile ödeme
- 🎫 **Bilet Yönetimi**: Satın alınan biletleri görüntüleme ve yönetme
- 📧 **E-posta Bildirimleri**: Bilet ve ödeme bilgilendirmeleri
- 🔒 **Şifre Sıfırlama**: E-posta ile güvenli şifre sıfırlama

### 🛠️ Admin Panel Özellikleri

- 📊 **Dashboard**: Genel sistem özeti ve önemli metriklər
- 🚌 **Otobüs Yönetimi**: Otobüs ekleme, düzenleme, silme
- 🗺️ **Güzergah Yönetimi**: Güzergah tanımlama ve yönetimi
- 🕐 **Sefer Yönetimi**: Sefer programlama ve düzenleme
- 🎫 **Bilet Yönetimi**: Tüm bilet işlemlerini görüntüleme ve yönetme
- 👥 **Kullanıcı Yönetimi**: Kullanıcı hesaplarını yönetme
- 📈 **Raporlama Sistemi**:
  - 💰 Gelir raporları
  - 📊 Satış analizleri
  - 🎯 Sefer performans raporları
  - 👥 Müşteri analizleri
  - ❌ İptal ve iade raporları

### 🔧 Teknik Özellikler

- 🏗️ **N-Tier Architecture**: Katmanlı mimari yapısı
- 🔄 **Repository Pattern**: Veri erişim katmanı soyutlaması
- 🆔 **Identity Framework**: Güvenli kullanıcı yönetimi
- 📱 **Responsive Design**: Mobil uyumlu arayüz
- 🛡️ **Güvenlik**: Authentication ve Authorization
- 📊 **Session Yönetimi**: Güvenli oturum yönetimi
- 🗄️ **Oracle Database**: Güçlü veritabanı desteği

## 🛠️ Teknolojiler

### Backend

- **Framework**: ASP.NET Core 9.0
- **Database**: Oracle Database
- **ORM**: Entity Framework Core 9.0
- **Authentication**: ASP.NET Core Identity
- **API**: RESTful API with Swagger
- **Mapping**: AutoMapper
- **Language**: C# 12

### Frontend

- **Framework**: ASP.NET Core MVC
- **Styling**: Bootstrap 5
- **JavaScript**: jQuery
- **Icons**: Font Awesome
- **UI Components**: Custom responsive components

### Tools & Libraries

- **Development**: Visual Studio 2022
- **Package Manager**: NuGet
- **Email Service**: SMTP
- **Validation**: Data Annotations
- **Logging**: ILogger
- **Session**: In-Memory Cache

## 🏗️ Proje Mimarisi

```
OtobusBiletSistemi/
├── 📁 OtobusBiletSistemi.Core/          # Core katmanı
│   ├── 📁 Data/                         # Veritabanı context
│   ├── 📁 Entities/                     # Varlık sınıfları
│   ├── 📁 Interfaces/                   # Arayüzler
│   └── 📁 Repositories/                 # Repository implementasyonları
├── 📁 OtobusBiletSistemi.Web/           # Web uygulaması
│   ├── 📁 Areas/Admin/                  # Admin paneli
│   ├── 📁 Controllers/                  # MVC Controllers
│   ├── 📁 Models/                       # View Models
│   ├── 📁 Views/                        # Razor Views
│   ├── 📁 Services/                     # Business services
│   └── 📁 wwwroot/                      # Statik dosyalar
├── 📁 OtobusBiletSistemi.API/           # RESTful API
│   └── 📁 Controllers/                  # API Controllers
└── 📁 OtobusBiletSistemi.Mobile/        # Mobile uygulama (gelecek)
```

### Katmanlı Mimari

- **Presentation Layer**: Web UI ve API endpoints
- **Business Layer**: Business logic ve services
- **Data Access Layer**: Repository pattern ile veri erişimi
- **Database Layer**: Oracle Database

## 🚀 Kurulum

### Gereksinimler

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Oracle Database](https://www.oracle.com/database/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (önerilen)

### Adım Adım Kurulum

1. **Projeyi klonlayın**
   
   ```bash
   git clone https://github.com/[username]/OtobusBiletSistemi.git
   cd OtobusBiletSistemi
   ```

2. **Bağımlılıkları yükleyin**
   
   ```bash
   dotnet restore
   ```

3. **Veritabanı bağlantısını yapılandırın**
   `appsettings.json` dosyasında Oracle connection string'ini güncelleyin:
   
   ```json
   {
   "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost:1521/XE;User Id=your_user;Password=your_password;"
   }
   }
   ```

4. **Veritabanı migration'larını çalıştırın**
   
   ```bash
   dotnet ef database update --project OtobusBiletSistemi.Core
   ```

5. **Projeyi çalıştırın**
   
   ```bash
   # Web uygulaması için
   cd OtobusBiletSistemi.Web
   dotnet run
   ```

# API için

cd OtobusBiletSistemi.API
dotnet run

```
6. **Tarayıcıda açın**
- Web App: `https://localhost:5001`
- API: `https://localhost:5003`
- Swagger UI: `https://localhost:5003/swagger`

## 💻 Kullanım

### Kullanıcı İşlemleri

1. **Kayıt Olma**
   - Ana sayfadan "Kayıt Ol" linkine tıklayın
   - Gerekli bilgileri doldurun
   - E-posta doğrulaması yapın

2. **Sefer Arama**
   - Ana sayfada nereden/nereye bilgilerini girin
   - Tarih ve yolcu sayısını seçin
   - "Sefer Ara" butonuna tıklayın

3. **Bilet Satın Alma**
   - Uygun seferi seçin
   - Koltukları seçin
   - Yolcu bilgilerini girin
   - Ödeme bilgilerini girin
   - Satın almayı tamamlayın

### Admin İşlemleri

1. **Admin Girişi**
   - `/Admin/Auth/Login` adresinden giriş yapın
   - Admin hesabı ile giriş yapın

2. **Otobüs Ekleme**
   - Admin panel > Otobüs Yönetimi
   - "Yeni Otobüs" butonuna tıklayın
   - Plaka, tip ve koltuk sayısını girin

3. **Sefer Oluşturma**
   - Admin panel > Sefer Yönetimi
   - "Yeni Sefer" butonuna tıklayın
   - Otobüs, güzergah, tarih ve fiyat bilgilerini girin

## 📡 API Dokümantasyonu

API endpoint'leri `/swagger` adresinden incelenebilir.

### Ana Endpoint'ler

#### Seferler
```http
GET /api/sefer                    # Tüm seferler
GET /api/sefer/{id}              # Sefer detayı
POST /api/sefer                  # Yeni sefer
PUT /api/sefer/{id}              # Sefer güncelleme
DELETE /api/sefer/{id}           # Sefer silme
```

#### Biletler

```http
GET /api/bilet                   # Tüm biletler
GET /api/bilet/{id}             # Bilet detayı
POST /api/bilet                 # Yeni bilet
PUT /api/bilet/{id}             # Bilet güncelleme
DELETE /api/bilet/{id}          # Bilet silme
```

#### Otobüsler

```http
GET /api/otobus                 # Tüm otobüsler
GET /api/otobus/{id}           # Otobüs detayı
POST /api/otobus               # Yeni otobüs
PUT /api/otobus/{id}           # Otobüs güncelleme
DELETE /api/otobus/{id}        # Otobüs silme
```

## 🗄️ Veritabanı Şeması

### Ana Tablolar

#### Yolcu (YolcuUser)

- **YolcuID**: Primary Key
- **TCNo**: T.C. Kimlik Numarası
- **Ad**: Yolcu adı
- **Soyad**: Yolcu soyadı
- **TelNo**: Telefon numarası
- **Email**: E-posta adresi

#### Otobus

- **OtobusID**: Primary Key
- **Plaka**: Otobüs plakası
- **OtobusTipi**: Otobüs tipi (2+1, 2+2 vb.)
- **KoltukSayısı**: Toplam koltuk sayısı

#### Guzergah

- **GuzergahID**: Primary Key
- **Nereden**: Kalkış noktası
- **Nereye**: Varış noktası
- **Mesafe**: Km cinsinden mesafe

#### Sefer

- **SeferID**: Primary Key
- **OtobusID**: Foreign Key (Otobus)
- **GuzergahID**: Foreign Key (Guzergah)
- **Tarih**: Sefer tarihi
- **Saat**: Sefer saati
- **Kalkis**: Kalkış noktası
- **Varis**: Varış noktası
- **Fiyat**: Bilet fiyatı

#### Koltuk

- **KoltukID**: Primary Key
- **OtobusID**: Foreign Key (Otobus)
- **KoltukNo**: Koltuk numarası

#### Bilet

- **BiletID**: Primary Key
- **SeferID**: Foreign Key (Sefer)
- **KoltukID**: Foreign Key (Koltuk)
- **YolcuID**: Foreign Key (Yolcu)
- **OdemeID**: Foreign Key (Odeme)
- **BiletTarihi**: Bilet oluşturma tarihi
- **BiletDurumu**: Bilet durumu (Aktif, İptal vb.)
- **BiletNo**: Benzersiz bilet numarası

#### Odeme

- **OdemeID**: Primary Key
- **YolcuID**: Foreign Key (Yolcu)
- **OdemeTarihi**: Ödeme tarihi
- **OdemeTutari**: Ödeme tutarı
- **OdemeYontemi**: Ödeme yöntemi
- **OdemeDurumu**: Ödeme durumu
- **KartSahibiAdi**: Kart sahibi adı
- **KartNumarasi**: Maskelenmiş kart numarası
- **BiletSayisi**: Satın alınan bilet sayısı
- 

## 🔧 Geliştirme

### Test Etme

```bash
# Unit testleri çalıştır
dotnet test

# Test coverage raporu
dotnet test --collect:"XPlat Code Coverage"
```

### Build ve Deploy

```bash
# Production build
dotnet publish -c Release

# Docker ile deploy
docker build -t otobus-bilet-sistemi .
docker run -p 8080:80 otobus-bilet-sistemi
```



## 📝 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakınız.

# 