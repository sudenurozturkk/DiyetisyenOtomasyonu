# Diyetisyen Hasta Otomasyonu Sistemi

## ?? Proje Özeti

Diyetisyen Hasta Otomasyonu, diyetisyenlerin hasta kayýtlarýný, kilo takibini, haftalýk diyet planlarýný ve hedeflerini yönetebilmeleri için geliþtirilmiþ profesyonel bir Windows Forms uygulamasýdýr.

### ?? Özellikler

#### Doktor Modülü
- ? Hasta kayýt ve yönetimi
- ? Haftalýk diyet planý atama
- ? Öðün bazýnda plan oluþturma
- ? Hedef ve not takibi
- ? Hasta mesajlaþma
- ? Analitik ve grafikler (kilo trendi, makro daðýlýmý)

#### Hasta Modülü
- ? Haftalýk menü görüntüleme
- ? Öðün onaylama ("Yedim" iþaretleme)
- ? Hedef takibi (su, kilo, adým, protein vb.)
- ? Ýlerleme grafikleri
- ? Doktor ile mesajlaþma
- ? AI Asistan (diyet önerileri)

## ??? Teknoloji Yýðýný

- **.NET Framework:** 4.8
- **UI Framework:** Windows Forms + DevExpress WinForms v25.1
- **Programlama Dili:** C# 12.0
- **Mimari:** Katmanlý mimari (Domain, Infrastructure, Forms, Shared)
- **Veri:** In-Memory (opsiyonel JSON persist hazýr)

## ?? Proje Yapýsý

```
DiyetisyenOtomasyonu/
?
??? Domain/                      # Veri modelleri
?   ??? Enums.cs                # UserRole, MealType, GoalType
?   ??? User.cs                 # Temel kullanýcý
?   ??? Doctor.cs               # Diyetisyen
?   ??? Patient.cs              # Hasta (BMI hesaplama dahil)
?   ??? DietWeek.cs             # Haftalýk plan
?   ??? DietDay.cs              # Günlük plan
?   ??? MealItem.cs             # Öðün kalemi
?   ??? WeightEntry.cs          # Kilo kaydý
?   ??? Goal.cs                 # Hedef
?   ??? Note.cs                 # Doktor notu
?   ??? Message.cs              # Mesaj
?   ??? ProgressSnapshot.cs     # Ýlerleme metrikleri
?
??? Infrastructure/
?   ??? Persistence/
?   ?   ??? IDataStore.cs       # Veri deposu arayüzü
?   ?   ??? InMemoryStore.cs    # Bellek içi depo (Singleton)
?   ??? Security/
?   ?   ??? PasswordHasher.cs   # SHA256 + salt
?   ?   ??? AuthContext.cs      # Oturum yönetimi
?   ??? Services/
?   ?   ??? PatientService.cs   # Hasta CRUD
?   ?   ??? DietService.cs      # Diyet planý yönetimi
?   ?   ??? GoalService.cs      # Hedef yönetimi
?   ?   ??? MessageService.cs   # Mesajlaþma
?   ?   ??? AiAssistantService.cs # AI stub
?   ??? Seed/
?       ??? DataSeeder.cs       # Örnek veri
?
??? Forms/
?   ??? Login/
?   ?   ??? FrmLogin.cs         # Giriþ formu
?   ??? Doctor/
?   ?   ??? FrmDoctorShell.cs   # Doktor ana kabuk (Ribbon)
?   ?   ??? FrmPatients.cs      # Hasta yönetimi
?   ?   ??? FrmAssignPlans.cs   # Plan atama
?   ?   ??? FrmGoalsNotes.cs    # Hedef ve notlar
?   ?   ??? FrmMessagesDoctor.cs # Mesajlaþma
?   ?   ??? FrmAnalytics.cs     # Analitik
?   ??? Patient/
?       ??? FrmPatientShell.cs  # Hasta ana kabuk
?       ??? FrmWeeklyMenu.cs    # Haftalýk menü
?       ??? FrmGoals.cs         # Hedeflerim
?       ??? FrmProgress.cs      # Ýlerleme
?       ??? FrmMessagesPatient.cs # Mesajlaþma
?       ??? FrmAiAssistant.cs   # AI Asistan
?
??? Bootstrap/
?   ??? ThemeBootstrapper.cs    # DevExpress tema ayarlarý
?
??? Shared/
?   ??? UiStyles.cs             # Ortak stiller ve renkler
?   ??? Guards.cs               # Validasyon yardýmcýlarý
?
??? Program.cs                  # Giriþ noktasý
??? App.config                  # Yapýlandýrma

```

