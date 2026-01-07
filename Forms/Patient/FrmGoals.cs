using System;
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
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Forms.Patient
{
    public partial class FrmGoals : XtraForm
    {
        private readonly GoalService _goalService;
        private BindingList<Goal> _goals;

        // Modern Renkler
        private readonly Color PrimaryGreen = Color.FromArgb(13, 148, 136);
        private readonly Color SuccessGreen = Color.FromArgb(34, 197, 94);
        private readonly Color InfoBlue = Color.FromArgb(59, 130, 246);
        private readonly Color WarningOrange = Color.FromArgb(249, 115, 22);
        private readonly Color DangerRed = Color.FromArgb(239, 68, 68);
        private readonly Color CardWhite = Color.White;
        private readonly Color BackgroundLight = Color.FromArgb(248, 250, 252);
        private readonly Color TextDark = Color.FromArgb(30, 41, 59);
        private readonly Color TextMedium = Color.FromArgb(100, 116, 139);
        private readonly Color BorderGray = Color.FromArgb(226, 232, 240);

        // UI Elements
        private FlowLayoutPanel flowGoals;
        private Panel pnlQuickUpdate;

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
            this.BackColor = BackgroundLight;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(15);

            // Main split - Sol hedefler, sağ hızlı güncelleme
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            // Sol Panel - Hedef Kartları
            var leftPanel = CreateGoalsPanel();
            mainPanel.Controls.Add(leftPanel, 0, 0);

            // Sağ Panel - Hızlı Güncelleme
            var rightPanel = CreateQuickUpdatePanel();
            mainPanel.Controls.Add(rightPanel, 1, 0);

            this.Controls.Add(mainPanel);
        }

        private Panel CreateGoalsPanel()
        {
            var container = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0, 0, 15, 0) };

            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent };
            var lblTitle = new Label
            {
                Text = "🎯 Aktif Hedeflerim",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(5, 10),
                AutoSize = true
            };
            header.Controls.Add(lblTitle);

            var btnRefresh = new SimpleButton
            {
                Text = "↻ Yenile",
                Location = new Point(230, 8),
                Size = new Size(90, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryGreen, ForeColor = Color.White }
            };
            btnRefresh.Click += (s, e) => LoadGoals();
            header.Controls.Add(btnRefresh);
            container.Controls.Add(header);

            // Scrollable goal cards container
            var scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent };
            flowGoals = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 5, 0, 0)
            };
            scrollPanel.Controls.Add(flowGoals);
            container.Controls.Add(scrollPanel);

            return container;
        }

        private Panel CreateQuickUpdatePanel()
        {
            pnlQuickUpdate = new Panel { Dock = DockStyle.Fill, BackColor = CardWhite, Padding = new Padding(20) };
            pnlQuickUpdate.Paint += (s, e) => DrawRoundedBorder(e.Graphics, pnlQuickUpdate, 12);

            var lblTitle = new Label
            {
                Text = "⚡ Hızlı Güncelle",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = PrimaryGreen,
                Location = new Point(20, 20),
                AutoSize = true
            };
            pnlQuickUpdate.Controls.Add(lblTitle);

            int y = 65;

            // Su İçme
            AddQuickButton(ref y, "💧 Su İçme", "+1 Bardak (0.25L)", PrimaryGreen, GoalType.Water, 0.25);

            // Adım
            AddQuickButton(ref y, "👟 Adım Sayısı", "+1000 Adım", InfoBlue, GoalType.Steps, 1000);

            // Uyku
            AddQuickButton(ref y, "😴 Uyku", "+1 Saat", WarningOrange, GoalType.Sleep, 1);

            return pnlQuickUpdate;
        }

        private void AddQuickButton(ref int y, string label, string buttonText, Color color, GoalType goalType, double value)
        {
            var lbl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextMedium,
                Location = new Point(20, y),
                AutoSize = true
            };
            pnlQuickUpdate.Controls.Add(lbl);

            var btn = new Button
            {
                Text = buttonText,
                Location = new Point(20, y + 25),
                Size = new Size(180, 38),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => UpdateGoal(goalType, value);
            pnlQuickUpdate.Controls.Add(btn);

            y += 85;
        }

        private void LoadGoals()
        {
            flowGoals.Controls.Clear();
            var goals = _goalService.GetActiveGoals(AuthContext.UserId);
            _goals = new BindingList<Goal>(goals);

            if (_goals.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "Henüz aktif hedef bulunmuyor.\nDoktorunuzdan hedef ataması isteyebilirsiniz.",
                    Font = new Font("Segoe UI", 11F),
                    ForeColor = TextMedium,
                    AutoSize = true,
                    Padding = new Padding(10)
                };
                flowGoals.Controls.Add(lblEmpty);
                return;
            }

            foreach (var goal in _goals)
            {
                var card = CreateGoalCard(goal);
                flowGoals.Controls.Add(card);
            }
        }

        private Panel CreateGoalCard(Goal goal)
        {
            var card = new Panel
            {
                Size = new Size(flowGoals.Parent.Width - 30, 90),
                BackColor = CardWhite,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(15)
            };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 10);

            // İkon ve Hedef Adı
            string icon = GetGoalIcon(goal.GoalType);
            var lblName = new Label
            {
                Text = $"{icon} {goal.GoalTypeName}",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 12),
                AutoSize = true
            };
            card.Controls.Add(lblName);

            // Değerler
            var lblValues = new Label
            {
                Text = $"{goal.CurrentValue:F1} / {goal.TargetValue:F1} {goal.Unit}",
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextMedium,
                Location = new Point(15, 38),
                AutoSize = true
            };
            card.Controls.Add(lblValues);

            // Progress Bar
            int progressWidth = 200;
            var progressBg = new Panel
            {
                Location = new Point(card.Width - progressWidth - 130, 20),
                Size = new Size(progressWidth, 16),
                BackColor = Color.FromArgb(229, 231, 235)
            };
            card.Controls.Add(progressBg);

            int fillWidth = Math.Min((int)(goal.ProgressPercentage / 100 * progressWidth), progressWidth);
            Color progressColor = goal.ProgressPercentage >= 100 ? SuccessGreen :
                                  goal.ProgressPercentage >= 50 ? WarningOrange : DangerRed;
            var progressFill = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(fillWidth, 16),
                BackColor = progressColor
            };
            progressBg.Controls.Add(progressFill);

            // Yüzde
            var lblPercent = new Label
            {
                Text = $"%{goal.ProgressPercentage:F0}",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = progressColor,
                Location = new Point(card.Width - 100, 18),
                AutoSize = true
            };
            card.Controls.Add(lblPercent);

            // Durum Badge
            var lblStatus = new Label
            {
                Text = goal.Status,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = progressColor,
                Location = new Point(card.Width - 100, 50),
                Size = new Size(80, 24),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblStatus);

            // Resize handler
            flowGoals.Parent.Resize += (s, e) => card.Width = flowGoals.Parent.Width - 30;

            return card;
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

        private void DrawRoundedBorder(Graphics g, Panel panel, int radius)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = CreateRoundedRect(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), radius))
            using (var brush = new SolidBrush(panel.BackColor))
            using (var pen = new Pen(BorderGray, 1))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
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
            this.ClientSize = new System.Drawing.Size(1100, 600);
            this.Name = "FrmGoals";
            this.ResumeLayout(false);
        }
    }
}
