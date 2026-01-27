using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DiyetisyenOtomasyonu.Domain;
using DiyetisyenOtomasyonu.Infrastructure.Repositories;
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    /// <summary>
    /// FrmReports - Hasta bazlı detaylı raporlar
    /// Gerçek veritabanı verileriyle çalışır
    /// </summary>
    public partial class FrmReports : DevExpress.XtraEditors.XtraForm
    {
        // UI Controls
        private PanelControl mainPanel;
        private PanelControl headerPanel;
        private TableLayoutPanel cardsContainer;
        private ComboBoxEdit cmbPatient;
        private LabelControl lblPatientInfo;
        
        // Repositories
        private readonly PatientRepository _patientRepo;
        private readonly ReportRepository _reportRepo;
        private readonly WeightEntryRepository _weightRepo;
        private readonly NoteRepository _noteRepo;
        private readonly GoalRepository _goalRepo;
        private readonly BodyMeasurementRepository _bodyRepo;
        
        // Data
        private List<DiyetisyenOtomasyonu.Domain.Patient> _patients;
        private DiyetisyenOtomasyonu.Domain.Patient _selectedPatient;
        private DietitianSupportResponse _lastAiResponse;
        
        // Colors
        private Color PrimaryColor { get { return UiStyles.PrimaryColor; } }
        private Color SuccessColor { get { return UiStyles.SuccessColor; } }
        private Color DangerColor { get { return UiStyles.DangerColor; } }
        private Color WarningColor { get { return UiStyles.WarningColor; } }
        private Color InfoColor { get { return UiStyles.InfoColor; } }
        private Color CardBg { get { return Color.White; } }
        private Color BackgroundColor { get { return UiStyles.BackgroundColor; } }
        private Color TextPrimary { get { return UiStyles.TextPrimary; } }
        private Color TextSecondary { get { return UiStyles.TextSecondary; } }

        public FrmReports(int patientId = 0)
        {
            _patientRepo = new PatientRepository();
            _reportRepo = new ReportRepository();
            _weightRepo = new WeightEntryRepository();
            _noteRepo = new NoteRepository();
            _goalRepo = new GoalRepository();
            _bodyRepo = new BodyMeasurementRepository();
            
            InitializeComponent();
            SetupUI();
            LoadPatients();
            
            if (patientId > 0)
            {
                SelectPatientById(patientId);
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Hasta Raporları";
            this.Size = new Size(1600, 1000);
            this.BackColor = BackgroundColor;
        }

        private void SetupUI()
        {
            // Main Container
            mainPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = BackgroundColor,
                Padding = new Padding(15)
            };
            this.Controls.Add(mainPanel);

            // Header Panel
            headerPanel = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 80,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardBg,
                Padding = new Padding(15)
            };

            // Title
            var lblTitle = new LabelControl
            {
                Text = "📊 Hasta Raporları",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(15, 10)
            };
            headerPanel.Controls.Add(lblTitle);

            // Patient Selection Label
            var lblSelect = new LabelControl
            {
                Text = "Hasta Seçin:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextSecondary,
                Location = new Point(15, 45)
            };
            headerPanel.Controls.Add(lblSelect);

            // Patient Combo
            cmbPatient = new ComboBoxEdit
            {
                Location = new Point(100, 42),
                Size = new Size(280, 28),
                Font = new Font("Segoe UI", 10F)
            };
            cmbPatient.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cmbPatient.SelectedIndexChanged += CmbPatient_SelectedIndexChanged;
            headerPanel.Controls.Add(cmbPatient);

            // Patient Info Label
            lblPatientInfo = new LabelControl
            {
                Text = "",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextSecondary,
                Location = new Point(400, 46),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(400, 20)
            };
            headerPanel.Controls.Add(lblPatientInfo);

            // Buttons
            var btnRefresh = new SimpleButton
            {
                Text = "🔄 Yenile",
                Location = new Point(headerPanel.Width - 200, 40),
                Size = new Size(90, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnRefresh.Appearance.BackColor = SuccessColor;
            btnRefresh.Appearance.ForeColor = Color.White;
            btnRefresh.Appearance.Options.UseBackColor = true;
            btnRefresh.Appearance.Options.UseForeColor = true;
            btnRefresh.Click += (s, e) => LoadPatientReports();
            headerPanel.Controls.Add(btnRefresh);

            var btnPdf = new SimpleButton
            {
                Text = "📄 PDF",
                Location = new Point(headerPanel.Width - 100, 40),
                Size = new Size(80, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnPdf.Appearance.BackColor = PrimaryColor;
            btnPdf.Appearance.ForeColor = Color.White;
            btnPdf.Appearance.Options.UseBackColor = true;
            btnPdf.Appearance.Options.UseForeColor = true;
            btnPdf.Click += (s, e) => ExportPdf();
            headerPanel.Controls.Add(btnPdf);

            mainPanel.Controls.Add(headerPanel);

            // Cards Container (3x3 Grid)
            cardsContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 0)
            };
            
            for (int i = 0; i < 3; i++)
                cardsContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            for (int i = 0; i < 3; i++)
                cardsContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));

            mainPanel.Controls.Add(cardsContainer);
            cardsContainer.BringToFront();
        }

        private void LoadPatients()
        {
            try
            {
                int doctorId = AuthContext.UserId > 0 ? AuthContext.UserId : 1;
                var patients = _patientRepo.GetByDoctorId(doctorId);
                _patients = patients.ToList();
                
                cmbPatient.Properties.Items.Clear();
                cmbPatient.Properties.Items.Add("-- Hasta Seçin --");
                
                foreach (var p in _patients)
                {
                    cmbPatient.Properties.Items.Add(p.AdSoyad + " (ID: " + p.Id + ")");
                }
                
                cmbPatient.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hasta listesi yüklenemedi: " + ex.Message, "Hata");
            }
        }

        private void SelectPatientById(int patientId)
        {
            if (_patients == null) return;
            
            for (int i = 0; i < _patients.Count; i++)
            {
                if (_patients[i].Id == patientId)
                {
                    cmbPatient.SelectedIndex = i + 1;
                    break;
                }
            }
        }

        private void CmbPatient_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPatient.SelectedIndex <= 0)
            {
                _selectedPatient = null;
                lblPatientInfo.Text = "";
                ShowEmptyState();
                return;
            }
            
            _selectedPatient = _patients[cmbPatient.SelectedIndex - 1];
            UpdatePatientInfo();
            LoadPatientReports();
        }

        private void UpdatePatientInfo()
        {
            if (_selectedPatient == null) return;
            
            double bmi = _selectedPatient.GuncelKilo / Math.Pow(_selectedPatient.Boy / 100, 2);
            lblPatientInfo.Text = "Boy: " + _selectedPatient.Boy + " cm | Kilo: " + _selectedPatient.GuncelKilo + " kg | BMI: " + bmi.ToString("F1");
        }

        private void ShowEmptyState()
        {
            cardsContainer.Controls.Clear();
            
            var emptyPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BackColor = CardBg
            };
            
            var lblEmpty = new LabelControl
            {
                Text = "📋 Raporları görüntülemek için yukarıdan bir hasta seçin.",
                Font = new Font("Segoe UI", 14F),
                ForeColor = TextSecondary,
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(500, 40),
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center } }
            };
            emptyPanel.Controls.Add(lblEmpty);
            
            // Center label when panel is resized
            emptyPanel.Resize += (s, e) => {
                lblEmpty.Location = new Point(
                    Math.Max(0, (emptyPanel.Width - lblEmpty.Width) / 2), 
                    Math.Max(0, (emptyPanel.Height - lblEmpty.Height) / 2)
                );
            };
            
            cardsContainer.SetColumnSpan(emptyPanel, 3);
            cardsContainer.SetRowSpan(emptyPanel, 3);
            cardsContainer.Controls.Add(emptyPanel, 0, 0);
        }

        private async void LoadPatientReports()
        {
            if (_selectedPatient == null)
            {
                ShowEmptyState();
                return;
            }
            
            cardsContainer.Controls.Clear();
            int patientId = _selectedPatient.Id;
            
            try
            {
                // Row 1: Kilo Takibi, Hedefler, Notlar
                var card1 = CreateCard("📈 Kilo Takibi", PrimaryColor);
                var chart1 = CreateWeightChart(patientId);
                chart1.Dock = DockStyle.Fill;
                card1.Controls.Add(chart1);
                chart1.BringToFront();
                cardsContainer.Controls.Add(card1, 0, 0);

                var card2 = CreateCard("🎯 Hedefler", SuccessColor);
                var goalsPanel = CreateGoalsPanel(patientId);
                goalsPanel.Dock = DockStyle.Fill;
                card2.Controls.Add(goalsPanel);
                goalsPanel.BringToFront();
                cardsContainer.Controls.Add(card2, 1, 0);

                var card3 = CreateCard("📝 Son Notlar", InfoColor);
                var notesPanel = CreateNotesPanel(patientId);
                notesPanel.Dock = DockStyle.Fill;
                card3.Controls.Add(notesPanel);
                notesPanel.BringToFront();
                cardsContainer.Controls.Add(card3, 2, 0);

                // Row 2: Vücut Ölçüleri, Öğün Uyumu, Özet
                var card4 = CreateCard("📏 Vücut Ölçüleri", WarningColor);
                var bodyPanel = CreateBodyMeasurementsPanel(patientId);
                bodyPanel.Dock = DockStyle.Fill;
                card4.Controls.Add(bodyPanel);
                bodyPanel.BringToFront();
                cardsContainer.Controls.Add(card4, 0, 1);

                var card5 = CreateCard("🍽️ Öğün Uyumu", DangerColor);
                var mealChart = CreateMealAdherenceChart(patientId);
                mealChart.Dock = DockStyle.Fill;
                card5.Controls.Add(mealChart);
                mealChart.BringToFront();
                cardsContainer.Controls.Add(card5, 1, 1);

                var card6 = CreateCard("📊 Özet Bilgiler", PrimaryColor);
                var summaryPanel = CreateSummaryPanel(patientId);
                summaryPanel.Dock = DockStyle.Fill;
                card6.Controls.Add(summaryPanel);
                summaryPanel.BringToFront();
                cardsContainer.Controls.Add(card6, 2, 1);

                // Row 3: BMI Trendi, İlerleme, AI Önerileri
                var card7 = CreateCard("⚖️ BMI Trendi", SuccessColor);
                var bmiChart = CreateBMIChart(patientId);
                bmiChart.Dock = DockStyle.Fill;
                card7.Controls.Add(bmiChart);
                bmiChart.BringToFront();
                cardsContainer.Controls.Add(card7, 0, 2);

                var card8 = CreateCard("📈 İlerleme Durumu", InfoColor);
                var progressPanel = CreateProgressPanel(patientId);
                progressPanel.Dock = DockStyle.Fill;
                card8.Controls.Add(progressPanel);
                progressPanel.BringToFront();
                cardsContainer.Controls.Add(card8, 1, 2);

                var card9 = CreateCard("🤖 AI Önerileri (Gemini)", Color.FromArgb(139, 92, 246));
                // AI yükleniyor göstergesi
                var loadingLabel = new LabelControl 
                { 
                    Text = "AI Analizi Yapılıyor...", 
                    AutoSizeMode = LabelAutoSizeMode.None,
                    Dock = DockStyle.Fill,
                    Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center, VAlignment = DevExpress.Utils.VertAlignment.Center } }
                };
                card9.Controls.Add(loadingLabel);
                cardsContainer.Controls.Add(card9, 2, 2);

                // Asenkron AI çağrısı
                var aiPanel = await CreateAIPanelAsync(patientId);
                card9.Controls.Clear();
                
                // Header'ı tekrar ekle (CreateCard içinde oluşturuluyordu ama içeriği temizledik)
                // En iyisi CreateCard mantığını burada tekrar uygulamak veya card9'u temizlememek
                // Ama card9'u temizlemezsek loadingLabel kalır.
                // Basit çözüm: Header'ı yeniden oluştur.
                
                var header = new PanelControl
                {
                    Dock = DockStyle.Top,
                    Height = 36,
                    BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
                };
                header.Paint += (s, e) =>
                {
                    using (var brush = new LinearGradientBrush(
                        new Rectangle(0, 0, header.Width, header.Height),
                        Color.FromArgb(139, 92, 246), Color.FromArgb(169, 122, 255),
                        LinearGradientMode.Horizontal))
                    {
                        e.Graphics.FillRectangle(brush, 0, 0, header.Width, header.Height);
                    }
                };
                var lbl = new LabelControl
                {
                    Text = "🤖 AI Önerileri (Gemini)",
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(10, 8),
                    BackColor = Color.Transparent
                };
                header.Controls.Add(lbl);
                card9.Controls.Add(header);
                
                aiPanel.Dock = DockStyle.Fill;
                card9.Controls.Add(aiPanel);
                aiPanel.BringToFront();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Raporlar yüklenemedi: " + ex.Message, "Hata");
            }
        }

        private PanelControl CreateCard(string title, Color accentColor)
        {
            var card = new PanelControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BackColor = CardBg
            };

            // Header
            var header = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 36,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            header.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, header.Width, header.Height),
                    accentColor, Color.FromArgb(
                        Math.Min(255, accentColor.R + 30),
                        Math.Min(255, accentColor.G + 30),
                        Math.Min(255, accentColor.B + 30)),
                    LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, header.Width, header.Height);
                }
            };

            var lbl = new LabelControl
            {
                Text = title,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 8),
                BackColor = Color.Transparent
            };
            header.Controls.Add(lbl);
            card.Controls.Add(header);

            return card;
        }

        private ChartControl CreateWeightChart(int patientId)
        {
            var chart = new ChartControl { BackColor = CardBg };
            chart.BorderOptions.Visibility = DevExpress.Utils.DefaultBoolean.False;
            chart.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;

            var weights = _reportRepo.GetWeightHistory(patientId, DateTime.MinValue, DateTime.MaxValue);
            var series = new Series("Kilo", ViewType.Spline);
            
            if (weights != null && weights.Count > 0)
            {
                foreach (var w in weights.OrderBy(x => x.Date))
                {
                    series.Points.Add(new SeriesPoint(w.Date.ToString("dd/MM"), w.Weight));
                }
            }
            else
            {
                // Fallback
                var entries = _weightRepo.GetByPatientId(patientId);
                foreach (var we in entries.OrderBy(x => x.Date).Take(10))
                {
                    series.Points.Add(new SeriesPoint(we.Date.ToString("dd/MM"), we.Weight));
                }
            }

            var view = (SplineSeriesView)series.View;
            view.Color = PrimaryColor;
            view.LineStyle.Thickness = 3;
            view.MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
            
            chart.Series.Add(series);
            return chart;
        }

        private PanelControl CreateGoalsPanel(int patientId)
        {
            var panel = new PanelControl
            {
                BackColor = CardBg,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                AutoScroll = true
            };

            var goals = _goalRepo.GetByPatientId(patientId);
            int y = 10;
            
            if (goals == null || !goals.Any())
            {
                var lbl = new LabelControl
                {
                    Text = "Henüz hedef belirlenmemiş.",
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = TextSecondary,
                    Location = new Point(10, y)
                };
                panel.Controls.Add(lbl);
            }
            else
            {
                foreach (var g in goals.Where(x => x.IsActive).Take(5))
                {
                    double progress = g.TargetValue > 0 ? (g.CurrentValue / g.TargetValue * 100) : 0;
                    string status = progress >= 100 ? "✅" : (progress >= 50 ? "🔄" : "⏳");
                    string goalName = GetGoalTypeName(g.GoalType);
                    
                    var lbl = new LabelControl
                    {
                        Text = status + " " + goalName + ": " + g.CurrentValue.ToString("F1") + "/" + g.TargetValue.ToString("F1") + " " + g.Unit,
                        Font = new Font("Segoe UI", 9F),
                        ForeColor = TextPrimary,
                        Location = new Point(10, y),
                        AutoSizeMode = LabelAutoSizeMode.None,
                        Size = new Size(280, 22)
                    };
                    panel.Controls.Add(lbl);
                    y += 28;
                }
            }

            return panel;
        }

        private string GetGoalTypeName(GoalType type)
        {
            if (type == GoalType.Weight) return "Kilo Hedefi";
            if (type == GoalType.Water) return "Su Tüketimi";
            if (type == GoalType.Steps) return "Adım Sayısı";
            if (type == GoalType.Calories) return "Kalori";
            return "Hedef";
        }

        private PanelControl CreateNotesPanel(int patientId)
        {
            var panel = new PanelControl
            {
                BackColor = CardBg,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                AutoScroll = true
            };

            var notes = _noteRepo.GetByPatientId(patientId);
            int y = 10;
            
            if (notes == null || !notes.Any())
            {
                var lbl = new LabelControl
                {
                    Text = "Henüz not eklenmemiş.",
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = TextSecondary,
                    Location = new Point(10, y)
                };
                panel.Controls.Add(lbl);
            }
            else
            {
                foreach (var n in notes.OrderByDescending(x => x.Date).Take(4))
                {
                    string content = n.Content ?? "";
                    if (content.Length > 50) content = content.Substring(0, 50) + "...";
                    
                    var lbl = new LabelControl
                    {
                        Text = "📌 " + n.Date.ToString("dd/MM") + ": " + content,
                        Font = new Font("Segoe UI", 8.5F),
                        ForeColor = TextPrimary,
                        Location = new Point(10, y),
                        AutoSizeMode = LabelAutoSizeMode.None,
                        Size = new Size(280, 36)
                    };
                    panel.Controls.Add(lbl);
                    y += 40;
                }
            }

            return panel;
        }

        private PanelControl CreateBodyMeasurementsPanel(int patientId)
        {
            var panel = new PanelControl
            {
                BackColor = CardBg,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            var measurements = _bodyRepo.GetByPatient(patientId);
            BodyMeasurement latest = null;
            if (measurements != null && measurements.Count > 0)
            {
                latest = measurements.OrderByDescending(m => m.Date).FirstOrDefault();
            }
            
            int y = 10;
            if (latest == null)
            {
                var lbl = new LabelControl
                {
                    Text = "Ölçüm verisi bulunamadı.",
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = TextSecondary,
                    Location = new Point(10, y)
                };
                panel.Controls.Add(lbl);
            }
            else
            {
                string[] items = {
                    "📅 Tarih: " + latest.Date.ToString("dd/MM/yyyy"),
                    "📏 Göğüs: " + latest.Chest + " cm",
                    "📏 Bel: " + latest.Waist + " cm",
                    "📏 Kalça: " + latest.Hip + " cm",
                    "📏 Kol: " + latest.Arm + " cm"
                };
                
                foreach (var item in items)
                {
                    var lbl = new LabelControl
                    {
                        Text = item,
                        Font = new Font("Segoe UI", 9F),
                        ForeColor = TextPrimary,
                        Location = new Point(10, y)
                    };
                    panel.Controls.Add(lbl);
                    y += 26;
                }
            }

            return panel;
        }

        private ChartControl CreateMealAdherenceChart(int patientId)
        {
            var chart = new ChartControl { BackColor = CardBg };
            chart.BorderOptions.Visibility = DevExpress.Utils.DefaultBoolean.False;
            chart.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
            chart.Legend.AlignmentHorizontal = LegendAlignmentHorizontal.Center;
            chart.Legend.AlignmentVertical = LegendAlignmentVertical.BottomOutside;

            var adherence = _reportRepo.GetMealAdherence(patientId);
            var series = new Series("Uyum", ViewType.Pie);
            
            int consumed = 0;
            int skipped = 0;
            
            if (adherence != null && adherence.Count > 0)
            {
                consumed = adherence.Count(a => a.AdherenceScore >= 80);
                skipped = adherence.Count - consumed;
            }
            
            if (consumed > 0 || skipped > 0)
            {
                series.Points.Add(new SeriesPoint("Yenilen", consumed) { Color = SuccessColor });
                series.Points.Add(new SeriesPoint("Atlanan", skipped) { Color = DangerColor });
            }
            else
            {
                series.Points.Add(new SeriesPoint("Veri Yok", 1) { Color = Color.LightGray });
            }
            
            chart.Series.Add(series);
            return chart;
        }

        private PanelControl CreateSummaryPanel(int patientId)
        {
            var panel = new PanelControl
            {
                BackColor = CardBg,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            if (_selectedPatient == null) return panel;
            
            double kiloKaybi = _selectedPatient.BaslangicKilosu - _selectedPatient.GuncelKilo;
            double bmi = _selectedPatient.GuncelKilo / Math.Pow(_selectedPatient.Boy / 100, 2);
            double target = _reportRepo.GetTargetWeight(patientId);
            
            string[] items = {
                "👤 Yaş: " + _selectedPatient.Yas,
                "⚖️ Başlangıç: " + _selectedPatient.BaslangicKilosu + " kg",
                "⚖️ Güncel: " + _selectedPatient.GuncelKilo + " kg",
                "📉 Kayıp: " + kiloKaybi.ToString("F1") + " kg",
                "📊 BMI: " + bmi.ToString("F1"),
                "🎯 Hedef: " + target.ToString("F1") + " kg"
            };
            
            int y = 10;
            foreach (var item in items)
            {
                Color color = item.Contains("Kayıp") && kiloKaybi > 0 ? SuccessColor : TextPrimary;
                var lbl = new LabelControl
                {
                    Text = item,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = color,
                    Location = new Point(10, y)
                };
                panel.Controls.Add(lbl);
                y += 24;
            }

            return panel;
        }

        private ChartControl CreateBMIChart(int patientId)
        {
            var chart = new ChartControl { BackColor = CardBg };
            chart.BorderOptions.Visibility = DevExpress.Utils.DefaultBoolean.False;
            chart.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;

            var weights = _weightRepo.GetByPatientId(patientId);
            var series = new Series("BMI", ViewType.Area);
            
            double height = _selectedPatient != null ? _selectedPatient.Boy : 170;
            foreach (var w in weights.OrderBy(x => x.Date).Take(10))
            {
                double bmi = w.Weight / Math.Pow(height / 100, 2);
                series.Points.Add(new SeriesPoint(w.Date.ToString("dd/MM"), bmi));
            }

            var view = (AreaSeriesView)series.View;
            view.Color = SuccessColor;
            view.Transparency = 120;
            
            chart.Series.Add(series);
            return chart;
        }

        private PanelControl CreateProgressPanel(int patientId)
        {
            var panel = new PanelControl
            {
                BackColor = CardBg,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            if (_selectedPatient == null) return panel;
            
            double kiloKaybi = _selectedPatient.BaslangicKilosu - _selectedPatient.GuncelKilo;
            double target = _reportRepo.GetTargetWeight(patientId);
            double hedefKayip = _selectedPatient.BaslangicKilosu - target;
            double ilerleme = hedefKayip > 0 ? (kiloKaybi / hedefKayip * 100) : 0;
            ilerleme = Math.Min(100, Math.Max(0, ilerleme));
            
            Color progressColor = ilerleme >= 75 ? SuccessColor : (ilerleme >= 50 ? WarningColor : DangerColor);
            
            var lblProgress = new LabelControl
            {
                Text = "İlerleme: %" + ilerleme.ToString("F0"),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = progressColor,
                Location = new Point(10, 20)
            };
            panel.Controls.Add(lblProgress);
            
            var progressBar = new ProgressBarControl
            {
                Location = new Point(10, 50),
                Size = new Size(250, 25)
            };
            progressBar.Properties.Maximum = 100;
            progressBar.Properties.Minimum = 0;
            progressBar.EditValue = (int)ilerleme;
            panel.Controls.Add(progressBar);
            
            double kalan = _selectedPatient.GuncelKilo - target;
            var lblDetail = new LabelControl
            {
                Text = "Hedefe kalan: " + kalan.ToString("F1") + " kg",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextSecondary,
                Location = new Point(10, 85)
            };
            panel.Controls.Add(lblDetail);

            return panel;
        }

        private async Task<PanelControl> CreateAIPanelAsync(int patientId)
        {
            var panel = new PanelControl
            {
                BackColor = CardBg,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                AutoScroll = true
            };

            if (_selectedPatient == null) return panel;
            
            int y = 10;
            
            try
            {
                // Gerçek Gemini API Çağrısı (Yapılandırılmış JSON)
                var aiService = new AiAssistantService();
                var response = await aiService.GetStructuredAnalysisAsync(patientId);
                
                // AI yanıtını sakla (PDF için)
                _lastAiResponse = response;
                
                // Hata kontrolü - fallback yanıtı mı?
                bool isError = response.AnalysisSummary != null && 
                               (response.AnalysisSummary.Contains("ulaşılamıyor") || 
                                response.AnalysisSummary.Contains("Error") ||
                                response.AnalysisSummary.Contains("Hata"));
                
                if (isError)
                {
                    // Hata durumunda kullanıcı dostu mesaj
                    AddAILabel(panel, "⚠️ AI servisine şu anda bağlanılamıyor.", WarningColor, ref y);
                    AddAILabel(panel, "📋 Lütfen internet bağlantınızı kontrol edin.", TextSecondary, ref y);
                    AddAILabel(panel, "🔄 Yenile butonuna basarak tekrar deneyebilirsiniz.", TextSecondary, ref y);
                    
                    if (response.Warnings != null && response.Warnings.Count > 0)
                    {
                        AddAILabel(panel, "Detay: " + response.Warnings[0], TextSecondary, ref y);
                    }
                }
                else
                {
                    // 1. Analiz Özeti
                    if (!string.IsNullOrEmpty(response.AnalysisSummary))
                    {
                        AddAILabel(panel, "📊 " + response.AnalysisSummary, InfoColor, ref y);
                    }

                    // 2. Beslenme Yorumu
                    if (!string.IsNullOrEmpty(response.NutritionComment))
                    {
                        AddAILabel(panel, "🍎 " + response.NutritionComment, SuccessColor, ref y);
                    }

                    // 3. Günlük Öneriler
                    if (response.DailyRecommendations != null)
                    {
                        foreach (var rec in response.DailyRecommendations)
                        {
                            AddAILabel(panel, "💡 " + rec, TextPrimary, ref y);
                        }
                    }

                    // 4. Uyarılar
                    if (response.Warnings != null)
                    {
                        foreach (var warn in response.Warnings)
                        {
                            AddAILabel(panel, "⚠️ " + warn, WarningColor, ref y);
                        }
                    }

                    // 5. Not Önerisi
                    if (!string.IsNullOrEmpty(response.DietitianNoteSuggestion))
                    {
                        AddAILabel(panel, "📝 Not Önerisi: " + response.DietitianNoteSuggestion, TextSecondary, ref y);
                    }
                }
            }
            catch (Exception ex)
            {
                _lastAiResponse = null;
                AddAILabel(panel, "❌ AI analizi sırasında hata oluştu.", DangerColor, ref y);
                AddAILabel(panel, "Hata: " + ex.Message, TextSecondary, ref y);
            }
            
            // Fallback
            if (panel.Controls.Count == 0)
            {
                AddAILabel(panel, "AI analizi tamamlandı ancak içerik oluşturulamadı.", TextSecondary, ref y);
            }

            return panel;
        }

        private void AddAILabel(PanelControl panel, string text, Color color, ref int y)
        {
            var lbl = new LabelControl
            {
                Text = text,
                Font = new Font("Segoe UI", 9F),
                ForeColor = color,
                Location = new Point(10, y),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(panel.Width - 40, 40) // Dinamik genişlik
            };
            lbl.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            lbl.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            
            // Yüksekliği içeriğe göre ayarla (basitçe)
            if (text.Length > 100) lbl.Height = 60;
            if (text.Length > 200) lbl.Height = 80;

            panel.Controls.Add(lbl);
            y += lbl.Height + 5;
        }



        private void ExportPdf()
        {
            if (_selectedPatient == null)
            {
                XtraMessageBox.Show("Lütfen önce bir hasta seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                // HTML rapor oluştur
                string htmlContent = GenerateHtmlReport();
                
                // Geçici HTML dosyası oluştur
                string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), 
                    "Rapor_" + _selectedPatient.AdSoyad.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMdd") + ".html");
                
                System.IO.File.WriteAllText(tempPath, htmlContent, System.Text.Encoding.UTF8);
                
                // Tarayıcıda aç
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });
                
                XtraMessageBox.Show("Rapor tarayıcınızda açıldı.\n\nPDF olarak kaydetmek için:\n• Ctrl+P tuşlarına basın\n• 'PDF olarak kaydet' seçeneğini seçin", 
                    "Rapor Hazır", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Rapor oluşturulurken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private string GenerateHtmlReport()
        {
            int patientId = _selectedPatient.Id;
            double bmi = _selectedPatient.GuncelKilo / Math.Pow(_selectedPatient.Boy / 100, 2);
            double kiloKaybi = _selectedPatient.BaslangicKilosu - _selectedPatient.GuncelKilo;
            double target = _reportRepo.GetTargetWeight(patientId);
            
            var weights = _weightRepo.GetByPatientId(patientId).OrderBy(w => w.Date).ToList();
            var goals = _goalRepo.GetByPatientId(patientId).Where(g => g.IsActive).ToList();
            var notes = _noteRepo.GetByPatientId(patientId).OrderByDescending(n => n.Date).Take(5).ToList();
            var measurements = _bodyRepo.GetByPatient(patientId);
            var latestMeasurement = measurements?.OrderByDescending(m => m.Date).FirstOrDefault();
            
            string html = @"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Hasta Raporu - " + _selectedPatient.AdSoyad + @"</title>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; max-width: 800px; margin: 0 auto; padding: 20px; }
        h1 { color: #16a34a; border-bottom: 2px solid #16a34a; padding-bottom: 10px; }
        h2 { color: #059669; margin-top: 30px; }
        .header { background: linear-gradient(135deg, #16a34a, #22c55e); color: white; padding: 20px; border-radius: 10px; margin-bottom: 20px; }
        .header h1 { color: white; border: none; margin: 0; }
        .info-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 15px; margin: 20px 0; }
        .info-box { background: #f0fdf4; padding: 15px; border-radius: 8px; text-align: center; }
        .info-box .value { font-size: 24px; font-weight: bold; color: #16a34a; }
        .info-box .label { font-size: 12px; color: #666; }
        table { width: 100%; border-collapse: collapse; margin: 15px 0; }
        th, td { padding: 10px; text-align: left; border-bottom: 1px solid #e5e7eb; }
        th { background: #f0fdf4; color: #16a34a; }
        .success { color: #16a34a; }
        .warning { color: #f59e0b; }
        .footer { margin-top: 40px; text-align: center; color: #666; font-size: 12px; }
        @media print { .no-print { display: none; } }
    </style>
</head>
<body>
    <div class='header'>
        <h1>🏥 DiyetPro - Hasta Raporu</h1>
        <p>Tarih: " + DateTime.Now.ToString("dd.MM.yyyy") + @"</p>
    </div>
    
    <h2>👤 Hasta Bilgileri</h2>
    <div class='info-grid'>
        <div class='info-box'>
            <div class='value'>" + _selectedPatient.AdSoyad + @"</div>
            <div class='label'>Ad Soyad</div>
        </div>
        <div class='info-box'>
            <div class='value'>" + _selectedPatient.Yas + @"</div>
            <div class='label'>Yaş</div>
        </div>
        <div class='info-box'>
            <div class='value'>" + _selectedPatient.Boy + @" cm</div>
            <div class='label'>Boy</div>
        </div>
    </div>
    
    <h2>⚖️ Kilo Takibi</h2>
    <div class='info-grid'>
        <div class='info-box'>
            <div class='value'>" + _selectedPatient.BaslangicKilosu + @" kg</div>
            <div class='label'>Başlangıç</div>
        </div>
        <div class='info-box'>
            <div class='value'>" + _selectedPatient.GuncelKilo + @" kg</div>
            <div class='label'>Güncel</div>
        </div>
        <div class='info-box'>
            <div class='value " + (kiloKaybi > 0 ? "success" : "warning") + @"'>" + kiloKaybi.ToString("F1") + @" kg</div>
            <div class='label'>Kayıp</div>
        </div>
    </div>
    <div class='info-grid'>
        <div class='info-box'>
            <div class='value'>" + bmi.ToString("F1") + @"</div>
            <div class='label'>BMI</div>
        </div>
        <div class='info-box'>
            <div class='value'>" + target.ToString("F1") + @" kg</div>
            <div class='label'>Hedef Kilo</div>
        </div>
        <div class='info-box'>
            <div class='value'>" + (_selectedPatient.GuncelKilo - target).ToString("F1") + @" kg</div>
            <div class='label'>Hedefe Kalan</div>
        </div>
    </div>";

            // Hedefler
            if (goals.Any())
            {
                html += @"
    <h2>🎯 Aktif Hedefler</h2>
    <table>
        <tr><th>Hedef</th><th>Mevcut</th><th>Hedef</th><th>Birim</th></tr>";
                foreach (var g in goals)
                {
                    html += "<tr><td>" + GetGoalTypeName(g.GoalType) + "</td><td>" + g.CurrentValue.ToString("F1") + "</td><td>" + g.TargetValue.ToString("F1") + "</td><td>" + g.Unit + "</td></tr>";
                }
                html += "</table>";
            }

            // Vücut Ölçüleri
            if (latestMeasurement != null)
            {
                html += @"
    <h2>📏 Son Vücut Ölçüleri (" + latestMeasurement.Date.ToString("dd.MM.yyyy") + @")</h2>
    <div class='info-grid'>
        <div class='info-box'><div class='value'>" + latestMeasurement.Chest + @" cm</div><div class='label'>Göğüs</div></div>
        <div class='info-box'><div class='value'>" + latestMeasurement.Waist + @" cm</div><div class='label'>Bel</div></div>
        <div class='info-box'><div class='value'>" + latestMeasurement.Hip + @" cm</div><div class='label'>Kalça</div></div>
    </div>";
            }

            // Notlar
            if (notes.Any())
            {
                html += @"
    <h2>📝 Son Notlar</h2>
    <table>
        <tr><th>Tarih</th><th>Not</th></tr>";
                foreach (var n in notes)
                {
                    html += "<tr><td>" + n.Date.ToString("dd.MM.yyyy") + "</td><td>" + (n.Content ?? "") + "</td></tr>";
                }
                html += "</table>";
            }

            // Kilo Geçmişi
            if (weights.Any())
            {
                html += @"
    <h2>📊 Kilo Geçmişi (Son 10 Kayıt)</h2>
    <table>
        <tr><th>Tarih</th><th>Kilo</th><th>Not</th></tr>";
                foreach (var w in weights.Skip(Math.Max(0, weights.Count - 10)))
                {
                    html += "<tr><td>" + w.Date.ToString("dd.MM.yyyy") + "</td><td>" + w.Weight + " kg</td><td>" + (w.Notes ?? "") + "</td></tr>";
                }
                html += "</table>";
            }

            // AI Önerileri
            if (_lastAiResponse != null && !string.IsNullOrEmpty(_lastAiResponse.AnalysisSummary) 
                && !_lastAiResponse.AnalysisSummary.Contains("ulaşılamıyor"))
            {
                html += @"
    <h2>🤖 AI Destekli Analiz ve Öneriler</h2>
    <div style='background: linear-gradient(135deg, #8b5cf6, #a78bfa); padding: 15px; border-radius: 10px; color: white; margin-bottom: 15px;'>
        <p style='font-weight: bold; margin: 0 0 10px 0;'>📊 Genel Değerlendirme</p>
        <p style='margin: 0;'>" + System.Net.WebUtility.HtmlEncode(_lastAiResponse.AnalysisSummary) + @"</p>
    </div>";

                if (!string.IsNullOrEmpty(_lastAiResponse.NutritionComment))
                {
                    html += @"
    <div style='background: #f0fdf4; padding: 15px; border-radius: 8px; border-left: 4px solid #16a34a; margin-bottom: 15px;'>
        <p style='font-weight: bold; color: #16a34a; margin: 0 0 5px 0;'>🍎 Beslenme Analizi</p>
        <p style='margin: 0; color: #333;'>" + System.Net.WebUtility.HtmlEncode(_lastAiResponse.NutritionComment) + @"</p>
    </div>";
                }

                if (_lastAiResponse.DailyRecommendations != null && _lastAiResponse.DailyRecommendations.Count > 0)
                {
                    html += @"
    <div style='background: #eff6ff; padding: 15px; border-radius: 8px; border-left: 4px solid #2563eb; margin-bottom: 15px;'>
        <p style='font-weight: bold; color: #2563eb; margin: 0 0 10px 0;'>💡 Günlük Öneriler</p>
        <ul style='margin: 0; padding-left: 20px; color: #333;'>";
                    foreach (var rec in _lastAiResponse.DailyRecommendations)
                    {
                        html += "<li>" + System.Net.WebUtility.HtmlEncode(rec) + "</li>";
                    }
                    html += @"</ul>
    </div>";
                }

                if (_lastAiResponse.Warnings != null && _lastAiResponse.Warnings.Count > 0)
                {
                    html += @"
    <div style='background: #fffbeb; padding: 15px; border-radius: 8px; border-left: 4px solid #f59e0b; margin-bottom: 15px;'>
        <p style='font-weight: bold; color: #f59e0b; margin: 0 0 10px 0;'>⚠️ Dikkat Edilmesi Gerekenler</p>
        <ul style='margin: 0; padding-left: 20px; color: #333;'>";
                    foreach (var warn in _lastAiResponse.Warnings)
                    {
                        html += "<li>" + System.Net.WebUtility.HtmlEncode(warn) + "</li>";
                    }
                    html += @"</ul>
    </div>";
                }

                if (!string.IsNullOrEmpty(_lastAiResponse.DietitianNoteSuggestion))
                {
                    html += @"
    <div style='background: #faf5ff; padding: 15px; border-radius: 8px; border-left: 4px solid #9333ea; margin-bottom: 15px;'>
        <p style='font-weight: bold; color: #9333ea; margin: 0 0 5px 0;'>📝 Diyetisyen Notu Önerisi</p>
        <p style='margin: 0; color: #333; font-style: italic;'>" + System.Net.WebUtility.HtmlEncode(_lastAiResponse.DietitianNoteSuggestion) + @"</p>
    </div>";
                }

                html += @"
    <p style='font-size: 11px; color: #888; text-align: center;'>Bu AI analizi Gemini tarafından oluşturulmuştur. Son karar diyetisyene aittir.</p>";
            }

            html += @"
    <div class='footer'>
        <p>Bu rapor DiyetPro uygulaması tarafından otomatik olarak oluşturulmuştur.</p>
        <p>Dr. Sudenur Öztürk - Beslenme ve Diyetetik Uzmanı</p>
    </div>
</body>
</html>";

            return html;
        }
    }
}
