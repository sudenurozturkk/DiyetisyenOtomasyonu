using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Forms.Patient
{
    public partial class FrmGoals : XtraForm
    {
        private readonly GoalService _goalService;
        private BindingList<Goal> _goals;

        private Color PrimaryColor => UiStyles.PrimaryColor;
        private Color SuccessGreen => UiStyles.SuccessColor;
        private Color InfoBlue => UiStyles.InfoColor;
        private Color WarningOrange => UiStyles.WarningColor;
        private Color CardColor => Color.White;
        private Color BackgroundColor => Color.FromArgb(248, 250, 252);
        private Color TextPrimary => UiStyles.TextPrimary;
        private Color TextSecondary => UiStyles.TextSecondary;

        private PanelControl scrollContainer;

        public FrmGoals()
        {
            InitializeComponent();
            _goalService = new GoalService();
            SetupUI();
            LoadGoals();
        }

        private void SetupUI()
        {
            this.Text = "Hedeflerim";
            this.BackColor = BackgroundColor;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(25, 20, 25, 20);

            var mainPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = BackgroundColor
            };

            // Header - Kompakt
            var header = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 40,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.Transparent
            };

            var lblTitle = new LabelControl
            {
                Text = "🎯 Aktif Hedeflerim",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, 8),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(220, 24)
            };
            header.Controls.Add(lblTitle);

            var btnRefresh = new SimpleButton
            {
                Text = "Yenile",
                Location = new Point(230, 6),
                Size = new Size(70, 28),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Appearance = { BackColor = SuccessGreen, ForeColor = Color.White }
            };
            btnRefresh.Click += (s, e) => LoadGoals();
            header.Controls.Add(btnRefresh);
            mainPanel.Controls.Add(header);

            // Content Area - Scrollable
            scrollContainer = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.Transparent,
                AutoScroll = true,
                Padding = new Padding(0, 0, 0, 0),
                Margin = new Padding(0)
            };
            mainPanel.Controls.Add(scrollContainer);

            this.Controls.Add(mainPanel);
        }

        private void LoadGoals()
        {
            scrollContainer.Controls.Clear();
            var goals = _goalService.GetActiveGoals(AuthContext.UserId);
            _goals = new BindingList<Goal>(goals);

            if (_goals.Count == 0)
            {
                var emptyPanel = new PanelControl
                {
                    Location = new Point(0, 20),
                    Size = new Size(scrollContainer.Width - 20, 180),
                    BorderStyle = BorderStyles.Simple,
                    BackColor = CardColor
                };

                var lblIcon = new LabelControl
                {
                    Text = "🎯",
                    Font = new Font("Segoe UI", 40F),
                    Location = new Point((emptyPanel.Width - 50) / 2, 20),
                    AutoSizeMode = LabelAutoSizeMode.None,
                    Size = new Size(50, 50)
                };
                emptyPanel.Controls.Add(lblIcon);

                var lblEmpty = new LabelControl
                {
                    Text = "Henüz aktif hedef bulunmuyor.\nDoktorunuzdan hedef ataması isteyebilirsiniz.",
                    Font = new Font("Segoe UI", 11F),
                    ForeColor = TextSecondary,
                    AutoSizeMode = LabelAutoSizeMode.None,
                    Size = new Size(emptyPanel.Width - 40, 50),
                    Location = new Point(20, 90),
                    Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center } }
                };
                emptyPanel.Controls.Add(lblEmpty);
                scrollContainer.Controls.Add(emptyPanel);
                return;
            }

            // Scroll container'ın görünür alanını hesapla
            int availableWidth = scrollContainer.ClientSize.Width > 0 ? scrollContainer.ClientSize.Width - 20 : scrollContainer.Width - 20;
            // KRİTİK: İçeriği header yüksekliği kadar (40px) aşağıdan başlat
            // Header scroll container'ın üstünde, o yüzden içeriği direkt 40px aşağıdan başlat
            int yPos = 40;
            int cardWidth = (availableWidth - 15) / 2;
            int cardHeight = 110;
            int spacing = 15;
            int x = 0;
            int col = 0;

            foreach (var goal in _goals.Take(4))
            {
                var card = CreateGoalCard(goal, cardWidth, cardHeight);
                card.Location = new Point(x, yPos);
                scrollContainer.Controls.Add(card);

                col++;
                if (col >= 2)
                {
                    col = 0;
                    x = 0;
                    yPos += cardHeight + spacing;
                }
                else
                {
                    x += cardWidth + spacing;
                }
            }

            if (_goals.Count > 0)
            {
                yPos += 20; // Boşluk
            }

            // 2. Hızlı Takip Bölümü
            var quickTrackPanel = CreateQuickTrackPanel(availableWidth);
            quickTrackPanel.Location = new Point(0, yPos);
            scrollContainer.Controls.Add(quickTrackPanel);
            yPos += quickTrackPanel.Height + 20;

            // 3. Hedef Gerçekleşme Trendi
            var trendPanel = CreateTrendPanel(availableWidth);
            trendPanel.Location = new Point(0, yPos);
            scrollContainer.Controls.Add(trendPanel);
            yPos += trendPanel.Height + 20;

            // 4. Haftalık Başarı Kartı
            var weeklyCard = CreateWeeklyAchievementCard(availableWidth);
            weeklyCard.Location = new Point(0, yPos);
            scrollContainer.Controls.Add(weeklyCard);
            
            // Scroll container'ın minimum boyutunu ayarla
            int totalHeight = yPos + weeklyCard.Height + 20;
            scrollContainer.AutoScrollMinSize = new Size(availableWidth + 20, totalHeight);
            
            // Layout'u güncelle
            scrollContainer.PerformLayout();
            Application.DoEvents();
            
            // Scroll pozisyonunu en üste al - KRİTİK!
            scrollContainer.HorizontalScroll.Value = 0;
            scrollContainer.VerticalScroll.Value = 0;
            scrollContainer.AutoScrollPosition = new Point(0, 0);
            
            // Tüm kontrolleri yeniden çiz
            scrollContainer.Invalidate();
            scrollContainer.Update();
            scrollContainer.Refresh();
            
            // Form yüklendikten sonra scroll pozisyonunu ayarla
            this.Shown += (s, e) => {
                scrollContainer.HorizontalScroll.Value = 0;
                scrollContainer.VerticalScroll.Value = 0;
                scrollContainer.AutoScrollPosition = new Point(0, 0);
                scrollContainer.Refresh();
            };
            
            // LoadGoals tamamlandıktan sonra scroll pozisyonunu ayarla - BİRKAÇ KEZ DENEYELİM
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new System.Action(() => {
                    scrollContainer.HorizontalScroll.Value = 0;
                    scrollContainer.VerticalScroll.Value = 0;
                    scrollContainer.AutoScrollPosition = new Point(0, 0);
                    scrollContainer.Refresh();
                    
                    // Bir kez daha dene
                    System.Threading.Thread.Sleep(100);
                    scrollContainer.HorizontalScroll.Value = 0;
                    scrollContainer.VerticalScroll.Value = 0;
                    scrollContainer.AutoScrollPosition = new Point(0, 0);
                    scrollContainer.Refresh();
                }));
            }
        }

        private PanelControl CreateGoalCard(Goal goal, int width, int height)
        {
            Color goalColor = GetGoalColor(goal);

            var card = new PanelControl
            {
                Size = new Size(width, height),
                BorderStyle = BorderStyles.Simple,
                BackColor = CardColor,
                Padding = new Padding(0)
            };

            var content = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.Transparent,
                Padding = new Padding(12, 10, 12, 10)
            };

            // Renkli kare ikon (sol üst) - Küçültüldü
            var iconSquare = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(40, 40),
                BorderStyle = BorderStyles.NoBorder,
                BackColor = goalColor
            };
            content.Controls.Add(iconSquare);

            string icon = GetGoalIcon(goal.GoalType);
            var lblIcon = new LabelControl
            {
                Text = icon,
                Font = new Font("Segoe UI", 20F),
                ForeColor = Color.White,
                Location = new Point(0, 0),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(40, 40),
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center, VAlignment = DevExpress.Utils.VertAlignment.Center } }
            };
            iconSquare.Controls.Add(lblIcon);

            // Başlık (ikonun yanı) - Küçültüldü
            var lblName = new LabelControl
            {
                Text = GetGoalTitle(goal.GoalType),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(48, 2),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width - 100, 18)
            };
            content.Controls.Add(lblName);

            // Değerler - Küçültüldü
            var lblValues = new LabelControl
            {
                Text = $"{goal.CurrentValue:F1} / {goal.TargetValue:F1} {goal.Unit}",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(48, 22),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width - 100, 16)
            };
            content.Controls.Add(lblValues);

            // Progress Bar - Küçültüldü
            var progressBar = new ProgressBarControl
            {
                Location = new Point(48, 42),
                Size = new Size(width - 70, 14),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            progressBar.Properties.Maximum = 100;
            progressBar.Properties.ShowTitle = false;
            progressBar.Properties.PercentView = false;
            progressBar.Properties.EndColor = goalColor;
            progressBar.Properties.StartColor = Color.FromArgb(240, goalColor.R, goalColor.G, goalColor.B);
            progressBar.EditValue = (int)Math.Min(100, goal.ProgressPercentage);
            content.Controls.Add(progressBar);

            // Yüzde (sağ üst) - Küçültüldü
            var lblPercent = new LabelControl
            {
                Text = $"%{goal.ProgressPercentage:F0}",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = goalColor,
                Location = new Point(content.Width - 50, 2),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(45, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } }
            };
            content.Controls.Add(lblPercent);

            // Kalan veya Tamamlandı - Küçültüldü
            if (goal.ProgressPercentage >= 100)
            {
                var lblCompleted = new LabelControl
                {
                    Text = "✔ Tamamlandı!",
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    ForeColor = SuccessGreen,
                    Location = new Point(48, 60),
                    AutoSizeMode = LabelAutoSizeMode.None,
                    Size = new Size(width - 70, 14),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                content.Controls.Add(lblCompleted);
            }
            else
            {
                double remaining = Math.Max(0, goal.TargetValue - goal.CurrentValue);
                var lblRemaining = new LabelControl
                {
                    Text = $"Hedefe kalan: {remaining:F1} {goal.Unit}",
                    Font = new Font("Segoe UI", 8.5F),
                    ForeColor = TextSecondary,
                    Location = new Point(48, 60),
                    AutoSizeMode = LabelAutoSizeMode.None,
                    Size = new Size(width - 70, 14),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                content.Controls.Add(lblRemaining);
            }

            content.Resize += (s, e) => {
                lblPercent.Location = new Point(content.Width - 60, 3);
            };

            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(250, 250, 250);
            card.MouseLeave += (s, e) => card.BackColor = CardColor;

            card.Controls.Add(content);
            return card;
        }

        private PanelControl CreateQuickTrackPanel(int width)
        {
            var panel = new PanelControl
            {
                Size = new Size(width, 90),
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.Transparent
            };

            var lblTitle = new LabelControl
            {
                Text = "⚡ Hızlı Takip",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, 0),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(150, 20)
            };
            panel.Controls.Add(lblTitle);

            int buttonWidth = (width - 30) / 3;
            int x = 0;

            // +1 Bardak Su
            var btnWater = new SimpleButton
            {
                Text = "💧 +1 Bardak Su\n200 ml Ekle",
                Location = new Point(x, 25),
                Size = new Size(buttonWidth, 60),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = InfoBlue, ForeColor = Color.White },
                Cursor = Cursors.Hand
            };
            btnWater.Click += (s, e) => UpdateGoal(GoalType.Water, 0.2);
            panel.Controls.Add(btnWater);
            x += buttonWidth + 15;

            // +1000 Adım
            var btnSteps = new SimpleButton
            {
                Text = "⚡ +1000 Adım\nManuel Veri Girişi",
                Location = new Point(x, 25),
                Size = new Size(buttonWidth, 60),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = SuccessGreen, ForeColor = Color.White },
                Cursor = Cursors.Hand
            };
            btnSteps.Click += (s, e) => UpdateGoal(GoalType.Steps, 1000);
            panel.Controls.Add(btnSteps);
            x += buttonWidth + 15;

            // +1 Saat Uyku
            var btnSleep = new SimpleButton
            {
                Text = "😴 +1 Saat Uyku\nSüre Güncelle",
                Location = new Point(x, 25),
                Size = new Size(buttonWidth, 60),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = WarningOrange, ForeColor = Color.White },
                Cursor = Cursors.Hand
            };
            btnSleep.Click += (s, e) => UpdateGoal(GoalType.Sleep, 1);
            panel.Controls.Add(btnSleep);

            return panel;
        }

        private PanelControl CreateTrendPanel(int width)
        {
            var panel = new PanelControl
            {
                Size = new Size(width, 150),
                BorderStyle = BorderStyles.Simple,
                BackColor = CardColor,
                Padding = new Padding(15)
            };

            var header = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 25,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.Transparent
            };

            var lblTitle = new LabelControl
            {
                Text = "Hedef Gerçekleşme Trendi",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, 3),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(200, 20)
            };
            header.Controls.Add(lblTitle);

            var comboPeriod = new ComboBoxEdit
            {
                Location = new Point(width - 140, 1),
                Size = new Size(120, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            comboPeriod.Properties.Items.AddRange(new[] { "Son 7 Gün", "Son 30 Gün", "Son 3 Ay" });
            comboPeriod.SelectedIndex = 0;
            header.Controls.Add(comboPeriod);

            panel.Controls.Add(header);

            // Chart Panel
            var chartPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 5, 0, 0)
            };
            chartPanel.Paint += (s, e) => PaintTrendChart(e.Graphics, chartPanel);
            panel.Controls.Add(chartPanel);

            return panel;
        }

        private void PaintTrendChart(Graphics g, PanelControl panel)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = panel.Width;
            int h = panel.Height;

            if (w < 100 || h < 50) return;

            // Bar chart için basit çizim
            string[] days = { "PZT", "SAL", "ÇAR", "PER", "CUM", "CMT", "PAZ" };
            int[] values = { 85, 92, 78, 95, 88, 90, 87 }; // Örnek veri

            int barWidth = (w - 60) / 7;
            int maxHeight = h - 40;
            int left = 30;
            int bottom = h - 20;

            using (var brush = new SolidBrush(Color.FromArgb(14, 165, 233)))
            using (var font = new Font("Segoe UI", 8F))
            using (var textBrush = new SolidBrush(TextSecondary))
            {
                for (int i = 0; i < 7; i++)
                {
                    int barHeight = (int)(values[i] * maxHeight / 100.0);
                    int x = left + i * barWidth;
                    int y = bottom - barHeight;

                    g.FillRectangle(brush, x, y, barWidth - 5, barHeight);
                    g.DrawString(days[i], font, textBrush, x + (barWidth - 5) / 2 - 10, bottom + 3);
                }
            }
        }

        private PanelControl CreateWeeklyAchievementCard(int width)
        {
            var card = new PanelControl
            {
                Size = new Size(width, 90),
                BorderStyle = BorderStyles.Simple,
                BackColor = PrimaryColor,
                Padding = new Padding(18, 15, 18, 15)
            };

            // Trophy icon - Küçültüldü
            var lblTrophy = new LabelControl
            {
                Text = "🏆",
                Font = new Font("Segoe UI", 24F),
                Location = new Point(0, 5),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(40, 40)
            };
            card.Controls.Add(lblTrophy);

            // Text - Küçültüldü
            var lblText = new LabelControl
            {
                Text = "Bu hafta 4 hedefini de %90 üzerinde tamamladın. Harika gidiyorsun!",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Black, // Siyah renk - okunabilir olması için
                Location = new Point(50, 10),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(width - 180, 40)
            };
            card.Controls.Add(lblText);

            // Button - Küçültüldü
            var btnDetails = new SimpleButton
            {
                Text = "Detayları Gör",
                Location = new Point(width - 120, 25),
                Size = new Size(100, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = Color.White, ForeColor = PrimaryColor },
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            card.Controls.Add(btnDetails);

            return card;
        }

        private string GetGoalTitle(GoalType type)
        {
            switch (type)
            {
                case GoalType.Weight: return "Kilo Hedefi";
                case GoalType.Water: return "Su Tüketimi";
                case GoalType.Steps: return "Günlük Adım";
                case GoalType.Sleep: return "Uyku Düzeni";
                case GoalType.Exercise: return "Egzersiz";
                case GoalType.Calories: return "Kalori";
                default: return "Hedef";
            }
        }

        private Color GetGoalColor(Goal goal)
        {
            switch (goal.GoalType)
            {
                case GoalType.Weight: return Color.FromArgb(79, 70, 229); // Mor
                case GoalType.Water: return Color.FromArgb(14, 165, 233); // Mavi
                case GoalType.Steps: return Color.FromArgb(34, 197, 94); // Yeşil
                case GoalType.Sleep: return Color.FromArgb(245, 158, 11); // Turuncu
                case GoalType.Exercise: return Color.FromArgb(239, 68, 68); // Kırmızı
                case GoalType.Calories: return Color.FromArgb(168, 85, 247); // Mor
                default: return PrimaryColor;
            }
        }

        private string GetGoalIcon(GoalType type)
        {
            switch (type)
            {
                case GoalType.Weight: return "⚖️";
                case GoalType.Water: return "💧";
                case GoalType.Steps: return "👟";
                case GoalType.Sleep: return "😴";
                case GoalType.Exercise: return "🏃";
                case GoalType.Calories: return "🔥";
                default: return "🎯";
            }
        }

        private void UpdateGoal(GoalType type, double increment)
        {
            var goal = _goals?.FirstOrDefault(g => g.GoalType == type);
            if (goal != null)
            {
                _goalService.IncrementGoal(goal.Id, increment);
                LoadGoals();
                ToastNotification.ShowSuccess($"{goal.GoalTypeName} güncellendi!");
            }
            else
            {
                ToastNotification.ShowWarning($"{type} hedefi bulunamadı. Doktorunuzdan bu hedefi atamasını isteyin.");
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1100, 600);
            this.Name = "FrmGoals";
            this.ResumeLayout(false);
        }
    }
}
