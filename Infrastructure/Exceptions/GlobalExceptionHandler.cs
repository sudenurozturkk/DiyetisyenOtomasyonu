using System;
using System.Windows.Forms;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Infrastructure.Exceptions
{
    /// <summary>
    /// Global exception handler
    /// Tüm hataları yakalar, loglar ve kullanıcıya gösterir
    /// </summary>
    public static class GlobalExceptionHandler
    {
        /// <summary>
        /// Exception'ı handle et
        /// </summary>
        public static void Handle(Exception exception, string context = "")
        {
            try
            {
                // Loglama
                LoggerService.Error($"Exception in {context}", exception, "GlobalExceptionHandler");

                // Kullanıcıya göster
                var userMessage = GetUserFriendlyMessage(exception);
                var title = GetExceptionTitle(exception);

                ToastNotification.ShowError($"{title}\n{userMessage}");

                // Debug modunda detaylı bilgi göster
#if DEBUG
                var detailedMessage = $"{userMessage}\n\nDetaylar:\n{exception.Message}\n\nStack Trace:\n{exception.StackTrace}";
                MessageBox.Show(detailedMessage, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
            }
            catch (Exception ex)
            {
                // Exception handler'da hata olursa sadece console'a yaz
                System.Diagnostics.Debug.WriteLine($"Error in GlobalExceptionHandler: {ex.Message}");
            }
        }

        /// <summary>
        /// Kullanıcı dostu mesaj al
        /// </summary>
        private static string GetUserFriendlyMessage(Exception exception)
        {
            return exception switch
            {
                ValidationException ve => ve.Message,
                DatabaseException de => "Veritabanı işlemi sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyin.",
                AppException ae => ae.Message,
                ArgumentException ae => $"Geçersiz parametre: {ae.Message}",
                ArgumentNullException ane => $"Eksik parametre: {ane.ParamName}",
                UnauthorizedAccessException uae => "Bu işlem için yetkiniz bulunmamaktadır.",
                TimeoutException te => "İşlem zaman aşımına uğradı. Lütfen tekrar deneyin.",
                _ => "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin."
            };
        }

        /// <summary>
        /// Exception başlığı al
        /// </summary>
        private static string GetExceptionTitle(Exception exception)
        {
            return exception switch
            {
                ValidationException => "Validasyon Hatası",
                DatabaseException => "Veritabanı Hatası",
                AppException => "Uygulama Hatası",
                ArgumentException => "Parametre Hatası",
                UnauthorizedAccessException => "Yetki Hatası",
                TimeoutException => "Zaman Aşımı",
                _ => "Hata"
            };
        }
    }
}
