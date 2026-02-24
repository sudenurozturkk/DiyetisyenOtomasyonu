using System;

namespace DiyetisyenOtomasyonu.Infrastructure.Exceptions
{
    /// <summary>
    /// Uygulama temel exception sınıfı
    /// </summary>
    public class AppException : Exception
    {
        public string ErrorCode { get; }
        public object[] Parameters { get; }

        public AppException(string message) : base(message)
        {
            ErrorCode = "APP_ERROR";
        }

        public AppException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "APP_ERROR";
        }

        public AppException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public AppException(string errorCode, string message, params object[] parameters) : base(message)
        {
            ErrorCode = errorCode;
            Parameters = parameters;
        }
    }
}
