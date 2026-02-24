using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// Fotoğraf Yönetim Servisi
    /// Profil fotoğrafları, ilerleme fotoğrafları, görsel optimizasyon
    /// </summary>
    public class PhotoService
    {
        private static string _photoDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Photos");
        private const int MAX_PHOTO_SIZE_KB = 500; // Maksimum fotoğraf boyutu
        private const int THUMBNAIL_SIZE = 200; // Thumbnail boyutu

        static PhotoService()
        {
            if (!Directory.Exists(_photoDirectory))
            {
                Directory.CreateDirectory(_photoDirectory);
            }
        }

        /// <summary>
        /// Fotoğraf seç ve yükle
        /// </summary>
        public string SelectAndUploadPhoto(string fileName = null)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*";
                dialog.Title = "Fotoğraf Seç";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return UploadPhoto(dialog.FileName, fileName);
                }
            }

            return null;
        }

        /// <summary>
        /// Fotoğraf yükle ve optimize et
        /// </summary>
        public string UploadPhoto(string sourcePath, string fileName = null)
        {
            try
            {
                if (!File.Exists(sourcePath))
                    throw new FileNotFoundException("Kaynak dosya bulunamadı");

                // Dosya adı oluştur
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = $"{Guid.NewGuid()}.jpg";
                }

                var destinationPath = Path.Combine(_photoDirectory, fileName);

                // Fotoğrafı yükle ve optimize et
                using (var image = Image.FromFile(sourcePath))
                {
                    var optimizedImage = OptimizeImage(image);
                    optimizedImage.Save(destinationPath, ImageFormat.Jpeg);
                }

                return fileName;
            }
            catch (Exception ex)
            {
                throw new Exception($"Fotoğraf yükleme hatası: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Görseli optimize et (boyut ve kalite)
        /// </summary>
        private Image OptimizeImage(Image image)
        {
            // Maksimum boyutlar
            int maxWidth = 1920;
            int maxHeight = 1080;

            // Boyut kontrolü ve yeniden boyutlandırma
            if (image.Width > maxWidth || image.Height > maxHeight)
            {
                var ratio = Math.Min((double)maxWidth / image.Width, (double)maxHeight / image.Height);
                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                var resized = new Bitmap(newWidth, newHeight);
                using (var graphics = Graphics.FromImage(resized))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                }
                image.Dispose();
                image = resized;
            }

            return image;
        }

        /// <summary>
        /// Thumbnail oluştur
        /// </summary>
        public string CreateThumbnail(string photoFileName)
        {
            try
            {
                var photoPath = Path.Combine(_photoDirectory, photoFileName);
                if (!File.Exists(photoPath))
                    return null;

                var thumbnailFileName = $"thumb_{photoFileName}";
                var thumbnailPath = Path.Combine(_photoDirectory, thumbnailFileName);

                using (var image = Image.FromFile(photoPath))
                {
                    var thumbnail = image.GetThumbnailImage(THUMBNAIL_SIZE, THUMBNAIL_SIZE, null, IntPtr.Zero);
                    thumbnail.Save(thumbnailPath, ImageFormat.Jpeg);
                }

                return thumbnailFileName;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Fotoğraf yükle (byte array)
        /// </summary>
        public string UploadPhoto(byte[] photoData, string fileName = null)
        {
            try
            {
                if (photoData == null || photoData.Length == 0)
                    throw new ArgumentException("Fotoğraf verisi boş");

                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = $"{Guid.NewGuid()}.jpg";
                }

                var destinationPath = Path.Combine(_photoDirectory, fileName);

                using (var ms = new MemoryStream(photoData))
                using (var image = Image.FromStream(ms))
                {
                    var optimizedImage = OptimizeImage(image);
                    optimizedImage.Save(destinationPath, ImageFormat.Jpeg);
                }

                return fileName;
            }
            catch (Exception ex)
            {
                throw new Exception($"Fotoğraf yükleme hatası: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Fotoğraf sil
        /// </summary>
        public void DeletePhoto(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return;

                var photoPath = Path.Combine(_photoDirectory, fileName);
                if (File.Exists(photoPath))
                {
                    File.Delete(photoPath);
                }

                // Thumbnail'i de sil
                var thumbnailPath = Path.Combine(_photoDirectory, $"thumb_{fileName}");
                if (File.Exists(thumbnailPath))
                {
                    File.Delete(thumbnailPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fotoğraf silme hatası: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Fotoğraf yolu al
        /// </summary>
        public string GetPhotoPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            var path = Path.Combine(_photoDirectory, fileName);
            return File.Exists(path) ? path : null;
        }

        /// <summary>
        /// Fotoğraf yükle (Image olarak)
        /// </summary>
        public Image LoadPhoto(string fileName)
        {
            try
            {
                var path = GetPhotoPath(fileName);
                if (path == null)
                    return null;

                return Image.FromFile(path);
            }
            catch
            {
                return null;
            }
        }
    }
}
