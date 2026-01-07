using System;
using System.Collections.Generic;
using System.Data;
using DiyetisyenOtomasyonu.Domain;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    public class MealFeedbackRepository : BaseRepository<MealFeedback>
    {
        public MealFeedbackRepository() : base("mealfeedback")
        {
        }

        protected override MealFeedback MapFromReader(IDataReader reader)
        {
            return new MealFeedback
            {
                Id = Convert.ToInt32(reader["Id"]),
                PatientId = Convert.ToInt32(reader["PatientId"]),
                MealAssignmentId = reader["MealAssignmentId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["MealAssignmentId"]) : null,
                Date = Convert.ToDateTime(reader["Date"]),
                MealTime = (MealTime)Convert.ToInt32(reader["MealTime"]),
                IsConsumed = Convert.ToBoolean(reader["IsConsumed"]),
                Reason = reader["Reason"]?.ToString(),
                Notes = reader["Notes"]?.ToString(),
                MealName = reader["MealName"]?.ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }

        protected override Dictionary<string, object> MapToParameters(MealFeedback entity)
        {
            return new Dictionary<string, object>
            {
                { "PatientId", entity.PatientId },
                { "MealAssignmentId", (object)entity.MealAssignmentId ?? DBNull.Value },
                { "Date", entity.Date },
                { "MealTime", (int)entity.MealTime },
                { "IsConsumed", entity.IsConsumed },
                { "Reason", (object)entity.Reason ?? DBNull.Value },
                { "Notes", (object)entity.Notes ?? DBNull.Value },
                { "MealName", (object)entity.MealName ?? DBNull.Value }
            };
        }

        protected override string GetInsertSql()
        {
            return @"INSERT INTO mealfeedback (PatientId, MealAssignmentId, Date, MealTime, IsConsumed, Reason, Notes, MealName) 
                     VALUES (@PatientId, @MealAssignmentId, @Date, @MealTime, @IsConsumed, @Reason, @Notes, @MealName)";
        }

        protected override string GetUpdateSql()
        {
            return @"UPDATE mealfeedback SET 
                     PatientId = @PatientId, 
                     MealAssignmentId = @MealAssignmentId, 
                     Date = @Date, 
                     MealTime = @MealTime, 
                     IsConsumed = @IsConsumed, 
                     Reason = @Reason, 
                     Notes = @Notes, 
                     MealName = @MealName 
                     WHERE Id = @Id";
        }

        public IEnumerable<MealFeedback> GetByPatientId(int patientId, int lastDays = 30)
        {
            string sql = $"SELECT * FROM {_tableName} WHERE PatientId = @PatientId AND Date >= @StartDate ORDER BY Date DESC";
            var parameters = new Dictionary<string, object>
            {
                { "PatientId", patientId },
                { "StartDate", DateTime.Today.AddDays(-lastDays) }
            };
            return ExecuteQuery(sql, parameters);
        }
    }
}
