using System;
using System.Collections.Generic;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Öğün kalemi (bir öğün içindeki yemek/besin)
    /// 
    /// OOP Principle: Encapsulation - Öğün bilgileri ve hesaplamalar kapsüllenir
    /// Academic: Meal entity with portion, timing and alternative options
    /// </summary>
    public class MealItem
    {
        public int Id { get; set; }
        public int DietDayId { get; set; }
        public MealType MealType { get; set; }
        public string Name { get; set; }
        public double Calories { get; set; }
        public double Protein { get; set; } // gram
        public double Carbs { get; set; } // gram
        public double Fat { get; set; } // gram
        
        // Gelişmiş özellikler
        public string PortionSize { get; set; }  // Örn: "150g", "1 porsiyon"
        public string TimeRange { get; set; }    // Örn: "07:00-09:00"
        
        // Hasta onay bilgileri
        public bool IsConfirmedByPatient { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string SkippedReason { get; set; }  // Atlandıysa nedeni

        // Alternatif öğünler
        private List<AlternativeMeal> _alternatives;
        public List<AlternativeMeal> Alternatives
        {
            get { return _alternatives ?? (_alternatives = new List<AlternativeMeal>()); }
            set { _alternatives = value; }
        }

        public MealItem()
        {
            IsConfirmedByPatient = false;
        }

        /// <summary>
        /// Öğün tipi Türkçe adı
        /// </summary>
        public string MealTypeName
        {
            get
            {
                switch (MealType)
                {
                    case MealType.Breakfast: return "Kahvaltı";
                    case MealType.Snack1: return "Ara Öğün 1";
                    case MealType.Lunch: return "Öğle Yemeği";
                    case MealType.Snack2: return "Ara Öğün 2";
                    case MealType.Dinner: return "Akşam Yemeği";
                    default: return "Bilinmiyor";
                }
            }
        }

        /// <summary>
        /// Öğün durumu
        /// </summary>
        public string Status
        {
            get
            {
                if (IsConfirmedByPatient) return "Yendi ✓";
                if (!string.IsNullOrEmpty(SkippedReason)) return "Atlandı";
                return "Bekliyor";
            }
        }

        /// <summary>
        /// Makro besin toplamı
        /// </summary>
        public double TotalMacros => Protein + Carbs + Fat;

        /// <summary>
        /// Protein yüzdesi
        /// </summary>
        public double ProteinPercentage => TotalMacros > 0 ? Math.Round((Protein / TotalMacros) * 100, 1) : 0;

        /// <summary>
        /// Karbonhidrat yüzdesi
        /// </summary>
        public double CarbsPercentage => TotalMacros > 0 ? Math.Round((Carbs / TotalMacros) * 100, 1) : 0;

        /// <summary>
        /// Yağ yüzdesi
        /// </summary>
        public double FatPercentage => TotalMacros > 0 ? Math.Round((Fat / TotalMacros) * 100, 1) : 0;

        /// <summary>
        /// Öğün özeti
        /// </summary>
        public string Summary => $"{Calories} kcal | P:{Protein}g K:{Carbs}g Y:{Fat}g";
    }

    /// <summary>
    /// Alternatif öğün seçeneği
    /// Hastalara farklı seçenekler sunmak için
    /// 
    /// OOP Principle: Composition - MealItem ile bileşim ilişkisi
    /// Academic: Alternative meals for flexibility
    /// </summary>
    public class AlternativeMeal
    {
        public int Id { get; set; }
        public int MealItemId { get; set; }
        public string Name { get; set; }
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
        public string PortionSize { get; set; }

        /// <summary>
        /// Özet bilgi
        /// </summary>
        public string Summary => $"{Name} ({Calories} kcal)";
    }
}
