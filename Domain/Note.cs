using System;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Doktor Notu - Hasta takip notları
    /// </summary>
    public class Note
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public string DoctorName { get; set; }
        public string PatientName { get; set; } // For display purposes
        public DateTime Date { get; set; }
        public string Content { get; set; }
        
        // Gelişmiş özellikler
        public NoteCategory Category { get; set; }  // Genel, Beslenme, Tıbbi, vb.
        public bool IsImportant { get; set; }

        public Note()
        {
            Date = DateTime.Now;
            Category = NoteCategory.General;
            IsImportant = false;
        }

        /// <summary>
        /// Kategori adı
        /// </summary>
        public string CategoryName
        {
            get
            {
                switch (Category)
                {
                    case NoteCategory.General: return "Genel";
                    case NoteCategory.Nutrition: return "Beslenme";
                    case NoteCategory.Medical: return "Tıbbi";
                    case NoteCategory.Exercise: return "Egzersiz";
                    case NoteCategory.Psychological: return "Psikolojik";
                    default: return "Genel";
                }
            }
        }

        /// <summary>
        /// Not yaşı
        /// </summary>
        public string TimeAgo
        {
            get
            {
                var span = DateTime.Now - Date;
                
                if (span.TotalMinutes < 1)
                    return "Az önce";
                if (span.TotalMinutes < 60)
                    return $"{(int)span.TotalMinutes} dakika önce";
                if (span.TotalHours < 24)
                    return $"{(int)span.TotalHours} saat önce";
                if (span.TotalDays < 7)
                    return $"{(int)span.TotalDays} gün önce";
                if (span.TotalDays < 30)
                    return $"{(int)(span.TotalDays / 7)} hafta önce";
                
                return Date.ToString("dd.MM.yyyy");
            }
        }

        /// <summary>
        /// Kısa içerik (önizleme için)
        /// </summary>
        public string ShortContent
        {
            get
            {
                if (string.IsNullOrEmpty(Content)) return "";
                return Content.Length > 100 ? Content.Substring(0, 100) + "..." : Content;
            }
        }
    }
}
