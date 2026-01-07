using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;

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

        // RENK PALETI - UiStyles'dan alınıyor
        private Color BackgroundColor => UiStyles.BackgroundColor;
        private Color CardColor => UiStyles.CardBackgroundColor;
        private Color TextPrimary => UiStyles.TextPrimary;
        private Color TextSecondary => UiStyles.TextSecondary;
        
        // Ana Renkler - Healthcare Theme
        private Color PrimaryBlue => UiStyles.PrimaryColor;       // Teal
        private Color SuccessGreen => UiStyles.SuccessColor;
        private Color DangerRed => UiStyles.DangerColor;
        private Color WarningOrange => UiStyles.WarningColor;
        private Color InfoCyan => UiStyles.InfoColor;
        private Color Purple => UiStyles.SecondaryColor;          // Indigo

        public FrmPatientShell()
        {
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
                Height = 60,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardColor
            };

            btnHome = new SimpleButton
            {
                Text = "< Ana Sayfa",
                Location = new Point(15, 14),
                Size = new Size(110, 32),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Appearance = { BackColor = SuccessGreen, ForeColor = Color.White, BorderColor = SuccessGreen },
                Visible = false,
                AllowFocus = false
            };
            btnHome.Click += (s, e) => LoadWelcomeScreen();
            topPanel.Controls.Add(btnHome);

            lblCurrentPage = new LabelControl
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(15, 17),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(400, 28)
            };
            topPanel.Controls.Add(lblCurrentPage);

            var lblUser = new LabelControl
            {
                Text = AuthContext.UserName,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = SuccessGreen,
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(280, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } }
            };
            lblUser.Location = new Point(mainContainer.Width - 300, 20);
            topPanel.Controls.Add(lblUser);

            // Content Panel
            contentPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = BackgroundColor,
                Padding = new Padding(25),
                AutoScroll = true
            };

            mainContainer.Controls.Add(contentPanel);
            mainContainer.Controls.Add(topPanel);
            this.Controls.Add(mainContainer);

            // Sidebar
            sidebar = new ModernSidebar();
            sidebar.SetPanelTitle("Hasta Paneli");
            sidebar.AddMenuItem("H", "Ana Sayfa", "home");
            sidebar.AddMenuItem("M", "Haftalık Menü", "myplan");
            sidebar.AddMenuItem("T", "Hedeflerim", "goals");
            sidebar.AddMenuItem("I", "İlerlemem", "myprogress");
            sidebar.AddMenuItem("O", "Vücut Ölçülerim", "measurements");
            sidebar.AddMenuItem("E", "Egzersiz Görevleri", "mytasks");
            sidebar.AddMenuItem("R", "Randevularım", "appointments");
            sidebar.AddMenuItem("C", "Mesajlar", "messages");
            sidebar.AddMenuItem("S", "Ayarlar", "settings");
            sidebar.AddMenuItem("X", "Çıkış", "logout");
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
                case "settings":
                    XtraMessageBox.Show("Ayarlar modülü yapım aşamasında...", "Bilgi",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case "logout":
                    Logout();
                    break;
            }
        }

        private void LoadWelcomeScreen()
        {
            lblCurrentPage.Text = "Ana Sayfa";
            lblCurrentPage.Location = new Point(15, 17);
            btnHome.Visible = false;
            contentPanel.Controls.Clear();

            // Hosgeldin
            var lblWelcome = new LabelControl
            {
                Text = "Hoş Geldiniz, " + AuthContext.UserName,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, 0),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(700, 40)
            };
            contentPanel.Controls.Add(lblWelcome);

            var lblSubtitle = new LabelControl
            {
                Text = "Sağlıklı yaşam yolculuğunuz burada başlıyor!",
                Font = new Font("Segoe UI", 12F),
                ForeColor = TextSecondary,
                Location = new Point(0, 45),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(500, 25)
            };
            contentPanel.Controls.Add(lblSubtitle);

            // Dashboard kartlari - 4 kart yan yana
            int startY = 100;
            int cardWidth = 260;
            int cardHeight = 140;
            int spacing = 20;

            CreateDashboardCard(0, startY, cardWidth, cardHeight,
                "7", "Haftalık Menü", "Diyet planını görüntüle", PrimaryBlue, "menu");

            CreateDashboardCard(cardWidth + spacing, startY, cardWidth, cardHeight,
                "3", "Aktif Hedef", "Hedeflerini takip et", SuccessGreen, "goals");

            CreateDashboardCard(2 * (cardWidth + spacing), startY, cardWidth, cardHeight,
                "-2.5", "Kilo Değişimi", "İlerlemeni incele", Purple, "progress");

            CreateDashboardCard(3 * (cardWidth + spacing), startY, cardWidth, cardHeight,
                "1", "Yeni Mesaj", "Doktorunla iletişim", WarningOrange, "messages");
        }

        private void CreateDashboardCard(int x, int y, int width, int height,
            string stat, string title, string description, Color accentColor, string key)
        {
            // Gölge paneli (arka plan)
            var shadowPanel = new Panel
            {
                Location = new Point(x + 3, y + 3),
                Size = new Size(width, height),
                BackColor = Color.FromArgb(40, 0, 0, 0)
            };
            contentPanel.Controls.Add(shadowPanel);
            shadowPanel.SendToBack();

            var card = new PanelControl
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple,
                BackColor = CardColor,
                Cursor = Cursors.Hand
            };

            // Sol renk çizgisi
            var colorBar = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(5, height),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = accentColor
            };
            card.Controls.Add(colorBar);

            // Büyük sayı/değer
            var lblStat = new LabelControl
            {
                Text = stat,
                Font = new Font("Segoe UI", 36F, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(20, 15),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(120, 50)
            };
            card.Controls.Add(lblStat);

            // Başlık
            var lblTitle = new LabelControl
            {
                Text = title,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(20, 70),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width - 40, 25)
            };
            card.Controls.Add(lblTitle);

            // Açıklama
            var lblDesc = new LabelControl
            {
                Text = description,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextSecondary,
                Location = new Point(20, 98),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width - 40, 20)
            };
            card.Controls.Add(lblDesc);

            // Profesyonel Hover efekti
            card.MouseEnter += (s, e) => {
                card.BackColor = Color.FromArgb(245, 248, 255);
                card.Location = new Point(x - 2, y - 2);
                shadowPanel.Location = new Point(x + 5, y + 5);
            };
            card.MouseLeave += (s, e) => {
                card.BackColor = CardColor;
                card.Location = new Point(x, y);
                shadowPanel.Location = new Point(x + 3, y + 3);
            };

            // Alt kontrollere de hover ve tıklama ekle
            foreach (Control ctrl in card.Controls)
            {
                ctrl.MouseEnter += (s, e) => {
                    card.BackColor = Color.FromArgb(245, 248, 255);
                    card.Location = new Point(x - 2, y - 2);
                    shadowPanel.Location = new Point(x + 5, y + 5);
                };
                ctrl.MouseLeave += (s, e) => {
                    card.BackColor = CardColor;
                    card.Location = new Point(x, y);
                    shadowPanel.Location = new Point(x + 3, y + 3);
                };
            }

            // Tıklama
            card.Click += (s, e) => Sidebar_MenuItemClicked(this, key);
            foreach (Control ctrl in card.Controls)
            {
                ctrl.Click += (s, e) => Sidebar_MenuItemClicked(this, key);
                ctrl.Cursor = Cursors.Hand;
            }

            contentPanel.Controls.Add(card);
            card.BringToFront();
        }

        private void LoadChildForm(Form childForm, string pageTitle)
        {
            btnHome.Visible = true;
            lblCurrentPage.Text = pageTitle;
            lblCurrentPage.Location = new Point(135, 17);

            contentPanel.Controls.Clear();
            
            childForm.TopLevel = false;
            childForm.Dock = DockStyle.Fill;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.BackColor = BackgroundColor;
            childForm.Opacity = 0;
            
            contentPanel.Controls.Add(childForm);
            childForm.Show();

            // Fade-in animasyonu
            var fadeTimer = new Timer { Interval = 15 };
            fadeTimer.Tick += (s, e) =>
            {
                if (childForm.Opacity < 1)
                {
                    childForm.Opacity += 0.1;
                }
                else
                {
                    childForm.Opacity = 1;
                    fadeTimer.Stop();
                    fadeTimer.Dispose();
                }
            };
            fadeTimer.Start();
        }

        private void Logout()
        {
            var result = XtraMessageBox.Show("Çıkış yapmak istediğinizden emin misiniz?", "Çıkış",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                AuthContext.SignOut();
                this.Close();
            }
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
