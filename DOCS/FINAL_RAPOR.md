# DİYETİSYEN OTOMASYON SİSTEMİ
## Final Proje Raporu

---

**Proje Adı:** Diyetisyen Otomasyon Sistemi  
**Öğrenci Adı:** [AD SOYAD]  
**Öğrenci No:** [ÖĞRENCİ NO]  
**Ders:** Yazılım Mühendisliği  
**Dönem:** 2024-2025 Güz  
**Teslim Tarihi:** Aralık 2024

---

# 1. GİRİŞ

## 1.1 Projenin Tanıtılması

**Diyetisyen Otomasyon Sistemi**, diyetisyenler ve hastaları arasındaki iletişimi, diyet planlamasını ve ilerleme takibini dijitalleştiren kapsamlı bir masaüstü uygulamasıdır.

Bu sistem:
- Diyetisyenlerin hastalarını merkezi bir panelden yönetmesini sağlar
- **Yapay zeka destekli** analiz ve öneri sistemi içerir
- Kilo takibi ve trend analizi yapar
- Profesyonel mesajlaşma altyapısı sunar
- Görsel analitik raporlar üretir

**Projenin Özellikleri:**
- Modern ve profesyonel kullanıcı arayüzü (DevExpress)
- Katmanlı mimari (Layered Architecture)
- Nesneye yönelik tasarım prensipleri (OOP)
- Repository Pattern ile veri erişim soyutlaması
- MySQL veritabanı entegrasyonu

## 1.2 Projenin Amacı

Bu projenin temel amaçları:

1. **Hasta Yönetimi**: Diyetisyenlerin hastalarını ekleyebileceği, düzenleyebileceği ve takip edebileceği bir sistem oluşturmak

2. **Diyet Planlama**: Kişiselleştirilmiş haftalık diyet planları oluşturma ve hastaya atama

3. **Yapay Zeka Entegrasyonu**: 
   - Kilo trend analizi
   - Diyet uyum değerlendirmesi
   - Risk tespiti ve uyarı sistemi
   - Motivasyon mesajları üretme
   - Soru-cevap asistanı

4. **İletişim**: Diyetisyen-hasta arasında güvenli mesajlaşma platformu

5. **Raporlama**: Görsel grafikler ve analitik dashboardlar ile ilerleme takibi

## 1.3 Projenin Kapsamı

### Kapsam İçi

| Modül | Açıklama |
|-------|----------|
| Kimlik Doğrulama | Giriş, kayıt, rol tabanlı erişim |
| Hasta Yönetimi | CRUD işlemleri, profil yönetimi |
| Kilo Takibi | Kilo kayıtları, trend grafikleri |
| Diyet Planlama | Yemek veritabanı, haftalık plan atama |
| Hedef Yönetimi | Hedef belirleme, ilerleme takibi |
| Mesajlaşma | İki yönlü mesajlaşma, kategori/öncelik |
| AI Asistan | Analiz, öneri, motivasyon |
| Analitik | Grafikler, raporlar |

### Kapsam Dışı

- Mobil uygulama
- Online ödeme sistemi
- Randevu takvimi
- Video görüşme
- E-posta entegrasyonu
- Çoklu dil desteği

## 1.4 Kullanılacak Teknolojiler

| Kategori | Teknoloji | Versiyon |
|----------|-----------|----------|
| **Programlama Dili** | C# | 7.3 |
| **Framework** | .NET Framework | 4.8 |
| **UI Framework** | Windows Forms | - |
| **UI Bileşenleri** | DevExpress | 25.1 |
| **Veritabanı** | MySQL | 8.0 |
| **Veri Erişim** | ADO.NET | - |
| **IDE** | Visual Studio | 2022 |
| **Veritabanı Yönetimi** | MySQL Workbench / phpMyAdmin | - |
| **Versiyon Kontrol** | Git | - |

