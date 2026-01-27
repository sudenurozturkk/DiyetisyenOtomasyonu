using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace DiyetisyenOtomasyonu.Infrastructure.Database
{
    /// <summary>
    /// Veritabanı bağlantı ayarları - Singleton Pattern
    /// </summary>
    public class DatabaseConfig
    {
        private static DatabaseConfig _instance;
        private static readonly object _lock = new object();
        
        // MySQL bağlantı dizesi - utf8mb4 Türkçe karakterler için gerekli
        private readonly string _connectionString = "Server=localhost;Database=dietpro_db;Uid=root;Pwd=;CharSet=utf8mb4;SslMode=Preferred;";

        private DatabaseConfig()
        {
        }

        public static DatabaseConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DatabaseConfig();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Yeni bir veritabanı bağlantısı oluşturur
        /// </summary>
        public IDbConnection CreateConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return connection;
        }

        /// <summary>
        /// Bağlantı dizesini döndürür
        /// </summary>
        public string GetConnectionString()
        {
            return _connectionString;
        }
    }
}

