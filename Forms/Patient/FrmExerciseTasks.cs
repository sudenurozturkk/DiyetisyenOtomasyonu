using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Forms.Patient
{
    /// <summary>
    /// Egzersiz Görevlerim - Modern Hasta Paneli
    /// </summary>
    public partial class FrmExerciseTasks : XtraForm
    {
        private readonly ExerciseTaskRepository _repository;
        private List<ExerciseTask> _tasks;

        // Modern Renkler - Yeşil Tema
        private readonly Color PrimaryGreen = Color.FromArgb(13, 148, 136);
        private readonly Color SuccessGreen = Color.FromArgb(34, 197, 94);
        private readonly Color DangerRed = Color.FromArgb(239, 68, 68);
        private readonly Color WarningOrange = Color.FromArgb(249, 115, 22);
        private readonly Color InfoBlue = Color.FromArgb(59, 130, 246);
        private readonly Color CardWhite = Color.White;
        private readonly Color BackgroundLight = Color.FromArgb(248, 250, 252);
        private readonly Color TextDark = Color.FromArgb(30, 41, 59);
        private readonly Color TextMedium = Color.FromArgb(100, 116, 139);
        private readonly Color BorderGray = Color.FromArgb(226, 232, 240);

        // Controls
        private GridControl gridTasks;
        private GridView viewTasks;
        private Label lblTotalTasks, lblCompletedTasks, lblPendingTasks, lblSuccessRate;

        public FrmExerciseTasks()
        {
            _repository = new ExerciseTaskRepository();
            InitializeComponent();
            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            this.Text = "Egzersiz Görevlerim";
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = BackgroundLight;
            this.Padding = new Padding(15);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));  // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // Stats
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Grid

            mainLayout.Controls.Add(CreateHeader(), 0, 0);
            mainLayout.Controls.Add(CreateStatsSection(), 0, 1);
            mainLayout.Controls.Add(CreateGridSection(), 0, 2);

            this.Controls.Add(mainLayout);
        }

        private Panel CreateHeader()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = CardWhite, Padding = new Padding(20), Margin = new Padding(0, 0, 0, 15) };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 12);

            var lblTitle = new Label
            {
                Text = "🏃 Egzersiz Görevlerim",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(20, 15),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            var lblSubtitle = new Label
            {
                Text = "Size atanan egzersizleri buradan takip edebilir ve tamamlayabilirsiniz.",
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextMedium,
                Location = new Point(22, 45),
                AutoSize = true
            };
            panel.Controls.Add(lblSubtitle);

            var btnRefresh = new SimpleButton
            {
                Text = "YENİLE",
                Location = new Point(panel.Width - 120, 25),
                Size = new Size(100, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryGreen, ForeColor = Color.White }
            };
            btnRefresh.Click += (s, e) => LoadData();
            panel.Controls.Add(btnRefresh);

            return panel;
        }

        private Panel CreateStatsSection()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Margin = new Padding(0, 0, 0, 15) };
            
            int cardWidth = 250;
            int gap = 20;

            CreateStatCard(panel, 0, "Toplam Görev", "0", InfoBlue, out lblTotalTasks);
            CreateStatCard(panel, cardWidth + gap, "Tamamlanan", "0", SuccessGreen, out lblCompletedTasks);
            CreateStatCard(panel, (cardWidth + gap) * 2, "Bekleyen", "0", WarningOrange, out lblPendingTasks);
            CreateStatCard(panel, (cardWidth + gap) * 3, "Başarı Oranı", "%0", PrimaryGreen, out lblSuccessRate);

            return panel;
        }

        private void CreateStatCard(Panel parent, int x, string title, string value, Color color, out Label lblValueRef)
        {
            var card = new Panel
            {
                Location = new Point(x, 0),
                Size = new Size(250, 100),
                BackColor = CardWhite
            };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 12, color);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextMedium,
                Location = new Point(20, 20),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(20, 45),
                AutoSize = true
            };
            card.Controls.Add(lblValue);
            lblValueRef = lblValue;

            parent.Controls.Add(card);
        }

        private Panel CreateGridSection()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = CardWhite, Padding = new Padding(10) };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 12);

            gridTasks = new GridControl { Dock = DockStyle.Fill, LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat } };
            viewTasks = new GridView(gridTasks);
            gridTasks.MainView = viewTasks;

            viewTasks.OptionsView.ShowGroupPanel = false;
            viewTasks.OptionsView.ShowIndicator = false;
            viewTasks.OptionsBehavior.Editable = true;
            viewTasks.RowHeight = 45;
            
            viewTasks.Appearance.HeaderPanel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            viewTasks.Appearance.HeaderPanel.BackColor = Color.FromArgb(241, 245, 249);
            viewTasks.Appearance.HeaderPanel.ForeColor = TextDark;
            viewTasks.Appearance.Row.Font = new Font("Segoe UI", 10F);

            // Kolonlar
            viewTasks.Columns.Add(new GridColumn { FieldName = "Title", Caption = "Egzersiz", Visible = true, Width = 200 });
            viewTasks.Columns.Add(new GridColumn { FieldName = "DurationMinutes", Caption = "Süre (dk)", Visible = true, Width = 80 });
            viewTasks.Columns.Add(new GridColumn { FieldName = "DifficultyText", Caption = "Zorluk", Visible = true, Width = 80 });
            viewTasks.Columns.Add(new GridColumn { FieldName = "DueDate", Caption = "Tarih", Visible = true, Width = 100 });
            viewTasks.Columns["DueDate"].DisplayFormat.FormatString = "dd.MM.yyyy";

            var colStatus = viewTasks.Columns.Add();
            colStatus.FieldName = "IsCompleted";
            colStatus.Caption = "Tamamlandı";
            colStatus.Visible = true;
            colStatus.Width = 100;
            var checkEdit = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            checkEdit.CheckBoxOptions.Style = DevExpress.XtraEditors.Controls.CheckBoxStyle.SvgCheckBox1;
            colStatus.ColumnEdit = checkEdit;

            // Sadece Checkbox düzenlenebilir
            viewTasks.ShowingEditor += (s, e) =>
            {
                if (viewTasks.FocusedColumn.FieldName != "IsCompleted")
                    e.Cancel = true;
            };

            viewTasks.CellValueChanged += ViewTasks_CellValueChanged;

            panel.Controls.Add(gridTasks);
            return panel;
        }

        private void ViewTasks_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "IsCompleted")
            {
                var task = viewTasks.GetRow(e.RowHandle) as ExerciseTask;
                if (task != null)
                {
                    try
                    {
                        _repository.UpdateStatus(task.Id, task.IsCompleted);
                        UpdateStats();
                        ToastNotification.ShowSuccess(task.IsCompleted ? "Egzersiz tamamlandı! 💪" : "Egzersiz durumu güncellendi.");
                    }
                    catch (Exception ex)
                    {
                        ToastNotification.ShowError("Hata: " + ex.Message);
                    }
                }
            }
        }

        private void LoadData()
        {
            try
            {
                _tasks = _repository.GetByPatient(AuthContext.UserId).OrderByDescending(t => t.DueDate).ToList();
                gridTasks.DataSource = new System.ComponentModel.BindingList<ExerciseTask>(_tasks);
                UpdateStats();
            }
            catch (Exception ex)
            {
                ToastNotification.ShowError("Veriler yüklenirken hata: " + ex.Message);
            }
        }

        private void UpdateStats()
        {
            if (_tasks == null) return;

            int total = _tasks.Count;
            int completed = _tasks.Count(t => t.IsCompleted);
            int pending = total - completed;
            double rate = total > 0 ? (double)completed / total * 100 : 0;

            lblTotalTasks.Text = total.ToString();
            lblCompletedTasks.Text = completed.ToString();
            lblPendingTasks.Text = pending.ToString();
            lblSuccessRate.Text = $"%{rate:F0}";
        }

        private void DrawRoundedBorder(Graphics g, Panel panel, int radius, Color? borderColor = null)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = CreateRoundedRect(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), radius))
            using (var brush = new SolidBrush(panel.BackColor))
            using (var pen = new Pen(borderColor ?? BorderGray, 1))
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
            this.ClientSize = new System.Drawing.Size(1150, 700);
            this.Name = "FrmExerciseTasks";
            this.ResumeLayout(false);
        }
    }
}
