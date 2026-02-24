using System;
using System.Collections.Generic;

namespace DiyetisyenOtomasyonu.Domain
{
    public class Patient : User
    {
        // Temel bilgiler
        public string Cinsiyet { get; set; }
        public int Yas { get; set; }
        public double Boy { get; set; } // cm
        public double BaslangicKilosu { get; set; } // kg
        public double GuncelKilo { get; set; } // kg
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } // Not mapped, filled by repository
        public string Notlar { get; set; }

        // Tıbbi bilgiler
        public string ThyroidStatus { get; set; }
        public string InsulinStatus { get; set; }
        public string MedicalHistory { get; set; }  // Kronik hastalıklar
        public string Medications { get; set; }      // Kullanılan ilaçlar
        public string AllergiesText { get; set; }    // Alerjiler (metin)

        // Yaşam tarzı ve aktivite
        public LifestyleType LifestyleType { get; set; }
        public ActivityLevel ActivityLevel { get; set; }

        // Navigation properties (lazy loading için)
        private List<PatientAllergy> _allergies;
        public List<PatientAllergy> Allergies
        {
            get { return _allergies ?? (_allergies = new List<PatientAllergy>()); }
            set { _allergies = value; }
        }

        /// <summary>
        /// BMI hesaplama (Vücut Kitle İndeksi)
        /// Formül: Kilo (kg) / Boy² (m²)
        /// </summary>
        public double BMI
        {
            get
            {
                if (Boy <= 0) return 0;
                return Math.Round(GuncelKilo / Math.Pow(Boy / 100, 2), 2);
            }
        }

        /// <summary>
        /// BMI kategorisi
        /// WHO standartlarına göre sınıflandırma
        /// </summary>
        public string BMIKategori
        {
            get
            {
                if (BMI < 18.5) return "Zayıf";
                if (BMI < 24.9) return "Normal";
                if (BMI < 29.9) return "Fazla Kilolu";
                if (BMI < 34.9) return "Obez (Tip 1)";
                if (BMI < 39.9) return "Obez (Tip 2)";
                return "Morbid Obez";
            }
        }

        /// <summary>
        /// Kilo değişimi (başlangıçtan itibaren)
        /// </summary>
        public double KiloDegisimi
        {
            get { return Math.Round(GuncelKilo - BaslangicKilosu, 2); }
        }

        /// <summary>
        /// Kilo değişim yüzdesi
        /// </summary>
        public double KiloDegisimYuzdesi
        {
            get
            {
                if (BaslangicKilosu <= 0) return 0;
                return Math.Round((KiloDegisimi / BaslangicKilosu) * 100, 2);
            }
        }

        /// <summary>
        /// Bazal Metabolizma Hızı (BMR) hesaplama
        /// Mifflin-St Jeor denklemi kullanılır
        /// </summary>
        public double BMR
        {
            get
            {
                // Mifflin-St Jeor Equation
                double bmr;
                if (Cinsiyet?.ToLower() == "erkek")
                {
                    bmr = (10 * GuncelKilo) + (6.25 * Boy) - (5 * Yas) + 5;
                }
                else
                {
                    bmr = (10 * GuncelKilo) + (6.25 * Boy) - (5 * Yas) - 161;
                }
                return Math.Round(bmr, 0);
            }
        }

        /// <summary>
        /// Günlük Kalori İhtiyacı (TDEE)
        /// BMR x Aktivite Çarpanı
        /// </summary>
        public double TDEE
        {
            get
            {
                double multiplier = 1.2; // Sedentary default
                switch (ActivityLevel)
                {
                    case ActivityLevel.LightlyActive:
                        multiplier = 1.375;
                        break;
                    case ActivityLevel.ModeratelyActive:
                        multiplier = 1.55;
                        break;
                    case ActivityLevel.VeryActive:
                        multiplier = 1.725;
                        break;
                    case ActivityLevel.ExtraActive:
                        multiplier = 1.9;
                        break;
                }
                return Math.Round(BMR * multiplier, 0);
            }
        }

        /// <summary>
        /// İdeal kilo aralığı (BMI 18.5-24.9 için)
        /// </summary>
        public string IdealKiloAraligi
        {
            get
            {
                if (Boy <= 0) return "Hesaplanamıyor";
                double boyMetre = Boy / 100;
                double minKilo = Math.Round(18.5 * boyMetre * boyMetre, 1);
                double maxKilo = Math.Round(24.9 * boyMetre * boyMetre, 1);
                return $"{minKilo} - {maxKilo} kg";
            }
        }

        /// <summary>
        /// Yaşam tarzı açıklaması
        /// </summary>
        public string LifestyleDescription
        {
            get
            {
                switch (LifestyleType)
                {
                    case LifestyleType.Student: return "Öğrenci";
                    case LifestyleType.OfficeWorker: return "Ofis Çalışanı";
                    case LifestyleType.NightShift: return "Gece Vardiyası";
                    case LifestyleType.Athlete: return "Sporcu";
                    case LifestyleType.HomeMaker: return "Ev Hanımı";
                    case LifestyleType.Retired: return "Emekli";
                    case LifestyleType.Freelancer: return "Serbest Çalışan";
                    default: return "Bilinmiyor";
                }
            }
        }

        /// <summary>
        /// Aktivite seviyesi açıklaması
        /// </summary>
        public string ActivityDescription
        {
            get
            {
                switch (ActivityLevel)
                {
                    case ActivityLevel.Sedentary: return "Hareketsiz";
                    case ActivityLevel.LightlyActive: return "Hafif Aktif";
                    case ActivityLevel.ModeratelyActive: return "Orta Aktif";
                    case ActivityLevel.VeryActive: return "Çok Aktif";
                    case ActivityLevel.ExtraActive: return "Aşırı Aktif";
                    default: return "Bilinmiyor";
                }
            }
        }

        public Patient()
        {
            Role = UserRole.Patient;
            LifestyleType = LifestyleType.OfficeWorker;
            ActivityLevel = ActivityLevel.LightlyActive;
        }
    }

    public class PatientAllergy
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string AllergyType { get; set; }
        public string Description { get; set; }
        public AllergySeverity Severity { get; set; }

        /// <summary>
        /// Şiddet açıklaması
        /// </summary>
        public string SeverityDescription
        {
            get
            {
                switch (Severity)
                {
                    case AllergySeverity.Mild: return "Hafif";
                    case AllergySeverity.Moderate: return "Orta";
                    case AllergySeverity.Severe: return "Şiddetli";
                    case AllergySeverity.LifeThreatening: return "Hayati Tehlike";
                    default: return "Bilinmiyor";
                }
            }
        }
    }
}
