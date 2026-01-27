using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Forms.Patient
{
    public partial class FrmProgress : XtraForm
    {
        private readonly DiyetisyenOtomasyonu.Infrastructure.Services.ReportService _reportService;
        private readonly DiyetisyenOtomasyonu.Infrastructure.Repositories.PatientRepository _patientRepo;
        private ChartControl chartWeight;
        private ComboBoxEdit cmbDateRange;

        // Modern Renkler - UiStyles
        private Color PrimaryColor => UiStyles.PrimaryColor;
        private Color SuccessGreen => UiStyles.SuccessColor;
        private Color InfoBlue => UiStyles.InfoColor;
        private Color WarningOrange => UiStyles.WarningColor;
        private Color CardColor => Color.White;
        private Color BackgroundColor => Color.FromArgb(245, 247, 250);
        private Color TextPrimary => UiStyles.TextPrimary;
        private Color TextSecondary => UiStyles.TextSecondary;

        public FrmProgress()
        {
            InitializeComponent();
            _reportService = new DiyetisyenOtomasyonu.Infrastructure.Services.ReportService();
            _patientRepo = new DiyetisyenOtomasyonu.Infrastructure.Repositories.PatientRepository();
            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            this.Text = "İlerlemem";
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
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // Summary Cards
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Chart

            // 1. Summary Cards
            var summaryPanel = CreateSummaryPanel();
            mainLayout.Controls.Add(summaryPanel, 0, 0);

            // 2. Chart Section
            var chartPanel = CreateChartPanel();
            mainLayout.Controls.Add(chartPanel, 0, 1);

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

            var patient = _patientRepo.GetById(AuthContext.UserId);
            if (patient != null)
            {
                double lostWeight = patient.BaslangicKilosu - patient.GuncelKilo;
                string lostWeightText = lostWeight > 0 ? $"-{lostWeight:F1} kg" : $"+{Math.Abs(lostWeight):F1} kg";
                Color lostColor = lostWeight > 0 ? SuccessGreen : WarningOrange;

                double bmi = patient.Boy > 0 ? patient.GuncelKilo / (patient.Boy * patient.Boy) : 0;
                
                int cardWidth = 220;
                int spacing = 20;

                CreateStatCard(panel, 0, 0, "Güncel Kilo", $"{patient.GuncelKilo} kg", PrimaryColor);
                CreateStatCard(panel, cardWidth + spacing, 0, "Toplam Değişim", lostWeightText, lostColor);
                CreateStatCard(panel, 2 * (cardWidth + spacing), 0, "BMI", $"{bmi:F1}", InfoBlue);
            }

            // Date Range Filter (Right aligned)
            cmbDateRange = new ComboBoxEdit
            {
                Location = new Point(panel.Width - 220, 20),
                Size = new Size(200, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Properties = { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor }
            };
            cmbDateRange.Properties.Items.AddRange(new[] { "Son 1 Ay", "Son 3 Ay", "Son 6 Ay", "Tüm Zamanlar" });
            cmbDateRange.SelectedIndex = 0;
            cmbDateRange.SelectedIndexChanged += (s, e) => LoadData();
            panel.Controls.Add(cmbDateRange);

            return panel;
        }

        private void CreateStatCard(PanelControl parent, int x, int y, string title, string value, Color color)
        {
            var card = new PanelControl
            {
                Location = new Point(x, y),
                Size = new Size(220, 80),
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
        }

        private PanelControl CreateChartPanel()
        {
            var panel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardColor,
                Padding = new Padding(10)
            };

            chartWeight = new ChartControl
            {
                Dock = DockStyle.Fill,
                BorderOptions = { Visibility = DevExpress.Utils.DefaultBoolean.False }
            };

            // Chart Setup
            chartWeight.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
            
            // Series
            Series series = new Series("Kilo Değişimi", ViewType.SplineArea);
            series.ArgumentScaleType = ScaleType.DateTime;
            series.View.Color = Color.FromArgb(100, PrimaryColor); // Transparent fill
            ((SplineAreaSeriesView)series.View).Border.Color = PrimaryColor;
            ((SplineAreaSeriesView)series.View).Border.Thickness = 3;
            ((SplineAreaSeriesView)series.View).MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
            
            chartWeight.Series.Add(series);
            panel.Controls.Add(chartWeight);

            // Diagram Configuration (Must be done AFTER adding series)
            if (chartWeight.Diagram is XYDiagram diagram)
            {
                diagram.AxisX.DateTimeScaleOptions.MeasureUnit = DateTimeMeasureUnit.Day;
                diagram.AxisX.Label.TextPattern = "{A:dd MMM}";
                diagram.AxisY.Title.Text = "Kilo (kg)";
                diagram.AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
                diagram.EnableAxisXScrolling = true;
                diagram.EnableAxisXZooming = true;
                
                // Grid lines
                diagram.AxisY.GridLines.Visible = true;
                diagram.AxisY.GridLines.Color = Color.FromArgb(240, 240, 240);
                diagram.AxisX.GridLines.Visible = false;
                
                diagram.DefaultPane.BackColor = Color.White;
                diagram.DefaultPane.BorderVisible = false;
            }

            return panel;
        }

        private void LoadData()
        {
            DateTime startDate = DateTime.Now.AddMonths(-1);
            if (cmbDateRange.SelectedIndex == 1) startDate = DateTime.Now.AddMonths(-3);
            else if (cmbDateRange.SelectedIndex == 2) startDate = DateTime.Now.AddMonths(-6);
            else if (cmbDateRange.SelectedIndex == 3) startDate = DateTime.MinValue;

            var weightHistory = _reportService.GetWeightHistory(AuthContext.UserId, startDate, DateTime.Now);
            
            chartWeight.Series[0].Points.Clear();
            foreach (var entry in weightHistory)
            {
                chartWeight.Series[0].Points.Add(new SeriesPoint(entry.Date, entry.Weight));
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1100, 600);
            this.Name = "FrmProgress";
            this.ResumeLayout(false);
        }
    }
}
