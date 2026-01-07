using System;
using System.Collections.Generic;
using System.Linq;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// Diyet yönetim servisi
    /// 
    /// OOP Principle: Single Responsibility - Diyet plan işlemlerinden sorumlu
    /// OOP Principle: Encapsulation - İş mantığı kapsüllenir
    /// Design Pattern: Service Layer - İş mantığı UI'dan ayrılır
    /// </summary>
    public class DietService
    {
        private readonly DietRepository _dietRepository;

        public DietService()
        {
            _dietRepository = new DietRepository();
        }

        /// <summary>
        /// Hastanın haftalık planını getir
        /// </summary>
        public DietWeek GetWeeklyPlan(int patientId, DateTime weekStart)
        {
            return _dietRepository.GetWeeklyPlan(patientId, weekStart);
        }

        /// <summary>
        /// Haftalık plan oluştur veya güncelle
        /// </summary>
        public DietWeek CreateOrUpdateWeeklyPlan(int patientId, DateTime weekStart, int doctorId)
        {
            return _dietRepository.CreateOrUpdateWeeklyPlan(patientId, weekStart, doctorId);
        }

        /// <summary>
        /// Öğün ekle
        /// </summary>
        public MealItem AddMeal(int dietDayId, MealType mealType, string name,
            double calories, double protein, double carbs, double fat,
            string portionSize = null, string timeRange = null)
        {
            var meal = new MealItem
            {
                DietDayId = dietDayId,
                MealType = mealType,
                Name = name,
                Calories = calories,
                Protein = protein,
                Carbs = carbs,
                Fat = fat,
                PortionSize = portionSize,
                TimeRange = timeRange
            };

            int mealId = _dietRepository.AddMeal(meal);
            meal.Id = mealId;

            return meal;
        }

        /// <summary>
        /// Öğün güncelle
        /// </summary>
        public bool UpdateMeal(MealItem meal)
        {
            return _dietRepository.UpdateMeal(meal);
        }

        /// <summary>
        /// Öğün sil
        /// </summary>
        public void DeleteMeal(int mealId)
        {
            _dietRepository.DeleteMeal(mealId);
        }

        /// <summary>
        /// Günün tüm öğünlerini başka güne kopyala
        /// </summary>
        public void CopyDayMeals(int sourceDayId, int targetDayId)
        {
            _dietRepository.CopyDayMeals(sourceDayId, targetDayId);
        }

        /// <summary>
        /// Öğün onayı (hasta tarafından)
        /// </summary>
        public void ConfirmMeal(int mealId, bool isConfirmed, string skippedReason = null)
        {
            _dietRepository.ConfirmMeal(mealId, isConfirmed, skippedReason);
        }

        /// <summary>
        /// Günlük planı getir
        /// </summary>
        public DietDay GetDayPlan(int dayId)
        {
            return _dietRepository.GetDayPlan(dayId);
        }

        /// <summary>
        /// Hastanın tüm haftalık planlarını getir
        /// </summary>
        public List<DietWeek> GetPatientAllWeeks(int patientId)
        {
            return _dietRepository.GetPatientAllWeeks(patientId).ToList();
        }

        /// <summary>
        /// Haftalık diyet uyum skorunu hesapla
        /// </summary>
        public double CalculateWeeklyComplianceScore(int weekId)
        {
            var week = _dietRepository.GetWeeklyPlanById(weekId);
            if (week == null) return 0;

            return week.WeeklyCompletionRate;
        }

        /// <summary>
        /// Günlük kalori özetini hesapla
        /// </summary>
        public DailyNutritionSummary GetDailyNutritionSummary(int dayId)
        {
            var day = _dietRepository.GetDayPlan(dayId);
            if (day == null) return null;

            return new DailyNutritionSummary
            {
                DayId = dayId,
                Date = day.Date,
                TotalCalories = day.TotalCalories,
                TotalProtein = day.TotalProtein,
                TotalCarbs = day.TotalCarbs,
                TotalFat = day.TotalFat,
                TargetCalories = day.TargetCalories,
                CompletionRate = day.CompletionRate,
                MealCount = day.Meals.Count,
                CompletedMealCount = day.Meals.Count(m => m.IsConfirmedByPatient)
            };
        }

        /// <summary>
        /// Haftalık makro dağılımını hesapla
        /// </summary>
        public WeeklyMacroDistribution GetWeeklyMacroDistribution(int weekId)
        {
            var week = _dietRepository.GetWeeklyPlanById(weekId);
            if (week == null) return null;

            double totalProtein = week.WeeklyTotalProtein;
            double totalCarbs = week.WeeklyTotalCarbs;
            double totalFat = week.WeeklyTotalFat;
            double totalMacros = totalProtein + totalCarbs + totalFat;

            return new WeeklyMacroDistribution
            {
                WeekId = weekId,
                WeekStartDate = week.WeekStartDate,
                TotalProtein = totalProtein,
                TotalCarbs = totalCarbs,
                TotalFat = totalFat,
                ProteinPercentage = totalMacros > 0 ? Math.Round(totalProtein / totalMacros * 100, 1) : 0,
                CarbsPercentage = totalMacros > 0 ? Math.Round(totalCarbs / totalMacros * 100, 1) : 0,
                FatPercentage = totalMacros > 0 ? Math.Round(totalFat / totalMacros * 100, 1) : 0,
                AverageDailyCalories = week.AverageDailyCalories
            };
        }
    }

    /// <summary>
    /// Günlük besin özeti
    /// </summary>
    public class DailyNutritionSummary
    {
        public int DayId { get; set; }
        public DateTime Date { get; set; }
        public double TotalCalories { get; set; }
        public double TotalProtein { get; set; }
        public double TotalCarbs { get; set; }
        public double TotalFat { get; set; }
        public double TargetCalories { get; set; }
        public double CompletionRate { get; set; }
        public int MealCount { get; set; }
        public int CompletedMealCount { get; set; }

        public double CalorieDeviation => TotalCalories - TargetCalories;
        public bool IsOnTarget => Math.Abs(CalorieDeviation) <= TargetCalories * 0.1; // %10 tolerans
    }

    /// <summary>
    /// Haftalık makro dağılımı
    /// </summary>
    public class WeeklyMacroDistribution
    {
        public int WeekId { get; set; }
        public DateTime WeekStartDate { get; set; }
        public double TotalProtein { get; set; }
        public double TotalCarbs { get; set; }
        public double TotalFat { get; set; }
        public double ProteinPercentage { get; set; }
        public double CarbsPercentage { get; set; }
        public double FatPercentage { get; set; }
        public double AverageDailyCalories { get; set; }

        /// <summary>
        /// Makro dağılımı dengeli mi?
        /// İdeal: Protein %25-35, Karb %40-50, Yağ %20-30
        /// </summary>
        public bool IsBalanced
        {
            get
            {
                return ProteinPercentage >= 20 && ProteinPercentage <= 40 &&
                       CarbsPercentage >= 35 && CarbsPercentage <= 55 &&
                       FatPercentage >= 15 && FatPercentage <= 35;
            }
        }

        public string BalanceDescription
        {
            get
            {
                if (IsBalanced) return "Dengeli";
                if (ProteinPercentage < 20) return "Protein Yetersiz";
                if (ProteinPercentage > 40) return "Protein Fazla";
                if (CarbsPercentage < 35) return "Karbonhidrat Yetersiz";
                if (CarbsPercentage > 55) return "Karbonhidrat Fazla";
                if (FatPercentage < 15) return "Yağ Yetersiz";
                if (FatPercentage > 35) return "Yağ Fazla";
                return "Kontrol Gerekli";
            }
        }
    }
}
