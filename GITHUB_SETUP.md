# GitHub'a Yükleme Kılavuzu

## ✅ Hazırlık Tamamlandı

Proje GitHub'a yüklenmeye hazır! Aşağıdaki değişiklikler yapıldı:

### 🔒 Güvenlik
- ✅ API key'ler placeholder ile değiştirildi (`API_KEYINIZI_YAZIN`)
- ✅ Dosyalar GitHub'a yüklenecek ama API key'ler görünmeyecek
- ✅ `.gitignore` güncellendi
- ✅ SRS dokümanı `.gitignore`'a eklendi

### 📝 Yapılan Değişiklikler

1. **API Key'ler Placeholder ile Değiştirildi:**
   - `Infrastructure/Services/AiAssistantService.cs` - `API_KEYINIZI_YAZIN`
   - `Forms/Doctor/FrmAIAnalysis.cs` - `API_KEYINIZI_YAZIN`
   - `Forms/Doctor/FrmGoalsNotes.cs` - `API_KEYINIZI_YAZIN`
   - Bu dosyalar GitHub'a yüklenecek ama API key'ler görünmeyecek

2. **.gitignore Güncellendi:**
   - SRS dokümanı eklendi
   - API key dosyaları eklendi
   - Python script'leri eklendi
   - Geçici dosyalar eklendi

3. **README.md Güncellendi:**
   - API key yapılandırma bilgileri eklendi
   - Kurulum adımları güncellendi

## 🚀 GitHub'a Yükleme Adımları

### 1. GitHub Repository Oluşturma

```bash
# GitHub'da yeni repository oluşturun
# Repository adı: DiyetPro veya DiyetisyenOtomasyonu
# Public veya Private seçin
```

### 2. Git İlk Kurulum (Eğer yapılmadıysa)

```bash
# Git kullanıcı bilgilerini ayarlayın
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

### 3. Projeyi Git Repository'ye Dönüştürme

```bash
# Proje klasörüne gidin
cd C:\Users\Administrator\Desktop\Projelerim\DiyetisyenOtomasyonu

# Git repository başlatın
git init

# Tüm dosyaları ekleyin
git add .

# İlk commit
git commit -m "Initial commit: DiyetPro - Diyetisyen Hasta Takip Otomasyonu"

# GitHub repository URL'ini ekleyin
git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git

# Branch adını main olarak ayarlayın
git branch -M main

# GitHub'a yükleyin
git push -u origin main
```

### 4. Alternatif: GitHub Desktop Kullanımı

1. GitHub Desktop'ı açın
2. File → Add Local Repository
3. Proje klasörünü seçin
4. "Publish repository" butonuna tıklayın
5. Repository adını girin ve "Publish" yapın

## 📋 Yüklenen Dosyalar

### ✅ Yüklenecek Dosyalar
- Tüm C# kaynak kodları
- Proje dosyaları (.csproj, .sln)
- Dokümantasyon (SRS hariç)
- README.md
- .gitignore

### ❌ Yüklenmeyecek Dosyalar (.gitignore'da)
- `bin/` ve `obj/` klasörleri
- `.vs/` Visual Studio cache
- `packages/` NuGet paketleri
- `DOCS/SRS_SOFTWARE_REQUIREMENTS_SPECIFICATION.md` (SRS dokümanı)
- `*.py` Python script'leri
- `ER_DIYAGRAM_KILAVUZU.md`

### ✅ Yüklenecek Dosyalar (API key'ler placeholder ile)
- `Infrastructure/Services/AiAssistantService.cs` - API key: `API_KEYINIZI_YAZIN`
- `Forms/Doctor/FrmAIAnalysis.cs` - API key: `API_KEYINIZI_YAZIN`
- `Forms/Doctor/FrmGoalsNotes.cs` - API key: `API_KEYINIZI_YAZIN`
- `Infrastructure/Services/GeminiAIService.cs` - Constructor parametresi (güvenli)

## 🔐 API Key Yapılandırması

**Önemli:** API key'ler `API_KEYINIZI_YAZIN` placeholder'ı ile değiştirildi. Dosyalar GitHub'a yüklenecek ama gerçek API key'ler görünmeyecek.

**Kullanıcılar için:**
1. Projeyi klonladıktan sonra aşağıdaki dosyalarda `API_KEYINIZI_YAZIN` yerine kendi API key'inizi yazın:
   - `Infrastructure/Services/AiAssistantService.cs`
   - `Forms/Doctor/FrmAIAnalysis.cs`
   - `Forms/Doctor/FrmGoalsNotes.cs`
2. OpenRouter API key alın: https://openrouter.ai/
3. AI özelliklerini kullanmak için API key gereklidir

## 📝 Commit Mesajları Önerileri

```bash
git commit -m "feat: Add patient management module"
git commit -m "fix: Fix BMI calculation bug"
git commit -m "docs: Update README with API key instructions"
git commit -m "refactor: Clean up code according to clean code principles"
```

## 🎯 Repository Açıklaması Önerisi

```
DiyetPro - Professional Dietitian Patient Management System

A comprehensive Windows Forms application for dietitians to manage patients, 
create diet plans, track progress, and analyze data. Built with C#, .NET Framework 4.8, 
DevExpress WinForms, and MySQL.

Features:
- 9 Smart Algorithms (BMI, TDEE, Risk Analysis)
- AI Integration (Google Gemini via OpenRouter)
- Secure Authentication (PBKDF2)
- Comprehensive Reporting
- Real-time Messaging

Tech Stack: C# 12.0, .NET Framework 4.8, DevExpress WinForms 25.1.5, MySQL 8.4
```

## ⚠️ Önemli Notlar

1. **API Key'ler:** Asla API key'leri commit etmeyin. Her zaman placeholder kullanın.
2. **Veritabanı:** Connection string'lerde hassas bilgiler varsa bunları da kaldırın.
3. **Lisans:** DevExpress lisansı gerektirir. Kullanıcılar kendi lisanslarını kullanmalıdır.
4. **Demo Verileri:** Demo verileri içeriyorsa, bunların gerçek veri olmadığını belirtin.

## 📞 Destek

Sorularınız için:
- GitHub Issues kullanın
- README.md'deki dokümantasyonu inceleyin
- DOCS/ klasöründeki detaylı dokümanları okuyun

---

**Hazır!** Projeniz GitHub'a yüklenmeye hazır. 🚀
