using System;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// Kilo kaydı
    /// </summary>
    public class WeightEntry
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime Date { get; set; }
        public double Weight { get; set; }
        public string Notes { get; set; }
        public string MeasurementTime { get; set; }  // Sabah, akşam, vb.

        public WeightEntry()
        {
            Date = DateTime.Now;
        }

        /// <summary>
        /// Tarih formatı
        /// </summary>
        public string DateDisplay => Date.ToString("dd.MM.yyyy");

        /// <summary>
        /// Tarih ve saat formatı
        /// </summary>
        public string DateTimeDisplay => Date.ToString("dd.MM.yyyy HH:mm");

        /// <summary>
        /// Kilo formatı
        /// </summary>
        public string WeightDisplay => $"{Weight:F1} kg";
    }
}
