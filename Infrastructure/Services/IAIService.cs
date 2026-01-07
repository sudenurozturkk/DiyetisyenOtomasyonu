using System.Threading.Tasks;
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
    }
}
