using System;
using System.Collections.Generic;
using System.Data;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    /// <summary>
    /// Body Measurement Repository - Vücut ölçüm verileri
    /// </summary>
    public class BodyMeasurementRepository : BaseRepository<BodyMeasurement>
    {
        public BodyMeasurementRepository() : base("BodyMeasurements") { }

        protected override BodyMeasurement MapFromReader(IDataReader reader)
        {
            var m = new BodyMeasurement
            {
                Id = Convert.ToInt32(reader["Id"]),
                PatientId = Convert.ToInt32(reader["PatientId"]),
                Date = DateTime.Parse(reader["Date"].ToString())
            };

            if (reader["Chest"] != DBNull.Value) m.Chest = Convert.ToDouble(reader["Chest"]);
            if (reader["Waist"] != DBNull.Value) m.Waist = Convert.ToDouble(reader["Waist"]);
            if (reader["Hip"] != DBNull.Value) m.Hip = Convert.ToDouble(reader["Hip"]);
            if (reader["Arm"] != DBNull.Value) m.Arm = Convert.ToDouble(reader["Arm"]);
            if (reader["Thigh"] != DBNull.Value) m.Thigh = Convert.ToDouble(reader["Thigh"]);
            if (reader["Neck"] != DBNull.Value) m.Neck = Convert.ToDouble(reader["Neck"]);
            if (reader["Notes"] != DBNull.Value) m.Notes = reader["Notes"].ToString();

            return m;
        }

        protected override Dictionary<string, object> MapToParameters(BodyMeasurement entity)
        {
            return new Dictionary<string, object>
            {
                { "Id", entity.Id },
                { "PatientId", entity.PatientId },
                { "Date", entity.Date.ToString("yyyy-MM-dd HH:mm:ss") },
                { "Chest", entity.Chest },
                { "Waist", entity.Waist },
                { "Hip", entity.Hip },
                { "Arm", entity.Arm },
                { "Thigh", entity.Thigh },
                { "Neck", entity.Neck },
                { "Notes", entity.Notes }
            };
        }

        protected override string GetInsertSql()
        {
            return @"INSERT INTO BodyMeasurements 
                    (PatientId, Date, Chest, Waist, Hip, Arm, Thigh, Neck, Notes)
                    VALUES (@PatientId, @Date, @Chest, @Waist, @Hip, @Arm, @Thigh, @Neck, @Notes)";
        }

        protected override string GetUpdateSql()
        {
            return @"UPDATE BodyMeasurements SET 
                    Date = @Date, Chest = @Chest, Waist = @Waist, Hip = @Hip,
                    Arm = @Arm, Thigh = @Thigh, Neck = @Neck, Notes = @Notes
                    WHERE Id = @Id";
        }

        public List<BodyMeasurement> GetByPatient(int patientId)
        {
            var result = new List<BodyMeasurement>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM BodyMeasurements WHERE PatientId = @PatientId ORDER BY Date DESC";
                    AddParameter(cmd, "@PatientId", patientId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            return result;
        }

        public BodyMeasurement GetLatest(int patientId)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM BodyMeasurements WHERE PatientId = @PatientId ORDER BY Date DESC LIMIT 1";
                    AddParameter(cmd, "@PatientId", patientId);

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
    }
}
