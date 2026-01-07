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
using DiyetisyenOtomasyonu.Infrastructure.Security;
using DiyetisyenOtomasyonu.Shared;

namespace DiyetisyenOtomasyonu.Forms.Doctor
{
    /// <summary>
    /// Finansal Özet - Modern ve Profesyonel Tasarım
    /// </summary>
    public partial class FrmFinancials : XtraForm
    {
        private readonly AppointmentRepository _repository;
        private readonly PatientRepository _patientRepository;
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
        private readonly Color PurpleColor = ColorTranslator.FromHtml("#8B5CF6");
        private readonly Color PurpleBg = ColorTranslator.FromHtml("#EDE9FE");
        
        private readonly Color BackgroundLight = ColorTranslator.FromHtml("#F8FAFC");
        private readonly Color CardWhite = Color.White;
        private readonly Color BorderGray = ColorTranslator.FromHtml("#E2E8F0");
        private readonly Color InputBorder = ColorTranslator.FromHtml("#CBD5E1");
        
        private readonly Color TextDark = ColorTranslator.FromHtml("#1E293B");
        private readonly Color TextMedium = ColorTranslator.FromHtml("#64748B");
        #endregion

        #region Controls
        private GridControl gridAppointments;
        private GridView viewAppointments;
        private DateEdit dtStartDate, dtEndDate;
        
        // Summary labels
        private Label lblTodayRevenue, lblWeekRevenue, lblMonthRevenue, lblTotalRevenue;
        private Label lblTodayRevenueBottom, lblWeekRevenueBottom, lblMonthRevenueBottom, lblTotalRevenueBottom;
        private Label lblOnlineRate, lblAvgPrice, lblTotalPatients, lblSummaryInfo;
        private Panel pnlOnlineChart;
        private int _onlinePercentage = 0;
        #endregion

        public FrmFinancials()
        {
            _repository = new AppointmentRepository();
            _patientRepository = new PatientRepository();
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1200, 750);
            this.Text = "Finansal Özet";
            this.BackColor = BackgroundLight;
            this.Padding = new Padding(25);
            this.FormBorderStyle = FormBorderStyle.None;
            this.ResumeLayout(false);
            
            SetupUI();
        }

