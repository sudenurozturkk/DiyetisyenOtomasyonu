using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using DevExpress.XtraEditors;

namespace DiyetisyenOtomasyonu.Shared
{
    /// <summary>
    /// Modern Healthcare Sidebar - Gradient Teal Theme
    /// DevExpress tarzı profesyonel navigasyon
    /// </summary>
    public class ModernSidebar : PanelControl
    {
        private bool isExpanded = true;
        private const int CollapsedWidth = 70;
        private const int ExpandedWidth = 260;
        private Timer animationTimer;
        private int targetWidth;
        private List<SidebarItem> menuItems = new List<SidebarItem>();
        private string selectedKey = "home";
        
        // Future feature: Badge count for unread messages notification
        private int _badgeCount = 0;
        
        // Healthcare Gradient Theme
        private readonly Color SidebarBgStart = ColorTranslator.FromHtml("#0F766E");
        private readonly Color SidebarBgEnd = ColorTranslator.FromHtml("#0D9488");
        private readonly Color HeaderBg = Color.FromArgb(20, 0, 0, 0);
        private readonly Color ItemHover = Color.FromArgb(50, 255, 255, 255);
        private readonly Color ItemActive = Color.FromArgb(80, 255, 255, 255);
        private readonly Color TextNormal = Color.FromArgb(220, 255, 255, 255);
        private readonly Color TextActive = Color.White;
        
        // Menu Icons - Dinamik olarak AddMenuItem'dan alinan ikonlar
        private readonly Dictionary<string, string> MenuIcons = new Dictionary<string, string>();
        
        public PanelControl MenuPanel { get; private set; }
        public event EventHandler<string> MenuItemClicked;

        public ModernSidebar()
        {
            SetupUI();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Check if rectangle has valid dimensions before drawing gradient
            if (this.ClientRectangle.Width <= 0 || this.ClientRectangle.Height <= 0)
                return;
            
            // Gradient background
            using (var brush = new LinearGradientBrush(
                this.ClientRectangle,
                SidebarBgStart,
                SidebarBgEnd,
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void SetupUI()
        {
            this.Dock = DockStyle.Left;
            this.Width = ExpandedWidth;
            this.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.BackColor = SidebarBgStart;
            
            // Enable double buffering for smooth painting
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                         ControlStyles.AllPaintingInWmPaint | 
                         ControlStyles.UserPaint, true);
            
            animationTimer = new Timer { Interval = 12 };
            animationTimer.Tick += AnimationTimer_Tick;
            
            // IMPORTANT: Create MenuPanel FIRST with Dock.Fill
            CreateMenuPanel();
            // Then create Header with Dock.Top - this ensures header stays on top
            CreateHeader();
        }

        private void CreateHeader()
        {
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = HeaderBg
            };

            // Logo - resim olarak
            var logoPanel = new PictureBox
            {
                Size = new Size(44, 44),
                Location = new Point(12, 12),
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.Zoom,
                Name = "logoPanel"
            };
            
            // Logo resmini yükle
            try
            {
                string logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    logoPanel.Image = Image.FromFile(logoPath);
                }
                else
                {
                    // Fallback: DP yazısı çiz
                    logoPanel.Paint += (s, e) => {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        using (var brush = new SolidBrush(Color.White))
                        {
                            e.Graphics.FillEllipse(brush, 0, 0, 43, 43);
                        }
                        using (var brush = new SolidBrush(SidebarBgStart))
                        using (var font = new Font("Segoe UI", 14F, FontStyle.Bold))
                        using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        {
                            e.Graphics.DrawString("DP", font, brush, new RectangleF(0, 0, 44, 44), sf);
                        }
                    };
                }
            }
            catch
            {
                // Fallback: DP yazısı çiz
                logoPanel.Paint += (s, e) => {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var brush = new SolidBrush(Color.White))
                    {
                        e.Graphics.FillEllipse(brush, 0, 0, 43, 43);
                    }
                    using (var brush = new SolidBrush(SidebarBgStart))
                    using (var font = new Font("Segoe UI", 14F, FontStyle.Bold))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        e.Graphics.DrawString("DP", font, brush, new RectangleF(0, 0, 44, 44), sf);
                    }
                };
            }
            headerPanel.Controls.Add(logoPanel);

