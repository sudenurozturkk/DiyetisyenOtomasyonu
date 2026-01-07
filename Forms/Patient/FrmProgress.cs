using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;

namespace DiyetisyenOtomasyonu.Forms.Patient
{
    /// <summary>
    /// İlerleme Takip Formu - Modern Tasarım
    /// </summary>
    public partial class FrmProgress : XtraForm
    {
        private readonly PatientService _patientService;
        private readonly DietService _dietService;
        private ChartControl chartWeight;
        private ChartControl chartCompletion;
        private DateEdit dateStart;
        private DateEdit dateEnd;
        private LabelControl lblWeightChange;

        // Modern Renkler - Yeşil Tema
        private readonly Color PrimaryGreen = Color.FromArgb(13, 148, 136);
        private readonly Color SuccessGreen = Color.FromArgb(34, 197, 94);
        private readonly Color DangerRed = Color.FromArgb(239, 68, 68);
        private readonly Color InfoBlue = Color.FromArgb(59, 130, 246);
        private readonly Color CardWhite = Color.White;
        private readonly Color BackgroundLight = Color.FromArgb(248, 250, 252);
        private readonly Color TextDark = Color.FromArgb(30, 41, 59);
        private readonly Color TextMedium = Color.FromArgb(100, 116, 139);
        private readonly Color BorderGray = Color.FromArgb(226, 232, 240);

        public FrmProgress()
        {
            InitializeComponent();
            _patientService = new PatientService();
            _dietService = new DietService();
            InitializeUI();
            LoadCharts();
        }

        private void InitializeUI()
        {
            this.Text = "İlerlemem";
            this.BackColor = BackgroundLight;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(15);

            // Ana Layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); // Filtre
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Grafikler

            // 1. Filtre Paneli
            var pnlFilter = CreateFilterPanel();
            mainLayout.Controls.Add(pnlFilter, 0, 0);

            // 2. Grafikler (Split)
            var splitCharts = new SplitContainerControl
            {
                Dock = DockStyle.Fill,
                Horizontal = false,
                SplitterPosition = 300,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.Transparent
            };
            
            // Kilo Grafiği Kartı
            var pnlWeight = CreateChartCard("📉 Kilo Değişimi", out chartWeight);
            splitCharts.Panel1.Controls.Add(pnlWeight);
            splitCharts.Panel1.Padding = new Padding(0, 0, 0, 10);

            // Tamamlama Grafiği Kartı
            var pnlCompletion = CreateChartCard("✅ Haftalık Tamamlama Oranı", out chartCompletion);
            splitCharts.Panel2.Controls.Add(pnlCompletion);
            splitCharts.Panel2.Padding = new Padding(0, 10, 0, 0);

            mainLayout.Controls.Add(splitCharts, 0, 1);
            this.Controls.Add(mainLayout);
        }

