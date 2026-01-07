using DiyetisyenOtomasyonu.Domain;

namespace DiyetisyenOtomasyonu.Infrastructure.Security
{
    /// <summary>
    /// Oturum bilgilerini tutar (Aktif kullanici ve rol)
    /// </summary>
    public static class AuthContext
    {
        public static int UserId { get; private set; }
        public static string UserName { get; private set; }
        public static UserRole Role { get; private set; }
        public static bool IsAuthenticated { get; private set; }

        /// <summary>
        /// Kullanici girisi
        /// </summary>
        public static void SignIn(int userId, string userName, UserRole role)
        {
            UserId = userId;
            UserName = userName;
            Role = role;
            IsAuthenticated = true;
        }

        /// <summary>
        /// Kullanici cikisi
        /// </summary>
        public static void SignOut()
        {
            UserId = 0;
            UserName = string.Empty;
            Role = default;
            IsAuthenticated = false;
        }

        /// <summary>
        /// Doktor kontrolu
        /// </summary>
        public static bool IsDoctor()
        {
            return IsAuthenticated && Role == UserRole.Doctor;
        }

        /// <summary>
        /// Hasta kontrolu
        /// </summary>
        public static bool IsPatient()
        {
            return IsAuthenticated && Role == UserRole.Patient;
        }
    }
}
