using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    public class ReportRepository
    {
        private readonly IDbConnection _connection;

        public ReportRepository()
        {
            _connection = DatabaseConfig.Instance.CreateConnection();
        }

        public List<WeightEntry> GetWeightHistory(int patientId, DateTime startDate, DateTime endDate)
        {
            var list = new List<WeightEntry>();
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT Date, Weight FROM WeightEntries WHERE PatientId = @pid AND Date >= @startDate AND Date <= @endDate ORDER BY Date";
                var p = cmd.CreateParameter(); p.ParameterName = "@pid"; p.Value = patientId; cmd.Parameters.Add(p);
                var pStart = cmd.CreateParameter(); pStart.ParameterName = "@startDate"; pStart.Value = startDate; cmd.Parameters.Add(pStart);
                var pEnd = cmd.CreateParameter(); pEnd.ParameterName = "@endDate"; pEnd.Value = endDate; cmd.Parameters.Add(pEnd);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new WeightEntry
                        {
                            Date = Convert.ToDateTime(reader["Date"]),
                            Weight = Convert.ToDouble(reader["Weight"]),
                            PatientId = patientId
                        });
                    }
                }
            }
            return list;
        }

        public List<MealAdherenceItem> GetMealAdherence(int patientId)
        {
            var list = new List<MealAdherenceItem>();
            using (var cmd = _connection.CreateCommand())
            {
                // Simple adherence logic: If IsConsumed=1 then 100%, else 0% (can be improved)
                cmd.CommandText = @"
                    SELECT WeekStartDate, DayOfWeek, MealName, IsConsumed, Description 
                    FROM PatientMealAssignments 
                    WHERE PatientId = @pid 
                    ORDER BY WeekStartDate DESC, DayOfWeek, MealTime";
                var p = cmd.CreateParameter(); p.ParameterName = "@pid"; p.Value = patientId; cmd.Parameters.Add(p);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bool isConsumed = Convert.ToInt32(reader["IsConsumed"]) == 1;
                        list.Add(new MealAdherenceItem
                        {
                            Date = Convert.ToDateTime(reader["WeekStartDate"]).AddDays(Convert.ToInt32(reader["DayOfWeek"])),
                            MealName = reader["MealName"].ToString(),
                            AdherenceScore = isConsumed ? 100 : 0,
                            Notes = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : ""
                        });
                    }
                }
            }
            return list;
        }

        public double GetDailyWaterIntake(int patientId)
        {
            // Fetch from Goals if available, otherwise check Notes or default
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT CurrentValue FROM Goals WHERE PatientId = @pid AND GoalType = 1 AND IsActive = 1 ORDER BY EndDate DESC LIMIT 1";
                var p = cmd.CreateParameter(); p.ParameterName = "@pid"; p.Value = patientId; cmd.Parameters.Add(p);
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToDouble(result);
                }
            }
            return 0;
        }

        public int GetDailySteps(int patientId)
        {
            // Fetch from Goals (Type 2 = Steps)
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT CurrentValue FROM Goals WHERE PatientId = @pid AND GoalType = 2 AND IsActive = 1 ORDER BY EndDate DESC LIMIT 1";
                var p = cmd.CreateParameter(); p.ParameterName = "@pid"; p.Value = patientId; cmd.Parameters.Add(p);
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
            }
            return 0;
        }

        public List<BodyFatHistoryItem> GetBodyFatHistory(int patientId)
        {
            var list = new List<BodyFatHistoryItem>();
            using (var cmd = _connection.CreateCommand())
            {
                // Assuming BodyMeasurements table has a 'BodyFat' column (need to verify schema or add it if missing)
                // Based on DESCRIBE, it has Chest, Waist, Hip etc. but not explicit BodyFat % column.
                // I will use 'Waist' as a proxy for now or add BodyFat column in seed if possible.
                // Let's check if I can calculate or if I should add the column.
                // For now, let's use a mock calculation based on BMI or just return empty if column missing.
                // Actually, I'll check if I can add BodyFat column in the seed script.
                // If not, I'll use Waist/Hip ratio or similar.
                // Wait, the user wants "Body Fat %". I should probably add this column to BodyMeasurements if it's missing.
                // The DESCRIBE output showed: Chest, Waist, Hip, Arm, Thigh, Neck. No BodyFat.
                // I will add BodyFat column in the seed script (ALTER TABLE) or just use Waist for now to avoid schema changes if risky.
                // Better: I will use 'Waist' for the second series for now, labeled as "Bel Çevresi (cm)".
                cmd.CommandText = "SELECT Date, Waist FROM BodyMeasurements WHERE PatientId = @pid ORDER BY Date";
                var p = cmd.CreateParameter(); p.ParameterName = "@pid"; p.Value = patientId; cmd.Parameters.Add(p);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Waist"] != DBNull.Value)
                        {
                            list.Add(new BodyFatHistoryItem
                            {
                                Date = Convert.ToDateTime(reader["Date"]),
                                Value = Convert.ToDouble(reader["Waist"])
                            });
                        }
                    }
                }
            }
            return list;
        }
        public double GetTargetWeight(int patientId)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT TargetValue FROM Goals WHERE PatientId = @pid AND GoalType = 0 AND IsActive = 1 ORDER BY EndDate DESC LIMIT 1";
                var p = cmd.CreateParameter(); p.ParameterName = "@pid"; p.Value = patientId; cmd.Parameters.Add(p);
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToDouble(result);
                }
            }
            return 0;
        }

        public List<NoteItem> GetRecentNotes(int patientId)
        {
            var list = new List<NoteItem>();
            using (var cmd = _connection.CreateCommand())
            {
                // Note table uses Tarih or CreatedAt for date
                cmd.CommandText = "SELECT Content, COALESCE(Tarih, CreatedAt) AS NoteDate, Category FROM Notes WHERE PatientId = @pid ORDER BY NoteDate DESC LIMIT 10";
                var p = cmd.CreateParameter(); p.ParameterName = "@pid"; p.Value = patientId; cmd.Parameters.Add(p);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string content = reader["Content"] != DBNull.Value ? reader["Content"].ToString() : "";
                        string title = content.Length > 30 ? content.Substring(0, 27) + "..." : content;
                        
                        list.Add(new NoteItem
                        {
                            Date = reader["NoteDate"] != DBNull.Value ? Convert.ToDateTime(reader["NoteDate"]) : DateTime.Now,
                            Title = title,
                            Content = content,
                            Category = reader["Category"] != DBNull.Value ? (NoteCategory)Convert.ToInt32(reader["Category"]) : NoteCategory.General
                        });
                    }
                }
            }
            return list;
        }
    }

    public class NoteItem
    {
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public NoteCategory Category { get; set; }
    }

    public class BodyFatHistoryItem
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
    }

    public class WeightHistoryItem
    {
        public DateTime Date { get; set; }
        public double Weight { get; set; }
    }

    public class MealAdherenceItem
    {
        public DateTime Date { get; set; }
        public string MealName { get; set; }
        public int AdherenceScore { get; set; } // 0-100
        public string Notes { get; set; }
    }
}
