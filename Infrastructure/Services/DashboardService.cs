using System;
using System.Collections.Generic;
using System.Linq;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// Gelişmiş Dashboard Servisi
    /// KPI'lar, istatistikler, trend analizleri
    /// </summary>
    public class DashboardService
    {
        private readonly PatientRepository _patientRepository;
        private readonly AppointmentRepository _appointmentRepository;
        private readonly GoalRepository _goalRepository;
        private readonly MessageRepository _messageRepository;
        private readonly WeightEntryRepository _weightRepository;
        private readonly CacheService _cacheService;

        public DashboardService()
        {
            _patientRepository = new PatientRepository();
            _appointmentRepository = new AppointmentRepository();
            _goalRepository = new GoalRepository();
            _messageRepository = new MessageRepository();
            _weightRepository = new WeightEntryRepository();
            _cacheService = new CacheService();
        }

        /// <summary>
        /// Diyetisyen dashboard verileri
        /// </summary>
        public DoctorDashboardData GetDoctorDashboard(int doctorId)
        {
            var cacheKey = $"DoctorDashboard_{doctorId}_{DateTime.Now:yyyyMMdd}";
            
            return _cacheService.GetOrSet(cacheKey, () =>
            {
                var patients = _patientRepository.GetByDoctorId(doctorId);
                var appointments = _appointmentRepository.GetAll()
                    .Where(a => a.DoctorId == doctorId)
                    .ToList();
                var goals = _goalRepository.GetAll()
                    .Where(g => patients.Any(p => p.Id == g.PatientId))
                    .ToList();
                var messages = _messageRepository.GetAll()
                    .Where(m => m.ToUserId == doctorId && !m.IsRead)
                    .ToList();

                return new DoctorDashboardData
                {
                    TotalPatients = patients.Count,
                    ActivePatients = patients.Count(p => p.AktifMi),
                    TodayAppointments = appointments.Count(a => a.DateTime.Date == DateTime.Today),
                    PendingAppointments = appointments.Count(a => a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Scheduled),
                    UnreadMessages = messages.Count,
                    CompletedGoals = goals.Count(g => g.IsActive && g.CurrentValue >= g.TargetValue),
                    TotalGoals = goals.Count(g => g.IsActive),
                    AverageCompliance = CalculateAverageCompliance(patients),
                    HighRiskPatients = patients.Count(p => p.BMI < 18.5 || p.BMI >= 30),
                    RecentWeightLoss = CalculateRecentWeightLoss(patients)
                };
            }, TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// Hasta dashboard verileri
        /// </summary>
        public PatientDashboardData GetPatientDashboard(int patientId)
        {
            var cacheKey = $"PatientDashboard_{patientId}_{DateTime.Now:yyyyMMdd}";
            
            return _cacheService.GetOrSet(cacheKey, () =>
            {
                var patient = _patientRepository.GetById(patientId);
                if (patient == null) return null;

                var goals = _goalRepository.GetByPatientId(patientId)
                    .Where(g => g.IsActive)
                    .ToList();
                var weightEntries = _weightRepository.GetByPatientId(patientId)
                    .OrderByDescending(w => w.Date)
                    .Take(7)
                    .ToList();

                return new PatientDashboardData
                {
                    CurrentWeight = patient.GuncelKilo,
                    BMI = patient.BMI,
                    BMICategory = patient.BMICategory,
                    ActiveGoals = goals.Count,
                    CompletedGoals = goals.Count(g => g.CurrentValue >= g.TargetValue),
                    WeeklyProgress = CalculateWeeklyProgress(weightEntries),
                    DailyCompliance = CalculateDailyCompliance(patientId),
                    StreakDays = CalculateStreakDays(patientId),
                    NextAppointment = GetNextAppointment(patientId)
                };
            }, TimeSpan.FromMinutes(2));
        }

        private double CalculateAverageCompliance(List<Patient> patients)
        {
            // Basit uyum hesaplama (gerçek implementasyon daha karmaşık olabilir)
            if (patients.Count == 0) return 0;
            
            // Placeholder - gerçek uyum hesaplaması yapılacak
            return 75.5; // Ortalama %75.5 uyum
        }

        private double CalculateRecentWeightLoss(List<Patient> patients)
        {
            var totalLoss = 0.0;
            var count = 0;

            foreach (var patient in patients)
            {
                var weightEntries = _weightRepository.GetByPatientId(patient.Id)
                    .OrderByDescending(w => w.Date)
                    .Take(2)
                    .ToList();

                if (weightEntries.Count >= 2)
                {
                    var loss = weightEntries[1].Weight - weightEntries[0].Weight;
                    if (loss > 0)
                    {
                        totalLoss += loss;
                        count++;
                    }
                }
            }

            return count > 0 ? totalLoss / count : 0;
        }

        private double CalculateWeeklyProgress(List<WeightEntry> weightEntries)
        {
            if (weightEntries.Count < 2) return 0;
            return weightEntries[0].Weight - weightEntries[weightEntries.Count - 1].Weight;
        }

        private double CalculateDailyCompliance(int patientId)
        {
            // Günlük uyum hesaplama (placeholder)
            return 85.0; // %85 uyum
        }

        private int CalculateStreakDays(int patientId)
        {
            // Ardışık gün sayısı hesaplama (placeholder)
            return 5; // 5 gün streak
        }

        private DateTime? GetNextAppointment(int patientId)
        {
            var appointment = _appointmentRepository.GetAll()
                .Where(a => a.PatientId == patientId && a.DateTime > DateTime.Now && a.Status == AppointmentStatus.Scheduled)
                .OrderBy(a => a.DateTime)
                .FirstOrDefault();

            return appointment?.DateTime;
        }
    }

    /// <summary>
    /// Diyetisyen dashboard verileri
    /// </summary>
    public class DoctorDashboardData
    {
        public int TotalPatients { get; set; }
        public int ActivePatients { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int UnreadMessages { get; set; }
        public int CompletedGoals { get; set; }
        public int TotalGoals { get; set; }
        public double AverageCompliance { get; set; }
        public int HighRiskPatients { get; set; }
        public double RecentWeightLoss { get; set; }
    }

    /// <summary>
    /// Hasta dashboard verileri
    /// </summary>
    public class PatientDashboardData
    {
        public double CurrentWeight { get; set; }
        public double BMI { get; set; }
        public string BMICategory { get; set; }
        public int ActiveGoals { get; set; }
        public int CompletedGoals { get; set; }
        public double WeeklyProgress { get; set; }
        public double DailyCompliance { get; set; }
        public int StreakDays { get; set; }
        public DateTime? NextAppointment { get; set; }
    }
}
