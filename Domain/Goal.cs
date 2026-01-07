using System;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Hasta hedefleri
    /// </summary>
    public class Goal
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public GoalType GoalType { get; set; }
        public double TargetValue { get; set; }
        public double CurrentValue { get; set; }
        public string Unit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Goal()
        {
            StartDate = DateTime.Now;
            UpdatedAt = DateTime.Now;
            IsActive = true;
            CurrentValue = 0;
        }

        /// <summary>
        /// Hedef tamamlanma yüzdesi
        /// </summary>
        public double ProgressPercentage
        {
            get
            {
                if (TargetValue <= 0) return 0;
                return Math.Round((CurrentValue / TargetValue) * 100, 2);
            }
        }

        /// <summary>
        /// Hedef tipi Türkçe adı
        /// </summary>
        public string GoalTypeName
        {
            get
            {
                switch (GoalType)
                {
                    case GoalType.Weight: return "Kilo";
                    case GoalType.Water: return "Su";
                    case GoalType.Steps: return "Adım";
                    case GoalType.Exercise: return "Egzersiz";
                    case GoalType.Sleep: return "Uyku";
                    case GoalType.Protein: return "Protein";
                    default: return "Diğer";
                }
            }
        }

        /// <summary>
        /// Hedef durumu
        /// </summary>
        public string Status
        {
            get
            {
                if (!IsActive) return "Pasif";
                double progress = ProgressPercentage;
                if (progress >= 100) return "Tamamlandı";
                if (progress >= 80) return "İyi";
                if (progress >= 50) return "Devam";
                if (progress < 50 && EndDate.HasValue && DateTime.Now > EndDate.Value.AddDays(-7)) return "Riskli";
                if (StartDate.AddDays(7) > DateTime.Now) return "New";
                return "Devam";
            }
        }
    }
}
