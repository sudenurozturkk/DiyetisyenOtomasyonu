using System;
using System.IO;
using System.Diagnostics;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// Profesyonel Logging Servisi
    /// Structured logging, dosya tabanlı loglama, log seviyeleri
    /// </summary>
    public class LoggerService
    {
        private static readonly object _lockObject = new object();
        private static string _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static string _logFile = Path.Combine(_logDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");

        static LoggerService()
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// Debug seviyesinde log
        /// </summary>
        public static void Debug(string message, string context = "")
        {
            WriteLog(LogLevel.Debug, message, context);
        }

        /// <summary>
        /// Info seviyesinde log
        /// </summary>
        public static void Info(string message, string context = "")
        {
            WriteLog(LogLevel.Info, message, context);
        }

        /// <summary>
        /// Warning seviyesinde log
        /// </summary>
        public static void Warning(string message, string context = "")
        {
            WriteLog(LogLevel.Warning, message, context);
        }

        /// <summary>
        /// Error seviyesinde log
        /// </summary>
        public static void Error(string message, Exception exception = null, string context = "")
        {
            var fullMessage = message;
            if (exception != null)
            {
                fullMessage += $"\nException: {exception.Message}\nStackTrace: {exception.StackTrace}";
            }
            WriteLog(LogLevel.Error, fullMessage, context);
        }

        /// <summary>
        /// Critical seviyesinde log
        /// </summary>
        public static void Critical(string message, Exception exception = null, string context = "")
        {
            var fullMessage = message;
            if (exception != null)
            {
                fullMessage += $"\nException: {exception.Message}\nStackTrace: {exception.StackTrace}";
            }
            WriteLog(LogLevel.Critical, fullMessage, context);
        }

        /// <summary>
        /// Log yazma metodu
        /// </summary>
        private static void WriteLog(LogLevel level, string message, string context)
        {
            lock (_lockObject)
            {
                try
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logEntry = $"[{timestamp}] [{level}] [{context}] {message}";

                    // Console'a yaz (Debug modunda)
#if DEBUG
                    Debug.WriteLine(logEntry);
#endif

                    // Dosyaya yaz
                    File.AppendAllText(_logFile, logEntry + Environment.NewLine);

                    // Error ve Critical seviyelerde Event Log'a da yaz
                    if (level == LogLevel.Error || level == LogLevel.Critical)
                    {
                        WriteToEventLog(level, message, context);
                    }
                }
                catch (Exception ex)
                {
                    // Loglama hatası - sadece console'a yaz
                    Debug.WriteLine($"Logging error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Windows Event Log'a yaz
        /// </summary>
        private static void WriteToEventLog(LogLevel level, string message, string context)
        {
            try
            {
                var source = "DiyetPro";
                var logName = "Application";
                var eventLogEntryType = level == LogLevel.Critical ? EventLogEntryType.Error : EventLogEntryType.Warning;

                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, logName);
                }

                EventLog.WriteEntry(source, $"[{context}] {message}", eventLogEntryType);
            }
            catch
            {
                // Event Log yazma hatası - sessizce devam et
            }
        }

        /// <summary>
        /// Eski log dosyalarını temizle (30 günden eski)
        /// </summary>
        public static void CleanOldLogs(int daysToKeep = 30)
        {
            try
            {
                var files = Directory.GetFiles(_logDirectory, "app_*.log");
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Error("Error cleaning old logs", ex, "LoggerService");
            }
        }
    }

    /// <summary>
    /// Log seviyeleri
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }
}
