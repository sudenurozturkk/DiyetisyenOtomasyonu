using System;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Öğün/Yemek modeli - Diyetisyenin oluşturduğu tüm yemekler
    /// Bu model, yemek kütüphanesi olarak kullanılır
    /// </summary>
    public class Meal
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Yemeği oluşturan diyetisyen
        /// </summary>
        public int DoctorId { get; set; }
        
        /// <summary>
        /// Yemek adı (örn: Izgara Tavuk, Mercimek Çorbası)
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Yemek içeriği/tarifi
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Öğün tipi (Kahvaltı, Öğle, Akşam, Ara Öğün)
        /// </summary>
        public MealTimeType MealTime { get; set; }
        
        /// <summary>
        /// Kalori değeri
        /// </summary>
        public double Calories { get; set; }
        
        /// <summary>
        /// Protein (gram)
        /// </summary>
        public double Protein { get; set; }
        
        /// <summary>
        /// Karbonhidrat (gram)
        /// </summary>
        public double Carbs { get; set; }
        
        /// <summary>
        /// Yağ (gram)
        /// </summary>
        public double Fat { get; set; }
        
        /// <summary>
        /// Porsiyon miktarı (gram)
        /// </summary>
        public double PortionGrams { get; set; }
        
        /// <summary>
        /// Porsiyon açıklaması (örn: 1 tabak, 2 dilim)
        /// </summary>
        public string PortionDescription { get; set; }
        
        /// <summary>
        /// Kategori (örn: Et, Sebze, Çorba, Tatlı)
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Özel notlar (alerjen bilgisi, hazırlama ipuçları)
        /// </summary>
        public string Notes { get; set; }
        
        /// <summary>
        /// Aktif mi?
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Oluşturulma tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; }

        public Meal()
        {
            CreatedAt = DateTime.Now;
            IsActive = true;
            PortionGrams = 100;
        }

        /// <summary>
        /// Öğün tipi Türkçe adı
        /// </summary>
        public string MealTimeName
        {
            get
            {
                switch (MealTime)
                {
                    case MealTimeType.Breakfast: return "Kahvalti";
                    case MealTimeType.MorningSnack: return "Kus. Ara Ogun";
                    case MealTimeType.Lunch: return "Ogle Yemegi";
                    case MealTimeType.AfternoonSnack: return "Ik. Ara Ogun";
                    case MealTimeType.Dinner: return "Aksam Yemegi";
                    case MealTimeType.EveningSnack: return "Gece Ara Ogun";
                    default: return "Diger";
                }
            }
        }

        /// <summary>
        /// Makro özeti
        /// </summary>
        public string MacroSummary => $"K:{Calories:F0} | P:{Protein:F0}g | C:{Carbs:F0}g | Y:{Fat:F0}g";
    }

    /// <summary>
    /// Öğün zamanı tipi
    /// </summary>
    public enum MealTimeType
    {
        Breakfast = 0,      // Kahvaltı
        MorningSnack = 1,   // Kuşluk (Ara Öğün 1)
        Lunch = 2,          // Öğle Yemeği
        AfternoonSnack = 3, // İkindi (Ara Öğün 2)
        Dinner = 4,         // Akşam Yemeği
        EveningSnack = 5    // Gece Ara Öğün
    }

    /// <summary>
    /// Hasta diyet planındaki öğün ataması
    /// Bir hastanın belirli bir gün ve zamandaki yemeği
    /// </summary>
    public class PatientMealAssignment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        
        /// <summary>
        /// Referans meal (kütüphaneden) - null ise custom meal
        /// </summary>
        public int? MealId { get; set; }
        
        /// <summary>
        /// Hafta başlangıç tarihi
        /// </summary>
        public DateTime WeekStartDate { get; set; }
        
        /// <summary>
        /// Haftanın günü (0=Pazartesi, 6=Pazar)
        /// </summary>
        public int DayOfWeek { get; set; }
        
        /// <summary>
        /// Öğün zamanı
        /// </summary>
        public MealTimeType MealTime { get; set; }
        
        /// <summary>
        /// Yemek adı (custom ise direkt yazılır, değilse Meal'den gelir)
        /// </summary>
        public string MealName { get; set; }
        
        /// <summary>
        /// Yemek açıklaması
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Kalori
        /// </summary>
        public double Calories { get; set; }
        
        /// <summary>
        /// Protein (g)
        /// </summary>
        public double Protein { get; set; }
        
        /// <summary>
        /// Karbonhidrat (g)
        /// </summary>
        public double Carbs { get; set; }
        
        /// <summary>
        /// Yağ (g)
        /// </summary>
        public double Fat { get; set; }
        
        /// <summary>
        /// Porsiyon (gram)
        /// </summary>
        public double PortionGrams { get; set; }
        
        /// <summary>
        /// Porsiyon açıklaması
        /// </summary>
        public string PortionDescription { get; set; }
        
        /// <summary>
        /// Hasta tarafından yenildi mi?
        /// </summary>
        public bool IsConsumed { get; set; }
        
        /// <summary>
        /// Tüketim tarihi
        /// </summary>
        public DateTime? ConsumedAt { get; set; }
        
        /// <summary>
        /// Notlar
        /// </summary>
        public string Notes { get; set; }
        
        /// <summary>
        /// Oluşturulma tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; }

        public PatientMealAssignment()
        {
            CreatedAt = DateTime.Now;
            IsConsumed = false;
            PortionGrams = 100;
        }

        /// <summary>
        /// Gün adı
        /// </summary>
        public string DayName
        {
            get
            {
                string[] days = { "Pazartesi", "Sali", "Carsamba", "Persembe", "Cuma", "Cumartesi", "Pazar" };
                return DayOfWeek >= 0 && DayOfWeek < 7 ? days[DayOfWeek] : "Bilinmiyor";
            }
        }

        /// <summary>
        /// Öğün zamanı adı
        /// </summary>
        public string MealTimeName
        {
            get
            {
                switch (MealTime)
                {
                    case MealTimeType.Breakfast: return "Kahvalti";
                    case MealTimeType.MorningSnack: return "Kus. Ara Ogun";
                    case MealTimeType.Lunch: return "Ogle Yemegi";
                    case MealTimeType.AfternoonSnack: return "Ik. Ara Ogun";
                    case MealTimeType.Dinner: return "Aksam Yemegi";
                    case MealTimeType.EveningSnack: return "Gece Ara Ogun";
                    default: return "Diger";
                }
            }
        }
    }
}

