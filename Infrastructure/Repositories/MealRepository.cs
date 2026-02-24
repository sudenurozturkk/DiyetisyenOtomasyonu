using System;
using System.Collections.Generic;
using System.Data;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    /// <summary>
    /// Meal Repository - Öğün/Yemek kütüphanesi veri erişim katmanı
    /// </summary>
    public class MealRepository
    {
        /// <summary>
        /// Veritabanı bağlantısı oluştur
        /// </summary>
        protected IDbConnection CreateConnection()
        {
            return DatabaseConfig.Instance.CreateConnection();
        }

        protected void AddParameter(IDbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(param);
        }

        /// <summary>
        /// Yeni yemek ekle
        /// </summary>
        public int Add(Meal meal)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO Meals (DoctorId, Name, Description, MealTime, Calories, Protein, Carbs, Fat, 
                            PortionGrams, PortionDescription, Category, Notes, IsActive, CreatedAt)
                        VALUES (@doctorId, @name, @desc, @mealTime, @cal, @protein, @carbs, @fat,
                            @portion, @portionDesc, @category, @notes, @active, @created);
                        SELECT LAST_INSERT_ID();";

                    AddParameter(cmd, "@doctorId", meal.DoctorId);
                    AddParameter(cmd, "@name", meal.Name);
                    AddParameter(cmd, "@desc", meal.Description);
                    AddParameter(cmd, "@mealTime", (int)meal.MealTime);
                    AddParameter(cmd, "@cal", meal.Calories);
                    AddParameter(cmd, "@protein", meal.Protein);
                    AddParameter(cmd, "@carbs", meal.Carbs);
                    AddParameter(cmd, "@fat", meal.Fat);
                    AddParameter(cmd, "@portion", meal.PortionGrams);
                    AddParameter(cmd, "@portionDesc", meal.PortionDescription);
                    AddParameter(cmd, "@category", meal.Category);
                    AddParameter(cmd, "@notes", meal.Notes);
                    AddParameter(cmd, "@active", meal.IsActive ? 1 : 0);
                    AddParameter(cmd, "@created", meal.CreatedAt.ToString("o"));

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Yemek güncelle
        /// </summary>
        public void Update(Meal meal)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        UPDATE Meals SET Name = @name, Description = @desc, MealTime = @mealTime,
                            Calories = @cal, Protein = @protein, Carbs = @carbs, Fat = @fat,
                            PortionGrams = @portion, PortionDescription = @portionDesc, 
                            Category = @category, Notes = @notes, IsActive = @active
                        WHERE Id = @id";

                    AddParameter(cmd, "@id", meal.Id);
                    AddParameter(cmd, "@name", meal.Name);
                    AddParameter(cmd, "@desc", meal.Description);
                    AddParameter(cmd, "@mealTime", (int)meal.MealTime);
                    AddParameter(cmd, "@cal", meal.Calories);
                    AddParameter(cmd, "@protein", meal.Protein);
                    AddParameter(cmd, "@carbs", meal.Carbs);
                    AddParameter(cmd, "@fat", meal.Fat);
                    AddParameter(cmd, "@portion", meal.PortionGrams);
                    AddParameter(cmd, "@portionDesc", meal.PortionDescription);
                    AddParameter(cmd, "@category", meal.Category);
                    AddParameter(cmd, "@notes", meal.Notes);
                    AddParameter(cmd, "@active", meal.IsActive ? 1 : 0);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Yemek sil (soft delete)
        /// </summary>
        public void Delete(int id)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Meals SET IsActive = 0 WHERE Id = @id";
                    AddParameter(cmd, "@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// ID'ye göre yemek getir
        /// </summary>
        public Meal GetById(int id)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Meals WHERE Id = @id";
                    AddParameter(cmd, "@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapFromReader(reader);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Diyetisyenin tüm yemeklerini getir
        /// </summary>
        public List<Meal> GetByDoctorId(int doctorId, bool activeOnly = true)
        {
            var meals = new List<Meal>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    var sql = "SELECT * FROM Meals WHERE DoctorId = @doctorId";
                    if (activeOnly) sql += " AND IsActive = 1";
                    sql += " ORDER BY MealTime, Name";

                    cmd.CommandText = sql;
                    AddParameter(cmd, "@doctorId", doctorId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            meals.Add(MapFromReader(reader));
                    }
                }
            }
            return meals;
        }

        /// <summary>
        /// Öğün zamanına göre yemekler
        /// </summary>
        public List<Meal> GetByMealTime(int doctorId, MealTimeType mealTime)
        {
            var meals = new List<Meal>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM Meals 
                        WHERE DoctorId = @doctorId AND MealTime = @mealTime AND IsActive = 1
                        ORDER BY Name";
                    AddParameter(cmd, "@doctorId", doctorId);
                    AddParameter(cmd, "@mealTime", (int)mealTime);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            meals.Add(MapFromReader(reader));
                    }
                }
            }
            return meals;
        }

        /// <summary>
        /// Yemek ara
        /// </summary>
        public List<Meal> Search(int doctorId, string searchText)
        {
            var meals = new List<Meal>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM Meals 
                        WHERE DoctorId = @doctorId AND IsActive = 1 
                            AND (Name LIKE @search OR Category LIKE @search OR Description LIKE @search)
                        ORDER BY Name";
                    AddParameter(cmd, "@doctorId", doctorId);
                    AddParameter(cmd, "@search", $"%{searchText}%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            meals.Add(MapFromReader(reader));
                    }
                }
            }
            return meals;
        }

        /// <summary>
        /// Reader'dan Meal nesnesine dönüştür
        /// </summary>
        private Meal MapFromReader(IDataReader reader)
        {
            var meal = new Meal
            {
                Id = Convert.ToInt32(reader["Id"]),
                DoctorId = Convert.ToInt32(reader["DoctorId"]),
                Name = reader["Name"].ToString(),
                MealTime = (MealTimeType)Convert.ToInt32(reader["MealTime"]),
                Calories = Convert.ToDouble(reader["Calories"]),
                Protein = Convert.ToDouble(reader["Protein"]),
                Carbs = Convert.ToDouble(reader["Carbs"]),
                Fat = Convert.ToDouble(reader["Fat"]),
                PortionGrams = Convert.ToDouble(reader["PortionGrams"]),
                IsActive = Convert.ToInt32(reader["IsActive"]) == 1
            };

            if (reader["Description"] != DBNull.Value)
                meal.Description = reader["Description"].ToString();

            if (reader["PortionDescription"] != DBNull.Value)
                meal.PortionDescription = reader["PortionDescription"].ToString();

            if (reader["Category"] != DBNull.Value)
                meal.Category = reader["Category"].ToString();

            if (reader["Notes"] != DBNull.Value)
                meal.Notes = reader["Notes"].ToString();

            if (reader["CreatedAt"] != DBNull.Value)
                meal.CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString());

            return meal;
        }
    }

    /// <summary>
    /// PatientMealAssignment Repository - Hasta öğün atamaları
    /// </summary>
    public class PatientMealAssignmentRepository
    {
        protected IDbConnection CreateConnection()
        {
            return DatabaseConfig.Instance.CreateConnection();
        }

        protected void AddParameter(IDbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(param);
        }

        /// <summary>
        /// Öğün ataması ekle
        /// </summary>
        public int Add(PatientMealAssignment assignment)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO PatientMealAssignments (PatientId, DoctorId, MealId, WeekStartDate, DayOfWeek, 
                            MealTime, MealName, Description, Calories, Protein, Carbs, Fat, PortionGrams, 
                            PortionDescription, IsConsumed, ConsumedAt, Notes, CreatedAt)
                        VALUES (@patientId, @doctorId, @mealId, @weekStart, @day, @mealTime, @name, @desc, 
                            @cal, @protein, @carbs, @fat, @portion, @portionDesc, @consumed, @consumedAt, @notes, @created);
                        SELECT LAST_INSERT_ID();";

                    AddParameter(cmd, "@patientId", assignment.PatientId);
                    AddParameter(cmd, "@doctorId", assignment.DoctorId);
                    AddParameter(cmd, "@mealId", assignment.MealId.HasValue ? (object)assignment.MealId.Value : DBNull.Value);
                    AddParameter(cmd, "@weekStart", assignment.WeekStartDate.ToString("yyyy-MM-dd"));
                    AddParameter(cmd, "@day", assignment.DayOfWeek);
                    AddParameter(cmd, "@mealTime", (int)assignment.MealTime);
                    AddParameter(cmd, "@name", assignment.MealName);
                    AddParameter(cmd, "@desc", assignment.Description);
                    AddParameter(cmd, "@cal", assignment.Calories);
                    AddParameter(cmd, "@protein", assignment.Protein);
                    AddParameter(cmd, "@carbs", assignment.Carbs);
                    AddParameter(cmd, "@fat", assignment.Fat);
                    AddParameter(cmd, "@portion", assignment.PortionGrams);
                    AddParameter(cmd, "@portionDesc", assignment.PortionDescription);
                    AddParameter(cmd, "@consumed", assignment.IsConsumed ? 1 : 0);
                    AddParameter(cmd, "@consumedAt", assignment.ConsumedAt.HasValue ? (object)assignment.ConsumedAt.Value.ToString("o") : DBNull.Value);
                    AddParameter(cmd, "@notes", assignment.Notes);
                    AddParameter(cmd, "@created", assignment.CreatedAt.ToString("o"));

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Öğün atamasını sil
        /// </summary>
        public void Delete(int id)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM PatientMealAssignments WHERE Id = @id";
                    AddParameter(cmd, "@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Hastanın haftalık öğün planını getir
        /// </summary>
        public List<PatientMealAssignment> GetWeeklyPlan(int patientId, DateTime weekStart)
        {
            var assignments = new List<PatientMealAssignment>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM PatientMealAssignments 
                        WHERE PatientId = @patientId AND WeekStartDate = @weekStart
                        ORDER BY DayOfWeek, MealTime";
                    AddParameter(cmd, "@patientId", patientId);
                    AddParameter(cmd, "@weekStart", weekStart.ToString("yyyy-MM-dd"));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            assignments.Add(MapFromReader(reader));
                    }
                }
            }
            return assignments;
        }

        /// <summary>
        /// Belirli gün ve öğün için atamaları getir
        /// </summary>
        public List<PatientMealAssignment> GetByDayAndMealTime(int patientId, DateTime weekStart, int dayOfWeek, MealTimeType mealTime)
        {
            var assignments = new List<PatientMealAssignment>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM PatientMealAssignments 
                        WHERE PatientId = @patientId AND WeekStartDate = @weekStart 
                            AND DayOfWeek = @day AND MealTime = @mealTime
                        ORDER BY CreatedAt";
                    AddParameter(cmd, "@patientId", patientId);
                    AddParameter(cmd, "@weekStart", weekStart.ToString("yyyy-MM-dd"));
                    AddParameter(cmd, "@day", dayOfWeek);
                    AddParameter(cmd, "@mealTime", (int)mealTime);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            assignments.Add(MapFromReader(reader));
                    }
                }
            }
            return assignments;
        }

        /// <summary>
        /// Tüketim durumunu güncelle
        /// </summary>
        public void UpdateConsumption(int id, bool isConsumed)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        UPDATE PatientMealAssignments 
                        SET IsConsumed = @consumed, ConsumedAt = @consumedAt 
                        WHERE Id = @id";
                    AddParameter(cmd, "@id", id);
                    AddParameter(cmd, "@consumed", isConsumed ? 1 : 0);
                    AddParameter(cmd, "@consumedAt", isConsumed ? (object)DateTime.Now.ToString("o") : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private PatientMealAssignment MapFromReader(IDataReader reader)
        {
            var assignment = new PatientMealAssignment
            {
                Id = Convert.ToInt32(reader["Id"]),
                PatientId = Convert.ToInt32(reader["PatientId"]),
                DoctorId = Convert.ToInt32(reader["DoctorId"]),
                WeekStartDate = DateTime.Parse(reader["WeekStartDate"].ToString()),
                DayOfWeek = Convert.ToInt32(reader["DayOfWeek"]),
                MealTime = (MealTimeType)Convert.ToInt32(reader["MealTime"]),
                MealName = reader["MealName"].ToString(),
                Calories = Convert.ToDouble(reader["Calories"]),
                Protein = Convert.ToDouble(reader["Protein"]),
                Carbs = Convert.ToDouble(reader["Carbs"]),
                Fat = Convert.ToDouble(reader["Fat"]),
                PortionGrams = Convert.ToDouble(reader["PortionGrams"]),
                IsConsumed = Convert.ToInt32(reader["IsConsumed"]) == 1
            };

            if (reader["MealId"] != DBNull.Value)
                assignment.MealId = Convert.ToInt32(reader["MealId"]);

            if (reader["Description"] != DBNull.Value)
                assignment.Description = reader["Description"].ToString();

            if (reader["PortionDescription"] != DBNull.Value)
                assignment.PortionDescription = reader["PortionDescription"].ToString();

            if (reader["ConsumedAt"] != DBNull.Value)
                assignment.ConsumedAt = DateTime.Parse(reader["ConsumedAt"].ToString());

            if (reader["Notes"] != DBNull.Value)
                assignment.Notes = reader["Notes"].ToString();

            if (reader["CreatedAt"] != DBNull.Value)
                assignment.CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString());

            return assignment;
        }
    }
}

