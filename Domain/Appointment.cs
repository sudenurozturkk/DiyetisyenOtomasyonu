using System;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Randevu türleri
    /// </summary>
    public enum AppointmentType
    {
        Online = 0,     // Online randevu
        Clinic = 1      // Klinik (yüz yüze)
    }
    
    /// <summary>
    /// Randevu durumları
    /// </summary>
    public enum AppointmentStatus
    {
        Pending = 0,    // Beklemede (onay bekliyor)
        Approved = 1,   // Onaylandı
        Completed = 2,  // Tamamlandı
        Cancelled = 3   // İptal edildi
    }
    
    /// <summary>
    /// Randevu - Online veya Klinik randevular
    /// </summary>
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        
        public DateTime DateTime { get; set; }
        public AppointmentType Type { get; set; }
        public AppointmentStatus Status { get; set; }
        public string Notes { get; set; }
        public decimal Price { get; set; } // Randevu ücreti
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        
        // Durum kontrolü
        public bool IsUpcoming => Status == AppointmentStatus.Approved && DateTime > System.DateTime.Now;
        public bool IsPast => DateTime < System.DateTime.Now;
        
        public string TypeText => Type == AppointmentType.Online ? "Online" : "Klinik";
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case AppointmentStatus.Pending: return "Beklemede";
                    case AppointmentStatus.Approved: return "Onaylandı";
                    case AppointmentStatus.Completed: return "Tamamlandı";
                    case AppointmentStatus.Cancelled: return "İptal";
                    default: return "Bilinmiyor";
                }
            }
        }
    }
}