**Kullanılan Araçlar:**
- Visual Studio 2022 - Geliştirme ortamı
- XAMPP - Yerel MySQL sunucu
- NuGet - Paket yönetimi
- Mermaid - UML diyagram çizimi

---

# 2. PROJE PLANI

## 2.1 Sistemin Kullanıcıları

| Kullanıcı | Rol | Yetkiler |
|-----------|-----|----------|
| **Diyetisyen (Doktor)** | Yönetici | Hasta ekleme/düzenleme, diyet planı oluşturma, yemek veritabanı yönetimi, hedef belirleme, not ekleme, mesaj gönderme, analitik görüntüleme |
| **Hasta** | Son Kullanıcı | Profil görüntüleme, kilo girişi, ilerleme takibi, haftalık menü görüntüleme, mesaj gönderme, AI asistan kullanma |

## 2.2 GANTT İş Akış Diyagramı

```mermaid
gantt
    title Diyetisyen Otomasyon Sistemi - Proje Planı
    dateFormat  YYYY-MM-DD
    
    section Analiz
    Gereksinim Toplama       :done, req, 2024-10-01, 7d
    Use Case Tanımlama       :done, uc, after req, 5d
    Fizibilite Analizi       :done, feas, after uc, 3d
    
    section Tasarım
    Veritabanı Tasarımı      :done, db, 2024-10-16, 5d
    Sınıf Tasarımı           :done, cls, after db, 5d
    UI Wireframe             :done, ui, after cls, 4d
    Mimari Tasarım           :done, arch, after ui, 3d
    
    section Geliştirme
    Domain Layer             :done, dom, 2024-11-01, 10d
    Repository Layer         :done, repo, after dom, 12d
    Service Layer            :done, svc, after repo, 8d
    AI Modülü                :done, ai, after svc, 7d
    UI Formları              :done, forms, after ai, 14d
    
    section Test
    Birim Testleri           :done, unit, 2024-12-10, 5d
    Entegrasyon Testleri     :done, int, after unit, 4d
    Sistem Testleri          :done, sys, after int, 3d
    
    section Dokümantasyon
    Proje Raporu             :done, doc, 2024-12-22, 5d
    Final Hazırlık           :active, final, 2024-12-27, 3d
```

## 2.3 İşlevsel İhtiyaçlar (Olmazsa Olmazlar)

| No | İhtiyaç | Açıklama |
|----|---------|----------|
| F1 | Kullanıcı Girişi | Güvenli giriş ve rol tabanlı yönlendirme |
| F2 | Hasta Yönetimi | Hasta ekleme, düzenleme, silme, listeleme |
| F3 | Kilo Takibi | Kilo kaydı girme ve geçmiş görüntüleme |
| F4 | Diyet Planı | Haftalık plan oluşturma ve atama |
| F5 | Yemek Veritabanı | Yemek ekleme, kalori/makro bilgisi |
| F6 | Mesajlaşma | Diyetisyen-hasta iletişimi |
| F7 | Hedef Belirleme | Hedef tanımlama ve ilerleme takibi |
| F8 | Analitik | Kilo grafiği, makro dağılımı |

## 2.4 İşlevsel Olmayan İhtiyaçlar (İlave Özellikler)

| No | İhtiyaç | Açıklama |
|----|---------|----------|
| NF1 | **Güvenlik** | SHA256 + Salt parola hashleme, rol tabanlı erişim |
| NF2 | **Kullanılabilirlik** | Sezgisel arayüz, Türkçe dil desteği |
| NF3 | **Performans** | < 2 saniye sayfa yükleme |
| NF4 | **Bakılabilirlik** | Modüler yapı, kod dokümantasyonu |
| NF5 | **Ölçeklenebilirlik** | Katmanlı mimari ile genişletilebilir |
| NF6 | **AI Desteği** | Yapay zeka destekli analiz ve öneriler |

## 2.5 UML Diyagramları

