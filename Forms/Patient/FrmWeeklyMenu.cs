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
using DevExpress.XtraTab;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Forms.Patient
{
    /// <summary>
    /// Haftalık menü ve öğün onaylama formu - Modern Tasarım
    /// </summary>
    public partial class FrmWeeklyMenu : XtraForm
    {
        private readonly DietService _dietService;
        private DateEdit dateWeek;
        private SimpleButton btnLoadWeek;
        private XtraTabControl tabDays;
        private Panel pnlSummaryCards;

        private DietWeek _currentWeek;
        private System.Collections.Generic.Dictionary<int, GridControl> _dayGrids = new System.Collections.Generic.Dictionary<int, GridControl>();

        // Modern Renkler - Yeşil Tema
        private readonly Color PrimaryGreen = Color.FromArgb(13, 148, 136);
        private readonly Color SuccessGreen = Color.FromArgb(34, 197, 94);
        private readonly Color InfoBlue = Color.FromArgb(59, 130, 246);
        private readonly Color WarningOrange = Color.FromArgb(249, 115, 22);
        private readonly Color CardWhite = Color.White;
        private readonly Color BackgroundLight = Color.FromArgb(248, 250, 252);
        private readonly Color TextDark = Color.FromArgb(30, 41, 59);
        private readonly Color TextMedium = Color.FromArgb(100, 116, 139);
        private readonly Color BorderGray = Color.FromArgb(226, 232, 240);

        public FrmWeeklyMenu()
        {
            InitializeComponent();
            _dietService = new DietService();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Haftalık Menüm";
            this.BackColor = BackgroundLight;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(15);

            // Ana Layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));  // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // Summary Cards
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Tabs/Grid

            // 1. Header Panel
            var pnlHeader = CreateHeaderPanel();
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // 2. Summary Cards Panel
            pnlSummaryCards = CreateSummaryPanel();
            mainLayout.Controls.Add(pnlSummaryCards, 0, 1);

            // 3. Tab Control (Günler)
            tabDays = new XtraTabControl
            {
                Dock = DockStyle.Fill,
                Appearance = { BackColor = BackgroundLight },
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            tabDays.AppearancePage.Header.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            tabDays.AppearancePage.HeaderActive.ForeColor = PrimaryGreen;
            
            mainLayout.Controls.Add(tabDays, 0, 2);

            string[] dayNames = { "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi", "Pazar" };
            for (int i = 0; i < 7; i++)
            {
                var tabPage = new XtraTabPage { Text = dayNames[i] };
                tabPage.Appearance.Header.Font = new Font("Segoe UI", 10F);
                tabDays.TabPages.Add(tabPage);
                CreateDayPanel(tabPage, i);
            }

            this.Controls.Add(mainLayout);
        }

        private Panel CreateHeaderPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            var lblTitle = new Label
            {
                Text = "📅 Haftalık Yemek Planım",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(0, 10),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            // Tarih Seçimi
            var lblWeek = new Label
            {
                Text = "Hafta Başlangıcı:",
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextMedium,
                Location = new Point(300, 18),
                AutoSize = true
            };
            panel.Controls.Add(lblWeek);

            dateWeek = new DateEdit
            {
                Location = new Point(410, 15),
                Size = new Size(140, 30),
                Properties = { Appearance = { Font = new Font("Segoe UI", 10F) } }
            };
            dateWeek.DateTime = GetMonday(DateTime.Now);
            panel.Controls.Add(dateWeek);

            btnLoadWeek = new SimpleButton
            {
                Text = "Planı Getir",
                Location = new Point(560, 14),
                Size = new Size(100, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryGreen, ForeColor = Color.White }
            };
            btnLoadWeek.Click += BtnLoadWeek_Click;
            panel.Controls.Add(btnLoadWeek);

            return panel;
        }

        private Panel CreateSummaryPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0, 5, 0, 15) };
            
            // 4 Kart: Kalori, Protein, Karb, Yağ
            int cardWidth = 200;
            int gap = 20;

            CreateSummaryCard(panel, 0, "🔥 Günlük Ort. Kalori", "0 kcal", WarningOrange);
            CreateSummaryCard(panel, cardWidth + gap, "🥩 Günlük Protein", "0 g", PrimaryGreen);
            CreateSummaryCard(panel, (cardWidth + gap) * 2, "🍞 Günlük Karb", "0 g", InfoBlue);
            CreateSummaryCard(panel, (cardWidth + gap) * 3, "🥑 Günlük Yağ", "0 g", Color.Gray);

            return panel;
        }

        private void CreateSummaryCard(Panel parent, int x, string title, string value, Color color)
        {
            var card = new Panel
            {
                Location = new Point(x, 5),
                Size = new Size(200, 80),
                BackColor = CardWhite
            };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 10, color);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextMedium,
                Location = new Point(15, 15),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            var lblValue = new Label
            {
                Name = "lblValue_" + title.Split(' ')[1], // Tag için basit isim
                Text = value,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(15, 35),
                AutoSize = true
            };
            card.Controls.Add(lblValue);

            parent.Controls.Add(card);
        }

        private void CreateDayPanel(XtraTabPage tabPage, int dayIndex)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = CardWhite, Padding = new Padding(10) };
            
            var grid = new GridControl { Dock = DockStyle.Fill, LookAndFeel = { UseDefaultLookAndFeel = false, Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat } };
            var view = new GridView(grid);
            grid.MainView = view;
            
            // Grid Ayarları
            view.OptionsView.ShowGroupPanel = false;
            view.OptionsView.ShowIndicator = false;
            view.OptionsBehavior.Editable = true; // Checkbox için true olmalı
            view.RowHeight = 45;
            
            view.Appearance.HeaderPanel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            view.Appearance.HeaderPanel.BackColor = Color.FromArgb(241, 245, 249);
            view.Appearance.HeaderPanel.ForeColor = TextDark;
            
            view.Appearance.Row.Font = new Font("Segoe UI", 10F);
            view.Appearance.Row.ForeColor = TextDark;

            // Kolonlar
            view.Columns.Add(new GridColumn { FieldName = "MealTypeName", Caption = "Öğün", Visible = true, Width = 100 });
            view.Columns.Add(new GridColumn { FieldName = "Name", Caption = "Yemek / Besin", Visible = true, Width = 300 });
            view.Columns.Add(new GridColumn { FieldName = "Calories", Caption = "Kalori", Visible = true, Width = 80 });
            view.Columns.Add(new GridColumn { FieldName = "Protein", Caption = "Prot (g)", Visible = true, Width = 80 });
            view.Columns.Add(new GridColumn { FieldName = "Carbs", Caption = "Karb (g)", Visible = true, Width = 80 });
            
            var colConfirmed = view.Columns.Add();
            colConfirmed.FieldName = "IsConfirmedByPatient";
            colConfirmed.Caption = "Yedim ✅";
            colConfirmed.Visible = true;
            colConfirmed.Width = 80;
            var checkEdit = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            checkEdit.CheckBoxOptions.Style = DevExpress.XtraEditors.Controls.CheckBoxStyle.SvgCheckBox1;
            colConfirmed.ColumnEdit = checkEdit;

            // Sadece Checkbox düzenlenebilir olsun
            view.ShowingEditor += (s, e) => 
            {
                if (view.FocusedColumn.FieldName != "IsConfirmedByPatient")
                    e.Cancel = true;
            };

            // Checkbox değiştiğinde
            view.CellValueChanged += (s, e) =>
            {
                if (e.Column.FieldName == "IsConfirmedByPatient")
                {
                    var meal = view.GetRow(e.RowHandle) as MealItem;
                    if (meal != null)
                    {
                        _dietService.ConfirmMeal(meal.Id, meal.IsConfirmedByPatient);
                        UpdateWeekStats(); // İstatistikleri güncelle
                    }
                }
            };

            panel.Controls.Add(grid);
            tabPage.Controls.Add(panel);
            _dayGrids[dayIndex] = grid;
        }

        private void BtnLoadWeek_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime weekStart = GetMonday(dateWeek.DateTime);
                _currentWeek = _dietService.GetWeeklyPlan(AuthContext.UserId, weekStart);

                if (_currentWeek == null)
                {
                    ToastNotification.ShowWarning("Bu hafta için plan bulunamadı.");
                    ClearGrids();
                    return;
                }

                // Grid'leri doldur
                for (int i = 0; i < 7; i++)
                {
                    if (i < _currentWeek.Days.Count)
                    {
                        var day = _currentWeek.Days[i];
                        _dayGrids[i].DataSource = new BindingList<MealItem>(day.Meals);
                    }
                    else
                    {
                        _dayGrids[i].DataSource = null;
                    }
                }

                UpdateWeekStats();
                ToastNotification.ShowSuccess("Haftalık menü yüklendi!");
            }
            catch (Exception ex)
            {
                ToastNotification.ShowError($"Hata: {ex.Message}");
            }
        }

        private void ClearGrids()
        {
            foreach (var grid in _dayGrids.Values)
                grid.DataSource = null;
            UpdateSummaryCard("Ort.", "0 kcal");
            UpdateSummaryCard("Protein", "0 g");
            UpdateSummaryCard("Karb", "0 g");
            UpdateSummaryCard("Yağ", "0 g");
        }

        private void UpdateWeekStats()
        {
            if (_currentWeek == null) return;

            UpdateSummaryCard("Ort.", $"{_currentWeek.AverageDailyCalories:F0} kcal");
            
            // Basit ortalamalar (gerçek veride servisten gelmeli ama burada hesaplayabiliriz)
            double totalProt = 0, totalCarb = 0, totalFat = 0;
            int days = 0;
            foreach(var day in _currentWeek.Days)
            {
                if(day.Meals.Any())
                {
                    totalProt += day.TotalProtein;
                    totalCarb += day.TotalCarbs;
                    totalFat += day.TotalFat;
                    days++;
                }
            }
            
            if (days > 0)
            {
                UpdateSummaryCard("Protein", $"{totalProt/days:F0} g");
                UpdateSummaryCard("Karb", $"{totalCarb/days:F0} g");
                UpdateSummaryCard("Yağ", $"{totalFat/days:F0} g");
            }
        }

        private void UpdateSummaryCard(string key, string value)
        {
            foreach(Control ctrl in pnlSummaryCards.Controls)
            {
                var lbl = ctrl.Controls.OfType<Label>().FirstOrDefault(l => l.Name.Contains(key));
                if (lbl != null)
                {
                    lbl.Text = value;
                    break;
                }
            }
        }

        private DateTime GetMonday(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private void DrawRoundedBorder(Graphics g, Panel panel, int radius, Color borderColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = CreateRoundedRect(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), radius))
            using (var brush = new SolidBrush(panel.BackColor))
            using (var pen = new Pen(borderColor, 2))
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
            this.ClientSize = new System.Drawing.Size(1100, 650);
            this.Name = "FrmWeeklyMenu";
            this.ResumeLayout(false);
        }
    }
}
