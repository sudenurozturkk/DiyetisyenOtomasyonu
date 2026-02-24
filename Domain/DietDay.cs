using System;
using System.Collections.Generic;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Günlük diyet planı
    /// </summary>
    public class DietDay
    {
        public int Id { get; set; }
        public int DietWeekId { get; set; }
        public DateTime Date { get; set; }
        public List<MealItem> Meals { get; set; }
        public string Notes { get; set; }
        
        // Gelişmiş özellikler
        public double TargetCalories { get; set; }  // Hedef kalori

        public DietDay()
        {
            Meals = new List<MealItem>();
        }

        /// <summary>
        /// Gün adı (Pazartesi, Salı, vb.)
        /// </summary>
        public string DayName
        {
            get
            {
                string[] days = { "Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi" };
                return days[(int)Date.DayOfWeek];
            }
        }

        /// <summary>
        /// Tarih formatı
        /// </summary>
        public string DateDisplay
        {
            get { return Date.ToString("dd.MM.yyyy"); }
        }

        /// <summary>
        /// Günlük toplam kalori
        /// </summary>
        public double TotalCalories
        {
            get
            {
                double total = 0;
                foreach (var meal in Meals)
                    total += meal.Calories;
                return Math.Round(total, 2);
            }
        }

        /// <summary>
        /// Günlük toplam protein
        /// </summary>
        public double TotalProtein
        {
            get
            {
                double total = 0;
                foreach (var meal in Meals)
                    total += meal.Protein;
                return Math.Round(total, 2);
            }
        }

        /// <summary>
        /// Günlük toplam karbonhidrat
        /// </summary>
        public double TotalCarbs
        {
            get
            {
                double total = 0;
                foreach (var meal in Meals)
                    total += meal.Carbs;
                return Math.Round(total, 2);
            }
        }

        /// <summary>
        /// Günlük toplam yağ
        /// </summary>
        public double TotalFat
        {
            get
            {
                double total = 0;
                foreach (var meal in Meals)
                    total += meal.Fat;
                return Math.Round(total, 2);
            }
        }

        /// <summary>
        /// Hasta öğün onay oranı (%)
        /// </summary>
        public double CompletionRate
        {
            get
            {
                if (Meals.Count == 0) return 0;
                int confirmed = 0;
                foreach (var meal in Meals)
                    if (meal.IsConfirmedByPatient) confirmed++;
                return Math.Round((double)confirmed / Meals.Count * 100, 2);
            }
        }

        /// <summary>
        /// Kalori hedefine ulaşım yüzdesi
        /// </summary>
        public double CalorieTargetPercentage
        {
            get
            {
                if (TargetCalories <= 0) return 0;
                return Math.Round((TotalCalories / TargetCalories) * 100, 2);
            }
        }

        /// <summary>
        /// Kalori sapması (hedeften fark)
        /// </summary>
        public double CalorieDeviation
        {
            get
            {
                if (TargetCalories <= 0) return 0;
                return Math.Round(TotalCalories - TargetCalories, 2);
            }
        }

        /// <summary>
        /// Protein yüzdesi (makro dağılımı)
        /// </summary>
        public double ProteinPercentage
        {
            get
            {
                double totalMacros = TotalProtein + TotalCarbs + TotalFat;
                if (totalMacros <= 0) return 0;
                return Math.Round((TotalProtein / totalMacros) * 100, 1);
            }
        }

        /// <summary>
        /// Karbonhidrat yüzdesi (makro dağılımı)
        /// </summary>
        public double CarbsPercentage
        {
            get
            {
                double totalMacros = TotalProtein + TotalCarbs + TotalFat;
                if (totalMacros <= 0) return 0;
                return Math.Round((TotalCarbs / totalMacros) * 100, 1);
            }
        }

        /// <summary>
        /// Yağ yüzdesi (makro dağılımı)
        /// </summary>
        public double FatPercentage
        {
            get
            {
                double totalMacros = TotalProtein + TotalCarbs + TotalFat;
                if (totalMacros <= 0) return 0;
                return Math.Round((TotalFat / totalMacros) * 100, 1);
            }
        }

        /// <summary>
        /// Bugün mü?
        /// </summary>
        public bool IsToday
        {
            get { return Date.Date == DateTime.Today; }
        }

        /// <summary>
        /// Geçmiş bir gün mü?
        /// </summary>
        public bool IsPast
        {
            get { return Date.Date < DateTime.Today; }
        }

        /// <summary>
        /// Gelecek bir gün mü?
        /// </summary>
        public bool IsFuture
        {
            get { return Date.Date > DateTime.Today; }
        }

        /// <summary>
        /// Özet bilgi
        /// </summary>
        public string Summary
        {
            get
            {
                return $"{DayName} - {TotalCalories} kcal ({CompletionRate}% tamamlandı)";
            }
        }
    }
}
