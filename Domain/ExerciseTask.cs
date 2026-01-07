using System;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Egzersiz görevi - Diyetisyen tarafından hastaya atanan görevler
    /// </summary>
    public class ExerciseTask
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        
        public string Title { get; set; }           // "30 dk yürüyüş"
        public string Description { get; set; }
        public int DurationMinutes { get; set; }    // Süre (dakika)
        public int DifficultyLevel { get; set; }    // 1-5 zorluk
        
        public DateTime DueDate { get; set; }       // Yapılması gereken tarih
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string PatientNote { get; set; }     // "Yağmur nedeniyle yapamadım"
        
        // Progress Tracking - Hasta tarafından bildirilen ilerleme
        public int ProgressPercentage { get; set; }  // 0-100 arası
        public int CompletedDuration { get; set; }   // Tamamlanan dakika
        public string PatientFeedback { get; set; }  // Hasta geri bildirimi
        
        public DateTime CreatedAt { get; set; }
        
        // Computed Properties
        public string ProgressStatus
        {
            get
            {
                if (IsCompleted || ProgressPercentage >= 100) return "Tamamlandı";
                if (ProgressPercentage > 0) return "Devam Ediyor";
                return "Başlanmadı";
            }
        }
        
        // Navigation
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string StatusText => IsCompleted ? "Tamamlandı" : "Bekliyor";
        public string DifficultyText
        {
            get
            {
                if (DifficultyLevel == 1) return "Kolay";
                if (DifficultyLevel == 2) return "Hafif";
                if (DifficultyLevel == 3) return "Orta";
                if (DifficultyLevel == 4) return "Zor";
                if (DifficultyLevel == 5) return "Çok Zor";
                return "Orta";
            }
        }
        
        // Durum kontrolü
        public bool IsOverdue => !IsCompleted && DueDate.Date < DateTime.Today;
        public bool IsDueToday => DueDate.Date == DateTime.Today;
    }
}
