using System;
using System.Data;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Database
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            // MySQL veritabanı için tablo kontrolü
            using (var connection = DatabaseConfig.Instance.CreateConnection())
            {
                // Tabloları oluştur (IF NOT EXISTS kullanıldığından güvenli)
                CreateUsersTable(connection);
                CreatePatientsTable(connection);
                CreateDoctorsTable(connection);
                CreateGoalsTable(connection);
                FixGoalsTable(connection); // Fix missing columns
                CreateNotesTable(connection);
                CreateMessagesTable(connection);
                CreateWeightEntriesTable(connection);
                CreateDietWeeksTable(connection);
                CreateDietDaysTable(connection);
                CreateMealItemsTable(connection);
                
                // Yeni tablolar - Faz 2-5
                CreateBodyMeasurementsTable(connection);
                CreateExerciseTasksTable(connection);
                CreateAppointmentsTable(connection);
                CreateMealFeedbackTable(connection);
                CreateMealsTable(connection);
                CreatePatientMealAssignmentsTable(connection);
                CreateAiChatLogsTable(connection);
            }
        }

        private static void ExecuteNonQuery(IDbConnection connection, string sql)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        private static void CreateUsersTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    AdSoyad VARCHAR(255) NOT NULL,
                    KullaniciAdi VARCHAR(100) NOT NULL UNIQUE,
                    ParolaHash VARCHAR(255) NOT NULL,
                    Role INT NOT NULL,
                    KayitTarihi DATETIME NOT NULL,
                    AktifMi TINYINT NOT NULL DEFAULT 1
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
        }

        private static void CreatePatientsTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS Patients (
                    Id INT PRIMARY KEY,
                    Cinsiyet VARCHAR(20),
                    Yas INT NOT NULL,
                    Boy DOUBLE NOT NULL,
                    BaslangicKilosu DOUBLE NOT NULL,
                    GuncelKilo DOUBLE NOT NULL,
                    DoctorId INT NOT NULL,
                    Notlar TEXT,
                    ThyroidStatus VARCHAR(100),
                    InsulinStatus VARCHAR(100),
                    MedicalHistory TEXT,
                    LifestyleType INT DEFAULT 0,
                    ActivityLevel INT DEFAULT 0,
                    FOREIGN KEY (Id) REFERENCES Users(Id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
        }

        private static void CreateDoctorsTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS Doctors (
                    Id INT PRIMARY KEY,
                    Uzmanlik VARCHAR(255),
                    Telefon VARCHAR(50),
                    Email VARCHAR(255),
                    FOREIGN KEY (Id) REFERENCES Users(Id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
        }
        
        private static void CreateGoalsTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS Goals (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    PatientId INT NOT NULL,
                    GoalType INT NOT NULL,
                    TargetValue DOUBLE NOT NULL,
                    CurrentValue DOUBLE NOT NULL,
                    Unit VARCHAR(50),
                    StartDate DATETIME NOT NULL,
                    EndDate DATETIME,
                    IsActive TINYINT NOT NULL DEFAULT 1,
                    FOREIGN KEY (PatientId) REFERENCES Users(Id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
        }

        /// <summary>
        /// Fix Goals table - add ALL missing columns for existing databases
        /// </summary>
        private static void FixGoalsTable(IDbConnection connection)
        {
            // Add all potentially missing columns
            var columnsToAdd = new[]
            {
                "ALTER TABLE Goals ADD COLUMN GoalType INT NOT NULL DEFAULT 0",
                "ALTER TABLE Goals ADD COLUMN TargetValue DOUBLE NOT NULL DEFAULT 0",
                "ALTER TABLE Goals ADD COLUMN CurrentValue DOUBLE NOT NULL DEFAULT 0",
                "ALTER TABLE Goals ADD COLUMN Unit VARCHAR(50) DEFAULT 'birim'",
                "ALTER TABLE Goals ADD COLUMN StartDate DATETIME DEFAULT CURRENT_TIMESTAMP",
                "ALTER TABLE Goals ADD COLUMN EndDate DATETIME NULL",
                "ALTER TABLE Goals ADD COLUMN IsActive TINYINT NOT NULL DEFAULT 1"
            };

            foreach (var sql in columnsToAdd)
            {
                try
                {
                    ExecuteNonQuery(connection, sql);
                }
                catch { /* Column already exists - ignore */ }
            }
        }


        private static void CreateNotesTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS Notes (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    PatientId INT NOT NULL,
                    DoctorId INT NOT NULL,
                    DoctorName VARCHAR(255),
                    Content TEXT NOT NULL,
                    Date DATETIME NOT NULL,
                    Category INT DEFAULT 0,
                    FOREIGN KEY (PatientId) REFERENCES Users(Id) ON DELETE CASCADE,
                    FOREIGN KEY (DoctorId) REFERENCES Users(Id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
        }

        private static void CreateMessagesTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS Messages (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    FromUserId INT NOT NULL,
                    ToUserId INT NOT NULL,
                    FromUserName VARCHAR(255),
                    ToUserName VARCHAR(255),
                    Content TEXT NOT NULL,
                    SentAt DATETIME NOT NULL,
                    IsRead TINYINT NOT NULL DEFAULT 0,
                    Category INT DEFAULT 0,
                    Priority INT DEFAULT 0,
                    ParentMessageId INT,
                    IsDeletedBySender TINYINT DEFAULT 0,
                    IsDeletedByReceiver TINYINT DEFAULT 0,
                    IsDeletedForEveryone TINYINT DEFAULT 0,
                    FOREIGN KEY (FromUserId) REFERENCES Users(Id) ON DELETE CASCADE,
                    FOREIGN KEY (ToUserId) REFERENCES Users(Id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
        }

        private static void CreateWeightEntriesTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS WeightEntries (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    PatientId INT NOT NULL,
                    Date DATETIME NOT NULL,
                    Weight DOUBLE NOT NULL,
                    Notes TEXT,
                    FOREIGN KEY (PatientId) REFERENCES Users(Id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
        }

        private static void CreateDietWeeksTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS DietWeeks (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    PatientId INT NOT NULL,
                    WeekStartDate DATETIME NOT NULL,
                    CreatedAt DATETIME NOT NULL,
                    CreatedByDoctorId INT NOT NULL,
                    WeekNotes TEXT,
                    Version INT DEFAULT 1,
                    IsActive TINYINT NOT NULL DEFAULT 1,
                    FOREIGN KEY (PatientId) REFERENCES Users(Id) ON DELETE CASCADE,
                    FOREIGN KEY (CreatedByDoctorId) REFERENCES Users(Id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
        }

        private static void CreateDietDaysTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS DietDays (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    DietWeekId INT NOT NULL,
                    Date DATETIME NOT NULL,
                    Notes TEXT,
                    TargetCalories DOUBLE DEFAULT 0,
                    FOREIGN KEY (DietWeekId) REFERENCES DietWeeks(Id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
        }

        private static void CreateMealItemsTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS MealItems (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    DietDayId INT NOT NULL,
                    MealType INT NOT NULL,
                    Name VARCHAR(255) NOT NULL,
                    Calories DOUBLE NOT NULL,
                    Protein DOUBLE NOT NULL,
                    Carbs DOUBLE NOT NULL,
                    Fat DOUBLE NOT NULL,
                    PortionSize VARCHAR(100),
                    TimeRange VARCHAR(100),
                    IsConfirmedByPatient TINYINT DEFAULT 0,
                    ConfirmedAt DATETIME,
                    SkippedReason TEXT,
                    FOREIGN KEY (DietDayId) REFERENCES DietDays(Id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
        }
        
        // ============================================
        // YENİ TABLOLAR - Faz 2-5
        // ============================================
        
        private static void CreateBodyMeasurementsTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS BodyMeasurements (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    PatientId INT NOT NULL,
                    Date DATETIME NOT NULL,
                    Chest DOUBLE,
                    Waist DOUBLE,
                    Hip DOUBLE,
                    Arm DOUBLE,
                    Thigh DOUBLE,
                    Neck DOUBLE,
                    Notes TEXT,
                    FOREIGN KEY (PatientId) REFERENCES Users(Id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
        }
        
        private static void CreateExerciseTasksTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS ExerciseTasks (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    PatientId INT NOT NULL,
                    DoctorId INT NOT NULL,
                    Title VARCHAR(255) NOT NULL,
                    Description TEXT,
                    DurationMinutes INT DEFAULT 30,
                    DifficultyLevel INT DEFAULT 1,
                    DueDate DATETIME NOT NULL,
                    IsCompleted TINYINT DEFAULT 0,
                    CompletedAt DATETIME,
                    PatientNote TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (PatientId) REFERENCES Users(Id) ON DELETE CASCADE,
                    FOREIGN KEY (DoctorId) REFERENCES Users(Id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
            
            // Add progress tracking columns for existing databases
            FixExerciseTasksTable(connection);
        }
        
        /// <summary>
        /// Add progress tracking columns to ExerciseTasks table for existing databases
        /// </summary>
        private static void FixExerciseTasksTable(IDbConnection connection)
        {
            var columnsToAdd = new[]
            {
                "ALTER TABLE ExerciseTasks ADD COLUMN ProgressPercentage INT DEFAULT 0",
                "ALTER TABLE ExerciseTasks ADD COLUMN CompletedDuration INT DEFAULT 0",
                "ALTER TABLE ExerciseTasks ADD COLUMN PatientFeedback TEXT"
            };

            foreach (var sql in columnsToAdd)
            {
                try
                {
                    ExecuteNonQuery(connection, sql);
                }
                catch { /* Column already exists - ignore */ }
            }
        }
        
        private static void CreateAppointmentsTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS Appointments (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    PatientId INT NOT NULL,
                    DoctorId INT NOT NULL,
                    DateTime DATETIME NOT NULL,
                    Type INT NOT NULL DEFAULT 0,
                    Status INT NOT NULL DEFAULT 0,
                    Notes TEXT,
                    Price DECIMAL(10,2) DEFAULT 0,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (PatientId) REFERENCES Users(Id) ON DELETE CASCADE,
                    FOREIGN KEY (DoctorId) REFERENCES Users(Id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
            
            // Add Price column if it doesn't exist (for existing databases)
            try
            {
                ExecuteNonQuery(connection, "ALTER TABLE Appointments ADD COLUMN Price DECIMAL(10,2) DEFAULT 0");
            }
            catch { /* Column already exists */ }
        }
        
        private static void CreateMealFeedbackTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS MealFeedback (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    PatientId INT NOT NULL,
                    MealAssignmentId INT,
                    Date DATETIME NOT NULL,
                    MealTime INT NOT NULL,
                    IsConsumed TINYINT DEFAULT 0,
                    Reason TEXT,
                    Notes TEXT,
                    MealName VARCHAR(255),
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (PatientId) REFERENCES Users(Id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
        }

        private static void CreateMealsTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS Meals (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    DoctorId INT,
                    Name VARCHAR(255),
                    Description TEXT,
                    MealTime INT,
                    Calories DOUBLE,
                    Protein DOUBLE,
                    Carbs DOUBLE,
                    Fat DOUBLE,
                    PortionGrams DOUBLE,
                    PortionDescription VARCHAR(100),
                    Category VARCHAR(50),
                    Notes TEXT,
                    IsActive TINYINT,
                    CreatedAt DATETIME,
                    ResimYolu VARCHAR(255)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
        }

        private static void CreatePatientMealAssignmentsTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS PatientMealAssignments (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    PatientId INT NOT NULL,
                    DoctorId INT NOT NULL,
                    MealId INT,
                    WeekStartDate DATETIME NOT NULL,
                    DayOfWeek INT NOT NULL,
                    MealTime INT NOT NULL,
                    MealName VARCHAR(255),
                    Description TEXT,
                    Calories DOUBLE,
                    Protein DOUBLE,
                    Carbs DOUBLE,
                    Fat DOUBLE,
                    PortionGrams DOUBLE,
                    PortionDescription VARCHAR(100),
                    IsConsumed TINYINT DEFAULT 0,
                    ConsumedAt DATETIME,
                    Notes TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
        }
        private static void CreateAiChatLogsTable(IDbConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS AiChatLogs (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    PatientId INT NOT NULL,
                    DoctorId INT NOT NULL,
                    Message TEXT,
                    IsAiResponse TINYINT NOT NULL DEFAULT 0,
                    Timestamp DATETIME NOT NULL,
                    FOREIGN KEY (PatientId) REFERENCES Users(Id) ON DELETE CASCADE,
                    FOREIGN KEY (DoctorId) REFERENCES Users(Id) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            ExecuteNonQuery(connection, sql);
        }
    }
}
