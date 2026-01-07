using System;
using System.Collections.Generic;
using System.Data;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    /// <summary>
    /// Weight Entry Repository - Kilo takip veri erişim katmanı
    /// 
    /// OOP Principle: Inheritance - BaseRepository'den miras alır
    /// Design Pattern: Repository Pattern
    /// </summary>
    public class WeightEntryRepository : BaseRepository<WeightEntry>
    {
        public WeightEntryRepository() : base("WeightEntries") { }

        protected override WeightEntry MapFromReader(IDataReader reader)
        {
            var entry = new WeightEntry
            {
                Id = Convert.ToInt32(reader["Id"]),
                PatientId = Convert.ToInt32(reader["PatientId"]),
                Date = DateTime.Parse(reader["Date"].ToString()),
                Weight = Convert.ToDouble(reader["Weight"])
            };

            if (reader["Notes"] != DBNull.Value)
                entry.Notes = reader["Notes"].ToString();

            return entry;
        }

        protected override Dictionary<string, object> MapToParameters(WeightEntry entity)
        {
            return new Dictionary<string, object>
            {
                { "Id", entity.Id },
                { "PatientId", entity.PatientId },
                { "Date", entity.Date.ToString("o") },
                { "Weight", entity.Weight },
                { "Notes", entity.Notes }
            };
        }

        protected override string GetInsertSql()
        {
            return @"INSERT INTO WeightEntries (PatientId, Date, Weight, Notes)
                     VALUES (@PatientId, @Date, @Weight, @Notes)";
        }

        protected override string GetUpdateSql()
        {
            return @"UPDATE WeightEntries SET Weight = @Weight, Notes = @Notes WHERE Id = @Id";
        }

        /// <summary>
        /// Hastanın kilo geçmişini getirir
        /// </summary>
        public IEnumerable<WeightEntry> GetByPatientId(int patientId, int? days = null)
        {
            var entries = new List<WeightEntry>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    var sql = "SELECT * FROM WeightEntries WHERE PatientId = @patientId";
                    if (days.HasValue)
                    {
                        sql += " AND Date >= @startDate";
                    }
                    sql += " ORDER BY Date DESC";

                    cmd.CommandText = sql;
                    AddParameter(cmd, "@patientId", patientId);
                    if (days.HasValue)
                    {
                        AddParameter(cmd, "@startDate", DateTime.Now.AddDays(-days.Value).ToString("o"));
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entries.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            return entries;
        }

        /// <summary>
        /// Hastanın son kilo kaydını getirir
        /// </summary>
        public WeightEntry GetLastEntry(int patientId)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM WeightEntries 
                        WHERE PatientId = @patientId 
                        ORDER BY Date DESC LIMIT 1";
                    AddParameter(cmd, "@patientId", patientId);

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
        /// Belirli tarih aralığındaki kilo kayıtlarını getirir
        /// </summary>
        public IEnumerable<WeightEntry> GetByDateRange(int patientId, DateTime startDate, DateTime endDate)
        {
            var entries = new List<WeightEntry>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM WeightEntries 
                        WHERE PatientId = @patientId 
                          AND Date >= @startDate AND Date <= @endDate
                        ORDER BY Date";
                    AddParameter(cmd, "@patientId", patientId);
                    AddParameter(cmd, "@startDate", startDate.ToString("o"));
                    AddParameter(cmd, "@endDate", endDate.ToString("o"));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entries.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            return entries;
        }
    }
}

