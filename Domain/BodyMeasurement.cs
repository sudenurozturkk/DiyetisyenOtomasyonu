using System;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Vücut ölçüm kaydı - Bel, kalça, göğüs, kol, bacak ölçümleri
    /// </summary>
    public class BodyMeasurement
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime Date { get; set; }
        
        // Ölçümler (cm)
        public double? Chest { get; set; }      // Göğüs
        public double? Waist { get; set; }      // Bel
        public double? Hip { get; set; }        // Kalça
        public double? Arm { get; set; }        // Kol
        public double? Thigh { get; set; }      // Bacak
        public double? Neck { get; set; }       // Boyun
        
        public string Notes { get; set; }
        
        // Hesaplanan değerler
        public double? WaistToHipRatio => (Waist.HasValue && Hip.HasValue && Hip.Value > 0) 
            ? Math.Round(Waist.Value / Hip.Value, 2) 
            : (double?)null;
    }
}
