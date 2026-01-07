using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Forms.Patient
{
    /// <summary>
    /// Vücut Ölçümleri Formu - Modern Tasarım
    /// </summary>
    public partial class FrmBodyMeasurements : XtraForm
    {
        private readonly BodyMeasurementRepository _repository;
        private List<BodyMeasurement> _measurements;
        
        // UI Components
        private GridControl gridMeasurements;
        private GridView gridView;
        private ChartControl chart;
        
        // Input fields
        private SpinEdit txtChest, txtWaist, txtHip, txtArm, txtThigh, txtNeck;
        private MemoEdit txtNotes;
        private DateEdit dateEdit;
        
        // Modern Renkler - Yeşil Tema
        private readonly Color PrimaryGreen = Color.FromArgb(13, 148, 136);
        private readonly Color SuccessGreen = Color.FromArgb(34, 197, 94);
        private readonly Color InfoBlue = Color.FromArgb(59, 130, 246);
        private readonly Color CardWhite = Color.White;
        private readonly Color BackgroundLight = Color.FromArgb(248, 250, 252);
        private readonly Color TextDark = Color.FromArgb(30, 41, 59);
        private readonly Color TextMedium = Color.FromArgb(100, 116, 139);
        private readonly Color BorderGray = Color.FromArgb(226, 232, 240);

        public FrmBodyMeasurements()
        {
            _repository = new BodyMeasurementRepository();
            InitializeComponent();
            InitializeUI();
            LoadData();
        }
        
        private void InitializeUI()
        {
            this.Text = "Vücut Ölçülerim";
            this.BackColor = BackgroundLight;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(15);
            
            // Ana Layout - 2 Kolon (Sol: Input, Sağ: Chart & Grid)
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350)); // Sol panel sabit
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));  // Sağ panel esnek
            
            // Sol Panel - Yeni Ölçüm
            var leftPanel = CreateInputPanel();
            mainLayout.Controls.Add(leftPanel, 0, 0);
            
            // Sağ Panel - Grafik ve Liste
            var rightPanel = CreateRightPanel();
            mainLayout.Controls.Add(rightPanel, 1, 0);
            
            this.Controls.Add(mainLayout);
        }
        
        private Panel CreateInputPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = CardWhite, Padding = new Padding(20), Margin = new Padding(0, 0, 15, 0) };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 12);
            
            var lblTitle = new Label
            {
                Text = "📏 Yeni Ölçüm Ekle",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = PrimaryGreen,
                Location = new Point(20, 20),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);
            
            int y = 70;
            
            // Tarih
            AddLabel(panel, "Tarih:", 20, y);
            dateEdit = new DateEdit
            {
                Location = new Point(120, y - 3),
                Size = new Size(180, 30),
                Properties = { Appearance = { Font = new Font("Segoe UI", 10F) } }
            };
            dateEdit.DateTime = DateTime.Today;
            panel.Controls.Add(dateEdit);
            y += 50;
            
            // Ölçümler
            txtChest = CreateMeasurementInput(panel, "Göğüs (cm):", y); y += 45;
            txtWaist = CreateMeasurementInput(panel, "Bel (cm):", y); y += 45;
            txtHip = CreateMeasurementInput(panel, "Kalça (cm):", y); y += 45;
            txtArm = CreateMeasurementInput(panel, "Kol (cm):", y); y += 45;
            txtThigh = CreateMeasurementInput(panel, "Bacak (cm):", y); y += 45;
            txtNeck = CreateMeasurementInput(panel, "Boyun (cm):", y); y += 55;
            
            // Not
            AddLabel(panel, "Not:", 20, y);
            txtNotes = new MemoEdit
            {
                Location = new Point(20, y + 25),
                Size = new Size(280, 60),
                Properties = { Appearance = { Font = new Font("Segoe UI", 10F) } }
            };
            panel.Controls.Add(txtNotes);
            y += 100;
            
            // Butonlar
            var btnSave = new SimpleButton
            {
                Text = "KAYDET",
                Location = new Point(20, y),
                Size = new Size(130, 40),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryGreen, ForeColor = Color.White }
            };
            btnSave.Click += BtnSave_Click;
            panel.Controls.Add(btnSave);
            
            var btnClear = new SimpleButton
            {
                Text = "TEMİZLE",
                Location = new Point(160, y),
                Size = new Size(130, 40),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Appearance = { BackColor = TextMedium, ForeColor = Color.White }
            };
            btnClear.Click += (s, e) => ClearInputs();
            panel.Controls.Add(btnClear);
            
            return panel;
        }
        
        private Panel CreateRightPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            
            // Üst - Grafik Kartı
            var chartCard = new Panel { Dock = DockStyle.Top, Height = 300, BackColor = CardWhite, Padding = new Padding(10), Margin = new Padding(0, 0, 0, 15) };
            chartCard.Paint += (s, e) => DrawRoundedBorder(e.Graphics, chartCard, 12);
            
            var lblChartTitle = new Label
            {
                Text = "📈 Değişim Grafiği",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 10),
                AutoSize = true
            };
            chartCard.Controls.Add(lblChartTitle);
            
            chart = new ChartControl
            {
                Dock = DockStyle.Bottom,
                Height = 250,
                BorderOptions = { Visibility = DevExpress.Utils.DefaultBoolean.False }
            };
            chartCard.Controls.Add(chart);
            panel.Controls.Add(chartCard);
            
            // Alt - Liste Kartı
            var gridCard = new Panel { Dock = DockStyle.Fill, BackColor = CardWhite, Padding = new Padding(10) };
            // Padding hack for spacing
            var spacer = new Panel { Dock = DockStyle.Top, Height = 15, BackColor = Color.Transparent };
            panel.Controls.Add(spacer);
            
            gridCard.Paint += (s, e) => DrawRoundedBorder(e.Graphics, gridCard, 12);
            
            var lblGridTitle = new Label
            {
                Text = "📋 Geçmiş Ölçümler",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 10),
                AutoSize = true
            };
            gridCard.Controls.Add(lblGridTitle);
            
            gridMeasurements = new GridControl { Dock = DockStyle.Bottom, Height = 200 }; // Dynamic height handled by dock fill in real app usually, but here simple
            // Better:
            var gridContainer = new Panel { Location = new Point(10, 40), Size = new Size(100, 100), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            gridMeasurements.Dock = DockStyle.Fill;
            gridContainer.Controls.Add(gridMeasurements);
            gridCard.Controls.Add(gridContainer);
            
            gridView = new GridView(gridMeasurements);
            gridMeasurements.MainView = gridView;
            
            gridView.OptionsView.ShowGroupPanel = false;
            gridView.OptionsView.ShowIndicator = false;
            gridView.OptionsBehavior.Editable = false;
            gridView.RowHeight = 40;
            gridView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            gridView.Appearance.HeaderPanel.BackColor = Color.FromArgb(241, 245, 249);
            gridView.Appearance.Row.Font = new Font("Segoe UI", 10F);
            
            gridView.Columns.AddVisible("Date", "Tarih").DisplayFormat.FormatString = "dd.MM.yyyy";
            gridView.Columns.AddVisible("Waist", "Bel (cm)");
            gridView.Columns.AddVisible("Hip", "Kalça (cm)");
            gridView.Columns.AddVisible("Chest", "Göğüs (cm)");
            
            panel.Controls.Add(gridCard);
            
            return panel;
        }

        private SpinEdit CreateMeasurementInput(Panel parent, string label, int y)
        {
            AddLabel(parent, label, 20, y + 3);
            var spin = new SpinEdit
            {
                Location = new Point(120, y),
                Size = new Size(100, 30),
                Properties = { 
                    MinValue = 0, MaxValue = 300, Increment = 0.5m, IsFloatValue = true,
                    Appearance = { Font = new Font("Segoe UI", 10F) }
                }
            };
            parent.Controls.Add(spin);
            
            var lblCm = new Label
            {
                Text = "cm",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMedium,
                Location = new Point(225, y + 5),
                AutoSize = true
            };
            parent.Controls.Add(lblCm);
            
            return spin;
        }
        
        private void AddLabel(Panel parent, string text, int x, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextDark,
                Location = new Point(x, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);
        }
        
        private void LoadData()
        {
            try
            {
                _measurements = _repository.GetByPatient(AuthContext.UserId);
                gridMeasurements.DataSource = _measurements.OrderByDescending(m => m.Date).ToList();
                UpdateChart();
            }
            catch (Exception ex)
            {
                ToastNotification.ShowError("Veri yüklenirken hata: " + ex.Message);
            }
        }
        
        private void UpdateChart()
        {
            chart.Series.Clear();
            if (_measurements == null || !_measurements.Any()) return;
            
            var orderedData = _measurements.OrderBy(m => m.Date).ToList();
            
            // Bel
            var waistSeries = new Series("Bel", ViewType.Spline);
            waistSeries.ArgumentDataMember = "Date";
            waistSeries.ValueDataMembers.AddRange(new[] { "Waist" });
            waistSeries.DataSource = orderedData;
            ((LineSeriesView)waistSeries.View).Color = PrimaryGreen;
            ((LineSeriesView)waistSeries.View).LineStyle.Thickness = 3;
            chart.Series.Add(waistSeries);
            
            // Kalça
            var hipSeries = new Series("Kalça", ViewType.Spline);
            hipSeries.ArgumentDataMember = "Date";
            hipSeries.ValueDataMembers.AddRange(new[] { "Hip" });
            hipSeries.DataSource = orderedData;
            ((LineSeriesView)hipSeries.View).Color = InfoBlue;
            ((LineSeriesView)hipSeries.View).LineStyle.Thickness = 3;
            chart.Series.Add(hipSeries);
            
            chart.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
            chart.Legend.AlignmentHorizontal = LegendAlignmentHorizontal.Right;
            chart.Legend.AlignmentVertical = LegendAlignmentVertical.TopOutside;
            
            // Diagram ayarları
            if (chart.Diagram is XYDiagram diagram)
            {
                diagram.DefaultPane.BackColor = Color.White;
                diagram.DefaultPane.BorderVisible = false;
                diagram.AxisX.Label.Font = new Font("Segoe UI", 8F);
                diagram.AxisY.Label.Font = new Font("Segoe UI", 8F);
                diagram.AxisX.DateTimeScaleOptions.MeasureUnit = DateTimeMeasureUnit.Day;
                diagram.AxisX.DateTimeScaleOptions.GridAlignment = DateTimeGridAlignment.Day;
            }
        }
        
        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var measurement = new BodyMeasurement
                {
                    PatientId = AuthContext.UserId,
                    Date = dateEdit.DateTime,
                    Chest = GetNullableDouble(txtChest),
                    Waist = GetNullableDouble(txtWaist),
                    Hip = GetNullableDouble(txtHip),
                    Arm = GetNullableDouble(txtArm),
                    Thigh = GetNullableDouble(txtThigh),
                    Neck = GetNullableDouble(txtNeck),
                    Notes = txtNotes.Text
                };
                
                _repository.Add(measurement);
                ToastNotification.ShowSuccess("Ölçüm kaydedildi! ✅");
                ClearInputs();
                LoadData();
            }
            catch (Exception ex)
            {
                ToastNotification.ShowError("Hata: " + ex.Message);
            }
        }
        
        private double? GetNullableDouble(SpinEdit spin)
        {
            if (spin.EditValue == null || Convert.ToDouble(spin.EditValue) == 0)
                return null;
            return Convert.ToDouble(spin.EditValue);
        }
        
        private void ClearInputs()
        {
            txtChest.EditValue = 0;
            txtWaist.EditValue = 0;
            txtHip.EditValue = 0;
            txtArm.EditValue = 0;
            txtThigh.EditValue = 0;
            txtNeck.EditValue = 0;
            txtNotes.Text = "";
            dateEdit.EditValue = DateTime.Today;
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
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Name = "FrmBodyMeasurements";
            this.ResumeLayout(false);
        }
    }
}
