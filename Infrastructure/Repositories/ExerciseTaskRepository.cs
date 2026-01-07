using System;
using System.Collections.Generic;
using System.Data;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    /// <summary>
    /// Exercise Task Repository - Egzersiz görevleri
    /// </summary>
    public class ExerciseTaskRepository : BaseRepository<ExerciseTask>
    {
        public ExerciseTaskRepository() : base("ExerciseTasks") { }

        protected override ExerciseTask MapFromReader(IDataReader reader)
        {
            var task = new ExerciseTask
            {
                Id = Convert.ToInt32(reader["Id"]),
                PatientId = Convert.ToInt32(reader["PatientId"]),
                DoctorId = Convert.ToInt32(reader["DoctorId"]),
                Title = reader["Title"].ToString(),
                DurationMinutes = reader["DurationMinutes"] != DBNull.Value ? Convert.ToInt32(reader["DurationMinutes"]) : 0,
                DifficultyLevel = reader["DifficultyLevel"] != DBNull.Value ? Convert.ToInt32(reader["DifficultyLevel"]) : 1,
                DueDate = DateTime.Parse(reader["DueDate"].ToString()),
                IsCompleted = reader["IsCompleted"] != DBNull.Value && Convert.ToBoolean(reader["IsCompleted"])
            };

            if (reader["Description"] != DBNull.Value) task.Description = reader["Description"].ToString();
            if (reader["CompletedAt"] != DBNull.Value) task.CompletedAt = DateTime.Parse(reader["CompletedAt"].ToString());
            if (reader["PatientNote"] != DBNull.Value) task.PatientNote = reader["PatientNote"].ToString();
            if (reader["CreatedAt"] != DBNull.Value) task.CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString());
            
            // Progress tracking fields
            try { if (reader["ProgressPercentage"] != DBNull.Value) task.ProgressPercentage = Convert.ToInt32(reader["ProgressPercentage"]); } catch { }
            try { if (reader["CompletedDuration"] != DBNull.Value) task.CompletedDuration = Convert.ToInt32(reader["CompletedDuration"]); } catch { }
            try { if (reader["PatientFeedback"] != DBNull.Value) task.PatientFeedback = reader["PatientFeedback"].ToString(); } catch { }

            return task;
        }

        protected override Dictionary<string, object> MapToParameters(ExerciseTask entity)
        {
            return new Dictionary<string, object>
            {
                { "Id", entity.Id },
                { "PatientId", entity.PatientId },
                { "DoctorId", entity.DoctorId },
                { "Title", entity.Title },
                { "Description", entity.Description },
                { "DurationMinutes", entity.DurationMinutes },
                { "DifficultyLevel", entity.DifficultyLevel },
                { "DueDate", entity.DueDate.ToString("yyyy-MM-dd HH:mm:ss") },
                { "IsCompleted", entity.IsCompleted ? 1 : 0 },
                { "CompletedAt", entity.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss") },
                { "PatientNote", entity.PatientNote }
            };
        }

        protected override string GetInsertSql()
        {
            return @"INSERT INTO ExerciseTasks 
                    (PatientId, DoctorId, Title, Description, DurationMinutes, DifficultyLevel, DueDate)
                    VALUES (@PatientId, @DoctorId, @Title, @Description, @DurationMinutes, @DifficultyLevel, @DueDate)";
        }

        protected override string GetUpdateSql()
        {
            return @"UPDATE ExerciseTasks SET 
                    Title = @Title, Description = @Description, DurationMinutes = @DurationMinutes,
                    DifficultyLevel = @DifficultyLevel, IsCompleted = @IsCompleted, 
                    CompletedAt = @CompletedAt, PatientNote = @PatientNote
                    WHERE Id = @Id";
        }

        public List<ExerciseTask> GetByPatient(int patientId)
        {
            var result = new List<ExerciseTask>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM ExerciseTasks WHERE PatientId = @PatientId ORDER BY DueDate DESC";
                    AddParameter(cmd, "@PatientId", patientId);

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

        public List<ExerciseTask> GetByDoctor(int doctorId)
        {
            var result = new List<ExerciseTask>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.*, u.AdSoyad as PatientName 
                                        FROM ExerciseTasks e 
                                        LEFT JOIN Patients p ON e.PatientId = p.Id 
                                        LEFT JOIN Users u ON p.Id = u.Id
                                        WHERE e.DoctorId = @DoctorId 
                                        ORDER BY e.DueDate DESC";
                    AddParameter(cmd, "@DoctorId", doctorId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var task = MapFromReader(reader);
                            // PatientName'i oku
                            try 
                            { 
                                if (reader["PatientName"] != DBNull.Value) 
                                    task.PatientName = reader["PatientName"].ToString(); 
                            } 
                            catch { }
                            result.Add(task);
                        }
                    }
                }
            }
            return result;
        }

        public void MarkComplete(int taskId, string patientNote = null)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE ExerciseTasks SET 
                                       IsCompleted = 1, CompletedAt = NOW(), PatientNote = @PatientNote 
                                       WHERE Id = @Id";
                    AddParameter(cmd, "@Id", taskId);
                    AddParameter(cmd, "@PatientNote", patientNote);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkIncomplete(int taskId, string patientNote)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE ExerciseTasks SET 
                                       IsCompleted = 0, PatientNote = @PatientNote 
                                       WHERE Id = @Id";
                    AddParameter(cmd, "@Id", taskId);
                    AddParameter(cmd, "@PatientNote", patientNote);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        
        /// <summary>
        /// Hasta tarafından ilerleme güncelleme
        /// </summary>
        public void UpdateProgress(int taskId, int percentage, int completedDuration, string feedback)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    bool isComplete = percentage >= 100;
                    cmd.CommandText = @"UPDATE ExerciseTasks SET 
                                       ProgressPercentage = @ProgressPercentage,
                                       CompletedDuration = @CompletedDuration,
                                       PatientFeedback = @PatientFeedback,
                                       IsCompleted = @IsCompleted,
                                       CompletedAt = CASE WHEN @IsCompleted = 1 THEN NOW() ELSE CompletedAt END
                                       WHERE Id = @Id";
                    AddParameter(cmd, "@Id", taskId);
                    AddParameter(cmd, "@ProgressPercentage", percentage);
                    AddParameter(cmd, "@CompletedDuration", completedDuration);
                    AddParameter(cmd, "@PatientFeedback", feedback);
                    AddParameter(cmd, "@IsCompleted", isComplete ? 1 : 0);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateStatus(int id, bool isCompleted)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "UPDATE ExerciseTasks SET IsCompleted = @IsCompleted, CompletedAt = @CompletedAt WHERE Id = @Id";
                    AddParameter(cmd, "@IsCompleted", isCompleted ? 1 : 0);
                    AddParameter(cmd, "@CompletedAt", isCompleted ? (object)DateTime.Now : DBNull.Value);
                    AddParameter(cmd, "@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public ExerciseTaskStats GetStatsForPatient(int patientId)
        {
            var stats = new ExerciseTaskStats();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"SELECT 
                                       COUNT(*) as TotalTasks,
                                       SUM(CASE WHEN IsCompleted = 1 THEN 1 ELSE 0 END) as CompletedTasks,
                                       SUM(CASE WHEN IsCompleted = 0 AND DueDate < CURDATE() THEN 1 ELSE 0 END) as OverdueTasks
                                       FROM ExerciseTasks WHERE PatientId = @PatientId";
                    AddParameter(cmd, "@PatientId", patientId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            stats.TotalTasks = reader["TotalTasks"] != DBNull.Value ? Convert.ToInt32(reader["TotalTasks"]) : 0;
                            stats.CompletedTasks = reader["CompletedTasks"] != DBNull.Value ? Convert.ToInt32(reader["CompletedTasks"]) : 0;
                            stats.OverdueTasks = reader["OverdueTasks"] != DBNull.Value ? Convert.ToInt32(reader["OverdueTasks"]) : 0;
                        }
                    }
                }
            }
            return stats;
        }
    }

    public class ExerciseTaskStats
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public double CompletionRate => TotalTasks > 0 ? Math.Round(100.0 * CompletedTasks / TotalTasks, 1) : 0;
    }
}
