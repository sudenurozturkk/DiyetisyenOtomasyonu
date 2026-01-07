# Sınıf Diyagramları - Diyetisyen Otomasyon Sistemi

## Genel Mimari

Proje katmanlı mimari (Layered Architecture) kullanmaktadır:
- **Domain Layer**: İş mantığı ve entity'ler
- **Infrastructure Layer**: Veri erişimi ve servisler
- **Presentation Layer**: Windows Forms UI

---

## Domain Layer Sınıf Diyagramı

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
        +double BaslangicKilosu
        +double GuncelKilo
        +int DoctorId
        +string ThyroidStatus
        +string InsulinStatus
        +LifestyleType LifestyleType
        +ActivityLevel ActivityLevel
        +List~PatientAllergy~ Allergies
        +double BMI
        +double BMR
        +double TDEE
        +string BMIKategori
        +string IdealKiloAraligi
    }
    
    class Doctor {
        +string UzmanlikAlani
        +string DiplomaNo
        +string CalistigiKurum
    }
    
    class Meal {
        +int Id
        +string Ad
        +double Kalori
        +double Protein
        +double Karbonhidrat
        +double Yag
        +string Kategori
        +string Tarif
        +int OlusturanDoktorId
    }
    
    class Goal {
        +int Id
        +int PatientId
        +string Baslik
        +string Aciklama
        +double HedefDeger
        +double MevcutDeger
        +string Birim
        +DateTime BaslangicTarihi
        +DateTime BitisTarihi
        +bool TamamlandiMi
        +double IlerlemeYuzdesi
    }
    
    class Message {
        +int Id
        +int GonderenId
        +int AliciId
        +string Icerik
        +DateTime GonderimTarihi
        +bool OkunduMu
        +MessageCategory Category
        +MessagePriority Priority
    }
    
    class WeightEntry {
        +int Id
        +int PatientId
        +double Kilo
        +DateTime Tarih
        +string Notlar
    }
    
    class Note {
        +int Id
        +int PatientId
        +int DoctorId
        +string Icerik
        +DateTime Tarih
        +NoteCategory Category
    }
    
    class PatientAllergy {
        +int Id
        +int PatientId
        +string AllergyType
        +AllergySeverity Severity
    }

    User <|-- Patient : inherits
    User <|-- Doctor : inherits
    Patient "1" *-- "*" PatientAllergy : has
    Patient "1" o-- "*" WeightEntry : tracks
    Patient "1" o-- "*" Goal : has
    Patient "1" o-- "*" Note : has
    Doctor "1" o-- "*" Patient : manages
    Doctor "1" o-- "*" Meal : creates
    User "1" o-- "*" Message : sends/receives
```

---

## Infrastructure Layer - Repository Pattern

```mermaid
classDiagram
    class IRepository~T~ {
        <<interface>>
        +T GetById(int id)
        +IEnumerable~T~ GetAll()
        +IEnumerable~T~ Find(Func predicate)
        +T FirstOrDefault(Func predicate)
        +int Add(T entity)
        +bool Update(T entity)
        +bool Delete(int id)
        +int Count()
        +bool Any(Func predicate)
    }
    
    class BaseRepository~T~ {
        <<abstract>>
        #DatabaseConfig _dbConfig
        #MySqlConnection CreateConnection()
        #abstract Dictionary MapToParameters(T entity)
        #abstract string GetInsertSql()
        #abstract string GetUpdateSql()
        #IEnumerable~T~ ExecuteQuery(string sql)
        #object ExecuteScalar(string sql)
        #int ExecuteNonQuery(string sql)
    }
    
    class PatientRepository {
        +Patient GetById(int id)
        +IEnumerable~Patient~ GetByDoctorId(int doctorId)
        +bool UpdateWeight(int patientId, double weight)
    }
    
    class MealRepository {
        +IEnumerable~Meal~ GetByCategory(string category)
        +IEnumerable~Meal~ GetByDoctorId(int doctorId)
    }
    
    class MessageRepository {
        +IEnumerable~Message~ GetConversation(int user1, int user2)
        +int GetUnreadCount(int userId)
        +bool MarkAsRead(int messageId)
    }
    
    class GoalRepository {
        +IEnumerable~Goal~ GetActiveGoals(int patientId)
        +bool UpdateProgress(int goalId, double value)
    }

    IRepository~T~ <|.. BaseRepository~T~ : implements
    BaseRepository~T~ <|-- PatientRepository : extends
    BaseRepository~T~ <|-- MealRepository : extends
    BaseRepository~T~ <|-- MessageRepository : extends
    BaseRepository~T~ <|-- GoalRepository : extends
