using System;
using System.Collections.Generic;

namespace DiyetisyenOtomasyonu.Domain
{
    /// <summary>
    /// AI Analiz Sonucu - Yapay zeka destekli karar desteği
    /// 
    /// OOP Principle: Encapsulation - Analiz mantığı ve sonuçları kapsüllenir
    /// Academic: AI-assisted decision support with explainable results
    /// </summary>
    public class AIAnalysisResult
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public AIAnalysisType AnalysisType { get; set; }
        public DateTime AnalysisDate { get; set; }
        public string Result { get; set; }
        public string Recommendations { get; set; }
        public double Confidence { get; set; }  // 0-1 arası güven skoru
        public bool IsActedUpon { get; set; }   // Uygulandı mı?

        public AIAnalysisResult()
        {
            AnalysisDate = DateTime.Now;
            Confidence = 0;
            IsActedUpon = false;
        }

        /// <summary>
        /// Analiz tipi açıklaması
        /// </summary>
        public string AnalysisTypeName
        {
            get
            {
                switch (AnalysisType)
                {
                    case AIAnalysisType.WeightTrend: return "Kilo Trendi Analizi";
                    case AIAnalysisType.DietCompliance: return "Diyet Uyum Analizi";
                    case AIAnalysisType.NutrientBalance: return "Besin Dengesi Analizi";
                    case AIAnalysisType.RiskAssessment: return "Risk Değerlendirmesi";
                    case AIAnalysisType.MealSuggestion: return "Öğün Önerisi";
                    case AIAnalysisType.MotivationMessage: return "Motivasyon Mesajı";
                    default: return "Genel Analiz";
                }
            }
        }

        /// <summary>
        /// Güven seviyesi açıklaması
        /// </summary>
        public string ConfidenceLevel
        {
            get
            {
                if (Confidence >= 0.9) return "Çok Yüksek";
                if (Confidence >= 0.7) return "Yüksek";
                if (Confidence >= 0.5) return "Orta";
                if (Confidence >= 0.3) return "Düşük";
                return "Çok Düşük";
            }
        }
    }

    /// <summary>
    /// Risk Uyarısı - Otomatik tespit edilen riskler
    /// 
    /// Academic: Early risk detection system
    /// </summary>
    public class RiskAlert
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public RiskAlertType AlertType { get; set; }
        public DateTime AlertDate { get; set; }
        public string Message { get; set; }
        public int Severity { get; set; }  // 1-5 arası
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int? ResolvedByDoctorId { get; set; }

        public RiskAlert()
        {
            AlertDate = DateTime.Now;
            Severity = 1;
            IsResolved = false;
        }

        /// <summary>
        /// Uyarı tipi açıklaması
        /// </summary>
        public string AlertTypeName
        {
            get
            {
                switch (AlertType)
                {
                    case RiskAlertType.WeightPlateau: return "Kilo Platosu";
                    case RiskAlertType.RapidWeightLoss: return "Hızlı Kilo Kaybı";
                    case RiskAlertType.RapidWeightGain: return "Hızlı Kilo Alımı";
                    case RiskAlertType.LowCompliance: return "Düşük Diyet Uyumu";
                    case RiskAlertType.MissedMeals: return "Atlanan Öğünler";
                    case RiskAlertType.InsufficientWater: return "Yetersiz Su Tüketimi";
                    case RiskAlertType.HighCalorieDeviation: return "Yüksek Kalori Sapması";
                    case RiskAlertType.LowProteinIntake: return "Düşük Protein Alımı";
                    default: return "Genel Uyarı";
                }
            }
        }

        /// <summary>
        /// Şiddet açıklaması
        /// </summary>
        public string SeverityName
        {
            get
            {
                switch (Severity)
                {
                    case 1: return "Bilgilendirme";
                    case 2: return "Düşük";
                    case 3: return "Orta";
                    case 4: return "Yüksek";
                    case 5: return "Kritik";
                    default: return "Bilinmiyor";
                }
            }
        }
    }

    /// <summary>
    /// Diyet Uyum Kaydı - Günlük uyum takibi
    /// 
    /// Academic: Diet compliance tracking and analysis
    /// </summary>
    public class DietComplianceLog
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime Date { get; set; }
        public double ComplianceScore { get; set; }  // 0-100 arası
        public int MealsFollowed { get; set; }
        public int MealsTotal { get; set; }
        public string DeviationNotes { get; set; }

        /// <summary>
        /// Uyum yüzdesi
        /// </summary>
        public double CompliancePercentage => MealsTotal > 0 
            ? Math.Round((double)MealsFollowed / MealsTotal * 100, 1) 
            : 0;

        /// <summary>
        /// Uyum seviyesi açıklaması
        /// </summary>
        public string ComplianceLevel
        {
            get
            {
                if (ComplianceScore >= 90) return "Mükemmel";
                if (ComplianceScore >= 75) return "İyi";
                if (ComplianceScore >= 50) return "Orta";
                if (ComplianceScore >= 25) return "Zayıf";
                return "Çok Zayıf";
            }
        }
    }

    /// <summary>
    /// Haftalık Performans Raporu
    /// 
    /// Academic: Weekly performance analysis
    /// </summary>
    public class WeeklyPerformanceReport
    {
        public int PatientId { get; set; }
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        
        // Kilo metrikleri
        public double StartWeight { get; set; }
        public double EndWeight { get; set; }
        public double WeightChange { get; set; }
        
        // Uyum metrikleri
        public double AverageComplianceScore { get; set; }
        public int TotalMealsPlanned { get; set; }
        public int TotalMealsCompleted { get; set; }
        
        // Kalori metrikleri
        public double AverageDailyCalories { get; set; }
        public double TargetDailyCalories { get; set; }
        public double CalorieDeviation { get; set; }
        
        // Makro dağılımı
        public double AverageProteinPercentage { get; set; }
        public double AverageCarbsPercentage { get; set; }
        public double AverageFatPercentage { get; set; }

        // AI önerileri
        public List<string> Recommendations { get; set; }
        public List<string> Achievements { get; set; }
        public List<string> ImprovementAreas { get; set; }

        public WeeklyPerformanceReport()
        {
            Recommendations = new List<string>();
            Achievements = new List<string>();
            ImprovementAreas = new List<string>();
        }

        /// <summary>
        /// Genel performans skoru
        /// </summary>
        public double OverallScore
        {
            get
            {
                double complianceWeight = 0.4;
                double calorieWeight = 0.3;
                double weightWeight = 0.3;

                double complianceScore = AverageComplianceScore;
                
                double calorieScore = 100 - Math.Min(100, Math.Abs(CalorieDeviation) / TargetDailyCalories * 100);
                
                double weightScore = 100; // Başlangıç
                if (WeightChange < 0 && TargetDailyCalories < 2000) // Kilo verme hedefi
                    weightScore = Math.Min(100, 100 + WeightChange * 10);
                else if (WeightChange > 0 && TargetDailyCalories >= 2000) // Kilo alma hedefi
                    weightScore = Math.Min(100, 100 + WeightChange * 10);

                return Math.Round(
                    complianceScore * complianceWeight +
                    calorieScore * calorieWeight +
                    weightScore * weightWeight, 1);
            }
        }

        /// <summary>
        /// Performans seviyesi
        /// </summary>
        public string PerformanceLevel
        {
            get
            {
                if (OverallScore >= 90) return "Mükemmel";
                if (OverallScore >= 75) return "Çok İyi";
                if (OverallScore >= 60) return "İyi";
                if (OverallScore >= 40) return "Geliştirilmeli";
                return "Düşük";
            }
        }
    }
}

