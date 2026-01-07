using System;
using System.Collections.Generic;
using System.Linq;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// Mesajlaşma servisi
    /// 
    /// OOP Principle: Single Responsibility - Mesaj işlemlerinden sorumlu
    /// Design Pattern: Service Layer
    /// Academic: Professional messaging with categorization and priority
    /// </summary>
    public class MessageService
    {
        private readonly MessageRepository _messageRepository;
        private readonly UserRepository _userRepository;

        public MessageService()
        {
            _messageRepository = new MessageRepository();
            _userRepository = new UserRepository();
        }

        /// <summary>
        /// Mesaj gönder
        /// </summary>
        public Message SendMessage(int fromUserId, int toUserId, string content,
            MessageCategory category = MessageCategory.General,
            MessagePriority priority = MessagePriority.Normal,
            int? parentMessageId = null)
        {
            var fromUser = _userRepository.GetById(fromUserId);
            var toUser = _userRepository.GetById(toUserId);

            if (fromUser == null || toUser == null)
                throw new ArgumentException("Gönderici veya alıcı bulunamadı");

            var message = new Message
            {
                FromUserId = fromUserId,
                ToUserId = toUserId,
                FromUserName = fromUser.AdSoyad,
                ToUserName = toUser.AdSoyad,
                Content = content,
                Category = category,
                Priority = priority,
                ParentMessageId = parentMessageId,
                SentAt = DateTime.Now,
                IsRead = false
            };

            int messageId = _messageRepository.Add(message);
            message.Id = messageId;

            return message;
        }

        /// <summary>
        /// Konuşmayı getir
        /// </summary>
        public List<Message> GetConversation(int userId1, int userId2)
        {
            return _messageRepository.GetConversation(userId1, userId2).ToList();
        }

        /// <summary>
        /// Gelen kutusu
        /// </summary>
        public List<Message> GetInbox(int userId)
        {
            return _messageRepository.GetInbox(userId).ToList();
        }

        /// <summary>
        /// Giden kutusu
        /// </summary>
        public List<Message> GetSentMessages(int userId)
        {
            return _messageRepository.GetSent(userId).ToList();
        }

        /// <summary>
        /// Okunmamış mesaj sayısı
        /// </summary>
        public int GetUnreadCount(int userId)
        {
            return _messageRepository.GetUnreadCount(userId);
        }

        /// <summary>
        /// Acil mesajları getir
        /// </summary>
        public List<Message> GetUrgentMessages(int userId)
        {
            return _messageRepository.GetUrgentMessages(userId).ToList();
        }

        /// <summary>
        /// Mesajı okundu olarak işaretle
        /// </summary>
        public void MarkAsRead(int messageId)
        {
            _messageRepository.MarkAsRead(messageId);
        }

        /// <summary>
        /// Tüm mesajları okundu olarak işaretle
        /// </summary>
        public void MarkAllAsRead(int userId)
        {
            _messageRepository.MarkAllAsRead(userId);
        }

        /// <summary>
        /// Hasta öncelik kuyruğunu getir (doktor için)
        /// </summary>
        public List<PatientMessageSummary> GetPatientPriorityQueue(int doctorId)
        {
            return _messageRepository.GetPatientPriorityQueue(doctorId).ToList();
        }

        /// <summary>
        /// Mesajı benden sil (sadece kullanıcıdan gizle)
        /// </summary>
        public bool DeleteMessageForMe(int messageId, int userId)
        {
            var message = _messageRepository.GetById(messageId);
            if (message == null)
                return false;
            
            // Gönderen veya alıcı silebilir
            if (message.FromUserId == userId)
            {
                message.IsDeletedBySender = true;
            }
            else if (message.ToUserId == userId)
            {
                message.IsDeletedByReceiver = true;
            }
            else
            {
                throw new UnauthorizedAccessException("Bu mesajı silme yetkiniz yok");
            }
            
            return _messageRepository.Update(message);
        }

        /// <summary>
        /// Mesajı herkesten sil (sadece gönderen yapabilir)
        /// </summary>
        public bool DeleteMessageForEveryone(int messageId, int userId)
        {
            var message = _messageRepository.GetById(messageId);
            if (message == null)
                return false;
            
            // Sadece gönderen herkesten silebilir
            if (message.FromUserId != userId)
                throw new UnauthorizedAccessException("Sadece kendi mesajlarınızı herkesten silebilirsiniz");
            
            message.IsDeletedForEveryone = true;
            return _messageRepository.Update(message);
        }

        /// <summary>
        /// Tüm konuşmayı sil (benden sil)
        /// </summary>
        public bool DeleteConversationForMe(int userId, int otherUserId)
        {
            var messages = _messageRepository.GetConversation(userId, otherUserId);
            foreach (var message in messages)
            {
                if (message.FromUserId == userId)
                    message.IsDeletedBySender = true;
                else
                    message.IsDeletedByReceiver = true;
                
                _messageRepository.Update(message);
            }
            return true;
        }

        /// <summary>
        /// Eski DeleteMessage - geriye uyumluluk için
        /// </summary>
        public bool DeleteMessage(int messageId, int userId)
        {
            return DeleteMessageForEveryone(messageId, userId);
        }

        /// <summary>
        /// AI destekli yanıt taslağı oluştur
        /// </summary>
        public string GenerateAIResponseDraft(Message originalMessage)
        {
            if (originalMessage == null) return "";

            string content = originalMessage.Content.ToLowerInvariant();

            // Basit anahtar kelime tabanlı yanıt önerileri
            if (content.Contains("acil") || content.Contains("kötü hissediyorum"))
            {
                return "Merhaba, durumunuzu anlıyorum. Lütfen şu an nasıl hissettiğinizi detaylı bir şekilde anlatır mısınız? " +
                       "Gerekirse en kısa sürede randevu ayarlayabiliriz.";
            }

            if (content.Contains("diyet") && (content.Contains("değiştirir") || content.Contains("güncelleyebilir")))
            {
                return "Merhaba, diyet planınız hakkındaki geri bildiriminiz için teşekkürler. " +
                       "Planınızı güncellemek için bir sonraki görüşmemizde detaylı değerlendirme yapacağız.";
            }

            if (content.Contains("kilo") && (content.Contains("ver") || content.Contains("kayb")))
            {
                return "Merhaba, kilo verme sürecinizle ilgili sorularınızı anladım. " +
                       "Haftalık hedeflerimize uyduğunuz sürece sonuçları göreceksiniz. Sabırlı olmaya devam edin.";
            }

            if (content.Contains("yemek") && content.Contains("dışarı"))
            {
                return "Merhaba, dışarıda yemek yiyeceğiniz için teşekkürler. " +
                       "Izgara et veya balık tercih edebilir, yanında bol salata alabilirsiniz. " +
                       "Ekmek ve kızartmalardan uzak durmaya çalışın.";
            }

            if (content.Contains("egzersiz") || content.Contains("spor"))
            {
                return "Merhaba, egzersiz planınız hakkında sorularınız için teşekkürler. " +
                       "Haftada en az 3 gün 30 dakika tempolu yürüyüş yapmanızı öneriyorum.";
            }

            // Genel yanıt
            return "Merhaba, mesajınız için teşekkürler. Sorularınızı en kısa sürede yanıtlayacağım.";
        }
    }
}