## ?? Kurulum

### Gereksinimler

1. **Visual Studio 2022** (veya 2019)
2. **.NET Framework 4.8 SDK**
3. **DevExpress WinForms v25.1** (veya uyumlu sürüm)

### Adým 1: DevExpress Referanslarýný Ekleyin

Proje dosyasýný (DiyetisyenOtomasyonu.csproj) açýn ve aþaðýdaki DevExpress referanslarýný ekleyin:

```xml
<ItemGroup>
  <Reference Include="DevExpress.Data.v25.1" />
  <Reference Include="DevExpress.Utils.v25.1" />
  <Reference Include="DevExpress.XtraBars.v25.1" />
  <Reference Include="DevExpress.XtraCharts.v25.1" />
  <Reference Include="DevExpress.XtraEditors.v25.1" />
  <Reference Include="DevExpress.XtraGrid.v25.1" />
  <Reference Include="DevExpress.XtraLayout.v25.1" />
  <Reference Include="DevExpress.XtraTab.v25.1" />
  <Reference Include="DevExpress.Printing.v25.1.Core" />
</ItemGroup>
```

**Not:** DevExpress yüklü deðilse:
- [DevExpress](https://www.devexpress.com/) sitesinden trial veya lisanslý sürüm indirin
- NuGet üzerinden de eklenebilir (lisans gerektir)

### Adým 2: Projeyi Derleyin

```bash
dotnet build
```

veya Visual Studio'da **Build > Build Solution** (Ctrl+Shift+B)

### Adým 3: Çalýþtýrýn

```bash
dotnet run
```

veya Visual Studio'da **F5** tuþu ile

## ?? Test Kullanýcýlarý

Uygulama ilk çalýþtýrýldýðýnda örnek kullanýcýlar otomatik olarak oluþturulur:

### Doktor Giriþi
- **Kullanýcý Adý:** `drayse`
- **Parola:** `12345678`

### Hasta Giriþi
- **Kullanýcý Adý:** `mehmet`
- **Parola:** `12345678`

**Diðer Hastalar:**
- zeynep / 12345678
- ali / 12345678

## ?? Kullaným

### Doktor Ýþlemleri

1. **Yeni Hasta Ekle:**
   - "Hastalar" sekmesine gidin
   - Sol panelden hasta bilgilerini doldurun
   - "Hasta Ekle" butonuna týklayýn
   - Oluþturulan kullanýcý adý/þifre hastaya verilir

2. **Haftalýk Plan Ata:**
   - "Planlar" sekmesine gidin
   - Hasta ve hafta seçin
   - "Haftayý Yükle" butonuna týklayýn
   - Her gün için "Öðün Ekle" ile yemekler ekleyin
   - Önceki günden kopyalama yapabilirsiniz

3. **Hedef Belirle:**
   - "Hedef ve Notlar" sekmesine gidin
   - Hasta seçin
   - "Hedef Ekle" butonuyla yeni hedef oluþturun

4. **Analitik Görüntüle:**
   - "Raporlar" sekmesine gidin
   - Hasta seçin
   - Kilo trendi ve makro daðýlýmý grafiklerini görün

### Hasta Ýþlemleri

1. **Haftalýk Menüyü Gör:**
   - "Haftalýk Menü" sekmesine gidin
   - Hafta seçip "Haftayý Yükle" yapýn
   - Her öðün için "Yedim ?" iþaretleyebilirsiniz

2. **Hedefleri Takip Et:**
   - "Hedeflerim" sekmesine gidin
   - Sað panelden hýzlý güncelleme yapýn (su, adým)

3. **Ýlerleme Ýzle:**
   - "Ýlerleme" sekmesine gidin
   - Kilo deðiþimi ve haftalýk tamamlama grafiklerini görün

4. **AI Asistan Kullan:**
   - "AI Asistan" sekmesine gidin
   - Hýzlý sorulardan birini seçin veya kendi sorunuzu yazýn

## ?? Tema ve Görünüm

- **Tema:** Office 2019 Colorful
- **Font:** Segoe UI
- **Renk Paleti:**
  - Primary: #3498DB (Mavi)
  - Success: #2ECC71 (Yeþil)
  - Danger: #E74C3C (Kýrmýzý)
  - Warning: #F39C12 (Turuncu)
  - Background: #F5F7FA (Açýk gri)

## ?? Güvenlik

- Parolalar **SHA256 + Salt** ile hash'lenir
- Minimum parola uzunluðu: 8 karakter
- Rol tabanlý yetkilendirme (Doctor/Patient)
- Kullanýcý oturumu AuthContext ile yönetilir

## ?? Veri Kalýcýlýðý

**Þu anda:** In-Memory (uygulama kapanýnca veriler silinir)

**Gelecek:** JSON veya EF Core + SQLite entegrasyonu hazýr

```csharp
// JSON kalýcýlýk için (opsiyonel)
// InMemoryStore yerine JsonFileStore kullanýlabilir
```

## ?? Gelecek Geliþtirmeler

- [ ] SQLite + Entity Framework Core entegrasyonu
- [ ] PDF rapor oluþturma (DevExpress.XtraPrinting)
- [ ] Gerçek AI API entegrasyonu (OpenAI, FitTürkAI vb.)
- [ ] Randevu yönetimi
- [ ] SMS/Email bildirimleri
- [ ] Mobil uygulama (Xamarin/MAUI)
- [ ] Web arayüzü (Blazor)

## ?? Kod Standartlarý

- **Sýnýf Adlarý:** PascalCase
- **Metodlar:** PascalCase
- **Deðiþkenler:** camelCase
- **Private Alanlar:** _camelCase
- **UI Elementleri:** Açýklayýcý + tip (btnEkle, txtAd, gridHastalar)
- **Yorumlar:** Türkçe XML dokümantasyonu

## ?? Bilinen Sorunlar

1. **DevExpress Lisans Uyarýsý:**
   - Trial kullanýyorsanýz baþlangýçta uyarý çýkar
   - Lisanslý sürümde sorun olmaz

2. **Grid Refresh:**
   - Bazý durumlarda manuel refresh gerekebilir
   - `gridControl.RefreshDataSource()` kullanýn

## ?? Destek

- **Geliþtirici:** Sudenur Öztürk
- **Proje Tipi:** Eðitim/Demo
- **Versiyon:** 1.0.0

## ?? Lisans

Bu proje eðitim amaçlý geliþtirilmiþtir. DevExpress bileþenleri için ayrý lisans gereklidir.

---

## ?? Öðrenme Kaynaklarý

- [DevExpress WinForms Documentation](https://docs.devexpress.com/WindowsForms/2162/winforms-controls)
- [.NET Framework 4.8 Guide](https://docs.microsoft.com/en-us/dotnet/framework/)
- [C# Best Practices](https://docs.microsoft.com/en-us/dotnet/csharp/)

---

**Not:** Proje DevExpress v25.1 ile test edilmiþtir. Farklý versiyonlar için namespace'ler güncellenmelidir.
