using System;
using System.Configuration;

namespace DiyetisyenOtomasyonu.Infrastructure.Configuration
{
    /// <summary>
    /// Uygulama yapılandırma yönetimi
    /// App.config'den değerleri okur
    /// </summary>
    public class AppConfiguration
    {
        private static AppConfiguration _instance;
        private static readonly object _lock = new object();

        private AppConfiguration() { }

        public static AppConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new AppConfiguration();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Veritabanı bağlantı dizesi
        /// </summary>
        public string DatabaseConnectionString
        {
            get
            {
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    // Fallback: Environment variable veya default
                    connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
                        ?? "Server=localhost;Database=dietpro_db;Uid=root;Pwd=;CharSet=utf8mb4;SslMode=Preferred;";
                }
                return connectionString;
            }
        }

        /// <summary>
        /// Google Gemini API Key
        /// </summary>
        public string GeminiApiKey
        {
            get
            {
                var apiKey = ConfigurationManager.AppSettings["GeminiApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "API_KEYINIZI_YAZIN";
                }
                return apiKey;
            }
        }

        /// <summary>
        /// OpenRouter API Key
        /// </summary>
        public string OpenRouterApiKey
        {
            get
            {
                var apiKey = ConfigurationManager.AppSettings["OpenRouterApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? "API_KEYINIZI_YAZIN";
                }
                return apiKey;
            }
        }

        /// <summary>
        /// Uygulama adı
        /// </summary>
        public string ApplicationName => ConfigurationManager.AppSettings["ApplicationName"] ?? "DiyetPro";

        /// <summary>
        /// Uygulama versiyonu
        /// </summary>
        public string ApplicationVersion => ConfigurationManager.AppSettings["ApplicationVersion"] ?? "2.1.0";

        /// <summary>
        /// Log seviyesi
        /// </summary>
        public string LogLevel => ConfigurationManager.AppSettings["LogLevel"] ?? "Info";

        /// <summary>
        /// Cache süresi (dakika)
        /// </summary>
        public int CacheDurationMinutes
        {
            get
            {
                if (int.TryParse(ConfigurationManager.AppSettings["CacheDurationMinutes"], out int duration))
                {
                    return duration;
                }
                return 5; // Default 5 dakika
            }
        }

        /// <summary>
        /// Maksimum bağlantı sayısı
        /// </summary>
        public int MaxConnectionPoolSize
        {
            get
            {
                if (int.TryParse(ConfigurationManager.AppSettings["MaxConnectionPoolSize"], out int max))
                {
                    return max;
                }
                return 10; // Default 10
            }
        }

        /// <summary>
        /// API rate limit (requests per minute)
        /// </summary>
        public int ApiRateLimit
        {
            get
            {
                if (int.TryParse(ConfigurationManager.AppSettings["ApiRateLimit"], out int limit))
                {
                    return limit;
                }
                return 60; // Default 60 requests/minute
            }
        }

        /// <summary>
        /// Debug modu
        /// </summary>
        public bool IsDebugMode
        {
            get
            {
                var debug = ConfigurationManager.AppSettings["DebugMode"];
                return !string.IsNullOrEmpty(debug) && debug.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
