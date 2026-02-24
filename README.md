# DiyetPro - Diyetisyen Hasta Takip Otomasyonu

<div align="center">

![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)
![.NET](https://img.shields.io/badge/.NET_Framework-4.8-purple.svg)
![DevExpress](https://img.shields.io/badge/DevExpress-25.1-orange.svg)
![MySQL](https://img.shields.io/badge/MySQL-8.4-blue.svg)
![C#](https://img.shields.io/badge/C%23-12.0-green.svg)

</div>

---

## Proje Hakkinda

DiyetPro, diyetisyenlerin hastalarini takip edebilmesi icin gelistirilmis bir Windows masaustu uygulamasidir. Katmanli mimari (Domain, Infrastructure, Forms, Shared) uzerine kurulmustur.

**Temel Ozellikler:**

- Hasta kayit, profil yonetimi, BMI / BMR / TDEE hesaplamalari
- Haftalik ve gunluk diyet plani olusturma, ogun kutuphanesi
- Kilo takibi, trend analizi, ilerleme grafikleri
- Hedef takibi (su, kilo, adim, egzersiz)
- Randevu yonetimi
- Diyetisyen-hasta mesajlasma
- Gorsel raporlama ve istatistik ekranlari
- OpenRouter API uzerinden AI destekli oneri sistemi
- Rol bazli yetkilendirme (Diyetisyen / Hasta)

---

## Teknoloji Yigini

| Bilesen | Teknoloji |
|---------|-----------|
| Platform | Windows Forms (.NET Framework 4.8) |
| Dil | C# |
| UI | DevExpress WinForms 25.1 |
| Veritabani | MySQL 8.4 |
| Sifre Guvenligi | SHA-256 + Salt |
| AI Entegrasyonu | OpenRouter API |

---

## Proje Yapisi

```
DiyetisyenOtomasyonu/
├── Domain/                     # 20 entity sinifi
├── Infrastructure/
│   ├── Database/               # Baglanti yonetimi, tablo olusturma
│   ├── Repositories/           # 16 repository + BaseRepository
│   ├── Services/               # 19 servis sinifi
│   ├── Security/               # Sifre hashleme, oturum yonetimi
│   ├── DI/                     # Dependency Injection container
│   ├── Configuration/          # Merkezi yapilandirma (App.config)
│   └── Exceptions/             # Merkezi hata yonetimi
├── Forms/
│   ├── Doctor/                 # 14 diyetisyen paneli formu
│   ├── Patient/                # 10 hasta paneli formu
│   └── Login/                  # 3 giris/kayit formu
├── Shared/                     # Tema, sidebar, toast, validasyon
├── Bootstrap/                  # DevExpress tema ayarlari
├── DOCS/                       # Proje dokumantasyonu
└── Program.cs                  # Uygulama giris noktasi
```

---

## Kurulum

### Gereksinimler

- Windows 10/11
- Visual Studio 2022
- .NET Framework 4.8
- MySQL Server 8.4+
- DevExpress WinForms 25.1

### Adimlar

1. Depoyu klonlayin:
   ```
   git clone https://github.com/sudenurozturkk/DiyetisyenOtomasyonu.git
   ```

2. `App.config` dosyasinda veritabani baglanti bilgilerini kendi ortaminiza gore duzenleyin.
   Uygulama ilk calistirmada tablolari otomatik olusturur.

3. AI ozellikleri icin (istege bagli): `App.config` dosyasindaki `OpenRouterApiKey` alanina
   [OpenRouter](https://openrouter.ai/) uzerinden aldiginiz API key'i yazin.

4. `DiyetisyenOtomasyonu.sln` dosyasini Visual Studio ile acip `F5` ile calistirin.

### Giris Bilgileri

| Rol | Kullanici Adi | Sifre |
|-----|---------------|-------|
| Diyetisyen | whodenur | 12345678 |
| Hasta (ornek) | ahmetyilmaz | 12345678 |

Uygulama ilk calistirmada 15 ornek hasta otomatik olusturulur (sifre: 12345678).
Yeni diyetisyen hesabi icin giris ekranindaki **Kayit Ol** butonunu kullanabilirsiniz.

---

## Mimari

### Katmanlar

- **Domain**: Entity siniflari (Patient, Doctor, Meal, Goal, vb.)
- **Infrastructure**: Veritabani erisimi, servisler, guvenlik, DI, yapilandirma
- **Forms**: Windows Forms arayuzu (Diyetisyen paneli, Hasta paneli, Giris)
- **Shared**: Tema yonetimi, sidebar, toast bildirimi, input validasyonu

### Tasarim Kaliplari

- Repository Pattern
- Service Layer
- Singleton (DatabaseConfig, AppConfiguration, ServiceContainer)
- Dependency Injection (constructor injection)

### Veritabani

MySQL uzerinde 18 tablo: Users, Patients, Doctors, Goals, Notes, Messages,
WeightEntries, DietWeeks, DietDays, MealItems, BodyMeasurements, ExerciseTasks,
Appointments, MealFeedback, Meals, PatientMealAssignments, AiChatLogs, Badges.

Tum tablolar `DatabaseInitializer.cs` tarafindan `CREATE TABLE IF NOT EXISTS` ile otomatik olusturulur.

### Hesaplama Algoritmalari

| Algoritma | Aciklama |
|-----------|----------|
| BMI | Vucut kutle indeksi: kilo / (boy/100)^2 |
| BMR | Mifflin-St Jeor denklemi ile bazal metabolizma |
| TDEE | BMR x aktivite carpani |
| Ideal Kilo | BMI 18.5-24.9 araligina gore |
| Risk Analizi | Hizli kilo degisimi tespiti |
| Diyet Uyum | Ogun tamamlama orani |
| Kilo Trend | Zaman serisi bazli trend analizi |

---

## Dokumantasyon

| Dokuman | Icerik |
|---------|--------|
| [FINAL_RAPOR.md](DOCS/FINAL_RAPOR.md) | Proje raporu |
| [USECASE_DIYAGRAMI.md](DOCS/USECASE_DIYAGRAMI.md) | Use Case analizi |
| [SINIF_DIYAGRAMI.md](DOCS/SINIF_DIYAGRAMI.md) | Sinif diyagramlari |
| [ER_DIYAGRAMI.md](DOCS/ER_DIYAGRAMI.md) | Veritabani sema tasarimi |
| [TEST_PLANI.md](DOCS/TEST_PLANI.md) | Test plani |
| [AI_INTEGRATION.md](DOCS/AI_INTEGRATION.md) | AI entegrasyon detaylari |

---

## Guvenlik

- SHA-256 ile sifre hashleme (statik salt)
- Rol bazli yetkilendirme (Diyetisyen / Hasta)
- Oturum yonetimi (AuthContext)
- Parametreli SQL sorgulari
- API key'ler `App.config` veya ortam degiskenlerinden okunur

---

## Lisans

Bu proje egitim amaciyla gelistirilmistir.
