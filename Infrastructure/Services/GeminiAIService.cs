using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DiyetisyenOtomasyonu.Domain;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    public class GeminiAIService : IAIService
    {
        private readonly string _apiKey;
        private readonly string _apiUrl = "https://openrouter.ai/api/v1/chat/completions";
        private readonly HttpClient _httpClient;

        public GeminiAIService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://dietpro.app");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "DiyetPro");
        }

        public async Task<string> GetAIResponseAsync(string prompt)
        {
            int maxRetries = 5;
            int retryDelayMs = 3000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var requestBody = new
                    {
                        model = "google/gemini-2.0-flash-001",
                        messages = new[]
                        {
                            new { role = "user", content = prompt }
                        }
                    };

                    string jsonRequest = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(_apiUrl, content);
                    
                    if ((int)response.StatusCode == 429)
                    {
                        if (attempt < maxRetries)
                        {
                            await Task.Delay(retryDelayMs * attempt);
                            continue;
                        }
                        return "⚠️ API istek limiti aşıldı. Birkaç dakika bekleyip tekrar deneyin. İpucu: OpenRouter hesabınıza kredi ekleyin.";
                    }
                    
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        return $"❌ API Hatası ({(int)response.StatusCode}): {jsonResponse.Substring(0, Math.Min(200, jsonResponse.Length))}";
                    }

                    using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                    {
                        var choices = doc.RootElement.GetProperty("choices");
                        if (choices.GetArrayLength() > 0)
                        {
                            return choices[0]
                                .GetProperty("message")
                                .GetProperty("content")
                                .GetString();
                        }
                        return "❌ AI yanıt üretemedi.";
                    }
                }
                catch (HttpRequestException ex)
                {
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(retryDelayMs * attempt);
                        continue;
                    }
                    return $"❌ Bağlantı Hatası: {ex.Message}";
                }
                catch (Exception ex)
                {
                    return $"❌ Hata: {ex.Message}";
                }
            }
            return "❌ API yanıt vermedi. Lütfen daha sonra tekrar deneyin.";
        }

        public async Task<AIAnalysisResult> AnalyzePatientDataAsync(string context, int patientId)
        {
            string prompt = $@"Aşağıdaki hasta verilerini analiz et ve profesyonel bir diyetisyen gibi öneriler sun. 
Yanıtını MUTLAKA şu formatta ver:
SONUÇ: [Hastanın genel durumu hakkında 1-2 cümlelik özet]
BESLENME ANALİZİ: [Öğün uyumu, kalori alımı ve beslenme alışkanlıkları hakkında analiz]
AKTİVİTE ANALİZİ: [Egzersiz performansı ve hareketlilik durumu hakkında analiz]
ÖNERİLER: [Diyetisyen için madde madde profesyonel aksiyon tavsiyeleri]

HASTA VERİLERİ:
{context}";

            string aiResponse = await GetAIResponseAsync(prompt);

            var result = new AIAnalysisResult
            {
                PatientId = patientId,
                AnalysisDate = DateTime.Now,
                Confidence = 0.9,
                Result = "Analiz Tamamlandı",
                Recommendations = aiResponse
            };

            if (aiResponse.Contains("SONUÇ:"))
            {
                int start = aiResponse.IndexOf("SONUÇ:") + 6;
                int end = aiResponse.IndexOf("ÖNERİLER:");
                if (end > start)
                {
                    result.Result = aiResponse.Substring(start, end - start).Trim();
                    result.Recommendations = aiResponse.Substring(end + 9).Trim();
                }
            }

            return result;
        }


        public async Task<DietitianSupportResponse> GetStructuredAnalysisAsync(string context)
        {
            string systemPrompt = @"Sen bir diyetisyen destek asistanısın.
Görevin SADECE verilen hasta verilerini yorumlamak ve destek metinleri üretmek.
Sen doktor DEĞİLSİN. Kesinlikle tanı koymaz veya tedavi önermezsin.
SON KARARLARI diyetisyen verir, sen sadece yardımcısın.
BMI, kalori veya hedef HESAPLAMASI yapmazsın - bunlar zaten veriliyor.
ÖNEMLİ: Sadece sana verilen verilere dayanarak yorum yap. VERİLMEYEN bilgileri (aile geçmişi, kronik hastalık vb.) UYDURMAYACAKSIN.
Çıktı MUTLAKA şu JSON şemasına uygun olmalı:
{
  ""analysis_summary"": ""string - genel durum özeti"",
  ""nutrition_comment"": ""string - beslenme yorumu"",
  ""daily_recommendations"": [""string - günlük tavsiyeler""],
  ""warnings"": [""string - dikkat edilmesi gerekenler""],
  ""dietitian_note_suggestion"": ""string - diyetisyen için not önerisi""
}
Nötr ve profesyonel dil kullan. Türkçe yaz.
Markdown formatı kullanma (```json gibi). Sadece ham JSON döndür.";

            string userPrompt = $"HASTA VERİLERİ:\n{context}";

            int maxRetries = 5;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var requestBody = new
                    {
                        model = "google/gemini-2.0-flash-001",
                        messages = new[]
                        {
                            new { role = "system", content = systemPrompt },
                            new { role = "user", content = userPrompt }
                        },
                        response_format = new { type = "json_object" }
                    };

                    string jsonRequest = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(_apiUrl, content);
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        if (attempt < maxRetries) { await Task.Delay(1000 * attempt); continue; }
                        return CreateFallbackResponse("API Error: " + response.StatusCode);
                    }

                    using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                    {
                        var choices = doc.RootElement.GetProperty("choices");
                        if (choices.GetArrayLength() > 0)
                        {
                            string aiContent = choices[0].GetProperty("message").GetProperty("content").GetString();
                            
                            // Clean up markdown if present
                            aiContent = aiContent.Replace("```json", "").Replace("```", "").Trim();

                            try
                            {
                                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                                return JsonSerializer.Deserialize<DietitianSupportResponse>(aiContent, options);
                            }
                            catch (JsonException)
                            {
                                if (attempt < maxRetries) continue;
                                return CreateFallbackResponse("JSON Parsing Error");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (attempt < maxRetries) { await Task.Delay(1000 * attempt); continue; }
                    return CreateFallbackResponse("Connection Error: " + ex.Message);
                }
            }

            return CreateFallbackResponse("Service Unavailable");
        }

        private DietitianSupportResponse CreateFallbackResponse(string reason)
        {
            return new DietitianSupportResponse
            {
                AnalysisSummary = "AI servisine şu anda ulaşılamıyor.",
                NutritionComment = "Lütfen manuel değerlendirme yapınız.",
                DailyRecommendations = new List<string> { "Sistem bağlantısı kontrol ediliyor." },
                Warnings = new List<string> { reason },
                DietitianNoteSuggestion = "Otomatik not oluşturulamadı."
            };
        }
    }
}
