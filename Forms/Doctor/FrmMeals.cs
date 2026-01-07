using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    /// <summary>
    /// Yemek Kutuphanesi Sayfasi
    /// </summary>
    public partial class FrmMeals : XtraForm
    {
        private readonly MealService _mealService;

        private GridControl gridMeals;
        private GridView viewMeals;
        private TextEdit txtSearch;
        private ComboBoxEdit cmbMealTimeFilter;
        private LabelControl lblTotalCount;

        // Detail Form Controls
        private TextEdit txtName;
        private MemoEdit txtDescription;
        private ComboBoxEdit cmbMealTime;
        private SpinEdit spnCalories;
        private SpinEdit spnProtein;
        private SpinEdit spnCarbs;
        private SpinEdit spnFat;
        private SpinEdit spnPortionGrams;
        private TextEdit txtPortionDesc;
        private TextEdit txtCategory;
        private MemoEdit txtNotes;
        private LabelControl lblFormTitle;

        private BindingList<Meal> _meals;
        private int _editingMealId = 0;

        // Renkler - Teal Theme (FrmPatients ile uyumlu)
        private readonly Color PrimaryColor = Color.FromArgb(0, 121, 107); // Teal
        private readonly Color DarkColor = Color.FromArgb(0, 77, 64);    // Dark Teal
        private readonly Color LightColor = Color.FromArgb(178, 223, 219); // Light Teal
        private readonly Color AccentColor = Color.FromArgb(38, 166, 154);  // Accent Teal
        
        private readonly Color SuccessGreen = Color.FromArgb(25, 135, 84);
        private readonly Color DangerRed = Color.FromArgb(220, 53, 69);
        private readonly Color WarningOrange = Color.FromArgb(255, 193, 7);
        private readonly Color BackgroundColor = Color.FromArgb(245, 247, 250);
        private readonly Color CardColor = Color.White;
        private readonly Color TextPrimary = Color.FromArgb(33, 37, 41);
        private readonly Color TextSecondary = Color.FromArgb(108, 117, 125);

        public FrmMeals()
        {
            InitializeComponent();
            _mealService = new MealService();
            InitializeUI();
            LoadMeals();
        }

        private void InitializeUI()
        {
            this.Text = "Yemek Kutuphanesi";
            this.BackColor = BackgroundColor;

            // ANA PANEL
            var mainPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = BackgroundColor,
                Padding = new Padding(10)
            };

            // SOL PANEL - Liste
            var leftPanel = new PanelControl
            {
                Dock = DockStyle.Left,
                Width = 750, // Genişletildi
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = BackgroundColor,
                Padding = new Padding(0, 0, 10, 0)
            };

            CreateListPanel(leftPanel);

            // SAG PANEL - Form
            var rightPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = BackgroundColor
            };

            CreateDetailPanel(rightPanel);

            mainPanel.Controls.Add(rightPanel);
            mainPanel.Controls.Add(leftPanel);

            this.Controls.Add(mainPanel);
        }

        private void CreateListPanel(PanelControl parent)
        {
            var grpList = new GroupControl
            {
                Text = "YEMEK KÜTÜPHANESİ",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = PrimaryColor,
                Appearance = { BackColor = CardColor }
            };

            // Toolbar
            var toolbar = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 60,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardColor,
                Padding = new Padding(10)
            };

            txtSearch = new TextEdit
            {
                Location = new Point(10, 15),
                Size = new Size(200, 30),
                Properties = { NullText = "Yemek ara...", Appearance = { Font = new Font("Segoe UI", 10F) } }
            };
            txtSearch.KeyUp += (s, e) => { if (e.KeyCode == Keys.Enter) SearchMeals(); };
            toolbar.Controls.Add(txtSearch);

            cmbMealTimeFilter = new ComboBoxEdit
            {
                Location = new Point(220, 15),
                Size = new Size(150, 30)
            };
            cmbMealTimeFilter.Properties.Items.AddRange(new[] { "Tüm Öğünler", "Kahvaltı", "Ara Öğün 1", "Öğle", "Ara Öğün 2", "Akşam", "Gece" });
            cmbMealTimeFilter.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            cmbMealTimeFilter.SelectedIndex = 0;
            cmbMealTimeFilter.SelectedIndexChanged += (s, e) => FilterMeals();
            toolbar.Controls.Add(cmbMealTimeFilter);

            var btnSearch = new SimpleButton
            {
                Text = "ARA",
                Location = new Point(380, 13),
                Size = new Size(80, 34),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryColor, ForeColor = Color.White }
            };
            btnSearch.Click += (s, e) => SearchMeals();
            toolbar.Controls.Add(btnSearch);

            var btnRefresh = new SimpleButton
            {
                Text = "YENİLE",
                Location = new Point(470, 13),
                Size = new Size(80, 34),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = TextSecondary, ForeColor = Color.White }
            };
            btnRefresh.Click += (s, e) => LoadMeals();
            toolbar.Controls.Add(btnRefresh);

            lblTotalCount = new LabelControl
            {
                Text = "Toplam: 0 yemek",
                Location = new Point(570, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextSecondary
            };
            toolbar.Controls.Add(lblTotalCount);

            // Grid
            gridMeals = new GridControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F)
            };

            viewMeals = new GridView(gridMeals)
            {
                OptionsView = {
                    ShowGroupPanel = false,
                    ShowIndicator = false,
                    ColumnAutoWidth = true,
                    RowAutoHeight = false,
                    EnableAppearanceEvenRow = true
                },
                OptionsBehavior = { Editable = false },
                RowHeight = 36
            };
            viewMeals.Appearance.Row.Font = new Font("Segoe UI", 10F);
            viewMeals.Appearance.EvenRow.BackColor = Color.FromArgb(240, 253, 250); // Light Teal tint
            viewMeals.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            viewMeals.Appearance.HeaderPanel.ForeColor = PrimaryColor;
            viewMeals.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            viewMeals.Appearance.Row.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            // Kolonları düzenle - Makroları ayır
            var colName = viewMeals.Columns.AddVisible("Name", "YEMEK ADI");
            colName.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            colName.Width = 180;

            viewMeals.Columns.AddVisible("MealTimeName", "ÖĞÜN").Width = 90;
            viewMeals.Columns.AddVisible("Category", "KATEGORİ").Width = 90;
            viewMeals.Columns.AddVisible("Calories", "KALORİ").Width = 70;
            viewMeals.Columns.AddVisible("PortionGrams", "GRAM").Width = 60;
            
            // Makrolar ayrı kolonlarda
            viewMeals.Columns.AddVisible("Protein", "PRO (g)").Width = 60;
            viewMeals.Columns.AddVisible("Carbs", "KARB (g)").Width = 60;
            viewMeals.Columns.AddVisible("Fat", "YAĞ (g)").Width = 60;

            gridMeals.MainView = viewMeals;
            viewMeals.FocusedRowChanged += ViewMeals_FocusedRowChanged;
            viewMeals.DoubleClick += ViewMeals_DoubleClick;

            grpList.Controls.Add(gridMeals);
            grpList.Controls.Add(toolbar);

            parent.Controls.Add(grpList);
        }

        private void CreateDetailPanel(PanelControl parent)
        {
            var grpForm = new GroupControl
            {
                Text = "YEMEĞİ DÜZENLE",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AccentColor,
                Appearance = { BackColor = CardColor }
            };

            var scrollPanel = new XtraScrollableControl
            {
                Dock = DockStyle.Fill,
                BackColor = CardColor
            };

            var formContainer = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(380, 650),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardColor
            };

            int x = 20;
            int y = 15;
            int spacing = 65;

            lblFormTitle = new LabelControl
            {
                Text = "YENİ YEMEK EKLE",
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = PrimaryColor
            };
            formContainer.Controls.Add(lblFormTitle);
            y += 40;

            // Yemek Adı
            AddLabel(formContainer, "YEMEK ADI *", x, y);
            txtName = AddTextEdit(formContainer, "", x, y + 25, 320);
            y += spacing;

            // Öğün Zamanı
            AddLabel(formContainer, "ÖĞÜN ZAMANI *", x, y);
            cmbMealTime = new ComboBoxEdit
            {
                Location = new Point(x, y + 25),
                Size = new Size(320, 32)
            };
            cmbMealTime.Properties.Items.AddRange(new[] { "Kahvaltı", "Ara Öğün 1", "Öğle Yemeği", "Ara Öğün 2", "Akşam Yemeği", "Gece" });
            cmbMealTime.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            cmbMealTime.SelectedIndex = 0;
            formContainer.Controls.Add(cmbMealTime);
            y += spacing;

            // Kalori - Protein - Karb - Yag
            AddLabel(formContainer, "Kalori", x, y);
            AddLabel(formContainer, "Protein (g)", x + 85, y);
            AddLabel(formContainer, "Karb (g)", x + 170, y);
            AddLabel(formContainer, "Yağ (g)", x + 255, y);

            spnCalories = AddSpinEdit(formContainer, x, y + 25, 75, 0, 5000, 0);
            spnProtein = AddSpinEdit(formContainer, x + 85, y + 25, 75, 0, 500, 0);
            spnCarbs = AddSpinEdit(formContainer, x + 170, y + 25, 75, 0, 500, 0);
            spnFat = AddSpinEdit(formContainer, x + 255, y + 25, 75, 0, 500, 0);
            y += spacing;

            // Porsiyon
            AddLabel(formContainer, "Gram", x, y);
            AddLabel(formContainer, "Porsiyon", x + 85, y);
            spnPortionGrams = AddSpinEdit(formContainer, x, y + 25, 75, 1, 2000, 100);
            txtPortionDesc = AddTextEdit(formContainer, "1 porsiyon", x + 85, y + 25, 235);
            y += spacing;

            // Kategori
            AddLabel(formContainer, "KATEGORİ", x, y);
            txtCategory = AddTextEdit(formContainer, "Et, Sebze, Çorba...", x, y + 25, 320);
            y += spacing;

            // Açıklama
            AddLabel(formContainer, "AÇIKLAMA / TARİF", x, y);
            txtDescription = new MemoEdit
            {
                Location = new Point(x, y + 25),
                Size = new Size(320, 70),
                Properties = { NullText = "Yemek tarifi...", Appearance = { Font = new Font("Segoe UI", 10F) } }
            };
            formContainer.Controls.Add(txtDescription);
            y += 100;

            // Notlar
            AddLabel(formContainer, "NOTLAR", x, y);
            txtNotes = new MemoEdit
            {
                Location = new Point(x, y + 25),
                Size = new Size(320, 60),
                Properties = { NullText = "Alerjen bilgisi...", Appearance = { Font = new Font("Segoe UI", 10F) } }
            };
            formContainer.Controls.Add(txtNotes);
            y += 90;

            // Butonlar
            var btnSave = new SimpleButton
            {
                Text = "KAYDET",
                Location = new Point(x, y),
                Size = new Size(140, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Appearance = { BackColor = SuccessGreen, ForeColor = Color.White }
            };
            btnSave.Click += BtnSave_Click;
            formContainer.Controls.Add(btnSave);

            var btnClear = new SimpleButton
            {
                Text = "TEMİZLE",
                Location = new Point(x + 150, y),
                Size = new Size(100, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Appearance = { BackColor = TextSecondary, ForeColor = Color.White }
            };
            btnClear.Click += (s, e) => ClearForm();
            formContainer.Controls.Add(btnClear);

            var btnDelete = new SimpleButton
            {
                Text = "SİL",
                Location = new Point(x + 260, y),
                Size = new Size(60, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Appearance = { BackColor = DangerRed, ForeColor = Color.White }
            };
            btnDelete.Click += BtnDelete_Click;
            formContainer.Controls.Add(btnDelete);

            scrollPanel.Controls.Add(formContainer);
            grpForm.Controls.Add(scrollPanel);
            parent.Controls.Add(grpForm);
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            var lbl = new LabelControl
            {
                Text = text,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = PrimaryColor
            };
            parent.Controls.Add(lbl);
        }

        private TextEdit AddTextEdit(Control parent, string placeholder, int x, int y, int width)
        {
            var txt = new TextEdit
            {
                Location = new Point(x, y),
                Size = new Size(width, 32),
                Properties = { NullText = placeholder, Appearance = { Font = new Font("Segoe UI", 10F) } }
            };
            parent.Controls.Add(txt);
            return txt;
        }

        private SpinEdit AddSpinEdit(Control parent, int x, int y, int width, decimal min, decimal max, decimal defaultVal)
        {
            var spn = new SpinEdit
            {
                Location = new Point(x, y),
                Size = new Size(width, 32),
                Properties = { MinValue = min, MaxValue = max, Appearance = { Font = new Font("Segoe UI", 10F) } }
            };
            spn.EditValue = defaultVal;
            parent.Controls.Add(spn);
            return spn;
        }

        private void LoadMeals()
        {
            try
            {
                var meals = _mealService.GetDoctorMeals(AuthContext.UserId);
                _meals = new BindingList<Meal>(meals);
                gridMeals.DataSource = _meals;
                lblTotalCount.Text = $"Toplam: {meals.Count} yemek";
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Yemekler yüklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchMeals()
        {
            try
            {
                var meals = _mealService.SearchMeals(AuthContext.UserId, txtSearch.Text.Trim());
                FilterMealsByTime(meals);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Arama hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterMeals()
        {
            var meals = _mealService.GetDoctorMeals(AuthContext.UserId);
            FilterMealsByTime(meals);
        }

        private void FilterMealsByTime(System.Collections.Generic.List<Meal> meals)
        {
            if (cmbMealTimeFilter.SelectedIndex > 0)
            {
                var mealTime = (MealTimeType)(cmbMealTimeFilter.SelectedIndex - 1);
                meals = meals.Where(m => m.MealTime == mealTime).ToList();
            }
            _meals = new BindingList<Meal>(meals);
            gridMeals.DataSource = _meals;
            lblTotalCount.Text = $"Toplam: {meals.Count} yemek";
        }

        private void ViewMeals_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            var selected = viewMeals.GetFocusedRow() as Meal;
            if (selected != null)
            {
                LoadMealToForm(selected);
            }
        }

        private void ViewMeals_DoubleClick(object sender, EventArgs e)
        {
            var selected = viewMeals.GetFocusedRow() as Meal;
            if (selected != null)
            {
                LoadMealToForm(selected);
            }
        }

        private void LoadMealToForm(Meal meal)
        {
            _editingMealId = meal.Id;
            lblFormTitle.Text = "YEMEĞİ DÜZENLE";
            lblFormTitle.ForeColor = WarningOrange;
            txtName.Text = meal.Name;
            cmbMealTime.SelectedIndex = (int)meal.MealTime;
            spnCalories.Value = (decimal)meal.Calories;
            spnProtein.Value = (decimal)meal.Protein;
            spnCarbs.Value = (decimal)meal.Carbs;
            spnFat.Value = (decimal)meal.Fat;
            spnPortionGrams.Value = (decimal)meal.PortionGrams;
            txtPortionDesc.Text = meal.PortionDescription;
            txtCategory.Text = meal.Category;
            txtDescription.Text = meal.Description;
            txtNotes.Text = meal.Notes;
        }

        private void ClearForm()
        {
            _editingMealId = 0;
            lblFormTitle.Text = "YENİ YEMEK EKLE";
            lblFormTitle.ForeColor = PrimaryColor;
            txtName.Text = "";
            cmbMealTime.SelectedIndex = 0;
            spnCalories.Value = 0;
            spnProtein.Value = 0;
            spnCarbs.Value = 0;
            spnFat.Value = 0;
            spnPortionGrams.Value = 100;
            txtPortionDesc.Text = "";
            txtCategory.Text = "";
            txtDescription.Text = "";
            txtNotes.Text = "";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                XtraMessageBox.Show("Yemek adı boş olamaz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            try
            {
                if (_editingMealId > 0)
                {
                    var meal = _mealService.GetMealById(_editingMealId);
                    if (meal != null)
                    {
                        meal.Name = txtName.Text.Trim();
                        meal.MealTime = (MealTimeType)cmbMealTime.SelectedIndex;
                        meal.Calories = (double)spnCalories.Value;
                        meal.Protein = (double)spnProtein.Value;
                        meal.Carbs = (double)spnCarbs.Value;
                        meal.Fat = (double)spnFat.Value;
                        meal.PortionGrams = (double)spnPortionGrams.Value;
                        meal.PortionDescription = txtPortionDesc.Text.Trim();
                        meal.Category = txtCategory.Text.Trim();
                        meal.Description = txtDescription.Text.Trim();
                        meal.Notes = txtNotes.Text.Trim();

                        _mealService.UpdateMeal(meal);
                        XtraMessageBox.Show("Yemek güncellendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    _mealService.CreateMeal(
                        AuthContext.UserId,
                        txtName.Text.Trim(),
                        (MealTimeType)cmbMealTime.SelectedIndex,
                        (double)spnCalories.Value,
                        (double)spnProtein.Value,
                        (double)spnCarbs.Value,
                        (double)spnFat.Value,
                        (double)spnPortionGrams.Value,
                        txtPortionDesc.Text.Trim(),
                        txtCategory.Text.Trim(),
                        txtDescription.Text.Trim(),
                        txtNotes.Text.Trim()
                    );
                    XtraMessageBox.Show("Yemek eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ClearForm();
                LoadMeals();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Kayıt hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var selected = viewMeals.GetFocusedRow() as Meal;
            if (selected == null)
            {
                XtraMessageBox.Show("Lütfen silinecek yemeği seçin", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = XtraMessageBox.Show($"'{selected.Name}' yemeğini silmek istediğinizden emin misiniz?", "Onay",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _mealService.DeleteMeal(selected.Id);
                    ClearForm();
                    LoadMeals();
                    XtraMessageBox.Show("Yemek silindi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Silme hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Name = "FrmMeals";
            this.ResumeLayout(false);
        }
    }
}
