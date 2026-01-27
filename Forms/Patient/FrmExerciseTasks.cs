using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Forms.Patient
{
    public partial class FrmExerciseTasks : XtraForm
    {
        private readonly ExerciseService _exerciseService;
        private XtraScrollableControl pnlTasks;
        private LabelControl lblTotalTasks;
        private LabelControl lblCompletedTasks;
        private LabelControl lblCompletionRate;

        // Modern Renkler - UiStyles
        private Color PrimaryColor => UiStyles.PrimaryColor;
        private Color SuccessGreen => UiStyles.SuccessColor;
        private Color InfoBlue => UiStyles.InfoColor;
        private Color WarningOrange => UiStyles.WarningColor;
        private Color DangerRed => UiStyles.DangerColor;
        private Color CardColor => Color.White;
        private Color BackgroundColor => Color.FromArgb(245, 247, 250);
        private Color TextPrimary => UiStyles.TextPrimary;
        private Color TextSecondary => UiStyles.TextSecondary;

        public FrmExerciseTasks()
        {
            InitializeComponent();
            _exerciseService = new ExerciseService();
            SetupUI();
            LoadTasks();
        }

        private void SetupUI()
        {
            this.Text = "Egzersiz Görevlerim";
            this.BackColor = BackgroundColor;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(20);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // Summary
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Tasks

            // 1. Summary Panel
            var summaryPanel = CreateSummaryPanel();
            mainLayout.Controls.Add(summaryPanel, 0, 0);

            // 2. Tasks List
            pnlTasks = new XtraScrollableControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            mainLayout.Controls.Add(pnlTasks, 0, 1);

            this.Controls.Add(mainLayout);
        }

        private PanelControl CreateSummaryPanel()
        {
            var panel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.Transparent
            };

            int cardWidth = 200;
            int spacing = 20;

            lblTotalTasks = CreateStatCard(panel, 0, 10, "Toplam Görev", "0", InfoBlue);
            lblCompletedTasks = CreateStatCard(panel, cardWidth + spacing, 10, "Tamamlanan", "0", SuccessGreen);
            lblCompletionRate = CreateStatCard(panel, 2 * (cardWidth + spacing), 10, "Başarı Oranı", "%0", WarningOrange);

            var btnRefresh = new SimpleButton
            {
                Text = "Yenile",
                Location = new Point(panel.Width - 120, 30),
                Size = new Size(100, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Appearance = { BackColor = PrimaryColor, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) }
            };
            btnRefresh.Click += (s, e) => LoadTasks();
            panel.Controls.Add(btnRefresh);

            return panel;
        }

        private LabelControl CreateStatCard(PanelControl parent, int x, int y, string title, string value, Color color)
        {
            var card = new PanelControl
            {
                Location = new Point(x, y),
                Size = new Size(200, 80),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardColor
            };

            var border = new PanelControl
            {
                Dock = DockStyle.Left,
                Width = 4,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = color
            };
            card.Controls.Add(border);

            var lblTitle = new LabelControl
            {
                Text = title,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextSecondary,
                Location = new Point(15, 15),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            var lblValue = new LabelControl
            {
                Text = value,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(15, 35),
                AutoSize = true
            };
            card.Controls.Add(lblValue);

            parent.Controls.Add(card);
            return lblValue;
        }

        private void LoadTasks()
        {
            pnlTasks.Controls.Clear();
            var tasks = _exerciseService.GetPatientTasks(AuthContext.UserId);

            // Update stats
            lblTotalTasks.Text = tasks.Count.ToString();
            int completed = tasks.Count(t => t.IsCompleted);
            lblCompletedTasks.Text = completed.ToString();
            lblCompletionRate.Text = tasks.Count > 0 ? $"%{(int)((double)completed / tasks.Count * 100)}" : "%0";

            if (tasks.Count == 0)
            {
                var lbl = new LabelControl { Text = "Henüz atanmış egzersiz görevi bulunmuyor.", Location = new Point(20, 20) };
                pnlTasks.Controls.Add(lbl);
                return;
            }

            int y = 0;
            foreach (var task in tasks.OrderBy(t => t.IsCompleted).ThenBy(t => t.DueDate))
            {
                var card = CreateTaskCard(task);
                card.Location = new Point(0, y);
                card.Width = pnlTasks.Width - 20;
                pnlTasks.Controls.Add(card);
                y += card.Height + 15;
            }
        }

        private PanelControl CreateTaskCard(ExerciseTask task)
        {
            var card = new PanelControl
            {
                Height = 100,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = task.IsCompleted ? Color.FromArgb(245, 245, 245) : CardColor,
                Padding = new Padding(15)
            };

            // Status Icon
            var lblIcon = new LabelControl
            {
                Text = task.IsCompleted ? "✅" : "🏃",
                Font = new Font("Segoe UI", 20F),
                Location = new Point(15, 30),
                AutoSize = true
            };
            card.Controls.Add(lblIcon);

            // Title & Desc
            var lblTitle = new LabelControl
            {
                Text = task.Title,
                Font = new Font("Segoe UI", 12F, task.IsCompleted ? FontStyle.Strikeout : FontStyle.Bold),
                ForeColor = task.IsCompleted ? TextSecondary : TextPrimary,
                Location = new Point(60, 20),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            var lblDesc = new LabelControl
            {
                Text = $"{task.DurationMinutes} dk | {GetDifficultyText(task.DifficultyLevel)}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextSecondary,
                Location = new Point(60, 50),
                AutoSize = true
            };
            card.Controls.Add(lblDesc);

            // Action Button (if not completed)
            if (!task.IsCompleted)
            {
                var btnComplete = new SimpleButton
                {
                    Text = "Tamamla",
                    Location = new Point(card.Width - 120, 30),
                    Size = new Size(100, 40),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Appearance = { BackColor = SuccessGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
                };
                btnComplete.Click += (s, e) => {
                    _exerciseService.CompleteTask(task.Id);
                    LoadTasks();
                };
                card.Controls.Add(btnComplete);
            }
            else
            {
                var lblDate = new LabelControl
                {
                    Text = $"Tamamlandı: {task.CompletedAt?.ToString("dd.MM.yyyy")}",
                    Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                    ForeColor = SuccessGreen,
                    Location = new Point(card.Width - 150, 40),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    AutoSize = true
                };
                card.Controls.Add(lblDate);
            }

            // Resize handler
            card.Resize += (s, e) => {
                // Button location update handled by Anchor
            };

            return card;
        }

        private string GetDifficultyText(int level)
        {
            switch (level)
            {
                case 1: return "Kolay";
                case 2: return "Orta";
                case 3: return "Zor";
                default: return "Normal";
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1100, 600);
            this.Name = "FrmExerciseTasks";
            this.ResumeLayout(false);
        }
    }
}
