using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DiyetisyenOtomasyonu.Shared
{
    /// <summary>
    /// Klavye Kısayolları Yöneticisi
    /// Global ve form bazlı kısayol tuşları
    /// </summary>
    public static class KeyboardShortcuts
    {
        private static Dictionary<Keys, Action> _globalShortcuts = new Dictionary<Keys, Action>();
        private static Dictionary<Form, Dictionary<Keys, Action>> _formShortcuts = new Dictionary<Form, Dictionary<Keys, Action>>();

        /// <summary>
        /// Global kısayol kaydet
        /// </summary>
        public static void RegisterGlobalShortcut(Keys key, Action action)
        {
            _globalShortcuts[key] = action;
        }

        /// <summary>
        /// Form bazlı kısayol kaydet
        /// </summary>
        public static void RegisterFormShortcut(Form form, Keys key, Action action)
        {
            if (!_formShortcuts.ContainsKey(form))
            {
                _formShortcuts[form] = new Dictionary<Keys, Action>();
            }
            _formShortcuts[form][key] = action;
        }

        /// <summary>
        /// KeyDown event handler - Form'a eklenmeli
        /// </summary>
        public static void HandleKeyDown(Form form, KeyEventArgs e)
        {
            // Form bazlı kısayolları kontrol et
            if (_formShortcuts.ContainsKey(form) && _formShortcuts[form].ContainsKey(e.KeyData))
            {
                _formShortcuts[form][e.KeyData].Invoke();
                e.Handled = true;
                return;
            }

            // Global kısayolları kontrol et
            if (_globalShortcuts.ContainsKey(e.KeyData))
            {
                _globalShortcuts[e.KeyData].Invoke();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Varsayılan kısayolları kaydet
        /// </summary>
        public static void RegisterDefaultShortcuts()
        {
            // Global kısayollar
            RegisterGlobalShortcut(Keys.F1, () => ShowHelp());
            RegisterGlobalShortcut(Keys.Control | Keys.F, () => ShowSearch());
            RegisterGlobalShortcut(Keys.Escape, () => CloseCurrentForm());
        }

        private static void ShowHelp()
        {
            MessageBox.Show("Yardım menüsü yakında eklenecek.", "Yardım", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void ShowSearch()
        {
            // Arama penceresi açılacak
            MessageBox.Show("Arama özelliği yakında eklenecek.", "Arama", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void CloseCurrentForm()
        {
            if (Application.OpenForms.Count > 0)
            {
                var activeForm = Form.ActiveForm;
                if (activeForm != null && activeForm.Modal == false)
                {
                    activeForm.Close();
                }
            }
        }

        /// <summary>
        /// Kısayol açıklaması al
        /// </summary>
        public static string GetShortcutDescription(Keys key)
        {
            return key switch
            {
                Keys.F1 => "Yardım",
                Keys.Control | Keys.F => "Ara",
                Keys.Control | Keys.N => "Yeni",
                Keys.Control | Keys.S => "Kaydet",
                Keys.Control | Keys.P => "Yazdır",
                Keys.Escape => "Kapat",
                _ => key.ToString()
            };
        }
    }
}