### 2.5.1 Class Diyagramı

```mermaid
classDiagram
    class User {
        +int Id
        +string AdSoyad
        +string KullaniciAdi
        +string ParolaHash
        +UserRole Role
        +DateTime KayitTarihi
        +bool AktifMi
    }
    
    class Patient {
        +string Cinsiyet
        +int Yas
        +double Boy
        +double GuncelKilo
        +int DoctorId
        +double BMI
        +double BMR
        +double TDEE
    }
    
    class Doctor {
        +string UzmanlikAlani
        +string DiplomaNo
    }
    
    class Meal {
        +int Id
        +string Ad
        +double Kalori
        +double Protein
        +double Karbonhidrat
        +double Yag
    }
    
    class Goal {
        +int Id
        +int PatientId
        +double HedefDeger
        +double MevcutDeger
        +bool TamamlandiMi
    }
    
    class Message {
        +int Id
        +int GonderenId
        +int AliciId
        +string Icerik
        +bool OkunduMu
    }

    User <|-- Patient
    User <|-- Doctor
    Doctor "1" o-- "*" Patient
    Patient "1" o-- "*" Goal
    User "1" o-- "*" Message
```

### 2.5.2 Use Case Diyagramı

```mermaid
flowchart TB
    subgraph Actors
        D["🩺 Diyetisyen"]
        H["👤 Hasta"]
    end
    
    subgraph Auth["Kimlik Doğrulama"]
        UC1["Giriş Yap"]
        UC2["Kayıt Ol"]
    end
    
    subgraph PatientMgmt["Hasta Yönetimi"]
        UC3["Hasta Listele"]
        UC4["Hasta Ekle"]
        UC5["Hasta Güncelle"]
        UC6["Kilo Kaydı Gir"]
    end
    
    subgraph DietMgmt["Diyet Yönetimi"]
        UC7["Diyet Planı Oluştur"]
        UC8["Öğün Ata"]
        UC9["Yemek Ekle"]
    end
    
    subgraph Comm["İletişim"]
        UC10["Mesaj Gönder"]
        UC11["Mesaj Oku"]
    end
    
    subgraph AI["AI Özellikler"]
        UC12["Trend Analizi"]
        UC13["AI Asistan"]
    end

    D --> UC1
    D --> UC3
    D --> UC4
    D --> UC5
    D --> UC7
    D --> UC8
    D --> UC9
    D --> UC10
    D --> UC12
    
    H --> UC1
    H --> UC2
    H --> UC6
    H --> UC10
    H --> UC11
    H --> UC13
```

### 2.5.3 Sequence Diyagramı - Giriş Akışı

```mermaid
sequenceDiagram
    participant U as Kullanıcı
    participant L as FrmLogin
    participant UR as UserRepository
    participant PH as PasswordHasher
    participant AC as AuthContext
    
    U->>L: Kullanıcı adı ve şifre girer
    L->>UR: GetByUsername(kullaniciAdi)
    UR-->>L: User objesi
    L->>PH: VerifyPassword(girilen, hash)
    PH-->>L: true/false
    
    alt Doğrulama başarılı
        L->>AC: SignIn(user)
        L-->>U: Ana form açılır
    else Başarısız
        L-->>U: Hata mesajı
    end
```

### 2.5.4 Sequence Diyagramı - AI Analiz

```mermaid
sequenceDiagram
    participant D as Diyetisyen
    participant A as FrmAnalytics
    participant AI as AiAssistantService
    participant WR as WeightEntryRepository
    
    D->>A: Hasta seçer
    A->>AI: AnalyzeWeightTrend(patientId)
    AI->>WR: GetByPatientId(patientId)
    WR-->>AI: Kilo kayıtları
    AI->>AI: Trend hesaplama
    AI-->>A: WeightTrendAnalysis
    A-->>D: Grafik ve öneriler
```

### 2.5.5 Activity Diyagramı - Diyet Planı Oluşturma

