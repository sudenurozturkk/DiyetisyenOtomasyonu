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
using DiyetisyenOtomasyonu.Infrastructure.Services;
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    /// <summary>
    /// Egzersiz Görev Yönetimi - Geniş ve Modern Tasarım
    /// Üstte: Form + Özet Kartları, Altta: Geniş Tablo
    /// </summary>
    public partial class FrmExerciseManager : XtraForm
    {
        private readonly ExerciseTaskRepository _taskRepository;
        private readonly PatientService _patientService;
        private List<ExerciseTask> _allTasks;
        private List<ExerciseTask> _filteredTasks;

        #region Colors
        private readonly Color PrimaryGreen = ColorTranslator.FromHtml("#0D9488");
        private readonly Color LightGreen = ColorTranslator.FromHtml("#CCFBF1");
        private readonly Color SuccessGreen = ColorTranslator.FromHtml("#22C55E");
        private readonly Color SuccessBg = ColorTranslator.FromHtml("#DCFCE7");
        private readonly Color DangerRed = ColorTranslator.FromHtml("#EF4444");
        private readonly Color DangerBg = ColorTranslator.FromHtml("#FEE2E2");
        private readonly Color WarningOrange = ColorTranslator.FromHtml("#F97316");
        private readonly Color WarningBg = ColorTranslator.FromHtml("#FFEDD5");
        private readonly Color GoldColor = ColorTranslator.FromHtml("#EAB308");
        private readonly Color GoldBg = ColorTranslator.FromHtml("#FEF9C3");
        private readonly Color InfoBlue = ColorTranslator.FromHtml("#3B82F6");
        private readonly Color InfoBg = ColorTranslator.FromHtml("#DBEAFE");
        
        private readonly Color BackgroundLight = ColorTranslator.FromHtml("#F8FAFC");
        private readonly Color CardWhite = Color.White;
        private readonly Color BorderGray = ColorTranslator.FromHtml("#E2E8F0");
        private readonly Color InputBorder = ColorTranslator.FromHtml("#CBD5E1");
        
        private readonly Color TextDark = ColorTranslator.FromHtml("#1E293B");
        private readonly Color TextMedium = ColorTranslator.FromHtml("#64748B");
        #endregion

        #region Controls
        private ComboBoxEdit cmbHasta, cmbZorluk, cmbFilterHasta, cmbFilterDurum;
        private TextEdit txtGorevAdi;
        private MemoEdit txtAciklama;
        private SpinEdit spnSure;
        private DateEdit dtTarih;
        private GridControl gridTasks;
        private GridView viewTasks;
        
        // Summary labels (seçilen hastaya göre)
        private Label lblTotalTasks, lblCompletedTasks, lblPendingTasks, lblSuccessRate;
        private Label lblPatientStats, lblTaskCount;
        #endregion

        public FrmExerciseManager()
        {
            InitializeComponent();
            _taskRepository = new ExerciseTaskRepository();
            _patientService = new PatientService();
            SetupUI();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1200, 750);
            this.Name = "FrmExerciseManager";
            this.Text = "Egzersiz Görev Yönetimi";
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = BackgroundLight;
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            this.Padding = new Padding(15);

            // Ana dikey layout - daha kompakt
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 175)); // Üst: Form + Kartlar (daha kompact)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // Orta: Filtreler
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Alt: Grid

            mainLayout.Controls.Add(CreateTopSection(), 0, 0);
            mainLayout.Controls.Add(CreateFilterSection(), 0, 1);
            mainLayout.Controls.Add(CreateGridSection(), 0, 2);

            this.Controls.Add(mainLayout);
        }

        #region Top Section - Form + Summary Cards
        private Panel CreateTopSection()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            // Sol: Form paneli
            var formCard = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(520, 200),
                BackColor = CardWhite
            };
            formCard.Paint += (s, e) => DrawRoundedBorder(e.Graphics, formCard, 12);
            CreateFormContent(formCard);
            panel.Controls.Add(formCard);

            // Sağ: Özet kartları
            var statsPanel = new Panel
            {
                Location = new Point(545, 0),
                Size = new Size(620, 200),
                BackColor = Color.Transparent
            };
            CreateStatsCards(statsPanel);
            panel.Controls.Add(statsPanel);

            // Resize handler
            panel.Resize += (s, e) =>
            {
                int formWidth = Math.Max(450, (int)(panel.Width * 0.42));
                formCard.Size = new Size(formWidth, 200);
                statsPanel.Location = new Point(formWidth + 25, 0);
                statsPanel.Size = new Size(panel.Width - formWidth - 25, 200);
            };

            return panel;
        }

        private void CreateFormContent(Panel card)
        {
            int x = 20, y = 15;

            // Başlık
            var lblTitle = new Label
            {
                Text = "📋 Yeni Egzersiz Görevi Ekle",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(x, y),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);
            y += 35;

            // Satır 1: Hasta + Görev Adı
            AddFormLabel(card, "Hasta:", x, y);
            cmbHasta = new ComboBoxEdit { Location = new Point(x, y + 18), Size = new Size(160, 26) };
            card.Controls.Add(cmbHasta);

            AddFormLabel(card, "Görev Adı:", x + 175, y);
            txtGorevAdi = new TextEdit
            {
                Location = new Point(x + 175, y + 18),
                Size = new Size(200, 26),
                Properties = { NullText = "30 dakika yürüyüş" }
            };
            card.Controls.Add(txtGorevAdi);

            AddFormLabel(card, "Açıklama:", x + 390, y);
            txtAciklama = new MemoEdit
            {
                Location = new Point(x + 390, y + 18),
                Size = new Size(100, 26),
                Properties = { NullText = "Detay..." }
            };
            card.Controls.Add(txtAciklama);
            y += 52;

            // Satır 2: Süre + Zorluk + Tarih
            AddFormLabel(card, "Süre:", x, y);
            spnSure = new SpinEdit { Location = new Point(x, y + 18), Size = new Size(60, 26) };
            spnSure.Properties.MinValue = 5;
            spnSure.Properties.MaxValue = 180;
            spnSure.EditValue = 30;
            card.Controls.Add(spnSure);

            var lblDk = new Label
            {
                Text = "dk",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMedium,
                Location = new Point(x + 65, y + 22),
                AutoSize = true
            };
            card.Controls.Add(lblDk);

            AddFormLabel(card, "Zorluk:", x + 100, y);
            cmbZorluk = new ComboBoxEdit { Location = new Point(x + 100, y + 18), Size = new Size(120, 26) };
            cmbZorluk.Properties.Items.AddRange(new[] { "1 - Kolay", "2 - Hafif", "3 - Orta", "4 - Zor", "5 - Çok Zor" });
            cmbZorluk.SelectedIndex = 2;
            card.Controls.Add(cmbZorluk);

            AddFormLabel(card, "Tarih:", x + 235, y);
            dtTarih = new DateEdit { Location = new Point(x + 235, y + 18), Size = new Size(130, 26) };
            dtTarih.EditValue = DateTime.Today.AddDays(1);
            card.Controls.Add(dtTarih);

            // Ekle Butonu
            var btnEkle = new SimpleButton
            {
                Text = "+ GÖREV ATA",
                Location = new Point(x + 380, y + 15),
                Size = new Size(110, 32),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryGreen, ForeColor = Color.White, BorderColor = PrimaryGreen }
            };
            btnEkle.Click += BtnAdd_Click;
            card.Controls.Add(btnEkle);

            // Hastaları yükle
            LoadPatients();
        }

        private void CreateStatsCards(Panel parent)
        {
            // Seçilen hasta bilgisi
            lblPatientStats = new Label
            {
                Text = "📊 Tüm Hastalar - Genel İstatistikler",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(0, 0),
                AutoSize = true
            };
            parent.Controls.Add(lblPatientStats);

            // 4 kart yan yana
            int cardY = 35;
            int cardWidth = 140;
            int cardHeight = 100;
            int gap = 15;

            // Kart 1: Toplam
            lblTotalTasks = new Label { Text = "0", Font = new Font("Segoe UI", 24F, FontStyle.Bold), ForeColor = TextDark };
            parent.Controls.Add(CreateStatCard("📋", "Toplam Görev", lblTotalTasks, PrimaryGreen, LightGreen, 0, cardY, cardWidth, cardHeight));

            // Kart 2: Tamamlanan
            lblCompletedTasks = new Label { Text = "0", Font = new Font("Segoe UI", 24F, FontStyle.Bold), ForeColor = TextDark };
            parent.Controls.Add(CreateStatCard("✓", "Tamamlandı", lblCompletedTasks, SuccessGreen, SuccessBg, cardWidth + gap, cardY, cardWidth, cardHeight));

            // Kart 3: Bekliyor
            lblPendingTasks = new Label { Text = "0", Font = new Font("Segoe UI", 24F, FontStyle.Bold), ForeColor = TextDark };
            parent.Controls.Add(CreateStatCard("⏳", "Bekliyor", lblPendingTasks, WarningOrange, WarningBg, (cardWidth + gap) * 2, cardY, cardWidth, cardHeight));

            // Kart 4: Başarı Oranı
            lblSuccessRate = new Label { Text = "%0", Font = new Font("Segoe UI", 24F, FontStyle.Bold), ForeColor = PrimaryGreen };
            parent.Controls.Add(CreateStatCard("🏆", "Başarı Oranı", lblSuccessRate, GoldColor, GoldBg, (cardWidth + gap) * 3, cardY, cardWidth, cardHeight));
        }

        private Panel CreateStatCard(string icon, string title, Label valueLabel, Color iconColor, Color bgColor, int x, int y, int width, int height)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = CardWhite
            };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 10);

            // Icon
            var iconPanel = new Panel { Location = new Point(12, 12), Size = new Size(36, 36), BackColor = Color.Transparent };
            iconPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(bgColor))
                    e.Graphics.FillEllipse(brush, 0, 0, 35, 35);
                using (var font = new Font("Segoe UI", 12F))
                using (var brush = new SolidBrush(iconColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(icon, font, brush, new RectangleF(0, 0, 36, 36), sf);
            };
            card.Controls.Add(iconPanel);

            // Title
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextMedium,
                Location = new Point(52, 12),
                Size = new Size(80, 30)
            };
            card.Controls.Add(lblTitle);

            // Value
            valueLabel.Location = new Point(12, 55);
            valueLabel.AutoSize = true;
            card.Controls.Add(valueLabel);

            return card;
        }
        #endregion

        #region Filter Section
        private Panel CreateFilterSection()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardWhite,
                Padding = new Padding(20, 12, 20, 12)
            };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 10);

            int x = 20;

            // Hasta Filtresi
            var lblFilterHasta = new Label
            {
                Text = "Hasta:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMedium,
                Location = new Point(x, 16),
                AutoSize = true
            };
            panel.Controls.Add(lblFilterHasta);

            cmbFilterHasta = new ComboBoxEdit { Location = new Point(x + 50, 12), Size = new Size(180, 28) };
            cmbFilterHasta.Properties.NullText = "Tüm Hastalar";
            cmbFilterHasta.SelectedIndexChanged += (s, e) => FilterTasks();
            panel.Controls.Add(cmbFilterHasta);
            x += 250;

            // Durum Filtresi
            var lblFilterDurum = new Label
            {
                Text = "Durum:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMedium,
                Location = new Point(x, 16),
                AutoSize = true
            };
            panel.Controls.Add(lblFilterDurum);

            cmbFilterDurum = new ComboBoxEdit { Location = new Point(x + 55, 12), Size = new Size(140, 28) };
            cmbFilterDurum.Properties.Items.AddRange(new[] { "Tüm Durumlar", "Bekliyor", "Tamamlandı" });
            cmbFilterDurum.SelectedIndex = 0;
            cmbFilterDurum.SelectedIndexChanged += (s, e) => FilterTasks();
            panel.Controls.Add(cmbFilterDurum);
            x += 220;

            // Aksiyon Butonları (sağ tarafta)
            var btnTamamlandi = new SimpleButton
            {
                Text = "✓ Tamamlandı",
                Location = new Point(x, 10),
                Size = new Size(110, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = SuccessGreen, ForeColor = Color.White, BorderColor = SuccessGreen }
            };
            btnTamamlandi.Click += BtnComplete_Click;
            panel.Controls.Add(btnTamamlandi);
            x += 120;

            var btnSifirla = new SimpleButton
            {
                Text = "↻ Yenile",
                Location = new Point(x, 10),
                Size = new Size(80, 32),
                Font = new Font("Segoe UI", 9F),
                Appearance = { BackColor = CardWhite, ForeColor = TextMedium, BorderColor = InputBorder }
            };
            btnSifirla.Click += (s, e) => LoadData();
            panel.Controls.Add(btnSifirla);
            x += 90;

            var btnSil = new SimpleButton
            {
                Text = "🗑 Sil",
                Location = new Point(x, 10),
                Size = new Size(70, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = DangerRed, ForeColor = Color.White, BorderColor = DangerRed }
            };
            btnSil.Click += BtnDelete_Click;
            panel.Controls.Add(btnSil);

            return panel;
        }
        #endregion

        #region Grid Section
        private Panel CreateGridSection()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardWhite,
                Padding = new Padding(0)
            };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 12);

            // Başlık
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(20, 12, 20, 12)
            };

            var lblTitle = new Label
            {
                Text = "📋 Egzersiz Görev Listesi",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(20, 12),
                AutoSize = true
            };
            header.Controls.Add(lblTitle);

            lblTaskCount = new Label
            {
                Text = "0 görev listeleniyor",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMedium,
                Location = new Point(200, 14),
                AutoSize = true
            };
            header.Controls.Add(lblTaskCount);

            panel.Controls.Add(header);

            // Grid
            gridTasks = new GridControl { Dock = DockStyle.Fill };
            viewTasks = new GridView(gridTasks)
            {
                OptionsBehavior = { Editable = false },
                OptionsView = { ShowGroupPanel = false, ShowIndicator = false, ColumnAutoWidth = true, EnableAppearanceEvenRow = true },
                RowHeight = 45
            };
            viewTasks.Appearance.Row.Font = new Font("Segoe UI", 10F);
            viewTasks.Appearance.EvenRow.BackColor = Color.FromArgb(250, 251, 252);
            viewTasks.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            viewTasks.Appearance.HeaderPanel.ForeColor = TextMedium;

            SetupGridColumns();
            gridTasks.MainView = viewTasks;

            // Renk stilleri
            viewTasks.RowCellStyle += (s, e) =>
            {
                if (e.Column.FieldName == "DifficultyText")
                {
                    var diff = e.CellValue?.ToString() ?? "";
                    if (diff.Contains("Kolay") || diff.Contains("Hafif"))
                    {
                        e.Appearance.BackColor = SuccessBg;
                        e.Appearance.ForeColor = SuccessGreen;
                    }
                    else if (diff.Contains("Orta"))
                    {
                        e.Appearance.BackColor = WarningBg;
                        e.Appearance.ForeColor = WarningOrange;
                    }
                    else if (diff.Contains("Zor"))
                    {
                        e.Appearance.BackColor = DangerBg;
                        e.Appearance.ForeColor = DangerRed;
                    }
                    e.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                if (e.Column.FieldName == "ProgressStatus")
                {
                    var status = e.CellValue?.ToString() ?? "";
                    if (status.Contains("Tamamlandı"))
                    {
                        e.Appearance.BackColor = SuccessBg;
                        e.Appearance.ForeColor = SuccessGreen;
                    }
                    else if (status.Contains("Devam"))
                    {
                        e.Appearance.BackColor = WarningBg;
                        e.Appearance.ForeColor = WarningOrange;
                    }
                    else
                    {
                        e.Appearance.BackColor = InfoBg;
                        e.Appearance.ForeColor = InfoBlue;
                    }
                    e.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
            };

            // Önce grid ekle, sonra header ekle (z-order için)
            panel.Controls.Add(gridTasks);
            gridTasks.BringToFront();
            header.BringToFront();

            return panel;
        }

        private void SetupGridColumns()
        {
            viewTasks.Columns.Clear();
            viewTasks.Columns.Add(new GridColumn { FieldName = "PatientName", Caption = "Hasta", Visible = true, Width = 130 });
            viewTasks.Columns.Add(new GridColumn { FieldName = "Title", Caption = "Görev Adı", Visible = true, Width = 160 });
            viewTasks.Columns.Add(new GridColumn { FieldName = "DurationMinutes", Caption = "Hedef (dk)", Visible = true, Width = 70 });
            viewTasks.Columns.Add(new GridColumn { FieldName = "CompletedDuration", Caption = "Yapılan (dk)", Visible = true, Width = 80 });
            viewTasks.Columns.Add(new GridColumn { FieldName = "ProgressPercentage", Caption = "İlerleme %", Visible = true, Width = 75 });
            viewTasks.Columns.Add(new GridColumn { FieldName = "DifficultyText", Caption = "Zorluk", Visible = true, Width = 80 });
            viewTasks.Columns.Add(new GridColumn { FieldName = "DueDate", Caption = "Tarih", Visible = true, Width = 90, DisplayFormat = { FormatType = DevExpress.Utils.FormatType.DateTime, FormatString = "dd.MM.yyyy" } });
            viewTasks.Columns.Add(new GridColumn { FieldName = "ProgressStatus", Caption = "Durum", Visible = true, Width = 95 });
        }
        #endregion

        #region Helpers
        private void DrawRoundedBorder(Graphics g, Panel panel, int radius)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = CreateRoundedRect(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), radius))
            using (var pen = new Pen(BorderGray, 1))
            {
                g.DrawPath(pen, path);
            }
        }

        private GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void AddFormLabel(Panel parent, string text, int x, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextMedium,
                Location = new Point(x, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);
        }

        private void LoadPatients()
        {
            try
            {
                var patients = _patientService.GetPatientsByDoctor(AuthContext.UserId);
                cmbHasta.Properties.Items.Clear();
                
                if (cmbFilterHasta != null)
                {
                    cmbFilterHasta.Properties.Items.Clear();
                    cmbFilterHasta.Properties.Items.Add(new PatientItem { Id = 0, Name = "Tüm Hastalar" });
                }
                
                foreach (var p in patients)
                {
                    var item = new PatientItem { Id = p.Id, Name = p.AdSoyad };
                    cmbHasta.Properties.Items.Add(item);
                    
                    if (cmbFilterHasta != null)
                    {
                        cmbFilterHasta.Properties.Items.Add(item);
                    }
                }
                if (cmbHasta.Properties.Items.Count > 0)
                    cmbHasta.SelectedIndex = 0;
                    
                if (cmbFilterHasta != null && cmbFilterHasta.Properties.Items.Count > 0)
                    cmbFilterHasta.SelectedIndex = 0;
            }
            catch { }
        }
        #endregion

        #region Data Operations
        private void LoadData()
        {
            try
            {
                _allTasks = _taskRepository.GetByDoctor(AuthContext.UserId).ToList();
                _filteredTasks = _allTasks;
                gridTasks.DataSource = _filteredTasks;
                UpdateSummary();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterTasks()
        {
            try
            {
                _filteredTasks = _allTasks.ToList();

                // Hasta filtresi
                var selectedPatient = cmbFilterHasta.EditValue as PatientItem;
                if (selectedPatient != null && selectedPatient.Id > 0)
                {
                    _filteredTasks = _filteredTasks.Where(t => t.PatientId == selectedPatient.Id).ToList();
                }

                // Durum filtresi
                var status = cmbFilterDurum.Text;
                if (status == "Tamamlandı")
                    _filteredTasks = _filteredTasks.Where(t => t.IsCompleted).ToList();
                else if (status == "Bekliyor")
                    _filteredTasks = _filteredTasks.Where(t => !t.IsCompleted).ToList();

                gridTasks.DataSource = _filteredTasks;
                UpdateSummary();
            }
            catch { }
        }

        private void UpdateSummary()
        {
            // Seçilen hastaya göre istatistikleri hesapla
            var selectedPatient = cmbFilterHasta?.EditValue as PatientItem;
            List<ExerciseTask> statsSource;
            
            if (selectedPatient != null && selectedPatient.Id > 0)
            {
                // Belirli hasta seçildi
                statsSource = _allTasks.Where(t => t.PatientId == selectedPatient.Id).ToList();
                lblPatientStats.Text = $"📊 {selectedPatient.Name} - Kişisel İstatistikler";
            }
            else
            {
                // Tüm hastalar
                statsSource = _allTasks;
                lblPatientStats.Text = "📊 Tüm Hastalar - Genel İstatistikler";
            }

            int total = statsSource.Count;
            int completed = statsSource.Count(t => t.IsCompleted);
            int pending = statsSource.Count(t => !t.IsCompleted);
            int rate = total > 0 ? (int)((double)completed / total * 100) : 0;

            lblTotalTasks.Text = total.ToString();
            lblCompletedTasks.Text = completed.ToString();
            lblPendingTasks.Text = pending.ToString();
            lblSuccessRate.Text = "%" + rate;
            lblTaskCount.Text = _filteredTasks.Count + " görev listeleniyor";
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedPatient = cmbHasta.EditValue as PatientItem;
                if (selectedPatient == null)
                {
                    XtraMessageBox.Show("Lütfen hasta seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtGorevAdi.Text))
                {
                    XtraMessageBox.Show("Görev adı boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var task = new ExerciseTask
                {
                    PatientId = selectedPatient.Id,
                    DoctorId = AuthContext.UserId,
                    Title = txtGorevAdi.Text.Trim(),
                    Description = txtAciklama.Text,
                    DurationMinutes = Convert.ToInt32(spnSure.Value),
                    DifficultyLevel = cmbZorluk.SelectedIndex + 1,
                    DueDate = (DateTime)dtTarih.EditValue,
                    IsCompleted = false,
                    CreatedAt = DateTime.Now
                };

                _taskRepository.Add(task);
                ToastNotification.ShowSuccess("Görev başarıyla atandı!");
                ClearInputs();
                LoadData();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            var task = GetSelectedTask();
            if (task == null) return;

            task.IsCompleted = true;
            task.CompletedAt = DateTime.Now;
            _taskRepository.Update(task);
            ToastNotification.ShowSuccess("Görev tamamlandı!");
            LoadData();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var task = GetSelectedTask();
            if (task == null) return;

            if (XtraMessageBox.Show("Bu görevi silmek istediğinize emin misiniz?", "Onay",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _taskRepository.Delete(task.Id);
                ToastNotification.ShowInfo("Görev silindi.");
                LoadData();
            }
        }

        private ExerciseTask GetSelectedTask()
        {
            var rowHandle = viewTasks.FocusedRowHandle;
            if (rowHandle < 0)
            {
                XtraMessageBox.Show("Lütfen bir görev seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
            return viewTasks.GetRow(rowHandle) as ExerciseTask;
        }

        private void ClearInputs()
        {
            txtGorevAdi.Text = "";
            txtAciklama.Text = "";
            spnSure.Value = 30;
            cmbZorluk.SelectedIndex = 2;
            dtTarih.EditValue = DateTime.Today.AddDays(1);
        }
        #endregion

        private class PatientItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public override string ToString() => Name;
        }
    }
}
