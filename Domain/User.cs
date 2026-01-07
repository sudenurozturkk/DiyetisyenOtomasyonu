using System;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Temel kullan�c� s�n�f�
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; }
        public string KullaniciAdi { get; set; }
        public string ParolaHash { get; set; }
        public UserRole Role { get; set; }
        public DateTime KayitTarihi { get; set; }
        public bool AktifMi { get; set; }

        public User()
        {
            KayitTarihi = DateTime.Now;
            AktifMi = true;
        }
    }
}
