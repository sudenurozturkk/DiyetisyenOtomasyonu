using System;
using System.Collections.Generic;
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
    public partial class FrmWeeklyMenu : XtraForm
    {
        private readonly DietService _dietService;
        private DietWeek _currentDietWeek;
        private DateTime _selectedDate;
        private List<DietDay> _dietDays;

        // Modern Renkler - UiStyles
        private Color PrimaryColor => UiStyles.PrimaryColor;
        private Color SuccessGreen => UiStyles.SuccessColor;
        private Color InfoBlue => UiStyles.InfoColor;
        private Color WarningOrange => UiStyles.WarningColor;
        private Color CardColor => Color.White;
        private Color BackgroundColor => Color.FromArgb(245, 247, 250);
        private Color TextPrimary => UiStyles.TextPrimary;
        private Color TextSecondary => UiStyles.TextSecondary;

        // UI Elements
        private PanelControl pnlDays;
        private XtraScrollableControl pnlMeals;
        private LabelControl lblTotalCalories;
        private LabelControl lblTotalProtein;
        private LabelControl lblTotalCarb;
        private LabelControl lblTotalFat;
        private ComboBoxEdit cmbWeeks;

        public FrmWeeklyMenu()
        {
            InitializeComponent();
            _dietService = new DietService();
            _selectedDate = DateTime.Today;
            SetupUI();
            LoadDietWeeks();
        }

        private void SetupUI()
        {
            this.Text = "Haftalık Menü";
            this.BackColor = BackgroundColor;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(20);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // Header & Summary
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Days
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Meals

            // 1. Header & Summary
            var headerPanel = CreateHeaderPanel();
            mainLayout.Controls.Add(headerPanel, 0, 0);

            // 2. Days Navigation
            pnlDays = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.Transparent
            };
            mainLayout.Controls.Add(pnlDays, 0, 1);

            // 3. Meals List
            pnlMeals = new XtraScrollableControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            mainLayout.Controls.Add(pnlMeals, 0, 2);

            this.Controls.Add(mainLayout);
        }

        private PanelControl CreateHeaderPanel()
        {
            var panel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = Color.Transparent
            };

            // Title & Week Selection
            var lblTitle = new LabelControl
            {
                Text = "📅 Haftalık Yemek Planım",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, 10),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            cmbWeeks = new ComboBoxEdit
            {
                Location = new Point(300, 12),
                Size = new Size(200, 30),
                Properties = { TextEditStyle = TextEditStyles.DisableTextEditor }
            };
            cmbWeeks.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            cmbWeeks.SelectedIndexChanged += (s, e) => LoadSelectedWeek();
            panel.Controls.Add(cmbWeeks);

            var btnRefresh = new SimpleButton
            {
                Text = "Planı Getir",
                Location = new Point(510, 10),
                Size = new Size(100, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryColor, ForeColor = Color.White, BorderColor = PrimaryColor }
            };
            btnRefresh.Click += (s, e) => LoadDietWeeks();
            panel.Controls.Add(btnRefresh);

            // Summary Cards
            int cardWidth = 180;
            int spacing = 15;
            int startY = 60;

            lblTotalCalories = CreateSummaryCard(panel, 0, startY, "Günlük Ort. Kalori", "0 kcal", WarningOrange);
            lblTotalProtein = CreateSummaryCard(panel, cardWidth + spacing, startY, "Günlük Protein", "0 g", PrimaryColor);
            lblTotalCarb = CreateSummaryCard(panel, 2 * (cardWidth + spacing), startY, "Günlük Karb", "0 g", InfoBlue);
            lblTotalFat = CreateSummaryCard(panel, 3 * (cardWidth + spacing), startY, "Günlük Yağ", "0 g", Color.Gray);

            return panel;
        }

        private LabelControl CreateSummaryCard(PanelControl parent, int x, int y, string title, string value, Color color)
        {
            var card = new PanelControl
            {
                Location = new Point(x, y),
                Size = new Size(180, 80),
                BorderStyle = BorderStyles.NoBorder,
                BackColor = CardColor
            };
            
            // Left border
            var border = new PanelControl
            {
                Dock = DockStyle.Left,
                Width = 4,
                BorderStyle = BorderStyles.NoBorder,
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
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(15, 35),
                AutoSize = true
            };
            card.Controls.Add(lblValue);

            parent.Controls.Add(card);
            return lblValue;
        }

        private void LoadDietWeeks()
        {
            var weeks = _dietService.GetDietWeeks(AuthContext.UserId);
            cmbWeeks.Properties.Items.Clear();
            
            if (weeks.Any())
            {
                foreach (var week in weeks)
                {
                    cmbWeeks.Properties.Items.Add(new ComboBoxItem(week));
                }
                cmbWeeks.SelectedIndex = 0; // Select latest
            }
            else
            {
                XtraMessageBox.Show("Henüz atanmış bir diyet listeniz bulunmuyor.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LoadSelectedWeek()
        {
            if (cmbWeeks.SelectedItem is ComboBoxItem item)
            {
                _currentDietWeek = item.Value;
                _dietDays = _dietService.GetDietDays(_currentDietWeek.Id).ToList();
                
                CreateDayButtons();
                LoadMealsForDate(_selectedDate);
                UpdateSummary();
            }
        }

        private void CreateDayButtons()
        {
            pnlDays.Controls.Clear();
            string[] days = { "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi", "Pazar" };
            
            int x = 0;
            int width = 120;
            
            for (int i = 0; i < 7; i++)
            {
                var dayName = days[i];
                // Tarihi bul (Hafta başlangıcına göre)
                var date = _currentDietWeek.WeekStartDate.AddDays(i);
                bool isSelected = date.Date == _selectedDate.Date;

                var btn = new SimpleButton
                {
                    Text = dayName,
                    Location = new Point(x, 10),
                    Size = new Size(width, 40),
                    Font = new Font("Segoe UI", 9F, isSelected ? FontStyle.Bold : FontStyle.Regular),
                    Appearance = { 
                        BackColor = isSelected ? PrimaryColor : CardColor, 
                        ForeColor = isSelected ? Color.White : TextPrimary,
                        BorderColor = isSelected ? PrimaryColor : Color.LightGray
                    },
                    Cursor = Cursors.Hand
                };
                
                btn.Click += (s, e) => {
                    _selectedDate = date;
                    CreateDayButtons(); // Refresh styles
                    LoadMealsForDate(_selectedDate);
                };

                pnlDays.Controls.Add(btn);
                x += width + 10;
            }
        }

        private void LoadMealsForDate(DateTime date)
        {
            pnlMeals.Controls.Clear();
            
            var day = _dietDays.FirstOrDefault(d => d.Date.Date == date.Date);
            if (day == null)
            {
                var lbl = new LabelControl { Text = "Bu gün için plan bulunamadı.", Location = new Point(20, 20) };
                pnlMeals.Controls.Add(lbl);
                return;
            }

            var meals = _dietService.GetMealItems(day.Id);
            int y = 0;

            foreach (var meal in meals)
            {
                var card = CreateMealCard(meal);
                card.Location = new Point(0, y);
                card.Width = pnlMeals.Width - 20;
                pnlMeals.Controls.Add(card);
                y += card.Height + 15;
            }
        }

        private PanelControl CreateMealCard(MealItem meal)
        {
            var card = new PanelControl
            {
                Height = 80,
                BorderStyle = BorderStyles.NoBorder,
                BackColor = CardColor,
                Padding = new Padding(15)
            };

            // Icon
            var lblIcon = new LabelControl
            {
                Text = GetMealIcon(meal.MealType),
                Font = new Font("Segoe UI", 20F),
                Location = new Point(15, 20),
                AutoSize = true
            };
            card.Controls.Add(lblIcon);

            // Meal Name & Type
            var lblName = new LabelControl
            {
                Text = meal.Name,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(60, 15),
                AutoSize = true
            };
            card.Controls.Add(lblName);

            var lblType = new LabelControl
            {
                Text = meal.MealType.ToString(),
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextSecondary,
                Location = new Point(60, 40),
                AutoSize = true
            };
            card.Controls.Add(lblType);

            // Macros
            var lblMacros = new LabelControl
            {
                Text = $"{meal.Calories} kcal | P: {meal.Protein}g | K: {meal.Carbs}g | Y: {meal.Fat}g",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = InfoBlue,
                Location = new Point(300, 30),
                AutoSize = true
            };
            card.Controls.Add(lblMacros);

            // Checkbox
            var chk = new CheckEdit
            {
                Text = "Yedim",
                Location = new Point(card.Width - 100, 25),
                Properties = { Caption = "Yedim" }
            };
            chk.Checked = meal.IsConfirmedByPatient;
            chk.CheckedChanged += (s, e) => {
                _dietService.ConfirmMeal(meal.Id, chk.Checked);
                meal.IsConfirmedByPatient = chk.Checked;
            };
            card.Controls.Add(chk);

            // Resize
            card.Resize += (s, e) => chk.Location = new Point(card.Width - 100, 25);

            return card;
        }

        private string GetMealIcon(MealType type)
        {
            switch (type)
            {
                case MealType.Breakfast: return "🍳";
                case MealType.Lunch: return "🥗";
                case MealType.Dinner: return "🥩";
                case MealType.Snack: return "🍎";
                default: return "🍽️";
            }
        }

        private void UpdateSummary()
        {
            if (_dietDays == null || !_dietDays.Any()) return;

            // Calculate averages or totals for the selected day
            var day = _dietDays.FirstOrDefault(d => d.Date.Date == _selectedDate.Date);
            if (day != null)
            {
                var meals = _dietService.GetMealItems(day.Id);
                lblTotalCalories.Text = $"{meals.Sum(m => m.Calories):F0} kcal";
                lblTotalProtein.Text = $"{meals.Sum(m => m.Protein):F0} g";
                lblTotalCarb.Text = $"{meals.Sum(m => m.Carbs):F0} g";
                lblTotalFat.Text = $"{meals.Sum(m => m.Fat):F0} g";
            }
        }

        private class ComboBoxItem
        {
            public DietWeek Value { get; }
            public ComboBoxItem(DietWeek value) { Value = value; }
            public override string ToString() => $"Hafta Başlangıcı: {Value.WeekStartDate:dd.MM.yyyy}";
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1100, 600);
            this.Name = "FrmWeeklyMenu";
            this.ResumeLayout(false);
        }
    }
}
