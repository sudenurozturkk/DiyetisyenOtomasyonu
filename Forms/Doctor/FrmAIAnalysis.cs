using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Shared;
using DiyetisyenOtomasyonu.Infrastructure.DI;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    /// <summary>
    /// AI Analiz Paneli - DevExpress + Gercek AI Entegrasyonu
    /// </summary>
    public class FrmAIAnalysis : XtraForm
    {
        private readonly PatientService _patientService;
        private readonly GeminiAIService _aiService;
        private readonly WeightEntryRepository _weightRepo;
        private readonly DietRepository _dietRepo;
        private readonly AiChatRepository _chatRepo;

        // Renkler
        private readonly Color Primary = ColorTranslator.FromHtml("#0D9488");
        private readonly Color PrimaryLight = ColorTranslator.FromHtml("#14B8A6");
        private readonly Color Success = ColorTranslator.FromHtml("#22C55E");
        private readonly Color Warning = ColorTranslator.FromHtml("#F59E0B");
        private readonly Color Danger = ColorTranslator.FromHtml("#EF4444");
        private readonly Color Info = ColorTranslator.FromHtml("#3B82F6");
        private readonly Color Purple = ColorTranslator.FromHtml("#8B5CF6");
        private readonly Color Background = Color.White;
        private readonly Color CardBg = Color.White;
        private readonly Color TextDark = ColorTranslator.FromHtml("#1E293B");
        private readonly Color TextGray = ColorTranslator.FromHtml("#64748B");
        private readonly Color Border = ColorTranslator.FromHtml("#E2E8F0");

        // DevExpress Controls
        private PanelControl mainPanel;
        private ComboBoxEdit cmbPatient;
        private LabelControl lblStatus, lblHeightWeight, lblCalorie, lblBMI;
        private LabelControl lblAnalysisText;
        private MemoEdit txtMealRecommendations, txtHealthRecommendations;
        private MemoEdit txtChat;
        private PanelControl pnlChatHistory;
        private SimpleButton btnGetAIRecommendations;

        // Chart Panels
        private PanelControl pnlWeightChart, pnlCaloriePie, pnlNutritionBars;

        private Domain.Patient _patient;
        private double[] _weightData;
        private int[] _macroData = { 30, 45, 25 }; // Protein, Carb, Fat
        private int[] _nutritionData = { 80, 70, 60, 85, 75 }; // Protein, Carb, Fat, Fiber, Calcium

        public FrmAIAnalysis()
        {
            var container = ServiceContainer.Instance;
            _patientService = container.GetService<PatientService>();
            _weightRepo = new WeightEntryRepository();
            _dietRepo = new DietRepository();
            _chatRepo = new AiChatRepository();
            
            // API Key - ortam degiskeninden veya varsayilan
            string apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                // API key'inizi buraya yazın
                apiKey = "API_KEYINIZI_YAZIN";
            }
            _aiService = new GeminiAIService(apiKey);
            
            InitializeComponent();
            SetupUI();
            LoadPatients();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1200, 800);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Background;
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            mainPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.White
            };
            mainPanel.Appearance.BackColor = Color.White;
            mainPanel.Appearance.Options.UseBackColor = true;
            this.Controls.Add(mainPanel);

            // Header
            CreateHeader();

            // Summary Cards Row
            CreateSummaryCards();

            // Main Content - 2 Columns
            CreateMainContent();

            // Bottom Row - 3 Cards
            CreateBottomRow();
        }

        private void CreateHeader()
        {
            var headerPanel = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(1400, 60),
                Dock = DockStyle.Top,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Primary
            };

            var lblTitle = new LabelControl
            {
                Text = "AI Analiz Paneli",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15)
            };
            headerPanel.Controls.Add(lblTitle);

            // Hasta Secimi Label
            var lblPatient = new LabelControl
            {
                Text = "Hasta:",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                Location = new Point(500, 18)
            };
            headerPanel.Controls.Add(lblPatient);

            // Hasta ComboBox
            cmbPatient = new ComboBoxEdit
            {
                Location = new Point(550, 15),
                Size = new Size(200, 28)
            };
            cmbPatient.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            cmbPatient.SelectedIndexChanged += CmbPatient_Changed;
            headerPanel.Controls.Add(cmbPatient);

            // AI Oneri Al Butonu - BELIRGIN
            btnGetAIRecommendations = new SimpleButton
            {
                Text = "Onerilerini Al",
                Location = new Point(770, 12),
                Size = new Size(120, 36),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnGetAIRecommendations.Appearance.BackColor = Color.White;
            btnGetAIRecommendations.Appearance.ForeColor = Primary;
            btnGetAIRecommendations.Appearance.Options.UseBackColor = true;
            btnGetAIRecommendations.Appearance.Options.UseForeColor = true;
            btnGetAIRecommendations.Click += async (s, e) => await GetAIRecommendationsAsync();
            headerPanel.Controls.Add(btnGetAIRecommendations);

            mainPanel.Controls.Add(headerPanel);
        }

        private void CreateSummaryCards()
        {
            var statsPanel = new PanelControl
            {
                Location = new Point(20, 75),
                Size = new Size(mainPanel.Width - 40, 80),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.White
            };

            int cardWidth = 220;
            int gap = 20;
            int x = 0;

            // Genel Durum
            lblStatus = new LabelControl { Text = "Saglikli", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Success };
            CreateSummaryCard(statsPanel, x, 0, cardWidth, 70, "Genel Durum", lblStatus, Success);
            x += cardWidth + gap;

            // Boy-Kilo
            lblHeightWeight = new LabelControl { Text = "170 cm / 70 kg", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = TextDark };
            CreateSummaryCard(statsPanel, x, 0, cardWidth, 70, "Boy - Kilo", lblHeightWeight, Info);
            x += cardWidth + gap;

            // Gunluk Kalori
            lblCalorie = new LabelControl { Text = "2000 kcal", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Warning };
            CreateSummaryCard(statsPanel, x, 0, cardWidth, 70, "Gunluk Kalori", lblCalorie, Warning);
            x += cardWidth + gap;

            // BMI
            lblBMI = new LabelControl { Text = "24.2 Normal", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Success };
            CreateSummaryCard(statsPanel, x, 0, cardWidth, 70, "BMI", lblBMI, Primary);

            mainPanel.Controls.Add(statsPanel);
        }

        private void CreateSummaryCard(PanelControl parent, int x, int y, int width, int height, string title, LabelControl valueLabel, Color accentColor)
        {
            var card = new PanelControl
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BorderStyle = BorderStyles.Simple,
                BackColor = Color.White
            };
            card.Appearance.BackColor = Color.White;
            card.Appearance.Options.UseBackColor = true;

            // Sol renk cizgisi
            var colorBar = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(5, height),
                BorderStyle = BorderStyles.NoBorder,
                BackColor = accentColor
            };
            card.Controls.Add(colorBar);

            var lblTitle = new LabelControl
            {
                Text = title,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextGray,
                Location = new Point(15, 10)
            };
            card.Controls.Add(lblTitle);

            valueLabel.Location = new Point(15, 35);
            card.Controls.Add(valueLabel);

            parent.Controls.Add(card);
        }

        private void CreateMainContent()
        {
            // Sol Panel - Analiz + Grafikler
            var leftPanel = new PanelControl
            {
                Location = new Point(20, 170),
                Size = new Size(680, 280),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.White
            };
            leftPanel.Appearance.BackColor = Color.White;
            leftPanel.Appearance.BorderColor = Color.White;
            leftPanel.Appearance.Options.UseBackColor = true;
            leftPanel.Appearance.Options.UseBorderColor = true;

            // Header
            var leftHeader = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(leftPanel.Width, 45),
                Dock = DockStyle.Top,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Primary
            };
            var lblLeftTitle = new LabelControl
            {
                Text = "Hasta Analiz Sonuclari",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 12)
            };
            leftHeader.Controls.Add(lblLeftTitle);
            leftPanel.Controls.Add(leftHeader);

            // Analiz Metni
            lblAnalysisText = new LabelControl
            {
                Text = "Hasta secildiginde analiz sonuclari burada gorunecek...",
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextGray,
                Location = new Point(15, 55),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(leftPanel.Width - 30, 40)
            };
            leftPanel.Controls.Add(lblAnalysisText);

            // Kilo Degisimi Grafigi
            var lblWeightTitle = new LabelControl
            {
                Text = "Kilo Degisimi",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 100)
            };
            leftPanel.Controls.Add(lblWeightTitle);

            pnlWeightChart = new PanelControl
            {
                Location = new Point(15, 125),
                Size = new Size(300, 140),
                BorderStyle = BorderStyles.Simple,
                BackColor = Color.White
            };
            pnlWeightChart.Paint += PaintWeightChart;
            leftPanel.Controls.Add(pnlWeightChart);

            // Kalori Dagilimi
            var lblCalorieTitle = new LabelControl
            {
                Text = "Kalori Dagilimi",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(340, 100)
            };
            leftPanel.Controls.Add(lblCalorieTitle);

            pnlCaloriePie = new PanelControl
            {
                Location = new Point(340, 125),
                Size = new Size(320, 140),
                BorderStyle = BorderStyles.Simple,
                BackColor = Color.White
            };
            pnlCaloriePie.Paint += PaintCaloriePie;
            leftPanel.Controls.Add(pnlCaloriePie);

            mainPanel.Controls.Add(leftPanel);

            // Sag Panel - Saglik Onerileri (AI)
            var rightPanel = new PanelControl
            {
                Location = new Point(715, 170),
                Size = new Size(mainPanel.Width - 735, 280),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyles.Simple,
                BackColor = Color.White
            };
            rightPanel.Appearance.BackColor = Color.White;
            rightPanel.Appearance.Options.UseBackColor = true;

            // Header
            var rightHeader = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(rightPanel.Width, 45),
                Dock = DockStyle.Top,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Success
            };
            var lblRightTitle = new LabelControl
            {
                Text = "AI Saglik Onerileri",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 12)
            };
            rightHeader.Controls.Add(lblRightTitle);
            rightPanel.Controls.Add(rightHeader);

            txtHealthRecommendations = new MemoEdit
            {
                Location = new Point(10, 55),
                Size = new Size(rightPanel.Width - 20, 210),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            txtHealthRecommendations.Properties.ReadOnly = true;
            txtHealthRecommendations.Properties.NullValuePrompt = "AI saglik onerilerini almak icin 'AI Onerilerini Al' butonuna tiklayin...";
            txtHealthRecommendations.Font = new Font("Segoe UI", 10F);
            txtHealthRecommendations.BackColor = Color.White;
            txtHealthRecommendations.Properties.Appearance.BackColor = Color.White;
            rightPanel.Controls.Add(txtHealthRecommendations);

            mainPanel.Controls.Add(rightPanel);
        }

        private void CreateBottomRow()
        {
            int bottomY = 465;
            int cardWidth = 370;
            int cardHeight = mainPanel.Height - bottomY - 30;

            // Ogun Onerileri (AI)
            var mealPanel = new PanelControl
            {
                Location = new Point(20, bottomY),
                Size = new Size(cardWidth, cardHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                BorderStyle = BorderStyles.Simple,
                BackColor = Color.White
            };
            mealPanel.Appearance.BackColor = Color.White;
            mealPanel.Appearance.Options.UseBackColor = true;

            var mealHeader = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(mealPanel.Width, 40),
                Dock = DockStyle.Top,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Warning
            };
            var lblMealTitle = new LabelControl
            {
                Text = "AI Ogun Onerileri",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 10)
            };
            mealHeader.Controls.Add(lblMealTitle);
            mealPanel.Controls.Add(mealHeader);

            txtMealRecommendations = new MemoEdit
            {
                Location = new Point(8, 48),
                Size = new Size(mealPanel.Width - 16, cardHeight - 58),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            txtMealRecommendations.Properties.ReadOnly = true;
            txtMealRecommendations.Properties.NullValuePrompt = "AI ogun onerilerini almak icin 'AI Onerilerini Al' butonuna tiklayin...";
            txtMealRecommendations.Font = new Font("Segoe UI", 9.5F);
            txtMealRecommendations.BackColor = Color.White;
            txtMealRecommendations.Properties.Appearance.BackColor = Color.White;
            mealPanel.Controls.Add(txtMealRecommendations);

            mainPanel.Controls.Add(mealPanel);

            // Besin Dengesi
            var nutritionPanel = new PanelControl
            {
                Location = new Point(405, bottomY),
                Size = new Size(cardWidth, cardHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                BorderStyle = BorderStyles.Simple,
                BackColor = Color.White
            };
            nutritionPanel.Appearance.BackColor = Color.White;
            nutritionPanel.Appearance.Options.UseBackColor = true;

            var nutritionHeader = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(nutritionPanel.Width, 40),
                Dock = DockStyle.Top,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Info
            };
            var lblNutritionTitle = new LabelControl
            {
                Text = "Besin Dengesi (Haftalik)",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 10)
            };
            nutritionHeader.Controls.Add(lblNutritionTitle);
            nutritionPanel.Controls.Add(nutritionHeader);

            pnlNutritionBars = new PanelControl
            {
                Location = new Point(8, 48),
                Size = new Size(nutritionPanel.Width - 16, cardHeight - 58),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.White
            };
            pnlNutritionBars.Paint += PaintNutritionBars;
            nutritionPanel.Controls.Add(pnlNutritionBars);

            mainPanel.Controls.Add(nutritionPanel);

            // AI Asistan Chat
            var chatPanel = new PanelControl
            {
                Location = new Point(790, bottomY),
                Size = new Size(mainPanel.Width - 810, cardHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyles.Simple,
                BackColor = Color.White
            };
            chatPanel.Appearance.BackColor = Color.White;
            chatPanel.Appearance.Options.UseBackColor = true;

            var chatHeader = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(chatPanel.Width, 40),
                Dock = DockStyle.Top,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Purple
            };
            var lblChatTitle = new LabelControl
            {
                Text = "AI Asistan",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 10)
            };
            chatHeader.Controls.Add(lblChatTitle);
            chatPanel.Controls.Add(chatHeader);

            pnlChatHistory = new PanelControl
            {
                Location = new Point(8, 48),
                Size = new Size(chatPanel.Width - 16, cardHeight - 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyles.Simple,
                BackColor = Color.White,
                AutoScroll = true
            };
            pnlChatHistory.Appearance.BackColor = Color.White;
            pnlChatHistory.Appearance.Options.UseBackColor = true;
            chatPanel.Controls.Add(pnlChatHistory);

            txtChat = new MemoEdit
            {
                Location = new Point(8, cardHeight - 45),
                Size = new Size(chatPanel.Width - 80, 38),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            txtChat.Properties.NullValuePrompt = "Soru sorun...";
            txtChat.BackColor = Color.White;
            txtChat.Properties.Appearance.BackColor = Color.White;
            chatPanel.Controls.Add(txtChat);

            var btnSend = new SimpleButton
            {
                Text = "Sor",
                Location = new Point(chatPanel.Width - 65, cardHeight - 45),
                Size = new Size(55, 38),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnSend.Appearance.BackColor = Purple;
            btnSend.Appearance.ForeColor = Color.White;
            btnSend.Appearance.Options.UseBackColor = true;
            btnSend.Appearance.Options.UseForeColor = true;
            btnSend.Click += async (s, e) => await SendChatMessageAsync();
            chatPanel.Controls.Add(btnSend);

            AddChatMessage("Merhaba! Hasta hakkinda sorularinizi sorabilirsiniz.", false);

            mainPanel.Controls.Add(chatPanel);
        }

        #region Chart Painting
        private void PaintWeightChart(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (_weightData == null || _weightData.Length == 0)
            {
                g.DrawString("Veri yok", new Font("Segoe UI", 10), new SolidBrush(TextGray), 100, 60);
                return;
            }

            int w = pnlWeightChart.Width - 60;
            int h = pnlWeightChart.Height - 40;
            double max = _weightData.Max() + 2;
            double min = _weightData.Min() - 2;
            double range = max - min;
            if (range == 0) range = 1;

            // Grid lines
            using (var pen = new Pen(Border, 1))
            using (var font = new Font("Segoe UI", 7))
            using (var brush = new SolidBrush(TextGray))
            {
                for (int i = 0; i <= 4; i++)
                {
                    int y = 15 + (i * h / 4);
                    g.DrawLine(pen, 45, y, w + 45, y);
                    double val = max - (i * range / 4);
                    g.DrawString($"{val:F0} kg", font, brush, 5, y - 6);
                }
            }

            // Line chart
            if (_weightData.Length > 1)
            {
                var points = new List<PointF>();
                for (int i = 0; i < _weightData.Length; i++)
                {
                    float x = 45 + (float)(i * w / (_weightData.Length - 1));
                    float y = 15 + (float)((max - _weightData[i]) / range * h);
                    points.Add(new PointF(x, y));
                }

                using (var pen = new Pen(Primary, 2.5f))
                    g.DrawLines(pen, points.ToArray());

                foreach (var pt in points)
                {
                    g.FillEllipse(new SolidBrush(Primary), pt.X - 5, pt.Y - 5, 10, 10);
                    g.FillEllipse(new SolidBrush(Color.White), pt.X - 3, pt.Y - 3, 6, 6);
                }
            }
        }

        private void PaintCaloriePie(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int[] values = _macroData;
            Color[] colors = { Success, Info, Warning };
            string[] labels = { "Protein", "Karbonhidrat", "Yag" };

            int cx = 60, cy = 70, r = 45;
            float startAngle = -90;

            for (int i = 0; i < values.Length; i++)
            {
                float sweep = values[i] * 3.6f;
                g.FillPie(new SolidBrush(colors[i]), cx - r, cy - r, r * 2, r * 2, startAngle, sweep);
                startAngle += sweep;
            }

            g.FillEllipse(new SolidBrush(Color.White), cx - 25, cy - 25, 50, 50);

            using (var font = new Font("Segoe UI", 9))
            using (var boldFont = new Font("Segoe UI", 10, FontStyle.Bold))
            {
                for (int i = 0; i < labels.Length; i++)
                {
                    int y = 15 + i * 40;
                    g.FillRectangle(new SolidBrush(colors[i]), 140, y, 14, 14);
                    g.DrawString(labels[i], font, new SolidBrush(TextDark), 160, y - 2);
                    g.DrawString($"{values[i]}%", boldFont, new SolidBrush(colors[i]), 250, y - 2);
                }
            }
        }

        private void PaintNutritionBars(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            string[] labels = { "Protein", "Karbonhidrat", "Yag", "Lif", "Kalsiyum" };
            Color[] colors = { Success, Info, Warning, Success, Info };

            int y = 10;
            int barWidth = pnlNutritionBars.Width - 120;

            using (var font = new Font("Segoe UI", 9))
            using (var boldFont = new Font("Segoe UI", 9, FontStyle.Bold))
            {
                for (int i = 0; i < labels.Length; i++)
                {
                    g.DrawString(labels[i], font, new SolidBrush(TextDark), 5, y + 2);
                    
                    // Background bar
                    g.FillRectangle(new SolidBrush(Border), 85, y, barWidth, 18);
                    
                    // Value bar
                    int fillWidth = (int)(barWidth * _nutritionData[i] / 100.0);
                    g.FillRectangle(new SolidBrush(colors[i]), 85, y, fillWidth, 18);
                    
                    g.DrawString($"{_nutritionData[i]}%", boldFont, new SolidBrush(colors[i]), barWidth + 95, y + 1);
                    
                    y += 32;
                }
            }
        }
        #endregion

        #region Data Loading
        private void LoadPatients()
        {
            try
            {
                var patients = _patientService.GetPatientsByDoctor(AuthContext.UserId);
                cmbPatient.Properties.Items.Clear();
                foreach (var p in patients)
                    cmbPatient.Properties.Items.Add(new PatientItem { Id = p.Id, Name = p.AdSoyad });
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
            }
        }

        private void UpdatePatientData()
        {
            if (_patient == null) return;

            // Summary cards
            lblHeightWeight.Text = $"{_patient.Boy} cm / {_patient.GuncelKilo} kg";
            
            double bmr = 10 * _patient.GuncelKilo + 6.25 * _patient.Boy - 5 * _patient.Yas + (_patient.Cinsiyet == "Erkek" ? 5 : -161);
            int tdee = (int)(bmr * 1.5);
            lblCalorie.Text = $"{tdee} kcal";

            string category = _patient.BMI < 18.5 ? "Zayif" : _patient.BMI < 25 ? "Normal" : _patient.BMI < 30 ? "Fazla Kilolu" : "Obez";
            lblBMI.Text = $"{_patient.BMI:F1} {category}";
            lblBMI.ForeColor = _patient.BMI >= 25 ? Warning : (_patient.BMI < 18.5 ? Info : Success);

            lblStatus.Text = _patient.BMI >= 18.5 && _patient.BMI < 25 ? "Saglikli" : "Dikkat";
            lblStatus.ForeColor = _patient.BMI >= 18.5 && _patient.BMI < 25 ? Success : Warning;

            lblAnalysisText.Text = $"{_patient.AdSoyad} icin analiz: BMI {_patient.BMI:F1} ({category}). Gunluk kalori ihtiyaci: {tdee} kcal. " +
                                   $"Ideal kilo araligi: {(_patient.Boy - 100) * 0.9:F0} - {(_patient.Boy - 100) * 1.1:F0} kg.";

            // Weight data
            var entries = _weightRepo.GetByPatientId(_patient.Id, 30).OrderBy(w => w.Date).ToList();
            if (entries.Count > 0)
                _weightData = entries.Select(w => w.Weight).ToArray();
            else
                _weightData = new[] { _patient.BaslangicKilosu, _patient.GuncelKilo };
            
            pnlWeightChart?.Invalidate();

            // Calculate macro distribution based on patient data
            CalculateMacros();
            pnlCaloriePie?.Invalidate();

            // Calculate nutrition based on diet
            CalculateNutrition();
            pnlNutritionBars?.Invalidate();

            // AI oneri alanlarini temizle - butonla doldurulacak
            txtHealthRecommendations.Text = "";
            txtMealRecommendations.Text = "";
        }

        private void CalculateMacros()
        {
            if (_patient == null) return;

            // BMI'ye gore makro dagitimi
            if (_patient.BMI > 25) // Kilolu - daha fazla protein
            {
                _macroData = new[] { 35, 40, 25 };
            }
            else if (_patient.BMI < 18.5) // Zayif - daha fazla karb
            {
                _macroData = new[] { 25, 50, 25 };
            }
            else // Normal
            {
                _macroData = new[] { 30, 45, 25 };
            }
        }

        private void CalculateNutrition()
        {
            if (_patient == null) return;

            // Varsayilan degerler - hastaya ozel hesaplama
            int baseProtein = 70 + (_patient.Yas < 40 ? 10 : 0);
            int baseCarb = 65 + (_patient.ActivityLevel == ActivityLevel.VeryActive ? 15 : 0);
            int baseFat = 55 + (_patient.BMI < 25 ? 10 : 0);
            int baseFiber = 75 + (_patient.Cinsiyet == "Kadin" ? 10 : 0);
            int baseCalcium = 70 + (_patient.Yas > 50 ? 10 : 0);

            _nutritionData = new[] { 
                Math.Min(100, baseProtein), 
                Math.Min(100, baseCarb), 
                Math.Min(100, baseFat), 
                Math.Min(100, baseFiber), 
                Math.Min(100, baseCalcium) 
            };
        }
        #endregion

        #region AI Functions
        private async Task GetAIRecommendationsAsync()
        {
            if (_patient == null)
            {
                XtraMessageBox.Show("Lutfen once bir hasta secin!", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnGetAIRecommendations.Enabled = false;
            btnGetAIRecommendations.Text = "Yukleniyor...";
            txtHealthRecommendations.Text = "AI onerileri aliniyor...";
            txtMealRecommendations.Text = "AI onerileri aliniyor...";

            try
            {
                if (_aiService != null)
                {
                    // Hasta verilerini hazirla
                    double idealKilo = (_patient.Boy - 100) * 0.9;
                    string kronikHastalik = !string.IsNullOrEmpty(_patient.MedicalHistory) ? _patient.MedicalHistory : "Yok";
                    string ilaclar = !string.IsNullOrEmpty(_patient.Medications) ? _patient.Medications : "Yok";
                    string alerjiler = !string.IsNullOrEmpty(_patient.AllergiesText) ? _patient.AllergiesText : "Yok";
                    // Eski alerji listesinden de kontrol et
                    if (alerjiler == "Yok" && _patient.Allergies?.Count > 0)
                        alerjiler = string.Join(", ", _patient.Allergies.Select(a => a.AllergyType));
                    
                    // Saglik onerileri - TUM TIBBI BILGILERLE
                    string healthPrompt = $"Diyetisyen olarak, su hasta icin Turkce kisa saglik onerileri yaz (madde madde, 5-6 madde):\n" +
                        $"Ad: {_patient.AdSoyad}, Yas: {_patient.Yas}, Cinsiyet: {_patient.Cinsiyet}\n" +
                        $"Boy: {_patient.Boy}cm, Kilo: {_patient.GuncelKilo}kg, BMI: {_patient.BMI:F1} ({_patient.BMIKategori})\n" +
                        $"Ideal Kilo: {idealKilo:F0}kg, Baslangic Kilosu: {_patient.BaslangicKilosu}kg\n" +
                        $"Kronik Hastaliklar: {kronikHastalik}\n" +
                        $"Kullanilan Ilaclar: {ilaclar}\n" +
                        $"Alerjiler: {alerjiler}\n" +
                        "ONEMLI: Kronik hastaliklara ve ilaclara DIKKAT EDEREK oneriler sun. Ornegin diyabetli hastaya seker uyarisi, tansiyon hastasina tuz uyarisi ver.";
                    
                    string healthResponse = await _aiService.GetAIResponseAsync(healthPrompt);
                    txtHealthRecommendations.Text = healthResponse;

                    // Ogun onerileri - ALERJILERE DIKKAT
                    string mealPrompt = $"Diyetisyen olarak, su hasta icin Turkce gunluk ogun onerisi yaz (Kahvalti, Ogle, Aksam, Ara Ogun):\n" +
                        $"Yas: {_patient.Yas}, Cinsiyet: {_patient.Cinsiyet}, Kilo: {_patient.GuncelKilo}kg, Ideal Kilo: {idealKilo:F0}kg\n" +
                        $"BMI: {_patient.BMI:F1} ({_patient.BMIKategori})\n" +
                        $"Kronik Hastaliklar: {kronikHastalik}\n" +
                        $"Kullanilan Ilaclar: {ilaclar}\n" +
                        $"ALERJILER (KESINLIKLE BUNLARI ICEREN YIYECEK ONERME): {alerjiler}\n" +
                        "Her ogun icin yakla kalori belirt. Alerjilere KESINLIKLE dikkat et, alerjen icerebilecek yiyecekleri ONERME.";
                    
                    string mealResponse = await _aiService.GetAIResponseAsync(mealPrompt);
                    txtMealRecommendations.Text = mealResponse;
                }
                else
                {
                    // AI servisi yok - hastaya ozel hesaplanmis oneriler
                    GeneratePatientSpecificRecommendations();
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda hastaya ozel oneriler goster
                GeneratePatientSpecificRecommendations();
            }
            finally
            {
                btnGetAIRecommendations.Enabled = true;
                btnGetAIRecommendations.Text = "Onerilerini Al";
            }
        }

        private void GeneratePatientSpecificRecommendations()
        {
            if (_patient == null) return;

            string category = _patient.BMI < 18.5 ? "Zayif" : _patient.BMI < 25 ? "Normal" : _patient.BMI < 30 ? "Fazla Kilolu" : "Obez";
            double idealKilo = (_patient.Boy - 100) * 0.9;
            double kiloFark = _patient.GuncelKilo - idealKilo;
            
            // Saglik onerileri - hastaya ozel
            var healthRecs = new List<string>();
            healthRecs.Add($"[{_patient.AdSoyad} icin Ozel Oneriler]");
            healthRecs.Add("");
            
            if (_patient.BMI >= 30)
            {
                healthRecs.Add($"- Hedef: {kiloFark:F0} kg vermek (BMI: {_patient.BMI:F1} -> 25)");
                healthRecs.Add("- Gunluk 500-750 kalori acik olusturun");
                healthRecs.Add("- Haftada 5 gun, 45 dk yuruyus yapin");
                healthRecs.Add("- Sekerli ve islenmi gidalardan kacinin");
                healthRecs.Add("- Porsiyonlarinizi %30 azaltin");
            }
            else if (_patient.BMI >= 25)
            {
                healthRecs.Add($"- Hedef: {kiloFark:F0} kg vermek");
                healthRecs.Add("- Gunluk 300-500 kalori acik olusturun");
                healthRecs.Add("- Haftada en az 150 dakika kardiyo egzersizi");
                healthRecs.Add("- Ekmek ve pilav miktarini azaltin");
            }
            else if (_patient.BMI < 18.5)
            {
                healthRecs.Add($"- Hedef: {Math.Abs(kiloFark):F0} kg almak");
                healthRecs.Add("- Gunluk kalori aliminizi 300-500 artirin");
                healthRecs.Add("- Protein agirlikli beslenin (yumurta, et, sut)");
                healthRecs.Add("- Guc antrenmanlari yapin");
            }
            else
            {
                healthRecs.Add("- Mevcut kilonuzu korumaya devam edin");
                healthRecs.Add("- Duzenli egzersiz yapmaya devam edin");
                healthRecs.Add("- Dengeli beslenmeyi surduru");
            }
            
            healthRecs.Add("");
            healthRecs.Add("- Gunde en az 2-2.5 litre su icin");
            healthRecs.Add("- Her ogun sebze tuketin");
            healthRecs.Add("- Uyku duzeni: 23:00-07:00");
            
            txtHealthRecommendations.Text = string.Join("\n", healthRecs);

            // Ogun onerileri - hastaya ozel kalori hesabi
            double bmr = 10 * _patient.GuncelKilo + 6.25 * _patient.Boy - 5 * _patient.Yas + (_patient.Cinsiyet == "Erkek" ? 5 : -161);
            int tdee = (int)(bmr * 1.5);
            int targetCal = _patient.BMI >= 25 ? tdee - 500 : (_patient.BMI < 18.5 ? tdee + 300 : tdee);
            
            int breakfast = (int)(targetCal * 0.25);
            int lunch = (int)(targetCal * 0.35);
            int dinner = (int)(targetCal * 0.30);
            int snack = (int)(targetCal * 0.10);

            var meals = new List<string>();
            meals.Add($"[{_patient.AdSoyad} - Gunluk {targetCal} kcal]");
            meals.Add("");
            
            meals.Add($"KAHVALTI ({breakfast} kcal):");
            if (_patient.BMI >= 25)
            {
                meals.Add("- 1 dilim tam bugday ekmegi");
                meals.Add("- 2 yumurta beyazi omlet");
                meals.Add("- Az yagli beyaz peynir (30g)");
                meals.Add("- Domates, salatalik, yesil biber");
                meals.Add("- Sekersiz cay");
            }
            else
            {
                meals.Add("- 2 dilim tam bugday ekmegi");
                meals.Add("- 1 tam yumurta (haslanmis)");
                meals.Add("- Beyaz peynir, zeytin");
                meals.Add("- Domates, salatalik");
                meals.Add("- 1 bardak sut");
            }
            
            meals.Add("");
            meals.Add($"OGLE ({lunch} kcal):");
            meals.Add("- 150g izgara tavuk veya balik");
            meals.Add(_patient.BMI >= 25 ? "- 1/2 porsiyon bulgur pilavi" : "- 1 porsiyon bulgur pilavi");
            meals.Add("- Bol yesil salata (limon, zeytinyagi)");
            meals.Add("- 1 kase yogurt");
            
            meals.Add("");
            meals.Add($"AKSAM ({dinner} kcal):");
            meals.Add("- Zeytinyagli sebze yemegi");
            meals.Add("- 100g izgara et veya balik");
            meals.Add("- Salata");
            meals.Add(_patient.BMI >= 25 ? "- Ekmeksiz" : "- 1 dilim ekmek");
            
            meals.Add("");
            meals.Add($"ARA OGUN ({snack} kcal):");
            meals.Add("- 1 porsiyon meyve (elma/armut)");
            meals.Add("- 10 adet badem");

            txtMealRecommendations.Text = string.Join("\n", meals);
        }

        private async Task SendChatMessageAsync()
        {
            string msg = txtChat.Text?.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            AddChatMessage(msg, true);
            txtChat.Text = "";

            if (_aiService != null && _patient != null)
            {
                try
                {
                    // Hasta bilgilerini hazirla
                    string kronik = !string.IsNullOrEmpty(_patient.MedicalHistory) ? _patient.MedicalHistory : "Yok";
                    string ilac = !string.IsNullOrEmpty(_patient.Medications) ? _patient.Medications : "Yok";
                    string alerji = !string.IsNullOrEmpty(_patient.AllergiesText) ? _patient.AllergiesText : "Yok";
                    
                    string prompt = $"Diyetisyen asistani olarak cevap ver.\n" +
                        $"HASTA VERILERI:\n" +
                        $"- Ad: {_patient.AdSoyad}\n" +
                        $"- Yas: {_patient.Yas}\n" + 
                        $"- Kilo: {_patient.GuncelKilo}kg, BMI: {_patient.BMI:F1}\n" +
                        $"- Kronik Hastalik: {kronik}\n" +
                        $"- Ilaclar: {ilac}\n" + 
                        $"- Alerjiler: {alerji}\n\n" +
                        $"SORU: {msg}\n\n" +
                        "ONEMLI KURALLAR:\n" +
                        "1. SADECE yukaridaki verilere dayanarak cevap ver.\n" +
                        "2. Verilmemis bilgileri (aile gecmisi, baska hastalik) ASLA uydurma.\n" +
                        "3. Kisa, profesyonel ve Turkce cevap ver.\n" +
                        "4. Emin olmadigin seyleri tahmin etme, 'bilinmiyor' de.";

                string response = await _aiService.GetAIResponseAsync(prompt);
                    AddChatMessage(response, false);
            }
            catch (Exception ex)
            {
                    AddChatMessage("Hata: " + ex.Message, false);
                }
            }
            else
            {
                AddChatMessage("AI servisi aktif degil. Lutfen API anahtarini kontrol edin.", false);
            }
        }

        private void AddChatMessage(string msg, bool isUser)
        {
            // Mesaj uzunluguna gore dinamik yukseklik hesapla
            int maxWidth = pnlChatHistory.Width - 40;
            int charPerLine = maxWidth / 7; // Yaklasik karakter genisligi
            int lineCount = (int)Math.Ceiling((double)msg.Length / charPerLine);
            int height = Math.Max(40, Math.Min(200, lineCount * 18 + 10)); // Min 40, Max 200
            
            var lbl = new LabelControl
            {
                Text = msg,
                Font = new Font("Segoe UI", 9),
                ForeColor = isUser ? Color.White : TextDark,
                BackColor = isUser ? Primary : Color.FromArgb(240, 253, 250),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(maxWidth, height),
                Appearance = { TextOptions = { WordWrap = DevExpress.Utils.WordWrap.Wrap } }
            };

            int y = 5;
            foreach (Control c in pnlChatHistory.Controls)
                y = Math.Max(y, c.Bottom + 8);

            lbl.Location = new Point(isUser ? 20 : 5, y);
            pnlChatHistory.Controls.Add(lbl);
            pnlChatHistory.ScrollControlIntoView(lbl);
        }
        #endregion

        private class PatientItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public override string ToString() => Name;
        }
    }
}
