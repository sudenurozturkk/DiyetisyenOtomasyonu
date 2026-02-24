using System;
using System.Collections.Generic;
using System.Linq;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// Gelişmiş Arama Servisi
    /// Full-text search, akıllı filtreleme, kayıtlı filtreler
    /// </summary>
    public class SearchService
    {
        private readonly PatientRepository _patientRepository;
        private readonly UserRepository _userRepository;
        private readonly AppointmentRepository _appointmentRepository;
        private readonly MessageRepository _messageRepository;

        public SearchService()
        {
            _patientRepository = new PatientRepository();
            _userRepository = new UserRepository();
            _appointmentRepository = new AppointmentRepository();
            _messageRepository = new MessageRepository();
        }

        /// <summary>
        /// Hasta arama - Full-text search
        /// </summary>
        public List<Patient> SearchPatients(string searchTerm, PatientSearchFilter filter = null)
        {
            var allPatients = _patientRepository.GetAll();
            
            // Full-text search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                allPatients = allPatients.Where(p =>
                {
                    var user = _userRepository.GetById(p.Id);
                    if (user == null) return false;

                    return user.AdSoyad.ToLower().Contains(searchLower) ||
                           user.KullaniciAdi.ToLower().Contains(searchLower) ||
                           (p.Cinsiyet != null && p.Cinsiyet.ToLower().Contains(searchLower)) ||
                           p.Yas.ToString().Contains(searchTerm);
                }).ToList();
            }

            // Filtreleme
            if (filter != null)
            {
                allPatients = ApplyFilters(allPatients, filter);
            }

            return allPatients;
        }

        /// <summary>
        /// Filtreleri uygula
        /// </summary>
        private List<Patient> ApplyFilters(List<Patient> patients, PatientSearchFilter filter)
        {
            var filtered = patients.AsQueryable();

            // BMI kategorisi filtresi
            if (filter.BMICategory.HasValue)
            {
                filtered = filtered.Where(p =>
                {
                    var bmi = p.BMI;
                    return filter.BMICategory.Value switch
                    {
                        BMICategory.Underweight => bmi < 18.5,
                        BMICategory.Normal => bmi >= 18.5 && bmi < 25,
                        BMICategory.Overweight => bmi >= 25 && bmi < 30,
                        BMICategory.Obese => bmi >= 30,
                        _ => true
                    };
                });
            }

            // Risk seviyesi filtresi
            if (filter.RiskLevel.HasValue)
            {
                // Risk seviyesi hesaplama burada yapılabilir
            }

            // Son görüşme tarihi filtresi
            if (filter.LastAppointmentDays.HasValue)
            {
                var cutoffDate = DateTime.Now.AddDays(-filter.LastAppointmentDays.Value);
                var patientIds = _appointmentRepository.GetAll()
                    .Where(a => a.DateTime >= cutoffDate)
                    .Select(a => a.PatientId)
                    .Distinct()
                    .ToList();
                filtered = filtered.Where(p => patientIds.Contains(p.Id));
            }

            // Hedef durumu filtresi
            if (filter.GoalStatus.HasValue)
            {
                var goalRepository = new GoalRepository();
                var allGoals = goalRepository.GetAll();
                
                if (filter.GoalStatus == GoalStatus.Completed)
                {
                    var completedPatientIds = allGoals
                        .Where(g => g.IsActive && g.CurrentValue >= g.TargetValue)
                        .Select(g => g.PatientId)
                        .Distinct()
                        .ToList();
                    filtered = filtered.Where(p => completedPatientIds.Contains(p.Id));
                }
                else if (filter.GoalStatus == GoalStatus.InProgress)
                {
                    var inProgressPatientIds = allGoals
                        .Where(g => g.IsActive && g.CurrentValue < g.TargetValue && g.CurrentValue > 0)
                        .Select(g => g.PatientId)
                        .Distinct()
                        .ToList();
                    filtered = filtered.Where(p => inProgressPatientIds.Contains(p.Id));
                }
            }

            return filtered.ToList();
        }

        /// <summary>
        /// Hızlı filtreler - Önceden tanımlı filtreler
        /// </summary>
        public List<Patient> ApplyQuickFilter(QuickFilterType filterType, int? doctorId = null)
        {
            var allPatients = doctorId.HasValue 
                ? _patientRepository.GetByDoctorId(doctorId.Value) 
                : _patientRepository.GetAll();

            return filterType switch
            {
                QuickFilterType.RecentPatients => GetRecentPatients(allPatients, 30),
                QuickFilterType.HighRiskPatients => GetHighRiskPatients(allPatients),
                QuickFilterType.GoalAchievers => GetGoalAchievers(allPatients),
                QuickFilterType.NeedAttention => GetNeedAttentionPatients(allPatients),
                _ => allPatients
            };
        }

        private List<Patient> GetRecentPatients(List<Patient> patients, int days)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            var recentPatientIds = _appointmentRepository.GetAll()
                .Where(a => a.DateTime >= cutoffDate)
                .Select(a => a.PatientId)
                .Distinct()
                .ToList();
            return patients.Where(p => recentPatientIds.Contains(p.Id)).ToList();
        }

        private List<Patient> GetHighRiskPatients(List<Patient> patients)
        {
            return patients.Where(p =>
            {
                var bmi = p.BMI;
                return bmi < 18.5 || bmi >= 30; // Underweight veya Obese
            }).ToList();
        }

        private List<Patient> GetGoalAchievers(List<Patient> patients)
        {
            var goalRepository = new GoalRepository();
            var completedGoals = goalRepository.GetAll()
                .Where(g => g.IsActive && g.CurrentValue >= g.TargetValue)
                .Select(g => g.PatientId)
                .Distinct()
                .ToList();
            return patients.Where(p => completedGoals.Contains(p.Id)).ToList();
        }

        private List<Patient> GetNeedAttentionPatients(List<Patient> patients)
        {
            var cutoffDate = DateTime.Now.AddDays(-60);
            var recentPatientIds = _appointmentRepository.GetAll()
                .Where(a => a.DateTime >= cutoffDate)
                .Select(a => a.PatientId)
                .Distinct()
                .ToList();
            return patients.Where(p => !recentPatientIds.Contains(p.Id)).ToList();
        }
    }

    /// <summary>
    /// Hasta arama filtresi
    /// </summary>
    public class PatientSearchFilter
    {
        public BMICategory? BMICategory { get; set; }
        public RiskLevel? RiskLevel { get; set; }
        public int? LastAppointmentDays { get; set; }
        public GoalStatus? GoalStatus { get; set; }
        public string Gender { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
    }

    /// <summary>
    /// BMI kategorileri
    /// </summary>
    public enum BMICategory
    {
        Underweight,  // < 18.5
        Normal,       // 18.5 - 24.9
        Overweight,   // 25 - 29.9
        Obese         // >= 30
    }

    /// <summary>
    /// Risk seviyeleri
    /// </summary>
    public enum RiskLevel
    {
        Low,
        Medium,
        High
    }

    /// <summary>
    /// Hedef durumları
    /// </summary>
    public enum GoalStatus
    {
        Completed,
        InProgress,
        NotStarted
    }

    /// <summary>
    /// Hızlı filtre tipleri
    /// </summary>
    public enum QuickFilterType
    {
        RecentPatients,      // Son 30 günde görüşülen
        HighRiskPatients,    // Yüksek riskli hastalar
        GoalAchievers,       // Hedef tamamlayanlar
        NeedAttention        // Dikkat gereken (60+ gün görüşülmeyen)
    }
}
