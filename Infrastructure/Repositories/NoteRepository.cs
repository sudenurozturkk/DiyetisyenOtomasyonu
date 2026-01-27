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
            // DoctorName sutunu olmayabilir, sadece Users tablosundan al
            var sql = @"SELECT n.Id, n.PatientId, n.DoctorId, n.Content, n.Date, n.Category,
                               u.AdSoyad as DoctorNameFromUser 
                        FROM Notes n 
                        LEFT JOIN Users u ON n.DoctorId = u.Id
                        WHERE n.PatientId = @patientId 
                        ORDER BY n.Id DESC";
            
            return ExecuteQuery(sql, new Dictionary<string, object> { { "patientId", patientId } });
        }

        protected override Note MapFromReader(IDataReader reader)
        {
            var note = new Note
            {
                Id = Convert.ToInt32(reader["Id"]),
                PatientId = Convert.ToInt32(reader["PatientId"]),
                DoctorId = Convert.ToInt32(reader["DoctorId"]),
                Content = reader["Content"]?.ToString() ?? ""
            };
            
            // Date sütununu güvenli şekilde al
            try
            {
                if (HasColumn(reader, "Date") && reader["Date"] != DBNull.Value)
                {
                    note.Date = DateTime.Parse(reader["Date"].ToString());
                }
                else
                {
                    note.Date = DateTime.Now; // Varsayılan olarak şimdiki zaman
                }
            }
            catch
            {
                note.Date = DateTime.Now;
            }

            // Doktor adini Users tablosundan al
            try
            {
                if (HasColumn(reader, "DoctorNameFromUser") && reader["DoctorNameFromUser"] != DBNull.Value)
                    note.DoctorName = reader["DoctorNameFromUser"].ToString();
            }
            catch { }

            // Fallback - DoctorName sutunu varsa oradan al
            try
            {
                if (HasColumn(reader, "DoctorName") && reader["DoctorName"] != DBNull.Value && string.IsNullOrEmpty(note.DoctorName))
                    note.DoctorName = reader["DoctorName"].ToString();
            }
            catch { }

            try
            {
                if (HasColumn(reader, "Category") && reader["Category"] != DBNull.Value)
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
        
        private bool HasColumn(IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        protected override Dictionary<string, object> MapToParameters(Note entity)
        {
            return new Dictionary<string, object>
            {
                { "Id", entity.Id },
                { "PatientId", entity.PatientId },
                { "DoctorId", entity.DoctorId },
                { "Content", entity.Content ?? "" },
                { "Date", entity.Date.ToString("yyyy-MM-dd HH:mm:ss") },
                { "Category", (int)entity.Category }
            };
        }

        protected override string GetInsertSql()
        {
            // DoctorName sutunu kullanmiyoruz, doktor adi Users tablosundan alinir
            return @"INSERT INTO Notes (PatientId, DoctorId, Content, Date, Category)
                     VALUES (@PatientId, @DoctorId, @Content, @Date, @Category)";
        }

        protected override string GetUpdateSql()
        {
            return @"UPDATE Notes 
                     SET PatientId = @PatientId, DoctorId = @DoctorId,
                         Content = @Content, Date = @Date, Category = @Category 
                     WHERE Id = @Id";
        }
    }
}
