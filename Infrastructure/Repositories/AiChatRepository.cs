using System;
using System.Collections.Generic;
using System.Data;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    public class AiChatRepository : BaseRepository<AiChatMessage>
    {
        public AiChatRepository() : base("AiChatLogs") { }

        protected override AiChatMessage MapFromReader(IDataReader reader)
        {
            return new AiChatMessage
            {
                Id = Convert.ToInt32(reader["Id"]),
                PatientId = Convert.ToInt32(reader["PatientId"]),
                DoctorId = Convert.ToInt32(reader["DoctorId"]),
                Message = reader["Message"].ToString(),
                IsAiResponse = Convert.ToBoolean(reader["IsAiResponse"]),
                Timestamp = DateTime.Parse(reader["Timestamp"].ToString())
            };
        }

        protected override Dictionary<string, object> MapToParameters(AiChatMessage entity)
        {
            return new Dictionary<string, object>
            {
                { "Id", entity.Id },
                { "PatientId", entity.PatientId },
                { "DoctorId", entity.DoctorId },
                { "Message", entity.Message },
                { "IsAiResponse", entity.IsAiResponse },
                { "Timestamp", entity.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") }
            };
        }

        protected override string GetInsertSql()
        {
            return @"INSERT INTO AiChatLogs (PatientId, DoctorId, Message, IsAiResponse, Timestamp) 
                     VALUES (@PatientId, @DoctorId, @Message, @IsAiResponse, @Timestamp)";
        }

        protected override string GetUpdateSql()
        {
            throw new NotImplementedException("Chat logs are immutable.");
        }

        public List<AiChatMessage> GetChatHistory(int patientId, int doctorId)
        {
            var result = new List<AiChatMessage>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM AiChatLogs WHERE PatientId = @PatientId AND DoctorId = @DoctorId ORDER BY Timestamp ASC";
                    AddParameter(cmd, "@PatientId", patientId);
                    AddParameter(cmd, "@DoctorId", doctorId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            return result;
        }

        public void DeleteChatHistory(int patientId, int doctorId)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM AiChatLogs WHERE PatientId = @PatientId AND DoctorId = @DoctorId";
                    AddParameter(cmd, "@PatientId", patientId);
                    AddParameter(cmd, "@DoctorId", doctorId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
