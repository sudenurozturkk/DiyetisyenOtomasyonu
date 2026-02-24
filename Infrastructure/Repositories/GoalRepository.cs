using System;
using System.Collections.Generic;
using System.Data;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    /// <summary>
    /// Goal Repository - Hedef veri erişim katmanı
    /// </summary>
    public class GoalRepository : BaseRepository<Goal>
    {
        public GoalRepository() : base("Goals") { }

        protected override Goal MapFromReader(IDataReader reader)
        {
            var goal = new Goal
            {
                Id = Convert.ToInt32(reader["Id"]),
                PatientId = Convert.ToInt32(reader["PatientId"]),
                GoalType = (GoalType)Convert.ToInt32(reader["GoalType"]),
                TargetValue = Convert.ToDouble(reader["TargetValue"]),
                CurrentValue = Convert.ToDouble(reader["CurrentValue"]),
                StartDate = DateTime.Parse(reader["StartDate"].ToString()),
                IsActive = Convert.ToInt32(reader["IsActive"]) == 1
            };

            if (reader["Unit"] != DBNull.Value)
                goal.Unit = reader["Unit"].ToString();

            if (reader["EndDate"] != DBNull.Value)
                goal.EndDate = DateTime.Parse(reader["EndDate"].ToString());

            return goal;
        }

        protected override Dictionary<string, object> MapToParameters(Goal entity)
        {
            return new Dictionary<string, object>
            {
                { "Id", entity.Id },
                { "PatientId", entity.PatientId },
                { "GoalType", (int)entity.GoalType },
                { "TargetValue", entity.TargetValue },
                { "CurrentValue", entity.CurrentValue },
                { "Unit", entity.Unit },
                { "StartDate", entity.StartDate.ToString("o") },
                { "EndDate", entity.EndDate?.ToString("o") },
                { "IsActive", entity.IsActive ? 1 : 0 }
            };
        }

        protected override string GetInsertSql()
        {
            return @"INSERT INTO Goals (PatientId, GoalType, TargetValue, CurrentValue, Unit, StartDate, EndDate, IsActive)
                     VALUES (@PatientId, @GoalType, @TargetValue, @CurrentValue, @Unit, @StartDate, @EndDate, @IsActive)";
        }

        protected override string GetUpdateSql()
        {
            return @"UPDATE Goals SET TargetValue = @TargetValue, CurrentValue = @CurrentValue, 
                     EndDate = @EndDate, IsActive = @IsActive WHERE Id = @Id";
        }

        /// <summary>
        /// Hastanın hedeflerini getirir
        /// </summary>
        public IEnumerable<Goal> GetByPatientId(int patientId, bool activeOnly = true)
        {
            var goals = new List<Goal>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    var sql = "SELECT * FROM Goals WHERE PatientId = @patientId";
                    if (activeOnly)
                        sql += " AND IsActive = 1";
                    sql += " ORDER BY StartDate DESC";

                    cmd.CommandText = sql;
                    AddParameter(cmd, "@patientId", patientId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            goals.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            return goals;
        }

        /// <summary>
        /// Belirli tipteki hedefi getirir
        /// </summary>
        public Goal GetByType(int patientId, GoalType goalType)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM Goals 
                        WHERE PatientId = @patientId AND GoalType = @goalType AND IsActive = 1
                        ORDER BY StartDate DESC LIMIT 1";
                    AddParameter(cmd, "@patientId", patientId);
                    AddParameter(cmd, "@goalType", (int)goalType);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Hedef değerini günceller
        /// </summary>
        public bool UpdateCurrentValue(int goalId, double newValue)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Goals SET CurrentValue = @value WHERE Id = @id";
                    AddParameter(cmd, "@id", goalId);
                    AddParameter(cmd, "@value", newValue);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}

