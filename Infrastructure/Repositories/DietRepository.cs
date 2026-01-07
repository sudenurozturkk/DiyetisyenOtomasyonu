using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    /// <summary>
    /// Diet Repository - Diyet planı veri erişim katmanı
    /// Haftalık plan, günlük plan ve öğün yönetimi
    /// 
    /// OOP Principle: Encapsulation - Karmaşık diyet ilişkileri kapsüllenir
    /// Design Pattern: Repository Pattern
    /// </summary>
    public class DietRepository
    {
        /// <summary>
        /// Hastanın haftalık planını getirir
        /// </summary>
        public DietWeek GetWeeklyPlan(int patientId, DateTime weekStart)
        {
            DateTime monday = GetMondayOfWeek(weekStart);

            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM DietWeeks 
                        WHERE PatientId = @patientId AND DATE(WeekStartDate) = DATE(@weekStart) AND IsActive = 1";
                    AddParameter(cmd, "@patientId", patientId);
                    AddParameter(cmd, "@weekStart", monday.ToString("yyyy-MM-dd"));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var week = MapDietWeek(reader);
                            week.Days = GetDietDays(week.Id).ToList();
                            return week;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Haftalık plan oluşturur veya günceller
        /// </summary>
        public DietWeek CreateOrUpdateWeeklyPlan(int patientId, DateTime weekStart, int doctorId)
        {
            DateTime monday = GetMondayOfWeek(weekStart);

            var existing = GetWeeklyPlan(patientId, monday);
            if (existing != null)
                return existing;

            using (var connection = CreateConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int weekId;
                        // Haftalık plan oluştur
                        using (var weekCmd = connection.CreateCommand())
                        {
                            weekCmd.Transaction = transaction;
                            weekCmd.CommandText = @"
                                INSERT INTO DietWeeks (PatientId, WeekStartDate, CreatedAt, CreatedByDoctorId, IsActive)
                                VALUES (@patientId, @weekStart, @createdAt, @doctorId, 1);
                                SELECT LAST_INSERT_ID();";

                            AddParameter(weekCmd, "@patientId", patientId);
                            AddParameter(weekCmd, "@weekStart", monday.ToString("o"));
                            AddParameter(weekCmd, "@createdAt", DateTime.Now.ToString("o"));
                            AddParameter(weekCmd, "@doctorId", doctorId);

                            weekId = Convert.ToInt32(weekCmd.ExecuteScalar());
                        }

                        // 7 gün oluştur
                        for (int i = 0; i < 7; i++)
                        {
                            using (var dayCmd = connection.CreateCommand())
                            {
                                dayCmd.Transaction = transaction;
                                dayCmd.CommandText = @"
                                    INSERT INTO DietDays (DietWeekId, Date, TargetCalories)
                                    VALUES (@weekId, @date, 0)";

                                AddParameter(dayCmd, "@weekId", weekId);
                                AddParameter(dayCmd, "@date", monday.AddDays(i).ToString("o"));
                                dayCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();

                        return GetWeeklyPlanById(weekId);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// ID'ye göre haftalık planı getirir
        /// </summary>
        public DietWeek GetWeeklyPlanById(int weekId)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM DietWeeks WHERE Id = @id";
                    AddParameter(cmd, "@id", weekId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var week = MapDietWeek(reader);
                            week.Days = GetDietDays(week.Id).ToList();
                            return week;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Hastanın tüm haftalık planlarını getirir
        /// </summary>
        public IEnumerable<DietWeek> GetPatientAllWeeks(int patientId)
        {
            var weeks = new List<DietWeek>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM DietWeeks 
                        WHERE PatientId = @patientId AND IsActive = 1
                        ORDER BY WeekStartDate DESC";
                    AddParameter(cmd, "@patientId", patientId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var week = MapDietWeek(reader);
                            weeks.Add(week);
                        }
                    }
                }
            }

            // Günleri yükle
            foreach (var week in weeks)
            {
                week.Days = GetDietDays(week.Id).ToList();
            }

            return weeks;
        }

        /// <summary>
        /// Haftalık plana ait günleri getirir
        /// </summary>
        public IEnumerable<DietDay> GetDietDays(int weekId)
        {
            var days = new List<DietDay>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM DietDays WHERE DietWeekId = @weekId ORDER BY Date";
                    AddParameter(cmd, "@weekId", weekId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var day = MapDietDay(reader);
                            days.Add(day);
                        }
                    }
                }
            }

            // Öğünleri yükle
            foreach (var day in days)
            {
                day.Meals = GetMealItems(day.Id).ToList();
            }

            return days;
        }

        /// <summary>
        /// Günlük plana ait öğünleri getirir
        /// </summary>
        public IEnumerable<MealItem> GetMealItems(int dayId)
        {
            var meals = new List<MealItem>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM MealItems WHERE DietDayId = @dayId ORDER BY MealType";
                    AddParameter(cmd, "@dayId", dayId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            meals.Add(MapMealItem(reader));
                        }
                    }
                }
            }
            return meals;
        }

        /// <summary>
        /// Öğün ekler
        /// </summary>
        public int AddMeal(MealItem meal)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO MealItems (DietDayId, MealType, Name, Calories, Protein, Carbs, Fat, 
                            PortionSize, TimeRange, IsConfirmedByPatient)
                        VALUES (@dayId, @mealType, @name, @calories, @protein, @carbs, @fat,
                            @portionSize, @timeRange, 0);
                        SELECT LAST_INSERT_ID();";

                    AddParameter(cmd, "@dayId", meal.DietDayId);
                    AddParameter(cmd, "@mealType", (int)meal.MealType);
                    AddParameter(cmd, "@name", meal.Name);
                    AddParameter(cmd, "@calories", meal.Calories);
                    AddParameter(cmd, "@protein", meal.Protein);
                    AddParameter(cmd, "@carbs", meal.Carbs);
                    AddParameter(cmd, "@fat", meal.Fat);
                    AddParameter(cmd, "@portionSize", meal.PortionSize);
                    AddParameter(cmd, "@timeRange", meal.TimeRange);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Öğün günceller
        /// </summary>
        public bool UpdateMeal(MealItem meal)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        UPDATE MealItems SET Name = @name, Calories = @calories, Protein = @protein,
                            Carbs = @carbs, Fat = @fat, PortionSize = @portionSize, TimeRange = @timeRange
                        WHERE Id = @id";

                    AddParameter(cmd, "@id", meal.Id);
                    AddParameter(cmd, "@name", meal.Name);
                    AddParameter(cmd, "@calories", meal.Calories);
                    AddParameter(cmd, "@protein", meal.Protein);
                    AddParameter(cmd, "@carbs", meal.Carbs);
                    AddParameter(cmd, "@fat", meal.Fat);
                    AddParameter(cmd, "@portionSize", meal.PortionSize);
                    AddParameter(cmd, "@timeRange", meal.TimeRange);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Öğün siler
        /// </summary>
        public bool DeleteMeal(int mealId)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM MealItems WHERE Id = @id";
                    AddParameter(cmd, "@id", mealId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Öğün onaylar (hasta tarafından)
        /// </summary>
        public bool ConfirmMeal(int mealId, bool isConfirmed, string skippedReason = null)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        UPDATE MealItems SET IsConfirmedByPatient = @confirmed, 
                            ConfirmedAt = @confirmedAt, SkippedReason = @reason
                        WHERE Id = @id";

                    AddParameter(cmd, "@id", mealId);
                    AddParameter(cmd, "@confirmed", isConfirmed ? 1 : 0);
                    AddParameter(cmd, "@confirmedAt", isConfirmed ? DateTime.Now.ToString("o") : (object)DBNull.Value);
                    AddParameter(cmd, "@reason", skippedReason);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Günün tüm öğunlerini başka güne kopyalar
        /// </summary>
        public void CopyDayMeals(int sourceDayId, int targetDayId)
        {
            using (var connection = CreateConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Hedef günün mevcut öğünlerini temizle
                        using (var deleteCmd = connection.CreateCommand())
                        {
                            deleteCmd.Transaction = transaction;
                            deleteCmd.CommandText = "DELETE FROM MealItems WHERE DietDayId = @dayId";
                            AddParameter(deleteCmd, "@dayId", targetDayId);
                            deleteCmd.ExecuteNonQuery();
                        }

                        // Kaynak günün öğünlerini getir
                        var meals = new List<MealItem>();
                        using (var selectCmd = connection.CreateCommand())
                        {
                            selectCmd.Transaction = transaction;
                            selectCmd.CommandText = "SELECT * FROM MealItems WHERE DietDayId = @dayId";
                            AddParameter(selectCmd, "@dayId", sourceDayId);

                            using (var reader = selectCmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    meals.Add(MapMealItem(reader));
                                }
                            }
                        }

                        // Öğünleri kopyala
                        foreach (var meal in meals)
                        {
                            using (var insertCmd = connection.CreateCommand())
                            {
                                insertCmd.Transaction = transaction;
                                insertCmd.CommandText = @"
                                    INSERT INTO MealItems (DietDayId, MealType, Name, Calories, Protein, Carbs, Fat,
                                        PortionSize, TimeRange, IsConfirmedByPatient)
                                    VALUES (@dayId, @mealType, @name, @calories, @protein, @carbs, @fat,
                                        @portionSize, @timeRange, 0)";

                                AddParameter(insertCmd, "@dayId", targetDayId);
                                AddParameter(insertCmd, "@mealType", (int)meal.MealType);
                                AddParameter(insertCmd, "@name", meal.Name);
                                AddParameter(insertCmd, "@calories", meal.Calories);
                                AddParameter(insertCmd, "@protein", meal.Protein);
                                AddParameter(insertCmd, "@carbs", meal.Carbs);
                                AddParameter(insertCmd, "@fat", meal.Fat);
                                AddParameter(insertCmd, "@portionSize", meal.PortionSize);
                                AddParameter(insertCmd, "@timeRange", meal.TimeRange);
                                insertCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Günlük planı getirir
        /// </summary>
        public DietDay GetDayPlan(int dayId)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM DietDays WHERE Id = @id";
                    AddParameter(cmd, "@id", dayId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var day = MapDietDay(reader);
                            day.Meals = GetMealItems(day.Id).ToList();
                            return day;
                        }
                    }
                }
            }
            return null;
        }

        #region Helper Methods

        private IDbConnection CreateConnection()
        {
            return DatabaseConfig.Instance.CreateConnection();
        }

        private void AddParameter(IDbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(param);
        }

        private DateTime GetMondayOfWeek(DateTime date)
        {
            DateTime monday = date.AddDays(-(int)date.DayOfWeek + (int)DayOfWeek.Monday);
            if (date.DayOfWeek == DayOfWeek.Sunday)
                monday = monday.AddDays(-7);
            return monday.Date;
        }

        private DietWeek MapDietWeek(IDataReader reader)
        {
            var week = new DietWeek
            {
                Id = Convert.ToInt32(reader["Id"]),
                PatientId = Convert.ToInt32(reader["PatientId"]),
                WeekStartDate = Convert.ToDateTime(reader["WeekStartDate"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                CreatedByDoctorId = Convert.ToInt32(reader["CreatedByDoctorId"])
            };

            if (reader["WeekNotes"] != DBNull.Value)
                week.WeekNotes = reader["WeekNotes"].ToString();

            if (reader["Version"] != DBNull.Value)
                week.Version = Convert.ToInt32(reader["Version"]);

            return week;
        }

        private DietDay MapDietDay(IDataReader reader)
        {
            var day = new DietDay
            {
                Id = Convert.ToInt32(reader["Id"]),
                DietWeekId = Convert.ToInt32(reader["DietWeekId"]),
                Date = Convert.ToDateTime(reader["Date"])
            };

            if (reader["Notes"] != DBNull.Value)
                day.Notes = reader["Notes"].ToString();

            if (reader["TargetCalories"] != DBNull.Value)
                day.TargetCalories = Convert.ToDouble(reader["TargetCalories"]);

            return day;
        }

        private MealItem MapMealItem(IDataReader reader)
        {
            var meal = new MealItem
            {
                Id = Convert.ToInt32(reader["Id"]),
                DietDayId = Convert.ToInt32(reader["DietDayId"]),
                MealType = (MealType)Convert.ToInt32(reader["MealType"]),
                Name = reader["Name"].ToString(),
                Calories = Convert.ToDouble(reader["Calories"]),
                Protein = Convert.ToDouble(reader["Protein"]),
                Carbs = Convert.ToDouble(reader["Carbs"]),
                Fat = Convert.ToDouble(reader["Fat"]),
                IsConfirmedByPatient = Convert.ToInt32(reader["IsConfirmedByPatient"]) == 1
            };

            if (reader["PortionSize"] != DBNull.Value)
                meal.PortionSize = reader["PortionSize"].ToString();

            if (reader["TimeRange"] != DBNull.Value)
                meal.TimeRange = reader["TimeRange"].ToString();

            if (reader["ConfirmedAt"] != DBNull.Value)
                meal.ConfirmedAt = Convert.ToDateTime(reader["ConfirmedAt"]);

            if (reader["SkippedReason"] != DBNull.Value)
                meal.SkippedReason = reader["SkippedReason"].ToString();

            return meal;
        }

        #endregion
    }
}