            // Brand name
            var lblBrand = new Label
            {
                Text = "DiyetPro",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(62, 12),
                AutoSize = false,
                Size = new Size(130, 26),
                Name = "lblBrand",
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(lblBrand);

            var lblSubtitle = new Label
            {
                Text = "Profesyonel Takip Sistemi",
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(180, 255, 255, 255),
                Location = new Point(62, 38),
                AutoSize = false,
                Size = new Size(160, 18),
                Name = "lblSubtitle",
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(lblSubtitle);

            // Toggle button
            var btnToggle = new Label
            {
                Text = "☰",
                Font = new Font("Segoe UI", 14F),
                ForeColor = Color.White,
                Size = new Size(28, 28),
                Location = new Point(ExpandedWidth - 38, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Name = "btnToggle",
                BackColor = Color.Transparent
            };
            btnToggle.Click += (s, e) => ToggleSidebar();
            btnToggle.MouseEnter += (s, e) => btnToggle.ForeColor = Color.FromArgb(200, 255, 255, 255);
            btnToggle.MouseLeave += (s, e) => btnToggle.ForeColor = Color.White;
            headerPanel.Controls.Add(btnToggle);

            this.Controls.Add(headerPanel);
        }

        private void CreateMenuPanel()
        {
            MenuPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.Transparent,
                Padding = new Padding(10, 5, 10, 10)
            };
            this.Controls.Add(MenuPanel);
        }

        public void AddMenuItem(string icon, string text, string key)
        {
            var item = new SidebarItem { Icon = icon, Text = text, Key = key };
            menuItems.Add(item);
            
            // İkonu dinamik olarak kaydet
            MenuIcons[key] = icon;
            string iconText = icon;
            bool isSelected = key == selectedKey;

            var btn = new SimpleButton
            {
                Text = isExpanded ? $"   {iconText}    {text}" : iconText,
                Tag = key,
                Height = 46,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 2, 0, 2),
                Font = new Font("Segoe UI", 11F, isSelected ? FontStyle.Bold : FontStyle.Regular),
                Name = "btn_" + key,
                AllowFocus = false,
                Appearance = {
                    BackColor = isSelected ? ItemActive : Color.Transparent,
                    ForeColor = isSelected ? TextActive : TextNormal,
                    BorderColor = Color.Transparent,
                    TextOptions = { 
                        HAlignment = isExpanded ? DevExpress.Utils.HorzAlignment.Near : DevExpress.Utils.HorzAlignment.Center 
                    }
                }
            };
            
            // Rounded appearance
            btn.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
            btn.LookAndFeel.UseDefaultLookAndFeel = false;
            btn.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            
            // Hover effect
            btn.MouseEnter += (s, e) => {
                if ((string)btn.Tag != selectedKey)
                {
                    btn.Appearance.BackColor = ItemHover;
                    btn.Cursor = Cursors.Hand;
                }
            };
            
            btn.MouseLeave += (s, e) => {
                if ((string)btn.Tag != selectedKey)
                {
                    btn.Appearance.BackColor = Color.Transparent;
                }
            };
            
            btn.Click += (s, e) => {
                selectedKey = key;
                UpdateButtonStyles();
                MenuItemClicked?.Invoke(this, key);
            };

            // Separator before logout
            if (key == "logout")
            {
                var spacer = new Panel { Height = 20, Dock = DockStyle.Top, BackColor = Color.Transparent };
                MenuPanel.Controls.Add(spacer);
                spacer.BringToFront();
                
                var separator = new Panel
                {
                    Height = 1,
                    Dock = DockStyle.Top,
                    BackColor = Color.FromArgb(60, 255, 255, 255),
                    Margin = new Padding(15, 0, 15, 0)
                };
                MenuPanel.Controls.Add(separator);
                separator.BringToFront();
                
                var spacer2 = new Panel { Height = 15, Dock = DockStyle.Top, BackColor = Color.Transparent };
                MenuPanel.Controls.Add(spacer2);
                spacer2.BringToFront();
            }

            MenuPanel.Controls.Add(btn);
            btn.BringToFront();
        }
        
