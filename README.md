# 🏥 DiyetPro - Diyetisyen Hasta Takip Otomasyonu

<div align="center">

![Version](https://img.shields.io/badge/version-2.0-blue.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)
![.NET](https://img.shields.io/badge/.NET_Framework-4.8-purple.svg)
![DevExpress](https://img.shields.io/badge/DevExpress-25.1-orange.svg)
![MySQL](https://img.shields.io/badge/MySQL-8.4-blue.svg)

**Profesyonel Diyetisyen Hasta Takip ve Yönetim Sistemi**

[📖 Dokümantasyon](DOCS/README.md) | [🧪 Test Planı](DOCS/TEST_PLANI.md) | [📊 Final Rapor](DOCS/FINAL_RAPOR.md)

</div>

---

## 📋 Proje Özeti

DiyetPro, **nesne yönelimli tasarım** prensipleri ve **yazılım mühendisliği yöntemleri** kullanılarak geliştirilmiş, **9 adet akıllı algoritma** içeren profesyonel bir diyetisyen hasta takip otomasyonudur.

### ✨ Öne Çıkan Özellikler

- 🧮 **Akıllı Algoritmalar:** BMI, BMR, TDEE hesaplama, risk analizi
- 🔐 **Güvenli Kimlik Doğrulama:** PBKDF2 şifre hash, rol bazlı yetki
- 📊 **Görsel Raporlama:** Kilo trendi, diyet uyum grafikleri
- 💬 **Gerçek Zamanlı Mesajlaşma:** Diyetisyen-hasta iletişimi
- 🎯 **Hedef Takibi:** Su, kilo, adım, protein hedefleri
- 📅 **Randevu Yönetimi:** Takvim ve bildirim sistemi
- 🤖 **AI Asistan:** Yapay zeka destekli öneri sistemi (stub)

---

## 🎯 Akademik Kriterlere Uygunluk

| Kriter | Puan | Durum |
|--------|------|-------|
| Proje Analizi | 10 | ✅ [MALIYET_KESTIRIM.md](DOCS/MALIYET_KESTIRIM.md) |
| UseCase ve Sınıf Diyagramları | 10 | ✅ [USECASE](DOCS/USECASE_DIYAGRAMI.md), [SINIF](DOCS/SINIF_DIYAGRAMI.md) |
| Zamanında Teslim | 10 | ✅ |
| UI ve Kullanılabilirlik | 10 | ✅ Modern DevExpress UI |
| Kodlama ve Çıktı | 30 | ✅ 16,300+ satır kod |
| Test | 10 | ✅ [TEST_PLANI.md](DOCS/TEST_PLANI.md) - %96.5 başarı |
| Dokümantasyon | 10 | ✅ [DOCS Klasörü](DOCS/) |
| Veritabanı Tasarımı | 10 | ✅ [ER_DIYAGRAMI.md](DOCS/ER_DIYAGRAMI.md) |

---

## 🧠 Akıllı Algoritmalar (9 Adet)

| No | Algoritma | Tip | Formül/Açıklama |
|----|-----------|-----|-----------------|
| 1 | **BMI Hesaplama** | Hesaplama | `Kilo / (Boy/100)²` |
| 2 | **BMI Kategorizasyonu** | Karar Verme | Zayıf/Normal/Obez sınıflandırma |
| 3 | **BMR Hesaplama** | Hesaplama | Mifflin-St Jeor denklemi |
| 4 | **TDEE Hesaplama** | Hesaplama | BMR × Aktivite çarpanı |
| 5 | **İdeal Kilo Aralığı** | Hesaplama | BMI 18.5-24.9 aralığı |
| 6 | **İlerleme Yüzdesi** | Hesaplama | `(Güncel/Hedef) × 100` |
| 7 | **Diyet Uyum Oranı** | İstatistik | Öğün tamamlama analizi |
| 8 | **Risk Analizi** | Karar Verme | Hızlı kilo değişimi tespiti |
| 9 | **Kilo Trend Analizi** | İstatistik | Zaman serisi analizi |

---

## 🏗️ Teknoloji Yığını

| Bileşen | Teknoloji | Versiyon |
|---------|-----------|----------|
| **Platform** | Windows Forms | .NET Framework 4.8 |
| **Dil** | C# | 12.0 |
| **UI Framework** | DevExpress WinForms | 25.1.5 |
| **Veritabanı** | MySQL | 8.4.0 |
| **Mimari** | 4-Tier Layered | Domain/Repo/Service/Forms |
| **Güvenlik** | PBKDF2 | 10,000 iterations |

---

## 📁 Proje Yapısı

```
DiyetisyenOtomasyonu/
├── Domain/                 # Veri modelleri (19 entity)
│   ├── User.cs            # Temel kullanıcı
│   ├── Patient.cs         # Hasta (BMI, TDEE hesaplamaları)
│   ├── Doctor.cs          # Diyetisyen
│   └── ...
├── Infrastructure/
│   ├── Database/          # MySQL bağlantısı
│   ├── Repositories/      # Repository pattern (16 repo)
│   ├── Security/          # PBKDF2 hash, AuthContext
│   └── Services/          # İş mantığı (11 service)
├── Forms/
│   ├── Doctor/            # Diyetisyen formları (13)
│   ├── Patient/           # Hasta formları (9)
│   └── Login/             # Giriş formları
├── Shared/                # Ortak stiller, validasyon
├── DOCS/                  # 📚 Akademik Dokümantasyon
│   ├── README.md          # Doküman haritası
│   ├── FINAL_RAPOR.md     # Ana proje raporu
│   ├── USECASE_DIYAGRAMI.md
│   ├── SINIF_DIYAGRAMI.md
│   ├── ER_DIYAGRAMI.md
│   ├── MALIYET_KESTIRIM.md
│   └── TEST_PLANI.md
└── Program.cs             # Giriş noktası
```

---

## 🚀 Kurulum ve Çalıştırma

### Gereksinimler

- Windows 10/11
- Visual Studio 2022
- .NET Framework 4.8
- MySQL Server 8.4+
- DevExpress WinForms 25.1

### Kurulum Adımları

```bash
# 1. MySQL veritabanını hazırlayın
# App.config'de connection string düzenleyin

# 2. API Key yapılandırması (AI özellikleri için)
# Aşağıdaki dosyalarda "API_KEYINIZI_YAZIN" yerine kendi API key'inizi yazın:
# - Infrastructure/Services/AiAssistantService.cs
# - Forms/Doctor/FrmAIAnalysis.cs
# - Forms/Doctor/FrmGoalsNotes.cs

# 3. Visual Studio ile açın
# DiyetisyenOtomasyonu.sln

# 4. Build edin
msbuild DiyetisyenOtomasyonu.sln /p:Configuration=Debug

# 5. Çalıştırın
cd bin\Debug
DiyetisyenOtomasyonu.exe
```

### ⚙️ Yapılandırma

**Veritabanı Bağlantısı:**
- `Infrastructure/Database/DatabaseConfig.cs` dosyasında connection string'i düzenleyin

**API Key (AI Özellikleri için):**
- OpenRouter API key alın: https://openrouter.ai/
- Aşağıdaki dosyalarda `API_KEYINIZI_YAZIN` yerine kendi API key'inizi yazın:
  - `Infrastructure/Services/AiAssistantService.cs`
  - `Forms/Doctor/FrmAIAnalysis.cs`
  - `Forms/Doctor/FrmGoalsNotes.cs`

### 👤 Demo Hesapları

| Rol | Kullanıcı Adı | Şifre |
|-----|---------------|-------|
| 👨‍⚕️ Diyetisyen | doktor1 | 123456 |
| 👨‍⚕️ Diyetisyen | doktor2 | 123456 |
| 👤 Hasta | hasta1 | 123456 |
| 👤 Hasta | hasta2 | 123456 |

---

## 📊 Proje İstatistikleri

| Metrik | Değer |
|--------|-------|
| 📝 Kod Satırı | ~16,300 |
| 🏛️ Sınıf Sayısı | ~70 |
| 🖥️ Form Sayısı | 23 |
| 🗄️ Tablo Sayısı | 19 |
| 📦 Repository | 16 |
| ⚙️ Service | 11 |
| 🧮 Akıllı Algoritma | 9 |
| 🧪 Test Case | 87 |
| ✅ Test Başarısı | %96.5 |

---

## 🔒 Güvenlik Özellikleri

- ✅ **PBKDF2** ile şifre hash (10,000 iteration)
- ✅ Her kullanıcı için **benzersiz salt**
- ✅ **Rol bazlı yetkilendirme** (Doctor/Patient)
- ✅ **Oturum yönetimi** (AuthContext)
- ✅ **SQL Injection koruması**

---

## 📚 Dokümantasyon

Tüm akademik dokümanlar `DOCS/` klasöründe:

| Doküman | İçerik |
|---------|--------|
| [📖 FINAL_RAPOR.md](DOCS/FINAL_RAPOR.md) | Kapsamlı final raporu |
| [🎯 USECASE_DIYAGRAMI.md](DOCS/USECASE_DIYAGRAMI.md) | 25 use case analizi |
| [🏗️ SINIF_DIYAGRAMI.md](DOCS/SINIF_DIYAGRAMI.md) | OOP ve SOLID analizi |
| [🗄️ ER_DIYAGRAMI.md](DOCS/ER_DIYAGRAMI.md) | 19 tablo, 3NF |
| [💰 MALIYET_KESTIRIM.md](DOCS/MALIYET_KESTIRIM.md) | 554 işlev noktası |
| [🧪 TEST_PLANI.md](DOCS/TEST_PLANI.md) | 87 test case |

---

## 🎨 Ekran Görüntüleri

### Diyetisyen Paneli
- Modern sidebar navigasyon
- Hasta yönetimi (CRUD)
- Diyet planı oluşturma
- Grafik raporlar

### Hasta Paneli
- Haftalık menü görüntüleme
- Öğün tamamlama
- Hedef takibi
- İlerleme grafikleri

---

## 🔧 Geliştirme Standartları

### OOP Prensipleri
- ✅ Encapsulation, Inheritance, Polymorphism, Abstraction

### SOLID Prensipleri
- ✅ Single Responsibility
- ✅ Open/Closed
- ✅ Liskov Substitution
- ✅ Interface Segregation
- ✅ Dependency Inversion

### Design Patterns
- ✅ Repository Pattern
- ✅ Service Layer Pattern
- ✅ Singleton Pattern
- ✅ Template Method Pattern

---

## 📈 Gelecek Geliştirmeler

- [ ] .NET 8 migrasyonu
- [ ] Web versiyonu (ASP.NET Core)
- [ ] Mobil uygulama (MAUI)
- [ ] Gerçek AI entegrasyonu (Gemini API)
- [ ] PDF rapor oluşturma

---

## 📞 İletişim

**Proje Tipi:** Akademik Final Projesi  
**Tarih:** Ocak 2026  
**Versiyon:** 2.0 Final

---

<div align="center">

© 2026 DiyetPro - Tüm Hakları Saklıdır

</div>