```mermaid
flowchart TD
    A[Başla] --> B[Hasta Seç]
    B --> C{Hasta Seçildi mi?}
    C -->|Hayır| B
    C -->|Evet| D[Tarih Aralığı Belirle]
    D --> E[Yemek Veritabanını Aç]
    E --> F{Her Gün İçin}
    F --> G[Kahvaltı Seç]
    G --> H[Öğle Yemeği Seç]
    H --> I[Akşam Yemeği Seç]
    I --> J{Tüm Günler Tamamlandı?}
    J -->|Hayır| F
    J -->|Evet| K[Planı Kaydet]
    K --> L{Kayıt Başarılı?}
    L -->|Evet| M[Bildirim Göster]
    L -->|Hayır| N[Hata Göster]
    M --> O[Bitir]
    N --> O
```

### 2.5.6 Interaction Diyagramı - Mesajlaşma

```mermaid
sequenceDiagram
    participant D as Diyetisyen
    participant DF as FrmMessagesDoctor
    participant MS as MessageService
    participant MR as MessageRepository
    participant DB as MySQL
    participant PF as FrmMessagesPatient
    participant H as Hasta
    
    D->>DF: Mesaj yazar
    D->>DF: Gönder butonuna tıklar
    DF->>MS: SendMessage(doctorId, patientId, content)
    MS->>MR: Add(message)
    MR->>DB: INSERT INTO Messages
    DB-->>MR: messageId
    MR-->>MS: true
    MS-->>DF: Başarılı
    DF-->>D: Mesaj gönderildi
    
    Note over H: Hasta uygulamayı açar
    H->>PF: Mesajlar ekranına gider
    PF->>MS: GetConversation(patientId, doctorId)
    MS->>MR: GetConversation(...)
    MR->>DB: SELECT * FROM Messages
    DB-->>MR: Mesaj listesi
    MR-->>MS: Messages
    MS-->>PF: Mesajlar
    PF-->>H: Konuşma gösterilir
```

---

# 3. PROJE GERÇEKLEŞTİRİLMESİ

## 3.1 Modüllerin ve Tüm Formların Tasarımı

### 3.1.1 Giriş Modülü

#### FrmSplash - Açılış Ekranı
Uygulama başlatıldığında gösterilen splash ekranı. Logo ve yükleme animasyonu içerir.

**Özellikler:**
- Uygulama logosu
- Yükleme progress bar
- 3 saniye sonra otomatik kapanış

---

#### FrmLogin - Giriş Ekranı
Kullanıcı girişi için ana form.

**Bileşenler:**
- Kullanıcı adı TextBox
- Şifre TextBox (PasswordChar)
- Giriş butonu
- Kayıt ol linki

**Validasyonlar:**
- Boş alan kontrolü
- Kullanıcı adı varlık kontrolü
- Şifre doğrulama

---

#### FrmRegister - Kayıt Ekranı
Yeni hasta kaydı için form.

**Bileşenler:**
- Ad Soyad
- Kullanıcı adı
- Şifre / Şifre tekrar
- Kayıt butonu

---

### 3.1.2 Diyetisyen Modülü

#### FrmDoctorShell - Ana Panel
Diyetisyen için ana kabuk formu. Sidebar navigasyon içerir.

**Menü Öğeleri:**
- 🏠 Dashboard
- 👥 Hastalar
- 🍽️ Yemekler
- 📋 Plan Ata
- 🎯 Hedefler & Notlar
- 💬 Mesajlar
- 📊 Analitik

---

#### FrmPatients - Hasta Listesi
Tüm hastaların listelendiği ve yönetildiği form.

**Özellikler:**
- DataGrid ile hasta listesi
- Arama/filtreleme
- Yeni hasta ekleme butonu
- Detay görüntüleme
- Hasta silme

