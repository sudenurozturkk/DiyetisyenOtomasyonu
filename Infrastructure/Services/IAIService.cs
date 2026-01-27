using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DiyetisyenOtomasyonu.Domain;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// AI Servis Arayüzü - Farklı AI sağlayıcıları (Gemini, OpenAI vb.) için standart
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// Genel bir prompt gönderir ve yanıt alır
        /// </summary>
        Task<string> GetAIResponseAsync(string prompt);

        /// <summary>
        /// Hasta verilerini analiz eder ve yapılandırılmış sonuç döner
        /// </summary>
        Task<AIAnalysisResult> AnalyzePatientDataAsync(string context, int patientId);

        /// <summary>
        /// STRICT JSON formatında yapılandırılmış diyetisyen desteği sağlar
        /// </summary>
        Task<DietitianSupportResponse> GetStructuredAnalysisAsync(string context);
    }

    /// <summary>
    /// DTO for AI Dietitian Support Response
    /// Matches the required JSON output schema strictly.
    /// </summary>
    public class DietitianSupportResponse
    {
        [JsonPropertyName("analysis_summary")]
        public string AnalysisSummary { get; set; }

        [JsonPropertyName("nutrition_comment")]
        public string NutritionComment { get; set; }

        [JsonPropertyName("daily_recommendations")]
        public List<string> DailyRecommendations { get; set; } = new List<string>();

        [JsonPropertyName("warnings")]
        public List<string> Warnings { get; set; } = new List<string>();

        [JsonPropertyName("dietitian_note_suggestion")]
        public string DietitianNoteSuggestion { get; set; }
    }
}
