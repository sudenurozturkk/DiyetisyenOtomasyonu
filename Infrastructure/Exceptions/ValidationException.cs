using System;
using System.Collections.Generic;
using System.Linq;

namespace DiyetisyenOtomasyonu.Infrastructure.Exceptions
{
    /// <summary>
    /// Validasyon hatası exception
    /// </summary>
    public class ValidationException : AppException
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException(string message) : base("VALIDATION_ERROR", message)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(Dictionary<string, string[]> errors) : base("VALIDATION_ERROR", "Validasyon hatası")
        {
            Errors = errors ?? new Dictionary<string, string[]>();
        }

        public ValidationException(string property, string error) : base("VALIDATION_ERROR", "Validasyon hatası")
        {
            Errors = new Dictionary<string, string[]>
            {
                { property, new[] { error } }
            };
        }

        public override string Message
        {
            get
            {
                if (Errors.Count == 0)
                    return base.Message;

                var errorMessages = Errors.SelectMany(e => e.Value.Select(v => $"{e.Key}: {v}"));
                return string.Join(Environment.NewLine, errorMessages);
            }
        }
    }
}
