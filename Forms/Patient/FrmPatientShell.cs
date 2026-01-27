using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using System.Linq;

namespace DiyetisyenOtomasyonu.Forms.Patient
{
    public partial class FrmPatientShell : XtraForm
    {
        private ModernSidebar sidebar;
        private PanelControl mainContainer;
        private PanelControl topPanel;
        private PanelControl contentPanel;
        private LabelControl lblCurrentPage;
        private SimpleButton btnHome;
        private PatientRepository _patientRepo;

        // RENK PALETI - UiStyles'dan alınıyor
        private Color BackgroundColor => Color.FromArgb(245, 247, 250);
        private Color CardColor => Color.White;
        private Color TextPrimary => UiStyles.TextPrimary;
        private Color TextSecondary => UiStyles.TextSecondary;
        
        // Ana Renkler - Healthcare Theme
        private Color PrimaryColor => UiStyles.PrimaryColor;       // Teal
        private Color SuccessGreen => UiStyles.SuccessColor;
        private Color WarningOrange => UiStyles.WarningColor;
        private Color InfoBlue => UiStyles.InfoColor;

        public FrmPatientShell()
        {
            _patientRepo = new PatientRepository();
            InitializeComponent();
            SetupUI();
            LoadWelcomeScreen();
        }

        private void SetupUI()
        {
            this.Text = "DiyetPro - Hasta Paneli";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = BackgroundColor;
            this.IconOptions.ShowIcon = false;

            // Main Container
            mainContainer = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = BackgroundColor
            };

            // Top Panel
            topPanel = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 70,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardColor
            };
            // Altına ince çizgi
            topPanel.Paint += (s, e) => {
                using (var pen = new Pen(Color.FromArgb(230, 230, 230), 1))
                    e.Graphics.DrawLine(pen, 0, topPanel.Height - 1, topPanel.Width, topPanel.Height - 1);
            };

            btnHome = new SimpleButton
            {
                Text = "← Geri",
                Location = new Point(20, 20),
                Size = new Size(90, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = Color.FromArgb(240, 245, 250), ForeColor = TextPrimary, BorderColor = Color.Transparent },
                Visible = false,
                AllowFocus = false
            };
            btnHome.Click += (s, e) => LoadWelcomeScreen();
            topPanel.Controls.Add(btnHome);

            lblCurrentPage = new LabelControl
            {
                Text = "Ana Sayfa",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(20, 20),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(400, 30)
            };
            topPanel.Controls.Add(lblCurrentPage);

            // User Profile Badge (Sağ Üst)
            var pnlProfile = new Panel
            {
                Size = new Size(250, 50),
                Location = new Point(mainContainer.Width - 270, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            
            var lblUser = new LabelControl
            {
                Text = AuthContext.UserName,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(50, 8),
                AutoSize = true
            };
            
            var lblRole = new LabelControl
            {
                Text = "Hasta",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextSecondary,
                Location = new Point(50, 28),
                AutoSize = true
            };

            // Avatar (Basit daire)
            var pnlAvatar = new Panel
            {
                Size = new Size(40, 40),
                Location = new Point(0, 5),
                BackColor = PrimaryColor
            };
            pnlAvatar.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(PrimaryColor))
                    e.Graphics.FillEllipse(brush, 0, 0, 39, 39);
                
                string initials = string.Join("", AuthContext.UserName.Split(' ').Select(n => n[0])).ToUpper();
                if (initials.Length > 2) initials = initials.Substring(0, 2);
                
                var size = e.Graphics.MeasureString(initials, new Font("Segoe UI", 12F, FontStyle.Bold));
                e.Graphics.DrawString(initials, new Font("Segoe UI", 12F, FontStyle.Bold), Brushes.White, 
                    (40 - size.Width) / 2, (40 - size.Height) / 2);
            };

            pnlProfile.Controls.Add(pnlAvatar);
            pnlProfile.Controls.Add(lblUser);
            pnlProfile.Controls.Add(lblRole);
            topPanel.Controls.Add(pnlProfile);

