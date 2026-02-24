using System;
using System.Windows.Forms;
using DiyetisyenOtomasyonu.Bootstrap;
using DiyetisyenOtomasyonu.Forms.Login;
using DiyetisyenOtomasyonu.Forms.Doctor;
using DiyetisyenOtomasyonu.Forms.Patient;
using DiyetisyenOtomasyonu.Infrastructure.Database;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.DI;
using DiyetisyenOtomasyonu.Infrastructure.Exceptions;

namespace DiyetisyenOtomasyonu
{
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
                    GlobalExceptionHandler.Handle(e.Exception, "Application.ThreadException");
                };

                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    var ex = e.ExceptionObject as Exception;
                    GlobalExceptionHandler.Handle(ex, "AppDomain.UnhandledException");
                };

                // DevExpress tema ayarları
                ThemeBootstrapper.Apply();

                // Tema yöneticisi - Dark/Light mode
                ThemeManager.LoadTheme();

                // Klavye kısayolları
                KeyboardShortcuts.RegisterDefaultShortcuts();

                // Veritabanı başlatma (MySQL)
                InitializeDatabase();

                // Dependency Injection Container başlatma
                InitializeDependencyInjection();

                // Logger servisi başlatma
                LoggerService.Info("Uygulama başlatıldı", "Program.Main");

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
        /// Dependency Injection Container'ı başlat
        /// </summary>
        private static void InitializeDependencyInjection()
        {
            var container = ServiceContainer.Instance;
            ServiceRegistration.RegisterServices(container);
            LoggerService.Info("Dependency Injection Container başlatıldı", "Program.InitializeDI");
        }

        /// <summary>
        /// Hataları loglar. LoggerService kullanır.
        /// </summary>
        private static void LogError(string context, Exception ex)
        {
            LoggerService.Error($"{context}: {ex?.Message}", ex, "Program");
        }
    }
}
