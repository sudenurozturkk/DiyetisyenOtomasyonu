using System;
using System.Collections.Generic;
using System.Linq;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// Hedef yönetim servisi
    /// 
    /// OOP Principle: Single Responsibility - Hedef işlemlerinden sorumlu
    /// Design Pattern: Service Layer
    /// </summary>
    public class GoalService
    {
        private readonly GoalRepository _goalRepository;

        public GoalService()
        {
            _goalRepository = new GoalRepository();
        }

        /// <summary>
        /// Hastanın tüm hedeflerini getir
        /// </summary>
        public List<Goal> GetPatientGoals(int patientId, bool activeOnly = true)
        {
            return _goalRepository.GetByPatientId(patientId, activeOnly).ToList();
        }

        /// <summary>
        /// Hedef oluştur
        /// </summary>
        public Goal CreateGoal(int patientId, GoalType goalType, double targetValue, 
            string unit, DateTime? endDate = null, int? doctorId = null)
        {
            var goal = new Goal
            {
                PatientId = patientId,
                GoalType = goalType,
                TargetValue = targetValue,
                CurrentValue = 0,
                Unit = unit,
                StartDate = DateTime.Now,
                EndDate = endDate,
                IsActive = true
            };

            int goalId = _goalRepository.Add(goal);
            goal.Id = goalId;

            return goal;
        }

        /// <summary>
        /// Hedef değerini güncelle
        /// </summary>
        public void UpdateGoalProgress(int goalId, double newValue)
        {
            _goalRepository.UpdateCurrentValue(goalId, newValue);
        }

        /// <summary>
        /// Hedefi tamamla
        /// </summary>
        public void CompleteGoal(int goalId)
        {
            var goal = _goalRepository.GetById(goalId);
            if (goal != null)
            {
                goal.IsActive = false;
                _goalRepository.Update(goal);
            }
        }

        /// <summary>
        /// Belirli tipteki hedefi getir
        /// </summary>
        public Goal GetGoalByType(int patientId, GoalType goalType)
        {
            return _goalRepository.GetByType(patientId, goalType);
        }

        /// <summary>
        /// Aktif hedefleri getir (alias)
        /// </summary>
        public List<Goal> GetActiveGoals(int patientId)
        {
            return GetPatientGoals(patientId, true);
        }

        /// <summary>
        /// Hedef sil
        /// </summary>
        public void DeleteGoal(int goalId)
        {
            _goalRepository.Delete(goalId);
        }

        /// <summary>
        /// Hedef guncelle
        /// </summary>
        public void UpdateGoal(Goal goal)
        {
            _goalRepository.Update(goal);
        }

        /// <summary>
        /// Hedef değerini artır
        /// </summary>
        public void IncrementGoal(int goalId, double incrementValue = 1)
        {
            var goal = _goalRepository.GetById(goalId);
            if (goal != null)
            {
                goal.CurrentValue += incrementValue;
                _goalRepository.Update(goal);
            }
        }

        /// <summary>
        /// Hedef özet bilgisi
        /// </summary>
        public GoalSummary GetGoalSummary(int patientId)
        {
            var goals = GetPatientGoals(patientId, true);
            
            return new GoalSummary
            {
                TotalGoals = goals.Count,
                CompletedGoals = goals.Count(g => g.CurrentValue >= g.TargetValue),
                InProgressGoals = goals.Count(g => g.CurrentValue < g.TargetValue && g.IsActive),
                AverageProgress = goals.Count > 0 
                    ? goals.Average(g => g.ProgressPercentage) 
                    : 0,
                Goals = goals
            };
        }
    }

    /// <summary>
    /// Hedef özet bilgisi
    /// </summary>
    public class GoalSummary
    {
        public int TotalGoals { get; set; }
        public int CompletedGoals { get; set; }
        public int InProgressGoals { get; set; }
        public double AverageProgress { get; set; }
        public List<Goal> Goals { get; set; }
    }
}
