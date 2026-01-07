using System;
using System.Collections.Generic;
using System.Data;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Database;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    /// <summary>
    /// Appointment Repository - Randevu yönetimi
    /// </summary>
    public class AppointmentRepository : BaseRepository<Appointment>
    {
        public AppointmentRepository() : base("Appointments") { }

        protected override Appointment MapFromReader(IDataReader reader)
        {
            var apt = new Appointment
            {
                Id = Convert.ToInt32(reader["Id"]),
                PatientId = Convert.ToInt32(reader["PatientId"]),
                DoctorId = Convert.ToInt32(reader["DoctorId"]),
                DateTime = DateTime.Parse(reader["DateTime"].ToString()),
                Type = (AppointmentType)Convert.ToInt32(reader["Type"]),
                Status = (AppointmentStatus)Convert.ToInt32(reader["Status"])
            };

            if (reader["Notes"] != DBNull.Value) apt.Notes = reader["Notes"].ToString();
            if (reader["CreatedAt"] != DBNull.Value) apt.CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString());
            
            // Price field - check if column exists
            try { if (reader["Price"] != DBNull.Value) apt.Price = Convert.ToDecimal(reader["Price"]); } catch { }

            return apt;
        }

        protected override Dictionary<string, object> MapToParameters(Appointment entity)
        {
            return new Dictionary<string, object>
            {
                { "Id", entity.Id },
                { "PatientId", entity.PatientId },
                { "DoctorId", entity.DoctorId },
                { "DateTime", entity.DateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                { "Type", (int)entity.Type },
                { "Status", (int)entity.Status },
                { "Notes", entity.Notes },
                { "Price", entity.Price }
            };
        }

        protected override string GetInsertSql()
        {
            return @"INSERT INTO Appointments 
                    (PatientId, DoctorId, DateTime, Type, Status, Notes, Price)
                    VALUES (@PatientId, @DoctorId, @DateTime, @Type, @Status, @Notes, @Price)";
        }

        protected override string GetUpdateSql()
        {
            return @"UPDATE Appointments SET 
                    DateTime = @DateTime, Type = @Type, Status = @Status, Notes = @Notes, Price = @Price
                    WHERE Id = @Id";
        }

        public void DeleteAppointment(int id)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Appointments WHERE Id = @Id";
                    AddParameter(cmd, "@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public decimal GetTotalRevenue(int doctorId, DateTime? startDate = null, DateTime? endDate = null)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    var sql = "SELECT COALESCE(SUM(Price), 0) FROM Appointments WHERE DoctorId = @DoctorId AND Status = 2";
                    if (startDate.HasValue) sql += " AND DateTime >= @StartDate";
                    if (endDate.HasValue) sql += " AND DateTime <= @EndDate";
                    
                    cmd.CommandText = sql;
                    AddParameter(cmd, "@DoctorId", doctorId);
                    if (startDate.HasValue) AddParameter(cmd, "@StartDate", startDate.Value.ToString("yyyy-MM-dd"));
                    if (endDate.HasValue) AddParameter(cmd, "@EndDate", endDate.Value.ToString("yyyy-MM-dd 23:59:59"));
                    
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
        }

        public List<Appointment> GetTodayAppointments(int doctorId)
        {
            var result = new List<Appointment>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"SELECT * FROM Appointments 
                                       WHERE DoctorId = @DoctorId 
                                       AND DATE(DateTime) = CURDATE()
                                       AND Status IN (0, 1)
                                       ORDER BY DateTime ASC";
                    AddParameter(cmd, "@DoctorId", doctorId);

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

        public List<Appointment> GetByDoctor(int doctorId)
        {
            var result = new List<Appointment>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"SELECT a.*, u.AdSoyad as PatientName 
                                        FROM Appointments a 
                                        LEFT JOIN Patients p ON a.PatientId = p.Id 
                                        LEFT JOIN Users u ON p.Id = u.Id
                                        WHERE a.DoctorId = @DoctorId 
                                        ORDER BY a.DateTime DESC";
                    AddParameter(cmd, "@DoctorId", doctorId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var apt = MapFromReader(reader);
                            // PatientName'i oku
                            try 
                            { 
                                if (reader["PatientName"] != DBNull.Value) 
                                    apt.PatientName = reader["PatientName"].ToString(); 
                            } 
                            catch { }
                            result.Add(apt);
                        }
                    }
                }
            }
            return result;
        }

        public List<Appointment> GetByPatient(int patientId)
        {
            var result = new List<Appointment>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Appointments WHERE PatientId = @PatientId ORDER BY DateTime DESC";
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

        public List<Appointment> GetUpcoming(int doctorId, int limit = 10)
        {
            var result = new List<Appointment>();
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"SELECT * FROM Appointments 
                                       WHERE DoctorId = @DoctorId 
                                       AND DateTime >= NOW() AND Status = 1
                                       ORDER BY DateTime ASC LIMIT @Limit";
                    AddParameter(cmd, "@DoctorId", doctorId);
                    AddParameter(cmd, "@Limit", limit);

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

        public void UpdateStatus(int id, AppointmentStatus status)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Appointments SET Status = @Status WHERE Id = @Id";
                    AddParameter(cmd, "@Id", id);
                    AddParameter(cmd, "@Status", (int)status);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
