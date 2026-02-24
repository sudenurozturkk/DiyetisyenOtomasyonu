using System;
using System.Collections.Generic;
using System.Linq;
using DiyetisyenOtomasyonu.Domain;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// Rozet Servisi - Gamification
    /// Hasta başarılarını takip eder ve rozet verir
    /// </summary>
    public class BadgeService
    {
        private readonly GoalRepository _goalRepository;
        private readonly WeightEntryRepository _weightRepository;
        private readonly DietRepository _dietRepository;
        private readonly ExerciseTaskRepository _exerciseRepository;

        public BadgeService()
        {
            _goalRepository = new GoalRepository();
            _weightRepository = new WeightEntryRepository();
            _dietRepository = new DietRepository();
            _exerciseRepository = new ExerciseTaskRepository();
        }

        /// <summary>
        /// Hasta için tüm rozetleri kontrol et ve güncelle
        /// </summary>
        public List<Badge> CheckAndUpdateBadges(int patientId)
        {
            var badges = new List<Badge>();

            // Uyum rozetleri
            badges.AddRange(CheckComplianceBadges(patientId));

            // Hedef rozetleri
            badges.AddRange(CheckGoalBadges(patientId));

            // Kilo rozetleri
            badges.AddRange(CheckWeightBadges(patientId));

            // Aktivite rozetleri
            badges.AddRange(CheckActivityBadges(patientId));

            return badges;
        }

        /// <summary>
        /// Uyum rozetlerini kontrol et
        /// </summary>
        private List<Badge> CheckComplianceBadges(int patientId)
        {
            var badges = new List<Badge>();
            var compliance = CalculateCompliance(patientId);

            // Perfect Week (7 gün mükemmel uyum)
            if (compliance.PerfectDays >= 7)
            {
                badges.Add(CreateBadge(patientId, BadgeType.PerfectWeek, "Mükemmel Hafta", 
                    "7 gün boyunca mükemmel diyet uyumu!", "🏆"));
            }

            // Perfect Month (30 gün mükemmel uyum)
            if (compliance.PerfectDays >= 30)
            {
                badges.Add(CreateBadge(patientId, BadgeType.PerfectMonth, "Mükemmel Ay", 
                    "30 gün boyunca mükemmel diyet uyumu!", "🌟"));
            }

            // Consistent (14 gün ardışık uyum)
            if (compliance.ConsecutiveDays >= 14)
            {
                badges.Add(CreateBadge(patientId, BadgeType.Consistent, "Tutarlılık", 
                    "14 gün ardışık mükemmel uyum!", "💪"));
            }

            return badges;
        }

        /// <summary>
        /// Hedef rozetlerini kontrol et
        /// </summary>
        private List<Badge> CheckGoalBadges(int patientId)
        {
            var badges = new List<Badge>();
            var goals = _goalRepository.GetByPatientId(patientId);
            var completedGoals = goals.Count(g => g.IsActive && g.CurrentValue >= g.TargetValue);

            if (completedGoals >= 1)
            {
                badges.Add(CreateBadge(patientId, BadgeType.GoalAchiever, "Hedef Avcısı", 
                    "İlk hedefini tamamladın!", "🎯"));
            }

            if (completedGoals >= 5)
            {
                badges.Add(CreateBadge(patientId, BadgeType.GoalMaster, "Hedef Ustası", 
                    "5 hedef tamamladın!", "⭐"));
            }

            if (completedGoals >= 10)
            {
                badges.Add(CreateBadge(patientId, BadgeType.GoalLegend, "Hedef Efsanesi", 
                    "10 hedef tamamladın!", "👑"));
            }

            return badges;
        }

        /// <summary>
        /// Kilo rozetlerini kontrol et
        /// </summary>
        private List<Badge> CheckWeightBadges(int patientId)
        {
            var badges = new List<Badge>();
            var weightEntries = _weightRepository.GetByPatientId(patientId);
            if (weightEntries.Count < 2) return badges;

            var firstWeight = weightEntries.OrderBy(w => w.Date).First().Weight;
            var lastWeight = weightEntries.OrderByDescending(w => w.Date).First().Weight;
            var weightLoss = firstWeight - lastWeight;

            if (weightLoss >= 1)
            {
                badges.Add(CreateBadge(patientId, BadgeType.FirstKilo, "İlk Adım", 
                    "İlk kilonu verdin!", "🎉"));
            }

            if (weightLoss >= 5)
            {
                badges.Add(CreateBadge(patientId, BadgeType.FiveKilo, "5 Kilo Kahramanı", 
                    "5 kilo verdin!", "🔥"));
            }

            if (weightLoss >= 10)
            {
                badges.Add(CreateBadge(patientId, BadgeType.TenKilo, "10 Kilo Şampiyonu", 
                    "10 kilo verdin!", "💎"));
            }

            if (weightLoss >= 20)
            {
                badges.Add(CreateBadge(patientId, BadgeType.TwentyKilo, "20 Kilo Efsanesi", 
                    "20 kilo verdin!", "👑"));
            }

            return badges;
        }

        /// <summary>
        /// Aktivite rozetlerini kontrol et
        /// </summary>
        private List<Badge> CheckActivityBadges(int patientId)
        {
            var badges = new List<Badge>();
            var goals = _goalRepository.GetByPatientId(patientId);

            // Step Champion
            var stepGoal = goals.FirstOrDefault(g => g.GoalType == GoalType.Steps);
            if (stepGoal != null && stepGoal.CurrentValue >= 10000)
            {
                badges.Add(CreateBadge(patientId, BadgeType.StepChampion, "Adım Şampiyonu", 
                    "Günlük 10.000 adım hedefini tamamladın!", "🚶"));
            }

            // Water Drinker
            var waterGoal = goals.FirstOrDefault(g => g.GoalType == GoalType.Water);
            if (waterGoal != null && waterGoal.CurrentValue >= waterGoal.TargetValue)
            {
                badges.Add(CreateBadge(patientId, BadgeType.WaterDrinker, "Su İçicisi", 
                    "Günlük su hedefini tamamladın!", "💧"));
            }

            return badges;
        }

        private Badge CreateBadge(int patientId, BadgeType type, string name, string description, string icon)
        {
            return new Badge
            {
                PatientId = patientId,
                Type = type,
                Name = name,
                Description = description,
                Icon = icon,
                EarnedDate = DateTime.Now,
                IsEarned = true,
                Progress = 100
            };
        }

        private ComplianceStats CalculateCompliance(int patientId)
        {
            // Basit uyum hesaplama (gerçek implementasyon daha karmaşık olabilir)
            var dietWeeks = _dietRepository.GetDietWeeksByPatientId(patientId);
            var perfectDays = 0;
            var consecutiveDays = 0;

            // Burada gerçek uyum hesaplaması yapılacak
            // Şimdilik placeholder

            return new ComplianceStats
            {
                PerfectDays = perfectDays,
                ConsecutiveDays = consecutiveDays
            };
        }

        private class ComplianceStats
        {
            public int PerfectDays { get; set; }
            public int ConsecutiveDays { get; set; }
        }
    }
}