        public void AddSectionHeader(string title)
        {
            var lblSection = new Label
            {
                Text = isExpanded ? $"  {title.ToUpper()}" : "—",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 255, 255, 255),
                Height = 30,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(5, 0, 0, 5),
                BackColor = Color.Transparent,
                Name = "section_" + title.Replace(" ", "")
            };
            
            MenuPanel.Controls.Add(lblSection);
            lblSection.BringToFront();
        }

        public void SetBadgeCount(string menuKey, int count)
        {
            // For future badge implementation
            _badgeCount = count;
        }

        private void UpdateButtonStyles()
        {
            foreach (Control ctrl in MenuPanel.Controls)
            {
                if (ctrl is SimpleButton btn && btn.Tag != null)
                {
                    string key = btn.Tag.ToString();
                    
                    if (key == selectedKey)
                    {
                        btn.Appearance.BackColor = ItemActive;
                        btn.Appearance.ForeColor = TextActive;
                        btn.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                    }
                    else
                    {
                        btn.Appearance.BackColor = Color.Transparent;
                        btn.Appearance.ForeColor = TextNormal;
                        btn.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
                    }
                }
            }
        }
        
        private void ToggleSidebar()
        {
            isExpanded = !isExpanded;
            targetWidth = isExpanded ? ExpandedWidth : CollapsedWidth;
            animationTimer.Start();
        }
        
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            int step = 30;
            
            if (Math.Abs(this.Width - targetWidth) <= step)
            {
                this.Width = targetWidth;
                animationTimer.Stop();
                UpdateMenuTexts();
            }
            else
            {
                this.Width += this.Width < targetWidth ? step : -step;
            }
            
            // Update toggle button position
            var btnToggle = this.Controls.Find("btnToggle", true);
            if (btnToggle.Length > 0)
            {
                // Center toggle button when collapsed, move to right when expanded
                if (targetWidth == CollapsedWidth) // Collapsing
                {
                     btnToggle[0].Left = (this.Width - btnToggle[0].Width) / 2;
                }
                else // Expanding
                {
                     btnToggle[0].Left = this.Width - 45;
                }
            }
        }
        
        private void UpdateMenuTexts()
        {
            // Header labels
            var lblBrand = this.Controls.Find("lblBrand", true);
            var lblSubtitle = this.Controls.Find("lblSubtitle", true);
            var logoPanel = this.Controls.Find("logoPanel", true); // Find logo panel

            if (lblBrand.Length > 0) lblBrand[0].Visible = isExpanded;
            if (lblSubtitle.Length > 0) lblSubtitle[0].Visible = isExpanded;
            if (logoPanel.Length > 0) logoPanel[0].Visible = isExpanded; // Hide logo when collapsed
            
            // Menu buttons
            foreach (Control ctrl in MenuPanel.Controls)
            {
                if (ctrl is SimpleButton btn && btn.Tag != null)
                {
                    string key = btn.Tag.ToString();
                    var item = menuItems.Find(m => m.Key == key);
                    if (item != null)
                    {
                        string iconText = MenuIcons.ContainsKey(key) ? MenuIcons[key] : "•";
                        btn.Text = isExpanded ? $"   {iconText}    {item.Text}" : iconText;
                        btn.Appearance.TextOptions.HAlignment = isExpanded 
                            ? DevExpress.Utils.HorzAlignment.Near 
                            : DevExpress.Utils.HorzAlignment.Center;
                    }
                }
                else if (ctrl is Label lbl && lbl.Name.StartsWith("section_"))
                {
                    lbl.Text = isExpanded ? $"  {lbl.Name.Replace("section_", "").ToUpper()}" : "—";
                }
            }
        }
        
        public void SetSelectedMenu(string key)
        {
            selectedKey = key;
            UpdateButtonStyles();
        }
        
        public void SetPanelTitle(string title)
        {
            var lblSubtitle = this.Controls.Find("lblSubtitle", true);
            if (lblSubtitle.Length > 0)
            {
                ((Label)lblSubtitle[0]).Text = title;
            }
        }

        private class SidebarItem
        {
            public string Icon { get; set; }
            public string Text { get; set; }
            public string Key { get; set; }
        }
    }
}
