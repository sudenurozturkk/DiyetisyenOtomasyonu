using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
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
    /// Randevu Talep ve Görüntüleme Formu - Hasta için
    /// Modern ve temiz tasarım
    /// </summary>
    public partial class FrmPatientAppointments : XtraForm
    {
        private readonly AppointmentRepository _repository;
        private readonly PatientRepository _patientRepository;

        private GridControl grdAppointments;
        private GridView grdView;
        private DateEdit dtDate;
        private TimeEdit dtTime;
        private ComboBoxEdit cmbType;
        private MemoEdit txtNotes;
        private Label lblUpcoming;

        // Modern Renkler
        private readonly Color PrimaryGreen = Color.FromArgb(13, 148, 136);
        private readonly Color SuccessGreen = Color.FromArgb(34, 197, 94);
        private readonly Color WarningOrange = Color.FromArgb(249, 115, 22);
        private readonly Color DangerRed = Color.FromArgb(239, 68, 68);
        private readonly Color InfoBlue = Color.FromArgb(59, 130, 246);
        private readonly Color CardWhite = Color.White;
        private readonly Color BackgroundLight = Color.FromArgb(248, 250, 252);
        private readonly Color TextDark = Color.FromArgb(30, 41, 59);
        private readonly Color TextMedium = Color.FromArgb(100, 116, 139);
        private readonly Color BorderGray = Color.FromArgb(226, 232, 240);

        public FrmPatientAppointments()
        {
            _repository = new AppointmentRepository();
            _patientRepository = new PatientRepository();
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1000, 600);
            this.Text = "Randevularım";
            this.BackColor = BackgroundLight;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(15);

            // Ana Container - 2 satırlı layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180)); // Üst: talep formu
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Alt: randevu listesi

            // Üst Panel - Randevu Talebi
            var topPanel = CreateRequestPanel();
            mainLayout.Controls.Add(topPanel, 0, 0);

            // Alt Panel - Randevu Listesi
            var bottomPanel = CreateAppointmentsPanel();
            mainLayout.Controls.Add(bottomPanel, 0, 1);

            this.Controls.Add(mainLayout);
            this.ResumeLayout(false);
        }

        private Panel CreateRequestPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = CardWhite, Padding = new Padding(20) };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 12);

            // Başlık
            var lblTitle = new Label
            {
                Text = "📅 Yeni Randevu Talebi",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = PrimaryGreen,
                Location = new Point(20, 15),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);

            // Form satırı 1
            int y = 55;

            // Tarih
            var lblDate = new Label { Text = "Tarih:", Font = new Font("Segoe UI", 10F), ForeColor = TextMedium, Location = new Point(20, y), AutoSize = true };
            panel.Controls.Add(lblDate);
            dtDate = new DateEdit { Location = new Point(70, y - 3), Size = new Size(130, 30) };
            dtDate.DateTime = DateTime.Today.AddDays(1);
            dtDate.Properties.MinValue = DateTime.Today;
            panel.Controls.Add(dtDate);

            // Saat
            var lblTime = new Label { Text = "Saat:", Font = new Font("Segoe UI", 10F), ForeColor = TextMedium, Location = new Point(220, y), AutoSize = true };
            panel.Controls.Add(lblTime);
            dtTime = new TimeEdit { Location = new Point(270, y - 3), Size = new Size(100, 30) };
            dtTime.Time = new DateTime(2000, 1, 1, 10, 0, 0);
            panel.Controls.Add(dtTime);

            // Tür
            var lblType = new Label { Text = "Tür:", Font = new Font("Segoe UI", 10F), ForeColor = TextMedium, Location = new Point(390, y), AutoSize = true };
            panel.Controls.Add(lblType);
            cmbType = new ComboBoxEdit { Location = new Point(430, y - 3), Size = new Size(130, 30) };
            cmbType.Properties.Items.AddRange(new[] { "🌐 Online", "🏥 Klinik" });
            cmbType.SelectedIndex = 0;
            panel.Controls.Add(cmbType);

            // Randevu Talep Et butonu
            var btnRequest = new SimpleButton
            {
                Text = "📩 Randevu Talep Et",
                Location = new Point(580, y - 5),
                Size = new Size(180, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Appearance = { BackColor = PrimaryGreen, ForeColor = Color.White }
            };
            btnRequest.Click += BtnRequest_Click;
            panel.Controls.Add(btnRequest);

            // Not
            y += 45;
            var lblNote = new Label { Text = "Not:", Font = new Font("Segoe UI", 10F), ForeColor = TextMedium, Location = new Point(20, y), AutoSize = true };
            panel.Controls.Add(lblNote);
            txtNotes = new MemoEdit
            {
                Location = new Point(70, y - 3),
                Size = new Size(500, 50),
                Properties = { MaxLength = 300 }
            };
            txtNotes.Properties.NullValuePrompt = "Randevu sebebi veya notunuz (isteğe bağlı)...";
            panel.Controls.Add(txtNotes);

            return panel;
        }

        private Panel CreateAppointmentsPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = CardWhite, Padding = new Padding(20), Margin = new Padding(0, 10, 0, 0) };
            panel.Paint += (s, e) => DrawRoundedBorder(e.Graphics, panel, 12);

            // Başlık Satırı
            var header = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.Transparent };

            var lblTitle = new Label
            {
                Text = "📋 Randevularım",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = TextDark,
                Location = new Point(5, 8),
                AutoSize = true
            };
            header.Controls.Add(lblTitle);

            lblUpcoming = new Label
            {
                Text = "⏳ Yükleniyor...",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = SuccessGreen,
                Location = new Point(200, 12),
                AutoSize = true
            };
            header.Controls.Add(lblUpcoming);

            var btnRefresh = new SimpleButton
            {
                Text = "↻ Yenile",
                Location = new Point(400, 5),
                Size = new Size(90, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Appearance = { BackColor = InfoBlue, ForeColor = Color.White }
            };
            btnRefresh.Click += (s, e) => LoadData();
            header.Controls.Add(btnRefresh);

            panel.Controls.Add(header);

            // Grid
            grdAppointments = new GridControl { Dock = DockStyle.Fill };
            grdView = new GridView(grdAppointments);
            grdAppointments.MainView = grdView;

            grdView.OptionsBehavior.Editable = false;
            grdView.OptionsView.ShowGroupPanel = false;
            grdView.OptionsView.ShowIndicator = false;
            grdView.OptionsSelection.EnableAppearanceFocusedCell = false;
            grdView.RowHeight = 40;
            grdView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grdView.Appearance.HeaderPanel.ForeColor = TextDark;
            grdView.Appearance.HeaderPanel.BackColor = Color.FromArgb(241, 245, 249);
            grdView.Appearance.Row.Font = new Font("Segoe UI", 10F);

            grdView.Columns.AddVisible("DateTime", "Tarih / Saat").Width = 180;
            grdView.Columns.AddVisible("TypeText", "Tür").Width = 120;
            grdView.Columns.AddVisible("StatusText", "Durum").Width = 130;
            grdView.Columns.AddVisible("Notes", "Not").Width = 280;

            // Durum renklerini ayarla
            grdView.RowCellStyle += (s, e) =>
            {
                if (e.Column.FieldName == "StatusText" && e.RowHandle >= 0)
                {
                    var apt = grdView.GetRow(e.RowHandle) as Appointment;
                    if (apt != null)
                    {
                        switch (apt.Status)
                        {
                            case AppointmentStatus.Pending:
                                e.Appearance.ForeColor = WarningOrange;
                                e.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                                break;
                            case AppointmentStatus.Approved:
                                e.Appearance.ForeColor = SuccessGreen;
                                e.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                                break;
                            case AppointmentStatus.Cancelled:
                                e.Appearance.ForeColor = DangerRed;
                                e.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                                break;
                            case AppointmentStatus.Completed:
                                e.Appearance.ForeColor = InfoBlue;
                                e.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                                break;
                        }
                    }
                }
            };

            panel.Controls.Add(grdAppointments);

            return panel;
        }

        private void LoadData()
        {
            try
            {
                var appointments = _repository.GetByPatient(AuthContext.UserId);
                grdAppointments.DataSource = appointments.OrderByDescending(a => a.DateTime).ToList();

                // Yaklaşan randevu sayısı
                var upcomingCount = appointments.Count(a => a.Status == AppointmentStatus.Approved && a.DateTime > DateTime.Now);
                var pendingCount = appointments.Count(a => a.Status == AppointmentStatus.Pending);

                if (upcomingCount > 0)
                    lblUpcoming.Text = $"✅ {upcomingCount} onaylı randevu";
                else if (pendingCount > 0)
                    lblUpcoming.Text = $"⏳ {pendingCount} bekleyen talep";
                else
                    lblUpcoming.Text = "📭 Randevu yok";
            }
            catch (Exception ex)
            {
                lblUpcoming.Text = "❌ Yüklenemedi";
                System.Diagnostics.Debug.WriteLine("LoadData error: " + ex.Message);
            }
        }

        private void BtnRequest_Click(object sender, EventArgs e)
        {
            var patient = _patientRepository.GetById(AuthContext.UserId);
            if (patient == null)
            {
                ToastNotification.ShowError("Hasta bilgisi bulunamadı.");
                return;
            }

            var requestedDateTime = dtDate.DateTime.Date + dtTime.Time.TimeOfDay;

            if (requestedDateTime < DateTime.Now)
            {
                ToastNotification.ShowWarning("Geçmiş bir tarih için randevu talep edemezsiniz.");
                return;
            }

            var existingAppointments = _repository.GetByPatient(AuthContext.UserId);
            if (existingAppointments.Any(a => a.DateTime == requestedDateTime && a.Status != AppointmentStatus.Cancelled))
            {
                ToastNotification.ShowWarning("Bu tarih/saatte zaten bir randevunuz var.");
                return;
            }

            var appointment = new Appointment
            {
                PatientId = AuthContext.UserId,
                DoctorId = patient.DoctorId,
                DateTime = requestedDateTime,
                Type = cmbType.SelectedIndex == 0 ? AppointmentType.Online : AppointmentType.Clinic,
                Status = AppointmentStatus.Pending,
                Notes = txtNotes.Text,
                CreatedAt = DateTime.Now
            };

            _repository.Add(appointment);
            ToastNotification.ShowSuccess("Randevu talebiniz gönderildi!");
            ClearInputs();
            LoadData();
        }

        private void ClearInputs()
        {
            dtDate.DateTime = DateTime.Today.AddDays(1);
            dtTime.Time = new DateTime(2000, 1, 1, 10, 0, 0);
            cmbType.SelectedIndex = 0;
            txtNotes.Text = "";
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
    }
}
