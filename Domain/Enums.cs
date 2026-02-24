using System;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Kullanıcı rolleri: Doktor veya Hasta
    /// </summary>
    public enum UserRole
    {
        Doctor = 0,
        Patient = 1
    }

    /// <summary>
    /// Öğün tipleri: Kahvaltı, Ara Öğün, Öğle Yemeği, vb.
    /// </summary>
    public enum MealType
    {
        Breakfast = 0,  // Kahvaltı
        Snack1 = 1,     // Ara Öğün 1
        Lunch = 2,      // Öğle Yemeği
        Snack2 = 3,     // Ara Öğün 2
        Dinner = 4,     // Akşam Yemeği
        Snack = 5       // Genel Ara Öğün
    }

    /// <summary>
    /// Hedef tipleri: Su, Kilo, Adım, Uyku, Protein
    /// </summary>
    public enum GoalType
    {
        Water = 0,      // Su
        Weight = 1,     // Kilo
        Steps = 2,      // Adım
        Sleep = 3,      // Uyku
        Protein = 4,    // Protein
        Calories = 5,   // Kalori
        Exercise = 6    // Egzersiz
    }

    /// <summary>
    /// Yaşam tarzı tipleri - diyet planlaması için önemli
    /// </summary>
    public enum LifestyleType
    {
        Student = 0,        // Öğrenci
        OfficeWorker = 1,   // Ofis çalışanı
        NightShift = 2,     // Gece vardiyası
        Athlete = 3,        // Sporcu
        HomeMaker = 4,      // Ev hanımı
        Retired = 5,        // Emekli
        Freelancer = 6      // Serbest çalışan
    }

    /// <summary>
    /// Aktivite seviyeleri - kalori hesaplaması için
    /// </summary>
    public enum ActivityLevel
    {
        Sedentary = 0,      // Hareketsiz (1.2)
        LightlyActive = 1,  // Hafif aktif (1.375)
        ModeratelyActive = 2, // Orta aktif (1.55)
        VeryActive = 3,     // Çok aktif (1.725)
        ExtraActive = 4     // Aşırı aktif (1.9)
    }

    /// <summary>
    /// Mesaj kategorileri
    /// </summary>
    public enum MessageCategory
    {
        General = 0,        // Genel
        Question = 1,       // Soru
        Emergency = 2,      // Acil
        Information = 3,    // Bilgi
        Feedback = 4,       // Geri bildirim
        Appointment = 5     // Randevu
    }

    /// <summary>
    /// Mesaj öncelik seviyeleri
    /// </summary>
    public enum MessagePriority
    {
        Low = 0,            // Düşük
        Normal = 1,         // Normal
        High = 2,           // Yüksek
        Urgent = 3          // Acil
    }

    /// <summary>
    /// Alerji şiddet seviyeleri
    /// </summary>
    public enum AllergySeverity
    {
        Mild = 0,           // Hafif
        Moderate = 1,       // Orta
        Severe = 2,         // Şiddetli
        LifeThreatening = 3 // Hayati tehlike
    }

    /// <summary>
    /// Risk uyarı tipleri - AI analiz için
    /// </summary>
    public enum RiskAlertType
    {
        WeightPlateau = 0,      // Kilo platosunda
        RapidWeightLoss = 1,    // Hızlı kilo kaybı
        RapidWeightGain = 2,    // Hızlı kilo alımı
        LowCompliance = 3,      // Düşük uyum
        MissedMeals = 4,        // Atlanan öğünler
        InsufficientWater = 5,  // Yetersiz su tüketimi
        HighCalorieDeviation = 6, // Yüksek kalori sapması
        LowProteinIntake = 7    // Düşük protein alımı
    }

    /// <summary>
    /// AI analiz tipleri
    /// </summary>
    public enum AIAnalysisType
    {
        WeightTrend = 0,        // Kilo trendi
        DietCompliance = 1,     // Diyet uyumu
        NutrientBalance = 2,    // Besin dengesi
        RiskAssessment = 3,     // Risk değerlendirmesi
        MealSuggestion = 4,     // Öğün önerisi
        MotivationMessage = 5   // Motivasyon mesajı
    }



    /// <summary>
    /// Not kategorileri
    /// </summary>
    public enum NoteCategory
    {
        General = 0,        // Genel
        Nutrition = 1,      // Beslenme
        Medical = 2,        // Tıbbi
        Exercise = 3,       // Egzersiz
        Psychological = 4,  // Psikolojik
        Progress = 5        // İlerleme
    }
}
