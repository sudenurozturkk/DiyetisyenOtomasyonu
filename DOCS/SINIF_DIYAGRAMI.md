# SINIF DİYAGRAMI
## Diyetisyen Hasta Takip Otomasyonu

**Proje Adı:** DiyetPro - Diyetisyen Hasta Otomasyonu  
**Tarih:** 17 Ocak 2026  
**Versiyon:** 2.0

---

## 1. KATMANLI MİMARİ YAPISI

Proje **4 katmanlı mimari** kullanmaktadır:

```
┌─────────────────────────────────────────┐
│         PRESENTATION LAYER              │
│         (Forms - UI)                    │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│         BUSINESS LOGIC LAYER            │
│         (Services)                      │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│         DATA ACCESS LAYER               │
│         (Repositories)                  │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│         DOMAIN LAYER                    │
│         (Entities/Models)               │
└─────────────────────────────────────────┘
```

---

## 2. DOMAIN LAYER (Veri Modelleri)

### 2.1 User (Temel Kullanıcı Sınıfı)

```csharp
┌─────────────────────────────────┐
│          <<Entity>>             │
│            User                 │
├─────────────────────────────────┤
│ - Id: int                       │
│ - AdSoyad: string               │
│ - KullaniciAdi: string          │
│ - ParolaHash: string            │
│ - Role: UserRole (enum)         │
│ - KayitTarihi: DateTime         │
│ - AktifMi: bool                 │
├─────────────────────────────────┤
│ + User()                        │
└─────────────────────────────────┘
```

**İlişkiler:**
- Patient ve Doctor sınıflarının **base class**'ı (inheritance)

---

### 2.2 Patient (Hasta Sınıfı)

```csharp
┌──────────────────────────────────────────────────┐
│              <<Entity>>                          │
│              Patient : User                      │
├──────────────────────────────────────────────────┤
│ # Temel Bilgiler                                 │
│ - Cinsiyet: string                               │
│ - Yas: int                                       │
│ - Boy: double (cm)                               │
│ - BaslangicKilosu: double (kg)                   │
│ - GuncelKilo: double (kg)                        │
│ - DoctorId: int                                  │
│ - Notlar: string                                 │
│                                                  │
│ # Tıbbi Bilgiler                                 │
│ - ThyroidStatus: string                          │
│ - InsulinStatus: string                          │
│ - MedicalHistory: string                         │
│                                                  │
│ # Yaşam Tarzı                                    │
│ - LifestyleType: LifestyleType (enum)            │
│ - ActivityLevel: ActivityLevel (enum)            │
│                                                  │
│ # Navigation Properties                          │
│ - Allergies: List<PatientAllergy>                │
├──────────────────────────────────────────────────┤
│ # Calculated Properties (AKILLI ALGORITMALAR)    │
│ + BMI: double {get}                              │
│   Formula: Kilo / (Boy²)                         │
│                                                  │
│ + BMIKategori: string {get}                      │
│   Karar: <18.5=Zayıf, 18.5-24.9=Normal,         │
│          25-29.9=Fazla Kilolu, >30=Obez          │
│                                                  │
│ + KiloDegisimi: double {get}                     │
│   Formula: GuncelKilo - BaslangicKilosu          │
│                                                  │
│ + KiloDegisimYuzdesi: double {get}               │
│   Formula: (KiloDegisimi / BaslangicKilosu)*100  │
│                                                  │
│ + BMR: double {get}                              │
│   Formula (Mifflin-St Jeor):                     │
│   Erkek: (10×kilo)+(6.25×boy)-(5×yaş)+5         │
│   Kadın: (10×kilo)+(6.25×boy)-(5×yaş)-161       │
│                                                  │
│ + TDEE: double {get}                             │
│   Formula: BMR × ActivityMultiplier              │
│   (1.2, 1.375, 1.55, 1.725, 1.9)                │
│                                                  │
│ + IdealKiloAraligi: string {get}                 │
│   Formula: 18.5×Boy² - 24.9×Boy²                 │
├──────────────────────────────────────────────────┤
│ + Patient()                                      │
└──────────────────────────────────────────────────┘
```

