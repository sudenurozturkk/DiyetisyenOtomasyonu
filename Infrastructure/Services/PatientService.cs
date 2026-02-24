using System;
using System.Collections.Generic;
using System.Linq;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    public class PatientService
    {
        private readonly PatientRepository _patientRepository;
        private readonly UserRepository _userRepository;
        private readonly WeightEntryRepository _weightRepository;

        // Validation constants
        private const int MIN_AGE = 1;
        private const int MAX_AGE = 120;
        private const double MIN_HEIGHT = 50.0;
        private const double MAX_HEIGHT = 250.0;
        private const double MIN_WEIGHT = 20.0;
        private const double MAX_WEIGHT = 500.0;

        public PatientService(
            PatientRepository patientRepository,
            UserRepository userRepository,
            WeightEntryRepository weightRepository)
        {
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _weightRepository = weightRepository ?? throw new ArgumentNullException(nameof(weightRepository));
        }

        /// <summary>
        /// Tüm hastaları getir
        /// </summary>
        public List<Patient> GetAllPatients()
        {
            return _patientRepository.GetAllWithUserInfo().ToList();
        }

        /// <summary>
        /// Doktora ait hastaları getir
        /// </summary>
        public List<Patient> GetPatientsByDoctor(int doctorId)
        {
            Guards.AgainstZeroOrNegative(doctorId, nameof(doctorId));
            return _patientRepository.GetByDoctorId(doctorId).ToList();
        }

        /// <summary>
        /// ID'ye göre hasta getir
        /// </summary>
        public Patient GetPatientById(int id)
        {
            Guards.AgainstZeroOrNegative(id, nameof(id));
            return _patientRepository.GetFullPatientById(id);
        }

        /// <summary>
        /// Yeni hasta oluştur
        /// </summary>
        public Patient CreatePatient(string adSoyad, string kullaniciAdi, string parola,
            string cinsiyet, int yas, double boy, double baslangicKilo, int doctorId,
            LifestyleType lifestyle = LifestyleType.OfficeWorker,
            ActivityLevel activity = ActivityLevel.LightlyActive)
        {
            // Validasyon
            Guards.AgainstNullOrEmpty(adSoyad, nameof(adSoyad));
            Guards.AgainstNullOrEmpty(kullaniciAdi, nameof(kullaniciAdi));
            Guards.AgainstNullOrEmpty(parola, nameof(parola));
            Guards.AgainstZeroOrNegative(doctorId, nameof(doctorId));

            // Kullanıcı adı kontrolü
            if (_userRepository.UsernameExists(kullaniciAdi))
                throw new ArgumentException("Bu kullanıcı adı zaten kullanılıyor", nameof(kullaniciAdi));

            // Parola validasyonu
            if (!PasswordHasher.IsValidPassword(parola, out string errorMsg))
                throw new ArgumentException(errorMsg, nameof(parola));

            // Yaş kontrolü
            if (yas < MIN_AGE || yas > MAX_AGE)
                throw new ArgumentOutOfRangeException(nameof(yas), $"Yaş {MIN_AGE}-{MAX_AGE} arasında olmalıdır");

            // Boy kontrolü
            if (boy < MIN_HEIGHT || boy > MAX_HEIGHT)
                throw new ArgumentOutOfRangeException(nameof(boy), $"Boy {MIN_HEIGHT}-{MAX_HEIGHT} cm arasında olmalıdır");

            // Kilo kontrolü
            if (baslangicKilo < MIN_WEIGHT || baslangicKilo > MAX_WEIGHT)
                throw new ArgumentOutOfRangeException(nameof(baslangicKilo), $"Kilo {MIN_WEIGHT}-{MAX_WEIGHT} kg arasında olmalıdır");

            var patient = new Patient
            {
                AdSoyad = adSoyad.Trim(),
                KullaniciAdi = kullaniciAdi.Trim().ToLower(),
                ParolaHash = PasswordHasher.HashPassword(parola),
                Cinsiyet = cinsiyet?.Trim(),
                Yas = yas,
                Boy = boy,
                BaslangicKilosu = baslangicKilo,
                GuncelKilo = baslangicKilo,
                DoctorId = doctorId,
                LifestyleType = lifestyle,
                ActivityLevel = activity,
                AktifMi = true
            };

            int patientId = _patientRepository.CreatePatient(patient);
            patient.Id = patientId;

            // İlk kilo kaydını oluştur
            CreateInitialWeightEntry(patientId, baslangicKilo);

            return patient;
        }

        /// <summary>
        /// Hasta ekle (Nesne ile)
        /// </summary>
        public void AddPatient(Patient patient)
        {
            Guards.AgainstNull(patient, nameof(patient));

            // Kullanıcı adı kontrolü
            if (_userRepository.UsernameExists(patient.KullaniciAdi))
                throw new ArgumentException("Bu kullanıcı adı zaten kullanılıyor", nameof(patient.KullaniciAdi));

            // Parola hashleme (eğer hashlenmemişse)
            if (!string.IsNullOrEmpty(patient.ParolaHash) && !patient.ParolaHash.StartsWith("$2a$"))
            {
                patient.ParolaHash = PasswordHasher.HashPassword(patient.ParolaHash);
            }

            patient.AktifMi = true;
            int id = _patientRepository.CreatePatient(patient);
            patient.Id = id;

            // İlk kilo kaydını oluştur
            CreateInitialWeightEntry(id, patient.BaslangicKilosu);
        }

        /// <summary>
        /// Hasta güncelle
        /// </summary>
        public void UpdatePatient(Patient patient)
        {
            Guards.AgainstNull(patient, nameof(patient));

            var existing = _patientRepository.GetFullPatientById(patient.Id);
            if (existing == null)
                throw new ArgumentException("Hasta bulunamadı", nameof(patient));

            _patientRepository.Update(patient);
        }

        /// <summary>
        /// Hasta sil
        /// </summary>
        public void DeletePatient(int id)
        {
            Guards.AgainstZeroOrNegative(id, nameof(id));

            var patient = _patientRepository.GetById(id);
            if (patient == null)
                throw new ArgumentException("Hasta bulunamadı", nameof(id));

            _patientRepository.DeletePatient(id);
        }

        /// <summary>
        /// Hasta ara
        /// </summary>
        public List<Patient> SearchPatients(string searchText, int? doctorId = null)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return doctorId.HasValue
                    ? GetPatientsByDoctor(doctorId.Value)
                    : GetAllPatients();
            }

            return _patientRepository.Search(searchText, doctorId).ToList();
        }

        /// <summary>
        /// Kilo güncelle
        /// </summary>
        public void UpdateWeight(int patientId, double newWeight, string notes = "")
        {
            Guards.AgainstZeroOrNegative(patientId, nameof(patientId));
            Guards.AgainstZeroOrNegative(newWeight, nameof(newWeight));

            var patient = GetPatientById(patientId);
            if (patient == null)
                throw new ArgumentException("Hasta bulunamadı", nameof(patientId));

            patient.GuncelKilo = newWeight;
            _patientRepository.Update(patient);

            // Yeni kilo kaydı ekle
            _weightRepository.Add(new WeightEntry
            {
                PatientId = patientId,
                Date = DateTime.Now,
                Weight = newWeight,
                Notes = notes ?? string.Empty
            });
        }

        /// <summary>
        /// Hastanın kilo geçmişini getir
        /// </summary>
        public List<WeightEntry> GetWeightHistory(int patientId, int? days = null)
        {
            Guards.AgainstZeroOrNegative(patientId, nameof(patientId));
            return _weightRepository.GetByPatientId(patientId, days).ToList();
        }

        /// <summary>
        /// Hastanın risk durumunu kontrol et
        /// </summary>
        public PatientRiskStatus GetPatientRiskStatus(int patientId)
        {
            var patient = GetPatientById(patientId);
            if (patient == null) return null;

            var weightHistory = GetWeightHistory(patientId, 30);
            var riskStatus = new PatientRiskStatus { PatientId = patientId };

            // Kilo değişimi analizi
            if (weightHistory.Count >= 2)
            {
                var sortedWeights = weightHistory.OrderBy(w => w.Date).ToList();
                var firstWeight = sortedWeights.First().Weight;
                var lastWeight = sortedWeights.Last().Weight;
                var weightChange = lastWeight - firstWeight;
                var changePercent = Math.Abs(weightChange / firstWeight * 100);

                if (changePercent < 0.5 && weightHistory.Count >= 7)
                {
                    riskStatus.HasWeightPlateau = true;
                    riskStatus.RiskMessages.Add("Kilo platosunda: Son 30 günde kilo değişimi yok");
                }
                else if (weightChange < -3)
                {
                    riskStatus.HasRapidWeightLoss = true;
                    riskStatus.RiskMessages.Add($"Hızlı kilo kaybı: {Math.Abs(weightChange):F1} kg");
                }
                else if (weightChange > 3)
                {
                    riskStatus.HasRapidWeightGain = true;
                    riskStatus.RiskMessages.Add($"Hızlı kilo alımı: {weightChange:F1} kg");
                }
            }

            // BMI kontrolü
            if (patient.BMI < 18.5)
            {
                riskStatus.RiskMessages.Add("Düşük BMI: Zayıf kategorisinde");
            }
            else if (patient.BMI >= 30)
            {
                riskStatus.RiskMessages.Add("Yüksek BMI: Obez kategorisinde");
            }

            riskStatus.OverallRiskLevel = riskStatus.RiskMessages.Count > 2 ? 3 :
                                          riskStatus.RiskMessages.Count > 0 ? 2 : 1;

            return riskStatus;
        }

        /// <summary>
        /// İlk kilo kaydını oluştur
        /// </summary>
        private void CreateInitialWeightEntry(int patientId, double initialWeight)
        {
            _weightRepository.Add(new WeightEntry
            {
                PatientId = patientId,
                Date = DateTime.Now,
                Weight = initialWeight,
                Notes = "Başlangıç kilosu"
            });
        }
    }

    /// <summary>
    /// Hasta risk durumu
    /// </summary>
    public class PatientRiskStatus
    {
        public int PatientId { get; set; }
        public bool HasWeightPlateau { get; set; }
        public bool HasRapidWeightLoss { get; set; }
        public bool HasRapidWeightGain { get; set; }
        public int OverallRiskLevel { get; set; }  // 1: Düşük, 2: Orta, 3: Yüksek
        public List<string> RiskMessages { get; set; } = new List<string>();

        public bool HasAnyRisk => RiskMessages.Count > 0;

        public string RiskLevelName
        {
            get
            {
                switch (OverallRiskLevel)
                {
                    case 1: return "Düşük";
                    case 2: return "Orta";
                    case 3: return "Yüksek";
                    default: return "Belirsiz";
                }
            }
        }
    }
}