**Grid Kolonları:**
| Kolon | Açıklama |
|-------|----------|
| Ad Soyad | Hasta adı |
| Yaş | Hesaplanan yaş |
| Kilo | Güncel kilo |
| BMI | Hesaplanan BMI |
| Durum | BMI kategorisi |

---

#### FrmPatientProfile - Hasta Profili
Seçilen hastanın detaylı profil sayfası.

**Bölümler:**
- Kişisel bilgiler kartı
- Sağlık bilgileri (BMI, BMR, TDEE)
- Kilo grafiği
- Son notlar
- Hızlı aksiyonlar

---

#### FrmMeals - Yemek Yönetimi
Yemek veritabanı yönetim formu.

**Özellikler:**
- Yemek listesi (kategoriye göre)
- Yemek ekleme/düzenleme
- Makro besin bilgileri
- Tarif açıklaması

**Alanlar:**
- Ad, Kalori, Protein, Karbonhidrat, Yağ
- Kategori (Kahvaltı/Öğle/Akşam/Atıştırmalık)
- Tarif

---

#### FrmAssignPlans - Plan Atama
Hastaya haftalık diyet planı atama formu.

**Özellikler:**
- Hasta seçimi
- Hafta seçimi
- Gün bazlı öğün atama
- Drag-drop yemek ekleme

---

#### FrmGoalsNotes - Hedefler ve Notlar
Hasta hedefleri ve klinik notlar yönetimi.

**Sekmeler:**
- Hedefler: Hedef ekleme, ilerleme güncelleme
- Notlar: Klinik not ekleme, kategorizasyon

---

#### FrmMessagesDoctor - Mesajlar
Diyetisyen mesajlaşma ekranı.

**Özellikler:**
- Hasta listesi (sol panel)
- Konuşma geçmişi (orta panel)
- Mesaj yazma alanı
- Okunmamış sayacı

---

#### FrmAnalytics - Analitik
Hasta analitik ve grafik ekranı.

**Grafikler:**
- Kilo trendi çizgi grafiği
- Makro besin pasta grafiği
- Özet kartlar (BMI, hedef yakınlık)

---

### 3.1.3 Hasta Modülü

#### FrmPatientShell - Hasta Ana Panel
Hasta için ana kabuk formu.

**Menü Öğeleri:**
- 📈 İlerleme
- 📅 Haftalık Menü
- 🎯 Hedeflerim
- 💬 Mesajlar
- 🤖 AI Asistan

---

#### FrmProgress - İlerleme
Hastanın kilo takibi ve ilerleme ekranı.

**Özellikler:**
- Kilo girişi
- Kilo grafiği
- BMI göstergesi
- İlerleme özeti

---

#### FrmWeeklyMenu - Haftalık Menü
Hastanın atanmış haftalık diyet planı görüntüleme.

**Özellikler:**
- Hafta seçimi
- Gün bazlı öğün görüntüleme
- Kalori toplamları

---

#### FrmGoals - Hedeflerim
Hasta hedefleri görüntüleme.

**Özellikler:**
- Aktif hedefler listesi
- İlerleme çubuğu
- Hedef detayları

---

#### FrmMessagesPatient - Mesajlar
Hasta mesajlaşma ekranı.

**Özellikler:**
- Diyetisyen ile konuşma
- Mesaj geçmişi
- Mesaj gönderme

---

#### FrmAiAssistant - AI Asistan
Yapay zeka destekli asistan ekranı.

**Özellikler:**
- Günlük ipucu
- Soru-cevap alanı
- Motivasyon mesajları

---

## 3.2 Veritabanı Tasarımı (ER Diyagramı)

