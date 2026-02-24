using System;
using System.Collections.Generic;
using System.Data;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    /// <summary>
    /// Badge Repository - Rozet veri erişimi
    /// </summary>
    public class BadgeRepository : BaseRepository<Badge>
    {
        public BadgeRepository() : base("Badges") { }

        protected override Badge MapFromReader(IDataReader reader)
        {
            return new Badge
            {
                Id = Convert.ToInt32(reader["Id"]),
                PatientId = Convert.ToInt32(reader["PatientId"]),
                Type = (BadgeType)Convert.ToInt32(reader["Type"]),
                Name = reader["Name"].ToString(),
                Description = reader["Description"]?.ToString(),
                Icon = reader["Icon"]?.ToString(),
                EarnedDate = reader["EarnedDate"] != DBNull.Value ? DateTime.Parse(reader["EarnedDate"].ToString()) : DateTime.Now,
                Progress = reader["Progress"] != DBNull.Value ? Convert.ToInt32(reader["Progress"]) : 0,
                IsEarned = reader["IsEarned"] != DBNull.Value && Convert.ToBoolean(reader["IsEarned"])
            };
        }

        /// <summary>
        /// Hasta rozetlerini getir
        /// </summary>
        public List<Badge> GetByPatientId(int patientId)
        {
            var badges = new List<Badge>();
            using (var connection = DatabaseConfig.Instance.CreateConnection())
            {
                var query = "SELECT * FROM Badges WHERE PatientId = @PatientId ORDER BY EarnedDate DESC";
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = query;
                    var param = cmd.CreateParameter();
                    param.ParameterName = "@PatientId";
                    param.Value = patientId;
                    cmd.Parameters.Add(param);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            badges.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            return badges;
        }

        protected override Dictionary<string, object> MapToParameters(Badge entity)
        {
            return new Dictionary<string, object>
            {
                { "PatientId", entity.PatientId },
                { "Type", (int)entity.Type },
                { "Name", entity.Name },
                { "Description", entity.Description ?? (object)DBNull.Value },
                { "Icon", entity.Icon ?? (object)DBNull.Value },
                { "EarnedDate", entity.EarnedDate },
                { "Progress", entity.Progress },
                { "IsEarned", entity.IsEarned }
            };
        }

        protected override string GetInsertSql()
        {
            return @"INSERT INTO Badges (PatientId, Type, Name, Description, Icon, EarnedDate, Progress, IsEarned) 
                     VALUES (@PatientId, @Type, @Name, @Description, @Icon, @EarnedDate, @Progress, @IsEarned)";
        }

        protected override string GetUpdateSql()
        {
            return @"UPDATE Badges SET PatientId = @PatientId, Type = @Type, Name = @Name, Description = @Description, 
                     Icon = @Icon, EarnedDate = @EarnedDate, Progress = @Progress, IsEarned = @IsEarned WHERE Id = @Id";
        }
    }
}
