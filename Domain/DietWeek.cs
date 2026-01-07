using System;
using System.Collections.Generic;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Haftalık diyet planı
    /// 
    /// OOP Principle: Composition - DietDay listesi içerir
    /// OOP Principle: Encapsulation - Hesaplamalar kapsüllenir
    /// Academic: Weekly diet plan with version control
    /// </summary>
    public class DietWeek
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime WeekStartDate { get; set; }
        public List<DietDay> Days { get; set; }
        public string WeekNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedByDoctorId { get; set; }
        
        // Gelişmiş özellikler
        public int Version { get; set; }  // Diyet versiyonu (revizyon takibi)
        public bool IsActive { get; set; }

        public DietWeek()
        {
            Days = new List<DietDay>();
            CreatedAt = DateTime.Now;
            Version = 1;
            IsActive = true;
        }

        /// <summary>
        /// Hafta bitiş tarihi
        /// </summary>
        public DateTime WeekEndDate
        {
            get { return WeekStartDate.AddDays(6); }
        }

        /// <summary>
        /// Hafta görüntü adı
        /// </summary>
        public string WeekDisplayName
        {
            get
            {
                return $"{WeekStartDate:dd.MM} - {WeekEndDate:dd.MM.yyyy}";
            }
        }

        /// <summary>
        /// Haftalık toplam kalori
        /// </summary>
        public double WeeklyTotalCalories
        {
            get
            {
                double total = 0;
                foreach (var day in Days)
                    total += day.TotalCalories;
                return Math.Round(total, 2);
            }
        }

        /// <summary>
        /// Haftalık ortalama günlük kalori
        /// </summary>
        public double AverageDailyCalories
        {
            get
            {
                if (Days.Count == 0) return 0;
                return Math.Round(WeeklyTotalCalories / Days.Count, 2);
            }
        }

        /// <summary>
        /// Haftalık tamamlanma oranı (%)
        /// </summary>
        public double WeeklyCompletionRate
        {
            get
            {
                if (Days.Count == 0) return 0;
                double total = 0;
                foreach (var day in Days)
                    total += day.CompletionRate;
                return Math.Round(total / Days.Count, 2);
            }
        }

        /// <summary>
        /// Toplam öğün sayısı
        /// </summary>
        public int TotalMealCount
        {
            get
            {
                int count = 0;
                foreach (var day in Days)
                    count += day.Meals.Count;
                return count;
            }
        }

        /// <summary>
        /// Tamamlanan öğün sayısı
        /// </summary>
        public int CompletedMealCount
        {
            get
            {
                int count = 0;
                foreach (var day in Days)
                {
                    foreach (var meal in day.Meals)
                    {
                        if (meal.IsConfirmedByPatient) count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Haftalık toplam protein (gram)
        /// </summary>
        public double WeeklyTotalProtein
        {
            get
            {
                double total = 0;
                foreach (var day in Days)
                    total += day.TotalProtein;
                return Math.Round(total, 2);
            }
        }

        /// <summary>
        /// Haftalık toplam karbonhidrat (gram)
        /// </summary>
        public double WeeklyTotalCarbs
        {
            get
            {
                double total = 0;
                foreach (var day in Days)
                    total += day.TotalCarbs;
                return Math.Round(total, 2);
            }
        }

        /// <summary>
        /// Haftalık toplam yağ (gram)
        /// </summary>
        public double WeeklyTotalFat
        {
            get
            {
                double total = 0;
                foreach (var day in Days)
                    total += day.TotalFat;
                return Math.Round(total, 2);
            }
        }

        /// <summary>
        /// Ortalama günlük protein (gram)
        /// </summary>
        public double AverageDailyProtein
        {
            get
            {
                if (Days.Count == 0) return 0;
                return Math.Round(WeeklyTotalProtein / Days.Count, 2);
            }
        }

        /// <summary>
        /// Ortalama günlük karbonhidrat (gram)
        /// </summary>
        public double AverageDailyCarbs
        {
            get
            {
                if (Days.Count == 0) return 0;
                return Math.Round(WeeklyTotalCarbs / Days.Count, 2);
            }
        }

        /// <summary>
        /// Ortalama günlük yağ (gram)
        /// </summary>
        public double AverageDailyFat
        {
            get
            {
                if (Days.Count == 0) return 0;
                return Math.Round(WeeklyTotalFat / Days.Count, 2);
            }
        }
    }
}