```mermaid
erDiagram
    USERS {
        int Id PK
        varchar AdSoyad
        varchar KullaniciAdi UK
        varchar ParolaHash
        int Role
        datetime KayitTarihi
        tinyint AktifMi
    }
    
    PATIENTS {
        int Id PK,FK
        int DoctorId FK
        datetime DogumTarihi
        double Boy
        double Kilo
        double HedefKilo
        varchar KanGrubu
        text KronikHastaliklar
        text Alerjiler
    }
    
    DOCTORS {
        int Id PK,FK
        varchar UzmanlikAlani
        varchar DiplomaNo
        varchar CalistigiKurum
    }
    
    MEALS {
        int Id PK
        varchar Ad
        double Kalori
        double Protein
        double Karbonhidrat
        double Yag
        varchar Kategori
        text Tarif
        int OlusturanDoktorId FK
    }
    
    GOALS {
        int Id PK
        int PatientId FK
        varchar Baslik
        double HedefDeger
        double MevcutDeger
        datetime BitisTarihi
        tinyint TamamlandiMi
    }
    
    MESSAGES {
        int Id PK
        int GonderenId FK
        int AliciId FK
        text Icerik
        datetime GonderimTarihi
        tinyint OkunduMu
    }
    
    WEIGHTENTRIES {
        int Id PK
        int PatientId FK
        double Kilo
        datetime Tarih
    }
    
    NOTES {
        int Id PK
        int PatientId FK
        int DoctorId FK
        text Icerik
        datetime Tarih
    }
    
    DIETPLANS {
        int Id PK
        int PatientId FK
        int DoctorId FK
        varchar Baslik
        datetime BaslangicTarihi
        datetime BitisTarihi
    }

    USERS ||--o| PATIENTS : "is-a"
    USERS ||--o| DOCTORS : "is-a"
    DOCTORS ||--o{ PATIENTS : "manages"
    DOCTORS ||--o{ MEALS : "creates"
    PATIENTS ||--o{ GOALS : "has"
    PATIENTS ||--o{ WEIGHTENTRIES : "records"
    PATIENTS ||--o{ NOTES : "has"
    USERS ||--o{ MESSAGES : "sends"
```

### Tablo Açıklamaları

| Tablo | Kayıt Sayısı | Açıklama |
|-------|-------------|----------|
| Users | ~10 | Ana kullanıcı tablosu |
| Patients | ~5 | Hasta detayları |
| Doctors | ~2 | Doktor detayları |
| Meals | ~20 | Yemek veritabanı |
| Goals | ~10 | Hasta hedefleri |
| Messages | ~20 | Mesajlar |
| WeightEntries | ~30 | Kilo kayıtları |
| Notes | ~15 | Klinik notlar |
| DietPlans | ~5 | Diyet planları |

## 3.3 Çıktılar & Raporlar

### Mevcut Çıktılar

| Çıktı Tipi | Açıklama |
|------------|----------|
| Kilo Grafiği | Hastanın kilo değişim trendi |
| Makro Grafiği | Protein/Karbonhidrat/Yağ dağılımı |
| BMI Göstergesi | Görsel BMI kategorisi |
| İlerleme Özeti | Hedefe kalan miktar |

### PDF Çıktısı (Planlanan)

> **Not:** PDF export özelliği gelecek versiyonda eklenecektir. Şu an grafikler ve raporlar uygulama içinde görüntülenmektedir.

---

# 4. PROJEDE ÖNGÖRÜLEN EKSİKLİKLER

## 4.1 Proje Planında Yapılması Planlanmış Ancak Eksik Kalan Modüller

| Modül | Durum | Açıklama |
|-------|-------|----------|
| PDF Rapor Export | ⏳ Eksik | Raporların PDF olarak dışa aktarımı |
| Otomatik Test | ⏳ Eksik | NUnit/xUnit test projesi |
| Bildirim Sistemi | ⏳ Eksik | Push notification |

## 4.2 Projeye Eklenmesi İçeriği Zenginleştirecek Modüller

