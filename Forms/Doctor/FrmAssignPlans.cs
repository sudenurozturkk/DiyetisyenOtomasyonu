using System;
using System.Collections.Generic;
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
using PatientEntity = DiyetisyenOtomasyonu.Domain.Patient;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    /// <summary>
    /// Haftalik Diyet Plani Atama Formu
    /// </summary>
    public partial class FrmAssignPlans : XtraForm
    {
        private readonly MealService _mealService;
        private readonly PatientService _patientService;

        private LookUpEdit cmbPatient;
        private DateEdit dateWeekStart;
        private LabelControl lblPatientInfo;
        private LabelControl lblWeekSummary;
        private XtraScrollableControl scrollContainer;

        private int _selectedPatientId;
        private DateTime _currentWeekStart;
        private List<PatientMealAssignment> _weeklyAssignments;

        // Haftalik tablo butonlari [gun, ogun]
        private SimpleButton[,] _mealButtons = new SimpleButton[7, 6];

        // Modern Renkler
        private readonly Color PrimaryColor = Color.FromArgb(79, 70, 229);
        private readonly Color SuccessColor = Color.FromArgb(34, 197, 94);
        private readonly Color DangerColor = Color.FromArgb(239, 68, 68);
        private readonly Color WarningColor = Color.FromArgb(245, 158, 11);
        private readonly Color InfoColor = Color.FromArgb(14, 165, 233);
        private readonly Color CardColor = Color.White;
        private readonly Color BackgroundColor = Color.FromArgb(248, 250, 252);
        private readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private readonly Color TextSecondary = Color.FromArgb(100, 116, 139);

        private readonly string[] DayNames = { "Pazartesi", "Sali", "Carsamba", "Persembe", "Cuma", "Cumartesi", "Pazar" };
        private readonly string[] MealTimeNames = { "Kahvalti", "Ara Ogun 1", "Ogle", "Ara Ogun 2", "Aksam", "Gece" };
        private readonly Color[] MealTimeColors = { 
            Color.FromArgb(254, 243, 199), // Kahvalti - sari
            Color.FromArgb(220, 252, 231), // Ara 1 - yesil
            Color.FromArgb(219, 234, 254), // Ogle - mavi
            Color.FromArgb(254, 226, 226), // Ara 2 - kirmizi
            Color.FromArgb(243, 232, 255), // Aksam - mor
            Color.FromArgb(224, 242, 254)  // Gece - acik mavi
        };

        public FrmAssignPlans()
        {
            InitializeComponent();
            _mealService = new MealService();
            _patientService = new PatientService();
            _currentWeekStart = GetMonday(DateTime.Now);
            InitializeUI();
            LoadPatients();
        }

        private void InitializeUI()
        {
            this.Text = "Haftalik Diyet Plani";
            this.BackColor = BackgroundColor;

            // ANA CONTAINER
            var mainPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = BackgroundColor,
                Padding = new Padding(15)
            };

            // ============ UST PANEL ============
            var pnlTop = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 70,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardColor
            };

            var lblTitle = new LabelControl
            {
                Text = "HAFTALIK DIYET PLANI",
                Location = new Point(15, 10),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = PrimaryColor
            };
            pnlTop.Controls.Add(lblTitle);

            var lblPatient = new LabelControl
            {
                Text = "Hasta:",
                Location = new Point(15, 42),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextSecondary
            };
            pnlTop.Controls.Add(lblPatient);

            cmbPatient = new LookUpEdit
            {
                Location = new Point(65, 38),
                Size = new Size(200, 28)
            };
            cmbPatient.Properties.DisplayMember = "AdSoyad";
            cmbPatient.Properties.ValueMember = "Id";
            cmbPatient.Properties.NullText = "-- Hasta seçiniz --";
            cmbPatient.Properties.Appearance.Font = new Font("Segoe UI", 9F);
            cmbPatient.Properties.PopupFormSize = new Size(280, 180);
            cmbPatient.Properties.Columns.Clear();
            cmbPatient.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("AdSoyad", "Hasta", 160));
            cmbPatient.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("BMIKategori", "Durum", 90));
            cmbPatient.EditValueChanged += CmbPatient_EditValueChanged;
            pnlTop.Controls.Add(cmbPatient);

            var lblWeek = new LabelControl
            {
                Text = "Hafta:",
                Location = new Point(280, 42),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextSecondary
            };
            pnlTop.Controls.Add(lblWeek);

            dateWeekStart = new DateEdit
            {
                Location = new Point(325, 38),
                Size = new Size(110, 28)
            };
            dateWeekStart.DateTime = _currentWeekStart;
            dateWeekStart.Properties.Appearance.Font = new Font("Segoe UI", 9F);
            dateWeekStart.EditValueChanged += DateWeekStart_EditValueChanged;
            pnlTop.Controls.Add(dateWeekStart);

            var btnLoad = new SimpleButton
            {
                Text = "Yukle",
                Location = new Point(445, 36),
                Size = new Size(70, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryColor, ForeColor = Color.White }
            };
            btnLoad.Click += BtnLoad_Click;
            pnlTop.Controls.Add(btnLoad);

            var btnPrevWeek = new SimpleButton
            {
                Text = "<",
                Location = new Point(525, 36),
                Size = new Size(35, 32),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Appearance = { BackColor = Color.FromArgb(107, 114, 128), ForeColor = Color.White }
            };
            btnPrevWeek.Click += (s, e) => ChangeWeek(-7);
            pnlTop.Controls.Add(btnPrevWeek);

            var btnNextWeek = new SimpleButton
            {
                Text = ">",
                Location = new Point(565, 36),
                Size = new Size(35, 32),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Appearance = { BackColor = Color.FromArgb(107, 114, 128), ForeColor = Color.White }
            };
            btnNextWeek.Click += (s, e) => ChangeWeek(7);
            pnlTop.Controls.Add(btnNextWeek);

            lblPatientInfo = new LabelControl
            {
                Text = "",
                Location = new Point(615, 42),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = InfoColor,
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(350, 22)
            };
            pnlTop.Controls.Add(lblPatientInfo);

            // ============ OZET PANEL ============
            var pnlSummary = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 35,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.FromArgb(238, 242, 255)
            };

            lblWeekSummary = new LabelControl
            {
                Text = "   Hasta seçiniz ve haftayı yükleyiniz...",
                Location = new Point(10, 8),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = PrimaryColor,
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(800, 22)
            };
            pnlSummary.Controls.Add(lblWeekSummary);

            // ============ TABLO ALANI ============
            scrollContainer = new XtraScrollableControl
            {
                Dock = DockStyle.Fill,
                BackColor = BackgroundColor
            };

            CreateWeeklyTable();

            mainPanel.Controls.Add(scrollContainer);
            mainPanel.Controls.Add(pnlSummary);
            mainPanel.Controls.Add(pnlTop);

            this.Controls.Add(mainPanel);
        }

        private void CreateWeeklyTable()
        {
            scrollContainer.Controls.Clear();

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 8,
                RowCount = 7,
                BackColor = BackgroundColor,
                Padding = new Padding(0, 0, 0, 20),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            // Column Styles
            // Column 0: Meal Headers (Fixed width)
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            
            // Columns 1-7: Days (Equal percentage)
            float dayPercent = 100F / 7F;
            for (int i = 0; i < 7; i++)
            {
                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, dayPercent));
            }

            // Row Styles
            // Row 0: Header (Fixed height)
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            
            // Rows 1-6: Meals (Fixed height)
            for (int i = 0; i < 6; i++)
            {
                tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            }

            // 1. Empty Top-Left Cell
            tableLayout.Controls.Add(new PanelControl { BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder, BackColor = BackgroundColor }, 0, 0);

            // 2. Day Headers (Row 0)
            for (int day = 0; day < 7; day++)
            {
                var lblDay = new LabelControl
                {
                    Text = DayNames[day],
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Appearance = { 
                        BackColor = PrimaryColor, 
                        TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center, VAlignment = DevExpress.Utils.VertAlignment.Center } 
                    },
                    AutoSizeMode = LabelAutoSizeMode.None
                };
                tableLayout.Controls.Add(lblDay, day + 1, 0);
            }

            // 3. Meal Rows
            for (int mealTime = 0; mealTime < 6; mealTime++)
            {
                // Meal Header (Column 0)
                var lblMealTime = new LabelControl
                {
                    Text = MealTimeNames[mealTime],
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = TextPrimary,
                    Appearance = { 
                        TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Near, VAlignment = DevExpress.Utils.VertAlignment.Center } 
                    },
                    AutoSizeMode = LabelAutoSizeMode.None,
                    Padding = new Padding(10, 0, 0, 0)
                };
                tableLayout.Controls.Add(lblMealTime, 0, mealTime + 1);

                // Meal Cells (Columns 1-7)
                for (int day = 0; day < 7; day++)
                {
                    var btn = new SimpleButton
                    {
                        Dock = DockStyle.Fill,
                        Text = "+",
                        Font = new Font("Segoe UI", 18F),
                        Tag = new int[] { day, mealTime },
                        Appearance = { 
                            BackColor = MealTimeColors[mealTime],
                            ForeColor = TextSecondary,
                            BorderColor = Color.FromArgb(200, 200, 200)
                        },
                        AllowFocus = false,
                        Margin = new Padding(2)
                    };
                    btn.Click += MealCell_Click;
                    tableLayout.Controls.Add(btn, day + 1, mealTime + 1);
                    _mealButtons[day, mealTime] = btn;
                }
            }

            scrollContainer.Controls.Add(tableLayout);
        }

        private void LoadPatients()
        {
            try
            {
                var patients = _patientService.GetPatientsByDoctor(AuthContext.UserId);
                cmbPatient.Properties.DataSource = patients;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hastalar yuklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbPatient_EditValueChanged(object sender, EventArgs e)
        {
            if (cmbPatient.EditValue != null)
            {
                _selectedPatientId = (int)cmbPatient.EditValue;
                var patient = _patientService.GetPatientById(_selectedPatientId);
                if (patient != null)
                {
                    lblPatientInfo.Text = $"BMI: {patient.BMI:F1} ({patient.BMIKategori}) | Kilo: {patient.GuncelKilo} kg";
                }
            }
        }

        private void DateWeekStart_EditValueChanged(object sender, EventArgs e)
        {
            _currentWeekStart = GetMonday(dateWeekStart.DateTime);
            dateWeekStart.DateTime = _currentWeekStart;
        }

        private void ChangeWeek(int days)
        {
            _currentWeekStart = _currentWeekStart.AddDays(days);
            dateWeekStart.DateTime = _currentWeekStart;
            if (_selectedPatientId > 0)
                LoadWeeklyPlan();
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            if (_selectedPatientId == 0)
            {
                XtraMessageBox.Show("Lütfen hasta seçiniz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            LoadWeeklyPlan();
        }

        private void LoadWeeklyPlan()
        {
            try
            {
                _weeklyAssignments = _mealService.GetWeeklyPlan(_selectedPatientId, _currentWeekStart);
                
                // Butonlari sifirla
                for (int day = 0; day < 7; day++)
                {
                    for (int mealTime = 0; mealTime < 6; mealTime++)
                    {
                        _mealButtons[day, mealTime].Text = "+";
                        _mealButtons[day, mealTime].Font = new Font("Segoe UI", 18F);
                        _mealButtons[day, mealTime].Appearance.BackColor = MealTimeColors[mealTime];
                        _mealButtons[day, mealTime].Appearance.ForeColor = TextSecondary;
                    }
                }

                // Atanmis ogunleri goster
                double totalCalories = 0;
                int assignedCount = 0;

                foreach (var assignment in _weeklyAssignments)
                {
                    int dayIndex = assignment.DayOfWeek;
                    int mealTimeIndex = (int)assignment.MealTime;
                    
                    if (dayIndex >= 0 && dayIndex < 7 && mealTimeIndex >= 0 && mealTimeIndex < 6)
                    {
                        var btn = _mealButtons[dayIndex, mealTimeIndex];
                        btn.Text = TruncateText(assignment.MealName, 12) + "\n" + assignment.Calories.ToString("0") + " kcal";
                        btn.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
                        btn.Appearance.BackColor = Color.FromArgb(187, 247, 208);
                        btn.Appearance.ForeColor = TextPrimary;
                        
                        totalCalories += assignment.Calories;
                        assignedCount++;
                    }
                }

                lblWeekSummary.Text = $"   Hafta: {_currentWeekStart:dd.MM} - {_currentWeekStart.AddDays(6):dd.MM.yyyy} | " +
                                     $"Atanan Ogun: {assignedCount} | Toplam: {totalCalories:N0} kcal";
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Plan yuklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 2) + "..";
        }

        private void MealCell_Click(object sender, EventArgs e)
        {
            if (_selectedPatientId == 0)
            {
                XtraMessageBox.Show("Lütfen önce hasta seçiniz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var btn = sender as SimpleButton;
            var indices = btn.Tag as int[];
            int dayIndex = indices[0];
            int mealTimeIndex = indices[1];

            MealTimeType mealTime = (MealTimeType)mealTimeIndex;

            // Mevcut atama var mi kontrol et
            var existingAssignment = _weeklyAssignments?.FirstOrDefault(a => 
                a.DayOfWeek == dayIndex && (int)a.MealTime == mealTimeIndex);

            using (var dialog = new FrmMealAssignDialog(_selectedPatientId, _currentWeekStart, dayIndex, mealTime, _mealService, existingAssignment))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadWeeklyPlan();
                }
            }
        }

        private DateTime GetMonday(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Name = "FrmAssignPlans";
            this.ResumeLayout(false);
        }
    }

    /// <summary>
    /// Ogun Atama/Duzenleme/Silme Dialog
    /// </summary>
    public class FrmMealAssignDialog : XtraForm
    {
        private readonly int _patientId;
        private readonly DateTime _weekStart;
        private readonly int _dayOfWeek;
        private readonly MealTimeType _mealTime;
        private readonly MealService _mealService;
        private readonly PatientMealAssignment _existingAssignment;

        private GridControl gridMeals;
        private GridView viewMeals;
        private TextEdit txtMealName;
        private SpinEdit spnCalories;
        private SpinEdit spnProtein;
        private SpinEdit spnCarbs;
        private SpinEdit spnFat;
        private TextEdit txtPortion;
        private GroupControl grpExisting;

        private readonly Color PrimaryColor = Color.FromArgb(79, 70, 229);
        private readonly Color SuccessColor = Color.FromArgb(34, 197, 94);
        private readonly Color DangerColor = Color.FromArgb(239, 68, 68);
        private readonly Color WarningColor = Color.FromArgb(245, 158, 11);

        private readonly string[] DayNames = { "Pazartesi", "Sali", "Carsamba", "Persembe", "Cuma", "Cumartesi", "Pazar" };

        public FrmMealAssignDialog(int patientId, DateTime weekStart, int dayOfWeek, MealTimeType mealTime, MealService mealService, PatientMealAssignment existingAssignment)
        {
            _patientId = patientId;
            _weekStart = weekStart;
            _dayOfWeek = dayOfWeek;
            _mealTime = mealTime;
            _mealService = mealService;
            _existingAssignment = existingAssignment;
            InitializeComponent();
            InitializeUI();
            LoadMeals();
        }

        private void InitializeUI()
        {
            bool hasExisting = _existingAssignment != null;
            this.Text = hasExisting ? $"Ogun Duzenle - {DayNames[_dayOfWeek]} {GetMealTimeName(_mealTime)}" 
                                    : $"Ogun Ata - {DayNames[_dayOfWeek]} {GetMealTimeName(_mealTime)}";
            this.Size = new Size(720, hasExisting ? 650 : 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);

            int currentY = 15;

            // ============ MEVCUT OGUN BILGISI (varsa) ============
            if (hasExisting)
            {
                grpExisting = new GroupControl
                {
                    Text = "MEVCUT OGUN",
                    Location = new Point(15, currentY),
                    Size = new Size(675, 100),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = WarningColor
                };

                var lblExistingName = new LabelControl
                {
                    Text = $"Yemek: {_existingAssignment.MealName}",
                    Location = new Point(15, 30),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42)
                };
                grpExisting.Controls.Add(lblExistingName);

                var lblExistingInfo = new LabelControl
                {
                    Text = $"Kalori: {_existingAssignment.Calories:F0} | Protein: {_existingAssignment.Protein:F0}g | Karb: {_existingAssignment.Carbs:F0}g | Yag: {_existingAssignment.Fat:F0}g",
                    Location = new Point(15, 55),
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(100, 116, 139)
                };
                grpExisting.Controls.Add(lblExistingInfo);

                // Sil butonu
                var btnDelete = new SimpleButton
                {
                    Text = "Bu Ogunu Sil",
                    Location = new Point(520, 35),
                    Size = new Size(130, 40),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Appearance = { BackColor = DangerColor, ForeColor = Color.White }
                };
                btnDelete.Click += BtnDelete_Click;
                grpExisting.Controls.Add(btnDelete);

                this.Controls.Add(grpExisting);
                currentY += 110;
            }

            // ============ BASLIK ============
            var lblTitle = new LabelControl
            {
                Text = hasExisting ? "Yeni Ogun Ile Degistir" : "Yemek Kutuphanesinden Sec veya Yeni Ekle",
                Location = new Point(15, currentY),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = PrimaryColor
            };
            this.Controls.Add(lblTitle);
            currentY += 30;

            // ============ KUTUPHANEDEN SECIM ============
            var grpLibrary = new GroupControl
            {
                Text = "YEMEK KUTUPHANESI",
                Location = new Point(15, currentY),
                Size = new Size(675, 200),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = SuccessColor
            };

            gridMeals = new GridControl
            {
                Dock = DockStyle.Fill
            };
            viewMeals = new GridView(gridMeals)
            {
                OptionsView = { ShowGroupPanel = false, ShowIndicator = false, ColumnAutoWidth = true },
                OptionsBehavior = { Editable = false },
                RowHeight = 26
            };
            viewMeals.Columns.Add(new GridColumn { FieldName = "Name", Caption = "Yemek Adi", Width = 180, Visible = true });
            viewMeals.Columns.Add(new GridColumn { FieldName = "Calories", Caption = "Kalori", Width = 65, Visible = true });
            viewMeals.Columns.Add(new GridColumn { FieldName = "Protein", Caption = "Protein", Width = 55, Visible = true });
            viewMeals.Columns.Add(new GridColumn { FieldName = "Carbs", Caption = "Karb", Width = 55, Visible = true });
            viewMeals.Columns.Add(new GridColumn { FieldName = "Fat", Caption = "Yag", Width = 55, Visible = true });
            viewMeals.Columns.Add(new GridColumn { FieldName = "PortionDescription", Caption = "Porsiyon", Width = 80, Visible = true });
            gridMeals.MainView = viewMeals;
            grpLibrary.Controls.Add(gridMeals);

            this.Controls.Add(grpLibrary);
            currentY += 210;

            var btnSelectFromLibrary = new SimpleButton
            {
                Text = hasExisting ? "Secilen Ile Degistir" : "Secileni Ata",
                Location = new Point(540, currentY),
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Appearance = { BackColor = SuccessColor, ForeColor = Color.White }
            };
            btnSelectFromLibrary.Click += BtnSelectFromLibrary_Click;
            this.Controls.Add(btnSelectFromLibrary);
            currentY += 45;

            // ============ YENI OGUN EKLEME ============
            var grpNew = new GroupControl
            {
                Text = "VEYA YENI OGUN EKLE",
                Location = new Point(15, currentY),
                Size = new Size(675, 130),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = PrimaryColor
            };

            int yPos = 28;
            var font = new Font("Segoe UI", 9F);

            var lblName = new LabelControl { Text = "Yemek Adi:", Location = new Point(15, yPos), Font = font };
            txtMealName = new TextEdit { Location = new Point(90, yPos - 2), Size = new Size(170, 26) };
            txtMealName.Properties.Appearance.Font = font;

            var lblCal = new LabelControl { Text = "Kalori:", Location = new Point(275, yPos), Font = font };
            spnCalories = new SpinEdit { Location = new Point(320, yPos - 2), Size = new Size(65, 26) };
            spnCalories.Properties.MinValue = 0;
            spnCalories.Properties.MaxValue = 5000;

            var lblPortion = new LabelControl { Text = "Porsiyon:", Location = new Point(400, yPos), Font = font };
            txtPortion = new TextEdit { Location = new Point(460, yPos - 2), Size = new Size(190, 26), Text = "1 porsiyon" };
            txtPortion.Properties.Appearance.Font = font;

            grpNew.Controls.Add(lblName);
            grpNew.Controls.Add(txtMealName);
            grpNew.Controls.Add(lblCal);
            grpNew.Controls.Add(spnCalories);
            grpNew.Controls.Add(lblPortion);
            grpNew.Controls.Add(txtPortion);

            yPos += 32;

            var lblProtein = new LabelControl { Text = "Protein:", Location = new Point(15, yPos), Font = font };
            spnProtein = new SpinEdit { Location = new Point(65, yPos - 2), Size = new Size(60, 26) };
            spnProtein.Properties.MinValue = 0;
            spnProtein.Properties.MaxValue = 500;

            var lblCarbs = new LabelControl { Text = "Karb:", Location = new Point(140, yPos), Font = font };
            spnCarbs = new SpinEdit { Location = new Point(180, yPos - 2), Size = new Size(60, 26) };
            spnCarbs.Properties.MinValue = 0;
            spnCarbs.Properties.MaxValue = 500;

            var lblFat = new LabelControl { Text = "Yag:", Location = new Point(255, yPos), Font = font };
            spnFat = new SpinEdit { Location = new Point(285, yPos - 2), Size = new Size(60, 26) };
            spnFat.Properties.MinValue = 0;
            spnFat.Properties.MaxValue = 500;

            grpNew.Controls.Add(lblProtein);
            grpNew.Controls.Add(spnProtein);
            grpNew.Controls.Add(lblCarbs);
            grpNew.Controls.Add(spnCarbs);
            grpNew.Controls.Add(lblFat);
            grpNew.Controls.Add(spnFat);

            yPos += 32;

            var btnAddNew = new SimpleButton
            {
                Text = hasExisting ? "Yeni Ile Degistir" : "Yeni Olarak Ata",
                Location = new Point(460, yPos),
                Size = new Size(190, 32),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryColor, ForeColor = Color.White }
            };
            btnAddNew.Click += BtnAddNew_Click;
            grpNew.Controls.Add(btnAddNew);

            this.Controls.Add(grpNew);
            currentY += 140;

            // ============ KAPAT BUTONU ============
            var btnCancel = new SimpleButton
            {
                Text = "Kapat",
                Location = new Point(15, currentY + 10),
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10F),
                Appearance = { BackColor = Color.FromArgb(107, 114, 128), ForeColor = Color.White }
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void LoadMeals()
        {
            try
            {
                var meals = _mealService.GetDoctorMeals(AuthContext.UserId);
                gridMeals.DataSource = new BindingList<Meal>(meals);
            }
            catch { }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var result = XtraMessageBox.Show("Bu ogunu silmek istediginizden emin misiniz?", "Onay",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _mealService.RemoveAssignment(_existingAssignment.Id);
                    XtraMessageBox.Show("Ogun silindi!", "Basarili", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnSelectFromLibrary_Click(object sender, EventArgs e)
        {
            var selected = viewMeals.GetFocusedRow() as Meal;
            if (selected == null)
            {
                XtraMessageBox.Show("Lütfen bir yemek seçiniz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Mevcut varsa sil
                if (_existingAssignment != null)
                {
                    _mealService.RemoveAssignment(_existingAssignment.Id);
                }

                _mealService.AssignMealFromLibrary(_patientId, AuthContext.UserId, selected.Id,
                    _weekStart, _dayOfWeek, _mealTime, null);

                XtraMessageBox.Show(_existingAssignment != null ? "Ogun degistirildi!" : "Ogun atandi!", 
                    "Basarili", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddNew_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMealName.Text))
            {
                XtraMessageBox.Show("Yemek adi bos olamaz", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((double)spnCalories.Value <= 0)
            {
                XtraMessageBox.Show("Kalori 0dan buyuk olmali", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Mevcut varsa sil
                if (_existingAssignment != null)
                {
                    _mealService.RemoveAssignment(_existingAssignment.Id);
                }

                _mealService.AssignNewMeal(_patientId, AuthContext.UserId, _weekStart, _dayOfWeek, _mealTime,
                    txtMealName.Text.Trim(), null,
                    (double)spnCalories.Value, (double)spnProtein.Value, (double)spnCarbs.Value,
                    (double)spnFat.Value, 100, txtPortion.Text.Trim(), null, null, true);

                XtraMessageBox.Show(_existingAssignment != null ? "Ogun degistirildi ve kutupaneye eklendi!" : "Ogun atandi ve kutupaneye eklendi!", 
                    "Basarili", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetMealTimeName(MealTimeType type)
        {
            switch (type)
            {
                case MealTimeType.Breakfast: return "Kahvalti";
                case MealTimeType.MorningSnack: return "Ara Ogun 1";
                case MealTimeType.Lunch: return "Ogle";
                case MealTimeType.AfternoonSnack: return "Ara Ogun 2";
                case MealTimeType.Dinner: return "Aksam";
                case MealTimeType.EveningSnack: return "Gece";
                default: return "";
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(720, 650);
            this.Name = "FrmMealAssignDialog";
            this.ResumeLayout(false);
        }
    }
}
