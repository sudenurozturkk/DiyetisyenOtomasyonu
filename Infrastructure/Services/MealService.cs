using System;
using System.Collections.Generic;
using System.Linq;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// Öğün/Yemek yönetim servisi
    /// </summary>
    public class MealService
    {
        private readonly MealRepository _mealRepository;
        private readonly PatientMealAssignmentRepository _assignmentRepository;

        public MealService()
        {
            _mealRepository = new MealRepository();
            _assignmentRepository = new PatientMealAssignmentRepository();
        }

        #region Meal Library (Yemek Kütüphanesi)

        /// <summary>
        /// Yeni yemek ekle (kütüphaneye)
        /// </summary>
        public Meal CreateMeal(int doctorId, string name, MealTimeType mealTime, 
            double calories, double protein, double carbs, double fat,
            double portionGrams = 100, string portionDesc = null, 
            string category = null, string description = null, string notes = null)
        {
            var meal = new Meal
            {
                DoctorId = doctorId,
                Name = name,
                MealTime = mealTime,
                Calories = calories,
                Protein = protein,
                Carbs = carbs,
                Fat = fat,
                PortionGrams = portionGrams,
                PortionDescription = portionDesc,
                Category = category,
                Description = description,
                Notes = notes
            };

            int mealId = _mealRepository.Add(meal);
            meal.Id = mealId;

            return meal;
        }

        /// <summary>
        /// Yemek güncelle
        /// </summary>
        public void UpdateMeal(Meal meal)
        {
            _mealRepository.Update(meal);
        }

        /// <summary>
        /// Yemek sil
        /// </summary>
        public void DeleteMeal(int mealId)
        {
            _mealRepository.Delete(mealId);
        }

        /// <summary>
        /// ID'ye göre yemek getir
        /// </summary>
        public Meal GetMealById(int mealId)
        {
            return _mealRepository.GetById(mealId);
        }

        /// <summary>
        /// Diyetisyenin tüm yemeklerini getir
        /// </summary>
        public List<Meal> GetDoctorMeals(int doctorId)
        {
            return _mealRepository.GetByDoctorId(doctorId);
        }

        /// <summary>
        /// Öğün zamanına göre yemekleri getir
        /// </summary>
        public List<Meal> GetMealsByTime(int doctorId, MealTimeType mealTime)
        {
            return _mealRepository.GetByMealTime(doctorId, mealTime);
        }

        /// <summary>
        /// Yemek ara
        /// </summary>
        public List<Meal> SearchMeals(int doctorId, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return GetDoctorMeals(doctorId);
            return _mealRepository.Search(doctorId, searchText);
        }

        /// <summary>
        /// Kategorileri getir
        /// </summary>
        public List<string> GetCategories(int doctorId)
        {
            var meals = GetDoctorMeals(doctorId);
            return meals
                .Where(m => !string.IsNullOrEmpty(m.Category))
                .Select(m => m.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        #endregion

        #region Meal Assignments (Hasta Öğün Atamaları)

        /// <summary>
        /// Hastaya öğün ata (kütüphaneden)
        /// </summary>
        public PatientMealAssignment AssignMealFromLibrary(int patientId, int doctorId, int mealId, 
            DateTime weekStart, int dayOfWeek, MealTimeType mealTime, string notes = null)
        {
            var meal = _mealRepository.GetById(mealId);
            if (meal == null)
                throw new ArgumentException("Yemek bulunamadi");

            var assignment = new PatientMealAssignment
            {
                PatientId = patientId,
                DoctorId = doctorId,
                MealId = mealId,
                WeekStartDate = GetMonday(weekStart),
                DayOfWeek = dayOfWeek,
                MealTime = mealTime,
                MealName = meal.Name,
                Description = meal.Description,
                Calories = meal.Calories,
                Protein = meal.Protein,
                Carbs = meal.Carbs,
                Fat = meal.Fat,
                PortionGrams = meal.PortionGrams,
                PortionDescription = meal.PortionDescription,
                Notes = notes
            };

            int assignmentId = _assignmentRepository.Add(assignment);
            assignment.Id = assignmentId;

            return assignment;
        }

        /// <summary>
        /// Hastaya yeni öğün ata (custom/yeni)
        /// Aynı zamanda yemek kütüphanesine de ekler
        /// </summary>
        public PatientMealAssignment AssignNewMeal(int patientId, int doctorId, DateTime weekStart, 
            int dayOfWeek, MealTimeType mealTime, string mealName, string description,
            double calories, double protein, double carbs, double fat,
            double portionGrams = 100, string portionDesc = null, 
            string category = null, string notes = null, bool addToLibrary = true)
        {
            int? mealId = null;

            // Kütüphaneye ekle
            if (addToLibrary)
            {
                var meal = CreateMeal(doctorId, mealName, mealTime, calories, protein, carbs, fat,
                    portionGrams, portionDesc, category, description, notes);
                mealId = meal.Id;
            }

            var assignment = new PatientMealAssignment
            {
                PatientId = patientId,
                DoctorId = doctorId,
                MealId = mealId,
                WeekStartDate = GetMonday(weekStart),
                DayOfWeek = dayOfWeek,
                MealTime = mealTime,
                MealName = mealName,
                Description = description,
                Calories = calories,
                Protein = protein,
                Carbs = carbs,
                Fat = fat,
                PortionGrams = portionGrams,
                PortionDescription = portionDesc,
                Notes = notes
            };

            int assignmentId = _assignmentRepository.Add(assignment);
            assignment.Id = assignmentId;

            return assignment;
        }

        /// <summary>
        /// Öğün atamasını sil
        /// </summary>
        public void RemoveAssignment(int assignmentId)
        {
            _assignmentRepository.Delete(assignmentId);
        }

        /// <summary>
        /// Hastanın haftalık planını getir
        /// </summary>
        public List<PatientMealAssignment> GetWeeklyPlan(int patientId, DateTime weekStart)
        {
            return _assignmentRepository.GetWeeklyPlan(patientId, GetMonday(weekStart));
        }

        /// <summary>
        /// Belirli gün ve öğün için atamaları getir
        /// </summary>
        public List<PatientMealAssignment> GetMealsForSlot(int patientId, DateTime weekStart, int dayOfWeek, MealTimeType mealTime)
        {
            return _assignmentRepository.GetByDayAndMealTime(patientId, GetMonday(weekStart), dayOfWeek, mealTime);
        }

        /// <summary>
        /// Öğün tüketimini işaretle
        /// </summary>
        public void MarkAsConsumed(int assignmentId, bool isConsumed)
        {
            _assignmentRepository.UpdateConsumption(assignmentId, isConsumed);
        }

        /// <summary>
        /// Haftalık özet istatistikleri
        /// </summary>
        public WeeklySummary GetWeeklySummary(int patientId, DateTime weekStart)
        {
            var assignments = GetWeeklyPlan(patientId, weekStart);

            var summary = new WeeklySummary
            {
                WeekStartDate = GetMonday(weekStart),
                TotalMeals = assignments.Count,
                ConsumedMeals = assignments.Count(a => a.IsConsumed),
                TotalCalories = assignments.Sum(a => a.Calories),
                TotalProtein = assignments.Sum(a => a.Protein),
                TotalCarbs = assignments.Sum(a => a.Carbs),
                TotalFat = assignments.Sum(a => a.Fat)
            };

            // Günlük ortalamalar (7 gün)
            summary.AvgDailyCalories = summary.TotalCalories / 7;
            summary.AvgDailyProtein = summary.TotalProtein / 7;
            summary.AvgDailyCarbs = summary.TotalCarbs / 7;
            summary.AvgDailyFat = summary.TotalFat / 7;

            return summary;
        }

        #endregion

        /// <summary>
        /// Haftanın Pazartesi gününü bul
        /// </summary>
        private DateTime GetMonday(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
    }

    /// <summary>
    /// Haftalık özet bilgileri
    /// </summary>
    public class WeeklySummary
    {
        public DateTime WeekStartDate { get; set; }
        public int TotalMeals { get; set; }
        public int ConsumedMeals { get; set; }
        public double TotalCalories { get; set; }
        public double TotalProtein { get; set; }
        public double TotalCarbs { get; set; }
        public double TotalFat { get; set; }
        public double AvgDailyCalories { get; set; }
        public double AvgDailyProtein { get; set; }
        public double AvgDailyCarbs { get; set; }
        public double AvgDailyFat { get; set; }

        public double ComplianceRate => TotalMeals > 0 ? (double)ConsumedMeals / TotalMeals * 100 : 0;
    }
}

