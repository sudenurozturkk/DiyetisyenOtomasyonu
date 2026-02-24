using System;

namespace DiyetisyenOtomasyonu.Infrastructure.Exceptions
{
    /// <summary>
    /// Veritabanı hatası exception
    /// </summary>
    public class DatabaseException : AppException
    {
        public string SqlQuery { get; }

        public DatabaseException(string message) : base("DATABASE_ERROR", message)
        {
        }

        public DatabaseException(string message, Exception innerException) : base("DATABASE_ERROR", message, innerException)
        {
        }

        public DatabaseException(string message, string sqlQuery, Exception innerException) 
            : base("DATABASE_ERROR", message, innerException)
        {
            SqlQuery = sqlQuery;
        }
    }
}