            // Content Panel
            contentPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = BackgroundColor,
                Padding = new Padding(30),
                AutoScroll = true
            };

            mainContainer.Controls.Add(contentPanel);
            mainContainer.Controls.Add(topPanel);
            this.Controls.Add(mainContainer);

            // Sidebar
            sidebar = new ModernSidebar();
            sidebar.SetPanelTitle("Hasta Paneli");
            sidebar.AddMenuItem("🏠", "Ana Sayfa", "home");
            sidebar.AddMenuItem("👤", "Profilim", "profile");
            sidebar.AddMenuItem("🍽️", "Haftalık Menü", "myplan");
            sidebar.AddMenuItem("🎯", "Hedeflerim", "goals");
            sidebar.AddMenuItem("📈", "İlerlemem", "myprogress");
            sidebar.AddMenuItem("📏", "Vücut Ölçülerim", "measurements");
            sidebar.AddMenuItem("🏃", "Egzersizler", "mytasks");
            sidebar.AddMenuItem("📅", "Randevular", "appointments");
            sidebar.AddMenuItem("💬", "Mesajlar", "messages");
            sidebar.AddMenuItem("🚪", "Çıkış", "logout");
            sidebar.MenuItemClicked += Sidebar_MenuItemClicked;
            this.Controls.Add(sidebar);
        }

        private void Sidebar_MenuItemClicked(object sender, string key)
        {
            switch (key)
            {
                case "home":
                    LoadWelcomeScreen();
                    break;
                case "profile":
                    LoadChildForm(new FrmPatientProfile(AuthContext.UserId), "Profilim");
                    break;
                case "myplan":
                    LoadChildForm(new FrmWeeklyMenu(), "Haftalık Menü");
                    break;
                case "goals":
                    LoadChildForm(new FrmGoals(), "Hedeflerim");
                    break;
                case "myprogress":
                    LoadChildForm(new FrmProgress(), "İlerlemem");
                    break;
                case "measurements":
                    LoadChildForm(new FrmBodyMeasurements(), "Vücut Ölçülerim");
                    break;
                case "mytasks":
                    LoadChildForm(new FrmExerciseTasks(), "Egzersiz Görevlerim");
                    break;
                case "appointments":
                    LoadChildForm(new FrmPatientAppointments(), "Randevularım");
                    break;
                case "messages":
                    LoadChildForm(new FrmMessagesPatient(), "Mesajlarım");
                    break;
                case "logout":
                    Logout();
                    break;
            }
        }

        private void LoadWelcomeScreen()
        {
            lblCurrentPage.Text = "Ana Sayfa";
            lblCurrentPage.Location = new Point(20, 20);
            btnHome.Visible = false;
            contentPanel.Controls.Clear();

            // Hasta verilerini çek
            var patient = _patientRepo.GetById(AuthContext.UserId);
            string welcomeMsg = "Hoş Geldiniz";
            if (patient != null)
            {
                int hour = DateTime.Now.Hour;
                if (hour < 12) welcomeMsg = "Günaydın";
                else if (hour < 18) welcomeMsg = "İyi Günler";
                else welcomeMsg = "İyi Akşamlar";
                
                welcomeMsg += $", {AuthContext.UserName.Split(' ')[0]}! 👋";
            }

            // 1. Welcome Header
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.Transparent };
            
            var lblWelcome = new LabelControl
            {
                Text = welcomeMsg,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, 10),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblWelcome);

            var lblSubtitle = new LabelControl
            {
                Text = "Bugün hedeflerine ulaşmak için harika bir gün.",
                Font = new Font("Segoe UI", 11F),
                ForeColor = TextSecondary,
                Location = new Point(2, 55),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblSubtitle);
            contentPanel.Controls.Add(pnlHeader);

            // 2. İSTATİSTİK KARTLARI (Renkli ve büyük)
            int startY = 100;
            int rightMargin = 60;
            int totalWidth = contentPanel.Width - rightMargin;
            int cardSpacing = 20;
            int cardWidth = (totalWidth - (3 * cardSpacing)) / 4;
            int cardHeight = 120;

            if (patient != null)
            {
                double bmi = patient.Boy > 0 ? patient.GuncelKilo / (patient.Boy * patient.Boy) : 0;
                string bmiStatus = bmi < 18.5 ? "Zayıf" : bmi < 25 ? "Normal" : bmi < 30 ? "Kilolu" : "Obez";
                
                // Kilo Kartı
                CreateColoredStatCard(0, startY, cardWidth, cardHeight,
                    "MEVCUT KİLO", $"{patient.GuncelKilo} kg", $"Başlangıç: {patient.BaslangicKilosu}", "⚖️", 
                    Color.FromArgb(59, 130, 246), Color.FromArgb(37, 99, 235), "myprogress");

                // BMI Kartı
                Color bmiColor = bmi < 18.5 ? InfoBlue : bmi < 25 ? SuccessGreen : bmi < 30 ? WarningOrange : Color.Red;
                Color bmiDarkColor = ControlPaint.Dark(bmiColor);
                CreateColoredStatCard(cardWidth + cardSpacing, startY, cardWidth, cardHeight,
                    "VÜCUT KİTLE İNDEKSİ", $"{bmi:F1}", bmiStatus, "📊", 
                    bmiColor, bmiDarkColor, "measurements");

                // Su Kartı (Örnek veri)
                CreateColoredStatCard(2 * (cardWidth + cardSpacing), startY, cardWidth, cardHeight,
                    "SU TÜKETİMİ", "1.5 Lt", "Hedef: 2.5 Lt", "💧", 
                    Color.FromArgb(14, 165, 233), Color.FromArgb(2, 132, 199), "goals");

                // Adım Kartı (Örnek veri)
                CreateColoredStatCard(3 * (cardWidth + cardSpacing), startY, cardWidth, cardHeight,
                    "ADIM SAYISI", "4,250", "Hedef: 10,000", "👣", 
                    Color.FromArgb(249, 115, 22), Color.FromArgb(234, 88, 12), "goals");
            }

            // 3. HIZLI ERİŞİM BÖLÜMÜ
            int quickY = startY + cardHeight + 30;

            var lblQuickActions = new LabelControl
            {
                Text = "⚡ Hızlı İşlemler",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, quickY),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(200, 25)
            };
            contentPanel.Controls.Add(lblQuickActions);

            quickY += 35;
            int btnCount = 4;
            int btnSpacing = 20;
            int btnWidth = (totalWidth - ((btnCount - 1) * btnSpacing)) / btnCount;
            int btnHeight = 90;

            CreateModernQuickButton(0, quickY, btnWidth, btnHeight, "🍽️", "Öğün Bildir", SuccessGreen, () => Sidebar_MenuItemClicked(this, "myplan"));
            CreateModernQuickButton(btnWidth + btnSpacing, quickY, btnWidth, btnHeight, "💧", "Su Ekle", InfoBlue, () => Sidebar_MenuItemClicked(this, "goals"));
            CreateModernQuickButton(2 * (btnWidth + btnSpacing), quickY, btnWidth, btnHeight, "⚖️", "Kilo Gir", WarningOrange, () => Sidebar_MenuItemClicked(this, "myprogress"));
            CreateModernQuickButton(3 * (btnWidth + btnSpacing), quickY, btnWidth, btnHeight, "📅", "Randevu Al", PrimaryColor, () => Sidebar_MenuItemClicked(this, "appointments"));

            // 4. GÜNLÜK ÖZET
            int summaryY = quickY + btnHeight + 30;
            
            var summaryPanel = new PanelControl
            {
                Location = new Point(0, summaryY),
                Size = new Size(totalWidth, 150),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardColor
            };
            
            // Rounded corners simulation
            summaryPanel.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = CreateRoundedRect(new Rectangle(0, 0, summaryPanel.Width - 1, summaryPanel.Height - 1), 15))
                using (var pen = new Pen(Color.FromArgb(230, 230, 230), 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            };

            var lblSummaryTitle = new LabelControl
            {
                Text = "📅 Günün Özeti",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(20, 20),
                AutoSize = true
            };
            summaryPanel.Controls.Add(lblSummaryTitle);

            var lblSummaryText = new LabelControl
            {
                Text = "Bugünkü diyet planınızın %65'ini tamamladınız. Harika gidiyorsunuz!",
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextSecondary,
                Location = new Point(20, 50),
                AutoSize = true
            };
            summaryPanel.Controls.Add(lblSummaryText);

            var progressBar = new ProgressBarControl
            {
                Location = new Point(20, 80),
                Size = new Size(summaryPanel.Width - 40, 20),
                Properties = { Maximum = 100, ShowTitle = true, PercentView = true }
            };
            progressBar.EditValue = 65;
            summaryPanel.Controls.Add(progressBar);

            contentPanel.Controls.Add(summaryPanel);
        }

        private void CreateColoredStatCard(int x, int y, int width, int height, string title, string value, string subtitle, string icon, Color bgColor, Color darkBgColor, string key)
        {
            var card = new PanelControl
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = bgColor,
                Cursor = Cursors.Hand
            };

            // Başlık
            var lblTitle = new LabelControl
            {
                Text = title,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 15),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width - 50, 20)
            };
            card.Controls.Add(lblTitle);

            // İkon
            var lblIcon = new LabelControl
            {
                Text = icon,
                Font = new Font("Segoe UI", 18F),
                ForeColor = Color.FromArgb(220, 255, 255, 255),
                Location = new Point(width - 45, 10),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(40, 35)
            };
            card.Controls.Add(lblIcon);

            // Değer
            var lblValue = new LabelControl
            {
                Text = value,
                Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 40),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width - 30, 50)
            };
            card.Controls.Add(lblValue);

            // Alt Başlık
            var lblSubtitle = new LabelControl
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(230, 255, 255, 255),
                Location = new Point(15, height - 25),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width - 30, 20)
            };
            card.Controls.Add(lblSubtitle);

            // Hover
            card.MouseEnter += (s, e) => card.BackColor = darkBgColor;
            card.MouseLeave += (s, e) => card.BackColor = bgColor;
            foreach (Control ctrl in card.Controls)
            {
                ctrl.MouseEnter += (s, e) => card.BackColor = darkBgColor;
                ctrl.MouseLeave += (s, e) => card.BackColor = bgColor;
                ctrl.Cursor = Cursors.Hand;
                ctrl.Click += (s, e) => Sidebar_MenuItemClicked(this, key);
            }
            card.Click += (s, e) => Sidebar_MenuItemClicked(this, key);

            contentPanel.Controls.Add(card);
        }

        private void CreateModernQuickButton(int x, int y, int width, int height, string icon, string text, Color borderColor, Action onClick)
        {
            Color lightBgColor = Color.FromArgb(25, borderColor.R, borderColor.G, borderColor.B);
            Color hoverBgColor = Color.FromArgb(45, borderColor.R, borderColor.G, borderColor.B);
            
            var btn = new PanelControl
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = lightBgColor,
                Cursor = Cursors.Hand
            };

            var colorBar = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(5, height),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = borderColor
            };
            btn.Controls.Add(colorBar);

            var lblIcon = new LabelControl
            {
                Text = icon,
                Font = new Font("Segoe UI", 24F),
                ForeColor = borderColor,
                Location = new Point(0, 15),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width, 40),
                Cursor = Cursors.Hand
            };
            lblIcon.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            btn.Controls.Add(lblIcon);

            var lblText = new LabelControl
            {
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, height - 30),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width, 20),
                Cursor = Cursors.Hand
            };
            lblText.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            btn.Controls.Add(lblText);

            btn.MouseEnter += (s, e) => btn.BackColor = hoverBgColor;
            btn.MouseLeave += (s, e) => btn.BackColor = lightBgColor;
            foreach (Control ctrl in btn.Controls)
            {
                ctrl.MouseEnter += (s, e) => btn.BackColor = hoverBgColor;
                ctrl.MouseLeave += (s, e) => btn.BackColor = lightBgColor;
                ctrl.Click += (s, e) => onClick();
            }
            btn.Click += (s, e) => onClick();

            contentPanel.Controls.Add(btn);
        }

        private void LoadChildForm(Form childForm, string pageTitle)
        {
            btnHome.Visible = true;
            lblCurrentPage.Text = pageTitle;
            lblCurrentPage.Location = new Point(120, 20);

            contentPanel.Controls.Clear();
            
            childForm.TopLevel = false;
            childForm.Dock = DockStyle.Fill;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.BackColor = BackgroundColor;
            childForm.Opacity = 0;
            
            contentPanel.Controls.Add(childForm);
            childForm.Show();

            // Fade-in
            var fadeTimer = new Timer { Interval = 15 };
            fadeTimer.Tick += (s, e) =>
            {
                if (childForm.Opacity < 1) childForm.Opacity += 0.1;
                else { childForm.Opacity = 1; fadeTimer.Stop(); fadeTimer.Dispose(); }
            };
            fadeTimer.Start();
        }

        private void Logout()
        {
            if (XtraMessageBox.Show("Çıkış yapmak istediğinizden emin misiniz?", "Çıkış",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                AuthContext.SignOut();
                this.Close();
            }
        }

        private GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1400, 850);
            this.Name = "FrmPatientShell";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
        }
    }
}
