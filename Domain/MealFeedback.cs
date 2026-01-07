using System;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Öğün geri bildirimi - Hasta öğünü yedi mi, yemediyse neden
    /// </summary>
    public class MealFeedback
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int? MealAssignmentId { get; set; }
        
        public DateTime Date { get; set; }
        public MealTime MealTime { get; set; }      // Kahvaltı, öğle, akşam, ara öğün
        
        public bool IsConsumed { get; set; }        // Yedim / Yemedim
        public string Reason { get; set; }          // Sapma sebebi: "dışarıda yedim", "aç değildim"
        public string Notes { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Öğün bilgisi
        public string MealName { get; set; }
        
        public string MealTimeText
        {
            get
            {
                switch (MealTime)
                {
                    case MealTime.Breakfast: return "Kahvaltı";
                    case MealTime.Lunch: return "Öğle Yemeği";
                    case MealTime.Dinner: return "Akşam Yemeği";
                    case MealTime.Snack: return "Ara Öğün";
                    default: return "Diğer";
                }
            }
        }
        
        public string StatusText => IsConsumed ? "✅ Yedim" : "❌ Yemedim";
    }
    
    /// <summary>
    /// Öğün zamanı enum
    /// </summary>
    public enum MealTime
    {
        Breakfast = 0,  // Kahvaltı
        Lunch = 1,      // Öğle
        Dinner = 2,     // Akşam
        Snack = 3       // Ara öğün
    }
}
