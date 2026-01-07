using System;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// �lerleme �zet metrikleri
    /// </summary>
    public class ProgressSnapshot
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime Tarih { get; set; }
        public double Kilo { get; set; }
        public double SuMiktari { get; set; } // Litre
        public int AdimSayisi { get; set; }
        public double KaloriAlimi { get; set; }
        public double HaftaTamamlamaYuzdesi { get; set; }

        public ProgressSnapshot()
        {
            Tarih = DateTime.Now;
        }
    }
}
