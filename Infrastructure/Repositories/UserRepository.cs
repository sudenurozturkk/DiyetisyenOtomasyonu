using System;
using System.Collections.Generic;
using System.Data;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>
    {
        public UserRepository() : base("Users")
        {
        }

        protected override User MapFromReader(IDataReader reader)
        {
            var user = new User
            {
                Id = Convert.ToInt32(reader["Id"]),
                AdSoyad = reader["AdSoyad"].ToString(),
                KullaniciAdi = reader["KullaniciAdi"].ToString(),
                ParolaHash = reader["ParolaHash"].ToString(),
                Role = (UserRole)Convert.ToInt32(reader["Role"]),
                KayitTarihi = DateTime.Parse(reader["KayitTarihi"].ToString()),
                AktifMi = Convert.ToInt32(reader["AktifMi"]) == 1
            };

            if (reader["ProfilePhoto"] != DBNull.Value)
            {
                user.ProfilePhoto = reader["ProfilePhoto"].ToString();
            }

            return user;
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
                { "KayitTarihi", entity.KayitTarihi },
                { "AktifMi", entity.AktifMi ? 1 : 0 },
                { "ProfilePhoto", entity.ProfilePhoto }
            };
        }

        protected override string GetInsertSql()
        {
            return @"INSERT INTO Users (AdSoyad, KullaniciAdi, ParolaHash, Role, KayitTarihi, AktifMi, ProfilePhoto)
                     VALUES (@AdSoyad, @KullaniciAdi, @ParolaHash, @Role, @KayitTarihi, @AktifMi, @ProfilePhoto)";
        }

        protected override string GetUpdateSql()
        {
            return @"UPDATE Users SET AdSoyad = @AdSoyad, KullaniciAdi = @KullaniciAdi, 
                     ParolaHash = @ParolaHash, Role = @Role, AktifMi = @AktifMi, ProfilePhoto = @ProfilePhoto
                     WHERE Id = @Id";
        }

        public User GetByUsername(string username)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Users WHERE KullaniciAdi = @username AND AktifMi = 1";
                    AddParameter(cmd, "@username", username);

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

        public bool UsernameExists(string username)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE KullaniciAdi = @username";
                    AddParameter(cmd, "@username", username);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }
    }
}
