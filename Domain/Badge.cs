using System;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Rozet (Badge) Entity - Gamification için
    /// Hasta başarılarını ödüllendirmek için rozet sistemi
    /// </summary>
    public class Badge
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public BadgeType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public DateTime EarnedDate { get; set; }
        public int Progress { get; set; } // 0-100
        public bool IsEarned { get; set; }

        public Badge()
        {
            EarnedDate = DateTime.Now;
            Progress = 0;
            IsEarned = false;
        }
    }

    /// <summary>
    /// Rozet tipleri
    /// </summary>
    public enum BadgeType
    {
        // Uyum Rozetleri
        PerfectWeek = 1,        // 7 gün mükemmel uyum
        PerfectMonth = 2,       // 30 gün mükemmel uyum
        Consistent = 3,          // 14 gün ardışık uyum
        
        // Hedef Rozetleri
        GoalAchiever = 10,      // İlk hedef tamamlama
        GoalMaster = 11,        // 5 hedef tamamlama
        GoalLegend = 12,        // 10 hedef tamamlama
        
        // Kilo Rozetleri
        FirstKilo = 20,         // İlk kilo kaybı
        FiveKilo = 21,          // 5 kilo kaybı
        TenKilo = 22,           // 10 kilo kaybı
        TwentyKilo = 23,        // 20 kilo kaybı
        
        // Aktivite Rozetleri
        StepChampion = 30,      // 10.000 adım günlük
        ExerciseWarrior = 31,   // 7 gün egzersiz
        WaterDrinker = 32,      // Günlük su hedefi
        
        // Özel Rozetler
        Comeback = 40,          // Geri dönüş (uzun süre sonra)
        Motivator = 41,         // Başkalarına ilham veren
        EarlyBird = 42,         // Erken başlangıç
        NightOwl = 43           // Gece çalışan
    }
}
