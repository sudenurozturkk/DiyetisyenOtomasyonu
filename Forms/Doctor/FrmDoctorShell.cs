using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Shared;
using DiyetisyenOtomasyonu.Infrastructure.DI;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    public partial class FrmDoctorShell : XtraForm
    {
        private ModernSidebar sidebar;
        private PanelControl mainContainer;
        private PanelControl topPanel;
        private PanelControl contentPanel;
        private LabelControl lblCurrentPage;
        private SimpleButton btnHome;
        private LabelControl _lblUser;

        // Services
        private readonly PatientService _patientService;
        private readonly MessageService _messageService;
        private readonly GoalService _goalService;
        private readonly MealService _mealService;
        private readonly AppointmentRepository _appointmentRepo;
        private readonly UserRepository _userRepo;

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
        private Color PurpleColor => UiStyles.SecondaryColor;     // Indigo

        public FrmDoctorShell()
        {
            InitializeComponent();

            var container = ServiceContainer.Instance;
            _patientService = container.GetService<PatientService>();
            _messageService = container.GetService<MessageService>();
            _goalService = container.GetService<GoalService>();
            _mealService = container.GetService<MealService>();
            _appointmentRepo = container.GetService<AppointmentRepository>();
            _userRepo = container.GetService<UserRepository>();

            SetupUI();
            
            // Form tam boyutuna ulaştıktan sonra welcome screen'i yükle
            this.Shown += (s, e) => LoadWelcomeScreen();
        }

        private void SetupUI()
        {
            this.Text = "DiyetPro - Diyetisyen Paneli";
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
                Appearance = { BackColor = PrimaryBlue, ForeColor = Color.White, BorderColor = PrimaryBlue },
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
                Location = new Point(15, 17),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(400, 28)
            };
            topPanel.Controls.Add(lblCurrentPage);

            string doctorName = AuthContext.UserName;
            if (!doctorName.StartsWith("Dr.", StringComparison.OrdinalIgnoreCase))
            {
                doctorName = "Dr. " + doctorName;
            }

            _lblUser = new LabelControl
            {
                Text = doctorName,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                // Daha okunaklı nötr renk
                ForeColor = TextPrimary,
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(320, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } }
            };
            _lblUser.Location = new Point(mainContainer.Width - _lblUser.Width - 20, 20);
            topPanel.Controls.Add(_lblUser);

            // Form resize olunca sağ hizayı koru
            this.SizeChanged += (s, e) =>
            {
                if (_lblUser != null && mainContainer != null)
                {
                    _lblUser.Location = new Point(mainContainer.Width - _lblUser.Width - 20, 20);
                }
            };

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
            sidebar.SetPanelTitle("Diyetisyen Paneli");
            sidebar.AddMenuItem("🏠", "Ana Sayfa", "home");
            sidebar.AddMenuItem("👥", "Hastalarım", "patients");
            sidebar.AddMenuItem("🥗", "Diyet Planları", "plans");
            sidebar.AddMenuItem("🍽️", "Öğünler", "meals");
            sidebar.AddMenuItem("🏃", "Egzersiz Görevleri", "exercises");
            sidebar.AddMenuItem("📅", "Randevular", "appointments");
            sidebar.AddMenuItem("💰", "Finansal Özet", "financials");
            sidebar.AddMenuItem("🎯", "Hedefler", "goals");
            sidebar.AddMenuItem("📝", "Notlar", "notes");
            sidebar.AddMenuItem("💬", "Mesajlar", "messages");
            sidebar.AddMenuItem("📊", "Raporlar", "analytics");
            sidebar.AddMenuItem("🤖", "AI Analiz", "aianalysis");
            sidebar.AddMenuItem("⚙️", "Ayarlar", "settings");
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
                case "patients":
                    LoadChildForm(new FrmPatients(), "Hasta Yönetimi");
                    break;
                case "plans":
                    LoadChildForm(new FrmAssignPlans(), "Diyet Planları");
                    break;
                case "meals":
                    LoadChildForm(new FrmMeals(), "Yemek Kütüphanesi");
                    break;
                case "exercises":
                    LoadChildForm(new FrmExerciseManager(), "Egzersiz Görev Yönetimi");
                    break;
                case "appointments":
                    LoadChildForm(new FrmAppointments(), "Randevu Yönetimi");
                    break;
                case "financials":
                    LoadChildForm(new FrmFinancials(), "Finansal Özet");
                    break;
                case "goals":
                    LoadChildForm(new FrmGoalsNotes(), "Hasta Hedefleri");
                    break;
                case "notes":
                    LoadChildForm(new FrmNotesModern(), "Hasta Notları");
                    break;
                case "messages":
                    LoadChildForm(new FrmMessagesModern(), "Mesajlaşma");
                    break;
                case "analytics":
                    LoadChildForm(new FrmReports(0), "Raporlar ve Analizler");
                    break;
                case "aianalysis":
                    LoadChildForm(new FrmAIAnalysis(), "AI Analiz");
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
            lblCurrentPage.Font = new Font("Segoe UI", 20F, FontStyle.Bold); // Increased font size
            lblCurrentPage.Location = new Point(15, 14);
            btnHome.Visible = false;
            contentPanel.Controls.Clear();

            // Hosgeldin - Dr. eklemeden önce kontrol et
            string displayName = AuthContext.UserName;
            if (!displayName.StartsWith("Dr.", StringComparison.OrdinalIgnoreCase))
            {
                displayName = "Dr. " + displayName;
            }
            var lblWelcome = new LabelControl
            {
                Text = "Hoş Geldiniz, " + displayName,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, 0),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(800, 35)
            };
            contentPanel.Controls.Add(lblWelcome);

            var lblSubtitle = new LabelControl
            {
                Text = "Bugün nasıl yardımcı olabilirim?",
                Font = new Font("Segoe UI", 11F),
                ForeColor = TextSecondary,
                Location = new Point(0, 38),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(500, 22)
            };
            contentPanel.Controls.Add(lblSubtitle);

            // Gerçek istatistikleri al
            int patientCount = 0, mealCount = 0, unreadMessages = 0, appointmentCount = 0;
            try
            {
                var patients = _patientService.GetPatientsByDoctor(AuthContext.UserId);
                patientCount = patients.Count;
                var meals = _mealService.GetDoctorMeals(AuthContext.UserId);
                mealCount = meals.Count;
                unreadMessages = _messageService.GetUnreadCount(AuthContext.UserId);
                appointmentCount = _appointmentRepo.GetTodayAppointments(AuthContext.UserId).Count;
            }
            catch { }

            // ========== İSTATİSTİK KARTLARI (Renkli ve büyük) ==========
            int startY = 70;
            int rightMargin = 68; // ~1.8cm kenar boşluğu
            int totalWidth = contentPanel.Width - rightMargin;
            int cardSpacing = 18;
            int cardWidth = (totalWidth - (3 * cardSpacing)) / 4;
            int cardHeight = 115;

            // Yeşil kart - Toplam Hasta
            CreateColoredStatCard(0, startY, cardWidth, cardHeight,
                "TOPLAM HASTA", patientCount.ToString(), "+1 Bu hafta", "👥", 
                Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105), "patients");

            // Turuncu kart - Yemek Tarifi
            CreateColoredStatCard(cardWidth + cardSpacing, startY, cardWidth, cardHeight,
                "YEMEK TARİFİ", mealCount.ToString(), "Öğün kütüphaneniz", "🍴", 
                Color.FromArgb(251, 191, 36), Color.FromArgb(245, 158, 11), "meals");

            // Mavi kart - Bugün Randevu
            CreateColoredStatCard(2 * (cardWidth + cardSpacing), startY, cardWidth, cardHeight,
                "BUGÜN RANDEVU", appointmentCount.ToString(), appointmentCount == 0 ? "Dinlenme günü" : $"{appointmentCount} randevu", "📅", 
                Color.FromArgb(59, 130, 246), Color.FromArgb(37, 99, 235), "appointments");

            // Mor kart - Mesajlar
            CreateColoredStatCard(3 * (cardWidth + cardSpacing), startY, cardWidth, cardHeight,
                "MESAJLAR", unreadMessages.ToString(), unreadMessages == 0 ? "Tüm mesajlar okundu" : $"{unreadMessages} okunmamış", "💬", 
                Color.FromArgb(168, 85, 247), Color.FromArgb(139, 92, 246), "messages");

            // ========== HIZLI ERİŞİM BÖLÜMÜ ==========
            int quickY = startY + cardHeight + 20;

            var lblQuickActions = new LabelControl
            {
                Text = "⚡ Hızlı Erişim",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, quickY),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(200, 25)
            };
            contentPanel.Controls.Add(lblQuickActions);

            quickY += 30;
            // Sayfaya orantılı buton genişliği
            int totalBtnWidth = totalWidth;
            int btnCount = 6;
            int btnSpacing = 20; // Butonlar arası boşluk artırıldı
            int btnWidth = (totalBtnWidth - ((btnCount - 1) * btnSpacing)) / btnCount;
            int btnHeight = 80;

            CreateModernQuickButton(0, quickY, btnWidth, btnHeight, "👤+", "Hasta Ekle", SuccessGreen, () => LoadChildForm(new FrmPatients(), "Hasta Yönetimi"));
            CreateModernQuickButton(btnWidth + btnSpacing, quickY, btnWidth, btnHeight, "📋", "Diyet Planı", PrimaryBlue, () => LoadChildForm(new FrmAssignPlans(), "Diyet Planları"));
            CreateModernQuickButton(2 * (btnWidth + btnSpacing), quickY, btnWidth, btnHeight, "📅", "Randevu", PurpleColor, () => LoadChildForm(new FrmAppointments(), "Randevular"));
            CreateModernQuickButton(3 * (btnWidth + btnSpacing), quickY, btnWidth, btnHeight, "🤖", "AI Analiz", InfoCyan, () => LoadChildForm(new FrmAIAnalysis(), "AI Analiz"));
            CreateModernQuickButton(4 * (btnWidth + btnSpacing), quickY, btnWidth, btnHeight, "📊", "Raporlar", DangerRed, () => LoadChildForm(new FrmAnalytics(), "Raporlar"));
            CreateModernQuickButton(5 * (btnWidth + btnSpacing), quickY, btnWidth, btnHeight, "✉️", "Mesajlar", PrimaryBlue, () => LoadChildForm(new FrmMessagesModern(), "Mesajlaşma"));

            // ========== BUGÜNÜN RANDEVULARI ==========
            int calendarY = quickY + btnHeight + 20;

            var headerPanel = new PanelControl
            {
                Location = new Point(0, calendarY),
                Size = new Size(totalWidth, 32),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.Transparent
            };

            var lblCalendarTitle = new LabelControl
            {
                Text = "📋 Bugünün Randevuları",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, 3),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(250, 25)
            };
            headerPanel.Controls.Add(lblCalendarTitle);

            var lblSeeAll = new LabelControl
            {
                Text = "Tümünü Gör",
                Font = new Font("Segoe UI", 10F, FontStyle.Underline),
                ForeColor = PrimaryBlue,
                Location = new Point(headerPanel.Width - 100, 5),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(90, 20),
                Cursor = Cursors.Hand
            };
            lblSeeAll.Click += (s, e) => Sidebar_MenuItemClicked(this, "appointments");
            headerPanel.Controls.Add(lblSeeAll);
            contentPanel.Controls.Add(headerPanel);

            calendarY += 35;

            // Randevular için BEYAZ kart - tam genişlik, alt kenar 68px (~1.8cm) kalacak şekilde yükseklik
            int bottomMargin = 68; // ~1.8cm
            int appointmentCardHeight = contentPanel.Height - calendarY - bottomMargin;
            if (appointmentCardHeight < 120) appointmentCardHeight = 120; // Minimum yükseklik
            
            var appointmentCard = new PanelControl
            {
                Location = new Point(0, calendarY),
                Size = new Size(totalWidth, appointmentCardHeight),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.White,
                Appearance = { BackColor = Color.White }
            };
            appointmentCard.LookAndFeel.UseDefaultLookAndFeel = false;
            appointmentCard.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;

            var todayAppointments = _appointmentRepo.GetTodayAppointments(AuthContext.UserId);

            if (todayAppointments.Count == 0)
            {
                // Boş durum - görseldeki gibi
                var iconLabel = new LabelControl
                {
                    Text = "📅",
                    Font = new Font("Segoe UI", 40F),
                    ForeColor = UiStyles.TextMuted,
                    Location = new Point((appointmentCard.Width - 50) / 2, 30),
                    AutoSizeMode = LabelAutoSizeMode.None,
                    Size = new Size(50, 50)
                };
                appointmentCard.Controls.Add(iconLabel);

                var lblEmpty = new LabelControl
                {
                    Text = "Randevu Bulunmuyor",
                    Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                    ForeColor = TextPrimary,
                    Location = new Point(0, 90),
                    AutoSizeMode = LabelAutoSizeMode.None,
                    Size = new Size(appointmentCard.Width, 25)
                };
                lblEmpty.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                appointmentCard.Controls.Add(lblEmpty);

                var lblEmptyDesc = new LabelControl
                {
                    Text = "Bugün için planlanmış bir görüşmeniz yok.\nYeni randevu eklemek için hızlı erişimi kullanabilirsiniz.",
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = TextSecondary,
                    Location = new Point(0, 120),
                    AutoSizeMode = LabelAutoSizeMode.None,
                    Size = new Size(appointmentCard.Width, 45)
                };
                lblEmptyDesc.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                appointmentCard.Controls.Add(lblEmptyDesc);

                // Buton pozisyonunu dinamik yap
                int btnY = Math.Min(appointmentCardHeight - 55, 175);
                var btnAdd = new SimpleButton
                {
                    Text = "+ Randevu Oluştur",
                    Location = new Point((appointmentCard.Width - 180) / 2, btnY),
                    Size = new Size(180, 42),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    AllowFocus = false
                };
                btnAdd.Appearance.BackColor = SuccessGreen;
                btnAdd.Appearance.ForeColor = Color.White;
                btnAdd.Appearance.Options.UseBackColor = true;
                btnAdd.Appearance.Options.UseForeColor = true;
                btnAdd.Click += (s, e) => Sidebar_MenuItemClicked(this, "appointments");
                appointmentCard.Controls.Add(btnAdd);
            }
            else
            {
                int aptX = 15;
                int aptY = 15;
                foreach (var apt in todayAppointments.Take(5))
                {
                    var patient = _userRepo.GetById(apt.PatientId);
                    string patientName = patient?.AdSoyad ?? "Hasta";
                    CreateAppointmentMiniCard(appointmentCard, aptX, aptY, apt, patientName);
                    aptX += 235;
                }
            }

            contentPanel.Controls.Add(appointmentCard);
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

            // Başlık (üstte, beyaz)
            var lblTitle = new LabelControl
            {
                Text = title,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(18, 15),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width - 60, 20)
            };
            card.Controls.Add(lblTitle);

            // İkon (sağ üst, beyaz)
            var lblIcon = new LabelControl
            {
                Text = icon,
                Font = new Font("Segoe UI", 20F),
                ForeColor = Color.FromArgb(220, 255, 255, 255),
                Location = new Point(width - 48, 10),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(40, 35)
            };
            card.Controls.Add(lblIcon);

            // Büyük sayı (beyaz)
            var lblValue = new LabelControl
            {
                Text = value,
                Font = new Font("Segoe UI", 38F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(18, 40),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(120, 50)
            };
            card.Controls.Add(lblValue);

            // Alt yazı (açık beyaz)
            var lblSubtitle = new LabelControl
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(230, 255, 255, 255),
                Location = new Point(18, height - 32),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width - 36, 20)
            };
            card.Controls.Add(lblSubtitle);

            // Hover efekti
            card.MouseEnter += (s, e) => card.BackColor = darkBgColor;
            card.MouseLeave += (s, e) => card.BackColor = bgColor;
            foreach (Control ctrl in card.Controls)
            {
                ctrl.MouseEnter += (s, e) => card.BackColor = darkBgColor;
                ctrl.MouseLeave += (s, e) => card.BackColor = bgColor;
                ctrl.Cursor = Cursors.Hand;
            }

            card.Click += (s, e) => Sidebar_MenuItemClicked(this, key);
            foreach (Control ctrl in card.Controls)
                ctrl.Click += (s, e) => Sidebar_MenuItemClicked(this, key);

            contentPanel.Controls.Add(card);
        }

        private void CreateModernStatCard(int x, int y, int width, int height, string title, string value, string subtitle, string icon, Color accentColor, string key)
        {
            var card = new PanelControl
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            // Başlık (üstte)
            var lblTitle = new LabelControl
            {
                Text = title,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextSecondary,
                Location = new Point(15, 12),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width - 50, 18)
            };
            card.Controls.Add(lblTitle);

            // İkon (sağ üst)
            var lblIcon = new LabelControl
            {
                Text = icon,
                Font = new Font("Segoe UI", 16F),
                ForeColor = accentColor,
                Location = new Point(width - 40, 8),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(30, 30)
            };
            card.Controls.Add(lblIcon);

            // Büyük sayı
            var lblValue = new LabelControl
            {
                Text = value,
                Font = new Font("Segoe UI", 32F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(15, 35),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(100, 42)
            };
            card.Controls.Add(lblValue);

            // Alt yazı
            var lblSubtitle = new LabelControl
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 9F),
                ForeColor = accentColor,
                Location = new Point(15, height - 28),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width - 30, 18)
            };
            card.Controls.Add(lblSubtitle);

            // Hover efekti
            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(248, 250, 252);
            card.MouseLeave += (s, e) => card.BackColor = Color.White;
            foreach (Control ctrl in card.Controls)
            {
                ctrl.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(248, 250, 252);
                ctrl.MouseLeave += (s, e) => card.BackColor = Color.White;
                ctrl.Cursor = Cursors.Hand;
            }

            card.Click += (s, e) => Sidebar_MenuItemClicked(this, key);
            foreach (Control ctrl in card.Controls)
                ctrl.Click += (s, e) => Sidebar_MenuItemClicked(this, key);

            contentPanel.Controls.Add(card);
        }

        private void CreateModernQuickButton(int x, int y, int width, int height, string icon, string text, Color borderColor, Action onClick)
        {
            // Açık ton arka plan rengi hesapla (kenar renginin %15 opaklığı)
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

            // Sol kenarlık rengi
            var colorBar = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(5, height),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = borderColor
            };
            btn.Controls.Add(colorBar);

            // İkon
            var lblIcon = new LabelControl
            {
                Text = icon,
                Font = new Font("Segoe UI", 22F),
                ForeColor = borderColor,
                Location = new Point(0, 12),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width, 35),
                Cursor = Cursors.Hand
            };
            lblIcon.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            btn.Controls.Add(lblIcon);

            // Metin
            var lblText = new LabelControl
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, height - 28),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width, 20),
                Cursor = Cursors.Hand
            };
            lblText.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            btn.Controls.Add(lblText);

            // Hover efekti - daha koyu açık ton
            btn.MouseEnter += (s, e) => btn.BackColor = hoverBgColor;
            btn.MouseLeave += (s, e) => btn.BackColor = lightBgColor;
            foreach (Control ctrl in btn.Controls)
            {
                ctrl.MouseEnter += (s, e) => btn.BackColor = hoverBgColor;
                ctrl.MouseLeave += (s, e) => btn.BackColor = lightBgColor;
            }

            btn.Click += (s, e) => onClick();
            foreach (Control ctrl in btn.Controls)
                ctrl.Click += (s, e) => onClick();

            contentPanel.Controls.Add(btn);
        }

        private void CreateAppointmentMiniCard(PanelControl parent, int x, int y, Appointment apt, string patientName)
        {
            var isPending = apt.Status == AppointmentStatus.Pending;
            var isApproved = apt.Status == AppointmentStatus.Scheduled;
            
            // Renk paleti
            Color headerColor, accentColor, bgColor;
            string statusText, statusIcon;
            
            if (isPending)
            {
                headerColor = Color.FromArgb(245, 158, 11); // Amber
                accentColor = Color.FromArgb(251, 191, 36);
                bgColor = Color.FromArgb(255, 251, 235);
                statusText = "Bekliyor";
                statusIcon = "⏳";
            }
            else if (isApproved)
            {
                headerColor = Color.FromArgb(16, 185, 129); // Green
                accentColor = Color.FromArgb(52, 211, 153);
                bgColor = Color.FromArgb(236, 253, 245);
                statusText = "Onaylı";
                statusIcon = "✓";
            }
            else
            {
                headerColor = Color.FromArgb(59, 130, 246); // Blue
                accentColor = Color.FromArgb(96, 165, 250);
                bgColor = Color.FromArgb(239, 246, 255);
                statusText = "Tamamlandı";
                statusIcon = "✔";
            }

            // Ana kart - daha büyük ve modern
            var card = new PanelControl
            {
                Location = new Point(x, y),
                Size = new Size(220, 140),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            // Gölge efekti
            card.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
            };

            // Renkli üst başlık şeridi
            var headerBar = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(220, 36),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = headerColor
            };
            
            var lblStatusIcon = new LabelControl
            {
                Text = statusIcon,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 8),
                BackColor = Color.Transparent
            };
            headerBar.Controls.Add(lblStatusIcon);

            var lblStatus = new LabelControl
            {
                Text = statusText,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(32, 9),
                BackColor = Color.Transparent
            };
            headerBar.Controls.Add(lblStatus);

            var lblTime = new LabelControl
            {
                Text = apt.DateTime.ToString("HH:mm"),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(160, 9),
                BackColor = Color.Transparent
            };
            headerBar.Controls.Add(lblTime);

            card.Controls.Add(headerBar);

            // İçerik paneli
            var contentPanel = new PanelControl
            {
                Location = new Point(0, 36),
                Size = new Size(220, 104),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = bgColor
            };

            // Avatar (ilk harf)
            string initials = "";
            var nameParts = patientName.Split(' ');
            if (nameParts.Length >= 2)
                initials = nameParts[0][0].ToString() + nameParts[nameParts.Length - 1][0].ToString();
            else if (nameParts.Length == 1 && nameParts[0].Length > 0)
                initials = nameParts[0][0].ToString();

            var avatarPanel = new PanelControl
            {
                Location = new Point(15, 15),
                Size = new Size(48, 48),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = accentColor
            };
            avatarPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(accentColor))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, 47, 47);
                }
                using (var font = new Font("Segoe UI", 14F, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var size = e.Graphics.MeasureString(initials.ToUpper(), font);
                    e.Graphics.DrawString(initials.ToUpper(), font, brush, 
                        (48 - size.Width) / 2, (48 - size.Height) / 2);
                }
            };
            contentPanel.Controls.Add(avatarPanel);

            // Hasta adı
            var displayName = patientName.Length > 18 ? patientName.Substring(0, 16) + ".." : patientName;
            var lblPatient = new LabelControl
            {
                Text = displayName,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(75, 18),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(135, 22),
                BackColor = Color.Transparent
            };
            contentPanel.Controls.Add(lblPatient);

            // Randevu türü (Notes'dan kontrol)
            string aptType = apt.Notes?.ToLower().Contains("ilk") == true ? "İlk Görüşme" : (apt.Type == AppointmentType.Online ? "Online" : "Klinik");
            var lblType = new LabelControl
            {
                Text = aptType,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextSecondary,
                Location = new Point(75, 42),
                BackColor = Color.Transparent
            };
            contentPanel.Controls.Add(lblType);

            // Detay butonu
            var btnDetail = new SimpleButton
            {
                Text = "Detay →",
                Location = new Point(130, 70),
                Size = new Size(75, 28),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                AllowFocus = false
            };
            btnDetail.Appearance.BackColor = headerColor;
            btnDetail.Appearance.ForeColor = Color.White;
            btnDetail.Appearance.Options.UseBackColor = true;
            btnDetail.Appearance.Options.UseForeColor = true;
            btnDetail.Click += (s, e) => Sidebar_MenuItemClicked(this, "appointments");
            contentPanel.Controls.Add(btnDetail);

            card.Controls.Add(contentPanel);

            // Hover efekti
            Color normalBg = Color.White;
            Color hoverBg = Color.FromArgb(248, 250, 252);
            
            void ApplyHover(bool isHover)
            {
                card.BackColor = isHover ? hoverBg : normalBg;
                if (isHover)
                {
                    card.Location = new Point(x - 2, y - 2);
                }
                else
                {
                    card.Location = new Point(x, y);
                }
            }

            card.MouseEnter += (s, e) => ApplyHover(true);
            card.MouseLeave += (s, e) => ApplyHover(false);
            headerBar.MouseEnter += (s, e) => ApplyHover(true);
            headerBar.MouseLeave += (s, e) => ApplyHover(false);
            contentPanel.MouseEnter += (s, e) => ApplyHover(true);
            contentPanel.MouseLeave += (s, e) => ApplyHover(false);

            foreach (Control ctrl in headerBar.Controls)
            {
                ctrl.MouseEnter += (s, e) => ApplyHover(true);
                ctrl.MouseLeave += (s, e) => ApplyHover(false);
                ctrl.Cursor = Cursors.Hand;
            }
            foreach (Control ctrl in contentPanel.Controls)
            {
                if (ctrl != btnDetail)
                {
                    ctrl.MouseEnter += (s, e) => ApplyHover(true);
                    ctrl.MouseLeave += (s, e) => ApplyHover(false);
                    ctrl.Cursor = Cursors.Hand;
                }
            }

            // Tıklama olayları
            void OnClick(object s, EventArgs e) => Sidebar_MenuItemClicked(this, "appointments");
            card.Click += OnClick;
            headerBar.Click += OnClick;
            contentPanel.Click += OnClick;
            foreach (Control ctrl in headerBar.Controls)
                ctrl.Click += OnClick;
            foreach (Control ctrl in contentPanel.Controls)
            {
                if (ctrl != btnDetail)
                    ctrl.Click += OnClick;
            }

            parent.Controls.Add(card);
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

            // Büyük sayı
            var lblStat = new LabelControl
            {
                Text = stat,
                Font = new Font("Segoe UI", 36F, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(20, 15),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(100, 50)
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

        private void CreateQuickActionButton(int x, int y, string text, Color color, Action action)
        {
            var btn = new SimpleButton
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(170, 45),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Appearance = { BackColor = color, ForeColor = Color.White },
                AllowFocus = false
            };
            btn.Click += (s, e) => action();
            contentPanel.Controls.Add(btn);
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

        /// <summary>
        /// Hasta profili formunu ac
        /// </summary>
        public void LoadPatientProfile(int patientId)
        {
            LoadChildForm(new FrmPatientProfile(patientId), "Hasta Profili");
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
            this.Name = "FrmDoctorShell";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
        }
    }
}
