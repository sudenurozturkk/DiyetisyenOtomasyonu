using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// Hasta Veri Toplayıcı - AI analizi için verileri yapılandırır
    /// </summary>
    public class PatientDataAggregator
    {
        private readonly PatientRepository _patientRepository;
        private readonly WeightEntryRepository _weightRepository;
        private readonly DietRepository _dietRepository;
        private readonly ExerciseTaskRepository _exerciseRepository;
        private readonly MealRepository _mealRepository;
        private readonly MealFeedbackRepository _feedbackRepository;
        private readonly BodyMeasurementRepository _measurementRepository;
        private readonly GoalRepository _goalRepository;
        private readonly NoteRepository _noteRepository;

        public PatientDataAggregator()
        {
            _patientRepository = new PatientRepository();
            _weightRepository = new WeightEntryRepository();
            _dietRepository = new DietRepository();
            _exerciseRepository = new ExerciseTaskRepository();
            _mealRepository = new MealRepository();
            _feedbackRepository = new MealFeedbackRepository();
            _measurementRepository = new BodyMeasurementRepository();
            _goalRepository = new GoalRepository();
            _noteRepository = new NoteRepository();
        }

        /// <summary>
        /// AI için hastanın tüm verilerini içeren bir özet metin oluşturur
        /// </summary>
        public string AggregatePatientData(int patientId, int days = 30)
        {
            var patient = _patientRepository.GetFullPatientById(patientId);
            if (patient == null) return "Hasta bulunamadı.";

            var sb = new StringBuilder();
            sb.AppendLine($"### HASTA PROFİLİ: {patient.AdSoyad}");
            sb.AppendLine($"- Yaş: {patient.Yas}, Cinsiyet: {patient.Cinsiyet}");
            sb.AppendLine($"- Boy: {patient.Boy} cm, Başlangıç Kilosu: {patient.BaslangicKilosu} kg, Güncel Kilo: {patient.GuncelKilo} kg");
            sb.AppendLine($"- BMI: {patient.BMI} ({patient.BMIKategori})");
            sb.AppendLine($"- Yaşam Tarzı: {patient.LifestyleDescription}, Aktivite: {patient.ActivityDescription}");
            sb.AppendLine($"- Tıbbi Geçmiş: {patient.MedicalHistory}");
            sb.AppendLine($"- Notlar: {patient.Notlar}");

            // 1. Kilo Geçmişi
            var weights = _weightRepository.GetByPatientId(patientId, days).OrderBy(w => w.Date).ToList();
            sb.AppendLine("\n### KİLO TAKİBİ (Son 30 Gün)");
            foreach (var w in weights)
            {
                sb.AppendLine($"- {w.Date:dd.MM.yyyy}: {w.Weight} kg");
            }

            // 1.1 Vücut Ölçümleri
            var measurements = _measurementRepository.GetByPatient(patientId).OrderByDescending(m => m.Date).Take(5).ToList();
            if (measurements.Any())
            {
                sb.AppendLine("\n### VÜCUT ÖLÇÜMLERİ (Son 5 Kayıt)");
                foreach (var m in measurements)
                {
                    sb.AppendLine($"- {m.Date:dd.MM.yyyy}: Bel: {m.Waist}cm, Kalça: {m.Hip}cm, Göğüs: {m.Chest}cm");
                }
            }

            // 2. Öğün Uyumu ve Geri Bildirimler
            // Not: MealFeedback tablosundan verileri çekiyoruz
            var feedbacks = GetMealFeedbacks(patientId, days);
            sb.AppendLine("\n### ÖĞÜN UYUMU VE GERİ BİLDİRİMLER");
            int totalMeals = feedbacks.Count;
            int consumedMeals = feedbacks.Count(f => f.IsConsumed);
            sb.AppendLine($"- Toplam Takip Edilen Öğün: {totalMeals}");
            sb.AppendLine($"- Uyum Oranı: %{(totalMeals > 0 ? Math.Round((double)consumedMeals / totalMeals * 100, 1) : 0)}");
            
            var deviations = feedbacks.Where(f => !f.IsConsumed).Take(10).ToList();
            if (deviations.Any())
            {
                sb.AppendLine("- Son Sapmalar ve Sebepleri:");
                foreach (var d in deviations)
                {
                    sb.AppendLine($"  * {d.Date:dd.MM.yyyy} ({d.MealTimeText}): {d.MealName} - Sebep: {d.Reason}");
                }
            }

            // 3. Egzersiz Takibi
            var exercises = _exerciseRepository.GetByPatient(patientId).Where(e => e.DueDate >= DateTime.Today.AddDays(-days)).ToList();
            sb.AppendLine("\n### EGZERSİZ TAKİBİ");
            int totalExercises = exercises.Count;
            int completedExercises = exercises.Count(e => e.IsCompleted);
            sb.AppendLine($"- Toplam Atanan Egzersiz: {totalExercises}");
            sb.AppendLine($"- Tamamlanma Oranı: %{(totalExercises > 0 ? Math.Round((double)completedExercises / totalExercises * 100, 1) : 0)}");
            
            var missedExercises = exercises.Where(e => !e.IsCompleted).Take(5).ToList();
            if (missedExercises.Any())
            {
                sb.AppendLine("- Yapılmayan Egzersizler:");
                foreach (var e in missedExercises)
                {
                    sb.AppendLine($"  * {e.DueDate:dd.MM.yyyy}: {e.Title}");
                }
            }

            // 4. Hedefler
            try
            {
                var goals = _goalRepository.GetByPatientId(patientId).Where(g => g.IsActive).ToList();
                if (goals.Any())
                {
                    sb.AppendLine("\n### AKTİF HEDEFLER");
                    foreach (var g in goals)
                    {
                        double progress = g.TargetValue > 0 ? (g.CurrentValue / g.TargetValue * 100) : 0;
                        string status = progress >= 100 ? "Tamamlandı" : (progress >= 50 ? "İlerliyor" : "Başlangıç");
                        sb.AppendLine($"- {GetGoalTypeName(g.GoalType)}: Mevcut {g.CurrentValue:F1} / Hedef {g.TargetValue:F1} {g.Unit} (%{progress:F0} - {status})");
                    }
                }
            }
            catch { /* Hedef verisi alınamazsa devam et */ }

            // 5. Diyetisyen Notları
            try
            {
                var notes = _noteRepository.GetByPatientId(patientId).OrderByDescending(n => n.Date).Take(5).ToList();
                if (notes.Any())
                {
                    sb.AppendLine("\n### DİYETİSYEN NOTLARI (Son 5)");
                    foreach (var n in notes)
                    {
                        string content = n.Content ?? "";
                        if (content.Length > 100) content = content.Substring(0, 100) + "...";
                        sb.AppendLine($"- {n.Date:dd.MM.yyyy}: {content}");
                    }
                }
            }
            catch { /* Not verisi alınamazsa devam et */ }

            return sb.ToString();
        }

        private string GetGoalTypeName(GoalType type)
        {
            switch (type)
            {
                case GoalType.Weight: return "Kilo Hedefi";
                case GoalType.Water: return "Su Tüketimi";
                case GoalType.Steps: return "Adım Sayısı";
                case GoalType.Calories: return "Kalori";
                default: return "Hedef";
            }
        }

        private List<MealFeedback> GetMealFeedbacks(int patientId, int days)
        {
            try 
            {
                return _feedbackRepository.GetByPatientId(patientId, days).ToList();
            }
            catch 
            {
                return new List<MealFeedback>();
            }
        }
    }
}
