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
            _patientService = new PatientService();
            _messageService = new MessageService();
            _goalService = new GoalService();
            _mealService = new MealService();
            _appointmentRepo = new AppointmentRepository();
            _userRepo = new UserRepository();
            SetupUI();
            LoadWelcomeScreen();
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
            sidebar.AddMenuItem("H", "Ana Sayfa", "home");
            sidebar.AddMenuItem("P", "Hastalarım", "patients");
            sidebar.AddMenuItem("D", "Diyet Planları", "plans");
            sidebar.AddMenuItem("O", "Öğünler", "meals");
            sidebar.AddMenuItem("E", "Egzersiz Görevleri", "exercises");
            sidebar.AddMenuItem("V", "Randevular", "appointments");
            sidebar.AddMenuItem("F", "Finansal Özet", "financials");
            sidebar.AddMenuItem("T", "Hedefler", "goals");
            sidebar.AddMenuItem("N", "Notlar", "notes");
            sidebar.AddMenuItem("M", "Mesajlar", "messages");
            sidebar.AddMenuItem("R", "Raporlar", "analytics");
            sidebar.AddMenuItem("I", "AI Analiz", "aianalysis");
            sidebar.AddMenuItem("A", "Ayarlar", "settings");
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
                    LoadChildForm(new FrmHedefler(), "Hasta Hedefleri");
                    break;
                case "notes":
                    LoadChildForm(new FrmNotlar(), "Hasta Notları");
                    break;
                case "messages":
                    LoadChildForm(new FrmMessagesDoctor(), "Mesajlar");
                    break;
                case "analytics":
                    LoadChildForm(new FrmAnalytics(), "Raporlar");
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
            lblCurrentPage.Location = new Point(15, 17);
            btnHome.Visible = false;
            contentPanel.Controls.Clear();

            // Hosgeldin
            var lblWelcome = new LabelControl
            {
                Text = "Hoş Geldiniz, Dr. " + AuthContext.UserName,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, 0),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(700, 40)
            };
            contentPanel.Controls.Add(lblWelcome);

            var lblSubtitle = new LabelControl
            {
                Text = "Bugün nasıl yardımcı olabilirim?",
                Font = new Font("Segoe UI", 12F),
                ForeColor = TextSecondary,
                Location = new Point(0, 45),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(500, 25)
            };
            contentPanel.Controls.Add(lblSubtitle);

            // Gercek istatistikleri al
            int patientCount = 0;
            int mealCount = 0;
            int unreadMessages = 0;
            int totalPatientGoals = 0;

            try
            {
                var patients = _patientService.GetPatientsByDoctor(AuthContext.UserId);
                patientCount = patients.Count;

                // Tum hastalarin aktif hedef sayisi
                foreach (var p in patients)
                {
                    var goals = _goalService.GetActiveGoals(p.Id);
                    totalPatientGoals += goals.Count;
                }

                var meals = _mealService.GetDoctorMeals(AuthContext.UserId);
                mealCount = meals.Count;

                unreadMessages = _messageService.GetUnreadCount(AuthContext.UserId);
            }
            catch { }

            // Dashboard kartlari - 4 kart yan yana
            int startY = 100;
            int cardWidth = 260;
            int cardHeight = 140;
            int spacing = 20;

            CreateDashboardCard(0, startY, cardWidth, cardHeight,
                patientCount.ToString(), "Toplam Hasta", "Hastalarınızı yönetin", PrimaryBlue, "patients");

            CreateDashboardCard(cardWidth + spacing, startY, cardWidth, cardHeight,
                mealCount.ToString(), "Yemek Tarifi", "Öğün kütüphaneniz", SuccessGreen, "meals");

            CreateDashboardCard(2 * (cardWidth + spacing), startY, cardWidth, cardHeight,
                totalPatientGoals.ToString(), "Hasta Hedefleri", "Tüm hastaların hedefleri", PurpleColor, "goals");

            CreateDashboardCard(3 * (cardWidth + spacing), startY, cardWidth, cardHeight,
                unreadMessages.ToString(), "Okunmamış Mesaj", "Mesajlarınızı okuyun", WarningOrange, "messages");

            // Hizli erisim butonlari
            int buttonY = startY + cardHeight + 40;

            var lblQuickActions = new LabelControl
            {
                Text = "Hızlı Erişim",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, buttonY),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(200, 30)
            };
            contentPanel.Controls.Add(lblQuickActions);

            buttonY += 40;

            CreateQuickActionButton(0, buttonY, "Yeni Hasta Ekle", PrimaryBlue, () => {
                LoadChildForm(new FrmPatients(), "Hasta Yonetimi");
            });

            CreateQuickActionButton(180, buttonY, "Diyet Plani Ata", SuccessGreen, () => {
                LoadChildForm(new FrmAssignPlans(), "Diyet Planlari");
            });

            CreateQuickActionButton(360, buttonY, "Yemek Ekle", InfoCyan, () => {
                LoadChildForm(new FrmMeals(), "Yemek Kutuphanesi");
            });

            CreateQuickActionButton(540, buttonY, "Raporlari Gor", DangerRed, () => {
                LoadChildForm(new FrmAnalytics(), "Raporlar");
            });

            // GÜNLÜK RANDEVU TAKVİMİ
            int calendarY = buttonY + 70;
            
            var lblCalendarTitle = new LabelControl
            {
                Text = "📅 Bugünkü Randevular",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = PrimaryBlue,
                Location = new Point(0, calendarY),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(300, 30)
            };
            contentPanel.Controls.Add(lblCalendarTitle);

            calendarY += 40;

            // Bugünkü randevuları getir
            var todayAppointments = _appointmentRepo.GetTodayAppointments(AuthContext.UserId);
            
            if (todayAppointments.Count == 0)
            {
                var lblNoAppointments = new LabelControl
                {
                    Text = "Bugün için randevunuz bulunmuyor.",
                    Font = new Font("Segoe UI", 11F, FontStyle.Italic),
                    ForeColor = TextSecondary,
                    Location = new Point(0, calendarY),
                    AutoSizeMode = LabelAutoSizeMode.None,
                    Size = new Size(400, 25)
                };
                contentPanel.Controls.Add(lblNoAppointments);
            }
            else
            {
                int appointmentX = 0;
                foreach (var apt in todayAppointments.Take(6)) // Max 6 randevu göster
                {
                    var patient = _userRepo.GetById(apt.PatientId);
                    string patientName = patient?.AdSoyad ?? "Hasta";
                    
                    var appointmentCard = new PanelControl
                    {
                        Location = new Point(appointmentX, calendarY),
                        Size = new Size(180, 80),
                        BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple,
                        BackColor = apt.Status == AppointmentStatus.Pending 
                            ? Color.FromArgb(254, 243, 199) 
                            : Color.FromArgb(220, 252, 231),
                        Cursor = Cursors.Hand
                    };

                    var lblTime = new LabelControl
                    {
                        Text = apt.DateTime.ToString("HH:mm"),
                        Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                        ForeColor = PrimaryBlue,
                        Location = new Point(10, 10)
                    };
                    appointmentCard.Controls.Add(lblTime);

                    var lblPatient = new LabelControl
                    {
                        Text = patientName.Length > 18 ? patientName.Substring(0, 15) + "..." : patientName,
                        Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                        ForeColor = TextPrimary,
                        Location = new Point(10, 38),
                        AutoSizeMode = LabelAutoSizeMode.None,
                        Size = new Size(160, 20)
                    };
                    appointmentCard.Controls.Add(lblPatient);

                    var lblType = new LabelControl
                    {
                        Text = apt.TypeText,
                        Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                        ForeColor = TextSecondary,
                        Location = new Point(10, 58)
                    };
                    appointmentCard.Controls.Add(lblType);

                    appointmentCard.Click += (s, e) => Sidebar_MenuItemClicked(this, "appointments");
                    foreach (Control ctrl in appointmentCard.Controls)
                    {
                        ctrl.Click += (s, e) => Sidebar_MenuItemClicked(this, "appointments");
                        ctrl.Cursor = Cursors.Hand;
                    }

                    contentPanel.Controls.Add(appointmentCard);
                    appointmentX += 195;
                }
            }
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
