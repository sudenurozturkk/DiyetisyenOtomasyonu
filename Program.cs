using System;
using System.Windows.Forms;
using DiyetisyenOtomasyonu.Bootstrap;
using DiyetisyenOtomasyonu.Forms.Login;
using DiyetisyenOtomasyonu.Forms.Doctor;
using DiyetisyenOtomasyonu.Forms.Patient;
using DiyetisyenOtomasyonu.Infrastructure.Database;
using DiyetisyenOtomasyonu.Infrastructure.Security;

namespace DiyetisyenOtomasyonu
{
    /// <summary>
    /// Uygulama giriş noktası
    /// 
    /// Academic: Application entry point with proper initialization
    /// - Theme bootstrapping
    /// - Database initialization
    /// - Authentication flow
    /// - Role-based navigation
    /// </summary>
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // Windows Forms temel ayarları
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Global hata yakalama
                Application.ThreadException += (s, e) =>
                {
                    LogError("Thread Exception", e.Exception);
                    ShowErrorMessage("Uygulama Hatası", e.Exception);
                };

                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    var ex = e.ExceptionObject as Exception;
                    LogError("Unhandled Exception", ex);
                    ShowErrorMessage("Kritik Hata", ex);
                };

                // DevExpress tema ayarları
                ThemeBootstrapper.Apply();

                // Veritabanı başlatma (MySQL)
                InitializeDatabase();

                // Ana uygulama döngüsü
                RunApplicationLoop();
            }
            catch (Exception ex)
            {
                LogError("Startup Exception", ex);
                ShowErrorMessage("Başlangıç Hatası", ex);
            }
        }

        /// <summary>
        /// Veritabanını başlatır
        /// </summary>
        private static void InitializeDatabase()
        {
            try
            {
                // MySQL veritabanını başlat ve tabloları oluştur
                DatabaseInitializer.Initialize();
                
                // Demo veri ekle (sadece bir kez - varsa atlanır)
                Infrastructure.Services.DemoDataSeeder.SeedDemoData();
            }
            catch (Exception ex)
            {
                throw new Exception("Veritabanı başlatılamadı: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Ana uygulama döngüsü - Splash, Login ve ana form yönetimi
        /// </summary>
        private static void RunApplicationLoop()
        {
            bool keepRunning = true;

            // Splash ekranını göster
            using (var splashForm = new FrmSplash())
            {
                splashForm.ShowDialog();
            }

            while (keepRunning)
            {
                // Giriş formunu göster
                using (var loginForm = new FrmLogin())
                {
                    var result = loginForm.ShowDialog();

                    if (result == DialogResult.OK && AuthContext.IsAuthenticated)
                    {
                        // Kullanıcı rolüne göre ana formu aç
                        Form mainForm = null;

                        if (AuthContext.IsDoctor())
                        {
                            mainForm = new FrmDoctorShell();
                        }
                        else if (AuthContext.IsPatient())
                        {
                            mainForm = new FrmPatientShell();
                        }

                        if (mainForm != null)
                        {
                            Application.Run(mainForm);
                            mainForm.Dispose();
                        }

                        // Ana form kapatıldığında oturumu kapat
                        AuthContext.SignOut();
                    }
                    else
                    {
                        // Kullanıcı giriş yapmadı, uygulamayı kapat
                        keepRunning = false;
                    }
                }
            }
        }

        /// <summary>
        /// Hata mesajını gösterir
        /// </summary>
        private static void ShowErrorMessage(string title, Exception ex)
        {
            string message = ex?.Message ?? "Bilinmeyen hata";

#if DEBUG
            message += "\n\n" + (ex?.StackTrace ?? "");
#endif

            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Hataları loglar. Debug modunda konsola yazar, production'da dosyaya yazılabilir.
        /// </summary>
        private static void LogError(string context, Exception ex)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logMessage = $"[{timestamp}] {context}: {ex?.Message}";
            
#if DEBUG
            System.Diagnostics.Debug.WriteLine(logMessage);
            if (ex?.StackTrace != null)
            {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
#else
            // Production'da dosyaya loglama eklenebilir
            // Örnek: File.AppendAllText("logs/error.log", logMessage + Environment.NewLine);
#endif
        }
    }
}