**Akıllı Algoritmalar:** 6 adet hesaplama algoritması içerir  
**OOP Prensipleri:** Inheritance (User'dan türer), Encapsulation

---

### 2.3 Doctor (Diyetisyen Sınıfı)

```csharp
┌─────────────────────────────────┐
│          <<Entity>>             │
│        Doctor : User            │
├─────────────────────────────────┤
│ - Uzmanlik: string              │
│ - Telefon: string               │
│ - Email: string                 │
├─────────────────────────────────┤
│ + Doctor()                      │
└─────────────────────────────────┘
```

---

### 2.4 DietWeek (Haftalık Plan)

```csharp
┌─────────────────────────────────┐
│          <<Entity>>             │
│          DietWeek               │
├─────────────────────────────────┤
│ - Id: int                       │
│ - PatientId: int                │
│ - WeekStartDate: DateTime       │
│ - CreatedAt: DateTime           │
│ - CreatedByDoctorId: int        │
│ - WeekNotes: string             │
│ - Version: int                  │
│ - IsActive: bool                │
│                                 │
│ # Navigation                    │
│ - Days: List<DietDay>           │
├─────────────────────────────────┤
│ + DietWeek()                    │
└─────────────────────────────────┘
```

**İlişkiler:**
- Patient ile **1:N** ilişkisi
- DietDay ile **1:N** composition ilişkisi

---

### 2.5 DietDay (Günlük Plan)

```csharp
┌─────────────────────────────────┐
│          <<Entity>>             │
│          DietDay                │
├─────────────────────────────────┤
│ - Id: int                       │
│ - DietWeekId: int               │
│ - Date: DateTime                │
│ - Notes: string                 │
│ - TargetCalories: double        │
│                                 │
│ # Navigation                    │
│ - MealItems: List<MealItem>     │
├─────────────────────────────────┤
│ + DietDay()                     │
└─────────────────────────────────┘
```

**İlişkiler:**
- DietWeek ile **N:1** ilişkisi
- MealItem ile **1:N** composition ilişkisi

---

### 2.6 MealItem (Öğün Kalemi)

```csharp
┌─────────────────────────────────┐
│          <<Entity>>             │
│          MealItem               │
├─────────────────────────────────┤
│ - Id: int                       │
│ - DietDayId: int                │
│ - MealType: MealType (enum)     │
│ - Name: string                  │
│ - Calories: double              │
│ - Protein: double               │
│ - Carbs: double                 │
│ - Fat: double                   │
│ - PortionSize: string           │
│ - TimeRange: string             │
│ - IsConfirmedByPatient: bool    │
│ - ConfirmedAt: DateTime?        │
│ - SkippedReason: string         │
├─────────────────────────────────┤
│ + MealItem()                    │
└─────────────────────────────────┘
```

---

### 2.7 Goal (Hedef)

```csharp
┌─────────────────────────────────┐
│          <<Entity>>             │
│            Goal                 │
├─────────────────────────────────┤
│ - Id: int                       │
│ - PatientId: int                │
│ - GoalType: GoalType (enum)     │
│ - TargetValue: double           │
│ - CurrentValue: double          │
│ - Unit: string                  │
│ - StartDate: DateTime           │
│ - EndDate: DateTime?            │
│ - IsActive: bool                │
├─────────────────────────────────┤
│ # Calculated Properties         │
│ + ProgressPercentage: double    │
│   Formula: (Current/Target)*100 │
│                                 │
│ + IsCompleted: bool             │
│   Karar: Current >= Target      │
├─────────────────────────────────┤
│ + Goal()                        │
└─────────────────────────────────┘
```

**Akıllı Algoritma:** İlerleme yüzdesi hesaplama

---

### 2.8 Message (Mesaj)

```csharp
┌─────────────────────────────────┐
│          <<Entity>>             │
│          Message                │
├─────────────────────────────────┤
│ - Id: int                       │
│ - FromUserId: int               │
│ - ToUserId: int                 │
│ - Content: string               │
│ - SentAt: DateTime              │
│ - IsRead: bool                  │
│ - Category: MessageCategory     │
│ - Priority: MessagePriority     │
│ - ParentMessageId: int?         │
├─────────────────────────────────┤
│ + Message()                     │
└─────────────────────────────────┘
```

---

### 2.9 Note (Hasta Notu)

```csharp
┌─────────────────────────────────┐
│          <<Entity>>             │
│            Note                 │
├─────────────────────────────────┤
│ - Id: int                       │
│ - PatientId: int                │
│ - DoctorId: int                 │
│ - DoctorName: string            │
│ - Content: string               │
│ - Date: DateTime                │
│ - Category: NoteCategory (enum) │
├─────────────────────────────────┤
│ + Note()                        │
└─────────────────────────────────┘
```

---

### 2.10 WeightEntry (Kilo Kaydı)

```csharp
┌─────────────────────────────────┐
│          <<Entity>>             │
│        WeightEntry              │
├─────────────────────────────────┤
│ - Id: int                       │
│ - PatientId: int                │
│ - Date: DateTime                │
│ - Weight: double                │
│ - Notes: string                 │
├─────────────────────────────────┤
│ + WeightEntry()                 │
└─────────────────────────────────┘
```

---

### 2.11 Appointment (Randevu)

```csharp
┌─────────────────────────────────┐
│          <<Entity>>             │
│        Appointment              │
├─────────────────────────────────┤
│ - Id: int                       │
│ - PatientId: int                │
│ - DoctorId: int                 │
│ - DateTime: DateTime            │
│ - Type: int                     │
│ - Status: int                   │
│ - Notes: string                 │
│ - Price: decimal                │
│ - CreatedAt: DateTime           │
├─────────────────────────────────┤
│ + Appointment()                 │
└─────────────────────────────────┘
```

---

## 3. ENUM TANIMLARI

### 3.1 UserRole (Kullanıcı Rolü)

```csharp
┌────────────────┐
│  <<Enum>>      │
│   UserRole     │
├────────────────┤
│ Doctor = 0     │
│ Patient = 1    │
└────────────────┘
```

### 3.2 MealType (Öğün Tipi)

```csharp
┌────────────────┐
│  <<Enum>>      │
│   MealType     │
├────────────────┤
│ Breakfast = 0  │
│ Snack1 = 1     │
│ Lunch = 2      │
│ Snack2 = 3     │
│ Dinner = 4     │
└────────────────┘
```

### 3.3 GoalType (Hedef Tipi)

```csharp
┌────────────────┐
│  <<Enum>>      │
│   GoalType     │
├────────────────┤
│ Water = 0      │
│ Weight = 1     │
│ Steps = 2      │
│ Sleep = 3      │
│ Protein = 4    │
│ Calories = 5   │
│ Exercise = 6   │
└────────────────┘
```

### 3.4 LifestyleType (Yaşam Tarzı)

```csharp
┌────────────────┐
│  <<Enum>>      │
│ LifestyleType  │
├────────────────┤
│ Student = 0    │
│ OfficeWorker=1 │
│ NightShift = 2 │
│ Athlete = 3    │
│ HomeMaker = 4  │
│ Retired = 5    │
│ Freelancer = 6 │
└────────────────┘
```

### 3.5 ActivityLevel (Aktivite Seviyesi)

```csharp
┌───────────────────┐
│    <<Enum>>       │
│  ActivityLevel    │
├───────────────────┤
│ Sedentary = 0     │
│ LightlyActive = 1 │
│ ModeratelyActive=2│
│ VeryActive = 3    │
│ ExtraActive = 4   │
└───────────────────┘
```

---

## 4. REPOSITORY LAYER (Veri Erişim)

### 4.1 IRepository<T> (Generic Interface)

```csharp
┌─────────────────────────────────────┐
│         <<Interface>>               │
│        IRepository<T>               │
├─────────────────────────────────────┤
│ + GetById(id: int): T               │
│ + GetAll(): IEnumerable<T>          │
│ + Find(predicate): IEnumerable<T>   │
│ + FirstOrDefault(predicate): T      │
│ + Add(entity: T): int               │
│ + Update(entity: T): bool           │
│ + Delete(id: int): bool             │
│ + Count(): int                      │
│ + Any(predicate): bool              │
└─────────────────────────────────────┘
```

**OOP Prensibi:** Generic programming, interface segregation

---

### 4.2 BaseRepository<T> (Abstract Base)

```csharp
┌─────────────────────────────────────┐
│       <<Abstract>>                  │
│    BaseRepository<T>                │
│    : IRepository<T>                 │
├─────────────────────────────────────┤
│ # _tableName: string                │
│ # _connectionString: string         │
├─────────────────────────────────────┤
│ # CreateConnection(): IDbConnection │
│ # ExecuteQuery(): IEnumerable<T>    │
│ # ExecuteScalar(): object           │
│ # ExecuteNonQuery(): int            │
│                                     │
│ # Abstract Methods                  │
│ + MapFromReader(reader): T          │
│ + MapToParameters(entity): Dict     │
│ + GetInsertSql(): string            │
│ + GetUpdateSql(): string            │
│                                     │
│ # Implemented (Template Method)     │
│ + GetById(id): T                    │
│ + GetAll(): IEnumerable<T>          │
│ + Add(entity): int                  │
│ + Update(entity): bool              │
│ + Delete(id): bool                  │
└─────────────────────────────────────┘
```

**Design Pattern:** Template Method Pattern  
**OOP Prensibi:** Inheritance, polymorphism

---

### 4.3 Concrete Repositories

```csharp
┌──────────────────────────────┐
│  PatientRepository           │
│  : BaseRepository<Patient>   │
├──────────────────────────────┤
│ + GetByDoctorId(id): List    │
│ + GetFullPatientById(id): P  │
│ + CreatePatient(p): int      │
│ + Search(text, id?): List    │
└──────────────────────────────┘

┌──────────────────────────────┐
│  DoctorRepository            │
│  : BaseRepository<Doctor>    │
├──────────────────────────────┤
│ + GetByUserId(id): Doctor    │
└──────────────────────────────┘

┌──────────────────────────────┐
│  MessageRepository           │
│  : BaseRepository<Message>   │
├──────────────────────────────┤
│ + GetConversation(u1,u2):L   │
│ + GetUnreadCount(uid): int   │
│ + MarkAsRead(u1,u2): void    │
└──────────────────────────────┘

┌──────────────────────────────┐
│  GoalRepository              │
│  : BaseRepository<Goal>      │
├──────────────────────────────┤
│ + GetByPatientId(id): List   │
│ + GetActiveGoals(id): List   │
└──────────────────────────────┘
```

**Toplam Repository:** 16 adet

---

## 5. SERVICE LAYER (İş Mantığı)

### 5.1 PatientService (Hasta İş Mantığı)

```csharp
┌──────────────────────────────────────────────────┐
│            PatientService                        │
├──────────────────────────────────────────────────┤
│ - _patientRepository: PatientRepository          │
│ - _userRepository: UserRepository                │
│ - _weightRepository: WeightEntryRepository       │
├──────────────────────────────────────────────────┤
│ + GetAllPatients(): List<Patient>                │
│ + GetPatientsByDoctor(docId): List<Patient>      │
│ + GetPatientById(id): Patient                    │
│                                                  │
│ + CreatePatient(params...): Patient              │
│   Validasyon: Ad, kullanıcı adı, parola         │
│   Hesaplama: BMI, TDEE (otomatik)               │
│                                                  │
│ + UpdatePatient(patient): void                   │
│ + DeletePatient(id): void                        │
│ + SearchPatients(text, docId?): List             │
│                                                  │
│ + UpdateWeight(patId, weight): void              │
│   Otomatik: Yeni WeightEntry kaydı              │
│                                                  │
│ + GetWeightHistory(patId, days?): List           │
│                                                  │
│ + GetPatientRiskStatus(id): PatientRiskStatus    │
│   AKILLI ALGORITMA: Risk analizi                 │
│   - Kilo platosunun tespiti                      │
│   - Hızlı kilo kaybı/alımı tespiti               │
│   - BMI risk değerlendirmesi                     │
└──────────────────────────────────────────────────┘
```

**Akıllı Algoritmalar:** Risk analizi, kilo trendi analizi  
**OOP Prensibi:** Single Responsibility, Dependency Injection

---

### 5.2 MessageService (Mesajlaşma İş Mantığı)

```csharp
┌──────────────────────────────────────┐
│        MessageService                │
├──────────────────────────────────────┤
│ - _messageRepository: MessageRepo    │
│ - _userRepository: UserRepository    │
├──────────────────────────────────────┤
│ + SendMessage(from, to, text): bool  │
│ + GetConversation(u1, u2): List      │
│ + GetUnreadCount(uid, oid): int      │
│ + MarkAsRead(uid, oid): void         │
│ + DeleteMessage(id, uid): void       │
└──────────────────────────────────────┘
```

---

### 5.3 DietService (Diyet Planı İş Mantığı)

```csharp
┌──────────────────────────────────────┐
│         DietService                  │
├──────────────────────────────────────┤
│ - _dietRepository: DietRepository    │
├──────────────────────────────────────┤
│ + CreateDietWeek(params): DietWeek   │
│ + GetWeeksByPatient(id): List        │
│ + AddMealItem(params): void          │
│ + ConfirmMeal(id): void              │
│ + GetComplianceRate(id): double      │
│   AKILLI ALGORITMA: Uyum oranı       │
└──────────────────────────────────────┘
```

---

### 5.4 GoalService (Hedef İş Mantığı)

```csharp
┌──────────────────────────────────────┐
│          GoalService                 │
├──────────────────────────────────────┤
│ - _goalRepository: GoalRepository    │
├──────────────────────────────────────┤
│ + CreateGoal(params): Goal           │
│ + UpdateGoalProgress(id, val): void  │
│   AKILLI ALGORITMA: Progress %       │
│ + GetActiveGoals(patId): List        │
│ + CompleteGoal(id): void             │
└──────────────────────────────────────┘
```

---

## 6. SECURITY LAYER (Güvenlik)

### 6.1 SecurePasswordHasher (PBKDF2)

```csharp
┌──────────────────────────────────────────────┐
│      <<Static Class>>                        │
│    SecurePasswordHasher                      │
├──────────────────────────────────────────────┤
│ - SaltSize: const int = 16                   │
│ - KeySize: const int = 32                    │
│ - Iterations: const int = 10000              │
├──────────────────────────────────────────────┤
│ + HashPassword(password): string             │
│   AKILLI ALGORITMA: PBKDF2                   │
│   - Random salt generation                   │
│   - 10,000 iterations                        │
│                                              │
│ + VerifyPassword(pwd, hash): bool            │
│   - Timing attack korumalı                   │
│                                              │
│ + IsValidPassword(pwd, out err): bool        │
│   - Length check (8-128)                     │
│   - Complexity check                         │
└──────────────────────────────────────────────┘
```

**Akıllı Algoritma:** Kriptografik hash (OWASP standart)

---

### 6.2 AuthContext (Oturum Yönetimi)

```csharp
┌──────────────────────────────────────┐
│      <<Static Class>>                │
│        AuthContext                   │
├──────────────────────────────────────┤
│ + UserId: int {get; private set}     │
│ + UserName: string {get; private}    │
│ + Role: UserRole {get; private}      │
│ + IsAuthenticated: bool {get}        │
├──────────────────────────────────────┤
│ + SignIn(id, name, role): void       │
│ + SignOut(): void                    │
│ + IsDoctor(): bool                   │
│ + IsPatient(): bool                  │
└──────────────────────────────────────┘
```

**Design Pattern:** Singleton (static), Session management

---

## 7. PRESENTATION LAYER (Forms)

### 7.1 Doctor Forms

```csharp
┌──────────────────────┐
│  FrmDoctorShell      │
│  : XtraForm          │
├──────────────────────┤
│ - sidebar            │
│ - contentPanel       │
├──────────────────────┤
│ + LoadChildForm()    │
└──────────────────────┘

┌──────────────────────┐
│  FrmPatients         │
│  : XtraForm          │
├──────────────────────┤
│ - _patientService    │
│ - gridPatients       │
├──────────────────────┤
│ + LoadPatients()     │
│ + SavePatient()      │
│ + CalculateBMI()     │
└──────────────────────┘

┌──────────────────────┐
│  FrmNotesModern      │
│  : XtraForm          │
├──────────────────────┤
│ - _noteRepository    │
├──────────────────────┤
│ + LoadNotes()        │
│ + SaveNote()         │
└──────────────────────┘

┌──────────────────────┐
│  FrmMessagesModern   │
│  : XtraForm          │
├──────────────────────┤
│ - _messageService    │
│ - _refreshTimer      │
├──────────────────────┤
│ + LoadMessages()     │
│ + SendMessage()      │
│ + RefreshMessages()  │
└──────────────────────┘
```

---

### 7.2 Patient Forms

```csharp
┌──────────────────────┐
│  FrmPatientShell     │
│  : XtraForm          │
├──────────────────────┤
│ + LoadDashboard()    │
└──────────────────────┘

┌──────────────────────┐
│  FrmWeeklyMenu       │
│  : XtraForm          │
├──────────────────────┤
│ + LoadWeek()         │
│ + ConfirmMeal()      │
└──────────────────────┘

┌──────────────────────┐
│  FrmGoals            │
│  : XtraForm          │
├──────────────────────┤
│ + LoadGoals()        │
│ + UpdateProgress()   │
└──────────────────────┘
```

**Toplam Form:** 23 adet (14 Doktor, 9 Hasta)

---

## 8. SINIF İLİŞKİLERİ (RELATIONSHIPS)

### 8.1 Inheritance (Kalıtım)

```
User
  ├── Patient (IS-A)
  └── Doctor (IS-A)

BaseRepository<T>
  ├── PatientRepository
  ├── DoctorRepository
  ├── MessageRepository
  ├── GoalRepository
  └── ... (12 more)
```

### 8.2 Composition (Bileşim)

```
DietWeek ◆──► List<DietDay>
DietDay ◆──► List<MealItem>
Patient ◆──► List<PatientAllergy>
```

### 8.3 Association (İlişki)

```
Patient ────► Doctor (N:1)
Patient ────► WeightEntry (1:N)
Patient ────► Goal (1:N)
Patient ────► DietWeek (1:N)
User ────► Message (1:N) [from/to]
```

---

## 9. DESIGN PATTERNS KULLANIMI

### 9.1 Creational Patterns
- **Singleton:** DatabaseConfig, AuthContext
- **Factory:** SecurePasswordHasher (static factory methods)

### 9.2 Structural Patterns
- **Repository Pattern:** BaseRepository + Concrete Repositories
- **Layered Architecture:** 4-tier (Domain, Data, Business, Presentation)

### 9.3 Behavioral Patterns
- **Template Method:** BaseRepository (abstract methods)
- **Service Layer:** PatientService, MessageService, etc.

---

## 10. SOLID PRENSİPLERİ

### S - Single Responsibility
✅ Her service tek bir iş mantığından sorumlu  
✅ Her repository tek bir entity'den sorumlu

### O - Open/Closed
✅ BaseRepository extensible (open for extension)  
✅ Değişiklikler için kapalı (closed for modification)

### L - Liskov Substitution
✅ Patient ve Doctor, User yerine kullanılabilir  
✅ Tüm repository'ler IRepository<T> yerine kullanılabilir

### I - Interface Segregation
✅ IRepository<T> sadece gerekli metodları içerir

### D - Dependency Inversion
✅ Services, interface'lere bağımlı (IRepository)  
✅ Forms, service'lere bağımlı (loose coupling)

---

## 11. SONUÇ

**Toplam Sınıf Sayısı:** ~70  
**Domain Models:** 19  
**Repositories:** 16  
**Services:** 11  
**Forms:** 23  
**Security:** 2  
**Shared:** 4  

**OOP Prensipleri:** ✅ Tümü uygulanmış  
**Design Patterns:** ✅ 6 adet  
**SOLID:** ✅ Tüm prensipler uygulanmış  
**Akıllı Algoritma:** ✅ 9 adet

---

**Hazırlayan:** Proje Ekibi  
**Onaylayan:** Danışman Öğretim Üyesi  
**Tarih:** 17 Ocak 2026