        private Panel CreateFilterPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = CardWhite, Padding = new Padding(15), Margin = new Padding(0, 0, 0, 15) };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 12);

            // Başlangıç
            AddLabel(panel, "Başlangıç:", 20, 28);
            dateStart = new DateEdit
            {
                Location = new Point(95, 25),
                Size = new Size(130, 30),
                Properties = { Appearance = { Font = new Font("Segoe UI", 10F) } }
            };
            dateStart.DateTime = DateTime.Now.AddMonths(-1);
            panel.Controls.Add(dateStart);

            // Bitiş
            AddLabel(panel, "Bitiş:", 240, 28);
            dateEnd = new DateEdit
            {
                Location = new Point(280, 25),
                Size = new Size(130, 30),
                Properties = { Appearance = { Font = new Font("Segoe UI", 10F) } }
            };
            dateEnd.DateTime = DateTime.Now;
            panel.Controls.Add(dateEnd);

            // Yenile Butonu
            var btnRefresh = new SimpleButton
            {
                Text = "YENİLE",
                Location = new Point(430, 23),
                Size = new Size(100, 34),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryGreen, ForeColor = Color.White }
            };
            btnRefresh.Click += (s, e) => LoadCharts();
            panel.Controls.Add(btnRefresh);

            // Özet Bilgi
            lblWeightChange = new LabelControl
            {
                Text = "Kilo Değişimi: --",
                Location = new Point(560, 28),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = SuccessGreen
            };
            panel.Controls.Add(lblWeightChange);

            return panel;
        }

        private Panel CreateChartCard(string title, out ChartControl chartCtrl)
        {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = CardWhite, Padding = new Padding(10) };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 12);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            var chart = new ChartControl
            {
                Dock = DockStyle.Bottom,
                Height = card.Height - 50, // Başlık payı
                BorderOptions = { Visibility = DevExpress.Utils.DefaultBoolean.False }
            };
            
            // Fix CS1628: Capture local variable instead of out parameter in lambda
            var capturedChart = chart; 
            card.Resize += (s, e) => capturedChart.Height = card.Height - 50;
            
            card.Controls.Add(chart);
            chartCtrl = chart; // Assign to out parameter at the end
            return card;
        }

        private void AddLabel(Panel parent, string text, int x, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextMedium,
                Location = new Point(x, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);
        }

        private void LoadCharts()
        {
            LoadWeightChart();
            LoadCompletionChart();
        }

        private void LoadWeightChart()
        {
            chartWeight.Series.Clear();

            var entries = _patientService.GetWeightHistory(AuthContext.UserId)
                .Where(w => w.Date >= dateStart.DateTime && w.Date <= dateEnd.DateTime)
                .OrderBy(w => w.Date)
                .ToList();

            if (entries.Count == 0)
            {
                lblWeightChange.Text = "Kilo Değişimi: Veri yok";
                lblWeightChange.ForeColor = TextMedium;
                return;
            }

            // Kilo degisimini hesapla
            double firstWeight = entries.First().Weight;
            double lastWeight = entries.Last().Weight;
            double change = lastWeight - firstWeight;

            if (change < 0)
            {
                lblWeightChange.Text = "Kilo Değişimi: " + change.ToString("0.0") + " kg";
                lblWeightChange.ForeColor = SuccessGreen;
            }
            else if (change > 0)
            {
                lblWeightChange.Text = "Kilo Değişimi: +" + change.ToString("0.0") + " kg";
                lblWeightChange.ForeColor = DangerRed;
            }
            else
            {
                lblWeightChange.Text = "Kilo Değişimi: 0 kg";
                lblWeightChange.ForeColor = TextMedium;
            }

            var series = new Series("Kilo (kg)", ViewType.Spline);
            series.ArgumentScaleType = ScaleType.DateTime;

            foreach (var entry in entries)
            {
                series.Points.Add(new SeriesPoint(entry.Date, entry.Weight));
            }

            var view = (LineSeriesView)series.View;
            view.LineStyle.Thickness = 4;
            view.Color = PrimaryGreen;
            view.MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
            // Fix CS1061: Use LineMarkerOptions instead of MarkerStyle if available, or just skip detailed marker styling if not essential
            // In some versions it is view.LineMarkerOptions.Kind = MarkerKind.Circle;
            // For now, let's just enable visibility which is safe.
            
            chartWeight.Series.Add(series);
            
            // Diagram ayarları
            if (chartWeight.Diagram is XYDiagram diagram)
            {
                diagram.DefaultPane.BackColor = Color.White;
                diagram.DefaultPane.BorderVisible = false;
                diagram.AxisX.Label.Font = new Font("Segoe UI", 9F);
                diagram.AxisY.Label.Font = new Font("Segoe UI", 9F);
                diagram.AxisX.DateTimeScaleOptions.MeasureUnit = DateTimeMeasureUnit.Day;
            }
        }

        private void LoadCompletionChart()
        {
            chartCompletion.Series.Clear();

            var weeks = _dietService.GetPatientAllWeeks(AuthContext.UserId)
                .OrderBy(w => w.WeekStartDate)
                .ToList();

            if (weeks.Count == 0) return;

            var series = new Series("Tamamlama %", ViewType.Bar);

            foreach (var week in weeks)
            {
                series.Points.Add(new SeriesPoint(week.WeekStartDate.ToString("dd.MM"), week.WeeklyCompletionRate));
            }

            var view = (BarSeriesView)series.View;
            view.Color = InfoBlue;
            // Fix CS0104: Specify full namespace for FillMode
            view.FillStyle.FillMode = DevExpress.XtraCharts.FillMode.Solid;
            
            chartCompletion.Series.Add(series);
            chartCompletion.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
            
            if (chartCompletion.Diagram is XYDiagram diagram)
            {
                diagram.DefaultPane.BackColor = Color.White;
                diagram.DefaultPane.BorderVisible = false;
                diagram.AxisX.Label.Font = new Font("Segoe UI", 9F);
                diagram.AxisY.Label.Font = new Font("Segoe UI", 9F);
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
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Name = "FrmProgress";
            this.ResumeLayout(false);
        }
    }
}
