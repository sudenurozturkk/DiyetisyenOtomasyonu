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
    /// Randevu Yönetimi - Mockup Tasarımına Tam Uyumlu
    /// </summary>
    public partial class FrmAppointments : XtraForm
    {
        private readonly AppointmentRepository _repository;
        private readonly PatientService _patientService;
        private List<Appointment> _appointments;

        #region Colors
        private readonly Color PrimaryGreen = ColorTranslator.FromHtml("#0D9488");
        private readonly Color DarkGreen = ColorTranslator.FromHtml("#0F766E");
        private readonly Color LightGreen = ColorTranslator.FromHtml("#CCFBF1");
        
        private readonly Color SuccessGreen = ColorTranslator.FromHtml("#22C55E");
        private readonly Color SuccessBg = ColorTranslator.FromHtml("#DCFCE7");
        private readonly Color DangerRed = ColorTranslator.FromHtml("#EF4444");
        private readonly Color DangerBg = ColorTranslator.FromHtml("#FEE2E2");
        private readonly Color WarningOrange = ColorTranslator.FromHtml("#F97316");
        private readonly Color WarningBg = ColorTranslator.FromHtml("#FFEDD5");
        private readonly Color InfoBlue = ColorTranslator.FromHtml("#3B82F6");
        private readonly Color InfoBg = ColorTranslator.FromHtml("#DBEAFE");
        
        private readonly Color BackgroundLight = ColorTranslator.FromHtml("#F8FAFC");
        private readonly Color CardWhite = Color.White;
        private readonly Color BorderGray = ColorTranslator.FromHtml("#E2E8F0");
        private readonly Color InputBorder = ColorTranslator.FromHtml("#CBD5E1");
        
        private readonly Color TextDark = ColorTranslator.FromHtml("#1E293B");
        private readonly Color TextMedium = ColorTranslator.FromHtml("#64748B");
        private readonly Color TextLight = ColorTranslator.FromHtml("#94A3B8");
        #endregion

        #region Form Controls
        private ComboBoxEdit cmbHasta, cmbTur, cmbFilterHasta, cmbFilterTur, cmbFilterDurum;
        private DateEdit dtTarih, dtFilterStart, dtFilterEnd;
        private TimeEdit timeSaat;
        private SpinEdit spnUcret;
        private MemoEdit txtNot;
        private GridControl gridAppointments;
        private GridView viewAppointments;
        
        // Summary labels
        private Label lblPending, lblToday, lblCompleted, lblOnlineRate;
        #endregion

        public FrmAppointments()
        {
            InitializeComponent();
            _repository = new AppointmentRepository();
            _patientService = new PatientService();
            SetupUI();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1100, 700);
            this.Name = "FrmAppointments";
            this.Text = "Randevu Yönetimi";
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = BackgroundLight;
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            this.Padding = new Padding(20);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Color.Transparent
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));  // Yeni randevu form (tek satır)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 85));  // Özet kartları
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));  // Filtre satırı
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));  // Aksiyon butonları
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Grid

            mainLayout.Controls.Add(CreateNewAppointmentSection(), 0, 0);
            mainLayout.Controls.Add(CreateSummaryCards(), 0, 1);
            mainLayout.Controls.Add(CreateFilterBar(), 0, 2);
            mainLayout.Controls.Add(CreateActionButtons(), 0, 3);
            mainLayout.Controls.Add(CreateGrid(), 0, 4);

            this.Controls.Add(mainLayout);
        }

        #region New Appointment Section
        private Panel CreateNewAppointmentSection()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardWhite,
                Padding = new Padding(10)
            };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 12);

            // Title
            var lblTitle = new Label
            {
                Text = "+ Yeni Randevu",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(15, 25),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            int x = 140;
            int y = 20;

            // Hasta
            AddFormLabel(panel, "Hasta:", x, y - 2);
            cmbHasta = new ComboBoxEdit { Location = new Point(x + 45, y - 2), Size = new Size(110, 24) };
            cmbHasta.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            panel.Controls.Add(cmbHasta);

            // Tarih
            AddFormLabel(panel, "Tarih:", x + 165, y - 2);
            dtTarih = new DateEdit { Location = new Point(x + 205, y - 2), Size = new Size(95, 24) };
            dtTarih.EditValue = DateTime.Today;
            panel.Controls.Add(dtTarih);

            // Saat
            AddFormLabel(panel, "Saat:", x + 310, y - 2);
            timeSaat = new TimeEdit { Location = new Point(x + 345, y - 2), Size = new Size(65, 24) };
            timeSaat.EditValue = new DateTime(2000, 1, 1, 10, 30, 0);
            panel.Controls.Add(timeSaat);

            // Tür
            AddFormLabel(panel, "Tür:", x + 420, y - 2);
            cmbTur = new ComboBoxEdit { Location = new Point(x + 450, y - 2), Size = new Size(75, 24) };
            cmbTur.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cmbTur.Properties.Items.AddRange(new[] { "Online", "Klinik" });
            cmbTur.SelectedIndex = 0;
            panel.Controls.Add(cmbTur);

            // Ücret
            AddFormLabel(panel, "Ücret:", x + 535, y - 2);
            spnUcret = new SpinEdit { Location = new Point(x + 575, y - 2), Size = new Size(70, 24) };
            spnUcret.Properties.MinValue = 0;
            spnUcret.Properties.MaxValue = 10000;
            spnUcret.EditValue = 500;
            panel.Controls.Add(spnUcret);

            // Ekle Button
            var btnEkle = new SimpleButton
            {
                Text = "+ Ekle",
                Location = new Point(x + 655, y - 4),
                Size = new Size(70, 28),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryGreen, ForeColor = Color.White, BorderColor = PrimaryGreen }
            };
            btnEkle.Click += BtnAdd_Click;
            panel.Controls.Add(btnEkle);

            // Hidden note field
            txtNot = new MemoEdit { Visible = false };
            panel.Controls.Add(txtNot);

            return panel;
        }
        #endregion

        #region Summary Cards
        private Panel CreateSummaryCards()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0, 10, 0, 0) };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            // Kart 1: Bekleyen Talepler
            lblPending = new Label();
            flow.Controls.Add(CreateSummaryCard("📅", "Bekleyen\nTalepler", lblPending, PrimaryGreen, LightGreen));

            // Kart 2: Bugünkü Randevular
            lblToday = new Label();
            flow.Controls.Add(CreateSummaryCard("📅", "Bugünkü\nRandevular", lblToday, WarningOrange, WarningBg));

            // Kart 3: Toplam Tamamlanan
            lblCompleted = new Label();
            flow.Controls.Add(CreateSummaryCard("✓", "Toplam\nTamamlanan", lblCompleted, SuccessGreen, SuccessBg));

            // Kart 4: Online Görüşme Oranı (with progress bar)
            lblOnlineRate = new Label();
            flow.Controls.Add(CreateOnlineRateCard());

            panel.Controls.Add(flow);
            return panel;
        }

        private Panel CreateSummaryCard(string icon, string title, Label valueLabel, Color iconColor, Color bgColor)
        {
            var card = new Panel
            {
                Size = new Size(160, 70),
                Margin = new Padding(0, 0, 15, 0),
                BackColor = CardWhite
            };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 10);

            // Icon
            var iconPanel = new Panel { Location = new Point(12, 15), Size = new Size(40, 40), BackColor = Color.Transparent };
            iconPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(bgColor))
                    e.Graphics.FillEllipse(brush, 0, 0, 39, 39);
                using (var font = new Font("Segoe UI", 14F))
                using (var brush = new SolidBrush(iconColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(icon, font, brush, new RectangleF(0, 0, 40, 40), sf);
            };
            card.Controls.Add(iconPanel);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextMedium,
                Location = new Point(58, 10),
                Size = new Size(70, 30)
            };
            card.Controls.Add(lblTitle);

            valueLabel.Text = "0";
            valueLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            valueLabel.ForeColor = TextDark;
            valueLabel.Location = new Point(58, 38);
            valueLabel.AutoSize = true;
            card.Controls.Add(valueLabel);

            return card;
        }

        private Panel CreateOnlineRateCard()
        {
            var card = new Panel
            {
                Size = new Size(200, 70),
                Margin = new Padding(0, 0, 15, 0),
                BackColor = CardWhite
            };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 10);

            var iconPanel = new Panel { Location = new Point(12, 15), Size = new Size(40, 40), BackColor = Color.Transparent };
            iconPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(InfoBg))
                    e.Graphics.FillEllipse(brush, 0, 0, 39, 39);
                using (var font = new Font("Segoe UI", 14F))
                using (var brush = new SolidBrush(InfoBlue))
                    e.Graphics.DrawString("💻", font, brush, 8, 8);
            };
            card.Controls.Add(iconPanel);

            var lblTitle = new Label
            {
                Text = "Online Görüşme Oranı",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextMedium,
                Location = new Point(58, 8),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            lblOnlineRate = new Label
            {
                Text = "60%",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = PrimaryGreen,
                Location = new Point(58, 28),
                AutoSize = true
            };
            card.Controls.Add(lblOnlineRate);

            // Progress bar
            var progressPanel = new Panel
            {
                Location = new Point(100, 35),
                Size = new Size(80, 18),
                BackColor = Color.Transparent
            };
            progressPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                // Background
                using (var brush = new SolidBrush(BorderGray))
                    e.Graphics.FillRectangle(brush, 0, 5, 80, 8);
                // Fill
                using (var brush = new SolidBrush(PrimaryGreen))
                    e.Graphics.FillRectangle(brush, 0, 5, 48, 8); // 60%
            };
            card.Controls.Add(progressPanel);

            return card;
        }
        #endregion

        #region List Header
        private Panel CreateListHeader()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            var lbl = new Label
            {
                Text = "📋 Randevu Listesi",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(0, 10),
                AutoSize = true
            };
            panel.Controls.Add(lbl);

            return panel;
        }
        #endregion

        #region Filter Bar
        private Panel CreateFilterBar()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            int x = 0;

            // Hasta filter - seçince otomatik filtrele
            cmbFilterHasta = new ComboBoxEdit { Location = new Point(x, 8), Size = new Size(140, 28) };
            cmbFilterHasta.Properties.NullText = "Tüm Hastalar";
            cmbFilterHasta.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cmbFilterHasta.SelectedIndexChanged += (s, e) => FilterAppointments();
            panel.Controls.Add(cmbFilterHasta);
            x += 150;

            // Tür
            cmbFilterTur = new ComboBoxEdit { Location = new Point(x, 8), Size = new Size(90, 28) };
            cmbFilterTur.Properties.NullText = "Tür";
            cmbFilterTur.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cmbFilterTur.Properties.Items.AddRange(new[] { "Tümü", "Online", "Klinik" });
            cmbFilterTur.SelectedIndex = 0;
            cmbFilterTur.SelectedIndexChanged += (s, e) => FilterAppointments();
            panel.Controls.Add(cmbFilterTur);
            x += 100;

            // Durum
            cmbFilterDurum = new ComboBoxEdit { Location = new Point(x, 8), Size = new Size(110, 28) };
            cmbFilterDurum.Properties.NullText = "Durum";
            cmbFilterDurum.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cmbFilterDurum.Properties.Items.AddRange(new[] { "Tümü", "Bekliyor", "Onaylı", "Tamamlandı", "İptal" });
            cmbFilterDurum.SelectedIndex = 0;
            cmbFilterDurum.SelectedIndexChanged += (s, e) => FilterAppointments();
            panel.Controls.Add(cmbFilterDurum);
            x += 120;

            // Tümünü Göster butonu
            var btnYenile = new SimpleButton
            {
                Text = "↻ Tümünü Göster",
                Location = new Point(x, 6),
                Size = new Size(115, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryGreen, ForeColor = Color.White, BorderColor = PrimaryGreen }
            };
            btnYenile.Click += (s, e) => ResetFilters();
            panel.Controls.Add(btnYenile);

            return panel;
        }
        
        private void FilterAppointments()
        {
            try
            {
                var filtered = _appointments.ToList();
                
                // Hasta filtresi
                var selectedPatient = cmbFilterHasta?.EditValue as PatientItem;
                if (selectedPatient != null && selectedPatient.Id > 0)
                {
                    filtered = filtered.Where(a => a.PatientId == selectedPatient.Id).ToList();
                }
                
                // Tür filtresi
                var tur = cmbFilterTur?.Text;
                if (!string.IsNullOrEmpty(tur) && tur != "Tümü")
                {
                    if (tur == "Online")
                        filtered = filtered.Where(a => a.Type == AppointmentType.Online).ToList();
                    else if (tur == "Klinik")
                        filtered = filtered.Where(a => a.Type == AppointmentType.Clinic).ToList();
                }
                
                // Durum filtresi
                var durum = cmbFilterDurum?.Text;
                if (!string.IsNullOrEmpty(durum) && durum != "Tümü")
                {
                    if (durum == "Bekliyor")
                        filtered = filtered.Where(a => a.Status == AppointmentStatus.Pending).ToList();
                    else if (durum == "Onaylı")
                        filtered = filtered.Where(a => a.Status == AppointmentStatus.Scheduled).ToList();
                    else if (durum == "Tamamlandı")
                        filtered = filtered.Where(a => a.Status == AppointmentStatus.Completed).ToList();
                    else if (durum == "İptal")
                        filtered = filtered.Where(a => a.Status == AppointmentStatus.Cancelled).ToList();
                }
                
                gridAppointments.DataSource = filtered;
                gridAppointments.RefreshDataSource();
            }
            catch { }
        }
        
        private void ResetFilters()
        {
            if (cmbFilterHasta != null && cmbFilterHasta.Properties.Items.Count > 0)
                cmbFilterHasta.SelectedIndex = 0;
            if (cmbFilterTur != null)
                cmbFilterTur.SelectedIndex = 0;
            if (cmbFilterDurum != null)
                cmbFilterDurum.SelectedIndex = 0;
            
            gridAppointments.DataSource = _appointments;
            gridAppointments.RefreshDataSource();
        }
        #endregion

        #region Action Buttons
        private Panel CreateActionButtons()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            int x = 0;

            // Onayla
            var btnOnayla = CreateActionBtn("✓ Onayla", x, SuccessGreen, Color.White);
            btnOnayla.Click += BtnApprove_Click;
            panel.Controls.Add(btnOnayla);
            x += 95;

            // Reddet
            var btnReddet = CreateActionBtn("✕ Reddet", x, DangerRed, Color.White);
            btnReddet.Click += BtnReject_Click;
            panel.Controls.Add(btnReddet);
            x += 95;

            // Tamamlandı
            var btnTamamlandi = CreateActionBtn("✓ Tamamlandı", x, WarningOrange, Color.White);
            btnTamamlandi.Size = new Size(110, 32);
            btnTamamlandi.Click += BtnComplete_Click;
            panel.Controls.Add(btnTamamlandi);
            x += 120;

            // Yenile
            var btnYenile = CreateActionBtn("↻ Yenile", x, CardWhite, TextDark, true);
            btnYenile.Click += (s, e) => LoadData();
            panel.Controls.Add(btnYenile);
            x += 95;

            // Sıfırla
            var btnSifirla = CreateActionBtn("⊘ Sıfırla", x, CardWhite, TextDark, true);
            btnSifirla.Click += (s, e) => ResetFilters();
            panel.Controls.Add(btnSifirla);
            x += 95;

            // Sil
            var btnSil = CreateActionBtn("🗑 Sil", x, DangerBg, DangerRed, true);
            btnSil.Click += BtnDelete_Click;
            panel.Controls.Add(btnSil);

            // Sağda: Sayfa bilgisi
            var lblPageInfo = new Label
            {
                Text = "1-2 arası toplam 2 randevu",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMedium,
                Location = new Point(panel.Width - 180, 12),
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            panel.Controls.Add(lblPageInfo);

            return panel;
        }

        private SimpleButton CreateActionBtn(string text, int x, Color bg, Color fg, bool hasBorder = false)
        {
            return new SimpleButton
            {
                Text = text,
                Location = new Point(x, 8),
                Size = new Size(85, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = bg, ForeColor = fg, BorderColor = hasBorder ? InputBorder : bg }
            };
        }
        #endregion

        #region Grid
        private Panel CreateGrid()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardWhite,
                Padding = new Padding(0)
            };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 12);

            gridAppointments = new GridControl { Dock = DockStyle.Fill };
            viewAppointments = new GridView(gridAppointments)
            {
                OptionsBehavior = { Editable = false },
                OptionsView = { ShowGroupPanel = false, ShowIndicator = false, ColumnAutoWidth = true, EnableAppearanceEvenRow = true },
                RowHeight = 45
            };
            viewAppointments.Appearance.Row.Font = new Font("Segoe UI", 10F);
            viewAppointments.Appearance.EvenRow.BackColor = Color.FromArgb(250, 251, 252);
            viewAppointments.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            viewAppointments.Appearance.HeaderPanel.ForeColor = TextMedium;

            SetupGridColumns();
            gridAppointments.MainView = viewAppointments;

            // Status coloring
            viewAppointments.RowCellStyle += (s, e) =>
            {
                if (e.Column.FieldName == "StatusText")
                {
                    var status = e.CellValue?.ToString() ?? "";
                    if (status.Contains("Tamamlandı"))
                    {
                        e.Appearance.BackColor = SuccessBg;
                        e.Appearance.ForeColor = SuccessGreen;
                    }
                    else if (status.Contains("Onaylı"))
                    {
                        e.Appearance.BackColor = InfoBg;
                        e.Appearance.ForeColor = InfoBlue;
                    }
                    else if (status.Contains("Bekliyor"))
                    {
                        e.Appearance.BackColor = WarningBg;
                        e.Appearance.ForeColor = WarningOrange;
                    }
                    else if (status.Contains("İptal"))
                    {
                        e.Appearance.BackColor = DangerBg;
                        e.Appearance.ForeColor = DangerRed;
                    }
                    e.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                if (e.Column.FieldName == "TypeText")
                {
                    var type = e.CellValue?.ToString() ?? "";
                    if (type == "Online")
                    {
                        e.Appearance.BackColor = InfoBg;
                        e.Appearance.ForeColor = InfoBlue;
                    }
                    else
                    {
                        e.Appearance.BackColor = SuccessBg;
                        e.Appearance.ForeColor = SuccessGreen;
                    }
                    e.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
            };

            panel.Controls.Add(gridAppointments);
            return panel;
        }

        private void SetupGridColumns()
        {
            viewAppointments.Columns.Clear();
            viewAppointments.Columns.Add(new GridColumn { FieldName = "PatientName", Caption = "Hasta", Visible = true, Width = 150 });
            viewAppointments.Columns.Add(new GridColumn { FieldName = "DateTime", Caption = "Tarih", Visible = true, Width = 100, DisplayFormat = { FormatType = DevExpress.Utils.FormatType.DateTime, FormatString = "dd.MM.yyyy" } });
            viewAppointments.Columns.Add(new GridColumn { FieldName = "DateTime", Caption = "Saat", Visible = true, Width = 60, DisplayFormat = { FormatType = DevExpress.Utils.FormatType.DateTime, FormatString = "HH:mm" } });
            viewAppointments.Columns.Add(new GridColumn { FieldName = "TypeText", Caption = "Tür", Visible = true, Width = 80 });
            viewAppointments.Columns.Add(new GridColumn { FieldName = "Price", Caption = "Ücret", Visible = true, Width = 80, DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "₺ #,##0" } });
            viewAppointments.Columns.Add(new GridColumn { FieldName = "StatusText", Caption = "Durum", Visible = true, Width = 100 });
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
                
                // Form combobox
                if (cmbHasta != null)
                {
                    cmbHasta.Properties.Items.Clear();
                    foreach (var p in patients)
                    {
                        cmbHasta.Properties.Items.Add(new PatientItem { Id = p.Id, Name = p.AdSoyad });
                    }
                    if (cmbHasta.Properties.Items.Count > 0)
                        cmbHasta.SelectedIndex = 0;
                }
                
                // Filtre combobox
                if (cmbFilterHasta != null)
                {
                    cmbFilterHasta.Properties.Items.Clear();
                    cmbFilterHasta.Properties.Items.Add(new PatientItem { Id = 0, Name = "Tüm Hastalar" });
                    foreach (var p in patients)
                    {
                        cmbFilterHasta.Properties.Items.Add(new PatientItem { Id = p.Id, Name = p.AdSoyad });
                    }
                    cmbFilterHasta.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadPatients Error: " + ex.Message);
            }
        }
        #endregion

        #region Data Operations
        private void LoadData()
        {
            try
            {
                // Hastaları yükle
                LoadPatients();
                
                _appointments = _repository.GetByDoctor(AuthContext.UserId).ToList();
                gridAppointments.DataSource = _appointments;
                gridAppointments.RefreshDataSource();

                // Update summary cards
                int pending = _appointments.Count(a => a.Status == AppointmentStatus.Pending);
                int today = _appointments.Count(a => a.DateTime.Date == DateTime.Today);
                int completed = _appointments.Count(a => a.Status == AppointmentStatus.Completed);
                int online = _appointments.Count(a => a.Type == AppointmentType.Online);
                int total = _appointments.Count;
                int onlineRate = total > 0 ? (int)((double)online / total * 100) : 0;

                lblPending.Text = pending.ToString();
                lblToday.Text = today.ToString();
                lblCompleted.Text = completed.ToString();
                lblOnlineRate.Text = onlineRate + "%";
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

                var date = (DateTime)dtTarih.EditValue;
                var time = (DateTime)timeSaat.EditValue;
                var dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);

                var appointment = new Appointment
                {
                    PatientId = selectedPatient.Id,
                    DoctorId = AuthContext.UserId,
                    DateTime = dateTime,
                    Type = cmbTur.Text == "Online" ? AppointmentType.Online : AppointmentType.Clinic,
                    Status = AppointmentStatus.Pending,
                    Price = Convert.ToDecimal(spnUcret.Value),
                    Notes = txtNot.Text
                };

                _repository.Add(appointment);
                ToastNotification.ShowSuccess("Randevu oluşturuldu!");
                ClearInputs();
                LoadData();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnApprove_Click(object sender, EventArgs e)
        {
            var apt = GetSelectedAppointment();
            if (apt == null) return;

            apt.Status = AppointmentStatus.Scheduled;
            _repository.Update(apt);
            ToastNotification.ShowSuccess("Randevu onaylandı!");
            LoadData();
        }

        private void BtnReject_Click(object sender, EventArgs e)
        {
            var apt = GetSelectedAppointment();
            if (apt == null) return;

            apt.Status = AppointmentStatus.Cancelled;
            _repository.Update(apt);
            ToastNotification.ShowInfo("Randevu reddedildi.");
            LoadData();
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            var apt = GetSelectedAppointment();
            if (apt == null) return;

            apt.Status = AppointmentStatus.Completed;
            _repository.Update(apt);
            ToastNotification.ShowSuccess("Randevu tamamlandı!");
            LoadData();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var apt = GetSelectedAppointment();
            if (apt == null) return;

            if (XtraMessageBox.Show("Bu randevuyu silmek istediğinize emin misiniz?", "Onay",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _repository.Delete(apt.Id);
                ToastNotification.ShowInfo("Randevu silindi.");
                LoadData();
            }
        }

        private Appointment GetSelectedAppointment()
        {
            var rowHandle = viewAppointments.FocusedRowHandle;
            if (rowHandle < 0)
            {
                XtraMessageBox.Show("Lütfen bir randevu seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
            return viewAppointments.GetRow(rowHandle) as Appointment;
        }

        private void ClearInputs()
        {
            if (cmbHasta.Properties.Items.Count > 0)
                cmbHasta.SelectedIndex = 0;
            dtTarih.EditValue = DateTime.Today;
            timeSaat.EditValue = new DateTime(2000, 1, 1, 10, 30, 0);
            cmbTur.SelectedIndex = 0;
            spnUcret.Value = 500;
            txtNot.Text = "";
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
