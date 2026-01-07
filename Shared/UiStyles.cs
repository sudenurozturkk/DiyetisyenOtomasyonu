using System.Drawing;
using System.Drawing.Drawing2D;

namespace DiyetisyenOtomasyonu.Shared
{
    /// <summary>
    /// Modern Healthcare UI Design System
    /// DevExpress tarzı profesyonel tasarım
    /// </summary>
    public static class UiStyles
    {
        // ============================================
        // ANA RENK PALETİ - Healthcare Theme
        // ============================================
        
        // Primary - Teal (Sağlık/Güven)
        public static readonly Color PrimaryColor = ColorTranslator.FromHtml("#0F766E");
        public static readonly Color PrimaryLight = ColorTranslator.FromHtml("#14B8A6");
        public static readonly Color PrimaryDark = ColorTranslator.FromHtml("#0D5D56");
        
        // Secondary - Indigo (Profesyonel)
        public static readonly Color SecondaryColor = ColorTranslator.FromHtml("#6366F1");
        public static readonly Color SecondaryLight = ColorTranslator.FromHtml("#818CF8");
        
        // Status Colors
        public static readonly Color SuccessColor = ColorTranslator.FromHtml("#22C55E");
        public static readonly Color SuccessLight = ColorTranslator.FromHtml("#BBF7D0");
        public static readonly Color WarningColor = ColorTranslator.FromHtml("#F59E0B");
        public static readonly Color WarningLight = ColorTranslator.FromHtml("#FEF3C7");
        public static readonly Color DangerColor = ColorTranslator.FromHtml("#EF4444");
        public static readonly Color DangerLight = ColorTranslator.FromHtml("#FEE2E2");
        public static readonly Color InfoColor = ColorTranslator.FromHtml("#3B82F6");
        public static readonly Color InfoLight = ColorTranslator.FromHtml("#DBEAFE");
        
        // Background & Surface
        public static readonly Color BackgroundColor = ColorTranslator.FromHtml("#F8FAFC");
        public static readonly Color CardBackgroundColor = Color.White;
        public static readonly Color SurfaceColor = ColorTranslator.FromHtml("#F1F5F9");
        public static readonly Color BorderColor = ColorTranslator.FromHtml("#E2E8F0");
        public static readonly Color DividerColor = ColorTranslator.FromHtml("#CBD5E1");
        
        // Text Colors
        public static readonly Color TextPrimary = ColorTranslator.FromHtml("#1E293B");
        public static readonly Color TextSecondary = ColorTranslator.FromHtml("#64748B");
        public static readonly Color TextMuted = ColorTranslator.FromHtml("#94A3B8");
        public static readonly Color TextOnPrimary = Color.White;
        
        // Sidebar Colors
        public static readonly Color SidebarBackground = ColorTranslator.FromHtml("#0F766E");
        public static readonly Color SidebarGradientStart = ColorTranslator.FromHtml("#0F766E");
        public static readonly Color SidebarGradientEnd = ColorTranslator.FromHtml("#0D9488");
        public static readonly Color SidebarItemHover = Color.FromArgb(40, 255, 255, 255);
        public static readonly Color SidebarItemActive = Color.FromArgb(60, 255, 255, 255);
        public static readonly Color SidebarText = Color.FromArgb(220, 255, 255, 255);
        public static readonly Color SidebarTextActive = Color.White;
        
        // Grid Colors
        public static readonly Color GridRowColor = ColorTranslator.FromHtml("#F8FAFC");
        public static readonly Color GridRowAltColor = Color.White;
        public static readonly Color GridHeaderColor = ColorTranslator.FromHtml("#F1F5F9");
        
        // Shadow & Effects
        public static readonly Color ShadowColor = Color.FromArgb(20, 0, 0, 0);
        public static readonly Color HoverColor = ColorTranslator.FromHtml("#F1F5F9");
        public static readonly Color ActiveColor = ColorTranslator.FromHtml("#E2E8F0");
        
        // ============================================
        // FONTLAR
        // ============================================
        public static readonly Font TitleFont = new Font("Segoe UI", 24F, FontStyle.Bold);
        public static readonly Font HeaderFont = new Font("Segoe UI", 18F, FontStyle.Bold);
        public static readonly Font SubHeaderFont = new Font("Segoe UI", 14F, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Segoe UI", 11F, FontStyle.Regular);
        public static readonly Font BodyBoldFont = new Font("Segoe UI", 11F, FontStyle.Bold);
        public static readonly Font SmallFont = new Font("Segoe UI", 9F, FontStyle.Regular);
        public static readonly Font SmallBoldFont = new Font("Segoe UI", 9F, FontStyle.Bold);
        public static readonly Font CardValueFont = new Font("Segoe UI", 28F, FontStyle.Bold);
        public static readonly Font SidebarFont = new Font("Segoe UI", 11F, FontStyle.Regular);
        public static readonly Font SidebarFontBold = new Font("Segoe UI", 11F, FontStyle.Bold);
        
        // Legacy font names for compatibility
        public static readonly Font NormalFont = BodyFont;
        public static readonly Font BoldFont = BodyBoldFont;
        public static readonly Font SectionFont = HeaderFont;
        public static readonly Font LargeFont = SubHeaderFont;
        
        // ============================================
        // BOYUTLAR
        // ============================================
        public const int GridRowHeight = 40;
        public const int ButtonHeight = 36;
        public const int ButtonWidth = 110;
        public const int Spacing = 12;
        public const int LargeSpacing = 24;
        public const int CardBorderRadius = 12;
        public const int ButtonBorderRadius = 8;
        public const int SidebarWidth = 240;
        public const int SidebarCollapsedWidth = 60;
        public const int CardPadding = 20;
        public const int HeaderHeight = 60;
        
        // ============================================
        // ANİMASYON SÜRELERİ (ms)
        // ============================================
        public const int AnimationFast = 150;
        public const int AnimationNormal = 250;
        public const int AnimationSlow = 400;
        public const int ToastDuration = 3000;
        public const int ToastFadeIn = 200;
        public const int ToastFadeOut = 300;
        
        // ============================================
        // YARDIMCI METODLAR
        // ============================================
        
        /// <summary>
        /// Gradient brush oluşturur
        /// </summary>
        public static LinearGradientBrush CreateGradientBrush(Rectangle rect, Color startColor, Color endColor, float angle = 135f)
        {
            return new LinearGradientBrush(rect, startColor, endColor, angle);
        }
        
        /// <summary>
        /// Sidebar gradient brush oluşturur
        /// </summary>
        public static LinearGradientBrush CreateSidebarGradient(Rectangle rect)
        {
            return new LinearGradientBrush(rect, SidebarGradientStart, SidebarGradientEnd, 180f);
        }
        
        /// <summary>
        /// Rounded rectangle path oluşturur
        /// </summary>
        public static GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            
            return path;
        }
        
        /// <summary>
        /// Kart gölgesi çizer
        /// </summary>
        public static void DrawCardShadow(Graphics g, Rectangle rect, int shadowDepth = 3)
        {
            for (int i = shadowDepth; i > 0; i--)
            {
                using (var pen = new Pen(Color.FromArgb(10 * i, 0, 0, 0), 1))
                {
                    var shadowRect = new Rectangle(rect.X + i, rect.Y + i, rect.Width, rect.Height);
                    using (var path = CreateRoundedRectangle(shadowRect, CardBorderRadius))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }
        }
    }
}