```

---

## Service Layer

```mermaid
classDiagram
    class PatientService {
        -PatientRepository _patientRepo
        -WeightEntryRepository _weightRepo
        +Patient GetPatientWithDetails(int id)
        +bool RecordWeight(int patientId, double weight)
        +PatientSummary GetSummary(int doctorId)
    }
    
    class AiAssistantService {
        -PatientRepository _patientRepo
        -MealRepository _mealRepo
        +DailyTip GenerateDailyTip(Patient patient)
        +DietComplianceAnalysis AnalyzeDietCompliance(int patientId)
        +WeightTrendAnalysis AnalyzeWeightTrend(int patientId)
        +string GenerateMotivationMessage(Patient patient)
        +MealCompensationSuggestion SuggestMealCompensation(string mealType)
        +string GetAIResponse(string question)
    }
    
    class DietService {
        -DietRepository _dietRepo
        -MealRepository _mealRepo
        +DietWeek GetCurrentWeek(int patientId)
        +bool AssignMeal(int patientId, DateTime date, MealType type, int mealId)
    }
    
    class MessageService {
        -MessageRepository _messageRepo
        +bool SendMessage(int from, int to, string content)
        +IEnumerable~Message~ GetConversation(int user1, int user2)
        +int GetUnreadCount(int userId)
    }

    PatientService --> PatientRepository : uses
    AiAssistantService --> PatientRepository : uses
    AiAssistantService --> MealRepository : uses
    DietService --> MealRepository : uses
    MessageService --> MessageRepository : uses
```

---

## Tasarım Desenleri

| Desen | Kullanım Yeri | Açıklama |
|-------|---------------|----------|
| **Repository Pattern** | Infrastructure/Repositories | Veri erişim katmanı soyutlama |
| **Template Method** | BaseRepository | Ortak CRUD operasyonları |
| **Inheritance** | User → Patient/Doctor | Domain modeli kalıtım |
| **Composition** | Patient ↔ PatientAllergy | Has-a ilişkisi |
| **Singleton** | AuthContext | Oturum yönetimi |
| **Strategy** | AiAssistantService | Farklı analiz algoritmaları |

---

## OOP Prensipleri Uygulaması

### Encapsulation (Kapsülleme)
```csharp
// Patient.cs - BMI hesaplama mantığı kapsüllenmiş
public double BMI
{
    get
    {
        if (Boy <= 0) return 0;
        return Math.Round(GuncelKilo / Math.Pow(Boy / 100, 2), 2);
    }
}
```

### Inheritance (Kalıtım)
```csharp
// Patient ve Doctor, User'dan miras alır
public class Patient : User { ... }
public class Doctor : User { ... }
```

### Polymorphism (Çok Biçimlilik)
```csharp
// IRepository<T> farklı tipler için farklı davranış
public interface IRepository<T> where T : class
{
    T GetById(int id);
    // ...
}
```

### Abstraction (Soyutlama)
```csharp
// BaseRepository abstract metotlar tanımlar
protected abstract string GetInsertSql();
protected abstract string GetUpdateSql();
```

---

## Sequence Diyagramları

### Hasta Giriş Akışı

```mermaid
sequenceDiagram
    participant U as Kullanıcı
    participant L as FrmLogin
    participant UR as UserRepository
    participant PH as PasswordHasher
    participant AC as AuthContext
    
    U->>L: Kullanıcı adı ve şifre girer
    L->>UR: GetByUsername(kullaniciAdi)
    UR-->>L: User objesi döner
    L->>PH: VerifyPassword(girilen, hash)
    PH-->>L: true/false
    
    alt Doğrulama başarılı
        L->>AC: SignIn(user)
        AC-->>L: Oturum açıldı
        L-->>U: Ana form açılır
    else Doğrulama başarısız
        L-->>U: Hata mesajı
    end
```

### AI Analiz Akışı

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

### Mesaj Gönderme Akışı

```mermaid
sequenceDiagram
    participant S as Gönderen
    participant F as FrmMessages
    participant MS as MessageService
    participant MR as MessageRepository
    
    S->>F: Mesaj yazar
    S->>F: Gönder butonuna tıklar
    F->>MS: SendMessage(from, to, content)
    MS->>MR: Add(message)
    MR-->>MS: messageId
    MS-->>F: true
    F-->>S: Mesaj gönderildi bildirimi
```

