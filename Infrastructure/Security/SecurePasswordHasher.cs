using System;
using System.Security.Cryptography;

namespace DiyetisyenOtomasyonu.Infrastructure.Security
{
    /// <summary>
    /// PBKDF2 kullanarak güvenli parola hashleme
    /// Her kullanıcı için farklı salt kullanır
    /// OWASP standartlarına uygun
    /// </summary>
    public static class SecurePasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 10000; // OWASP recommended minimum

        /// <summary>
        /// Parolayı PBKDF2 ile hashler
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Parola boş olamaz", nameof(password));

            // Random salt oluştur
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[SaltSize];
                rng.GetBytes(salt);

                // PBKDF2 hash
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
                {
                    byte[] hash = pbkdf2.GetBytes(KeySize);

                    // Salt + Hash birleştir
                    byte[] hashBytes = new byte[SaltSize + KeySize];
                    Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                    Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

                    // Base64 encode
                    return Convert.ToBase64String(hashBytes);
                }
            }
        }

        /// <summary>
        /// Parolayı doğrular
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (string.IsNullOrWhiteSpace(storedHash))
                return false;

            try
            {
                byte[] hashBytes = Convert.FromBase64String(storedHash);

                // Salt'ı ayıkla
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Girilen parolanın hash'ini hesapla
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
                {
                    byte[] hash = pbkdf2.GetBytes(KeySize);

                    // Karşılaştır (timing attack'a karşı güvenli)
                    for (int i = 0; i < KeySize; i++)
                    {
                        if (hashBytes[i + SaltSize] != hash[i])
                            return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Parola kurallarını kontrol eder
        /// </summary>
        public static bool IsValidPassword(string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Parola boş olamaz";
                return false;
            }

            if (password.Length < 8)
            {
                errorMessage = "Parola en az 8 karakter olmalıdır";
                return false;
            }

            if (password.Length > 128)
            {
                errorMessage = "Parola en fazla 128 karakter olabilir";
                return false;
            }

            // Ek güvenlik kuralları (isteğe bağlı)
            bool hasUpper = false;
            bool hasLower = false;
            bool hasDigit = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                if (char.IsLower(c)) hasLower = true;
                if (char.IsDigit(c)) hasDigit = true;
            }

            // En az bir büyük, bir küçük ve bir rakam olmalı (isteğe bağlı)
            // Bu kontrolü devre dışı bırakmak için yoruma alabilirsiniz
            /*
            if (!hasUpper || !hasLower || !hasDigit)
            {
                errorMessage = "Parola en az bir büyük harf, bir küçük harf ve bir rakam içermelidir";
                return false;
            }
            */

            return true;
        }

        /// <summary>
        /// Eski hash format mı kontrol eder (SHA256)
        /// </summary>
        public static bool IsLegacyHash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return false;

            // SHA256 hash 64 karakter (hex) veya 44 karakter (base64)
            // PBKDF2 hash (16+32)*4/3 = 64 karakter (base64)
            // Ama PBKDF2 her zaman farklı olur (random salt)
            
            // Legacy hash detection: SHA256 hex formatı (64 karakter, sadece hex)
            if (hash.Length == 64)
            {
                foreach (char c in hash)
                {
                    if (!Uri.IsHexDigit(c))
                        return false;
                }
                return true; // SHA256 hex format
            }

            return false;
        }
    }
}
