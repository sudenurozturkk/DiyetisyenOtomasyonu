using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Infrastructure.DI;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    public partial class FrmAnalytics : XtraForm
    {
        private readonly PatientService _patientService;
        private readonly DietService _dietService;

        private LookUpEdit cmbPatient;
        private DateEdit dateStart;
        private DateEdit dateEnd;
        private ChartControl chartWeight;
        private ChartControl chartMacros;
        private LabelControl lblCard0, lblCard1, lblCard2, lblCard3;
        private LabelControl lblPatientInfo;
        
        // AI Chat Controls
        private MemoEdit txtAiAnalysis;
        private ListBoxControl lstChatHistory;
        private TextEdit txtChatInput;
        private SimpleButton btnSendChat;
        private readonly AiAssistantService _aiService;

        // Teal Theme Colors
        private readonly Color PrimaryColor = Color.FromArgb(0, 121, 107);      // Teal 700
        private readonly Color DarkColor = Color.FromArgb(0, 77, 64);           // Teal 900
        private readonly Color LightColor = Color.FromArgb(178, 223, 219);      // Teal 100
        private readonly Color AccentColor = Color.FromArgb(38, 166, 154);      // Teal 400
        private readonly Color SuccessColor = Color.FromArgb(34, 197, 94);      // Green
        private readonly Color DangerColor = Color.FromArgb(239, 68, 68);       // Red
        private readonly Color WarningColor = Color.FromArgb(245, 158, 11);     // Amber
        private readonly Color InfoColor = Color.FromArgb(14, 165, 233);        // Blue
        private readonly Color CardColor = Color.White;
        private readonly Color BackgroundColor = Color.FromArgb(240, 253, 250); // Teal 50
        private readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private readonly Color TextSecondary = Color.FromArgb(100, 116, 139);

        public FrmAnalytics()
        {
            InitializeComponent();

            var container = ServiceContainer.Instance;
            _patientService = container.GetService<PatientService>();
            _dietService = container.GetService<DietService>();
            _aiService = container.GetService<AiAssistantService>();

            InitializeUI();
            LoadPatients();
        }

        private void InitializeUI()
        {
            this.Text = "Raporlar ve Analizler";
            this.BackColor = BackgroundColor;
            this.Padding = new Padding(15);

            // Ust Panel - Filtreler
            var pnlTop = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 90,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardColor,
                Padding = new Padding(20)
            };

            var lblTitle = new LabelControl
            {
                Text = "RAPORLAR VE ANALIZLER",
                Location = new Point(5, 8),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = PrimaryColor
            };
            pnlTop.Controls.Add(lblTitle);

            var lblPatient = new LabelControl
            {
                Text = "Hasta:",
                Location = new Point(5, 50),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextSecondary
            };
            pnlTop.Controls.Add(lblPatient);

            cmbPatient = new LookUpEdit
            {
                Location = new Point(60, 46),
                Size = new Size(220, 32)
            };
            cmbPatient.Properties.DisplayMember = "AdSoyad";
            cmbPatient.Properties.ValueMember = "Id";
            cmbPatient.Properties.NullText = "-- Hasta seçiniz --";
            cmbPatient.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            cmbPatient.Properties.PopupFormSize = new Size(300, 200);
            cmbPatient.Properties.Columns.Clear();
            cmbPatient.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("AdSoyad", "Hasta", 180));
            cmbPatient.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("BMIKategori", "Durum", 100));
            pnlTop.Controls.Add(cmbPatient);

            var lblStart = new LabelControl
            {
                Text = "Baslangic:",
                Location = new Point(300, 50),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextSecondary
            };
            pnlTop.Controls.Add(lblStart);

            dateStart = new DateEdit
            {
                Location = new Point(385, 46),
                Size = new Size(130, 32)
            };
            dateStart.DateTime = DateTime.Now.AddMonths(-1);
            dateStart.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            pnlTop.Controls.Add(dateStart);

            var lblEnd = new LabelControl
            {
                Text = "Bitis:",
                Location = new Point(535, 50),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextSecondary
            };
            pnlTop.Controls.Add(lblEnd);

            dateEnd = new DateEdit
            {
                Location = new Point(580, 46),
                Size = new Size(130, 32)
            };
            dateEnd.DateTime = DateTime.Now;
            dateEnd.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            pnlTop.Controls.Add(dateEnd);

            var btnRefresh = new SimpleButton
            {
                Text = "Analiz Et",
                Location = new Point(730, 44),
                Size = new Size(120, 38),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryColor, ForeColor = Color.White },
                AllowFocus = false
            };
            btnRefresh.Click += BtnRefresh_Click;
            pnlTop.Controls.Add(btnRefresh);

            this.Controls.Add(pnlTop);

            // Ozet Kartlari
            var pnlSummary = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 140,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 15, 0, 15)
            };

            lblCard0 = CreateSummaryCard(pnlSummary, 0, "Toplam Hasta", "0", PrimaryColor, "👥");
            lblCard1 = CreateSummaryCard(pnlSummary, 220, "Ortalama BMI", "0.0", SuccessColor, "📊");
            lblCard2 = CreateSummaryCard(pnlSummary, 440, "Aktif Plan", "0", WarningColor, "📋");
            lblCard3 = CreateSummaryCard(pnlSummary, 660, "Kilo Degisimi", "0 kg", InfoColor, "⚖️");

            this.Controls.Add(pnlSummary);

            // Grafikler - SplitContainer
            var splitCharts = new SplitContainerControl
            {
                Dock = DockStyle.Fill,
                Horizontal = true,
                SplitterPosition = 550,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            splitCharts.Panel1.Padding = new Padding(0, 0, 10, 0);
            splitCharts.Panel2.Padding = new Padding(10, 0, 0, 0);

            // Kilo Grafigi
            var grpWeight = new GroupControl
            {
                Text = "KILO TAKIBI",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = InfoColor,
                Appearance = { BackColor = CardColor }
            };

            chartWeight = new ChartControl { Dock = DockStyle.Fill };
            chartWeight.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
            chartWeight.BackColor = CardColor;
            grpWeight.Controls.Add(chartWeight);

            splitCharts.Panel1.Controls.Add(grpWeight);

            // Makro Grafigi
            var grpMacros = new GroupControl
            {
                Text = "MAKRO BESIN DAGILIMI",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = AccentColor,
                Appearance = { BackColor = CardColor }
            };

            chartMacros = new ChartControl { Dock = DockStyle.Fill };
            chartMacros.BackColor = CardColor;
            grpMacros.Controls.Add(chartMacros);

            splitCharts.Panel2.Controls.Add(grpMacros);
            splitCharts.Panel2.Controls.Add(grpMacros);
            
            // Main Content Panel (Fills the rest of the form)
            var pnlMainContent = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = Color.Transparent
            };
            this.Controls.Add(pnlMainContent);
            pnlMainContent.BringToFront(); // Ensure it fills the space after Top panels

            // AI Panel (Alt Kısım) - Add to Main Content
            var pnlAi = new PanelControl
            {
                Dock = DockStyle.Bottom,
                Height = 250,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Padding = new Padding(0, 10, 0, 0)
            };

            var splitAi = new SplitContainerControl
            {
                Dock = DockStyle.Fill,
                Horizontal = true,
                SplitterPosition = 600,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            // AI Analiz Sonuçları
            var grpAnalysis = new GroupControl
            {
                Text = "YAPAY ZEKA ANALİZİ",
                Dock = DockStyle.Fill,
                Appearance = { BackColor = CardColor }
            };
            
            txtAiAnalysis = new MemoEdit
            {
                Dock = DockStyle.Fill,
                Properties = { ReadOnly = true, ScrollBars = ScrollBars.Vertical }
            };
            grpAnalysis.Controls.Add(txtAiAnalysis);
            splitAi.Panel1.Controls.Add(grpAnalysis);

            // AI Sohbet
            var grpChat = new GroupControl
            {
                Text = "AI ASİSTAN İLE SOHBET",
                Dock = DockStyle.Fill,
                Appearance = { BackColor = CardColor }
            };

            var pnlChatInput = new PanelControl
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            btnSendChat = new SimpleButton
            {
                Text = "Gönder",
                Dock = DockStyle.Right,
                Width = 80
            };
            btnSendChat.Click += BtnSendChat_Click;

            txtChatInput = new TextEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false }
            };
            txtChatInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnSendChat_Click(s, e); };

            pnlChatInput.Controls.Add(txtChatInput);
            pnlChatInput.Controls.Add(btnSendChat);

            lstChatHistory = new ListBoxControl
            {
                Dock = DockStyle.Fill
            };

            grpChat.Controls.Add(lstChatHistory);
            grpChat.Controls.Add(pnlChatInput);
            splitAi.Panel2.Controls.Add(grpChat);

            pnlAi.Controls.Add(splitAi);
            pnlMainContent.Controls.Add(pnlAi); // Add AI panel to Main Content

            // Charts - Add to Main Content
            // Note: splitCharts was created earlier but not added to controls yet in this block
            // We need to ensure splitCharts is added AFTER pnlAi to pnlMainContent so it Fills remaining space
            pnlMainContent.Controls.Add(splitCharts); 
            splitCharts.BringToFront(); // Ensure charts are at top of Z-order in MainContent (Docked Last -> Fill)
        }

        private LabelControl CreateSummaryCard(PanelControl parent, int x, string title, string value, Color color, string icon)
        {
            var card = new PanelControl
            {
                Location = new Point(x, 0),
                Size = new Size(200, 110),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple,
                BackColor = CardColor
            };

            // Sol renk cizgisi
            var colorBar = new PanelControl
            {
                Location = new Point(0, 0),
                Size = new Size(6, 110),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = color
            };
            card.Controls.Add(colorBar);

            var lblIcon = new LabelControl
            {
                Text = icon,
                Location = new Point(150, 15),
                Font = new Font("Segoe UI", 28F),
                ForeColor = Color.FromArgb(200, color.R, color.G, color.B)
            };
            card.Controls.Add(lblIcon);

            var lblTitle = new LabelControl
            {
                Text = title,
                Location = new Point(20, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextSecondary
            };
            card.Controls.Add(lblTitle);

            var lblValue = new LabelControl
            {
                Text = value,
                Location = new Point(20, 50),
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                ForeColor = color,
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(150, 40)
            };
            card.Controls.Add(lblValue);

            parent.Controls.Add(card);
            return lblValue;
        }

        private void LoadPatients()
        {
            try
            {
                var patients = _patientService.GetPatientsByDoctor(AuthContext.UserId);
                cmbPatient.Properties.DataSource = patients;
                
                if (patients.Count > 0)
                {
                    cmbPatient.EditValue = patients[0].Id;
                    LoadAnalytics();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hastalar yuklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (cmbPatient.EditValue == null)
            {
                XtraMessageBox.Show("Lütfen hasta seçin", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            LoadAnalytics();
        }

        private async void BtnSendChat_Click(object sender, EventArgs e)
        {
            if (cmbPatient.EditValue == null) return;
            string question = txtChatInput.Text.Trim();
            if (string.IsNullOrEmpty(question)) return;

            int patientId = (int)cmbPatient.EditValue;
            
            lstChatHistory.Items.Add($"Siz: {question}");
            txtChatInput.Text = "";
            txtChatInput.Enabled = false;
            btnSendChat.Enabled = false;

            try
            {
                string response = await _aiService.GetChatResponseAsync(question, patientId);
                lstChatHistory.Items.Add($"AI: {response}");
                lstChatHistory.TopIndex = lstChatHistory.ItemCount - 1;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("AI hatası: " + ex.Message);
            }
            finally
            {
                txtChatInput.Enabled = true;
                btnSendChat.Enabled = true;
                txtChatInput.Focus();
            }
        }

        private async void LoadAnalytics()
        {
            try
            {
                int patientId = (int)cmbPatient.EditValue;
                var patients = _patientService.GetPatientsByDoctor(AuthContext.UserId);
                var avgBMI = patients.Count > 0 ? patients.Average(p => p.BMI) : 0;
                var dietWeeks = _dietService.GetPatientAllWeeks(patientId);
                var activePlans = dietWeeks.Count;

                // Kilo degisimi hesapla
                var weightHistory = _patientService.GetWeightHistory(patientId);
                double weightChange = 0;
                if (weightHistory.Count >= 2)
                {
                    var sorted = weightHistory.OrderBy(w => w.Date).ToList();
                    weightChange = sorted.Last().Weight - sorted.First().Weight;
                }

                lblCard0.Text = patients.Count.ToString();
                lblCard1.Text = avgBMI.ToString("F1");
                lblCard2.Text = activePlans.ToString();
                lblCard3.Text = weightChange >= 0 ? $"+{weightChange:F1} kg" : $"{weightChange:F1} kg";
                lblCard3.ForeColor = weightChange <= 0 ? SuccessColor : DangerColor;

                LoadWeightChart(patientId);
                LoadMacroChart(patientId);

                // AI Analizi Başlat
                txtAiAnalysis.Text = "Yapay zeka analizi yapılıyor, lütfen bekleyin...";
                var analysis = await _aiService.GetComprehensiveAnalysisAsync(patientId);
                txtAiAnalysis.Text = analysis.Recommendations;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Analiz yuklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtAiAnalysis.Text = "Analiz sırasında hata oluştu.";
            }
        }

        private void FrmAnalytics_Load(object sender, EventArgs e)
        {

        }

        private void LoadWeightChart(int patientId)
        {
            chartWeight.Series.Clear();
            var entries = _patientService.GetWeightHistory(patientId)
                .Where(w => w.Date >= dateStart.DateTime && w.Date <= dateEnd.DateTime)
                .OrderBy(w => w.Date).ToList();

            if (entries.Count == 0) 
            {
                // Bos grafik mesaji
                var title = new ChartTitle { Text = "Kilo verisi bulunamadi" };
                chartWeight.Titles.Clear();
                chartWeight.Titles.Add(title);
                return;
            }

            chartWeight.Titles.Clear();

            var series = new Series("Kilo (kg)", ViewType.Spline);
            series.ArgumentScaleType = ScaleType.DateTime;
            foreach (var e in entries) series.Points.Add(new SeriesPoint(e.Date, e.Weight));
            
            var view = (SplineSeriesView)series.View;
            view.LineStyle.Thickness = 3;
            view.Color = InfoColor;
            view.MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
            
            chartWeight.Series.Add(series);
        }

        private void LoadMacroChart(int patientId)
        {
            chartMacros.Series.Clear();
            var weeks = _dietService.GetPatientAllWeeks(patientId);
            
            if (weeks.Count == 0) 
            {
                var title = new ChartTitle { Text = "Diyet plani bulunamadi" };
                chartMacros.Titles.Clear();
                chartMacros.Titles.Add(title);
                return;
            }

            chartMacros.Titles.Clear();

            double protein = 0, carbs = 0, fat = 0;
            int count = 0;
            foreach (var w in weeks)
            {
                foreach (var d in w.Days) 
                { 
                    protein += d.TotalProtein; 
                    carbs += d.TotalCarbs; 
                    fat += d.TotalFat; 
                    count++; 
                }
            }

            if (count == 0) return;

            var series = new Series("Makro", ViewType.Pie3D);
            series.Points.Add(new SeriesPoint("Protein", protein / count) { Color = SuccessColor });
            series.Points.Add(new SeriesPoint("Karbonhidrat", carbs / count) { Color = WarningColor });
            series.Points.Add(new SeriesPoint("Yag", fat / count) { Color = DangerColor });
            
            var view = (Pie3DSeriesView)series.View;
            view.ExplodeMode = PieExplodeMode.UsePoints;
            
            series.Label.TextPattern = "{A}: {V:F1}g ({VP:P0})";
            series.LegendTextPattern = "{A}";
            
            chartMacros.Series.Add(series);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FrmAnalytics
            // 
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Name = "FrmAnalytics";
            this.Load += new System.EventHandler(this.FrmAnalytics_Load);
            this.ResumeLayout(false);

        }
    }
}
