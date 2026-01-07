using System;

namespace DiyetisyenOtomasyonu.Domain
{
    public class AiChatMessage
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public string Message { get; set; }
        public bool IsAiResponse { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
