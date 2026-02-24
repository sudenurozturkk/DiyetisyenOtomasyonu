using System;
using System.Collections.Generic;
using System.Data;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    /// <summary>
    /// Patient Repository - Hasta veri erişim katmanı
    /// Hasta ve User tablolarını birlikte yönetir
    /// </summary>
    public class PatientRepository : BaseRepository<Patient>
    {
        private readonly UserRepository _userRepository;

        public PatientRepository() : base("Patients")
        {
            _userRepository = new UserRepository();
        }

        protected override Patient MapFromReader(IDataReader reader)
        {
            var patient = new Patient
            {
                Id = Convert.ToInt32(reader["Id"]),
                Yas = Convert.ToInt32(reader["Yas"]),
                Boy = Convert.ToDouble(reader["Boy"]),
                BaslangicKilosu = Convert.ToDouble(reader["BaslangicKilosu"]),
                GuncelKilo = Convert.ToDouble(reader["GuncelKilo"]),
                DoctorId = Convert.ToInt32(reader["DoctorId"])
            };

            // Nullable alanlar
            if (reader["Cinsiyet"] != DBNull.Value)
                patient.Cinsiyet = reader["Cinsiyet"].ToString();

            if (reader["Notlar"] != DBNull.Value)
                patient.Notlar = reader["Notlar"].ToString();

            if (reader["ThyroidStatus"] != DBNull.Value)
                patient.ThyroidStatus = reader["ThyroidStatus"].ToString();

            if (reader["InsulinStatus"] != DBNull.Value)
                patient.InsulinStatus = reader["InsulinStatus"].ToString();

            if (reader["MedicalHistory"] != DBNull.Value)
                patient.MedicalHistory = reader["MedicalHistory"].ToString();

            if (reader["Medications"] != DBNull.Value)
                patient.Medications = reader["Medications"].ToString();

            if (reader["AllergiesText"] != DBNull.Value)
                patient.AllergiesText = reader["AllergiesText"].ToString();

            if (reader["LifestyleType"] != DBNull.Value)
                patient.LifestyleType = (LifestyleType)Convert.ToInt32(reader["LifestyleType"]);

            if (reader["ActivityLevel"] != DBNull.Value)
                patient.ActivityLevel = (ActivityLevel)Convert.ToInt32(reader["ActivityLevel"]);

            return patient;
        }

        protected override Dictionary<string, object> MapToParameters(Patient entity)
        {
            return new Dictionary<string, object>
            {
                { "Id", entity.Id },
                { "Cinsiyet", entity.Cinsiyet },
                { "Yas", entity.Yas },
                { "Boy", entity.Boy },
                { "BaslangicKilosu", entity.BaslangicKilosu },
                { "GuncelKilo", entity.GuncelKilo },
                { "DoctorId", entity.DoctorId },
                { "Notlar", entity.Notlar },
                { "ThyroidStatus", entity.ThyroidStatus },
                { "InsulinStatus", entity.InsulinStatus },
                { "MedicalHistory", entity.MedicalHistory },
                { "Medications", entity.Medications },
                { "AllergiesText", entity.AllergiesText },
                { "LifestyleType", (int)entity.LifestyleType },
                { "ActivityLevel", (int)entity.ActivityLevel }
            };
        }

        protected override string GetInsertSql()
        {
            return @"INSERT INTO Patients (Id, Cinsiyet, Yas, Boy, BaslangicKilosu, GuncelKilo, DoctorId, 
                     Notlar, ThyroidStatus, InsulinStatus, MedicalHistory, Medications, AllergiesText, LifestyleType, ActivityLevel)
                     VALUES (@Id, @Cinsiyet, @Yas, @Boy, @BaslangicKilosu, @GuncelKilo, @DoctorId,
                     @Notlar, @ThyroidStatus, @InsulinStatus, @MedicalHistory, @Medications, @AllergiesText, @LifestyleType, @ActivityLevel)";
        }

        protected override string GetUpdateSql()
        {
            return @"UPDATE Patients SET Cinsiyet = @Cinsiyet, Yas = @Yas, Boy = @Boy,
                     BaslangicKilosu = @BaslangicKilosu, GuncelKilo = @GuncelKilo, Notlar = @Notlar,
                     ThyroidStatus = @ThyroidStatus, InsulinStatus = @InsulinStatus, 
                     MedicalHistory = @MedicalHistory, Medications = @Medications, AllergiesText = @AllergiesText,
                     LifestyleType = @LifestyleType, ActivityLevel = @ActivityLevel WHERE Id = @Id";
        }

        public override bool Update(Patient entity)
        {
            using (var connection = CreateConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Update Patients table
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            cmd.CommandText = GetUpdateSql();
                            
                            var parameters = MapToParameters(entity);
                            foreach (var param in parameters)
                            {
                                AddParameter(cmd, "@" + param.Key, param.Value);
                            }
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Update Users table (AdSoyad, ProfilePhoto)
                        using (var cmdUser = connection.CreateCommand())
                        {
                            cmdUser.Transaction = transaction;
                            cmdUser.CommandText = "UPDATE Users SET AdSoyad = @AdSoyad, ProfilePhoto = @ProfilePhoto WHERE Id = @Id";
                            AddParameter(cmdUser, "@AdSoyad", entity.AdSoyad);
                            AddParameter(cmdUser, "@ProfilePhoto", entity.ProfilePhoto);
                            AddParameter(cmdUser, "@Id", entity.Id);
                            cmdUser.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
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
        /// Hasta bilgilerini User bilgileriyle birlikte getirir
        /// </summary>
        public Patient GetFullPatientById(int id)
        {
            var patient = GetById(id);
            if (patient == null) return null;

            var user = _userRepository.GetById(id);
            if (user != null)
            {
                patient.AdSoyad = user.AdSoyad;
                patient.KullaniciAdi = user.KullaniciAdi;
                patient.ParolaHash = user.ParolaHash;
                patient.Role = user.Role;
                patient.KayitTarihi = user.KayitTarihi;
                patient.AktifMi = user.AktifMi;
                patient.ProfilePhoto = user.ProfilePhoto; // ProfilePhoto'yu User'dan al
            }

            return patient;
        }

        /// <summary>
        /// Tüm hastaları User bilgileriyle birlikte getirir
        /// </summary>
        public IEnumerable<Patient> GetAllWithUserInfo()
        {
            var patients = new List<Patient>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT p.*, u.AdSoyad, u.KullaniciAdi, u.ParolaHash, u.Role, u.KayitTarihi, u.AktifMi
                        FROM Patients p
                        INNER JOIN Users u ON p.Id = u.Id
                        WHERE u.AktifMi = 1";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var patient = MapFromReader(reader);
                            patient.AdSoyad = reader["AdSoyad"].ToString();
                            patient.KullaniciAdi = reader["KullaniciAdi"].ToString();
                            patient.ParolaHash = reader["ParolaHash"].ToString();
                            patient.Role = (UserRole)Convert.ToInt32(reader["Role"]);
                            patient.KayitTarihi = DateTime.Parse(reader["KayitTarihi"].ToString());
                            patient.AktifMi = Convert.ToInt32(reader["AktifMi"]) == 1;
                            patients.Add(patient);
                        }
                    }
                }
            }
            return patients;
        }

        /// <summary>
        /// Doktora ait hastaları getirir
        /// </summary>
        public IEnumerable<Patient> GetByDoctorId(int doctorId)
        {
            var patients = new List<Patient>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT p.*, u.AdSoyad, u.KullaniciAdi, u.ParolaHash, u.Role, u.KayitTarihi, u.AktifMi
                        FROM Patients p
                        INNER JOIN Users u ON p.Id = u.Id
                        WHERE p.DoctorId = @doctorId AND u.AktifMi = 1";
                    AddParameter(cmd, "@doctorId", doctorId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var patient = MapFromReader(reader);
                            patient.AdSoyad = reader["AdSoyad"].ToString();
                            patient.KullaniciAdi = reader["KullaniciAdi"].ToString();
                            patient.ParolaHash = reader["ParolaHash"].ToString();
                            patient.Role = (UserRole)Convert.ToInt32(reader["Role"]);
                            patient.KayitTarihi = DateTime.Parse(reader["KayitTarihi"].ToString());
                            patient.AktifMi = Convert.ToInt32(reader["AktifMi"]) == 1;
                            patients.Add(patient);
                        }
                    }
                }
            }
            return patients;
        }

        /// <summary>
        /// Hasta arar (ad soyad veya kullanıcı adı ile)
        /// </summary>
        public IEnumerable<Patient> Search(string searchText, int? doctorId = null)
        {
            var patients = new List<Patient>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    var sql = @"
                        SELECT p.*, u.AdSoyad, u.KullaniciAdi, u.ParolaHash, u.Role, u.KayitTarihi, u.AktifMi
                        FROM Patients p
                        INNER JOIN Users u ON p.Id = u.Id
                        WHERE u.AktifMi = 1 AND (u.AdSoyad LIKE @search OR u.KullaniciAdi LIKE @search)";
                    
                    if (doctorId.HasValue)
                        sql += " AND p.DoctorId = @doctorId";

                    cmd.CommandText = sql;
                    AddParameter(cmd, "@search", $"%{searchText}%");
                    if (doctorId.HasValue)
                        AddParameter(cmd, "@doctorId", doctorId.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var patient = MapFromReader(reader);
                            patient.AdSoyad = reader["AdSoyad"].ToString();
                            patient.KullaniciAdi = reader["KullaniciAdi"].ToString();
                            patient.ParolaHash = reader["ParolaHash"].ToString();
                            patient.Role = (UserRole)Convert.ToInt32(reader["Role"]);
                            patient.KayitTarihi = DateTime.Parse(reader["KayitTarihi"].ToString());
                            patient.AktifMi = Convert.ToInt32(reader["AktifMi"]) == 1;
                            patients.Add(patient);
                        }
                    }
                }
            }
            return patients;
        }

        /// <summary>
        /// Yeni hasta oluşturur (User ve Patient birlikte)
        /// </summary>
        public int CreatePatient(Patient patient)
        {
            using (var connection = CreateConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int userId;
                        // Önce User oluştur
                        using (var userCmd = connection.CreateCommand())
                        {
                            userCmd.Transaction = transaction;
                            userCmd.CommandText = @"
                                INSERT INTO Users (AdSoyad, KullaniciAdi, ParolaHash, Role, KayitTarihi, AktifMi)
                                VALUES (@adSoyad, @kullaniciAdi, @parolaHash, @role, @kayitTarihi, @aktifMi);
                                SELECT LAST_INSERT_ID();";

                            AddParameter(userCmd, "@adSoyad", patient.AdSoyad);
                            AddParameter(userCmd, "@kullaniciAdi", patient.KullaniciAdi);
                            AddParameter(userCmd, "@parolaHash", patient.ParolaHash);
                            AddParameter(userCmd, "@role", (int)UserRole.Patient);
                            AddParameter(userCmd, "@kayitTarihi", DateTime.Now.ToString("o"));
                            AddParameter(userCmd, "@aktifMi", 1);

                            userId = Convert.ToInt32(userCmd.ExecuteScalar());
                        }

                        // Sonra Patient oluştur
                        using (var patientCmd = connection.CreateCommand())
                        {
                            patientCmd.Transaction = transaction;
                            patientCmd.CommandText = @"
                                INSERT INTO Patients (Id, Cinsiyet, Yas, Boy, BaslangicKilosu, GuncelKilo, DoctorId,
                                    Notlar, ThyroidStatus, InsulinStatus, MedicalHistory, Medications, AllergiesText, LifestyleType, ActivityLevel)
                                VALUES (@id, @cinsiyet, @yas, @boy, @baslangicKilo, @guncelKilo, @doctorId,
                                    @notlar, @thyroid, @insulin, @history, @medications, @allergies, @lifestyle, @activity)";

                            AddParameter(patientCmd, "@id", userId);
                            AddParameter(patientCmd, "@cinsiyet", patient.Cinsiyet);
                            AddParameter(patientCmd, "@yas", patient.Yas);
                            AddParameter(patientCmd, "@boy", patient.Boy);
                            AddParameter(patientCmd, "@baslangicKilo", patient.BaslangicKilosu);
                            AddParameter(patientCmd, "@guncelKilo", patient.GuncelKilo);
                            AddParameter(patientCmd, "@doctorId", patient.DoctorId);
                            AddParameter(patientCmd, "@notlar", patient.Notlar);
                            AddParameter(patientCmd, "@thyroid", patient.ThyroidStatus);
                            AddParameter(patientCmd, "@insulin", patient.InsulinStatus);
                            AddParameter(patientCmd, "@history", patient.MedicalHistory);
                            AddParameter(patientCmd, "@medications", patient.Medications);
                            AddParameter(patientCmd, "@allergies", patient.AllergiesText);
                            AddParameter(patientCmd, "@lifestyle", (int)patient.LifestyleType);
                            AddParameter(patientCmd, "@activity", (int)patient.ActivityLevel);

                            patientCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return userId;
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
        /// Hastayı siler (User ve Patient birlikte)
        /// </summary>
        public bool DeletePatient(int id)
        {
            using (var connection = CreateConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Önce Patient sil
                        using (var patientCmd = connection.CreateCommand())
                        {
                            patientCmd.Transaction = transaction;
                            
                            // 1. İlişkili kayıtları sil (Manuel Cascade)
                            var tables = new[] { "Goals", "Appointments", "Messages", "WeightEntries", "ExerciseTasks", "PatientMealAssignments" };
                            foreach (var table in tables)
                            {
                                using (var cmd = connection.CreateCommand())
                                {
                                    cmd.Transaction = transaction;
                                    // Messages tablosunda FromUserId veya ToUserId olabilir, diğerleri PatientId veya UserId
                                    if (table == "Messages")
                                        cmd.CommandText = $"DELETE FROM {table} WHERE FromUserId = @id OR ToUserId = @id";
                                    else if (table == "Appointments" || table == "PatientMealAssignments")
                                        cmd.CommandText = $"DELETE FROM {table} WHERE PatientId = @id";
                                    else
                                        cmd.CommandText = $"DELETE FROM {table} WHERE PatientId = @id"; // Goals, WeightEntries, ExerciseTasks
                                        
                                    AddParameter(cmd, "@id", id);
                                    try { cmd.ExecuteNonQuery(); } catch { } // Tablo yoksa veya hata olursa devam et
                                }
                            }

                            // 2. Patient sil
                            patientCmd.CommandText = "DELETE FROM Patients WHERE Id = @id";
                            AddParameter(patientCmd, "@id", id);
                            patientCmd.ExecuteNonQuery();
                        }

                        // Sonra User pasif yap
                        using (var userCmd = connection.CreateCommand())
                        {
                            userCmd.Transaction = transaction;
                            userCmd.CommandText = "UPDATE Users SET AktifMi = 0 WHERE Id = @id";
                            AddParameter(userCmd, "@id", id);
                            userCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}

