# DiyetPro - Diyetisyen Hasta Takip Otomasyonu

<div align="center">

![Version](https://img.shields.io/badge/version-2.1-blue.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)
![.NET](https://img.shields.io/badge/.NET_Framework-4.8-purple.svg)
![DevExpress](https://img.shields.io/badge/DevExpress-25.1-orange.svg)
![MySQL](https://img.shields.io/badge/MySQL-8.4-blue.svg)

**Profesyonel Diyetisyen Hasta Takip ve Yonetim Sistemi**

</div>

---

## Proje Hakkinda

DiyetPro, diyetisyenlerin hastalarini etkili bir sekilde takip edebilmesi icin gelistirilmis kapsamli bir masaustu uygulamasidir. Katmanli mimari, Repository ve Service Layer tasarim kaliplari kullanilarak olusturulmustur.

### Temel Ozellikler

- **Hasta Yonetimi:** Hasta kaydi, profil yonetimi, BMI/BMR/TDEE hesaplamalari
- **Diyet Planlama:** Haftalik/gunluk diyet planlari, ogun kutuphanesi, alternatif ogun onerileri
- **Kilo Takibi:** Kilo giris gecmisi, trend analizi, ilerleme grafikleri
- **Hedef Takibi:** Su, kilo, adim, protein hedefleri ve ilerleme izleme
- **Randevu Yonetimi:** Takvim ve bildirim sistemi
- **Mesajlasma:** Diyetisyen-hasta iletisim modulu
- **Raporlama:** Gorsel grafikler, istatistikler ve analiz ekranlari
- **AI Destekli Oneriler:** OpenRouter API ile yapay zeka destekli analiz ve oneri sistemi
- **Guvenlik:** PBKDF2 sifre hashleme, rol bazli yetkilendirme

---

## Teknoloji Yigini

| Bilesen | Teknoloji |
|---------|-----------|
| Platform | Windows Forms (.NET Framework 4.8) |
| Dil | C# |
| UI Framework | DevExpress WinForms 25.1 |
| Veritabani | MySQL 8.4 |
| Mimari | 4 Katmanli (Domain / Repository / Service / Presentation) |
| Guvenlik | PBKDF2 (10.000 iterasyon) |
| AI | OpenRouter API (Gemini) |

---

## Proje Yapisi

```
DiyetisyenOtomasyonu/
├── Domain/                          # Veri modelleri
│   ├── User.cs, Patient.cs, Doctor.cs
│   ├── Meal.cs, MealItem.cs, DietWeek.cs, DietDay.cs
│   ├── Goal.cs, Note.cs, Message.cs, Appointment.cs
│   ├── WeightEntry.cs, BodyMeasurement.cs, ExerciseTask.cs
│   ├── AIAnalysis.cs, Badge.cs, Enums.cs
│   └── ...
├── Infrastructure/
│   ├── Database/                    # Veritabani baglantisi ve baslatma
│   ├── Repositories/               # Repository Pattern (16 repository)
│   ├── Services/                    # Is mantigi servisleri (15+ servis)
│   ├── Security/                    # Kimlik dogrulama ve sifreleme
│   ├── DI/                          # Dependency Injection container
│   ├── Configuration/               # Uygulama yapilandirmasi
│   └── Exceptions/                  # Merkezi hata yonetimi
├── Forms/
│   ├── Doctor/                      # Diyetisyen paneli formlari
│   ├── Patient/                     # Hasta paneli formlari
│   └── Login/                       # Giris ve kayit formlari
├── Shared/                          # Ortak bilesenler (tema, sidebar, validasyon)
├── Bootstrap/                       # Tema baslangic ayarlari
├── DOCS/                            # Proje dokumantasyonu
└── Program.cs                       # Uygulama giris noktasi
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

1. **Depoyu klonlayin:**
   ```bash
   git clone https://github.com/sudenurozturkk/DiyetisyenOtomasyonu.git
   ```

2. **MySQL veritabanini hazirlayin:**
   - MySQL Server'in calistigindan emin olun.
   - `App.config` dosyasinda connection string'i kendi ortaminiza gore duzenleyin.
   - Uygulama ilk calistirmada tablolari otomatik olusturur.

3. **AI ozellikleri icin (istege bagli):**
   - [OpenRouter](https://openrouter.ai/) uzerinden API key alin.
   - `App.config` dosyasindaki `OpenRouterApiKey` degerini kendi key'inizle degistirin.

4. **Projeyi calistirin:**
   - `DiyetisyenOtomasyonu.sln` dosyasini Visual Studio ile acin.
   - `F5` ile build edip calistirin.

### Demo Hesabi

| Rol | Kullanici Adi | Sifre |
|-----|---------------|-------|
| Diyetisyen | whodenur | 12345678 |

> Yeni diyetisyen hesabi olusturmak icin giris ekranindaki **Kayit Ol** butonunu kullanabilirsiniz.
> Uygulama ilk calistiginda 15 ornek hasta otomatik olarak olusturulur.

---

## Mimari ve Tasarim

### Katmanli Mimari

- **Domain:** Entity siniflari ve is alani modelleri
- **Infrastructure:** Veritabani erisimi, servisler, guvenlik, DI container
- **Forms:** Windows Forms kullanici arayuzu
- **Shared:** Uygulamanin genelinde kullanilan yardimci bilesenler

### Uygulanan Tasarim Kaliplari

- **Repository Pattern** - Veri erisim soyutlamasi
- **Service Layer** - Is mantigi kapsulleme
- **Singleton Pattern** - Veritabani baglantisi ve DI container
- **Dependency Injection** - Constructor injection ile bagimlilik yonetimi

### Akilli Algoritmalar

| Algoritma | Aciklama |
|-----------|----------|
| BMI Hesaplama | Vucut kutle indeksi |
| BMI Kategorizasyonu | Zayif / Normal / Fazla kilolu / Obez siniflandirma |
| BMR Hesaplama | Mifflin-St Jeor denklemi ile bazal metabolizma |
| TDEE Hesaplama | BMR x Aktivite carpani |
| Ideal Kilo Araligi | BMI 18.5-24.9 bazli hesaplama |
| Ilerleme Yuzdesi | Hedef bazli ilerleme hesabi |
| Diyet Uyum Orani | Ogun tamamlama analizi |
| Risk Analizi | Hizli kilo degisimi tespiti |
| Kilo Trend Analizi | Zaman serisi bazli trend |

---

## Dokumantasyon

Detayli proje dokumantasyonu `DOCS/` klasorunde bulunmaktadir:

| Dokuman | Icerik |
|---------|--------|
| [FINAL_RAPOR.md](DOCS/FINAL_RAPOR.md) | Kapsamli proje raporu |
| [USECASE_DIYAGRAMI.md](DOCS/USECASE_DIYAGRAMI.md) | Use Case analizi |
| [SINIF_DIYAGRAMI.md](DOCS/SINIF_DIYAGRAMI.md) | Sinif diyagramlari ve OOP analizi |
| [ER_DIYAGRAMI.md](DOCS/ER_DIYAGRAMI.md) | Veritabani sema tasarimi |
| [TEST_PLANI.md](DOCS/TEST_PLANI.md) | Test plani ve sonuclari |
| [AI_INTEGRATION.md](DOCS/AI_INTEGRATION.md) | AI entegrasyon dokumantasyonu |

---

## Guvenlik

- PBKDF2 ile sifre hashleme (10.000 iterasyon, benzersiz salt)
- Rol bazli yetkilendirme (Diyetisyen / Hasta)
- Oturum yonetimi (AuthContext)
- Parametreli SQL sorgulari ile SQL Injection korunmasi
- API key'ler ortam degiskenleri veya yapilandirma dosyasindan okunur

---

## Lisans

Bu proje egitim ve gosterim amaciyla gelistirilmistir.

---

<div align="center">
  2026 DiyetPro
</div>
