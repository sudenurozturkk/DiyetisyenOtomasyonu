using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DevExpress.XtraCharts;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrintingLinks;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    /// <summary>
    /// Hasta Hedefleri - Profesyonel Yeni Tasarım
    /// Dashboard kartları, akıllı sihirbaz, takip tablosu, grafikler, AI yorum
    /// </summary>
    public class FrmHedefler : XtraForm
    {
        private readonly GoalService _goalService;
        private readonly PatientService _patientService;
        private readonly AiAssistantService _aiAssistant;
        private readonly WeightEntryRepository _weightRepo;
        private readonly ExerciseTaskRepository _exerciseRepo;

        #region Colors
        private readonly Color PrimaryGreen = ColorTranslator.FromHtml("#0D9488");
        private readonly Color DarkGreen = ColorTranslator.FromHtml("#0F766E");
        private readonly Color LightGreen = ColorTranslator.FromHtml("#CCFBF1");
        private readonly Color SuccessGreen = ColorTranslator.FromHtml("#22C55E");
        private readonly Color SuccessBg = ColorTranslator.FromHtml("#DCFCE7");
        private readonly Color DangerRed = ColorTranslator.FromHtml("#EF4444");
        private readonly Color DangerBg = ColorTranslator.FromHtml("#FEE2E2");
        private readonly Color WarningOrange = ColorTranslator.FromHtml("#F97316");
        private readonly Color WarningBg = ColorTranslator.FromHtml("#FFEDD5");
        private readonly Color InfoBlue = ColorTranslator.FromHtml("#3B82F6");
        private readonly Color InfoBg = ColorTranslator.FromHtml("#DBEAFE");
        private readonly Color PurpleColor = ColorTranslator.FromHtml("#8B5CF6");
        private readonly Color PurpleBg = ColorTranslator.FromHtml("#EDE9FE");
        
        private readonly Color BackgroundLight = ColorTranslator.FromHtml("#F8FAFC");
        private readonly Color CardWhite = Color.White;
        private readonly Color BorderGray = ColorTranslator.FromHtml("#E2E8F0");
        private readonly Color InputBorder = ColorTranslator.FromHtml("#CBD5E1");
        private readonly Color TextDark = ColorTranslator.FromHtml("#1E293B");
        private readonly Color TextMedium = ColorTranslator.FromHtml("#64748B");
        #endregion

        #region Controls
        private ComboBoxEdit cmbPatient;
        private ComboBoxEdit cmbGoalType;
        private SpinEdit spnTargetValue;
        private DateEdit dtStartDate, dtEndDate;
        private RadioButton rdDaily, rdWeekly;
        private GridControl gridGoals;
        private GridView viewGoals;
        
        // Dashboard Labels
        private Label lblPatientName, lblPatientAge, lblPatientHeight, lblPatientBMI, lblPatientChronic;
        private Label lblWeightStart, lblWeightCurrent, lblWeightTarget, lblWeightDuration, lblWeightPercent;
        private Label lblStepGoal, lblStepAvg, lblStepPercent;
        private Panel pnlWeightProgress, pnlStepProgress;
        
        // AI Labels
        private Label lblAiEvaluation, lblAiSuggestions;
        
        // Chart controls
        private ChartControl chartWeight, chartWater, chartActivity;
        #endregion

        private BindingList<Goal> _goals;
        private int _selectedPatientId;
        private Domain.Patient _selectedPatient;
        private int _weightProgress = 0;
        private int _stepProgress = 0;
        private int _waterProgress = 0;
        private double[] _weightChartData = new double[0];
        private double[] _waterChartData = new double[0];
        private double[] _activityChartData = new double[0];

        public FrmHedefler()
        {
            InitializeComponent();
            _goalService = new GoalService();
            _patientService = new PatientService();
            _weightRepo = new WeightEntryRepository();
            _exerciseRepo = new ExerciseTaskRepository();
            
            _aiAssistant = new AiAssistantService();
            
            SetupUI();
            LoadPatients();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1280, 820);
            this.Name = "FrmHedefler";
            this.Text = "Hasta Hedefleri";
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = BackgroundLight;
            this.Padding = new Padding(20);
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            var innerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1240, 950),
                BackColor = Color.Transparent
            };

            int y = 0;

            // 1. Hasta Seçimi ve Profil Header
            var headerCard = CreatePatientSelectionHeader();
            headerCard.Location = new Point(0, y);
            innerPanel.Controls.Add(headerCard);
            y += 55;

            // 2. Üst Dashboard Kartları (Kilo, Profil, Aktivite)
            var dashboardRow = CreateDashboardRow();
            dashboardRow.Location = new Point(0, y);
            innerPanel.Controls.Add(dashboardRow);
            y += 175;

            // 3. Orta Bölüm (Sihirbaz + Takip Tablosu)
            var middleRow = CreateMiddleRow();
            middleRow.Location = new Point(0, y);
            innerPanel.Controls.Add(middleRow);
            y += 290;

            // 4. Alt Bölüm (Grafikler + AI Yorum)
            var bottomRow = CreateBottomRow();
            bottomRow.Location = new Point(0, y);
            innerPanel.Controls.Add(bottomRow);

            scrollPanel.Controls.Add(innerPanel);
            this.Controls.Add(scrollPanel);
        }

        #region Header - Hasta Seçimi
        private Panel CreatePatientSelectionHeader()
        {
            var card = new Panel
            {
                Size = new Size(600, 48),
                BackColor = CardWhite
            };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 8);

            var lblIcon = new Label
            {
                Text = "👤",
                Font = new Font("Segoe UI", 12F),
                Location = new Point(12, 12),
                AutoSize = true
            };
            card.Controls.Add(lblIcon);

            var lblTitle = new Label
            {
                Text = "Hasta Seçimi ve Profili",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(38, 6),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            var lblSelect = new Label
            {
                Text = "Hasta Seçiniz:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMedium,
                Location = new Point(38, 26),
                AutoSize = true
            };
            card.Controls.Add(lblSelect);

            cmbPatient = new ComboBoxEdit
            {
                Location = new Point(130, 22),
                Size = new Size(250, 24)
            };
            cmbPatient.Properties.NullText = "Ahmet Yılmaz (35, Erkek)";
            cmbPatient.SelectedIndexChanged += CmbPatient_SelectedIndexChanged;
            card.Controls.Add(cmbPatient);

            return card;
        }
        #endregion

        #region Dashboard Row - Kilo, Profil, Aktivite
        private Panel CreateDashboardRow()
        {
            var row = new Panel
            {
                Size = new Size(1220, 165),
                BackColor = Color.Transparent
            };

            // Kilo Hedefi Kartı
            var kiloCard = CreateKiloCard();
            kiloCard.Location = new Point(0, 0);
            row.Controls.Add(kiloCard);

            // Hasta Profili Kartı
            var profileCard = CreateProfileCard();
            profileCard.Location = new Point(320, 0);
            row.Controls.Add(profileCard);

            // Adım/Aktivite Kartı
            var activityCard = CreateActivityCard();
            activityCard.Location = new Point(620, 0);
            row.Controls.Add(activityCard);

            return row;
        }

        private Panel CreateKiloCard()
        {
            var card = new Panel { Size = new Size(305, 160), BackColor = CardWhite };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 10);

            // Header
            var lblTitle = new Label
            {
                Text = "Kilo Hedefi",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            // Nav buttons
            var btnPrev = new Label { Text = "◀", Font = new Font("Segoe UI", 10F), ForeColor = PrimaryGreen, Location = new Point(230, 10), AutoSize = true, Cursor = Cursors.Hand };
            var btnNext = new Label { Text = "▶", Font = new Font("Segoe UI", 10F), ForeColor = PrimaryGreen, Location = new Point(260, 10), AutoSize = true, Cursor = Cursors.Hand };
            card.Controls.Add(btnPrev);
            card.Controls.Add(btnNext);

            // Stats
            lblWeightStart = new Label { Text = "Başlangıç: 84 kg", Font = new Font("Segoe UI", 9F), ForeColor = TextDark, Location = new Point(15, 38), AutoSize = true };
            lblWeightCurrent = new Label { Text = "Mevcut: 82.5 kg", Font = new Font("Segoe UI", 9F), ForeColor = TextDark, Location = new Point(15, 56), AutoSize = true };
            lblWeightTarget = new Label { Text = "Hedef: 75 kg", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = PrimaryGreen, Location = new Point(150, 38), AutoSize = true };
            lblWeightDuration = new Label { Text = "⏱ Süre: 8 hafta", Font = new Font("Segoe UI", 9F), ForeColor = TextMedium, Location = new Point(150, 56), AutoSize = true };
            card.Controls.Add(lblWeightStart);
            card.Controls.Add(lblWeightCurrent);
            card.Controls.Add(lblWeightTarget);
            card.Controls.Add(lblWeightDuration);

            // Progress bar
            pnlWeightProgress = new Panel { Location = new Point(15, 85), Size = new Size(200, 10), BackColor = Color.Transparent };
            pnlWeightProgress.Paint += (s, e) => DrawProgressBar(e.Graphics, 200, 10, _weightProgress, PrimaryGreen);
            card.Controls.Add(pnlWeightProgress);

            // Percent
            lblWeightPercent = new Label
            {
                Text = $"~{_weightProgress}%",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(225, 75),
                AutoSize = true
            };
            card.Controls.Add(lblWeightPercent);

            // AI Suggestion
            var lblAiTip = new Label
            {
                Text = "🤖 AI Önerisi: Bu hasta için önerilen haftalık kilo kaybı: 0.5 - 0.8 kg",
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = PrimaryGreen,
                Location = new Point(15, 105),
                Size = new Size(280, 35)
            };
            card.Controls.Add(lblAiTip);

            var lblWarning = new Label
            {
                Text = "⚠ If input is risky, hedef değer ediliyet olmuyor.",
                Font = new Font("Segoe UI", 7F),
                ForeColor = DangerRed,
                Location = new Point(15, 138),
                AutoSize = true
            };
            card.Controls.Add(lblWarning);

            return card;
        }

        private Panel CreateProfileCard()
        {
            var card = new Panel { Size = new Size(285, 160), BackColor = CardWhite };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 10);

            var lblTitle = new Label
            {
                Text = "Hasta Profili:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMedium,
                Location = new Point(15, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            lblPatientName = new Label
            {
                Text = "Ahmet Yılmaz",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = PrimaryGreen,
                Location = new Point(15, 28),
                AutoSize = true
            };
            card.Controls.Add(lblPatientName);

            lblPatientAge = new Label { Text = "Yaş: 35", Font = new Font("Segoe UI", 9F), ForeColor = TextDark, Location = new Point(15, 52), AutoSize = true };
            lblPatientHeight = new Label { Text = "Boy: 178 cm", Font = new Font("Segoe UI", 9F), ForeColor = TextDark, Location = new Point(15, 70), AutoSize = true };
            lblPatientBMI = new Label { Text = "BMI: 26.5", Font = new Font("Segoe UI", 9F), ForeColor = TextDark, Location = new Point(15, 88), AutoSize = true };
            lblPatientChronic = new Label { Text = "Kronik Hastalıklar: Yok", Font = new Font("Segoe UI", 9F), ForeColor = TextDark, Location = new Point(15, 106), Size = new Size(150, 40) };
            card.Controls.Add(lblPatientAge);
            card.Controls.Add(lblPatientHeight);
            card.Controls.Add(lblPatientBMI);
            card.Controls.Add(lblPatientChronic);

            // Profile picture placeholder
            var picPanel = new Panel { Location = new Point(185, 35), Size = new Size(85, 110), BackColor = LightGreen };
            picPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var font = new Font("Segoe UI", 32F))
                using (var brush = new SolidBrush(PrimaryGreen))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString("👤", font, brush, new RectangleF(0, 0, 85, 110), sf);
            };
            card.Controls.Add(picPanel);

            return card;
        }

        private Panel CreateActivityCard()
        {
            var card = new Panel { Size = new Size(590, 160), BackColor = CardWhite };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 10);

            var lblTitle = new Label
            {
                Text = "Adım / Aktivite",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            // Running icons
            var lblIcons = new Label
            {
                Text = "🏃‍♂️ 🏃‍♀️",
                Font = new Font("Segoe UI", 14F),
                ForeColor = SuccessGreen,
                Location = new Point(500, 8),
                AutoSize = true
            };
            card.Controls.Add(lblIcons);

            lblStepGoal = new Label
            {
                Text = "Günlük hedef: 8.000 adım",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 42),
                AutoSize = true
            };
            card.Controls.Add(lblStepGoal);

            lblStepAvg = new Label
            {
                Text = "Haftalık ortalama: 6.200",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMedium,
                Location = new Point(15, 65),
                AutoSize = true
            };
            card.Controls.Add(lblStepAvg);

            // Progress bar
            pnlStepProgress = new Panel { Location = new Point(15, 95), Size = new Size(350, 12), BackColor = Color.Transparent };
            pnlStepProgress.Paint += (s, e) => DrawProgressBar(e.Graphics, 350, 12, _stepProgress, SuccessGreen);
            card.Controls.Add(pnlStepProgress);

            // Circular percentage
            var pnlCircle = new Panel { Location = new Point(440, 35), Size = new Size(120, 120), BackColor = Color.Transparent };
            pnlCircle.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderGray, 10)) e.Graphics.DrawEllipse(pen, 10, 10, 95, 95);
                using (var pen = new Pen(SuccessGreen, 10)) e.Graphics.DrawArc(pen, 10, 10, 95, 95, -90, (int)(360 * _stepProgress / 100.0));
                using (var font = new Font("Segoe UI", 16F, FontStyle.Bold))
                using (var brush = new SolidBrush(PrimaryGreen))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString($"~{_stepProgress}%", font, brush, new RectangleF(0, 0, 120, 120), sf);
            };
            card.Controls.Add(pnlCircle);

            return card;
        }
        #endregion

        #region Middle Row - Sihirbaz + Takip Tablosu
        private Panel CreateMiddleRow()
        {
            var row = new Panel { Size = new Size(1220, 280), BackColor = Color.Transparent };

            // Akıllı Hedef Sihirbazı
            var wizardCard = CreateWizardCard();
            wizardCard.Location = new Point(0, 0);
            row.Controls.Add(wizardCard);

            // Hedef Takip Tablosu
            var trackingCard = CreateTrackingCard();
            trackingCard.Location = new Point(480, 0);
            row.Controls.Add(trackingCard);

            return row;
        }

        private Panel CreateWizardCard()
        {
            var card = new Panel { Size = new Size(465, 275), BackColor = CardWhite };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 10);

            var lblTitle = new Label
            {
                Text = "Akıllı Hedef Sihirbazı",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            int y = 40;

            // ① Hedef Türü
            var lblStep1 = new Label { Text = "① Hedef Türü", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = TextMedium, Location = new Point(15, y), AutoSize = true };
            card.Controls.Add(lblStep1);
            y += 22;

            // Icon buttons
            string[] icons = { "⚖️", "💧", "👟", "🏋️", "📏" };
            string[] names = { "Kilo", "Su", "Adım", "Egzersiz", "Ölçü" };
            for (int i = 0; i < icons.Length; i++)
            {
                var idx = i;
                var btn = new Panel
                {
                    Location = new Point(15 + i * 55, y),
                    Size = new Size(50, 48),
                    BackColor = i == 0 ? LightGreen : CardWhite,
                    Tag = i
                };
                btn.Paint += (s, e) =>
                {
                    DrawRoundedBorder(e.Graphics, (Panel)s, 6);
                    using (var font = new Font("Segoe UI", 14F))
                    using (var brush = new SolidBrush(TextDark))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        e.Graphics.DrawString(icons[idx], font, brush, new RectangleF(0, 0, 50, 35), sf);
                };
                var lbl = new Label
                {
                    Text = names[i],
                    Font = new Font("Segoe UI", 7F),
                    ForeColor = TextMedium,
                    Location = new Point(0, 34),
                    Size = new Size(50, 12),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                btn.Controls.Add(lbl);
                btn.Click += (s, e) => SelectWizardIcon(idx);
                foreach(Control c in btn.Controls) c.Click += (s, e) => SelectWizardIcon(idx);
                
                card.Controls.Add(btn);
            }
            y += 55;

            // Dropdown
            cmbGoalType = new ComboBoxEdit { Location = new Point(15, y), Size = new Size(180, 26) };
            cmbGoalType.Properties.Items.AddRange(new[] { "Hedef Türünü Seçiniz", "Kilo Verme", "Su İçme", "Adım", "Egzersiz", "Ölçü" });
            cmbGoalType.SelectedIndex = 0;
            card.Controls.Add(cmbGoalType);

            // ② Süre & Takip
            var lblStep2 = new Label { Text = "② Süre & Takip", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = TextMedium, Location = new Point(230, y - 32), AutoSize = true };
            card.Controls.Add(lblStep2);

            var lblDates = new Label { Text = "Başlangıç / Bitiş", Font = new Font("Segoe UI", 8F), ForeColor = TextMedium, Location = new Point(230, y - 12), AutoSize = true };
            card.Controls.Add(lblDates);

            dtStartDate = new DateEdit { Location = new Point(330, y - 15), Size = new Size(120, 24) };
            dtStartDate.DateTime = DateTime.Today;
            card.Controls.Add(dtStartDate);

            dtEndDate = new DateEdit { Location = new Point(330, y + 15), Size = new Size(120, 24) };
            dtEndDate.DateTime = DateTime.Today.AddMonths(1);
            card.Controls.Add(dtEndDate);

            // Daily/Weekly
            rdDaily = new RadioButton { Text = "Günlük", Font = new Font("Segoe UI", 8F), Location = new Point(230, y + 42), AutoSize = true, Checked = true };
            rdWeekly = new RadioButton { Text = "Haftalık", Font = new Font("Segoe UI", 8F), Location = new Point(300, y + 42), AutoSize = true };
            card.Controls.Add(rdDaily);
            card.Controls.Add(rdWeekly);
            y += 35;

            // AI Destekli Öneri
            var lblAi = new Label { Text = "🤖 AI Destekli Öneri", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = PrimaryGreen, Location = new Point(15, y + 30), AutoSize = true };
            card.Controls.Add(lblAi);

            var lblAiTip = new Label
            {
                Text = "AI Önerisi: Bu hasta için önerilen haftalık\nkilo kaybı: 0.5 - 0.8 kg  Manuel:",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextMedium,
                Location = new Point(15, y + 48),
                Size = new Size(200, 30)
            };
            card.Controls.Add(lblAiTip);

            spnTargetValue = new SpinEdit { Location = new Point(215, y + 58), Size = new Size(55, 24) };
            spnTargetValue.Properties.MinValue = 0;
            spnTargetValue.Properties.MaxValue = 1000;
            spnTargetValue.Properties.Increment = 0.1m;
            spnTargetValue.EditValue = 0.6;
            card.Controls.Add(spnTargetValue);

            var lblKg = new Label { Text = "kg", Font = new Font("Segoe UI", 8F), ForeColor = TextMedium, Location = new Point(275, y + 62), AutoSize = true };
            card.Controls.Add(lblKg);

            var lblWarn = new Label
            {
                Text = "⚠ If input is risky, hedef değer ediliyet olmuyor.",
                Font = new Font("Segoe UI", 7F),
                ForeColor = DangerRed,
                Location = new Point(15, y + 82),
                AutoSize = true
            };
            card.Controls.Add(lblWarn);

            // Hedef Ekle button
            var btnAdd = new SimpleButton
            {
                Text = "✓ Hedef Ekle",
                Location = new Point(320, y + 55),
                Size = new Size(130, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryGreen, ForeColor = Color.White, BorderColor = PrimaryGreen }
            };
            btnAdd.Click += BtnAddGoal_Click;
            card.Controls.Add(btnAdd);

            return card;
        }

        private Panel CreateTrackingCard()
        {
            var card = new Panel { Size = new Size(730, 275), BackColor = CardWhite };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 10);

            var lblTitle = new Label
            {
                Text = "Hedef Takip Tablosu",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            // Grid
            gridGoals = new GridControl { Location = new Point(15, 40), Size = new Size(700, 180) };
            viewGoals = new GridView(gridGoals)
            {
                OptionsView = { ShowGroupPanel = false, ShowIndicator = false, ColumnAutoWidth = true },
                OptionsBehavior = { Editable = false },
                RowHeight = 40
            };
            viewGoals.Appearance.Row.Font = new Font("Segoe UI", 9F);
            viewGoals.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            viewGoals.Appearance.HeaderPanel.ForeColor = TextMedium;

            viewGoals.Columns.Add(new GridColumn { FieldName = "GoalTypeName", Caption = "Hedef", Width = 80, Visible = true });
            viewGoals.Columns.Add(new GridColumn { FieldName = "Status", Caption = "Durum", Width = 75, Visible = true });
            viewGoals.Columns.Add(new GridColumn { FieldName = "ProgressPercentage", Caption = "İlerleme", Width = 85, Visible = true });
            viewGoals.Columns.Add(new GridColumn { FieldName = "UpdatedAt", Caption = "Son Güncelleme", Width = 100, Visible = true, DisplayFormat = { FormatType = DevExpress.Utils.FormatType.DateTime, FormatString = "dd.MM.yyyy" } });

            // Row styling
            viewGoals.RowCellStyle += (s, e) =>
            {
                if (e.Column.FieldName == "Status")
                {
                    var status = e.CellValue?.ToString() ?? "";
                    if (status.Contains("İyi") || status.Contains("Devam"))
                    {
                        e.Appearance.BackColor = SuccessBg;
                        e.Appearance.ForeColor = SuccessGreen;
                    }
                    else if (status.Contains("Riskli"))
                    {
                        e.Appearance.BackColor = DangerBg;
                        e.Appearance.ForeColor = DangerRed;
                    }
                    else if (status.Contains("New"))
                    {
                        e.Appearance.BackColor = InfoBg;
                        e.Appearance.ForeColor = InfoBlue;
                    }
                    e.Appearance.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
                }
            };

            gridGoals.MainView = viewGoals;
            card.Controls.Add(gridGoals);

            // Buttons
            var btnPdf = new SimpleButton
            {
                Text = "📄 PDF İndir",
                Location = new Point(480, 230),
                Size = new Size(100, 32),
                Font = new Font("Segoe UI", 9F),
                Appearance = { BackColor = CardWhite, ForeColor = TextDark, BorderColor = InputBorder }
            };
            card.Controls.Add(btnPdf);

            var btnShare = new SimpleButton
            {
                Text = "📤 Hasta Paylaş",
                Location = new Point(590, 230),
                Size = new Size(115, 32),
                Font = new Font("Segoe UI", 9F),
                Appearance = { BackColor = CardWhite, ForeColor = TextDark, BorderColor = InputBorder }
            };
            card.Controls.Add(btnShare);

            return card;
        }
        #endregion

        #region Bottom Row - Grafikler + AI Yorum
        private Panel CreateBottomRow()
        {
            var row = new Panel { Size = new Size(1220, 220), BackColor = Color.Transparent };

            // Grafikler kartı
            var chartsCard = CreateChartsCard();
            chartsCard.Location = new Point(0, 0);
            row.Controls.Add(chartsCard);

            // AI Yorum kartı
            var aiCard = CreateAiCard();
            aiCard.Location = new Point(870, 0);
            row.Controls.Add(aiCard);

            return row;
        }

        private Panel CreateChartsCard()
        {
            var card = new Panel { Size = new Size(855, 210), BackColor = CardWhite };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 10);

            var lblTitle = new Label
            {
                Text = "Grafikler",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            // PDF + Paylaş buttons
            var btnPdf = new SimpleButton { Text = "📄 PDF İndir", Location = new Point(590, 8), Size = new Size(95, 28), Font = new Font("Segoe UI", 8F), Appearance = { BackColor = CardWhite, ForeColor = TextDark, BorderColor = InputBorder } };
            btnPdf.Click += BtnPdf_Click;
            var btnShare = new SimpleButton { Text = "📤 Hasta Paylaş", Location = new Point(693, 8), Size = new Size(100, 28), Font = new Font("Segoe UI", 8F), Appearance = { BackColor = CardWhite, ForeColor = TextDark, BorderColor = InputBorder } };
            btnShare.Click += BtnShare_Click;
            card.Controls.Add(btnPdf);
            card.Controls.Add(btnShare);

            // 3 Charts - DevExpress ChartControl
            int chartW = 270, chartH = 160, startY = 40;

            chartWeight = CreateDevExpressChart("Kilo Değişimi", 10, startY, chartW, chartH, ViewType.Area);
            card.Controls.Add(chartWeight);

            chartWater = CreateDevExpressChart("Su Tüketimi", 290, startY, chartW, chartH, ViewType.Bar);
            card.Controls.Add(chartWater);

            chartActivity = CreateDevExpressChart("Aktivite", 570, startY, chartW, chartH, ViewType.Line);
            card.Controls.Add(chartActivity);

            return card;
        }

        private ChartControl CreateDevExpressChart(string title, int x, int y, int w, int h, ViewType viewType)
        {
            var chart = new ChartControl
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BorderOptions = { Visibility = DevExpress.Utils.DefaultBoolean.False },
                Legend = { Visibility = DevExpress.Utils.DefaultBoolean.False }
            };

            // Diagram setup
            // Series - Add BEFORE configuring diagram
            var series = new Series(title, viewType);
            chart.Series.Add(series);

            // Diagram setup - Configure existing diagram
            if (chart.Diagram is XYDiagram diagram)
            {
                diagram.AxisX.Visibility = DevExpress.Utils.DefaultBoolean.True;
                diagram.AxisY.Visibility = DevExpress.Utils.DefaultBoolean.False;
                diagram.AxisX.Label.Font = new Font("Segoe UI", 7F);
                diagram.AxisX.Tickmarks.MinorVisible = false;
                diagram.DefaultPane.BackColor = Color.Transparent;
                diagram.DefaultPane.BorderVisible = false;
            }

            // Title
            var chartTitle = new ChartTitle { Text = title, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            chart.Titles.Add(chartTitle);

            return chart;
        }

        private Panel CreateMiniChart(string title, int x, int y, int w, int h, double[] values, bool isBar, Color color)
        {
            var panel = new Panel { Location = new Point(x, y), Size = new Size(w, h), BackColor = Color.Transparent };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextMedium,
                Location = new Point(0, 0),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            var chartArea = new Panel { Location = new Point(0, 22), Size = new Size(w, h - 25), BackColor = Color.FromArgb(250, 252, 254) };
            chartArea.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                if (values == null || values.Length == 0) return;

                double max = values.Max();
                double min = values.Min();
                double range = max - min;
                if (range == 0) range = 1;

                int barWidth = (w - 20) / values.Length;
                int chartHeight = h - 45;

                for (int i = 0; i < values.Length; i++)
                {
                    int barHeight = (int)((values[i] - min) / range * chartHeight * 0.8);
                    int barX = 10 + i * barWidth;
                    int barY = chartHeight - barHeight;

                    using (var brush = new SolidBrush(color))
                    {
                        if (isBar)
                            e.Graphics.FillRectangle(brush, barX, barY, barWidth - 3, barHeight);
                        else
                            e.Graphics.FillRectangle(brush, barX, barY, barWidth - 3, barHeight);
                    }
                }
            };
            panel.Controls.Add(chartArea);

            return panel;
        }

        private Panel CreateAiCard()
        {
            var card = new Panel { Size = new Size(345, 210), BackColor = CardWhite };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 10);

            var lblTitle = new Label
            {
                Text = "🤖 AI Klinik Yorum",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            // AI Suggest Button
            var btnAiSuggest = new SimpleButton
            {
                Text = "✨ Öneri Sun",
                Location = new Point(230, 8),
                Size = new Size(100, 26),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Appearance = { BackColor = PurpleBg, ForeColor = PurpleColor, BorderColor = PurpleBg }
            };
            btnAiSuggest.Click += BtnAiSuggest_Click;
            card.Controls.Add(btnAiSuggest);

            var lblEvalTitle = new Label
            {
                Text = "AI Değerlendirme",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = PrimaryGreen,
                Location = new Point(15, 38),
                AutoSize = true
            };
            card.Controls.Add(lblEvalTitle);

            lblAiEvaluation = new Label
            {
                Text = "Henüz analiz yapılmadı. 'Öneri Sun' butonuna basarak AI analizi başlatabilirsiniz.",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextDark,
                Location = new Point(15, 56),
                Size = new Size(315, 50)
            };
            card.Controls.Add(lblAiEvaluation);

            var lblSuggestTitle = new Label
            {
                Text = "Öneriler",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = WarningOrange,
                Location = new Point(15, 112),
                AutoSize = true
            };
            card.Controls.Add(lblSuggestTitle);

            lblAiSuggestions = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextDark,
                Location = new Point(15, 130),
                Size = new Size(315, 50)
            };
            card.Controls.Add(lblAiSuggestions);

            return card;
        }
        #endregion

        #region Helpers
        private void DrawRoundedBorder(Graphics g, Panel panel, int radius)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = CreateRoundedRect(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), radius))
            {
                using (var brush = new SolidBrush(panel.BackColor))
                    g.FillPath(brush, path);
                using (var pen = new Pen(BorderGray, 1))
                    g.DrawPath(pen, path);
            }
        }

        private GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void DrawProgressBar(Graphics g, int width, int height, int percent, Color fillColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var bgBrush = new SolidBrush(BorderGray))
                g.FillRectangle(bgBrush, 0, 0, width, height);
            int fillWidth = (int)(width * percent / 100.0);
            using (var fillBrush = new SolidBrush(fillColor))
                g.FillRectangle(fillBrush, 0, 0, fillWidth, height);
        }

        private void LoadPatients()
        {
            try
            {
                var patients = _patientService.GetPatientsByDoctor(AuthContext.UserId);
                cmbPatient.Properties.Items.Clear();
                foreach (var p in patients)
                {
                    cmbPatient.Properties.Items.Add(new PatientItem { Id = p.Id, Name = $"{p.AdSoyad} ({p.Yas}, {p.Cinsiyet})" });
                }
                if (cmbPatient.Properties.Items.Count > 0)
                {
                    cmbPatient.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void CmbPatient_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = cmbPatient.EditValue as PatientItem;
            if (selected != null)
            {
                _selectedPatientId = selected.Id;
                _selectedPatient = _patientService.GetPatientById(_selectedPatientId);
                UpdatePatientProfile();
                LoadGoals();
                LoadChartData();
                LoadAICommentAsync();
            }
        }

        private void UpdatePatientProfile()
        {
            if (_selectedPatient == null) return;

            lblPatientName.Text = _selectedPatient.AdSoyad;
            lblPatientAge.Text = $"Yaş: {_selectedPatient.Yas}";
            lblPatientHeight.Text = $"Boy: {_selectedPatient.Boy} cm";
            lblPatientBMI.Text = $"BMI: {_selectedPatient.BMI:F1}";
            lblPatientChronic.Text = $"Kronik Hastalıklar: {(_selectedPatient.MedicalHistory ?? "Yok")}";

            lblWeightStart.Text = $"Başlangıç: {_selectedPatient.BaslangicKilosu} kg";
            lblWeightCurrent.Text = $"Mevcut: {_selectedPatient.GuncelKilo} kg";
            
            // Hedef kiloyu Goals'dan al veya ideal BMI hesapla
            double targetWeight = 75;
            var kiloGoal = _goals?.FirstOrDefault(g => g.GoalType == GoalType.Weight);
            if (kiloGoal != null)
            {
                targetWeight = kiloGoal.TargetValue;
            }
            else
            {
                targetWeight = 22 * Math.Pow(_selectedPatient.Boy / 100.0, 2);
            }
            lblWeightTarget.Text = $"Hedef: {targetWeight:F0} kg";

            double totalLoss = _selectedPatient.BaslangicKilosu - targetWeight;
            double currentLoss = _selectedPatient.BaslangicKilosu - _selectedPatient.GuncelKilo;
            _weightProgress = totalLoss > 0 ? Math.Min(100, Math.Max(0, (int)(currentLoss / totalLoss * 100))) : 0;
            lblWeightPercent.Text = $"~{_weightProgress}%";
            pnlWeightProgress?.Invalidate();

            // Adım/Aktivite hedefini güncelle
            var stepGoal = _goals?.FirstOrDefault(g => g.GoalType == GoalType.Steps);
            if (stepGoal != null)
            {
                lblStepGoal.Text = $"Günlük hedef: {stepGoal.TargetValue:N0} adım";
                lblStepAvg.Text = $"Haftalık ortalama: {stepGoal.CurrentValue:N0}";
                _stepProgress = (int)stepGoal.ProgressPercentage;
            }
            pnlStepProgress?.Invalidate();
        }

        private void LoadChartData()
        {
            try
            {
                // Kilo verileri - son 12 hafta
                var weightEntries = _weightRepo.GetByPatientId(_selectedPatientId, 90).ToList();
                if (weightEntries.Count > 0)
                {
                    // Haftalık ortalama al
                    var weeklyData = weightEntries
                        .GroupBy(w => (int)((DateTime.Today - w.Date.Date).TotalDays / 7))
                        .OrderBy(g => g.Key)
                        .Take(12)
                        .Select(g => g.Average(w => w.Weight))
                        .Reverse()
                        .ToArray();
                    _weightChartData = weeklyData.Length > 0 ? weeklyData : new[] { _selectedPatient.GuncelKilo };
                }
                else
                {
                    _weightChartData = new[] { _selectedPatient.BaslangicKilosu, _selectedPatient.GuncelKilo };
                }

                // Su tüketimi (hedeflerden simüle et)
                var waterGoal = _goals?.FirstOrDefault(g => g.GoalType == GoalType.Water);
                if (waterGoal != null)
                {
                    var random = new Random(_selectedPatientId);
                    _waterChartData = Enumerable.Range(0, 7)
                        .Select(i => waterGoal.CurrentValue * (0.8 + random.NextDouble() * 0.4))
                        .ToArray();
                    _waterProgress = (int)waterGoal.ProgressPercentage;
                }
                else
                {
                    _waterChartData = new[] { 2.0, 1.8, 2.2, 2.5, 2.3, 1.9, 2.1 };
                }

                // Aktivite verileri - egzersiz tamamlama
                var exercises = _exerciseRepo.GetByPatient(_selectedPatientId).ToList();
                if (exercises.Count > 0)
                {
                    // Haftalık tamamlanan dakika
                    var weeklyActivity = exercises
                        .Where(ex => ex.DueDate >= DateTime.Today.AddDays(-49))
                        .GroupBy(ex => (int)((DateTime.Today - ex.DueDate.Date).TotalDays / 7))
                        .OrderBy(g => g.Key)
                        .Take(7)
                        .Select(g => (double)g.Sum(ex => ex.CompletedDuration))
                        .Reverse()
                        .ToArray();
                    _activityChartData = weeklyActivity.Length > 0 ? weeklyActivity : new[] { 60.0, 75, 80, 65, 90, 70, 85 };
                }
                else
                {
                    _activityChartData = new[] { 60.0, 75, 80, 65, 90, 70, 85 };
                }

                // Grafikleri yenile
                // Grafikleri yenile
                if (chartWeight != null)
                {
                    chartWeight.Series[0].Points.Clear();
                    for (int i = 0; i < _weightChartData.Length; i++)
                        chartWeight.Series[0].Points.Add(new SeriesPoint(i + 1, _weightChartData[i]));
                }

                if (chartWater != null)
                {
                    chartWater.Series[0].Points.Clear();
                    for (int i = 0; i < _waterChartData.Length; i++)
                        chartWater.Series[0].Points.Add(new SeriesPoint(i + 1, _waterChartData[i]));
                }

                if (chartActivity != null)
                {
                    chartActivity.Series[0].Points.Clear();
                    for (int i = 0; i < _activityChartData.Length; i++)
                        chartActivity.Series[0].Points.Add(new SeriesPoint(i + 1, _activityChartData[i]));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chart data loading error: " + ex.Message);
            }
        }

        private void LoadGoals()
        {
            try
            {
                var goals = _goalService.GetActiveGoals(_selectedPatientId);
                _goals = new BindingList<Goal>(goals);
                gridGoals.DataSource = _goals;
            }
            catch { }
        }

        private async void BtnAiSuggest_Click(object sender, EventArgs e)
        {
            if (_selectedPatient == null)
            {
                ToastNotification.ShowError("Lütfen önce bir hasta seçin.");
                return;
            }

            try
            {
                lblAiEvaluation.Text = "⏳ AI analizi yapılıyor, lütfen bekleyin...";
                lblAiSuggestions.Text = "";
                
                var analysis = await _aiAssistant.GetAdvancedAnalysisAsync(_selectedPatientId);
                
                if (analysis != null)
                {
                    lblAiEvaluation.Text = analysis.Result;
                    lblAiSuggestions.Text = analysis.Recommendations;
                }
                else
                {
                    lblAiEvaluation.Text = "Analiz yapılamadı.";
                }
            }
            catch (Exception ex)
            {
                lblAiEvaluation.Text = "Hata oluştu.";
                lblAiSuggestions.Text = ex.Message;
            }
        }
        
        private void LoadAICommentAsync()
        {
            // Otomatik yükleme kaldırıldı, butona basınca yüklenecek
            lblAiEvaluation.Text = "Analiz için 'Öneri Sun' butonuna basın.";
            lblAiSuggestions.Text = "";
        }

        private void BtnAddGoal_Click(object sender, EventArgs e)
        {
            if (_selectedPatientId == 0)
            {
                XtraMessageBox.Show("Lütfen hasta seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                GoalType goalType = GoalType.Weight;
                string unit = "kg";

                switch (cmbGoalType.SelectedIndex)
                {
                    case 1: // Kilo
                        goalType = GoalType.Weight;
                        unit = "kg";
                        break;
                    case 2: // Su
                        goalType = GoalType.Water;
                        unit = "lt";
                        break;
                    case 3: // Adım
                        goalType = GoalType.Steps;
                        unit = "adım";
                        break;
                    case 4: // Egzersiz
                        goalType = GoalType.Exercise;
                        unit = "dk";
                        break;
                    default:
                        goalType = GoalType.Weight;
                        break;
                }

                _goalService.CreateGoal(
                    _selectedPatientId,
                    goalType,
                    (double)spnTargetValue.Value,
                    unit,
                    dtEndDate.DateTime
                );
                
                LoadGoals();
                ToastNotification.ShowSuccess("Hedef eklendi!");
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hedef eklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region PDF ve Paylaş
        private void BtnPdf_Click(object sender, EventArgs e)
        {
            if (_selectedPatient == null)
            {
                ToastNotification.ShowError("Önce hasta seçin.");
                return;
            }

            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PDF Dosyası (*.pdf)|*.pdf";
                    saveDialog.FileName = $"Hedef_Raporu_{_selectedPatient.AdSoyad}_{DateTime.Now:yyyyMMdd}.pdf";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var ps = new PrintingSystem();
                        var link = new CompositeLink(ps);
                        
                        // Header
                        var linkHeader = new Link();
                        linkHeader.CreateDetailArea += (s, args) =>
                        {
                            args.Graph.StringFormat = new BrickStringFormat(StringAlignment.Center);
                            args.Graph.Font = new Font("Segoe UI", 14, FontStyle.Bold);
                            args.Graph.DrawString($"HASTA HEDEF VE İLERLEME RAPORU", Color.Black, new RectangleF(0, 0, 650, 40), BorderSide.None);
                            
                            args.Graph.Font = new Font("Segoe UI", 10);
                            args.Graph.DrawString($"Hasta: {_selectedPatient.AdSoyad} | Tarih: {DateTime.Now:dd.MM.yyyy}", Color.Black, new RectangleF(0, 40, 650, 20), BorderSide.Bottom);
                        };
                        link.Links.Add(linkHeader);

                        // Grid
                        var linkGrid = new PrintableComponentLink(ps);
                        linkGrid.Component = gridGoals;
                        link.Links.Add(linkGrid);

                        // Charts
                        if (chartWeight != null)
                        {
                            var linkChart1 = new PrintableComponentLink(ps);
                            linkChart1.Component = chartWeight;
                            link.Links.Add(linkChart1);
                        }
                        
                        if (chartWater != null)
                        {
                            var linkChart2 = new PrintableComponentLink(ps);
                            linkChart2.Component = chartWater;
                            link.Links.Add(linkChart2);
                        }

                        link.Landscape = false;
                        link.CreateDocument();
                        link.ExportToPdf(saveDialog.FileName);
                        
                        ToastNotification.ShowSuccess("PDF Raporu oluşturuldu!");
                        System.Diagnostics.Process.Start(saveDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                ToastNotification.ShowError("PDF hatası: " + ex.Message);
            }
        }

        private void BtnShare_Click(object sender, EventArgs e)
        {
            if (_selectedPatient == null)
            {
                ToastNotification.ShowError("Önce hasta seçin.");
                return;
            }

            // Hasta ile paylaşım bilgisi göster
            string message = $"Hasta: {_selectedPatient.AdSoyad}\n\n";
            message += "Hedef raporu PDF olarak indirip hastaya iletebilirsiniz.";
            
            XtraMessageBox.Show(message, "Hasta ile Paylaş", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        private void SelectWizardIcon(int index)
        {
            // Update Combo
            // Icons: 0:Kilo, 1:Su, 2:Adım, 3:Egzersiz, 4:Ölçü
            // Combo: 0:Seç, 1:Kilo, 2:Su, 3:Adım, 4:Egzersiz, 5:Ölçü
            if (index >= 0 && index < 5)
            {
                cmbGoalType.SelectedIndex = index + 1;
            }
        }

        private class PatientItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public override string ToString() => Name;
        }
    }
}
