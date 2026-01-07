using System.Drawing;
using DevExpress.LookAndFeel;
using DevExpress.Skins;

namespace DiyetisyenOtomasyonu.Bootstrap
{
    /// <summary>
    /// Tema ve görünüm ayarları
    /// </summary>
    public static class ThemeBootstrapper
    {
        /// <summary>
        /// Tema ayarlarını uygula
        /// </summary>
        public static void Apply()
        {
            // DevExpress teması - native Windows görünümü
            UserLookAndFeel.Default.SetSkinStyle(SkinStyle.WXI);
            
            // Form skinlerini etkinleştir
            SkinManager.EnableFormSkins();
            
            // Varsayılan font
            DevExpress.Utils.AppearanceObject.DefaultFont = new Font("Segoe UI", 10F);
        }
    }
}