        private void SetupUI()
        {
            // Ana dikey layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.Transparent
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // Üst kartlar
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));  // Filtre + Başlık
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));   // Grid
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));   // Alt kartlar + grafik

            mainLayout.Controls.Add(CreateTopCards(), 0, 0);
            mainLayout.Controls.Add(CreateFilterSection(), 0, 1);
            mainLayout.Controls.Add(CreateGridSection(), 0, 2);
            mainLayout.Controls.Add(CreateBottomSection(), 0, 3);

            this.Controls.Add(mainLayout);
        }

        #region Top Summary Cards
        private Panel CreateTopCards()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            // Kart 1: Bugünkü Gelir
            lblTodayRevenue = new Label { Text = "₺0", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = TextDark };
            flow.Controls.Add(CreateTopCard("💰", "Bugünkü Gelir", lblTodayRevenue, SuccessGreen, SuccessBg));

            // Kart 2: Bu Hafta
            lblWeekRevenue = new Label { Text = "₺0", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = TextDark };
            flow.Controls.Add(CreateTopCard("📅", "Bu Hafta", lblWeekRevenue, InfoBlue, InfoBg));

            // Kart 3: Bu Ay
            lblMonthRevenue = new Label { Text = "₺0", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = TextDark };
            flow.Controls.Add(CreateTopCard("📊", "Bu Ay", lblMonthRevenue, WarningOrange, WarningBg));

            // Kart 4: Toplam Ciro
            lblTotalRevenue = new Label { Text = "₺0", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = TextDark };
            flow.Controls.Add(CreateTopCard("🏆", "Toplam Ciro", lblTotalRevenue, PurpleColor, PurpleBg));

            panel.Controls.Add(flow);
            return panel;
        }

        private Panel CreateTopCard(string icon, string title, Label valueLabel, Color iconColor, Color bgColor)
        {
            var card = new Panel
            {
                Size = new Size(260, 90),
                Margin = new Padding(0, 0, 20, 0),
                BackColor = CardWhite
            };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 12);

            // Sol renk çizgisi
            var colorBar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(5, 90),
                BackColor = iconColor
            };
            card.Controls.Add(colorBar);

            // Icon
            var iconPanel = new Panel { Location = new Point(18, 20), Size = new Size(45, 45), BackColor = Color.Transparent };
            iconPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(bgColor))
                    e.Graphics.FillEllipse(brush, 0, 0, 44, 44);
                using (var font = new Font("Segoe UI", 16F))
                using (var brush = new SolidBrush(iconColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(icon, font, brush, new RectangleF(0, 0, 45, 45), sf);
            };
            card.Controls.Add(iconPanel);

            // Value
            valueLabel.Location = new Point(75, 18);
            valueLabel.AutoSize = true;
            card.Controls.Add(valueLabel);

            // Title
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextMedium,
                Location = new Point(75, 52),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

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

            // Başlık
            var lblTitle = new Label
            {
                Text = "📋 Tamamlanan Randevular (Gelir Detayı)",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = PrimaryGreen,
                Location = new Point(20, 16),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            // Tarih filtreleri
            int x = 400;

            var lblStart = new Label { Text = "Başlangıç:", Font = new Font("Segoe UI", 9F), ForeColor = TextMedium, Location = new Point(x, 16), AutoSize = true };
            panel.Controls.Add(lblStart);
            
            dtStartDate = new DateEdit { Location = new Point(x + 70, 12), Size = new Size(120, 28) };
            dtStartDate.DateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            panel.Controls.Add(dtStartDate);
            x += 210;

            var lblEnd = new Label { Text = "Bitiş:", Font = new Font("Segoe UI", 9F), ForeColor = TextMedium, Location = new Point(x, 16), AutoSize = true };
            panel.Controls.Add(lblEnd);
            
            dtEndDate = new DateEdit { Location = new Point(x + 40, 12), Size = new Size(120, 28) };
            dtEndDate.DateTime = DateTime.Today;
            panel.Controls.Add(dtEndDate);
            x += 180;

            // Filtrele butonu
            var btnFilter = new SimpleButton
            {
                Text = "🔍 Filtrele",
                Location = new Point(x, 10),
                Size = new Size(100, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryGreen, ForeColor = Color.White, BorderColor = PrimaryGreen }
            };
            btnFilter.Click += (s, e) => LoadData();
            panel.Controls.Add(btnFilter);

            // Fit it ele butonu
            var btnFix = new SimpleButton
            {
                Text = "📥 Fit it ele",
                Location = new Point(x + 110, 10),
                Size = new Size(90, 32),
                Font = new Font("Segoe UI", 9F),
                Appearance = { BackColor = CardWhite, ForeColor = TextMedium, BorderColor = InputBorder }
            };
            panel.Controls.Add(btnFix);

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

            // Özet bilgi satırı
            var infoBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = LightGreen,
                Padding = new Padding(20, 10, 20, 10)
            };

            lblSummaryInfo = new Label
            {
                Text = "📊 Bu 10 günde tamamlanan toplam 0 randevu, kazanç: ₺0",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = DarkGreen,
                Location = new Point(20, 10),
                AutoSize = true
            };
            infoBar.Controls.Add(lblSummaryInfo);
            panel.Controls.Add(infoBar);

            // Grid
            gridAppointments = new GridControl { Dock = DockStyle.Fill };
            viewAppointments = new GridView(gridAppointments)
            {
                OptionsBehavior = { Editable = false },
                OptionsView = { ShowGroupPanel = false, ShowIndicator = false, ColumnAutoWidth = true, EnableAppearanceEvenRow = true, ShowFooter = true },
                RowHeight = 42
            };
            viewAppointments.Appearance.Row.Font = new Font("Segoe UI", 10F);
            viewAppointments.Appearance.EvenRow.BackColor = Color.FromArgb(250, 251, 252);
            viewAppointments.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            viewAppointments.Appearance.HeaderPanel.ForeColor = TextMedium;

            SetupGridColumns();
            gridAppointments.MainView = viewAppointments;

            // Row styling
            viewAppointments.RowCellStyle += (s, e) =>
            {
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
                if (e.Column.FieldName == "PatientName")
                {
                    e.Appearance.ForeColor = PrimaryGreen;
                    e.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                }
            };

            panel.Controls.Add(gridAppointments);

            // Footer
            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(20, 10, 20, 10)
            };

            var lblFooterInfo = new Label
            {
                Text = "Bugünün Geliri  0 ₺",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMedium,
                Location = new Point(20, 10),
                AutoSize = true
            };
            footer.Controls.Add(lblFooterInfo);

            panel.Controls.Add(footer);

            return panel;
        }

        private void SetupGridColumns()
        {
            viewAppointments.Columns.Clear();
            viewAppointments.Columns.Add(new GridColumn { FieldName = "PatientName", Caption = "Hasta", Visible = true, Width = 150 });
            viewAppointments.Columns.Add(new GridColumn { FieldName = "DateTime", Caption = "Tarih/Saat", Visible = true, Width = 150, DisplayFormat = { FormatType = DevExpress.Utils.FormatType.DateTime, FormatString = "dd.MM.yyyy HH:mm" } });
            viewAppointments.Columns.Add(new GridColumn { FieldName = "TypeText", Caption = "Tür", Visible = true, Width = 90 });
            
            var colPrice = new GridColumn { FieldName = "Price", Caption = "Ücret", Visible = true, Width = 100, DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "₺ #,##0" } };
            colPrice.SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            colPrice.SummaryItem.DisplayFormat = "Toplam: ₺{0:N0}";
            viewAppointments.Columns.Add(colPrice);
            
            viewAppointments.Columns.Add(new GridColumn { FieldName = "Notes", Caption = "Not", Visible = true, Width = 200 });
        }
        #endregion

        #region Bottom Section
        private Panel CreateBottomSection()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            // Sol: 4 mini kart (2x2)
            var leftPanel = new Panel
            {
                Location = new Point(0, 10),
                Size = new Size(400, 180),
                BackColor = Color.Transparent
            };
            CreateBottomCards(leftPanel);
            panel.Controls.Add(leftPanel);

            // Orta: Büyük toplam ciro kartı
            var centerCard = CreateTotalCard();
            centerCard.Location = new Point(420, 10);
            panel.Controls.Add(centerCard);

            // Sağ: Grafik ve istatistikler
            var rightPanel = CreateRightStats();
            rightPanel.Location = new Point(680, 10);
            panel.Controls.Add(rightPanel);

            // Resize handler
            panel.Resize += (s, e) =>
            {
                leftPanel.Size = new Size(380, Math.Min(180, panel.Height - 20));
                centerCard.Location = new Point(400, 10);
                centerCard.Size = new Size(250, Math.Min(180, panel.Height - 20));
                rightPanel.Location = new Point(670, 10);
                rightPanel.Size = new Size(panel.Width - 690, Math.Min(180, panel.Height - 20));
            };

            return panel;
        }

        private void CreateBottomCards(Panel parent)
        {
            int cardW = 175, cardH = 80, gap = 10;

            // Satır 1
            lblTodayRevenueBottom = new Label { Text = "₺0", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = TextDark };
            parent.Controls.Add(CreateMiniCard("💰", "Bugünkü Gelir", lblTodayRevenueBottom, PrimaryGreen, 0, 0, cardW, cardH));

            lblWeekRevenueBottom = new Label { Text = "₺0", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = TextDark };
            parent.Controls.Add(CreateMiniCard("📅", "Bu Hafta", lblWeekRevenueBottom, SuccessGreen, cardW + gap, 0, cardW, cardH));

            // Satır 2
            var lblHazir = new Label { Text = "₺0", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = TextDark };
            parent.Controls.Add(CreateMiniCard("💵", "Hazır Gelir", lblHazir, WarningOrange, 0, cardH + gap, cardW, cardH));

            lblMonthRevenueBottom = new Label { Text = "₺0", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = TextDark };
            parent.Controls.Add(CreateMiniCard("📊", "Bu Ay", lblMonthRevenueBottom, InfoBlue, cardW + gap, cardH + gap, cardW, cardH));
        }

        private Panel CreateMiniCard(string icon, string title, Label valueLabel, Color iconColor, int x, int y, int w, int h)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = CardWhite
            };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 8);

            // Icon
            var iconPanel = new Panel { Location = new Point(10, 15), Size = new Size(32, 32), BackColor = Color.Transparent };
            iconPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(40, iconColor)))
                    e.Graphics.FillEllipse(brush, 0, 0, 31, 31);
                using (var font = new Font("Segoe UI", 11F))
                using (var brush = new SolidBrush(iconColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(icon, font, brush, new RectangleF(0, 0, 32, 32), sf);
            };
            card.Controls.Add(iconPanel);

            // Value
            valueLabel.Location = new Point(50, 12);
            valueLabel.AutoSize = true;
            card.Controls.Add(valueLabel);

            // Title
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextMedium,
                Location = new Point(50, 38),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            return card;
        }

        private Panel CreateTotalCard()
        {
            var card = new Panel
            {
                Size = new Size(240, 180),
                BackColor = CardWhite
            };
            card.Paint += (s, e) => DrawRoundedBorder(e.Graphics, card, 12);

            // Icon
            var iconPanel = new Panel { Location = new Point(20, 20), Size = new Size(50, 50), BackColor = Color.Transparent };
            iconPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(LightGreen))
                    e.Graphics.FillEllipse(brush, 0, 0, 49, 49);
                using (var font = new Font("Segoe UI", 18F))
                using (var brush = new SolidBrush(PrimaryGreen))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString("💎", font, brush, new RectangleF(0, 0, 50, 50), sf);
            };
            card.Controls.Add(iconPanel);

            // Value
            lblTotalRevenueBottom = new Label
            {
                Text = "₺0",
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                ForeColor = PrimaryGreen,
                Location = new Point(20, 80),
                AutoSize = true
            };
            card.Controls.Add(lblTotalRevenueBottom);

            // Title
            var lblTitle = new Label
            {
                Text = "Toplam Ciro",
                Font = new Font("Segoe UI", 11F),
                ForeColor = TextMedium,
                Location = new Point(20, 120),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            return card;
        }

        private Panel CreateRightStats()
        {
            var panel = new Panel
            {
                Size = new Size(450, 180),
                BackColor = CardWhite
            };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 12);

            // Circular chart (simulated)
            pnlOnlineChart = new Panel { Location = new Point(20, 30), Size = new Size(120, 120), BackColor = Color.Transparent };
            pnlOnlineChart.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Background circle
                using (var pen = new Pen(BorderGray, 10))
                    e.Graphics.DrawEllipse(pen, 10, 10, 100, 100);
                
                // Progress arc
                using (var pen = new Pen(PrimaryGreen, 10))
                {
                    int sweepAngle = (int)(360 * _onlinePercentage / 100.0);
                    e.Graphics.DrawArc(pen, 10, 10, 100, 100, -90, sweepAngle);
                }
                
                // Center text
                using (var font = new Font("Segoe UI", 18F, FontStyle.Bold))
                using (var brush = new SolidBrush(PrimaryGreen))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString($"{_onlinePercentage}%", font, brush, new RectangleF(0, 0, 120, 120), sf);
            };
            panel.Controls.Add(pnlOnlineChart);

            // Stats
            int y = 25;

            lblAvgPrice = new Label
            {
                Text = "Ortalama Ücret  ₺0",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(160, y),
                AutoSize = true
            };
            panel.Controls.Add(lblAvgPrice);
            y += 30;

            lblOnlineRate = new Label
            {
                Text = "Online Görüşme Oranı  0%",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(160, y),
                AutoSize = true
            };
            panel.Controls.Add(lblOnlineRate);
            y += 30;

            lblTotalPatients = new Label
            {
                Text = "Toplam Hasta  0",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(160, y),
                AutoSize = true
            };
            panel.Controls.Add(lblTotalPatients);

            return panel;
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
        #endregion

        #region Data Operations
        private void LoadData()
        {
            try
            {
                // Tüm randevuları al
                var allAppointments = _repository.GetByDoctor(AuthContext.UserId);
                
                // Tarih aralığına göre filtrele (sadece tamamlananlar)
                _appointments = allAppointments
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .Where(a => a.DateTime.Date >= dtStartDate.DateTime.Date && a.DateTime.Date <= dtEndDate.DateTime.Date)
                    .OrderByDescending(a => a.DateTime)
                    .ToList();

                gridAppointments.DataSource = _appointments;
                
                // İstatistikleri güncelle
                UpdateSummary(allAppointments.Where(a => a.Status == AppointmentStatus.Completed).ToList());
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSummary(List<Appointment> completedAppointments)
        {
            // Bugün
            var today = DateTime.Today;
            var todayRevenue = completedAppointments.Where(a => a.DateTime.Date == today).Sum(a => a.Price);
            
            // Bu hafta
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var weekRevenue = completedAppointments.Where(a => a.DateTime.Date >= weekStart).Sum(a => a.Price);
            
            // Bu ay
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthRevenue = completedAppointments.Where(a => a.DateTime.Date >= monthStart).Sum(a => a.Price);
            
            // Toplam
            var totalRevenue = completedAppointments.Sum(a => a.Price);
            
            // Online oranı
            var onlineCount = completedAppointments.Count(a => a.Type == AppointmentType.Online);
            var totalCount = completedAppointments.Count;
            _onlinePercentage = totalCount > 0 ? (int)((double)onlineCount / totalCount * 100) : 0;
            
            // Ortalama ücret
            var avgPrice = totalCount > 0 ? totalRevenue / totalCount : 0;
            
            // Toplam hasta sayısı
            var uniquePatients = completedAppointments.Select(a => a.PatientId).Distinct().Count();

            // Üst kartları güncelle
            lblTodayRevenue.Text = $"₺{todayRevenue:N0}";
            lblWeekRevenue.Text = $"₺{weekRevenue:N0}";
            lblMonthRevenue.Text = $"₺{monthRevenue:N0}";
            lblTotalRevenue.Text = $"₺{totalRevenue:N0}";

            // Alt kartları güncelle
            lblTodayRevenueBottom.Text = $"₺{todayRevenue:N0}";
            lblWeekRevenueBottom.Text = $"₺{weekRevenue:N0}";
            lblMonthRevenueBottom.Text = $"₺{monthRevenue:N0}";
            lblTotalRevenueBottom.Text = $"₺{totalRevenue:N0}";

            // Sağ istatistikler
            lblAvgPrice.Text = $"Ortalama Ücret  ₺{avgPrice:N0}";
            lblOnlineRate.Text = $"Online Görüşme Oranı  {_onlinePercentage}%";
            lblTotalPatients.Text = $"Toplam Hasta  {uniquePatients}";
            
            // Info bar
            var daysDiff = (dtEndDate.DateTime - dtStartDate.DateTime).Days + 1;
            var filteredRevenue = _appointments.Sum(a => a.Price);
            lblSummaryInfo.Text = $"📊 Bu {daysDiff} günde tamamlanan toplam {_appointments.Count} randevu, kazanç: ₺{filteredRevenue:N0}";

            // Refresh chart
            pnlOnlineChart?.Invalidate();
        }
        #endregion
    }
}
