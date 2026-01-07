using System;
using System.Collections.Generic;
using System.Data;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    /// <summary>
    /// Message Repository - Mesaj veri erişim katmanı
    /// 
    /// OOP Principle: Inheritance - BaseRepository'den miras alır
    /// Design Pattern: Repository Pattern
    /// Academic: Professional messaging with categorization
    /// </summary>
    public class MessageRepository : BaseRepository<Message>
    {
        public MessageRepository() : base("Messages") { }

        protected override Message MapFromReader(IDataReader reader)
        {
            var message = new Message
            {
                Id = Convert.ToInt32(reader["Id"]),
                FromUserId = Convert.ToInt32(reader["FromUserId"]),
                ToUserId = Convert.ToInt32(reader["ToUserId"]),
                Content = reader["Content"].ToString(),
                SentAt = DateTime.Parse(reader["SentAt"].ToString()),
                IsRead = Convert.ToInt32(reader["IsRead"]) == 1,
                Category = MessageCategory.General, // Default value
                Priority = MessagePriority.Normal   // Default value
            };

            // Category ve Priority - sütun yoksa hata vermez
            try
            {
                if (HasColumn(reader, "Category") && reader["Category"] != DBNull.Value)
                    message.Category = (MessageCategory)Convert.ToInt32(reader["Category"]);
            }
            catch { message.Category = MessageCategory.General; }

            try
            {
                if (HasColumn(reader, "Priority") && reader["Priority"] != DBNull.Value)
                    message.Priority = (MessagePriority)Convert.ToInt32(reader["Priority"]);
            }
            catch { message.Priority = MessagePriority.Normal; }

            if (HasColumn(reader, "FromUserName") && reader["FromUserName"] != DBNull.Value)
                message.FromUserName = reader["FromUserName"].ToString();

            if (HasColumn(reader, "ToUserName") && reader["ToUserName"] != DBNull.Value)
                message.ToUserName = reader["ToUserName"].ToString();

            if (HasColumn(reader, "ParentMessageId") && reader["ParentMessageId"] != DBNull.Value)
                message.ParentMessageId = Convert.ToInt32(reader["ParentMessageId"]);

            // Silme durumu alanlarını oku
            try
            {
                if (HasColumn(reader, "IsDeletedBySender") && reader["IsDeletedBySender"] != DBNull.Value)
                    message.IsDeletedBySender = Convert.ToInt32(reader["IsDeletedBySender"]) == 1;
                    
                if (HasColumn(reader, "IsDeletedByReceiver") && reader["IsDeletedByReceiver"] != DBNull.Value)
                    message.IsDeletedByReceiver = Convert.ToInt32(reader["IsDeletedByReceiver"]) == 1;
                    
                if (HasColumn(reader, "IsDeletedForEveryone") && reader["IsDeletedForEveryone"] != DBNull.Value)
                    message.IsDeletedForEveryone = Convert.ToInt32(reader["IsDeletedForEveryone"]) == 1;
            }
            catch { } // Eski veritabanlarında kolon yoksa hata verir

            return message;
        }

        private bool HasColumn(IDataReader reader, string columnName)
        {
            try
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            catch { return false; }
        }

        protected override Dictionary<string, object> MapToParameters(Message entity)
        {
            return new Dictionary<string, object>
            {
                { "Id", entity.Id },
                { "FromUserId", entity.FromUserId },
                { "ToUserId", entity.ToUserId },
                { "FromUserName", entity.FromUserName },
                { "ToUserName", entity.ToUserName },
                { "SentAt", entity.SentAt.ToString("o") },
                { "Content", entity.Content },
                { "IsRead", entity.IsRead ? 1 : 0 },
                { "IsDeletedBySender", entity.IsDeletedBySender ? 1 : 0 },
                { "IsDeletedByReceiver", entity.IsDeletedByReceiver ? 1 : 0 }
            };
        }

        protected override string GetInsertSql()
        {
            return @"INSERT INTO Messages 
                    (FromUserId, ToUserId, Content, SentAt, IsRead, IsDeletedBySender, IsDeletedByReceiver)
                VALUES 
                    (@FromUserId, @ToUserId, @Content, @SentAt, @IsRead, 0, 0);
                SELECT LAST_INSERT_ID();";
        }

        protected override string GetUpdateSql()
        {
            return @"UPDATE Messages SET 
                     IsRead = @IsRead,
                     IsDeletedBySender = @IsDeletedBySender,
                     IsDeletedByReceiver = @IsDeletedByReceiver
                     WHERE Id = @Id";
        }

        /// <summary>
        /// İki kullanıcı arasındaki mesajları getirir (konuşma)
        /// </summary>
        public IEnumerable<Message> GetConversation(int userId1, int userId2)
        {
            var messages = new List<Message>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM Messages 
                        WHERE (FromUserId = @user1 AND ToUserId = @user2)
                           OR (FromUserId = @user2 AND ToUserId = @user1)
                        ORDER BY SentAt DESC";
                    AddParameter(cmd, "@user1", userId1);
                    AddParameter(cmd, "@user2", userId2);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var message = MapFromReader(reader);
                            
                            if (message.IsDeletedForEveryone)
                            {
                                message.Content = "🚫 Bu mesaj silindi";
                            }
                            else if (message.FromUserId == userId1 && message.IsDeletedBySender)
                            {
                                continue;
                            }
                            else if (message.ToUserId == userId1 && message.IsDeletedByReceiver)
                            {
                                continue;
                            }
                            
                            messages.Add(message);
                        }
                    }
                }
            }
            return messages;
        }

        /// <summary>
        /// Kullanıcının gelen mesajlarını getirir
        /// </summary>
        public IEnumerable<Message> GetInbox(int userId)
        {
            var messages = new List<Message>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM Messages 
                        WHERE ToUserId = @userId
                        ORDER BY SentAt DESC";
                    AddParameter(cmd, "@userId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            return messages;
        }

        /// <summary>
        /// Kullanıcının giden mesajlarını getirir
        /// </summary>
        public IEnumerable<Message> GetSent(int userId)
        {
            var messages = new List<Message>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM Messages 
                        WHERE FromUserId = @userId
                        ORDER BY SentAt DESC";
                    AddParameter(cmd, "@userId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            return messages;
        }

        /// <summary>
        /// Okunmamış mesaj sayısını getirir
        /// </summary>
        public int GetUnreadCount(int userId)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT COUNT(*) FROM Messages 
                        WHERE ToUserId = @userId AND IsRead = 0";
                    AddParameter(cmd, "@userId", userId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Acil mesajları getirir
        /// </summary>
        public IEnumerable<Message> GetUrgentMessages(int userId)
        {
            var messages = new List<Message>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM Messages 
                        WHERE ToUserId = @userId 
                          AND IsRead = 0
                        ORDER BY SentAt DESC";
                    AddParameter(cmd, "@userId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            return messages;
        }

        /// <summary>
        /// Mesajı okundu olarak işaretler
        /// </summary>
        public bool MarkAsRead(int messageId)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Messages SET IsRead = 1 WHERE Id = @id";
                    AddParameter(cmd, "@id", messageId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Kullanıcının tüm mesajlarını okundu olarak işaretler
        /// </summary>
        public int MarkAllAsRead(int userId)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Messages SET IsRead = 1 WHERE ToUserId = @userId AND IsRead = 0";
                    AddParameter(cmd, "@userId", userId);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Öncelik sırasına göre hastaları getirir (doktor için)
        /// </summary>
        public IEnumerable<PatientMessageSummary> GetPatientPriorityQueue(int doctorId)
        {
            var summaries = new List<PatientMessageSummary>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                            p.Id as PatientId,
                            u.AdSoyad as PatientName,
                            COUNT(CASE WHEN m.IsRead = 0 THEN 1 END) as UnreadCount,
                            0 as UrgentCount,
                            MAX(m.SentAt) as LastMessageDate
                        FROM Patients p
                        INNER JOIN Users u ON p.Id = u.Id
                        LEFT JOIN Messages m ON m.FromUserId = p.Id AND m.ToUserId = @doctorId
                        WHERE p.DoctorId = @doctorId
                        GROUP BY p.Id, u.AdSoyad
                        ORDER BY UnreadCount DESC, LastMessageDate DESC";
                    AddParameter(cmd, "@doctorId", doctorId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var summary = new PatientMessageSummary
                            {
                                PatientId = Convert.ToInt32(reader[0]),
                                PatientName = reader[1].ToString(),
                                UnreadCount = Convert.ToInt32(reader[2]),
                                UrgentCount = Convert.ToInt32(reader[3])
                            };

                            if (reader[4] != DBNull.Value)
                                summary.LastMessageDate = DateTime.Parse(reader[4].ToString());

                            summaries.Add(summary);
                        }
                    }
                }
            }
            return summaries;
        }
    }

    /// <summary>
    /// Hasta mesaj özeti - Öncelik kuyruğu için
    /// </summary>
    public class PatientMessageSummary
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int UnreadCount { get; set; }
        public int UrgentCount { get; set; }
        public DateTime? LastMessageDate { get; set; }

        public bool HasUrgent => UrgentCount > 0;
        public bool HasUnread => UnreadCount > 0;
    }
}

