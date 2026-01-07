using System;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Diyetisyen/Doktor s�n�f�
    /// </summary>
    public class Doctor : User
    {
        public string Uzmanlik { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }

        public Doctor()
        {
            Role = UserRole.Doctor;
        }
    }
}
