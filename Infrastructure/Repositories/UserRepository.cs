using System;
using System.Collections.Generic;
using System.Data;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>
    {
        public UserRepository() : base("Users") { }

        protected override User MapFromReader(IDataReader reader)
        {
            return new User
            {
                Id = Convert.ToInt32(reader["Id"]),
                AdSoyad = reader["AdSoyad"].ToString(),
                KullaniciAdi = reader["KullaniciAdi"].ToString(),
                ParolaHash = reader["ParolaHash"].ToString(),
                Role = (UserRole)Convert.ToInt32(reader["Role"]),
                KayitTarihi = Convert.ToDateTime(reader["KayitTarihi"]),
                AktifMi = Convert.ToInt32(reader["AktifMi"]) == 1
            };
        }

        protected override Dictionary<string, object> MapToParameters(User entity)
        {
            return new Dictionary<string, object>
            {
                { "Id", entity.Id },
                { "AdSoyad", entity.AdSoyad },
                { "KullaniciAdi", entity.KullaniciAdi },
                { "ParolaHash", entity.ParolaHash },
                { "Role", (int)entity.Role },
                { "KayitTarihi", entity.KayitTarihi.ToString("o") },
                { "AktifMi", entity.AktifMi ? 1 : 0 }
            };
        }

        protected override string GetInsertSql()
        {
            return @"INSERT INTO Users (AdSoyad, KullaniciAdi, ParolaHash, Role, KayitTarihi, AktifMi)
                     VALUES (@AdSoyad, @KullaniciAdi, @ParolaHash, @Role, @KayitTarihi, @AktifMi)";
        }

        protected override string GetUpdateSql()
        {
            return @"UPDATE Users SET AdSoyad = @AdSoyad, KullaniciAdi = @KullaniciAdi, 
                     ParolaHash = @ParolaHash, AktifMi = @AktifMi WHERE Id = @Id";
        }

        public User GetByUsername(string username)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Users WHERE KullaniciAdi = @Username";
                    AddParameter(cmd, "@Username", username);
                    
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
        /// Kullanýcý adýnýn mevcut olup olmadýðýný kontrol eder
        /// </summary>
        public bool UsernameExists(string username)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE KullaniciAdi = @Username";
                    AddParameter(cmd, "@Username", username);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }
    }
}

