using System;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Mesajlaşma entity'si - Hasta-Doktor iletişimi
    /// 
    /// OOP Principle: Encapsulation - Mesaj durumu ve öncelik mantığı kapsüllenir
    /// Academic: Professional messaging with categorization
    /// </summary>
    public class Message
    {
        public int Id { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public string FromUserName { get; set; }
        public string ToUserName { get; set; }
        public DateTime SentAt { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        
        // Silme Durumu (WhatsApp tarzı) 
        public bool IsDeletedBySender { get; set; }      // Gönderen sildi mi (benden sil)
        public bool IsDeletedByReceiver { get; set; }    // Alıcı sildi mi (benden sil)
        public bool IsDeletedForEveryone { get; set; }   // Herkesten silindi mi
        
        // Yeni özellikler - Profesyonel mesajlaşma için
        public MessageCategory Category { get; set; }
        public MessagePriority Priority { get; set; }
        public int? ParentMessageId { get; set; }  // Yanıt zinciri için

        public Message()
        {
            SentAt = DateTime.Now;
            IsRead = false;
            Category = MessageCategory.General;
            Priority = MessagePriority.Normal;
            IsDeletedBySender = false;
            IsDeletedByReceiver = false;
            IsDeletedForEveryone = false;
        }

        /// <summary>
        /// Kategori açıklaması
        /// </summary>
        public string CategoryName
        {
            get
            {
                switch (Category)
                {
                    case MessageCategory.General: return "Genel";
                    case MessageCategory.Question: return "Soru";
                    case MessageCategory.Emergency: return "Acil";
                    case MessageCategory.Information: return "Bilgi";
                    case MessageCategory.Feedback: return "Geri Bildirim";
                    case MessageCategory.Appointment: return "Randevu";
                    default: return "Bilinmiyor";
                }
            }
        }

        /// <summary>
        /// Öncelik açıklaması
        /// </summary>
        public string PriorityName
        {
            get
            {
                switch (Priority)
                {
                    case MessagePriority.Low: return "Düşük";
                    case MessagePriority.Normal: return "Normal";
                    case MessagePriority.High: return "Yüksek";
                    case MessagePriority.Urgent: return "Acil";
                    default: return "Normal";
                }
            }
        }

        /// <summary>
        /// Mesaj yaşı (ne kadar önce gönderildi)
        /// </summary>
        public string TimeAgo
        {
            get
            {
                var span = DateTime.Now - SentAt;
                
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
                
                return SentAt.ToString("dd.MM.yyyy HH:mm");
            }
        }

        /// <summary>
        /// Acil mesaj mı?
        /// </summary>
        public bool IsUrgent => Category == MessageCategory.Emergency || Priority == MessagePriority.Urgent;

        /// <summary>
        /// Yanıt mesajı mı?
        /// </summary>
        public bool IsReply => ParentMessageId.HasValue;
    }
}
