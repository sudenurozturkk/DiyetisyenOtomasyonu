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
                System.IO.File.WriteAllText("startup_log.txt", "Starting application...\n");
                
                // Windows Forms temel ayarları
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                System.IO.File.AppendAllText("startup_log.txt", "Visual styles enabled\n");

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
                System.IO.File.AppendAllText("startup_log.txt", "Applying theme...\n");
                ThemeBootstrapper.Apply();
                System.IO.File.AppendAllText("startup_log.txt", "Theme applied\n");

                // Veritabanı başlatma (MySQL)
                System.IO.File.AppendAllText("startup_log.txt", "Initializing database...\n");
                InitializeDatabase();
                System.IO.File.AppendAllText("startup_log.txt", "Database initialized\n");

                // Ana uygulama döngüsü
                System.IO.File.AppendAllText("startup_log.txt", "Running application loop...\n");
                RunApplicationLoop();
                System.IO.File.AppendAllText("startup_log.txt", "Application loop ended\n");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("startup_log.txt", "ERROR: " + ex.ToString() + "\n");
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
        /// Hatayı loglar (gelecekte dosyaya yazılabilir)
        /// </summary>
        private static void LogError(string context, Exception ex)
        {
            // TODO: Dosyaya veya veritabanına loglama eklenebilir
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}: {ex?.Message}");
            System.Diagnostics.Debug.WriteLine(ex?.StackTrace);
        }
    }
}