| Modül | Öncelik | Açıklama |
|-------|---------|----------|
| Mobil Uygulama | Yüksek | Xamarin/MAUI ile cross-platform |
| Randevu Sistemi | Yüksek | Takvim entegrasyonu |
| Video Görüşme | Orta | Online konsültasyon |
| ML.NET Entegrasyonu | Orta | Daha gelişmiş AI modeli |
| E-posta Bildirimi | Düşük | Otomatik hatırlatmalar |
| Çoklu Dil | Düşük | İngilizce destek |
| Egzersiz Modülü | Orta | Egzersiz planları |
| Besin Arama API | Orta | Harici besin veritabanı |

---

# 5. PROJE TESLİM

## 5.1 Kurulum Gereksinimleri

| Gereksinim | Minimum |
|------------|---------|
| İşletim Sistemi | Windows 10/11 |
| .NET Framework | 4.8 |
| RAM | 4 GB |
| Disk Alanı | 500 MB |
| Veritabanı | MySQL 8.0 veya XAMPP |

## 5.2 Kurulum Adımları

### Adım 1: XAMPP Kurulumu
1. XAMPP'ı indirin (https://www.apachefriends.org/)
2. Kurulumu tamamlayın
3. XAMPP Control Panel'i açın
4. MySQL'i başlatın

### Adım 2: Veritabanı Oluşturma
1. phpMyAdmin'i açın (http://localhost/phpmyadmin)
2. "Yeni" butonuna tıklayın
3. Veritabanı adı: `dietpro_db`
4. Collation: `utf8mb4_turkish_ci`
5. `seed_data.sql` dosyasını import edin

### Adım 3: Uygulama Kurulumu
1. Visual Studio 2022'yi açın
2. Projeyi açın (DiyetisyenOtomasyonu.sln)
3. NuGet paketlerini restore edin
4. Build > Build Solution
5. Debug > Start Debugging (F5)

### Adım 4: Giriş Yapma
- **Diyetisyen:** kullanıcı: `whodenur`, şifre: `12345678`
- **Hasta:** kullanıcı: `vesudenur`, şifre: `12345678`

## 5.3 Setup Dosyası

> **Not:** Visual Studio Installer Project ile .exe setup dosyası oluşturulacaktır. Kurulum dosyası aşağıdaki bileşenleri içerecektir:
> - Ana uygulama dosyaları
> - DevExpress DLL'leri
> - MySQL Connector
> - .NET Framework 4.8 önkoşul kontrolü

---

# 6. SONUÇ

## 6.1 Projenin Genel Değerlendirmesi

### Artıları

| Özellik | Açıklama |
|---------|----------|
| **Modern Arayüz** | DevExpress ile profesyonel UI |
| **AI Entegrasyonu** | Yapay zeka destekli analiz ve öneriler |
| **Modüler Yapı** | Katmanlı mimari, kolay genişletilebilir |
| **OOP Uyumu** | Inheritance, Encapsulation, Polymorphism |
| **Tasarım Desenleri** | Repository, Template Method |
| **Güvenlik** | Hash'li parolalar, rol tabanlı erişim |

### Eksileri

| Özellik | Açıklama |
|---------|----------|
| **Sadece Masaüstü** | Mobil uygulama yok |
| **PDF Export Yok** | Rapor dışa aktarımı eksik |
| **Çevrimdışı Çalışmaz** | Veritabanı bağlantısı gerekli |

### Tercih Edilme Sebebi

Bu proje, diyetisyen-hasta ilişkisinde yaşanan iletişim ve takip sorunlarına çözüm sunması, **yapay zeka destekli analiz özellikleri** içermesi ve **modern yazılım mühendisliği prensipleri** ile geliştirilmiş olması nedeniyle tercih edilmiştir.

## 6.2 Projenin Geliştirme Süresi Boyunca Katkısı

Bu proje sürecinde kazanılan deneyimler:

1. **Teknik Kazanımlar:**
   - Katmanlı mimari tasarımı
   - Repository Pattern uygulaması
   - Windows Forms ile profesyonel UI geliştirme
   - MySQL veritabanı tasarımı ve yönetimi
   - AI algoritma geliştirme

