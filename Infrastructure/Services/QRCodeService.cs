using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// QR Kod Servisi
    /// Hasta erişimi, menü QR kodları, randevu QR kodları
    /// </summary>
    public class QRCodeService
    {
        /// <summary>
        /// QR kod oluştur
        /// Not: QRCoder NuGet paketi gereklidir (packages.config'e eklenmeli)
        /// Şimdilik placeholder QR kod döndürür
        /// </summary>
        public Image GenerateQRCode(string data, int size = 300)
        {
            // QRCoder kütüphanesi eklenince aktif edilecek:
            /*
            try
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new QRCode(qrCodeData))
                    {
                        return qrCode.GetGraphic(20);
                    }
                }
            }
            catch
            {
                return CreatePlaceholderQR(size, data);
            }
            */
            
            // Şimdilik placeholder
            return CreatePlaceholderQR(size, data);
        }

        /// <summary>
        /// Hasta erişim QR kodu oluştur
        /// </summary>
        public Image GeneratePatientAccessQR(int patientId, string baseUrl = "dietpro://patient/")
        {
            var data = $"{baseUrl}{patientId}";
            return GenerateQRCode(data);
        }

        /// <summary>
        /// Menü QR kodu oluştur
        /// </summary>
        public Image GenerateMenuQR(int patientId, DateTime weekStart, string baseUrl = "dietpro://menu/")
        {
            var data = $"{baseUrl}{patientId}/{weekStart:yyyyMMdd}";
            return GenerateQRCode(data);
        }

        /// <summary>
        /// Randevu QR kodu oluştur
        /// </summary>
        public Image GenerateAppointmentQR(int appointmentId, string baseUrl = "dietpro://appointment/")
        {
            var data = $"{baseUrl}{appointmentId}";
            return GenerateQRCode(data);
        }

        /// <summary>
        /// QR kodu dosyaya kaydet
        /// </summary>
        public string SaveQRCode(Image qrImage, string fileName = null)
        {
            try
            {
                var qrDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QRCodes");
                if (!Directory.Exists(qrDirectory))
                {
                    Directory.CreateDirectory(qrDirectory);
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = $"{Guid.NewGuid()}.png";
                }

                var filePath = Path.Combine(qrDirectory, fileName);
                qrImage.Save(filePath, ImageFormat.Png);

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"QR kod kaydetme hatası: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// QR kodu oku (opsiyonel - gelecek özellik)
        /// </summary>
        public string ReadQRCode(string imagePath)
        {
            // QR kod okuma özelliği gelecekte eklenebilir
            // ZXing.Net veya benzeri kütüphane gerekli
            throw new NotImplementedException("QR kod okuma özelliği henüz eklenmedi");
        }

        private Image CreatePlaceholderQR(int size, string data = "")
        {
            var bitmap = new Bitmap(size, size);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                
                // Border
                using (var pen = new Pen(Color.Black, 2))
                {
                    graphics.DrawRectangle(pen, 5, 5, size - 10, size - 10);
                }
                
                // QR kod benzeri kare desen
                var squareSize = size / 10;
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if ((i + j) % 2 == 0)
                        {
                            graphics.FillRectangle(Brushes.Black, i * squareSize, j * squareSize, squareSize, squareSize);
                        }
                    }
                }
                
                // Data bilgisi
                if (!string.IsNullOrEmpty(data))
                {
                    var font = new Font("Arial", 8);
                    var text = data.Length > 30 ? data.Substring(0, 30) + "..." : data;
                    graphics.DrawString(text, font, Brushes.Blue, new RectangleF(10, size - 30, size - 20, 20));
                }
                
                graphics.DrawString("QR Code\n(QRCoder NuGet required)", 
                    new Font("Arial", 10, FontStyle.Bold), Brushes.Red, new RectangleF(10, size / 2 - 20, size - 20, 40));
            }
            return bitmap;
        }
    }
}
