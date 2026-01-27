# AI Integration Architecture

## Overview
This document outlines the architectural implementation of the AI-powered dietitian support assistant within the Dietitian Automation System. The system uses the OpenRouter API (Gemini model) to provide supportive, non-medical analysis.

## Core Principles
1.  **Support Role:** AI is a tool for the dietitian, not a replacement.
2.  **No Diagnosis:** AI explicitly refuses to diagnose or treat.
3.  **No Calculations:** All numerical data (BMI, targets) is calculated by the C# backend.
4.  **Structured Output:** AI returns strict JSON to ensure UI consistency.

## Architecture

### 1. DTOs (Data Transfer Objects)
Located in `Infrastructure/Services/IAIService.cs`.

```csharp
public class DietitianSupportResponse
{
    [JsonPropertyName("analysis_summary")]
    public string AnalysisSummary { get; set; }

    [JsonPropertyName("nutrition_comment")]
    public string NutritionComment { get; set; }

    [JsonPropertyName("daily_recommendations")]
    public List<string> DailyRecommendations { get; set; }

    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; }

    [JsonPropertyName("dietitian_note_suggestion")]
    public string DietitianNoteSuggestion { get; set; }
}
```

### 2. Service Layer (`GeminiAIService.cs`)
- **Method:** `GetStructuredAnalysisAsync(string context)`
- **System Prompt:** Enforces the role and JSON schema.
- **JSON Parsing:** Uses `System.Text.Json` to deserialize the response safely.
- **Fallback:** Returns a safe default object if the API fails.

### 3. UI Integration (`FrmReports.cs`)
- **Asynchronous Loading:** AI analysis runs in the background (`CreateAIPanelAsync`).
- **Structured Display:** Fields are displayed in distinct sections (Summary, Nutrition, etc.).
- **Visual Cues:** Different colors for different types of information (Info, Success, Warning).
- **Response Caching:** `_lastAiResponse` field stores the last analysis for PDF export.

### 4. PDF Export Integration
- **AI Recommendations in PDF:** The `GenerateHtmlReport` method includes AI analysis results.
- **Styled Cards:** Each AI section (Summary, Nutrition, Recommendations, Warnings) is styled with distinct colors.
- **Conditional Display:** AI section only appears if analysis was successful (not error state).

## System Prompt
The following prompt is sent to the AI for every request:

```text
You are an autonomous dietitian support assistant.
Your role is to interpret data and generate supportive text.
You are NOT a doctor. You do NOT diagnose or treat.
You do NOT make final decisions.
You do NOT calculate BMI, calories, or targets.
Output MUST be valid JSON matching this schema:
{
  "analysis_summary": "string",
  "nutrition_comment": "string",
  "daily_recommendations": ["string"],
  "warnings": ["string"],
  "dietitian_note_suggestion": "string"
}
Ensure neutral, professional language.
Do not include any markdown formatting (like ```json). Just the raw JSON.
```

## Example Usage

### Request
```json
{
  "model": "google/gemini-2.0-flash-exp:free",
  "messages": [
    { "role": "system", "content": "...system prompt..." },
    { "role": "user", "content": "PATIENT DATA:\nName: Ahmet Yılmaz\nBMI: 24.9\n..." }
  ],
  "response_format": { "type": "json_object" }
}
```

### Response Handling (C#)
```csharp
var response = await aiService.GetStructuredAnalysisAsync(patientId);
lblSummary.Text = response.AnalysisSummary;
// ... bind other fields ...
```

## Security & Safety
- **Prompt Injection:** The system prompt is prepended and cannot be overridden by user data (which is sent as `user` role).
- **API Key:** Stored securely in the service (not exposed to frontend).
- **Fail-Safe:** `try-catch` blocks ensure the application continues even if AI fails.
