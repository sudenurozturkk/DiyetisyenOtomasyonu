using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    /// <summary>
    /// AI Analiz Paneli - Profesyonel Yeni Tasarım
    /// Hasta analizi, kilo grafiği, kalori dağılımı, AI önerileri, örnek öğünler, besin dengesi, AI sohbet
    /// </summary>
    public class FrmAIAnalysis : XtraForm
    {
        private readonly PatientService _patientService;
        private readonly GeminiAIService _aiService;
        private readonly WeightEntryRepository _weightRepo;
        private readonly AiChatRepository _chatRepo;

        #region Colors
        private readonly Color PrimaryGreen = ColorTranslator.FromHtml("#0D9488");
        private readonly Color DarkGreen = ColorTranslator.FromHtml("#0F766E");
        private readonly Color LightGreen = ColorTranslator.FromHtml("#CCFBF1");
        private readonly Color SuccessGreen = ColorTranslator.FromHtml("#22C55E");
        private readonly Color DangerRed = ColorTranslator.FromHtml("#EF4444");
        private readonly Color WarningOrange = ColorTranslator.FromHtml("#F97316");
        private readonly Color InfoBlue = ColorTranslator.FromHtml("#3B82F6");
        private readonly Color BackgroundLight = ColorTranslator.FromHtml("#F8FAFC");
        private readonly Color CardWhite = Color.White;
        private readonly Color BorderGray = ColorTranslator.FromHtml("#E2E8F0");
        private readonly Color TextDark = ColorTranslator.FromHtml("#1E293B");
        private readonly Color TextMedium = ColorTranslator.FromHtml("#64748B");
        #endregion

        #region Controls
        private ComboBoxEdit cmbPatient;
        private Label lblHealthStatus, lblHeightWeight, lblDailyCalorie, lblBMI;
        private Label lblAnalysisText;
        private Panel pnlWeightChart, pnlCaloriePie;
        private Label lblAiRecommendation1, lblAiRecommendation2, lblAiRecommendation3;
        private MemoEdit txtChatInput;
        private Panel pnlChatHistory;
        private SimpleButton btnSendChat;
        #endregion

        private Domain.Patient _selectedPatient;
        private int _selectedPatientId;
        private double[] _weightData = new double[0];

        public FrmAIAnalysis()
        {
            InitializeComponent();
            _patientService = new PatientService();
            _weightRepo = new WeightEntryRepository();
            _chatRepo = new AiChatRepository();
            
            string apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? "";
            if (!string.IsNullOrEmpty(apiKey))
            {
                _aiService = new GeminiAIService(apiKey);
            }
            
            SetupUI();
            LoadPatients();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1280, 850);
            this.Name = "FrmAIAnalysis";
            this.Text = "AI Analiz Paneli";
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = BackgroundLight;
            this.Padding = new Padding(20);
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            // Ana container - scroll destekli
            var mainContainer = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(15) };
            
            // İçerik paneli - geniş layout
            var contentPanel = new Panel { Location = new Point(15, 15), BackColor = Color.Transparent };

            int y = 0;
            int fullWidth = 900; // Sidebar sonrası kullanılabilir alan

            // Header
            var headerPanel = CreateHeader();
            headerPanel.Size = new Size(fullWidth, 45);
            headerPanel.Location = new Point(0, y);
            contentPanel.Controls.Add(headerPanel);
            y += 50;

            // Summary Cards - 4 kart yan yana
            var summaryRow = CreateSummaryCards();
            summaryRow.Size = new Size(fullWidth, 60);
            summaryRow.Location = new Point(0, y);
            contentPanel.Controls.Add(summaryRow);
            y += 70;

            // Main Content Row - sol ve sağ paneller
            var mainRow = CreateMainContentRow();
            mainRow.Size = new Size(fullWidth, 280);
            mainRow.Location = new Point(0, y);
            contentPanel.Controls.Add(mainRow);
            y += 290;

            // Bottom Row - 3 kart
            var bottomRow = CreateBottomRow();
            bottomRow.Size = new Size(fullWidth, 260);
            bottomRow.Location = new Point(0, y);
            contentPanel.Controls.Add(bottomRow);
            y += 270;

            contentPanel.Size = new Size(fullWidth + 20, y);
            mainContainer.Controls.Add(contentPanel);
            this.Controls.Add(mainContainer);
        }

        #region Header
        private Panel CreateHeader()
        {
            var panel = new Panel { Size = new Size(1220, 50), BackColor = Color.Transparent };

            var lblTitle = new Label
            {
                Text = "🤖 AI Analiz Paneli",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(0, 8),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            // Search box
            var searchBox = new TextEdit
            {
                Location = new Point(600, 10),
                Size = new Size(250, 28),
                Properties = { NullValuePrompt = "🔍 Hasta veya diyet ara..." }
            };
            panel.Controls.Add(searchBox);

            // Patient selector
            cmbPatient = new ComboBoxEdit { Location = new Point(880, 10), Size = new Size(200, 28) };
            cmbPatient.SelectedIndexChanged += CmbPatient_SelectedIndexChanged;
            panel.Controls.Add(cmbPatient);

            // Date
            var lblDate = new Label
            {
                Text = $"📅 {DateTime.Now:dddd, dd MMMM yyyy}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMedium,
                Location = new Point(1100, 15),
                AutoSize = true
            };
            panel.Controls.Add(lblDate);

            return panel;
        }
        #endregion

        #region Summary Cards
        private Panel CreateSummaryCards()
        {
            var row = new Panel { Size = new Size(1220, 65), BackColor = Color.Transparent };

            // Card 1 - Genel Durum
            var card1 = CreateSummaryCard("✓", "Genel Durum", "Sağlıklı", SuccessGreen, 0);
            row.Controls.Add(card1);

            // Card 2 - Boy-Kilo
            var card2 = CreateSummaryCard("📏", "Boy-Kilo", "178 cm / 84 kg", TextDark, 200);
            lblHeightWeight = (Label)card2.Controls[2];
            row.Controls.Add(card2);

            // Card 3 - Günlük Kalori
            var card3 = CreateSummaryCard("🔥", "Günlük Kalori", "2000 kcal", SuccessGreen, 400);
            lblDailyCalorie = (Label)card3.Controls[2];
            row.Controls.Add(card3);

            // Card 4 - BMI
            var card4 = CreateSummaryCard("⚖️", "BMI", "26.5 Fazla Kilolu", WarningOrange, 600);
            lblBMI = (Label)card4.Controls[2];
            row.Controls.Add(card4);

            return row;
        }

        private Panel CreateSummaryCard(string icon, string title, string value, Color valueColor, int x)
        {
            var card = new Panel { Location = new Point(x, 0), Size = new Size(185, 58), BackColor = CardWhite };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 10);

            var lblIcon = new Label { Text = icon, Font = new Font("Segoe UI", 14F), Location = new Point(10, 15), AutoSize = true };
            var lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 8F), ForeColor = TextMedium, Location = new Point(45, 8), AutoSize = true };
            var lblValue = new Label { Text = value, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = valueColor, Location = new Point(45, 28), AutoSize = true };
            
            card.Controls.Add(lblIcon);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            
            return card;
        }
        #endregion

        #region Main Content Row
        private Panel CreateMainContentRow()
        {
            var row = new Panel { Size = new Size(900, 280), BackColor = Color.Transparent };

            // Left side - Analysis + Charts (580px)
            var leftPanel = CreateAnalysisPanel();
            leftPanel.Location = new Point(0, 0);
            row.Controls.Add(leftPanel);

            // Right side - AI Recommendations (300px)
            var rightPanel = CreateRecommendationsPanel();
            rightPanel.Location = new Point(600, 0);
            row.Controls.Add(rightPanel);

            return row;
        }

        private Panel CreateAnalysisPanel()
        {
            var panel = new Panel { Size = new Size(580, 270), BackColor = CardWhite };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 10);

            // Header with patient name
            var lblHeader = new Label
            {
                Text = "👤 Ahmet Yılmaz'ın Analiz Sonuçları",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 12),
                AutoSize = true
            };
            panel.Controls.Add(lblHeader);

            // Period buttons
            var periods = new[] { "Haftalık", "Aylık", "Yıllık" };
            for (int i = 0; i < periods.Length; i++)
            {
                var btn = new SimpleButton
                {
                    Text = periods[i],
                    Location = new Point(450 + i * 75, 10),
                    Size = new Size(70, 26),
                    Font = new Font("Segoe UI", 8F),
                    Appearance = { BackColor = i == 0 ? PrimaryGreen : CardWhite, ForeColor = i == 0 ? CardWhite : TextDark, BorderColor = BorderGray }
                };
                panel.Controls.Add(btn);
            }

            // AI Analysis text
            lblAnalysisText = new Label
            {
                Text = "🤖 Ahmet Bey sağlıklı durumda ancak BMI değeri 26.5 ile fazla kilolu kategorisindedir. Haftalık ortalama kilo kaybı: sağlıklı sınırlar içinde. Mevcut kilo gidişatı iyi görükuyor. Kilo koruma için beslenme düzeni önerilecektir.",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMedium,
                Location = new Point(15, 45),
                Size = new Size(740, 40)
            };
            panel.Controls.Add(lblAnalysisText);

            // Weight Chart
            var lblWeightTitle = new Label { Text = "Son Kilo Değişimi", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = TextDark, Location = new Point(15, 95), AutoSize = true };
            panel.Controls.Add(lblWeightTitle);

            pnlWeightChart = new Panel { Location = new Point(15, 120), Size = new Size(360, 230), BackColor = Color.FromArgb(250, 252, 254) };
            pnlWeightChart.Paint += PnlWeightChart_Paint;
            panel.Controls.Add(pnlWeightChart);

            // Calorie Pie Chart
            var lblCalorieTitle = new Label { Text = "🥧 Kalori Dağılımı", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = TextDark, Location = new Point(400, 95), AutoSize = true };
            panel.Controls.Add(lblCalorieTitle);

            pnlCaloriePie = new Panel { Location = new Point(400, 120), Size = new Size(360, 230), BackColor = Color.FromArgb(250, 252, 254) };
            pnlCaloriePie.Paint += PnlCaloriePie_Paint;
            panel.Controls.Add(pnlCaloriePie);

            return panel;
        }

        private void PnlWeightChart_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            
            if (_weightData == null || _weightData.Length == 0)
            {
                _weightData = new[] { 88.0, 87.5, 86.8, 86.0, 85.5, 85.0, 84.5, 84.0 }; // Default data
            }

            double max = _weightData.Max() + 1;
            double min = _weightData.Min() - 1;
            double range = max - min;
            if (range == 0) range = 1;

            int w = pnlWeightChart.Width - 50;
            int h = pnlWeightChart.Height - 50;

            // Draw grid lines and labels
            using (var gridPen = new Pen(BorderGray, 1))
            using (var font = new Font("Segoe UI", 7F))
            using (var brush = new SolidBrush(TextMedium))
            {
                for (int i = 0; i <= 4; i++)
                {
                    int y = 20 + (int)(i * h / 4.0);
                    e.Graphics.DrawLine(gridPen, 40, y, w + 40, y);
                    double val = max - (i * range / 4);
                    e.Graphics.DrawString($"{val:F0} kg", font, brush, 0, y - 6);
                }
            }

            // Draw line chart
            if (_weightData.Length > 1)
            {
                var points = new List<PointF>();
                for (int i = 0; i < _weightData.Length; i++)
                {
                    float x = 40 + (float)(i * w / (_weightData.Length - 1));
                    float y = 20 + (float)((max - _weightData[i]) / range * h);
                    points.Add(new PointF(x, y));
                }

                using (var linePen = new Pen(PrimaryGreen, 2))
                    e.Graphics.DrawLines(linePen, points.ToArray());

                // Draw points
                foreach (var pt in points)
                {
                    using (var brush = new SolidBrush(PrimaryGreen))
                        e.Graphics.FillEllipse(brush, pt.X - 4, pt.Y - 4, 8, 8);
                }
            }

            // X-axis labels
            string[] weeks = { "4 Hafta Önce", "3 Hafta Önce", "2 Hafta Önce", "1 Hafta Önce", "Bu Hafta" };
            using (var font = new Font("Segoe UI", 7F))
            using (var brush = new SolidBrush(TextMedium))
            {
                for (int i = 0; i < Math.Min(weeks.Length, _weightData.Length); i++)
                {
                    float x = 40 + (float)(i * w / Math.Max(1, _weightData.Length - 1));
                    e.Graphics.DrawString(weeks[i], font, brush, x - 20, h + 25);
                }
            }
        }

        private void PnlCaloriePie_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Protein 21%, Karbohidrat 50%, Yağ 35%
            int[] percentages = { 21, 50, 35 };
            Color[] colors = { SuccessGreen, InfoBlue, WarningOrange };
            string[] labels = { "Protein", "Karbonhidrat", "Yağ" };
            string[] values = { "105g", "250g", "64g" };

            int centerX = 100, centerY = 100, radius = 80;
            float startAngle = 0;

            // Draw pie slices
            for (int i = 0; i < percentages.Length; i++)
            {
                float sweepAngle = percentages[i] * 360f / 100f;
                using (var brush = new SolidBrush(colors[i]))
                    e.Graphics.FillPie(brush, centerX - radius, centerY - radius, radius * 2, radius * 2, startAngle, sweepAngle);
                startAngle += sweepAngle;
            }

            // Draw center circle (donut effect)
            using (var brush = new SolidBrush(Color.FromArgb(250, 252, 254)))
                e.Graphics.FillEllipse(brush, centerX - 40, centerY - 40, 80, 80);

            // Draw legend
            using (var font = new Font("Segoe UI", 9F))
            using (var boldFont = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                for (int i = 0; i < labels.Length; i++)
                {
                    int legendY = 40 + i * 50;
                    using (var brush = new SolidBrush(colors[i]))
                        e.Graphics.FillEllipse(brush, 220, legendY, 12, 12);
                    using (var brush = new SolidBrush(TextDark))
                        e.Graphics.DrawString(labels[i], font, brush, 240, legendY - 2);
                    using (var brush = new SolidBrush(colors[i]))
                        e.Graphics.DrawString($"{percentages[i]}%", boldFont, brush, 240, legendY + 15);
                    using (var brush = new SolidBrush(TextMedium))
                        e.Graphics.DrawString(values[i], font, brush, 280, legendY + 15);
                }
            }
        }

        private Panel CreateRecommendationsPanel()
        {
            var panel = new Panel { Size = new Size(290, 270), BackColor = CardWhite };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 10);

            var lblTitle = new Label
            {
                Text = "✨ Sağlık ve Diyet Önerileri",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 12),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            // Recommendations
            int y = 50;
            
            lblAiRecommendation1 = CreateRecommendationItem(panel, "✓", "Mevcut kilonuzu korumak için haftalık en fazla 0.6 - 1 kg vermeye devam edin 🎯", SuccessGreen, y);
            y += 80;
            
            lblAiRecommendation2 = CreateRecommendationItem(panel, "⚠️", "Haftalık en az 150 dakika (yaklaşık 30 dk/gün) orta düzeyde egzersiz yapmanız önerilir 🏃", WarningOrange, y);
            y += 80;
            
            lblAiRecommendation3 = CreateRecommendationItem(panel, "💧", "Günlük su tüketiminizi en az 8 bardek (yaklaşık 2 litre) olarak tutmaya özen gösterin 💙", InfoBlue, y);

            return panel;
        }

        private Label CreateRecommendationItem(Panel panel, string icon, string text, Color iconColor, int y)
        {
            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 14F),
                ForeColor = iconColor,
                Location = new Point(15, y),
                AutoSize = true
            };
            panel.Controls.Add(lblIcon);

            var lblText = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextDark,
                Location = new Point(50, y),
                Size = new Size(350, 60)
            };
            panel.Controls.Add(lblText);
            
            return lblText;
        }
        #endregion

        #region Bottom Row
        private Panel CreateBottomRow()
        {
            var row = new Panel { Size = new Size(900, 260), BackColor = Color.Transparent };

            // Haftalık Örnek Öğünler
            var mealsCard = CreateMealsCard();
            mealsCard.Location = new Point(0, 0);
            row.Controls.Add(mealsCard);

            // Makro ve Mikro Besin Dengesi
            var nutritionCard = CreateNutritionCard();
            nutritionCard.Location = new Point(305, 0);
            row.Controls.Add(nutritionCard);

            // AI Sohbet
            var chatCard = CreateChatCard();
            chatCard.Location = new Point(610, 0);
            row.Controls.Add(chatCard);

            return row;
        }

        private Panel CreateMealsCard()
        {
            var panel = new Panel { Size = new Size(290, 250), BackColor = CardWhite };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 10);

            var lblTitle = new Label
            {
                Text = "🍽️ Haftalık Örnek Öğünler",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 12),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            // Meals
            int y = 50;
            CreateMealItem(panel, "🌅 Kahvaltı", "Yulaf Ezmesi (50g), Süt (200ml), Ceviz (10g), Elma (1 adet) - 450 kcal", y);
            y += 80;
            CreateMealItem(panel, "☀️ Öğle Yemeği", "Tavuk Izgara (150g), Bulgur Pilavı (100g), Salata (1 porsiyon), Yoğurt (1 kase) - 600 kcal", y);
            y += 80;
            CreateMealItem(panel, "🌙 Akşam Yemeği", "Fırın Sebzeli Balık (200g), Tam Tahıllı Ekmek (1 dilim), Zeytinyağlı Brokoli (150g) - 550 kcal", y);

            return panel;
        }

        private void CreateMealItem(Panel panel, string title, string description, int y)
        {
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, y),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            var lblDesc = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = TextMedium,
                Location = new Point(15, y + 22),
                Size = new Size(355, 45)
            };
            panel.Controls.Add(lblDesc);
        }

        private Panel CreateNutritionCard()
        {
            var panel = new Panel { Size = new Size(290, 250), BackColor = CardWhite };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 10);

            var lblTitle = new Label
            {
                Text = "📊 Makro ve Mikro Besin Dengesi",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 12),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            // Nutrition bars
            var nutrients = new[] {
                ("Protein", SuccessGreen, 105, 120),
                ("Karbonhidrat", InfoBlue, 250, 300),
                ("Yağ", WarningOrange, 64, 80),
                ("Fiber", SuccessGreen, 28, 35),
                ("Sodyum", DangerRed, 2050, 2300),
                ("Kalsiyum", InfoBlue, 850, 1000),
                ("Vitamin C", SuccessGreen, 99, 100),
                ("Demir", DangerRed, 16, 18)
            };

            int y = 45;
            foreach (var (name, color, current, target) in nutrients)
            {
                CreateNutritionBar(panel, name, color, current, target, y);
                y += 32;
            }

            return panel;
        }

        private void CreateNutritionBar(Panel panel, string name, Color color, int current, int target, int y)
        {
            var lblName = new Label
            {
                Text = name,
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextDark,
                Location = new Point(15, y),
                Size = new Size(85, 16)
            };
            panel.Controls.Add(lblName);

            var pnlBar = new Panel { Location = new Point(105, y), Size = new Size(180, 14), BackColor = Color.Transparent };
            pnlBar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var bgBrush = new SolidBrush(BorderGray))
                    e.Graphics.FillRectangle(bgBrush, 0, 2, 180, 10);
                int fillWidth = Math.Min(180, (int)(180.0 * current / target));
                using (var fillBrush = new SolidBrush(color))
                    e.Graphics.FillRectangle(fillBrush, 0, 2, fillWidth, 10);
            };
            panel.Controls.Add(pnlBar);

            var lblValue = new Label
            {
                Text = current >= 1000 ? $"{current} mg" : $"{current}g",
                Font = new Font("Segoe UI", 7F),
                ForeColor = TextMedium,
                Location = new Point(290, y),
                AutoSize = true
            };
            panel.Controls.Add(lblValue);

            var lblIdeal = new Label
            {
                Text = "İdeal",
                Font = new Font("Segoe UI", 7F),
                ForeColor = current >= target * 0.9 ? SuccessGreen : WarningOrange,
                Location = new Point(340, y),
                AutoSize = true
            };
            panel.Controls.Add(lblIdeal);
        }

        private Panel CreateChatCard()
        {
            var panel = new Panel { Size = new Size(280, 250), BackColor = CardWhite };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 10);

            var lblTitle = new Label
            {
                Text = "🧠 AI Sordu",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 12),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            var lblSubtitle = new Label
            {
                Text = "AI Asistanı",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMedium,
                Location = new Point(15, 35),
                AutoSize = true
            };
            panel.Controls.Add(lblSubtitle);

            // Chat history panel
            pnlChatHistory = new Panel
            {
                Location = new Point(15, 60),
                Size = new Size(390, 170),
                AutoScroll = true,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            panel.Controls.Add(pnlChatHistory);

            // Default chat messages
            AddChatMessage("Hastanız için sorun veya öneri isteyin...", true);
            AddChatMessage("Hastanız için kanturomaun veya öneri için değeri 20.7'diyVermeye yanımla:süt ve yoğu..1 akza salızzğlarım sam alarsz yemeği gösterin...", false);
            AddChatMessage("AI: AI statım. Bu üine göre, kohiczma için oüan tamsillik ikerepe izu öneri. Halfvu 1 bil açın", false);

            // Input area
            txtChatInput = new MemoEdit
            {
                Location = new Point(15, 235),
                Size = new Size(310, 55),
                Properties = { NullValuePrompt = "Hastanız için soru sorun veya öneri isteyin..." }
            };
            panel.Controls.Add(txtChatInput);

            // Send button
            btnSendChat = new SimpleButton
            {
                Text = "➤",
                Location = new Point(330, 235),
                Size = new Size(35, 55),
                Font = new Font("Segoe UI", 14F),
                Appearance = { BackColor = PrimaryGreen, ForeColor = CardWhite, BorderColor = PrimaryGreen }
            };
            btnSendChat.Click += BtnSendChat_Click;
            panel.Controls.Add(btnSendChat);

            // Bottom buttons
            var btnSuggest = new SimpleButton
            {
                Text = "Öneri İste",
                Location = new Point(260, 275),
                Size = new Size(80, 28),
                Font = new Font("Segoe UI", 8F),
                Appearance = { BackColor = PrimaryGreen, ForeColor = CardWhite }
            };
            btnSuggest.Click += async (s, e) => await RequestAISuggestionAsync();
            panel.Controls.Add(btnSuggest);

            var btnClear = new SimpleButton
            {
                Text = "Konuşmayı Sil",
                Location = new Point(345, 275),
                Size = new Size(70, 28),
                Font = new Font("Segoe UI", 8F),
                Appearance = { BackColor = DangerRed, ForeColor = CardWhite }
            };
            btnClear.Click += (s, e) => ClearChat();
            panel.Controls.Add(btnClear);

            return panel;
        }

        private void AddChatMessage(string message, bool isUser)
        {
            var lblMsg = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 8F),
                ForeColor = isUser ? CardWhite : TextDark,
                BackColor = isUser ? PrimaryGreen : Color.FromArgb(240, 240, 240),
                Padding = new Padding(8),
                AutoSize = false,
                MaximumSize = new Size(280, 0),
                Size = new Size(270, 45)
            };
            
            int y = 5;
            foreach (Control c in pnlChatHistory.Controls)
            {
                y = Math.Max(y, c.Bottom + 5);
            }
            
            lblMsg.Location = new Point(isUser ? 110 : 5, y);
            pnlChatHistory.Controls.Add(lblMsg);
            pnlChatHistory.ScrollControlIntoView(lblMsg);
        }

        private void ClearChat()
        {
            pnlChatHistory.Controls.Clear();
            AddChatMessage("Sohbet temizlendi. Yeni bir soru sorabilirsiniz.", false);
        }
        #endregion

        #region Event Handlers & Data Loading
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
                LoadPatientData();
                LoadAIAnalysisAsync();
            }
        }

        private void LoadPatientData()
        {
            if (_selectedPatient == null) return;

            // Update summary cards
            lblHeightWeight.Text = $"{_selectedPatient.Boy} cm / {_selectedPatient.GuncelKilo} kg";
            
            // Calculate daily calories (BMR * activity factor)
            double bmr = 10 * _selectedPatient.GuncelKilo + 6.25 * _selectedPatient.Boy - 5 * _selectedPatient.Yas + (_selectedPatient.Cinsiyet == "Erkek" ? 5 : -161);
            lblDailyCalorie.Text = $"{(int)(bmr * 1.5)} kcal";

            // BMI
            string bmiCategory = _selectedPatient.BMI < 18.5 ? "Zayıf" : _selectedPatient.BMI < 25 ? "Normal" : _selectedPatient.BMI < 30 ? "Fazla Kilolu" : "Obez";
            lblBMI.Text = $"{_selectedPatient.BMI:F1} {bmiCategory}";
            lblBMI.ForeColor = _selectedPatient.BMI >= 25 ? WarningOrange : SuccessGreen;

            // Load weight chart data
            var weightEntries = _weightRepo.GetByPatientId(_selectedPatientId, 30).OrderBy(w => w.Date).ToList();
            if (weightEntries.Count > 0)
            {
                _weightData = weightEntries.Select(w => w.Weight).ToArray();
            }
            else
            {
                _weightData = new[] { _selectedPatient.BaslangicKilosu, _selectedPatient.GuncelKilo };
            }
            pnlWeightChart?.Invalidate();
        }

        private async void LoadAIAnalysisAsync()
        {
            if (_selectedPatient == null || _aiService == null) return;

            try
            {
                lblAnalysisText.Text = "⏳ AI analizi yükleniyor...";

                string context = $@"Hasta: {_selectedPatient.AdSoyad}
Yaş: {_selectedPatient.Yas}, Cinsiyet: {_selectedPatient.Cinsiyet}
Boy: {_selectedPatient.Boy} cm, Mevcut Kilo: {_selectedPatient.GuncelKilo} kg, Başlangıç Kilosu: {_selectedPatient.BaslangicKilosu} kg
BMI: {_selectedPatient.BMI:F1}
Kronik Hastalıklar: {_selectedPatient.MedicalHistory ?? "Yok"}";

                string prompt = $@"Aşağıdaki hasta verilerini analiz et ve kısa bir değerlendirme yaz. Türkçe yanıt ver.

HASTA VERİLERİ:
{context}

Yanıtını 2-3 cümle ile sınırla. Hastanın genel durumunu, BMI kategorisini ve kilo değişimini değerlendir.";

                string response = await _aiService.GetAIResponseAsync(prompt);
                lblAnalysisText.Text = "🤖 " + (response.Length > 300 ? response.Substring(0, 300) + "..." : response);

                // Also get recommendations
                await LoadAIRecommendationsAsync();
            }
            catch (Exception ex)
            {
                lblAnalysisText.Text = "🤖 AI analizi yüklenemedi: " + ex.Message;
            }
        }

        private async System.Threading.Tasks.Task LoadAIRecommendationsAsync()
        {
            if (_selectedPatient == null || _aiService == null) return;

            try
            {
                string prompt = $@"Hasta için 3 adet kişisel sağlık önerisi ver. Türkçe yanıt ver.

Hasta: {_selectedPatient.AdSoyad}, Yaş: {_selectedPatient.Yas}, BMI: {_selectedPatient.BMI:F1}
Mevcut Kilo: {_selectedPatient.GuncelKilo} kg

YANIT FORMATI (sadece 3 numarat maddeli):
1. [Kilo ile ilgili öneri]
2. [Egzersiz önerisi]
3. [Su/beslenme önerisi]";

                string response = await _aiService.GetAIResponseAsync(prompt);
                
                // Parse recommendations
                var lines = response.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length && i < 3; i++)
                {
                    string line = lines[i].TrimStart('1', '2', '3', '.', ' ', '-');
                    switch (i)
                    {
                        case 0: lblAiRecommendation1.Text = "✓ " + line; break;
                        case 1: lblAiRecommendation2.Text = "⚠️ " + line; break;
                        case 2: lblAiRecommendation3.Text = "💧 " + line; break;
                    }
                }
            }
            catch { }
        }

        private async void BtnSendChat_Click(object sender, EventArgs e)
        {
            string message = txtChatInput.Text?.Trim();
            if (string.IsNullOrEmpty(message)) return;

            AddChatMessage(message, true);
            txtChatInput.Text = "";

            if (_aiService != null && _selectedPatient != null)
            {
                try
                {
                    string prompt = $@"Sen bir diyetisyen asistanısın. Hasta hakkında sorulan soruyu yanıtla. Türkçe yanıt ver.

Hasta: {_selectedPatient.AdSoyad}, Yaş: {_selectedPatient.Yas}, Boy: {_selectedPatient.Boy} cm, Kilo: {_selectedPatient.GuncelKilo} kg, BMI: {_selectedPatient.BMI:F1}

Soru: {message}

Kısa ve öz yanıt ver (2-3 cümle).";

                    string response = await _aiService.GetAIResponseAsync(prompt);
                    AddChatMessage("AI: " + (response.Length > 200 ? response.Substring(0, 200) + "..." : response), false);
                    
                    // Save to database
                    _chatRepo.Add(new AiChatMessage { PatientId = _selectedPatientId, DoctorId = AuthContext.UserId, Message = message, IsAiResponse = false, Timestamp = DateTime.Now });
                    _chatRepo.Add(new AiChatMessage { PatientId = _selectedPatientId, DoctorId = AuthContext.UserId, Message = response, IsAiResponse = true, Timestamp = DateTime.Now });
                }
                catch (Exception ex)
                {
                    AddChatMessage("AI: Hata oluştu - " + ex.Message, false);
                }
            }
            else
            {
                AddChatMessage("AI: API bağlantısı yok. OPENROUTER_API_KEY ortam değişkenini kontrol edin.", false);
            }
        }

        private async System.Threading.Tasks.Task RequestAISuggestionAsync()
        {
            if (_aiService == null || _selectedPatient == null)
            {
                AddChatMessage("AI: API bağlantısı yok veya hasta seçilmedi.", false);
                return;
            }

            AddChatMessage("Hasta için genel öneri iste", true);

            try
            {
                string prompt = $@"Hasta için kişiselleştirilmiş bir diyet ve sağlık önerisi ver. Türkçe yanıt ver.

Hasta: {_selectedPatient.AdSoyad}
Yaş: {_selectedPatient.Yas}, Cinsiyet: {_selectedPatient.Cinsiyet}
Boy: {_selectedPatient.Boy} cm, Kilo: {_selectedPatient.GuncelKilo} kg
BMI: {_selectedPatient.BMI:F1}

3-4 cümlelik kısa ve pratik bir öneri ver.";

                string response = await _aiService.GetAIResponseAsync(prompt);
                AddChatMessage("AI: " + response, false);
            }
            catch (Exception ex)
            {
                AddChatMessage("AI: Hata - " + ex.Message, false);
            }
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
        #endregion

        private class PatientItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public override string ToString() => Name;
        }
    }
}
