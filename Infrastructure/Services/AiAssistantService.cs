using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// AI Asistan Servisi - Yapay zeka destekli karar desteği
    /// 
    /// OOP Principle: Single Responsibility - AI analiz ve öneri işlemlerinden sorumlu
    /// OOP Principle: Encapsulation - Analiz algoritmaları kapsüllenir
    /// Academic: AI-assisted decision support with explainable and non-intrusive suggestions
    /// </summary>
    public class AiAssistantService
    {
        private readonly PatientRepository _patientRepository;
        private readonly WeightEntryRepository _weightRepository;
        private readonly DietRepository _dietRepository;
        private readonly PatientDataAggregator _dataAggregator;
        private readonly IAIService _aiService;

        // AI yanıt şablonları
        private readonly Dictionary<string, string[]> _responseTemplates;

        public AiAssistantService()
        {
            _patientRepository = new PatientRepository();
            _weightRepository = new WeightEntryRepository();
            _dietRepository = new DietRepository();
            _dataAggregator = new PatientDataAggregator();
            
            // OpenRouter API Entegrasyonu
            _aiService = new GeminiAIService("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");

            _responseTemplates = InitializeResponseTemplates();
        }
        
        public AiAssistantService(IAIService aiService) : this()
        {
            _aiService = aiService;
        }

        /// <summary>
        /// Hasta için günlük ipucu üretir
        /// </summary>
        public DailyTip GenerateDailyTip(int patientId)
        {
            var patient = _patientRepository.GetFullPatientById(patientId);
            if (patient == null) return null;

            var tips = new List<DailyTip>();

            // Kilo bazlı ipuçları
            if (patient.BMI > 25)
            {
                tips.Add(new DailyTip
                {
                    Title = "Su Tüketimi",
                    Message = "Günde en az 2.5 litre su içmeyi hedefleyin. Su metabolizmayı hızlandırır ve tok hissetmenizi sağlar.",
                    Category = "Beslenme",
                    Priority = 2
                });
            }

            // Yaşam tarzına göre ipuçları
            switch (patient.LifestyleType)
            {
                case LifestyleType.OfficeWorker:
                    tips.Add(new DailyTip
                    {
                        Title = "Hareket Hatırlatması",
                        Message = "Her 45 dakikada bir kalkıp 5 dakika yürüyüş yapın. Metabolizmanızı aktif tutun!",
                        Category = "Aktivite",
                        Priority = 1
                    });
                    break;

                case LifestyleType.NightShift:
                    tips.Add(new DailyTip
                    {
                        Title = "Gece Beslenmesi",
                        Message = "Gece vardiyasında hafif protein ağırlıklı atıştırmalıklar tercih edin. Ağır yemeklerden kaçının.",
                        Category = "Beslenme",
                        Priority = 2
                    });
                    break;

                case LifestyleType.Student:
                    tips.Add(new DailyTip
                    {
                        Title = "Beyin Sağlığı",
                        Message = "Konsantrasyonunuzu artırmak için omega-3 içeren besinler tüketin: ceviz, balık, keten tohumu.",
                        Category = "Beslenme",
                        Priority = 1
                    });
                    break;
            }

            // Rastgele bir ipucu seç
            if (tips.Count == 0)
            {
                tips.Add(new DailyTip
                {
                    Title = "Günün Motivasyonu",
                    Message = "Küçük adımlar büyük sonuçlar doğurur. Bugün de hedeflerinize bir adım daha yaklaşın!",
                    Category = "Motivasyon",
                    Priority = 1
                });
            }

            var random = new Random();
            return tips[random.Next(tips.Count)];
        }

        /// <summary>
        /// Diyet uyum analizi yapar
        /// </summary>
        public DietComplianceAnalysis AnalyzeDietCompliance(int patientId, int days = 7)
        {
            var patient = _patientRepository.GetFullPatientById(patientId);
            if (patient == null) return null;

            var analysis = new DietComplianceAnalysis
            {
                PatientId = patientId,
                AnalysisDate = DateTime.Now,
                AnalysisPeriodDays = days
            };

            // Son haftalık planı al
            var dietWeeks = _dietRepository.GetPatientAllWeeks(patientId).ToList();
            if (dietWeeks.Count == 0)
            {
                analysis.Message = "Henüz diyet planı bulunmuyor.";
                analysis.OverallScore = 0;
                return analysis;
            }

            var latestWeek = dietWeeks.First();
            int totalMeals = 0;
            int completedMeals = 0;
            double totalCalories = 0;

            foreach (var day in latestWeek.Days)
            {
                foreach (var meal in day.Meals)
                {
                    totalMeals++;
                    if (meal.IsConfirmedByPatient)
                    {
                        completedMeals++;
                        totalCalories += meal.Calories;
                    }
                }
            }

            analysis.TotalMeals = totalMeals;
            analysis.CompletedMeals = completedMeals;
            analysis.OverallScore = totalMeals > 0 ? Math.Round((double)completedMeals / totalMeals * 100, 1) : 0;
            analysis.AverageDailyCalories = latestWeek.Days.Count > 0 ? Math.Round(totalCalories / latestWeek.Days.Count, 0) : 0;

            // Analiz mesajı oluştur
            if (analysis.OverallScore >= 90)
            {
                analysis.Message = "Harika! Diyet programınıza mükemmel uyum sağlıyorsunuz.";
                analysis.Recommendations.Add("Böyle devam edin, başarılı gidiyorsunuz!");
            }
            else if (analysis.OverallScore >= 70)
            {
                analysis.Message = "İyi gidiyorsunuz, ancak bazı öğünleri kaçırıyorsunuz.";
                analysis.Recommendations.Add("Öğün atlamamaya dikkat edin.");
                analysis.Recommendations.Add("Ara öğünleri unutmayın.");
            }
            else if (analysis.OverallScore >= 50)
            {
                analysis.Message = "Geliştirilmesi gereken alanlar var.";
                analysis.Recommendations.Add("Öğün saatlerine hatırlatıcı koyun.");
                analysis.Recommendations.Add("Diyetisyeninizle görüşün.");
            }
            else
            {
                analysis.Message = "Diyet programına uyum düşük seviyede.";
                analysis.Recommendations.Add("Acil olarak diyetisyeninizle görüşmeniz önerilir.");
                analysis.Recommendations.Add("Programı daha uygulanabilir hale getirmek için destek alın.");
            }

            return analysis;
        }

        /// <summary>
        /// Kilo trendi analizi yapar
        /// </summary>
        public WeightTrendAnalysis AnalyzeWeightTrend(int patientId)
        {
            var patient = _patientRepository.GetFullPatientById(patientId);
            if (patient == null) return null;

            var weightHistory = _weightRepository.GetByPatientId(patientId, 30).ToList();
            var analysis = new WeightTrendAnalysis
            {
                PatientId = patientId,
                AnalysisDate = DateTime.Now
            };

            if (weightHistory.Count < 2)
            {
                analysis.TrendDescription = "Yeterli kilo verisi yok. En az 2 kayıt gerekli.";
                analysis.Trend = WeightTrend.Unknown;
                return analysis;
            }

            var sortedWeights = weightHistory.OrderBy(w => w.Date).ToList();
            analysis.StartWeight = sortedWeights.First().Weight;
            analysis.CurrentWeight = sortedWeights.Last().Weight;
            analysis.WeightChange = Math.Round(analysis.CurrentWeight - analysis.StartWeight, 2);
            analysis.PercentageChange = Math.Round(analysis.WeightChange / analysis.StartWeight * 100, 2);

            // Trend belirleme
            if (Math.Abs(analysis.WeightChange) < 0.3)
            {
                analysis.Trend = WeightTrend.Stable;
                analysis.TrendDescription = "Kilonuz stabil seyrediyor.";
            }
            else if (analysis.WeightChange < -2)
            {
                analysis.Trend = WeightTrend.RapidLoss;
                analysis.TrendDescription = "Hızlı kilo kaybı var. Dikkatli olun.";
                analysis.Warnings.Add("Haftada 0.5-1 kg kayıp idealdir.");
            }
            else if (analysis.WeightChange < 0)
            {
                analysis.Trend = WeightTrend.Losing;
                analysis.TrendDescription = "Sağlıklı bir şekilde kilo veriyorsunuz.";
            }
            else if (analysis.WeightChange > 2)
            {
                analysis.Trend = WeightTrend.RapidGain;
                analysis.TrendDescription = "Hızlı kilo alımı tespit edildi.";
                analysis.Warnings.Add("Beslenme alışkanlıklarınızı gözden geçirin.");
            }
            else
            {
                analysis.Trend = WeightTrend.Gaining;
                analysis.TrendDescription = "Kilo alımı mevcut.";
            }

            // Plato tespiti
            var recentWeights = sortedWeights.Skip(Math.Max(0, sortedWeights.Count - 7)).ToList();
            if (recentWeights.Count >= 7)
            {
                var maxDiff = recentWeights.Max(w => w.Weight) - recentWeights.Min(w => w.Weight);
                if (maxDiff < 0.5)
                {
                    analysis.IsInPlateau = true;
                    analysis.Warnings.Add("Kilo platosu tespit edildi. Diyet veya aktivitenizi değiştirmeyi düşünün.");
                }
            }

            return analysis;
        }

        /// <summary>
        /// Motivasyon mesajı üretir
        /// </summary>
        public string GenerateMotivationMessage(int patientId)
        {
            var patient = _patientRepository.GetFullPatientById(patientId);
            if (patient == null) return "Hedeflerinize ulaşmak için bugün de motive olun!";

            var messages = new List<string>();

            // Kilo değişimine göre mesaj
            if (patient.KiloDegisimi < 0)
            {
                messages.Add($"Tebrikler! Başlangıçtan bu yana {Math.Abs(patient.KiloDegisimi):F1} kg verdiniz!");
                messages.Add("Her kilo veriş bir zaferdir. Böyle devam edin!");
            }
            else
            {
                messages.Add("Her yeni gün, sağlıklı seçimler için yeni bir fırsat!");
                messages.Add("Küçük adımlar büyük değişimler yaratır. Bugün de bir adım atın!");
            }

            // BMI'ya göre ek mesaj
            if (patient.BMI >= 25 && patient.BMI < 30)
            {
                messages.Add($"Hedefinize yaklaşıyorsunuz! İdeal kilo aralığı: {patient.IdealKiloAraligi}");
            }

            var random = new Random();
            return messages[random.Next(messages.Count)];
        }

        /// <summary>
        /// Atlanan öğün için telafi önerisi
        /// </summary>
        public MealCompensationSuggestion SuggestMealCompensation(int patientId, MealType missedMealType)
        {
            var patient = _patientRepository.GetFullPatientById(patientId);
            if (patient == null) return null;

            var suggestion = new MealCompensationSuggestion
            {
                PatientId = patientId,
                MissedMealType = missedMealType,
                SuggestionDate = DateTime.Now
            };

            switch (missedMealType)
            {
                case MealType.Breakfast:
                    suggestion.CompensationAdvice = "Kahvaltıyı atladıysanız, öğlen yemeğini yarım saat erken yemeyi deneyin.";
                    suggestion.AlternativeSnack = "Ara öğün olarak 1 avuç badem + 1 meyve tüketin.";
                    suggestion.CalorieAdjustment = "Öğle ve akşam yemeklerine 100'er kalori ekleyin.";
                    break;

                case MealType.Lunch:
                    suggestion.CompensationAdvice = "Öğle yemeğini atladıysanız, ikindi ara öğününüzü doyurucu yapın.";
                    suggestion.AlternativeSnack = "Protein bar veya peynir + tam buğday kraker";
                    suggestion.CalorieAdjustment = "Akşam yemeğine 200 kalori ekleyebilirsiniz.";
                    break;

                case MealType.Dinner:
                    suggestion.CompensationAdvice = "Akşam yemeğini atladıysanız, hafif bir atıştırmalık yapın.";
                    suggestion.AlternativeSnack = "1 kase yoğurt + muz veya süt + yulaf";
                    suggestion.CalorieAdjustment = "Ertesi gün kahvaltınızı doyurucu yapın.";
                    break;

                default:
                    suggestion.CompensationAdvice = "Ara öğün atlandığında bir sonraki öğüne kadar bekleyebilirsiniz.";
                    suggestion.AlternativeSnack = "1 meyve veya 5 adet badem";
                    break;
            }

            return suggestion;
        }

        /// <summary>
        /// Gelişmiş AI Analizi - Gerçek Gemini API Entegrasyonu
        /// </summary>
        public async Task<AIAnalysisResult> GetAdvancedAnalysisAsync(int patientId)
        {
            string context = _dataAggregator.AggregatePatientData(patientId);
            return await _aiService.AnalyzePatientDataAsync(context, patientId);
        }

        /// <summary>
        /// Kapsamlı AI Analizi - Raporlar sayfası için
        /// </summary>
        public async Task<AIAnalysisResult> GetComprehensiveAnalysisAsync(int patientId)
        {
            string context = _dataAggregator.AggregatePatientData(patientId);
            // GeminiAIService içindeki AnalyzePatientDataAsync zaten bu context'i kullanıyor
            // Ancak prompt'u daha detaylı hale getirmek için servisi güncelleyeceğiz
            return await _aiService.AnalyzePatientDataAsync(context, patientId);
        }

        /// <summary>
        /// AI ile Sohbet - Hasta verileri bağlamında
        /// </summary>
        public async Task<string> GetChatResponseAsync(string question, int patientId)
        {
            string context = _dataAggregator.AggregatePatientData(patientId);
            string prompt = $@"Aşağıdaki hasta verilerini dikkate alarak, bir diyetisyen asistanı olarak şu soruyu cevapla:
SORU: {question}

HASTA VERİLERİ:
{context}

Cevabın profesyonel, kısa ve net olsun.";

            return await _aiService.GetAIResponseAsync(prompt);
        }

        /// <summary>
        /// Soru-cevap sistemi (alias)
        /// </summary>
        public string GetResponse(string question, int patientId = 0)
        {
            return GetAIResponse(question, patientId);
        }

        /// <summary>
        /// Soru-cevap sistemi
        /// </summary>
        public string GetAIResponse(string question, int patientId = 0)
        {
            if (string.IsNullOrWhiteSpace(question))
                return "Lütfen bir soru sorun.";

            string lowerQuestion = question.ToLowerInvariant();

            // Anahtar kelime eşleştirme
            if (lowerQuestion.Contains("su") || lowerQuestion.Contains("sıvı"))
            {
                return GetRandomResponse("water");
            }
            if (lowerQuestion.Contains("protein") || lowerQuestion.Contains("et") || lowerQuestion.Contains("balık"))
            {
                return GetRandomResponse("protein");
            }
            if (lowerQuestion.Contains("kilo") || lowerQuestion.Contains("zayıfla") || lowerQuestion.Contains("diyet"))
            {
                return GetRandomResponse("weight");
            }
            if (lowerQuestion.Contains("egzersiz") || lowerQuestion.Contains("spor") || lowerQuestion.Contains("hareket"))
            {
                return GetRandomResponse("exercise");
            }
            if (lowerQuestion.Contains("şeker") || lowerQuestion.Contains("tatlı") || lowerQuestion.Contains("karbonhidrat"))
            {
                return GetRandomResponse("sugar");
            }
            if (lowerQuestion.Contains("kahvaltı") || lowerQuestion.Contains("sabah"))
            {
                return GetRandomResponse("breakfast");
            }
            if (lowerQuestion.Contains("akşam") || lowerQuestion.Contains("gece"))
            {
                return GetRandomResponse("dinner");
            }
            if (lowerQuestion.Contains("meyve") || lowerQuestion.Contains("sebze"))
            {
                return GetRandomResponse("fruits");
            }

            return GetRandomResponse("general");
        }

        private string GetRandomResponse(string category)
        {
            if (_responseTemplates.TryGetValue(category, out var responses))
            {
                var random = new Random();
                return responses[random.Next(responses.Length)];
            }
            return "Bu konuda size yardımcı olabilirim. Lütfen daha spesifik bir soru sorun.";
        }

        private Dictionary<string, string[]> InitializeResponseTemplates()
        {
            return new Dictionary<string, string[]>
            {
                {
                    "water", new[]
                    {
                        "Günde en az 2-2.5 litre su içmeniz önerilir. Vücudunuzun kilo başına 30ml suya ihtiyacı vardır.",
                        "Su içmeyi hatırlamak için yanınızda şişe taşıyın ve her saat başı bir bardak için.",
                        "Kahve ve çay su ihtiyacınızı karşılamaz. Saf su tüketimini artırın."
                    }
                },
                {
                    "protein", new[]
                    {
                        "Günlük protein ihtiyacınız kilo başına 0.8-1.2 gram arasındadır. Tavuk, balık, yumurta iyi kaynaklardır.",
                        "Bitkisel proteinler için mercimek, nohut ve kinoa tüketebilirsiniz.",
                        "Her ana öğünde avuç içi kadar protein kaynağı tüketmeye çalışın."
                    }
                },
                {
                    "weight", new[]
                    {
                        "Sağlıklı kilo vermek için haftada 0.5-1 kg kayıp idealdir. Sabırlı olun!",
                        "Kalori açığı oluşturmak önemli ama aşırı kısıtlama metabolizmayı yavaşlatır.",
                        "Kilo vermek maraton, sprint değil. Sürdürülebilir alışkanlıklar kazanın."
                    }
                },
                {
                    "exercise", new[]
                    {
                        "Haftada en az 150 dakika orta yoğunlukta egzersiz yapmanız önerilir.",
                        "Yürüyüş basit ama etkili bir egzersizdir. Günde 30 dakika ile başlayın.",
                        "Direnç egzersizleri kas kütlenizi korur ve metabolizmayı hızlandırır."
                    }
                },
                {
                    "sugar", new[]
                    {
                        "Günlük şeker tüketimini 25 gramın altında tutmaya çalışın.",
                        "Doğal şeker kaynağı olarak meyveleri tercih edin, işlenmiş şekerlerden kaçının.",
                        "Şeker isteği geldiğinde tarçınlı çay veya bitter çikolata (1-2 kare) tercih edin."
                    }
                },
                {
                    "breakfast", new[]
                    {
                        "Kahvaltı günün en önemli öğünüdür. Protein içeren bir kahvaltı uzun süre tok tutar.",
                        "İdeal kahvaltı: Yumurta + tam tahıl + sebze + sağlıklı yağ",
                        "Kahvaltıyı atlamak metabolizmayı yavaşlatır ve gün içinde aşırı yemeye neden olabilir."
                    }
                },
                {
                    "dinner", new[]
                    {
                        "Akşam yemeğini yatmadan 2-3 saat önce yemeye çalışın.",
                        "Akşam öğünü hafif olmalı: Sebze ağırlıklı + az protein",
                        "Gece atıştırma isteği gelirse ılık süt veya papatya çayı için."
                    }
                },
                {
                    "fruits", new[]
                    {
                        "Günde 2-3 porsiyon meyve tüketimi idealdir. Meyveleri öğün arasında yiyin.",
                        "Sebzelerin yarısını çiğ tüketmeye çalışın, vitamin kaybını önleyin.",
                        "Mevsim meyve ve sebzelerini tercih edin, daha besleyici ve ekonomiktir."
                    }
                },
                {
                    "general", new[]
                    {
                        "Sağlıklı beslenme bir yaşam tarzıdır, kısa vadeli diyet değil.",
                        "Hedeflerinize ulaşmak için sabırlı ve tutarlı olun.",
                        "Diyetisyeninizle düzenli iletişim halinde kalın.",
                        "Her yeni gün sağlıklı seçimler için yeni bir fırsattır!"
                    }
                }
            };
        }
    }

    #region AI Analysis Models

    public class DailyTip
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Category { get; set; }
        public int Priority { get; set; }
    }

    public class DietComplianceAnalysis
    {
        public int PatientId { get; set; }
        public DateTime AnalysisDate { get; set; }
        public int AnalysisPeriodDays { get; set; }
        public double OverallScore { get; set; }
        public int TotalMeals { get; set; }
        public int CompletedMeals { get; set; }
        public double AverageDailyCalories { get; set; }
        public string Message { get; set; }
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    public enum WeightTrend
    {
        Unknown,
        Stable,
        Losing,
        RapidLoss,
        Gaining,
        RapidGain
    }

    public class WeightTrendAnalysis
    {
        public int PatientId { get; set; }
        public DateTime AnalysisDate { get; set; }
        public double StartWeight { get; set; }
        public double CurrentWeight { get; set; }
        public double WeightChange { get; set; }
        public double PercentageChange { get; set; }
        public WeightTrend Trend { get; set; }
        public string TrendDescription { get; set; }
        public bool IsInPlateau { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class MealCompensationSuggestion
    {
        public int PatientId { get; set; }
        public MealType MissedMealType { get; set; }
        public DateTime SuggestionDate { get; set; }
        public string CompensationAdvice { get; set; }
        public string AlternativeSnack { get; set; }
        public string CalorieAdjustment { get; set; }
    }

    #endregion
}
