using System;
using System.Collections.Generic;
using System.Data;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    /// <summary>
    /// Doctor Repository - Doktor veri erişim katmanı
    /// 
    /// OOP Principle: Inheritance - BaseRepository'den miras alır
    /// Design Pattern: Repository Pattern
    /// </summary>
    public class DoctorRepository : BaseRepository<Doctor>
    {
        private readonly UserRepository _userRepository;

        public DoctorRepository() : base("Doctors")
        {
            _userRepository = new UserRepository();
        }

        protected override Doctor MapFromReader(IDataReader reader)
        {
            var doctor = new Doctor
            {
                Id = Convert.ToInt32(reader["Id"])
            };

            if (reader["Uzmanlik"] != DBNull.Value)
                doctor.Uzmanlik = reader["Uzmanlik"].ToString();

            if (reader["Telefon"] != DBNull.Value)
                doctor.Telefon = reader["Telefon"].ToString();

            if (reader["Email"] != DBNull.Value)
                doctor.Email = reader["Email"].ToString();

            return doctor;
        }

        protected override Dictionary<string, object> MapToParameters(Doctor entity)
        {
            return new Dictionary<string, object>
            {
                { "Id", entity.Id },
                { "Uzmanlik", entity.Uzmanlik },
                { "Telefon", entity.Telefon },
                { "Email", entity.Email }
            };
        }

        protected override string GetInsertSql()
        {
            return "INSERT INTO Doctors (Id, Uzmanlik, Telefon, Email) VALUES (@Id, @Uzmanlik, @Telefon, @Email)";
        }

        protected override string GetUpdateSql()
        {
            return "UPDATE Doctors SET Uzmanlik = @Uzmanlik, Telefon = @Telefon, Email = @Email WHERE Id = @Id";
        }

        /// <summary>
        /// Doktor bilgilerini User bilgileriyle birlikte getirir
        /// </summary>
        public Doctor GetFullDoctorById(int id)
        {
            var doctor = GetById(id);
            if (doctor == null) return null;

            var user = _userRepository.GetById(id);
            if (user != null)
            {
                doctor.AdSoyad = user.AdSoyad;
                doctor.KullaniciAdi = user.KullaniciAdi;
                doctor.ParolaHash = user.ParolaHash;
                doctor.Role = user.Role;
                doctor.KayitTarihi = user.KayitTarihi;
                doctor.AktifMi = user.AktifMi;
            }

            return doctor;
        }

        /// <summary>
        /// Yeni doktor oluşturur (User + Doctor kayıtları)
        /// </summary>
        public int CreateDoctor(Doctor doctor)
        {
            using (var connection = CreateConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Önce Users tablosuna ekle
                        using (var userCmd = connection.CreateCommand())
                        {
                            userCmd.Transaction = transaction;
                            userCmd.CommandText = @"
                                INSERT INTO Users (AdSoyad, KullaniciAdi, ParolaHash, Role, KayitTarihi, AktifMi)
                                VALUES (@adSoyad, @kullaniciAdi, @parolaHash, @role, @kayitTarihi, @aktifMi);
                                SELECT LAST_INSERT_ID();";
                            
                            AddParameter(userCmd, "@adSoyad", doctor.AdSoyad);
                            AddParameter(userCmd, "@kullaniciAdi", doctor.KullaniciAdi);
                            AddParameter(userCmd, "@parolaHash", doctor.ParolaHash);
                            AddParameter(userCmd, "@role", (int)UserRole.Doctor);
                            AddParameter(userCmd, "@kayitTarihi", DateTime.Now);
                            AddParameter(userCmd, "@aktifMi", 1);

                            int doctorId = Convert.ToInt32(userCmd.ExecuteScalar());

                            // Sonra Doctors tablosuna ekle
                            using (var doctorCmd = connection.CreateCommand())
                            {
                                doctorCmd.Transaction = transaction;
                                doctorCmd.CommandText = @"
                                    INSERT INTO Doctors (Id, Uzmanlik, Telefon, Email)
                                    VALUES (@id, @uzmanlik, @telefon, @email)";
                                
                                AddParameter(doctorCmd, "@id", doctorId);
                                AddParameter(doctorCmd, "@uzmanlik", doctor.Uzmanlik ?? "");
                                AddParameter(doctorCmd, "@telefon", doctor.Telefon ?? "");
                                AddParameter(doctorCmd, "@email", doctor.Email ?? "");
                                doctorCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return doctorId;
                        }
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
        /// Tüm doktorları User bilgileriyle birlikte getirir
        /// </summary>
        public IEnumerable<Doctor> GetAllWithUserInfo()
        {
            var doctors = new List<Doctor>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT d.*, u.AdSoyad, u.KullaniciAdi, u.ParolaHash, u.Role, u.KayitTarihi, u.AktifMi
                        FROM Doctors d
                        INNER JOIN Users u ON d.Id = u.Id
                        WHERE u.AktifMi = 1";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var doctor = MapFromReader(reader);
                            doctor.AdSoyad = reader["AdSoyad"].ToString();
                            doctor.KullaniciAdi = reader["KullaniciAdi"].ToString();
                            doctor.ParolaHash = reader["ParolaHash"].ToString();
                            doctor.Role = (UserRole)Convert.ToInt32(reader["Role"]);
                            doctor.KayitTarihi = Convert.ToDateTime(reader["KayitTarihi"]);
                            doctor.AktifMi = Convert.ToInt32(reader["AktifMi"]) == 1;
                            doctors.Add(doctor);
                        }
                    }
                }
            }
            return doctors;
        }
    }
}

