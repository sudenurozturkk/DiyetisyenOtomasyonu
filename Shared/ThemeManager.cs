using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;

namespace DiyetisyenOtomasyonu.Shared
{
    /// <summary>
    /// Tema Yöneticisi - Dark/Light Mode
    /// Modern tema sistemi, kullanıcı tercihleri
    /// </summary>
    public static class ThemeManager
    {
        private static ThemeMode _currentTheme = ThemeMode.Light;
        private const string THEME_SETTING_KEY = "ApplicationTheme";

        /// <summary>
        /// Mevcut tema
        /// </summary>
        public static ThemeMode CurrentTheme
        {
            get => _currentTheme;
            private set => _currentTheme = value;
        }

        /// <summary>
        /// Tema yükle (uygulama başlangıcında)
        /// </summary>
        public static void LoadTheme()
        {
            try
            {
                var savedTheme = Properties.Settings.Default[THEME_SETTING_KEY]?.ToString();
                if (Enum.TryParse<ThemeMode>(savedTheme, out var theme))
                {
                    ApplyTheme(theme);
                }
                else
                {
                    // Sistem temasına göre otomatik seç
                    ApplyTheme(IsSystemDarkMode() ? ThemeMode.Dark : ThemeMode.Light);
                }
            }
            catch
            {
                ApplyTheme(ThemeMode.Light);
            }
        }

        /// <summary>
        /// Tema uygula
        /// </summary>
        public static void ApplyTheme(ThemeMode mode)
        {
            _currentTheme = mode;

            try
            {
                if (mode == ThemeMode.Dark)
                {
                    UserLookAndFeel.Default.SetSkinStyle("Office 2019 Dark");
                }
                else
                {
                    UserLookAndFeel.Default.SetSkinStyle("Office 2019 Colorful");
                }

                // Tema tercihini kaydet
                Properties.Settings.Default[THEME_SETTING_KEY] = mode.ToString();
                Properties.Settings.Default.Save();

                // Tüm açık formları güncelle
                UpdateAllForms();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Theme apply error: {ex.Message}");
            }
        }

        /// <summary>
        /// Tema değiştir (toggle)
        /// </summary>
        public static void ToggleTheme()
        {
            var newTheme = _currentTheme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light;
            ApplyTheme(newTheme);
        }

        /// <summary>
        /// Tüm açık formları güncelle
        /// </summary>
        private static void UpdateAllForms()
        {
            foreach (Form form in Application.OpenForms)
            {
                form.Invalidate();
                form.Refresh();
            }
        }

        /// <summary>
        /// Sistem dark mode kontrolü
        /// </summary>
        private static bool IsSystemDarkMode()
        {
            try
            {
                // Windows 10/11 dark mode kontrolü
                var registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                
                if (registryKey != null)
                {
                    var appsUseLightTheme = registryKey.GetValue("AppsUseLightTheme");
                    if (appsUseLightTheme != null)
                    {
                        return (int)appsUseLightTheme == 0;
                    }
                }
            }
            catch
            {
                // Registry erişim hatası - varsayılan light mode
            }

            return false;
        }

        /// <summary>
        /// Tema renklerini al
        /// </summary>
        public static Color GetThemeColor(ThemeColorType colorType)
        {
            return _currentTheme == ThemeMode.Dark
                ? GetDarkModeColor(colorType)
                : GetLightModeColor(colorType);
        }

        private static Color GetDarkModeColor(ThemeColorType colorType)
        {
            return colorType switch
            {
                ThemeColorType.Background => Color.FromArgb(32, 32, 32),
                ThemeColorType.Surface => Color.FromArgb(40, 40, 40),
                ThemeColorType.Text => Color.FromArgb(255, 255, 255),
                ThemeColorType.TextSecondary => Color.FromArgb(200, 200, 200),
                ThemeColorType.Accent => Color.FromArgb(32, 178, 170), // Teal
                ThemeColorType.Success => Color.FromArgb(40, 167, 69),
                ThemeColorType.Warning => Color.FromArgb(255, 193, 7),
                ThemeColorType.Error => Color.FromArgb(220, 53, 69),
                _ => Color.FromArgb(32, 32, 32)
            };
        }

        private static Color GetLightModeColor(ThemeColorType colorType)
        {
            return colorType switch
            {
                ThemeColorType.Background => Color.FromArgb(255, 255, 255),
                ThemeColorType.Surface => Color.FromArgb(248, 249, 250),
                ThemeColorType.Text => Color.FromArgb(33, 37, 41),
                ThemeColorType.TextSecondary => Color.FromArgb(108, 117, 125),
                ThemeColorType.Accent => Color.FromArgb(32, 178, 170), // Teal
                ThemeColorType.Success => Color.FromArgb(40, 167, 69),
                ThemeColorType.Warning => Color.FromArgb(255, 193, 7),
                ThemeColorType.Error => Color.FromArgb(220, 53, 69),
                _ => Color.FromArgb(255, 255, 255)
            };
        }
    }

    /// <summary>
    /// Tema modları
    /// </summary>
    public enum ThemeMode
    {
        Light,
        Dark
    }

    /// <summary>
    /// Tema renk tipleri
    /// </summary>
    public enum ThemeColorType
    {
        Background,
        Surface,
        Text,
        TextSecondary,
        Accent,
        Success,
        Warning,
        Error
    }
}
