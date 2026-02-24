using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;
using DiyetisyenOtomasyonu.Infrastructure.DI;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    public partial class FrmGoalsNotes : XtraForm
    {
        private readonly GoalService _goalService;
        private readonly PatientService _patientService;
        private readonly GeminiAIService _aiService;

        // Renkler
        private readonly Color Primary = ColorTranslator.FromHtml("#0D9488");
        private readonly Color Success = ColorTranslator.FromHtml("#22C55E");
        private readonly Color Warning = ColorTranslator.FromHtml("#F59E0B");
        private readonly Color Danger = ColorTranslator.FromHtml("#EF4444");
        private readonly Color Info = ColorTranslator.FromHtml("#3B82F6");
        private readonly Color Background = ColorTranslator.FromHtml("#F1F5F9");
        private readonly Color CardBg = Color.White;
        private readonly Color TextDark = ColorTranslator.FromHtml("#1E293B");
        private readonly Color TextGray = ColorTranslator.FromHtml("#64748B");
        private readonly Color Border = ColorTranslator.FromHtml("#E2E8F0");

        // Controls
        private ComboBoxEdit cmbPatient;
        private Label lblWeightStart, lblWeightTarget, lblWeightCurrent;
        private Label lblPatientName, lblPatientAge, lblPatientHeight, lblPatientBMI;
        private Label lblStepTarget, lblStepAvg;
        private ComboBoxEdit cmbGoalType;
        private DateEdit dtStart, dtEnd;
        private SpinEdit spnValue;
        private Label lblAIRecommendation;
        private GridControl gridGoals;
        private GridView gridView;
        private MemoEdit txtAIComment;
        
        // Grafik ve progress panelleri
        private Panel pnlWeightProgress, pnlStepProgress;
        private Panel pnlLineChart, pnlBarChart;
        
        private int _weightPercent = 0;
        private int _stepPercent = 0;
        private double[] _lineChartData;
        private int[] _barChartData;

        private Domain.Patient _patient;

        public FrmGoalsNotes()
        {
            var container = ServiceContainer.Instance;
            _goalService = container.GetService<GoalService>();
            _patientService = container.GetService<PatientService>();
            
            string apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
                // API key'inizi buraya yazın
                apiKey = "API_KEYINIZI_YAZIN";
            _aiService = new GeminiAIService(apiKey);

            InitializeComponent();
            SetupUI();
            LoadPatients();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1150, 780);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Background;
            this.Name = "FrmGoalsNotes";
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            // Ust bolum - Hasta Secimi ve Profil (yukseklik: 195)
            CreateTopSection();
            
            // Orta bolum - Hedef Sihirbazi + Hedef Tablosu (yukseklik: 260)
            CreateMiddleSection();
            
            // Alt bolum - Grafikler + AI Yorum (yukseklik: 290)
            CreateBottomSection();
        }

        private void CreateTopSection()
        {
            var topPanel = new Panel
            {
                Location = new Point(15, 10),
                Size = new Size(this.Width - 30, 195),
                BackColor = CardBg
            };
            topPanel.Paint += (s, e) => DrawBorder(e.Graphics, topPanel);

            // Baslik
            var lblTitle = new Label
            {
                Text = "Hasta Secimi ve Profili",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(35, 12),
                AutoSize = true
            };
            topPanel.Controls.Add(lblTitle);

            // Hasta Secimi
            var lblSelect = new Label
            {
                Text = "Hasta Seciniz:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextGray,
                Location = new Point(240, 15),
                AutoSize = true
            };
            topPanel.Controls.Add(lblSelect);

            cmbPatient = new ComboBoxEdit
            {
                Location = new Point(330, 11),
                Size = new Size(200, 26)
            };
            cmbPatient.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            cmbPatient.SelectedIndexChanged += CmbPatient_Changed;
            topPanel.Controls.Add(cmbPatient);

            // 3 Kart yan yana (her biri ~340px)
            int cardY = 48;
            int cardHeight = 135;
            int cardWidth = 340;
            int gap = 15;

            // Kart 1: Kilo Hedefi
            CreateWeightCard(topPanel, 15, cardY, cardWidth, cardHeight);
            
            // Kart 2: Hasta Profili
            CreateProfileCard(topPanel, 15 + cardWidth + gap, cardY, cardWidth, cardHeight);
            
            // Kart 3: Adim/Aktivite
            CreateActivityCard(topPanel, 15 + (cardWidth + gap) * 2, cardY, cardWidth, cardHeight);

            this.Controls.Add(topPanel);
        }

        private void CreateWeightCard(Panel parent, int x, int y, int w, int h)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = Color.FromArgb(250, 252, 255)
            };
            card.Paint += (s, e) => DrawBorder(e.Graphics, card);

            // Baslik + oklar
            var lblTitle = new Label
            {
                Text = "Kilo Hedefi",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(12, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            var btnLeft = new Label { Text = "<", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Primary, Location = new Point(w - 55, 8), Size = new Size(20, 20), Cursor = Cursors.Hand };
            var btnRight = new Label { Text = ">", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Primary, Location = new Point(w - 30, 8), Size = new Size(20, 20), Cursor = Cursors.Hand };
            card.Controls.Add(btnLeft);
            card.Controls.Add(btnRight);

            // Baslangic
            var lbl1 = new Label { Text = "Baslangic:", Font = new Font("Segoe UI", 8F), ForeColor = TextGray, Location = new Point(12, 38), AutoSize = true };
            lblWeightStart = new Label { Text = "-- kg", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = TextDark, Location = new Point(75, 38), AutoSize = true };
            card.Controls.Add(lbl1);
            card.Controls.Add(lblWeightStart);

            // Hedef
            var lbl2 = new Label { Text = "Hedef:", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Success, Location = new Point(150, 38), AutoSize = true };
            lblWeightTarget = new Label { Text = "-- kg", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Success, Location = new Point(195, 38), AutoSize = true };
            card.Controls.Add(lbl2);
            card.Controls.Add(lblWeightTarget);

            // Mevcut
            var lbl3 = new Label { Text = "Mevcut:", Font = new Font("Segoe UI", 8F), ForeColor = TextGray, Location = new Point(12, 58), AutoSize = true };
            lblWeightCurrent = new Label { Text = "-- kg", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = TextDark, Location = new Point(65, 58), AutoSize = true };
            card.Controls.Add(lbl3);
            card.Controls.Add(lblWeightCurrent);

            // Sure
            var lblSure = new Label { Text = "Sure: 8 hafta", Font = new Font("Segoe UI", 8F), ForeColor = TextGray, Location = new Point(150, 58), AutoSize = true };
            card.Controls.Add(lblSure);

            // Progress bar (yatay)
            var progressPanel = new Panel { Location = new Point(12, 82), Size = new Size(200, 12), BackColor = Color.FromArgb(226, 232, 240) };
            progressPanel.Paint += (s, e) =>
            {
                int fillWidth = (int)(200 * _weightPercent / 100.0);
                e.Graphics.FillRectangle(new SolidBrush(Success), 0, 0, fillWidth, 12);
            };
            card.Controls.Add(progressPanel);
            pnlWeightProgress = progressPanel;

            // Yuzde
            var lblPercent = new Label
            {
                Text = "~58%",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(230, 68),
                AutoSize = true
            };
            card.Controls.Add(lblPercent);

            // AI Onerisi
            lblAIRecommendation = new Label
            {
                Text = "AI Onerisi: Bu hasta icin onerilen haftalik kilo kaybi: 0.5 - 0.8 kg",
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = Info,
                Location = new Point(12, 100),
                Size = new Size(w - 24, 28)
            };
            card.Controls.Add(lblAIRecommendation);

            parent.Controls.Add(card);
        }

        private void CreateProfileCard(Panel parent, int x, int y, int w, int h)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = Color.FromArgb(250, 252, 255)
            };
            card.Paint += (s, e) => DrawBorder(e.Graphics, card);

            var lblTitle = new Label
            {
                Text = "Hasta Profili",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(12, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            lblPatientName = new Label
            {
                Text = "--",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Primary,
                Location = new Point(12, 35),
                AutoSize = true
            };
            card.Controls.Add(lblPatientName);

            lblPatientAge = new Label { Text = "Yas: --", Font = new Font("Segoe UI", 9F), ForeColor = TextGray, Location = new Point(12, 62), AutoSize = true };
            lblPatientHeight = new Label { Text = "Boy: -- cm", Font = new Font("Segoe UI", 9F), ForeColor = TextGray, Location = new Point(12, 82), AutoSize = true };
            lblPatientBMI = new Label { Text = "BMI: --", Font = new Font("Segoe UI", 9F), ForeColor = TextGray, Location = new Point(12, 102), AutoSize = true };
            var lblDisease = new Label { Text = "Kronik Hastaliklar: Yok", Font = new Font("Segoe UI", 8F), ForeColor = TextGray, Location = new Point(12, 118), Size = new Size(180, 16) };
            
            card.Controls.Add(lblPatientAge);
            card.Controls.Add(lblPatientHeight);
            card.Controls.Add(lblPatientBMI);
            card.Controls.Add(lblDisease);

            // Avatar (sag tarafta dikdortgen)
            var avatarPanel = new Panel
            {
                Location = new Point(w - 90, 35),
                Size = new Size(70, 90),
                BackColor = Color.FromArgb(240, 253, 250)
            };
            avatarPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Primary, 2))
                    e.Graphics.DrawRectangle(pen, 1, 1, 67, 87);
            };
            card.Controls.Add(avatarPanel);

            parent.Controls.Add(card);
        }

        private void CreateActivityCard(Panel parent, int x, int y, int w, int h)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = Color.FromArgb(250, 252, 255)
            };
            card.Paint += (s, e) => DrawBorder(e.Graphics, card);

            var lblTitle = new Label
            {
                Text = "Adim / Aktivite",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(12, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            lblStepTarget = new Label
            {
                Text = "Gunluk hedef: 8.000 adim",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(12, 40),
                AutoSize = true
            };
            card.Controls.Add(lblStepTarget);

            lblStepAvg = new Label
            {
                Text = "Haftalik ortalama: 6.200",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextGray,
                Location = new Point(12, 62),
                AutoSize = true
            };
            card.Controls.Add(lblStepAvg);

            // Progress bar (yatay)
            var progressPanel = new Panel { Location = new Point(12, 88), Size = new Size(180, 10), BackColor = Color.FromArgb(226, 232, 240) };
            progressPanel.Paint += (s, e) =>
            {
                int fillWidth = (int)(180 * _stepPercent / 100.0);
                e.Graphics.FillRectangle(new SolidBrush(Primary), 0, 0, fillWidth, 10);
            };
            card.Controls.Add(progressPanel);
            pnlStepProgress = progressPanel;

            // Dairesel yuzde gosterge (sag tarafta)
            var circlePanel = new Panel
            {
                Location = new Point(w - 100, 35),
                Size = new Size(85, 85),
                BackColor = Color.Transparent
            };
            circlePanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                // Arka plan daire
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 8))
                    e.Graphics.DrawArc(pen, 5, 5, 74, 74, 0, 360);
                // Ilerleme dairesi
                using (var pen = new Pen(Primary, 8))
                    e.Graphics.DrawArc(pen, 5, 5, 74, 74, -90, (int)(360 * _stepPercent / 100.0));
                // Yuzde metni
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString($"~{_stepPercent}%", new Font("Segoe UI", 12, FontStyle.Bold), new SolidBrush(Primary), new RectangleF(0, 0, 85, 85), sf);
            };
            card.Controls.Add(circlePanel);

            parent.Controls.Add(card);
        }

        private void CreateMiddleSection()
        {
            int startY = 215;

            // Sol: Akilli Hedef Sihirbazi
            var wizardPanel = new Panel
            {
                Location = new Point(15, startY),
                Size = new Size(470, 255),
                BackColor = CardBg
            };
            wizardPanel.Paint += (s, e) => DrawBorder(e.Graphics, wizardPanel);

            var lblWizTitle = new Label { Text = "Akilli Hedef Sihirbazi", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = TextDark, Location = new Point(12, 12), AutoSize = true };
            wizardPanel.Controls.Add(lblWizTitle);

            // Hedef Turu label
            var lblType = new Label { Text = "Hedef Turu", Font = new Font("Segoe UI", 9F), ForeColor = TextGray, Location = new Point(12, 42), AutoSize = true };
            wizardPanel.Controls.Add(lblType);

            // Hedef tipi butonlari
            // Buttons: Su, Kilo, Adim, Uyku, Egzersiz
            // Combo indices: 0=Hedef Sec, 1=Su, 2=Kilo, 3=Adim, 4=Uyku, 5=Protein, 6=Kalori, 7=Egzersiz
            string[] types = { "Su", "Kilo", "Adim", "Uyku", "Egzersiz" };
            int[] comboIndices = { 1, 2, 3, 4, 7 }; // Su=1, Kilo=2, Adim=3, Uyku=4, Egzersiz=7
            int btnX = 12;
            for (int i = 0; i < types.Length; i++)
            {
                var btn = new Button
                {
                    Text = types[i],
                    Location = new Point(btnX, 62),
                    Size = new Size(52, 38),
                    Font = new Font("Segoe UI", 8F),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = CardBg,
                    ForeColor = TextDark,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = Border;
                int comboIdx = comboIndices[i];
                btn.Click += (s, e) =>
                {
                    foreach (Control c in wizardPanel.Controls)
                        if (c is Button b && b.Size == new Size(52, 38))
                        {
                            b.BackColor = CardBg;
                            b.ForeColor = TextDark;
                        }
                    btn.BackColor = Primary;
                    btn.ForeColor = Color.White;
                    cmbGoalType.SelectedIndex = comboIdx;
                };
                wizardPanel.Controls.Add(btn);
                btnX += 57;
            }

            // Combo - Enum sirasina gore: Water=0, Weight=1, Steps=2, Sleep=3, Protein=4, Calories=5, Exercise=6
            cmbGoalType = new ComboBoxEdit { Location = new Point(12, 108), Size = new Size(160, 26) };
            cmbGoalType.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            cmbGoalType.Properties.Items.AddRange(new[] { "Hedef Turunu Seciniz", "Su Icme", "Kilo Verme", "Adim", "Uyku", "Protein", "Kalori", "Egzersiz" });
            cmbGoalType.SelectedIndex = 0;
            wizardPanel.Controls.Add(cmbGoalType);

            // Takip Baslangic/Bitis
            var lblPeriod = new Label { Text = "Takip\nbaslangic / Bitis", Font = new Font("Segoe UI", 8F), ForeColor = TextGray, Location = new Point(300, 42), Size = new Size(100, 30) };
            wizardPanel.Controls.Add(lblPeriod);

            dtStart = new DateEdit { Location = new Point(300, 72), Size = new Size(100, 24), DateTime = DateTime.Now };
            dtEnd = new DateEdit { Location = new Point(300, 100), Size = new Size(100, 24), DateTime = DateTime.Now.AddMonths(1) };
            wizardPanel.Controls.Add(dtStart);
            wizardPanel.Controls.Add(dtEnd);

            // Gunluk/Haftalik
            var chkDaily = new CheckEdit { Text = "Gunluk", Location = new Point(300, 130), Font = new Font("Segoe UI", 8F), Checked = true };
            var chkWeekly = new CheckEdit { Text = "Haftalik", Location = new Point(365, 130), Font = new Font("Segoe UI", 8F) };
            wizardPanel.Controls.Add(chkDaily);
            wizardPanel.Controls.Add(chkWeekly);

            // AI Destekli Oneri
            var lblAI = new Label { Text = "AI Destekli Oneri", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Info, Location = new Point(12, 145), AutoSize = true };
            wizardPanel.Controls.Add(lblAI);

            var lblAIText = new Label
            {
                Text = "AI Onerisi: Bu hasta icin onerilen haftalik\nkilo kaybi: 0.5 - 0.8 kg",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextGray,
                Location = new Point(12, 165),
                Size = new Size(250, 35)
            };
            wizardPanel.Controls.Add(lblAIText);

            // Deger girisi
            spnValue = new SpinEdit { Location = new Point(12, 210), Size = new Size(70, 26) };
            spnValue.Properties.MinValue = 0;
            spnValue.Properties.MaxValue = 10000;
            spnValue.EditValue = 0.6m;
            wizardPanel.Controls.Add(spnValue);

            var lblUnit = new Label { Text = "kg", Font = new Font("Segoe UI", 9F), ForeColor = TextGray, Location = new Point(88, 215), AutoSize = true };
            wizardPanel.Controls.Add(lblUnit);

            // Combo degistiginde birim guncelle
            cmbGoalType.SelectedIndexChanged += (s, e) =>
            {
                // Combo index -> GoalType enum: 1=Su(0), 2=Kilo(1), 3=Adim(2), 4=Uyku(3), 5=Protein(4), 6=Kalori(5), 7=Egzersiz(6)
                int selIdx = cmbGoalType.SelectedIndex;
                if (selIdx > 0)
                {
                    GoalType goalType = (GoalType)(selIdx - 1);
                    lblUnit.Text = GetUnitForGoalType(goalType);
                }
                else
                {
                    lblUnit.Text = "birim";
                }
            };

            // Hedef Ekle butonu
            var btnAdd = new SimpleButton
            {
                Text = "Hedef Ekle",
                Location = new Point(300, 205),
                Size = new Size(150, 38),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnAdd.Appearance.BackColor = Success;
            btnAdd.Appearance.ForeColor = Color.White;
            btnAdd.Appearance.Options.UseBackColor = true;
            btnAdd.Appearance.Options.UseForeColor = true;
            btnAdd.Click += BtnAddGoal_Click;
            wizardPanel.Controls.Add(btnAdd);

            this.Controls.Add(wizardPanel);

            // Sag: Hedef Takip Tablosu
            var tablePanel = new Panel
            {
                Location = new Point(500, startY),
                Size = new Size(this.Width - 530, 255),
                BackColor = CardBg
            };
            tablePanel.Paint += (s, e) => DrawBorder(e.Graphics, tablePanel);

            var lblTableTitle = new Label { Text = "Hedef Takip Tablosu", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = TextDark, Location = new Point(12, 12), AutoSize = true };
            tablePanel.Controls.Add(lblTableTitle);

            // Grid
            gridGoals = new GridControl { Location = new Point(10, 40), Size = new Size(tablePanel.Width - 20, 165) };
            gridView = new GridView(gridGoals);
            gridGoals.MainView = gridView;

            gridView.Columns.Add(new GridColumn { Caption = "Hedef Turu", FieldName = "GoalTypeName", VisibleIndex = 0, Width = 70 });
            gridView.Columns.Add(new GridColumn { Caption = "Hedef", FieldName = "TargetText", VisibleIndex = 1, Width = 80 });
            gridView.Columns.Add(new GridColumn { Caption = "Mevcut", FieldName = "CurrentText", VisibleIndex = 2, Width = 80 });
            gridView.Columns.Add(new GridColumn { Caption = "Ilerleme", FieldName = "ProgressText", VisibleIndex = 3, Width = 60 });
            gridView.Columns.Add(new GridColumn { Caption = "Durum", FieldName = "StatusText", VisibleIndex = 4, Width = 75 });
            gridView.Columns.Add(new GridColumn { Caption = "Tarih", FieldName = "DateText", VisibleIndex = 5, Width = 80 });

            gridView.OptionsBehavior.Editable = false;
            gridView.OptionsView.ShowGroupPanel = false;
            gridView.RowHeight = 32;
            gridView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gridView.Appearance.HeaderPanel.ForeColor = TextGray;

            gridView.RowCellStyle += (s, e) =>
            {
                if (e.Column.FieldName == "StatusText")
                {
                    var val = e.CellValue?.ToString();
                    if (val == "Tamamlandi") e.Appearance.ForeColor = Success;
                    else if (val == "Devam") e.Appearance.ForeColor = Warning;
                    else if (val == "Iyi") e.Appearance.ForeColor = Info;
                    e.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
            };

            tablePanel.Controls.Add(gridGoals);

            // Alt butonlar
            var btnPDF = new SimpleButton { Text = "PDF Indir", Location = new Point(tablePanel.Width - 190, 215), Size = new Size(85, 28), Font = new Font("Segoe UI", 8F) };
            btnPDF.Appearance.BackColor = CardBg;
            btnPDF.Appearance.ForeColor = TextDark;
            btnPDF.Appearance.BorderColor = Border;
            btnPDF.Appearance.Options.UseBackColor = true;
            btnPDF.Appearance.Options.UseForeColor = true;
            btnPDF.Appearance.Options.UseBorderColor = true;
            btnPDF.Click += (s, e) => GeneratePDFReport();
            tablePanel.Controls.Add(btnPDF);

            var btnShare = new SimpleButton { Text = "Hasta Paylas", Location = new Point(tablePanel.Width - 95, 215), Size = new Size(85, 28), Font = new Font("Segoe UI", 8F) };
            btnShare.Appearance.BackColor = Primary;
            btnShare.Appearance.ForeColor = Color.White;
            btnShare.Appearance.Options.UseBackColor = true;
            btnShare.Appearance.Options.UseForeColor = true;
            tablePanel.Controls.Add(btnShare);

            this.Controls.Add(tablePanel);
        }

        private void CreateBottomSection()
        {
            int startY = 480;

            // Sol: Grafikler
            var chartPanel = new Panel
            {
                Location = new Point(15, startY),
                Size = new Size(720, 285),
                BackColor = CardBg
            };
            chartPanel.Paint += (s, e) => DrawBorder(e.Graphics, chartPanel);

            var lblChartTitle = new Label { Text = "Grafikler", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = TextDark, Location = new Point(12, 12), AutoSize = true };
            chartPanel.Controls.Add(lblChartTitle);

            var btnPDF2 = new SimpleButton { Text = "PDF Indir", Location = new Point(chartPanel.Width - 180, 10), Size = new Size(80, 26), Font = new Font("Segoe UI", 8F) };
            btnPDF2.Appearance.BackColor = CardBg;
            btnPDF2.Appearance.ForeColor = TextGray;
            btnPDF2.Appearance.BorderColor = Border;
            btnPDF2.Appearance.Options.UseBackColor = true;
            btnPDF2.Appearance.Options.UseForeColor = true;
            btnPDF2.Appearance.Options.UseBorderColor = true;
            btnPDF2.Click += (s, e) => GeneratePDFReport();
            chartPanel.Controls.Add(btnPDF2);

            var btnShare2 = new SimpleButton { Text = "Hasta Paylas", Location = new Point(chartPanel.Width - 90, 10), Size = new Size(80, 26), Font = new Font("Segoe UI", 8F) };
            btnShare2.Appearance.BackColor = Primary;
            btnShare2.Appearance.ForeColor = Color.White;
            btnShare2.Appearance.Options.UseBackColor = true;
            btnShare2.Appearance.Options.UseForeColor = true;
            chartPanel.Controls.Add(btnShare2);

            // Cizgi grafik (sol) - Kilo Takibi
            pnlLineChart = new Panel
            {
                Location = new Point(10, 45),
                Size = new Size(350, 230),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            pnlLineChart.Paint += PaintLineChart;
            chartPanel.Controls.Add(pnlLineChart);

            // Cubuk grafik (sag) - Hedef Ilerleme
            pnlBarChart = new Panel
            {
                Location = new Point(370, 45),
                Size = new Size(350, 230),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            pnlBarChart.Paint += PaintBarChart;
            chartPanel.Controls.Add(pnlBarChart);

            this.Controls.Add(chartPanel);

            // Sag: AI Klinik Yorum
            var aiPanel = new Panel
            {
                Location = new Point(750, startY),
                Size = new Size(this.Width - 780, 285),
                BackColor = CardBg
            };
            aiPanel.Paint += (s, e) => DrawBorder(e.Graphics, aiPanel);

            var lblAITitle = new Label { Text = "AI Klinik Yorum", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = TextDark, Location = new Point(12, 12), AutoSize = true };
            aiPanel.Controls.Add(lblAITitle);

            var btnGetAI = new SimpleButton { Text = "Oneri Sun", Location = new Point(aiPanel.Width - 90, 10), Size = new Size(80, 26), Font = new Font("Segoe UI", 8F) };
            btnGetAI.Appearance.BackColor = Success;
            btnGetAI.Appearance.ForeColor = Color.White;
            btnGetAI.Appearance.Options.UseBackColor = true;
            btnGetAI.Appearance.Options.UseForeColor = true;
            btnGetAI.Click += async (s, e) => await GetAICommentAsync();
            aiPanel.Controls.Add(btnGetAI);

            var lblAILabel = new Label { Text = "AI Degerlendirme", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Info, Location = new Point(12, 45), AutoSize = true };
            aiPanel.Controls.Add(lblAILabel);

            txtAIComment = new MemoEdit
            {
                Location = new Point(10, 68),
                Size = new Size(aiPanel.Width - 20, 205)
            };
            txtAIComment.Properties.ReadOnly = true;
            txtAIComment.Properties.NullValuePrompt = "Analiz icin 'Oneri Sun' butonuna basin.";
            txtAIComment.Font = new Font("Segoe UI", 9.5F);
            aiPanel.Controls.Add(txtAIComment);

            this.Controls.Add(aiPanel);

            // Varsayilan grafik verileri
            _lineChartData = new double[] { 92, 91, 90, 89, 88, 87, 86, 85, 84, 83, 82, 81, 80, 79 };
            _barChartData = new int[] { 60, 75, 50, 80, 90, 85, 70, 55, 95, 80, 70, 88 };
        }

        private void PaintLineChart(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int W = pnlLineChart.Width;
            int H = pnlLineChart.Height;
            if (W < 100 || H < 80) return;

            // Baslik
            using (var titleFont = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.FromArgb(55, 65, 81)))
            {
                g.DrawString("Kilo Degisimi (kg)", titleFont, titleBrush, 15, 8);
            }

            // Cizim alani
            int left = 45, top = 35, right = 15, bottom = 30;
            int chartW = W - left - right;
            int chartH = H - top - bottom;
            if (chartW < 50 || chartH < 40) return;

            // Veri yoksa mesaj goster
            if (_lineChartData == null || _lineChartData.Length < 2)
            {
                using (var font = new Font("Segoe UI", 9))
                using (var brush = new SolidBrush(Color.Gray))
                    g.DrawString("Hasta secin...", font, brush, left + 20, top + chartH / 2);
                return;
            }

            double maxVal = _lineChartData.Max();
            double minVal = _lineChartData.Min();
            double range = maxVal - minVal;
            if (range < 1) { range = 10; maxVal = minVal + 10; }

            // Y ekseni - 5 cizgi
            using (var gridPen = new Pen(Color.FromArgb(230, 230, 230), 1))
            using (var labelFont = new Font("Segoe UI", 7))
            using (var labelBrush = new SolidBrush(Color.FromArgb(120, 120, 120)))
            {
                for (int i = 0; i <= 4; i++)
                {
                    int y = top + (i * chartH / 4);
                    g.DrawLine(gridPen, left, y, left + chartW, y);
                    double val = maxVal - (i * range / 4);
                    g.DrawString(val.ToString("F0"), labelFont, labelBrush, 5, y - 6);
                }
            }

            // Noktalar hesapla
            var pts = new List<PointF>();
            for (int i = 0; i < _lineChartData.Length; i++)
            {
                float px = left + (float)i / (_lineChartData.Length - 1) * chartW;
                float py = top + (float)((maxVal - _lineChartData[i]) / range) * chartH;
                pts.Add(new PointF(px, py));
            }

            // Alan dolgusu
            var areaPts = new List<PointF>(pts);
            areaPts.Add(new PointF(pts[pts.Count - 1].X, top + chartH));
            areaPts.Add(new PointF(pts[0].X, top + chartH));
            using (var areaBrush = new SolidBrush(Color.FromArgb(60, 33, 150, 243)))
                g.FillPolygon(areaBrush, areaPts.ToArray());

            // Cizgi
            using (var linePen = new Pen(Color.FromArgb(33, 150, 243), 2.5f))
                g.DrawLines(linePen, pts.ToArray());

            // Noktalar ve degerler
            using (var dotBrush = new SolidBrush(Color.FromArgb(33, 150, 243)))
            using (var whiteBrush = new SolidBrush(Color.White))
            using (var valFont = new Font("Segoe UI", 7, FontStyle.Bold))
            {
                for (int i = 0; i < pts.Count; i++)
                {
                    g.FillEllipse(dotBrush, pts[i].X - 4, pts[i].Y - 4, 8, 8);
                    g.FillEllipse(whiteBrush, pts[i].X - 2, pts[i].Y - 2, 4, 4);
                    
                    // Ilk, son ve ortadaki noktada deger goster
                    if (i == 0 || i == pts.Count - 1 || i == pts.Count / 2)
                    {
                        string val = _lineChartData[i].ToString("F1");
                        g.DrawString(val, valFont, dotBrush, pts[i].X - 12, pts[i].Y - 18);
                    }
                }
            }

            // X ekseni - Hafta etiketleri
            using (var labelFont = new Font("Segoe UI", 7))
            using (var labelBrush = new SolidBrush(Color.FromArgb(120, 120, 120)))
            {
                string[] labels = { "Hafta 1", "", "", "Hafta 4", "", "", "", "Hafta 8" };
                int step = Math.Max(1, _lineChartData.Length / 8);
                for (int i = 0; i < Math.Min(8, _lineChartData.Length); i += 3)
                {
                    float px = left + (float)i / (_lineChartData.Length - 1) * chartW;
                    g.DrawString($"H{i + 1}", labelFont, labelBrush, px - 8, top + chartH + 5);
                }
            }
        }

        private void PaintBarChart(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int W = pnlBarChart.Width;
            int H = pnlBarChart.Height;
            if (W < 100 || H < 80) return;

            // Baslik
            using (var titleFont = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.FromArgb(55, 65, 81)))
            {
                g.DrawString("Aylik Hedef Ilerleme (%)", titleFont, titleBrush, 15, 8);
            }

            // Cizim alani
            int left = 35, top = 35, right = 15, bottom = 40;
            int chartW = W - left - right;
            int chartH = H - top - bottom;
            if (chartW < 50 || chartH < 40) return;

            // Veri yoksa
            if (_barChartData == null || _barChartData.Length == 0)
            {
                using (var font = new Font("Segoe UI", 9))
                using (var brush = new SolidBrush(Color.Gray))
                    g.DrawString("Veri bekleniyor...", font, brush, left + 20, top + chartH / 2);
                return;
            }

            int barCount = Math.Min(12, _barChartData.Length);
            int gap = 6;
            int barW = (chartW - (barCount - 1) * gap) / barCount;
            if (barW < 8) barW = 8;

            // Y ekseni grid
            using (var gridPen = new Pen(Color.FromArgb(230, 230, 230), 1))
            using (var labelFont = new Font("Segoe UI", 7))
            using (var labelBrush = new SolidBrush(Color.FromArgb(120, 120, 120)))
            {
                for (int i = 0; i <= 4; i++)
                {
                    int y = top + (i * chartH / 4);
                    g.DrawLine(gridPen, left, y, left + chartW, y);
                    int val = 100 - (i * 25);
                    g.DrawString(val.ToString(), labelFont, labelBrush, 8, y - 6);
                }
            }

            // Ay isimleri
            string[] months = { "Oca", "Sub", "Mar", "Nis", "May", "Haz", "Tem", "Agu", "Eyl", "Eki", "Kas", "Ara" };
            Color barColor = Color.FromArgb(33, 150, 243);

            using (var barBrush = new SolidBrush(barColor))
            using (var labelFont = new Font("Segoe UI", 7))
            using (var labelBrush = new SolidBrush(Color.FromArgb(100, 100, 100)))
            using (var valFont = new Font("Segoe UI", 6, FontStyle.Bold))
            {
                for (int i = 0; i < barCount; i++)
                {
                    int val = _barChartData[i];
                    int barH = Math.Max(2, val * chartH / 100);
                    int x = left + i * (barW + gap);
                    int y = top + chartH - barH;

                    // Bar
                    g.FillRectangle(barBrush, x, y, barW, barH);

                    // Deger (bar ustunde)
                    if (val > 0)
                    {
                        g.DrawString(val.ToString(), valFont, barBrush, x + barW / 2 - 6, y - 12);
                    }

                    // Ay etiketi
                    g.DrawString(months[i], labelFont, labelBrush, x, top + chartH + 5);
                }
            }
        }

        private void DrawBorder(Graphics g, Panel panel)
        {
            using (var pen = new Pen(Border, 1))
                g.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        }

        #region Data
        private void LoadPatients()
                {
                    try
                    {
                var patients = _patientService.GetPatientsByDoctor(AuthContext.UserId);
                cmbPatient.Properties.Items.Clear();
                foreach (var p in patients)
                    cmbPatient.Properties.Items.Add(new PatientItem { Id = p.Id, Name = $"{p.AdSoyad} ({p.Yas}, {p.Cinsiyet})" });
                if (cmbPatient.Properties.Items.Count > 0)
                    cmbPatient.SelectedIndex = 0;
            }
            catch { }
        }

        private void CmbPatient_Changed(object sender, EventArgs e)
        {
            var item = cmbPatient.EditValue as PatientItem;
            if (item != null)
            {
                _patient = _patientService.GetPatientById(item.Id);
                UpdatePatientData();
                        LoadGoals();
            }
        }

        private void UpdatePatientData()
        {
            if (_patient == null) return;

            lblWeightStart.Text = $"{_patient.BaslangicKilosu:F0} kg";
            double idealKilo = (_patient.Boy - 100) * 0.9;
            lblWeightTarget.Text = $"{idealKilo:F0} kg";
            lblWeightCurrent.Text = $"{_patient.GuncelKilo:F0} kg";

            double kayip = _patient.BaslangicKilosu - _patient.GuncelKilo;
            double hedefKayip = _patient.BaslangicKilosu - idealKilo;
            _weightPercent = hedefKayip > 0 ? (int)(kayip / hedefKayip * 100) : 0;
            _weightPercent = Math.Max(0, Math.Min(100, _weightPercent));
            pnlWeightProgress?.Invalidate();

            lblPatientName.Text = _patient.AdSoyad;
            lblPatientAge.Text = $"Yas: {_patient.Yas}";
            lblPatientHeight.Text = $"Boy: {_patient.Boy} cm";
            lblPatientBMI.Text = $"BMI: {_patient.BMI:F1}";

            _stepPercent = 78; // Ornek
            pnlStepProgress?.Invalidate();
            
            // AI onerisi
            double weeklyLoss = (_patient.BMI > 25) ? 0.5 + (_patient.BMI - 25) * 0.05 : 0.3;
            weeklyLoss = Math.Min(1.0, weeklyLoss);
            lblAIRecommendation.Text = $"AI Onerisi: Bu hasta icin onerilen haftalik kilo kaybi: {weeklyLoss:F1} - {weeklyLoss + 0.3:F1} kg";

            // Kilo verisi grafik icin - gercekci simulasyon
            var weights = new List<double>();
            double startKilo = _patient.BaslangicKilosu;
            double endKilo = _patient.GuncelKilo;
            int dataPoints = 8; // 8 haftalik veri
            double kiloFark = startKilo - endKilo;
            
            for (int i = 0; i < dataPoints; i++)
            {
                // Dogrusal azalis + kucuk varyasyonlar
                double progress = (double)i / (dataPoints - 1);
                double kilo = startKilo - (kiloFark * progress);
                // Rastgele varyasyon ekle (-0.3 ile +0.3 arasi)
                kilo += (new Random(i * 100).NextDouble() - 0.5) * 0.6;
                weights.Add(Math.Round(kilo, 1));
            }
            // Son deger kesinlikle guncel kilo
            weights[weights.Count - 1] = endKilo;
            _lineChartData = weights.ToArray();
            
            // Bar chart - aylik hedef ilerleme oranlari
            _barChartData = new int[] { 
                45 + new Random(1).Next(30),  // Oca
                50 + new Random(2).Next(30),  // Sub
                55 + new Random(3).Next(30),  // Mar
                60 + new Random(4).Next(30),  // Nis
                65 + new Random(5).Next(30),  // May
                70 + new Random(6).Next(25),  // Haz
                _weightPercent,               // Tem - mevcut ilerleme
                0, 0, 0, 0, 0                 // Gelecek aylar
            };
            
            pnlLineChart?.Invalidate();
            pnlBarChart?.Invalidate();
        }

        private void LoadGoals()
        {
            if (_patient == null) return;
            try
            {
                var goals = _goalService.GetActiveGoals(_patient.Id);
                var list = goals.Select(g => new
                {
                    GoalTypeName = g.GoalTypeName,
                    TargetText = $"{g.TargetValue} {GetUnitForGoalType(g.GoalType)}",
                    CurrentText = $"{g.CurrentValue} {GetUnitForGoalType(g.GoalType)}",
                    StatusText = g.ProgressPercentage >= 100 ? "Tamamlandi" : (g.ProgressPercentage >= 50 ? "Iyi" : "Devam"),
                    ProgressText = $"%{g.ProgressPercentage:F0}",
                    DateText = (g.UpdatedAt ?? g.StartDate).ToString("dd.MM.yyyy")
                }).ToList();
                gridGoals.DataSource = list;
                gridGoals.RefreshDataSource();
            }
            catch { }
        }

        private void BtnAddGoal_Click(object sender, EventArgs e)
        {
            if (_patient == null)
            {
                XtraMessageBox.Show("Lutfen hasta secin!", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbGoalType.SelectedIndex <= 0)
            {
                XtraMessageBox.Show("Lutfen hedef turu secin!", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                // GoalType enum sirasina gore: Water=0, Weight=1, Steps=2, Sleep=3, Protein=4, Calories=5, Exercise=6
                var goalType = (GoalType)(cmbGoalType.SelectedIndex - 1);
                
                // GoalType'a gore DOGRU birim atamasi
                string unit = GetUnitForGoalType(goalType);
                
                _goalService.CreateGoal(_patient.Id, goalType, (double)spnValue.Value, unit, dtEnd.DateTime);
                LoadGoals();
                XtraMessageBox.Show("Hedef eklendi!", "Basarili", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task GetAICommentAsync()
        {
            if (_patient == null) return;
            txtAIComment.Text = "AI analiz yapiliyor...";
            try
            {
                if (_aiService != null)
                {
                    var goals = _goalService.GetActiveGoals(_patient.Id);
                    string goalsText = goals.Count > 0 
                        ? string.Join(", ", goals.Select(g => $"{g.GoalTypeName}: %{g.ProgressPercentage:F0}"))
                        : "Aktif hedef yok";
                    
                    double kiloKaybi = _patient.BaslangicKilosu - _patient.GuncelKilo;
                    double idealKilo = (_patient.Boy - 100) * 0.9;
                    
                    // Tıbbi bilgileri hazırla
                    string kronikHastalik = !string.IsNullOrEmpty(_patient.MedicalHistory) ? _patient.MedicalHistory : "Yok";
                    string ilaclar = !string.IsNullOrEmpty(_patient.Medications) ? _patient.Medications : "Yok";
                    string alerjiler = !string.IsNullOrEmpty(_patient.AllergiesText) ? _patient.AllergiesText : "Yok";
                    
                    string prompt = $"Diyetisyen olarak su hastanin hedef ilerlemesi hakkinda Turkce detayli klinik yorum yaz (5-6 cumlede):\n" +
                        $"Ad: {_patient.AdSoyad}\n" +
                        $"Yas: {_patient.Yas}, Cinsiyet: {_patient.Cinsiyet}\n" +
                        $"Boy: {_patient.Boy}cm, Baslangic Kilo: {_patient.BaslangicKilosu}kg, Mevcut Kilo: {_patient.GuncelKilo}kg\n" +
                        $"Kilo Degisimi: {kiloKaybi:F1}kg, BMI: {_patient.BMI:F1} ({_patient.BMIKategori})\n" +
                        $"Ideal Kilo: {idealKilo:F0}kg\n" +
                        $"Kronik Hastaliklar: {kronikHastalik}\n" +
                        $"Kullanilan Ilaclar: {ilaclar}\n" +
                        $"Alerjiler: {alerjiler}\n" +
                        $"Aktif Hedefler: {goalsText}\n" +
                        "ONEMLI: Kronik hastaliklari ve ilaclari dikkate alarak oneriler sun. Alerjilere dikkat et. Hastanin ilerlemesini degerlendir ve profesyonel oneriler sun.";
                    
                    string response = await _aiService.GetAIResponseAsync(prompt);
                    txtAIComment.Text = response;
                }
                else
                {
                    GenerateDefaultAIComment();
                }
                }
                catch (Exception ex)
                {
                GenerateDefaultAIComment();
            }
        }

        private void GenerateDefaultAIComment()
        {
            if (_patient == null) return;
            
            double kayip = _patient.BaslangicKilosu - _patient.GuncelKilo;
            double idealKilo = (_patient.Boy - 100) * 0.9;
            double kalanKilo = _patient.GuncelKilo - idealKilo;
            
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[{_patient.AdSoyad} - Klinik Degerlendirme]");
            sb.AppendLine();
            
            if (kayip > 0)
            {
                sb.AppendLine($"Hasta baslangicindan bu yana {kayip:F1} kg kilo vermistir. Bu olumlu bir gelisme.");
            }
            else if (kayip < 0)
            {
                sb.AppendLine($"Hasta {Math.Abs(kayip):F1} kg kilo almistir. Beslenme plani gozden gecirilmeli.");
            }
            
            sb.AppendLine($"Mevcut BMI: {_patient.BMI:F1} ({_patient.BMIKategori})");
            
            if (kalanKilo > 2)
            {
                sb.AppendLine($"Ideal kiloya ulasmak icin {kalanKilo:F0} kg daha vermesi gerekmektedir.");
                sb.AppendLine("Oneriler: Porsiyon kontrolu, gunluk 30dk yuruyus, bol su tuketimi.");
            }
            else if (kalanKilo > -2)
            {
                sb.AppendLine("Hasta ideal kilo araligindadir. Mevcut programi surdurmesi onerilir.");
            }
            
            txtAIComment.Text = sb.ToString();
        }

        private void GeneratePDFReport()
        {
            if (_patient == null)
            {
                XtraMessageBox.Show("Lutfen once bir hasta secin!", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // HTML Rapor Olustur
                var html = GenerateHTMLReport();
                
                // Dosya kaydet
                string fileName = $"Hasta_Raporu_{_patient.AdSoyad.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                
                System.IO.File.WriteAllText(filePath, html, System.Text.Encoding.UTF8);
                
                // Tarayicida ac
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
                
                XtraMessageBox.Show($"Rapor masaustune kaydedildi:\n{fileName}\n\nTarayicida PDF olarak kaydetmek icin Ctrl+P yapin.", 
                    "Rapor Olusturuldu", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Rapor olusturulamadi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerateHTMLReport()
        {
            var goals = _goalService.GetActiveGoals(_patient.Id);
            double kiloKaybi = _patient.BaslangicKilosu - _patient.GuncelKilo;
            double idealKilo = (_patient.Boy - 100) * 0.9;
            double bmr = 10 * _patient.GuncelKilo + 6.25 * _patient.Boy - 5 * _patient.Yas + (_patient.Cinsiyet == "Erkek" ? 5 : -161);
            int tdee = (int)(bmr * 1.5);

            var html = new System.Text.StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='tr'>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<title>Hasta Ilerleme Raporu - " + _patient.AdSoyad + "</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; max-width: 800px; margin: 0 auto; padding: 20px; background: #f5f5f5; }");
            html.AppendLine(".header { background: linear-gradient(135deg, #0D9488, #14B8A6); color: white; padding: 30px; border-radius: 10px; margin-bottom: 20px; }");
            html.AppendLine(".header h1 { margin: 0 0 10px 0; }");
            html.AppendLine(".card { background: white; border-radius: 10px; padding: 20px; margin-bottom: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            html.AppendLine(".card h2 { color: #0D9488; border-bottom: 2px solid #0D9488; padding-bottom: 10px; margin-top: 0; }");
            html.AppendLine(".stats { display: flex; flex-wrap: wrap; gap: 15px; }");
            html.AppendLine(".stat-box { flex: 1; min-width: 150px; background: #f8f9fa; padding: 15px; border-radius: 8px; text-align: center; }");
            html.AppendLine(".stat-box .value { font-size: 24px; font-weight: bold; color: #0D9488; }");
            html.AppendLine(".stat-box .label { font-size: 12px; color: #666; }");
            html.AppendLine(".progress-bar { background: #e0e0e0; border-radius: 10px; height: 20px; margin: 10px 0; }");
            html.AppendLine(".progress-fill { background: #22C55E; height: 100%; border-radius: 10px; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; }");
            html.AppendLine("th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }");
            html.AppendLine("th { background: #0D9488; color: white; }");
            html.AppendLine(".success { color: #22C55E; font-weight: bold; }");
            html.AppendLine(".warning { color: #F59E0B; font-weight: bold; }");
            html.AppendLine(".danger { color: #EF4444; font-weight: bold; }");
            html.AppendLine(".footer { text-align: center; color: #666; font-size: 12px; margin-top: 30px; }");
            html.AppendLine("@media print { body { background: white; } .card { box-shadow: none; border: 1px solid #ddd; } }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Header
            html.AppendLine("<div class='header'>");
            html.AppendLine("<h1>DiyetPro - Hasta Ilerleme Raporu</h1>");
            html.AppendLine($"<p>Rapor Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}</p>");
            html.AppendLine("</div>");

            // Hasta Bilgileri
            html.AppendLine("<div class='card'>");
            html.AppendLine("<h2>Hasta Bilgileri</h2>");
            html.AppendLine("<div class='stats'>");
            html.AppendLine($"<div class='stat-box'><div class='value'>{_patient.AdSoyad}</div><div class='label'>Ad Soyad</div></div>");
            html.AppendLine($"<div class='stat-box'><div class='value'>{_patient.Yas}</div><div class='label'>Yas</div></div>");
            html.AppendLine($"<div class='stat-box'><div class='value'>{_patient.Cinsiyet}</div><div class='label'>Cinsiyet</div></div>");
            html.AppendLine($"<div class='stat-box'><div class='value'>{_patient.Boy} cm</div><div class='label'>Boy</div></div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            // Kilo Takibi
            html.AppendLine("<div class='card'>");
            html.AppendLine("<h2>Kilo Takibi</h2>");
            html.AppendLine("<div class='stats'>");
            html.AppendLine($"<div class='stat-box'><div class='value'>{_patient.BaslangicKilosu} kg</div><div class='label'>Baslangic Kilosu</div></div>");
            html.AppendLine($"<div class='stat-box'><div class='value'>{_patient.GuncelKilo} kg</div><div class='label'>Guncel Kilo</div></div>");
            html.AppendLine($"<div class='stat-box'><div class='value'>{idealKilo:F0} kg</div><div class='label'>Ideal Kilo</div></div>");
            string kayipClass = kiloKaybi > 0 ? "success" : (kiloKaybi < 0 ? "danger" : "");
            html.AppendLine($"<div class='stat-box'><div class='value {kayipClass}'>{(kiloKaybi > 0 ? "-" : "+")}{Math.Abs(kiloKaybi):F1} kg</div><div class='label'>Kilo Degisimi</div></div>");
            html.AppendLine("</div>");
            
            // Progress bar
            double progressPct = Math.Max(0, Math.Min(100, (kiloKaybi / (_patient.BaslangicKilosu - idealKilo)) * 100));
            html.AppendLine($"<p><strong>Hedefe Ilerleme: %{progressPct:F0}</strong></p>");
            html.AppendLine($"<div class='progress-bar'><div class='progress-fill' style='width:{progressPct}%'></div></div>");
            html.AppendLine("</div>");

            // BMI Analizi
            html.AppendLine("<div class='card'>");
            html.AppendLine("<h2>BMI Analizi</h2>");
            html.AppendLine("<div class='stats'>");
            html.AppendLine($"<div class='stat-box'><div class='value'>{_patient.BMI:F1}</div><div class='label'>BMI Degeri</div></div>");
            string bmiClass = _patient.BMI < 18.5 || _patient.BMI >= 30 ? "danger" : (_patient.BMI >= 25 ? "warning" : "success");
            html.AppendLine($"<div class='stat-box'><div class='value {bmiClass}'>{_patient.BMIKategori}</div><div class='label'>BMI Kategorisi</div></div>");
            html.AppendLine($"<div class='stat-box'><div class='value'>{tdee}</div><div class='label'>Gunluk Kalori Ihtiyaci (kcal)</div></div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            // Hedefler
            html.AppendLine("<div class='card'>");
            html.AppendLine("<h2>Aktif Hedefler</h2>");
            if (goals.Count > 0)
            {
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>Hedef Turu</th><th>Hedef</th><th>Mevcut</th><th>Ilerleme</th><th>Durum</th></tr>");
                foreach (var g in goals)
                {
                    string statusClass = g.ProgressPercentage >= 100 ? "success" : (g.ProgressPercentage >= 50 ? "warning" : "danger");
                    string status = g.ProgressPercentage >= 100 ? "Tamamlandi" : (g.ProgressPercentage >= 50 ? "Iyi" : "Devam");
                    // GoalType'a gore DOGRU birim kullan
                    string correctUnit = GetUnitForGoalType(g.GoalType);
                    html.AppendLine($"<tr>");
                    html.AppendLine($"<td>{g.GoalTypeName}</td>");
                    html.AppendLine($"<td>{g.TargetValue} {correctUnit}</td>");
                    html.AppendLine($"<td>{g.CurrentValue} {correctUnit}</td>");
                    html.AppendLine($"<td>%{g.ProgressPercentage:F0}</td>");
                    html.AppendLine($"<td class='{statusClass}'>{status}</td>");
                    html.AppendLine($"</tr>");
                }
                html.AppendLine("</table>");
            }
            else
            {
                html.AppendLine("<p>Aktif hedef bulunmamaktadir.</p>");
            }
            html.AppendLine("</div>");

            // Oneriler
            html.AppendLine("<div class='card'>");
            html.AppendLine("<h2>Oneriler</h2>");
            html.AppendLine("<ul>");
            if (_patient.BMI >= 25)
            {
                html.AppendLine("<li>Gunluk kalori aliminizi 300-500 kcal azaltin</li>");
                html.AppendLine("<li>Haftada en az 150 dakika kardiyo egzersizi yapin</li>");
                html.AppendLine("<li>Sekerli ve islenmi gidalardan kacinin</li>");
            }
            else if (_patient.BMI < 18.5)
            {
                html.AppendLine("<li>Gunluk kalori aliminizi 300-500 kcal artirin</li>");
                html.AppendLine("<li>Protein agirlikli beslenmeye odaklanin</li>");
            }
            else
            {
                html.AppendLine("<li>Mevcut beslenme duzeni devam ettirin</li>");
            }
            html.AppendLine("<li>Gunde en az 2-2.5 litre su icin</li>");
            html.AppendLine("<li>Her gun en az 7-8 saat uyuyun</li>");
            html.AppendLine("<li>Duzenli kontrollere devam edin</li>");
            html.AppendLine("</ul>");
            html.AppendLine("</div>");

            // Footer
            html.AppendLine("<div class='footer'>");
            html.AppendLine("<p>Bu rapor DiyetPro sistemi tarafindan otomatik olusturulmustur.</p>");
            html.AppendLine($"<p>Doktor: {AuthContext.UserName}</p>");
            html.AppendLine("</div>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }
        #endregion

        /// <summary>
        /// GoalType'a gore dogru birim dondurur
        /// </summary>
        private string GetUnitForGoalType(GoalType goalType)
        {
            switch (goalType)
            {
                case GoalType.Water: return "litre";      // Su - litre
                case GoalType.Weight: return "kg";        // Kilo - kg
                case GoalType.Steps: return "adim";       // Adim - adim
                case GoalType.Sleep: return "saat";       // Uyku - saat
                case GoalType.Protein: return "gram";     // Protein - gram
                case GoalType.Calories: return "kcal";    // Kalori - kcal
                case GoalType.Exercise: return "dakika";  // Egzersiz - dakika
                default: return "birim";
            }
        }

        private class PatientItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public override string ToString() => Name;
        }
    }

    public class FrmAddGoal : XtraForm
    {
        public GoalType SelectedGoalType { get; private set; }
        public double TargetValue { get; private set; }
        public string Unit { get; private set; }
        public DateTime? EndDate { get; private set; }

        private ComboBoxEdit cmbGoalType;
        private SpinEdit spnTarget;
        private TextEdit txtUnit;
        private DateEdit dateEnd;
        private CheckEdit chkHasEndDate;

        public FrmAddGoal()
        {
            this.Text = "Hedef Ekle";
            this.Size = new Size(420, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);

            int y = 20;
            var lblType = new LabelControl { Text = "Hedef Tipi:", Location = new Point(20, y), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cmbGoalType = new ComboBoxEdit { Location = new Point(130, y - 2), Size = new Size(250, 28) };
            cmbGoalType.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            // Enum sirasina gore: Water=0, Weight=1, Steps=2, Sleep=3, Protein=4, Calories=5, Exercise=6
            cmbGoalType.Properties.Items.AddRange(new[] { "Su Icme", "Kilo Verme", "Adim", "Uyku", "Protein", "Kalori", "Egzersiz" });
            cmbGoalType.SelectedIndex = 0;
            cmbGoalType.SelectedIndexChanged += (s, e) => {
                string[] units = { "litre", "kg", "adim", "saat", "gram", "kcal", "dakika" };
                if (cmbGoalType.SelectedIndex < units.Length)
                    txtUnit.Text = units[cmbGoalType.SelectedIndex];
            };
            this.Controls.Add(lblType);
            this.Controls.Add(cmbGoalType);

            y += 45;
            var lblTarget = new LabelControl { Text = "Hedef Deger:", Location = new Point(20, y), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            spnTarget = new SpinEdit { Location = new Point(130, y - 2), Size = new Size(250, 28) };
            spnTarget.Properties.MinValue = 0;
            spnTarget.Properties.MaxValue = 100000;
            this.Controls.Add(lblTarget);
            this.Controls.Add(spnTarget);

            y += 45;
            var lblUnit = new LabelControl { Text = "Birim:", Location = new Point(20, y), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtUnit = new TextEdit { Location = new Point(130, y - 2), Size = new Size(250, 28), Text = "litre" };
            this.Controls.Add(lblUnit);
            this.Controls.Add(txtUnit);

            y += 45;
            chkHasEndDate = new CheckEdit { Text = "Bitis Tarihi Belirle", Location = new Point(130, y) };
            chkHasEndDate.CheckedChanged += (s, e) => dateEnd.Enabled = chkHasEndDate.Checked;
            this.Controls.Add(chkHasEndDate);

            y += 30;
            var lblEnd = new LabelControl { Text = "Bitis Tarihi:", Location = new Point(20, y), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            dateEnd = new DateEdit { Location = new Point(130, y - 2), Size = new Size(250, 28), Enabled = false, DateTime = DateTime.Now.AddMonths(1) };
            this.Controls.Add(lblEnd);
            this.Controls.Add(dateEnd);

            y += 45;
            var btnOk = new SimpleButton { Text = "Ekle", Location = new Point(200, y), Size = new Size(90, 36), Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnOk.Appearance.BackColor = ColorTranslator.FromHtml("#22C55E");
            btnOk.Appearance.ForeColor = Color.White;
            btnOk.Appearance.Options.UseBackColor = true;
            btnOk.Appearance.Options.UseForeColor = true;
            btnOk.Click += (s, e) => {
                if (spnTarget.Value <= 0) { XtraMessageBox.Show("Hedef deger girin"); return; }
                SelectedGoalType = (GoalType)cmbGoalType.SelectedIndex;
                TargetValue = (double)spnTarget.Value;
                Unit = txtUnit.Text;
                EndDate = chkHasEndDate.Checked ? (DateTime?)dateEnd.DateTime : null;
                DialogResult = DialogResult.OK;
            };
            this.Controls.Add(btnOk);

            var btnCancel = new SimpleButton { Text = "Iptal", Location = new Point(300, y), Size = new Size(80, 36) };
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }
    }
}
