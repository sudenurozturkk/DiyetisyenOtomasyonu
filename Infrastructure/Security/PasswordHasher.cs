using System;
using System.Security.Cryptography;
using System.Text;

namespace DiyetisyenOtomasyonu.Infrastructure.Security
{
    /// <summary>
    /// Parola hash ve dogrulama islemleri
    /// SHA256 + Salt kullanir
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Parolayi hash'ler
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Parola bos olamaz");

            // Basit salt (gercek uygulamada her kullanici icin farkli olmali)
            string salt = "DiyetisyenOto2024!";
            string saltedPassword = password + salt;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Parolayi dogrular
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            string hashOfInput = HashPassword(password);
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfInput, hash) == 0;
        }

        /// <summary>
        /// Parola kurallarini kontrol eder
        /// </summary>
        public static bool IsValidPassword(string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Parola bos olamaz";
                return false;
            }

            if (password.Length < 8)
            {
                errorMessage = "Parola en az 8 karakter olmalidir";
                return false;
            }

            return true;
        }
    }
}
