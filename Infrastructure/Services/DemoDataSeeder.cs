using System;
using System.Collections.Generic;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// Demo Data Seeder - Ahmet Yılmaz için 3 aylık kapsamlı örnek veri
    /// </summary>
    public static class DemoDataSeeder
    {
        public static void SeedProfessionalData()
        {
            try
            {
                // 1. Doktor ID'sini al (yoksa varsayılan 1)
                int doctorId = 1;

                // 2. Örnek hastaları oluştur - 15 hasta
                var patients = new[]
                {
                    new { Name = "Ahmet Yılmaz", User = "ahmetyilmaz", Gender = "Erkek", Age = 35, Height = 178.0, Weight = 84.0, Job = LifestyleType.OfficeWorker, Note = "Masa başı çalışan, bel ağrısı şikayeti var." },
                    new { Name = "Ayşe Demir", User = "aysedemir", Gender = "Kadın", Age = 28, Height = 165.0, Weight = 68.0, Job = LifestyleType.OfficeWorker, Note = "Öğretmen, düzensiz besleniyor." },
                    new { Name = "Mehmet Kaya", User = "mehmetkaya", Gender = "Erkek", Age = 45, Height = 182.0, Weight = 95.0, Job = LifestyleType.OfficeWorker, Note = "Şoför, hareketsiz yaşam." },
                    new { Name = "Zeynep Çelik", User = "zeynepcelik", Gender = "Kadın", Age = 32, Height = 170.0, Weight = 62.0, Job = LifestyleType.HomeMaker, Note = "Ev hanımı, doğum sonrası kilo vermek istiyor." },
                    new { Name = "Can Yıldız", User = "canyildiz", Gender = "Erkek", Age = 22, Height = 185.0, Weight = 78.0, Job = LifestyleType.Student, Note = "Üniversite öğrencisi, sporcu beslenmesi istiyor." },
                    new { Name = "Elif Şahin", User = "elifsahin", Gender = "Kadın", Age = 40, Height = 160.0, Weight = 85.0, Job = LifestyleType.OfficeWorker, Note = "Bankacı, insülin direnci var." },
                    new { Name = "Burak Özkan", User = "burakozkan", Gender = "Erkek", Age = 38, Height = 175.0, Weight = 88.0, Job = LifestyleType.OfficeWorker, Note = "Yazılımcı, gece çalışması yapıyor." },
                    new { Name = "Selin Arslan", User = "selinarslan", Gender = "Kadın", Age = 25, Height = 168.0, Weight = 58.0, Job = LifestyleType.Student, Note = "Tıp öğrencisi, kilo almak istiyor." },
                    new { Name = "Murat Koç", User = "muratkoc", Gender = "Erkek", Age = 52, Height = 172.0, Weight = 92.0, Job = LifestyleType.Retired, Note = "Emekli, diyabet riski var." },
                    new { Name = "Deniz Akın", User = "denizakin", Gender = "Kadın", Age = 30, Height = 162.0, Weight = 70.0, Job = LifestyleType.OfficeWorker, Note = "Avukat, stres yiyor." },
                    new { Name = "Cem Yalçın", User = "cemyalcin", Gender = "Erkek", Age = 42, Height = 180.0, Weight = 102.0, Job = LifestyleType.OfficeWorker, Note = "Mühendis, obezite sınırında." },
                    new { Name = "Pınar Erdem", User = "pinarerdem", Gender = "Kadın", Age = 27, Height = 158.0, Weight = 55.0, Job = LifestyleType.HomeMaker, Note = "Yeni anne, emzirme döneminde." },
                    new { Name = "Oğuz Kara", User = "oguzkara", Gender = "Erkek", Age = 33, Height = 176.0, Weight = 82.0, Job = LifestyleType.OfficeWorker, Note = "Satış temsilcisi, sık seyahat ediyor." },
                    new { Name = "İrem Tunç", User = "iremtunc", Gender = "Kadın", Age = 35, Height = 166.0, Weight = 72.0, Job = LifestyleType.OfficeWorker, Note = "Hemşire, vardiyalı çalışıyor." },
                    new { Name = "Serkan Aydın", User = "serkanydin", Gender = "Erkek", Age = 29, Height = 183.0, Weight = 90.0, Job = LifestyleType.Student, Note = "Yüksek lisans öğrencisi, spor yapıyor." }
                };

                foreach (var p in patients)
                {
                    int patientId = FindPatientIdByName(p.Name);
                    if (patientId == 0)
                    {
                        patientId = CreateDemoPatient(p.Name, p.User, p.Gender, p.Age, p.Height, p.Weight, doctorId, p.Job, p.Note);
                        Console.WriteLine($"Hasta oluşturuldu: {p.Name}");
                    }

                    // Veri ekle
                    SeedWeightEntries(patientId, p.Weight, p.Weight - 5);
                    SeedGoals(patientId);
                    SeedExerciseTasks(patientId, doctorId);
                    SeedBodyMeasurements(patientId);
                    SeedAppointments(patientId, doctorId);
                    SeedMessages(patientId, doctorId, p.Name);
                }

                // Bugünkü randevular ekle (dashboard'da görünsün)
                SeedTodayAppointments(doctorId);
                
                // Öğünler/Yemek tarifleri ekle
                SeedMeals(doctorId);
                
                // Notlar ekle
                SeedNotes(doctorId);

                Console.WriteLine("Profesyonel demo veriler başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Demo veri eklenirken hata: " + ex.Message);
            }
        }

        public static void SeedDemoData()
        {
            SeedProfessionalData();
        }

        private static int FindPatientIdByName(string name)
        {
            using (var connection = DatabaseConfig.Instance.CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id FROM Users WHERE AdSoyad LIKE @name LIMIT 1";
                    var param = cmd.CreateParameter();
                    param.ParameterName = "@name";
                    param.Value = "%" + name + "%";
                    cmd.Parameters.Add(param);

                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        private static int GetDoctorIdForPatient(int patientId)
        {
            using (var connection = DatabaseConfig.Instance.CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT DoctorId FROM Patients WHERE Id = @id";
                    var param = cmd.CreateParameter();
                    param.ParameterName = "@id";
                    param.Value = patientId;
                    cmd.Parameters.Add(param);

                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        private static void ExecuteSql(string sql)
        {
            using (var connection = DatabaseConfig.Instance.CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 3 aylık kilo takip verileri - 84 kg'dan 77 kg'a düşüş
        /// </summary>
        /// <summary>
        /// 3 aylık kilo takip verileri
        /// </summary>
        private static void SeedWeightEntries(int patientId, double startWeight = 84.0, double targetWeight = 75.0)
        {
            // Önce mevcut verileri temizle
            ExecuteSql($"DELETE FROM WeightEntries WHERE PatientId = {patientId}");

            var random = new Random(patientId); // Random seed patientId olsun ki her hasta farklı olsun ama deterministik
            DateTime startDate = DateTime.Today.AddDays(-90);
            double currentWeight = startWeight;
            double weeklyLoss = (startWeight - targetWeight) / 12.0; // 12 hafta

            var entries = new List<string>();
            for (int week = 0; week < 12; week++)
            {
                for (int day = 0; day < 7; day++)
                {
                    DateTime date = startDate.AddDays(week * 7 + day);
                    if (date > DateTime.Today) break;

                    // Haftalık düşüş + günlük varyasyon
                    double dailyVariation = (random.NextDouble() - 0.5) * 0.4;
                    double weight = startWeight - (week * weeklyLoss) - (day * weeklyLoss / 7) + dailyVariation;
                    weight = Math.Round(weight, 1);

                    string notes = "";
                    if (day == 0) notes = "Haftalık ölçüm";
                    else if (random.NextDouble() < 0.1) notes = "Spor sonrası";

                    entries.Add($"({patientId}, '{date:yyyy-MM-dd}', {weight.ToString().Replace(',', '.')}, '{notes}')");
                    currentWeight = weight;
                }
            }

            if (entries.Count > 0)
            {
                string sql = $"INSERT INTO WeightEntries (PatientId, Date, Weight, Notes) VALUES {string.Join(", ", entries)}";
                ExecuteSql(sql);
            }

            // Hasta güncel kilosunu güncelle
            ExecuteSql($"UPDATE Patients SET GuncelKilo = {currentWeight.ToString().Replace(',', '.')} WHERE Id = {patientId}");
        }

        /// <summary>
        /// Hedefler - Kilo, Su, Adım, Egzersiz
        /// </summary>
        private static void SeedGoals(int patientId)
        {
            ExecuteSql($"DELETE FROM Goals WHERE PatientId = {patientId}");

            DateTime now = DateTime.Now;
            string startDate = now.AddMonths(-2).ToString("yyyy-MM-dd");
            string endDate = now.AddMonths(1).ToString("yyyy-MM-dd");

            // Kilo hedefi - devam ediyor
            ExecuteSql($@"INSERT INTO Goals (PatientId, GoalType, TargetValue, CurrentValue, Unit, StartDate, EndDate, IsActive)
                          VALUES ({patientId}, 0, 75, 77.5, 'kg', '{startDate}', '{endDate}', 1)");

            // Su hedefi - riskli
            ExecuteSql($@"INSERT INTO Goals (PatientId, GoalType, TargetValue, CurrentValue, Unit, StartDate, EndDate, IsActive)
                          VALUES ({patientId}, 1, 2.5, 1.9, 'litre', '{startDate}', '{endDate}', 1)");

            // Adım hedefi - iyi gidiyor
            ExecuteSql($@"INSERT INTO Goals (PatientId, GoalType, TargetValue, CurrentValue, Unit, StartDate, EndDate, IsActive)
                          VALUES ({patientId}, 2, 8000, 6800, 'adım', '{startDate}', '{endDate}', 1)");

            // Egzersiz hedefi - yeni
            ExecuteSql($@"INSERT INTO Goals (PatientId, GoalType, TargetValue, CurrentValue, Unit, StartDate, EndDate, IsActive)
                          VALUES ({patientId}, 3, 150, 105, 'dakika', '{now.AddDays(-7):yyyy-MM-dd}', '{endDate}', 1)");
        }

        /// <summary>
        /// 3 aylık egzersiz görevleri - çeşitli durumlar
        /// </summary>
        private static void SeedExerciseTasks(int patientId, int doctorId)
        {
            ExecuteSql($"DELETE FROM ExerciseTasks WHERE PatientId = {patientId}");

            var random = new Random(42);
            DateTime startDate = DateTime.Today.AddDays(-90);
            
            string[] exercises = { "Yürüyüş", "Koşu", "Bisiklet", "Yüzme", "Yoga", "Pilates", "Ağırlık Antrenmanı" };
            int[] durations = { 20, 30, 45, 60 };
            
            var tasks = new List<string>();
            
            for (int week = 0; week < 12; week++)
            {
                // Haftada 3-5 görev
                int tasksPerWeek = random.Next(3, 6);
                for (int t = 0; t < tasksPerWeek; t++)
                {
                    int dayOffset = random.Next(0, 7);
                    DateTime dueDate = startDate.AddDays(week * 7 + dayOffset);
                    
                    string exercise = exercises[random.Next(exercises.Length)];
                    int duration = durations[random.Next(durations.Length)];
                    int difficulty = random.Next(1, 6);
                    
                    bool isCompleted = dueDate < DateTime.Today.AddDays(-1);
                    int progressPercent = 0;
                    int completedDuration = 0;
                    string feedback = "";
                    
                    if (isCompleted)
                    {
                        // %70 tam tamamlandı, %20 kısmi, %10 yapılmadı
                        double rand = random.NextDouble();
                        if (rand < 0.70)
                        {
                            progressPercent = 100;
                            completedDuration = duration;
                            feedback = random.NextDouble() < 0.3 ? "Harika hissediyorum!" : "";
                        }
                        else if (rand < 0.90)
                        {
                            progressPercent = random.Next(40, 90);
                            completedDuration = (int)(duration * progressPercent / 100.0);
                            feedback = "Yarım kaldı ama devam edeceğim";
                        }
                        else
                        {
                            progressPercent = 0;
                            completedDuration = 0;
                            isCompleted = false;
                            feedback = "Bugün çok yorgundum";
                        }
                    }
                    else if (dueDate.Date == DateTime.Today)
                    {
                        // Bugünkü görevler - bazıları başladı
                        if (random.NextDouble() < 0.5)
                        {
                            progressPercent = random.Next(20, 60);
                            completedDuration = (int)(duration * progressPercent / 100.0);
                        }
                    }
                    
                    string completedAt = isCompleted && progressPercent >= 100 ? $"'{dueDate:yyyy-MM-dd HH:mm:ss}'" : "NULL";
                    
                    tasks.Add($@"({patientId}, {doctorId}, '{duration} Dakika {exercise}', 
                        'Haftalık egzersiz planı', {duration}, {difficulty}, 
                        '{dueDate:yyyy-MM-dd}', {(isCompleted && progressPercent >= 100 ? 1 : 0)}, {completedAt}, 
                        '{feedback}', NOW(), {progressPercent}, {completedDuration}, '{feedback}')");
                }
            }
            
            // Gelecek hafta için görevler
            for (int d = 1; d <= 7; d++)
            {
                DateTime dueDate = DateTime.Today.AddDays(d);
                string exercise = exercises[random.Next(exercises.Length)];
                int duration = durations[random.Next(durations.Length)];
                int difficulty = random.Next(1, 4);
                
                tasks.Add($@"({patientId}, {doctorId}, '{duration} Dakika {exercise}', 
                    'Yeni hafta planı', {duration}, {difficulty}, 
                    '{dueDate:yyyy-MM-dd}', 0, NULL, '', NOW(), 0, 0, '')");
            }

            if (tasks.Count > 0)
            {
                string sql = $@"INSERT INTO ExerciseTasks 
                    (PatientId, DoctorId, Title, Description, DurationMinutes, DifficultyLevel, 
                     DueDate, IsCompleted, CompletedAt, PatientNote, CreatedAt, 
                     ProgressPercentage, CompletedDuration, PatientFeedback) 
                    VALUES {string.Join(", ", tasks)}";
                ExecuteSql(sql);
            }
        }

        /// <summary>
        /// 3 aylık vücut ölçümleri
        /// </summary>
        private static void SeedBodyMeasurements(int patientId)
        {
            ExecuteSql($"DELETE FROM BodyMeasurements WHERE PatientId = {patientId}");

            var random = new Random(42);
            DateTime startDate = DateTime.Today.AddDays(-90);
            
            // Başlangıç ölçüleri
            double chest = 105, waist = 96, hip = 102, arm = 36, thigh = 60, neck = 42;
            
            // Haftalık ölçümler (2 haftada bir)
            var measurements = new List<string>();
            for (int week = 0; week <= 12; week += 2)
            {
                DateTime date = startDate.AddDays(week * 7);
                if (date > DateTime.Today) break;
                
                // Kademeli azalma
                double reduction = week * 0.3;
                double c = Math.Round(chest - reduction * 0.8 + (random.NextDouble() - 0.5), 1);
                double w = Math.Round(waist - reduction * 1.2 + (random.NextDouble() - 0.5), 1);
                double h = Math.Round(hip - reduction * 0.6 + (random.NextDouble() - 0.5), 1);
                double a = Math.Round(arm - reduction * 0.2 + (random.NextDouble() - 0.3), 1);
                double t = Math.Round(thigh - reduction * 0.4 + (random.NextDouble() - 0.4), 1);
                double n = Math.Round(neck - reduction * 0.1 + (random.NextDouble() - 0.2), 1);

                measurements.Add($"({patientId}, '{date:yyyy-MM-dd}', {c.ToString().Replace(',', '.')}, {w.ToString().Replace(',', '.')}, {h.ToString().Replace(',', '.')}, {a.ToString().Replace(',', '.')}, {t.ToString().Replace(',', '.')}, {n.ToString().Replace(',', '.')}, 'Rutin ölçüm')");
            }

            if (measurements.Count > 0)
            {
                string sql = $"INSERT INTO BodyMeasurements (PatientId, Date, Chest, Waist, Hip, Arm, Thigh, Neck, Notes) VALUES {string.Join(", ", measurements)}";
                ExecuteSql(sql);
            }
        }

        /// <summary>
        /// 3 aylık randevular
        /// </summary>
        private static void SeedAppointments(int patientId, int doctorId)
        {
            ExecuteSql($"DELETE FROM Appointments WHERE PatientId = {patientId}");

            DateTime startDate = DateTime.Today.AddDays(-90);
            var appointments = new List<string>();

            // Geçmiş randevular - 2 haftada bir
            for (int week = 0; week < 12; week += 2)
            {
                DateTime date = startDate.AddDays(week * 7).AddHours(10 + (week % 4));
                if (date > DateTime.Today) break;
                
                int status = 2; // Tamamlandı
                string notes = week == 0 ? "İlk görüşme" : "Kontrol randevusu";
                appointments.Add($"({patientId}, {doctorId}, '{date:yyyy-MM-dd HH:mm:ss}', 0, {status}, '{notes}', 300, NOW())");
            }

            // Gelecek randevu
            DateTime nextAppt = DateTime.Today.AddDays(7).AddHours(14);
            appointments.Add($"({patientId}, {doctorId}, '{nextAppt:yyyy-MM-dd HH:mm:ss}', 0, 0, 'Aylık kontrol', 300, NOW())");

            if (appointments.Count > 0)
            {
                string sql = $"INSERT INTO Appointments (PatientId, DoctorId, DateTime, Type, Status, Notes, Price, CreatedAt) VALUES {string.Join(", ", appointments)}";
                ExecuteSql(sql);
            }
        }

        private static void SeedMessages(int patientId, int doctorId, string patientName)
        {
            ExecuteSql($"DELETE FROM Messages WHERE (FromUserId = {patientId} AND ToUserId = {doctorId}) OR (FromUserId = {doctorId} AND ToUserId = {patientId})");

            var messages = new List<string>();
            var random = new Random(patientId);
            DateTime date = DateTime.Now.AddDays(-30);

            // 1. Tanışma
            messages.Add(CreateMessageSql(patientId, doctorId, "Merhaba hocam, randevu almıştım. Diyet listemi ne zaman hazırlarsınız?", date, true));
            date = date.AddMinutes(15);
            messages.Add(CreateMessageSql(doctorId, patientId, $"Merhaba {patientName.Split(' ')[0]} Hanım/Bey, hoş geldiniz. İlk görüşmemizden sonra 24 saat içinde listenizi hazırlayıp göndereceğim.", date, true));

            // 2. Soru
            date = date.AddDays(2);
            messages.Add(CreateMessageSql(patientId, doctorId, "Hocam listedeki avokado yerine ne yiyebilirim? Bulamadım da.", date, true));
            date = date.AddMinutes(45);
            messages.Add(CreateMessageSql(doctorId, patientId, "Avokado yerine 5-6 adet zeytin veya 2 tam ceviz tüketebilirsiniz.", date, true));

            // 3. Geri bildirim
            date = date.AddDays(7);
            messages.Add(CreateMessageSql(patientId, doctorId, "İlk hafta bitti, 1.5 kilo vermişim! Çok mutluyum.", date, true));
            date = date.AddHours(1);
            messages.Add(CreateMessageSql(doctorId, patientId, "Harika bir başlangıç! Tebrik ederim. Aynı motivasyonla devam edelim.", date, true));

            // 4. Yeni soru (Okunmamış olabilir)
            date = DateTime.Now.AddHours(-2);
            bool isRead = random.NextDouble() > 0.5;
            messages.Add(CreateMessageSql(patientId, doctorId, "Hocam yarın akşam dışarıda yemek yiyeceğim, ne önerirsiniz?", date, isRead));

            if (messages.Count > 0)
            {
                string sql = "INSERT INTO Messages (FromUserId, ToUserId, Content, SentAt, IsRead, ParentMessageId, IsDeletedBySender, IsDeletedByReceiver) VALUES " + string.Join(", ", messages);
                ExecuteSql(sql);
            }
        }

        private static string CreateMessageSql(int fromId, int toId, string content, DateTime sentAt, bool isRead)
        {
            return $"({fromId}, {toId}, '{content.Replace("'", "''")}', '{sentAt:yyyy-MM-dd HH:mm:ss}', {(isRead ? 1 : 0)}, 0, 0, 0)";
        }

        private static int CreateDemoPatient(string name, string username, string gender, int age, double height, double weight, int doctorId, LifestyleType job, string note)
        {
            string hashedPassword = Security.PasswordHasher.HashPassword("12345678");
            string sqlUser = $@"INSERT INTO Users (AdSoyad, KullaniciAdi, ParolaHash, Role, KayitTarihi, AktifMi) 
                                VALUES ('{name}', '{username}', '{hashedPassword}', 1, NOW(), 1); 
                                SELECT LAST_INSERT_ID();";
            
            int userId = 0;
            using (var connection = DatabaseConfig.Instance.CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sqlUser;
                    userId = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            // Patient oluştur
            string sqlPatient = $@"INSERT INTO Patients (Id, Cinsiyet, Yas, Boy, BaslangicKilosu, GuncelKilo, DoctorId, Notlar, LifestyleType, ActivityLevel) 
                                   VALUES ({userId}, '{gender}', {age}, {height.ToString().Replace(',', '.')}, {weight.ToString().Replace(',', '.')}, {weight.ToString().Replace(',', '.')}, {doctorId}, '{note}', {(int)job}, 1)";
            ExecuteSql(sqlPatient);

            return userId;
        }

        /// <summary>
        /// Bugünkü randevular - Dashboard'da görünsün
        /// </summary>
        private static void SeedTodayAppointments(int doctorId)
        {
            // Bugün için randevular ekle
            DateTime today = DateTime.Today;
            var random = new Random(42);
            
            // Tüm hastaları al
            var patientIds = new List<int>();
            using (var connection = DatabaseConfig.Instance.CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id FROM Users WHERE Role = 1 LIMIT 10";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            patientIds.Add(reader.GetInt32(0));
                        }
                    }
                }
            }

            if (patientIds.Count == 0) return;

            // Bugün 4-5 randevu ekle
            string[] times = { "09:00", "10:30", "13:00", "14:30", "16:00" };
            string[] notes = { "Haftalık kontrol", "İlk görüşme", "Diyet planı değişikliği", "Aylık değerlendirme", "Ölçüm randevusu" };
            
            for (int i = 0; i < Math.Min(times.Length, patientIds.Count); i++)
            {
                DateTime aptTime = today.Add(TimeSpan.Parse(times[i]));
                int patientId = patientIds[i];
                int status = aptTime < DateTime.Now ? 2 : 0; // Geçmiş olanlar tamamlandı
                
                string sql = $@"INSERT INTO Appointments (PatientId, DoctorId, DateTime, Type, Status, Notes, Price, CreatedAt) 
                               VALUES ({patientId}, {doctorId}, '{aptTime:yyyy-MM-dd HH:mm:ss}', 0, {status}, '{notes[i]}', {300 + random.Next(100, 300)}, NOW())
                               ON DUPLICATE KEY UPDATE Notes = Notes";
                try { ExecuteSql(sql); } catch { }
            }
        }

        /// <summary>
        /// Yemek tarifleri/Öğünler
        /// </summary>
        private static void SeedMeals(int doctorId)
        {
            // Mevcut öğünleri kontrol et
            int mealCount = 0;
            using (var connection = DatabaseConfig.Instance.CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Meals WHERE DoctorId = @doctorId";
                    var param = cmd.CreateParameter();
                    param.ParameterName = "@doctorId";
                    param.Value = doctorId;
                    cmd.Parameters.Add(param);
                    mealCount = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            if (mealCount >= 15) return; // Zaten yeterli öğün var

            var meals = new[]
            {
                new { Name = "Kahvaltı Tabağı", Cal = 450, Prot = 20.0, Carb = 45.0, Fat = 18.0, Type = 0 },
                new { Name = "Yulaf Ezmesi", Cal = 350, Prot = 12.0, Carb = 55.0, Fat = 8.0, Type = 0 },
                new { Name = "Menemen", Cal = 280, Prot = 14.0, Carb = 15.0, Fat = 18.0, Type = 0 },
                new { Name = "Avokadolu Tost", Cal = 420, Prot = 10.0, Carb = 35.0, Fat = 28.0, Type = 0 },
                new { Name = "Smoothie Bowl", Cal = 380, Prot = 8.0, Carb = 60.0, Fat = 12.0, Type = 0 },
                new { Name = "Tavuk Salata", Cal = 380, Prot = 35.0, Carb = 15.0, Fat = 20.0, Type = 1 },
                new { Name = "Mercimek Çorbası", Cal = 180, Prot = 10.0, Carb = 28.0, Fat = 4.0, Type = 1 },
                new { Name = "Izgara Balık", Cal = 320, Prot = 38.0, Carb = 5.0, Fat = 16.0, Type = 1 },
                new { Name = "Ton Balıklı Salata", Cal = 290, Prot = 28.0, Carb = 12.0, Fat = 14.0, Type = 1 },
                new { Name = "Kinoa Tabağı", Cal = 400, Prot = 15.0, Carb = 52.0, Fat = 14.0, Type = 1 },
                new { Name = "Sebzeli Tavuk", Cal = 420, Prot = 40.0, Carb = 20.0, Fat = 18.0, Type = 2 },
                new { Name = "Fırın Somon", Cal = 380, Prot = 35.0, Carb = 8.0, Fat = 22.0, Type = 2 },
                new { Name = "Zeytinyağlı Fasulye", Cal = 220, Prot = 8.0, Carb = 25.0, Fat = 10.0, Type = 2 },
                new { Name = "Köfte Izgara", Cal = 350, Prot = 32.0, Carb = 8.0, Fat = 20.0, Type = 2 },
                new { Name = "Sebzeli Omlet", Cal = 280, Prot = 18.0, Carb = 10.0, Fat = 18.0, Type = 2 },
                new { Name = "Protein Bar", Cal = 200, Prot = 20.0, Carb = 22.0, Fat = 6.0, Type = 3 },
                new { Name = "Meyve Tabağı", Cal = 150, Prot = 2.0, Carb = 38.0, Fat = 1.0, Type = 3 },
                new { Name = "Yoğurt + Ceviz", Cal = 220, Prot = 12.0, Carb = 18.0, Fat = 12.0, Type = 3 },
                new { Name = "Badem + Kuru Üzüm", Cal = 180, Prot = 5.0, Carb = 22.0, Fat = 10.0, Type = 3 },
                new { Name = "Lor Peyniri", Cal = 120, Prot = 15.0, Carb = 3.0, Fat = 5.0, Type = 3 }
            };

            foreach (var m in meals)
            {
                string sql = $@"INSERT INTO Meals (DoctorId, Name, Calories, Protein, Carbohydrates, Fat, MealType, IsActive, CreatedAt) 
                               VALUES ({doctorId}, '{m.Name}', {m.Cal}, {m.Prot.ToString().Replace(',', '.')}, {m.Carb.ToString().Replace(',', '.')}, {m.Fat.ToString().Replace(',', '.')}, {m.Type}, 1, NOW())";
                try { ExecuteSql(sql); } catch { }
            }
        }

        /// <summary>
        /// Hasta notları
        /// </summary>
        private static void SeedNotes(int doctorId)
        {
            // Her hasta için notlar ekle
            var noteTexts = new[]
            {
                "İlk görüşme yapıldı. Hasta motivasyonu yüksek.",
                "Diyet planına uyum iyi. Haftalık kilo kaybı hedefte.",
                "Su tüketimi yetersiz, 2.5 litre hedefi verildi.",
                "Spor rutini eklendi. Haftada 3 gün yürüyüş.",
                "Kan tahlilleri normal. Vitamin D takviyesi önerildi.",
                "Tatlı isteği var, sağlıklı alternatifler sunuldu.",
                "İş stresi nedeniyle ara öğünler atlanıyor.",
                "Kilo kaybı yavaşladı, porsiyon kontrolü konuşuldu.",
                "Hafta sonu dışarıda yemek konusu tartışıldı.",
                "Hedef kiloya ulaşıldı, koruma programına geçildi."
            };

            var random = new Random(42);
            
            // Tüm hastaları al
            using (var connection = DatabaseConfig.Instance.CreateConnection())
            {
                var patientIds = new List<int>();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id FROM Users WHERE Role = 1";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            patientIds.Add(reader.GetInt32(0));
                        }
                    }
                }

                foreach (int patientId in patientIds)
                {
                    // Her hasta için 2-4 not ekle
                    int notesToAdd = random.Next(2, 5);
                    for (int i = 0; i < notesToAdd; i++)
                    {
                        DateTime noteDate = DateTime.Now.AddDays(-random.Next(1, 90));
                        string noteText = noteTexts[random.Next(noteTexts.Length)];
                        
                        string sql = $@"INSERT INTO Notes (PatientId, DoctorId, Content, CreatedAt, IsImportant) 
                                       VALUES ({patientId}, {doctorId}, '{noteText}', '{noteDate:yyyy-MM-dd HH:mm:ss}', {(random.NextDouble() < 0.3 ? 1 : 0)})";
                        try { ExecuteSql(sql); } catch { }
                    }
                }
            }
        }
    }
}
