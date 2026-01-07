using System;

namespace DiyetisyenOtomasyonu.Shared
{
    /// <summary>
    /// Parametre validasyon kontrolleri
    /// </summary>
    public static class Guards
    {
        /// <summary>
        /// Null kontrolü
        /// </summary>
        public static void AgainstNull(object value, string parameterName)
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);
        }

        /// <summary>
        /// Boş string kontrolü
        /// </summary>
        public static void AgainstNullOrEmpty(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{parameterName} boş olamaz", parameterName);
        }

        /// <summary>
        /// Negatif sayı kontrolü
        /// </summary>
        public static void AgainstNegative(double value, string parameterName)
        {
            if (value < 0)
                throw new ArgumentException($"{parameterName} negatif olamaz", parameterName);
        }

        /// <summary>
        /// Sıfır veya negatif sayı kontrolü
        /// </summary>
        public static void AgainstZeroOrNegative(double value, string parameterName)
        {
            if (value <= 0)
                throw new ArgumentException($"{parameterName} sıfır veya negatif olamaz", parameterName);
        }
    }
}