2. **Yazılım Mühendisliği:**
   - UML diyagram çizimi
   - Gereksinim analizi
   - Test planlama ve uygulama
   - Dokümantasyon hazırlama

3. **Kişisel Gelişim:**
   - Problem çözme becerisi
   - Proje yönetimi
   - Zaman planlaması
   - Dokümantasyon yazımı

---

# 7. KAYNAKLAR

## Kitaplar
1. Martin, R. C. (2008). *Clean Code: A Handbook of Agile Software Craftsmanship*. Prentice Hall.
2. Gamma, E., Helm, R., Johnson, R., & Vlissides, J. (1994). *Design Patterns: Elements of Reusable Object-Oriented Software*. Addison-Wesley.
3. Fowler, M. (2002). *Patterns of Enterprise Application Architecture*. Addison-Wesley.

## Web Kaynakları
4. Microsoft Docs - C# Programming Guide: https://docs.microsoft.com/en-us/dotnet/csharp/
5. DevExpress Documentation: https://docs.devexpress.com/
6. MySQL Documentation: https://dev.mysql.com/doc/

## Akademik Kaynaklar
7. Mifflin, M. D., et al. (1990). "A new predictive equation for resting energy expenditure in healthy individuals." *The American Journal of Clinical Nutrition*, 51(2), 241-247.
8. McCabe, T. J. (1976). "A Complexity Measure." *IEEE Transactions on Software Engineering*, SE-2(4), 308-320.

## Video Kaynakları
9. C# Windows Forms Tutorial - YouTube
10. DevExpress Getting Started - YouTube

---

# EK: MALİYET KESTİRİM DOKÜMANI

## Proje Bilgileri
**Proje Adı:** Diyetisyen Otomasyon Sistemi

## Ölçüm Parametreleri

| Ölçüm Parametresi | Sayı | Ağırlık Faktörü | Toplam |
|-------------------|------|-----------------|--------|
| Kullanıcı Girdi Sayısı | 18 | 3 | 54 |
| Kullanıcı Çıktı Sayısı | 12 | 4 | 48 |
| Kullanıcı Sorgu Sayısı | 15 | 3 | 45 |
| Veri Tabanındaki Tablo Sayısı | 9 | 7 | 63 |
| Arayüz Sayısı | 14 | 5 | 70 |
| **Ana İşlev Nokta Sayısı (AİN)** | | | **280** |

## Teknik Karmaşıklık Faktörü

| No | Soru | Puan |
|----|------|------|
| 1 | Güvenilir yedekleme ve kurtarma | 3 |
| 2 | Veri iletişimi | 4 |
| 3 | Dağıtık işlem işlevleri | 0 |
| 4 | Performans kritik mi | 3 |
| 5 | Ağır yüklü ortam | 2 |
| 6 | Çevrim içi veri girişi | 5 |
| 7 | Birden çok ekran gereksinimi | 4 |
| 8 | Ana kütükler çevrim-içi | 5 |
| 9 | Karmaşık girdi/çıktı | 3 |
| 10 | İçsel işlemler karmaşık | 4 |
| 11 | Yeniden kullanılabilir kod | 4 |
| 12 | Dönüştürme/kurulum dikkate alınacak | 3 |
| 13 | Çoklu kurulum | 2 |
| 14 | Kolay kullanılabilir | 4 |
| **Toplam (TKF)** | | **46** |

## Hesaplama

```
İN = AİN × (0.65 + 0.01 × TKF)
İN = 280 × (0.65 + 0.01 × 46)
İN = 280 × 1.11
İN = 310.8

Satır Sayısı = İN × 30
Satır Sayısı = 310.8 × 30
Satır Sayısı ≈ 9,324 satır
```

**Gerçek Satır Sayısı:** ~10,350 (Tahmine yakın)
