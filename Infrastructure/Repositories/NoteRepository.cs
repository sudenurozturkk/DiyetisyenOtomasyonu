using System;
using System.Collections.Generic;
using System.Data;
using DiyetisyenOtomasyonu.Domain;

namespace DiyetisyenOtomasyonu.Infrastructure.Repositories
{
    /// <summary>
    /// Not repository - Notlar için veritabanı işlemleri
    /// 
    /// OOP Principle: Single Responsibility - Not verisi işlemlerinden sorumlu
    /// Design Pattern: Repository Pattern
    /// </summary>
    public class NoteRepository : BaseRepository<Note>
    {
        public NoteRepository() : base("Notes") { }

        public IEnumerable<Note> GetByPatientId(int patientId)
        {
            var sql = @"SELECT n.*, u.AdSoyad as DoctorNameFromUser 
                        FROM Notes n 
                        LEFT JOIN Users u ON n.DoctorId = u.Id
                        WHERE n.PatientId = @patientId 
                        ORDER BY n.Date DESC";
            
            return ExecuteQuery(sql, new Dictionary<string, object> { { "patientId", patientId } });
        }

        protected override Note MapFromReader(IDataReader reader)
        {
            var note = new Note
            {
                Id = Convert.ToInt32(reader["Id"]),
                PatientId = Convert.ToInt32(reader["PatientId"]),
                DoctorId = Convert.ToInt32(reader["DoctorId"]),
                Content = reader["Content"]?.ToString() ?? "",
                Date = DateTime.Parse(reader["Date"].ToString())
            };

            if (reader["DoctorName"] != DBNull.Value)
                note.DoctorName = reader["DoctorName"].ToString();

            // Fallback - get doctor name from join if available
            try
            {
                if (reader["DoctorNameFromUser"] != DBNull.Value && string.IsNullOrEmpty(note.DoctorName))
                    note.DoctorName = reader["DoctorNameFromUser"].ToString();
            }
            catch { }

            try
            {
                if (reader["Category"] != DBNull.Value)
                {
                    var categoryValue = reader["Category"];
                    if (categoryValue is int)
                        note.Category = (NoteCategory)(int)categoryValue;
                    else if (categoryValue is long)
                        note.Category = (NoteCategory)(int)(long)categoryValue;
                    else
                    {
                        int catInt;
                        if (int.TryParse(categoryValue.ToString(), out catInt))
                            note.Category = (NoteCategory)catInt;
                    }
                }
            }
            catch { note.Category = NoteCategory.General; }

            return note;
        }

        protected override Dictionary<string, object> MapToParameters(Note entity)
        {
            return new Dictionary<string, object>
            {
                { "Id", entity.Id },
                { "PatientId", entity.PatientId },
                { "DoctorId", entity.DoctorId },
                { "DoctorName", entity.DoctorName ?? "" },
                { "Content", entity.Content ?? "" },
                { "Date", entity.Date.ToString("yyyy-MM-dd HH:mm:ss") },
                { "Category", (int)entity.Category }
            };
        }

        protected override string GetInsertSql()
        {
            return @"INSERT INTO Notes (PatientId, DoctorId, DoctorName, Content, Date, Category)
                     VALUES (@PatientId, @DoctorId, @DoctorName, @Content, @Date, @Category)";
        }

        protected override string GetUpdateSql()
        {
            return @"UPDATE Notes 
                     SET PatientId = @PatientId, DoctorId = @DoctorId, DoctorName = @DoctorName,
                         Content = @Content, Date = @Date, Category = @Category 
                     WHERE Id = @Id";
        }
    }
}
